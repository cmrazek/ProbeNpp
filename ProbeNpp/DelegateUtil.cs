using System;
using System.Collections.Generic;
using System.Text;

namespace ProbeNpp
{
#if DOTNET4
#else
	public delegate void Action();
#endif
}
