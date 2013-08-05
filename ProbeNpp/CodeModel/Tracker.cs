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
					if (edit.insert) point.Line -= edit.endLoc.Line - edit.startLoc.Line;
					else point.Line += edit.endLoc.Line - edit.startLoc.Line;
					continue;
				}
				else if (edit.endLoc < point)
				{
					// Edit affects this line, but before the point.

					if (edit.insert)
					{
						point.Line -= edit.endLoc.Line - edit.startLoc.Line;
						point.CharPosition = point.CharPosition - (edit.endLoc.CharPosition - 1) + (edit.startLoc.CharPosition - 1);
					}
					else // delete
					{
						point.Line += edit.endLoc.Line - edit.startLoc.Line;
						point.CharPosition = point.CharPosition + (edit.endLoc.CharPosition - 1) - (edit.startLoc.CharPosition - 1);
					}
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
