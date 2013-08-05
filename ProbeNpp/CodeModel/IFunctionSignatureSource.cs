using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel
{
    internal interface IFunctionSignatureSource
    {
        IEnumerable<FunctionSignature> FunctionSignatures { get; }
    }

    internal sealed class FunctionSignature
    {
        public string Name { get; private set; }
        public string Signature { get; private set; }
        public string Documentation { get; private set; }

        public FunctionSignature(string name, string signature, string documentation)
        {
            this.Name = name;
            this.Signature = signature;
            this.Documentation = documentation;
        }
    }
}
