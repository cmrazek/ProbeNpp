using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using NppSharp;

namespace ProbeNpp
{
	[NppDisplayName("Probe Source")]
	[NppDescription("Probe Source File")]
	public class ProbeSourceLexer : ProbeLexer
	{
		public const string Name = "Probe Source";

		public ProbeSourceLexer()
			: base(ProbeLexerType.Source)
		{
		}

		public override IEnumerable<string> GetExtensions()
		{
			return new string[] { ProbeNppPlugin.Instance.Settings.Probe.SourceExtensions };
		}
	}

	[NppDisplayName("Probe Table")]
	[NppDescription("Probe Dictionary File")]
	public class ProbeDictLexer : ProbeLexer
	{
		public const string Name = "Probe Table";

		public ProbeDictLexer()
			: base(ProbeLexerType.Dict)
		{
		}

		public override IEnumerable<string> GetExtensions()
		{
			return new string[] { ProbeNppPlugin.Instance.Settings.Probe.DictExtensions };
		}
	}

	internal enum ProbeLexerType
	{
		Source,
		Dict
	}

	[LexerComments(BlockStart = "/*", BlockEnd = "*/", Line = "//")]
	public abstract class ProbeLexer : ILexer
	{
		ProbeLexerType _type;

		public IEnumerable<string> Extensions
		{
			get { return GetExtensions(); }
		}

		public abstract IEnumerable<string> GetExtensions();

		// Styles used by this lexer.
		LexerStyle _defaultStyle = new LexerStyle("Default");
		LexerStyle _commentStyle = new LexerStyle("Comments", Color.Green, FontStyle.Italic);
		LexerStyle _numberStyle = new LexerStyle("Numbers", Color.DarkRed);
		LexerStyle _stringStyle = new LexerStyle("Strings", Color.DarkRed);
		LexerStyle _operatorStyle = new LexerStyle("Operators", Color.DimGray);
		LexerStyle _keywordStyle = new LexerStyle("Keywords", Color.Blue);
		LexerStyle _functionStyle = new LexerStyle("Functions", Color.DarkMagenta);
		LexerStyle _constantStyle = new LexerStyle("Constants", Color.Navy);
		LexerStyle _dataTypeStyle = new LexerStyle("Data Types", Color.Teal);
		LexerStyle _preprocessorStyle = new LexerStyle("Preprocessor", Color.Gray);
		LexerStyle _tableStyle = new LexerStyle("Tables", Color.SteelBlue);
		LexerStyle _fieldStyle = new LexerStyle("Fields", Color.SteelBlue);
		LexerStyle _replacedStyle = new LexerStyle("Replaced", Color.DarkGray);
		LexerStyle _errorStyle = new LexerStyle("Error", Color.Red, "", FontStyle.Bold);

		internal ProbeLexer(ProbeLexerType type)
		{
			_type = type;
		}

		public IEnumerable<LexerStyle> Styles
		{
			get
			{
				if (_type == ProbeLexerType.Source)
				{
					return new LexerStyle[] { _defaultStyle, _commentStyle, _numberStyle, _stringStyle,
						_operatorStyle, _keywordStyle, _functionStyle, _constantStyle, _dataTypeStyle,
						_preprocessorStyle, _tableStyle, _fieldStyle, _replacedStyle, _errorStyle };
				}
				else
				{
					return new LexerStyle[] { _defaultStyle, _commentStyle, _numberStyle, _stringStyle,
						_operatorStyle, _keywordStyle, _constantStyle, _dataTypeStyle,
						_preprocessorStyle, _tableStyle, _fieldStyle, _replacedStyle, _errorStyle };
				}
			}
		}

		// State bits
		public const int State_InsideComment = 0x01;
		private const int State_InsideReplace = 0x02;
		private const int State_InsideReplaceWith = 0x04;
		private const int State_InsideInsert = 0x08;

		private const int State_TokenMask = 0xff00;
		private const int State_Token_None = 0;
		private const int State_Token_Unknown = 0xff00;
		private const int State_Token_BlockComment = 1;
		private const int State_Token_StreamComment = 2;
		private const int State_Token_Keyword = 3;
		private const int State_Token_Function = 4;
		private const int State_Token_Constant = 5;
		private const int State_Token_DataType = 6;
		private const int State_Token_Table = 7;
		private const int State_Token_TableDelim = 8;
		private const int State_Token_Field = 9;
		private const int State_Token_UnknownIdent = 10;
		private const int State_Token_Number = 11;
		private const int State_Token_String = 12;
		private const int State_Token_Operator = 13;
		private const int State_Token_Preprocessor = 14;

		private ILexerLine _line = null;
		private int _state = 0;
		private int _tokenCount = 0;
		private string _table = string.Empty;	// Used to validate table.field.
		private CodeModel.CodeModel _model = null;

		private int GetLastToken()
		{
			return (_state & State_TokenMask) >> 8;
		}

		private void SetLastToken(int token)
		{
			_state &= ~State_TokenMask;
			_state |= (token << 8) & State_TokenMask;

			switch (token)
			{
				case State_Token_None:
					break;
				default:
					_tokenCount++;
					break;
			}
		}

		public int StyleLine(ILexerLine line, int state)
		{
			_line = line;
			_state = state;
			_tokenCount = 0;
			_table = string.Empty;
			_model = null;

			char nextCh;
			int lastPos = -1;

			while (!line.EOL)
			{
				nextCh = line.NextChar;

				if (line.Position == lastPos) throw new LexerException(string.Format(
					"The lexer did not advance the line position when styling line [{0}] at position [{1}]",
					line.Text, lastPos));
				lastPos = line.Position;

				if ((_state & State_InsideComment) != 0)
				{
					// Currently inside block comment.
					if (line.Match("*/"))
					{
						line.Style(_commentStyle, 2);
						_state &= ~State_InsideComment;
					}
					else
					{
						line.Style(_commentStyle);
					}
					SetLastToken(State_Token_BlockComment);
				}
				else if ((_state & State_InsideReplace) != 0 && line.Peek(5) != "#with")
				{
					_line.Style(_replacedStyle);
					SetLastToken(State_Token_None);
				}
				else if (line.Match("//"))
				{
					// Start of line comment; rest of line is forfeit.
					line.StyleRemainder(_commentStyle);
					SetLastToken(State_Token_StreamComment);
				}
				else if (line.Match("/*"))
				{
					// Start of block comment. Switch the state on so it passes down to other lines.
					_line.Style(_commentStyle, 2);
					_state |= State_InsideComment;
					SetLastToken(State_Token_BlockComment);
				}
				else if (Char.IsLetter(nextCh) || nextCh == '_')
				{
					// Token beginning with letter or underscore.  Standard identifier.
					StyleIdent();
				}
				else if (Char.IsDigit(nextCh))
				{
					// Token beginning with number.
					line.Style(_numberStyle, (ch) => Char.IsDigit(ch) || ch == '.');
					SetLastToken(State_Token_Number);
				}
				else if (nextCh == '\"' || nextCh == '\'')
				{
					// String literal.
					char lastCh = '\\';
					bool done = false;
					line.Style(_stringStyle, (ch) =>
						{
							if (done) return false;
							if (ch == nextCh && lastCh != '\\') done = true;
							lastCh = ch;
							return true;
						});
					SetLastToken(State_Token_String);
				}
				else if (nextCh == '{')
				{
					// Opening braces are the start of a code folding section.
					line.FoldStart();
					line.Style(_operatorStyle);
					SetLastToken(State_Token_Operator);
				}
				else if (nextCh == '}')
				{
					// Closing braces are the end of a code folding section.
					line.FoldEnd();
					line.Style(_operatorStyle);
					SetLastToken(State_Token_Operator);
				}
				else if (nextCh == '#')
				{
					StylePreprocessor();
				}
				else if (nextCh == '.')
				{
					line.Style(_operatorStyle);

					if (GetLastToken() == State_Token_Table)
					{
						SetLastToken(State_Token_TableDelim);
					}
					else
					{
						SetLastToken(State_Token_Operator);
					}
				}
				else if (ProbeNppPlugin.Instance.OperatorChars.Contains(nextCh))
				{
					line.Style(_operatorStyle);
					SetLastToken(State_Token_Operator);
				}
				else if (!Char.IsWhiteSpace(nextCh))
				{
					line.Style(_defaultStyle);
					SetLastToken(State_Token_Unknown);
				}
				else
				{
					line.Style(_defaultStyle);
				}
			}

			return _state;
		}

		private void StyleIdent()
		{
			string token = _line.Peek((ch) => Char.IsLetterOrDigit(ch) || ch == '_');

			if (GetLastToken() == State_Token_TableDelim)
			{
				if (!string.IsNullOrEmpty(_table))
				{
					var env = ProbeNppPlugin.Instance.Environment;
					var table = env.GetTable(_table);
					if (table != null && table.IsField(token))
					{
						_line.Style(_fieldStyle, token.Length);
						SetLastToken(State_Token_Field);
						return;
					}
				}
			}
			else
			{
				var env = ProbeNppPlugin.Instance.Environment;
				if (env.IsProbeTable(token))
				{
					_line.Style(_tableStyle, token.Length);
					SetLastToken(State_Token_Table);
					_table = token;
					return;
				}
			}

			if ((_type == ProbeLexerType.Source && ProbeNppPlugin.Instance.SourceKeywords.Contains(token)) ||
				(_type == ProbeLexerType.Dict && ProbeNppPlugin.Instance.DictKeywords.Contains(token)))
			{
				_line.Style(_keywordStyle, token.Length);
				SetLastToken(State_Token_Keyword);
				return;
			}

			if ((ProbeNppPlugin.Instance.FunctionSignatures.Keys.Contains(token) || GetFunctionList().Contains(token)) &&
				PeekNextChar(token.Length) == '(')
			{
				_line.Style(_functionStyle, token.Length);
				SetLastToken(State_Token_Function);
				return;
			}

			if (ProbeNppPlugin.Instance.UserConstants.Contains(token) || GetConstantNames().Contains(token))
			{
			    _line.Style(_constantStyle, token.Length);
			    SetLastToken(State_Token_Constant);
			    return;
			}

			if (ProbeNppPlugin.Instance.DataTypes.Contains(token) || GetDataTypeNames().Contains(token))
			{
				_line.Style(_dataTypeStyle, token.Length);
				SetLastToken(State_Token_DataType);
				return;
			}

			_line.Style(_defaultStyle, token.Length);
			SetLastToken(State_Token_UnknownIdent);
		}

		private void StylePreprocessor()
		{
			if (_tokenCount == 0)
			{
				string cmd = _line.Peek(ch => Char.IsLetter(ch) || ch == '#');
				switch (cmd)
				{
					case "#define":
					case "#ifdef":
					case "#ifndef":
						_line.Style(_preprocessorStyle, cmd.Length);
						if (StyleWhiteSpace() && IsIdentChar(_line.NextChar, true))
						{
							_line.Style(_constantStyle, ch => IsIdentChar(ch, false));
						}
						SetLastToken(State_Token_Preprocessor);
						break;

					case "#replace":
						if ((_state & (State_InsideReplace | State_InsideReplaceWith)) != 0)
						{
							// #replace's cannot be nested.
							_line.Style(_errorStyle, cmd.Length);
						}
						else
						{
							_line.Style(_preprocessorStyle, cmd.Length);
							_line.FoldStart();
						}
						_state |= State_InsideReplace;
						SetLastToken(State_Token_Preprocessor);
						break;

					case "#with":
						if (((_state & State_InsideReplace) == 0) || ((_state & State_InsideReplaceWith) != 0))
						{
							// #with must always occur after #replace.
							_line.Style(_errorStyle, cmd.Length);
						}
						else
						{
							_line.FoldEnd();
							_line.Style(_preprocessorStyle, cmd.Length);
							_line.FoldStart();
						}
						_state = (_state & ~State_InsideReplace) | State_InsideReplaceWith;
						SetLastToken(State_Token_Preprocessor);
						break;

					case "#endreplace":
						if ((_state & State_InsideReplaceWith) == 0)
						{
							// #endreplace must always occur after #with.
							_line.Style(_errorStyle, cmd.Length);
						}
						else
						{
							_line.FoldEnd();
							_line.Style(_preprocessorStyle, cmd.Length);
						}
						_state &= ~State_InsideReplaceWith;
						SetLastToken(State_Token_Preprocessor);
						break;

					case "#insert":
						if ((_state & State_InsideInsert) != 0)
						{
							// #insert cannot be inside another #insert.
							_line.Style(_errorStyle, cmd.Length);
						}
						else
						{
							_line.FoldStart();
							_line.Style(_preprocessorStyle, cmd.Length);
							if (StyleWhiteSpace() && IsIdentChar(_line.NextChar, true))
							{
								_line.Style(_constantStyle, ch => IsIdentChar(ch, false));
							}
						}
						_state |= State_InsideInsert;
						SetLastToken(State_Token_Preprocessor);
						break;

					case "#endinsert":
						if ((_state & State_InsideInsert) == 0)
						{
							// #endinsert must occur after #insert.
							_line.Style(_errorStyle, cmd.Length);
						}
						else
						{
							_line.FoldEnd();
							_line.Style(_preprocessorStyle, cmd.Length);
						}
						_state &= ~State_InsideInsert;
						SetLastToken(State_Token_Preprocessor);
						break;

					case "#label":
						_line.Style(_preprocessorStyle, cmd.Length);
						if (StyleWhiteSpace() && IsIdentChar(_line.NextChar, true))
						{
							_line.Style(_constantStyle, ch => IsIdentChar(ch, false));
						}
						SetLastToken(State_Token_Preprocessor);
						break;

					case "#include":
						_line.Style(_preprocessorStyle, cmd.Length);
						StyleWhiteSpace();

						char nextCh = _line.NextChar;
						switch (nextCh)
						{
							case '\"':
								{
									char lastCh = '\\';
									bool done = false;
									_line.Style(_stringStyle, (ch) =>
									{
										if (done) return false;
										if (ch == nextCh && lastCh != '\\') done = true;
										lastCh = ch;
										return true;
									});
								}
								break;

							case '<':
								{
									bool done = false;
									_line.Style(_stringStyle, (ch) =>
									{
										if (done) return false;
										if (ch == '>') done = true;
										return true;
									});
								}
								break;
						}

						SetLastToken(State_Token_Preprocessor);
						break;

					default:
						_line.Style(_preprocessorStyle, cmd.Length);
						SetLastToken(State_Token_Preprocessor);
						break;
				}
			}
			else
			{
				_line.Style(_defaultStyle);
				SetLastToken(State_Token_Operator);
			}
		}

		/// <summary>
		/// Consumes the next whitespace on the line.
		/// </summary>
		/// <returns>True if whitespace was found, otherwise false.</returns>
		private bool StyleWhiteSpace()
		{
			bool foundWhiteSpace = false;
			while (Char.IsWhiteSpace(_line.NextChar))
			{
				_line.Style(_defaultStyle);
				foundWhiteSpace = true;
			}
			return foundWhiteSpace;
		}

		private static bool IsIdentChar(char ch, bool firstChar)
		{
			if (Char.IsLetter(ch) || ch == '_') return true;
			if (!firstChar && Char.IsDigit(ch)) return true;
			return false;
		}

		private void ParseWordList(string text, HashSet<string> wordList)
		{
			StringBuilder sb = new StringBuilder();

			foreach (char ch in text)
			{
				if (Char.IsWhiteSpace(ch))
				{
					if (sb.Length > 0) wordList.Add(sb.ToString());
					sb.Clear();
				}
				else sb.Append(ch);
			}

			if (sb.Length > 0) wordList.Add(sb.ToString());
		}

		private IEnumerable<string> GetFunctionList()
		{
			if (_model == null) _model = ProbeNppPlugin.Instance.CurrentModel;
			if (_model != null) return _model.FunctionNames;
			return new string[0];
		}

		private IEnumerable<string> GetConstantNames()
		{
			if (_model == null) _model = ProbeNppPlugin.Instance.CurrentModel;
			if (_model != null) return _model.ConstantNames;
			return new string[0];
		}

		private IEnumerable<string> GetDataTypeNames()
		{
			if (_model == null) _model = ProbeNppPlugin.Instance.CurrentModel;
			if (_model != null) return _model.DataTypeNames;
			return new string[0];
		}

		private char PeekNextChar(int offset)
		{
			while (offset + _line.Position < _line.Length)
			{
				var ch = _line.PeekChar(offset++);
				if (!char.IsWhiteSpace(ch)) return ch;
			}
			return '\0';
		}
	}
}
