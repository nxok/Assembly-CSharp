using System;
using System.Collections;
using UnityEngine;

public class PartyQuests
{
	public static PartyQuests Instance
	{
		get
		{
			PartyQuests partyQuests;
			if ((partyQuests = PartyQuests.instance) == null)
			{
				partyQuests = (PartyQuests.instance = new PartyQuests());
			}
			return partyQuests;
		}
	}

	public static void EnforeInstance()
	{
		PartyQuests partyQuests = PartyQuests.Instance;
	}

	public static bool AutoShare
	{
		get
		{
			return GamePrefs.GetBool(EnumGamePrefs.OptionsQuestsAutoShare);
		}
	}

	public static bool AutoAccept
	{
		get
		{
			return GamePrefs.GetBool(EnumGamePrefs.OptionsQuestsAutoAccept);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PartyQuests()
	{
		GameManager.Instance.OnLocalPlayerChanged += this.localPlayerChangedEvent;
		World world = GameManager.Instance.World;
		EntityPlayerLocal entityPlayerLocal = ((world != null) ? world.GetPrimaryPlayer() : null);
		if (entityPlayerLocal != null)
		{
			this.gameStarted(entityPlayerLocal);
		}
		Log.Out("[PartyQuests] Initialized");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void localPlayerChangedEvent(EntityPlayerLocal _newLocalPlayer)
	{
		if (_newLocalPlayer == null)
		{
			this.gameEnded();
			return;
		}
		this.gameStarted(_newLocalPlayer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameStarted(EntityPlayerLocal _newLocalPlayer)
	{
		this.localPlayer = _newLocalPlayer;
		this.localPlayer.PartyJoined += this.playerJoinedParty;
		this.localPlayer.QuestAccepted += this.newQuestAccepted;
		this.localPlayer.SharedQuestAdded += this.sharedQuestReceived;
		Log.Out(string.Format("[PartyQuests] Player registered: {0}", _newLocalPlayer));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameEnded()
	{
		if (this.localPlayer != null)
		{
			this.localPlayer.PartyJoined -= this.playerJoinedParty;
			this.localPlayer.QuestAccepted -= this.newQuestAccepted;
			this.localPlayer.SharedQuestAdded -= this.sharedQuestReceived;
		}
		this.localPlayer = null;
		Log.Out("[PartyQuests] Player unregistered");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void playerJoinedParty(Party _affectedParty, EntityPlayer _player)
	{
		if (PartyQuests.AutoShare)
		{
			ThreadManager.StartCoroutine(this.shareQuestsLater());
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator shareQuestsLater()
	{
		yield return PartyQuests.sendQuestsDelay;
		PartyQuests.ShareAllQuestsWithParty(this.localPlayer);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void newQuestAccepted(Quest _q)
	{
		if (PartyQuests.AutoShare && _q.IsShareable)
		{
			PartyQuests.logQuest("Auto-sharing new quest", _q);
			ThreadManager.StartCoroutine(this.shareQuestLater(_q));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator shareQuestLater(Quest _q)
	{
		yield return PartyQuests.sendQuestsDelay;
		PartyQuests.ShareQuestWithParty(_q, this.localPlayer, false);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void sharedQuestReceived(SharedQuestEntry _entry)
	{
		if (PartyQuests.AutoAccept)
		{
			int sharedByPlayerID = _entry.SharedByPlayerID;
			string text = "-unknown-";
			EntityPlayer entityPlayer;
			if (GameManager.Instance.World.Players.dict.TryGetValue(sharedByPlayerID, out entityPlayer))
			{
				text = entityPlayer.EntityName;
			}
			PartyQuests.logQuest("Received shared quest from " + text, _entry.Quest);
			PartyQuests.AcceptSharedQuest(_entry, this.localPlayer);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void logQuest(string _prefix, Quest _q)
	{
		Log.Out(string.Format("[PartyQuests] {0}: id={1}, code={2}, name={3}, POI {4}", new object[]
		{
			_prefix,
			_q.ID,
			_q.QuestCode,
			_q.QuestClass.Name,
			_q.GetParsedText("{poi.name}")
		}));
	}

	public static void ShareAllQuestsWithParty(EntityPlayerLocal _localPlayer)
	{
		if (_localPlayer == null)
		{
			return;
		}
		foreach (Quest quest in _localPlayer.QuestJournal.quests)
		{
			if (quest.IsShareable)
			{
				PartyQuests.logQuest("Auto-sharing quest with new party", quest);
				PartyQuests.ShareQuestWithParty(quest, _localPlayer, false);
			}
		}
	}

	public static void ShareQuestWithParty(Quest _selectedQuest, EntityPlayerLocal _localPlayer, bool _showTooltips)
	{
		if (_selectedQuest == null)
		{
			if (_showTooltips)
			{
				GameManager.ShowTooltip(_localPlayer, Localization.Get("ttQuestShareNoQuest", false, null), false, false, 0f);
			}
			return;
		}
		if (!_selectedQuest.IsShareable)
		{
			return;
		}
		if (!_localPlayer.IsInParty())
		{
			if (_showTooltips)
			{
				GameManager.ShowTooltip(_localPlayer, Localization.Get("ttQuestShareNoParty", false, null), false, false, 0f);
			}
			return;
		}
		_selectedQuest.SetupQuestCode();
		int num = 0;
		for (int i = 0; i < _localPlayer.Party.MemberList.Count; i++)
		{
			EntityPlayer entityPlayer = _localPlayer.Party.MemberList[i];
			if (!(entityPlayer == _localPlayer))
			{
				if (_selectedQuest.HasSharedWith(entityPlayer))
				{
					if (PartyQuests.AutoShare)
					{
						Log.Out("[PartyQuests] Not sharing with party member " + entityPlayer.EntityName + ", already shared");
					}
				}
				else
				{
					Vector3 vector;
					_selectedQuest.GetPositionData(out vector, Quest.PositionDataTypes.QuestGiver);
					GameManager.Instance.QuestShareServer(new NetPackageSharedQuest.SharedQuestData(_selectedQuest.QuestCode, _selectedQuest.ID, _selectedQuest.GetPOIName(), _selectedQuest.GetLocation(), _selectedQuest.GetLocationSize(), vector, _localPlayer.entityId, entityPlayer.entityId, _selectedQuest.QuestGiverID));
					num++;
					if (PartyQuests.AutoShare)
					{
						Log.Out("[PartyQuests] Shared with party member " + entityPlayer.EntityName);
					}
				}
			}
		}
		if (_showTooltips)
		{
			GameManager.ShowTooltip(_localPlayer, (num == 0) ? Localization.Get("ttQuestShareNoPartyInRange", false, null) : string.Format(Localization.Get("ttQuestShareWithParty", false, null), _selectedQuest.QuestClass.Name), false, false, 0f);
		}
	}

	public static void AcceptSharedQuest(SharedQuestEntry _sharedQuest, EntityPlayerLocal _localPlayer)
	{
		if (_sharedQuest == null)
		{
			return;
		}
		QuestJournal questJournal = _localPlayer.QuestJournal;
		Quest quest = _sharedQuest.Quest;
		quest.RemoveMapObject();
		questJournal.AddQuest(quest, true);
		questJournal.RemoveSharedQuestEntry(_sharedQuest);
		quest.AddSharedLocation(_sharedQuest.Position, _sharedQuest.Size);
		quest.SetPositionData(Quest.PositionDataTypes.QuestGiver, _sharedQuest.ReturnPos);
		quest.Position = _sharedQuest.Position;
		NetPackageSharedQuest netPackageSharedQuest = NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(quest.QuestCode, _sharedQuest.SharedByPlayerID, _localPlayer.entityId, true);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(netPackageSharedQuest, false, _sharedQuest.SharedByPlayerID, -1, -1, null, 192, false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(netPackageSharedQuest, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PartyQuests instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly WaitForSeconds sendQuestsDelay = new WaitForSeconds(0.5f);
}
