using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class UnknownToken : Token
    {
        string _text;

        public UnknownToken(Token parent, Scope scope, Span span, string text)
            : base(parent, scope, span)
        {
            _text = text;
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("Unknown");
            xml.WriteAttributeString("span", Span.ToString());
            xml.WriteString(_text);
            xml.WriteEndElement();
        }
    }
}
