using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ProbeNpp
{
	internal class FindInProbeFilesArgs
	{
		public string SearchText { get; set; }
		public Regex SearchRegex { get; set; }
		public FindInProbeFilesMethod Method { get; set; }
		public bool MatchCase { get; set; }
		public bool MatchWholeWord { get; set; }
		public bool ProbeFilesOnly { get; set; }
		public FindInProbeFilesPanel Panel { get; set; }
		public ProbeEnvironment Probe { get; set; }
	}

	internal class FindInProbeFilesMatch
	{
		public string FileName { get; set; }
		public int LineNumber { get; set; }
		public string LineText { get; set; }
	}

	public enum FindInProbeFilesMethod
	{
		Normal,

		[Description("Regular Expression")]
		RegularExpression,

		[Description("Code Friendly")]
		CodeFriendly
	}

	internal class FindInProbeFilesThread
	{
		private Thread _thread = null;
		private volatile bool _kill = false;
		private FindInProbeFilesPanel _panel = null;
		private ProbeEnvironment _probe = null;

		private string _searchText = string.Empty;
		private Regex _searchRegex = null;
		private FindInProbeFilesMethod _method = FindInProbeFilesMethod.Normal;
		private bool _matchCase = false;
		private bool _matchWholeWord = false;
		private bool _probeFilesOnly = false;

		private const int k_maxFileSize = 1024 * 1024 * 10;	// 10 MB

		public void Search(FindInProbeFilesArgs args)
		{
			_searchText = args.SearchText;
			_searchRegex = args.SearchRegex;
			_method = args.Method;
			_matchCase = args.MatchCase;
			_matchWholeWord = args.MatchWholeWord;
			_probeFilesOnly = args.ProbeFilesOnly;
			_panel = args.Panel;
			_probe = args.Probe;

			_thread = new Thread(new ThreadStart(ThreadProc));
			_thread.Name = "Find in Probe Files";
			_thread.Start();
		}

		public void Kill()
		{
			_kill = true;
		}

		private void ThreadProc()
		{
			try
			{
				_panel.OnSearchStarted();
				_panel.Clear();
				_panel.FindStopped += _panel_FindStopped;

				foreach (var dir in _probe.SourceDirs) SearchDir(dir);
				foreach (var dir in _probe.IncludeDirs) SearchDir(dir);

				_panel.FindStopped -= _panel_FindStopped;
				_panel.OnSearchEnded();
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(NppSharp.OutputStyle.Error, ex.ToString());
				ProbeNppPlugin.Instance.Output.Show();
				_panel.OnSearchEnded();
			}

			
		}

		void _panel_FindStopped(object sender, EventArgs e)
		{
			try
			{
				Kill();
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(NppSharp.OutputStyle.Error,
					string.Format("Error when stopping find:\n{0}", ex.ToString()));
				ProbeNppPlugin.Instance.Output.Show();
			}
		}

		private void SearchDir(string dir)
		{
			try
			{
				foreach (var fileName in Directory.GetFiles(dir))
				{
					if (_kill) return;
					if (!_probeFilesOnly || _probe.IsProbeFile(fileName))
					{
						SearchFile(fileName);
					}
				}
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(NppSharp.OutputStyle.Error,
					string.Format("Error when searching directory '{0}':\n{1}", dir, ex.ToString()));
				ProbeNppPlugin.Instance.Output.Show();
			}
		}

		private void SearchFile(string fileName)
		{
			try
			{
				var fileInfo = new FileInfo(fileName);
				if (fileInfo.Length > k_maxFileSize) return;

				var content = File.ReadAllText(fileName);

				if (_searchRegex != null)
				{
					SearchRegex(fileName, content);
				}
				else
				{
					SearchNormal(fileName, content);
				}
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(NppSharp.OutputStyle.Error,
					string.Format("Error when searching file '{0}':\n{1}", fileName, ex.ToString()));
				ProbeNppPlugin.Instance.Output.Show();
			}
		}

		private void AddMatch(string fileName, string content, int index)
		{
			var lineText = string.Empty;
			var lineNumber = GetLineNumber(content, index, out lineText);
			_panel.AddMatch(new FindInProbeFilesMatch
			{
				FileName = fileName,
				LineNumber = lineNumber,
				LineText = lineText
			});
		}

		private int GetLineNumber(string content, int index, out string lineText)
		{
			var lineNumber = 1;
			var pos = 0;
			var len = content.Length;
			var lineStart = 0;

			while (pos < len && pos < index)
			{
				if (content[pos] == '\n')
				{
					lineNumber++;
					lineStart = pos + 1;
				}
				pos++;
			}

			if (pos >= len)
			{
				lineText = string.Empty;
			}
			else
			{
				var nextLineStart = content.IndexOf('\n', lineStart + 1);
				if (nextLineStart < 0) nextLineStart = len;
				lineText = content.Substring(lineStart, nextLineStart - lineStart);
			}

			return lineNumber;
		}

		private void SearchNormal(string fileName, string fileContent)
		{
			var pos = 0;
			var len = fileContent.Length;
			var index = 0;

			while (pos < len)
			{
				index = fileContent.IndexOf(_searchText, pos, _matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase);
				if (index < 0) return;

				if (_matchWholeWord == false || CheckMatchWord(fileContent, index, _searchText.Length))
				{
					AddMatch(fileName, fileContent, index);
					pos = index + _searchText.Length;
				}
				else
				{
					pos = index + 1;
				}
			}
		}

		private bool IsWordChar(char ch)
		{
			return Char.IsLetterOrDigit(ch) || ch == '_';
		}

		private void SearchRegex(string fileName, string fileContent)
		{
			foreach (var match in _searchRegex.Matches(fileContent).Cast<Match>())
			{
				if (_matchWholeWord == false || CheckMatchWord(fileContent, match.Index, match.Length))
				{
					AddMatch(fileName, fileContent, match.Index);
				}
			}
		}

		private bool CheckMatchWord(string fileContent, int matchIndex, int matchLength)
		{
			if (matchLength == 0) return false;

			if (matchIndex > 0)
			{
				if (IsWordChar(fileContent[matchIndex - 1]) && IsWordChar(fileContent[matchIndex])) return false;
			}

			if (matchIndex + matchLength < fileContent.Length - 1)
			{
				if (IsWordChar(fileContent[matchIndex + matchLength]) && IsWordChar(fileContent[matchIndex + matchLength - 1])) return false;
			}

			return true;
		}

		
	}
}
