using System;
using System.Collections.Generic;
#if DOTNET4
using System.Linq;
#endif
using System.Text;

namespace ProbeNpp.CodeProcessing
{
	internal class CodeFile
	{
		public string FileName { get; private set; }
		public bool Base { get; private set; }

		public CodeFile(string fileName, bool baseFile)
		{
			FileName = fileName;
			Base = baseFile;
		}
	}
}
