using System;
using System.Collections.Generic;
#if DOTNET4
using System.Linq;
#endif
using System.Text;
using System.Windows.Forms;

namespace ProbeNpp
{
	public static class ComboBoxUtil
	{
#if DOTNET4
		public static void InitForEnum<T>(this ComboBox comboBox, T initialValue) where T : struct, IConvertible
#else
		public static void InitForEnum<T>(ComboBox comboBox, T initialValue) where T : struct, IConvertible
#endif
		{
			comboBox.Items.Clear();
			TagString selectItem = null;

			foreach (T val in Enum.GetValues(typeof(T)))
			{
				var item = new TagString(EnumUtil.GetEnumDesc<T>(val), val);
				if (val.Equals(initialValue)) selectItem = item;
				comboBox.Items.Add(item);
			}

			if (selectItem != null) comboBox.SelectedItem = selectItem;
		}

#if DOTNET4
		public static T GetEnumValue<T>(this ComboBox comboBox) where T : struct, IConvertible
#else
		public static T GetEnumValue<T>(ComboBox comboBox) where T : struct, IConvertible
#endif
		{
			var item = comboBox.SelectedItem;
			if (item == null || item.GetType() != typeof(TagString)) return default(T);

			TagString ts = item as TagString;
			if (ts.Tag == null || ts.Tag.GetType() != typeof(T)) return default(T);

			return (T)ts.Tag;
		}

#if DOTNET4
		public static void SetEnumValue<T>(this ComboBox comboBox, T value) where T : struct, IConvertible
#else
		public static void SetEnumValue<T>(ComboBox comboBox, T value) where T : struct, IConvertible
#endif
		{
			foreach (var item in comboBox.Items)
			{
				if (item != null && item.GetType() == typeof(TagString))
				{
					TagString ts = item as TagString;
					if (ts.Tag != null && ts.Tag.GetType() == typeof(T) && value.Equals(ts.Tag))
					{
						comboBox.SelectedItem = item;
						return;
					}
				}
			}
		}
	}
}
