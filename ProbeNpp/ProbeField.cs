using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp
{
	internal class ProbeField
	{
		public ProbeTable Table { get; private set; }
		public string Name { get; private set; }
		public string Prompt { get; private set; }
		public string Comment { get; private set; }
		public string DataType { get; private set; }

		public ProbeField(ProbeTable table, string name, string prompt, string comment, string dataType)
		{
			if (table == null) throw new ArgumentNullException("table");
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Field name is blank.");

			Table = table;
			Name = name;
			Prompt = prompt;
			Comment = comment;
			DataType = dataType;
		}
	}
}
