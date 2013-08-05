using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class DataTypeKeywordToken : WordToken
    {
        public DataTypeKeywordToken(Token parent, Scope scope, Span span, string text)
            : base(parent, scope, span, text)
        { }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("DataTypeKeyword");
            xml.WriteAttributeString("span", Span.ToString());
            xml.WriteString(Text);
            xml.WriteEndElement();
        }
    }
}
