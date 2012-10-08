using System;
using System.Collections.Generic;
using System.ComponentModel;
#if DOTNET4
using System.Linq;
#endif
using System.Text;

namespace ProbeNpp
{
	public static class EnumUtil
	{
		public static string GetEnumDesc<T>(T value) where T : struct, IConvertible
		{
#if DOTNET4
			DescriptionAttribute descAttrib = (from d in typeof(T).GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>()
											   select d as DescriptionAttribute).FirstOrDefault();

			if (descAttrib != null) return descAttrib.Description;
#else
			foreach (DescriptionAttribute d in typeof(T).GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false))
			{
				return d.Description;
			}
#endif
			return value.ToString();
		}
	}
}
