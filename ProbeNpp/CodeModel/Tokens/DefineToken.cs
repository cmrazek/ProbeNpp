using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class DefineToken : Token, IAutoCompletionSource, IFunctionSignatureSource
    {
        private Token[] _tokens;
        private IdentifierToken _nameToken;
        private BracketsToken _argsToken;   // Optional
        private Token[] _bodyTokens;
        private bool _isDataType = false;

        private DefineToken(Token parent, Scope scope, IEnumerable<Token> tokens, IdentifierToken nameToken, BracketsToken argsToken, IEnumerable<Token> bodyTokens)
            : base(parent, scope, new Span(tokens.First().Span.Start, tokens.Last().Span.End))
        {
            _tokens = tokens.ToArray();
            foreach (var token in _tokens) token.Parent = this;

            _nameToken = nameToken;
            _argsToken = argsToken;
            _bodyTokens = bodyTokens.ToArray();

            if (_argsToken == null && _bodyTokens.Length == 1 &&
                (_bodyTokens[0].GetType() == typeof(DataTypeToken) || _bodyTokens[0].GetType() == typeof(DataTypeKeywordToken)))
            {
                _isDataType = true;
                //File.AddDefinedDataType(_nameToken.Text);
            }
        }

        private static Regex _rxBraceMacro = new Regex(@".*\{\s*$");

        public static Token Parse(Token parent, Scope scope, PreprocessorToken prepToken)
        {
            var startPos = scope.File.Position;

            var defineScope = scope;
            defineScope.Hint |= ScopeHint.SuppressFunctionDefinition | ScopeHint.SuppressVarDecl | ScopeHint.SuppressFunctionCall;

            var lineNum = prepToken.Span.Start.LineNum;

            var tokens = new List<Token>();
            var bodyTokens = new List<Token>();
            tokens.Add(prepToken);

            // Define name
            var token = scope.File.ParseSingleToken(parent, defineScope);
            if (token == null || token.GetType() != typeof(IdentifierToken) || token.Span.Start.LineNum != lineNum)
            {
                scope.File.Position = startPos;
                return prepToken;
            }
            var nameToken = token as IdentifierToken;
            tokens.Add(nameToken);

            var resetPos = scope.File.Position;

            // Arguments
            BracketsToken argsToken = null;
            token = scope.File.ParseToken(parent, defineScope);
            if (token == null || token.Span.Start.LineNum != lineNum)
            {
                scope.File.Position = resetPos;
                return new DefineToken(parent, scope, tokens, nameToken, null, bodyTokens);
            }

            if (token.GetType() == typeof(BracketsToken))
            {
                argsToken = token as BracketsToken;
                tokens.Add(argsToken);
                resetPos = scope.File.Position;
                token = scope.File.ParseToken(parent, defineScope);
                if (token == null || token.Span.Start.LineNum != lineNum)
                {
                    scope.File.Position = resetPos;
                    return new DefineToken(parent, scope, tokens, nameToken, argsToken, bodyTokens);
                }
            }

            // Body enclosed in { }
            if (token.GetType() == typeof(BracesToken))
            {
                tokens.Add(token);
                bodyTokens.Add(token);
                return new DefineToken(parent, scope, tokens, nameToken, argsToken, bodyTokens);
            }


            var done = false;
            while (!done)
            {
                if (token.GetType() == typeof(UnknownToken) && token.Text == "\\")
                {
                    // Line end token
                    lineNum++;
                    tokens.Add(token);
                }
                else
                {
                    tokens.Add(token);
                    bodyTokens.Add(token);
                }

                if (!scope.File.SkipWhiteSpaceAndComments()) break;
                var pos = scope.File.Position;
                if (pos.LineNum != lineNum) break;

                token = scope.File.ParseToken(parent, defineScope);
                if (token == null || token.Span.Start.LineNum != lineNum)
                {
                    scope.File.Position = pos;
                    done = true;
                }
            }

            return new DefineToken(parent, scope, tokens, nameToken, argsToken, bodyTokens);
        }

        public override bool BreaksStatement
        {
            get
            {
                return true;
            }
        }

        IEnumerable<AutoCompletionItem> IAutoCompletionSource.AutoCompletionItems
        {
            get
            {
                if (_bodyTokens.Any())
                {
                    return new AutoCompletionItem[]
                    {
                        new AutoCompletionItem(_nameToken.Text, _nameToken.Text, NormalizedText, _isDataType ? AutoCompletionType.DataType : AutoCompletionType.Constant)
                    };
                }
                else
                {
                    return new AutoCompletionItem[0];
                }
            }
        }

        IEnumerable<FunctionSignature> IFunctionSignatureSource.FunctionSignatures
        {
            get
            {

                if (_argsToken != null)
                {
                    var sig = new StringBuilder();
                    sig.Append("#define");
                    sig.Append(" ");
                    sig.Append(_nameToken.Text);
                    sig.Append(" ");
                    sig.Append(_argsToken.NormalizedText);

                    return new FunctionSignature[] { new FunctionSignature(_nameToken.Text, sig.ToString(), "") };
                }
                else
                {
                    return new FunctionSignature[0];
                }
            }
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("Define");
            xml.WriteAttributeString("name", _nameToken.Text);
            xml.WriteAttributeString("span", Span.ToString());
            if (_argsToken != null)
            {
                xml.WriteStartElement("DefineArgs");
                _argsToken.DumpTree(xml);
                xml.WriteEndElement();
            }
            if (_bodyTokens.Length > 0)
            {
                xml.WriteStartElement("DefineBody");
                foreach (var token in _bodyTokens) token.DumpTree(xml);
                xml.WriteEndElement();
            }
            xml.WriteEndElement();
        }
    }
}
