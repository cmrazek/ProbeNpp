using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ProbeNpp.CodeModel.Tokens
{
	internal class CodeFile : Token, IGroupToken
	{
		#region Variables
		private CodeModel _model;
		private string _source;
		private string _fileName;
		private int _pos = 0;
		private int _length = 0;
		private int _lineNum = 0;
		private int _linePos = 0;
		private List<Token> _tokens = new List<Token>();
		private List<IncludeToken.IncludeDef> _includeFiles = new List<IncludeToken.IncludeDef>();

		private static HashSet<string> _dataTypes = null;
		private static HashSet<string> _keywords = null;
		#endregion

		#region Construction
		public CodeFile(CodeModel model)
			: base(null, new Scope(), new Span())
		{
			if (model == null) throw new ArgumentNullException("model");
			_model = model;

			if (_dataTypes == null) _dataTypes = Util.ParseWordList(Res.ProbeDataTypeKeywords);
			if (_keywords == null) _keywords = Util.ParseWordList(Res.ProbeKeywords);
		}

		public string FileName
		{
			get { return _fileName; }
		}
		#endregion

		#region Parsing
		public void Parse(string source, string fileName)
		{
			_source = source;
			_fileName = fileName;

			_pos = 0;
			_length = _source.Length;
			_lineNum = 0;
			_linePos = 0;

			var scope = new Scope(this, 0, ScopeHint.None);

			while (_pos < _length)
			{
				var token = ParseToken(this, scope);
				if (token != null) _tokens.Add(token);
			}

			Span = new Span(new Position(), Position);

			// Process include files
			foreach (var token in _tokens)
			{
				foreach (var incl in token.GetUnprocessedIncludes())
				{
					_includeFiles.Add(incl);
					_model.AddIncludeFile(_fileName, incl.FileName, incl.SearchFileDir);
				}
			}
		}

		public Token ParseToken(Token parent, Scope scope)
		{
			var token = ParseSingleToken(parent, scope);
			if (token == null) return null;

			if (token.GetType() == typeof(DataTypeKeywordToken))
			{
				var dataTypeToken = DataTypeToken.Parse(parent, scope, token as DataTypeKeywordToken);
				var token2 = ParseAfterDataType(parent, scope, dataTypeToken);
				if (token2 != null) return token2;

				return dataTypeToken;
			}

			if (token.GetType() == typeof(IdentifierToken))
			{
				var funcToken = FunctionToken.TryParse(parent, scope, null, token as IdentifierToken);
				if (funcToken != null) return funcToken;

				var pos = Position;
				var token2 = ParseToken(parent, scope);
				if (token2 == null) Position = pos;
				else if (token2.GetType() == typeof(IdentifierToken))
				{
					var varDeclToken = VariableDeclarationToken.TryParse(parent, scope, token, token2 as IdentifierToken);
					if (varDeclToken == null) Position = pos;
					else return varDeclToken;
				}
				else if (token2.GetType() == typeof(BracketsToken) && !scope.Hint.HasFlag(ScopeHint.SuppressFunctionCall))
				{
					return new FunctionCallToken(parent, scope, token as IdentifierToken, token2 as BracketsToken);
				}
				else Position = pos;

				return token;
			}

			if (token.GetType() == typeof(KeywordToken))
			{
				var keywordToken = token as KeywordToken;
				switch (keywordToken.Text)
				{
					case "if":
						return IfStatementToken.Parse(parent, scope, keywordToken);
					case "extern":
						return ParseExtern(parent, scope, keywordToken);
					default:
						return keywordToken;
				}
			}

			if (token.GetType() == typeof(PreprocessorToken))
			{
				var prepToken = token as PreprocessorToken;
				var prepEndPos = Position;

				switch (prepToken.Text)
				{
					case "#define":
						return DefineToken.Parse(parent, scope, prepToken);
					case "#include":
						return IncludeToken.Parse(parent, scope, prepToken);
					case "#replace":
						return ReplaceSetToken.Parse(parent, scope, prepToken);
					case "#insert":
						return InsertToken.Parse(parent, scope, prepToken);
					default:
						SeekEndOfLine();
						prepToken.Instructions = GetText(new Span(prepEndPos, Position)).Trim();
						return prepToken;
				}
			}

			return token;
		}

		private Token ParseAfterDataType(Token parent, Scope scope, Token dataTypeToken)
		{
			var startPos = scope.File.Position;

			// Function or variable name.
			var token = ParseSingleToken(parent, scope);
			if (token == null) return null;

			if (token.GetType() == typeof(IdentifierToken))
			{
				// Name after a data type means this is a either a variable declaration or function definition.
				var nameToken = token as IdentifierToken;

				var funcToken = FunctionToken.TryParse(parent, scope, dataTypeToken, nameToken);
				if (funcToken != null) return funcToken;

				var varDeclToken = VariableDeclarationToken.TryParse(parent, scope, dataTypeToken, nameToken);
				if (varDeclToken != null) return varDeclToken;
			}

			scope.File.Position = startPos;
			return null;
		}

		private Token ParseExtern(Token parent, Scope scope, KeywordToken externToken)
		{
			var externScope = scope;
			externScope.Hint |= ScopeHint.SuppressFunctionCall | ScopeHint.SuppressFunctionDefinition | ScopeHint.SuppressVarDecl;

			List<Token> tokens;

			// Function declaration with data type
			if (TryParseTokenSet(parent, externScope, new Type[] { typeof(DataTypeToken), typeof(IdentifierToken), typeof(BracketsToken) }, out tokens))
			{
				var token = TryParseToken<StatementEndToken>(parent, externScope, typeof(StatementEndToken));
				if (token != null) tokens.Add(token);
				return new ExternFunctionToken(parent, externScope, tokens, tokens[0], tokens[1] as IdentifierToken, tokens[2] as BracketsToken);
			}

			// Function declaration with data type word
			if (TryParseTokenSet(parent, externScope, new Type[] { typeof(WordToken), typeof(IdentifierToken), typeof(BracketsToken) }, out tokens))
			{
				var token = TryParseToken<StatementEndToken>(parent, externScope, typeof(StatementEndToken));
				if (token != null) tokens.Add(token);
				return new ExternFunctionToken(parent, externScope, tokens, tokens[0], tokens[1] as IdentifierToken, tokens[2] as BracketsToken);
			}

			// Function declaration without data type
			if (TryParseTokenSet(parent, externScope, new Type[] { typeof(IdentifierToken), typeof(BracketsToken) }, out tokens))
			{
				var token = TryParseToken<StatementEndToken>(parent, externScope, typeof(StatementEndToken));
				if (token != null) tokens.Add(token);
				return new ExternFunctionToken(parent, externScope, tokens, null, tokens[0] as IdentifierToken, tokens[1] as BracketsToken);
			}

			Token dataTypeToken = null;
			List<WordToken> nameTokens = null;

			// Variable declaration with data type
			if (TryParseTokenSet(parent, externScope, new Type[] { typeof(DataTypeToken), typeof(WordToken) }, out tokens))
			{
				dataTypeToken = tokens[0];
				nameTokens = new List<WordToken>();
				nameTokens.Add(tokens[1] as WordToken);
			}
			// Variable declaration with data type word
			else if (TryParseTokenSet(parent, externScope, new Type[] { typeof(WordToken), typeof(WordToken) }, out tokens))
			{
				dataTypeToken = tokens[0];
				nameTokens = new List<WordToken>();
				nameTokens.Add(tokens[1] as WordToken);
			}
			// Variable declaration without data type
			else if (TryParseTokenSet(parent, externScope, new Type[] { typeof(WordToken) }, out tokens))
			{
				nameTokens = new List<WordToken>();
				nameTokens.Add(tokens[0] as WordToken);
			}
			else return externToken;

			// Need to parse for more variable names or statement end.
			while (true)
			{
				var token = ParseSingleToken(parent, externScope);
				if (token == null) break;
				if (token.GetType() == typeof(StatementEndToken))
				{
					tokens.Add(token);
					break;
				}
				else if (token.GetType() == typeof(DelimiterToken))
				{
					tokens.Add(token);
				}
				else if (typeof(WordToken).IsAssignableFrom(token.GetType()))
				{
					tokens.Add(token);
					nameTokens.Add(token as WordToken);
				}
				else
				{
					// Unknown token, stop the statement here.
					Position = token.Span.Start;
					break;
				}
			}

			return new ExternVariableToken(parent, scope, tokens, dataTypeToken, nameTokens);
		}

		public Token ParseSingleToken(Token parent, Scope scope)
		{
			if (_pos >= _length) return null;

			while (_pos < _length)
			{
				var startPos = Position;
				var ch = _source[_pos];

				if (Char.IsWhiteSpace(ch))
				{
					// WhiteSpace
					MoveNext();
					while (_pos < _length && Char.IsWhiteSpace(_source[_pos])) MoveNext();
					if (_pos >= _length) return null;

					startPos = Position;
					ch = _source[_pos];
				}

				if (ch == '/')
				{
					if (_pos + 1 < _length && _source[_pos + 1] == '/')
					{
						// Single-line comment
						SeekEndOfLine();
						continue;
					}
					else if (_pos + 1 < _length && _source[_pos + 1] == '*')
					{
						// Multi-line comment
						SeekMatching("*/");
						MoveNext(2);    // Skip past comment end
						continue;
					}
					else if (_pos + 1 < _length && _source[_pos + 1] == '=')
					{
						// /= operator
						MoveNext(2);
						return new OperatorToken(parent, scope, new Span(startPos, Position), "/=");
					}
					else
					{
						// / operator
						MoveNext();
						return new OperatorToken(parent, scope, new Span(startPos, Position), "/");
					}
				}

				if (Char.IsLetter(ch) || ch == '_')
				{
					SeekNonWordChar();
					var span = new Span(startPos, Position);
					var text = GetText(span);
					if (text == "and" || text == "or")
					{
						return new OperatorToken(parent, scope, span, text);
					}
					else if (_dataTypes.Contains(text))
					{
						return new DataTypeKeywordToken(parent, scope, span, text);
					}
					//else if (IsDefinedDataType(text))
					//{
					//    return DataTypeToken.CreateFromDefinedWord(parent, scope, span, text);
					//}
					else if (_keywords.Contains(text))
					{
						return new KeywordToken(parent, scope, span, text);
					}
					else
					{
						return new IdentifierToken(parent, scope, span, GetText(span));
					}
				}

				if (Char.IsDigit(ch))
				{
					ParseNumber();
					var span = new Span(startPos, Position);
					return new NumberToken(parent, scope, span, GetText(span));
				}

				if (ch == '-')
				{
					if (_pos + 1 < _length && Char.IsDigit(_source[_pos + 1]))
					{
						// Number with leading minus sign
						ParseNumber();
						var span = new Span(startPos, Position);
						return new NumberToken(parent, scope, span, GetText(span));
					}
					else if (_pos + 1 < _length && _source[_pos + 1] == '=')
					{
						// -= operator
						MoveNext(2);
						return new OperatorToken(parent, scope, new Span(startPos, Position), "-=");
					}
					else
					{
						// - operator
						MoveNext();
						return new OperatorToken(parent, scope, new Span(startPos, Position), "-");
					}
				}

				if (ch == '+')
				{
					if (_pos + 1 < _length && _source[_pos + 1] == '=')
					{
						// += operator
						MoveNext(2);
						return new OperatorToken(parent, scope, new Span(startPos, Position), "+=");
					}
					else
					{
						// + operator
						MoveNext();
						return new OperatorToken(parent, scope, new Span(startPos, Position), "+");
					}
				}

				if (ch == '*')
				{
					if (_pos + 1 < _length && _source[_pos + 1] == '=')
					{
						// *= operator
						MoveNext(2);
						return new OperatorToken(parent, scope, new Span(startPos, Position), "*=");
					}
					else
					{
						// * operator
						MoveNext();
						return new OperatorToken(parent, scope, new Span(startPos, Position), "*");
					}
				}

				if (ch == '%')
				{
					if (_pos + 1 < _length && _source[_pos + 1] == '=')
					{
						// %= operator
						MoveNext(2);
						return new OperatorToken(parent, scope, new Span(startPos, Position), "%=");
					}
					else
					{
						// % operator
						MoveNext();
						return new OperatorToken(parent, scope, new Span(startPos, Position), "%");
					}
				}

				if (ch == '=')
				{
					if (_pos + 1 < _length && _source[_pos + 1] == '=')
					{
						// == operator
						MoveNext(2);
						return new OperatorToken(parent, scope, new Span(startPos, Position), "==");
					}
					else
					{
						// = operator
						MoveNext();
						return new OperatorToken(parent, scope, new Span(startPos, Position), "=");
					}
				}

				if (ch == '(')
				{
					return BracketsToken.Parse(parent, scope);
				}

				if (ch == ')')
				{
					MoveNext();
					return new OperatorToken(parent, scope, new Span(startPos, Position), ")");
				}

				if (ch == '{')
				{
					return BracesToken.Parse(parent, scope);
				}

				if (ch == '}')
				{
					MoveNext();
					return new OperatorToken(parent, scope, new Span(startPos, Position), "}");
				}

				if (ch == '[')
				{
					return ArrayBracesToken.Parse(parent, scope);
				}

				if (ch == ']')
				{
					MoveNext();
					return new OperatorToken(parent, scope, new Span(startPos, Position), "]");
				}

				if (ch == ';')
				{
					MoveNext();
					return new StatementEndToken(parent, scope, new Span(startPos, Position));
				}

				if (ch == ',')
				{
					MoveNext();
					return new DelimiterToken(parent, scope, new Span(startPos, Position));
				}

				if (ch == '.')
				{
					MoveNext();
					return new DotToken(parent, scope, new Span(startPos, Position));
				}

				if (ch == '\"' || ch == '\'')
				{
					ParseStringLiteral();
					var span = new Span(startPos, Position);
					return new StringLiteralToken(parent, scope, span, GetText(span));
				}

				if (ch == ':')
				{
					MoveNext();
					return new OperatorToken(parent, scope, new Span(startPos, Position), ":");
				}

				if (ch == '#')
				{
					MoveNext();	// Skip #
					SeekNonWordChar();
					var wordSpan = new Span(startPos, Position);

					return new PreprocessorToken(parent, scope, new Span(wordSpan.Start, Position), GetText(wordSpan));
				}

				{
					MoveNext();
					var span = new Span(startPos, Position);
					return new UnknownToken(parent, scope, span, GetText(span));
				}
			}

			return null;
		}

		public bool TryParseTokenSet(Token parent, Scope scope, IEnumerable<Type> tokenTypes, out List<Token> tokens)
		{
			var startPos = Position;
			var tokenList = new List<Token>();

			foreach (var type in tokenTypes)
			{
				var token = ParseToken(parent, scope);
				if (token == null)
				{
					Position = startPos;
					tokens = null;
					return false;
				}

				if (type.IsAssignableFrom(token.GetType()))
				{
					tokenList.Add(token);
				}
				else
				{
					Position = startPos;
					tokens = null;
					return false;
				}
			}

			tokens = tokenList;
			return true;
		}

		public T TryParseToken<T>(Token parent, Scope scope, Type tokenType) where T : class
		{
			var startPos = Position;

			var token = ParseToken(parent, scope);
			if (token == null) return null;

			if (!tokenType.IsAssignableFrom(token.GetType()))
			{
				Position = startPos;
				return null;
			}

			return token as T;
		}

		public bool SkipWhiteSpaceAndComments()
		{
			if (_pos >= _length) return false;

			while (_pos < _length)
			{
				var ch = _source[_pos];

				if (Char.IsWhiteSpace(ch))
				{
					// WhiteSpace
					MoveNext();
					while (_pos < _length && Char.IsWhiteSpace(_source[_pos])) MoveNext();
					if (_pos >= _length) return false;
					ch = _source[_pos];
				}

				if (ch == '/')
				{
					if (_pos + 1 < _length && _source[_pos + 1] == '/')
					{
						// Single-line comment
						SeekEndOfLine();
						continue;
					}
					else if (_pos + 1 < _length && _source[_pos + 1] == '*')
					{
						// Multi-line comment
						SeekMatching("*/");
						MoveNext(2);    // Skip past comment end
						continue;
					}
				}

				return true;
			}

			return false;
		}

		public void MoveNext()
		{
			if (_pos < _length)
			{
				if (_source[_pos] == '\n')
				{
					_lineNum++;
					_linePos = 0;
				}
				else
				{
					_linePos++;
				}
				_pos++;
			}

		}

		public void MoveNext(int numChars)
		{
			while (numChars > 0 && _pos < _length)
			{
				if (_source[_pos] == '\n')
				{
					_lineNum++;
					_linePos = 0;
				}
				else
				{
					_linePos++;
				}
				_pos++;
				numChars--;
			}
		}

		public void SeekEndOfLine()
		{
			while (_pos < _length && _source[_pos] != '\n') MoveNext();
		}

		public bool SeekMatching(string str)
		{
			if (string.IsNullOrEmpty(str)) throw new ArgumentException("Search term cannot be blank.");

			var maxPos = _length - str.Length;
			var startCh = str[0];
			while (_pos <= maxPos)
			{
				if (_source[_pos] == startCh && _source.Substring(_pos, str.Length) == str) return true;
				MoveNext();
			}

			return false;
		}

		public void SeekNonWordChar()
		{
			char ch;
			while (_pos < _length)
			{
				ch = _source[_pos];
				if (!Char.IsLetterOrDigit(ch) && ch != '_') return;
				MoveNext();
			}
		}

		public string GetText(Span span)
		{
			var startOffset = span.Start.Offset;
			if (startOffset < 0 || startOffset > _length) throw new ArgumentException("Span start offset is outside bounds.");

			var endOffset = span.End.Offset;
			if (endOffset < 0 || endOffset > _length || endOffset < startOffset) throw new ArgumentException("Span end offset is outside bounds.");

			return _source.Substring(startOffset, endOffset - startOffset);
		}

		public void ParseNumber()
		{
			int startPos = _pos;
			bool gotDot = false;
			char ch;

			while (_pos < _length)
			{
				ch = _source[_pos];
				if (Char.IsDigit(ch))
				{
					MoveNext();
				}
				else if (ch == '.')
				{
					if (gotDot) return;
					gotDot = true;
					MoveNext();
				}
				else if (ch == '-' && _pos == startPos)
				{
					// Leading minus sign
					MoveNext();
				}
				else return;
			}
		}

		public void ParseStringLiteral()
		{
			var startCh = _source[_pos];
			var ch = '\0';
			var lastCh = '\0';

			MoveNext(); // Move past starting char.

			while (_pos < _length)
			{
				ch = _source[_pos];
				MoveNext();
				if (ch == '\\') MoveNext();	// Move past escaped char
				else if (ch == startCh) return;
				lastCh = ch;
			}
		}

		public bool EndOfFile
		{
			get { return _pos >= _length; }
		}

		public bool IsMatch(string text)
		{
			return _pos + text.Length <= _source.Length && _source.Substring(_pos, text.Length) == text;
		}

		public bool SkipMatch(string text)
		{
			if (IsMatch(text))
			{
				MoveNext(text.Length);
				return true;
			}
			return false;
		}

		public char PeekChar(int offset)
		{
			if (offset >= 0 && offset < _length) return _source[offset];
			return '\0';
		}
		#endregion

		#region IGroupToken
		IEnumerable<Token> IGroupToken.SubTokens
		{
			get { return _tokens; }
		}
		#endregion

		#region Debugging
		public string DumpTree()
		{
			var settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.OmitXmlDeclaration = true;

			var sb = new StringBuilder();
			using (var xml = XmlWriter.Create(sb, settings))
			{
				DumpTree(xml);
			}
			return sb.ToString();
		}

		public override void DumpTree(XmlWriter xml)
		{
			xml.WriteStartDocument();
			xml.WriteStartElement("CodeFile");
			foreach (var t in _tokens) t.DumpTree(xml);
			xml.WriteEndElement();
			xml.WriteEndDocument();
		}
		#endregion

		#region Position calculations
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

		public Position FindPosition(int lineNum, int linePos)
		{
			int pos = 0;
			int seekLineNum = 0;
			int seekLinePos = 0;

			while (pos < _length)
			{
				if (_source[pos] == '\n')
				{
					if (seekLineNum == lineNum)
					{
						return new Position(pos, seekLineNum, seekLinePos + 1);
					}

					seekLineNum++;
					seekLinePos = 0;
				}
				else
				{
					seekLinePos++;
				}
				pos++;

				if (seekLineNum == lineNum && seekLinePos == linePos)
				{
					return new Position(pos, lineNum, linePos);
				}
			}

			return new Position(pos, seekLineNum, seekLinePos);
		}

		public Position FindPosition(int offset)
		{
			int pos = 0;
			int lineNum = 0;
			int linePos = 0;

			if (offset > _length) offset = _length;
			while (pos < offset)
			{
				if (_source[pos] == '\n')
				{
					lineNum++;
					linePos = 0;
				}
				else
				{
					linePos++;
				}
				pos++;
			}

			return new Position(pos, lineNum, linePos);
		}

		public Position FindStartOfLine(Position pos)
		{
			var offset = pos.Offset;
			if (offset > _length) offset = _length;

			while (offset > 0 && _source[offset - 1] != '\n') offset--;

			return new Position(offset, pos.LineNum, 0);
		}

		public Position FindEndOfPreviousLine(Position pos)
		{
			var startOfLine = FindStartOfLine(pos);
			if (startOfLine.Offset <= 0) return startOfLine;

			var offset = startOfLine.Offset - 1;
			var linePos = 0;
			var lineEndOffset = offset;
			while (offset > 0 && _source[offset - 1] != '\n')
			{
				if (_source[offset] != '\r' && _source[offset] != '\n') linePos++;
				else lineEndOffset = offset;
				offset--;
			}

			return new Position(lineEndOffset, startOfLine.LineNum - 1, linePos);
		}

		public Position FindEndOfLine(Position pos)
		{
			var offset = pos.Offset;
			var linePos = pos.LinePos;
			if (offset < 0) throw new ArgumentOutOfRangeException("pos.Offset");

			while (offset < _length && !_source[offset].IsEndOfLineChar())
			{
				offset++;
				linePos++;
			}

			return new Position(offset, pos.LineNum, linePos);
		}

		public Position FindStartOfNextLine(Position pos)
		{
			var offset = pos.Offset;
			if (offset < 0) throw new ArgumentOutOfRangeException("pos.Offset");

			while (offset < _length && !_source[offset].IsEndOfLineChar()) offset++;
			if (offset < _length && _source[offset].IsEndOfLineChar())
			{
				while (offset < _length && _source[offset].IsEndOfLineChar()) offset++;
				return new Position(offset, pos.LineNum + 1, 0);
			}
			else
			{
				return new Position(offset, pos.LineNum, CalculateLinePos(offset));
			}
		}

		public int CalculateLinePos(int offset)
		{
			if (offset < 0 || offset > _length) throw new ArgumentOutOfRangeException("offset");

			var linePos = 0;

			while (offset > 0 && !_source[offset - 1].IsEndOfLineChar())
			{
				offset--;
				linePos++;
			}

			return linePos;
		}
		#endregion

		#region Auto-Completion and Function Signatures
		public override IEnumerable<AutoCompletionItem> GetAutoCompletionItems(Position pos)
		{
			foreach (var item in base.GetAutoCompletionItems(pos)) yield return item;

			var processedFiles = new List<string>();
			if (!string.IsNullOrEmpty(_fileName)) processedFiles.Add(_fileName.ToLower());
		}

		private AutoCompletionItem[] _globalAutoCompletionItems = null;
		public override IEnumerable<AutoCompletionItem> GetGlobalAutoCompletionItems()
		{
			if (_globalAutoCompletionItems == null)
			{
				var list = new List<AutoCompletionItem>();
				foreach (var token in _tokens)
				{
					list.AddRange(token.GetGlobalAutoCompletionItems());
				}
				_globalAutoCompletionItems = list.ToArray();
			}

			return _globalAutoCompletionItems;
		}

		List<FunctionSignature> _functionSignatures = null;
		public override IEnumerable<FunctionSignature> GetFunctionSignatures()
		{
			if (_functionSignatures == null)
			{
				_functionSignatures = base.GetFunctionSignatures().ToList();
			}
			return _functionSignatures;
		}

		//public void AddDefinedDataType(string name)
		//{
		//    _definedDataTypes.Add(name);
		//}

		//public bool IsDefinedDataType(string name)
		//{
		//    if (_definedDataTypes.Contains(name)) return true;

		//    foreach (var file in _includeFiles)
		//    {
		//        if (file.IsDefinedDataType(name)) return true;
		//    }

		//    return false;
		//}
		#endregion

		#region Include Files
		public IEnumerable<IncludeToken.IncludeDef> IncludeFiles
		{
			get { return _includeFiles; }
		}
		#endregion
	}
}
