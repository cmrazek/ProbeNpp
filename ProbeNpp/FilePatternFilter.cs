using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProbeNpp
{
	internal class FilePatternFilter
	{
		private bool _all;
		private List<Regex> _filters = new List<Regex>();

		private const string k_specialRegexChars = "[]\\^$.|?*+()";

		public FilePatternFilter()
		{
		}

		public FilePatternFilter(string filterList)
		{
			AddFilterList(filterList);
		}

		public bool AddFilter(string filter)
		{
			filter = filter.Trim();
			if (string.IsNullOrEmpty(filter)) return false;

			switch (filter)
			{
				case "*":
				case "*.*":
					_all = true;
					_filters.Clear();
					return true;
			}

			if (!_all)
			{
				var sb = new StringBuilder();
				sb.Append("^");

				foreach (var ch in filter)
				{
					if (ch == '*')
					{
						sb.Append(".*");
					}
					else if (ch == '?')
					{
						sb.Append('.');
					}
					else if (k_specialRegexChars.IndexOf(ch) >= 0)
					{
						sb.Append('\\');
						sb.Append(ch);
					}
					else if (char.IsWhiteSpace(ch))
					{
						sb.Append("\\s");
					}
					else
					{
						sb.Append(ch);
					}
				}

				sb.Append("$");
				_filters.Add(new Regex(sb.ToString(), RegexOptions.IgnoreCase));
			}
			
			return true;
		}

		public bool IsEmpty
		{
			get { return !_all && _filters.Count == 0; }
		}

		public bool IsMatch(string pathName)
		{
			if (_all) return true;
			if (_filters.Count == 0) return false;

			string fileName;
			try
			{
				fileName = Path.GetFileName(pathName);
			}
			catch (Exception)
			{
				fileName = pathName;
			}

			return (from f in _filters where f.IsMatch(fileName) select f).Any();
		}

		public void AddFilterList(string filterList)
		{
			foreach (var filter in filterList.Split(';'))
			{
				if (!string.IsNullOrWhiteSpace(filter))
				{
					AddFilter(filter.Trim());
				}
			}
		}
	}
}
