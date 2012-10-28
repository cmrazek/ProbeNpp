﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;
using System.Drawing;
#if DOTNET4
using System.Linq;
#endif
using NppSharp;

// 10	Add File Header				(toolbar icon)
// 20	Tag Change					(toolbar icon)
// 30	Insert Date					(toolbar icon)
// 40	Insert Diag					(toolbar icon)
//		--------------------------
// 50	FEC File
// 55	FEC to Visual C
// 60	PST Table
// 65	Merge File
// 70	Show File List
// 75	Show Function List
// 80	Show Sidebar
//		--------------------------
// 85	Find in Probe Files
//		--------------------------
// 90	Compile
// 100	Stop Compile
// 110	Show Compile Panel
//		--------------------------
// 120	Run
//		--------------------------
// 130	Probe Shortcut
// 140	Probe Settings

namespace ProbeNpp
{
	[NppSortOrder(100)]
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

			CharAdded += new CharAddedEventHandler(ProbeNppPlugin_CharAdded);
		}

		private void Plugin_Ready(object sender, EventArgs e)
		{
			try
			{
				if (Settings.Sidebar.ShowOnStartup) ShowSidebar();
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
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

		private Bitmap IconToBitmap(Icon icon)
		{
			var bmp = new Bitmap(icon.Width, icon.Height);
			using (var g = Graphics.FromImage(bmp))
			{
				g.Clear(Color.Transparent);
				g.DrawIcon(icon, new Rectangle(0, 0, icon.Width, icon.Height));
			}
			return bmp;
		}
		#endregion

		#region Probe Integration
		private ProbeEnvironment _env = null;
		private int _probeLanguageId = 0;

		internal ProbeEnvironment Environment
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

		[NppDisplayName("Show Sidebar")]
		[NppShortcut(true, false, true, Keys.F9)]
		[NppSortOrder(80)]
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
		[NppSortOrder(70)]
		public void ShowSidebarFileList()
		{
			try
			{
				ShowSidebar();
				if (_sidebar != null) _sidebar.ShowFileList(SelectedText);
			}
			catch (Exception ex)
			{
				Errors.Show(_nppWindow, ex);
			}
		}

		[NppDisplayName("Show Function List")]
		[NppShortcut(true, false, true, Keys.F12)]
		[NppSortOrder(75)]
		public void ShowSidebarFunctionList()
		{
			try
			{
				ShowSidebar();
				if (_sidebar != null) _sidebar.ShowFunctionList(SelectedText);
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

		[NppDisplayName("Show Compile Panel")]
		[NppShortcut(true, true, true, Keys.F7)]
		[NppSortOrder(110)]
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

		internal void HideCompilePanel()
		{
			if (_compilePanelDock != null) _compilePanelDock.Hide();
		}

		[NppDisplayName("Compile")]
		[NppShortcut(true, false, true, Keys.F7)]
		[NppSortOrder(90)]
		[NppSeparator]
		[NppToolbarIcon(Property = "CompileIcon")]
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

		public Bitmap CompileIcon
		{
			get { return IconToBitmap(Res.CompileIcon); }
		}

		[NppDisplayName("Stop Compile")]
		[NppShortcut(true, false, true, Keys.F8)]
		[NppSortOrder(100)]
		[NppToolbarIcon(Property = "StopCompileIcon")]
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

		public Bitmap StopCompileIcon
		{
			get { return IconToBitmap(Res.StopCompileIcon); }
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
		[NppSortOrder(140)]
		[NppToolbarIcon(Property = "ShowSettingsIcon")]
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

		public Bitmap ShowSettingsIcon
		{
			get { return IconToBitmap(Res.SettingsIcon); }
		}

		internal Settings Settings
		{
			get { return _settings; }
		}

		internal void OnSettingsSaved()
		{
			_env.OnSettingsSaved();
		}
		#endregion

		#region Run
		[NppDisplayName("Run SAM/CAM")]
		[NppShortcut(true, false, true, Keys.F5)]
		[NppSortOrder(120)]
		[NppSeparator]
		[NppToolbarIcon(Property = "RunIcon")]
		public void Run()
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

		public Bitmap RunIcon
		{
			get { return IconToBitmap(Res.RunIcon); }
		}
		#endregion

		#region PST
		[NppDisplayName("PST Table")]
		[NppSortOrder(60)]
		[NppToolbarIcon(Property = "PstTableIcon")]
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

		public Bitmap PstTableIcon
		{
			get { return IconToBitmap(Res.PstIcon); }
		}
		#endregion

		#region FEC
		[NppDisplayName("FEC File")]
		[NppSortOrder(50)]
		[NppSeparator]
		[NppToolbarIcon(Property = "FecFileIcon")]
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

		public Bitmap FecFileIcon
		{
			get { return IconToBitmap(Res.FecIcon); }
		}

		[NppDisplayName("FEC to Visual C")]
		[NppSortOrder(55)]
		public void CompileToVisualC()
		{
			try
			{
				string baseFileName = Environment.FindBaseFile(ActiveFileName);
				if (string.IsNullOrEmpty(baseFileName))
				{
					MessageBox.Show("Base file could not be found.");
					return;
				}

				var pr = new ProcessRunner();
				int exitCode = pr.ExecuteProcess("fec.exe", string.Concat("\"", baseFileName, "\""),
					Path.GetDirectoryName(baseFileName), true);

				if (exitCode != 0)
				{
					Errors.Show(NppWindow, "FEC returned exit code {0}.", exitCode);
					return;
				}

				var cFileName = Path.Combine(Path.GetDirectoryName(baseFileName),
					string.Concat(Path.GetFileNameWithoutExtension(baseFileName), ".c"));
				if (!File.Exists(cFileName))
				{
					Errors.Show(NppWindow, "Unable to find .c file produced by FEC.");
					return;
				}

				OpenFile(cFileName);
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
		[NppSortOrder(130)]
		[NppSeparator]
		public void ProbeShortcut()
		{
			try
			{
				var form = new ShortcutForm();

				form.AddAction(Keys.L, "FEC File", () => { FecFile(); });
				form.AddAction(Keys.M, "Merge File", () => { MergeFile(); });
				form.AddAction(Keys.T, "PST Table", () => { PstTable(); });
				form.AddAction(Keys.F, "Show Functions", () => { ShowSidebarFunctionList(); });
				form.AddAction(Keys.O, "Open Files", () => { ShowSidebarFileList(); });
				form.AddAction(Keys.I, "Find in Probe Files", () => { FindInProbeFiles(); });
				form.AddAction(Keys.C, "Compile", () => { Compile(); });
				form.AddAction(Keys.H, "Add File Header", () => { AddFileHeader(); });
				form.AddAction(Keys.D, "Insert Diag", () => { InsertDiag(); });
				form.AddAction(Keys.S, "Settings", () => { ShowSettings(); });

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
		[NppSortOrder(10)]
		[NppToolbarIcon(Property = "AddFileHeaderIcon")]
		public void AddFileHeader()
		{
			try
			{
				GoTo(Start);
				Insert(CreateFileHeaderText(ActiveFileName));
				GoTo(GetLineEndPos(3));	// To enter file description.
			}
			catch (Exception ex)
			{
				Errors.Show(NppWindow, ex);
			}
		}

		public Bitmap AddFileHeaderIcon
		{
			get { return IconToBitmap(Res.AddFileHeaderIcon); }
		}

		internal string CreateFileHeaderText(string fileName)
		{
			var sb = new StringBuilder();
			sb.AppendLine("// -------------------------------------------------------------------------------------------------");
			sb.Append("// File Name: ");
			sb.AppendLine(Path.GetFileName(fileName));
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
#if DOTNET4
			if (!string.IsNullOrWhiteSpace(prob))
#else
			if (!StringUtil.IsNullOrWhiteSpace(prob))
#endif
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

			return sb.ToString();
		}

		[NppDisplayName("Insert &Diag")]
		[NppShortcut(true, false, true, Keys.D)]
		[NppSortOrder(40)]
		[NppToolbarIcon(Property = "InsertDiagIcon")]
		public void InsertDiag()
		{
			var selText = SelectedText.Trim();
			if (selText.IndexOf('\n') >= 0) selText = string.Empty;

			var sb = new StringBuilder();
			sb.Append("diag(\"");
			if (Settings.Tagging.InitialsInDiags &&
#if DOTNET4
				!string.IsNullOrWhiteSpace(Settings.Tagging.Initials)
#else
				!StringUtil.IsNullOrWhiteSpace(Settings.Tagging.Initials)
#endif
				)
			{
				sb.Append(Settings.Tagging.Initials);
				sb.Append(": ");
			}

			if (Settings.Tagging.FileNameInDiags)
			{
				sb.Append(Path.GetFileName(ActiveFileName));
				sb.Append(": ");
			}

#if DOTNET4
			if (!string.IsNullOrWhiteSpace(selText))
#else
			if (!StringUtil.IsNullOrWhiteSpace(selText))
#endif
			{
				sb.Append(ProbeEnvironment.StringEscape(selText));
				sb.Append(" [\", ");
				sb.Append(selText);
				sb.Append(", \"]");
			}

			int lengthBefore = sb.Length;

			sb.Append("\\n\");");
			if (Settings.Tagging.TodoAfterDiags) sb.Append("\t// TODO");

			Insert(sb.ToString());

#if DOTNET4
			if (string.IsNullOrWhiteSpace(selText))
#else
			if (StringUtil.IsNullOrWhiteSpace(selText))
#endif
			{
				GoTo(CurrentLocation - (sb.Length - lengthBefore));
			}
		}

		public Bitmap InsertDiagIcon
		{
			get { return IconToBitmap(Res.DiagIcon); }
		}

		[NppDisplayName("&Tag Change")]
		[NppShortcut(true, false, true, Keys.T)]
		[NppSortOrder(20)]
		[NppToolbarIcon(Property = "TagChangeIcon")]
		public void TagChange()
		{
			var selStart = SelectionStart < SelectionEnd ? SelectionStart : SelectionEnd;
			var selEnd = SelectionStart < SelectionEnd ? SelectionEnd : SelectionStart;
			var startLine = selStart.Line;
			var endLine = selEnd.Line;

			var sb = new StringBuilder();
#if DOTNET4
			if (!string.IsNullOrWhiteSpace(Settings.Tagging.Initials))
#else
			if (!StringUtil.IsNullOrWhiteSpace(Settings.Tagging.Initials))
#endif
			{
				sb.Append(Settings.Tagging.Initials);
			}

			if (Settings.Tagging.TagDate)
			{
				if (sb.Length > 0) sb.Append(" ");
				sb.Append(DateTime.Now.ToString("ddMMMyyyy"));
			}

#if DOTNET4
			if (!string.IsNullOrWhiteSpace(Settings.Tagging.WorkOrderNumber))
#else
			if (!StringUtil.IsNullOrWhiteSpace(Settings.Tagging.WorkOrderNumber))
#endif
			{
				if (sb.Length > 0) sb.Append(" ");
				sb.Append(Settings.Tagging.WorkOrderNumber);
			}

#if DOTNET4
			if (!string.IsNullOrWhiteSpace(Settings.Tagging.ProblemNumber))
#else
			if (!StringUtil.IsNullOrWhiteSpace(Settings.Tagging.ProblemNumber))
#endif
			{
				if (sb.Length > 0) sb.Append(" ");
				sb.Append(Settings.Tagging.ProblemNumber);
			}

			if (startLine == endLine)
			{
				// Single line change.
				ApplyTagChangeToLine(startLine, sb.ToString(), TagChangeLine.Single);
			}
			else
			{
				// Multi-line change.
				ApplyTagChangeToLine(endLine, string.Concat(sb, " End"), TagChangeLine.End);
				ApplyTagChangeToLine(startLine, string.Concat(sb, " Start"), TagChangeLine.Start);
			}
		}

		public Bitmap TagChangeIcon
		{
			get { return IconToBitmap(Res.TagIcon); }
		}

		private enum TagChangeLine
		{
			Single,
			Start,
			End
		}

		private void ApplyTagChangeToLine(int line, string str, TagChangeLine tcl)
		{
			if (tcl == TagChangeLine.Start && Settings.Tagging.MultiLineTagsOnSeparateLines)
			{
				var indentPos = GetIndentPosOnLine(line);
				var lineStartPos = GetLineStartPos(line);
				var indent = GetText(lineStartPos, indentPos.CharPosition - lineStartPos.CharPosition);

				GoTo(indentPos);
				Insert(string.Concat("\r\n", indent));
				GoTo(indentPos);
				Insert(string.Concat("// ", str));
			}
			else if (tcl == TagChangeLine.End && Settings.Tagging.MultiLineTagsOnSeparateLines)
			{
				var indentPos = GetIndentPosOnLine(line);
				var lineStartPos = GetLineStartPos(line);
				var indent = GetText(lineStartPos, indentPos.CharPosition - lineStartPos.CharPosition);

				var lineEndPos = GetLineEndPos(line);
				GoTo(lineEndPos);
				Insert(string.Concat("\r\n", indent));
				Insert(string.Concat("// ", str));
			}
			else
			{
				GoTo(GetLineEndPos(line));
				Insert(string.Concat("\t// ", str));
			}
		}

		private TextLocation GetIndentPosOnLine(int line)
		{
		    var pos = new TextLocation(line, 1);
			while (true)
			{
				var ch = GetText(pos, 1);
				if (string.IsNullOrEmpty(ch)) break;
				if (!Char.IsWhiteSpace(ch[0])) return pos;
				pos++;
			}
			return new TextLocation(line, 1);
		}

		[NppDisplayName("Insert Date")]
		[NppShortcut(true, false, true, Keys.Y)]
		[NppSortOrder(30)]
		[NppToolbarIcon(Property = "InsertDateIcon")]
		public void InsertDate()
		{
			Insert(DateTime.Now.ToString("ddMMMyyyy"));
		}

		public Bitmap InsertDateIcon
		{
			get { return IconToBitmap(Res.DateIcon); }
		}
		#endregion

		#region AutoCompletion
		void ProbeNppPlugin_CharAdded(object sender, CharAddedEventArgs e)
		{
			if (e.Character == '.' && LanguageName == ProbeSourceLexer.Name)
			{
				CheckAutoCompletion();
			}
		}

		private void CheckAutoCompletion()
		{
			var wordEnd = CurrentLocation - 1;
			var wordStart = GetWordStartPos(wordEnd, false);
			var word = GetText(wordStart, wordEnd);

#if DOTNET4
			if (!string.IsNullOrWhiteSpace(word))
#else
			if (!StringUtil.IsNullOrWhiteSpace(word))
#endif
			{
				var table = _env.GetTable(word);
				if (table == null) return;

#if DOTNET4
				var fields = table.Fields;
				if (!fields.Any()) return;
				ShowAutoCompletion(0, (from f in fields orderby f.Name.ToLower() select f.Name), true);
#else
				var fields = new List<string>();
				foreach (var field in table.Fields)
				{
					fields.Add(field.Name);
				}
				fields.Sort(new StringComparerIgnoreCase());
				ShowAutoCompletion(0, fields, true);
#endif
			}
		}
		#endregion

		#region Merge File
		[NppDisplayName("Merge File")]
		[NppSortOrder(65)]
		[NppToolbarIcon(Property = "MergeFileIcon")]
		public void MergeFile()
		{
			try
			{
				var fileName = ActiveFileName;

				var cp = new CodeProcessing.CodeProcessor(_env);
				cp.ShowMergeComments = true;
				cp.ProcessFile(fileName);

				string tempFileName = string.Empty;
				using (var tempFileOutput = new TempFileOutput(string.Concat(Path.GetFileNameWithoutExtension(fileName), "_merge"),
					Path.GetExtension(fileName)))
				{
					var errors = cp.Errors;
#if DOTNET4
					if (errors.Any())
					{
						tempFileOutput.WriteLine("// Errors encountered during processing:");
						foreach (var error in errors)
						{
							if (error.Line != null && error.Line.File != null) tempFileOutput.WriteLine(string.Format("// {0}({1}): {2}", error.Line.FileName, error.Line.LineNum, error.Message));
							else tempFileOutput.WriteLine(error.Message);
						}
						tempFileOutput.WriteLine(string.Empty);
					}
#else
					var firstError = true;
					foreach (var error in errors)
					{
						if (firstError)
						{
							tempFileOutput.WriteLine("// Errors encountered during processing:");
							firstError = false;
						}

						if (error.Line != null && error.Line.File != null) tempFileOutput.WriteLine(string.Format("// {0}({1}): {2}", error.Line.FileName, error.Line.LineNum, error.Message));
						else tempFileOutput.WriteLine(error.Message);
					}
					if (!firstError)
					{
						tempFileOutput.WriteLine(string.Empty);
					}
#endif

					foreach (var line in cp.Lines)
					{
						//if (line.File != null) tempFileOutput.WriteLine(string.Format("/*{0}({1})*/ {2}", line.FileName, line.LineNum, line.Text));
						//else
						tempFileOutput.WriteLine(line.Text);
					}

					tempFileName = tempFileOutput.FileName;
				}

				OpenFile(tempFileName);
			}
			catch (Exception ex)
			{
				Errors.Show(NppWindow, ex);
			}
		}

		public Bitmap MergeFileIcon
		{
			get { return IconToBitmap(Res.MergeIcon); }
		}
		#endregion

		#region Find in Probe Files
		FindInProbeFilesPanel _findInProbeFilesPanel = null;
		IDockWindow _findInProbeFilesDock = null;
		FindInProbeFilesThread _findInProbeFilesThread = null;

		private const int k_findInProbeFilesPanelId = 14965;

		[NppDisplayName("Find in Probe Files")]
		[NppSortOrder(85)]
		[NppSeparator]
		[NppToolbarIcon(Property = "FindInProbeFilesIcon")]
		public void FindInProbeFiles()
		{
			try
			{
				var form = new FindInProbeFilesDialog();

				string selected = SelectedText;
				if (!string.IsNullOrEmpty(selected) && !selected.Contains('\n'))
				{
					form.SearchText = selected;
				}

				if (form.ShowDialog(NppWindow) == DialogResult.OK)
				{
					if (_findInProbeFilesDock != null)
					{
						_findInProbeFilesDock.Show();
					}
					else
					{
						_findInProbeFilesPanel = new FindInProbeFilesPanel();
						_findInProbeFilesDock = DockWindow(_findInProbeFilesPanel, "Find in Probe Files", DockWindowAlignment.Bottom, k_findInProbeFilesPanelId);
					}

					if (_findInProbeFilesThread != null) _findInProbeFilesThread.Kill();

					_findInProbeFilesThread = new FindInProbeFilesThread();
					_findInProbeFilesThread.Search(new FindInProbeFilesArgs
					{
						SearchText = form.SearchText,
						SearchRegex = form.SearchRegex,
						Method = form.Method,
						MatchCase = form.MatchCase,
						MatchWholeWord = form.MatchWholeWord,
						ProbeFilesOnly = form.OnlyProbeFiles,
						Panel = _findInProbeFilesPanel,
						Probe = Environment
					});
				}
			}
			catch (Exception ex)
			{
				Errors.Show(NppWindow, ex);
			}
		}

		public Bitmap FindInProbeFilesIcon
		{
			get { return IconToBitmap(Res.FindIcon); }
		}
		#endregion
	}
}
