using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class ExternFunctionToken : Token, IGroupToken, IAutoCompletionSource, IFunctionSignatureSource
    {
        private Token[] _tokens;
        private Token _dataTypeToken;   // Optional
        private IdentifierToken _nameToken;
        private BracketsToken _argsToken;
        private FunctionSignature _signature = null;

        public ExternFunctionToken(Token parent, Scope scope, IEnumerable<Token> tokens, Token dataTypeToken, IdentifierToken nameToken, BracketsToken argsToken)
            : base(parent, scope, new Span(tokens.First().Span.Start, tokens.Last().Span.End))
        {
            _tokens = tokens.ToArray();
            _dataTypeToken = dataTypeToken;
            _nameToken = nameToken;
            _argsToken = argsToken;
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("ExternFunction");
            xml.WriteAttributeString("name", _nameToken.Text);
            if (_dataTypeToken != null) xml.WriteAttributeString("dataType", _dataTypeToken.Text);
            xml.WriteAttributeString("args", _argsToken.Text);
            xml.WriteAttributeString("span", Span.ToString());
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
                string desc;
                if (_dataTypeToken != null) desc = string.Concat(_dataTypeToken.NormalizedText, " ", _nameToken.Text, _argsToken.NormalizedText);
                else desc = string.Concat(_nameToken.Text, _argsToken.NormalizedText);

                return new AutoCompletionItem[]
                {
                    new AutoCompletionItem(_nameToken.Text, _nameToken.Text, desc, AutoCompletionType.Function)
                };
            }
        }

        IEnumerable<FunctionSignature> IFunctionSignatureSource.FunctionSignatures
        {
            get
            {
                if (_signature == null)
                {
                    var sb = new StringBuilder();
                    if (_dataTypeToken != null)
                    {
                        sb.Append(_dataTypeToken.NormalizedText);
                        sb.Append(" ");
                    }
                    sb.Append(_nameToken.Text);

                    var needSpace = false;
                    foreach (var arg in (_argsToken as IGroupToken).SubTokens)
                    {
                        if (arg.GetType() == typeof(BracketToken))
                        {
                            sb.Append(arg.Text);
                            needSpace = false;
                        }
                        else if (arg.GetType() == typeof(DelimiterToken))
                        {
                            sb.Append(",");
                            needSpace = true;
                        }
                        else
                        {
                            if (needSpace) sb.Append(" ");
                            sb.Append(arg.NormalizedText);
                            needSpace = true;
                        }
                    }

                    _signature = new FunctionSignature(_nameToken.Text, sb.ToString(), "");
                }

                return new FunctionSignature[] { _signature };
            }
        }
    }
}
