using System;
using System.Collections.Generic;
using System.Text;

namespace ProbeNpp
{
	internal enum CompileRefType
	{
		None,
		Warning,
		Error,
		WarningReport,
		ErrorReport,
		SuccessReport,
		Exception
	}

	internal class CompileRef
	{
		private string _compileOutput = "";
		private string _fileName = "";
		private int _line = -1;
		private CompileRefType _type = CompileRefType.None;

		public CompileRef(string compileOutput, CompileRefType type, string fileName, int line)
		{
			_compileOutput = compileOutput;
			_fileName = fileName;
			_line = line;
			_type = type;
		}

		public CompileRef(string compileOutput, CompileRefType type)
		{
			_compileOutput = compileOutput;
			_type = type;
		}

		public override string ToString()
		{
			return _compileOutput;
		}

		public string FileName
		{
			get { return _fileName; }
		}

		public int Line
		{
			get { return _line; }
		}

		public CompileRefType Type
		{
			get { return _type; }
			set { _type = value; }
		}
	}
}
