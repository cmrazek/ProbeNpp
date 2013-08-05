using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProbeNpp.CodeModel.Tokens;

namespace ProbeNpp.CodeModel
{
	interface IBraceMatchingToken
	{
		IEnumerable<Token> BraceMatchingTokens { get; }
	}
}
