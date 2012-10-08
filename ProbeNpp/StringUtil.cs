using System;
using System.Collections.Generic;
using System.Text;

namespace ProbeNpp
{
	internal class StringUtil
	{
		public static bool IsNullOrWhiteSpace(string str)
		{
			if (str == null) return true;
			foreach (char ch in str)
			{
				if (!Char.IsWhiteSpace(ch)) return false;
			}
			return true;
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
