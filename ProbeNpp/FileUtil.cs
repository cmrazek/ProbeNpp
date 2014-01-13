using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProbeNpp
{
	internal static class FileUtil
	{
		public static void CreateDirectoryRecursive(string path)
		{
			if (Directory.Exists(path)) return;
			CheckSubDirCreated(path);
			Directory.CreateDirectory(path);
		}

		private static void CheckSubDirCreated(string path)
		{
			var parent = Path.GetDirectoryName(path);
			if (parent == null) throw new DirectoryNotFoundException(string.Format("Unable to determine parent directory of path '{0}'.", path));
			if (!Directory.Exists(parent))
			{
				CheckSubDirCreated(parent);
				Directory.CreateDirectory(parent);
			}
		}
	}
}
