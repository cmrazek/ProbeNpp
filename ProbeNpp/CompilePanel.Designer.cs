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
			this.lstHistory.Size = new System.Drawing.Size(809, 139);
			this.lstHistory.TabIndex = 0;
			this.lstHistory.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.lstHistory_DrawItem);
			this.lstHistory.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.lstHistory_MeasureItem);
			this.lstHistory.DoubleClick += new System.EventHandler(this.lstHistory_DoubleClick);
			// 
			// cmCompile
			// 
			this.cmCompile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ciCompile,
            this.ciKillCompile});
			this.cmCompile.Name = "cmCompile";
			this.cmCompile.Size = new System.Drawing.Size(138, 48);
			// 
			// ciCompile
			// 
			this.ciCompile.Name = "ciCompile";
			this.ciCompile.Size = new System.Drawing.Size(137, 22);
			this.ciCompile.Text = "&Compile";
			this.ciCompile.Click += new System.EventHandler(this.ciCompile_Click);
			// 
			// ciKillCompile
			// 
			this.ciKillCompile.Name = "ciKillCompile";
			this.ciKillCompile.Size = new System.Drawing.Size(137, 22);
			this.ciKillCompile.Text = "&Kill Compile";
			this.ciKillCompile.Click += new System.EventHandler(this.ciKillCompile_Click);
			// 
			// CompilePanel
			// 
			this.ClientSize = new System.Drawing.Size(809, 139);
			this.Controls.Add(this.lstHistory);
			this.Name = "CompilePanel";
			this.Text = "CompilePanel";
			this.cmCompile.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox lstHistory;
		private System.Windows.Forms.ContextMenuStrip cmCompile;
		private System.Windows.Forms.ToolStripMenuItem ciCompile;
		private System.Windows.Forms.ToolStripMenuItem ciKillCompile;
	}
}