using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    internal class BracketsToken : Token, IGroupToken
    {
        private List<Token> _tokens = new List<Token>();
        private BracketToken _openToken = null;
        private BracketToken _closeToken = null;

        private BracketsToken(Token parent, Scope scope, Span span)
            : base(parent, scope, span)
        {
        }

        public static BracketsToken Parse(Token parent, Scope scope)
        {
            var file = scope.File;
            var startPos = file.Position;
            file.MoveNext();

            var scopeIndent = scope.IndentNonRoot();
            scopeIndent.Hint |= ScopeHint.SuppressVarDecl | ScopeHint.SuppressFunctionDefinition | ScopeHint.SuppressFunctionCall;

            var retToken = new BracketsToken(parent, scope, new Span(startPos, startPos));
            retToken._openToken = new BracketToken(retToken, scope, new Span(startPos, file.Position), retToken, true);
            retToken._tokens.Add(retToken._openToken);

            var closeToken = null as Token;
            var done = false;

            while (!file.EndOfFile && !done)
            {
                var token = file.ParseToken(retToken, scopeIndent);
                if (token != null)
                {
                    if (token.GetType() == typeof(OperatorToken) && token.Text == ")")
                    {
                        retToken._closeToken = new BracketToken(retToken, scope, token.Span, retToken, false);
                        retToken._tokens.Add(retToken._closeToken);
                        done = true;
                    }
                    else
                    {
                        retToken._tokens.Add(token);
                    }
                }
            }

            retToken.Span = new Span(startPos, file.Position);
            return retToken;
        }

        IEnumerable<Token> IGroupToken.SubTokens
        {
            get { return _tokens; }
        }

        public override void DumpTree(System.Xml.XmlWriter xml)
        {
            xml.WriteStartElement("Brackets");
            xml.WriteAttributeString("span", Span.ToString());
            foreach (var t in _tokens) t.DumpTree(xml);
            xml.WriteEndElement();
        }

        public BracketToken OpenToken
        {
            get { return _openToken; }
        }

        public BracketToken CloseToken
        {
            get { return _closeToken; }
        }

        public IEnumerable<AutoCompletionItem> GetArgsAutoCompletionItems()
        {
            var list = new List<Token>();

            foreach (var token in _tokens)
            {
                var type = token.GetType();
                if (type == typeof(BracketToken)) continue;

                if (type == typeof(DelimiterToken))
                {
                    if (list.Count > 0)
                    {
                        var item = FindArgAutoCompletionItem(list);
                        if (item.HasValue) yield return item.Value;
                    }
                }
                else
                {
                    list.Add(token);
                }
            }

            if (list.Count > 0)
            {
                var item = FindArgAutoCompletionItem(list);
                if (item.HasValue) yield return item.Value;
            }
        }

        private AutoCompletionItem? FindArgAutoCompletionItem(List<Token> list)
        {
            var desc = new StringBuilder();

            foreach (var token in list)
            {
                if (desc.Length > 0) desc.Append(" ");
                desc.Append(token.NormalizedText);
            }

            var nameToken = list.LastOrDefault(x => x.GetType() == typeof(IdentifierToken)) as IdentifierToken;
            if (nameToken != null)
            {
                return new AutoCompletionItem(nameToken.Text, nameToken.Text, desc.ToString(), AutoCompletionType.Variable);
            }
            return null;
        }
    }
}
