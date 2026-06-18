using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Platform.Shared;
using Platform.XBL.Save;
using Platform.XBL.Save.Storage;
using Unity.XGamingRuntime;

namespace Platform.XBL
{
	public class SaveGameProviderGameCore : IPlatformSaveGameProvider, IPlatformSaveGameIOProvider
	{
		public SaveGameProviderGameCore(long maxStorageSizeBytes = 1073741824L)
		{
			this.m_maxStorageSizeBytes = maxStorageSizeBytes;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnUserHandleReady(XUserHandle userHandle)
		{
			if (userHandle == null)
			{
				Log.Error("[XBL] SaveGameProviderGameCore.OnUserHandleReady: Attempting to retrieve XSTS token before acquiring XUserHandle");
				return;
			}
			if (userHandle.IsInvalid)
			{
				Log.Error("[XBL] SaveGameProviderGameCore.OnUserHandleReady: m_userHandle.IsInvalid is true");
			}
			if (userHandle.IsClosed)
			{
				Log.Error("[XBL] SaveGameProviderGameCore.OnUserHandleReady: m_userHandle.IsClosed is true");
			}
			this.InitializeSaveStorageProvider();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void InitializeSaveStorageProvider()
		{
			SaveStorageProvider value = LaunchPrefs.GameCoreSaveStorageProvider.Value;
			SaveGameProviderGameCore.LogInfo(string.Format("[GDK] Initializing Save Storage Provider '{0}'.", value));
			this.m_saveStorageProvider = value.Create();
			this.m_saveStorageProvider.InitializeAsync(this.m_owner, this.m_maxStorageSizeBytes, this.m_taskScheduler, new OnGameSaveProviderStatusChanged(this.OnGameSaveProviderStatusChanged));
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnGameSaveProviderStatusChanged(ESaveGameProviderStatus status)
		{
			if (status == ESaveGameProviderStatus.Ok && this.m_rootSaveContainer == null)
			{
				this.OnGameSaveProviderReady();
			}
			object initializedDelegateLock = this.m_initializedDelegateLock;
			lock (initializedDelegateLock)
			{
				bool flag2 = status == ESaveGameProviderStatus.Ok && this.Status != ESaveGameProviderStatus.Ok;
				this.Status = status;
				if (flag2)
				{
					Action initializedDelegate = this.m_initializedDelegate;
					if (initializedDelegate != null)
					{
						initializedDelegate();
					}
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnGameSaveProviderReady()
		{
			this.m_rootSaveContainer = new SaveContainer(this.m_saveStorageProvider.RootSaveStorageContainer, this.m_saveStorageProvider.SizeTracker);
			this.ResumeIO();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnApplicationStateChanged(ApplicationState newstate)
		{
			bool flag = newstate == ApplicationState.Suspended;
			if (flag != this.m_suspended)
			{
				this.m_suspended = flag;
				if (flag)
				{
					this.OnSuspend();
					return;
				}
				this.OnResume();
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnSuspend()
		{
			this.PauseIO();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnResume()
		{
			if (!this.Status.IsTerminal())
			{
				SaveGameProviderGameCore.LogWarning("Status was not terminal on resume? Did sleep happen during loading?");
			}
			SaveContainer rootSaveContainer = this.m_rootSaveContainer;
			if (rootSaveContainer != null)
			{
				rootSaveContainer.Dispose();
			}
			this.m_rootSaveContainer = null;
			ISaveStorageProvider saveStorageProvider = this.m_saveStorageProvider;
			if (saveStorageProvider != null)
			{
				saveStorageProvider.Dispose();
			}
			this.m_saveStorageProvider = null;
			this.Status = ESaveGameProviderStatus.Uninitialized;
			this.InitializeSaveStorageProvider();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PauseIO()
		{
			if (this.m_paused)
			{
				SaveGameProviderGameCore.LogError("PauseIO should only be called when IO is not paused.");
				return;
			}
			this.m_paused = true;
			while (this.m_paused && this.m_operations > 0)
			{
				Thread.Sleep(5);
			}
			this.Flush(true);
			if (!this.m_paused)
			{
				SaveGameProviderGameCore.LogWarning("Was un-paused before we finished waiting on IO to pause.");
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ResumeIO()
		{
			if (!this.m_paused)
			{
				SaveGameProviderGameCore.LogError("ResumeIO should only be called when IO is paused.");
				return;
			}
			this.m_paused = false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveGameProviderGameCore.OperationScope CreateOperationScope(string debugIdentifier)
		{
			if (this.m_paused)
			{
				while (this.m_paused)
				{
					Thread.Sleep(50);
				}
			}
			return new SaveGameProviderGameCore.OperationScope(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GetSaveContainerAndRelativePath(SaveDataManagedPath path, out SaveContainer saveContainer, out string relativePath)
		{
			saveContainer = this.m_rootSaveContainer;
			relativePath = path.PathRelativeToRoot;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogInfo(string text)
		{
			Log.Out("[XBL: SaveGameProvider] " + text);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogWarning(string text)
		{
			Log.Warning("[XBL: SaveGameProvider] " + text);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogError(string text)
		{
			Log.Error("[XBL: SaveGameProvider] " + text);
		}

		[Conditional("DEBUG_SAVE_DATA_MANAGER")]
		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogTrace(string text)
		{
			Log.Out("[XBL: SaveGameProvider] " + text);
		}

		public ESaveGameProviderStatus Status
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public IPlatformSaveGameIOProvider Cache
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public event Action Initialized
		{
			add
			{
				object initializedDelegateLock = this.m_initializedDelegateLock;
				lock (initializedDelegateLock)
				{
					this.m_initializedDelegate = (Action)Delegate.Combine(this.m_initializedDelegate, value);
					if (this.Status == ESaveGameProviderStatus.Ok)
					{
						value();
					}
				}
			}
			remove
			{
				object initializedDelegateLock = this.m_initializedDelegateLock;
				lock (initializedDelegateLock)
				{
					this.m_initializedDelegate = (Action)Delegate.Remove(this.m_initializedDelegate, value);
				}
			}
		}

		public void Init(IPlatform _owner)
		{
			this.PauseIO();
			this.m_taskScheduler = new SingleThreadTaskScheduler("GameCore", "Game Core Save Tasks");
			this.m_owner = _owner;
			this.m_appState = _owner.ApplicationState;
			this.m_appState.OnApplicationStateChanged += this.OnApplicationStateChanged;
			this.m_userClient = (User)this.m_owner.User;
			this.m_userClient.UserHandleReady += this.OnUserHandleReady;
			this.Cache = new SaveGameIOProviderFixedRoot(GameIO.GetNormalizedPath(GameIO.GetDeviceLocalUserGameDataDir()) + "Cache");
		}

		public void Destroy()
		{
			if (this.m_rootSaveContainer != null)
			{
				this.m_rootSaveContainer.Dispose();
				this.m_rootSaveContainer = null;
			}
			ISaveStorageProvider saveStorageProvider = this.m_saveStorageProvider;
			if (saveStorageProvider != null)
			{
				saveStorageProvider.Dispose();
			}
			this.m_saveStorageProvider = null;
			if (this.m_userClient != null)
			{
				this.m_userClient.UserHandleReady -= this.OnUserHandleReady;
				this.m_userClient = null;
			}
			if (this.m_appState != null)
			{
				this.m_appState.OnApplicationStateChanged -= this.OnApplicationStateChanged;
				this.m_appState = null;
			}
			this.m_owner = null;
			SingleThreadTaskScheduler taskScheduler = this.m_taskScheduler;
			if (taskScheduler != null)
			{
				taskScheduler.Dispose();
			}
			this.m_taskScheduler = null;
		}

		public bool ShouldBackup()
		{
			return false;
		}

		public bool ShouldCommit()
		{
			return !this.m_paused;
		}

		public double GetCommitProgress()
		{
			return 1.0;
		}

		public void Flush(bool waitForFlush)
		{
			ISaveStorageProvider saveStorageProvider = this.m_saveStorageProvider;
			if (saveStorageProvider == null)
			{
				return;
			}
			saveStorageProvider.Flush(waitForFlush);
		}

		public bool ShouldLimitSize()
		{
			return true;
		}

		public void UpdateSizes()
		{
			using (this.CreateOperationScope("UpdateSizes"))
			{
				this.m_saveStorageProvider.SizeTracker.RefreshSync();
			}
		}

		public SaveDataSizes GetSizes()
		{
			SaveDataSizes sizes;
			using (this.CreateOperationScope("GetSizes"))
			{
				sizes = this.m_saveStorageProvider.SizeTracker.Sizes;
			}
			return sizes;
		}

		public void ManagedFileRead(SaveDataManagedPath path, Stream dest)
		{
			using (this.CreateOperationScope("ManagedFileRead"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(path, out saveContainer, out text);
				saveContainer.FileRead(text, dest);
			}
		}

		public void ManagedFileWrite(SaveDataManagedPath path, Stream src)
		{
			using (this.CreateOperationScope("ManagedFileWrite"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(path, out saveContainer, out text);
				saveContainer.FileWrite(text, src);
			}
		}

		public void ManagedFileCopy(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName, bool overwrite = false)
		{
			using (this.CreateOperationScope("ManagedFileCopy"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(destFileName, out saveContainer, out text);
				if (!overwrite && saveContainer.FileExists(text))
				{
					throw new IOException(string.Format("Destination '{0}' already exists.", destFileName));
				}
				SaveContainer saveContainer2;
				string text2;
				this.GetSaveContainerAndRelativePath(sourceFileName, out saveContainer2, out text2);
				using (MemoryStream memoryStream = new MemoryStream())
				{
					saveContainer2.FileRead(text2, memoryStream);
					memoryStream.Position = 0L;
					saveContainer.FileWrite(text, memoryStream);
				}
			}
		}

		public void ManagedFileDelete(SaveDataManagedPath path)
		{
			using (this.CreateOperationScope("ManagedFileDelete"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(path, out saveContainer, out text);
				saveContainer.FileDelete(text);
			}
		}

		public bool ManagedFileExists(SaveDataManagedPath path)
		{
			bool flag;
			using (this.CreateOperationScope("ManagedFileExists"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(path, out saveContainer, out text);
				flag = saveContainer.FileExists(text);
			}
			return flag;
		}

		public DateTime ManagedFileGetLastWriteTimeUtc(SaveDataManagedPath path)
		{
			DateTime lastWriteTimeUtc;
			using (this.CreateOperationScope("ManagedFileGetLastWriteTimeUtc"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(path, out saveContainer, out text);
				lastWriteTimeUtc = saveContainer.GetLastWriteTimeUtc(text);
			}
			return lastWriteTimeUtc;
		}

		public void ManagedFileMove(SaveDataManagedPath sourceFileName, SaveDataManagedPath destFileName)
		{
			using (this.CreateOperationScope("ManagedFileMove"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(destFileName, out saveContainer, out text);
				if (saveContainer.FileExists(text))
				{
					throw new IOException(string.Format("Destination '{0}' already exists.", destFileName));
				}
				SaveContainer saveContainer2;
				string text2;
				this.GetSaveContainerAndRelativePath(sourceFileName, out saveContainer2, out text2);
				if (saveContainer2 != saveContainer)
				{
					this.ManagedFileCopy(sourceFileName, destFileName, false);
					this.ManagedFileDelete(sourceFileName);
				}
				else
				{
					saveContainer2.FileMove(text2, text);
				}
			}
		}

		public SdDirectoryInfo ManagedDirectoryCreateDirectory(SaveDataManagedPath path)
		{
			SdDirectoryInfo sdDirectoryInfo;
			using (this.CreateOperationScope("ManagedDirectoryCreateDirectory"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(path, out saveContainer, out text);
				saveContainer.DirectoryCreate(text);
				sdDirectoryInfo = new SdDirectoryInfo(path.GetOriginalPath());
			}
			return sdDirectoryInfo;
		}

		public DateTime ManagedDirectoryGetLastWriteTimeUtc(SaveDataManagedPath path)
		{
			DateTime lastWriteTimeUtc;
			using (this.CreateOperationScope("ManagedDirectoryGetLastWriteTimeUtc"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(path, out saveContainer, out text);
				lastWriteTimeUtc = saveContainer.GetLastWriteTimeUtc(text);
			}
			return lastWriteTimeUtc;
		}

		public bool ManagedDirectoryExists(SaveDataManagedPath path)
		{
			bool flag;
			using (this.CreateOperationScope("ManagedDirectoryExists"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(path, out saveContainer, out text);
				flag = saveContainer.DirectoryExists(text);
			}
			return flag;
		}

		public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			IEnumerable<SaveDataManagedPath> enumerable;
			using (this.CreateOperationScope("ManagedDirectoryEnumerateDirectories"))
			{
				enumerable = this.ManagedDirectoryEnumerateInternal(path, searchPattern, searchOption, true, false).ToArray<SaveDataManagedPath>();
			}
			return enumerable;
		}

		public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			IEnumerable<SaveDataManagedPath> enumerable;
			using (this.CreateOperationScope("ManagedDirectoryEnumerateFiles"))
			{
				enumerable = this.ManagedDirectoryEnumerateInternal(path, searchPattern, searchOption, false, true).ToArray<SaveDataManagedPath>();
			}
			return enumerable;
		}

		public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateFileSystemEntries(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			IEnumerable<SaveDataManagedPath> enumerable;
			using (this.CreateOperationScope("ManagedDirectoryEnumerateFileSystemEntries"))
			{
				enumerable = this.ManagedDirectoryEnumerateInternal(path, searchPattern, searchOption, true, true).ToArray<SaveDataManagedPath>();
			}
			return enumerable;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerable<SaveDataManagedPath> ManagedDirectoryEnumerateInternal(SaveDataManagedPath path, string searchPattern, SearchOption searchOption, bool includeDirectories, bool includeFiles)
		{
			SaveContainer saveContainer;
			string text;
			this.GetSaveContainerAndRelativePath(path, out saveContainer, out text);
			IEnumerable<string> enumerable = saveContainer.DirectoryEnumerate(text, searchPattern, searchOption == SearchOption.AllDirectories, includeDirectories, includeFiles);
			return SaveGameProviderHelper.GetManagedPathsFromBaseAndSubPaths(path, text, enumerable);
		}

		public void ManagedDirectoryDelete(SaveDataManagedPath path, bool recursive)
		{
			using (this.CreateOperationScope("ManagedDirectoryDelete"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(path, out saveContainer, out text);
				saveContainer.DirectoryDelete(text, recursive);
			}
		}

		public long ManagedFileInfoLength(SaveDataManagedPath path)
		{
			long num;
			using (this.CreateOperationScope("ManagedFileInfoLength"))
			{
				SaveContainer saveContainer;
				string text;
				this.GetSaveContainerAndRelativePath(path, out saveContainer, out text);
				num = saveContainer.FileLength(text);
			}
			return num;
		}

		public IEnumerable<SdDirectoryInfo> ManagedDirectoryInfoEnumerateDirectories(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			IEnumerable<SdDirectoryInfo> enumerable;
			using (this.CreateOperationScope("ManagedDirectoryInfoEnumerateDirectories"))
			{
				enumerable = (from managedPath in this.ManagedDirectoryEnumerateInternal(path, searchPattern, searchOption, true, false)
					select new SdDirectoryInfo(managedPath)).ToArray<SdDirectoryInfo>();
			}
			return enumerable;
		}

		public IEnumerable<SdFileInfo> ManagedDirectoryInfoEnumerateFiles(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			IEnumerable<SdFileInfo> enumerable;
			using (this.CreateOperationScope("ManagedDirectoryInfoEnumerateFiles"))
			{
				enumerable = (from managedPath in this.ManagedDirectoryEnumerateInternal(path, searchPattern, searchOption, false, true)
					select new SdFileInfo(managedPath)).ToArray<SdFileInfo>();
			}
			return enumerable;
		}

		public IEnumerable<SdFileSystemInfo> ManagedDirectoryInfoEnumerateFileSystemInfos(SaveDataManagedPath path, string searchPattern, SearchOption searchOption)
		{
			IEnumerable<SdFileSystemInfo> enumerable;
			using (this.CreateOperationScope("ManagedDirectoryInfoEnumerateFileSystemInfos"))
			{
				string relativePath;
				SaveContainer saveContainer;
				this.GetSaveContainerAndRelativePath(path, out saveContainer, out relativePath);
				enumerable = saveContainer.DirectoryEnumerate(relativePath, searchPattern, searchOption == SearchOption.AllDirectories).Select(delegate(PathEnumerationInfo x)
				{
					SaveDataManagedPath managedPathFromBaseAndSubPath = SaveGameProviderHelper.GetManagedPathFromBaseAndSubPath(path, relativePath, x.RelativePath);
					if (!x.IsDirectory)
					{
						return new SdFileInfo(managedPathFromBaseAndSubPath);
					}
					return new SdDirectoryInfo(managedPathFromBaseAndSubPath);
				}).ToArray<SdFileSystemInfo>();
			}
			return enumerable;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public SingleThreadTaskScheduler m_taskScheduler;

		[PublicizedFrom(EAccessModifier.Private)]
		public Action m_initializedDelegate;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly object m_initializedDelegateLock = new object();

		[PublicizedFrom(EAccessModifier.Private)]
		public IPlatform m_owner;

		[PublicizedFrom(EAccessModifier.Private)]
		public IApplicationStateController m_appState;

		[PublicizedFrom(EAccessModifier.Private)]
		public User m_userClient;

		[PublicizedFrom(EAccessModifier.Private)]
		public long m_maxStorageSizeBytes;

		[PublicizedFrom(EAccessModifier.Private)]
		public ISaveStorageProvider m_saveStorageProvider;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_suspended;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_paused;

		[PublicizedFrom(EAccessModifier.Private)]
		public int m_operations;

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveContainer m_rootSaveContainer;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly struct OperationScope : IDisposable
		{
			public OperationScope(SaveGameProviderGameCore provider)
			{
				this.m_provider = provider;
				Interlocked.Increment(ref this.m_provider.m_operations);
			}

			public void Dispose()
			{
				Interlocked.Decrement(ref this.m_provider.m_operations);
			}

			[PublicizedFrom(EAccessModifier.Private)]
			public readonly SaveGameProviderGameCore m_provider;
		}
	}
}
