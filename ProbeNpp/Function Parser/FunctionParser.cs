using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace ProbeNpp
{
	internal class FunctionParser
	{
		private List<Function> _funcs = null;
		private TokenParser.Parser _parser = null;

		public IEnumerable<Function> Parse(string source)
		{
			_funcs = new List<Function>();

			_parser = new TokenParser.Parser(source);
			_parser.ReturnWhiteSpace = false;
			_parser.ReturnComments = false;

			while (!_parser.EndOfFile)
			{
				var startPos = _parser.Position;

				if (!ProcessDefine() && !ProcessFunction())
				{
					// Unable to find any recognizable pattern, so just skip the next token.
					_parser.Read();
				}
			}

			// Expand the function line ranges to fill in the function bodies.
			Function lastFunc = null;
			foreach (var func in _funcs)
			{
				if (lastFunc != null)
				{
					lastFunc.EndLine = func.StartLine > lastFunc.StartLine ? func.StartLine - 1 : lastFunc.StartLine;
				}
				lastFunc = func;
			}

			return _funcs;
		}

		bool ProcessFunction()
		{
			var startPos = _parser.Position;
			var retVal = false;
			try
			{
				var sig = new CodeStringBuilder();

				// Function name
				if (!_parser.Read() || _parser.TokenType != TokenParser.TokenType.Word) return false;
				var funcName = _parser.TokenText;
				var funcStartPos = _parser.TokenStartPostion;
				sig.Append(_parser);

				// Exclude probe keywords
				switch (funcName)
				{
					case "and":
					case "char":
					case "enum":
					case "numeric":
					case "oldvalue":
					case "or":
					case "else":
					case "for":
					case "if":
					case "select":
					case "switch":
					case "while":
						return false;
				}

				// Next char should be '(' - start of argument brackets
				if (!_parser.Read() || _parser.TokenText != "(") return false;
				sig.Append(_parser);
				if (!ParseNestable(")", false, sig)) return false;

				// Optional attributes.
				while (_parser.Read())
				{
					if (_parser.TokenText == "{") break;

					sig.Append(_parser);

					switch (_parser.TokenText)
					{
						case "(":
							ParseNestable(")", false, sig);
							break;
						case "[":
							ParseNestable("]", false, sig);
							break;
						case ";":
						case ")":
						case "]":
						case "}":
							return false;
					}
				}

				// Add the function to the list
				AddFunction(funcName, funcStartPos.LineNum, _parser.Position.LineNum, sig.ToString());

				return retVal = true;
			}
			finally
			{
				if (!retVal)
				{
					_parser.Position = startPos;
				}
			}
		}

		void AddFunction(string name, int startLine, int endLine, string signature)
		{
			var uniqueName = string.Concat(name, ":", (from f in _funcs where f.Name == name select f).Count());

			Function func = new Function(name, uniqueName, startLine, endLine);
			func.Signature = signature;
			_funcs.Add(func);
		}

		bool ParseNestable(string termToken, bool eofOk, CodeStringBuilder sig)
		{
			while (_parser.Read())
			{
				sig.Append(_parser);
				if (_parser.TokenText == termToken) return true;
				else if (_parser.TokenText == "(") ParseNestable(")", eofOk, sig);
				else if (_parser.TokenText == "{") ParseNestable("}", eofOk, sig);
				else if (_parser.TokenText == "[") ParseNestable("]", eofOk, sig);
			}

			return eofOk;
		}

		private bool ProcessDefine()
		{
			var startPos = _parser.Position;
			if (!_parser.Read() || _parser.TokenText != "#define")
			{
				_parser.Position = startPos;
				return false;
			}

			var definePos = _parser.TokenStartPostion;
			var pos = definePos;
			do
			{
				if (!_parser.Read()) return true;
				pos = _parser.TokenStartPostion;
			}
			while (pos.LineNum == definePos.LineNum);

			_parser.Position = pos;
			return true;
		}
	}
}
