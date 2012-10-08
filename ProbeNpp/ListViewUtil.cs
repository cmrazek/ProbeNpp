using System;
using System.Collections.Generic;
#if DOTNET4
using System.Linq;
#endif
using System.Text;
using System.Windows.Forms;

namespace ProbeNpp
{
	internal static class ListViewUtil
	{
#if DOTNET4
		public static void FitColumns(this ListView list)
#else
		public static void FitColumns(ListView list)
#endif
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

#if DOTNET4
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
#else
		public static void SelectSingleItem(ListView list, ListViewItem lvi)
		{
			foreach (ListViewItem nonsel in list.SelectedItems)
			{
				if (nonsel != lvi) nonsel.Selected = false;
			}

			if (lvi != null) lvi.Selected = true;
		}
#endif
	}
}
