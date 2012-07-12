using System;
using System.Windows.Forms;

namespace ProbeNpp
{
	/// <summary>
	/// Summary description for Function.
	/// </summary>
	internal class Function
	{
		private string _name = "";
		private string _uniqueName = "";
		private int _startLine = 0;
		private int _endLine = 0;
		private ListViewItem _lvi = null;
		private bool _used = false;

		internal Function(string name, string uniqueName, int startLine, int endLine)
		{
			_name = name;
			_uniqueName = uniqueName;
			_startLine = startLine;
			_endLine = endLine;
		}

		public string Name
		{
			get { return _name; }
		}

		public string UniqueName
		{
			get { return _uniqueName; }
		}

		public int StartLine
		{
			get { return _startLine; }
		}

		public int EndLine
		{
			get { return _endLine; }
		}

		internal ListViewItem LVI
		{
			get { return _lvi; }
			set { _lvi = value; }
		}

		internal bool Used
		{
			get { return _used; }
			set { _used = value; }
		}

		public void Update(Function f)
		{
			_name = f._name;
			_startLine = f._startLine;
			_endLine = f._endLine;
		}
	}
}
