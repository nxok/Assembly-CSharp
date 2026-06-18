using System;
using System.IO;
using Unity.XGamingRuntime;
using Unity.XGamingRuntime.Interop;

namespace Platform.XBL.Save.Storage.Files
{
	public sealed class SaveStorageStorageProviderFiles : ISaveStorageProvider, IDisposable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogInfo(string text)
		{
			Log.Out("[XBL: SaveStorageStorageProviderFiles] " + text);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void LogError(string text)
		{
			Log.Error("[XBL: SaveStorageStorageProviderFiles] " + text);
		}

		public void Dispose()
		{
			SaveStorageStorageContainerFiles rootSaveStorageContainer = this.m_rootSaveStorageContainer;
			if (rootSaveStorageContainer != null)
			{
				rootSaveStorageContainer.Dispose();
			}
			this.m_rootSaveStorageContainer = null;
			SizeTracker sizeTracker = this.m_sizeTracker;
			if (sizeTracker != null)
			{
				sizeTracker.Dispose();
			}
			this.m_sizeTracker = null;
			this.m_gameSaveDirectory = null;
			this.m_statusChanged = null;
			this.m_taskScheduler = null;
			this.m_userClient = null;
			this.m_api = null;
			this.m_isDisposed = true;
		}

		public SizeTracker SizeTracker
		{
			get
			{
				return this.m_sizeTracker;
			}
		}

		public ISaveStorageContainer RootSaveStorageContainer
		{
			get
			{
				return this.m_rootSaveStorageContainer;
			}
		}

		public void InitializeAsync(IPlatform owner, long maxSizeBytes, SingleThreadTaskScheduler taskScheduler, OnGameSaveProviderStatusChanged statusChanged)
		{
			this.m_api = (XblPlatformApi)owner.Api;
			this.m_userClient = (User)owner.User;
			this.m_maxSizeBytes = maxSizeBytes;
			this.m_taskScheduler = taskScheduler;
			this.m_statusChanged = statusChanged;
			SDK.XGameSaveFilesGetFolderWithUiAsync(this.m_userClient.UserHandle, this.m_api.SCID, new XGameSaveFilesGetFolderWithUiCompleted(this.OnGameSaveFilesGetFolderWithUiCompleted));
		}

		public void Flush(bool waitForFlush)
		{
			SaveStorageStorageContainerFiles rootSaveStorageContainer = this.m_rootSaveStorageContainer;
			if (rootSaveStorageContainer == null)
			{
				return;
			}
			rootSaveStorageContainer.Flush(waitForFlush);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void GetRemainingQuotaAsync(SizeTrackerGetRemainingQuotaCompleted completionRoutine)
		{
			this.m_taskScheduler.ExecuteNoWait(delegate
			{
				ulong num2;
				int num = SDK.XGameSaveFilesGetRemainingQuota(this.m_userClient.UserHandle, this.m_api.SCID, ref num2);
				completionRoutine(num, (long)num2);
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnGameSaveFilesGetFolderWithUiCompleted(int hr, string folderResult)
		{
			XblHelpers.LogHR(hr, "Initialize Game Saves (Files).", false);
			if (this.m_isDisposed)
			{
				SaveStorageStorageProviderFiles.LogInfo("Disposed before XGameSaveFilesGetFolderWithUiAsync completed, cancelling setup");
				return;
			}
			if (!HR.FAILED(hr))
			{
				this.m_gameSaveDirectory = Path.GetFullPath(folderResult);
				SaveStorageStorageProviderFiles.LogInfo("Game Save Files Directory: " + this.m_gameSaveDirectory);
				this.m_taskScheduler.ExecuteNoWait(new Action(this.OnGameSaveDirectoryReady));
				return;
			}
			if (hr == -2138898428)
			{
				this.m_statusChanged(ESaveGameProviderStatus.TemporaryError);
				SaveStorageStorageProviderFiles.LogInfo("User cancelled sync, retrying...");
				SDK.XGameSaveFilesGetFolderWithUiAsync(this.m_userClient.UserHandle, this.m_api.SCID, new XGameSaveFilesGetFolderWithUiCompleted(this.OnGameSaveFilesGetFolderWithUiCompleted));
				return;
			}
			SaveStorageStorageProviderFiles.LogError("Could not Initialize. This is a FATAL error!");
			this.m_statusChanged(ESaveGameProviderStatus.PermanentError);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void OnGameSaveDirectoryReady()
		{
			this.m_sizeTracker = new SizeTracker(this.m_maxSizeBytes, 0L, new SizeTrackerGetRemainingQuotaAsync(this.GetRemainingQuotaAsync), false);
			this.m_sizeTracker.RefreshSync();
			this.m_rootSaveStorageContainer = new SaveStorageStorageContainerFiles(this.m_gameSaveDirectory, "root");
			this.m_statusChanged(ESaveGameProviderStatus.Ok);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public XblPlatformApi m_api;

		[PublicizedFrom(EAccessModifier.Private)]
		public User m_userClient;

		[PublicizedFrom(EAccessModifier.Private)]
		public SingleThreadTaskScheduler m_taskScheduler;

		[PublicizedFrom(EAccessModifier.Private)]
		public OnGameSaveProviderStatusChanged m_statusChanged;

		[PublicizedFrom(EAccessModifier.Private)]
		public string m_gameSaveDirectory;

		[PublicizedFrom(EAccessModifier.Private)]
		public long m_maxSizeBytes;

		[PublicizedFrom(EAccessModifier.Private)]
		public SizeTracker m_sizeTracker;

		[PublicizedFrom(EAccessModifier.Private)]
		public SaveStorageStorageContainerFiles m_rootSaveStorageContainer;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_isDisposed;
	}
}
