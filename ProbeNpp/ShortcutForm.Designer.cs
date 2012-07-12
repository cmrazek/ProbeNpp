namespace ProbeNpp
{
	partial class ShortcutForm
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
			this.label1 = new System.Windows.Forms.Label();
			this.lstActions = new System.Windows.Forms.ListView();
			this.colKey = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.colAction = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(247, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Enter a key to trigger one of the commands, below:";
			// 
			// lstActions
			// 
			this.lstActions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lstActions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colKey,
            this.colAction});
			this.lstActions.FullRowSelect = true;
			this.lstActions.Location = new System.Drawing.Point(12, 25);
			this.lstActions.MultiSelect = false;
			this.lstActions.Name = "lstActions";
			this.lstActions.Size = new System.Drawing.Size(260, 145);
			this.lstActions.TabIndex = 1;
			this.lstActions.UseCompatibleStateImageBehavior = false;
			this.lstActions.View = System.Windows.Forms.View.Details;
			this.lstActions.ItemActivate += new System.EventHandler(this.lstActions_ItemActivate);
			this.lstActions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstActions_KeyDown);
			// 
			// colKey
			// 
			this.colKey.Text = "Key";
			// 
			// colAction
			// 
			this.colAction.Text = "Action";
			this.colAction.Width = 170;
			// 
			// ShortcutForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 182);
			this.Controls.Add(this.lstActions);
			this.Controls.Add(this.label1);
			this.MinimumSize = new System.Drawing.Size(300, 220);
			this.Name = "ShortcutForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Probe Shortcut";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ShortcutForm_KeyDown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView lstActions;
		private System.Windows.Forms.ColumnHeader colKey;
		private System.Windows.Forms.ColumnHeader colAction;
	}
}