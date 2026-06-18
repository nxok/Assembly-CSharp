using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Challenges;
using Quests;
using UnityEngine;

public class QuestEventManager
{
	public static QuestEventManager Current
	{
		get
		{
			if (QuestEventManager.instance == null)
			{
				QuestEventManager.instance = new QuestEventManager();
			}
			return QuestEventManager.instance;
		}
	}

	public static bool HasInstance
	{
		get
		{
			return QuestEventManager.instance != null;
		}
	}

	public void SetupTraderPrefabList(TraderArea area)
	{
		if (!this.TraderPrefabList.ContainsKey(area))
		{
			Vector3 vector = area.Position.ToVector3();
			List<PrefabInstance> list = new List<PrefabInstance>();
			GameManager.Instance.GetDynamicPrefabDecorator().GetPOIPrefabs(list);
			List<QuestEventManager.PrefabListData> list2 = new List<QuestEventManager.PrefabListData>();
			QuestEventManager.PrefabListData prefabListData = new QuestEventManager.PrefabListData();
			QuestEventManager.PrefabListData prefabListData2 = new QuestEventManager.PrefabListData();
			QuestEventManager.PrefabListData prefabListData3 = new QuestEventManager.PrefabListData();
			list2.Add(prefabListData);
			list2.Add(prefabListData2);
			list2.Add(prefabListData3);
			for (int i = 0; i < list.Count; i++)
			{
				float num = Vector3.Distance(vector, list[i].boundingBoxPosition);
				if (num <= 500f)
				{
					prefabListData.AddPOI(list[i]);
				}
				else if (num <= 1500f)
				{
					prefabListData2.AddPOI(list[i]);
				}
				else
				{
					prefabListData3.AddPOI(list[i]);
				}
			}
			this.TraderPrefabList.Add(area, list2);
		}
	}

	public List<PrefabInstance> GetPrefabsForTrader(TraderArea traderArea, int difficulty, int index, GameRandom gameRandom)
	{
		if (traderArea == null)
		{
			return null;
		}
		if (!this.TraderPrefabList.ContainsKey(traderArea))
		{
			this.SetupTraderPrefabList(traderArea);
		}
		QuestEventManager.PrefabListData prefabListData = this.TraderPrefabList[traderArea][index];
		if (gameRandom != null)
		{
			prefabListData.ShuffleDifficulty(difficulty, gameRandom);
		}
		List<PrefabInstance> list;
		if (prefabListData.TierData.TryGetValue(difficulty, out list))
		{
			return list;
		}
		return null;
	}

	public int GetTraderPoiCount(TraderArea traderArea, int difficulty, int index)
	{
		int num = 0;
		if (traderArea == null)
		{
			return 0;
		}
		if (!this.TraderPrefabList.ContainsKey(traderArea))
		{
			this.SetupTraderPrefabList(traderArea);
		}
		List<PrefabInstance> list;
		if (this.TraderPrefabList[traderArea][index].TierData.TryGetValue(difficulty, out list))
		{
			num = list.Count;
		}
		return num;
	}

	public List<PrefabInstance> GetPrefabsByDifficultyTier(int difficulty)
	{
		if (this.tierPrefabList.Count == 0)
		{
			List<PrefabInstance> list = new List<PrefabInstance>();
			GameManager.Instance.GetDynamicPrefabDecorator().GetPOIPrefabs(list);
			for (int i = 0; i < list.Count; i++)
			{
				if (!this.tierPrefabList.ContainsKey((int)list[i].prefab.DifficultyTier))
				{
					this.tierPrefabList.Add((int)list[i].prefab.DifficultyTier, new List<PrefabInstance>());
				}
				this.tierPrefabList[(int)list[i].prefab.DifficultyTier].Add(list[i]);
			}
		}
		if (this.tierPrefabList.ContainsKey(difficulty))
		{
			return this.tierPrefabList[difficulty];
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestEventManager()
	{
	}

	public event QuestEvent_BlockEvent BlockActivate;

	public void BlockActivated(string blockName, Vector3i blockPos)
	{
		if (this.BlockActivate != null)
		{
			this.BlockActivate(blockName, blockPos);
		}
	}

	public event QuestEvent_BlockChangedEvent BlockChange;

	public void BlockChanged(Block blockOld, Block blockNew, Vector3i blockPos)
	{
		if (this.BlockChange != null)
		{
			this.BlockChange(blockOld, blockNew, blockPos);
		}
	}

	public event QuestEvent_BlockDestroyEvent BlockDestroy;

	public void BlockDestroyed(Block block, BlockValueRef bvRef, Entity byEntity = null)
	{
		Vector3i vector3i;
		if (!bvRef.TryGetBlockPos(out vector3i))
		{
			return;
		}
		if (this.BlockDestroy != null)
		{
			this.BlockDestroy(block, vector3i);
		}
		if (block.AllowBlockTriggers && byEntity)
		{
			EntityPlayer entityPlayer = byEntity as EntityPlayer;
			if (!entityPlayer)
			{
				entityPlayer = byEntity.world.GetClosestPlayer(byEntity, 500f, false);
			}
			if (entityPlayer)
			{
				BlockValue blockValue = default(BlockValue);
				blockValue.type = block.blockID;
				block.HandleTrigger(entityPlayer, entityPlayer.world, vector3i, blockValue);
			}
		}
	}

	public event QuestEvent_BlockEvent BlockPickup;

	public void BlockPickedUp(string blockName, Vector3i blockPos)
	{
		if (this.BlockPickup != null)
		{
			this.BlockPickup(blockName, blockPos);
		}
	}

	public event QuestEvent_BlockEvent BlockPlace;

	public void BlockPlaced(string blockName, Vector3i blockPos)
	{
		if (this.BlockPlace != null)
		{
			this.BlockPlace(blockName, blockPos);
		}
	}

	public event QuestEvent_BlockEvent BlockUpgrade;

	public void BlockUpgraded(string blockName, Vector3i blockPos)
	{
		if (this.BlockUpgrade != null)
		{
			this.BlockUpgrade(blockName, blockPos);
		}
	}

	public event QuestEvent_ItemStackActionEvent AddItem;

	public void ItemAdded(ItemStack newStack)
	{
		if (this.AddItem != null)
		{
			this.AddItem(newStack);
		}
	}

	public event QuestEvent_HarvestStackActionEvent HarvestItem;

	public void HarvestedItem(ItemValue heldItem, ItemStack newStack, BlockValue bv)
	{
		if (this.HarvestItem != null)
		{
			this.HarvestItem(heldItem, newStack, bv);
		}
	}

	public event QuestEvent_ItemStackActionEvent AssembleItem;

	public void AssembledItem(ItemStack newStack)
	{
		if (this.AssembleItem != null)
		{
			this.AssembleItem(newStack);
		}
	}

	public event QuestEvent_ItemStackActionEvent CraftItem;

	public void CraftedItem(ItemStack newStack)
	{
		if (this.CraftItem != null)
		{
			this.CraftItem(newStack);
		}
	}

	public event QuestEvent_ItemStackActionEvent ExchangeFromItem;

	public void ExchangedFromItem(ItemStack newStack)
	{
		if (this.ExchangeFromItem != null)
		{
			this.ExchangeFromItem(newStack);
		}
	}

	public event QuestEvent_ItemStackActionEvent ScrapItem;

	public void ScrappedItem(ItemStack newStack)
	{
		if (this.ScrapItem != null)
		{
			this.ScrapItem(newStack);
		}
	}

	public event QuestEvent_ItemValueActionEvent RepairItem;

	public void RepairedItem(ItemValue newValue)
	{
		if (this.RepairItem != null)
		{
			this.RepairItem(newValue);
		}
	}

	public event QuestEvent_SkillPointSpent SkillPointSpent;

	public event QuestEvent_ItemValueActionEvent HoldItem;

	public void HeldItem(ItemValue newValue)
	{
		if (this.HoldItem != null)
		{
			this.HoldItem(newValue);
		}
	}

	public event QuestEvent_ItemValueActionEvent WearItem;

	public void WoreItem(ItemValue newValue)
	{
		if (this.WearItem != null)
		{
			this.WearItem(newValue);
		}
	}

	public void SpendSkillPoint(ProgressionValue skill)
	{
		if (this.SkillPointSpent != null)
		{
			this.SkillPointSpent(skill.ProgressionClass.Name);
		}
	}

	public event QuestEvent_WindowChanged WindowChanged;

	public void ChangedWindow(string windowName)
	{
		if (this.WindowChanged != null)
		{
			this.WindowChanged(windowName);
		}
	}

	public event QuestEvent_OpenContainer ContainerOpened;

	public void OpenedContainer(Vector3i containerLocation, ITileEntityLootable tileEntity)
	{
		if (this.ContainerOpened != null)
		{
			this.ContainerOpened(containerLocation, tileEntity);
		}
	}

	public event QuestEvent_OpenContainer ContainerClosed;

	public void ClosedContainer(Vector3i containerLocation, ITileEntityLootable tileEntity)
	{
		if (this.ContainerClosed != null)
		{
			this.ContainerClosed(containerLocation, tileEntity);
		}
	}

	public event QuestEvent_EntityKillEvent EntityKill;

	public void EntityKilled(EntityAlive killedBy, EntityAlive killedEntity)
	{
		if (this.EntityKill != null && killedBy != null && killedEntity != null)
		{
			this.EntityKill(killedBy, killedEntity);
		}
	}

	public event QuestEvent_NPCInteracted NPCInteract;

	public void NPCInteracted(EntityNPC entityNPC)
	{
		if (this.NPCInteract != null)
		{
			this.NPCInteract(entityNPC);
		}
	}

	public event QuestEvent_NPCInteracted NPCMeet;

	public void NPCMet(EntityNPC entityNPC)
	{
		if (this.NPCMeet != null)
		{
			this.NPCMeet(entityNPC);
		}
	}

	public event QuestEvent_SleepersCleared SleepersCleared;

	public void ClearedSleepers(Vector3 prefabPos)
	{
		if (this.SleepersCleared != null)
		{
			this.SleepersCleared(prefabPos);
		}
	}

	public event QuestEvent_Explosion ExplosionDetected;

	public void DetectedExplosion(Vector3 explosionPos, int entityID, float blockDamage)
	{
		if (this.ExplosionDetected != null)
		{
			this.ExplosionDetected(explosionPos, entityID, blockDamage);
		}
	}

	public event QuestEvent_PurchaseEvent BuyItems;

	public event QuestEvent_PurchaseEvent SellItems;

	public void BoughtItems(string traderName, int itemCount)
	{
		if (this.BuyItems != null)
		{
			this.BuyItems(traderName, itemCount);
		}
	}

	public void SoldItems(string traderName, int itemCount)
	{
		if (this.SellItems != null)
		{
			this.SellItems(traderName, itemCount);
		}
	}

	public event QuestEvent_ChallengeCompleteEvent ChallengeComplete;

	public void ChallengeCompleted(ChallengeClass challenge, bool isRedeemed)
	{
		if (this.ChallengeComplete != null)
		{
			this.ChallengeComplete(challenge, isRedeemed);
		}
	}

	public event QuestEvent_TwitchEvent TwitchEventReceive;

	public void TwitchEventReceived(TwitchObjectiveTypes actionType, string param)
	{
		if (this.TwitchEventReceive != null)
		{
			this.TwitchEventReceive(actionType, param);
		}
	}

	public event QuestEvent_QuestCompleteEvent QuestComplete;

	public void QuestCompleted(FastTags<TagGroup.Global> questTags, QuestClass questClass)
	{
		if (this.QuestComplete != null)
		{
			this.QuestComplete(questTags, questClass);
		}
	}

	public event QuestEvent_ChallengeAwardCredit ChallengeAwardCredit;

	public void ChallengeAwardCredited(string challengeStat, int creditAmount)
	{
		if (this.ChallengeAwardCredit != null)
		{
			this.ChallengeAwardCredit(challengeStat, creditAmount);
		}
	}

	public event QuestEvent_ChallengeAwardCredit QuestAwardCredit;

	public void QuestAwardCredited(string stat, int creditAmount)
	{
		if (this.QuestAwardCredit != null)
		{
			this.QuestAwardCredit(stat, creditAmount);
		}
	}

	public event QuestEvent_BiomeEvent BiomeEnter;

	public void BiomeEntered(BiomeDefinition biomeDef)
	{
		if (this.BiomeEnter != null)
		{
			this.BiomeEnter(biomeDef);
		}
	}

	public event QuestEvent_ItemValueActionEvent UseItem;

	public void UsedItem(ItemValue newValue)
	{
		if (this.UseItem != null)
		{
			this.UseItem(newValue);
		}
	}

	public event QuestEvent_FloatEvent TimeSurvive;

	public void TimeSurvived(float time)
	{
		if (this.TimeSurvive != null)
		{
			this.TimeSurvive(time);
		}
	}

	public event QuestEvent_Event BloodMoonSurvive;

	public void BloodMoonSurvived()
	{
		if (this.BloodMoonSurvive != null)
		{
			this.BloodMoonSurvive();
		}
	}

	public void Update()
	{
		ObjectiveRallyPoint.SetupFlags(this.objectivesToUpdate);
		float deltaTime = Time.deltaTime;
		for (int i = 0; i < this.objectivesToUpdate.Count; i++)
		{
			this.objectivesToUpdate[i].HandleUpdate(deltaTime);
		}
		for (int j = 0; j < this.challengeObjectivesToUpdate.Count; j++)
		{
			this.challengeObjectivesToUpdate[j].HandleUpdate(deltaTime);
		}
		for (int k = this.questTrackersToUpdate.Count - 1; k >= 0; k--)
		{
			if (!this.questTrackersToUpdate[k].Update(deltaTime))
			{
				this.questTrackersToUpdate.RemoveAt(k);
			}
		}
		if (this.challengeTrackerToUpdate != null && !this.challengeTrackerToUpdate.Update(deltaTime))
		{
			this.challengeTrackerToUpdate = null;
		}
		foreach (KeyValuePair<Vector3, SleeperEventData> keyValuePair in this.SleeperVolumeUpdateDictionary)
		{
			if (keyValuePair.Value.Update())
			{
				this.removeSleeperDataList.Add(keyValuePair.Value.position);
			}
		}
		for (int l = 0; l < this.removeSleeperDataList.Count; l++)
		{
			this.SleeperVolumeUpdateDictionary.Remove(this.removeSleeperDataList[l]);
		}
		this.removeSleeperDataList.Clear();
	}

	public void HandlePlayerDisconnect(EntityPlayer player)
	{
		for (int i = 0; i < player.QuestJournal.quests.Count; i++)
		{
			Quest quest = player.QuestJournal.quests[i];
			if (quest.CurrentState == Quest.QuestState.InProgress)
			{
				quest.HandleUnlockPOI(player);
				this.FinishTreasureQuest(quest.QuestCode, player);
			}
		}
	}

	public void HandleAllPlayersDisconnect()
	{
		foreach (int num in this.TreasureQuestDictionary.Keys)
		{
			this.TreasureQuestDictionary[num].Remove();
		}
		this.TreasureQuestDictionary.Clear();
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void AddTraderResetQuestsForPlayer(int playerID, int traderID)
	{
		if (!this.ForceResetQuestTrader.ContainsKey(playerID))
		{
			this.ForceResetQuestTrader.Add(playerID, traderID);
			return;
		}
		this.ForceResetQuestTrader[playerID] = traderID;
	}

	public void ClearTraderResetQuestsForPlayer(int playerID)
	{
		if (this.ForceResetQuestTrader.ContainsKey(playerID))
		{
			this.ForceResetQuestTrader.Remove(playerID);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckResetQuestTrader(int playerEntityID, int npcEntityID)
	{
		if (!this.ForceResetQuestTrader.ContainsKey(playerEntityID))
		{
			return false;
		}
		Log.Out(string.Format("CheckResetQuestTrader {0}", this.ForceResetQuestTrader[playerEntityID] == npcEntityID));
		return this.ForceResetQuestTrader[playerEntityID] == npcEntityID;
	}

	public void AddObjectiveToBeUpdated(BaseObjective obj)
	{
		if (!this.objectivesToUpdate.Contains(obj))
		{
			this.objectivesToUpdate.Add(obj);
		}
	}

	public void RemoveObjectiveToBeUpdated(BaseObjective obj)
	{
		if (this.objectivesToUpdate.Contains(obj))
		{
			this.objectivesToUpdate.Remove(obj);
		}
	}

	public void AddObjectiveToBeUpdated(BaseChallengeObjective obj)
	{
		if (!this.challengeObjectivesToUpdate.Contains(obj))
		{
			this.challengeObjectivesToUpdate.Add(obj);
		}
	}

	public void RemoveObjectiveToBeUpdated(BaseChallengeObjective obj)
	{
		if (this.challengeObjectivesToUpdate.Contains(obj))
		{
			this.challengeObjectivesToUpdate.Remove(obj);
		}
	}

	public void AddTrackerToBeUpdated(TrackingHandler track)
	{
		if (!this.questTrackersToUpdate.Contains(track))
		{
			this.questTrackersToUpdate.Add(track);
		}
	}

	public void RemoveTrackerToBeUpdated(TrackingHandler track)
	{
		if (this.questTrackersToUpdate.Contains(track))
		{
			this.questTrackersToUpdate.Remove(track);
		}
	}

	public void AddTrackerToBeUpdated(ChallengeTrackingHandler track)
	{
		this.challengeTrackerToUpdate = track;
	}

	public void RemoveTrackerToBeUpdated(ChallengeTrackingHandler track)
	{
		this.challengeTrackerToUpdate = null;
	}

	public event QuestEvent_SleeperVolumePositionChanged SleeperVolumePositionAdd;

	public event QuestEvent_SleeperVolumePositionChanged SleeperVolumePositionRemove;

	public void SleeperVolumePositionAdded(Vector3 pos)
	{
		if (this.SleeperVolumePositionAdd != null)
		{
			this.SleeperVolumePositionAdd(pos);
		}
	}

	public void SleeperVolumePositionRemoved(Vector3 pos)
	{
		if (this.SleeperVolumePositionRemove != null)
		{
			this.SleeperVolumePositionRemove(pos);
		}
	}

	public void AddSleeperVolumeLocation(Vector3 newLocation)
	{
		this.SleeperVolumeLocationList.Add(newLocation);
	}

	public void SubscribeToUpdateEvent(int entityID, Vector3 prefabPos)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!this.SleeperVolumeUpdateDictionary.ContainsKey(prefabPos))
			{
				SleeperEventData sleeperEventData = new SleeperEventData();
				sleeperEventData.SetupData(prefabPos);
				this.SleeperVolumeUpdateDictionary.Add(prefabPos, sleeperEventData);
			}
			SleeperEventData sleeperEventData2 = this.SleeperVolumeUpdateDictionary[prefabPos];
			this.removeSleeperDataList.Remove(prefabPos);
			if (!sleeperEventData2.EntityList.Contains(entityID))
			{
				sleeperEventData2.EntityList.Add(entityID);
				return;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ClearSleeper, entityID, prefabPos, true), false);
		}
	}

	public void UnSubscribeToUpdateEvent(int entityID, Vector3 prefabPos)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!this.SleeperVolumeUpdateDictionary.ContainsKey(prefabPos))
			{
				return;
			}
			SleeperEventData sleeperEventData = this.SleeperVolumeUpdateDictionary[prefabPos];
			if (!sleeperEventData.EntityList.Contains(entityID))
			{
				return;
			}
			sleeperEventData.EntityList.Remove(entityID);
			if (sleeperEventData.EntityList.Count == 0)
			{
				this.removeSleeperDataList.Add(prefabPos);
			}
			using (List<SleeperVolume>.Enumerator enumerator = sleeperEventData.SleeperVolumes.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					SleeperVolume sleeperVolume = enumerator.Current;
					QuestEventManager.Current.SleeperVolumePositionRemoved(sleeperVolume.Center);
				}
				return;
			}
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ClearSleeper, entityID, prefabPos, false), false);
	}

	public IEnumerator QuestLockPOI(int entityID, QuestClass questClass, Vector3 prefabPos, FastTags<TagGroup.Global> questTags, int[] sharedWithList)
	{
		List<PrefabInstance> prefabsFromWorldPosInside = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabsFromWorldPosInside(prefabPos, questTags);
		for (int i = 0; i < prefabsFromWorldPosInside.Count; i++)
		{
			PrefabInstance prefabInstance = prefabsFromWorldPosInside[i];
			if (prefabInstance.prefab.GetQuestTag(questTags) && (prefabInstance.lockInstance == null || prefabInstance.lockInstance.CheckQuestLock()))
			{
				prefabInstance.lockInstance = new QuestLockInstance(entityID);
				if (sharedWithList != null)
				{
					prefabInstance.lockInstance.AddQuesters(sharedWithList);
				}
			}
		}
		yield return GameManager.Instance.World.ResetPOIS(prefabsFromWorldPosInside, questTags, entityID, sharedWithList, questClass);
		yield return null;
		if (GameManager.Instance.World.GetPrimaryPlayerId() != entityID)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.POILocked, entityID), false, entityID, -1, -1, null, 192, false);
		}
		yield break;
	}

	public void QuestUnlockPOI(int entityID, Vector3 prefabPos)
	{
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)prefabPos.x, (int)prefabPos.z);
		if (prefabFromWorldPos.lockInstance != null)
		{
			prefabFromWorldPos.lockInstance.RemoveQuester(entityID);
		}
	}

	public QuestEventManager.POILockoutReasonTypes CheckForPOILockouts(int entityId, Vector2 prefabPos, out ulong extraData)
	{
		World world = GameManager.Instance.World;
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)prefabPos.x, (int)prefabPos.y);
		Rect rect;
		rect..ctor((float)prefabFromWorldPos.boundingBoxPosition.x, (float)prefabFromWorldPos.boundingBoxPosition.z, (float)prefabFromWorldPos.boundingBoxSize.x, (float)prefabFromWorldPos.boundingBoxSize.z);
		if (prefabFromWorldPos.lockInstance != null && prefabFromWorldPos.lockInstance.CheckQuestLock())
		{
			prefabFromWorldPos.lockInstance = null;
		}
		if (prefabFromWorldPos.lockInstance != null)
		{
			extraData = prefabFromWorldPos.lockInstance.LockedOutUntil;
			return QuestEventManager.POILockoutReasonTypes.QuestLock;
		}
		extraData = 0UL;
		EntityPlayer entityPlayer = (EntityPlayer)world.GetEntity(entityId);
		if (entityPlayer != null)
		{
			for (int i = 0; i < world.Players.list.Count; i++)
			{
				Vector3 position = world.Players.list[i].GetPosition();
				EntityPlayer entityPlayer2 = world.Players.list[i];
				if (entityPlayer != entityPlayer2 && (!entityPlayer.IsInParty() || !entityPlayer.Party.MemberList.Contains(entityPlayer2)) && rect.Contains(new Vector2(position.x, position.z)))
				{
					return QuestEventManager.POILockoutReasonTypes.PlayerInside;
				}
			}
		}
		GameUtils.EPlayerHomeType eplayerHomeType = prefabFromWorldPos.CheckForAnyPlayerHome(world);
		if (eplayerHomeType == GameUtils.EPlayerHomeType.Landclaim)
		{
			return QuestEventManager.POILockoutReasonTypes.LandClaim;
		}
		if (eplayerHomeType == GameUtils.EPlayerHomeType.Bedroll)
		{
			return QuestEventManager.POILockoutReasonTypes.Bedroll;
		}
		return QuestEventManager.POILockoutReasonTypes.None;
	}

	public void SetupRepairForMP(List<Vector3i> repairBlockList, List<bool> repairStates, World _world, Vector3 prefabPos)
	{
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)prefabPos.x, (int)prefabPos.z);
		Vector3i vector3i = new Vector3i(prefabPos);
		Vector3i size = prefabFromWorldPos.prefab.size;
		int num = World.toChunkXZ(vector3i.x - 1);
		int num2 = World.toChunkXZ(vector3i.x + size.x + 1);
		int num3 = World.toChunkXZ(vector3i.z - 1);
		int num4 = World.toChunkXZ(vector3i.z + size.z + 1);
		repairBlockList.Clear();
		repairStates.Clear();
		Rect rect;
		rect..ctor((float)vector3i.x, (float)vector3i.z, (float)size.x, (float)size.z);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				Chunk chunk = _world.GetChunkSync(i, j) as Chunk;
				if (chunk != null)
				{
					List<Vector3i> list = chunk.IndexedBlocks[Constants.cQuestRestorePowerIndexName];
					if (list != null)
					{
						for (int k = 0; k < list.Count; k++)
						{
							BlockValue block = chunk.GetBlock(list[k]);
							if (!block.ischild)
							{
								Vector3i vector3i2 = chunk.ToWorldPos(list[k]);
								if (rect.Contains(new Vector2((float)vector3i2.x, (float)vector3i2.z)))
								{
									repairStates.Add(!block.Block.UpgradeBlock.isair);
									repairBlockList.Add(vector3i2);
								}
							}
						}
					}
				}
			}
		}
	}

	public void SetupActivateForMP(int entityID, int questCode, string completeEvent, List<Vector3i> activateBlockList, World _world, Vector3 prefabPos, string indexName, int[] sharedWithList)
	{
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)prefabPos.x, (int)prefabPos.z);
		Vector3i vector3i = new Vector3i(prefabPos);
		Vector3i size = prefabFromWorldPos.prefab.size;
		EntityPlayer entityPlayer = _world.GetEntity(entityID) as EntityPlayer;
		int num = World.toChunkXZ(vector3i.x - 1);
		int num2 = World.toChunkXZ(vector3i.x + size.x + 1);
		int num3 = World.toChunkXZ(vector3i.z - 1);
		int num4 = World.toChunkXZ(vector3i.z + size.z + 1);
		activateBlockList.Clear();
		Rect rect;
		rect..ctor((float)vector3i.x, (float)vector3i.z, (float)size.x, (float)size.z);
		new BlockChangeInfo();
		List<BlockChangeInfo> list = new List<BlockChangeInfo>();
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				Chunk chunk = _world.GetChunkSync(i, j) as Chunk;
				if (chunk != null)
				{
					List<Vector3i> list2 = chunk.IndexedBlocks[indexName];
					if (list2 != null)
					{
						for (int k = 0; k < list2.Count; k++)
						{
							BlockValue block = chunk.GetBlock(list2[k]);
							if (!block.ischild)
							{
								Vector3i vector3i2 = chunk.ToWorldPos(list2[k]);
								if (rect.Contains(new Vector2((float)vector3i2.x, (float)vector3i2.z)))
								{
									activateBlockList.Add(vector3i2);
									if (block.Block is BlockQuestActivate)
									{
										(block.Block as BlockQuestActivate).SetupForQuest(_world, chunk, vector3i2, block, list);
									}
								}
							}
						}
					}
				}
			}
		}
		if (entityPlayer is EntityPlayerLocal)
		{
			entityPlayer.QuestJournal.HandleRestorePowerReceived(prefabPos, activateBlockList);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupRestorePower, entityPlayer.entityId, questCode, completeEvent, prefabPos, activateBlockList), false, entityPlayer.entityId, -1, -1, null, 192, false);
		}
		QuestEventManager.Current.AddRestorePowerQuest(questCode, entityID, new Vector3i(prefabPos), completeEvent);
		if (entityPlayer.IsInParty() && sharedWithList != null)
		{
			Party party = entityPlayer.Party;
			for (int l = 0; l < sharedWithList.Length; l++)
			{
				EntityPlayer entityPlayer2 = _world.GetEntity(sharedWithList[l]) as EntityPlayer;
				if (entityPlayer2 is EntityPlayerLocal)
				{
					entityPlayer2.QuestJournal.HandleRestorePowerReceived(prefabPos, activateBlockList);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupRestorePower, entityPlayer2.entityId, questCode, completeEvent, prefabPos, activateBlockList), false, entityPlayer2.entityId, -1, -1, null, 192, false);
				}
				QuestEventManager.Current.AddRestorePowerQuest(questCode, sharedWithList[l], new Vector3i(prefabPos), completeEvent);
			}
		}
		if (list.Count > 0)
		{
			GameManager.Instance.StartCoroutine(this.UpdateBlocks(list));
		}
		GameEventManager.Current.HandleAction("quest_poi_lights_off", null, entityPlayer, false, vector3i, "", "", false, true, "", null, null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator UpdateBlocks(List<BlockChangeInfo> blockChanges)
	{
		yield return new WaitForSeconds(1f);
		if (GameManager.Instance != null && GameManager.Instance.World != null)
		{
			GameManager.Instance.World.SetBlocksRPC(blockChanges);
		}
		yield break;
	}

	public void SetupFetchForMP(int entityID, Vector3 prefabPos, ObjectiveFetchFromContainer.FetchModeTypes fetchMode, int[] sharedWithList)
	{
		PrefabInstance prefabFromWorldPos = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabFromWorldPos((int)prefabPos.x, (int)prefabPos.z);
		this.HandleContainerPositions(GameManager.Instance.World, entityID, new Vector3i(prefabPos), prefabFromWorldPos.prefab.size, fetchMode, sharedWithList);
	}

	public void HandleContainerPositions(World _world, int _entityID, Vector3i _prefabPosition, Vector3i _prefabSize, ObjectiveFetchFromContainer.FetchModeTypes fetchMode, int[] sharedWithList)
	{
		int num = World.toChunkXZ(_prefabPosition.x - 1);
		int num2 = World.toChunkXZ(_prefabPosition.x + _prefabSize.x + 1);
		int num3 = World.toChunkXZ(_prefabPosition.z - 1);
		int num4 = World.toChunkXZ(_prefabPosition.z + _prefabSize.z + 1);
		List<Vector3i> list = new List<Vector3i>();
		Rect rect;
		rect..ctor((float)_prefabPosition.x, (float)_prefabPosition.z, (float)_prefabSize.x, (float)_prefabSize.z);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				Chunk chunk = _world.GetChunkSync(i, j) as Chunk;
				if (chunk != null)
				{
					List<Vector3i> list2 = chunk.IndexedBlocks[Constants.cQuestLootFetchContainerIndexName];
					if (list2 != null)
					{
						for (int k = 0; k < list2.Count; k++)
						{
							if (!chunk.GetBlock(list2[k]).ischild)
							{
								Vector3i vector3i = chunk.ToWorldPos(list2[k]);
								if (rect.Contains(new Vector2((float)vector3i.x, (float)vector3i.z)))
								{
									list.Add(vector3i);
								}
							}
						}
					}
				}
			}
		}
		if (list.Count == 0)
		{
			Log.Error("Valid container not found for fetch loot.");
			return;
		}
		List<int> list3 = new List<int>();
		EntityPlayer entityPlayer = _world.GetEntity(_entityID) as EntityPlayer;
		Quest.PositionDataTypes positionDataTypes = ((fetchMode == ObjectiveFetchFromContainer.FetchModeTypes.Standard) ? Quest.PositionDataTypes.FetchContainer : Quest.PositionDataTypes.HiddenCache);
		int num5 = _world.GetGameRandom().RandomRange(list.Count);
		if (entityPlayer is EntityPlayerLocal)
		{
			entityPlayer.QuestJournal.SetActivePositionData(positionDataTypes, list[num5]);
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupFetch, _entityID, list[num5].ToVector3(), fetchMode), false, -1, -1, -1, null, 192, false);
		}
		list3.Add(num5);
		if (entityPlayer.IsInParty() && sharedWithList != null)
		{
			Party party = entityPlayer.Party;
			for (int l = 0; l < sharedWithList.Length; l++)
			{
				entityPlayer = _world.GetEntity(sharedWithList[l]) as EntityPlayer;
				num5 = _world.GetGameRandom().RandomRange(list.Count);
				if (entityPlayer is EntityPlayerLocal)
				{
					entityPlayer.QuestJournal.SetActivePositionData(positionDataTypes, list[num5]);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupFetch, entityPlayer.entityId, list[num5].ToVector3(), fetchMode), false, -1, -1, -1, null, 192, false);
				}
				if (!list3.Contains(num5))
				{
					list3.Add(num5);
				}
			}
		}
		List<BlockChangeInfo> list4 = new List<BlockChangeInfo>();
		GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
		for (int m = 0; m < list.Count; m++)
		{
			if (!list3.Contains(m))
			{
				Chunk chunk2 = (Chunk)_world.GetChunkFromWorldPos(list[m]);
				BlockValue blockValue = BlockPlaceholderMap.Instance.Replace(Block.GetBlockValue("cntQuestRandomLootHelper", false), gameRandom, chunk2, list[m].x, 0, list[m].z, FastTags<TagGroup.Global>.none, false, true);
				list4.Add(new BlockChangeInfo(list[m], blockValue));
			}
		}
		if (list4.Count > 0)
		{
			GameManager.Instance.StartCoroutine(this.UpdateBlocks(list4));
		}
	}

	public void Cleanup()
	{
		this.BlockPickup = null;
		this.BlockPlace = null;
		this.BlockUpgrade = null;
		this.AddItem = null;
		this.AssembleItem = null;
		this.CraftItem = null;
		this.ExchangeFromItem = null;
		this.ScrapItem = null;
		this.RepairItem = null;
		this.SkillPointSpent = null;
		this.WearItem = null;
		this.WindowChanged = null;
		this.ContainerOpened = null;
		this.EntityKill = null;
		this.HarvestItem = null;
		this.SellItems = null;
		this.BuyItems = null;
		this.ExplosionDetected = null;
		this.ChallengeComplete = null;
		this.BiomeEnter = null;
		this.UseItem = null;
		this.TimeSurvive = null;
		this.BloodMoonSurvive = null;
		this.objectivesToUpdate = null;
		this.npcQuestData.Clear();
		this.npcQuestData = null;
		this.questTierRewards.Clear();
		this.questTierRewards = null;
		QuestEventManager.instance = null;
	}

	public void SetupQuestList(EntityTrader npc, int playerEntityID, List<Quest> questList)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			return;
		}
		if (!this.npcQuestData.ContainsKey(npc.entityId))
		{
			this.npcQuestData.Add(npc.entityId, new NPCQuestData());
		}
		if (!this.npcQuestData[npc.entityId].PlayerQuestList.ContainsKey(playerEntityID))
		{
			this.npcQuestData[npc.entityId].PlayerQuestList.Add(playerEntityID, new NPCQuestData.PlayerQuestData(questList));
		}
		else
		{
			this.npcQuestData[npc.entityId].PlayerQuestList[playerEntityID].QuestList = questList;
		}
		if (!(GameManager.Instance.World.GetEntity(playerEntityID) is EntityPlayerLocal))
		{
			NetPackageNPCQuestList.SendQuestPacketsToPlayer(npc, playerEntityID);
		}
	}

	public List<Quest> GetQuestList(World world, int npcEntityID, int playerEntityID)
	{
		if (this.npcQuestData.ContainsKey(npcEntityID))
		{
			NPCQuestData npcquestData = this.npcQuestData[npcEntityID];
			if (npcquestData.PlayerQuestList.ContainsKey(playerEntityID))
			{
				NPCQuestData.PlayerQuestData playerQuestData = npcquestData.PlayerQuestList[playerEntityID];
				if (QuestEventManager.Current.CheckResetQuestTrader(playerEntityID, npcEntityID))
				{
					playerQuestData.QuestList.Clear();
					playerQuestData.QuestList = null;
					QuestEventManager.Current.ClearTraderResetQuestsForPlayer(playerEntityID);
				}
				else if ((int)(world.GetWorldTime() - playerQuestData.LastUpdate) > 24000)
				{
					playerQuestData.QuestList.Clear();
					playerQuestData.QuestList = null;
				}
				return playerQuestData.QuestList;
			}
		}
		return null;
	}

	public void ClearQuestList(int npcEntityID)
	{
		if (this.npcQuestData.ContainsKey(npcEntityID))
		{
			this.npcQuestData[npcEntityID].PlayerQuestList.Clear();
		}
	}

	public void ClearQuestListForPlayer(int npcEntityID, int playerID)
	{
		if (this.npcQuestData.ContainsKey(npcEntityID))
		{
			NPCQuestData npcquestData = this.npcQuestData[npcEntityID];
			if (npcquestData.PlayerQuestList.ContainsKey(playerID))
			{
				npcquestData.PlayerQuestList.Remove(playerID);
			}
		}
	}

	public void AddQuestTierReward(QuestTierReward reward)
	{
		if (this.questTierRewards == null)
		{
			this.questTierRewards = new List<QuestTierReward>();
		}
		this.questTierRewards.Add(reward);
	}

	public void HandleNewCompletedQuest(EntityPlayer player, byte questFaction, int completedQuestTier, bool addsToTierComplete)
	{
		if (addsToTierComplete)
		{
			int currentFactionTier = player.QuestJournal.GetCurrentFactionTier(questFaction, 0, true);
			int currentFactionTier2 = player.QuestJournal.GetCurrentFactionTier(questFaction, completedQuestTier, true);
			if (currentFactionTier != currentFactionTier2)
			{
				for (int i = 0; i < this.questTierRewards.Count; i++)
				{
					if (this.questTierRewards[i].Tier == currentFactionTier2)
					{
						this.questTierRewards[i].GiveRewards(player);
					}
				}
			}
		}
	}

	public void HandleRallyMarkerActivate(EntityPlayerLocal _player, Vector3i blockPos, BlockValue blockValue)
	{
		Quest quest = _player.QuestJournal.HasQuestAtRallyPosition(blockPos.ToVector3(), true);
		if (quest != null)
		{
			Action action = delegate
			{
				QuestEventManager.Current.BlockActivated(blockValue.Block.GetBlockName(), blockPos);
			};
			if (_player.IsInParty())
			{
				List<EntityPlayer> sharedWithListNotInRange = quest.GetSharedWithListNotInRange();
				if (sharedWithListNotInRange != null && sharedWithListNotInRange.Count > 0)
				{
					StringBuilder stringBuilder = new StringBuilder();
					for (int i = 0; i < sharedWithListNotInRange.Count; i++)
					{
						stringBuilder.Append(sharedWithListNotInRange[i].PlayerDisplayName);
						if (i < sharedWithListNotInRange.Count - 1)
						{
							stringBuilder.Append(", ");
						}
					}
					XUiC_MessageBoxWindowGroup.ShowOkCancel(_player.PlayerUI.xui, "Rally Activate", string.Format(Localization.Get("xuiQuestRallyOutOfRange", false, null), stringBuilder.ToString().Trim(',')), action, null, false, true, false);
					return;
				}
				action();
				return;
			}
			else
			{
				action();
			}
		}
	}

	public void AddTreasureQuest(int _questCode, int _entityID, int _blocksPerReduction, Vector3i _position, Vector3 _treasureOffset)
	{
		if (!this.TreasureQuestDictionary.ContainsKey(_questCode))
		{
			TreasureQuestData treasureQuestData = new TreasureQuestData(_questCode, _entityID, _blocksPerReduction, _position, _treasureOffset);
			this.TreasureQuestDictionary.Add(_questCode, treasureQuestData);
		}
	}

	public void SetTreasureContainerPosition(int _questCode, Vector3i _updatedPosition)
	{
		if (this.TreasureQuestDictionary.ContainsKey(_questCode))
		{
			this.TreasureQuestDictionary[_questCode].UpdatePosition(_updatedPosition);
		}
	}

	public bool GetTreasureContainerPosition(int _questCode, float _distance, int _offset, float _treasureRadius, Vector3 _startPosition, int _entityID, bool _useNearby, int _currentBlocksPerReduction, out int _blocksPerReduction, out Vector3i _position, out Vector3 _treasureOffset)
	{
		_position = Vector3i.zero;
		_treasureOffset = Vector3.zero;
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(_questCode, _distance, _offset, _treasureRadius, _startPosition, _entityID, _useNearby, _currentBlocksPerReduction), false);
			_position = Vector3i.zero;
			_treasureOffset = Vector3.zero;
			_blocksPerReduction = _currentBlocksPerReduction;
			return true;
		}
		if (this.TreasureQuestDictionary.ContainsKey(_questCode))
		{
			_position = this.TreasureQuestDictionary[_questCode].Position;
			_treasureOffset = this.TreasureQuestDictionary[_questCode].TreasureOffset;
			this.TreasureQuestDictionary[_questCode].AddSharedQuester(_entityID, _currentBlocksPerReduction);
			_blocksPerReduction = this.TreasureQuestDictionary[_questCode].BlocksPerReduction;
			return true;
		}
		_blocksPerReduction = _currentBlocksPerReduction;
		float num = _distance + 25f;
		for (float num2 = _distance; num2 < num; num2 += 2.5f)
		{
			for (int i = 0; i < 5; i++)
			{
				if (ObjectiveTreasureChest.CalculateTreasurePoint(_startPosition, num2, _offset, _treasureRadius - 1f, _useNearby, out _position, out _treasureOffset))
				{
					this.AddTreasureQuest(_questCode, _entityID, _blocksPerReduction, _position, _treasureOffset);
					return true;
				}
			}
		}
		return false;
	}

	public void UpdateTreasureBlocksPerReduction(int _questCode, int _newBlocksPerReduction)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (this.TreasureQuestDictionary.ContainsKey(_questCode))
			{
				this.TreasureQuestDictionary[_questCode].SendBlocksPerReductionUpdate(_newBlocksPerReduction);
				return;
			}
		}
		else
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestTreasurePoint>().Setup(_questCode, _newBlocksPerReduction), false);
		}
	}

	public void FinishTreasureQuest(int _questCode, EntityPlayer _player)
	{
		TreasureQuestData treasureQuestData;
		if (this.TreasureQuestDictionary.TryGetValue(_questCode, out treasureQuestData))
		{
			treasureQuestData.RemoveSharedQuester(_player);
			ChunkProviderGenerateWorld chunkProviderGenerateWorld = GameManager.Instance.World.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;
			if (chunkProviderGenerateWorld != null)
			{
				Debug.Log(string.Format("[FinishTreasureQuest] Requesting reset at world position: {0}", treasureQuestData.Position));
				Vector2i vector2i = World.toChunkXZ(treasureQuestData.Position);
				for (int i = vector2i.x - 1; i <= vector2i.x + 1; i++)
				{
					for (int j = vector2i.y - 1; j <= vector2i.y + 1; j++)
					{
						long num = WorldChunkCache.MakeChunkKey(i, j);
						chunkProviderGenerateWorld.RequestChunkReset(num);
					}
				}
			}
		}
	}

	public void AddRestorePowerQuest(int _questCode, int _entityID, Vector3i _position, string _completeEvent)
	{
		if (!this.BlockActivateQuestDictionary.ContainsKey(_questCode))
		{
			RestorePowerQuestData restorePowerQuestData = new RestorePowerQuestData(_questCode, _entityID, _position, _completeEvent);
			this.BlockActivateQuestDictionary.Add(_questCode, restorePowerQuestData);
			return;
		}
		this.BlockActivateQuestDictionary[_questCode].AddSharedQuester(_entityID);
	}

	public void FinishManagedQuest(int _questCode, EntityPlayer _player)
	{
		if (this.BlockActivateQuestDictionary.ContainsKey(_questCode))
		{
			this.BlockActivateQuestDictionary[_questCode].RemoveSharedQuester(_player);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static QuestEventManager instance = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BaseObjective> objectivesToUpdate = new List<BaseObjective>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BaseChallengeObjective> challengeObjectivesToUpdate = new List<BaseChallengeObjective>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<TrackingHandler> questTrackersToUpdate = new List<TrackingHandler>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ChallengeTrackingHandler challengeTrackerToUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3> removeSleeperDataList = new List<Vector3>();

	public Dictionary<int, NPCQuestData> npcQuestData = new Dictionary<int, NPCQuestData>();

	public List<QuestTierReward> questTierRewards = new List<QuestTierReward>();

	public Dictionary<Vector3, SleeperEventData> SleeperVolumeUpdateDictionary = new Dictionary<Vector3, SleeperEventData>();

	public List<Vector3> SleeperVolumeLocationList = new List<Vector3>();

	public Dictionary<int, TreasureQuestData> TreasureQuestDictionary = new Dictionary<int, TreasureQuestData>();

	public Dictionary<int, RestorePowerQuestData> BlockActivateQuestDictionary = new Dictionary<int, RestorePowerQuestData>();

	public Dictionary<int, List<PrefabInstance>> tierPrefabList = new Dictionary<int, List<PrefabInstance>>();

	public Dictionary<TraderArea, List<QuestEventManager.PrefabListData>> TraderPrefabList = new Dictionary<TraderArea, List<QuestEventManager.PrefabListData>>();

	public Rect QuestBounds;

	public List<Vector3i> ActiveQuestBlocks = new List<Vector3i>();

	public Dictionary<int, int> ForceResetQuestTrader = new Dictionary<int, int>();

	public static FastTags<TagGroup.Global> manualResetTag = FastTags<TagGroup.Global>.Parse("manual");

	public static FastTags<TagGroup.Global> traderTag = FastTags<TagGroup.Global>.Parse("trader");

	public static FastTags<TagGroup.Global> clearTag = FastTags<TagGroup.Global>.Parse("clear");

	public static FastTags<TagGroup.Global> treasureTag = FastTags<TagGroup.Global>.Parse("treasure");

	public static FastTags<TagGroup.Global> fetchTag = FastTags<TagGroup.Global>.Parse("fetch");

	public static FastTags<TagGroup.Global> craftingTag = FastTags<TagGroup.Global>.Parse("crafting");

	public static FastTags<TagGroup.Global> restorePowerTag = FastTags<TagGroup.Global>.Parse("restore_power");

	public static FastTags<TagGroup.Global> infestedTag = FastTags<TagGroup.Global>.Parse("infested");

	public static FastTags<TagGroup.Global> banditTag = FastTags<TagGroup.Global>.Parse("bandit");

	public static FastTags<TagGroup.Global> allQuestTags = FastTags<TagGroup.Global>.CombineTags(FastTags<TagGroup.Global>.CombineTags(QuestEventManager.traderTag, QuestEventManager.clearTag, QuestEventManager.treasureTag, QuestEventManager.fetchTag), FastTags<TagGroup.Global>.CombineTags(QuestEventManager.craftingTag, QuestEventManager.restorePowerTag), FastTags<TagGroup.Global>.CombineTags(QuestEventManager.infestedTag, QuestEventManager.banditTag));

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cTreasurePointAttempts = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTreasurePointDistanceAdd = 2.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTreasurePointMaxDistanceAdd = 25f;

	public enum POILockoutReasonTypes
	{
		None,
		PlayerInside,
		Bedroll,
		LandClaim,
		QuestLock
	}

	public class PrefabListData
	{
		public void AddPOI(PrefabInstance poi)
		{
			int difficultyTier = (int)poi.prefab.DifficultyTier;
			if (!this.TierData.ContainsKey(difficultyTier))
			{
				this.TierData.Add(difficultyTier, new List<PrefabInstance>());
			}
			this.TierData[difficultyTier].Add(poi);
		}

		public void ShuffleDifficulty(int difficulty, GameRandom gameRandom)
		{
			List<PrefabInstance> list;
			if (this.TierData.TryGetValue(difficulty, out list))
			{
				list.Shuffle(gameRandom);
			}
		}

		public Dictionary<int, List<PrefabInstance>> TierData = new Dictionary<int, List<PrefabInstance>>();
	}
}
