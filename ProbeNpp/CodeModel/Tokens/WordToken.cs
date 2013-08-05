using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel.Tokens
{
    /// <summary>
    /// This class will be the base for all tokens that consist of a single word.
    /// </summary>
    internal abstract class WordToken : Token
    {
        private string _text;

        public WordToken(Token parent, Scope scope, Span span, string text)
            : base(parent, scope, span)
        {
            if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Text cannot be empty.");

            _text = text;
        }

        public override string Text
        {
            get { return _text; }
        }
    }
}
