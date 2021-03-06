using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace ProbeNpp
{
	internal class ProbeTable
	{
		private int _number = 0;
		private string _name = "";
		private string _prompt = "";
		private Dictionary<string, ProbeField> _fields = null;
		private HashSet<string> _relInds = null;

		public ProbeTable(int number, string name, string prompt)
		{
			_number = number;
			_name = name;
			_prompt = prompt;
		}

		public int Number
		{
			get { return _number; }
		}

		public string Name
		{
			get { return _name; }
		}

		public string Prompt
		{
			get { return _prompt; }
		}

		public bool IsField(string fieldName)
		{
			if (_fields == null) LoadFields();
			return _fields.ContainsKey(fieldName);
		}

		public IEnumerable<ProbeField> Fields
		{
			get
			{
				if (_fields == null) LoadFields();
				return _fields.Values;
			}
		}

		//[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands")]
		//private void LoadFields()
		//{
		//    try
		//    {
		//        _fields = new Dictionary<string, ProbeField>();

		//        var pstFileName = ProbeEnvironment.LocateFileInPath("pst.exe");
		//        if (!string.IsNullOrEmpty(pstFileName))
		//        {
		//            var callbackOutput = new CallbackOutput();
		//            callbackOutput.Callback = (line) =>
		//                {
		//                    // Field Name:   0 -> 39
		//                    // Prompt:      40 -> 79
		//                    // Comment:     80 -> 149
		//                    // DataType:   150 -> end

		//                    if (line.Length < 150) return;

		//                    var name = line.Substring(0, 40).Trim();
		//                    if (string.IsNullOrEmpty(name)) return;

		//                    _fields[name] = new ProbeField(this, name, line.Substring(40, 40).Trim(),
		//                        line.Substring(80, 70).Trim(), line.Substring(150).Trim());
		//                };

		//            var runner = new ProcessRunner();
		//            runner.CaptureProcess(pstFileName, string.Concat("/c ", _name),
		//                Path.GetDirectoryName(pstFileName), callbackOutput);
		//        }
		//    }
		//    catch (Exception)
		//    { }
		//}

		private void LoadFields()
		{
			_fields = new Dictionary<string, ProbeField>();
			var parser = new PstParser();
			parser.Process(_name);
			foreach (var table in parser.Tables)
			{
				if (table.Name == _name)
				{
					foreach (var field in table.Fields) _fields[field.Name] = field;
				}
			}

			_relInds = new HashSet<string>();
			foreach (var relInd in parser.RelInds) _relInds.Add(relInd.Name);
			ProbeEnvironment.RefreshRelInds();
		}

		public void AddField(ProbeField field)
		{
			if (_fields == null) _fields = new Dictionary<string, ProbeField>();
			_fields[field.Name] = field;
		}

		public void SetFieldsLoaded()
		{
			if (_fields == null) _fields = new Dictionary<string, ProbeField>();
		}

		public IEnumerable<string> RelInds
		{
			get
			{
				if (_relInds == null) _relInds = new HashSet<string>();
				return _relInds;
			}
		}
	}
}
