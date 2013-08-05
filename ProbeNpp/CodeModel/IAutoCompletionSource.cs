using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp.CodeModel
{
    internal interface IAutoCompletionSource
    {
        IEnumerable<AutoCompletionItem> AutoCompletionItems { get; }
    }

    internal struct AutoCompletionItem
    {
        private string _text;
        private string _insertText;
        private string _desc;
        private AutoCompletionType _type;

        public AutoCompletionItem(string text, string insertText, string description, AutoCompletionType type)
        {
            _text = text;
            _insertText = insertText;
            _desc = description;
            _type = type;
        }

        public string Text
        {
            get { return _text; }
        }

        public string InsertText
        {
            get { return _insertText; }
        }

        public string Description
        {
            get { return _desc; }
        }

        public AutoCompletionType Type
        {
            get { return _type; }
        }
    }

    internal enum AutoCompletionType
    {
        Function,
        Variable,
        Constant,
        DataType,
        Table,
        TableField,
        Keyword
    }
}
