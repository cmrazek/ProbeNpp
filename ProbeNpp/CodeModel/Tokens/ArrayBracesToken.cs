using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class ArrayBracesToken : Token, IGroupToken
    {
        private List<Token> _tokens = new List<Token>();
        private ArrayBraceToken _openToken = null;
        private ArrayBraceToken _closeToken = null;

        private ArrayBracesToken(Token parent, Scope scope, Span span)
            : base(parent, scope, span)
        {
        }

        public static ArrayBracesToken Parse(Token parent, Scope scope)
        {
            var startPos = scope.File.Position;
            scope.File.MoveNext();

            var ret = new ArrayBracesToken(parent, scope, new Span(startPos, startPos));
            ret._openToken = new ArrayBraceToken(ret, scope, new Span(startPos, scope.File.Position), ret, true);
            ret._tokens.Add(ret._openToken);

            var closeToken = null as Token;
            var scopeIndent = scope.IndentNonRoot();
            scopeIndent.Hint |= ScopeHint.SuppressFunctionDefinition | ScopeHint.SuppressVarDecl;

            var done = false;
            while (!scope.File.EndOfFile && !done)
            {
                var token = scope.File.ParseToken(ret, scopeIndent);
                if (token == null) break;
                if (token.GetType() == typeof(OperatorToken) && (token as OperatorToken).Text == "]")
                {
                    ret._closeToken = new ArrayBraceToken(ret, scope, token.Span, ret, false);
                    ret._tokens.Add(ret._closeToken);
                    done = true;
                }
                else if (token.BreaksStatement)
                {
                    scope.File.Position = token.Span.Start;
                    done = true;
                }
                else
                {
                    ret._tokens.Add(token);
                }
            }

            ret.Span = new Span(startPos, scope.File.Position);
            return ret;
        }

        IEnumerable<Token> IGroupToken.SubTokens
        {
            get { return _tokens; }
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("ArrayIndexer");
            xml.WriteAttributeString("span", Span.ToString());
            foreach (var t in _tokens) t.DumpTree(xml);
            xml.WriteEndElement();
        }

        public ArrayBraceToken OpenToken
        {
            get { return _openToken; }
        }

        public ArrayBraceToken CloseToken
        {
            get { return _closeToken; }
        }
    }
}
