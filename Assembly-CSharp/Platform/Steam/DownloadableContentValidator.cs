using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using UnityEngine;

namespace Platform.Steam
{
	public class DownloadableContentValidator : IEntitlementValidator
	{
		public void Init(IPlatform _owner)
		{
		}

		public bool IsAvailableOnPlatform(EntitlementSetEnum _dlcSet)
		{
			return _dlcSet == EntitlementSetEnum.None || DownloadableContentValidator.entitlementMap.ContainsKey(_dlcSet);
		}

		public bool IsEntitlementPurchasable(EntitlementSetEnum _dlcSet)
		{
			return DLCTitleStorageManager.Instance.IsDLCPurchasable(_dlcSet, DLCEnvironmentFlags.Dev | DLCEnvironmentFlags.Cert | DLCEnvironmentFlags.Retail);
		}

		public bool HasEntitlement(EntitlementSetEnum _dlcSet)
		{
			if (_dlcSet == EntitlementSetEnum.None)
			{
				return true;
			}
			if (!DownloadableContentValidator.entitlementMap.ContainsKey(_dlcSet))
			{
				return false;
			}
			if (!XUiC_MainMenu.openedOnce)
			{
				Log.Out("[DownloadableContentValidator] Ignored, game not fully loaded yet");
				return false;
			}
			return SteamApps.BIsDlcInstalled(new AppId_t(DownloadableContentValidator.entitlementMap[_dlcSet]));
		}

		public bool OpenStore(EntitlementSetEnum _dlcSet, Action<EntitlementSetEnum> _onDlcPurchased)
		{
			if (!DownloadableContentValidator.entitlementMap.ContainsKey(_dlcSet))
			{
				return false;
			}
			if (!XUiC_MainMenu.openedOnce)
			{
				Log.Out("[DownloadableContentValidator] Ignored, game not fully loaded yet");
				return true;
			}
			SteamFriends.ActivateGameOverlayToStore(new AppId_t(DownloadableContentValidator.entitlementMap[_dlcSet]), 2);
			this.pendingDlcChecks.Add(_dlcSet);
			this.dlcPurchaseCallbacks[_dlcSet] = _onDlcPurchased;
			if (this.pendingDlcChecks.Count == 1)
			{
				ThreadManager.StartCoroutine(this.CheckDlcPurchases());
			}
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator CheckDlcPurchases()
		{
			while (this.pendingDlcChecks.Count > 0)
			{
				yield return new WaitForSeconds(2f);
				List<EntitlementSetEnum> list = new List<EntitlementSetEnum>();
				foreach (EntitlementSetEnum entitlementSetEnum in this.pendingDlcChecks)
				{
					if (SteamApps.BIsDlcInstalled(new AppId_t(DownloadableContentValidator.entitlementMap[entitlementSetEnum])))
					{
						list.Add(entitlementSetEnum);
					}
				}
				foreach (EntitlementSetEnum entitlementSetEnum2 in list)
				{
					this.pendingDlcChecks.Remove(entitlementSetEnum2);
					if (this.dlcPurchaseCallbacks.ContainsKey(entitlementSetEnum2))
					{
						Action<EntitlementSetEnum> action = this.dlcPurchaseCallbacks[entitlementSetEnum2];
						if (action != null)
						{
							action(entitlementSetEnum2);
						}
						this.dlcPurchaseCallbacks.Remove(entitlementSetEnum2);
					}
				}
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly Dictionary<EntitlementSetEnum, uint> entitlementMap = new Dictionary<EntitlementSetEnum, uint>
		{
			{
				EntitlementSetEnum.MarauderCosmetic,
				3486400U
			},
			{
				EntitlementSetEnum.HoarderCosmetic,
				3314750U
			},
			{
				EntitlementSetEnum.DesertCosmetic,
				3635260U
			},
			{
				EntitlementSetEnum.ClassicSurvivorCosmetic,
				4206290U
			},
			{
				EntitlementSetEnum.PirateCosmetic,
				4234590U
			},
			{
				EntitlementSetEnum.ChristmasCosmetics,
				4234580U
			},
			{
				EntitlementSetEnum.HellreaverCosmetic,
				4234570U
			},
			{
				EntitlementSetEnum.ButcherCosmetic,
				4234560U
			},
			{
				EntitlementSetEnum.BeachCosmetic,
				4767590U
			},
			{
				EntitlementSetEnum.WorkingStiffCosmetic,
				4767600U
			}
		};

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<EntitlementSetEnum> pendingDlcChecks = new HashSet<EntitlementSetEnum>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Dictionary<EntitlementSetEnum, Action<EntitlementSetEnum>> dlcPurchaseCallbacks = new Dictionary<EntitlementSetEnum, Action<EntitlementSetEnum>>();
	}
}
