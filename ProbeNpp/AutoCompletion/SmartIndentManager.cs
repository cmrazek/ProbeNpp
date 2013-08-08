using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NppSharp;

namespace ProbeNpp.AutoCompletion
{
	internal class SmartIndentManager
	{
		private ProbeNppPlugin _app;

		public SmartIndentManager(ProbeNppPlugin app)
		{
			_app = app;
		}

		public void OnCharAdded(CharAddedEventArgs e)
		{
			//if (!_app.Settings.Editor.SmartIndent) return;

			//if (e.Character == '\n')
			//{
			//    var curLine = _app.CurrentLine;
			//    var curPos = _app.CurrentLocation;
			//    var prevLineText = curLine > 1 ? _app.GetLineText(curLine - 1) : "";
			//    if (_app.IsProbeLanguage)
			//    {
			//        if (IndentNextLine(prevLineText))
			//        {
			//            var indent = GetIndentText(prevLineText, "\t");
			//            _app.SetSelection(_app.GetLineStartPos(curLine), curPos);
			//            _app.Insert(indent);
			//        }
			//        else
			//        {
			//            var indent = GetIndentText(prevLineText);
			//            _app.SetSelection(_app.GetLineStartPos(curLine), curPos);
			//            _app.Insert(indent);
			//        }
			//    }
			//    else
			//    {
			//        var indent = GetIndentText(prevLineText);
			//        _app.SetSelection(_app.GetLineStartPos(curLine), curPos);
			//        _app.Insert(indent);
			//    }
			//}
			//else if (e.Character == '}' && _app.IsProbeLanguage)
			//{
			//    var curLoc = _app.CurrentLocation;
			//    var curLine = _app.CurrentLine;
			//    if (_app.GetText(_app.GetLineStartPos(curLoc.Line), curLoc).TrimStart() == "}")
			//    {
			//        TextLocation openLoc;
			//        if (TryFindOpeningBrace(curLoc, out openLoc))
			//        {
			//            var openIndent = _app.GetText(_app.GetLineStartPos(openLoc.Line), GetIndentLocation(openLoc.Line));
			//            _app.SetSelection(_app.GetLineStartPos(curLine), GetIndentLocation(curLine));
			//            _app.Insert(openIndent);
			//            curLoc = _app.CurrentLocation + 1;
			//            _app.SetSelection(curLoc, curLoc);
			//        }
			//    }
			//}
		}

		public void OnModification(ModifiedEventArgs e)
		{
		}

		private string GetIndentText(string lineText, string append = null)
		{
			var pos = 0;
			var sb = new StringBuilder();

			while (pos < lineText.Length && char.IsWhiteSpace(lineText[pos]))
			{
				sb.Append(lineText[pos++]);
			}

			if (!string.IsNullOrEmpty(append)) sb.Append(append);

			return sb.ToString();
		}

		private TextLocation GetIndentLocation(int lineNum)
		{
			var loc = _app.GetLineStartPos(lineNum);

			while (loc < _app.End)
			{
				var str = _app.GetText(loc, 1);
				if (str.Length == 1 && char.IsWhiteSpace(str[0])) loc++;
				else break;
			}

			return loc;
		}

		private bool TryFindOpeningBrace(TextLocation closeBraceLoc, out TextLocation openBraceLoc)
		{
			var source = _app.GetText(_app.Start, _app.CurrentLocation);
			var sourceLength = source.Length;

			var parser = new TokenParser.Parser(source);
			while (parser.ReadNestable())
			{
				if (parser.TokenType == TokenParser.TokenType.Nested &&
					parser.TokenText.EndsWith("}") &&
					parser.Position.Offset == sourceLength)
				{
					openBraceLoc = parser.TokenStartPostion.ToNppSharpTextLocation();
					return true;
				}
			}

			openBraceLoc = closeBraceLoc;
			return false;
		}

		private bool IndentNextLine(string lineText)
		{
			var parser = new TokenParser.Parser(lineText);
			var first = true;
			var startsWithCase = false;
			TokenParser.TokenType type = TokenParser.TokenType.Unknown;
			string text = "";

			while (parser.Read())
			{
				type = parser.TokenType;
				text = parser.TokenText;

				if (first && type == TokenParser.TokenType.Word && text == "case") startsWithCase = true;
				if (first) first = false;
			}

			if (type == TokenParser.TokenType.Operator && text == "{") return true;
			if (startsWithCase && type == TokenParser.TokenType.Operator && text == ":") return true;
			return false;
		}
	}
}
