using System;
using System.Collections.Generic;
using System.Text;

namespace ProbeNpp
{
	[Serializable]
	public class ProbeException : Exception
	{
		public ProbeException(string message)
			: base(message)
		{
		}
	}
}
