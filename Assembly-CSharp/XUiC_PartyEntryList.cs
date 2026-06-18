using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PartyEntryList : XUiController
{
	public override void Init()
	{
		base.Init();
		XUiC_PartyEntry[] childrenByType = base.GetChildrenByType<XUiC_PartyEntry>(null);
		for (int i = 0; i < childrenByType.Length; i++)
		{
			this.entryList.Add(childrenByType[i]);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.RefreshPartyList();
		EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
		entityPlayer.PartyJoined += this.EntityPlayer_PartyJoined;
		entityPlayer.PartyChanged += this.EntityPlayer_PartyJoined;
		entityPlayer.PartyLeave += this.EntityPlayer_PartyJoined;
		if (entityPlayer.Party != null)
		{
			entityPlayer.Party.PartyMemberAdded += this.Party_PartyMemberChanged;
			entityPlayer.Party.PartyMemberRemoved += this.Party_PartyMemberChanged;
			entityPlayer.Party.PartyLeaderChanged += this.Party_PartyLeaderChanged;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Party_PartyLeaderChanged(Party _affectedParty, EntityPlayer _player)
	{
		this.RefreshPartyList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Party_PartyMemberChanged(EntityPlayer _player)
	{
		this.RefreshPartyList();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EntityPlayer_PartyJoined(Party _affectedParty, EntityPlayer _player)
	{
		this.RefreshPartyList();
		EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
		if (entityPlayer.Party != null)
		{
			entityPlayer.Party.PartyMemberAdded -= this.Party_PartyMemberChanged;
			entityPlayer.Party.PartyMemberRemoved -= this.Party_PartyMemberChanged;
			entityPlayer.Party.PartyLeaderChanged -= this.Party_PartyLeaderChanged;
			entityPlayer.Party.PartyMemberAdded += this.Party_PartyMemberChanged;
			entityPlayer.Party.PartyMemberRemoved += this.Party_PartyMemberChanged;
			entityPlayer.Party.PartyLeaderChanged += this.Party_PartyLeaderChanged;
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
		entityPlayer.PartyJoined -= this.EntityPlayer_PartyJoined;
		entityPlayer.PartyChanged -= this.EntityPlayer_PartyJoined;
		entityPlayer.PartyLeave -= this.EntityPlayer_PartyJoined;
		if (entityPlayer.Party != null)
		{
			entityPlayer.Party.PartyMemberAdded -= this.Party_PartyMemberChanged;
			entityPlayer.Party.PartyMemberRemoved -= this.Party_PartyMemberChanged;
			entityPlayer.Party.PartyLeaderChanged -= this.Party_PartyLeaderChanged;
		}
	}

	public void RefreshPartyList()
	{
		EntityPlayer entityPlayer = this.xui.playerUI.entityPlayer;
		int i = 0;
		if (entityPlayer.Party != null)
		{
			for (int j = 0; j < entityPlayer.Party.MemberList.Count; j++)
			{
				EntityPlayer entityPlayer2 = entityPlayer.Party.MemberList[j];
				if (i >= this.entryList.Count)
				{
					IL_0090:
					while (i < this.entryList.Count)
					{
						this.entryList[i].SetPlayer(null);
						i++;
					}
					return;
				}
				if (entityPlayer2 != entityPlayer)
				{
					this.entryList[i++].SetPlayer(entityPlayer2);
				}
			}
			goto IL_0090;
		}
		for (int k = 0; k < this.entryList.Count; k++)
		{
			this.entryList[k].SetPlayer(null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_PartyEntry> entryList = new List<XUiC_PartyEntry>();
}
