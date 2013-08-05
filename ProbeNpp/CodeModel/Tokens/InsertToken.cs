using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class InsertToken : Token, IGroupToken
    {
        private Token[] _tokens;
        private InsertBoundaryToken _startToken;
        private InsertBoundaryToken _endToken;  // Optional

        private InsertToken(Token parent, Scope scope)
            : base(parent, scope, new Span())
        {
        }

        public static Token Parse(Token parent, Scope scope, PreprocessorToken insertToken)
        {
            var ret = new InsertToken(parent, scope);

            var scopeIndent = scope.Indent();
            var tokens = new List<Token>();
            tokens.Add(ret._startToken = new InsertBoundaryToken(ret, scope, insertToken.Span, true));

            while (true)
            {
                var token = scope.File.ParseToken(parent, scopeIndent);
                if (token == null) break;
                if (token.GetType() == typeof(PreprocessorToken) && token.Text == "#endinsert")
                {
                    tokens.Add(ret._endToken = new InsertBoundaryToken(ret, scope, token.Span, false));
                    break;
                }
                else tokens.Add(token);
            }

            ret._tokens = tokens.ToArray();
            ret.Span = new Span(ret._tokens.First().Span.Start, ret._tokens.Last().Span.End);
            return ret;
        }

        IEnumerable<Token> IGroupToken.SubTokens
        {
            get { return _tokens; }
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("InsertToken");
            xml.WriteAttributeString("Span", Span.ToString());
            foreach (var token in _tokens) token.DumpTree(xml);
            xml.WriteEndElement();
        }

        public override IEnumerable<Span> HiddenRegions
        {
            get
            {
                if (_startToken != null && _endToken != null && _startToken.Span.End.LineNum < _endToken.Span.Start.LineNum)
                {
                    return new Span[] { new Span(File.FindEndOfLine(_startToken.Span.End), File.FindEndOfPreviousLine(_endToken.Span.Start)) };
                }
                else
                {
                    return new Span[0];
                }
            }
        }

        private class InsertBoundaryToken : Token, IBraceMatchingToken
        {
            private bool _start;

            public InsertBoundaryToken(InsertToken parent, Scope scope, Span span, bool start)
                : base(parent, scope, span)
            {
                _start = start;
            }

            IEnumerable<Token> IBraceMatchingToken.BraceMatchingTokens
            {
                get
                {
                    var parent = Parent as InsertToken;
                    yield return parent._startToken;
                    if (parent._endToken != null) yield return parent._endToken;
                }
            }

            public override void DumpTree(System.Xml.XmlWriter xml)
            {
                xml.WriteStartElement("InsertBoundary");
                xml.WriteAttributeString("start", _start.ToString());
                xml.WriteAttributeString("Span", Span.ToString());
                xml.WriteEndElement();
            }
        }
    }
}
