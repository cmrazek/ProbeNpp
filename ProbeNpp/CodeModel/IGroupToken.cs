using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProbeNpp.CodeModel.Tokens;

namespace ProbeNpp.CodeModel
{
	interface IGroupToken
	{
		IEnumerable<Token> SubTokens { get; }
	}
}
