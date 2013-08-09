using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
	internal class FunctionToken : Token, IGroupToken, IAutoCompletionSource, IFunctionSignatureSource
	{
		private Token[] _tokens;
		private Token _dataTypeToken;
		private IdentifierToken _nameToken;
		private BracketsToken _argsToken;
		private BracesToken _scopeToken;
		private FunctionSignature _signature = null;

		public FunctionToken(Token parent, Scope scope, IEnumerable<Token> tokens, Token dataTypeToken, IdentifierToken nameToken, BracketsToken argsToken, BracesToken scopeToken)
			: base(parent, scope, new Span(tokens.First().Span.Start, tokens.Last().Span.End))
		{
			_tokens = tokens.ToArray();
			
			_dataTypeToken = dataTypeToken;
			if (_dataTypeToken != null) _dataTypeToken.Parent = this;
			
			_nameToken = nameToken;
			_nameToken.Parent = this;

			_argsToken = argsToken;
			_argsToken.Parent = this;

			_scopeToken = scopeToken;
			_scopeToken.Parent = this;

			foreach (var tok in _tokens) tok.Parent = this;
		}

		public override void DumpTree(System.Xml.XmlWriter xml)
		{
			xml.WriteStartElement("Function");
			xml.WriteAttributeString("name", _nameToken.Text);
			xml.WriteAttributeString("span", Span.ToString());
			if (_dataTypeToken != null)
			{
				xml.WriteStartElement("FunctionDataType");
				_dataTypeToken.DumpTree(xml);
				xml.WriteEndElement();  // FunctionDataType
			}
			xml.WriteStartElement("FunctionArgs");
			_argsToken.DumpTree(xml);
			xml.WriteEndElement();  // FunctionArgs
			xml.WriteStartElement("FunctionScope");
			_scopeToken.DumpTree(xml);
			xml.WriteEndElement();  // FunctionScope
			xml.WriteEndElement();  // Function
		}

		public override IEnumerable<FunctionToken> LocalFunctions
		{
			get
			{
				return new FunctionToken[] { this };
			}
		}

		public string Name
		{
			get { return _nameToken.Text; }
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
				else desc = string.Concat(" ", _nameToken.Text, _argsToken.NormalizedText);

				return new AutoCompletionItem[]
				{
					new AutoCompletionItem(_nameToken.Text, _nameToken.Text, desc, AutoCompletionType.Function)
				};
			}
		}

		public override IEnumerable<AutoCompletionItem> GetAutoCompletionItems(Position pos)
		{
			foreach (var item in base.GetAutoCompletionItems(pos)) yield return item;

			if (_scopeToken.Span.Contains(pos))
			{
				foreach (var item in _argsToken.GetArgsAutoCompletionItems()) yield return item;
			}
		}

		public override bool BreaksStatement
		{
			get
			{
				return true;
			}
		}

		public static FunctionToken TryParse(Token parent, Scope scope, Token dataTypeToken, IdentifierToken nameToken)
		{
			if ((scope.Hint & ScopeHint.SuppressFunctionDefinition) != 0) return null;

			var startPos = scope.File.Position;

			var tokens = new List<Token>();
			if (dataTypeToken != null) tokens.Add(dataTypeToken);
			tokens.Add(nameToken);

			var argsScope = scope;
			argsScope.Hint |= ScopeHint.FunctionArgs;

			var token = scope.File.ParseSingleToken(parent, argsScope);
			if (token == null || token.GetType() != typeof(BracketsToken))
			{
				scope.File.Position = startPos;
				return null;
			}

			var argsToken = token as BracketsToken;
			tokens.Add(argsToken);

			var innerScope = scope;
			innerScope.Hint |= ScopeHint.SuppressFunctionDefinition | ScopeHint.NotOnRoot;

			var scopeToken = null as BracesToken;
			var optionsTokens = new List<Token>();

			var done = false;
			while (!done)
			{
				token = scope.File.ParseSingleToken(parent, innerScope);
				if (token == null)
				{
					scope.File.Position = startPos;
					return null;
				}

				if (token.GetType() == typeof(BracesToken))
				{
					tokens.Add(token);
					scopeToken = token as BracesToken;
					done = true;
				}
				else if (token.BreaksStatement)
				{
					scope.File.Position = startPos;
					return null;
				}
				else
				{
					tokens.Add(token);
					optionsTokens.Add(token);
				}
			}

			return new FunctionToken(parent, scope, tokens, dataTypeToken, nameToken, argsToken, scopeToken);
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
