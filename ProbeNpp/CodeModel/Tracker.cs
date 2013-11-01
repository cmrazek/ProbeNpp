using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NppSharp;

namespace ProbeNpp
{
	internal class Tracker
	{
	    private int _origLength;
	    private int _curLength;
	    private List<Edit> _edits = new List<Edit>();

	    private struct Edit
	    {
			public bool insert;
			public TextLocation startLoc;
			public TextLocation endLoc;
	    }

	    public Tracker(string source)
	    {
	        _curLength = _origLength = source.Length;
	    }

		public void Modify(TextLocation location, string text, bool insert)
		{
			var lineEndIndex = text.LastIndexOf('\n');
			var edit = lineEndIndex < 0 ?
				new Edit
				{
					insert = insert,
					startLoc = location,
					endLoc = new TextLocation(location.Line, location.CharPosition + text.Length)
				}
				:
				new Edit
				{
					insert = insert,
					startLoc = location,
					endLoc = new TextLocation(location.Line + text.Count(c => c == '\n'), text.Length - lineEndIndex + 1)
				};
			_edits.Insert(0, edit);
		}

		public void TranslateToSnapshot(ref TextLocation point)
		{
			foreach (var edit in _edits)
			{
				if (edit.startLoc > point)
				{
					// Edit occurred after the point, so the position is not affected.
				}
				else if (edit.endLoc.Line < point.Line)
				{
					// Occurred on a line prior to this one.
					var line = point.Line;
					if (edit.insert) line -= edit.endLoc.Line - edit.startLoc.Line;
					else line += edit.endLoc.Line - edit.startLoc.Line;
					point.Line = line <= 0 ? 1 : line;
					continue;
				}
				else if (edit.endLoc < point)
				{
					// Edit affects this line, but before the point.

					var line = point.Line;
					var charPos = point.CharPosition;
					if (edit.insert)
					{
						line -= edit.endLoc.Line - edit.startLoc.Line;
						charPos = point.CharPosition - (edit.endLoc.CharPosition - 1) + (edit.startLoc.CharPosition - 1);
					}
					else // delete
					{
						line += edit.endLoc.Line - edit.startLoc.Line;
						charPos = point.CharPosition + (edit.endLoc.CharPosition - 1) - (edit.startLoc.CharPosition - 1);
					}
					point.Line = line <= 0 ? 1 : line;
					point.CharPosition = charPos <= 0 ? 1 : charPos;
				}
				else
				{
					// Edit surrounds the point.
					if (edit.insert) point = edit.startLoc;
					else point = edit.endLoc;
				}
			}
		}
	}
}
