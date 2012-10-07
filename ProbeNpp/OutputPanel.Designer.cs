namespace ProbeNpp
{
	partial class OutputPanel
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
			this.lstOutput = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// lstOutput
			// 
			this.lstOutput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstOutput.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.lstOutput.FormattingEnabled = true;
			this.lstOutput.Location = new System.Drawing.Point(0, 0);
			this.lstOutput.Name = "lstOutput";
			this.lstOutput.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
			this.lstOutput.Size = new System.Drawing.Size(150, 150);
			this.lstOutput.TabIndex = 0;
			// 
			// OutputPanel
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.lstOutput);
			this.Name = "OutputPanel";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ListBox lstOutput;

	}
}
