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
			public string Extensions = "ct ct& f f& fec i i& il il& pst sp sp& st st&";
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
		}
	}
}
