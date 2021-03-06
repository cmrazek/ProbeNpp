﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProbeNpp
{
	internal class LockedValue<T>
	{
		private T _value;

		public LockedValue(T value = default(T))
		{
			_value = value;
		}

		public T Value
		{
			get { lock (this) { return _value; } }
			set { lock (this) { _value = value; } }
		}
	}
}
