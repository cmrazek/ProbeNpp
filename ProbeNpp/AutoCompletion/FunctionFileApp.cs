using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProbeNpp.AutoCompletion
{
	internal class FunctionFileApp : IDisposable
	{
		private FunctionFileScanner _scanner;
		private string _name;
		private Dictionary<string, string> _functions = new Dictionary<string, string>();
		private Dictionary<string, DateTime> _fileDates = new Dictionary<string, DateTime>();
		private FileSystemWatcherCollection _watchers = new FileSystemWatcherCollection();

		public FunctionFileApp(FunctionFileScanner scanner, string name)
		{
			_scanner = scanner;
			_name = name;
		}

		public FunctionFileApp(FunctionFileScanner scanner, ProbeNpp.FunctionFileDatabase.Application_t dbApp)
		{
			_name = dbApp.name;

			foreach (var func in dbApp.function)
			{
				if (string.IsNullOrWhiteSpace(func.name) || string.IsNullOrWhiteSpace(func.signature)) continue;
				_functions[func.name] = func.signature;
			}

			foreach (var file in dbApp.file)
			{
				if (string.IsNullOrWhiteSpace(file.fileName)) continue;
				_fileDates[file.fileName] = file.modified;
			}
		}

		public FunctionFileDatabase.Application_t Save()
		{
			return new FunctionFileDatabase.Application_t
			{
				name = _name,
				function = (from f in _functions select new FunctionFileDatabase.Function_t { name = f.Key, signature = f.Value }).ToArray(),
				file = (from f in _fileDates select new FunctionFileDatabase.FunctionFile_t { fileName = f.Key, modified = f.Value }).ToArray()
			};
		}

		public void OnDeactivate()
		{
			foreach (var watcher in _watchers) watcher.Dispose();
			_watchers.Clear();
		}

		public void WatchDir(string dir)
		{
			try
			{
				if (_watchers.ContainsPath(dir.ToLower())) return;

				var fsw = new FileSystemWatcher();
				try
				{
					fsw.Path = dir;
					fsw.Filter = "*.f*";
					fsw.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;
					fsw.IncludeSubdirectories = true;

					fsw.Changed += new FileSystemEventHandler(fsw_Changed);
					fsw.Created += new FileSystemEventHandler(fsw_Created);
					fsw.Renamed += new RenamedEventHandler(fsw_Renamed);

					fsw.EnableRaisingEvents = true;

					_watchers.Add(fsw);
				}
				catch
				{
					fsw.Dispose();
					throw;
				}
			}
			catch (Exception ex)
			{
				Log.WriteError(ex, "Error when watching directory for function file changes.");
			}
		}

		private void fsw_Changed(object sender, FileSystemEventArgs e)
		{
			if (FunctionFileScanner.FunctionFilePattern.IsMatch(e.FullPath))
			{
				_scanner.EnqueueFile(e.FullPath);
			}
		}

		private void fsw_Created(object sender, FileSystemEventArgs e)
		{
			if (FunctionFileScanner.FunctionFilePattern.IsMatch(e.FullPath))
			{
				_scanner.EnqueueFile(e.FullPath);
			}
		}

		private void fsw_Renamed(object sender, RenamedEventArgs e)
		{
			if (FunctionFileScanner.FunctionFilePattern.IsMatch(e.FullPath))
			{
				_scanner.EnqueueFile(e.FullPath);
			}
		}

		public void Dispose()
		{
			if (_watchers != null) { _watchers.Dispose(); _watchers = null; }
		}

		public string Name
		{
			get { return _name; }
		}

		public void AddFunction(string name, string signature)
		{
			lock (_functions)
			{
				_functions[name] = signature;
			}
		}

		public void UpdateFile(string fileName, DateTime modified)
		{
			lock (_fileDates)
			{
				_fileDates[fileName.ToLower()] = modified;
			}
		}

		public IEnumerable<CodeModel.FunctionSignature> GetFunctionSignatures(string startsWith)
		{
			lock (_functions)
			{
				return (from f in _functions where f.Key.StartsWith(startsWith) select new CodeModel.FunctionSignature(f.Key, f.Value, "")).ToArray();
			}
		}

		public string GetFunctionSignature(string funcName)
		{
			lock (_functions)
			{
				return (from f in _functions where f.Key == funcName select f.Value).FirstOrDefault();
			}
		}

		public bool TryGetFileDate(string fileName, out DateTime modified)
		{
			lock (_fileDates)
			{
				DateTime mod;
				if (!_fileDates.TryGetValue(fileName.ToLower(), out mod))
				{
					modified = DateTime.MinValue;
					return false;
				}

				modified = mod;
				return true;
			}
		}
	}
}
