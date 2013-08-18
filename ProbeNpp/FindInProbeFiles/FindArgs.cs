using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProbeNpp.FindInProbeFiles
{
	internal class FindArgs
	{
		public string SearchText { get; set; }
		public Regex SearchRegex { get; set; }
		public FindMethod Method { get; set; }
		public bool MatchCase { get; set; }
		public bool MatchWholeWord { get; set; }
		public bool ProbeFilesOnly { get; set; }
		public ResultsPanel Panel { get; set; }
		public string IncludeExtensions { get; set; }
		public string ExcludeExtensions { get; set; }
	}
}
