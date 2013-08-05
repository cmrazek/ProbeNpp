using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class BracketToken : Token, IBraceMatchingToken
    {
        private BracketsToken _bracketsToken = null;
        private bool _open = false;

        public BracketToken(Token parent, Scope scope, Span span, BracketsToken bracketsToken, bool open)
            : base(parent, scope, span)
        {
            _bracketsToken = bracketsToken;
            _open = open;
        }

        IEnumerable<Token> IBraceMatchingToken.BraceMatchingTokens
        {
            get
            {
                yield return _bracketsToken.OpenToken;
                if (_bracketsToken.CloseToken != null) yield return _bracketsToken.CloseToken;
            }
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("Bracket");
            xml.WriteAttributeString("open", _open.ToString());
            xml.WriteAttributeString("span", Span.ToString());
            xml.WriteEndElement();
        }
    }
}
