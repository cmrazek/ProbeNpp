using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.FindInProbeFiles
{
	internal class FindMatch
	{
		private string _fileName;
		private int _lineNum;
		private string _lineText;
		private List<HighlightSpan> _highlightSpans = new List<HighlightSpan>();

		public class HighlightSpan
		{
			private int _start;
			private int _length;

			public HighlightSpan(int start, int length)
			{
				if (start < 0) throw new ArgumentOutOfRangeException("start");
				if (length <= 0) throw new ArgumentOutOfRangeException("length");

				_start = start;
				_length = length;
			}

			public int Start
			{
				get { return _start; }
			}

			public int Length
			{
				get { return _length; }
			}
		}

		public FindMatch(string fileName, int lineNum, string lineText, int highlightStartIndex, int highlightLength)
		{
			_fileName = fileName;
			_lineNum = lineNum;
			_lineText = lineText;
			_highlightSpans.Add(new HighlightSpan(highlightStartIndex, highlightLength));
		}

		public string FileName
		{
			get { return _fileName; }
		}

		public int LineNumber
		{
			get { return _lineNum; }
		}

		public string LineText
		{
			get { return _lineText; }
		}

		public bool IsSameLine(FindMatch match)
		{
			return string.Equals(_fileName, match._fileName, StringComparison.OrdinalIgnoreCase) && _lineNum == match._lineNum;
		}

		public void Merge(FindMatch match)
		{
			foreach (var span in match._highlightSpans) _highlightSpans.Add(span);
			RecalculateHighlightSpans();
		}

		private void RecalculateHighlightSpans()
		{
			var spans = new List<HighlightSpan>();
			var spanStart = -1;
			var textLength = _lineText.Length;

			for (int index = 0; index < textLength; index++)
			{
				if (IsCharWithinSpan(index))
				{
					if (spanStart == -1) spanStart = index;
				}
				else
				{
					if (spanStart != -1)
					{
						spans.Add(new HighlightSpan(spanStart, index - spanStart));
						spanStart = -1;
					}
				}
			}

			_highlightSpans = spans;
		}

		private bool IsCharWithinSpan(int index)
		{
			return (from s in _highlightSpans where s.Start <= index && s.Start + s.Length > index select s).Any();
		}

		public IEnumerable<HighlightSpan> HighlightSpans
		{
			get { return _highlightSpans; }
		}
	}
}
