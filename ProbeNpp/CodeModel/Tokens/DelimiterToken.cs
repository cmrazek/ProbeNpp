using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class DelimiterToken : Token
    {
        public DelimiterToken(Token parent, Scope scope, Span span)
            : base(parent, scope, span)
        {
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("Delimiter");
            xml.WriteAttributeString("span", Span.ToString());
            xml.WriteEndElement();
        }
    }
}
