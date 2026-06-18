using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Platform;

public class SaveDataMergedPlatformSaveGameIOProvider : IPlatformSaveGameProvider, IPlatformSaveGameIOProvider
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static void LogInfo(string text)
	{
		Log.Out("[SaveDataMergedPlatformSaveGameIOProvider] " + text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Regex CreateCachedManagedPathRegex()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("^(?:");
		stringBuilder.Append("(?:");
		stringBuilder.Append("(?:");
		stringBuilder.Append("(?:");
		stringBuilder.Append("Saves");
		stringBuilder.Append("[/]");
		stringBuilder.Append("[^/]+");
		stringBuilder.Append("[/]");
		stringBuilder.Append("[^/]+");
		stringBuilder.Append(')');
		stringBuilder.Append('|');
		stringBuilder.Append("(?:");
		stringBuilder.Append("SavesLocal");
		stringBuilder.Append("[/]");
		stringBuilder.Append("[^/]+");
		stringBuilder.Append(')');
		stringBuilder.Append(')');
		stringBuilder.Append("[/]");
		stringBuilder.Append("DynamicMeshes");
		stringBuilder.Append(')');
		stringBuilder.Append('|');
		stringBuilder.Append(Regex.Escape("CacheOwner.txt"));
		stringBuilder.Append('|');
		stringBuilder.Append(Regex.Escape("GeneratedWorlds"));
		stringBuilder.Append('/');
		stringBuilder.Append("[^/]+?");
		stringBuilder.Append('/');
		stringBuilder.Append("(?:");
		stringBuilder.Append(Regex.Escape("checksums.txt"));
		stringBuilder.Append('|');
		stringBuilder.Append("[^/]+?(?:");
		stringBuilder.Append(Regex.Escape("_processed"));
		stringBuilder.Append('|');
		stringBuilder.Append(Regex.Escape("_half"));
		stringBuilder.Append(")\\.(?:");
		stringBuilder.Append(Regex.Escape("raw"));
		stringBuilder.Append('|');
		stringBuilder.Append(Regex.Escape("png"));
		stringBuilder.Append('|');
		stringBuilder.Append(Regex.Escape("tga"));
		stringBuilder.Append(')');
		stringBuilder.Append(')');
		stringBuilder.Append(')');
		stringBuilder.Append("(?:");
		stringBuilder.Append(Regex.Escape(".bup"));
		stringBuilder.Append(")*");
		stringBuilder.Append("(?:/|$)");
		return new Regex(stringBuilder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);
	}

	public SaveDataMergedPlatformSaveGameIOProvider(IPlatformSaveGameProvider main)
	{
		this.m_main = main;
		this.m_cache = main.Cache;
		try
		{
			this.InitialSaveCacheSync();
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsCached(SaveDataManagedPath path)
	{
		return this.m_cache != null && SaveDataMergedPlatformSaveGameIOProvider.s_cachedManagedPathRegex.IsMatch(path.PathRelativeToRoot);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IPlatformSaveGameIOProvider GetIOProvider(SaveDataManagedPath path)
	{
		if (!this.IsCached(path))
		{
			return this.m_main;
		}
		return this.m_cache;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SyncParentDirectories(SaveDataManagedPath path)
	{
		if (!this.IsCached(path))
		{
			return;
		}
		if (this.m_cache.ManagedFileExists(path) || this.m_cache.ManagedDirectoryExists(path))
		{
			return;
		}
		SaveDataManagedPath saveDataManagedPath;
		if (!path.TryGetParentPath(out saveDataManagedPath) || saveDataManagedPath.PathRelativeToRoot.Length <= 0)
		{
			return;
		}
		if (this.m_cache.ManagedDirectoryExists(saveDataManagedPath))
		{
			return;
		}
		if (!this.m_main.ManagedDirectoryExists(saveDataManagedPath))
		{
			return;
		}
		this.m_cache.ManagedDirectoryCreateDirectory(saveDataManagedPath);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitialSaveCacheSync()
	{
		if (this.m_cache == null)
		{
			return;
		}
		using (MemoryStream memoryStream = new MemoryStream())
		{
			this.InitEnsureSameCacheUser(memoryStream);
			this.InitMoveToCache(memoryStream);
			this.InitRemoveInvalidCachedFiles();
			this.InitRemoveCachedGeneratedWorlds();
			this.InitRemoveEmptyCacheDirectories();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitEnsureSameCacheUser(MemoryStream buffer)
	{
		SaveDataMergedPlatformSaveGameIOProvider.<>c__DisplayClass12_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.buffer = buffer;
		CS$<>8__locals1.cacheOwnerPath = new SaveDataManagedPath("CacheOwner.txt");
		CS$<>8__locals1.saveOwnerPath = new SaveDataManagedPath("SaveOwner.txt");
		if (!this.m_main.ManagedFileExists(CS$<>8__locals1.saveOwnerPath))
		{
			SaveDataMergedPlatformSaveGameIOProvider.LogInfo("Cache needs a reset because the save owner is missing.");
			this.<InitEnsureSameCacheUser>g__ResetCache|12_0(ref CS$<>8__locals1);
			return;
		}
		if (!this.m_cache.ManagedFileExists(CS$<>8__locals1.cacheOwnerPath))
		{
			SaveDataMergedPlatformSaveGameIOProvider.LogInfo("Cache needs a reset because the cache owner is missing.");
			this.<InitEnsureSameCacheUser>g__ResetCache|12_0(ref CS$<>8__locals1);
			return;
		}
		CS$<>8__locals1.buffer.Position = 0L;
		CS$<>8__locals1.buffer.SetLength(0L);
		this.m_main.ManagedFileRead(CS$<>8__locals1.saveOwnerPath, CS$<>8__locals1.buffer);
		CS$<>8__locals1.buffer.Position = 0L;
		string text;
		using (StreamReader streamReader = new StreamReader(CS$<>8__locals1.buffer, Encoding.UTF8, true, 1024, true))
		{
			text = streamReader.ReadToEnd();
		}
		CS$<>8__locals1.buffer.Position = 0L;
		CS$<>8__locals1.buffer.SetLength(0L);
		this.m_cache.ManagedFileRead(CS$<>8__locals1.cacheOwnerPath, CS$<>8__locals1.buffer);
		CS$<>8__locals1.buffer.Position = 0L;
		string text2;
		using (StreamReader streamReader2 = new StreamReader(CS$<>8__locals1.buffer, Encoding.UTF8, true, 1024, true))
		{
			text2 = streamReader2.ReadToEnd();
		}
		if (text == text2)
		{
			return;
		}
		SaveDataMergedPlatformSaveGameIOProvider.LogInfo(string.Concat(new string[] { "Cache needs a reset because the save owner '", text, "' does not match the cache owner '", text2, "'." }));
		this.<InitEnsureSameCacheUser>g__ResetCache|12_0(ref CS$<>8__locals1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitMoveToCache(MemoryStream buffer)
	{
		foreach (SaveDataManagedPath saveDataManagedPath in this.m_main.ManagedDirectoryEnumerateFiles(SaveDataManagedPath.RootPath, "*", SearchOption.AllDirectories))
		{
			if (this.IsCached(saveDataManagedPath))
			{
				SaveDataManagedPath saveDataManagedPath2;
				if (saveDataManagedPath.TryGetParentPath(out saveDataManagedPath2))
				{
					this.m_cache.ManagedDirectoryCreateDirectory(saveDataManagedPath2);
				}
				buffer.Position = 0L;
				buffer.SetLength(0L);
				this.m_main.ManagedFileRead(saveDataManagedPath, buffer);
				buffer.Position = 0L;
				this.m_cache.ManagedFileWrite(saveDataManagedPath, buffer);
				this.m_main.ManagedFileDelete(saveDataManagedPath);
				SaveDataMergedPlatformSaveGameIOProvider.LogInfo(string.Format("Moved from save data to cache: {0}", saveDataManagedPath));
			}
		}
		foreach (SaveDataManagedPath saveDataManagedPath3 in from p in this.m_main.ManagedDirectoryEnumerateDirectories(SaveDataManagedPath.RootPath, "*", SearchOption.AllDirectories)
			orderby p.PathRelativeToRoot.Count((char c) => c == '/') descending
			select p)
		{
			if (this.IsCached(saveDataManagedPath3) && this.m_main.ManagedDirectoryExists(saveDataManagedPath3) && !this.m_main.ManagedDirectoryEnumerateFileSystemEntries(saveDataManagedPath3, "*", SearchOption.TopDirectoryOnly).Any<SaveDataManagedPath>())
			{
				this.m_main.ManagedDirectoryDelete(saveDataManagedPath3, false);
				SaveDataMergedPlatformSaveGameIOProvider.LogInfo(string.Format("Deleted cache directory remaining in main: {0}", saveDataManagedPath3));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRemoveInvalidCachedFiles()
	{
		foreach (SaveDataManagedPath saveDataManagedPath in this.m_cache.ManagedDirectoryEnumerateFiles(SaveDataManagedPath.RootPath, "*", SearchOption.AllDirectories))
		{
			if (!this.IsCached(saveDataManagedPath))
			{
				this.m_cache.ManagedFileDelete(saveDataManagedPath);
				SaveDataMergedPlatformSaveGameIOProvider.LogInfo(string.Format("Deleted invalid cached file: {0}", saveDataManagedPath));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRemoveCachedGeneratedWorlds()
	{
		SaveDataManagedPath saveDataManagedPath = new SaveDataManagedPath("GeneratedWorlds");
		if (!this.m_cache.ManagedDirectoryExists(saveDataManagedPath))
		{
			return;
		}
		foreach (SaveDataManagedPath saveDataManagedPath2 in this.m_cache.ManagedDirectoryEnumerateDirectories(saveDataManagedPath, "*", SearchOption.TopDirectoryOnly))
		{
			if (!this.m_main.ManagedDirectoryExists(saveDataManagedPath2))
			{
				this.m_cache.ManagedDirectoryDelete(saveDataManagedPath2, true);
				SaveDataMergedPlatformSaveGameIOProvider.LogInfo(string.Format("Delete cache for non-existent generated world: {0}", saveDataManagedPath2));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRemoveEmptyCacheDirectories()
	{
		foreach (SaveDataManagedPath saveDataManagedPath in from p in this.m_cache.ManagedDirectoryEnumerateDirectories(SaveDataManagedPath.RootPath, "*", SearchOption.AllDirectories)
			orderby p.PathRelativeToRoot.Count((char c) => c == '/') descending
			select p)
		{
			if (this.m_cache.ManagedDirectoryExists(saveDataManagedPath) && !this.m_cache.ManagedDirectoryEnumerateFileSystemEntries(saveDataManagedPath, "*", SearchOption.TopDirectoryOnly).Any<SaveDataManagedPath>())
			{
				this.m_cache.ManagedDirectoryDelete(saveDataManagedPath, false);
				SaveDataMergedPlatformSaveGameIOProvider.LogInfo(string.Format("Deleted empty cache directory: {0}", saveDataManagedPath));
			}
		}
	}

	public ESaveGameProviderStatus Status
	{
		get
		{
			return this.m_main.Status;
		}
	}

	public IPlatformSaveGameIOProvider Cache
	{
		get
		{
			return null;
		}
	}

	public event Action Initialized
	{
		add
		{
			this.m_main.Initialized += value;
		}
		remove
		{
			this.m_main.Initialized -= value;
		}
	}

	public void Init(IPlatform _owner)
	{
		this.m_main.Init(_owner);
	}

	public void Destroy()
	{
		this.m_main.Destroy();
	}

	public bool ShouldBackup()
	{
		return this.m_main.ShouldBackup();
	}

	public bool ShouldCommit()
	{
		return this.m_main.ShouldCommit();
	}

	public double GetCommitProgress()
	{
		return this.m_main.GetCommitProgress();
	}

	public void Flush(bool waitForFlush)
	{
		this.m_main.Flush(waitForFlush);
	}

	public bool ShouldLimitSize()
	{
		return this.m_main.ShouldLimitSize();
	}

	public void UpdateSizes()
	{
		this.m_main.UpdateSizes();
	}

	public SaveDataSizes GetSizes()
	{
		return this.m_main.GetSizes();
	}

	public void ManagedFileRead(SaveDataManagedPath path, Stream dest)
	{
		this.GetIOProvider(path).ManagedFileRead(path, dest);
	}

	public void ManagedFileWrite(SaveDataManagedPath path, Stream src)
	{
		this.SyncParentDirectories(path);
		this.GetIOProvider(path).ManagedFileWrite(path, src);
	}

	public void ManagedFileCopy(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName, bool overwrite = false)
	{
		IPlatformSaveGameIOProvider ioprovider = this.GetIOProvider(sourceFileName);
		IPlatformSaveGameIOProvider ioprovider2 = this.GetIOProvider(destFileName);
		if (ioprovider == ioprovider2)
		{
			ioprovider.ManagedFileCopy(sourceFileName, destFileName, overwrite);
			return;
		}
		if (!overwrite && ioprovider2.ManagedFileExists(destFileName))
		{
			throw new IOException(string.Format("File already exists at '{0}'.", destFileName));
		}
		this.SyncParentDirectories(destFileName);
		using (MemoryStream memoryStream = new MemoryStream())
		{
			ioprovider.ManagedFileRead(sourceFileName, memoryStream);
			memoryStream.Position = 0L;
			ioprovider2.ManagedFileWrite(destFileName, memoryStream);
		}
	}

	public void ManagedFileDelete(SaveDataManagedPath path)
	{
		this.GetIOProvider(path).ManagedFileDelete(path);
	}

	public bool ManagedFileExists(SaveDataManagedPath path)
	{
		return this.GetIOProvider(path).ManagedFileExists(path);
	}

	public DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return this.GetIOProvider(path).ManagedFileGetLastWriteTimeUtc(path);
	}

	public void ManagedFileMove(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName)
	{
		IPlatformSaveGameIOProvider ioprovider = this.GetIOProvider(sourceFileName);
		IPlatformSaveGameIOProvider ioprovider2 = this.GetIOProvider(destFileName);
		if (ioprovider == ioprovider2)
		{
			ioprovider.ManagedFileMove(sourceFileName, destFileName);
			return;
		}
		if (ioprovider2.ManagedFileExists(destFileName))
		{
			throw new IOException(string.Format("File already exists at '{0}'.", destFileName));
		}
		this.SyncParentDirectories(destFileName);
		using (MemoryStream memoryStream = new MemoryStream())
		{
			ioprovider.ManagedFileRead(sourceFileName, memoryStream);
			memoryStream.Position = 0L;
			ioprovider2.ManagedFileWrite(destFileName, memoryStream);
		}
		ioprovider.ManagedFileDelete(sourceFileName);
	}

	public SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path)
	{
		return this.GetIOProvider(path).ManagedDirectoryCreateDirectory(path);
	}

	public DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return this.GetIOProvider(path).ManagedDirectoryGetLastWriteTimeUtc(path);
	}

	public bool ManagedDirectoryExists(SaveDataManagedPath path)
	{
		return this.GetIOProvider(path).ManagedDirectoryExists(path);
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return this.ManagedDirectoryEnumerateInternal(path, (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryEnumerateDirectories(path, searchPattern, searchOption));
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return this.ManagedDirectoryEnumerateInternal(path, (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryEnumerateFiles(path, searchPattern, searchOption));
	}

	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return this.ManagedDirectoryEnumerateInternal(path, (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryEnumerateFileSystemEntries(path, searchPattern, searchOption));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateInternal(SaveDataManagedPath path, Func<IPlatformSaveGameIOProvider, IEnumerable<SaveDataManagedPath>> getEnumerable)
	{
		SaveDataMergedPlatformSaveGameIOProvider.<>c__DisplayClass46_0 CS$<>8__locals1 = new SaveDataMergedPlatformSaveGameIOProvider.<>c__DisplayClass46_0();
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.getEnumerable = getEnumerable;
		if (this.m_cache == null)
		{
			return CS$<>8__locals1.getEnumerable(this.m_main);
		}
		if (this.IsCached(path))
		{
			return CS$<>8__locals1.getEnumerable(this.m_cache).Where(new Func<SaveDataManagedPath, bool>(this.IsCached));
		}
		if (!this.m_cache.ManagedDirectoryExists(path))
		{
			return from p in CS$<>8__locals1.getEnumerable(this.m_main)
				where !CS$<>8__locals1.<>4__this.IsCached(p)
				select p;
		}
		if (!this.m_main.ManagedDirectoryExists(path))
		{
			throw new IOException(string.Format("Directory does not exist at '{0}'.", path));
		}
		return CS$<>8__locals1.<ManagedDirectoryEnumerateInternal>g__EnumerateBoth|1();
	}

	public void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive)
	{
		if (this.IsCached(path))
		{
			this.m_cache.ManagedDirectoryDelete(path, recursive);
			return;
		}
		this.m_main.ManagedDirectoryDelete(path, recursive);
		if (this.m_cache != null && this.m_cache.ManagedDirectoryExists(path))
		{
			this.m_cache.ManagedDirectoryDelete(path, recursive);
		}
	}

	public long ManagedFileInfoLength(SaveDataManagedPath path)
	{
		if (this.IsCached(path))
		{
			return 0L;
		}
		return this.m_main.ManagedFileInfoLength(path);
	}

	public IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return this.ManagedDirectoryInfoEnumerateInternal<SdDirectoryInfo>(path, (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryInfoEnumerateDirectories(path, searchPattern, searchOption));
	}

	public IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return this.ManagedDirectoryInfoEnumerateInternal<SdFileInfo>(path, (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryInfoEnumerateFiles(path, searchPattern, searchOption));
	}

	public IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return this.ManagedDirectoryInfoEnumerateInternal<SdFileSystemInfo>(path, (IPlatformSaveGameIOProvider provider) => provider.ManagedDirectoryInfoEnumerateFileSystemInfos(path, searchPattern, searchOption));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerable<T> ManagedDirectoryInfoEnumerateInternal<T>(SaveDataManagedPath path, Func<IPlatformSaveGameIOProvider, IEnumerable<T>> getEnumerable) where T : SdFileSystemInfo
	{
		SaveDataMergedPlatformSaveGameIOProvider.<>c__DisplayClass52_0<T> CS$<>8__locals1 = new SaveDataMergedPlatformSaveGameIOProvider.<>c__DisplayClass52_0<T>();
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.getEnumerable = getEnumerable;
		if (this.m_cache == null)
		{
			return CS$<>8__locals1.getEnumerable(this.m_main);
		}
		if (this.IsCached(path))
		{
			return from info in CS$<>8__locals1.getEnumerable(this.m_cache)
				where CS$<>8__locals1.<>4__this.IsCached(info.ManagedPath)
				select info;
		}
		if (!this.m_cache.ManagedDirectoryExists(path))
		{
			return from info in CS$<>8__locals1.getEnumerable(this.m_main)
				where !CS$<>8__locals1.<>4__this.IsCached(info.ManagedPath)
				select info;
		}
		if (!this.m_main.ManagedDirectoryExists(path))
		{
			throw new IOException(string.Format("Directory does not exist at '{0}'.", path));
		}
		return CS$<>8__locals1.<ManagedDirectoryInfoEnumerateInternal>g__EnumerateBoth|2();
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <InitEnsureSameCacheUser>g__ResetCache|12_0(ref SaveDataMergedPlatformSaveGameIOProvider.<>c__DisplayClass12_0 A_1)
	{
		foreach (SaveDataManagedPath saveDataManagedPath in this.m_cache.ManagedDirectoryEnumerateDirectories(SaveDataManagedPath.RootPath, "*", SearchOption.TopDirectoryOnly))
		{
			this.m_cache.ManagedDirectoryDelete(saveDataManagedPath, true);
		}
		foreach (SaveDataManagedPath saveDataManagedPath2 in this.m_cache.ManagedDirectoryEnumerateFiles(SaveDataManagedPath.RootPath, "*", SearchOption.TopDirectoryOnly))
		{
			this.m_cache.ManagedFileDelete(saveDataManagedPath2);
		}
		A_1.buffer.Position = 0L;
		A_1.buffer.SetLength(0L);
		if (!this.m_main.ManagedFileExists(A_1.saveOwnerPath))
		{
			using (StreamWriter streamWriter = new StreamWriter(A_1.buffer, Encoding.UTF8, 1024, true))
			{
				streamWriter.Write(Guid.NewGuid());
			}
			A_1.buffer.Position = 0L;
			this.m_main.ManagedFileWrite(A_1.saveOwnerPath, A_1.buffer);
		}
		else
		{
			this.m_main.ManagedFileRead(A_1.saveOwnerPath, A_1.buffer);
		}
		A_1.buffer.Position = 0L;
		this.m_cache.ManagedFileWrite(A_1.cacheOwnerPath, A_1.buffer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const string CACHE_OWNER_FILENAME = "CacheOwner.txt";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string SAVE_OWNER_FILENAME = "SaveOwner.txt";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex s_cachedManagedPathRegex = SaveDataMergedPlatformSaveGameIOProvider.CreateCachedManagedPathRegex();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IPlatformSaveGameProvider m_main;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IPlatformSaveGameIOProvider m_cache;
}
