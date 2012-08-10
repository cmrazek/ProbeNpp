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
			var settings = _plugin.Settings;

			// Compile
			chkCloseCompileAfterSuccess.Checked = settings.Compile.ClosePanelAfterSuccess;
			chkCloseCompileAfterWarnings.Checked = settings.Compile.ClosePanelAfterWarnings;

			// Extensions
			txtSourceExtensions.Text = settings.Probe.SourceExtensions;
			txtDictExtensions.Text = settings.Probe.DictExtensions;

			// Tagging
			txtInitials.Text = settings.Tagging.Initials;
			txtWorkOrderNumber.Text = settings.Tagging.WorkOrderNumber;
			txtProblemNumber.Text = settings.Tagging.ProblemNumber;
			chkInitialsInDiags.Checked = settings.Tagging.InitialsInDiags;
			chkFileNameInDiags.Checked = settings.Tagging.FileNameInDiags;
			chkTodoAfterDiags.Checked = settings.Tagging.TodoAfterDiags;
			chkSurroundingTagsOnNewLines.Checked = settings.Tagging.MultiLineTagsOnSeparateLines;
			chkTagDate.Checked = settings.Tagging.TagDate;
		}

		private bool SaveSettings()
		{
			var settings = _plugin.Settings;

			// Compile
			settings.Compile.ClosePanelAfterSuccess = chkCloseCompileAfterSuccess.Checked;
			settings.Compile.ClosePanelAfterWarnings = chkCloseCompileAfterWarnings.Checked;

			// Extensions
			settings.Probe.SourceExtensions = txtSourceExtensions.Text;
			settings.Probe.DictExtensions = txtDictExtensions.Text;

			// Tagging
			settings.Tagging.Initials = txtInitials.Text;
			settings.Tagging.WorkOrderNumber = txtWorkOrderNumber.Text;
			settings.Tagging.ProblemNumber = txtProblemNumber.Text;
			settings.Tagging.InitialsInDiags = chkInitialsInDiags.Checked;
			settings.Tagging.FileNameInDiags = chkFileNameInDiags.Checked;
			settings.Tagging.TodoAfterDiags = chkTodoAfterDiags.Checked;
			settings.Tagging.MultiLineTagsOnSeparateLines = chkSurroundingTagsOnNewLines.Checked;
			settings.Tagging.TagDate = chkTagDate.Checked;

			settings.Save();
			_plugin.OnSettingsSaved();

			return true;
		}

		private bool TestSettingsChanged()
		{
			var settings = _plugin.Settings;

			// Compile
			if (chkCloseCompileAfterSuccess.Checked != settings.Compile.ClosePanelAfterSuccess) return true;
			if (chkCloseCompileAfterWarnings.Checked != settings.Compile.ClosePanelAfterWarnings) return true;

			// Extensions
			if (txtSourceExtensions.Text != settings.Probe.SourceExtensions) return true;
			if (txtDictExtensions.Text != settings.Probe.DictExtensions) return true;

			// Tagging
			if (txtInitials.Text != settings.Tagging.Initials) return true;
			if (txtWorkOrderNumber.Text != settings.Tagging.WorkOrderNumber) return true;
			if (txtProblemNumber.Text != settings.Tagging.ProblemNumber) return true;
			if (chkInitialsInDiags.Checked != settings.Tagging.InitialsInDiags) return true;
			if (chkFileNameInDiags.Checked != settings.Tagging.FileNameInDiags) return true;
			if (chkTodoAfterDiags.Checked != settings.Tagging.TodoAfterDiags) return true;
			if (chkSurroundingTagsOnNewLines.Checked != settings.Tagging.MultiLineTagsOnSeparateLines) return true;
			if (chkTagDate.Checked != settings.Tagging.TagDate) return true;

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