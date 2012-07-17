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
			public string SourceExtensions = "ct ct& f f& fec i i& il il& sp sp& st st&";
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

			public int TagStartColumn = 0;
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
	}
}
