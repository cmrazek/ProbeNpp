#define DISABLE

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
		public SmartIndentManager()
		{ }

		public void OnCharAdded(CharAddedEventArgs e)
		{
#if !DISABLE
			var app = ProbeNppPlugin.Instance;

			if (!app.Settings.Editor.SmartIndent) return;

			if (e.Character == '\n')
			{
				var curLine = app.CurrentLine;
				var curPos = app.CurrentLocation;
				var prevLineText = curLine > 1 ? app.GetLineText(curLine - 1) : "";
				if (app.IsProbeLanguage)
				{
					if (IndentNextLine(prevLineText))
					{
						var indent = GetIndentText(prevLineText, "\t");
						app.SetSelection(app.GetLineStartPos(curLine), curPos);
						app.Insert(indent);
					}
					else
					{
						var indent = GetIndentText(prevLineText);
						app.SetSelection(app.GetLineStartPos(curLine), curPos);
						app.Insert(indent);
					}
				}
				else
				{
					var indent = GetIndentText(prevLineText);
					app.SetSelection(app.GetLineStartPos(curLine), curPos);
					app.Insert(indent);
				}
			}
			else if (e.Character == '}' && app.IsProbeLanguage)
			{
				var curLoc = app.CurrentLocation;
				var curLine = app.CurrentLine;
				if (app.GetText(app.GetLineStartPos(curLoc.Line), curLoc).TrimStart() == "}")
				{
					TextLocation openLoc;
					if (TryFindOpeningBrace(curLoc, out openLoc))
					{
						var openIndent = app.GetText(app.GetLineStartPos(openLoc.Line), GetIndentLocation(openLoc.Line));
						app.SetSelection(app.GetLineStartPos(curLine), GetIndentLocation(curLine));
						app.Insert(openIndent);
						curLoc = app.CurrentLocation + 1;
						app.SetSelection(curLoc, curLoc);
					}
				}
			}
#endif
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
			var app = ProbeNppPlugin.Instance;
			var loc = app.GetLineStartPos(lineNum);

			while (loc < app.End)
			{
				var str = app.GetText(loc, 1);
				if (str.Length == 1 && char.IsWhiteSpace(str[0])) loc++;
				else break;
			}

			return loc;
		}

		private bool TryFindOpeningBrace(TextLocation closeBraceLoc, out TextLocation openBraceLoc)
		{
			var app = ProbeNppPlugin.Instance;
			var source = app.GetText(app.Start, app.CurrentLocation);
			var sourceLength = source.Length;
			var found = false;
			var openLoc = closeBraceLoc;

			var parser = new TokenParser.Parser(source);
			while (parser.ReadNestable())
			{
				if (parser.TokenType == TokenParser.TokenType.Nested &&
					parser.TokenText.EndsWith("}") &&
					parser.Position.Offset == sourceLength)
				{
					openLoc = parser.TokenStartPostion.ToNppSharpTextLocation();
					found = true;
				}
			}

			if (found)
			{
				openBraceLoc = openLoc;
				return true;
			}
			else
			{
				openBraceLoc = closeBraceLoc;
				return false;
			}
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
