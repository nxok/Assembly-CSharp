using System;
using System.Collections;
using Platform;
using UnityEngine;

public class TitleStorageOverridesManager
{
	public static TitleStorageOverridesManager Instance
	{
		get
		{
			TitleStorageOverridesManager titleStorageOverridesManager;
			if ((titleStorageOverridesManager = TitleStorageOverridesManager.instance) == null)
			{
				titleStorageOverridesManager = (TitleStorageOverridesManager.instance = new TitleStorageOverridesManager());
			}
			return titleStorageOverridesManager;
		}
	}

	[method: PublicizedFrom(EAccessModifier.Private)]
	public event Action<TitleStorageOverridesManager.TSOverrides> fetchFinished;

	public void FetchFromSource(Action<TitleStorageOverridesManager.TSOverrides> _callback)
	{
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		if (crossplatformPlatform == null || crossplatformPlatform.PlatformIdentifier != EPlatformIdentifier.EOS)
		{
			if (_callback != null)
			{
				_callback(new TitleStorageOverridesManager.TSOverrides
				{
					Crossplay = true
				});
			}
			return;
		}
		bool flag = false;
		object obj = this.fetchLock;
		lock (obj)
		{
			if (!this.fetching)
			{
				flag = true;
				this.fetching = true;
			}
			this.fetchFinished += _callback;
		}
		if (flag)
		{
			ThreadManager.StartCoroutine(this.RequestDataCo());
		}
	}

	public void ClearOverrides()
	{
		this.overrides = default(TitleStorageOverridesManager.TSOverrides);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetLocalPlatformNetworkString()
	{
		if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
		{
			if (PlatformManager.NativePlatform.PlatformIdentifier == EPlatformIdentifier.Steam)
			{
				return "Standalone_Steam";
			}
			if (PlatformManager.NativePlatform.PlatformIdentifier == EPlatformIdentifier.XBL)
			{
				return "Standalone_XBL";
			}
		}
		else
		{
			if (DeviceFlag.XBoxSeriesX.IsCurrent())
			{
				return "XboxSeriesX_XBL";
			}
			if (DeviceFlag.XBoxSeriesS.IsCurrent())
			{
				return "XboxSeriesS_XBL";
			}
			if (DeviceFlag.PS5.IsCurrent())
			{
				return "PS5_PSN";
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator RequestDataCo()
	{
		TitleStorageOverridesManager.<>c__DisplayClass15_0 CS$<>8__locals1 = new TitleStorageOverridesManager.<>c__DisplayClass15_0();
		CS$<>8__locals1.<>4__this = this;
		try
		{
			if ((DateTime.Now - this.lastSuccess).TotalMinutes < 5.0)
			{
				Log.Out("[TitleStorageOverridesManager] Using cached last success.");
				yield break;
			}
			IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
			if (storage == null)
			{
				Log.Warning("[TitleStorageOverridesManager] No remote file storage implementation available.");
				yield break;
			}
			bool loggedSlow = false;
			float startTime = Time.time;
			while (!storage.IsReady)
			{
				if (storage.Unavailable)
				{
					Log.Warning("[TitleStorageOverridesManager] Remote Storage is unavailable");
					this.ClearOverrides();
					yield break;
				}
				yield return null;
				if (!loggedSlow && Time.time > startTime + 30f)
				{
					loggedSlow = true;
					Log.Warning("[TitleStorageOverridesManager] Waiting for title storage overrides from remote storage exceeded 30s");
				}
			}
			CS$<>8__locals1.fileDownloadComplete = false;
			storage.GetFile("PlatformOverrides", new IRemoteFileStorage.FileDownloadCompleteCallback(CS$<>8__locals1.<RequestDataCo>g__fileDownloadedCallback|0));
			while (!CS$<>8__locals1.fileDownloadComplete)
			{
				yield return null;
			}
			storage = null;
		}
		finally
		{
			object obj = this.fetchLock;
			lock (obj)
			{
				Action<TitleStorageOverridesManager.TSOverrides> action = this.fetchFinished;
				if (action != null)
				{
					action(this.overrides);
				}
				this.fetchFinished = null;
				this.fetching = false;
			}
		}
		yield break;
		yield break;
	}

	public const string RFSUri = "PlatformOverrides";

	[PublicizedFrom(EAccessModifier.Private)]
	public static TitleStorageOverridesManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime lastSuccess = DateTime.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object fetchLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool fetching;

	[PublicizedFrom(EAccessModifier.Private)]
	public TitleStorageOverridesManager.TSOverrides overrides;

	public struct TSOverrides
	{
		public bool Crossplay;
	}
}
