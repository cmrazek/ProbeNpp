using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using NppSharp;

namespace ProbeNpp
{
	internal static class ProbeEnvironment
	{
		#region Events
		public static event EventHandler AppChanged;
		#endregion

		#region Construction
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		public static void Initialize()
		{
			Reload();

			if (!string.IsNullOrEmpty(_probeIniFileName))
			{
				_probeIniWatcher = new FileSystemWatcher();
				_probeIniWatcher.Path = Path.GetDirectoryName(_probeIniFileName);
				_probeIniWatcher.Filter = Path.GetFileName(_probeIniFileName);
				_probeIniWatcher.NotifyFilter = NotifyFilters.LastWrite;
				_probeIniWatcher.Changed += new FileSystemEventHandler(_probeIniWatcher_Changed);
				_probeIniWatcher.EnableRaisingEvents = true;
			}
		}

		internal static void OnSettingsSaved()
		{
			_probeExtensions = null;
		}
		#endregion

		#region PSelect
		private static object _lock = new object();
		private static string _currentApp;
		private static IniFile _nvFile;
		private static string _probeIniFileName;
		private static FileSystemWatcher _probeIniWatcher;
		private static string[] _sourceDirs;
		private static string _objectDir;
		private static string _exeDir;
		private static string _tempDir;
		private static string _reportDir;
		private static string _dataDir;
		private static string _logDir;
		private static string[] _libDirs;
		private static string[] _includeDirs;
		private static string[] _appNames;
		private static int? _samPort;
		
		private static void _probeIniWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			try
			{
				// When we receive this event, the file is still likely not closed and cannot be accessed yet.
				// Hack - wait a small amount of time before checking.
				System.Threading.Thread.Sleep(100);

				OnIniFileChanged();
			}
			catch (Exception ex)
			{
				Plugin.Output.WriteLine(OutputStyle.Error, "Error when checking for probe app change: " + ex);
			}
		}

		public static IEnumerable<string> SourceDirs
		{
			get
			{
				lock (_lock)
				{
					if (_sourceDirs == null)
					{
						var dirs = new List<string>();
						foreach (var dir in _nvFile[_currentApp, "ps"].Split(';'))
						{
							try
							{
								if (!string.IsNullOrWhiteSpace(dir)) dirs.Add(Path.GetFullPath(dir.Trim()));
							}
							catch (Exception ex)
							{
								Log.WriteError(ex);
							}
						}
						_sourceDirs = dirs.ToArray();
					}
					return _sourceDirs;
				}
			}
		}

		public static string ObjectDir
		{
			get
			{
				lock (_lock)
				{
					if (_objectDir == null)
					{
						try
						{
							_objectDir = Path.GetFullPath(_nvFile[_currentApp, "po"].Trim());
						}
						catch (Exception ex)
						{
							Log.WriteError(ex);
							_objectDir = "";
						}
					}
					
					return _objectDir;
				}
			}
		}

		public static string ExeDir
		{
			get
			{
				lock (_lock)
				{
					if (_exeDir == null)
					{
						try
						{
							_exeDir = Path.GetFullPath(_nvFile[_currentApp, "pe"].Trim());
						}
						catch (Exception ex)
						{
							Log.WriteError(ex);
							_exeDir = "";
						}
					}
					return _exeDir;
				}
			}
		}

		public static string TempDir
		{
			get
			{
				lock (_lock)
				{
					if (_tempDir == null)
					{
						try
						{
							_tempDir = Path.GetFullPath(_nvFile[_currentApp, "pt"].Trim());
						}
						catch (Exception ex)
						{
							Log.WriteError(ex);
							_tempDir = "";
						}
					}
					return _tempDir;
				}
			}
		}

		public static string ReportDir
		{
			get
			{
				lock (_lock)
				{
					if (_reportDir == null)
					{
						try
						{
							_reportDir = Path.GetFullPath(_nvFile[_currentApp, "pr"].Trim());
						}
						catch (Exception ex)
						{
							Log.WriteError(ex);
							_reportDir = "";
						}
					}
					return _reportDir;
				}
			}
		}

		public static string DataDir
		{
			get
			{
				lock (_lock)
				{
					if (_dataDir == null)
					{
						try
						{
							_dataDir = Path.GetFullPath(_nvFile[_currentApp, "pd"].Trim());
						}
						catch (Exception ex)
						{
							Log.WriteError(ex);
							_dataDir = "";
						}
					}
					return _dataDir;
				}
			}
		}

		public static string LogDir
		{
			get
			{
				lock (_lock)
				{
					if (_logDir == null)
					{
						try
						{
							_logDir = Path.GetFullPath(_nvFile[_currentApp, "px"].Trim());
						}
						catch (Exception ex)
						{
							Log.WriteError(ex);
							_logDir = "";
						}
					}
					return _logDir;
				}
			}
		}

		public static IEnumerable<string> LibDirs
		{
			get
			{
				lock (_lock)
				{
					if (_libDirs == null)
					{
						var dirs = new List<string>();
						foreach (var dir in _nvFile[_currentApp, "lib"].Split(';'))
						{
							try
							{
								if (!string.IsNullOrWhiteSpace(dir)) dirs.Add(Path.GetFullPath(dir.Trim()));
							}
							catch (Exception ex)
							{
								Log.WriteError(ex);
							}
						}
						_libDirs = dirs.ToArray();
					}
					return _libDirs;
				}
			}
		}

		public static IEnumerable<string> IncludeDirs
		{
			get
			{
				lock (_lock)
				{
					if (_includeDirs == null)
					{
						var dirs = new List<string>();
						foreach (string dir in _nvFile[_currentApp, "include"].Split(';'))
						{
							try
							{
								if (!string.IsNullOrWhiteSpace(dir)) dirs.Add(Path.GetFullPath(dir.Trim()));
							}
							catch (Exception ex)
							{
								Log.WriteError(ex);
							}
						}
						_includeDirs = dirs.ToArray();
					}
					return _includeDirs;
				}
			}
		}

		public static IEnumerable<string> AppNames
		{
			get
			{
				lock (_lock)
				{
					if (_appNames == null) _appNames = _nvFile.SectionNames.ToArray();
					return _appNames;
				}
			}
		}

		public static int SamPort
		{
			get
			{
				lock (_lock)
				{
					if (!_samPort.HasValue)
					{
						int port;
						if (int.TryParse(_nvFile[_currentApp, "dp1"].Trim(), out port)) _samPort = port;
						else _samPort = 0;
					}
					return _samPort.Value;
				}
			}
		}

		public static string CurrentApp
		{
			get
			{
				lock (_lock)
				{
					return string.IsNullOrEmpty(_currentApp) ? string.Empty : _currentApp;
				}
			}
			set
			{
				try
				{
					if (value != _currentApp)
					{
						var exeFileName = LocateFileInPath("ProbeNV.exe");
						if (string.IsNullOrEmpty(exeFileName)) throw new FileNotFoundException("ProbeNV.exe not found.");

						var pr = new ProcessRunner();
						var exitCode = pr.ExecuteProcess(exeFileName, value, Path.GetDirectoryName(exeFileName), true);
						if (exitCode != 0) throw new ProbeException(string.Format("ProbeNV.exe returned exit code {0}.", exitCode));

						Reload();

						EventHandler ev = AppChanged;
						if (ev != null) ev(null, new EventArgs());
					}
				}
				catch (Exception ex)
				{
					Log.WriteError("Error when retrieving current Probe app: {0}", ex);
				}
			}
		}

		public static bool OnIniFileChanged()
		{
			try
			{
				if (!string.IsNullOrEmpty(_probeIniFileName))
				{
					IniFile probeFile = new IniFile(_probeIniFileName);
					string currentApp = probeFile["Options", "CurrentApp"];
					if (currentApp != _currentApp)
					{
						_currentApp = currentApp;
						Reload();

						EventHandler ev = AppChanged;
						if (ev != null) ev(null, new EventArgs());
					}

					return true;
				}
			}
			catch (Exception ex)
			{
				Plugin.Output.WriteLine(OutputStyle.Error, "Exception when checking for app change: " + ex);
			}

			return false;
		}

		public static void Reload()
		{
			lock (_lock)
			{
				_sourceDirs = null;
				_objectDir = null;
				_exeDir = null;
				_tempDir = null;
				_reportDir = null;
				_dataDir = null;
				_logDir = null;
				_libDirs = null;
				_includeDirs = null;
				_appNames = null;
				_samPort = null;

				_probeIniFileName = LocateFileInPath("probe.ini");
				if (string.IsNullOrEmpty(_probeIniFileName))
				{
					Plugin.Output.WriteLine(OutputStyle.Warning, "probe.ini could not be found.");
					_currentApp = null;
				}
				else
				{
					IniFile probeFile = new IniFile(_probeIniFileName);
					_currentApp = probeFile["Options", "CurrentApp"];
				}

				string nvFileName = LocateFileInPath("probenv.ini");
				if (string.IsNullOrEmpty(nvFileName))
				{
					_nvFile = new IniFile();
					Plugin.Output.WriteLine(OutputStyle.Warning, "probenv.ini could not be found.");
				}
				else
				{
					_nvFile = new IniFile(nvFileName);
				}

				ReloadTableList();
			}
		}
		#endregion

		#region Table List
		private static Dictionary<string, ProbeTable> _tables = new Dictionary<string, ProbeTable>();
		private static CodeModel.AutoCompletionItem[] _autoCompletionTables = null;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		private static void ReloadTableList()
		{
			// The function that calls this already has _lock locked.

			try
			{
				_tables.Clear();
				_autoCompletionTables = null;

				ProcessRunner pr = new ProcessRunner();
				CallbackOutput output = new CallbackOutput(GetTableList_Callback);
				int exitCode = pr.CaptureProcess("ptd.exe", "", TempDir, output);
			}
			catch (Exception ex)
			{
				Plugin.Output.WriteLine(OutputStyle.Error, "Exception when reloading Probe table list: {0}", ex);
			}
		}

		private static void GetTableList_Callback(string line)
		{
			try
			{
				if (string.IsNullOrEmpty(line) || string.IsNullOrEmpty(line.Trim())) return;

				if (line.Length >= 48)
				{
					int number;
					if (!Int32.TryParse(line.Substring(0, 4), out number)) return;

					string name = line.Substring(6, 8).Trim();
					if (string.IsNullOrEmpty(name)) return;

					string prompt = line.Substring(17, 31).Trim();
					if (string.IsNullOrEmpty(prompt)) prompt = name;

					_tables[name] = new ProbeTable(number, name, prompt);
				}
			}
			catch (Exception ex)
			{
				Plugin.Output.WriteLine(OutputStyle.Error, "Exception when getting table list: {0}", ex);
			}
		}

		public static IEnumerable<ProbeTable> Tables
		{
			get
			{
				lock (_lock)
				{
					return _tables.Values;
				}
			}
		}

		public static bool IsProbeTable(string tableName)
		{
			lock (_lock)
			{
				return _tables.ContainsKey(tableName);
			}
		}

		public static ProbeTable GetTable(string tableName)
		{
			lock (_lock)
			{
				ProbeTable table;
				return _tables.TryGetValue(tableName, out table) ? table : null;
			}
		}

		public static IEnumerable<CodeModel.AutoCompletionItem> AutoCompletionTables
		{
			get
			{
				if (_autoCompletionTables == null)
				{
					var list = new List<CodeModel.AutoCompletionItem>();
					foreach (var table in _tables.Values)
					{
						var desc = string.Format("Prompt: {0}", table.Prompt.Trim());
						list.Add(new CodeModel.AutoCompletionItem(table.Name, table.Name, desc, CodeModel.AutoCompletionType.Table));
					}
					_autoCompletionTables = list.ToArray();
				}
				return _autoCompletionTables;
			}
		}
		#endregion

		#region RelInds
		private static HashSet<string> _relInds = null;
		private static object _relIndsLock = new object();

		public static void RefreshRelInds()
		{
			lock (_relIndsLock)
			{
				_relInds = null;
			}
		}

		public static bool IsRelInd(string name)
		{
			bool reload;
			lock (_relIndsLock)
			{
				reload = _relInds == null;
			}

			if (reload)
			{
				var relInds = new HashSet<string>();
				foreach (var table in Tables)
				{
					foreach (var relInd in table.RelInds) relInds.Add(relInd);
				}

				lock (_relIndsLock)
				{
					_relInds = relInds;
					return _relInds.Contains(name);
				}
			}
			else
			{
				lock (_relIndsLock)
				{
					return _relInds.Contains(name);
				}
			}
		}
		#endregion

		#region File Paths
		private static string[] _probeExtensions = null;

		public static string LocateFileInPath(string fileName)
		{
			foreach (string path in Environment.GetEnvironmentVariable("path").Split(';'))
			{
				try
				{
					if (Directory.Exists(path.Trim()))
					{
						string fullPath = Path.Combine(path.Trim(), fileName);
						if (File.Exists(fullPath)) return Path.GetFullPath(fullPath);
					}
				}
				catch (Exception)
				{ }
			}
			return "";
		}

		public static string GetRelativePathName(string pathName)
		{
			if (string.IsNullOrEmpty(pathName)) return "";

			string fullFileName = Path.GetFullPath(pathName);

			foreach (string sourceDir in SourceDirs)
			{
				if (fullFileName.Length > sourceDir.Length + 1 &&
					fullFileName.StartsWith(sourceDir, StringComparison.OrdinalIgnoreCase))
				{
					string relPathName = fullFileName.Substring(sourceDir.Length);
					if (relPathName.StartsWith("\\")) relPathName = relPathName.Substring(1);
					return relPathName;
				}
			}

			return "";
		}

		public static string FindBaseFile(string pathName)
		{
			if (string.IsNullOrEmpty(pathName)) return "";

			string relPathName = GetRelativePathName(Path.GetFullPath(pathName));
			if (string.IsNullOrEmpty(relPathName)) return "";
			if (relPathName.EndsWith("&")) relPathName = relPathName.Substring(0, relPathName.Length - 1);

			foreach (string dir in SourceDirs)
			{
				string testPathName = Path.Combine(dir, relPathName);
				if (File.Exists(testPathName)) return testPathName;
			}

			return "";
		}

		public static IEnumerable<string> FindLocalFiles(string pathName, bool includeBaseFile)
		{
			List<string> files = new List<string>();

			if (string.IsNullOrEmpty(pathName)) return files;

			string relPathName = GetRelativePathName(Path.GetFullPath(pathName));
			if (string.IsNullOrEmpty(relPathName)) return files;
			if (relPathName.EndsWith("&")) relPathName = relPathName.Substring(0, relPathName.Length - 1);

			foreach (string dir in SourceDirs)
			{
				string testPathName = Path.Combine(dir, relPathName);
				if (includeBaseFile && File.Exists(testPathName)) files.Add(testPathName);

				testPathName += "&";
				if (File.Exists(testPathName)) files.Add(testPathName);
			}

			return files;
		}

		public static bool FileExistsInApp(string pathName)
		{
			return !string.IsNullOrEmpty(GetRelativePathName(Path.GetFullPath(pathName)));
		}

		public static bool IsProbeFile(string pathName)
		{
			// Populate the probe extension list, if it hasn't been already.
			if (_probeExtensions == null)
			{
				_probeExtensions = (from e in string.Concat(ProbeNppPlugin.Instance.Settings.Probe.SourceExtensions,
									 " ", ProbeNppPlugin.Instance.Settings.Probe.DictExtensions).Split(' ')
									where !string.IsNullOrWhiteSpace(e)
									select (e.StartsWith(".") ? e.ToLower() : string.Concat(".", e.ToLower()))).ToArray();
			}

			// Special exception for dictionary files.
			switch (Path.GetFileName(pathName).ToLower())
			{
				case "dict":
				case "dict&":
					return true;
			}

			// Search the file extension list.
			var fileExt = Path.GetExtension(pathName);
			return _probeExtensions.Contains(fileExt.ToLower());
		}
		#endregion

		#region Probe Language
		public static string StringEscape(string str)
		{
			var sb = new StringBuilder(str.Length);

			foreach (var ch in str)
			{
				switch (ch)
				{
					case '\t':
						sb.Append(@"\t");
						break;
					case '\r':
						sb.Append(@"\r");
						break;
					case '\n':
						sb.Append(@"\n");
						break;
					case '\\':
						sb.Append(@"\\");
						break;
					case '\"':
						sb.Append(@"\""");
						break;
					case '\'':
						sb.Append(@"\'");
						break;
					default:
						sb.Append(ch);
						break;
				}
			}

			return sb.ToString();
		}

		public static bool IsValidFunctionName(string str)
		{
			if (string.IsNullOrEmpty(str)) return false;

			bool firstCh = true;
			foreach (var ch in str)
			{
				if (firstCh)
				{
					if (!Char.IsLetter(ch) && ch != '_') return false;
					firstCh = false;
				}
				else
				{
					if (!Char.IsLetterOrDigit(ch) && ch != '_') return false;
				}
			}

			return true;
		}

		public static bool IsValidFileName(string str)
		{
			if (string.IsNullOrEmpty(str)) return false;

			var badPathChars = Path.GetInvalidPathChars();

			foreach (var ch in str)
			{
				if (badPathChars.Contains(ch) || Char.IsWhiteSpace(ch)) return false;
			}

			return true;
		}
		#endregion
	}
}
