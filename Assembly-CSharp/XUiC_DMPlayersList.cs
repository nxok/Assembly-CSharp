using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DMPlayersList : XUiC_DMBaseList<XUiC_DMPlayersList.ListEntry>
{
	public bool HasBlockedPlayers
	{
		get
		{
			return this.BlockedPlayerCount > 0;
		}
	}

	public int BlockedPlayerCount
	{
		get
		{
			return this.blockedPlayers.Count;
		}
	}

	public IEnumerable<SaveInfoProvider.PlayerEntryInfo> BlockedPlayers
	{
		get
		{
			return this.blockedPlayers;
		}
	}

	public override void Init()
	{
		base.Init();
		this.loadingView.IsVisible = false;
		base.GetChildrenById("btnProfile", this.profileButtons);
		foreach (XUiController xuiController in this.profileButtons)
		{
			xuiController.OnPress += this.ProfileButtonOnPress;
			xuiController.OnHover += base.childElementHovered;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProfileButtonOnPress(XUiController _sender, int _mouseButton)
	{
		for (int i = 0; i < this.profileButtons.Count; i++)
		{
			if (_sender == this.profileButtons[i])
			{
				this.showProfileForEntry(i);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void showProfileForEntry(int _index)
	{
		if (_index < 0 || _index >= this.listEntryControllers.Length)
		{
			Log.Error(string.Format("ProfileButton index out of range. Index: {0}", _index));
			return;
		}
		XUiC_DMPlayersList.ListEntry entry = this.listEntryControllers[_index].GetEntry();
		if (entry == null)
		{
			Log.Error("ProfileButton pressed for empty entry");
			return;
		}
		if (entry.NativeUserId == null)
		{
			Log.Error("ProfileButton pressed for null user id");
			return;
		}
		PlatformManager.MultiPlatform.User.ShowProfile(entry.NativeUserId);
	}

	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(IReadOnlyCollection<SaveInfoProvider.PlayerEntryInfo> _playerEntryInfos, bool _resetFilter = false)
	{
		this.ClearList(false);
		foreach (SaveInfoProvider.PlayerEntryInfo playerEntryInfo in _playerEntryInfos)
		{
			IPlatformUserBlockedData platformUserBlockedData;
			if (playerEntryInfo.PlatformUserData != null && playerEntryInfo.PlatformUserData.Blocked.TryGetValue(EBlockType.Play, out platformUserBlockedData) && platformUserBlockedData.State != EUserBlockState.NotBlocked)
			{
				this.blockedPlayers.Add(playerEntryInfo);
			}
			else
			{
				this.allEntries.Add(new XUiC_DMPlayersList.ListEntry(playerEntryInfo));
			}
		}
		if (this.lblPlayerLimit != null)
		{
			this.lblPlayerLimit.Text = string.Format("{0}/{1}", this.allEntries.Count + this.BlockedPlayerCount, 100);
		}
		this.loadingView.IsVisible = false;
		base.RebuildList(_resetFilter);
	}

	public void ClearList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		if (this.lblPlayerLimit != null)
		{
			this.lblPlayerLimit.Text = string.Empty;
		}
		this.blockedPlayers.Clear();
		base.RebuildList(_resetFilter);
	}

	public void ShowLoading()
	{
		this.loadingView.IsVisible = true;
	}

	[XuiBindComponent("lblPlayerLimit", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblPlayerLimit;

	[XuiBindComponent("loadingOverlay", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView loadingView;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiController> profileButtons = new List<XUiController>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<SaveInfoProvider.PlayerEntryInfo> blockedPlayers = new List<SaveInfoProvider.PlayerEntryInfo>();

	[Preserve]
	public class ListEntry : XUiListEntry<XUiC_DMPlayersList.ListEntry>
	{
		public ListEntry(SaveInfoProvider.PlayerEntryInfo _playerEntryInfo)
		{
			this.PlayerEntryInfo = _playerEntryInfo;
			this.ID = _playerEntryInfo.Id;
			this.CachedName = _playerEntryInfo.CachedName;
			IPlatformUserData platformUserData = _playerEntryInfo.PlatformUserData;
			this.PlayerName = ((platformUserData != null) ? platformUserData.Name : null);
			this.Platform = _playerEntryInfo.PlatformName;
			this.SaveSize = _playerEntryInfo.Size;
			this.LastPlayed = _playerEntryInfo.LastPlayed;
			this.PlayerLevel = _playerEntryInfo.PlayerLevel;
			this.DistanceWalked = _playerEntryInfo.DistanceWalked;
			this.NativeUserId = _playerEntryInfo.NativeUserId;
		}

		public bool CanShowProfile()
		{
			return this.NativeUserId != null && PlatformManager.MultiPlatform.User.CanShowProfile(this.NativeUserId);
		}

		public override int CompareTo(XUiC_DMPlayersList.ListEntry _otherEntry)
		{
			if (_otherEntry != null)
			{
				return this.PlayerEntryInfo.CompareTo(_otherEntry.PlayerEntryInfo);
			}
			return 1;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return (this.PlayerName ?? this.CachedName).ContainsCaseInsensitive(_searchString);
		}

		public readonly string ID;

		public readonly string CachedName;

		public readonly string PlayerName;

		public readonly string Platform;

		public readonly DateTime LastPlayed;

		public readonly int PlayerLevel;

		public readonly float DistanceWalked;

		public readonly long SaveSize;

		public readonly PlatformUserIdentifierAbs NativeUserId;

		public readonly SaveInfoProvider.PlayerEntryInfo PlayerEntryInfo;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_DMPlayersList.ListEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("savename")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveName()
		{
			XUiC_DMPlayersList.ListEntry entryData = this.entryData;
			string text;
			if ((text = ((entryData != null) ? entryData.PlayerName : null)) == null)
			{
				XUiC_DMPlayersList.ListEntry entryData2 = this.entryData;
				text = ((entryData2 != null) ? entryData2.CachedName : null) ?? "";
			}
			return text;
		}

		[XuiXmlBinding("platform")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingPlatform()
		{
			XUiC_DMPlayersList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.Platform : null) ?? "";
		}

		[XuiXmlBinding("lastplayed")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLastPlayed()
		{
			XUiC_DMPlayersList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.LastPlayed.ToString("yyyy-MM-dd HH:mm") : null) ?? "";
		}

		[XuiXmlBinding("lastplayedinfo")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLastPlayedInfo()
		{
			if (this.entryData == null)
			{
				return "";
			}
			int num = (int)(DateTime.Now - this.entryData.LastPlayed).TotalDays;
			return string.Format("{0} {1}", num, Localization.Get("xuiDmDaysAgo", false, null));
		}

		[XuiXmlBinding("level")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLevel()
		{
			if (this.entryData == null)
			{
				return "";
			}
			if (this.entryData.PlayerLevel >= 1)
			{
				return string.Format("{0} {1}", Localization.Get("xuiLevel", false, null), this.entryData.PlayerLevel);
			}
			return "-";
		}

		[XuiXmlBinding("distance")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingDistance()
		{
			if (this.entryData == null)
			{
				return "";
			}
			if (this.entryData.PlayerLevel >= 1)
			{
				return string.Format("{0} {1}", (int)(this.entryData.DistanceWalked / 1000f), Localization.Get("xuiKMTravelled", false, null));
			}
			return "-";
		}

		[XuiXmlBinding("canShowProfile")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingCanShowProfile()
		{
			XUiC_DMPlayersList.ListEntry entryData = this.entryData;
			return entryData != null && entryData.CanShowProfile();
		}
	}
}
