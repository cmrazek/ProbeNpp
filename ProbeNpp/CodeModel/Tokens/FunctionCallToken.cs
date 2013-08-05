using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal sealed class FunctionCallToken : Token, IGroupToken
    {
        private IdentifierToken _nameToken;
        private BracketsToken _argsToken;

        public FunctionCallToken(Token parent, Scope scope, IdentifierToken nameToken, BracketsToken argsToken)
            : base(parent, scope, new Span(nameToken.Span.Start, argsToken.Span.End))
        {
            _nameToken = nameToken;
            _argsToken = argsToken;

            _nameToken.Parent = this;
            _argsToken.Parent = this;
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("FunctionCall");
            xml.WriteAttributeString("name", _nameToken.Text);
            xml.WriteAttributeString("span", Span.ToString());
            _argsToken.DumpTree(xml);
            xml.WriteEndElement();
        }

        IEnumerable<Token> IGroupToken.SubTokens
        {
            get { return new Token[] { _nameToken, _argsToken }; }
        }

        public IdentifierToken NameToken
        {
            get { return _nameToken; }
        }

        public BracketsToken ArgsToken
        {
            get { return _argsToken; }
        }
    }
}
