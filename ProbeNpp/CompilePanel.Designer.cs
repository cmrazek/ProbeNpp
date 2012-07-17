namespace ProbeNpp
{
	partial class CompilePanel
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
			this.components = new System.ComponentModel.Container();
			this.lstHistory = new System.Windows.Forms.ListBox();
			this.cmCompile = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ciCompile = new System.Windows.Forms.ToolStripMenuItem();
			this.ciKillCompile = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.ciHideAfterSuccess = new System.Windows.Forms.ToolStripMenuItem();
			this.ciHideAfterWarnings = new System.Windows.Forms.ToolStripMenuItem();
			this.cmCompile.SuspendLayout();
			this.SuspendLayout();
			// 
			// lstHistory
			// 
			this.lstHistory.ContextMenuStrip = this.cmCompile;
			this.lstHistory.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstHistory.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.lstHistory.FormattingEnabled = true;
			this.lstHistory.Location = new System.Drawing.Point(0, 0);
			this.lstHistory.Name = "lstHistory";
			this.lstHistory.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.lstHistory.Size = new System.Drawing.Size(809, 139);
			this.lstHistory.TabIndex = 0;
			this.lstHistory.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstHistory_DrawItem);
			this.lstHistory.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.lstHistory_MeasureItem);
			this.lstHistory.DoubleClick += new System.EventHandler(this.lstHistory_DoubleClick);
			this.lstHistory.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstHistory_KeyDown);
			this.lstHistory.MouseLeave += new System.EventHandler(this.lstHistory_MouseLeave);
			// 
			// cmCompile
			// 
			this.cmCompile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ciCompile,
            this.ciKillCompile,
            this.toolStripMenuItem1,
            this.ciHideAfterSuccess,
            this.ciHideAfterWarnings});
			this.cmCompile.Name = "cmCompile";
			this.cmCompile.Size = new System.Drawing.Size(273, 98);
			this.cmCompile.Opening += new System.ComponentModel.CancelEventHandler(this.cmCompile_Opening);
			// 
			// ciCompile
			// 
			this.ciCompile.Name = "ciCompile";
			this.ciCompile.Size = new System.Drawing.Size(272, 22);
			this.ciCompile.Text = "&Compile";
			this.ciCompile.Click += new System.EventHandler(this.ciCompile_Click);
			// 
			// ciKillCompile
			// 
			this.ciKillCompile.Name = "ciKillCompile";
			this.ciKillCompile.Size = new System.Drawing.Size(272, 22);
			this.ciKillCompile.Text = "&Kill Compile";
			this.ciKillCompile.Click += new System.EventHandler(this.ciKillCompile_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(269, 6);
			// 
			// ciHideAfterSuccess
			// 
			this.ciHideAfterSuccess.Name = "ciHideAfterSuccess";
			this.ciHideAfterSuccess.Size = new System.Drawing.Size(272, 22);
			this.ciHideAfterSuccess.Text = "Hide After Successful Compile";
			this.ciHideAfterSuccess.Click += new System.EventHandler(this.ciHideAfterSuccess_Click);
			// 
			// ciHideAfterWarnings
			// 
			this.ciHideAfterWarnings.Name = "ciHideAfterWarnings";
			this.ciHideAfterWarnings.Size = new System.Drawing.Size(272, 22);
			this.ciHideAfterWarnings.Text = "Hide After Compile With Warnings Only";
			this.ciHideAfterWarnings.Click += new System.EventHandler(this.ciHideAfterWarnings_Click);
			// 
			// CompilePanel
			// 
			this.Controls.Add(this.lstHistory);
			this.Name = "CompilePanel";
			this.Size = new System.Drawing.Size(809, 139);
			this.cmCompile.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox lstHistory;
		private System.Windows.Forms.ContextMenuStrip cmCompile;
		private System.Windows.Forms.ToolStripMenuItem ciCompile;
		private System.Windows.Forms.ToolStripMenuItem ciKillCompile;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem ciHideAfterSuccess;
		private System.Windows.Forms.ToolStripMenuItem ciHideAfterWarnings;
	}
}