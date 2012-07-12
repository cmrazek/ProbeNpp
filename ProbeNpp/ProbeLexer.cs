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
		LexerStyle _keywordStyle = new LexerStyle("Keywords", Color.Blue);
		LexerStyle _functionStyle = new LexerStyle("Functions", Color.DarkMagenta);
		LexerStyle _constantStyle = new LexerStyle("Constants", Color.Navy);
		LexerStyle _dataTypeStyle = new LexerStyle("Data Types", Color.Teal);
		LexerStyle _preprocessorStyle = new LexerStyle("Preprocessor", Color.Gray);

		private HashSet<string> _keywords  = new HashSet<string>();
		private HashSet<string> _functions = new HashSet<string>();
		private HashSet<string> _constants = new HashSet<string>();
		private HashSet<string> _dataTypes = new HashSet<string>();

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
			}
		}

		// Default styles.
		// The user may modify the style appearance through Settings -> Style Configurator.
		public IEnumerable<LexerStyle> Styles
		{
			get
			{
				return new LexerStyle[] { _defaultStyle, _commentStyle, _numberStyle, _stringStyle,
					_keywordStyle, _functionStyle, _constantStyle, _dataTypeStyle, _preprocessorStyle };
			}
		}

		// The state will only make use of 1 bit, a flag to indicate if we're inside a block comment.
		private const int State_InsideComment = 1;

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
				}
				else if (line.Peek(2) == "//")
				{
					// Start of line comment; rest of line is forfeit.
					line.StyleRemainder(_commentStyle);
				}
				else if (line.Peek(2) == "/*")
				{
					// Start of block comment. Switch the state on so it passes down to other lines.
					state |= State_InsideComment;
				}
				else if (Char.IsLetter(nextCh) || nextCh == '_')
				{
					// Token beginning with letter or underscore.  Standard identifier.
					string token = line.Peek((ch) => Char.IsLetterOrDigit(ch) || ch == '_');
					if (_keywords.Contains(token)) line.Style(_keywordStyle, token.Length);
					else if (_functions.Contains(token)) line.Style(_functionStyle, token.Length);
					else if (_constants.Contains(token)) line.Style(_constantStyle, token.Length);
					else if (_dataTypes.Contains(token)) line.Style(_dataTypeStyle, token.Length);
					else line.Style(_defaultStyle, token.Length);
					tokenCount++;
				}
				else if (Char.IsDigit(nextCh))
				{
					// Token beginning with number.
					line.Style(_numberStyle, (ch) => Char.IsDigit(ch) || ch == '.');
					tokenCount++;
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
				}
				else if (nextCh == '{')
				{
					// Opening braces are the start of a code folding section.
					line.FoldStart();
					line.Style(_defaultStyle);
					tokenCount++;
				}
				else if (nextCh == '}')
				{
					// Closing braces are the end of a code folding section.
					line.FoldEnd();
					line.Style(_defaultStyle);
					tokenCount++;
				}
				else if (nextCh == '#')
				{
					if (tokenCount == 0)
					{
						line.StyleRemainder(_preprocessorStyle);
						tokenCount++;
					}
					else
					{
						line.Style(_defaultStyle);
					}
				}
				else if (!Char.IsWhiteSpace(nextCh))
				{
					line.Style(_defaultStyle);
					tokenCount++;
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
