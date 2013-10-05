using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ProbeNpp.CodeModel.Tokens;

namespace ProbeNpp.CodeModel
{
	internal class CodeModel
	{
		private CodeFile _file;
		private string _fileName;
		private FunctionSignature[] _functionSignatures = null;
		private HashSet<string> _functionNames = null;
		private HashSet<string> _dataTypeNames = null;
		private HashSet<string> _constantNames = null;
		private Tracker _tracker = null;

		public CodeModel(string source, string fileName)
		{
			_fileName = fileName;
			_file = new CodeFile(this);

			if (!string.IsNullOrEmpty(fileName))
			{
				switch (Path.GetFileName(fileName).ToLower())
				{
					case "stdlib.i":
					case "stdlib.i&":
						// Don't include this file if the user happens to have stdlib.i open right now.
						break;
					default:
						AddIncludeFile(_fileName, "stdlib.i", false);
						break;
				}
			}

			_tracker = new Tracker(source);
			_file.Parse(source, fileName);
		}

		#region External Properties
		public DateTime LastAccessTime { get; set; }

		public Tracker Tracker
		{
			get { return _tracker; }
		}
		#endregion

		#region Util functions
		public Position GetPosition(int lineNum, int linePos)
		{
			return _file.FindPosition(lineNum, linePos);
		}

		public Position GetPosition(int offset)
		{
			return _file.FindPosition(offset);
		}

		public Position GetPosition(NppSharp.TextLocation location)
		{
			return GetPosition(location.Line - 1, location.CharPosition - 1);
		}

		public string DumpTree()
		{
			return _file.DumpTree();
		}
		#endregion

		public IEnumerable<FunctionToken> LocalFunctions
		{
			get { return _file.LocalFunctions; }
		}

		public IEnumerable<AutoCompletionItem> GetAutoCompletionItems(Position pos)
		{
			foreach (var item in _file.GetAutoCompletionItems(pos)) yield return item;

			foreach (var incl in _includeFiles)
			{
				foreach (var item in incl.GetGlobalAutoCompletionItems()) yield return item;
			}
		}

		public IEnumerable<AutoCompletionItem> GetGlobalAutoCompletionItems()
		{
			foreach (var item in _file.GetGlobalAutoCompletionItems()) yield return item;

			foreach (var incl in _includeFiles)
			{
				foreach (var item in incl.GetGlobalAutoCompletionItems()) yield return item;
			}
		}

		public IEnumerable<FunctionSignature> FunctionSignatures
		{
			get
			{
				if (_functionSignatures == null) _functionSignatures = _file.GetFunctionSignatures().ToArray();
				foreach (var func in _functionSignatures)
				{
					yield return func;
				}

				foreach (var includeFile in _includeFiles)
				{
					foreach (var func in includeFile.GetFunctionSignatures()) yield return func;
				}
			}
		}

		public IEnumerable<string> FunctionNames
		{
			get
			{
				if (_functionNames == null) _functionNames = new HashSet<string>(from f in FunctionSignatures select f.Name);
				return _functionNames;
			}
		}

		public IEnumerable<string> DataTypeNames
		{
			get
			{
				if (_dataTypeNames == null)
				{
					_dataTypeNames = new HashSet<string>(from i in GetGlobalAutoCompletionItems()
														 where i.Type == AutoCompletionType.DataType
														 select i.Text);
				}
				return _dataTypeNames;
			}
		}

		public IEnumerable<string> ConstantNames
		{
			get
			{
				if (_constantNames == null) _constantNames = new HashSet<string>(from i in GetGlobalAutoCompletionItems()
																				 where i.Type == AutoCompletionType.Constant
																				 select i.Text);
				return _constantNames;
			}
		}

		public IEnumerable<Token> FindTokens(Position pos)
		{
			return _file.FindTokens(pos);
		}

		public string FileName
		{
			get { return _file.FileName; }
		}

		#region Include Files
		private List<CodeFile> _includeFiles = new List<CodeFile>();
		private static Dictionary<string, CodeFile> _cachedIncludeFiles = new Dictionary<string, CodeFile>();

		public void AddIncludeFile(string sourceFileName, string fileName, bool searchCurrentDir)
		{
			var includeFile = GetIncludeFile(sourceFileName, fileName, searchCurrentDir);
			if (includeFile != null && !_includeFiles.Contains(includeFile)) _includeFiles.Add(includeFile);
		}

		public CodeFile GetIncludeFile(string sourceFileName, string fileName, bool searchCurrentDir)
		{
			if (string.IsNullOrEmpty(fileName)) return null;

			CodeFile file = null;
			string key;

			if (searchCurrentDir)
			{
				if (!string.IsNullOrEmpty(sourceFileName))
				{
					key = string.Concat(sourceFileName, ">", fileName).ToLower();
					lock (_cachedIncludeFiles)
					{
						_cachedIncludeFiles.TryGetValue(key, out file);
					}
					if (file != null)
					{
						GetCachedNestedIncludeFiles(file);
						return file;
					}

					var pathName = Path.Combine(Path.GetDirectoryName(sourceFileName), fileName);
					if (File.Exists(pathName)) file = ProcessIncludeFile(pathName);
					else if (File.Exists(pathName + "&")) file = ProcessIncludeFile(pathName + "&");

					if (file != null)
					{
						lock (_cachedIncludeFiles)
						{
							_cachedIncludeFiles[key] = file;
						}
						return file;
					}
				}
			}

			key = fileName.ToLower();
			lock (_cachedIncludeFiles)
			{
				_cachedIncludeFiles.TryGetValue(key, out file);
			}
			if (file != null)
			{
				GetCachedNestedIncludeFiles(file);
				return file;
			}

			foreach (var includeDir in ProbeEnvironment.IncludeDirs)
			{
				var pathName = Path.Combine(includeDir, fileName);
				if (File.Exists(pathName)) file = ProcessIncludeFile(pathName);
				else if (File.Exists(pathName + "&")) file = ProcessIncludeFile(pathName + "&");

				if (file != null) break;
			}

			if (file != null)
			{
				lock (_cachedIncludeFiles)
				{
					_cachedIncludeFiles[key] = file;
				}
			}

			return file;
		}

		private CodeFile ProcessIncludeFile(string fullPathName)
		{
			try
			{
				Trace.WriteLine(string.Concat("Processing include file: ", fullPathName));

				var merger = new FileMerger();
				merger.MergeFile(fullPathName, true);

				var content = merger.MergedContent;
				if (string.IsNullOrEmpty(content)) return null;

				var file = new CodeFile(this);
				file.Parse(content, fullPathName);
				return file;
			}
			catch (Exception ex)
			{
				Trace.WriteLine(string.Format("Exception when merging file '{0}': {1}", fullPathName, ex));
				return null;
			}
		}

		private void GetCachedNestedIncludeFiles(CodeFile file)
		{
			foreach (var incl in file.IncludeFiles)
			{
				AddIncludeFile(incl.SourceFileName, incl.FileName, incl.SearchFileDir);
			}
		}

		public static void OnFileSaved(string fileName)
		{
			// Purge all include files that have this file name, so they get reloaded when the next code model is built.

			var fileTitle = Path.GetFileNameWithoutExtension(fileName);
			var fileExt = Path.GetExtension(fileName);
			if (fileExt.EndsWith("&")) fileExt = fileExt.Substring(0, fileExt.Length - 1);

			var removeList = new List<string>();

			lock (_cachedIncludeFiles)
			{
				foreach (var key in _cachedIncludeFiles.Keys)
				{
					var index = key.IndexOf('>');
					var cachedFileName = index >= 0 ? key.Substring(index + 1) : key;
					if (cachedFileName.EndsWith("&")) cachedFileName = cachedFileName.Substring(0, cachedFileName.Length - 1);

					if (string.Equals(Path.GetFileNameWithoutExtension(cachedFileName), fileTitle, StringComparison.OrdinalIgnoreCase) &&
						string.Equals(Path.GetExtension(cachedFileName), fileExt, StringComparison.OrdinalIgnoreCase))
					{
						removeList.Add(key);
					}
				}

				foreach (var rem in removeList) _cachedIncludeFiles.Remove(rem);
			}
		}
		#endregion
	}
}
