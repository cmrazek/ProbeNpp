using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ProbeNpp
{
	public partial class SettingsForm : Form
	{
		private ProbeNppPlugin _plugin;

		#region Construction
		public SettingsForm(ProbeNppPlugin plugin)
		{
			if (plugin == null) throw new ArgumentNullException("plugin is null.");
			_plugin = plugin;

			InitializeComponent();
		}

		private void SettingsForm_Load(object sender, EventArgs e)
		{
			try
			{
				LoadSettings();
				EnableControls(this, null);
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}
		#endregion

		#region Settings
		private void LoadSettings()
		{
			chkCloseCompileAfterSuccess.Checked = _plugin.Settings.Compile.ClosePanelAfterSuccess;
			chkCloseCompileAfterWarnings.Checked = _plugin.Settings.Compile.ClosePanelAfterWarnings;
			txtProbeExtensions.Text = _plugin.Settings.Probe.Extensions;
		}

		private bool SaveSettings()
		{
			_plugin.Settings.Compile.ClosePanelAfterSuccess = chkCloseCompileAfterSuccess.Checked;
			_plugin.Settings.Compile.ClosePanelAfterWarnings = chkCloseCompileAfterWarnings.Checked;
			_plugin.Settings.Probe.Extensions = txtProbeExtensions.Text;

			_plugin.Settings.Save();

			return true;
		}

		private bool TestSettingsChanged()
		{
			if (chkCloseCompileAfterSuccess.Checked != _plugin.Settings.Compile.ClosePanelAfterSuccess) return true;
			if (chkCloseCompileAfterWarnings.Checked != _plugin.Settings.Compile.ClosePanelAfterWarnings) return true;
			if (txtProbeExtensions.Text != _plugin.Settings.Probe.Extensions) return true;

			return false;
		}
		#endregion

		#region UI
		private void EnableControls(object sender, EventArgs e)
		{
			try
			{
				btnApply.Enabled = TestSettingsChanged();
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
				if (SaveSettings())
				{
					DialogResult = DialogResult.OK;
					Close();
				}
				else
				{
					EnableControls(this, null);
				}
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

		private void btnApply_Click(object sender, EventArgs e)
		{
			try
			{
				if (SaveSettings())
				{
					EnableControls(this, null);
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}
		#endregion
	}
}