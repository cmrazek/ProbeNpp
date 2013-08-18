using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ProbeNpp.FindInProbeFiles
{
	public enum FindMethod
	{
		Normal,

		[Description("Regular Expression")]
		RegularExpression,

		[Description("Code Friendly")]
		CodeFriendly
	}
}
