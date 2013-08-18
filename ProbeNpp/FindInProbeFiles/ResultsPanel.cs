using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ProbeNpp.FindInProbeFiles
{
	internal partial class ResultsPanel : UserControl
	{
		private bool _loaded = false;
		private int _numMatches = 0;
		private Font _contextFont = null;

		public event EventHandler FindStopped;

		public ResultsPanel()
		{
			InitializeComponent();
		}

		private void FindInProbeFilesPanel_Load(object sender, EventArgs e)
		{
			try
			{
				var settings = ProbeNppPlugin.Instance.Settings.FindInProbeFiles;
				if (settings.FileNameColumnWidth > 0) colFileName.Width = settings.FileNameColumnWidth;
				if (settings.LineNumberColumnWidth > 0) colLineNumber.Width = settings.LineNumberColumnWidth;
				if (settings.LineTextColumnWidth > 0) colContext.Width = settings.LineTextColumnWidth;

				_contextFont = Util.GetMonospaceFont(lstMatches.Font.Size);

				_loaded = true;
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (disposing)
			{
				if (_contextFont != null) { _contextFont.Dispose(); _contextFont = null; }
			}

			base.Dispose(disposing);
		}

		public void AddMatch(FindMatch match)
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action(() => { AddMatch(match); }));
					return;
				}

				var existingLvi = (from l in lstMatches.Items.Cast<ListViewItem>() where (l.Tag as FindMatch).IsSameLine(match) select l).FirstOrDefault();
				if (existingLvi != null)
				{
					(existingLvi.Tag as FindMatch).Merge(match);
				}
				else
				{
					var lvi = new ListViewItem(match.FileName);
					lvi.SubItems.Add(match.LineNumber.ToString());
					lvi.SubItems.Add(CleanLineText(match.LineText));
					lvi.Tag = match;

					lstMatches.Items.Add(lvi);

					_numMatches++;
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void AddInfo(string info)
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action(() => { AddInfo(info); }));
					return;
				}

				var lvi = new ListViewItem(info);
				lstMatches.Items.Add(lvi);
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
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

				lstMatches.Items.Clear();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void lstMatches_ItemActivate(object sender, EventArgs e)
		{
			try
			{
				var item = lstMatches.SelectedItems.Cast<ListViewItem>().FirstOrDefault();
				if (item == null || item.Tag == null || item.Tag.GetType() != typeof(FindMatch)) return;

				var match = (FindMatch)item.Tag;
				ProbeNppPlugin.Instance.OpenFile(match.FileName);
				ProbeNppPlugin.Instance.GoToLine(match.LineNumber);
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void lstMatches_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
		{
			try
			{
				if (_loaded)
				{
					var settings = ProbeNppPlugin.Instance.Settings.FindInProbeFiles;
					settings.FileNameColumnWidth = colFileName.Width;
					settings.LineNumberColumnWidth = colLineNumber.Width;
					settings.LineTextColumnWidth = colContext.Width;
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private string CleanLineText(string str)
		{
			var sb = new StringBuilder();
			foreach (var ch in str)
			{
				switch (ch)
				{
					case '\t':
						sb.Append("    ");
						break;
					case '\r':
					case '\n':
						break;
					default:
						sb.Append(ch);
						break;
				}
			}
			return sb.ToString();
		}

		public void OnSearchStarted()
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action(() => { OnSearchStarted(); }));
					return;
				}

				_numMatches = 0;
				progWorking.Enabled = progWorking.Visible = true;
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void OnSearchEnded()
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action(() => { OnSearchEnded(); }));
					return;
				}

				if (_numMatches == 1) AddInfo("1 match found.");
				else AddInfo(string.Format("{0} matches found.", _numMatches));

				progWorking.Enabled = progWorking.Visible = false;
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void ciStopFind_Click(object sender, EventArgs e)
		{
			try
			{
				var ev = FindStopped;
				if (ev != null) ev(this, new EventArgs());
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void lstMatches_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			e.DrawDefault = true;
		}

		private void lstMatches_DrawItem(object sender, DrawListViewItemEventArgs e)
		{
			//e.DrawBackground();
		}

		private void lstMatches_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			if (string.Equals(lstMatches.Columns[e.ColumnIndex].Tag, "context"))
			{
				DrawContextSubItem(e);
			}
			else
			{
				DrawSubItem(e);
			}
		}

		private void DrawSubItem(DrawListViewSubItemEventArgs e)
		{
			if (e.Item.Selected)
			{
				e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
			}
			else
			{
				e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
			}

			var subItem = e.SubItem;
			var text = subItem.Text;

			if (!string.IsNullOrWhiteSpace(text))
			{
				using (var sf = new StringFormat())
				{
					sf.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces;
					sf.LineAlignment = StringAlignment.Center;
					e.Graphics.DrawString(text, lstMatches.Font, e.Item.Selected ? SystemBrushes.HighlightText : SystemBrushes.WindowText, e.Bounds, sf);
				}
			}
		}

		private void DrawContextSubItem(DrawListViewSubItemEventArgs e)
		{
			var match = e.Item.Tag as FindMatch;
			if (match == null) DrawSubItem(e);

			if (e.Item.Selected)
			{
				e.Graphics.FillRectangle(SystemBrushes.Highlight, e.Bounds);
			}
			else
			{
				e.Graphics.FillRectangle(SystemBrushes.Window, e.Bounds);
			}

			var lineText = match.LineText.Replace('\t', ' ');
			var spans = match.HighlightSpans;
			var pos = 0;
			var length = lineText.Length;
			var bounds = new RectangleF(e.Bounds.Left, e.Bounds.Top, e.Bounds.Width, e.Bounds.Height);
			var normalBrush = e.Item.Selected ? SystemBrushes.HighlightText : SystemBrushes.WindowText;
			var highlightBrush = Brushes.Yellow;
			var highlightTextBrush = SystemBrushes.WindowText;

			using (var sf = new StringFormat())
			{
				sf.FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces;
				sf.LineAlignment = StringAlignment.Center;

				while (pos < length && !bounds.IsEmpty)
				{
					var nextSpan = (from s in spans where s.Start >= pos select s).FirstOrDefault();
					if (nextSpan == null)
					{
						// No more highlight text to draw
						e.Graphics.DrawString(lineText.Substring(pos), _contextFont, normalBrush, bounds, sf);
						pos = length;
					}
					else
					{
						// Draw normal text before the highlight range
						var drawNormal = false;
						string normalText = "";
						RectangleF normalBounds = new RectangleF();

						if (nextSpan.Start > pos)
						{
							drawNormal = true;
							normalText = lineText.Substring(pos, nextSpan.Start - pos);
							normalBounds = bounds;

							var normalSize = Util.MeasureString(e.Graphics, normalText, _contextFont, e.Bounds, sf);
							bounds = new RectangleF(bounds.Left + normalSize.Width, bounds.Top, bounds.Width - normalSize.Width, bounds.Height);
						}

						// Draw highlight range
						var text = lineText.Substring(nextSpan.Start, nextSpan.Length);

						var highlightSize = e.Graphics.MeasureString(text, _contextFont, bounds.Location, sf);	// MeasureString() includes surrounding margins
						var highlightRect = new RectangleF(bounds.Left, bounds.Top + (bounds.Height - highlightSize.Height) / 2, highlightSize.Width, highlightSize.Height);
						highlightRect.Intersect(e.Bounds);
						if (!highlightRect.IsEmpty) e.Graphics.FillRectangle(highlightBrush, highlightRect);

						if (drawNormal)
						{
							e.Graphics.DrawString(normalText, _contextFont, normalBrush, normalBounds, sf);
						}

						e.Graphics.DrawString(text, _contextFont, highlightTextBrush, bounds, sf);
						var size = Util.MeasureString(e.Graphics, text, _contextFont, e.Bounds, sf);
						bounds = new RectangleF(bounds.Left + size.Width, bounds.Top, bounds.Width - size.Width, bounds.Height);

						pos = nextSpan.Start + nextSpan.Length;
					}
				}
			}
		}

	}
}
