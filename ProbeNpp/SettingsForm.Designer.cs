namespace ProbeNpp
{
	partial class SettingsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnApply = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabCompile = new System.Windows.Forms.TabPage();
			this.chkCloseCompileAfterWarnings = new System.Windows.Forms.CheckBox();
			this.chkCloseCompileAfterSuccess = new System.Windows.Forms.CheckBox();
			this.tabExtensions = new System.Windows.Forms.TabPage();
			this.label2 = new System.Windows.Forms.Label();
			this.txtDictExtensions = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.lblProbeExtensions = new System.Windows.Forms.Label();
			this.txtSourceExtensions = new System.Windows.Forms.TextBox();
			this.tabTagging = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.chkTagDate = new System.Windows.Forms.CheckBox();
			this.chkSurroundingTagsOnNewLines = new System.Windows.Forms.CheckBox();
			this.grpDiags = new System.Windows.Forms.GroupBox();
			this.chkFnNameInDiags = new System.Windows.Forms.CheckBox();
			this.chkInitialsInDiags = new System.Windows.Forms.CheckBox();
			this.chkTodoAfterDiags = new System.Windows.Forms.CheckBox();
			this.chkFileNameInDiags = new System.Windows.Forms.CheckBox();
			this.txtProblemNumber = new System.Windows.Forms.TextBox();
			this.lblProblemNumber = new System.Windows.Forms.Label();
			this.txtWorkOrderNumber = new System.Windows.Forms.TextBox();
			this.lblWorkOrderNumber = new System.Windows.Forms.Label();
			this.lblInitials = new System.Windows.Forms.Label();
			this.txtInitials = new System.Windows.Forms.TextBox();
			this.tabMisc = new System.Windows.Forms.TabPage();
			this.chkShowSidebarOnStartup = new System.Windows.Forms.CheckBox();
			this.chkAutoCompletion = new System.Windows.Forms.CheckBox();
			this.panel1.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabCompile.SuspendLayout();
			this.tabExtensions.SuspendLayout();
			this.tabTagging.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.grpDiags.SuspendLayout();
			this.tabMisc.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.btnApply);
			this.panel1.Controls.Add(this.btnCancel);
			this.panel1.Controls.Add(this.btnOk);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 271);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(352, 30);
			this.panel1.TabIndex = 0;
			// 
			// btnApply
			// 
			this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnApply.Location = new System.Drawing.Point(274, 4);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(75, 23);
			this.btnApply.TabIndex = 2;
			this.btnApply.Text = "&Apply";
			this.btnApply.UseVisualStyleBackColor = true;
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(193, 4);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 1;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point(112, 4);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 0;
			this.btnOk.Text = "&OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabTagging);
			this.tabControl.Controls.Add(this.tabCompile);
			this.tabControl.Controls.Add(this.tabExtensions);
			this.tabControl.Controls.Add(this.tabMisc);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(352, 271);
			this.tabControl.TabIndex = 1;
			// 
			// tabCompile
			// 
			this.tabCompile.Controls.Add(this.chkCloseCompileAfterWarnings);
			this.tabCompile.Controls.Add(this.chkCloseCompileAfterSuccess);
			this.tabCompile.Location = new System.Drawing.Point(4, 22);
			this.tabCompile.Name = "tabCompile";
			this.tabCompile.Padding = new System.Windows.Forms.Padding(3);
			this.tabCompile.Size = new System.Drawing.Size(344, 245);
			this.tabCompile.TabIndex = 0;
			this.tabCompile.Text = "Compile";
			this.tabCompile.UseVisualStyleBackColor = true;
			// 
			// chkCloseCompileAfterWarnings
			// 
			this.chkCloseCompileAfterWarnings.AutoSize = true;
			this.chkCloseCompileAfterWarnings.Location = new System.Drawing.Point(8, 29);
			this.chkCloseCompileAfterWarnings.Name = "chkCloseCompileAfterWarnings";
			this.chkCloseCompileAfterWarnings.Size = new System.Drawing.Size(263, 17);
			this.chkCloseCompileAfterWarnings.TabIndex = 1;
			this.chkCloseCompileAfterWarnings.Text = "Hide compile panel after a build with warnings only";
			this.chkCloseCompileAfterWarnings.UseVisualStyleBackColor = true;
			this.chkCloseCompileAfterWarnings.CheckedChanged += new System.EventHandler(this.EnableControls);
			// 
			// chkCloseCompileAfterSuccess
			// 
			this.chkCloseCompileAfterSuccess.AutoSize = true;
			this.chkCloseCompileAfterSuccess.Location = new System.Drawing.Point(8, 6);
			this.chkCloseCompileAfterSuccess.Name = "chkCloseCompileAfterSuccess";
			this.chkCloseCompileAfterSuccess.Size = new System.Drawing.Size(227, 17);
			this.chkCloseCompileAfterSuccess.TabIndex = 0;
			this.chkCloseCompileAfterSuccess.Text = "Hide compile panel after a successful build";
			this.chkCloseCompileAfterSuccess.UseVisualStyleBackColor = true;
			this.chkCloseCompileAfterSuccess.CheckedChanged += new System.EventHandler(this.EnableControls);
			// 
			// tabExtensions
			// 
			this.tabExtensions.Controls.Add(this.label2);
			this.tabExtensions.Controls.Add(this.txtDictExtensions);
			this.tabExtensions.Controls.Add(this.label1);
			this.tabExtensions.Controls.Add(this.lblProbeExtensions);
			this.tabExtensions.Controls.Add(this.txtSourceExtensions);
			this.tabExtensions.Location = new System.Drawing.Point(4, 22);
			this.tabExtensions.Name = "tabExtensions";
			this.tabExtensions.Padding = new System.Windows.Forms.Padding(3);
			this.tabExtensions.Size = new System.Drawing.Size(344, 245);
			this.tabExtensions.TabIndex = 1;
			this.tabExtensions.Text = "Extensions";
			this.tabExtensions.UseVisualStyleBackColor = true;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(8, 143);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(263, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Extensions for files that contain Probe table definitions:";
			// 
			// txtDictExtensions
			// 
			this.txtDictExtensions.AcceptsReturn = true;
			this.txtDictExtensions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtDictExtensions.Location = new System.Drawing.Point(8, 159);
			this.txtDictExtensions.Multiline = true;
			this.txtDictExtensions.Name = "txtDictExtensions";
			this.txtDictExtensions.Size = new System.Drawing.Size(328, 80);
			this.txtDictExtensions.TabIndex = 3;
			this.txtDictExtensions.TextChanged += new System.EventHandler(this.EnableControls);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(8, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(153, 13);
			this.label1.TabIndex = 2;
			this.label1.Text = "(separate with any whitespace)";
			// 
			// lblProbeExtensions
			// 
			this.lblProbeExtensions.AutoSize = true;
			this.lblProbeExtensions.Location = new System.Drawing.Point(8, 3);
			this.lblProbeExtensions.Name = "lblProbeExtensions";
			this.lblProbeExtensions.Size = new System.Drawing.Size(214, 13);
			this.lblProbeExtensions.TabIndex = 1;
			this.lblProbeExtensions.Text = "Extensions for files that contain Probe code:";
			// 
			// txtSourceExtensions
			// 
			this.txtSourceExtensions.AcceptsReturn = true;
			this.txtSourceExtensions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.txtSourceExtensions.Location = new System.Drawing.Point(8, 34);
			this.txtSourceExtensions.Multiline = true;
			this.txtSourceExtensions.Name = "txtSourceExtensions";
			this.txtSourceExtensions.Size = new System.Drawing.Size(328, 106);
			this.txtSourceExtensions.TabIndex = 0;
			this.txtSourceExtensions.TextChanged += new System.EventHandler(this.EnableControls);
			// 
			// tabTagging
			// 
			this.tabTagging.Controls.Add(this.groupBox1);
			this.tabTagging.Controls.Add(this.grpDiags);
			this.tabTagging.Controls.Add(this.txtProblemNumber);
			this.tabTagging.Controls.Add(this.lblProblemNumber);
			this.tabTagging.Controls.Add(this.txtWorkOrderNumber);
			this.tabTagging.Controls.Add(this.lblWorkOrderNumber);
			this.tabTagging.Controls.Add(this.lblInitials);
			this.tabTagging.Controls.Add(this.txtInitials);
			this.tabTagging.Location = new System.Drawing.Point(4, 22);
			this.tabTagging.Name = "tabTagging";
			this.tabTagging.Padding = new System.Windows.Forms.Padding(3);
			this.tabTagging.Size = new System.Drawing.Size(344, 245);
			this.tabTagging.TabIndex = 2;
			this.tabTagging.Text = "Tagging";
			this.tabTagging.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.chkTagDate);
			this.groupBox1.Controls.Add(this.chkSurroundingTagsOnNewLines);
			this.groupBox1.Location = new System.Drawing.Point(11, 87);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(325, 69);
			this.groupBox1.TabIndex = 10;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Tags";
			// 
			// chkTagDate
			// 
			this.chkTagDate.AutoSize = true;
			this.chkTagDate.Location = new System.Drawing.Point(6, 42);
			this.chkTagDate.Name = "chkTagDate";
			this.chkTagDate.Size = new System.Drawing.Size(85, 17);
			this.chkTagDate.TabIndex = 2;
			this.chkTagDate.Text = "Include date";
			this.chkTagDate.UseVisualStyleBackColor = true;
			this.chkTagDate.CheckedChanged += new System.EventHandler(this.EnableControls);
			// 
			// chkSurroundingTagsOnNewLines
			// 
			this.chkSurroundingTagsOnNewLines.AutoSize = true;
			this.chkSurroundingTagsOnNewLines.Location = new System.Drawing.Point(6, 19);
			this.chkSurroundingTagsOnNewLines.Name = "chkSurroundingTagsOnNewLines";
			this.chkSurroundingTagsOnNewLines.Size = new System.Drawing.Size(185, 17);
			this.chkSurroundingTagsOnNewLines.TabIndex = 1;
			this.chkSurroundingTagsOnNewLines.Text = "Put surrounding tags on new lines";
			this.chkSurroundingTagsOnNewLines.UseVisualStyleBackColor = true;
			this.chkSurroundingTagsOnNewLines.CheckedChanged += new System.EventHandler(this.EnableControls);
			// 
			// grpDiags
			// 
			this.grpDiags.Controls.Add(this.chkFnNameInDiags);
			this.grpDiags.Controls.Add(this.chkInitialsInDiags);
			this.grpDiags.Controls.Add(this.chkTodoAfterDiags);
			this.grpDiags.Controls.Add(this.chkFileNameInDiags);
			this.grpDiags.Location = new System.Drawing.Point(11, 162);
			this.grpDiags.Name = "grpDiags";
			this.grpDiags.Size = new System.Drawing.Size(325, 66);
			this.grpDiags.TabIndex = 9;
			this.grpDiags.TabStop = false;
			this.grpDiags.Text = "Diags";
			// 
			// chkFnNameInDiags
			// 
			this.chkFnNameInDiags.AutoSize = true;
			this.chkFnNameInDiags.Location = new System.Drawing.Point(169, 42);
			this.chkFnNameInDiags.Name = "chkFnNameInDiags";
			this.chkFnNameInDiags.Size = new System.Drawing.Size(136, 17);
			this.chkFnNameInDiags.TabIndex = 3;
			this.chkFnNameInDiags.Text = "Include Function Name";
			this.chkFnNameInDiags.UseVisualStyleBackColor = true;
			this.chkFnNameInDiags.CheckedChanged += new System.EventHandler(this.EnableControls);
			// 
			// chkInitialsInDiags
			// 
			this.chkInitialsInDiags.AutoSize = true;
			this.chkInitialsInDiags.Location = new System.Drawing.Point(6, 19);
			this.chkInitialsInDiags.Name = "chkInitialsInDiags";
			this.chkInitialsInDiags.Size = new System.Drawing.Size(92, 17);
			this.chkInitialsInDiags.TabIndex = 0;
			this.chkInitialsInDiags.Text = "Include initials";
			this.chkInitialsInDiags.UseVisualStyleBackColor = true;
			this.chkInitialsInDiags.CheckedChanged += new System.EventHandler(this.EnableControls);
			// 
			// chkTodoAfterDiags
			// 
			this.chkTodoAfterDiags.AutoSize = true;
			this.chkTodoAfterDiags.Location = new System.Drawing.Point(6, 42);
			this.chkTodoAfterDiags.Name = "chkTodoAfterDiags";
			this.chkTodoAfterDiags.Size = new System.Drawing.Size(125, 17);
			this.chkTodoAfterDiags.TabIndex = 2;
			this.chkTodoAfterDiags.Text = "Add TODO comment";
			this.chkTodoAfterDiags.UseVisualStyleBackColor = true;
			this.chkTodoAfterDiags.CheckedChanged += new System.EventHandler(this.EnableControls);
			// 
			// chkFileNameInDiags
			// 
			this.chkFileNameInDiags.AutoSize = true;
			this.chkFileNameInDiags.Location = new System.Drawing.Point(169, 19);
			this.chkFileNameInDiags.Name = "chkFileNameInDiags";
			this.chkFileNameInDiags.Size = new System.Drawing.Size(106, 17);
			this.chkFileNameInDiags.TabIndex = 1;
			this.chkFileNameInDiags.Text = "Include file name";
			this.chkFileNameInDiags.UseVisualStyleBackColor = true;
			this.chkFileNameInDiags.CheckedChanged += new System.EventHandler(this.EnableControls);
			// 
			// txtProblemNumber
			// 
			this.txtProblemNumber.Location = new System.Drawing.Point(119, 61);
			this.txtProblemNumber.Name = "txtProblemNumber";
			this.txtProblemNumber.Size = new System.Drawing.Size(80, 20);
			this.txtProblemNumber.TabIndex = 2;
			this.txtProblemNumber.TextChanged += new System.EventHandler(this.EnableControls);
			// 
			// lblProblemNumber
			// 
			this.lblProblemNumber.AutoSize = true;
			this.lblProblemNumber.Location = new System.Drawing.Point(8, 64);
			this.lblProblemNumber.Name = "lblProblemNumber";
			this.lblProblemNumber.Size = new System.Drawing.Size(88, 13);
			this.lblProblemNumber.TabIndex = 4;
			this.lblProblemNumber.Text = "Problem Number:";
			// 
			// txtWorkOrderNumber
			// 
			this.txtWorkOrderNumber.Location = new System.Drawing.Point(119, 35);
			this.txtWorkOrderNumber.Name = "txtWorkOrderNumber";
			this.txtWorkOrderNumber.Size = new System.Drawing.Size(80, 20);
			this.txtWorkOrderNumber.TabIndex = 1;
			this.txtWorkOrderNumber.TextChanged += new System.EventHandler(this.EnableControls);
			// 
			// lblWorkOrderNumber
			// 
			this.lblWorkOrderNumber.AutoSize = true;
			this.lblWorkOrderNumber.Location = new System.Drawing.Point(8, 38);
			this.lblWorkOrderNumber.Name = "lblWorkOrderNumber";
			this.lblWorkOrderNumber.Size = new System.Drawing.Size(105, 13);
			this.lblWorkOrderNumber.TabIndex = 2;
			this.lblWorkOrderNumber.Text = "Work Order Number:";
			// 
			// lblInitials
			// 
			this.lblInitials.AutoSize = true;
			this.lblInitials.Location = new System.Drawing.Point(8, 12);
			this.lblInitials.Name = "lblInitials";
			this.lblInitials.Size = new System.Drawing.Size(39, 13);
			this.lblInitials.TabIndex = 1;
			this.lblInitials.Text = "Initials:";
			// 
			// txtInitials
			// 
			this.txtInitials.Location = new System.Drawing.Point(119, 9);
			this.txtInitials.Name = "txtInitials";
			this.txtInitials.Size = new System.Drawing.Size(80, 20);
			this.txtInitials.TabIndex = 0;
			this.txtInitials.TextChanged += new System.EventHandler(this.EnableControls);
			// 
			// tabMisc
			// 
			this.tabMisc.Controls.Add(this.chkAutoCompletion);
			this.tabMisc.Controls.Add(this.chkShowSidebarOnStartup);
			this.tabMisc.Location = new System.Drawing.Point(4, 22);
			this.tabMisc.Name = "tabMisc";
			this.tabMisc.Padding = new System.Windows.Forms.Padding(3);
			this.tabMisc.Size = new System.Drawing.Size(344, 245);
			this.tabMisc.TabIndex = 3;
			this.tabMisc.Text = "Misc";
			this.tabMisc.UseVisualStyleBackColor = true;
			// 
			// chkShowSidebarOnStartup
			// 
			this.chkShowSidebarOnStartup.AutoSize = true;
			this.chkShowSidebarOnStartup.Location = new System.Drawing.Point(8, 6);
			this.chkShowSidebarOnStartup.Name = "chkShowSidebarOnStartup";
			this.chkShowSidebarOnStartup.Size = new System.Drawing.Size(144, 17);
			this.chkShowSidebarOnStartup.TabIndex = 0;
			this.chkShowSidebarOnStartup.Text = "Show Sidebar on Startup";
			this.chkShowSidebarOnStartup.UseVisualStyleBackColor = true;
			// 
			// chkAutoCompletion
			// 
			this.chkAutoCompletion.AutoSize = true;
			this.chkAutoCompletion.Location = new System.Drawing.Point(8, 29);
			this.chkAutoCompletion.Name = "chkAutoCompletion";
			this.chkAutoCompletion.Size = new System.Drawing.Size(102, 17);
			this.chkAutoCompletion.TabIndex = 1;
			this.chkAutoCompletion.Text = "Auto-completion";
			this.chkAutoCompletion.UseVisualStyleBackColor = true;
			this.chkAutoCompletion.CheckedChanged += new System.EventHandler(this.EnableControls);
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(352, 301);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.panel1);
			this.MinimumSize = new System.Drawing.Size(360, 200);
			this.Name = "SettingsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Probe Plugin Settings";
			this.Load += new System.EventHandler(this.SettingsForm_Load);
			this.panel1.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.tabCompile.ResumeLayout(false);
			this.tabCompile.PerformLayout();
			this.tabExtensions.ResumeLayout(false);
			this.tabExtensions.PerformLayout();
			this.tabTagging.ResumeLayout(false);
			this.tabTagging.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.grpDiags.ResumeLayout(false);
			this.grpDiags.PerformLayout();
			this.tabMisc.ResumeLayout(false);
			this.tabMisc.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabCompile;
		private System.Windows.Forms.CheckBox chkCloseCompileAfterWarnings;
		private System.Windows.Forms.CheckBox chkCloseCompileAfterSuccess;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.TabPage tabExtensions;
		private System.Windows.Forms.TextBox txtSourceExtensions;
		private System.Windows.Forms.Label lblProbeExtensions;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtDictExtensions;
		private System.Windows.Forms.TabPage tabTagging;
		private System.Windows.Forms.CheckBox chkInitialsInDiags;
		private System.Windows.Forms.TextBox txtProblemNumber;
		private System.Windows.Forms.Label lblProblemNumber;
		private System.Windows.Forms.TextBox txtWorkOrderNumber;
		private System.Windows.Forms.Label lblWorkOrderNumber;
		private System.Windows.Forms.Label lblInitials;
		private System.Windows.Forms.TextBox txtInitials;
		private System.Windows.Forms.CheckBox chkTodoAfterDiags;
		private System.Windows.Forms.CheckBox chkFileNameInDiags;
		private System.Windows.Forms.GroupBox grpDiags;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox chkSurroundingTagsOnNewLines;
		private System.Windows.Forms.CheckBox chkTagDate;
		private System.Windows.Forms.TabPage tabMisc;
		private System.Windows.Forms.CheckBox chkShowSidebarOnStartup;
        private System.Windows.Forms.CheckBox chkFnNameInDiags;
		private System.Windows.Forms.CheckBox chkAutoCompletion;

	}
}