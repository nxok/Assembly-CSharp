using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Platform;
using Unity.Profiling;

public class SaveDataManager : SaveDataManagerBase
{
	public override bool AppliesSaveSizeLimit
	{
		get
		{
			return true;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override Stream GetStream(SaveDataManagedPath path, FileMode mode, FileAccess access, FileShare share)
	{
		if ((access == FileAccess.Read && (mode == FileMode.CreateNew || mode == FileMode.Create || mode == FileMode.Truncate || mode == FileMode.Append)) || (access == FileAccess.ReadWrite && mode == FileMode.Append))
		{
			throw new ArgumentException(string.Format("Combining {0}: {1} with {2}: {3} is invalid.", new object[] { "FileMode", mode, "FileAccess", access }));
		}
		if (this.cleanupComplete)
		{
			throw new InvalidOperationException("Attempting to get stream from Save Data Manager after Cleanup has been called.");
		}
		int num = path.PathRelativeToRoot.LastIndexOf('/');
		SaveDataManagedPath saveDataManagedPath = new SaveDataManagedPath((num >= 0) ? path.PathRelativeToRoot.Substring(0, num) : string.Empty);
		if (!this.ManagedDirectoryExists(saveDataManagedPath))
		{
			throw new IOException(SpanUtils.Concat("Parent of ", path.PathRelativeToRoot, " does not exist."));
		}
		object obj = this.mapLockObj;
		SaveDataManager.CachedStream cachedStream;
		lock (obj)
		{
			if (!this.pathToCacheMap.TryGetValue(path, out cachedStream))
			{
				cachedStream = new SaveDataManager.CachedStream(path, this.platformSaveGameProvider);
				this.pathToCacheMap[path] = cachedStream;
			}
			SaveDataManager.s_CachedStreamCount.Value = this.pathToCacheMap.Count;
		}
		Stream stream;
		cachedStream.EnterLock(mode, access, share, out stream);
		obj = this.mapLockObj;
		lock (obj)
		{
			this.streamToCacheMap[stream] = cachedStream;
			int value = SaveDataManager.s_StreamOperationCount.Value;
			ProfilerCounterValue<int> profilerCounterValue;
			profilerCounterValue.Value = value + 1;
		}
		return stream;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool IsPriorityFilePath(SaveDataManagedPath path)
	{
		return path.Type == SaveDataType.Saves && path.PathRelativeToSlot.AsSpan().IndexOf("Region") == 0;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ReturnStream(Stream stream)
	{
		if (this.cleanupComplete)
		{
			throw new InvalidOperationException("Attempting to return stream to Save Data Manager after Cleanup has been called.");
		}
		object obj = this.mapLockObj;
		SaveDataManager.CachedStream cachedStream;
		lock (obj)
		{
			cachedStream = this.streamToCacheMap[stream];
			cachedStream.ExitLock();
			this.streamToCacheMap.Remove(stream);
			long num;
			bool flag2;
			if (cachedStream.HasPendingChanges && this.TryGetTrackedSize(cachedStream.Path, out num, out flag2) && flag2 && num > cachedStream.PendingSize)
			{
				this.priorityCachedStreams.Add(cachedStream);
			}
			else
			{
				this.priorityCachedStreams.Remove(cachedStream);
			}
		}
		if (this.writeMode == SaveDataWriteMode.Immediate)
		{
			if (cachedStream.HasPendingChanges)
			{
				cachedStream.Commit();
			}
			obj = this.mapLockObj;
			lock (obj)
			{
				if (!cachedStream.IsLocked)
				{
					this.RemoveAndDispose(cachedStream);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveAndDispose(SaveDataManager.CachedStream cachedStream)
	{
		object obj = this.mapLockObj;
		lock (obj)
		{
			this.pathToCacheMap.Remove(cachedStream.Path);
			this.priorityCachedStreams.Remove(cachedStream);
			cachedStream.Dispose();
		}
	}

	public override bool ShouldLimitSize()
	{
		return this.platformSaveGameProvider.ShouldLimitSize();
	}

	public override void UpdateSizes()
	{
		this.platformSaveGameProvider.UpdateSizes();
	}

	public override SaveDataSizes GetSizes()
	{
		return this.platformSaveGameProvider.GetSizes();
	}

	public override void ManagedFileDelete(SaveDataManagedPath path)
	{
		object obj = this.mapLockObj;
		lock (obj)
		{
			SaveDataManager.CachedStream cachedStream;
			bool flag2 = this.pathToCacheMap.TryGetValue(path, out cachedStream);
			if (flag2 && cachedStream.IsLocked)
			{
				throw new IOException(string.Format("Cached Stream for '{0}' can not be deleted because it is current locked (stream is loaned out and not returned yet).", path));
			}
			if (this.platformSaveGameProvider.ShouldBackup())
			{
				SaveDataManagedPath backupPath = SaveDataUtils.GetBackupPath(path);
				if (this.platformSaveGameProvider.ManagedFileExists(backupPath))
				{
					this.platformSaveGameProvider.ManagedFileDelete(backupPath);
				}
			}
			this.platformSaveGameProvider.ManagedFileDelete(path);
			if (flag2)
			{
				this.RemoveAndDispose(cachedStream);
			}
			SaveDataManager.SlotSizeData slotSizeData;
			if (this.fileSizesBySlot.TryGetValue(path.Slot, out slotSizeData))
			{
				slotSizeData.RemoveFileSize(path);
			}
		}
	}

	public override bool ManagedFileExists(SaveDataManagedPath path)
	{
		object obj = this.mapLockObj;
		lock (obj)
		{
			SaveDataManager.CachedStream cachedStream;
			if (this.pathToCacheMap.TryGetValue(path, out cachedStream) && cachedStream.ExistsVirtual)
			{
				return true;
			}
		}
		return this.platformSaveGameProvider.ManagedFileExists(path);
	}

	public override DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		object obj = this.mapLockObj;
		lock (obj)
		{
			SaveDataManager.CachedStream cachedStream;
			if (this.pathToCacheMap.TryGetValue(path, out cachedStream))
			{
				return cachedStream.LastWriteTime;
			}
		}
		return this.platformSaveGameProvider.ManagedFileGetLastWriteTimeUtc(path);
	}

	public override SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path)
	{
		return this.platformSaveGameProvider.ManagedDirectoryCreateDirectory(path);
	}

	public override DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path)
	{
		return this.platformSaveGameProvider.ManagedDirectoryGetLastWriteTimeUtc(path);
	}

	public override bool ManagedDirectoryExists(SaveDataManagedPath path)
	{
		return this.platformSaveGameProvider.ManagedDirectoryExists(path);
	}

	public override IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return this.platformSaveGameProvider.ManagedDirectoryEnumerateDirectories(path, searchPattern, searchOption);
	}

	public override IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		HashSet<SaveDataManagedPath> cachedPaths = new HashSet<SaveDataManagedPath>();
		object obj = this.mapLockObj;
		lock (obj)
		{
			foreach (KeyValuePair<SaveDataManagedPath, SaveDataManager.CachedStream> keyValuePair in this.pathToCacheMap)
			{
				SaveDataManagedPath saveDataManagedPath;
				SaveDataManager.CachedStream cachedStream;
				keyValuePair.Deconstruct(out saveDataManagedPath, out cachedStream);
				SaveDataManagedPath saveDataManagedPath2 = saveDataManagedPath;
				if (cachedStream.ExistsVirtual && SaveDataManager.PathSearchMatches(path, saveDataManagedPath2, searchPattern, searchOption))
				{
					cachedPaths.Add(saveDataManagedPath2);
				}
			}
		}
		foreach (SaveDataManagedPath platformPath in this.platformSaveGameProvider.ManagedDirectoryEnumerateFiles(path, searchPattern, searchOption))
		{
			yield return platformPath;
			cachedPaths.Remove(platformPath);
			platformPath = null;
		}
		IEnumerator<SaveDataManagedPath> enumerator2 = null;
		foreach (SaveDataManagedPath saveDataManagedPath3 in cachedPaths)
		{
			yield return saveDataManagedPath3;
		}
		HashSet<SaveDataManagedPath>.Enumerator enumerator3 = default(HashSet<SaveDataManagedPath>.Enumerator);
		yield break;
		yield break;
	}

	public override IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		HashSet<SaveDataManagedPath> cachedPaths = new HashSet<SaveDataManagedPath>();
		object obj = this.mapLockObj;
		lock (obj)
		{
			foreach (KeyValuePair<SaveDataManagedPath, SaveDataManager.CachedStream> keyValuePair in this.pathToCacheMap)
			{
				SaveDataManagedPath saveDataManagedPath;
				SaveDataManager.CachedStream cachedStream;
				keyValuePair.Deconstruct(out saveDataManagedPath, out cachedStream);
				SaveDataManagedPath saveDataManagedPath2 = saveDataManagedPath;
				if (cachedStream.ExistsVirtual && SaveDataManager.PathSearchMatches(path, saveDataManagedPath2, searchPattern, searchOption))
				{
					cachedPaths.Add(saveDataManagedPath2);
				}
			}
		}
		foreach (SaveDataManagedPath platformPath in this.platformSaveGameProvider.ManagedDirectoryEnumerateFileSystemEntries(path, searchPattern, searchOption))
		{
			yield return platformPath;
			cachedPaths.Remove(platformPath);
			platformPath = null;
		}
		IEnumerator<SaveDataManagedPath> enumerator2 = null;
		foreach (SaveDataManagedPath saveDataManagedPath3 in cachedPaths)
		{
			yield return saveDataManagedPath3;
		}
		HashSet<SaveDataManagedPath>.Enumerator enumerator3 = default(HashSet<SaveDataManagedPath>.Enumerator);
		yield break;
		yield break;
	}

	public override void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive)
	{
		object obj = this.mapLockObj;
		lock (obj)
		{
			List<KeyValuePair<SaveDataManagedPath, SaveDataManager.CachedStream>> list = this.pathToCacheMap.Where((KeyValuePair<SaveDataManagedPath, SaveDataManager.CachedStream> kv) => path.IsParentOf(kv.Key)).ToList<KeyValuePair<SaveDataManagedPath, SaveDataManager.CachedStream>>();
			foreach (KeyValuePair<SaveDataManagedPath, SaveDataManager.CachedStream> keyValuePair in list)
			{
				SaveDataManagedPath saveDataManagedPath;
				SaveDataManager.CachedStream cachedStream;
				keyValuePair.Deconstruct(out saveDataManagedPath, out cachedStream);
				SaveDataManagedPath saveDataManagedPath2 = saveDataManagedPath;
				if (cachedStream.IsLocked)
				{
					throw new IOException(string.Format("Can not delete directory at '{0}' because Cached Stream for '{1}' can not be deleted because it is current locked (stream is loaned out and not returned yet).", path, saveDataManagedPath2));
				}
			}
			this.platformSaveGameProvider.ManagedDirectoryDelete(path, recursive);
			foreach (KeyValuePair<SaveDataManagedPath, SaveDataManager.CachedStream> keyValuePair in list)
			{
				SaveDataManagedPath saveDataManagedPath;
				SaveDataManager.CachedStream cachedStream;
				keyValuePair.Deconstruct(out saveDataManagedPath, out cachedStream);
				SaveDataManager.CachedStream cachedStream2 = cachedStream;
				this.RemoveAndDispose(cachedStream2);
			}
			foreach (SaveDataManager.SlotSizeData slotSizeData in this.fileSizesBySlot.Values)
			{
				slotSizeData.RemoveFileSizes(path);
			}
		}
	}

	public override long ManagedFileInfoLength(SaveDataManagedPath path)
	{
		object obj = this.mapLockObj;
		lock (obj)
		{
			SaveDataManager.CachedStream cachedStream;
			if (this.pathToCacheMap.TryGetValue(path, out cachedStream) && cachedStream.HasPendingChanges)
			{
				return cachedStream.PendingSize;
			}
		}
		return this.platformSaveGameProvider.ManagedFileInfoLength(path);
	}

	public override IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		return this.platformSaveGameProvider.ManagedDirectoryInfoEnumerateDirectories(path, searchPattern, searchOption);
	}

	public override IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		HashSet<SaveDataManagedPath> cachedPaths = new HashSet<SaveDataManagedPath>();
		object obj = this.mapLockObj;
		lock (obj)
		{
			foreach (KeyValuePair<SaveDataManagedPath, SaveDataManager.CachedStream> keyValuePair in this.pathToCacheMap)
			{
				SaveDataManagedPath saveDataManagedPath;
				SaveDataManager.CachedStream cachedStream;
				keyValuePair.Deconstruct(out saveDataManagedPath, out cachedStream);
				SaveDataManagedPath saveDataManagedPath2 = saveDataManagedPath;
				if (cachedStream.ExistsVirtual && SaveDataManager.PathSearchMatches(path, saveDataManagedPath2, searchPattern, searchOption))
				{
					cachedPaths.Add(saveDataManagedPath2);
				}
			}
		}
		foreach (SdFileInfo fileInfo in this.platformSaveGameProvider.ManagedDirectoryInfoEnumerateFiles(path, searchPattern, searchOption))
		{
			yield return fileInfo;
			cachedPaths.Remove(fileInfo.ManagedPath);
			fileInfo = null;
		}
		IEnumerator<SdFileInfo> enumerator2 = null;
		foreach (SaveDataManagedPath saveDataManagedPath3 in cachedPaths)
		{
			yield return new SdFileInfo(saveDataManagedPath3);
		}
		HashSet<SaveDataManagedPath>.Enumerator enumerator3 = default(HashSet<SaveDataManagedPath>.Enumerator);
		yield break;
		yield break;
	}

	public override IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		HashSet<SaveDataManagedPath> cachedPaths = new HashSet<SaveDataManagedPath>();
		object obj = this.mapLockObj;
		lock (obj)
		{
			foreach (KeyValuePair<SaveDataManagedPath, SaveDataManager.CachedStream> keyValuePair in this.pathToCacheMap)
			{
				SaveDataManagedPath saveDataManagedPath;
				SaveDataManager.CachedStream cachedStream;
				keyValuePair.Deconstruct(out saveDataManagedPath, out cachedStream);
				SaveDataManagedPath saveDataManagedPath2 = saveDataManagedPath;
				if (cachedStream.ExistsVirtual && SaveDataManager.PathSearchMatches(path, saveDataManagedPath2, searchPattern, searchOption))
				{
					cachedPaths.Add(saveDataManagedPath2);
				}
			}
		}
		foreach (SdFileSystemInfo fileInfo in this.platformSaveGameProvider.ManagedDirectoryInfoEnumerateFileSystemInfos(path, searchPattern, searchOption))
		{
			yield return fileInfo;
			cachedPaths.Remove(fileInfo.ManagedPath);
			fileInfo = null;
		}
		IEnumerator<SdFileSystemInfo> enumerator2 = null;
		foreach (SaveDataManagedPath saveDataManagedPath3 in cachedPaths)
		{
			yield return new SdFileInfo(saveDataManagedPath3);
		}
		HashSet<SaveDataManagedPath>.Enumerator enumerator3 = default(HashSet<SaveDataManagedPath>.Enumerator);
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool PathSearchMatches(SaveDataManagedPath parent, SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
	{
		if (!parent.IsParentOf(path))
		{
			return false;
		}
		if (searchOption == SearchOption.TopDirectoryOnly)
		{
			int num = ((parent.PathRelativeToRoot.Length > 0) ? (parent.PathRelativeToRoot.Length + 1) : 0);
			if (path.PathRelativeToRoot.IndexOf('/', num) >= 0)
			{
				return false;
			}
		}
		return SaveDataManager.MatchesSearchPattern(path, searchPattern);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool MatchesSearchPattern(SaveDataManagedPath path, string searchPattern)
	{
		if (searchPattern == "*")
		{
			return true;
		}
		StringSpan stringSpan = path.PathRelativeToRoot;
		int num = stringSpan.LastIndexOf('/');
		if (num >= 0 && num < stringSpan.Length - 1)
		{
			stringSpan = stringSpan.Slice(num + 1);
		}
		return SaveDataManager.<MatchesSearchPattern>g__MatchWildcard|32_0(stringSpan, searchPattern);
	}

	public SaveDataManager(IPlatformSaveGameProvider platformSaveGameProvider)
	{
		this.platformSaveGameProvider = new SaveDataMergedPlatformSaveGameIOProvider(platformSaveGameProvider);
	}

	public override void Init()
	{
		this.SetWriteMode(SaveDataWriteMode.Deferred);
		this.InitRestoreBackups();
		this.InitFileSizeTracking();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitRestoreBackups()
	{
		if (!this.platformSaveGameProvider.ShouldBackup())
		{
			return;
		}
		using (SaveDataManager.s_InitRestoreBackupsMarker.Auto())
		{
			SaveDataManagedPath path = SaveDataType.User.GetPath();
			if (this.ManagedDirectoryExists(path))
			{
				foreach (SdFileInfo sdFileInfo in this.ManagedDirectoryInfoEnumerateFiles(path, "*.bup", SearchOption.AllDirectories))
				{
					SaveDataManagedPath managedPath = sdFileInfo.ManagedPath;
					SaveDataManagedPath restorePath = SaveDataUtils.GetRestorePath(managedPath);
					if (this.platformSaveGameProvider.ManagedDirectoryExists(restorePath))
					{
						Log.Warning(string.Format("Could not restore to \"{0}\" using \"{1}\" because the former is a directory.", restorePath, managedPath));
					}
					else
					{
						this.platformSaveGameProvider.ManagedFileDelete(restorePath);
						this.platformSaveGameProvider.ManagedFileMove(managedPath, restorePath);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitFileSizeTracking()
	{
		using (SaveDataManager.s_InitFileSizeTrackingMarker.Auto())
		{
			this.fileSizesBySlot.Clear();
			SaveDataManagedPath path = SaveDataType.User.GetPath();
			if (this.ManagedDirectoryExists(path))
			{
				foreach (SdFileInfo sdFileInfo in this.ManagedDirectoryInfoEnumerateFiles(path, "*", SearchOption.AllDirectories))
				{
					SaveDataManagedPath managedPath = sdFileInfo.ManagedPath;
					SaveDataManager.SlotSizeData slotSizeData;
					if (!this.fileSizesBySlot.TryGetValue(managedPath.Slot, out slotSizeData))
					{
						SaveDataSlot simpleSlot = managedPath.Slot.GetSimpleSlot();
						slotSizeData = new SaveDataManager.SlotSizeData(simpleSlot);
						this.fileSizesBySlot[simpleSlot] = slotSizeData;
					}
					long length = sdFileInfo.Length;
					slotSizeData.SetFileSize(managedPath, length);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetTrackedSize(SaveDataManagedPath path, out long trackedSize, out bool isPriorityFile)
	{
		SaveDataManager.SlotSizeData slotSizeData;
		if (this.fileSizesBySlot.TryGetValue(path.Slot, out slotSizeData) && slotSizeData.TryGetFileSize(path, out trackedSize, out isPriorityFile))
		{
			return true;
		}
		trackedSize = 0L;
		isPriorityFile = false;
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int WriteTask(ThreadManager.ThreadInfo _threadInfo)
	{
		try
		{
			if (_threadInfo.TerminationRequested())
			{
				return -1;
			}
			int num = this.explicitCommitNeeded;
			TimeSpan timeSpan = DateTime.Now - this.lastCommitTime;
			if (num != this.explicitCommitCount || timeSpan > SaveDataManager.commitInterval)
			{
				if (this.CommitPendingChanges(false))
				{
					this.explicitCommitCount = num;
				}
				this.lastCommitTime = DateTime.Now;
			}
		}
		catch (Exception ex)
		{
			Log.Exception(ex);
		}
		return 5;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CommitPendingChanges(bool force = false)
	{
		if (!this.platformSaveGameProvider.ShouldCommit())
		{
			if (!force)
			{
				return false;
			}
			Log.Out("[SaveDataManager] Forcing CommitPendingChanges (platform currently suggests to not commit).");
		}
		bool flag = false;
		try
		{
			object obj = this.commitLockObj;
			lock (obj)
			{
				HashSet<SaveDataManager.CachedStream> hashSet = new HashSet<SaveDataManager.CachedStream>();
				SaveDataManager.CachedStream cachedStream;
				bool flag3;
				while (this.TryGetNextStreamToCommit(hashSet, out cachedStream, out flag3))
				{
					bool flag4 = true;
					try
					{
						if (!flag)
						{
							flag = true;
							base.OnCommitStarted();
						}
						long pendingSize = cachedStream.PendingSize;
						long num = pendingSize;
						SaveDataManager.SlotSizeData slotSizeData;
						long num2;
						if (!this.fileSizesBySlot.TryGetValue(cachedStream.Path.Slot, out slotSizeData))
						{
							SaveDataSlot simpleSlot = cachedStream.Path.Slot.GetSimpleSlot();
							slotSizeData = new SaveDataManager.SlotSizeData(simpleSlot);
							this.fileSizesBySlot[simpleSlot] = slotSizeData;
						}
						else if (slotSizeData.TryGetFileSize(cachedStream.Path, out num2))
						{
							num -= num2;
						}
						if (!flag3 && pendingSize > 0L)
						{
							object obj2 = this.regionFileLock;
							lock (obj2)
							{
								if (this.activeRegionFileManager != null && this.activeRegionFileManager.MaxBytes > 0L && cachedStream.Path.Slot == this.activeRegionFileManager.SaveDataSlot)
								{
									long num3 = slotSizeData.TotalFileSize + slotSizeData.LargestPriorityFileSize + pendingSize - this.activeRegionFileManager.MaxBytes;
									if (num3 > 0L)
									{
										cachedStream.ExitCommitLock();
										flag4 = false;
										if (this.activeRegionFileManager.MakeRoom(num3))
										{
											continue;
										}
										Log.Error(string.Format("[SaveDataManager] SaveDataLimit will be exceeded! Failed to clear enough space for file: {0}", cachedStream.Path));
										cachedStream.EnterCommitLock(ref flag4);
										if (!flag4)
										{
											Log.Error(string.Format("[SaveDataManager] Failed to take commit lock on cached stream for path: {0}", cachedStream.Path));
											continue;
										}
									}
								}
							}
						}
						slotSizeData.SetFileSize(cachedStream.Path, pendingSize);
						cachedStream.Commit();
						continue;
					}
					catch (Exception ex)
					{
						hashSet.Add(cachedStream);
						Log.Error(string.Format("[SaveDataManager] Failed to commit cached stream '{0}'.", cachedStream.Path));
						Log.Exception(ex);
						continue;
					}
					finally
					{
						if (flag4)
						{
							cachedStream.ExitCommitLock();
						}
						object obj2 = this.mapLockObj;
						lock (obj2)
						{
							if (!cachedStream.HasPendingChanges)
							{
								this.priorityCachedStreams.Remove(cachedStream);
							}
						}
					}
					break;
				}
				this.CloseDormantStreams();
			}
		}
		finally
		{
			try
			{
				IPlatformSaveGameProvider platformSaveGameProvider = this.platformSaveGameProvider;
				if (platformSaveGameProvider != null)
				{
					platformSaveGameProvider.Flush(true);
				}
			}
			catch (Exception ex2)
			{
				Log.Error(string.Format("Error while flushing platform save game provider: {0}", ex2));
			}
			if (flag)
			{
				base.OnCommitFinished();
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetNextStreamToCommit(HashSet<SaveDataManager.CachedStream> ignored, out SaveDataManager.CachedStream commitTarget, out bool isPriorityStream)
	{
		SaveDataManager.<>c__DisplayClass58_0 CS$<>8__locals1 = new SaveDataManager.<>c__DisplayClass58_0();
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.ignored = ignored;
		object obj = this.mapLockObj;
		bool flag3;
		lock (obj)
		{
			if (CS$<>8__locals1.<TryGetNextStreamToCommit>g__TryGetNextStreamInternal|0(out commitTarget, out isPriorityStream))
			{
				bool flag2 = false;
				commitTarget.EnterCommitLock(ref flag2);
				if (flag2)
				{
					return true;
				}
				commitTarget = null;
				isPriorityStream = false;
				Log.Error(string.Format("[SaveDataManager] Failed to take commit lock on cached stream for path: {0}", commitTarget.Path));
			}
			flag3 = false;
		}
		return flag3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CloseDormantStreams()
	{
		object obj = this.mapLockObj;
		lock (obj)
		{
			this.closeList.Clear();
			DateTime now = DateTime.Now;
			foreach (SaveDataManager.CachedStream cachedStream in this.pathToCacheMap.Values)
			{
				if (!cachedStream.HasPendingChanges && (now - cachedStream.LastAccessTime).TotalMilliseconds > 30000.0)
				{
					this.closeList.Add(cachedStream);
				}
			}
			foreach (SaveDataManager.CachedStream cachedStream2 in this.closeList)
			{
				this.RemoveAndDispose(cachedStream2);
			}
			SaveDataManager.s_CachedStreamCount.Value = this.pathToCacheMap.Count;
		}
	}

	public override void RegisterRegionFileManager(RegionFileManager regionFileManager)
	{
		object obj = this.regionFileLock;
		lock (obj)
		{
			if (this.activeRegionFileManager != null)
			{
				Log.Warning("[SaveDataManager] Attempting to register a RegionFileManager when one is already registered. The existing reference will be overridden.");
			}
			this.activeRegionFileManager = regionFileManager;
		}
	}

	public override void DeregisterRegionFileManager(RegionFileManager regionFileManager)
	{
		object obj = this.regionFileLock;
		lock (obj)
		{
			if (this.activeRegionFileManager != regionFileManager)
			{
				Log.Error("[SaveDataManager] Attempting to deregister a RegionFileManager which is not currently registered.");
			}
			else
			{
				this.activeRegionFileManager = null;
			}
		}
	}

	public override SaveDataWriteMode GetWriteMode()
	{
		return this.writeMode;
	}

	public override void SetWriteMode(SaveDataWriteMode writeMode)
	{
		if (this.writeMode == writeMode)
		{
			return;
		}
		object obj = this.commitLockObj;
		lock (obj)
		{
			object obj2 = this.mapLockObj;
			lock (obj2)
			{
				if (writeMode == SaveDataWriteMode.Deferred)
				{
					this.lastCommitTime = DateTime.Now;
					this.writerThreadInfo = ThreadManager.StartThread("SaveDataManager_WriterThread", null, new ThreadManager.ThreadFunctionLoopDelegate(this.WriteTask), null, null, null, false, false);
				}
				else
				{
					ThreadManager.ThreadInfo threadInfo = this.writerThreadInfo;
					if (threadInfo != null)
					{
						threadInfo.WaitForEnd(420);
					}
					this.writerThreadInfo = null;
					this.CommitPendingChanges(true);
				}
				this.writeMode = writeMode;
			}
		}
	}

	public override void Cleanup()
	{
		this.SetWriteMode(SaveDataWriteMode.None);
		this.cleanupComplete = true;
	}

	public override int CommitAsync()
	{
		if (this.writeMode != SaveDataWriteMode.Deferred)
		{
			return this.explicitCommitCount;
		}
		int num = this.explicitCommitNeeded + 1;
		this.explicitCommitNeeded = num;
		return num;
	}

	public override bool IsCommitPending(int token)
	{
		return token > this.explicitCommitCount;
	}

	public override void CommitSync()
	{
		if (this.writeMode != SaveDataWriteMode.Deferred)
		{
			return;
		}
		int num = this.CommitAsync();
		while (this.IsCommitPending(num) && this.writeMode == SaveDataWriteMode.Deferred)
		{
			Thread.Sleep(5);
		}
	}

	public override IEnumerator CommitCoroutine(Action<double> progressFeedback = null)
	{
		if (this.writeMode != SaveDataWriteMode.Deferred)
		{
			yield break;
		}
		int token = this.CommitAsync();
		while (this.IsCommitPending(token) && this.writeMode == SaveDataWriteMode.Deferred)
		{
			if (progressFeedback != null)
			{
				progressFeedback(this.platformSaveGameProvider.GetCommitProgress());
			}
			yield return null;
		}
		yield break;
	}

	[Conditional("DEBUG_SAVE_DATA_MANAGER")]
	[PublicizedFrom(EAccessModifier.Private)]
	public new static void DebugLog(string text)
	{
		Log.Out("[SaveDataManager] " + text);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public unsafe static bool <MatchesSearchPattern>g__MatchWildcard|32_0(StringSpan text, string pattern)
	{
		int i = 0;
		int num = 0;
		int num2 = -1;
		int num3 = -1;
		while (i < text.Length)
		{
			if (num < pattern.Length && (pattern[num] == '?' || char.ToLowerInvariant(pattern[num]) == char.ToLowerInvariant((char)(*text[i]))))
			{
				i++;
				num++;
			}
			else if (num < pattern.Length && pattern[num] == '*')
			{
				num3 = num++;
				num2 = i;
			}
			else
			{
				if (num3 < 0)
				{
					return false;
				}
				num = num3 + 1;
				num2 = (i = num2 + 1);
			}
		}
		while (num < pattern.Length && pattern[num] == '*')
		{
			num++;
		}
		return num == pattern.Length;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int writerThreadTerminationTimeout = 420;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IPlatformSaveGameProvider platformSaveGameProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object mapLockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object commitLockObj = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<SaveDataManagedPath, SaveDataManager.CachedStream> pathToCacheMap = new Dictionary<SaveDataManagedPath, SaveDataManager.CachedStream>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<SaveDataManager.CachedStream> priorityCachedStreams = new HashSet<SaveDataManager.CachedStream>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Stream, SaveDataManager.CachedStream> streamToCacheMap = new Dictionary<Stream, SaveDataManager.CachedStream>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TimeSpan commitInterval = TimeSpan.FromSeconds(30.0);

	[PublicizedFrom(EAccessModifier.Private)]
	public const int writeTaskDelay = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int memoryCopyTimeout = 30000;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_CommitPendingChangesMarker = new ProfilerMarker("SaveDataManager.CommitPendingChanges");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_InitRestoreBackupsMarker = new ProfilerMarker("SaveDataManager.InitRestoreBackups");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerMarker s_InitFileSizeTrackingMarker = new ProfilerMarker("SaveDataManager.InitFileSizeTracking");

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_CachedStreamCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Cached Streams", 3, 2);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ProfilerCounterValue<int> s_StreamOperationCount = new ProfilerCounterValue<int>(ProfilerCategory.Scripts, "Stream Operations", 3, 6);

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.ThreadInfo writerThreadInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime lastCommitTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public int explicitCommitNeeded;

	[PublicizedFrom(EAccessModifier.Private)]
	public int explicitCommitCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SaveDataManager.CachedStream> commitList = new List<SaveDataManager.CachedStream>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<SaveDataManager.CachedStream> closeList = new List<SaveDataManager.CachedStream>();

	[PublicizedFrom(EAccessModifier.Private)]
	public SaveDataWriteMode writeMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cleanupComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<SaveDataSlot, SaveDataManager.SlotSizeData> fileSizesBySlot = new Dictionary<SaveDataSlot, SaveDataManager.SlotSizeData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public object regionFileLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public RegionFileManager activeRegionFileManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public class CachedStream : IDisposable
	{
		public SaveDataManagedPath Path { get; }

		public long PendingSize
		{
			get
			{
				if (!this.HasPendingChanges)
				{
					return 0L;
				}
				SaveDataManager.CachedStream.VirtualFileStream virtualFileStream = this.commitCopy;
				if (virtualFileStream == null)
				{
					return 0L;
				}
				return virtualFileStream.Length;
			}
		}

		public bool HasPendingChanges
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool IsLocked
		{
			get
			{
				return this.sl.IsHeld;
			}
		}

		public DateTime LastAccessTime
		{
			get
			{
				if (!this.IsLocked)
				{
					return this.lastAccessTime;
				}
				return DateTime.Now;
			}
		}

		public DateTime LastWriteTime
		{
			get
			{
				return this.lastWriteTime;
			}
		}

		public bool ExistsVirtual
		{
			get
			{
				object obj = this.lockObj;
				bool flag2;
				lock (obj)
				{
					flag2 = this.workingCopy != null;
				}
				return flag2;
			}
		}

		public CachedStream(SaveDataManagedPath path, IPlatformSaveGameProvider platformSaveGameProvider)
		{
			this.Path = path;
			this.backupPath = SaveDataUtils.GetBackupPath(path);
			this.platformSaveGameProvider = platformSaveGameProvider;
			this.lastAccessTime = DateTime.MinValue;
			this.lastWriteTime = platformSaveGameProvider.ManagedFileGetLastWriteTimeUtc(path);
			if (platformSaveGameProvider.ShouldBackup() && platformSaveGameProvider.ManagedFileExists(this.backupPath))
			{
				if (platformSaveGameProvider.ManagedDirectoryExists(this.Path))
				{
					Log.Warning(string.Format("Could not restore to \"{0}\" using \"{1}\" because the former is a directory.", this.Path, this.backupPath));
					return;
				}
				platformSaveGameProvider.ManagedFileDelete(this.Path);
				platformSaveGameProvider.ManagedFileMove(this.backupPath, this.Path);
			}
		}

		public void EnterLock(FileMode mode, FileAccess access, FileShare share, out Stream stream)
		{
			bool flag = false;
			stream = null;
			try
			{
				this.sl.Enter(ref flag);
				if (!flag)
				{
					Log.Error("[SaveDataManager] Spin lock should have blocked till it was acquired or thrown an exception.");
				}
				else
				{
					object obj = this.lockObj;
					lock (obj)
					{
						bool flag3 = this.workingCopy != null;
						bool flag4 = this.platformSaveGameProvider.ManagedFileExists(this.Path);
						bool flag5 = flag3 || flag4;
						if (mode == FileMode.Create)
						{
							mode = (flag5 ? FileMode.Truncate : FileMode.CreateNew);
						}
						this.isReading = (access & FileAccess.Read) > (FileAccess)0;
						this.isWriting = (access & FileAccess.Write) > (FileAccess)0;
						if (mode == FileMode.Open || mode == FileMode.OpenOrCreate || mode == FileMode.Append)
						{
							if (!flag3)
							{
								SaveDataManager.CachedStream.VirtualFileStream virtualFileStream;
								if (mode != FileMode.Open && !flag4)
								{
									virtualFileStream = new SaveDataManager.CachedStream.VirtualFileStream();
									this.lastWriteTime = DateTime.UtcNow;
								}
								else
								{
									virtualFileStream = new SaveDataManager.CachedStream.VirtualFileStream();
									this.platformSaveGameProvider.ManagedFileRead(this.Path, virtualFileStream);
								}
								this.workingCopy = virtualFileStream;
							}
							SeekOrigin seekOrigin = ((mode == FileMode.Append) ? SeekOrigin.End : SeekOrigin.Begin);
							this.workingCopy.Seek(0L, seekOrigin);
						}
						else if (mode == FileMode.Truncate)
						{
							if (flag3)
							{
								this.workingCopy.SetLength(0L);
								this.lastWriteTime = DateTime.UtcNow;
							}
							else
							{
								if (!flag4)
								{
									throw new IOException(string.Format("File does not exist at {0}", this.Path));
								}
								this.workingCopy = new SaveDataManager.CachedStream.VirtualFileStream();
								this.lastWriteTime = DateTime.UtcNow;
							}
						}
						else
						{
							if (mode != FileMode.CreateNew)
							{
								throw new NotImplementedException(string.Format("Unhandled mode: {0}", mode));
							}
							if (flag5)
							{
								throw new IOException(string.Format("File already exists at {0}", this.Path));
							}
							this.workingCopy = new SaveDataManager.CachedStream.VirtualFileStream();
							this.lastWriteTime = DateTime.UtcNow;
						}
						this.workingCopy.ReadEnabled = this.isReading;
						this.workingCopy.WriteEnabled = this.isWriting;
						stream = this.workingCopy;
					}
				}
			}
			finally
			{
				if (stream == null && flag)
				{
					this.sl.Exit();
				}
			}
		}

		public void ExitLock()
		{
			bool flag = false;
			this.EnterCommitLock(ref flag);
			if (flag)
			{
				object obj = this.lockObj;
				lock (obj)
				{
					this.workingCopy.ReadEnabled = true;
					this.workingCopy.WriteEnabled = true;
					if (this.isWriting)
					{
						if (this.commitCopy == null)
						{
							this.commitCopy = new SaveDataManager.CachedStream.VirtualFileStream(this.workingCopy.Length);
						}
						else
						{
							this.commitCopy.Seek(0L, SeekOrigin.Begin);
							this.commitCopy.SetLength(0L);
						}
						this.workingCopy.Position = 0L;
						StreamUtils.StreamCopy(this.workingCopy, this.commitCopy, null, true);
						this.HasPendingChanges = true;
						this.lastWriteTime = DateTime.UtcNow;
					}
					this.isReading = false;
					this.isWriting = false;
					this.lastAccessTime = DateTime.Now;
					this.sl.Exit();
					goto IL_00F4;
				}
			}
			Log.Error(string.Format("[SaveDataManager] Failed to take commit lock on cached stream for path: {0}", this.Path));
			IL_00F4:
			this.ExitCommitLock();
		}

		public void EnterCommitLock(ref bool lockTaken)
		{
			this.commitLock.Enter(ref lockTaken);
		}

		public void ExitCommitLock()
		{
			this.commitLock.Exit();
		}

		public void Commit()
		{
			object obj = this.lockObj;
			lock (obj)
			{
				if (this.commitCopy == null)
				{
					Log.Error("CachedStream commit failed: commitCopy is null.");
				}
				else
				{
					if (this.platformSaveGameProvider.ShouldBackup() && this.platformSaveGameProvider.ManagedFileExists(this.Path))
					{
						if (this.platformSaveGameProvider.ManagedDirectoryExists(this.backupPath))
						{
							Log.Warning(string.Format("Could not create backup of \"{0}\" to \"{1}\" because the latter is a directory.", this.Path, this.backupPath));
						}
						else
						{
							this.platformSaveGameProvider.ManagedFileDelete(this.backupPath);
							this.platformSaveGameProvider.ManagedFileMove(this.Path, this.backupPath);
						}
					}
					this.commitCopy.Position = 0L;
					this.platformSaveGameProvider.ManagedFileWrite(this.Path, this.commitCopy);
					if (this.platformSaveGameProvider.ShouldBackup() && this.platformSaveGameProvider.ManagedFileExists(this.backupPath))
					{
						this.platformSaveGameProvider.ManagedFileDelete(this.backupPath);
					}
					this.HasPendingChanges = false;
				}
			}
		}

		public void Dispose()
		{
			object obj = this.lockObj;
			lock (obj)
			{
				SaveDataManager.CachedStream.VirtualFileStream virtualFileStream = this.workingCopy;
				if (virtualFileStream != null)
				{
					virtualFileStream.Dispose();
				}
				SaveDataManager.CachedStream.VirtualFileStream virtualFileStream2 = this.commitCopy;
				if (virtualFileStream2 != null)
				{
					virtualFileStream2.Dispose();
				}
				this.workingCopy = null;
				this.commitCopy = null;
				this.HasPendingChanges = false;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime lastAccessTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public DateTime lastWriteTime;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly IPlatformSaveGameProvider platformSaveGameProvider;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly SaveDataManagedPath backupPath;

		[PublicizedFrom(EAccessModifier.Private)]
		public SpinLock sl = new SpinLock(true);

		[PublicizedFrom(EAccessModifier.Private)]
		public SpinLock commitLock = new SpinLock(true);

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveDataManager.CachedStream.VirtualFileStream workingCopy;

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveDataManager.CachedStream.VirtualFileStream commitCopy;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isReading;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool isWriting;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object lockObj = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public class VirtualFileStream : MemoryStream
		{
			public bool ReadEnabled { get; set; }

			public bool WriteEnabled { get; set; }

			public override bool CanRead
			{
				get
				{
					return this.ReadEnabled && base.CanRead;
				}
			}

			public override bool CanWrite
			{
				get
				{
					return this.WriteEnabled && base.CanWrite;
				}
			}

			public VirtualFileStream()
			{
				this.ReadEnabled = true;
				this.WriteEnabled = true;
			}

			public VirtualFileStream(int capacity)
				: base(capacity)
			{
				this.ReadEnabled = true;
				this.WriteEnabled = true;
			}

			public VirtualFileStream(long capacity)
				: base((capacity < 2147483647L) ? ((int)capacity) : int.MaxValue)
			{
				this.ReadEnabled = true;
				this.WriteEnabled = true;
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				if (!this.CanRead)
				{
					throw new NotSupportedException("Stream does not support reading.");
				}
				return base.Read(buffer, offset, count);
			}

			public override int ReadByte()
			{
				if (!this.CanRead)
				{
					throw new NotSupportedException("Stream does not support reading.");
				}
				return base.ReadByte();
			}
		}
	}

	public class SlotSizeData
	{
		public long TotalFileSize
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public long LargestPriorityFileSize
		{
			get
			{
				return this.largestPriorityFile.Item2;
			}
		}

		public int FileCount
		{
			get
			{
				return this.otherFileSizes.Count + this.priorityFileSizes.Count;
			}
		}

		public SlotSizeData(SaveDataSlot saveDataSlot)
		{
			this.saveDataSlot = saveDataSlot;
		}

		public void SetFileSize(SaveDataManagedPath managedPath, long pendingSize)
		{
			long num = pendingSize;
			long num2;
			bool flag;
			if (this.TryGetFileSize(managedPath, out num2, out flag))
			{
				num -= num2;
			}
			else
			{
				flag = SaveDataManager.IsPriorityFilePath(managedPath);
			}
			this.TotalFileSize += num;
			if (flag)
			{
				this.priorityFileSizes[managedPath] = pendingSize;
				this.UpdateLargestPriorityFileSize(managedPath, pendingSize, num);
				return;
			}
			this.otherFileSizes[managedPath] = pendingSize;
		}

		public bool TryGetFileSize(SaveDataManagedPath managedPath, out long trackedSize)
		{
			bool flag;
			return this.TryGetFileSize(managedPath, out trackedSize, out flag);
		}

		public bool TryGetFileSize(SaveDataManagedPath managedPath, out long trackedSize, out bool isPriorityFile)
		{
			isPriorityFile = this.priorityFileSizes.TryGetValue(managedPath, out trackedSize);
			return isPriorityFile || this.otherFileSizes.TryGetValue(managedPath, out trackedSize);
		}

		public void RemoveFileSize(SaveDataManagedPath managedPath)
		{
			long num;
			bool flag;
			if (this.TryGetFileSize(managedPath, out num, out flag))
			{
				this.TotalFileSize -= num;
				if (flag)
				{
					this.priorityFileSizes.Remove(managedPath);
					this.UpdateLargestPriorityFileSize(managedPath, 0L, -num);
					return;
				}
				this.otherFileSizes.Remove(managedPath);
			}
		}

		public void RemoveFileSizes(SaveDataManagedPath parentManagedPath)
		{
			SaveDataManager.SlotSizeData.<>c__DisplayClass17_0 CS$<>8__locals1;
			CS$<>8__locals1.parentManagedPath = parentManagedPath;
			CS$<>8__locals1.<>4__this = this;
			List<SaveDataManagedPath> list = this.priorityFileSizes.Keys.ToList<SaveDataManagedPath>();
			this.<RemoveFileSizes>g__RemoveFileSizesInternal|17_0(list, ref CS$<>8__locals1);
			List<SaveDataManagedPath> list2 = this.otherFileSizes.Keys.ToList<SaveDataManagedPath>();
			this.<RemoveFileSizes>g__RemoveFileSizesInternal|17_0(list2, ref CS$<>8__locals1);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdateLargestPriorityFileSize(SaveDataManagedPath managedPath, long pendingSize, long delta)
		{
			if (delta == 0L)
			{
				return;
			}
			if (pendingSize > this.largestPriorityFile.Item2)
			{
				this.largestPriorityFile = new ValueTuple<SaveDataManagedPath, long>(managedPath, pendingSize);
				return;
			}
			if (delta < 0L && this.largestPriorityFile.Item1 == managedPath)
			{
				this.largestPriorityFile = default(ValueTuple<SaveDataManagedPath, long>);
				foreach (KeyValuePair<SaveDataManagedPath, long> keyValuePair in this.priorityFileSizes)
				{
					if (keyValuePair.Value > this.largestPriorityFile.Item2)
					{
						this.largestPriorityFile = new ValueTuple<SaveDataManagedPath, long>(keyValuePair.Key, pendingSize);
					}
				}
			}
		}

		[CompilerGenerated]
		[PublicizedFrom(EAccessModifier.Private)]
		public void <RemoveFileSizes>g__RemoveFileSizesInternal|17_0(List<SaveDataManagedPath> managedPaths, ref SaveDataManager.SlotSizeData.<>c__DisplayClass17_0 A_2)
		{
			foreach (SaveDataManagedPath saveDataManagedPath in managedPaths)
			{
				if (A_2.parentManagedPath.IsParentOf(saveDataManagedPath))
				{
					this.RemoveFileSize(saveDataManagedPath);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<SaveDataManagedPath, long> priorityFileSizes = new Dictionary<SaveDataManagedPath, long>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<SaveDataManagedPath, long> otherFileSizes = new Dictionary<SaveDataManagedPath, long>();

		[TupleElementNames(new string[] { "path", "size" })]
		[PublicizedFrom(EAccessModifier.Private)]
		public ValueTuple<SaveDataManagedPath, long> largestPriorityFile;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly SaveDataSlot saveDataSlot;
	}
}
