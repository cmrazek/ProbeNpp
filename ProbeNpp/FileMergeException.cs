using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp
{
	[Serializable]
	internal class FileMergeException : Exception
	{
		public FileMergeException(string message)
			: base(message)
		{ }
	}
}
