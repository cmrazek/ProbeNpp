using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal sealed class IfStatementToken : Token, IGroupToken
    {
        private List<Token> _tokens = new List<Token>();
        private List<Token> _condition = new List<Token>();
        private BracesToken _trueScope = null;
        private KeywordToken _elseToken = null;
        private BracesToken _falseScope = null;

        private IfStatementToken(Token parent, Scope scope, Span span)
            : base(parent, scope, span)
        {
        }

        public static IfStatementToken Parse(Token parent, Scope scope, KeywordToken ifToken)
        {
            var ret = new IfStatementToken(parent, scope, new Span());
            ret._tokens.Add(ifToken);
            ifToken.Parent = ret;

            var scopeIndent = scope;
            scopeIndent.Hint |= ScopeHint.NotOnRoot | ScopeHint.SuppressFunctionDefinition | ScopeHint.SuppressVarDecl;

            var done = false;
            while (!done && !scope.File.EndOfFile)
            {
                var token = scope.File.ParseToken(ret, scopeIndent);
                if (token == null) break;
                ret._tokens.Add(token);

                if (token.GetType() == typeof(BracesToken))
                {
                    ret._trueScope = token as BracesToken;
                    done = true;
                }
                else if (token.GetType() == typeof(StatementEndToken) ||
                    (token.GetType() == typeof(OperatorToken) && (token.Text == "}" || token.Text == ")")))
                {
                    ret._condition.Add(token);
                    done = true;
                }
                else
                {
                    ret._condition.Add(token);
                }
            }

            var afterBracesPos = scope.File.Position;

            var elseToken = scope.File.ParseSingleToken(ret, scopeIndent);
            if (elseToken != null && elseToken.GetType() == typeof(KeywordToken) && elseToken.Text == "else")
            {
                var falseScope = scope.File.ParseSingleToken(ret, scopeIndent);
                if (falseScope != null && falseScope.GetType() == typeof(BracesToken))
                {
                    ret._elseToken = elseToken as KeywordToken;
                    ret._falseScope = falseScope as BracesToken;
                    ret._tokens.Add(ret._elseToken);
                    ret._tokens.Add(ret._falseScope);
                }
                else scope.File.Position = afterBracesPos;
            }
            else scope.File.Position = afterBracesPos;

            ret.Span = new Span(ifToken.Span.Start, ret._tokens.Last().Span.End);
            return ret;
        }

        IEnumerable<Token> IGroupToken.SubTokens
        {
            get { return _tokens; }
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("If");

            xml.WriteStartElement("IfCondition");
            foreach (var token in _condition) token.DumpTree(xml);
            xml.WriteEndElement();

            if (_trueScope != null)
            {
                xml.WriteStartElement("IfTrueScope");
                _trueScope.DumpTree(xml);
                xml.WriteEndElement();
            }

            if (_falseScope != null)
            {
                xml.WriteStartElement("IfFalseScope");
                _falseScope.DumpTree(xml);
                xml.WriteEndElement();
            }

            xml.WriteEndElement();
        }

        public override bool BreaksStatement
        {
            get
            {
                return true;
            }
        }
    }
}
