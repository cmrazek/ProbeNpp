using System;
using System.Collections.Generic;
#if DOTNET4
using System.Linq;
#endif
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
#if DOTNET4
			if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Field name is blank.");
#else
			if (StringUtil.IsNullOrWhiteSpace(name)) throw new ArgumentException("Field name is blank.");
#endif

			Table = table;
			Name = name;
			Prompt = prompt;
			Comment = comment;
			DataType = dataType;
		}
	}
}
