using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ProbeNpp
{
	class FileMerger
	{
		private List<string> _lines;
		private string _origFileName = "";
		private List<string> _localFileNames;
		private string _currentLocalFileName = "";
		private int _currentLocalLine = 0;
		private MergeMode _mode = MergeMode.Normal;
		private List<string> _local;
		private List<string> _replace = new List<string>();
		private int _replaceLine;
		private List<LabelPos> _labels;
		private int _insertLine;
		private string _mergedContent = "";
		private bool _showMergeComments;

		enum MergeMode
		{
			Normal,
			ReplaceStart,
			ReplaceWith,
			Insert,
		}

		public FileMerger()
		{
		}

		public void MergeFile(string fileName, bool showMergeComments)
		{
			// locate all needed copies of files
			_origFileName = "";
			_localFileNames = new List<string>();
			_showMergeComments = showMergeComments;

			if (Path.IsPathRooted(fileName)) fileName = UnrootFileName(fileName);
			FindFiles(fileName);

			if (string.IsNullOrEmpty(_origFileName))
			{
				throw new FileMergeException(string.Format("Could not find base file for '{0}'.", fileName));
			}

			// perform localization
			CreateLineDataFromOrigFile();
			foreach (string sLocalFile in _localFileNames) MergeFile(sLocalFile);

			var newContent = new StringBuilder();
			foreach (string line in _lines) newContent.AppendLine(line);
			_mergedContent = newContent.ToString();
		}

		private string UnrootFileName(string fileName)
		{
			fileName = Path.GetFullPath(fileName);

			foreach (var dir in ProbeEnvironment.SourceDirs)
			{
				if (fileName.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
				{
					fileName = fileName.Substring(dir.Length).Trim();
					while (fileName.StartsWith("\\")) fileName = fileName.Substring(1);
					return fileName;
				}
			}

			foreach (var dir in ProbeEnvironment.IncludeDirs)
			{
				if (fileName.StartsWith(dir, StringComparison.OrdinalIgnoreCase))
				{
					fileName = fileName.Substring(dir.Length).Trim();
					while (fileName.StartsWith("\\")) fileName = fileName.Substring(1);
					return fileName;
				}
			}

			throw new FileMergeException(string.Format("File '{0}' does not belong to a Probe source or include directory.", fileName));
		}

		private void FindFiles(string fileName)
		{
			if (string.IsNullOrEmpty(fileName)) return;

			// strip trailing '&' off the end of the filename, if it exists
			if (fileName.EndsWith("&")) fileName = fileName.Substring(0, fileName.Length - 1);
			if (string.IsNullOrEmpty(fileName)) return;

			foreach (string probeDir in ProbeEnvironment.SourceDirs)
			{
				FindFiles_SearchDir(probeDir, fileName);
				if (!string.IsNullOrEmpty(_origFileName)) break;
			}

			if (string.IsNullOrEmpty(_origFileName))
			{
				foreach (string includeDir in ProbeEnvironment.IncludeDirs)
				{
					FindFiles_SearchDir(includeDir, fileName);
					if (!string.IsNullOrEmpty(_origFileName)) break;
				}
			}

		}

		private void FindFiles_SearchDir(string dir, string fileName)
		{
			string pathName = Path.Combine(dir, fileName);
			if (File.Exists(pathName))
			{
				// this is the original file
				_origFileName = pathName;
			}
			else if (File.Exists(pathName + "&"))
			{
				// this is a local file
				var ampFileName = pathName + "&";
				if (!_localFileNames.Any(x => string.Equals(x, ampFileName, StringComparison.OrdinalIgnoreCase))) _localFileNames.Add(ampFileName);
			}

			foreach (string subDir in Directory.GetDirectories(dir))
			{
				FindFiles_SearchDir(subDir, fileName);
			}
		}

		private void CreateLineDataFromOrigFile()
		{
			_lines = new List<string>();

			using (var reader = new StreamReader(_origFileName))
			{
				while (!reader.EndOfStream) _lines.Add(reader.ReadLine());
			}
		}

		private void MergeFile(string localFileName)
		{
			if (_showMergeComments) _lines.Add("// start of local file " + localFileName);

			AnalyzeOrigFile();

			_local = new List<string>();
			_currentLocalFileName = localFileName;
			_currentLocalLine = 1;

			using (var reader = new StreamReader(localFileName))
			{
				while (!reader.EndOfStream)
				{
					ProcessLocalLine(reader.ReadLine());
				}
			}

			if (_showMergeComments) _lines.Add("// end of local file " + localFileName);
		}

		private static Regex _rxLabelLine = new Regex(@"^\s*\#label\b");

		private void AnalyzeOrigFile()
		{
			_labels = new List<LabelPos>();

			var lineNum = 1;
			foreach (var line in _lines)
			{
				var match = _rxLabelLine.Match(line);
				if (match.Success)
				{
					_labels.Add(new LabelPos
					{
						name = line.Substring(match.Index + match.Length).Trim(),
						insertLineNum = lineNum + 1 // insert line after this one
					});
				}

				lineNum++;
			}
		}

		private static Regex _rxReplaceLine = new Regex(@"^\s*\#replace\b");
		private static Regex _rxInsertLine = new Regex(@"^\s*\#insert\b");
		private static Regex _rxWithLine = new Regex(@"^\s*\#with\b");
		private static Regex _rxEndReplaceLine = new Regex(@"^\s*\#endreplace\b");
		private static Regex _rxEndInsertLine = new Regex(@"^\s*\#endinsert\b");

		private void ProcessLocalLine(string line)
		{
			Match match;

			switch (_mode)
			{
				case MergeMode.Normal:
					if ((match = _rxReplaceLine.Match(line)).Success)
					{
						_mode = MergeMode.ReplaceStart;
						_replace.Clear();
					}
					else if ((match = _rxInsertLine.Match(line)).Success)
					{
						var labelName = line.Substring(match.Index + match.Length).Trim();
						_insertLine = GetLabelInsert(labelName);
						if (_insertLine < 0) throw new FileMergeException(this._currentLocalFileName + ": #label '" + labelName + "' not found");

						if (_showMergeComments)
						{
							_lines.Insert(_insertLine, "// insert from " + _currentLocalFileName + "(" + _currentLocalLine.ToString() + ")");
							BumpLabels(_insertLine, 1);
							_insertLine += 1;
						}

						_mode = MergeMode.Insert;
					}
					else
					{
						_lines.Add(line);
					}
					break;

				case MergeMode.ReplaceStart:
					if ((match = _rxWithLine.Match(line)).Success)
					{
						if (_replace.Count == 0) throw new FileMergeException(this._currentLocalFileName + ": empty #replace statement");

						_replaceLine = FindReplace();
						if (_replaceLine < 0) throw new FileMergeException(this._currentLocalFileName + ": #replace at line " + this._currentLocalLine.ToString() + " not found");

						_lines.RemoveRange(_replaceLine, _replace.Count);
						BumpLabels(_replaceLine, -_replace.Count);

						if (_showMergeComments)
						{
							_lines.Insert(_replaceLine, "// replace from " + _currentLocalFileName + "(" + _currentLocalLine.ToString() + ")");
							BumpLabels(_replaceLine, 1);
							_replaceLine += 1;
						}

						_replace.Clear();
						_mode = MergeMode.ReplaceWith;

						var remain = line.Substring(match.Index + match.Length);
						if (!string.IsNullOrWhiteSpace(remain)) _replace.Add(remain);
					}
					else
					{
						_replace.Add(line);
					}

					break;

				case MergeMode.ReplaceWith:
					if ((match = _rxEndReplaceLine.Match(line)).Success)
					{
						if (_showMergeComments)
						{
							_lines.Insert(_replaceLine, "// end of replace");
							BumpLabels(_replaceLine, 1);
							_replaceLine += 1;
						}

						_mode = MergeMode.Normal;
					}
					else
					{
						_lines.Insert(_replaceLine, line);
						BumpLabels(_replaceLine, 1);
						_replaceLine += 1;
					}
					break;

				case MergeMode.Insert:
					if ((match = _rxEndInsertLine.Match(line)).Success)
					{
						if (_showMergeComments)
						{
							_lines.Insert(_insertLine, "// end of insert");
							BumpLabels(_insertLine, 1);
						}

						_mode = MergeMode.Normal;
					}
					else
					{
						_lines.Insert(_insertLine, line);
						BumpLabels(_insertLine, 1);
						_insertLine += 1;
					}
					break;
			}

		}

		private int FindReplace()
		{
			int maxOrigLine = _lines.Count - _replace.Count + 1;
			int lineNum = 0;

			for (lineNum = 0; lineNum <= maxOrigLine - 1; lineNum++)
			{
				// check if this lines up
				bool match = true;
				int origLineNum = lineNum;
				int localLineNum = 0;

				for (localLineNum = 0; localLineNum <= _replace.Count - 1; localLineNum++)
				{
					if (CleanLineForCompare(_lines[origLineNum]) != CleanLineForCompare(_replace[localLineNum]))
					{
						match = false;
						break;
					}
					origLineNum++;
				}

				if (match) return lineNum;
			}

			return -1;
		}

		private string CleanLineForCompare(string line)
		{
			var sb = new StringBuilder(line.Length);
			var gotWhiteSpace = true;

			foreach (var ch in line)
			{
				if (char.IsWhiteSpace(ch))
				{
					if (!gotWhiteSpace) sb.Append(" ");
					gotWhiteSpace = true;
				}
				else
				{
					sb.Append(ch);
					gotWhiteSpace = false;
				}
			}

			return sb.ToString();
		}

		private int GetLabelInsert(string labelName)
		{
			var label = (from l in _labels where string.Equals(l.name, labelName, StringComparison.OrdinalIgnoreCase) select l).FirstOrDefault();
			if (label != null) return label.insertLineNum;
			return -1;
		}


		private void BumpLabels(int startLineNum, int delta)
		{
			foreach (var lbl in _labels)
			{
				if (lbl.insertLineNum >= startLineNum) lbl.insertLineNum += delta;
			}
		}

		private class LabelPos
		{
			public string name;
			public int insertLineNum;
		}

		public string MergedContent
		{
			get { return _mergedContent; }
		}

		public IEnumerable<string> FileNames
		{
			get
			{
				yield return _origFileName;
				foreach (var fn in _localFileNames) yield return fn;
			}
		}
	}
}
