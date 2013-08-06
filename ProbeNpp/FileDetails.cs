using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NppSharp;

namespace ProbeNpp
{
	internal class FileDetails
	{
		private uint _id = 0;
		private string _lastProbeApp = "";
		private bool _doneInitialUpdate = false;

		public FileDetails(uint id)
		{
			_id = id;
		}

		public void OnActivated()
		{
			if (!_funcsInitialized)
			{
				_funcsInitialized = true;

				var plugin = ProbeNppPlugin.Instance;
				var source = plugin.GetText(plugin.Start, plugin.End);
				ParseFunctions(source);
			}

			var lang = ProbeNppPlugin.Instance.LanguageName;
			if (lang == Res.ProbeSourceLanguageName || lang == Res.ProbeDictLanguageName)
			{
				if (!_doneInitialUpdate)
				{
					_doneInitialUpdate = true;
					ModelIdle();
				}
			}
		}

		public void OnIdle()
		{
			try
			{
				ModelIdle();
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, ex.ToString());
			}
		}

		public string LastProbeApp
		{
			get { return _lastProbeApp; }
			set { _lastProbeApp = value; }
		}

		public uint BufferId
		{
			get { return _id; }
		}

		#region Function Listing
		private Dictionary<string, Function> _funcs = new Dictionary<string, Function>();	// Locked using itself
		private bool _funcsInitialized = false;

		public void ParseFunctions(string source)
		{
			FunctionParser parser = new FunctionParser();
			var funcs = parser.Parse(source);

			lock (_funcs)
			{
				foreach (var func in funcs) _funcs[func.Id] = func;
			}
		}

		public IEnumerable<Function> GetFunctionsWithName(string name)
		{
			lock (_funcs)
			{
				return (from f in _funcs.Values where f.Name == name select f).ToArray();
			}
		}

		public IEnumerable<Function> Functions
		{
			get
			{
				lock (_funcs)
				{
					return _funcs.Values;
				}
			}
		}

		public bool FunctionIdExists(string funcId)
		{
			lock (_funcs)
			{
				return _funcs.ContainsKey(funcId);
			}
		}

		public Function GetFunction(string id)
		{
			lock (_funcs)
			{
				Function func;
				if (_funcs.TryGetValue(id, out func)) return func;
				return null;
			}
		}

		public IEnumerable<string> FunctionIds
		{
			get
			{
				lock (_funcs)
				{
					return _funcs.Keys;
				}
			}
		}

		public bool RemoveFunction(string id)
		{
			lock (_funcs)
			{
				return _funcs.Remove(id);
			}
		}

		public Function GetFunctionForLineNum(int lineNum)
		{
			lock (_funcs)
			{
				return (from f in _funcs.Values where f.StartLine <= lineNum && f.EndLine >= lineNum select f).FirstOrDefault();
			}
		}

		public void AddFunction(Function func)
		{
			lock (_funcs)
			{
				_funcs[func.Id] = func;
			}
		}
		#endregion

		#region Code Model
		private CodeModel.CodeModel _model = null;	// Locked using _modelLock
		private object _modelLock = new object();

		private void CreateModel()
		{
			try
			{
				var app = ProbeNppPlugin.Instance;
				var source = app.GetText(app.Start, app.End);
				var model = new CodeModel.CodeModel(source, app.ActiveFileName);

				lock (_modelLock)
				{
					_model = model;
				}
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, "Exception when generating code model: {0}", ex);
				_model = null;
			}
			
		}

		public void ModelIdle()
		{
			try
			{
				var lang = ProbeNppPlugin.Instance.LanguageName;
				if (lang == Res.ProbeSourceLanguageName || lang == Res.ProbeDictLanguageName)
				{
					ThreadPool.QueueUserWorkItem(obj =>
					{
						try
						{
							if (this == ProbeNppPlugin.Instance.CurrentFile)
							{
								CreateModel();
								ProbeNppPlugin.Instance.RefreshCustomLexers();
							}
						}
						catch (Exception ex)
						{
							ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, "Exception in code model background update thread: {0}", ex);
						}
					});
				}
			}
			catch (Exception ex)
			{
				ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, ex.ToString());
			}
		}

		public CodeModel.CodeModel Model
		{
			get
			{
				lock (_modelLock)
				{
					return _model;
				}
			}
		}
		#endregion
	}
}
