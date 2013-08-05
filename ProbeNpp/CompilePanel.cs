using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using NppSharp;

namespace ProbeNpp
{
	public partial class CompilePanel : UserControl
	{
		#region Constants
		private const int k_compileKillSleep = 10;
		private const int k_compileSleep = 100;
		private const string k_timeStampFormat = "yyyy-MM-dd HH:mm:ss";
		private const int k_compileWaitToHidePanel = 1000;
		private const int k_killCompileTimeout = 1000;
		#endregion

		#region Member Variables
		private ProbeNppPlugin _plugin;
		private Thread _compileThread = null;
		private volatile bool _kill;
		private Process _proc = null;
		private int _numErrors = 0;
		private int _numWarnings = 0;
		#endregion

		#region Construction
		public CompilePanel(ProbeNppPlugin plugin)
		{
			if (plugin == null) throw new ArgumentNullException("plugin");
			_plugin = plugin;

			InitializeComponent();
		}

		public void OnPanelLoad()
		{
		}

		public void OnShutdown()
		{
			try
			{
				KillCompile(0);
			}
			catch (Exception ex)
			{
				_plugin.Output.WriteLine(OutputStyle.Error, ex.ToString());
			}
		}
		#endregion

		#region Compilation
		private void ciCompile_Click(object sender, EventArgs e)
		{
			try
			{
				StartCompile();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void ciKillCompile_Click(object sender, EventArgs e)
		{
			try
			{
				KillCompile(0);
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void StartCompile()
		{
			if (_compileThread != null) KillCompile(k_killCompileTimeout);

			_plugin.SaveFilesInApp();

			Clear();
			_kill = false;

			_compileThread = new Thread(new ThreadStart(CompileThread));
			_compileThread.Name = "CompileThread";
			_compileThread.Start();
		}

		public void KillCompile(int timeout)
		{
			if (_compileThread == null) return;

			if (_compileThread.IsAlive)
			{
				DateTime killStartTime = DateTime.Now;
				while (_compileThread.IsAlive)
				{
					_kill = true;
					if (DateTime.Now.Subtract(killStartTime).TotalMilliseconds >= timeout) break;
					Thread.Sleep(k_compileKillSleep);
				}
			}

			_compileThread = null;
		}

		private void CompileThread()
		{
			try
			{
				DateTime startTime = DateTime.Now;
				WriteLine("Starting compile for application '{0}' at {1}.", _plugin.Environment.CurrentApp, startTime.ToString(k_timeStampFormat));

				_numErrors = _numWarnings = 0;

				_proc = new Process();
				ProcessStartInfo info = new ProcessStartInfo("pc.bat", "/w");
				info.UseShellExecute = false;
				info.RedirectStandardOutput = true;
				info.RedirectStandardError = true;
				info.CreateNoWindow = true;
				info.WorkingDirectory = _plugin.Environment.ObjectDir;
				_proc.StartInfo = info;
				if (!_proc.Start())
				{
					WriteLine("Unable to start Probe compiler.");
					return;
				}

				Thread stdOutThread = new Thread(new ThreadStart(StdOutThread));
				stdOutThread.Name = "StdOut Compile Thread";
				stdOutThread.Start();

				Thread stdErrThread = new Thread(new ThreadStart(StdErrThread));
				stdErrThread.Name = "StdErr Compile Thread";
				stdErrThread.Start();

				while (stdOutThread.IsAlive || stdErrThread.IsAlive)
				{
					if (_kill)
					{
						WriteLine("Compile was stopped before completion.");
						stdOutThread.Abort();
						stdErrThread.Abort();
						return;
					}
					Thread.Sleep(k_compileSleep);
				}

				if (_numErrors > 0 || _numWarnings > 0)
				{
					string str = "";
					if (_numErrors == 1) str = "1 error";
					else if (_numErrors > 1) str = string.Concat(_numErrors, " errors");

					if (_numWarnings > 0 && !string.IsNullOrEmpty(str)) str += " ";
					if (_numWarnings == 1) str += "1 warning";
					else if (_numWarnings > 1) str += string.Concat(_numWarnings, " warnings");

					WriteObject(new CompileRef(str, _numErrors > 0 ? CompileRefType.ErrorReport : CompileRefType.WarningReport));

					if (_numErrors > 0) return;
					else WriteLine("Running dccmp...");
				}
				else
				{
					WriteObject(new CompileRef("Compile succeeded; running dccmp...", CompileRefType.SuccessReport));
				}

				_proc = new Process();
				info = new ProcessStartInfo("dccmp.exe", "/z /D");
				info.UseShellExecute = false;
				info.RedirectStandardOutput = true;
				info.RedirectStandardError = true;
				info.CreateNoWindow = true;
				info.WorkingDirectory = _plugin.Environment.ObjectDir;
				_proc.StartInfo = info;
				if (!_proc.Start())
				{
					WriteLine("Unable to start dccmp.");
					return;
				}

				stdOutThread = new Thread(new ThreadStart(StdOutThread));
				stdOutThread.Name = "StdOut Dccmp Thread";
				stdOutThread.Start();

				stdErrThread = new Thread(new ThreadStart(StdErrThread));
				stdErrThread.Name = "StdErr Dccmp Thread";
				stdErrThread.Start();

				while (stdOutThread.IsAlive || stdErrThread.IsAlive)
				{
					if (_kill)
					{
						WriteLine("Dccmp was stopped before completion.");
						stdOutThread.Abort();
						stdErrThread.Abort();
						return;
					}
					Thread.Sleep(k_compileSleep);
				}

				if (_proc.ExitCode != 0)
				{
					WriteLine("Dccmp failed");
					return;
				}
				else
				{
					WriteObject(new CompileRef("Dccmp succeeded", CompileRefType.SuccessReport));
				}

				DateTime endTime = DateTime.Now;
				TimeSpan elapsed = endTime - startTime;
				WriteLine("Finished at {0} ({1:0}:{2:00} elapsed)", endTime.ToString(k_timeStampFormat), elapsed.TotalMinutes, elapsed.Seconds);

				if (_numErrors == 0 && _numWarnings == 0)
				{
					if (_plugin.Settings.Compile.ClosePanelAfterSuccess) HidePanelWhenOk();
				}
				else if (_numErrors == 0)
				{
					if (_plugin.Settings.Compile.ClosePanelAfterWarnings) HidePanelWhenOk();
				}
			}
			catch (Exception ex)
			{
				WriteObject(new CompileRef(string.Concat("Fatal error in compile thread: ", ex.ToString()),
					CompileRefType.Exception));
			}
		}
		#endregion

		#region Output Watching
		private void StdOutThread()
		{
			try
			{
				StreamReader stream = _proc.StandardOutput;

				while (!_proc.HasExited)
				{
					string line = stream.ReadLine();
					if (line == null)
					{
						Thread.Sleep(k_compileSleep);
					}
					else
					{
						CompileThreadOutput(line, false);
					}
				}

				while (!_proc.StandardOutput.EndOfStream)
				{
					string line = stream.ReadLine();
					if (line == null)
					{
						Thread.Sleep(k_compileSleep);
					}
					else
					{
						CompileThreadOutput(line, false);
					}
				}
			}
			catch (Exception ex)
			{
				_plugin.Output.WriteLine(OutputStyle.Error, "Error in StdOut compile thread: {0}", ex);
			}
		}

		private void StdErrThread()
		{
			try
			{
				StreamReader stream = _proc.StandardError;

				while (!_proc.HasExited)
				{
					string line = stream.ReadLine();
					if (line == null)
					{
						Thread.Sleep(k_compileSleep);
					}
					else
					{
						CompileThreadOutput(line, true);
					}
				}

				while (!_proc.StandardOutput.EndOfStream)
				{
					string line = stream.ReadLine();
					if (line == null)
					{
						Thread.Sleep(k_compileSleep);
					}
					else
					{
						CompileThreadOutput(line, true);
					}
				}
			}
			catch (Exception ex)
			{
				_plugin.Output.WriteLine(OutputStyle.Error, "Error in StdErr compile thread: {0}", ex);
			}
		}

		private Regex _rxError = new Regex(@"^\s*(.+)\s*\((\d+)\)\s*\:\s*error", RegexOptions.IgnoreCase);
		private Regex _rxWarning = new Regex(@"^\s*(.+)\s*\((\d+)\)\s*\:\s*warning", RegexOptions.IgnoreCase);
		private Regex _rxError2 = new Regex(@"^\s*(.+)\s*\((\d+)\)\s*\:");

		private void CompileThreadOutput(string line, bool stdErr)
		{
			Match match;

			if ((match = _rxError.Match(line)).Success)
			{
				WriteObject(new CompileRef(line, CompileRefType.Error,
					match.Groups[1].Value, Convert.ToInt32(match.Groups[2].Value)));
				_numErrors++;
			}
			else if ((match = _rxWarning.Match(line)).Success)
			{
				WriteObject(new CompileRef(line, CompileRefType.Warning,
					match.Groups[1].Value, Convert.ToInt32(match.Groups[2].Value)));
				_numWarnings++;
			}
			else if ((match = _rxError2.Match(line)).Success)
			{
				WriteObject(new CompileRef(line, CompileRefType.Error,
					match.Groups[1].Value, Convert.ToInt32(match.Groups[2].Value)));
				_numErrors++;
			}
			else if (line.StartsWith("LINK : fatal error"))
			{
				WriteObject(new CompileRef(line, CompileRefType.Error));
				_numErrors++;
			}
			else if (line == "PROBE build failed.")
			{
				WriteObject(new CompileRef(line, CompileRefType.ErrorReport));
				if (_numErrors == 0) _numErrors++;
			}
			else
			{
				WriteLine(line);
			}
		}
		#endregion

		public void WriteObject(object obj)
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new Action(() => { WriteObject(obj); }));
					return;
				}

				int selIndex = lstHistory.SelectedIndex;
				bool scrollToBottom = selIndex == -1 || (selIndex == lstHistory.Items.Count - 1);

				int index = lstHistory.Items.Add(obj);
				if (scrollToBottom)
				{
					foreach (var i in lstHistory.SelectedIndices.Cast<int>()) lstHistory.SetSelected(i, false);
					lstHistory.SetSelected(index, true);
					lstHistory.SetSelected(index, false);
				}
			}
			catch (Exception)
			{ }
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

				lstHistory.Items.Clear();
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

		private void lstHistory_DoubleClick(object sender, EventArgs e)
		{
			try
			{
				if (lstHistory.SelectedItems.Count == 1)
				{
					object item = lstHistory.SelectedItems[0];
					if (item != null && item.GetType() == typeof(CompileRef))
					{
						CompileRef cr = (CompileRef)item;
						if (!string.IsNullOrEmpty(cr.FileName))
						{
							if (_plugin.OpenFile(cr.FileName))
							{
								if (cr.Line > 0) _plugin.GoToLine(cr.Line);
							}
						}
						else
						{
							int selIndex = -1;
							switch (cr.Type)
							{
								case CompileRefType.ErrorReport:
									selIndex = FindFirstItemByType(CompileRefType.Error);
									break;
								case CompileRefType.WarningReport:
									selIndex = FindFirstItemByType(CompileRefType.Warning);
									break;
							}

							if (selIndex >= 0)
							{
								foreach (var i in lstHistory.SelectedIndices.Cast<int>()) lstHistory.SetSelected(i, false);
								lstHistory.SelectedIndex = selIndex;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private int FindFirstItemByType(CompileRefType type)
		{
			int index = 0;
			foreach (var item in lstHistory.Items)
			{
				if (item.GetType() == typeof(CompileRef))
				{
					if ((item as CompileRef).Type == type) return index;
				}
				index++;
			}
			return -1;
		}

		#region Owner draw history box
		private Brush _errorBrush = new SolidBrush(Color.Red);
		private Brush _warningBrush = new SolidBrush(Color.OrangeRed);
		private Brush _successBrush = new SolidBrush(Color.Green);

		private void lstHistory_MeasureItem(object sender, MeasureItemEventArgs e)
		{
			e.ItemHeight = (int)lstHistory.Font.GetHeight(e.Graphics) + 2;
		}

		private void lstHistory_DrawItem(object sender, DrawItemEventArgs e)
		{
			var defaultRequired = true;

			if (e.Index >= 0)
			{
				object obj = lstHistory.Items[e.Index];
				if (obj != null)
				{
					Brush textBrush;
					Brush backBrush;

					if (obj.GetType() == typeof(CompileRef))
					{
						var selected = (e.State & DrawItemState.Selected) != 0;

						switch (((CompileRef)obj).Type)
						{
							case CompileRefType.Error:
							case CompileRefType.ErrorReport:
							case CompileRefType.Exception:
								textBrush = selected ? SystemBrushes.Window : _errorBrush;
								backBrush = selected ? _errorBrush : SystemBrushes.Window;
								break;
							case CompileRefType.Warning:
							case CompileRefType.WarningReport:
								textBrush = selected ? SystemBrushes.Window : _warningBrush;
								backBrush = selected ? _warningBrush : SystemBrushes.Window;
								break;
							case CompileRefType.SuccessReport:
								textBrush = selected ? SystemBrushes.Window : _successBrush;
								backBrush = selected ? _successBrush : SystemBrushes.Window;
								break;
							default:
								textBrush = selected ? SystemBrushes.HighlightText : SystemBrushes.WindowText;
								backBrush = selected ? SystemBrushes.Highlight : SystemBrushes.Window;
								break;
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
					e.Graphics.DrawString(obj.ToString(), lstHistory.Font, textBrush, e.Bounds);
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

		private void cmCompile_Opening(object sender, CancelEventArgs e)
		{
			try
			{
				ciHideAfterSuccess.Checked = _plugin.Settings.Compile.ClosePanelAfterSuccess;
				ciHideAfterWarnings.Checked = _plugin.Settings.Compile.ClosePanelAfterWarnings;
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		#region Panel Hiding
		private bool _hidePending = false;
		System.Windows.Forms.Timer _hideTimer = null;

		private void HidePanelWhenOk()
		{
			if (InvokeRequired)
			{
				BeginInvoke(new Action(() => { HidePanelWhenOk(); }));
				return;
			}

			_hidePending = true;

			if (_hideTimer != null) _hideTimer.Stop();

			_hideTimer = new System.Windows.Forms.Timer();
			_hideTimer.Interval = k_compileWaitToHidePanel;
			_hideTimer.Tick += new EventHandler(_hideTimer_Tick);
			_hideTimer.Start();
		}

		private bool OkToHideTimer()
		{
			return !this.Bounds.Contains(this.PointToClient(Cursor.Position));
		}

		private void HidePanelIfOk()
		{
			_plugin.HideCompilePanel();
		}

		void  _hideTimer_Tick(object sender, EventArgs e)
		{
			try
			{
				if (OkToHideTimer())
				{
					_hidePending = false;
					_plugin.HideCompilePanel();
				}
				_hideTimer.Stop();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void lstHistory_MouseLeave(object sender, EventArgs e)
		{
			try
			{
				if (_hidePending) HidePanelWhenOk();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void ciHideAfterSuccess_Click(object sender, EventArgs e)
		{
			try
			{
				_plugin.Settings.Compile.ClosePanelAfterSuccess = !_plugin.Settings.Compile.ClosePanelAfterSuccess;
				_hidePending = false;
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void ciHideAfterWarnings_Click(object sender, EventArgs e)
		{
			try
			{
				_plugin.Settings.Compile.ClosePanelAfterWarnings = !_plugin.Settings.Compile.ClosePanelAfterWarnings;
				_hidePending = false;
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}
		#endregion

		private void lstHistory_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyCode == Keys.C && e.Control && !e.Alt && !e.Shift)
				{
					if (lstHistory.SelectedItems.Count > 0)
					{
						var sb = new StringBuilder();
						foreach (var i in lstHistory.SelectedItems)
						{
							if (sb.Length > 0) sb.AppendLine();
							sb.Append(i.ToString());
						}

						Clipboard.SetData(DataFormats.Text, sb.ToString());
					}

					e.Handled = true;
					e.SuppressKeyPress = true;
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

	}
}