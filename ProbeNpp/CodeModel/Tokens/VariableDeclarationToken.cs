using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
	internal class VariableDeclarationToken : Token, IAutoCompletionSource
	{
		private Token _dataTypeToken;
		private List<IdentifierToken> _nameTokens = new List<IdentifierToken>();
		private Token[] _tokens;

		private VariableDeclarationToken(Token parent, Scope scope, IEnumerable<Token> tokens, Token dataTypeToken, List<IdentifierToken> nameTokens)
			: base(parent, scope, new Span(tokens.First().Span.Start, tokens.Last().Span.End))
		{
			_dataTypeToken = dataTypeToken;
			_dataTypeToken.Parent = this;

			_nameTokens = nameTokens;

			_tokens = tokens.ToArray();
			foreach (Token tok in _tokens) tok.Parent = this;
		}

		public override void DumpTree(System.Xml.XmlWriter xml)
		{
			xml.WriteStartElement("VarDecl");
			foreach (var token in _tokens) token.DumpTree(xml);
			xml.WriteEndElement();  // VarDecl
		}

		IEnumerable<AutoCompletionItem> IAutoCompletionSource.AutoCompletionItems
		{
			get
			{
				foreach (var token in _nameTokens)
				{
					string desc;
					if (_dataTypeToken != null) desc = string.Concat(_dataTypeToken.NormalizedText, " ", token.Text);
					else desc = token.Text;

					yield return new AutoCompletionItem(token.Text, token.Text, desc, AutoCompletionType.Variable);
				}
			}
		}

		public override bool BreaksStatement
		{
			get
			{
				return true;
			}
		}

		public static VariableDeclarationToken TryParse(Token parent, Scope scope, Token dataTypeToken, IdentifierToken nameToken)
		{
			if (scope.Hint.HasFlag(ScopeHint.SuppressVarDecl)) return null;

			var startPos = scope.File.Position;

			var tokens = new List<Token>();
			tokens.Add(dataTypeToken);
			tokens.Add(nameToken);

			var nameTokens = new List<IdentifierToken>();
			nameTokens.Add(nameToken);

			if (scope.Hint.HasFlag(ScopeHint.FunctionArgs))
			{
				var token = scope.File.ParseSingleToken(parent, scope);
				if (token == null || (token.GetType() != typeof(StatementEndToken) && token.GetType() != typeof(DelimiterToken)))
				{
					scope.File.Position = startPos;
					return null;
				}
				tokens.Add(token);
			}
			else
			{
				var done = false;
				var needDelim = true;
				while (!done)
				{
					var token = scope.File.ParseSingleToken(parent, scope);
					if (token == null)
					{
						scope.File.Position = startPos;
						return null;
					}

					if (token.GetType() == typeof(StatementEndToken))
					{
						done = true;
						tokens.Add(token);
					}
					else if (needDelim && token.GetType() == typeof(DelimiterToken))
					{
						needDelim = false;
						tokens.Add(token);
					}
					else if (!needDelim && token.GetType() == typeof(IdentifierToken))
					{
						needDelim = true;
						tokens.Add(token);
						nameTokens.Add(token as IdentifierToken);
					}
					else if (needDelim && token.GetType() == typeof(ArrayBracesToken))
					{
						// This is an array variable.
						tokens.Add(token);
					}
					else
					{
						scope.File.Position = startPos;
						return null;
					}
				}
			}

			return new VariableDeclarationToken(parent, scope, tokens, dataTypeToken, nameTokens);
		}
	}
}
