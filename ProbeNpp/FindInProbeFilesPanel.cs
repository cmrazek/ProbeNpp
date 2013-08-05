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
	internal partial class FindInProbeFilesPanel : UserControl
	{
		private bool _loaded = false;
		private int _numMatches = 0;

		public event EventHandler FindStopped;

		public FindInProbeFilesPanel()
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

				_loaded = true;
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void AddMatch(FindInProbeFilesMatch match)
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action(() => { AddMatch(match); }));
					return;
				}

				var lvi = new ListViewItem(match.FileName);
				lvi.SubItems.Add(match.LineNumber.ToString());
				lvi.SubItems.Add(CleanLineText(match.LineText));
				lvi.Tag = match;

				lstMatches.Items.Add(lvi);

				_numMatches++;
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
				if (item == null || item.Tag == null || item.Tag.GetType() != typeof(FindInProbeFilesMatch)) return;

				var match = (FindInProbeFilesMatch)item.Tag;
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

	}
}
