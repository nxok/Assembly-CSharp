using System;
using System.Collections.Generic;

namespace MusicUtils
{
	public static class FileCleanupUtils
	{
		public static void CleanUpAllWaveFiles()
		{
			for (int i = 0; i < FileCleanupUtils.paths.Count; i++)
			{
				FileCleanupUtils.CleanUpWaveFile(FileCleanupUtils.paths[i]);
			}
		}

		public static void CleanUpWaveFile(string file)
		{
			WaveCleanUp.Create().GetComponent<WaveCleanUp>().FilePath = file;
		}

		public static List<string> paths = new List<string>();
	}
}
