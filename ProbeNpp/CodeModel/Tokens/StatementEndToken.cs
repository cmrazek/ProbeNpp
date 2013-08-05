using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class StatementEndToken : Token
    {
        public StatementEndToken(Token parent, Scope scope, Span span)
            : base(parent, scope, span)
        {
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("StatementEnd");
            xml.WriteAttributeString("span", Span.ToString());
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
