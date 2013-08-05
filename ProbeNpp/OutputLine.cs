using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace ProbeNpp
{
	public class OutputLine
	{
		private string _text;
		private Color _color;
		private Brush _brush = null;

		private static Dictionary<Color, Brush> s_brushes = new Dictionary<Color, Brush>();

		public OutputLine(string text, Color color)
		{
			_text = text;
			_color = color;
		}

		public virtual string Text
		{
			get { return _text; }
			protected set { _text = value; }
		}

		public virtual Color Color
		{
			get { return _color; }
			protected set
			{
				if (_color != value)
				{
					_color = value;
					_brush = null;
				}
			}
		}

		public Brush ColorBrush
		{
			get
			{
				if (_brush != null) return _brush;

				Brush brush;
				if (!s_brushes.TryGetValue(_color, out brush)) brush = new SolidBrush(_color);
				_brush = brush;
				return brush;
			}
		}

		public override string ToString()
		{
			return _text;
		}
	}
}
