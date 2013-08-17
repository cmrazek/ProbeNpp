using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;

namespace ProbeNpp
{
	public partial class RunForm : Form
	{
#region Types
		public enum RunApp
		{
			SamAndCam,
			Sam,
			Cam
		}
#endregion

#region Variables
		private ProbeNppPlugin _plugin;
		private Control _focusControl = null;
#endregion

		public RunForm(ProbeNppPlugin plugin)
		{
			if (plugin == null) throw new ArgumentNullException("plugin is null.");
			_plugin = plugin;

			InitializeComponent();
		}

		private void RunForm_Load(object sender, EventArgs e)
		{
			try
			{
				switch (_plugin.Settings.RunSamCam.App)
				{
					case RunApp.SamAndCam:
						radSamAndCam.Checked = true;
						_focusControl = radSamAndCam;
						break;
					case RunApp.Sam:
						radSam.Checked = true;
						_focusControl = radSam;
						break;
					case RunApp.Cam:
						radCam.Checked = true;
						_focusControl = radCam;
						break;
				}

				chkDiags.Checked = _plugin.Settings.RunSamCam.Diags;
				chkLoadSam.Checked = _plugin.Settings.RunSamCam.LoadSam;
				chkSetDbDate.Checked = _plugin.Settings.RunSamCam.SetDbDate;
				txtTransReportTimeout.Text = _plugin.Settings.RunSamCam.TransReportTimeout.ToString();
				txtTransAbortTimeout.Text = _plugin.Settings.RunSamCam.TransAbortTimeout.ToString();
				txtMinChannels.Text = _plugin.Settings.RunSamCam.MinChannels.ToString();
				txtMaxChannels.Text = _plugin.Settings.RunSamCam.MaxChannels.ToString();
				txtLoadSamTime.Text = _plugin.Settings.RunSamCam.LoadSamTime.ToString();
				txtCamWidth.Text = _plugin.Settings.RunSamCam.CamWidth.ToString();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void RunForm_Shown(object sender, EventArgs e)
		{
			try
			{
				if (_focusControl != null) _focusControl.Focus();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			try
			{
				int transReportTimeout, transAbortTimeout, minChannels, maxChannels, camWidth, loadSamTime;

				if (!ValidateNumericTextBox(txtTransReportTimeout, "TransReport Timeout", 0, 99, out transReportTimeout)) return;
				if (!ValidateNumericTextBox(txtTransAbortTimeout, "TransAbort Timeout", 0, 99, out transAbortTimeout)) return;
				if (!ValidateNumericTextBox(txtMinChannels, "Minimum Resource Channels", 1, 2, out minChannels)) return;
				if (!ValidateNumericTextBox(txtMaxChannels, "Maximum Resource Channels", 1, 48, out maxChannels)) return;
				if (!ValidateNumericTextBox(txtCamWidth, "CAM Width", 77, 256, out camWidth)) return;
				if (!ValidateNumericTextBox(txtLoadSamTime, "LoadSam Time", 0, 1000000, out loadSamTime)) return;

				RunApp app = RunApp.SamAndCam;
				if (radSamAndCam.Checked) app = RunApp.SamAndCam;
				else if (radSam.Checked) app = RunApp.Sam;
				else if (radCam.Checked) app = RunApp.Cam;

				_plugin.Settings.RunSamCam.App = app;
				_plugin.Settings.RunSamCam.Diags = chkDiags.Checked;
				_plugin.Settings.RunSamCam.SetDbDate = chkSetDbDate.Checked;
				_plugin.Settings.RunSamCam.TransReportTimeout = transReportTimeout;
				_plugin.Settings.RunSamCam.TransAbortTimeout = transAbortTimeout;
				_plugin.Settings.RunSamCam.MinChannels = minChannels;
				_plugin.Settings.RunSamCam.MaxChannels = maxChannels;
				_plugin.Settings.RunSamCam.CamWidth = camWidth;
				_plugin.Settings.RunSamCam.LoadSamTime = loadSamTime;
				_plugin.Settings.RunSamCam.CamWidth = camWidth;

				switch (app)
				{
					case RunApp.SamAndCam:
						if (RunSam()) RunCam();
						break;
					case RunApp.Sam:
						RunSam();
						break;
					case RunApp.Cam:
						RunCam();
						break;
				}

				DialogResult = DialogResult.OK;
				Close();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			try
			{
				DialogResult = DialogResult.Cancel;
				Close();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private bool RunSam()
		{
			if (_plugin.Settings.RunSamCam.SetDbDate)
			{
				ProcessRunner pr = new ProcessRunner();
				int exitCode = pr.ExecuteProcess("setdbdat", "today force", ProbeEnvironment.TempDir, true);
				if (exitCode != 0)
				{
					Errors.Show(this, "\"setdbdat today force\" returned exit code {0}.\n\n" +
						"(The SAM will still start, but the dbdate may be incorrect)", exitCode);
				}
			}

			StringBuilder args = new StringBuilder();
			args.Append(string.Format("/N{0}", CleanSamName(string.Concat(ProbeEnvironment.CurrentApp, "_", System.Environment.UserName))));
			args.Append(string.Format(" /p{0}", ProbeEnvironment.SamPort));
			args.Append(" /o0");
			args.Append(string.Format(" /y{0:00}{1:00}", _plugin.Settings.RunSamCam.TransReportTimeout, _plugin.Settings.RunSamCam.TransAbortTimeout));
			args.Append(string.Format(" /z{0}", _plugin.Settings.RunSamCam.MinChannels));
			args.Append(string.Format(" /Z{0}", _plugin.Settings.RunSamCam.MaxChannels));
			if (_plugin.Settings.RunSamCam.Diags) args.Append(" /d2");

			using (Process proc = new Process())
			{
				ProcessStartInfo info = new ProcessStartInfo("sam", args.ToString());
				info.UseShellExecute = false;
				info.RedirectStandardOutput = false;
				info.RedirectStandardError = false;
				info.CreateNoWindow = false;
				info.WorkingDirectory = ProbeEnvironment.ExeDir;
				proc.StartInfo = info;
				if (!proc.Start())
				{
					Errors.Show(this, "Unable to start the SAM.");
					return false;
				}
			}

			if (_plugin.Settings.RunSamCam.LoadSam && !_plugin.Settings.RunSamCam.Diags) RunLoadSam();

			return true;
		}

		private bool RunCam()
		{
			StringBuilder args = new StringBuilder();
			args.Append(string.Format("/N{0}", CleanSamName(System.Environment.UserName)));
			args.Append(string.Format(" /c{0}", _plugin.Settings.RunSamCam.CamWidth));
			if (_plugin.Settings.RunSamCam.Diags) args.Append(" /d2");

			using (Process proc = new Process())
			{
				ProcessStartInfo info = new ProcessStartInfo("cam", args.ToString());
				info.UseShellExecute = false;
				info.RedirectStandardOutput = false;
				info.RedirectStandardError = false;
				info.CreateNoWindow = false;
				info.WorkingDirectory = ProbeEnvironment.ExeDir;
				proc.StartInfo = info;
				if (!proc.Start())
				{
					Errors.Show(this, "Unable to start the CAM.");
					return false;
				}
			}

			return true;
		}

		private bool RunLoadSam()
		{
			string exeFileName = Path.Combine(Path.Combine(ProbeEnvironment.ExeDir, "uattools"), "loadsam.exe");
			if (!File.Exists(exeFileName)) return false;

			string args = string.Format("/Nloadsam {0}", _plugin.Settings.RunSamCam.LoadSamTime);

			using (Process proc = new Process())
			{
				ProcessStartInfo info = new ProcessStartInfo(exeFileName, args);
				info.UseShellExecute = false;
				info.RedirectStandardOutput = false;
				info.RedirectStandardError = false;
				info.CreateNoWindow = false;
				info.WorkingDirectory = Path.GetDirectoryName(exeFileName);
				proc.StartInfo = info;
				if (!proc.Start()) return false;
			}

			return true;
		}

		private bool ValidateNumericTextBox(TextBox txtBox, string name, int minValue, int maxValue, out int valueOut)
		{
			int value;
			if (!Int32.TryParse(txtBox.Text, out value))
			{
				FocusControl(txtBox);
				Errors.Show(this, "{0} does not contain a valid number.", name);
				valueOut = 0;
				return false;
			}

			if (value < minValue || value > maxValue)
			{
				FocusControl(txtBox);
				Errors.Show(this, "{0} must be between {1} and {2}", name, minValue, maxValue);
				valueOut = 0;
				return false;
			}

			valueOut = value;
			return true;
		}

		private void FocusControl(Control ctrl)
		{
			// Select the page this control is on.
			TabPage tabPage = null;
			Control ctrlIter = ctrl.Parent;
			while (ctrlIter != null)
			{
				Type type = ctrlIter.GetType();
				if (type == typeof(TabPage) || type.IsSubclassOf(typeof(TabPage)))
				{
					tabPage = null;
					break;
				}
				ctrlIter = ctrlIter.Parent;
			}
			if (tabPage != null) tabControl.SelectedTab = tabPage;

			// Select the control.
			ctrl.Focus();
		}

		private string CleanSamName(string name)
		{
			StringBuilder sb = new StringBuilder(name.Length);
			foreach (char ch in name)
			{
				if (Char.IsLetterOrDigit(ch) || ch == '_') sb.Append(ch);
			}
			return sb.ToString();
		}

	}
}