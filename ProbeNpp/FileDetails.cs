using System;
using System.Collections.Generic;
using System.Text;

namespace ProbeNpp
{
	internal class FileDetails
	{
		public uint id = 0;
		public List<Function> functions = null;
		public string lastProbeApp = "";

		public FileDetails(uint id)
		{
			this.id = id;
		}
	}
}
