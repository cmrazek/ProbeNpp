using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Reflection;
using System.IO;
using NppSharp;

namespace ProbeNpp
{
	[NppDisplayName("Probe")]
	[NppDescription("Probe Source File")]
	public class ProbeLexer : ILexer
	{
		// Default file extensions.
		// The user may modify this list through Settings -> Style Configurator.
		public IEnumerable<string> Extensions
		{
			get { return new string[] { "ct", "ct&", "f", "f&", "i", "i&", "il", "il&", "gp", "gp&", "st", "st&", "t", "t&" }; }
		}

		// Styles used by this lexer.
		LexerStyle _defaultStyle = new LexerStyle("Default");
		LexerStyle _commentStyle = new LexerStyle("Comments", Color.Green, FontStyle.Italic);
		LexerStyle _numberStyle = new LexerStyle("Numbers", Color.DarkRed);
		LexerStyle _stringStyle = new LexerStyle("Strings", Color.DarkRed);
		LexerStyle _operatorStyle = new LexerStyle("Operators", Color.DarkGray);
		LexerStyle _keywordStyle = new LexerStyle("Keywords", Color.Blue);
		LexerStyle _functionStyle = new LexerStyle("Functions", Color.DarkMagenta);
		LexerStyle _constantStyle = new LexerStyle("Constants", Color.Navy);
		LexerStyle _dataTypeStyle = new LexerStyle("Data Types", Color.Teal);
		LexerStyle _preprocessorStyle = new LexerStyle("Preprocessor", Color.Gray);
		LexerStyle _tableStyle = new LexerStyle("Tables", Color.DarkOrange);
        LexerStyle _fieldStyle = new LexerStyle("Fields", Color.DarkGray);

		private HashSet<string> _keywords  = new HashSet<string>();
		private HashSet<string> _functions = new HashSet<string>();
		private HashSet<string> _constants = new HashSet<string>();
		private HashSet<string> _dataTypes = new HashSet<string>();
		private HashSet<string> _operators = new HashSet<string>();

		public ProbeLexer()
		{
			LoadConfig();
		}

		private void LoadConfig()
		{
			var fileName = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ProbeNppLexer.xml");
			if (File.Exists(fileName))
			{
				var xmlDoc = new XmlDocument();
				xmlDoc.Load(fileName);

				foreach (XmlElement element in xmlDoc.SelectNodes("/ProbeNpp/Keywords"))
				{
					ParseWordList(element.InnerText, _keywords);
				}

				foreach (XmlElement element in xmlDoc.SelectNodes("/ProbeNpp/Functions"))
				{
					ParseWordList(element.InnerText, _functions);
				}

				foreach (XmlElement element in xmlDoc.SelectNodes("/ProbeNpp/Constants"))
				{
					ParseWordList(element.InnerText, _constants);
				}

				foreach (XmlElement element in xmlDoc.SelectNodes("/ProbeNpp/DataTypes"))
				{
					ParseWordList(element.InnerText, _dataTypes);
				}

				foreach (XmlElement element in xmlDoc.SelectNodes("/ProbeNpp/Operators"))
				{
					ParseWordList(element.InnerText, _operators);
				}
			}
		}

		// Default styles.
		// The user may modify the style appearance through Settings -> Style Configurator.
		public IEnumerable<LexerStyle> Styles
		{
			get
			{
				return new LexerStyle[] { _defaultStyle, _commentStyle, _numberStyle, _stringStyle,
					_operatorStyle, _keywordStyle, _functionStyle, _constantStyle, _dataTypeStyle,
					_preprocessorStyle, _tableStyle, _fieldStyle };
			}
		}

		// State bits
		private const int State_InsideComment = 1;

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

		private int GetLastToken(int state)
		{
			return (state & State_TokenMask) >> 8;
		}

		private void SetLastToken(ref int state, int token)
		{
			state &= ~State_TokenMask;
			state |= (token << 8) & State_TokenMask;
		}

		public int StyleLine(ILexerLine line, int state)
		{
			// This function is called by NppSharp to get the lexer to perform styling and code
			// folding on a line of text.

			int tokenCount = 0;
			char nextCh;

			while (!line.EOL)
			{
				nextCh = line.NextChar;

				if ((state & State_InsideComment) != 0)
				{
					// Currently inside block comment.
					if (line.Peek(2) == "*/")
					{
						line.Style(_commentStyle, 2);
						state &= ~State_InsideComment;
					}
					else
					{
						line.Style(_commentStyle);
					}
					SetLastToken(ref state, State_Token_BlockComment);
				}
				else if (line.Peek(2) == "//")
				{
					// Start of line comment; rest of line is forfeit.
					line.StyleRemainder(_commentStyle);
					SetLastToken(ref state, State_Token_StreamComment);
				}
				else if (line.Peek(2) == "/*")
				{
					// Start of block comment. Switch the state on so it passes down to other lines.
					state |= State_InsideComment;
					SetLastToken(ref state, State_Token_BlockComment);
				}
				else if (Char.IsLetter(nextCh) || nextCh == '_')
				{
					// Token beginning with letter or underscore.  Standard identifier.
					string token = line.Peek((ch) => Char.IsLetterOrDigit(ch) || ch == '_');
					if (_keywords.Contains(token))
					{
						line.Style(_keywordStyle, token.Length);
						SetLastToken(ref state, State_Token_Keyword);
					}
					else if (_functions.Contains(token))
					{
						line.Style(_functionStyle, token.Length);
						SetLastToken(ref state, State_Token_Function);
					}
					else if (_constants.Contains(token))
					{
						line.Style(_constantStyle, token.Length);
						SetLastToken(ref state, State_Token_Constant);
					}
					else if (_dataTypes.Contains(token))
					{
						line.Style(_dataTypeStyle, token.Length);
						SetLastToken(ref state, State_Token_DataType);
					}
					else if (ProbeNppPlugin.Instance.Environment.IsProbeTable(token))
					{
						line.Style(_tableStyle, token.Length);
						SetLastToken(ref state, State_Token_Table);
					}
					else if (GetLastToken(state) == State_Token_TableDelim)
					{
						line.Style(_fieldStyle, token.Length);
						SetLastToken(ref state, State_Token_Field);
					}
					else
					{
						line.Style(_defaultStyle, token.Length);
						SetLastToken(ref state, State_Token_UnknownIdent);
					}
					tokenCount++;
				}
				else if (Char.IsDigit(nextCh))
				{
					// Token beginning with number.
					line.Style(_numberStyle, (ch) => Char.IsDigit(ch) || ch == '.');
					tokenCount++;
					SetLastToken(ref state, State_Token_Number);
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
					tokenCount++;
					SetLastToken(ref state, State_Token_String);
				}
				else if (nextCh == '{')
				{
					// Opening braces are the start of a code folding section.
					line.FoldStart();
					line.Style(_operatorStyle);
					tokenCount++;
					SetLastToken(ref state, State_Token_Operator);
				}
				else if (nextCh == '}')
				{
					// Closing braces are the end of a code folding section.
					line.FoldEnd();
					line.Style(_operatorStyle);
					tokenCount++;
					SetLastToken(ref state, State_Token_Operator);
				}
				else if (nextCh == '#')
				{
					if (tokenCount == 0)
					{
						line.StyleRemainder(_preprocessorStyle);
						tokenCount++;
						SetLastToken(ref state, State_Token_Preprocessor);
					}
					else
					{
						line.Style(_defaultStyle);
						SetLastToken(ref state, State_Token_Operator);
					}
				}
				else if (nextCh == '.')
				{
					line.Style(_operatorStyle);
					tokenCount++;

					if (GetLastToken(state) == State_Token_Table)
					{
						SetLastToken(ref state, State_Token_TableDelim);
					}
					else
					{
						SetLastToken(ref state, State_Token_Operator);
					}
				}
				else if (_operators.Contains(nextCh.ToString()))
				{
					line.Style(_operatorStyle);
					tokenCount++;
					SetLastToken(ref state, State_Token_Operator);
				}
				else if (!Char.IsWhiteSpace(nextCh))
				{
					line.Style(_defaultStyle);
					tokenCount++;
					SetLastToken(ref state, State_Token_Unknown);
				}
				else
				{
					line.Style(_defaultStyle);
				}
			}

			return state;
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
	}
}
