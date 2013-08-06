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
		private ProbeNppPlugin _app;

		private static readonly Regex _rxAutoCompleteWord = new Regex(@"\b\w+$");
		private static readonly Regex _rxFuncCall = new Regex(@"\b(\w+)\s*\($");

		public AutoCompletionManager(ProbeNppPlugin app)
		{
			_app = app;
		}

		public void OnCharAdded(CharAddedEventArgs e)
		{
			if (e.Character == '.')
			{
				var wordEnd = _app.CurrentLocation - 1;
				var wordStart = _app.GetWordStartPos(wordEnd, false);
				var word = _app.GetText(wordStart, wordEnd);

				if (!string.IsNullOrWhiteSpace(word))
				{
					var table = _app.Environment.GetTable(word);
					if (table == null) return;

					var fields = table.Fields;
					if (!fields.Any()) return;
					_app.ShowAutoCompletion(0, (from f in fields orderby f.Name.ToLower() select f.Name), true);
				}
			}
			else if (char.IsLetterOrDigit(e.Character))
			{
				if (!_app.AutoCompletionIsActive)
				{
					var lineText = _app.GetText(_app.GetLineStartPos(_app.CurrentLine), _app.CurrentLocation);

					Match match;
					if ((match = _rxAutoCompleteWord.Match(lineText)).Success)
					{
						var word = match.Value;

						var model = _app.CurrentModel;
						if (model != null)
						{
							_app.ShowAutoCompletion(word.Length, GetSoloAutoCompletionItems(_app.CurrentLocation, word));
						}
					}
				}
			}
			else if (e.Character == '(')
			{
				if (!_app.FunctionSignatureIsActive)
				{
					var lineText = _app.GetText(_app.GetLineStartPos(_app.CurrentLine), _app.CurrentLocation);

					Match match;
					if ((match = _rxFuncCall.Match(lineText)).Success)
					{
						var funcName = match.Groups[1].Value;

						var entered = match.Length - (match.Groups[1].Index - match.Index);
						var callTipPos = new TextLocation(_app.CurrentLine, _app.CurrentLocation.CharPosition - entered);

						var funcSig = GetFunctionSignature(funcName);
						if (!string.IsNullOrEmpty(funcSig))
						{
							_app.ShowFunctionSignature(callTipPos, funcSig);

							int highlightStart, highlightLength;
							if (GetFunctionSignatureHighlightRange(funcSig, 0, out highlightStart, out highlightLength))
							{
								_app.SetFunctionSignatureHighlight(highlightStart, highlightLength);
							}
						}
					}
				}
			}
			else if (e.Character == ',')
			{
				var sigParser = new FunctionSignatureParser(_app);
				if (sigParser.GetFuncSigName(_app.CurrentLocation))
				{
					var funcSig = GetFunctionSignature(sigParser.FunctionName);
					if (!string.IsNullOrEmpty(funcSig))
					{
						_app.ShowFunctionSignature(_app.CurrentLocation, funcSig);

						int highlightStart, highlightLength;
						if (GetFunctionSignatureHighlightRange(funcSig, sigParser.CommaCount, out highlightStart, out highlightLength))
						{
							_app.SetFunctionSignatureHighlight(highlightStart, highlightLength);
						}
					}
				}
			}
			else if (e.Character == ')')
			{
				if (_app.FunctionSignatureIsActive) _app.CancelFunctionSignature();
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
			var model = _app.CurrentModel;
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

				foreach (var item in (from t in _app.Environment.AutoCompletionTables where t.Text.StartsWith(startsWith) select t.Text))
				{
					list.Add(item);
				}

				if (_app.LanguageName == Res.ProbeSourceLanguageName)
				{
					foreach (var item in (from k in _app.SourceKeywords where k.StartsWith(startsWith) select k))
					{
						list.Add(item);
					}
				}
				else if (_app.LanguageName == Res.ProbeDictLanguageName)
				{
					foreach (var item in (from k in _app.DictKeywords where k.StartsWith(startsWith) select k))
					{
						list.Add(item);
					}
				}

				foreach (var item in (from f in _app.FunctionSignatures.Keys where f.StartsWith(startsWith) select f))
				{
					list.Add(item);
				}

				foreach (var item in (from k in _app.DataTypes where k.StartsWith(startsWith) select k))
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
			var model = _app.CurrentModel;
			if (model != null)
			{
				var func = (from f in model.FunctionSignatures where f.Name == funcName select f).FirstOrDefault();
				if (func != null) return func.Signature;
			}

			var funcSig = (from f in _app.FunctionSignatures.Keys where f == funcName select _app.FunctionSignatures[f]).FirstOrDefault();
			if (!string.IsNullOrEmpty(funcSig)) return funcSig;

			return null;
		}
	}
}
