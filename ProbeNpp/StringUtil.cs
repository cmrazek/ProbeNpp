using System;
using System.Collections.Generic;
using System.Text;

namespace ProbeNpp
{
	internal static class StringUtil
	{
		public static bool IsEndOfLineChar(this char ch)
		{
			return ch == '\r' || ch == '\n';
		}
	}

	internal class StringComparerIgnoreCase : IComparer<string>
	{
		public int Compare(string a, string b)
		{
			return a.ToLower().CompareTo(b.ToLower());
		}
	}
}
