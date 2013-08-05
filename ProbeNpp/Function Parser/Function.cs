using System;
using System.Windows.Forms;
using NppSharp;

namespace ProbeNpp
{
	/// <summary>
	/// Summary description for Function.
	/// </summary>
	internal class Function
	{
		private string _name = "";
		private string _id = "";
		private int _startLine = 0;
		private int _endLine = 0;
		private ListViewItem _lvi = null;
		private string _signature = "";

		internal Function(string name, string uniqueName, int startLine, int endLine)
		{
			_name = name;
			_id = uniqueName;
			_startLine = startLine;
			_endLine = endLine;
		}

		public void Update(Function func)
		{
			_name = func._name;
			_id = func._id;
			_startLine = func._startLine;
			_endLine = func._endLine;
			_signature = func._signature;
		}

		public string Name
		{
			get { return _name; }
		}

		public string Id
		{
			get { return _id; }
		}

		public int StartLine
		{
			get { return _startLine; }
			set { _startLine = value; }
		}

		public int EndLine
		{
			get { return _endLine; }
			set { _endLine = value; }
		}

		internal ListViewItem LVI
		{
			get { return _lvi; }
			set { _lvi = value; }
		}

		public string Signature
		{
			get { return _signature; }
			set { _signature = value; }
		}
	}
}
