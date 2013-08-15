#define DEBUG_SCANNER
#define DISABLE

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
	internal static class FunctionFileScanner
	{
		private static Thread _thread;
		private static EventWaitHandle _kill = new EventWaitHandle(false, EventResetMode.ManualReset);
		private static LockedValue<bool> _running = new LockedValue<bool>(false);
		private static LockedValue<int> _threadWait = new LockedValue<int>(k_threadWaitIdle);

		private static Queue<string> _dirsToProcess = new Queue<string>();	// Locked using itself
		private static Queue<string> _filesToProcess = new Queue<string>();	// Locked using itself
		private static Dictionary<string, string> _functions = new Dictionary<string, string>();	// Locked using itself
		private static Dictionary<string, DateTime> _fileDates = new Dictionary<string, DateTime>();	// Locked using itself

		private const int k_threadWaitActive = 0;
		private const int k_threadWaitIdle = 100;
		private const int k_threadSleep = 100;

		public static void Initialize()
		{
			_thread = new Thread(new ThreadStart(ThreadProc));
			_thread.Name = "Function File Scanner";
			_thread.Priority = ThreadPriority.BelowNormal;
			_thread.Start();
		}

		public static void Close()
		{
			_kill.Set();
		}

		public static void Start()
		{
			_threadWait.Value = k_threadWaitActive;
			_running.Value = true;
		}

		public static void Stop()
		{
			_threadWait.Value = k_threadWaitIdle;
			_running.Value = false;
		}

		private static void ThreadProc()
		{
			try
			{
				_dirsToProcess.Clear();
				_filesToProcess.Clear();

#if !DISABLE
				foreach (var dir in ProbeNppPlugin.Instance.Environment.SourceDirs) _dirsToProcess.Enqueue(dir);

				string path;

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
							ProcessFunctionFile(path);
						}
						else
						{
							lock (_dirsToProcess)
							{
								if (_dirsToProcess.Count > 0) path = _dirsToProcess.Dequeue();
							}
							if (path != null)
							{
								ProcessSourceDir(path);
							}
							else
							{
								_threadWait.Value = k_threadWaitIdle;
								_running.Value = false;
#if DEBUG_SCANNER
								ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.NotImportant, "Function scan completed.");
#endif
							}
						}
					}
					else
					{
						Thread.Sleep(k_threadSleep);
					}
				}
#endif
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, string.Concat(
					"Exception in function file scanner: ", ex));
			}
		}

		private static Regex _rxFunctionFilePattern = new Regex(@"\\\w+\.[fF]\&?$");

		private static void ProcessSourceDir(string dir)
		{
			try
			{
				foreach (var fileName in Directory.GetFiles(dir, "*.f*"))
				{
					if (_rxFunctionFilePattern.IsMatch(fileName)) _filesToProcess.Enqueue(fileName);
				}

				foreach (var subDir in Directory.GetDirectories(dir))
				{
					_dirsToProcess.Enqueue(subDir);
				}
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, string.Format(
					"Exception when scanning directory '{0}' for functions: {1}", dir, ex));
			}
		}

		private static void ProcessFunctionFile(string fileName)
		{
			try
			{
				DateTime modified;
				lock (_fileDates)
				{
					if (!_fileDates.TryGetValue(fileName.ToLower(), out modified)) modified = DateTime.MinValue;
				}

				if (modified == DateTime.MinValue || Math.Abs(File.GetLastWriteTime(fileName).Subtract(modified).TotalSeconds) >= 1.0)
				{
#if DEBUG_SCANNER
					ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.NotImportant, "Processing function file: {0}", fileName);
#endif

					var merger = new FileMerger();
					merger.MergeFile(fileName, false);

					var fileTitle = Path.GetFileNameWithoutExtension(fileName);

					var model = new CodeModel.CodeModel(merger.MergedContent, fileName);
					foreach (var func in (from f in model.FunctionSignatures
										  where string.Equals(f.Name, fileTitle, StringComparison.OrdinalIgnoreCase)
										  select f))
					{
						lock (_functions)
						{
#if DEBUG_SCANNER
							ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.NotImportant, string.Format("Found function: {0}", func.Name));
#endif
							_functions[func.Name] = func.Signature;
						}
					}

					lock (_fileDates)
					{
						foreach (var fn in merger.FileNames)
						{
							_fileDates[fn.ToLower()] = File.GetLastWriteTime(fn);
						}
					}
				}
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, string.Format(
					"Exception when scanning function file '{0}': {1}", fileName, ex));
			}
		}

		public static IEnumerable<CodeModel.FunctionSignature> GetFunctionSignatures(string startsWith)
		{
			lock (_functions)
			{
				return (from f in _functions where f.Key.StartsWith(startsWith) select new CodeModel.FunctionSignature(f.Key, f.Value, "")).ToArray();
			}
		}

		public static void SaveSettings(XmlWriter xml)
		{
			//foreach (var func in _functions)
			//{
			//    xml.WriteStartElement("Function");
			//    xml.WriteAttributeString("Name", func.Key);
			//    xml.WriteAttributeString("Signature", func.Value);
			//    xml.WriteEndElement();
			//}

			//foreach (var file in _fileDates)
			//{
			//    xml.WriteStartElement("File");
			//    xml.WriteAttributeString("Path", file.Key);
			//    xml.WriteAttributeString("Modified", file.Value.ToString());
			//    xml.WriteEndElement();
			//}
		}

		public static void LoadSettings(XmlElement xml)
		{
			//foreach (var node in xml.SelectNodes("Function"))
			//{
			//    var element = node as XmlElement;
			//    var name = element.GetAttribute("Name");
			//    var sig = element.GetAttribute("Signature");
			//}
		}
	}
}
