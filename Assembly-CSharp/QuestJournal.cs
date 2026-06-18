using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class QuestJournal
{
	public Quest TrackedQuest
	{
		get
		{
			return this.trackedQuest;
		}
		set
		{
			LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(this.OwnerPlayer);
			if (uiforPlayer != null && uiforPlayer.xui != null && uiforPlayer.xui.QuestTracker != null)
			{
				Quest quest = uiforPlayer.xui.QuestTracker.TrackedQuest;
				if (quest != null)
				{
					quest.Tracked = false;
				}
			}
			this.trackedQuest = value;
			if (uiforPlayer != null && uiforPlayer.xui != null && uiforPlayer.xui.QuestTracker != null)
			{
				if (value != null)
				{
					this.trackedQuest.Tracked = true;
					uiforPlayer.xui.Recipes.TrackedRecipe = null;
					uiforPlayer.xui.QuestTracker.TrackedChallenge = null;
				}
				uiforPlayer.xui.QuestTracker.TrackedQuest = this.trackedQuest;
			}
		}
	}

	public void AddTraderPOI(Vector2 pos, int factionID)
	{
		if (!this.TraderPOIs.Contains(pos))
		{
			this.TraderPOIs.Add(pos);
		}
		if (!this.TradersByFaction.ContainsKey(factionID))
		{
			this.TradersByFaction.Add(factionID, new List<Vector2>());
		}
		if (!this.TradersByFaction[factionID].Contains(pos))
		{
			this.TradersByFaction[factionID].Add(pos);
		}
	}

	public bool HasTraderPOI(Vector2 pos)
	{
		return this.TraderPOIs.Contains(pos);
	}

	public List<Vector2> GetTraderList(int factionID)
	{
		if (this.TradersByFaction.ContainsKey(factionID))
		{
			return this.TradersByFaction[factionID];
		}
		return null;
	}

	public Quest GetSharedQuest(int questCode)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].QuestCode == questCode && this.quests[i].CurrentState == Quest.QuestState.InProgress)
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public void RemoveSharedQuestByOwner(int questCode)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].QuestCode == questCode && this.quests[i].CurrentState == Quest.QuestState.InProgress && !this.quests[i].RallyMarkerActivated)
			{
				Quest quest = this.quests[i];
				this.RemoveQuest(quest);
				GameManager.ShowTooltip(this.OwnerPlayer, "Shared quest {0} has been removed by quest owner.", quest.QuestClass.Name, null, null, false, false, 0f);
				return;
			}
		}
	}

	public void RemoveSharedQuestForOwner(int entityID)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].SharedOwnerID == entityID && this.quests[i].CurrentState == Quest.QuestState.InProgress && !this.quests[i].RallyMarkerActivated)
			{
				Quest quest = this.quests[i];
				this.RemoveQuest(quest);
				GameManager.ShowTooltip(this.OwnerPlayer, "Shared quest {0} has been removed.", quest.QuestClass.Name, null, null, false, false, 0f);
			}
		}
	}

	public void FailAllSharedQuests()
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].SharedOwnerID != -1 && this.quests[i].CurrentState == Quest.QuestState.InProgress && this.quests[i].CurrentPhase < this.quests[i].QuestClass.HighestPhase)
			{
				this.quests[i].CloseQuest(Quest.QuestState.Failed, null);
			}
		}
	}

	public void FailAllActivatedQuests()
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].RallyMarkerActivated && this.quests[i].CurrentState == Quest.QuestState.InProgress && this.quests[i].CurrentPhase < this.quests[i].QuestClass.HighestPhase)
			{
				this.quests[i].CloseQuest(Quest.QuestState.Failed, null);
			}
		}
	}

	public List<Vector2> GetUsedPOIs(Vector2 traderPOI, int tier)
	{
		QuestTraderData traderData = this.GetTraderData(traderPOI);
		if (traderData != null)
		{
			return traderData.GetTierPOIs(tier);
		}
		return null;
	}

	public void RemoveAllSharedQuests()
	{
		for (int i = this.quests.Count - 1; i >= 0; i--)
		{
			if (this.quests[i].SharedOwnerID != -1 && this.quests[i].CurrentState == Quest.QuestState.InProgress && this.quests[i].CurrentPhase < this.quests[i].QuestClass.HighestPhase)
			{
				Quest quest = this.quests[i];
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(quest.QuestCode, quest.SharedOwnerID, this.OwnerPlayer.entityId, false), false, quest.SharedOwnerID, -1, -1, null, 192, false);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(quest.QuestCode, quest.SharedOwnerID, this.OwnerPlayer.entityId, false), false);
				}
				this.RemoveQuest(quest);
				GameManager.ShowTooltip(this.OwnerPlayer, "Shared quest {0} has been removed.", quest.QuestClass.Name, null, null, false, false, 0f);
			}
		}
		for (int j = 0; j < this.sharedQuestEntries.Count; j++)
		{
			this.sharedQuestEntries[j].Quest.RemoveMapObject();
		}
		this.sharedQuestEntries.Clear();
		this.OwnerPlayer.TriggerSharedQuestRemovedEvent(null);
	}

	public void RemovePlayerFromSharedWiths(EntityPlayer player)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			this.quests[i].RemoveSharedWith(player);
		}
	}

	public Quest FindSharedQuest(int questCode)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].QuestCode == questCode)
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public Quest FindQuest(string questName, int questFaction = -1)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].ID == questName && (questFaction == -1 || (int)this.quests[i].QuestFaction == questFaction))
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public bool FindCompletedQuest(string questName, int questFaction = -1)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			Quest quest = this.quests[i];
			if (quest.ID == questName && (questFaction == -1 || (int)quest.QuestFaction == questFaction) && quest.CurrentState == Quest.QuestState.Completed)
			{
				return true;
			}
		}
		return false;
	}

	public Quest FindNonSharedQuest(string questName)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].ID == questName && this.quests[i].SharedOwnerID == -1)
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public Quest FindNonSharedQuest(int questCode)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].QuestCode == questCode && this.quests[i].SharedOwnerID == -1)
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public Quest FindLatestNonSharedQuest(string questName)
	{
		Quest quest = null;
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].ID == questName && (quest == null || this.quests[i].Active || this.quests[i].FinishTime > quest.FinishTime) && this.quests[i].SharedOwnerID == -1)
			{
				quest = this.quests[i];
			}
		}
		return quest;
	}

	public Quest FindActiveQuest()
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].CurrentState == Quest.QuestState.InProgress || this.quests[i].CurrentState == Quest.QuestState.ReadyForTurnIn)
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public bool QuestIsActive(Quest quest)
	{
		bool flag = false;
		for (int i = 0; i < this.quests.Count; i++)
		{
			Quest quest2 = this.quests[i];
			if (quest == quest2)
			{
				flag = quest2.CurrentState == Quest.QuestState.InProgress || quest2.CurrentState == Quest.QuestState.ReadyForTurnIn;
				break;
			}
		}
		return flag;
	}

	public Quest FindActiveQuest(int questCode)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].QuestCode == questCode && (this.quests[i].CurrentState == Quest.QuestState.InProgress || this.quests[i].CurrentState == Quest.QuestState.ReadyForTurnIn))
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public Quest FindActiveQuest(string questName, int questFaction = -1)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].ID == questName && (this.quests[i].CurrentState == Quest.QuestState.InProgress || this.quests[i].CurrentState == Quest.QuestState.ReadyForTurnIn) && (questFaction == -1 || (int)this.quests[i].QuestFaction == questFaction))
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public Quest FindActiveOrCompleteQuest(string questName, int questFaction = -1)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].ID == questName && this.quests[i].CurrentState != Quest.QuestState.Failed && (questFaction == -1 || (int)this.quests[i].QuestFaction == questFaction))
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public Quest FindActiveQuestByGiver(int questGiverID)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].CheckIsQuestGiver(questGiverID) && this.quests[i].CurrentState == Quest.QuestState.InProgress && this.quests[i].SharedOwnerID == -1)
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public Quest FindActiveQuestByGiver(int questGiverID, string questType)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].CheckIsQuestGiver(questGiverID) && (this.quests[i].CurrentState == Quest.QuestState.InProgress || this.quests[i].CurrentState == Quest.QuestState.ReadyForTurnIn) && this.quests[i].SharedOwnerID == -1 && this.quests[i].QuestClass.QuestType == questType)
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public Quest FindReadyForTurnInQuestByGiver(int questGiverID)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].CheckIsQuestGiver(questGiverID) && (this.quests[i].CurrentState == Quest.QuestState.InProgress || this.quests[i].CurrentState == Quest.QuestState.ReadyForTurnIn) && this.quests[i].RallyMarkerActivated)
			{
				return this.quests[i];
			}
		}
		return null;
	}

	public bool HasActiveQuestByQuestCode(int questCode)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].QuestCode == questCode && this.quests[i].CurrentState == Quest.QuestState.InProgress)
			{
				return true;
			}
		}
		return false;
	}

	public void AddQuest(Quest q, bool notify = true)
	{
		q.OwnerJournal = this;
		if (this.FindActiveQuest(q.QuestCode) == null)
		{
			q.StartQuest(true, notify);
			this.quests.Add(q);
			this.OwnerPlayer.TriggerQuestAddedEvent(q);
			foreach (KeyValuePair<Quest.PositionDataTypes, Vector3> keyValuePair in q.PositionData)
			{
				GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.OwnerPlayer.entityId).AddQuestPosition(q.QuestCode, keyValuePair.Key, World.worldToBlockPos(keyValuePair.Value));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void RefreshTracked()
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].Tracked)
			{
				this.TrackedQuest = this.quests[i];
				return;
			}
		}
	}

	public void CompleteQuest(Quest q)
	{
		q.CurrentState = Quest.QuestState.Completed;
		q.FinishTime = GameManager.Instance.World.worldTime;
		this.OwnerPlayer.TriggerQuestChangedEvent(q);
		if (q.QuestClass.AddsToTierComplete && q.AddsProgression)
		{
			this.AddQuestFactionPoint(q.QuestFaction, (int)q.QuestClass.DifficultyTier);
			if (q.PositionData.ContainsKey(Quest.PositionDataTypes.TraderPosition) && q.PositionData.ContainsKey(Quest.PositionDataTypes.POIPosition) && SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				Vector3 vector = q.PositionData[Quest.PositionDataTypes.TraderPosition];
				Vector3 vector2 = q.PositionData[Quest.PositionDataTypes.POIPosition];
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageNPCQuestList>().Setup(this.OwnerPlayer.entityId, new Vector2(vector.x, vector.z), (int)q.QuestClass.DifficultyTier, new Vector2(vector2.x, vector2.z)), false);
			}
			this.ResetAddToProgression();
		}
		if (this.ActiveQuest == q)
		{
			this.ActiveQuest = null;
			this.RefreshRallyMarkerPositions();
		}
		GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.OwnerPlayer.entityId).RemovePositionsForQuest(q.QuestCode);
	}

	public int AddTraderData(Vector2 pos)
	{
		for (int i = 0; i < this.TraderData.Count; i++)
		{
			if (this.TraderData[i].TraderPOI == pos)
			{
				return i;
			}
		}
		this.TraderData.Add(new QuestTraderData(pos)
		{
			Owner = this
		});
		return this.TraderData.Count - 1;
	}

	public void AddPOIToTraderData(int tier, Vector3 questGiver, Vector3 poiPosition)
	{
		int num = this.AddTraderData(new Vector2(questGiver.x, questGiver.z));
		this.TraderData[num].AddPOI(tier, new Vector2(poiPosition.x, poiPosition.z));
	}

	public void AddPOIToTraderData(int tier, Vector2 questGiver, Vector2 poiPosition)
	{
		int num = this.AddTraderData(questGiver);
		this.TraderData[num].AddPOI(tier, poiPosition);
	}

	public void ClearTraderDataTier(int tier, Vector2 questGiver)
	{
		QuestTraderData traderData = this.GetTraderData(questGiver);
		if (traderData != null)
		{
			traderData.ClearTier(tier);
		}
	}

	public QuestTraderData GetTraderData(Vector2 questGiver)
	{
		for (int i = 0; i < this.TraderData.Count; i++)
		{
			if (this.TraderData[i].TraderPOI == questGiver)
			{
				return this.TraderData[i];
			}
		}
		return null;
	}

	public void Clear()
	{
		this.TrackedQuest = null;
		this.ForceRemoveAllQuests();
		this.ActiveQuest = null;
		this.TraderData.Clear();
		this.sharedQuestEntries.Clear();
		this.TradersByFaction.Clear();
		this.QuestFactionPoints.Clear();
		this.GlobalFactionPoints = 0;
		this.TraderPOIs.Clear();
	}

	public void FailedQuest(Quest q)
	{
		q.CurrentState = Quest.QuestState.Failed;
		q.FinishTime = GameManager.Instance.World.worldTime;
		this.OwnerPlayer.TriggerQuestChangedEvent(q);
		GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.OwnerPlayer.entityId).RemovePositionsForQuest(q.QuestCode);
	}

	public void RemoveQuest(Quest q)
	{
		if (this.FindActiveQuest(q.QuestCode) != null)
		{
			q.CloseQuest(Quest.QuestState.Failed, null);
			this.quests.Remove(q);
			if (q.SharedOwnerID != -1)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(q.QuestCode, q.SharedOwnerID, this.OwnerPlayer.entityId, false), false, q.SharedOwnerID, -1, -1, null, 192, false);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(q.QuestCode, q.SharedOwnerID, this.OwnerPlayer.entityId, false), false);
				}
			}
			this.OwnerPlayer.TriggerQuestRemovedEvent(q);
			this.HandlePartyRemoveQuest(q);
			GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.OwnerPlayer.entityId).RemovePositionsForQuest(q.QuestCode);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandlePartyRemoveQuest(Quest q)
	{
		if (this.OwnerPlayer.IsInParty() && q.SharedOwnerID == -1)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				for (int i = 0; i < this.OwnerPlayer.Party.MemberList.Count; i++)
				{
					EntityPlayer entityPlayer = this.OwnerPlayer.Party.MemberList[i];
					if (entityPlayer is EntityPlayerLocal)
					{
						entityPlayer.QuestJournal.RemoveSharedQuestByOwner(q.QuestCode);
						entityPlayer.QuestJournal.RemoveSharedQuestEntry(q.QuestCode);
					}
					else
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(q.QuestCode, this.OwnerPlayer.entityId), false, entityPlayer.entityId, -1, -1, null, 192, false);
					}
				}
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageSharedQuest>().Setup(q.QuestCode, this.OwnerPlayer.entityId), false);
		}
	}

	public void ForceRemoveAllQuests()
	{
		for (int i = this.quests.Count - 1; i >= 0; i--)
		{
			this.ForceRemoveQuest(this.quests[i]);
		}
	}

	public void ForceRemoveQuest(Quest quest)
	{
		this.quests.Remove(quest);
		quest.UnhookQuest();
		this.OwnerPlayer.TriggerQuestRemovedEvent(quest);
		GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.OwnerPlayer.entityId).RemovePositionsForQuest(quest.QuestCode);
	}

	public void ForceRemoveQuest(string questID)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].ID.EqualsCaseInsensitive(questID))
			{
				Quest quest = this.quests[i];
				this.quests.Remove(quest);
				quest.UnhookQuest();
				this.OwnerPlayer.TriggerQuestRemovedEvent(quest);
				GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.OwnerPlayer.entityId).RemovePositionsForQuest(quest.QuestCode);
				return;
			}
		}
	}

	public void RefreshQuest(Quest q)
	{
		this.OwnerPlayer.TriggerQuestChangedEvent(q);
	}

	public void UnHookQuests()
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			this.quests[i].UnhookQuest();
		}
	}

	public void Read(PooledBinaryReader _br)
	{
		this.quests.Clear();
		byte b = _br.ReadByte();
		if (b >= 2)
		{
			int num = (int)_br.ReadByte();
			this.TraderPOIs.Clear();
			for (int i = 0; i < num; i++)
			{
				this.TraderPOIs.Add(StreamUtils.ReadVector2(_br));
			}
		}
		if (b > 2)
		{
			int num2 = (int)_br.ReadByte();
			this.TradersByFaction.Clear();
			for (int j = 0; j < num2; j++)
			{
				int num3 = _br.ReadInt32();
				List<Vector2> list = new List<Vector2>();
				int num4 = _br.ReadInt32();
				for (int k = 0; k < num4; k++)
				{
					list.Add(StreamUtils.ReadVector2(_br));
				}
				this.TradersByFaction.Add(num3, list);
			}
		}
		int num5 = (int)_br.ReadInt16();
		for (int l = 0; l < num5; l++)
		{
			PooledBinaryReader.StreamReadSizeMarker streamReadSizeMarker = default(PooledBinaryReader.StreamReadSizeMarker);
			if (b >= 5)
			{
				streamReadSizeMarker = _br.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
			}
			string text = _br.ReadString();
			try
			{
				byte b2 = _br.ReadByte();
				if (QuestClass.GetQuest(text) == null)
				{
					Log.Error("Loading player quests: Quest with ID " + text + " not found, ignoring");
					uint num6;
					_br.ValidateSizeMarker(ref streamReadSizeMarker, out num6, true);
				}
				else
				{
					Quest quest = QuestClass.CreateQuest(text);
					Quest quest2 = quest.Clone();
					quest2.CurrentQuestVersion = b2;
					quest2.Read(_br);
					if (quest.CurrentQuestVersion != b2)
					{
						quest2 = quest.Clone();
					}
					quest2.OwnerJournal = this;
					this.quests.Add(quest2);
					if (quest2.CurrentState == Quest.QuestState.Completed && quest2.QuestClass.AddsToTierComplete && quest2.AddsProgression)
					{
						this.AddQuestFactionPoint(quest2.QuestFaction, (int)quest2.QuestClass.DifficultyTier);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error(ex.ToString());
			}
			finally
			{
				uint num6;
				if (b >= 5 && !_br.ValidateSizeMarker(ref streamReadSizeMarker, out num6, true))
				{
					Log.Error("Loading player quests: Error loading quest " + text + ", ignoring");
				}
			}
		}
		if (b > 3)
		{
			this.TraderData.Clear();
			num5 = (int)_br.ReadByte();
			for (int m = 0; m < num5; m++)
			{
				QuestTraderData questTraderData = new QuestTraderData
				{
					Owner = this
				};
				questTraderData.Read(_br, b);
				this.TraderData.Add(questTraderData);
			}
		}
	}

	public void AddQuestFactionPoint(byte id, int difficultyTier)
	{
		if (difficultyTier == 0)
		{
			return;
		}
		this.GlobalFactionPoints += difficultyTier;
		if (!this.QuestFactionPoints.ContainsKey(id))
		{
			this.QuestFactionPoints.Add(id, difficultyTier);
			return;
		}
		Dictionary<byte, int> questFactionPoints = this.QuestFactionPoints;
		questFactionPoints[id] += difficultyTier;
	}

	public int GetQuestFactionPoints(byte id)
	{
		if (this.QuestFactionPoints.ContainsKey(id))
		{
			return this.QuestFactionPoints[id];
		}
		return 0;
	}

	public int GetQuestFactionMax(byte id, int tier)
	{
		int num = 0;
		for (int i = 1; i <= tier; i++)
		{
			num += i * Quest.QuestsPerTier;
		}
		return num;
	}

	public void ResetAddToProgression()
	{
		int num = 0;
		int worldDay = GameManager.Instance.World.WorldDay;
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].QuestProgressDay == worldDay)
			{
				num++;
			}
		}
		int questsPerDay = Quest.QuestsPerDay;
		this.CanAddProgression = questsPerDay == -1 || num < questsPerDay;
	}

	public void HandleQuestCompleteToday(Quest q)
	{
		int num = 0;
		int worldDay = GameManager.Instance.World.WorldDay;
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].QuestProgressDay == worldDay)
			{
				num++;
			}
		}
		int questsPerDay = Quest.QuestsPerDay;
		if (questsPerDay == -1 || num < questsPerDay)
		{
			q.QuestProgressDay = worldDay;
		}
		else
		{
			q.QuestProgressDay = -1;
		}
		this.ResetAddToProgression();
		if (!this.CanAddProgression)
		{
			this.OwnerPlayer.Buffs.AddBuff("buffShowQuestLimitReached", -1, true, false, -1f);
		}
	}

	public int GetCurrentFactionTier(byte id, int offset = 0, bool allowExtraTierOverMax = false)
	{
		int num = this.GetQuestFactionPoints(id) + offset;
		int i = 1;
		if (Quest.QuestsPerTier == 0)
		{
			return Quest.MaxQuestTier + (allowExtraTierOverMax ? 1 : 0);
		}
		while (i < 100)
		{
			num -= i * Quest.QuestsPerTier;
			if (num < 0)
			{
				return Math.Min(i, Quest.MaxQuestTier + (allowExtraTierOverMax ? 1 : 0));
			}
			i++;
		}
		return 1;
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write(5);
		int count = this.TraderPOIs.Count;
		_bw.Write((byte)count);
		for (int i = 0; i < count; i++)
		{
			StreamUtils.Write(_bw, this.TraderPOIs[i]);
		}
		_bw.Write((byte)this.TradersByFaction.Count);
		foreach (int num in this.TradersByFaction.Keys)
		{
			_bw.Write(num);
			_bw.Write(this.TradersByFaction[num].Count);
			for (int j = 0; j < this.TradersByFaction[num].Count; j++)
			{
				StreamUtils.Write(_bw, this.TradersByFaction[num][j]);
			}
		}
		int num2 = this.quests.Count;
		_bw.Write((ushort)num2);
		for (int k = 0; k < num2; k++)
		{
			PooledBinaryWriter.StreamWriteSizeMarker streamWriteSizeMarker = _bw.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
			this.quests[k].Write(_bw);
			_bw.FinalizeSizeMarker(ref streamWriteSizeMarker);
		}
		num2 = this.TraderData.Count;
		_bw.Write((byte)num2);
		for (int l = 0; l < num2; l++)
		{
			this.TraderData[l].Write(_bw);
		}
	}

	public QuestJournal Clone()
	{
		QuestJournal questJournal = new QuestJournal();
		questJournal.OwnerPlayer = this.OwnerPlayer;
		for (int i = 0; i < this.TraderPOIs.Count; i++)
		{
			questJournal.TraderPOIs.Add(this.TraderPOIs[i]);
		}
		foreach (int num in this.TradersByFaction.Keys)
		{
			questJournal.TradersByFaction.Add(num, this.TradersByFaction[num]);
		}
		for (int j = 0; j < this.quests.Count; j++)
		{
			questJournal.quests.Add(this.quests[j].Clone());
		}
		for (int k = 0; k < this.sharedQuestEntries.Count; k++)
		{
			questJournal.sharedQuestEntries.Add(this.sharedQuestEntries[k].Clone());
		}
		foreach (KeyValuePair<byte, int> keyValuePair in this.QuestFactionPoints)
		{
			questJournal.AddQuestFactionPoint(keyValuePair.Key, keyValuePair.Value);
		}
		for (int l = 0; l < this.TraderData.Count; l++)
		{
			questJournal.TraderData.Add(this.TraderData[l]);
		}
		return questJournal;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void StartQuests()
	{
		if (!GameManager.Instance.World.IsEditor() && !GameUtils.IsWorldEditor() && !GameUtils.IsPlaytesting())
		{
			this.OwnerPlayer.challengeJournal.StartChallenges(this.OwnerPlayer);
		}
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].RallyMarkerActivated)
			{
				if (this.quests[i].SharedOwnerID != -1 && this.quests[i].CurrentPhase < this.quests[i].QuestClass.HighestPhase && this.quests[i].CurrentState == Quest.QuestState.InProgress)
				{
					this.quests[i].CloseQuest(Quest.QuestState.Failed, null);
				}
				else
				{
					this.quests[i].ResetToRallyPointObjective();
					this.quests[i].StartQuest(false, true);
				}
			}
			else
			{
				this.quests[i].StartQuest(false, true);
			}
		}
	}

	public bool AddSharedQuestEntry(NetPackageSharedQuest.SharedQuestData sqd)
	{
		for (int i = 0; i < this.sharedQuestEntries.Count; i++)
		{
			SharedQuestEntry sharedQuestEntry = this.sharedQuestEntries[i];
			if (sharedQuestEntry.QuestCode == sqd.questCode && sharedQuestEntry.QuestID == sqd.questID && sharedQuestEntry.SharedByPlayerID == sqd.sharedByEntityID && sharedQuestEntry.QuestGiverID == sqd.questGiverID)
			{
				if (PartyQuests.AutoAccept)
				{
					Log.Out(string.Format("Ignoring received quest, already have a sharedquest with the questCode={0}, id={1}, sharer={2}, giver={3}:", new object[] { sqd.questCode, sqd.questID, sqd.sharedByEntityID, sqd.questGiverID }));
					for (int j = 0; j < this.sharedQuestEntries.Count; j++)
					{
						SharedQuestEntry sharedQuestEntry2 = this.sharedQuestEntries[j];
						Log.Out(string.Format("  {0}.: id={1}, code={2}, name={3}, POI={4}, state={5}, owner={6}", new object[]
						{
							j,
							sharedQuestEntry2.QuestID,
							sharedQuestEntry2.QuestCode,
							sharedQuestEntry2.Quest.QuestClass.Name,
							sharedQuestEntry2.Quest.GetParsedText("{poi.name}"),
							sharedQuestEntry2.Quest.CurrentState,
							sharedQuestEntry2.SharedByPlayerID
						}));
					}
					Log.Out("Quests:");
					for (int k = 0; k < this.quests.Count; k++)
					{
						Quest quest = this.quests[k];
						Log.Out(string.Format("  {0}.: id={1}, code={2}, name={3}, POI={4}, state={5}, owner={6}", new object[]
						{
							k,
							quest.ID,
							quest.QuestCode,
							quest.QuestClass.Name,
							quest.GetParsedText("{poi.name}"),
							quest.CurrentState,
							quest.SharedOwnerID
						}));
					}
				}
				return false;
			}
		}
		SharedQuestEntry sharedQuestEntry3 = new SharedQuestEntry(sqd.questCode, sqd.questID, sqd.poiName, sqd.position, sqd.size, sqd.returnPos, sqd.sharedByEntityID, sqd.questGiverID, this, null);
		Log.Out(string.Format("Received shared quest: questCode={0}, id={1}, POI {2}", sqd.questCode, sqd.questID, sqd.poiName));
		this.sharedQuestEntries.Add(sharedQuestEntry3);
		this.OwnerPlayer.TriggerSharedQuestAddedEvent(sharedQuestEntry3);
		return true;
	}

	public void RemoveSharedQuestEntry(SharedQuestEntry entry)
	{
		if (this.sharedQuestEntries.Contains(entry))
		{
			this.sharedQuestEntries.Remove(entry);
			entry.Quest.RemoveMapObject();
			this.OwnerPlayer.TriggerSharedQuestRemovedEvent(entry);
		}
	}

	public void RemoveSharedQuestEntry(int questCode)
	{
		for (int i = this.sharedQuestEntries.Count - 1; i >= 0; i--)
		{
			if (this.sharedQuestEntries[i].Quest.QuestCode == questCode)
			{
				SharedQuestEntry sharedQuestEntry = this.sharedQuestEntries[i];
				sharedQuestEntry.Quest.RemoveMapObject();
				this.sharedQuestEntries.RemoveAt(i);
				this.OwnerPlayer.TriggerSharedQuestRemovedEvent(sharedQuestEntry);
			}
		}
	}

	public void RemoveSharedQuestEntryByOwner(int entityID)
	{
		for (int i = this.sharedQuestEntries.Count - 1; i >= 0; i--)
		{
			if (this.sharedQuestEntries[i].Quest.SharedOwnerID == entityID)
			{
				SharedQuestEntry sharedQuestEntry = this.sharedQuestEntries[i];
				sharedQuestEntry.Quest.RemoveMapObject();
				this.sharedQuestEntries.RemoveAt(i);
				this.OwnerPlayer.TriggerSharedQuestRemovedEvent(sharedQuestEntry);
			}
		}
	}

	public void ClearSharedQuestMarkers()
	{
		for (int i = 0; i < this.sharedQuestEntries.Count; i++)
		{
			this.sharedQuestEntries[i].Quest.RemoveMapObject();
		}
	}

	public Quest GetNextCompletedQuest(Quest lastQuest, int entityId)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (lastQuest != null)
			{
				if (this.quests[i] == lastQuest)
				{
					lastQuest = null;
				}
			}
			else
			{
				Quest quest = this.quests[i];
				if (quest.CurrentState == Quest.QuestState.ReadyForTurnIn && (!quest.QuestClass.ReturnToQuestGiver || quest.QuestGiverID == -1 || quest.CheckIsQuestGiver(entityId)))
				{
					return quest;
				}
			}
		}
		return null;
	}

	public void SetActivePositionData(Quest.PositionDataTypes dataType, Vector3i position)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].Active && this.quests[i].RallyMarkerActivated)
			{
				this.quests[i].SetObjectivePosition(dataType, position);
			}
		}
	}

	public void HandleRestorePowerReceived(Vector3 prefabPos, List<Vector3i> activateList)
	{
		int num = 0;
		while (num < this.quests.Count && (!this.quests[num].Active || !this.quests[num].RallyMarkerActivated || !this.quests[num].HandleActivateListReceived(prefabPos, activateList)))
		{
			num++;
		}
	}

	public void HandleRallyMarkerActivation(int questCode, Vector3 prefabPos, bool rallyMarkerActivated, QuestEventManager.POILockoutReasonTypes lockoutReason, ulong extraData = 0UL)
	{
		int num = 0;
		while (num < this.quests.Count && (this.quests[num].QuestCode != questCode || !this.quests[num].Active || !this.quests[num].HandleRallyMarkerActivation(prefabPos, rallyMarkerActivated, lockoutReason, extraData)))
		{
			num++;
		}
	}

	public bool CheckRallyMarkerActivation()
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < this.quests.Count; i++)
		{
			if (this.quests[i].Active)
			{
				foreach (BaseObjective baseObjective in this.quests[i].Objectives)
				{
					ObjectiveRallyPoint objectiveRallyPoint = baseObjective as ObjectiveRallyPoint;
					if (objectiveRallyPoint != null)
					{
						flag2 = true;
						flag = objectiveRallyPoint.IsActivated();
						break;
					}
				}
			}
		}
		return !flag2 || flag;
	}

	public Quest HasQuestAtRallyPosition(Vector3 rallyPos, bool mustBeHost = true)
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			Quest quest = this.quests[i];
			Vector3 vector;
			if (quest.Active && (quest.SharedOwnerID == -1 || !mustBeHost) && !quest.RallyMarkerActivated && quest.GetPositionData(out vector, Quest.PositionDataTypes.Activate) && vector.x == rallyPos.x && vector.z == rallyPos.z)
			{
				return quest;
			}
		}
		return null;
	}

	public void RefreshRallyMarkerPositions()
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			Quest quest = this.quests[i];
			if (quest.Active && !quest.RallyMarkerActivated)
			{
				quest.RefreshRallyMarker();
			}
		}
	}

	public bool HasCraftingQuest()
	{
		for (int i = 0; i < this.quests.Count; i++)
		{
			Quest quest = this.quests[i];
			if (quest.Active && quest.QuestTags.Test_AnySet(QuestEventManager.craftingTag))
			{
				return true;
			}
		}
		return false;
	}

	public List<string> GetQuestRecipes()
	{
		this.questRecipeList.Clear();
		for (int i = 0; i < this.quests.Count; i++)
		{
			Quest quest = this.quests[i];
			if (quest.Active)
			{
				for (int j = 0; j < quest.Objectives.Count; j++)
				{
					if ((quest.Objectives[j].Phase == 0 || quest.Objectives[j].Phase == quest.CurrentPhase) && quest.Objectives[j] is ObjectiveCraft && !quest.Objectives[j].Complete)
					{
						this.questRecipeList.Add((quest.Objectives[j] as ObjectiveCraft).ID);
					}
				}
			}
		}
		return this.questRecipeList;
	}

	public T GetObjectiveForQuest<T>(int _questCode) where T : BaseObjective
	{
		Quest quest = this.FindActiveQuest(_questCode);
		if (quest != null)
		{
			for (int i = 0; i < quest.Objectives.Count; i++)
			{
				if (quest.CurrentPhase == quest.Objectives[i].Phase && quest.Objectives[i] is T)
				{
					return quest.Objectives[i] as T;
				}
			}
		}
		return default(T);
	}

	public int GetRewardedSkillPoints()
	{
		int num = 0;
		for (int i = 0; i < this.quests.Count; i++)
		{
			Quest quest = this.quests[i];
			if (quest.CurrentState == Quest.QuestState.Completed)
			{
				for (int j = 0; j < quest.QuestClass.Rewards.Count; j++)
				{
					RewardSkillPoints rewardSkillPoints = quest.QuestClass.Rewards[j] as RewardSkillPoints;
					if (rewardSkillPoints != null)
					{
						num += StringParsers.ParseSInt32(rewardSkillPoints.Value, 0, -1, NumberStyles.Integer);
					}
				}
			}
		}
		return num;
	}

	public void Update(int worldDay)
	{
		if (this.previousDay != worldDay)
		{
			this.previousDay = worldDay;
			this.ResetAddToProgression();
			if (this.CanAddProgression)
			{
				this.OwnerPlayer.Buffs.RemoveBuff("buffShowQuestLimitReached", -1, true);
				return;
			}
			this.OwnerPlayer.Buffs.AddBuff("buffShowQuestLimitReached", -1, true, false, -1f);
		}
	}

	public EntityPlayerLocal OwnerPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCurrentSaveVersion = 5;

	public List<Quest> quests = new List<Quest>();

	public List<SharedQuestEntry> sharedQuestEntries = new List<SharedQuestEntry>();

	public List<Vector2> TraderPOIs = new List<Vector2>();

	public List<QuestTraderData> TraderData = new List<QuestTraderData>();

	public Dictionary<int, List<Vector2>> TradersByFaction = new Dictionary<int, List<Vector2>>();

	public static bool IntroQuestEnabled = true;

	public static bool BuriedQuestsEnabled = true;

	public static bool POIQuestsEnabled = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest trackedQuest;

	public Quest ActiveQuest;

	public Dictionary<byte, int> QuestFactionPoints = new Dictionary<byte, int>();

	public int GlobalFactionPoints;

	public bool CanAddProgression;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<string> questRecipeList = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int previousDay = -1;
}
