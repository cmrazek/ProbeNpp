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
			this.label1 = new System.Windows.Forms.Label();
			this.lblProbeExtensions = new System.Windows.Forms.Label();
			this.txtProbeExtensions = new System.Windows.Forms.TextBox();
			this.panel1.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabCompile.SuspendLayout();
			this.tabExtensions.SuspendLayout();
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
			this.tabControl.Controls.Add(this.tabCompile);
			this.tabControl.Controls.Add(this.tabExtensions);
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
			this.tabExtensions.Controls.Add(this.label1);
			this.tabExtensions.Controls.Add(this.lblProbeExtensions);
			this.tabExtensions.Controls.Add(this.txtProbeExtensions);
			this.tabExtensions.Location = new System.Drawing.Point(4, 22);
			this.tabExtensions.Name = "tabExtensions";
			this.tabExtensions.Padding = new System.Windows.Forms.Padding(3);
			this.tabExtensions.Size = new System.Drawing.Size(344, 245);
			this.tabExtensions.TabIndex = 1;
			this.tabExtensions.Text = "Extensions";
			this.tabExtensions.UseVisualStyleBackColor = true;
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
			// txtProbeExtensions
			// 
			this.txtProbeExtensions.AcceptsReturn = true;
			this.txtProbeExtensions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtProbeExtensions.Location = new System.Drawing.Point(8, 34);
			this.txtProbeExtensions.Multiline = true;
			this.txtProbeExtensions.Name = "txtProbeExtensions";
			this.txtProbeExtensions.Size = new System.Drawing.Size(328, 205);
			this.txtProbeExtensions.TabIndex = 0;
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
		private System.Windows.Forms.TextBox txtProbeExtensions;
		private System.Windows.Forms.Label lblProbeExtensions;
		private System.Windows.Forms.Label label1;

	}
}