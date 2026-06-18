using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BlockedPlayersList : XUiController
{
	[XuiXmlBinding("blockedcount")]
	public int BlockedEntriesCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (BlockedPlayerList.Instance != null)
			{
				return BlockedPlayerList.Instance.EntryCount(true, false);
			}
			return 0;
		}
	}

	[XuiXmlBinding("blockedmax")]
	public int MaxBlockedEntries
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return 500;
		}
	}

	[XuiXmlBinding("recentcount")]
	public int RecentEntriesCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (BlockedPlayerList.Instance != null)
			{
				return BlockedPlayerList.Instance.EntryCount(false, false);
			}
			return 0;
		}
	}

	[XuiXmlBinding("recentmax")]
	public int MaxRecentEntries
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return 100;
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (BlockedPlayerList.Instance == null)
		{
			return;
		}
		this.blockedPager.Reset();
		this.recentPager.Reset();
		BlockedPlayerList.Instance.UpdatePlayersSeenInWorld(GameManager.Instance.World);
		ThreadManager.StartCoroutine(BlockedPlayerList.Instance.ResolveUserDetails());
		this.updateBlockedList();
		this.updateRecentList();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (BlockedPlayerList.Instance == null)
		{
			return;
		}
		if (!this.IsDirty)
		{
			return;
		}
		this.IsDirty = false;
		this.updateBlockedList();
		this.updateRecentList();
		base.RefreshBindings();
	}

	public void DisplayMessage(string _header, string _message)
	{
		XUiC_MessageBoxWindowGroup.ShowOk(this.xui, _header, _message, null, false, false, false);
	}

	[XuiBindEvent("OnPageChanged", "blockedPager")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBlockedList()
	{
		for (int i = 0; i < this.blockedPlayerList.Rows; i++)
		{
			this.blockedEntries[i].Clear();
		}
		if (BlockedPlayerList.Instance.PendingResolve())
		{
			this.blockedEntries[0].PlayerName.SetGenericName(Localization.Get("xuiFetchingData", false, null));
			this.IsDirty = true;
			return;
		}
		this.blockedPager.SetLastPageByElementsAndPageLength(BlockedPlayerList.Instance.EntryCount(true, true), this.blockedPlayerList.Rows);
		int num = this.blockedPlayerList.Rows * this.blockedPager.GetPage();
		int num2 = 0;
		foreach (BlockedPlayerList.ListEntry listEntry in BlockedPlayerList.Instance.GetEntriesOrdered(true, true))
		{
			if (num2 < num)
			{
				num2++;
			}
			else
			{
				int num3 = num2 - num;
				if (num3 >= this.blockedPlayerList.Rows)
				{
					break;
				}
				this.blockedEntries[num3].UpdateEntry(listEntry.PlayerData.PrimaryId);
				num2++;
			}
		}
		for (int j = num2 - num; j < this.blockedPlayerList.Rows; j++)
		{
			this.blockedEntries[j].Clear();
		}
	}

	[XuiBindEvent("OnPageChanged", "recentPager")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRecentList()
	{
		for (int i = 0; i < this.recentPlayerList.Rows; i++)
		{
			this.recentEntries[i].Clear();
		}
		if (BlockedPlayerList.Instance.PendingResolve())
		{
			this.recentEntries[0].PlayerName.SetGenericName(Localization.Get("xuiFetchingData", false, null));
			this.IsDirty = true;
			return;
		}
		this.recentPager.SetLastPageByElementsAndPageLength(BlockedPlayerList.Instance.EntryCount(false, true), this.recentPlayerList.Rows);
		int num = this.recentPlayerList.Rows * this.recentPager.GetPage();
		int num2 = 0;
		foreach (BlockedPlayerList.ListEntry listEntry in BlockedPlayerList.Instance.GetEntriesOrdered(false, true))
		{
			if (num2 < num)
			{
				num2++;
			}
			else
			{
				int num3 = num2 - num;
				if (num3 >= this.recentPlayerList.Rows)
				{
					break;
				}
				this.recentEntries[num3].UpdateEntry(listEntry.PlayerData.PrimaryId);
				num2++;
			}
		}
		for (int j = num2 - num; j < this.recentPlayerList.Rows; j++)
		{
			this.recentEntries[j].Clear();
		}
	}

	[XuiBindComponent("blockList", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Grid blockedPlayerList;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_PlayersBlockedListEntry[] blockedEntries;

	[XuiBindComponent("blockedPager", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Paging blockedPager;

	[XuiBindComponent("recentList", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Grid recentPlayerList;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_PlayersRecentListEntry[] recentEntries;

	[XuiBindComponent("recentPager", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Paging recentPager;
}
