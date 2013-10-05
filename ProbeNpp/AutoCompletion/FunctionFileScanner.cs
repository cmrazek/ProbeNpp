using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using NppSharp;

namespace ProbeNpp.AutoCompletion
{
	internal class FunctionFileScanner : IDisposable
	{
		private Thread _thread;
		private EventWaitHandle _kill = new EventWaitHandle(false, EventResetMode.ManualReset);
		private LockedValue<bool> _running = new LockedValue<bool>(false);
		private LockedValue<int> _threadWait = new LockedValue<int>(k_threadWaitIdle);

		private FunctionFileApp _currentApp;
		private object _currentAppLock = new object();
		private Queue<ProcessDir> _dirsToProcess = new Queue<ProcessDir>();
		private Queue<string> _filesToProcess = new Queue<string>();
		private FunctionFileAppCollection _apps = new FunctionFileAppCollection();

		private const int k_threadWaitActive = 0;
		private const int k_threadWaitIdle = 100;
		private const int k_threadSleep = 100;

		public static readonly Regex FunctionFilePattern = new Regex(@"\\\w+\.[fF]\&?$");

		private class ProcessDir
		{
			public string dir;
			public bool root;
		}

		public FunctionFileScanner()
		{
			LoadSettings();

			_thread = new Thread(new ThreadStart(ThreadProc));
			_thread.Name = "Function File Scanner";
			_thread.Priority = ThreadPriority.BelowNormal;
			_thread.Start();

			ProbeEnvironment.AppChanged += new EventHandler(ProbeEnvironment_AppChanged);
		}

		public void OnShutdown()
		{
			SaveSettings();
			Kill();
		}

		public void Dispose()
		{
			Kill();

			if (_apps != null) { _apps.Dispose(); _apps = null; }
			if (_kill != null) { _kill.Dispose(); _kill = null; }
		}

		private void ProbeEnvironment_AppChanged(object sender, EventArgs e)
		{
			RestartScanning();
		}

		public void Start()
		{
			_threadWait.Value = k_threadWaitActive;
			_running.Value = true;
		}

		public void Stop()
		{
			_threadWait.Value = k_threadWaitIdle;
			_running.Value = false;
		}

		private void Kill()
		{
			if (_thread != null)
			{
				_kill.Set();
				_thread.Join();
			}
		}

		private void ThreadProc()
		{
			try
			{
				RestartScanning();

				string path;
				ProcessDir processDir;

				while (!_kill.WaitOne(_threadWait.Value))
				{
					if (_running.Value)
					{
						path = null;
						lock (_filesToProcess)
						{
							if (_filesToProcess.Count > 0) path = _filesToProcess.Dequeue();
						}
						if (path != null)
						{
							ProcessFunctionFile(GetCurrentApp(), path);
						}
						else
						{
							processDir = null;
							lock (_dirsToProcess)
							{
								if (_dirsToProcess.Count > 0) processDir = _dirsToProcess.Dequeue();
							}
							if (processDir != null)
							{
								ProcessSourceDir(GetCurrentApp(), processDir.dir, processDir.root);
							}
							else
							{
								_threadWait.Value = k_threadWaitIdle;
								_running.Value = false;
							}
						}
					}
					else
					{
						Thread.Sleep(k_threadSleep);
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteError(ex, "Exception in function file scanner.");
			}
		}

		private void ProcessSourceDir(FunctionFileApp app, string dir, bool root)
		{
			try
			{
				foreach (var fileName in Directory.GetFiles(dir, "*.f*"))
				{
					if (FunctionFilePattern.IsMatch(fileName)) _filesToProcess.Enqueue(fileName);
				}

				foreach (var subDir in Directory.GetDirectories(dir))
				{
					_dirsToProcess.Enqueue(new ProcessDir { dir = subDir, root = false });
				}

				if (root) app.WatchDir(dir);
			}
			catch (Exception ex)
			{
				Log.WriteError(ex, "Exception when scanning directory '{0}' for functions.", dir);
			}
		}

		private void ProcessFunctionFile(FunctionFileApp app, string fileName)
		{
			try
			{
				if (!File.Exists(fileName)) return;

				DateTime modified;
				if (!app.TryGetFileDate(fileName, out modified)) modified = DateTime.MinValue;

				if (modified == DateTime.MinValue || Math.Abs(File.GetLastWriteTime(fileName).Subtract(modified).TotalSeconds) >= 1.0)
				{
					Log.WriteDiag("Processing function file: {0}", fileName);

					var merger = new FileMerger();
					merger.MergeFile(fileName, false);

					var fileTitle = Path.GetFileNameWithoutExtension(fileName);

					var model = new CodeModel.CodeModel(merger.MergedContent, fileName);
					var funcs = (from f in model.FunctionSignatures
								 where string.Equals(f.Name, fileTitle, StringComparison.OrdinalIgnoreCase)
								 select f).ToArray();
					if (funcs.Length > 0)
					{
						foreach (var func in funcs)
						{
							app.AddFunction(func.Name, func.Signature);
						}
					}
					else
					{
						app.RemoveFunctionIgnoreCase(fileTitle);
					}

					foreach (var fn in merger.FileNames)
					{
						app.UpdateFile(fn, File.GetLastWriteTime(fn));
					}
				}
			}
			catch (Exception)
			{
				try
				{
					// Don't show the error because this is just a non-critical background thread.
					if (File.Exists(fileName)) app.UpdateFile(fileName, File.GetLastWriteTime(fileName));
				}
				catch (Exception)
				{ }
			}
		}

		private FunctionFileApp GetCurrentApp()
		{
			var currentApp = ProbeEnvironment.CurrentApp;
			FunctionFileApp app;
			lock (_currentAppLock)
			{
				if (_currentApp == null || _currentApp.Name != currentApp)
				{
					if (_currentApp != null) _currentApp.OnDeactivate();

					if ((app = _apps.TryGet(currentApp)) == null)
					{
						app = new FunctionFileApp(this, currentApp);
						_apps[currentApp] = app;
					}

					_currentApp = app;
				}
				else app = _currentApp;

			}
			return app;
		}

		public IEnumerable<CodeModel.FunctionSignature> GetFunctionSignatures(string startsWith)
		{
			return GetCurrentApp().GetFunctionSignatures(startsWith);
		}

		public string GetFunctionSignature(string funcName)
		{
			return GetCurrentApp().GetFunctionSignature(funcName);
		}

		private void RestartScanning()
		{
			lock (_dirsToProcess)
			{
				_dirsToProcess.Clear();
				foreach (var dir in ProbeEnvironment.SourceDirs) _dirsToProcess.Enqueue(new ProcessDir { dir = dir, root = true });
			}
			lock (_filesToProcess)
			{
				_filesToProcess.Clear();
			}

			_threadWait.Value = k_threadWaitActive;
			_running.Value = true;
		}

		public void SaveSettings()
		{
			try
			{
				var db = new FunctionFileDatabase.Database_t();
				db.application = (from a in _apps.Values select a.Save()).ToArray();

				XmlUtil.SerializeToFile(db, Path.Combine(ProbeNppPlugin.Instance.ConfigDir, Res.FunctionFileDatabaseFileName), true);
			}
			catch (Exception ex)
			{
				Log.WriteError(ex, "Error when saving function file database.");
			}
		}

		public void LoadSettings()
		{
			try
			{
				lock (_apps)
				{
					_apps.Clear();

					var fileName = Path.Combine(ProbeNppPlugin.Instance.ConfigDir, Res.FunctionFileDatabaseFileName);
					if (File.Exists(fileName))
					{
						var db = XmlUtil.DeserializeFromFile<FunctionFileDatabase.Database_t>(fileName);
						if (db != null)
						{
							foreach (var dbApp in db.application)
							{
								_apps.Add(this, dbApp);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Log.WriteError(ex, "Error when loading function file database.");
			}
		}

		public void EnqueueFile(string fullPath)
		{
			lock (_filesToProcess)
			{
				if (!_filesToProcess.Any(x => string.Equals(x, fullPath, StringComparison.OrdinalIgnoreCase)))
				{
					_filesToProcess.Enqueue(fullPath);
				}
			}
		}
	}
}
