using System;
using System.Collections.Generic;
using System.IO;

public static class SdDirectoryInfoExtensions
{
	public static bool IsDirEmpty(this SdDirectoryInfo possiblyEmptyDir)
	{
		bool flag;
		using (IEnumerator<SdFileSystemInfo> enumerator = possiblyEmptyDir.EnumerateFileSystemInfos("*", SearchOption.AllDirectories).GetEnumerator())
		{
			flag = !enumerator.MoveNext();
		}
		return flag;
	}
}
