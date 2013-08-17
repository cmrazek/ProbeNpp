using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NppSharp;

namespace ProbeNpp
{
	internal static class Log
	{
		public static void WriteError(string message)
		{
			ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, message);
		}

		public static void WriteError(string format, params object[] args)
		{
			ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, string.Format(format, args));
		}

		public static void WriteError(Exception ex)
		{
			ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, ex.ToString());
		}

		public static void WriteError(Exception ex, string message)
		{
			ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, string.Concat(message, "\r\n", ex));
		}

		public static void WriteError(Exception ex, string format, params object[] args)
		{
			ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.Error, string.Concat(string.Format(format, args), "\r\n", ex));
		}

		public static void WriteDiag(string message)
		{
			ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.NotImportant, message);
		}

		public static void WriteDiag(string format, params object[] args)
		{
			ProbeNppPlugin.Instance.Output.WriteLine(OutputStyle.NotImportant, string.Format(format, args));
		}
	}
}
