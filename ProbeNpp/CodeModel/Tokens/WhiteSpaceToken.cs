using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class WhiteSpaceToken : Token
    {
        public WhiteSpaceToken(Token parent, Scope scope, Span span)
            : base(parent, scope, span)
        {
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("WhiteSpace");
            xml.WriteAttributeString("span", Span.ToString());
            xml.WriteEndElement();
        }
    }
}
