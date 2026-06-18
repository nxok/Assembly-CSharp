using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Platform
{
	public class DLCTitleStorageManager
	{
		public static DLCTitleStorageManager Instance
		{
			get
			{
				DLCTitleStorageManager dlctitleStorageManager;
				if ((dlctitleStorageManager = DLCTitleStorageManager.instance) == null)
				{
					dlctitleStorageManager = (DLCTitleStorageManager.instance = new DLCTitleStorageManager());
				}
				return dlctitleStorageManager;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public DLCTitleStorageManager()
		{
		}

		public bool IsDLCPurchasable(EntitlementSetEnum _dlcSet, DLCEnvironmentFlags _dlcEnvironments)
		{
			DLCTitleStorageManager.CatalogEntry catalogEntry;
			return _dlcSet != EntitlementSetEnum.None && this.dlcPurchasability.TryGetValue(_dlcSet, out catalogEntry) && (catalogEntry.environments & _dlcEnvironments) != DLCEnvironmentFlags.None && (!_dlcEnvironments.HasFlag(DLCEnvironmentFlags.Retail) || catalogEntry.retailDate == null || !(catalogEntry.retailDate.Value > DateTime.Now));
		}

		public void FetchFromSource()
		{
			if (!this.fetchAttempted)
			{
				IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
				if (crossplatformPlatform != null && crossplatformPlatform.PlatformIdentifier == EPlatformIdentifier.EOS)
				{
					this.fetchAttempted = true;
					PlatformManager.NativePlatform.User.UserLoggedIn += delegate(IPlatform _)
					{
						ThreadManager.StartCoroutine(this.RequestDataCo());
					};
					return;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator RequestDataCo()
		{
			DLCTitleStorageManager.<>c__DisplayClass10_0 CS$<>8__locals1 = new DLCTitleStorageManager.<>c__DisplayClass10_0();
			CS$<>8__locals1.<>4__this = this;
			IRemoteFileStorage storage = PlatformManager.MultiPlatform.RemoteFileStorage;
			if (storage == null)
			{
				Log.Warning("[DLCTitleStorageManager] No remote file storage implementation available.");
				yield break;
			}
			bool loggedSlow = false;
			float startTime = Time.time;
			while (!storage.IsReady)
			{
				if (storage.Unavailable)
				{
					Log.Warning("[DLCTitleStorageManager] Remote Storage is unavailable");
					yield break;
				}
				yield return null;
				if (!loggedSlow && Time.time > startTime + 30f)
				{
					loggedSlow = true;
					Log.Warning("[DLCTitleStorageManager] Waiting for DLC configuration from remote storage exceeded 30s");
				}
			}
			CS$<>8__locals1.fileDownloadComplete = false;
			storage.GetFile("DLCConfiguration", new IRemoteFileStorage.FileDownloadCompleteCallback(CS$<>8__locals1.<RequestDataCo>g__fileDownloadedCallback|0));
			while (!CS$<>8__locals1.fileDownloadComplete)
			{
				yield return null;
			}
			yield break;
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
				if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent())
				{
					return "XboxSeries_XBL";
				}
				if (DeviceFlag.PS5.IsCurrent())
				{
					return "PS5_PSN";
				}
			}
			return null;
		}

		public const string RFSUri = "DLCConfiguration";

		[PublicizedFrom(EAccessModifier.Private)]
		public static DLCTitleStorageManager instance;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<EntitlementSetEnum, DLCTitleStorageManager.CatalogEntry> dlcPurchasability = new Dictionary<EntitlementSetEnum, DLCTitleStorageManager.CatalogEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool fetchAttempted;

		public struct CatalogEntry
		{
			public DLCEnvironmentFlags environments;

			public DateTime? retailDate;
		}
	}
}
