using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp
{
	internal static class Util
	{
		/// <summary>
		/// Splits a string containing semi-colon delimited paths into a list of strings.
		/// </summary>
		/// <param name="str">The path string.</param>
		/// <returns>A list of paths contained in the string.</returns>
		public static List<string> ParsePathString(string str)
		{
			var ret = new List<string>();
			foreach (var path in str.Split(';'))
			{
				if (!string.IsNullOrWhiteSpace(path)) ret.Add(path.Trim());
			}
			return ret;
		}

		/// <summary>
		/// Splits a string containing distinct words delimited by whitespace.
		/// </summary>
		/// <param name="wordList">The string to be split.</param>
		/// <returns>A hash set containing the word list.</returns>
		public static HashSet<string> ParseWordList(string wordList)
		{
			var ret = new HashSet<string>();
			foreach (var word in (from w in wordList.Split(' ', '\t', '\r', '\n') where !string.IsNullOrEmpty(w) select w))
			{
				ret.Add(word);
			}
			return ret;
		}
	}
}
