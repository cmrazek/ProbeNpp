namespace ProbeNpp
{
	partial class SidebarForm
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SidebarForm));
			this.cmbApp = new System.Windows.Forms.ComboBox();
			this.cmAppCombo = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ciRefreshAppList = new System.Windows.Forms.ToolStripMenuItem();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabFiles = new System.Windows.Forms.TabPage();
			this.lstFiles = new System.Windows.Forms.ListView();
			this.colFileTitle = new System.Windows.Forms.ColumnHeader();
			this.colFileDir = new System.Windows.Forms.ColumnHeader();
			this.cmFiles = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ciRefreshFiles = new System.Windows.Forms.ToolStripMenuItem();
			this.txtFileFilter = new System.Windows.Forms.TextBox();
			this.treeFiles = new System.Windows.Forms.TreeView();
			this.imgFiles = new System.Windows.Forms.ImageList(this.components);
			this.tabFunctions = new System.Windows.Forms.TabPage();
			this.txtFunctionFilter = new System.Windows.Forms.TextBox();
			this.lstFunctions = new System.Windows.Forms.ListView();
			this.colFunctionName = new System.Windows.Forms.ColumnHeader();
			this.cmFunctions = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ciRefreshFunctions = new System.Windows.Forms.ToolStripMenuItem();
			this.imgFunctions = new System.Windows.Forms.ImageList(this.components);
			this.tabOther = new System.Windows.Forms.TabPage();
			this.btnPstTable = new System.Windows.Forms.Button();
			this.btnSettings = new System.Windows.Forms.Button();
			this.btnRunSamCam = new System.Windows.Forms.Button();
			this.btnFecFile = new System.Windows.Forms.Button();
			this.cmAppCombo.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.tabFiles.SuspendLayout();
			this.cmFiles.SuspendLayout();
			this.tabFunctions.SuspendLayout();
			this.cmFunctions.SuspendLayout();
			this.tabOther.SuspendLayout();
			this.SuspendLayout();
			// 
			// cmbApp
			// 
			this.cmbApp.ContextMenuStrip = this.cmAppCombo;
			this.cmbApp.Dock = System.Windows.Forms.DockStyle.Top;
			this.cmbApp.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbApp.FormattingEnabled = true;
			this.cmbApp.Location = new System.Drawing.Point(0, 0);
			this.cmbApp.Name = "cmbApp";
			this.cmbApp.Size = new System.Drawing.Size(205, 21);
			this.cmbApp.TabIndex = 1;
			this.cmbApp.SelectedIndexChanged += new System.EventHandler(this.cmbApp_SelectedIndexChanged);
			// 
			// cmAppCombo
			// 
			this.cmAppCombo.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ciRefreshAppList});
			this.cmAppCombo.Name = "cmAppCombo";
			this.cmAppCombo.Size = new System.Drawing.Size(124, 26);
			// 
			// ciRefreshAppList
			// 
			this.ciRefreshAppList.Name = "ciRefreshAppList";
			this.ciRefreshAppList.Size = new System.Drawing.Size(123, 22);
			this.ciRefreshAppList.Text = "&Refresh";
			this.ciRefreshAppList.Click += new System.EventHandler(this.ciRefreshAppList_Click);
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabFiles);
			this.tabControl.Controls.Add(this.tabFunctions);
			this.tabControl.Controls.Add(this.tabOther);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(0, 21);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(205, 506);
			this.tabControl.TabIndex = 2;
			// 
			// tabFiles
			// 
			this.tabFiles.Controls.Add(this.lstFiles);
			this.tabFiles.Controls.Add(this.txtFileFilter);
			this.tabFiles.Controls.Add(this.treeFiles);
			this.tabFiles.Location = new System.Drawing.Point(4, 22);
			this.tabFiles.Name = "tabFiles";
			this.tabFiles.Padding = new System.Windows.Forms.Padding(3);
			this.tabFiles.Size = new System.Drawing.Size(197, 480);
			this.tabFiles.TabIndex = 0;
			this.tabFiles.Text = "Files";
			this.tabFiles.UseVisualStyleBackColor = true;
			// 
			// lstFiles
			// 
			this.lstFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFileTitle,
            this.colFileDir});
			this.lstFiles.ContextMenuStrip = this.cmFiles;
			this.lstFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstFiles.FullRowSelect = true;
			this.lstFiles.Location = new System.Drawing.Point(3, 23);
			this.lstFiles.MultiSelect = false;
			this.lstFiles.Name = "lstFiles";
			this.lstFiles.Size = new System.Drawing.Size(191, 454);
			this.lstFiles.TabIndex = 3;
			this.lstFiles.UseCompatibleStateImageBehavior = false;
			this.lstFiles.View = System.Windows.Forms.View.Details;
			this.lstFiles.ItemActivate += new System.EventHandler(this.lstFiles_ItemActivate);
			this.lstFiles.Resize += new System.EventHandler(this.lstFiles_Resize);
			this.lstFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstFiles_KeyDown);
			// 
			// colFileTitle
			// 
			this.colFileTitle.Text = "File";
			this.colFileTitle.Width = 95;
			// 
			// colFileDir
			// 
			this.colFileDir.Text = "Dir";
			this.colFileDir.Width = 150;
			// 
			// cmFiles
			// 
			this.cmFiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ciRefreshFiles});
			this.cmFiles.Name = "cmFiles";
			this.cmFiles.Size = new System.Drawing.Size(124, 26);
			// 
			// ciRefreshFiles
			// 
			this.ciRefreshFiles.Name = "ciRefreshFiles";
			this.ciRefreshFiles.Size = new System.Drawing.Size(123, 22);
			this.ciRefreshFiles.Text = "&Refresh";
			this.ciRefreshFiles.Click += new System.EventHandler(this.ciRefreshFiles_Click);
			// 
			// txtFileFilter
			// 
			this.txtFileFilter.Dock = System.Windows.Forms.DockStyle.Top;
			this.txtFileFilter.Location = new System.Drawing.Point(3, 3);
			this.txtFileFilter.Name = "txtFileFilter";
			this.txtFileFilter.Size = new System.Drawing.Size(191, 20);
			this.txtFileFilter.TabIndex = 1;
			this.txtFileFilter.TextChanged += new System.EventHandler(this.txtFileFilter_TextChanged);
			this.txtFileFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFileFilter_KeyDown);
			// 
			// treeFiles
			// 
			this.treeFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.treeFiles.ContextMenuStrip = this.cmFiles;
			this.treeFiles.ImageIndex = 0;
			this.treeFiles.ImageList = this.imgFiles;
			this.treeFiles.Location = new System.Drawing.Point(3, 23);
			this.treeFiles.Name = "treeFiles";
			this.treeFiles.SelectedImageIndex = 0;
			this.treeFiles.Size = new System.Drawing.Size(191, 454);
			this.treeFiles.TabIndex = 0;
			this.treeFiles.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeFiles_NodeMouseDoubleClick);
			// 
			// imgFiles
			// 
			this.imgFiles.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgFiles.ImageStream")));
			this.imgFiles.TransparentColor = System.Drawing.Color.Transparent;
			this.imgFiles.Images.SetKeyName(0, "Folder.png");
			this.imgFiles.Images.SetKeyName(1, "File.png");
			// 
			// tabFunctions
			// 
			this.tabFunctions.Controls.Add(this.lstFunctions);
			this.tabFunctions.Controls.Add(this.txtFunctionFilter);
			this.tabFunctions.Location = new System.Drawing.Point(4, 22);
			this.tabFunctions.Name = "tabFunctions";
			this.tabFunctions.Padding = new System.Windows.Forms.Padding(3);
			this.tabFunctions.Size = new System.Drawing.Size(197, 480);
			this.tabFunctions.TabIndex = 1;
			this.tabFunctions.Text = "Functions";
			this.tabFunctions.UseVisualStyleBackColor = true;
			// 
			// txtFunctionFilter
			// 
			this.txtFunctionFilter.Dock = System.Windows.Forms.DockStyle.Top;
			this.txtFunctionFilter.Location = new System.Drawing.Point(3, 3);
			this.txtFunctionFilter.Name = "txtFunctionFilter";
			this.txtFunctionFilter.Size = new System.Drawing.Size(191, 20);
			this.txtFunctionFilter.TabIndex = 1;
			this.txtFunctionFilter.TextChanged += new System.EventHandler(this.txtFunctionFilter_TextChanged);
			this.txtFunctionFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFunctionFilter_KeyDown);
			// 
			// lstFunctions
			// 
			this.lstFunctions.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFunctionName});
			this.lstFunctions.ContextMenuStrip = this.cmFunctions;
			this.lstFunctions.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lstFunctions.FullRowSelect = true;
			this.lstFunctions.Location = new System.Drawing.Point(3, 23);
			this.lstFunctions.MultiSelect = false;
			this.lstFunctions.Name = "lstFunctions";
			this.lstFunctions.Size = new System.Drawing.Size(191, 454);
			this.lstFunctions.SmallImageList = this.imgFunctions;
			this.lstFunctions.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.lstFunctions.TabIndex = 0;
			this.lstFunctions.UseCompatibleStateImageBehavior = false;
			this.lstFunctions.View = System.Windows.Forms.View.Details;
			this.lstFunctions.ItemActivate += new System.EventHandler(this.lstFunctions_ItemActivate);
			this.lstFunctions.Resize += new System.EventHandler(this.lstFunctions_Resize);
			this.lstFunctions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lstFunctions_KeyDown);
			// 
			// colFunctionName
			// 
			this.colFunctionName.Text = "Name";
			this.colFunctionName.Width = 173;
			// 
			// cmFunctions
			// 
			this.cmFunctions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ciRefreshFunctions});
			this.cmFunctions.Name = "cmsFunctions";
			this.cmFunctions.Size = new System.Drawing.Size(124, 26);
			// 
			// ciRefreshFunctions
			// 
			this.ciRefreshFunctions.Name = "ciRefreshFunctions";
			this.ciRefreshFunctions.Size = new System.Drawing.Size(123, 22);
			this.ciRefreshFunctions.Text = "&Refresh";
			this.ciRefreshFunctions.Click += new System.EventHandler(this.ciRefreshFunctions_Click);
			// 
			// imgFunctions
			// 
			this.imgFunctions.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgFunctions.ImageStream")));
			this.imgFunctions.TransparentColor = System.Drawing.Color.Transparent;
			this.imgFunctions.Images.SetKeyName(0, "Function.png");
			// 
			// tabOther
			// 
			this.tabOther.Controls.Add(this.btnPstTable);
			this.tabOther.Controls.Add(this.btnSettings);
			this.tabOther.Controls.Add(this.btnRunSamCam);
			this.tabOther.Controls.Add(this.btnFecFile);
			this.tabOther.Location = new System.Drawing.Point(4, 22);
			this.tabOther.Name = "tabOther";
			this.tabOther.Padding = new System.Windows.Forms.Padding(3);
			this.tabOther.Size = new System.Drawing.Size(197, 480);
			this.tabOther.TabIndex = 2;
			this.tabOther.Text = "Other";
			this.tabOther.UseVisualStyleBackColor = true;
			// 
			// btnPstTable
			// 
			this.btnPstTable.Location = new System.Drawing.Point(0, 58);
			this.btnPstTable.Name = "btnPstTable";
			this.btnPstTable.Size = new System.Drawing.Size(120, 23);
			this.btnPstTable.TabIndex = 2;
			this.btnPstTable.Text = "PST Table";
			this.btnPstTable.UseVisualStyleBackColor = true;
			this.btnPstTable.Click += new System.EventHandler(this.btnPstTable_Click);
			// 
			// btnSettings
			// 
			this.btnSettings.Location = new System.Drawing.Point(0, 87);
			this.btnSettings.Name = "btnSettings";
			this.btnSettings.Size = new System.Drawing.Size(120, 23);
			this.btnSettings.TabIndex = 1;
			this.btnSettings.Text = "Settings";
			this.btnSettings.UseVisualStyleBackColor = true;
			this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
			// 
			// btnRunSamCam
			// 
			this.btnRunSamCam.Location = new System.Drawing.Point(0, 0);
			this.btnRunSamCam.Name = "btnRunSamCam";
			this.btnRunSamCam.Size = new System.Drawing.Size(120, 23);
			this.btnRunSamCam.TabIndex = 0;
			this.btnRunSamCam.Text = "Run SAM/CAM";
			this.btnRunSamCam.UseVisualStyleBackColor = true;
			this.btnRunSamCam.Click += new System.EventHandler(this.btnRunSamCam_Click);
			// 
			// btnFecFile
			// 
			this.btnFecFile.Location = new System.Drawing.Point(0, 29);
			this.btnFecFile.Name = "btnFecFile";
			this.btnFecFile.Size = new System.Drawing.Size(120, 23);
			this.btnFecFile.TabIndex = 0;
			this.btnFecFile.Text = "FEC Current File";
			this.btnFecFile.UseVisualStyleBackColor = true;
			this.btnFecFile.Click += new System.EventHandler(this.btnFecFile_Click);
			// 
			// SidebarForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(205, 527);
			this.Controls.Add(this.tabControl);
			this.Controls.Add(this.cmbApp);
			this.Name = "SidebarForm";
			this.Text = "Probe Sidebar";
			this.Resize += new System.EventHandler(this.SidebarForm_Resize);
			this.cmAppCombo.ResumeLayout(false);
			this.tabControl.ResumeLayout(false);
			this.tabFiles.ResumeLayout(false);
			this.tabFiles.PerformLayout();
			this.cmFiles.ResumeLayout(false);
			this.tabFunctions.ResumeLayout(false);
			this.tabFunctions.PerformLayout();
			this.cmFunctions.ResumeLayout(false);
			this.tabOther.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ComboBox cmbApp;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabFiles;
		private System.Windows.Forms.TreeView treeFiles;
		private System.Windows.Forms.ListView lstFiles;
		private System.Windows.Forms.TextBox txtFileFilter;
		private System.Windows.Forms.ColumnHeader colFileTitle;
		private System.Windows.Forms.ColumnHeader colFileDir;
		private System.Windows.Forms.TabPage tabFunctions;
		private System.Windows.Forms.ListView lstFunctions;
		private System.Windows.Forms.ColumnHeader colFunctionName;
		private System.Windows.Forms.ImageList imgFiles;
		private System.Windows.Forms.ContextMenuStrip cmFunctions;
		private System.Windows.Forms.ToolStripMenuItem ciRefreshFunctions;
		private System.Windows.Forms.ContextMenuStrip cmFiles;
		private System.Windows.Forms.ToolStripMenuItem ciRefreshFiles;
		private System.Windows.Forms.TextBox txtFunctionFilter;
		private System.Windows.Forms.ImageList imgFunctions;
		private System.Windows.Forms.TabPage tabOther;
		private System.Windows.Forms.Button btnFecFile;
		private System.Windows.Forms.ContextMenuStrip cmAppCombo;
		private System.Windows.Forms.ToolStripMenuItem ciRefreshAppList;
		private System.Windows.Forms.Button btnRunSamCam;
		private System.Windows.Forms.Button btnSettings;
		private System.Windows.Forms.Button btnPstTable;
	}
}