﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.TokenParser
{
	internal class Parser
	{
		private int _pos;
		private int _lineNum;
		private int _linePos;
		private string _source;
		private int _length;
		private StringBuilder _tokenText = new StringBuilder();
		private TokenType _tokenType = TokenType.Unknown;
		private bool _returnWhiteSpace = false;
		private bool _returnComments = false;
		private Position _tokenStartPos;

		public Parser(string source)
		{
			SetSource(source);
		}

		public void SetSource(string source)
		{
			_source = source;
			_length = _source.Length;

			_pos = 0;
			_lineNum = 1;
			_linePos = 1;
		}

		public bool Read()
		{
			while (_pos < _length)
			{
				_tokenStartPos = Position;
				_tokenText.Clear();
				var ch = _source[_pos];

				if (char.IsWhiteSpace(ch))
				{
					_tokenText.Append(ch);
					MoveNext();
					while (_pos < _length && char.IsWhiteSpace(_source[_pos]))
					{
						_tokenText.Append(_source[_pos]);
						MoveNext();
					}

					if (_returnWhiteSpace)
					{
						_tokenType = TokenType.WhiteSpace;
						return true;
					}
					else continue;
				}

				if (char.IsLetter(ch) || ch == '_')
				{
					while (_pos < _length && (char.IsLetterOrDigit(_source[_pos]) || _source[_pos] == '_'))
					{
						_tokenText.Append(_source[_pos]);
						MoveNext();
					}

					if ((_tokenText.Length == 3 && _tokenText.ToString() == "and") ||
						(_tokenText.Length == 2 && _tokenText.ToString() == "or"))
					{
						_tokenType = TokenParser.TokenType.Operator;
					}
					else
					{
						_tokenType = TokenType.Word;
					}
					return true;
				}

				if (char.IsDigit(ch))
				{
					var gotDot = false;
					while (_pos < _length)
					{
						ch = _source[_pos];
						if (char.IsDigit(ch))
						{
							_tokenText.Append(ch);
							MoveNext();
						}
						else if (ch == '.' && !gotDot)
						{
							_tokenText.Append(".");
							MoveNext();
							gotDot = true;
						}
						else break;
					}

					_tokenType = TokenType.Number;
					return true;
				}

				if (ch == '\"' || ch == '\'')
				{
					var startCh = ch;
					_tokenText.Append(ch);
					MoveNext();
					while (_pos < _length)
					{
						ch = _source[_pos];
						if (ch == '\\' && _pos + 1 < _length)
						{
							_tokenText.Append(ch);
							_tokenText.Append(_source[_pos + 1]);
							MoveNext(2);
						}
						else if (ch == startCh)
						{
							_tokenText.Append(ch);
							MoveNext();
							break;
						}
						else
						{
							_tokenText.Append(ch);
							MoveNext();
						}
					}

					_tokenType = TokenType.StringLiteral;
					return true;
				}

				if (ch == '/')
				{
					if (_pos + 1 < _length && _source[_pos + 1] == '/')
					{
						while (_pos < _length && _source[_pos] != '\r' && _source[_pos] != '\n')
						{
							_tokenText.Append(_source[_pos]);
							MoveNext();
						}

						if (_returnComments)
						{
							_tokenType = TokenType.Comment;
							return true;
						}
						else continue;
					}

					if (_pos + 1 < _length && _source[_pos + 1] == '*')
					{
						var index = _source.IndexOf("*/", _pos);
						if (index < 0) index = _length;
						else index += "*/".Length;

						while (_pos < index)
						{
							_tokenText.Append(_source[_pos]);
							MoveNext();
						}

						if (_returnComments)
						{
							_tokenType = TokenType.Comment;
							return true;
						}
						else continue;
					}

					if (_pos + 1 < _length && _source[_pos + 1] == '=')
					{
						_tokenText.Append("/=");
						MoveNext(2);
						_tokenType = TokenParser.TokenType.Operator;
						return true;
					}

					_tokenText.Append("/");
					MoveNext();
					_tokenType = TokenParser.TokenType.Operator;
					return true;
				}

				if (ch == '+' || ch == '-' || ch == '*' || ch == '%' || ch == '=')
				{
					_tokenText.Append(ch);
					MoveNext();
					_tokenType = TokenParser.TokenType.Operator;

					if (_pos < _length && _source[_pos] == '=')
					{
						_tokenText.Append("=");
						MoveNext();
					}

					return true;
				}

				if (ch == '?' || ch == ':' || ch == ',' || ch == '.' || ch == '(' || ch == ')' || ch == '{' || ch == '}' || ch == '[' || ch == ']' || ch == '&')
				{
					_tokenText.Append(ch);
					MoveNext();
					_tokenType = TokenParser.TokenType.Operator;
					return true;
				}

				if (ch == '#')
				{
					_tokenText.Append(ch);
					MoveNext();
					if (_pos < _length && char.IsLetter(_source[_pos]))
					{
						while (_pos < _length && char.IsLetter(_source[_pos]))
						{
							_tokenText.Append(_source[_pos]);
							MoveNext();
						}

						_tokenType = TokenType.Preprocessor;
						return true;
					}
					else
					{
						_tokenType = TokenType.Operator;
						return true;
					}
				}

				_tokenText.Append(ch);
				MoveNext();
				_tokenType = TokenType.Unknown;
				return true;
			}

			// End of file
			_tokenType = TokenType.WhiteSpace;
			_tokenText.Clear();
			return false;
		}

		public Position Position
		{
			get { return new Position(_pos, _lineNum, _linePos); }
			set
			{
				_pos = value.Offset;
				_lineNum = value.LineNum;
				_linePos = value.LinePos;
			}
		}

		public Position TokenStartPostion
		{
			get { return _tokenStartPos; }
		}

		public int Length
		{
			get { return _length; }
		}

		public bool ReturnWhiteSpace
		{
			get { return _returnWhiteSpace; }
			set { _returnWhiteSpace = value; }
		}

		public bool ReturnComments
		{
			get { return _returnComments; }
			set { _returnComments = value; }
		}

		private void MoveNext()
		{
			if (_pos < _length)
			{
				if (_source[_pos] == '\n')
				{
					_lineNum++;
					_linePos = 1;
				}
				_pos++;
				_linePos++;
			}
		}

		private void MoveNext(int numChars)
		{
			while ((numChars--) > 0) MoveNext();
		}

		public TokenType TokenType
		{
			get { return _tokenType; }
		}

		public string TokenText
		{
			get { return _tokenText.ToString(); }
		}

		public bool EndOfFile
		{
			get { return _pos >= _length; }
		}

		public bool ReadNestable()
		{
			var startPos = Position;

			if (!Read()) return false;

			var firstTokenType = _tokenType;

			if (_tokenType == TokenParser.TokenType.Operator)
			{
				switch (_tokenText.ToString())
				{
					case "(":
						if (ReadNestable_Inner(")"))
						{
							_tokenStartPos = startPos;
							_tokenText.Clear();
							_tokenText.Append(_source.Substring(_tokenStartPos.Offset, _pos - _tokenStartPos.Offset));
							_tokenType = TokenParser.TokenType.Nested;
							return true;
						}
						break;
					case "{":
						if (ReadNestable_Inner("}"))
						{
							_tokenStartPos = startPos;
							_tokenText.Clear();
							_tokenText.Append(_source.Substring(_tokenStartPos.Offset, _pos - _tokenStartPos.Offset));
							_tokenType = TokenParser.TokenType.Nested;
							return true;
						}
						break;
					case "[":
						if (ReadNestable_Inner("]"))
						{
							_tokenStartPos = startPos;
							_tokenText.Clear();
							_tokenText.Append(_source.Substring(_tokenStartPos.Offset, _pos - _tokenStartPos.Offset));
							_tokenType = TokenParser.TokenType.Nested;
							return true;
						}
						break;
				}
			}

			_tokenStartPos = startPos;
			_tokenType = firstTokenType;
			return true;
		}

		private bool ReadNestable_Inner(string endText)
		{
			var startPos = Position;

			while (Read())
			{
				if (_tokenType == TokenParser.TokenType.Operator)
				{
					if (_tokenText.ToString() == endText) return true;
					switch (_tokenText.ToString())
					{
						case "(":
							if (!ReadNestable_Inner(")")) return false;
							break;
						case "{":
							if (!ReadNestable_Inner("}")) return false;
							break;
						case "[":
							if (!ReadNestable_Inner("]")) return false;
							break;
					}
				}
			}

			Position = startPos;
			return false;
		}

		public string GetText(int startIndex, int length)
		{
			if (startIndex < 0 || startIndex + length > _length) throw new ArgumentOutOfRangeException();
			return _source.Substring(startIndex, length);
		}

		public bool Peek()
		{
			var pos = Position;
			if (!Read()) return false;
			Position = pos;
			return true;
		}

		public bool Peek(out Position tokenEndPos)
		{
			var pos = Position;
			if (!Read())
			{
				tokenEndPos = Position;
				return false;
			}
			tokenEndPos = Position;
			Position = pos;
			return true;
		}

		public void ResetPosition()
		{
			_pos = 0;
			_lineNum = 1;
			_linePos = 1;
		}

		public void SetOffset(int offset)
		{
			if (_pos > offset)
			{
				_pos = 0;
				_lineNum = 1;
				_linePos = 1;
			}

			var stop = _length < offset ? _length : offset;
			while (_pos < stop)
			{
				if (_source[_pos] == '\n')
				{
					_lineNum++;
					_linePos = 1;
				}
				_pos++;
				_linePos++;
			}
		}

		public static string StringLiteralToString(string str)
		{
			if (str.StartsWith("\"") || str.StartsWith("\'")) str = str.Substring(1);
			if (str.EndsWith("\"") || str.EndsWith("\'")) str = str.Substring(0, str.Length - 1);

			var sb = new StringBuilder(str.Length);
			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == '\\' && i + 1 < str.Length)
				{
					i++;
					switch (str[i])
					{
						case 'n':
							sb.Append('\n');
							break;
						case 'r':
							sb.Append('\r');
							break;
						case 't':
							sb.Append('\t');
							break;
						default:
							sb.Append(str[i]);
							break;
					}
				}
				else sb.Append(str[i]);
			}

			return sb.ToString();
		}

		public string Source
		{
			get { return _source; }
			set { SetSource(value); }
		}
	}

	internal enum TokenType
	{
		Unknown,
		WhiteSpace,
		Comment,
		Word,
		Number,
		StringLiteral,
		Operator,
		Preprocessor,
		Nested
	}

	internal struct Position
	{
		private int _offset;
		private int _lineNum;
		private int _linePos;

		public Position(int offset, int lineNum, int linePos)
		{
			_offset = offset;
			_lineNum = lineNum;
			_linePos = linePos;
		}

		public int Offset
		{
			get { return _offset; }
		}

		public int LineNum
		{
			get { return _lineNum; }
		}

		public int LinePos
		{
			get { return _linePos; }
		}

		public NppSharp.TextLocation ToNppSharpTextLocation()
		{
			return new NppSharp.TextLocation(_lineNum, _linePos);
		}
	}
}
