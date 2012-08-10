using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using NppSharp;

#if DOTNET4
using System.Linq;
#endif

namespace ProbeNpp
{
	internal class ProbeEnvironment
	{
		#region Events
		public event EventHandler AppChanged;
		#endregion

		#region Construction
		public ProbeEnvironment()
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

		internal void OnSettingsSaved()
		{
			_probeExtensions = null;
		}
		#endregion

		#region PSelect
		private string _currentApp = "";
		private IniFile _nvFile = null;
		private string _probeIniFileName = "";
		private FileSystemWatcher _probeIniWatcher = null;
		
		void _probeIniWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			try
			{
				// When we receive this event, the file is still likely not closed and cannot be accessed yet.
				// Hack - wait a small amount of time before checking.
				System.Threading.Thread.Sleep(100);

				CheckForAppChanged();
			}
			catch (Exception ex)
			{
				Plugin.Output.WriteLine(OutputStyle.Error, "Error when checking for probe app change: " + ex);
			}
		}

		public IEnumerable<string> SourceDirs
		{
			get
			{
				List<string> dirs = new List<string>();
				foreach (string dir in _nvFile[_currentApp, "ps"].Split(';'))
				{
					string d = dir.Trim();
					if (!string.IsNullOrEmpty(d)) dirs.Add(d);
				}
				return dirs;
			}
		}

		public string ObjectDir
		{
			get { return _nvFile[_currentApp, "po"].Trim(); }
		}

		public string ExeDir
		{
			get { return _nvFile[_currentApp, "pe"].Trim(); }
		}

		public string TempDir
		{
			get { return _nvFile[_currentApp, "pt"].Trim(); }
		}

		public string ReportDir
		{
			get { return _nvFile[_currentApp, "pr"].Trim(); }
		}

		public string DataDir
		{
			get { return _nvFile[_currentApp, "pd"].Trim(); }
		}

		public string LogDir
		{
			get { return _nvFile[_currentApp, "px"].Trim(); }
		}

		public IEnumerable<string> LibDirs
		{
			get
			{
				List<string> dirs = new List<string>();
				foreach (string dir in _nvFile[_currentApp, "lib"].Split(';'))
				{
					string d = dir.Trim();
					if (!string.IsNullOrEmpty(d)) dirs.Add(d);
				}
				return dirs;
			}
		}

		public IEnumerable<string> IncludeDirs
		{
			get
			{
				List<string> dirs = new List<string>();
				foreach (string dir in _nvFile[_currentApp, "include"].Split(';'))
				{
					string d = dir.Trim();
					if (!string.IsNullOrEmpty(d)) dirs.Add(d);
				}
				return dirs;
			}
		}

		public IEnumerable<string> AppNames
		{
			get { return _nvFile.SectionNames; }
		}

		public int SamPort
		{
			get
			{
				int port;
				if (!Int32.TryParse(_nvFile[_currentApp, "dp1"].Trim(), out port)) return 0;
				return port;
			}
		}

		public string CurrentApp
		{
			get { return _currentApp; }
			set
			{
				try
				{
					if (value != _currentApp)
					{
						string exeFileName = LocateFileInPath("ProbeNV.exe");
						if (string.IsNullOrEmpty(exeFileName)) throw new FileNotFoundException("ProbeNV.exe not found.");

						ProcessRunner pr = new ProcessRunner();
						int exitCode = pr.ExecuteProcess(exeFileName, value, Path.GetDirectoryName(exeFileName), true);
						if (exitCode != 0) throw new ProbeException(string.Format("ProbeNV.exe returned exit code {0}.", exitCode));

						ReloadCurrentApp();

						EventHandler ev = AppChanged;
						if (ev != null) ev(this, new EventArgs());
					}
				}
				catch (Exception ex)
				{
					Plugin.Output.WriteLine(OutputStyle.Error, "Error when retrieving current Probe app: {0}", ex);
				}
			}
		}

		public bool CheckForAppChanged()
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
						if (ev != null) ev(this, new EventArgs());
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

		public void ReloadCurrentApp()
		{
			_probeIniFileName = LocateFileInPath("probe.ini");
			if (string.IsNullOrEmpty(_probeIniFileName))
			{
				Plugin.Output.WriteLine(OutputStyle.Warning, "probe.ini could not be found.");
				_currentApp = "";
			}
			else
			{
				IniFile probeFile = new IniFile(_probeIniFileName);
				_currentApp = probeFile["Options", "CurrentApp"];
			}
		}

		public void Reload()
		{
			ReloadCurrentApp();

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
		#endregion

		#region Table List
		private Dictionary<string, ProbeTable> _tables = new Dictionary<string, ProbeTable>();

		private void ReloadTableList()
		{
			try
			{
				_tables.Clear();
				ProcessRunner pr = new ProcessRunner();
				CallbackOutput output = new CallbackOutput(GetTableList_Callback);
				int exitCode = pr.CaptureProcess("ptd.exe", "", TempDir, output);
			}
			catch (Exception ex)
			{
				Plugin.Output.WriteLine(OutputStyle.Error, "Exception when reloading Probe table list: {0}", ex);
			}
		}

		private void GetTableList_Callback(string line)
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

		public IEnumerable<ProbeTable> Tables
		{
			get { return _tables.Values; }
		}

		public bool IsProbeTable(string tableName)
		{
			return _tables.ContainsKey(tableName);
		}

		public ProbeTable GetTable(string tableName)
		{
			ProbeTable table;
			return _tables.TryGetValue(tableName, out table) ? table : null;
		}
		#endregion

		#region File Paths
		private string[] _probeExtensions = null;

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

		public string GetRelativePathName(string pathName)
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

		public string FindBaseFile(string pathName)
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

		public IEnumerable<string> FindLocalFiles(string pathName, bool includeBaseFile)
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

		public bool FileExistsInApp(string pathName)
		{
			return !string.IsNullOrEmpty(GetRelativePathName(Path.GetFullPath(pathName)));
		}

		/// <summary>
		/// Determines if a file has an extension considered to be a probe file.
		/// </summary>
		/// <param name="pathName">The path name of the file to test.</param>
		/// <returns>True if the file extension appears to be a type containing Probe code or table definition;
		/// Otherwise false.</returns>
		public bool IsProbeFile(string pathName)
		{
			// Populate the probe extension list, if it hasn't been already.
			if (_probeExtensions == null)
			{
#if DOTNET4
				_probeExtensions = (from e in string.Concat(ProbeNppPlugin.Instance.Settings.Probe.SourceExtensions,
									 " ", ProbeNppPlugin.Instance.Settings.Probe.DictExtensions).Split(' ')
									where !string.IsNullOrWhiteSpace(e)
									select (e.StartsWith(".") ? e.ToLower() : string.Concat(".", e.ToLower()))).ToArray();
#else
				var extList = new List<string>();
				foreach (var ext in string.Concat(ProbeNppPlugin.Instance.Settings.Probe.SourceExtensions,
									 " ", ProbeNppPlugin.Instance.Settings.Probe.DictExtensions).Split(' '))
				{
					if (!StringUtil.IsNullOrWhiteSpace(ext))
					{
						extList.Add(ext.StartsWith(".") ? ext.ToLower() : string.Concat(".", ext.ToLower()));
					}
				}
				_probeExtensions = extList.ToArray();
#endif
			}

			// Special exception for dictionary files.
			switch (Path.GetFileName(pathName).ToLower())
			{
				case "dict":
				case "dict&":
					return true;
			}

			// Search the file extension list.
			
#if DOTNET4
			var fileExt = Path.GetExtension(pathName);
			return _probeExtensions.Contains(fileExt.ToLower());
#else
			var fileExt = Path.GetExtension(pathName).ToLower();
			foreach (var ext in _probeExtensions)
			{
				if (ext == fileExt) return true;
			}
			return false;
#endif
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
		#endregion
	}
}
