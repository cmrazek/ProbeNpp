using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProbeNpp.TokenParser;

namespace ProbeNpp
{
	internal class CodeStringBuilder
	{
		private StringBuilder _sb = new StringBuilder();
		private TokenType _lastType = TokenType.Unknown;
		private string _lastText = "";

		private static readonly char[] k_noSpaceOperators = "()[]{},.".ToCharArray();

		public void Append(TokenType type, string text)
		{
			if (type == TokenType.WhiteSpace || type == TokenType.Comment || string.IsNullOrEmpty(text)) return;

			if (_sb.Length == 0)
			{
				_sb.Append(text);
			}
			else
			{
				if (text.Length == 1 && k_noSpaceOperators.Contains(text[0]))
				{
					_sb.Append(text);
				}
				else if (_lastText == "(")
				{
					_sb.Append(text);
				}
				else
				{
					_sb.Append(" ");
					_sb.Append(text);
				}
			}

			_lastType = type;
			_lastText = text;
		}

		public void Append(Parser parser)
		{
			Append(parser.TokenType, parser.TokenText);
		}

		public override string ToString()
		{
			return _sb.ToString();
		}
	}
}
