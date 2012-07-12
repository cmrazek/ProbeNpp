using System;
using System.Collections.Generic;
using System.Text;

namespace ProbeNpp
{
	public class ProbeTable
	{
		private int _number = 0;
		private string _name = "";
		private string _prompt = "";

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
	}
}
