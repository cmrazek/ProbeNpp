﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class StringLiteralToken : Token
    {
        private string _text;

        public StringLiteralToken(Token parent, Scope scope, Span span, string text)
            : base(parent, scope, span)
        {
            _text = text;
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("StringLiteral");
            xml.WriteAttributeString("span", Span.ToString());
            xml.WriteString(_text);
            xml.WriteEndElement();
        }
    }
}
