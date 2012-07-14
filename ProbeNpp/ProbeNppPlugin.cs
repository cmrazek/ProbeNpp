using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Drawing;
using NppSharp;

namespace ProbeNpp
{
	public class ProbeNppPlugin : NppScript
	{
		#region Static Instance
		private static ProbeNppPlugin _instance;

		public static ProbeNppPlugin Instance
		{
			get { return _instance; }
		}
		#endregion

		#region Notepad++ Integration
		private NativeWindow _nppWindow = null;
		private Dictionary<uint, FileDetails> _fileDetails = new Dictionary<uint, FileDetails>();
		private FileDetails _currentFile = null;

		public const string k_appNameIdent = "ProbeNpp";

		public enum DockPosition
		{
			Left,
			Top,
			Right,
			Bottom,
			Floating
		}

		public ProbeNppPlugin()
		{
			Init(NppWindow);

			Ready += new NppEventHandler(Plugin_Ready);
			Shutdown += new NppEventHandler(Plugin_Shutdown);
			FileOpened += new FileEventHandler(Plugin_FileOpened);
			FileClosed += new FileEventHandler(Plugin_FileClosed);
			FileActivated += new FileEventHandler(Plugin_FileActivated);
		}

		private void Plugin_Ready(object sender, EventArgs e)
		{
		}

		private void Init(NativeWindow nppWindow)
		{
			try
			{
				_instance = this;
				_nppWindow = nppWindow;

				_settings = new Settings(this);
				try
				{
					_settings.Load();
				}
				catch (Exception ex)
				{
					Errors.Show(_nppWindow, ex, "Exception when attempting to load ProbeNpp settings.");
				}

				_env = new ProbeEnvironment();
				_env.AppChanged += new EventHandler(_env_AppChanged);
				
				TempManager.Init(Path.Combine(ConfigDir, "Temp"));
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex, "The ProbeNpp plug-in thrown an error while initializing.");
			}
		}

		private void Plugin_Shutdown(object sender, EventArgs e)
		{
			try
			{
				if (_compilePanel != null) _compilePanel.OnShutdown();
				if (_sidebar != null) _sidebar.OnShutdown();
				_settings.Save();
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex, "Error while shutting down ProbeNpp plugin.");
			}
		}

		private void Plugin_FileOpened(object sender, FileEventArgs e)
		{
			try
			{
				if (!_fileDetails.ContainsKey(e.BufferId)) _fileDetails.Add(e.BufferId, new FileDetails(e.BufferId));
			}
			catch (Exception ex)
			{
				Output.WriteLine(OutputStyle.Error, "Exception in FileOpened event: {0}", ex);
			}
		}

		private void Plugin_FileClosed(object sender, FileEventArgs e)
		{
			try
			{
				if (_fileDetails.ContainsKey(e.BufferId)) _fileDetails.Remove(e.BufferId);
			}
			catch (Exception ex)
			{
				Output.WriteLine(OutputStyle.Error, "Exception in FileOpened event: {0}", ex);
			}
		}

		private void Plugin_FileActivated(object sender, FileEventArgs e)
		{
			try
			{
				FileDetails fd;
				if (!_fileDetails.TryGetValue(e.BufferId, out fd))
				{
					fd = new FileDetails(e.BufferId);
					_fileDetails.Add(e.BufferId, fd);
					fd.lastProbeApp = _env.CurrentApp;
				}
				_currentFile = fd;
				
				if (!string.IsNullOrEmpty(fd.lastProbeApp) && fd.lastProbeApp != _env.CurrentApp)
				{
					fd.lastProbeApp = _env.CurrentApp;
				}
				if (_sidebar != null) _sidebar.OnFileActivated(fd);
			}
			catch (Exception ex)
			{
				Output.WriteLine(OutputStyle.Error, "Exception in FileOpened event: {0}", ex);
			}
		}

		internal string ConfigDir
		{
			get
			{
				var dir = Path.Combine(ConfigDirectory, k_appNameIdent);
				if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
				return dir;
			}
		}
		#endregion

		#region Probe Integration
		private ProbeEnvironment _env = null;
		private int _probeLanguageId = 0;

		public ProbeEnvironment Environment
		{
			get { return _env; }
		}

		void _env_AppChanged(object sender, EventArgs e)
		{
			try
			{
				_sidebar.OnAppChanged();
				RefreshCustomLexers();
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
		}

		public int ProbeLanguageId
		{
			get { return _probeLanguageId; }
			set { _probeLanguageId = value; }
		}
		#endregion

		#region Sidebar
		SidebarForm _sidebar = null;
		IDockWindow _sidebarDock = null;

		private const int k_dockWindowId = 6481891;

		[NppDisplayName("Show Probe Sidebar")]
		[NppShortcut(true, false, true, Keys.F9)]
		public void ShowSidebar()
		{
			try
			{
				if (_sidebarDock != null)
				{
					_sidebarDock.Show();
				}
				else if (_sidebar == null)
				{
					_sidebar = new SidebarForm(this);
					_sidebarDock = DockWindow(_sidebar, "Probe Sidebar", DockWindowAlignment.Left, k_dockWindowId);
					_sidebar.OnSidebarLoad(_currentFile);
				}
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
		}

		[NppDisplayName("Show File List")]
		[NppShortcut(true, false, true, Keys.F11)]
		public void ShowSidebarFileList()
		{
			try
			{
				ShowSidebar();
				if (_sidebar != null) _sidebar.ShowFileList();
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
		}

		[NppDisplayName("Show Function List")]
		[NppShortcut(true, false, true, Keys.F12)]
		public void ShowSidebarFunctionList()
		{
			try
			{
				ShowSidebar();
				if (_sidebar != null) _sidebar.ShowFunctionList();
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
		}
		#endregion

		#region Compile Panel
		CompilePanel _compilePanel = null;
		IDockWindow _compilePanelDock = null;

		private const int k_compilePanelId = 564489;

		[NppDisplayName("Show Probe Compile Panel")]
		public void ShowCompilePanel()
		{
			try
			{
				if (_compilePanelDock != null)
				{
					_compilePanelDock.Show();
				}
				else if (_compilePanel == null)
				{
					_compilePanel = new CompilePanel(this);
					_compilePanelDock = DockWindow(_compilePanel, "Probe Compile", DockWindowAlignment.Bottom, k_compilePanelId);
					_compilePanel.OnPanelLoad();
				}
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
		}

		internal bool CompilePanelIsVisible()
		{
			return _compilePanel != null && _compilePanel.Visible;
		}

		[NppDisplayName("Compile Probe")]
		[NppShortcut(true, false, true, Keys.F7)]
		public void Compile()
		{
			try
			{
				ShowCompilePanel();
				_compilePanel.StartCompile();
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
		}

		[NppDisplayName("Stop Compile")]
		[NppShortcut(true, false, true, Keys.F8)]
		public void StopCompile()
		{
			try
			{
				if (_compilePanel != null) _compilePanel.KillCompile(0);
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
		}

		internal void SaveFilesInApp()
		{
			EditorView currentView = CurrentView;
			int currentFileMain = GetActiveFileIndex(EditorView.Main);
			int currentFileSub = GetActiveFileIndex(EditorView.Sub);

			SaveFilesInApp(EditorView.Main);
			SaveFilesInApp(EditorView.Sub);

			SetActiveFileIndex(EditorView.Main, currentFileMain);
			SetActiveFileIndex(EditorView.Sub, currentFileSub);
		}

		private void SaveFilesInApp(EditorView view)
		{
			int fileIndex = 0;
			foreach (var fileName in GetFileNames(EditorView.Main))
			{
				try
				{
					if (_env.FileExistsInApp(fileName))
					{
						SetActiveFileIndex(view, fileIndex);
						if (Modified)
						{
							if (!SaveFile())
							{
								Output.WriteLine(OutputStyle.Warning, "Could not save file '{0}'.", fileName);
								Output.Show();
							}
						}
					}
				}
				catch (Exception ex)
				{
					Output.WriteLine(OutputStyle.Error, "Exception when saving file '{0}':\r\n{1}", fileName, ex);
					Output.Show();
				}
				fileIndex++;
			}
		}
		#endregion

		#region Settings
		private Settings _settings = null;

		[NppDisplayName("Probe Settings")]
		public void ShowSettings()
		{
			try
			{
				SettingsForm form = new SettingsForm(this);
				form.ShowDialog(_nppWindow);
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
		}

		internal Settings Settings
		{
			get { return _settings; }
		}
		#endregion

		#region Run
		[NppDisplayName("Run Probe")]
		[NppShortcut(true, false, true, Keys.F5)]
		public void OnRun()
		{
			try
			{
				(new RunForm(this)).ShowDialog(_nppWindow);
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
		}
		#endregion

		#region PST
		[NppDisplayName("PST Table")]
		public void PstTable()
		{
			try
			{
				PromptForm dlg = new PromptForm();

				string selected = SelectedText;
				if (!string.IsNullOrEmpty(selected) && _env.IsProbeTable(selected))
				{
					dlg.Value = selected;
				}

				dlg.Text = "PST Table";
				dlg.Prompt = "Enter the name of the table to PST:";
				if (dlg.ShowDialog(_nppWindow) == DialogResult.OK)
				{
					string tableName = dlg.Value;

					ProcessRunner pr = new ProcessRunner();
					StringOutput output = new StringOutput();
					int exitCode = pr.CaptureProcess("pst.exe", tableName, _env.TempDir, output);

					if (exitCode != 0)
					{
						Errors.ShowExtended(_nppWindow, string.Format("PST returned exit code {0}.", exitCode), output.Text);
					}
					else
					{
						string tempFileName = TempManager.GetNewTempFileName(tableName, ".pst");
						File.WriteAllText(tempFileName, output.Text);
						OpenFile(tempFileName);
					}
				}
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
		}
		#endregion

		#region FEC
		[NppDisplayName("FEC File")]
		public void FecFile()
		{
			try
			{
				string baseFileName = Environment.FindBaseFile(ActiveFileName);
				if (string.IsNullOrEmpty(baseFileName))
				{
					MessageBox.Show("Base file could not be found.");
					return;
				}

				string fileName;
				using (TempFileOutput output = new TempFileOutput(
					Path.GetFileNameWithoutExtension(baseFileName) + "_fec",
					Path.GetExtension(baseFileName)))
				{
					ProcessRunner pr = new ProcessRunner();
					int exitCode = pr.CaptureProcess("fec.exe", "/p \"" + baseFileName + "\"",
						Path.GetDirectoryName(baseFileName), output);

					if (exitCode != 0)
					{
						Errors.Show(NppWindow, "FEC returned exit code {0}.", exitCode);
						return;
					}

					fileName = output.FileName;
				}

				OpenFile(fileName);
			}
			catch (Exception ex)
			{
				Errors.Show(NppWindow, ex);
			}
		}
		#endregion

		#region Shortcuts
		[NppDisplayName("Probe Shortcut")]
		[NppShortcut(true, false, false, Keys.Oemcomma)]
		public void ProbeShortcut()
		{
			try
			{
				var form = new ShortcutForm();

				form.AddAction(Keys.L, "FEC File", () => { FecFile(); });
				form.AddAction(Keys.T, "PST Table", () => { PstTable(); });
				form.AddAction(Keys.F, "Show Functions", () => { ShowSidebarFunctionList(); });
				form.AddAction(Keys.O, "Open Files", () => { ShowSidebarFileList(); });
				form.AddAction(Keys.C, "Compile", () => { Compile(); });
				form.AddAction(Keys.H, "Add File Header", () => { AddFileHeader(); });
				form.AddAction(Keys.I, "Set Initials", () => { SetUserInitials(); });
				form.AddAction(Keys.W, "Set Work Order Number", () => { SetWorkOrderNumber(); });
				form.AddAction(Keys.P, "Set Problem Number", () => { SetProblemNumber(); });

				if (form.ShowDialog(NppWindow) == DialogResult.OK)
				{
					var action = form.SelectedAction;
					if (action != null) action();
				}
			}
			catch (Exception ex)
			{
				Errors.Show(NppWindow, ex);
			}
		}
		#endregion

		#region Tagging
		[NppDisplayName("Add File Header")]
		public void AddFileHeader()
		{
			try
			{
				var sb = new StringBuilder();
				sb.AppendLine("// -------------------------------------------------------------------------------------------------");
				sb.Append("// File Name: ");
				sb.AppendLine(Path.GetFileName(ActiveFileName));
				sb.AppendLine("//\t");
				sb.AppendLine("//");
				sb.AppendLine("// Modification History:");
				sb.AppendLine("//\tDate        Who #       Description of Changes");
				sb.AppendLine("//\t----------- --- ------- ------------------------------------------------------------------------");
				sb.Append("//\t");
				sb.Append(DateTime.Now.ToString("ddMMMyyyy"));
				sb.Append("   ");
				sb.Append(Settings.Tagging.Initials.PadRight(4));
				sb.Append(Settings.Tagging.WorkOrderNumber.PadRight(8));

				var prob = Settings.Tagging.ProblemNumber;
				if (!string.IsNullOrWhiteSpace(prob))
				{
					sb.Append(prob);
					sb.Append(" Created");
				}
				else
				{
					sb.Append("Created");
				}
				sb.AppendLine();
				sb.AppendLine("// -------------------------------------------------------------------------------------------------");
				sb.AppendLine();

				GoToPos(0);
				Insert(sb.ToString());
				GoToPos(GetLineEndPos(3));	// To enter file description.
			}
			catch (Exception ex)
			{
				Errors.Show(NppWindow, ex);
			}
		}

		[NppDisplayName("Set User Initials")]
		public void SetUserInitials()
		{
			try
			{
				var form = new PromptForm();
				form.Value = Settings.Tagging.Initials;
				form.AllowEmpty = true;
				form.Prompt = "Enter your initials:";
				if (form.ShowDialog(NppWindow) == DialogResult.OK)
				{
					Settings.Tagging.Initials = form.Value;
				}
			}
			catch (Exception ex)
			{
				Errors.Show(NppWindow, ex);
			}
		}

		[NppDisplayName("Set Work Order Number")]
		public void SetWorkOrderNumber()
		{
			try
			{
				var form = new PromptForm();
				form.Value = Settings.Tagging.WorkOrderNumber;
				form.AllowEmpty = true;
				form.Prompt = "Enter the work order number you are working on:";
				if (form.ShowDialog(NppWindow) == DialogResult.OK)
				{
					Settings.Tagging.WorkOrderNumber = form.Value;
				}
			}
			catch (Exception ex)
			{
				Errors.Show(NppWindow, ex);
			}
		}

		[NppDisplayName("Set Problem Number")]
		public void SetProblemNumber()
		{
			try
			{
				var form = new PromptForm();
				form.Value = Settings.Tagging.ProblemNumber;
				form.AllowEmpty = true;
				form.Prompt = "Enter the problem number you are working on:";
				if (form.ShowDialog(NppWindow) == DialogResult.OK)
				{
					Settings.Tagging.ProblemNumber = form.Value;
				}
			}
			catch (Exception ex)
			{
				Errors.Show(NppWindow, ex);
			}
		}
		#endregion

	}
}
