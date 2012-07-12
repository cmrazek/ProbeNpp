using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NppSharp;

namespace ProbeNpp
{
	internal partial class SidebarForm : UserControl
	{
		private ProbeNppPlugin _plugin;
		private FileDetails _file = null;

		private const int k_imgFolder = 0;
		private const int k_imgFile = 1;

		#region Construction
		public SidebarForm(ProbeNppPlugin plugin)
		{
			_plugin = plugin;

			InitializeComponent();
		}

		public void OnSidebarLoad(FileDetails currentFile)
		{
			try
			{
				RefreshEnvironment();
				OnFileActivated(currentFile);
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void OnShutdown()
		{
		}
		#endregion

		#region UI
		private void EnableControls()
		{
			lstFiles.Visible = !string.IsNullOrEmpty(txtFileFilter.Text);
			treeFiles.Visible = string.IsNullOrEmpty(txtFileFilter.Text);
		}

		private void FitListViewColumns(ListView list)
		{
			int clientWidth = list.ClientSize.Width;
			if (clientWidth > 10)
			{
				int colWidth = 0;

				foreach (ColumnHeader col in list.Columns) colWidth += col.Width;

				if (colWidth != clientWidth)
				{
					foreach (ColumnHeader col in list.Columns)
					{
						int newWidth = (int)((float)col.Width / (float)colWidth * (float)clientWidth);
						col.Width = newWidth;
					}
				}
			}

		}

		public void OnFileActivated(FileDetails fd)
		{
			try
			{
				_file = fd;
				if (fd.functions == null) ParseFunctions();
				PopulateFunctionList();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void OnAppChanged()
		{
			try
			{
				RefreshEnvironment();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void SidebarForm_Resize(object sender, EventArgs e)
		{
			try
			{
				// .NET doesn't seem to play well with Notepad++, and when resizing the sidebar,
				// it'll be overwritten with background color.  This refresh is a cludge to handle this.
				this.Refresh();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}
		#endregion

		#region App Combo
		private void RefreshEnvironment()
		{
			if (InvokeRequired)
			{
				Invoke(new Action(() => { RefreshEnvironment(); }));
				return;
			}

			PopulateAppCombo();
			PopulateFileTree();
		}

		private void PopulateAppCombo()
		{
			string currentApp = _plugin.Environment.CurrentApp;
			TagString selItem = null;

			cmbApp.Items.Clear();
			foreach (string appName in _plugin.Environment.AppNames)
			{
				TagString t;
				if (string.Equals(appName, currentApp, StringComparison.OrdinalIgnoreCase))
				{
					t = new TagString(string.Concat(appName, " (current)"), appName);
					selItem = t;
				}
				else
				{
					t = new TagString(appName, appName);
				}
				cmbApp.Items.Add(t);
			}

			if (selItem != null) cmbApp.SelectedItem = selItem;
		}

		private void cmbApp_SelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				TagString ts = (TagString)cmbApp.SelectedItem;
				if (ts != null)
				{
					if (!string.Equals((string)ts.Tag, _plugin.Environment.CurrentApp, StringComparison.OrdinalIgnoreCase))
					{
						string newApp = (string)ts.Tag;
						_plugin.Environment.CurrentApp = newApp;
						_plugin.Environment.Reload();
						RefreshEnvironment();
					}
				}
				EnableControls();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void ciRefreshAppList_Click(object sender, EventArgs e)
		{
			try
			{
				string oldApp = _plugin.Environment.CurrentApp;
				PopulateAppCombo();
				if (_plugin.Environment.CurrentApp != oldApp) OnAppChanged();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}
		#endregion

		#region File List
		private class FileTreeNode
		{
			public bool isFile;
			public string pathName;

			public FileTreeNode(bool isFile, string path)
			{
				this.isFile = isFile;
				this.pathName = path;
			}
		}

		private class ProbeFile
		{
			public string pathName;
			public string title;
			public string dir;
		}
		List<ProbeFile> _files = new List<ProbeFile>();

		private void PopulateFileTree()
		{
			try
			{
				treeFiles.Nodes.Clear();
				lstFiles.Items.Clear();
				_files.Clear();
				txtFileFilter.Text = "";

				foreach (string srcDir in _plugin.Environment.SourceDirs)
				{
					TreeNode rootNode = new TreeNode(srcDir);
					rootNode.Tag = new FileTreeNode(false, srcDir);
					treeFiles.Nodes.Add(rootNode);
					PopulateFileTree_SearchDir(srcDir, rootNode);
				}
			}
			catch (Exception ex)
			{
				_plugin.Output.WriteLine(OutputStyle.Error, "Error when refreshing file tree view: {0}",  ex);
			}
		}

		private void PopulateFileTree_SearchDir(string parentDir, TreeNode parentNode)
		{
			try
			{
				if (!Directory.Exists(parentDir)) return;

				foreach (string subDir in Directory.GetDirectories(parentDir))
				{
					TreeNode subDirNode = new TreeNode(Path.GetFileName(subDir));
					subDirNode.Tag = new FileTreeNode(false, subDir);
					subDirNode.ImageIndex = k_imgFolder;
					subDirNode.SelectedImageIndex = k_imgFolder;

					parentNode.Nodes.Add(subDirNode);
					PopulateFileTree_SearchDir(subDir, subDirNode);
				}

				foreach (string file in Directory.GetFiles(parentDir))
				{
					TreeNode fileNode = new TreeNode(Path.GetFileName(file));
					fileNode.Tag = new FileTreeNode(true, file);
					fileNode.ImageIndex = k_imgFile;
					fileNode.SelectedImageIndex = k_imgFile;

					parentNode.Nodes.Add(fileNode);
					AddProbeFile(file);
				}
			}
			catch (Exception ex)
			{
				_plugin.Output.WriteLine(OutputStyle.Error, "Error when scanning probe directory '{0}': {1}", parentDir, ex);
			}
		}

		private void treeFiles_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			try
			{
				if (e.Node != null && e.Node.Tag != null && e.Node.Tag.GetType() == typeof(FileTreeNode))
				{
					FileTreeNode ftn = (FileTreeNode)e.Node.Tag;
					if (ftn.isFile) _plugin.OpenFile(ftn.pathName);
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void txtFileFilter_TextChanged(object sender, EventArgs e)
		{
			try
			{
				if (!string.IsNullOrEmpty(txtFileFilter.Text)) ApplyFileFilter();
				EnableControls();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void AddProbeFile(string pathName)
		{
			ProbeFile pf = new ProbeFile();
			pf.pathName = pathName;
			pf.title = Path.GetFileName(pathName);
			pf.dir = Path.GetDirectoryName(pathName);
			_files.Add(pf);
		}

		private void ApplyFileFilter()
		{
			TextFilter filter = new TextFilter(txtFileFilter.Text);
			lstFiles.BeginUpdate();
			lstFiles.Items.Clear();
			try
			{
				foreach (ProbeFile pf in _files)
				{
					if (filter.Match(pf.title))
					{
						ListViewItem lvi = new ListViewItem(pf.title);
						lvi.SubItems.Add(pf.dir);
						lvi.Tag = pf;
						lstFiles.Items.Add(lvi);
					}
				}
			}
			finally
			{
				lstFiles.EndUpdate();
			}
		}

		private void lstFiles_ItemActivate(object sender, EventArgs e)
		{
			try
			{
				if (lstFiles.SelectedItems.Count == 0) return;
				ProbeFile pf = (ProbeFile)lstFiles.SelectedItems[0].Tag;
				_plugin.OpenFile(pf.pathName);
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void txtFileFilter_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (lstFiles.Visible)
				{
					if (e.KeyCode == Keys.Down)
					{
						if (lstFiles.Items.Count > 0)
						{
							ListViewItem lviFirst = lstFiles.Items[0];
							lstFiles.FocusedItem = lviFirst;
							lviFirst.Selected = true;
						}
						lstFiles.Focus();
						e.Handled = true;
					}
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void lstFiles_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyCode == Keys.Up && lstFiles.SelectedItems.Count == 1 &&
					lstFiles.SelectedItems[0].Index == 0)
				{
					txtFileFilter.SelectAll();
					txtFileFilter.Focus();
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void ciRefreshFiles_Click(object sender, EventArgs e)
		{
			try
			{
				PopulateFileTree();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void lstFiles_Resize(object sender, EventArgs e)
		{
			try
			{
				FitListViewColumns(lstFiles);
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void ShowFileList()
		{
			tabControl.SelectedTab = tabFiles;
			txtFileFilter.Focus();
			txtFileFilter.SelectAll();
		}
		#endregion

		#region Function List
		private void ParseFunctions()
		{
			FunctionParser parser = new FunctionParser();
			parser.Parse(_plugin.GetText(0, _plugin.Length));
			_file.functions = new List<Function>();
			_file.functions.AddRange(parser.Functions);
		}

		private void PopulateFunctionList()
		{
			if (_file == null) return;

			TextFilter tf = new TextFilter(txtFunctionFilter.Text);

			lstFunctions.Items.Clear();
			foreach (Function func in _file.functions)
			{
				if (tf.Match(func.Name))
				{
					lstFunctions.Items.Add(CreateFunctionLvi(func));
				}
			}
		}

		private ListViewItem CreateFunctionLvi(Function func)
		{
			ListViewItem lvi = new ListViewItem(func.Name);
			lvi.Tag = func;
			lvi.ImageIndex = 0;
			func.LVI = lvi;
			return lvi;
		}

		private void lstFunctions_ItemActivate(object sender, EventArgs e)
		{
			try
			{
				if (lstFunctions.SelectedItems.Count != 1) return;
				ListViewItem lvi = lstFunctions.SelectedItems[0];
				if (lvi.Tag != null && lvi.Tag.GetType() == typeof(Function))
				{
					Function func = (Function)lvi.Tag;
					UpdateFunctionList();
					_plugin.GoToLine(_plugin.LineCount);
					_plugin.GoToLine(func.StartLine);

					_plugin.FocusEditor();
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void UpdateFunctionList()
		{
			if (_file == null) return;

			TextFilter tf = new TextFilter(txtFunctionFilter.Text);

			foreach (Function func in _file.functions) func.Used = false;

			// Get a list of the functions, and where they are in the file now.
			FunctionParser fp = new FunctionParser();
			fp.Parse(_plugin.GetText(0, _plugin.Length));

			// Refresh the visible function list.
			List<Function> newFuncs = new List<Function>();
			foreach (Function newFunc in fp.Functions)
			{
				newFunc.Used = true;

				foreach (Function func in _file.functions)
				{
					if (func.Used) continue;
					if (func.UniqueName == newFunc.UniqueName)
					{
						func.Update(newFunc);
						func.Used = true;
						newFunc.Used = false;
						break;
					}
				}

				if (newFunc.Used && tf.Match(newFunc.Name)) newFuncs.Add(newFunc);
			}

			lstFunctions.BeginUpdate();
			try
			{
				// Delete any missing functions from the listview.
				foreach (Function func in _file.functions)
				{
					if (!func.Used && func.LVI != null)
					{
						func.LVI.Remove();
						func.LVI = null;
					}
				}

				// Add any new functions into the listview.
				foreach (Function func in newFuncs)
				{
					if (func.Used) lstFunctions.Items.Add(CreateFunctionLvi(func));
				}
			}
			finally
			{
				lstFunctions.EndUpdate();
			}
		}

		private void ciRefreshFunctions_Click(object sender, EventArgs e)
		{
			try
			{
				if (_file != null)
				{
					ParseFunctions();
					PopulateFunctionList();
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void txtFunctionFilter_TextChanged(object sender, EventArgs e)
		{
			try
			{
				PopulateFunctionList();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void txtFunctionFilter_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyCode == Keys.Down)
				{
					if (lstFunctions.Items.Count > 0)
					{
						ListViewItem lviFirst = lstFunctions.Items[0];
						lstFunctions.FocusedItem = lviFirst;
						lviFirst.Selected = true;
					}
					lstFunctions.Focus();
					e.Handled = true;
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void lstFunctions_KeyDown(object sender, KeyEventArgs e)
		{
			try
			{
				if (e.KeyCode == Keys.Up && lstFunctions.SelectedItems.Count == 1 &&
					lstFunctions.SelectedItems[0].Index == 0)
				{
					txtFunctionFilter.SelectAll();
					txtFunctionFilter.Focus();
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void lstFunctions_Resize(object sender, EventArgs e)
		{
			try
			{
				FitListViewColumns(lstFunctions);
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void ShowFunctionList()
		{
			if (_file != null)
			{
				ParseFunctions();
				PopulateFunctionList();
			}

			tabControl.SelectedTab = tabFunctions;
			txtFunctionFilter.Focus();
			txtFunctionFilter.SelectAll();
		}
		#endregion

		#region Fec File
		private void btnFecFile_Click(object sender, EventArgs e)
		{
			try
			{
				_plugin.FecFile();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}
		#endregion

		private void btnRunSamCam_Click(object sender, EventArgs e)
		{
			try
			{
				_plugin.OnRun();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void btnSettings_Click(object sender, EventArgs e)
		{
			try
			{
				_plugin.ShowSettings();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void btnPstTable_Click(object sender, EventArgs e)
		{
			try
			{
				_plugin.PstTable();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}
	}
}