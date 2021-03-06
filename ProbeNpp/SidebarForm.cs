using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NppSharp;

namespace ProbeNpp
{
	internal partial class SidebarForm : UserControl
	{
		private ProbeNppPlugin _plugin;
		private FileDetails _file = null;
		private bool _loaded = false;
		private int _functionSelectedLine = -1;
		private BackgroundDeferrer _functionListWait;

		private const int k_imgFolder = 0;
		private const int k_imgFile = 1;
		private const int k_maxFileParseLength = 1024 * 1024 * 10;	// 10 MB

		#region Construction
		public SidebarForm(ProbeNppPlugin plugin)
		{
			_plugin = plugin;

			InitializeComponent();

			if (_plugin.Settings.FileListView.FileColumnWidth > 0) colFileTitle.Width = _plugin.Settings.FileListView.FileColumnWidth;
			if (_plugin.Settings.FileListView.DirColumnWidth > 0) colFileDir.Width = _plugin.Settings.FileListView.DirColumnWidth;

			if (_plugin.Settings.FunctionListView.FunctionColumnWidth > 0) colFunctionName.Width = _plugin.Settings.FunctionListView.FunctionColumnWidth;

			_functionListWait = new BackgroundDeferrer();
			_functionListWait.Execute += new EventHandler(FunctionListWait_Execute);

			lstFunctions.DrawItem += new DrawListViewItemEventHandler(lstFunctions_DrawItem);
			lstFunctions.DrawSubItem += new DrawListViewSubItemEventHandler(lstFunctions_DrawSubItem);
			lstFunctions.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(lstFunctions_DrawColumnHeader);

			_loaded = true;
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

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			if (disposing)
			{
				if (_argsBrush != null) { _argsBrush.Dispose(); _argsBrush = null; }
				if (_argsBrushHighlight != null) { _argsBrushHighlight.Dispose(); _argsBrushHighlight = null; }
				if (_functionListWait != null) { _functionListWait.Dispose(); _functionListWait = null; }
			}

			base.Dispose(disposing);
		}
		#endregion

		#region UI
		private void EnableControls()
		{
			lstFiles.Visible = !string.IsNullOrEmpty(txtFileFilter.Text);
			treeFiles.Visible = string.IsNullOrEmpty(txtFileFilter.Text);
		}

		public void OnFileActivated(FileDetails fd)
		{
			try
			{
				_file = fd;
				_functionListWait.Cancel();
				InitializeFunctionList();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void OnNonProbeFileActivated()
		{
			try
			{
				lstFunctions.Items.Clear();
				_functionListWait.Cancel();
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void OnSelectionChanged(int lineNum)
		{
			if (lineNum != _functionSelectedLine)
			{
				SelectFunction(lineNum);
				_functionSelectedLine = lineNum;
			}
		}

		public void OnModified(ModifiedEventArgs e)
		{
			_functionListWait.OnActivity();
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
			string currentApp = ProbeEnvironment.CurrentApp;
			TagString selItem = null;

			cmbApp.Items.Clear();
			foreach (string appName in ProbeEnvironment.AppNames)
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
					if (!string.Equals((string)ts.Tag, ProbeEnvironment.CurrentApp, StringComparison.OrdinalIgnoreCase))
					{
						string newApp = (string)ts.Tag;
						ProbeEnvironment.CurrentApp = newApp;
						ProbeEnvironment.Reload();
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
				ProbeEnvironment.Reload();
				PopulateAppCombo();
				OnAppChanged();
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
				// Reset file lists back to empty.
				treeFiles.Nodes.Clear();
				lstFiles.Items.Clear();
				_files.Clear();
				txtFileFilter.Text = "";

				// Add files in source directories.
				foreach (string srcDir in ProbeEnvironment.SourceDirs)
				{
					TreeNode rootNode = new TreeNode(srcDir);
					rootNode.Tag = new FileTreeNode(false, srcDir);
					treeFiles.Nodes.Add(rootNode);
					PopulateFileTree_SearchDir(srcDir, rootNode);
				}

				// Add files in include directories.
				foreach (string includeDir in ProbeEnvironment.IncludeDirs)
				{
					TreeNode rootNode = new TreeNode(includeDir);
					rootNode.Tag = new FileTreeNode(false, includeDir);
					treeFiles.Nodes.Add(rootNode);
					PopulateFileTree_SearchDir(includeDir, rootNode);
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
					PopulateFileTree_AddFile(parentNode, file);
					AddProbeFile(file);
				}
			}
			catch (Exception ex)
			{
				_plugin.Output.WriteLine(OutputStyle.Error, "Error when scanning probe directory '{0}': {1}", parentDir, ex);
			}
		}

		private void PopulateFileTree_AddFile(TreeNode parentNode, string fileName)
		{
			TreeNode fileNode = new TreeNode(Path.GetFileName(fileName));
			fileNode.Tag = new FileTreeNode(true, fileName);
			fileNode.ImageIndex = k_imgFile;
			fileNode.SelectedImageIndex = k_imgFile;

			parentNode.Nodes.Add(fileNode);
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
			// Protect against duplicate files.
			if (_files.Any(f => f.pathName.Equals(pathName, StringComparison.OrdinalIgnoreCase))) return;

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
					else if (e.KeyCode == Keys.Escape)
					{
						txtFileFilter.Text = string.Empty;
						e.Handled = true;
						e.SuppressKeyPress = true;
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

		private void lstFiles_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
		{
			try
			{
				if (_loaded)
				{
					_plugin.Settings.FileListView.FileColumnWidth = colFileTitle.Width;
					_plugin.Settings.FileListView.DirColumnWidth = colFileDir.Width;
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void ShowFileList(string selectedText = null)
		{
			tabControl.SelectedTab = tabFiles;
			txtFileFilter.Focus();

			if (!string.IsNullOrEmpty(selectedText) &&
				ProbeEnvironment.IsValidFileName(selectedText) &&
				_files.Any(x => x.title.Equals(selectedText, StringComparison.OrdinalIgnoreCase)))
			{
				txtFileFilter.Text = selectedText;
			}
			else
			{
				txtFileFilter.SelectAll();
			}
		}

		private void treeFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			try
			{
				treeFiles.SelectedNode = e.Node;

				if (e.Node != null)
				{
					var ftNode = e.Node.Tag as FileTreeNode;
					ciCreateNewFile.Visible = !ftNode.isFile;
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void ciCreateNewFile_Click(object sender, EventArgs e)
		{
			try
			{
				var node = treeFiles.SelectedNode;
				if (node != null)
				{
					var ftNode = node.Tag as FileTreeNode;
					if (!ftNode.isFile)
					{
						OnCreateNewFile(ftNode.pathName, node);
					}
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		private void OnCreateNewFile(string dir, TreeNode parentNode)
		{
			using (var form = new PromptForm())
			{
				form.AllowEmpty = false;
				if (form.ShowDialog() == DialogResult.OK)
				{
					var fileName = form.Value;
					if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
					{
						Errors.Show(this, "This file name contains invalid characters.");
					}
					else
					{
						var pathName = Path.Combine(dir, fileName);
						if (File.Exists(pathName))
						{
							Errors.Show(this, "This file already exists.");
						}
						else
						{
							using (var stream = File.Create(pathName))
							{
							}
							PopulateFileTree_AddFile(parentNode, pathName);
							AddProbeFile(pathName);
						}
						_plugin.OpenFile(pathName);
						_plugin.CreateFileHeaderText(pathName);
					}
				}
			}
		}
		#endregion

		#region Function List
		private SolidBrush _argsBrush = null;
		private SolidBrush _argsBrushHighlight = null;

		private void InitializeFunctionList()
		{
			if (_file == null)
			{
				lstFunctions.Items.Clear();
				return;
			}

			TextFilter tf = new TextFilter(txtFunctionFilter.Text);

			lstFunctions.Items.Clear();

			foreach (Function func in _file.Functions)
			{
				if (tf.Match(func.Name))
				{
					var lvi = lstFunctions.Items.Add(CreateFunctionLvi(func));
				}
			}
		}

		private ListViewItem CreateFunctionLvi(Function func)
		{
			ListViewItem lvi = new ListViewItem(func.Signature);
			lvi.Tag = func;
			lvi.ImageIndex = 0;
			func.LVI = lvi;
			return lvi;
		}

		private void UpdateFunctionLvi(ListViewItem lvi, Function func, Function newFunc)
		{
			if (lvi != null && func != null && newFunc != null)
			{
				lvi.Text = func.Signature;
				lvi.Tag = func;
				lvi.ImageIndex = 0;
				func.LVI = lvi;
				func.Update(newFunc);
			}
		}

		private void lstFunctions_ItemActivate(object sender, EventArgs e)
		{
			try
			{
				if (lstFunctions.SelectedItems.Count != 1) return;
				ListViewItem lvi = lstFunctions.SelectedItems[0];
				if (lvi.Tag != null && lvi.Tag.GetType() == typeof(Function))
				{
					txtFunctionFilter.Clear();

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
			if (_file == null)
			{
				lstFunctions.Items.Clear();
				return;
			}

			TextFilter tf = new TextFilter(txtFunctionFilter.Text);

			// Get a list of the functions, and where they are in the file now.
			FunctionParser fp = new FunctionParser();
			var parsedFuncs = fp.Parse(_plugin.GetText(_plugin.Start, _plugin.End)).ToArray();

			Action updateAction = () =>
			{
				var listFuncs = (from i in lstFunctions.Items.Cast<ListViewItem>() select i.Tag as Function).ToArray();
				var fileFuncs = _file.Functions.ToArray();

				var newFuncs = (from func in parsedFuncs where !_file.FunctionIdExists(func.Id) select func).ToArray();
				var updatedFuncs = (from func in parsedFuncs where _file.FunctionIdExists(func.Id) select _file.GetFunction(func.Id)).ToArray();
				var deletedFuncs = (from name in _file.FunctionIds where !parsedFuncs.Any(f => f.Id == name) select _file.GetFunction(name)).ToArray();

				lstFunctions.BeginUpdate();
				try
				{
					foreach (var func in deletedFuncs)
					{
						func.LVI.Remove();
						_file.RemoveFunction(func.Id);
					}

					foreach (var func in newFuncs)
					{
						var lvi = CreateFunctionLvi(func);
						lstFunctions.Items.Add(lvi);
						_file.AddFunction(func);
					}

					foreach (var func in updatedFuncs)
					{
						UpdateFunctionLvi(func.LVI, func, (from f in parsedFuncs where f.Id == func.Id select f).FirstOrDefault());
					}
				}
				finally
				{
					lstFunctions.EndUpdate();
				}
			};
			if (lstFunctions.InvokeRequired) lstFunctions.BeginInvoke(updateAction);
			else updateAction();
		}

		private void ciRefreshFunctions_Click(object sender, EventArgs e)
		{
			try
			{
				if (_file != null)
				{
					var source = _plugin.GetText(_plugin.Start, _plugin.End);
					_file.ParseFunctions(source);
					InitializeFunctionList();
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
				InitializeFunctionList();
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
				else if (e.KeyCode == Keys.Escape)
				{
					txtFunctionFilter.Text = string.Empty;
					e.Handled = true;
					e.SuppressKeyPress = true;
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

		private void lstFunctions_ColumnWidthChanged(object sender, ColumnWidthChangedEventArgs e)
		{
			try
			{
				if (_loaded)
				{
					_plugin.Settings.FunctionListView.FunctionColumnWidth = colFunctionName.Width;
				}
			}
			catch (Exception ex)
			{
				Errors.Show(this, ex);
			}
		}

		public void ShowFunctionList(string selectedText = null)
		{
			if (_file != null)
			{
				InitializeFunctionList();
			}

			tabControl.SelectedTab = tabFunctions;
			txtFunctionFilter.Focus();

			if (_file != null &&
				!string.IsNullOrEmpty(selectedText) &&
				ProbeEnvironment.IsValidFunctionName(selectedText) &&
				_file.GetFunctionsWithName(selectedText).Any())
			{
				txtFunctionFilter.Text = selectedText;
			}
			else
			{
				txtFunctionFilter.SelectAll();
			}
		}

		private void SelectFunction(int lineNum)
		{
			if (_file != null)
			{
				var func = _file.GetFunctionForLineNum(lineNum);
				if (func != null)
				{
					foreach (ListViewItem lvi in lstFunctions.Items)
					{
						if (lvi.Tag == func)
						{
							lvi.Selected = true;
							lvi.EnsureVisible();
						}
						else
						{
							lvi.Selected = false;
						}
					}
				}
			}
		}

		void FunctionListWait_Execute(object sender, EventArgs e)
		{
			System.Threading.ThreadPool.QueueUserWorkItem(x =>
			{
				try
				{
					UpdateFunctionList();
				}
				catch (Exception ex)
				{
					ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, "Exception in function list update background thread: {0}", ex);
				}
			});
		}

		void lstFunctions_DrawItem(object sender, DrawListViewItemEventArgs e)
		{
			var tag = e.Item.Tag;
			if (tag == null || tag.GetType() != typeof(Function))
			{
			    e.DrawDefault = true;
			}
			else
			{
			    e.DrawBackground();
			}
		}

		void lstFunctions_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			if (_argsBrush == null) _argsBrush = new SolidBrush(Util.MixColor(SystemColors.Window, SystemColors.WindowText, .5f));
			if (_argsBrushHighlight == null) _argsBrushHighlight = new SolidBrush(Util.MixColor(SystemColors.Highlight, SystemColors.HighlightText, .5f));

			if (e.ColumnIndex != 0 || e.Item == null || e.Item.Tag == null || e.Item.Tag.GetType() != typeof(Function))
			{
				e.DrawDefault = true;
			}
			else
			{
				e.DrawDefault = false;

				var func = e.Item.Tag as Function;
				var selected = e.Item.Selected;

				const int k_iconSize = 16;
				const int k_spacer = 2;
				var iconRect = new Rectangle(e.Bounds.Left + k_spacer, e.Bounds.Top + (e.Bounds.Height - k_iconSize) / 2, k_iconSize, k_iconSize);
				var textRect = new Rectangle(iconRect.Right + k_spacer, e.Bounds.Top, e.Bounds.Right - (iconRect.Right + k_spacer), e.Bounds.Height);

				using (var sf = new StringFormat())
				{
					sf.FormatFlags = StringFormatFlags.NoWrap;
					sf.LineAlignment = StringAlignment.Center;

					e.Graphics.FillRectangle(selected ? SystemBrushes.Highlight : SystemBrushes.Window, textRect);
					e.Graphics.DrawIcon(Res.FunctionIcon, iconRect);

					if (e.Item.Text.StartsWith(func.Name))
					{
						e.Graphics.DrawString(func.Name, lstFunctions.Font,
							selected ? SystemBrushes.HighlightText : SystemBrushes.WindowText, textRect, sf);

						if (e.Item.Text.Length > func.Name.Length)
						{
							var size = Util.MeasureString(e.Graphics, func.Name, lstFunctions.Font, textRect, sf);
							var argsRect = new RectangleF(textRect.Left + size.Width, textRect.Top, textRect.Width - size.Width, textRect.Height);
							if (!argsRect.IsEmpty)
							{
								e.Graphics.DrawString(e.Item.Text.Substring(func.Name.Length), lstFunctions.Font,
									selected ? _argsBrushHighlight : _argsBrush, argsRect, sf);
							}
						}
					}
					else
					{
						e.Graphics.DrawString(e.Item.Text, lstFunctions.Font,
							selected ? SystemBrushes.HighlightText : SystemBrushes.WindowText,
							textRect, sf);
					}

					e.DrawFocusRectangle(e.Bounds);
				}
			}
		}

		void lstFunctions_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
		{
			e.DrawDefault = true;
		}

		private void lstFunctions_SystemColorsChanged(object sender, EventArgs e)
		{
			if (_argsBrush != null)
			{
				_argsBrush.Dispose();
				_argsBrush = null;
			}

			if (_argsBrushHighlight != null)
			{
				_argsBrushHighlight.Dispose();
				_argsBrushHighlight = null;
			}
		}
		#endregion

		
	}
}