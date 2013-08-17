using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using NppSharp;

namespace ProbeNpp
{
	public static class Errors
	{
		public static void Show(IWin32Window parentWindow, Exception ex)
		{
			try
			{
				using (ErrorForm form = new ErrorForm())
				{
					form.Message = ex.Message;
					form.Details = ex.ToString();
					form.ShowDialog(parentWindow);
				}
			}
			catch (Exception)
			{ }
		}

		public static void Show(IWin32Window parentWindow, Exception ex, string message)
		{
			try
			{
				using (ErrorForm form = new ErrorForm())
				{
					form.Message = message;
					form.Details = ex.ToString();
					form.ShowDialog(parentWindow);
				}
			}
			catch (Exception)
			{ }
		}

		public static void Show(IWin32Window parentWindow, string message)
		{
			try
			{
				using (ErrorForm form = new ErrorForm())
				{
					form.Message = message;
					form.ShowDialog(parentWindow);
				}
			}
			catch (Exception)
			{ }
		}

		public static void Show(IWin32Window parentWindow, string format, params object[] args)
		{
			try
			{
				using (ErrorForm form = new ErrorForm())
				{
					form.Message = string.Format(format, args);
					form.ShowDialog(parentWindow);
				}
			}
			catch (Exception)
			{ }
		}

		public static void ShowExtended(IWin32Window parentWindow, string message, string details)
		{
			try
			{
				using (ErrorForm form = new ErrorForm())
				{
					form.Message = message;
					form.Details = details;
					form.ShowDialog(parentWindow);
				}
			}
			catch (Exception)
			{ }
		}
	}
}
