using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class ExternVariableToken : Token, IGroupToken, IAutoCompletionSource
    {
        private Token[] _tokens;
        private Token _dataTypeToken;   // Optional
        private WordToken[] _nameTokens;

        public ExternVariableToken(Token parent, Scope scope, IEnumerable<Token> tokens, Token dataTypeToken, IEnumerable<WordToken> nameTokens)
            : base(parent, scope, new Span(tokens.First().Span.Start, tokens.Last().Span.End))
        {
            _tokens = tokens.ToArray();
            _dataTypeToken = dataTypeToken;
            _nameTokens = nameTokens.ToArray();
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("ExternVar");
            if (_dataTypeToken != null) xml.WriteAttributeString("dataType", _dataTypeToken.Text);
            xml.WriteAttributeString("span", Span.ToString());
            foreach (var name in _nameTokens) name.DumpTree(xml);
            xml.WriteEndElement();
        }

        IEnumerable<Token> IGroupToken.SubTokens
        {
            get { return _tokens; }
        }

        IEnumerable<AutoCompletionItem> IAutoCompletionSource.AutoCompletionItems
        {
            get
            {
                foreach (var name in _nameTokens)
                {
                    string desc;
                    if (_dataTypeToken != null) desc = string.Concat(_dataTypeToken.NormalizedText, " ", name.Text);
                    else desc = name.Text;

                    yield return new AutoCompletionItem(name.Text, name.Text, desc, AutoCompletionType.Variable);
                }
            }
        }
    }
}
