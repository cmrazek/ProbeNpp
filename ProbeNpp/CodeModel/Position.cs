using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel
{
    internal struct Position
    {
        private int _offset;
        private int _lineNum;
        private int _linePos;

        public Position(int offset, int lineNum, int linePos)
        {
            _offset = offset;
            _lineNum = lineNum;
            _linePos = linePos;
        }

        public int Offset
        {
            get { return _offset; }
        }

        public int LineNum
        {
            get { return _lineNum; }
        }

        public int LinePos
        {
            get { return _linePos; }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Position)) return false;
            return ((Position)obj)._offset == _offset;
        }

        public override int GetHashCode()
        {
            return _offset.GetHashCode();
        }

        public static bool operator <(Position a, Position b)
        {
            return a._offset < b._offset;
        }

        public static bool operator <=(Position a, Position b)
        {
            return a._offset <= b._offset;
        }

        public static bool operator >(Position a, Position b)
        {
            return a._offset > b._offset;
        }

        public static bool operator >=(Position a, Position b)
        {
            return a._offset >= b._offset;
        }

        public override string ToString()
        {
            return string.Format("Line: {0} Ch: {1} Offset: {2}", _lineNum, _linePos, _offset);
        }
    }
}
