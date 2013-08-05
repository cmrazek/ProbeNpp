using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProbeNpp
{
	internal static class ListViewUtil
	{
		public static void FitColumns(this ListView list)
		{
			int clientWidth = list.ClientSize.Width;
			if (clientWidth > 10)
			{
				int colWidth = 0;

				foreach (ColumnHeader col in list.Columns) colWidth += col.Width;

				if (colWidth != clientWidth)
				{
					foreach (ColumnHeader col in list.Columns)
					{
						int newWidth = (int)((float)col.Width / (float)colWidth * (float)clientWidth);
						col.Width = newWidth;
					}
				}
			}

		}

		public static void SelectSingleItem(this ListView list, ListViewItem lvi)
		{
			foreach (var nonsel in (from l in list.SelectedItems.Cast<ListViewItem>()
									where l != lvi
									select l))
			{
				nonsel.Selected = false;
			}

			if (lvi != null) lvi.Selected = true;
		}

		public static IEnumerable<T> Tags<T>(this ListView.ListViewItemCollection items)
		{
			foreach (ListViewItem item in items) yield return (T)item.Tag;
		}
	}
}
