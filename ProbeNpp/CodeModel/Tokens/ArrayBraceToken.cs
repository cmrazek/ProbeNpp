using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class ArrayBraceToken : Token, IBraceMatchingToken
    {
        private ArrayBracesToken _braces;
        private bool _open;

        public ArrayBraceToken(Token parent, Scope scope, Span span, ArrayBracesToken braces, bool open)
            : base(parent, scope, span)
        {
            _braces = braces;
            _open = open;
        }

        IEnumerable<Token> IBraceMatchingToken.BraceMatchingTokens
        {
            get
            {
                var ret = new List<Token>();
                ret.Add(_braces.OpenToken);
                if (_braces.CloseToken != null) ret.Add(_braces.CloseToken);
                return ret;
            }
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("ArrayBrace");
            xml.WriteAttributeString("open", _open.ToString());
            xml.WriteAttributeString("span", Span.ToString());
            xml.WriteEndElement();
        }
    }
}
