using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

namespace ProbeNpp
{
	internal class FunctionParser
	{
		private List<Function> _funcs = new List<Function>();
		private string _file = "";
		private char[] _fileData;
		private int _pos = 0;
		private int _length = 0;
		private int _line  = 0;

		public void Parse(string fileData)
		{
			char ch;

			_file = fileData;
			_fileData = _file.ToCharArray();
			_length = _file.Length;
			_pos = 0;
			_line = 1;

			SkipWhiteSpace();

			while (_pos < _length)
			{
				ch = _fileData[_pos];

				if (Char.IsLetter(ch) || ch == '_')
				{
					int posSave = _pos;
					int lineSave = _line;
					if (!ProcessFunction())
					{
						_pos = posSave;
						_line = lineSave;
						MoveUntilNonAlNum();
					}
				}
				else
				{
					SkipToken();
				}

				SkipWhiteSpace();
			}
		}

		bool ProcessFunction()
		{
			// Get the function name.
			int startFuncPos = _pos;
			int startLine = _line;
			MoveUntilNonAlNum();
			string funcName = _file.Substring(startFuncPos, _pos - startFuncPos);

			// check if the function name is a probe keyword. if so then don't allow it.
			// this can happen with a badly formatted function, or if it's a localization file.
			switch(funcName)
			{
				case "and":
				case "char":
				case "enum":
				case "numeric":
				case "oldvalue":
				case "or":
					// keywords that may have brackets after, which could be a source of a false-positive.
					return false;

				case "else":
				case "for":
				case "if":
				case "select":
				case "switch":
				case "while":
					// Control statements that are always inside a function, and have '{' following.
					var index = FindForward('{', ';', '}');
					if (index < 0) return false;
					MoveTo(index + 1);
					return true;
			}

			// Next char should be '('
			SkipWhiteSpace();
			if (_pos >= _length) return false;
			if (_fileData[_pos] != '(') return false;

			// Parse the argument brackets.
			if (!ParseNestable(false)) return false;

			// Optional attributes.
			char ch;
			SkipWhiteSpace();
			while (_pos < _length)
			{
				ch = _fileData[_pos];
				if (ch == '{')
				{
					break;
				}
				else if (ch == ';' || ch == '}')
                {
                    return false;
                }
                else if (ch == '(' || ch == '[')
                {
                    if (!ParseNestable(false)) return false;
                    return false;
                }
                else
                {
                    SkipToken();
                    SkipWhiteSpace();
                }
			}
			if (_pos >= _length) return false;

			// Opening brace.
			if (_pos < _length && _fileData[_pos] != '{') return false;
			//if (!ParseNestable(true)) return false;	// Don't require the function to be closed.

			// Add the function to the list
			AddFunction(funcName, startLine, _line);

			return true;
		}

		void AddFunction(string name, int startLine, int endLine)
		{
			string uniqueName = name;
			bool found = true;
			int index = 1;
			while (found)
			{
				found = false;
				foreach (Function f in _funcs)
				{
					if (f.Name == uniqueName)
					{
						found = true;
						uniqueName = string.Concat(name, index++);
						break;
					}
				}
			}

			Function func = new Function(name, uniqueName, startLine, endLine);
			_funcs.Add(func);
		}

		bool ParseNestable(bool eofOk)
		{
			// This function leaves _pos sitting on the end character

			if (_pos >= _length) return false;

			char startChar = _fileData[_pos];
			char endChar;
			char ch;

			switch(startChar)
			{
				case '(':
					endChar = ')';
					break;
				case '{':
					endChar = '}';
					break;
				case  '[':
					endChar = ']';
					break;
				default:
					return false;
			}

			NextChar();
			SkipWhiteSpace();
			while (_pos < _length)
			{
				ch = _fileData[_pos];
				if (ch == endChar)
				{
					NextChar();
					return true;
				}

				if (ch == '(' || ch == '{' || ch == '[')
				{
					if (!ParseNestable(eofOk)) return false;
				}
				else
				{
					SkipToken();
					SkipWhiteSpace();
				}
			}

			return eofOk;
		}

		void MoveUntilAlNum()
		{
			while (_pos < _length)
			{
				char ch = _fileData[_pos];
				if (Char.IsLetterOrDigit(ch) || ch == '_') return;
				NextChar();
			}
		}

		void MoveUntilNonAlNum()
		{
			while (_pos < _length)
			{
				char ch = _fileData[_pos];
				if (!Char.IsLetterOrDigit(ch) && ch != '_') return;
				NextChar();
			}
		}

		void MoveUntilNonDigit()
		{
			while (_pos < _length)
			{
				char ch = _fileData[_pos];
				if (!Char.IsDigit(ch)) return;
				NextChar();
			}
		}

		void MoveUntilNonSpace()
		{
			while (_pos < _length)
			{
				char ch = _fileData[_pos];
				if (ch != ' ' && ch != 9 && ch != 10 && ch != 13) return;
				NextChar();
			}
		}

		void NextChar()
		{
			// This function will move to the next char, while keeping track of which line we are on.
			if (_pos < _length && _fileData[_pos] == '\n') _line++;
			_pos++;
		}

		void PrevChar()
		{
			if (_pos > 0)
			{
				_pos--;
				if (_pos < _length && _fileData[_pos] == '\n') _line--;
			}
		}

		void SkipWhiteSpace()
		{
			while (_pos < _length)
			{
				char ch = _fileData[_pos];
				if (ch == ' ' || ch == '\t' || ch == '\r' || ch == '\n')
				{
					NextChar();
				}
				else if (ch == '/' && _pos + 1 < _length)
				{
					ch = _fileData[_pos + 1];
					if (ch == '/')
					{
						while (_pos < _length && _fileData[_pos] != '\n') NextChar();
						if (_pos < _length) NextChar();
					}
					else if (ch == '*')
					{
						while (_pos + 1 < _length && (_fileData[_pos] != '*' || _fileData[_pos + 1] != '/')) NextChar();
						if (_pos + 1 < _length)
						{
							NextChar();
							NextChar();
						}
					}
					else return;
				}
				else return;
			}
		}

		void SkipToken()
		{
			if (_pos >= _length) return;
			char ch = _fileData[_pos];

			if (Char.IsLetter(ch) || ch == '_')
			{
				MoveUntilNonAlNum();
				return;
			}

			if (Char.IsDigit(ch))
			{
				while (_pos < _length)
				{
					ch = _fileData[_pos];
					if (!Char.IsDigit(ch) && ch != '.') break;
					NextChar();
				}
				return;
			}

			if (ch == '\"')
			{
				char chLast = '\0';
                NextChar();
				while (_pos < _length)
				{
					ch = _fileData[_pos];
                    if (ch == '\\' && _pos + 1 < _length && _fileData[_pos + 1] == '\\')
                    {
                        NextChar();
                        NextChar();
                        chLast = '\0';
                    }
                    else if (ch == '\"' && chLast != '\\')
                    {
                        NextChar();
                        break;
                    }
                    else if (ch == '\n' || ch == '\r')
                    {
                        break;
                    }
                    else
                    {
                        chLast = ch;
                        NextChar();
                    }
				}
				return;
			}

			if (ch == '\'')
			{
				char chLast = '\0';
                NextChar();
				while (_pos < _length)
				{
					ch = _fileData[_pos];
                    if (ch == '\\' && _pos + 1 < _length && _fileData[_pos + 1] == '\\')
                    {
                        NextChar();
                        NextChar();
                        chLast = '\0';
                    }
                    else if (ch == '\'' && chLast != '\\')
                    {
                        NextChar();
                        break;
                    }
                    else if (ch == '\n' || ch == '\r')
                    {
                        break;
                    }
                    else
                    {
                        chLast = ch;
                        NextChar();
                    }
				}
				return;
			}

			NextChar();
		}

		public IEnumerable<Function> Functions
		{
			get { return _funcs; }
		}

		int FindForward(char ch, params char[] termChars)
		{
			for (var pos = _pos; pos < _length; pos++)
			{
                if (_fileData[pos] == ch) return pos;
                else if (termChars != null && termChars.Contains(_fileData[pos])) return -1;
			}
			return -1;
		}

		void MoveTo(int pos)
		{
			if (pos > _pos)
			{
				while (pos > _pos) NextChar();
			}
			else if (pos < _pos)
			{
				while (pos < _pos) PrevChar();
			}
		}

	}
}
