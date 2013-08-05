using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ProbeNpp
{
	public static class EnumUtil
	{
		public static string GetEnumDesc<T>(T value) where T : struct, IConvertible
		{
			DescriptionAttribute descAttrib = (from d in typeof(T).GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>()
											   select d as DescriptionAttribute).FirstOrDefault();

			if (descAttrib != null) return descAttrib.Description;
			return value.ToString();
		}
	}
}
