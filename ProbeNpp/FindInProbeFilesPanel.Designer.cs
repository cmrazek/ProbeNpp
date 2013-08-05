namespace ProbeNpp
{
	partial class FindInProbeFilesPanel
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.lstMatches = new System.Windows.Forms.ListView();
			this.colFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colLineNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colContext = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.progWorking = new System.Windows.Forms.ProgressBar();
			this.cmProgress = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ciStopFind = new System.Windows.Forms.ToolStripMenuItem();
			this.cmProgress.SuspendLayout();
			this.SuspendLayout();
			// 
			// lstMatches
			// 
			this.lstMatches.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFileName,
            this.colLineNumber,
            this.colContext});
			this.lstMatches.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstMatches.FullRowSelect = true;
			this.lstMatches.HideSelection = false;
			this.lstMatches.Location = new System.Drawing.Point(0, 0);
			this.lstMatches.Name = "lstMatches";
			this.lstMatches.Size = new System.Drawing.Size(542, 137);
			this.lstMatches.TabIndex = 0;
			this.lstMatches.UseCompatibleStateImageBehavior = false;
			this.lstMatches.View = System.Windows.Forms.View.Details;
			this.lstMatches.ColumnWidthChanged += new System.Windows.Forms.ColumnWidthChangedEventHandler(this.lstMatches_ColumnWidthChanged);
			this.lstMatches.ItemActivate += new System.EventHandler(this.lstMatches_ItemActivate);
			// 
			// colFileName
			// 
			this.colFileName.Text = "File Name";
			this.colFileName.Width = 250;
			// 
			// colLineNumber
			// 
			this.colLineNumber.Text = "Line";
			// 
			// colContext
			// 
			this.colContext.Width = 300;
			// 
			// progWorking
			// 
			this.progWorking.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.progWorking.ContextMenuStrip = this.cmProgress;
			this.progWorking.Enabled = false;
			this.progWorking.Location = new System.Drawing.Point(439, 3);
			this.progWorking.Name = "progWorking";
			this.progWorking.Size = new System.Drawing.Size(100, 15);
			this.progWorking.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
			this.progWorking.TabIndex = 1;
			// 
			// cmProgress
			// 
			this.cmProgress.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ciStopFind});
			this.cmProgress.Name = "cmProgress";
			this.cmProgress.Size = new System.Drawing.Size(108, 26);
			// 
			// ciStopFind
			// 
			this.ciStopFind.Name = "ciStopFind";
			this.ciStopFind.Size = new System.Drawing.Size(107, 22);
			this.ciStopFind.Text = "&Stop";
			this.ciStopFind.Click += new System.EventHandler(this.ciStopFind_Click);
			// 
			// FindInProbeFilesPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.progWorking);
			this.Controls.Add(this.lstMatches);
			this.Name = "FindInProbeFilesPanel";
			this.Size = new System.Drawing.Size(542, 137);
			this.Load += new System.EventHandler(this.FindInProbeFilesPanel_Load);
			this.cmProgress.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListView lstMatches;
		private System.Windows.Forms.ColumnHeader colFileName;
		private System.Windows.Forms.ColumnHeader colLineNumber;
		private System.Windows.Forms.ColumnHeader colContext;
		private System.Windows.Forms.ProgressBar progWorking;
		private System.Windows.Forms.ContextMenuStrip cmProgress;
		private System.Windows.Forms.ToolStripMenuItem ciStopFind;
	}
}
