using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel
{
	internal struct Span
	{
		private Position _start;
		private Position _end;

		public Span(Position start, Position end)
		{
			_start = start;
			_end = end;
		}

		public Position Start
		{
			get { return _start; }
		}

		public Position End
		{
			get { return _end; }
		}

		public override string ToString()
		{
			return string.Format("[{0}] - [{1}]", _start, _end);
		}

		public bool Contains(Position pos)
		{
			return _start <= pos && _end > pos;
		}

		public bool ContainsNearby(Position pos)
		{
			return _start <= pos && _end >= pos;
		}

		//public Microsoft.VisualStudio.TextManager.Interop.TextSpan ToVsTextSpan()
		//{
		//    return new Microsoft.VisualStudio.TextManager.Interop.TextSpan()
		//    {
		//        iStartLine = _start.LineNum,
		//        iStartIndex = _start.LinePos,
		//        iEndLine = _end.LineNum,
		//        iEndIndex = _end.LinePos
		//    };
		//}
	}
}
