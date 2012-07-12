using System;
using System.Collections.Generic;
using System.Text;

namespace ProbeNpp
{
	public class TagString
	{
		private string _text = "";
		private object _tag = null;

		public TagString(string text, object tag)
		{
			_text = text;
			_tag = tag;
		}

		public string Text
		{
			get { return _text; }
			set { _text = value; }
		}

		public object Tag
		{
			get { return _tag; }
			set { _tag = value; }
		}

		public override string ToString()
		{
			return _text;
		}
	}
}
