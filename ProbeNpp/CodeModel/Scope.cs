using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProbeNpp.CodeModel.Tokens;

namespace ProbeNpp.CodeModel
{
    internal struct Scope
    {
        private CodeFile _file;
        private ScopeHint _hint;
        private int _depth;

        public Scope(CodeFile file, int depth, ScopeHint hint)
        {
            _file = file;
            _depth = depth;
            _hint = ScopeHint.None;
        }

        public Scope Indent()
        {
            return new Scope(_file, _depth + 1, _hint);
        }

        public Scope IndentNonRoot()
        {
            var scope = Indent();
            scope.Root = false;
            return scope;
        }

        public bool Root
        {
            get { return !_hint.HasFlag(ScopeHint.NotOnRoot); }
            set
            {
                if (value) _hint &= ~ScopeHint.NotOnRoot;
                else _hint |= ScopeHint.NotOnRoot;
            }
        }

        public int Depth
        {
            get { return _depth; }
        }

        public CodeFile File
        {
            get { return _file; }
        }

        public ScopeHint Hint
        {
            get { return _hint; }
            set { _hint = value; }
        }
    }

    [Flags]
    public enum ScopeHint
    {
        /// <summary>
        /// No special hints.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Inside argument list for a function definition.
        /// </summary>
        FunctionArgs = 0x01,

        /// <summary>
        /// Function calls are not allowed here.
        /// </summary>
        SuppressFunctionCall = 0x02,

        /// <summary>
        /// Variable declarations are not allowed here.
        /// </summary>
        SuppressVarDecl = 0x04,

        /// <summary>
        /// Function definitions are not allowed here.
        /// </summary>
        SuppressFunctionDefinition = 0x08,

        /// <summary>
        /// No longer parsing on the root.
        /// </summary>
        NotOnRoot = 0x10,
    }
}
