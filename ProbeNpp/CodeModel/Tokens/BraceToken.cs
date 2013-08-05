using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class BraceToken : Token, IBraceMatchingToken
    {
        private BracesToken _braces;
        private bool _open;

        public BraceToken(Token parent, Scope scope, Span span, BracesToken braces, bool open)
            : base(parent, scope, span)
        {
            _braces = braces;
            _open = open;
        }

        IEnumerable<Token> IBraceMatchingToken.BraceMatchingTokens
        {
            get
            {
                yield return _braces.OpenToken;
                if (_braces.CloseToken != null) yield return _braces.CloseToken;
            }
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("Brace");
            xml.WriteAttributeString("open", _open.ToString());
            xml.WriteAttributeString("span", Span.ToString());
            xml.WriteEndElement();
        }
    }
}
