using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace ProbeNpp
{
	public class IniFile
	{
		private class IniSection
		{
			public Dictionary<string, string> values = new Dictionary<string, string>();
		}

		private string _fileName = "";
		private Dictionary<string, IniSection> _sections = new Dictionary<string, IniSection>();
		private List<string> _sectionNames = new List<string>();

		private static Regex _rxSection = new Regex(@"^\[([^\]]+)\]\s*$");
		private static Regex _rxValue = new Regex(@"^(\w+)\=(.*)$");

		public IniFile()
		{
		}

		public IniFile(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) throw new InvalidOperationException("INI file name is blank.");

			_fileName = fileName;
			Load();
		}

		public void Load()
		{
			_sections.Clear();
			_sectionNames.Clear();
			if (string.IsNullOrEmpty(_fileName)) return;

			using (StreamReader sr = new StreamReader(_fileName))
			{
				IniSection curSection = null;

				while (!sr.EndOfStream)
				{
					string line = sr.ReadLine().Trim();
					if (line.StartsWith("#") || line.StartsWith(";")) continue;

					Match match = _rxSection.Match(line);
					if (match.Success)
					{
						string sectionName = match.Groups[1].Value;
						if (!_sections.TryGetValue(sectionName.ToLower(), out curSection))
						{
							curSection = new IniSection();
							_sections.Add(sectionName.ToLower(), curSection);
							_sectionNames.Add(sectionName);
						}
					}
					else if (curSection != null && (match = _rxValue.Match(line)).Success)
					{
						curSection.values[match.Groups[1].Value.ToLower()] = match.Groups[2].Value;
					}
				}
			}
		}

		public string this[string sectionName, string keyName]
		{
			get
			{
				if (string.IsNullOrEmpty(sectionName)) return string.Empty;

				IniSection section;
				if (!_sections.TryGetValue(sectionName.ToLower(), out section)) return string.Empty;

				string val;
				if (section.values.TryGetValue(keyName.ToLower(), out val)) return val;
				return string.Empty;
			}
		}

		public IEnumerable<string> SectionNames
		{
			get { return _sectionNames; }
		}
	}
}
