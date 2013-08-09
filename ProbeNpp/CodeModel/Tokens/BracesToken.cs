using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
	internal class BracesToken : Token, IGroupToken
	{
		private List<Token> _tokens = new List<Token>();
		private Token _openToken = null;
		private Token _closeToken = null;

		private BracesToken(Token parent, Scope scope, Span span)
			: base(parent, scope, span)
		{
		}

		public static BracesToken Parse(Token parent, Scope scope)
		{
			var startPos = scope.File.Position;
			scope.File.MoveNext();

			var indentScope = scope.IndentNonRoot();
			indentScope.Hint |= ScopeHint.SuppressFunctionDefinition;

			var retToken = new BracesToken(parent, scope, new Span(startPos, startPos));
			retToken._openToken = new BraceToken(retToken, scope, new Span(startPos, scope.File.Position), retToken, true);
			retToken._tokens.Add(retToken._openToken);

			var closeToken = null as Token;
			var done = false;

			while (!scope.File.EndOfFile && !done)
			{
				var token = scope.File.ParseToken(retToken, indentScope);
				if (token == null) break;
				if (token.GetType() == typeof(OperatorToken) && (token as OperatorToken).Text == "}")
				{
					retToken._closeToken = new BraceToken(retToken, scope, token.Span, retToken, false);
					retToken._tokens.Add(retToken._closeToken);
					done = true;
				}
				else
				{
					retToken._tokens.Add(token);
				}
			}

			retToken.Span = new Span(startPos, scope.File.Position);
			return retToken;
		}

		IEnumerable<Token> IGroupToken.SubTokens
		{
			get { return _tokens; }
		}

		public override void DumpTree(System.Xml.XmlWriter xml)
		{
			xml.WriteStartElement("Braces");
			xml.WriteAttributeString("span", Span.ToString());
			foreach (var t in _tokens) t.DumpTree(xml);
			xml.WriteEndElement();
		}

		public Token OpenToken
		{
			get { return _openToken; }
		}

		public Token CloseToken
		{
			get { return _closeToken; }
		}

		public override IEnumerable<Span> HiddenRegions
		{
			get
			{
				if (_openToken != null && _closeToken != null && _closeToken.Span.Start.LineNum > _openToken.Span.End.LineNum)
				{
					foreach (var region in base.HiddenRegions) yield return region;
					yield return new Span(_openToken.Span.End, _closeToken.Span.Start);
				}
				else
				{
					foreach (var region in base.HiddenRegions) yield return region;
				}
			}
		}

		public override IEnumerable<AutoCompletionItem> GetAutoCompletionItems(Position pos)
		{
			if (Span.Contains(pos))
			{
				foreach (var token in _tokens)
				{
					foreach (var item in token.GetAutoCompletionItems(pos))
					{
						yield return item;
					}
				}
			}
		}
	}
}
