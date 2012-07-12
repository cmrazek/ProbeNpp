using System;
using System.Collections;

namespace ProbeNpp
{
	/// <summary>
	/// Summary description for FunctionCollection.
	/// </summary>
	internal class FunctionCollection : CollectionBase
	{
		public FunctionCollection()
		{
		}

		public Function this[int index]
		{
			get { return (Function)List[index]; }
		}

		public void Add(Function func)
		{
			List.Add(func);
		}
	}
}
