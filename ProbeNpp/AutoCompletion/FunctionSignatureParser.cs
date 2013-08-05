using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NppSharp;

namespace ProbeNpp.AutoCompletion
{
	internal class FunctionSignatureParser
	{
		private ProbeNppPlugin _app;
		private StringBuilder _source = new StringBuilder();
		private string _funcName = "";
		private int _commaCount = 0;

		private enum ParseState
		{
			Found,
			NotEnoughSource,
			BadSyntax
		}

		public FunctionSignatureParser(ProbeNppPlugin app)
		{
			_app = app;
		}

		public bool GetFuncSigName(TextLocation currLoc)
		{
			_source.Clear();

			var lineNum = currLoc.Line;
			var lineText = _app.GetText(_app.GetLineStartPos(currLoc.Line), currLoc);
			_source.Append(RemoveComments(lineText, _app.GetLineState(lineNum)));

			while (true)
			{
				switch (ParseFunctionSigFromSource())
				{
					case ParseState.Found:
						return true;
					case ParseState.BadSyntax:
						return false;
					default:	// ParseState.NotEnoughSource:
						lineNum--;
						if (lineNum < 1) return false;
						_source.AppendLine(RemoveComments(_app.GetLineText(lineNum), _app.GetLineState(lineNum)));
						break;
				}
			}
		}

		private string RemoveComments(string line, int lineState)
		{
			if ((lineState & ProbeLexer.State_InsideComment) != 0)
			{
				var index = line.IndexOf("*/");
				if (index < 0) return "";
				line = line.Substring(index + 2);
			}

			var commentIndex = line.IndexOf("//");
			if (commentIndex >= 0) line = line.Substring(commentIndex);

			return line;
		}

		private ParseState ParseFunctionSigFromSource()
		{
			var parser = new TokenParser.Parser(_source.ToString());
			var lastTokenText = "";
			TokenParser.TokenType lastTokenType = TokenParser.TokenType.Unknown;

			_funcName = "";
			_commaCount = 0;

			while (parser.Read())
			{
				if (parser.TokenText == "(")
				{
					if (!ParseNestable(parser, ")", true))
					{
						if (lastTokenType == TokenParser.TokenType.Word)
						{
							_funcName = lastTokenText;
							return ParseState.Found;
						}
						return ParseState.BadSyntax;
					}
				}
				else if (parser.TokenText == ";")
				{
					return ParseState.BadSyntax;
				}
				else if (parser.TokenText == ",")
				{
					_commaCount++;
				}

				lastTokenText = parser.TokenText;
				lastTokenType = parser.TokenType;
			}

			return ParseState.NotEnoughSource;
		}

		private bool ParseNestable(TokenParser.Parser parser, string endToken, bool countCommas)
		{
			while (parser.Read())
			{
				if (parser.TokenText == endToken) return true;

				switch (parser.TokenText)
				{
					case "(":
						if (!ParseNestable(parser, ")", false)) return false;
						break;
					case "{":
						if (!ParseNestable(parser, "}", false)) return false;
						break;
					case "[":
						if (!ParseNestable(parser, "]", false)) return false;
						break;
					case ",":
						if (countCommas) _commaCount++;
						break;
				}
			}

			return false;
		}

		public string FunctionName
		{
			get { return _funcName; }
		}

		public int CommaCount
		{
			get { return _commaCount; }
		}

	}
}
