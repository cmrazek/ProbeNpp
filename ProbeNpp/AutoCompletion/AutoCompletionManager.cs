using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NppSharp;

namespace ProbeNpp.AutoCompletion
{
	internal class AutoCompletionManager
	{
		private static readonly Regex _rxAutoCompleteWord = new Regex(@"\b[a-zA-Z_]\w*$");
		private static readonly Regex _rxFuncCall = new Regex(@"\b([a-zA-Z_]\w*)\s*\($");
		private static readonly Regex _rxTableField = new Regex(@"\b([a-zA-Z_]\w*)\.([a-zA-Z_]\w*)$");

		public AutoCompletionManager()
		{
		}

		public void OnCharAdded(CharAddedEventArgs e)
		{
			var app = ProbeNppPlugin.Instance;

			if (!app.Settings.Editor.AutoCompletion) return;
			if (!IsAutoCompletionAllowedHere(app.CurrentLocation)) return;

			if (e.Character == '.')
			{
				var wordEnd = app.CurrentLocation - 1;
				var wordStart = app.GetWordStartPos(wordEnd, false);
				var word = app.GetText(wordStart, wordEnd);

				if (!string.IsNullOrWhiteSpace(word))
				{
					var table = ProbeEnvironment.GetTable(word);
					if (table == null) return;

					var fields = table.Fields;
					if (!fields.Any()) return;
					app.ShowAutoCompletion(0, (from f in fields orderby f.Name.ToLower() select f.Name), true);
				}
			}
			else if (char.IsLetterOrDigit(e.Character))
			{
				if (!app.AutoCompletionIsActive)
				{
					var lineText = app.GetText(app.GetLineStartPos(app.CurrentLine), app.CurrentLocation);

					Match match;
					ProbeTable table;
					if ((match = _rxTableField.Match(lineText)).Success &&
						(table = ProbeEnvironment.GetTable(match.Groups[1].Value)) != null)
					{
						var field = match.Groups[2].Value;
						app.ShowAutoCompletion(field.Length, (from f in table.Fields orderby f.Name select f.Name), true);
					}
					else if ((match = _rxAutoCompleteWord.Match(lineText)).Success)
					{
						var word = match.Value;

						var model = app.CurrentModel;
						if (model != null)
						{
							app.ShowAutoCompletion(word.Length, GetSoloAutoCompletionItems(app.CurrentLocation, word));
						}
					}
				}
			}
			else if (e.Character == '(')
			{
				if (!app.FunctionSignatureIsActive)
				{
					var lineText = app.GetText(app.GetLineStartPos(app.CurrentLine), app.CurrentLocation);

					Match match;
					if ((match = _rxFuncCall.Match(lineText)).Success)
					{
						var funcName = match.Groups[1].Value;

						var entered = match.Length - (match.Groups[1].Index - match.Index);
						var callTipPos = new TextLocation(app.CurrentLine, app.CurrentLocation.CharPosition - entered);

						var funcSig = GetFunctionSignature(funcName);
						if (!string.IsNullOrEmpty(funcSig))
						{
							app.ShowFunctionSignature(callTipPos, funcSig);

							int highlightStart, highlightLength;
							if (GetFunctionSignatureHighlightRange(funcSig, 0, out highlightStart, out highlightLength))
							{
								app.SetFunctionSignatureHighlight(highlightStart, highlightLength);
							}
						}
					}
				}
			}
			else if (e.Character == ',')
			{
				var sigParser = new FunctionSignatureParser();
				if (sigParser.GetFuncSigName(app.CurrentLocation))
				{
					var funcSig = GetFunctionSignature(sigParser.FunctionName);
					if (!string.IsNullOrEmpty(funcSig))
					{
						app.ShowFunctionSignature(app.CurrentLocation, funcSig);

						int highlightStart, highlightLength;
						if (GetFunctionSignatureHighlightRange(funcSig, sigParser.CommaCount, out highlightStart, out highlightLength))
						{
							app.SetFunctionSignatureHighlight(highlightStart, highlightLength);
						}
					}
				}
			}
			else if (e.Character == ')')
			{
				if (app.FunctionSignatureIsActive) app.CancelFunctionSignature();
			}
		}

		private bool GetFunctionSignatureHighlightRange(string sig, int argToHighlight, out int startIndex, out int length)
		{
			// Get the last set of closed brackets
			var parser = new TokenParser.Parser(sig);
			int argsStart = -1;
			int argsStartUnclosed = -1;
			int argsEnd = -1;
			while (parser.ReadNestable())
			{
				if (parser.TokenType == TokenParser.TokenType.Nested && parser.TokenText.StartsWith("("))
				{
					argsStart = parser.TokenStartPostion.Offset + 1;
					argsEnd = parser.Position.Offset - 1;
				}
				else if (parser.TokenType == TokenParser.TokenType.Operator && parser.TokenText == "(")
				{
					argsStartUnclosed = parser.TokenStartPostion.Offset;
				}
			}

			if (argsStart == -1)
			{
				argsStart = argsStartUnclosed;
				argsEnd = sig.Length;
			}
			if (argsStart == -1)
			{
				startIndex = length = 0;
				return false;
			}

			// Parse the argument text to get the parameters
			var args = parser.GetText(argsStart, argsEnd - argsStart);
			parser.SetSource(args);

			int argCount = 0;
			int argStart = 0;
			while (parser.ReadNestable())
			{
				if (parser.TokenType == TokenParser.TokenType.Operator && parser.TokenText == ",")
				{
					if (argCount == argToHighlight)
					{
						startIndex = argStart + argsStart;
						length = parser.TokenStartPostion.Offset - argStart;
						return true;
					}
					else
					{
						argCount++;
						argStart = parser.Position.Offset;
					}
				}
			}

			startIndex = argStart + argsStart;
			length = parser.Position.Offset - argStart;
			return true;
		}

		private IEnumerable<string> GetSoloAutoCompletionItems(TextLocation location, string startsWith)
		{
			var app = ProbeNppPlugin.Instance;

			var model = app.CurrentModel;
			if (model != null)
			{
				var list = new SortedSet<string>();

				var modelLoc = location;
				model.Tracker.TranslateToSnapshot(ref modelLoc);
				var pos = model.GetPosition(modelLoc);

				foreach (var item in (from i in model.GetAutoCompletionItems(pos) where i.Text.StartsWith(startsWith) select i.Text))
				{
					list.Add(item);
				}

				foreach (var item in (from t in ProbeEnvironment.AutoCompletionTables where t.Text.StartsWith(startsWith) select t.Text))
				{
					list.Add(item);
				}

				if (app.LanguageName == Res.ProbeSourceLanguageName)
				{
					foreach (var item in (from k in app.SourceKeywords where k.StartsWith(startsWith) select k))
					{
						list.Add(item);
					}
				}
				else if (app.LanguageName == Res.ProbeDictLanguageName)
				{
					foreach (var item in (from k in app.DictKeywords where k.StartsWith(startsWith) select k))
					{
						list.Add(item);
					}
				}

				foreach (var item in (from f in app.FunctionSignatures.Keys where f.StartsWith(startsWith) select f))
				{
					list.Add(item);
				}

				var funcScanner = app.FunctionFileScanner;
				if (funcScanner != null)
				{
					foreach (var item in (from f in funcScanner.GetFunctionSignatures(startsWith) select f.Name))
					{
						list.Add(item);
					}
				}

				foreach (var item in (from k in app.DataTypes where k.StartsWith(startsWith) select k))
				{
					list.Add(item);
				}

				//list.Sort((a, b) => string.Compare(a, b, true));
				return list;
			}

			return new string[0];
		}

		private string GetFunctionSignature(string funcName)
		{
			var app = ProbeNppPlugin.Instance;

			var model = app.CurrentModel;
			if (model != null)
			{
				var func = (from f in model.FunctionSignatures where f.Name == funcName select f).FirstOrDefault();
				if (func != null) return func.Signature;
			}

			var funcSig = (from f in app.FunctionSignatures.Keys where f == funcName select app.FunctionSignatures[f]).FirstOrDefault();
			if (!string.IsNullOrEmpty(funcSig)) return funcSig;

			var funcScanner = app.FunctionFileScanner;
			if (funcScanner != null)
			{
				funcSig = funcScanner.GetFunctionSignature(funcName);
				if (!string.IsNullOrEmpty(funcSig)) return funcSig;
			}

			return null;
		}

		public bool IsAutoCompletionAllowedHere(TextLocation location)
		{
			var app = ProbeNppPlugin.Instance;
			var currentLine = app.CurrentLine;
			var lineText = app.GetLineText(currentLine);
			var state = currentLine > 1 ? app.GetLineState(currentLine - 1) : 0;

			var startPos = 0;
			if ((state & ProbeLexer.State_InsideComment) != 0)
			{
				var index = lineText.IndexOf("*/");
				if (index < 0) return false;
				startPos = index + 2;
			}

			if (startPos + 1 > location.CharPosition) return false;

			var parser = new TokenParser.Parser(app.GetLineText(currentLine));
			parser.ReturnComments = true;
			parser.ReturnWhiteSpace = true;
			if (startPos > 0) parser.SetOffset(startPos);

			while (parser.Read())
			{
				if (parser.Position.LinePos > location.CharPosition || parser.EndOfFile)
				{
					if (parser.TokenType == TokenParser.TokenType.Comment || parser.TokenType == TokenParser.TokenType.StringLiteral)
					{
						return false;
					}
					return true;
				}
			}

			if (parser.TokenType == TokenParser.TokenType.Comment || parser.TokenType == TokenParser.TokenType.StringLiteral)
			{
				return false;
			}
			return true;
		}
	}
}
