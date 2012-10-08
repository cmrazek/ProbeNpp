using System;
using System.Collections.Generic;
using System.Text;

namespace ProbeNpp
{
	public partial class Settings
	{
		public CompileGroup Compile = new CompileGroup();
		public class CompileGroup : SettingsGroup
		{
			public bool ClosePanelAfterSuccess = true;
			public bool ClosePanelAfterWarnings = true;
		}

		public ProbeGroup Probe = new ProbeGroup();
		public class ProbeGroup : SettingsGroup
		{
			public string SourceExtensions = "ct ct& f f& fec gp gp& i i& ic id ie il il& sp sp& st st&";
			public string DictExtensions = "pst t t&";
		}

		public RunSamCamGroup RunSamCam = new RunSamCamGroup();
		public class RunSamCamGroup : SettingsGroup
		{
			public RunForm.RunApp App = RunForm.RunApp.SamAndCam;
			public bool Diags = false;
			public bool LoadSam = false;
			public int LoadSamTime = 10000;
			public bool SetDbDate = true;
			public int TransReportTimeout = 10;
			public int TransAbortTimeout = 20;
			public int MinChannels = 1;
			public int MaxChannels = 2;
			public int CamWidth = 85;
		}

		public TaggingGroup Tagging = new TaggingGroup();
		public class TaggingGroup : SettingsGroup
		{
			public string Initials = "";
			public string WorkOrderNumber = "";
			public string ProblemNumber = "";

			public bool InitialsInDiags = true;
			public bool FileNameInDiags = true;
			public bool TodoAfterDiags = false;

			public bool TagDate = false;
			public bool MultiLineTagsOnSeparateLines = true;
		}

		public FileListViewGroup FileListView = new FileListViewGroup();
		public class FileListViewGroup : SettingsGroup
		{
			public int FileColumnWidth = 0;
			public int DirColumnWidth = 0;
		}

		public FunctionListViewGroup FunctionListView = new FunctionListViewGroup();
		public class FunctionListViewGroup : SettingsGroup
		{
			public int FunctionColumnWidth = 0;
		}

		public SidebarGroup Sidebar = new SidebarGroup();
		public class SidebarGroup : SettingsGroup
		{
			public bool ShowOnStartup = true;
		}

		public FindInProbeFilesGroup FindInProbeFiles = new FindInProbeFilesGroup();
		public class FindInProbeFilesGroup : SettingsGroup
		{
			public string MRU = string.Empty;
			public FindInProbeFilesMethod Method = FindInProbeFilesMethod.Normal;
			public bool MatchCase = false;
			public bool MatchWholeWord = false;
			public bool OnlyProbeFiles = true;

			public int FileNameColumnWidth = 0;
			public int LineNumberColumnWidth = 0;
			public int LineTextColumnWidth = 0;
		}
	}
}
