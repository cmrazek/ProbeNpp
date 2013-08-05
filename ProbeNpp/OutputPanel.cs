using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProbeNpp
{
	public partial class OutputPanel : UserControl
	{
		public OutputPanel()
		{
			InitializeComponent();

			lstOutput.MeasureItem += _listBox_MeasureItem;
			lstOutput.DrawItem += _listBox_DrawItem;
		}

		#region History Writing
		private void WriteObject(object obj)
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action(() => { WriteObject(obj); }));
					return;
				}

				int selIndex = lstOutput.SelectedIndex;
				bool scrollToBottom = selIndex == -1 || (selIndex == lstOutput.Items.Count - 1);

				int index = lstOutput.Items.Add(obj);
				if (scrollToBottom)
				{
					foreach (var i in lstOutput.SelectedIndices.Cast<int>()) lstOutput.SetSelected(i, false);
					lstOutput.SetSelected(index, true);
					lstOutput.SetSelected(index, false);
				}
			}
			catch (Exception)
			{ }
		}

		public void WriteLine(string text)
		{
			WriteObject(text);
		}

		public void WriteLine(string format, params object[] args)
		{
			WriteObject(string.Format(format, args));
		}

		public void WriteColor(string text, Color color)
		{
			WriteObject(new OutputLine(text, color));
		}

		public void Clear()
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action(() => { Clear(); }));
					return;
				}

				lstOutput.Items.Clear();
			}
			catch (Exception)
			{ }
		}
		#endregion

		#region Owner-Draw List Box
		private void _listBox_MeasureItem(object sender, MeasureItemEventArgs e)
		{
			e.ItemHeight = (int)lstOutput.Font.GetHeight(e.Graphics) + 2;
		}

		private void _listBox_DrawItem(object sender, DrawItemEventArgs e)
		{
			var defaultRequired = true;

			if (e.Index >= 0)
			{
				object obj = lstOutput.Items[e.Index];
				if (obj != null)
				{
					Brush textBrush;
					Brush backBrush;

					if (typeof(OutputLine).IsAssignableFrom(obj.GetType()))
					{
						if ((e.State & DrawItemState.Selected) != 0)
						{
							textBrush = SystemBrushes.Window;
							backBrush = (obj as OutputLine).ColorBrush;
						}
						else
						{
							textBrush = (obj as OutputLine).ColorBrush;
							backBrush = SystemBrushes.Window;
						}
					}
					else
					{
						if ((e.State & DrawItemState.Selected) != 0)
						{
							textBrush = SystemBrushes.HighlightText;
							backBrush = SystemBrushes.Highlight;
						}
						else
						{
							textBrush = SystemBrushes.WindowText;
							backBrush = SystemBrushes.Window;
						}
					}

					e.Graphics.FillRectangle(backBrush, e.Bounds);
					e.Graphics.DrawString(obj.ToString(), lstOutput.Font, textBrush, e.Bounds);
					defaultRequired = false;
				}
			}

			if (defaultRequired)
			{
				e.DrawBackground();
				e.DrawFocusRectangle();
			}
		}
		#endregion
	}
}
