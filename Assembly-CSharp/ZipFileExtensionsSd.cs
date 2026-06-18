using System;
using System.IO;
using System.IO.Compression;

public static class ZipFileExtensionsSd
{
	public static void CreateFromDirectory(this ZipArchive archiveZip, string saveDir)
	{
		Log.Out("[BACKTRACE] CreateFromDirectory Path: " + saveDir);
		archiveZip.CreateFromDirectory(saveDir, CompressionLevel.Optimal, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateFromDirectory(this ZipArchive destination, string sourceDirectoryName, CompressionLevel compressionLevel, bool includeBaseDirectory)
	{
		bool flag = true;
		SdDirectoryInfo sdDirectoryInfo = new SdDirectoryInfo(sourceDirectoryName);
		string text = sdDirectoryInfo.FullName;
		if (includeBaseDirectory && sdDirectoryInfo.Parent != null)
		{
			text = sdDirectoryInfo.Parent.FullName;
		}
		Log.Out("[BACKTRACE] Checking Path: " + text);
		foreach (SdFileSystemInfo sdFileSystemInfo in sdDirectoryInfo.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
		{
			Log.Out("[BACKTRACE] Adding Path: " + sdFileSystemInfo.FullName);
			flag = false;
			int num = sdFileSystemInfo.FullName.Length - text.Length;
			string text2 = ZipFileExtensionsSd.EntryFromPath(sdFileSystemInfo.FullName, text.Length, num);
			if (sdFileSystemInfo is SdFileInfo)
			{
				destination.CreateEntryFromFile(sdFileSystemInfo, text2, compressionLevel);
			}
			else
			{
				SdDirectoryInfo sdDirectoryInfo2 = sdFileSystemInfo as SdDirectoryInfo;
				if (sdDirectoryInfo2 != null && sdDirectoryInfo2.IsDirEmpty())
				{
					destination.CreateEntry(text2 + "/");
				}
			}
		}
		if (!includeBaseDirectory || !flag)
		{
			return;
		}
		string text3 = ZipFileExtensionsSd.EntryFromPath(sdDirectoryInfo.Name, 0, sdDirectoryInfo.Name.Length);
		destination.CreateEntry(text3 + "/");
	}

	public static ZipArchiveEntry CreateEntryFromFile(this ZipArchive destination, SdFileSystemInfo directoryInfo, string entryName, CompressionLevel compressionLevel)
	{
		string fullName = directoryInfo.FullName;
		if (destination == null)
		{
			throw new ArgumentNullException("destination");
		}
		if (fullName == null)
		{
			throw new ArgumentNullException("sourceFileName");
		}
		if (entryName == null)
		{
			throw new ArgumentNullException("entryName");
		}
		ZipArchiveEntry zipArchiveEntry2;
		using (Stream stream = SdFile.Open(fullName, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			ZipArchiveEntry zipArchiveEntry = destination.CreateEntry(entryName, compressionLevel);
			DateTime lastWriteTime = SdFile.GetLastWriteTime(fullName);
			if (lastWriteTime.Year < 1980 || lastWriteTime.Year > 2107)
			{
				lastWriteTime = new DateTime(1980, 1, 1, 0, 0, 0);
			}
			zipArchiveEntry.LastWriteTime = lastWriteTime;
			using (Stream stream2 = zipArchiveEntry.Open())
			{
				stream.CopyTo(stream2);
			}
			zipArchiveEntry2 = zipArchiveEntry;
		}
		return zipArchiveEntry2;
	}

	public static void AddSearchPattern(this ZipArchive destination, SdDirectoryInfo directoryInfo, string pattern, SearchOption options)
	{
		string fullName = directoryInfo.FullName;
		Log.Out("[BACKTRACE] Directory full name: " + directoryInfo.FullName + ", Pattern: " + pattern);
		foreach (SdFileSystemInfo sdFileSystemInfo in directoryInfo.EnumerateFileSystemInfos(pattern, options))
		{
			Log.Out("[BACKTRACE] Found full name: " + sdFileSystemInfo.FullName);
			int num = sdFileSystemInfo.FullName.Length - fullName.Length;
			string text = ZipFileExtensionsSd.EntryFromPath(sdFileSystemInfo.FullName, fullName.Length, num);
			if (sdFileSystemInfo is SdFileInfo)
			{
				destination.CreateEntryFromFile(sdFileSystemInfo, text, CompressionLevel.Optimal);
			}
			else
			{
				SdDirectoryInfo sdDirectoryInfo = sdFileSystemInfo as SdDirectoryInfo;
				if (sdDirectoryInfo != null && sdDirectoryInfo.IsDirEmpty())
				{
					destination.CreateEntry(text + Path.DirectorySeparatorChar.ToString(), CompressionLevel.Optimal);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string EntryFromPath(string entry, int offset, int length)
	{
		while (length > 0 && (entry[offset] == Path.DirectorySeparatorChar || entry[offset] == Path.AltDirectorySeparatorChar))
		{
			offset++;
			length--;
		}
		if (length == 0)
		{
			return string.Empty;
		}
		char[] array = entry.ToCharArray(offset, length);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == Path.DirectorySeparatorChar || array[i] == Path.AltDirectorySeparatorChar)
			{
				array[i] = Path.DirectorySeparatorChar;
			}
		}
		return new string(array);
	}
}
