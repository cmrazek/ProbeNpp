using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class PreprocessorToken : Token
    {
        private string _text;
        private string _instructions;

        public PreprocessorToken(Token parent, Scope scope, Span span, string text)
            : base(parent, scope, span)
        {
            _text = text;
        }

        public override string Text
        {
            get { return _text; }
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("Preprocessor");
            xml.WriteAttributeString("span", Span.ToString());
            xml.WriteString(_text);
            xml.WriteEndElement();
        }

        public string Instructions
        {
            get { return _instructions; }
            set { _instructions = value; }
        }
    }
}
