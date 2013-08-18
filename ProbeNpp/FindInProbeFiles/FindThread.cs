using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ProbeNpp.FindInProbeFiles
{
	internal class FindThread
	{
		private Thread _thread = null;
		private volatile bool _kill = false;
		private ResultsPanel _panel = null;

		private string _searchText = string.Empty;
		private Regex _searchRegex = null;
		private FindMethod _method = FindMethod.Normal;
		private bool _matchCase = false;
		private bool _matchWholeWord = false;
		private bool _probeFilesOnly = false;
		private FilePatternFilter _includeFilter;
		private FilePatternFilter _excludeFilter;

		private const int k_maxFileSize = 1024 * 1024 * 10;	// 10 MB

		public void Search(FindArgs args)
		{
			_searchText = args.SearchText;
			_searchRegex = args.SearchRegex;
			_method = args.Method;
			_matchCase = args.MatchCase;
			_matchWholeWord = args.MatchWholeWord;
			_probeFilesOnly = args.ProbeFilesOnly;
			_panel = args.Panel;
			_includeFilter = new FilePatternFilter(args.IncludeExtensions);
			_excludeFilter = new FilePatternFilter(args.ExcludeExtensions);

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

				foreach (var dir in ProbeEnvironment.SourceDirs) SearchDir(dir);
				foreach (var dir in ProbeEnvironment.IncludeDirs) SearchDir(dir);

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
					if (_probeFilesOnly && !ProbeEnvironment.IsProbeFile(fileName)) continue;
					if (!_includeFilter.IsMatch(fileName)) continue;
					if (_excludeFilter.IsMatch(fileName)) continue;

					SearchFile(fileName);
				}

				foreach (var subDir in Directory.GetDirectories(dir))
				{
					if (_kill) return;
					SearchDir(subDir);
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

		private void AddMatch(string fileName, string content, int index, int length)
		{
			string lineText;
			int startIndex;
			var lineNumber = GetLineNumber(content, index, out lineText, out startIndex);
			_panel.AddMatch(new FindMatch(fileName, lineNumber, lineText, startIndex, length));
		}

		private int GetLineNumber(string content, int index, out string lineText, out int startIndex)
		{
			var lineNumber = 1;
			var pos = 0;
			var len = content.Length;
			var lineStart = 0;

			if (index > len) index = len;

			while (pos < index)
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

			startIndex = pos - lineStart;

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
					AddMatch(fileName, fileContent, index, _searchText.Length);
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
			foreach (Match match in _searchRegex.Matches(fileContent))
			{
				if (_matchWholeWord == false || CheckMatchWord(fileContent, match.Index, match.Length))
				{
					AddMatch(fileName, fileContent, match.Index, match.Length);
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
