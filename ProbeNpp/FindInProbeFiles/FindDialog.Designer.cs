namespace ProbeNpp.FindInProbeFiles
{
	partial class FindDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.cmbSearchText = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.cmbMethod = new System.Windows.Forms.ComboBox();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.chkMatchCase = new System.Windows.Forms.CheckBox();
			this.chkMatchWholeWord = new System.Windows.Forms.CheckBox();
			this.chkOnlyProbeFiles = new System.Windows.Forms.CheckBox();
			this.txtIncludeExtensions = new System.Windows.Forms.TextBox();
			this.txtExcludeExtensions = new System.Windows.Forms.TextBox();
			this.lblIncludeExtensions = new System.Windows.Forms.Label();
			this.lblExcludeExtensions = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 15);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(56, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Find what:";
			// 
			// cmbSearchText
			// 
			this.cmbSearchText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.cmbSearchText.FormattingEnabled = true;
			this.cmbSearchText.Location = new System.Drawing.Point(74, 12);
			this.cmbSearchText.Name = "cmbSearchText";
			this.cmbSearchText.Size = new System.Drawing.Size(256, 21);
			this.cmbSearchText.TabIndex = 1;
			this.cmbSearchText.SelectedIndexChanged += new System.EventHandler(this.cmbSearchText_SelectedIndexChanged);
			this.cmbSearchText.TextUpdate += new System.EventHandler(this.cmbSearchText_TextUpdate);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 42);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(46, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Method:";
			// 
			// cmbMethod
			// 
			this.cmbMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbMethod.FormattingEnabled = true;
			this.cmbMethod.Location = new System.Drawing.Point(74, 39);
			this.cmbMethod.Name = "cmbMethod";
			this.cmbMethod.Size = new System.Drawing.Size(121, 21);
			this.cmbMethod.TabIndex = 3;
			this.cmbMethod.SelectedIndexChanged += new System.EventHandler(this.cmbMethod_SelectedIndexChanged);
			// 
			// btnOk
			// 
			this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOk.Location = new System.Drawing.Point(174, 199);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(75, 23);
			this.btnOk.TabIndex = 11;
			this.btnOk.Text = "&OK";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(255, 199);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(75, 23);
			this.btnCancel.TabIndex = 12;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// chkMatchCase
			// 
			this.chkMatchCase.AutoSize = true;
			this.chkMatchCase.Location = new System.Drawing.Point(74, 66);
			this.chkMatchCase.Name = "chkMatchCase";
			this.chkMatchCase.Size = new System.Drawing.Size(82, 17);
			this.chkMatchCase.TabIndex = 4;
			this.chkMatchCase.Text = "Match &case";
			this.chkMatchCase.UseVisualStyleBackColor = true;
			// 
			// chkMatchWholeWord
			// 
			this.chkMatchWholeWord.AutoSize = true;
			this.chkMatchWholeWord.Location = new System.Drawing.Point(74, 89);
			this.chkMatchWholeWord.Name = "chkMatchWholeWord";
			this.chkMatchWholeWord.Size = new System.Drawing.Size(113, 17);
			this.chkMatchWholeWord.TabIndex = 5;
			this.chkMatchWholeWord.Text = "Match whole &word";
			this.chkMatchWholeWord.UseVisualStyleBackColor = true;
			// 
			// chkOnlyProbeFiles
			// 
			this.chkOnlyProbeFiles.AutoSize = true;
			this.chkOnlyProbeFiles.Location = new System.Drawing.Point(74, 113);
			this.chkOnlyProbeFiles.Name = "chkOnlyProbeFiles";
			this.chkOnlyProbeFiles.Size = new System.Drawing.Size(139, 17);
			this.chkOnlyProbeFiles.TabIndex = 6;
			this.chkOnlyProbeFiles.Text = "Only Search Probe Files";
			this.chkOnlyProbeFiles.UseVisualStyleBackColor = true;
			// 
			// txtIncludeExtensions
			// 
			this.txtIncludeExtensions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtIncludeExtensions.Location = new System.Drawing.Point(120, 136);
			this.txtIncludeExtensions.Name = "txtIncludeExtensions";
			this.txtIncludeExtensions.Size = new System.Drawing.Size(210, 20);
			this.txtIncludeExtensions.TabIndex = 8;
			// 
			// txtExcludeExtensions
			// 
			this.txtExcludeExtensions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.txtExcludeExtensions.Location = new System.Drawing.Point(120, 162);
			this.txtExcludeExtensions.Name = "txtExcludeExtensions";
			this.txtExcludeExtensions.Size = new System.Drawing.Size(210, 20);
			this.txtExcludeExtensions.TabIndex = 10;
			// 
			// lblIncludeExtensions
			// 
			this.lblIncludeExtensions.AutoSize = true;
			this.lblIncludeExtensions.Location = new System.Drawing.Point(12, 139);
			this.lblIncludeExtensions.Name = "lblIncludeExtensions";
			this.lblIncludeExtensions.Size = new System.Drawing.Size(99, 13);
			this.lblIncludeExtensions.TabIndex = 7;
			this.lblIncludeExtensions.Text = "Include Extensions:";
			// 
			// lblExcludeExtensions
			// 
			this.lblExcludeExtensions.AutoSize = true;
			this.lblExcludeExtensions.Location = new System.Drawing.Point(12, 165);
			this.lblExcludeExtensions.Name = "lblExcludeExtensions";
			this.lblExcludeExtensions.Size = new System.Drawing.Size(102, 13);
			this.lblExcludeExtensions.TabIndex = 9;
			this.lblExcludeExtensions.Text = "Exclude Extensions:";
			// 
			// FindDialog
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(342, 234);
			this.Controls.Add(this.lblExcludeExtensions);
			this.Controls.Add(this.lblIncludeExtensions);
			this.Controls.Add(this.txtExcludeExtensions);
			this.Controls.Add(this.txtIncludeExtensions);
			this.Controls.Add(this.chkOnlyProbeFiles);
			this.Controls.Add(this.chkMatchWholeWord);
			this.Controls.Add(this.chkMatchCase);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.cmbMethod);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cmbSearchText);
			this.Controls.Add(this.label1);
			this.MaximumSize = new System.Drawing.Size(32767, 268);
			this.MinimumSize = new System.Drawing.Size(350, 268);
			this.Name = "FindDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Find in Probe Files";
			this.Load += new System.EventHandler(this.FindInProbeFilesDialog_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cmbSearchText;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox cmbMethod;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.CheckBox chkMatchCase;
		private System.Windows.Forms.CheckBox chkMatchWholeWord;
		private System.Windows.Forms.CheckBox chkOnlyProbeFiles;
		private System.Windows.Forms.TextBox txtIncludeExtensions;
		private System.Windows.Forms.TextBox txtExcludeExtensions;
		private System.Windows.Forms.Label lblIncludeExtensions;
		private System.Windows.Forms.Label lblExcludeExtensions;
	}
}