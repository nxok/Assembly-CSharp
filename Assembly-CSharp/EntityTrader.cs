using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityTrader : EntityNPC, ITrader
{
	public TraderData TraderData { get; set; }

	public int PreferredDistanceIndex
	{
		get
		{
			return this.preferredDistanceIndex;
		}
	}

	public TraderInfo TraderInfo
	{
		get
		{
			if (this.TraderData != null)
			{
				return this.TraderData.TraderInfo;
			}
			return null;
		}
	}

	public override bool IsValidAimAssistSnapTarget
	{
		get
		{
			return false;
		}
	}

	public override void InitLocation(Vector3 _pos, Vector3 _rot)
	{
		_pos.y = Mathf.Floor(_pos.y);
		base.InitLocation(_pos, _rot);
		this.PhysicsTransform.gameObject.SetActive(false);
	}

	public override void PostInit()
	{
		base.PostInit();
		if (this.TraderData == null)
		{
			this.TraderData = new TraderData();
		}
		this.SetupStartingItems();
		if (base.NPCInfo != null && base.NPCInfo.TraderID > 0)
		{
			this.TraderData.TraderID = base.NPCInfo.TraderID;
			this.IsGodMode.Value = true;
		}
		this.inventory.SetHoldingItemIdx(0);
		this.emodel.avatarController.SetArchetypeStance(base.NPCInfo.CurrentStance);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SetupStartingItems()
	{
		for (int i = 0; i < this.itemsOnEnterGame.Count; i++)
		{
			ItemStack itemStack = this.itemsOnEnterGame[i];
			ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
			if (forId.HasQuality)
			{
				itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, null, 1f);
			}
			else
			{
				itemStack.count = forId.Stacknumber.Value;
			}
			this.inventory.SetItem(i, itemStack);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void InitLocalActivationCommands(Action<EntityActivationCommand> _addCallback)
	{
		_addCallback(new EntityActivationCommand("talk", "talk", null, null));
		_addCallback(new EntityActivationCommand("trade", "map_trader", null, null));
		_addCallback(new EntityActivationCommand("remove", "x", null, null));
	}

	public void SetupActiveQuestsForPlayer(EntityPlayer player, int overrideFactionPoints = -1)
	{
		this.activeQuests = this.PopulateActiveQuests(player, -1, overrideFactionPoints);
		QuestEventManager.Current.SetupQuestList(this, player.entityId, this.activeQuests);
	}

	public override bool AllowActivationCommand(ReadOnlySpan<char> _commandName, EntityPlayerLocal _playerFocusing)
	{
		if (this.IsDead() || base.NPCInfo == null)
		{
			return false;
		}
		if (base.CommandIs(_commandName, "talk") || base.CommandIs(_commandName, "trade"))
		{
			return true;
		}
		if (base.CommandIs(_commandName, "remove"))
		{
			return GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) && !GameUtils.IsPlaytesting();
		}
		return base.AllowActivationCommand(_commandName, _playerFocusing);
	}

	public override string GetActivationText()
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		string text = primaryPlayer.playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + primaryPlayer.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		return string.Format(Localization.Get("npcTooltipTalk", false, null), text, this.LocalizedEntityName);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityActivated(EntityActivationCommand _command, EntityPlayerLocal _playerFocusing)
	{
		if (!this.TraderData.TraderInfo.IsTraderActivitiesOpen)
		{
			GameManager.ShowTooltip(_playerFocusing, this.GetNextTimeMessage(), string.Empty, "ui_denied", null, true, false, 0f);
			return;
		}
		if (_playerFocusing != null && (!_playerFocusing.PlayerUI.windowManager.IsModalWindowOpen() || _playerFocusing.PlayerUI.windowManager.GetModalWindow().Id == "radial"))
		{
			LockManager.Instance.LockRequestLocal(this, new EntityTrader.EntityTraderLockContext(_command.commandId.ToString(), this.TraderData), 0);
		}
	}

	public override void MoveEntityHeaded(Vector3 _direction, bool _isDirAbsolute)
	{
	}

	public string GetNextTimeMessage()
	{
		return string.Format("ttNoInteractTrader", GameUtils.WorldTimeToHourMinutesString(this.TraderInfo.GetOpenTime()));
	}

	public override void OnUpdateLive()
	{
		base.OnUpdateLive();
		if (this.questDictionary.Count == 0)
		{
			this.PopulateQuestList();
		}
		if (this.nativeCollider)
		{
			this.nativeCollider.enabled = true;
			this.nativeCollider.includeLayers = 17825792;
		}
		if (!GameManager.IsDedicatedServer)
		{
			EntityPlayerLocal primaryPlayer = this.world.GetPrimaryPlayer();
			this.emodel.SetLookAt(primaryPlayer.getHeadPosition());
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (this.traderArea == null)
			{
				this.traderArea = this.world.GetTraderAreaAt(new Vector3i(this.position));
			}
			if (this.traderArea != null)
			{
				if (this.updateTime <= 0f)
				{
					this.updateTime = Time.time + 3f;
				}
				if (Time.time > this.updateTime)
				{
					this.updateTime = Time.time + 1f;
					List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(this, new Bounds(this.position, Vector3.one * 10f));
					if (entitiesInBounds.Count > 0)
					{
						for (int i = 0; i < entitiesInBounds.Count; i++)
						{
							if (entitiesInBounds[i] is EntityNPC && entitiesInBounds[i].EntityClass == base.EntityClass)
							{
								if (entitiesInBounds[i].entityId < this.entityId)
								{
									this.IsDespawned = true;
									this.MarkToUnload();
								}
							}
							else if (entitiesInBounds[i] is EntityPlayer)
							{
								EntityPlayer entityPlayer = entitiesInBounds[i] as EntityPlayer;
								if (base.CanSee(entityPlayer))
								{
									if (this.GreetingDictionary.ContainsKey(entityPlayer))
									{
										if (Time.time < this.GreetingDictionary[entityPlayer])
										{
											this.GreetingDictionary[entityPlayer] = Time.time + EntityTrader.traderTalkDelayTime;
											goto IL_02BD;
										}
										this.GreetingDictionary[entityPlayer] = Time.time + EntityTrader.traderTalkDelayTime;
									}
									else
									{
										this.GreetingDictionary.Add(entityPlayer, Time.time + EntityTrader.traderTalkDelayTime);
									}
									int worldHour = this.world.WorldHour;
									if (this.world.isEventBloodMoon)
									{
										this.PlayVoiceSetEntry("greetbloodmoon", entityPlayer, false, true);
									}
									else if (worldHour >= 4 && worldHour <= 11)
									{
										this.PlayVoiceSetEntry("greetmorn", entityPlayer, false, true);
									}
									else if (worldHour >= 12 && worldHour <= 16)
									{
										this.PlayVoiceSetEntry("greetaft", entityPlayer, false, true);
									}
									else if (worldHour >= 17 && worldHour <= 19)
									{
										this.PlayVoiceSetEntry("greeteve", entityPlayer, false, true);
									}
									else if (worldHour >= 20)
									{
										this.PlayVoiceSetEntry("greetnightfall", entityPlayer, false, true);
									}
									else
									{
										this.PlayVoiceSetEntry("greeting", entityPlayer, false, true);
									}
									this.SendAnimReaction(1);
								}
							}
							IL_02BD:;
						}
					}
					if (this.TraderInfo == null)
					{
						return;
					}
					if (!this.traderArea.IsClosed)
					{
						if (this.TraderInfo.IsWarningTime)
						{
							if (!this.warningPlayed)
							{
								this.warningPlayed = true;
								this.traderArea.HandleWarning(this.world, this);
							}
						}
						else
						{
							this.warningPlayed = false;
						}
					}
					bool flag = !this.TraderInfo.IsOpen;
					if (this.traderArea.IsClosed != flag || this.firstTime)
					{
						bool flag2;
						if (flag)
						{
							flag2 = this.TraderInfo.ShouldPlayCloseSound;
							if (LockManager.Instance.IsLockedServer(this, 0))
							{
								LockManager.Instance.ForceUnlockLockTarget(this);
							}
						}
						else
						{
							flag2 = this.TraderInfo.ShouldPlayOpenSound;
						}
						this.firstTime = !this.traderArea.SetClosed(this.world, flag, this, flag2);
					}
				}
			}
		}
	}

	public void PopulateQuestList()
	{
		if (base.NPCInfo == null || base.NPCInfo.Quests == null)
		{
			return;
		}
		this.specialQuestList = new List<QuestEntry>();
		this.questDictionary.Clear();
		for (int i = 0; i < base.NPCInfo.Quests.Count; i++)
		{
			string questID = base.NPCInfo.Quests[i].QuestID;
			if (QuestClass.GetQuest(questID).CheckCriteriaQuestGiver(this))
			{
				QuestEntry questEntry = base.NPCInfo.Quests[i];
				questEntry.QuestID = questID;
				if (questEntry.QuestClass.UniqueKey == "")
				{
					if (!this.questDictionary.ContainsKey((int)questEntry.QuestClass.DifficultyTier))
					{
						this.questDictionary.Add((int)questEntry.QuestClass.DifficultyTier, new List<QuestEntry>());
					}
					this.questDictionary[(int)questEntry.QuestClass.DifficultyTier].Add(questEntry);
				}
				else
				{
					this.specialQuestList.Add(questEntry);
				}
			}
		}
	}

	public bool UpdateLocations(int tier, List<Vector2> pois)
	{
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			List<PrefabInstance> prefabsForTrader = QuestEventManager.Current.GetPrefabsForTrader(this.traderArea, tier, i, null);
			num += ((prefabsForTrader != null) ? prefabsForTrader.Count : 0);
		}
		return pois.Count >= num;
	}

	public void SetActiveQuests(EntityPlayer player, NetPackageNPCQuestList.QuestPacketEntry[] questList)
	{
		if (this.activeQuests == null)
		{
			this.activeQuests = new List<Quest>();
		}
		this.activeQuests.Clear();
		if (questList != null)
		{
			for (int i = 0; i < questList.Length; i++)
			{
				Quest quest = QuestClass.GetQuest(questList[i].QuestID).CreateQuest();
				quest.QuestGiverID = this.entityId;
				quest.QuestFaction = base.NPCInfo.QuestFaction;
				quest.SetPosition(this, questList[i].QuestLocation, questList[i].QuestSize);
				quest.SetPositionData(Quest.PositionDataTypes.QuestGiver, this.position);
				quest.SetPositionData(Quest.PositionDataTypes.TraderPosition, questList[i].TraderPos);
				quest.DataVariables.Add("POIName", Localization.Get(questList[i].POIName, false, null));
				this.activeQuests.Add(quest);
			}
		}
	}

	public void ClearActiveQuests(int playerID)
	{
		try
		{
			this.activeQuests = null;
		}
		catch
		{
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			QuestEventManager.Current.ClearQuestListForPlayer(this.entityId, playerID);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageNPCQuestList>().Setup(this.entityId, playerID), false);
	}

	public List<Quest> PopulateActiveQuests(EntityPlayer player, int currentTier = -1, int questFactionPoints = -1)
	{
		if (this.questDictionary.Count == 0)
		{
			this.PopulateQuestList();
			if (this.questDictionary.Count == 0)
			{
				return null;
			}
		}
		bool @bool = GameStats.GetBool(EnumGameStats.EnemySpawnMode);
		List<Quest> list = new List<Quest>();
		this.uniqueKeysUsed.Clear();
		Vector2 vector;
		if (this.traderArea != null)
		{
			vector..ctor((float)this.traderArea.Position.x, (float)this.traderArea.Position.z);
		}
		else
		{
			vector..ctor(this.position.x, this.position.z);
		}
		if (currentTier == -1)
		{
			currentTier = player.QuestJournal.GetCurrentFactionTier(base.NPCInfo.QuestFaction, 0, false);
		}
		if (questFactionPoints == -1)
		{
			questFactionPoints = player.QuestJournal.GlobalFactionPoints;
		}
		QuestTraderData traderData = player.QuestJournal.GetTraderData(vector);
		if (traderData != null)
		{
			traderData.CheckReset(player);
		}
		this.usedPOILocations.Clear();
		List<QuestEntry> list2 = new List<QuestEntry>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		for (int i = 1; i <= currentTier; i++)
		{
			dictionary.Clear();
			List<Vector2> usedPOIs = player.QuestJournal.GetUsedPOIs(vector, i);
			if (usedPOIs != null)
			{
				if (this.UpdateLocations(i, usedPOIs))
				{
					if (traderData != null)
					{
						traderData.ClearTier(i);
						if (!(player is EntityPlayerLocal))
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageNPCQuestList>().SetupClear(player.entityId, vector, i), false, player.entityId, -1, -1, null, 192, false);
						}
					}
				}
				else
				{
					this.usedPOILocations.AddRange(usedPOIs);
				}
			}
			list2.Clear();
			List<QuestEntry> list3 = this.questDictionary[i];
			for (int j = 0; j < list3.Count; j++)
			{
				QuestEntry questEntry = list3[j];
				if ((questEntry.StartStage == -1 || questEntry.StartStage <= questFactionPoints) && (questEntry.EndStage == -1 || questEntry.EndStage >= questFactionPoints) && questEntry.CheckRequirement(player))
				{
					list2.Add(questEntry);
				}
			}
			int num = 0;
			int num2 = 0;
			while (num2 < 100 && list2.Count != 0)
			{
				this.preferredDistanceIndex = this.distanceIndices[num];
				int num3 = this.rand.RandomRange(list2.Count);
				QuestEntry questEntry2 = list2[num3];
				QuestClass questClass = questEntry2.QuestClass;
				bool flag = false;
				int num4;
				if (dictionary.TryGetValue(questClass.Name, out num4))
				{
					if (questClass.MaxQuestCount == 0 || num4 < questClass.MaxQuestCount)
					{
						dictionary[questClass.Name] = num4 + 1;
					}
					else
					{
						flag = true;
					}
				}
				else
				{
					dictionary[questClass.Name] = 1;
				}
				if (!flag && this.rand.RandomFloat < questEntry2.Prob)
				{
					Quest quest = questClass.CreateQuest();
					quest.QuestGiverID = this.entityId;
					quest.QuestFaction = base.NPCInfo.QuestFaction;
					quest.SetPositionData(Quest.PositionDataTypes.QuestGiver, this.position);
					quest.SetPositionData(Quest.PositionDataTypes.TraderPosition, (this.traderArea != null) ? this.traderArea.Position : this.position);
					quest.SetupTags();
					if (@bool || !quest.QuestTags.Test_AnySet(QuestEventManager.clearTag))
					{
						if (quest.SetupPosition(this, player, this.usedPOILocations, player.entityId))
						{
							this.preferredDistanceIndex = (this.preferredDistanceIndex + 1) % 3;
							if (questClass.SingleQuest)
							{
								list2.RemoveAt(num3);
							}
							list.Add(quest);
							num++;
						}
						if (quest.QuestTags.Test_AnySet(QuestEventManager.treasureTag) && GameSparksCollector.CollectGamePlayData)
						{
							GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestOfferedDistance, ((int)Vector3.Distance(quest.Position, this.position) / 50 * 50).ToString(), 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
						}
						if (num >= this.distanceIndices.Length)
						{
							break;
						}
					}
				}
				num2++;
			}
		}
		for (int k = 0; k < this.specialQuestList.Count; k++)
		{
			QuestEntry questEntry3 = this.specialQuestList[k];
			if ((questEntry3.StartStage == -1 || questEntry3.StartStage <= questFactionPoints) && (questEntry3.EndStage == -1 || questEntry3.EndStage >= questFactionPoints) && questEntry3.CheckRequirement(player))
			{
				list2.Add(questEntry3);
				if (questEntry3.QuestClass.UniqueKey == "" || !this.uniqueKeysUsed.Contains(questEntry3.QuestClass.UniqueKey))
				{
					QuestClass questClass2 = questEntry3.QuestClass;
					if ((int)(questClass2.DifficultyTier - 1) <= currentTier && !player.QuestJournal.FindCompletedQuest(questClass2.ID, questClass2.Repeatable ? ((int)base.NPCInfo.QuestFaction) : (-1)))
					{
						int l = 0;
						while (l < 100)
						{
							Quest quest2 = questClass2.CreateQuest();
							quest2.QuestGiverID = this.entityId;
							quest2.QuestFaction = base.NPCInfo.QuestFaction;
							quest2.SetPositionData(Quest.PositionDataTypes.QuestGiver, this.position);
							quest2.SetPositionData(Quest.PositionDataTypes.TraderPosition, (this.traderArea != null) ? this.traderArea.Position : this.position);
							quest2.SetupTags();
							if (!quest2.NeedsNPCSetPosition || quest2.SetupPosition(this, player, this.usedPOILocations, player.entityId))
							{
								list.Add(quest2);
								if (questClass2.UniqueKey != "")
								{
									this.uniqueKeysUsed.Add(questClass2.UniqueKey);
								}
								if (GameSparksCollector.CollectGamePlayData)
								{
									GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.QuestTraderToTraderDistance, ((int)Vector3.Distance(quest2.Position, this.position) / 50 * 50).ToString(), 1, true, GameSparksCollector.GSDataCollection.SessionUpdates);
									break;
								}
								break;
							}
							else
							{
								l++;
							}
						}
					}
				}
			}
		}
		list.Sort(delegate(Quest q0, Quest q1)
		{
			float sqrMagnitude = (q0.Position - player.position).sqrMagnitude;
			float sqrMagnitude2 = (q1.Position - player.position).sqrMagnitude;
			return sqrMagnitude.CompareTo(sqrMagnitude2);
		});
		return list;
	}

	public int GetQuestFactionPoints(EntityPlayer player)
	{
		return player.QuestJournal.GlobalFactionPoints;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityTargeted(EntityAlive target)
	{
		base.OnEntityTargeted(target);
		if (!this.isEntityRemote && base.GetSpawnerSource() != EnumSpawnerSource.Dynamic && target)
		{
			this.world.aiDirector.NotifyIntentToAttack(this, target);
		}
	}

	public override void ProcessDamageResponseLocal(DamageResponse _dmResponse)
	{
		if (base.NPCInfo != null && base.NPCInfo.TraderID > 0)
		{
			return;
		}
		base.SetAttackTarget((EntityAlive)GameManager.Instance.World.GetEntity(_dmResponse.Source.getEntityId()), 600);
		base.ProcessDamageResponseLocal(_dmResponse);
	}

	public override bool CanDamageEntity(int _sourceEntityId)
	{
		return false;
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
	{
		if (base.NPCInfo != null && base.NPCInfo.TraderID > 0)
		{
			return 0;
		}
		return base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
	}

	public override void AwardKill(EntityAlive killer)
	{
		if (base.NPCInfo != null && base.NPCInfo.TraderID > 0)
		{
			return;
		}
		base.AwardKill(killer);
	}

	public override Vector3 GetLookVector()
	{
		if (this.lookAtPosition.Equals(Vector3.zero))
		{
			return base.GetLookVector();
		}
		return Vector3.Normalize(this.lookAtPosition - this.getHeadPosition());
	}

	public override Ray GetLookRay()
	{
		return new Ray(this.position + new Vector3(0f, this.GetEyeHeight() * this.eyeHeightHackMod, 0f), this.GetLookVector());
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateSpeedForwardAndStrafe(Vector3 _dist, float _partialTicks)
	{
	}

	public override void PlayVoiceSetEntry(string name, EntityPlayer player, bool ignoreTime = true, bool showReactionAnim = true)
	{
		if (!TraderInfo.TraderDialog)
		{
			return;
		}
		if (this.lastVoiceTime - Time.time < 0f || ignoreTime)
		{
			string voiceSet = base.NPCInfo.VoiceSet;
			string text = base.NPCInfo.CurrentStance.ToStringCached<NPCInfo.StanceTypes>();
			if (voiceSet == "" || text == "")
			{
				return;
			}
			string text2 = (voiceSet + "_" + name).ToLower();
			Manager.StopAllSequencesOnEntity((player == null) ? this : player);
			if (player == null)
			{
				this.PlayOneShot(text2, false, false, false, null, 1f);
			}
			else if (player.isEntityRemote)
			{
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageAudioPlayInHead>().Setup(text2, true), false, player.entityId, -1, -1, null, 192, false);
			}
			else
			{
				player.PlayOneShot(text2, true, false, true, null, 1f);
			}
			if (showReactionAnim)
			{
				this.PlayAnimReaction(EntityTrader.AnimReaction.Neutral);
			}
			if (!ignoreTime)
			{
				this.lastVoiceTime = Time.time + 5f;
			}
		}
	}

	public void PlayAnimReaction(EntityTrader.AnimReaction reaction)
	{
		AvatarController avatarController = this.emodel.avatarController;
		if (avatarController)
		{
			avatarController.TriggerReaction((int)reaction);
		}
	}

	public void SendAnimReaction(int reaction)
	{
		List<AnimParamData> list = new List<AnimParamData>();
		list.Add(new AnimParamData(AvatarController.reactionTypeHash, AnimParamData.ValueTypes.Int, reaction));
		list.Add(new AnimParamData(AvatarController.reactionTriggerHash, AnimParamData.ValueTypes.Trigger, true));
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAnimationData>().Setup(this.entityId, list), false, -1, -1, this.entityId, null, 192, false);
	}

	public override bool IsSharedLock(ushort _channel)
	{
		if (_channel == 0)
		{
			return true;
		}
		if (_channel != 1)
		{
			throw new ArgumentOutOfRangeException("_channel", string.Format("Unsupported lock channel: {0}", _channel));
		}
		return false;
	}

	public override bool CanLockOnServer(int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		return !this.IsDead() && (this.traderArea == null || !this.traderArea.IsClosed || GameUtils.IsPlaytesting());
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return !this.IsDead() && LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		if (!_success)
		{
			return;
		}
		if (_channel == 0)
		{
			this.activeQuests = QuestEventManager.Current.GetQuestList(GameManager.Instance.World, this.entityId, _lockingPlayerID);
			if (this.activeQuests == null)
			{
				this.SetupActiveQuestsForPlayer(this.world.GetEntity(_lockingPlayerID) as EntityPlayer, -1);
			}
			else if (_lockingPlayerID != this.world.GetPrimaryPlayerId())
			{
				NetPackageNPCQuestList.SendQuestPacketsToPlayer(this, _lockingPlayerID);
			}
		}
		if (_channel == 1)
		{
			EntityTrader.EntityTraderLockContext entityTraderLockContext = _context as EntityTrader.EntityTraderLockContext;
			if (entityTraderLockContext != null)
			{
				GameManager.Instance.traderManager.TraderInventoryRequested(this.TraderData, _lockingPlayerID);
				entityTraderLockContext.TraderData = this.TraderData.Clone();
			}
		}
	}

	public override void OnUnlockedServer(int _unlockingPlayerId, ushort _channel)
	{
		if (_channel == 1)
		{
			this.TraderData.SetModified(this);
		}
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		if (_channel == 0)
		{
			if (!_success)
			{
				return;
			}
			EntityTrader.EntityTraderLockContext entityTraderLockContext = _context as EntityTrader.EntityTraderLockContext;
			if (entityTraderLockContext == null)
			{
				Log.Warning("[EntityTrader] Missing or invalid lock context.");
				LockManager.Instance.UnlockRequestLocal();
				return;
			}
			LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
			this.SetNextTraderWindow(EntityTrader.TraderWindowState.Close);
			QuestEventManager.Current.NPCInteracted(this);
			QuestEventManager.Current.NPCMet(this);
			Quest nextCompletedQuest = uiforPrimaryPlayer.entityPlayer.QuestJournal.GetNextCompletedQuest(null, this.entityId);
			uiforPrimaryPlayer.xui.Trader.Trader = ((nextCompletedQuest != null) ? null : this);
			if (nextCompletedQuest != null)
			{
				uiforPrimaryPlayer.xui.Dialog.QuestTurnIn = nextCompletedQuest;
				this.SetNextTraderWindow(EntityTrader.TraderWindowState.QuestComplete);
			}
			else if (base.CommandIs(entityTraderLockContext.Command, "talk"))
			{
				uiforPrimaryPlayer.xui.Dialog.Respondent = this;
				this.SetNextTraderWindow(EntityTrader.TraderWindowState.Dialog);
				QuestEventManager.Current.NPCInteracted(this);
			}
			else if (base.CommandIs(entityTraderLockContext.Command, "trade"))
			{
				this.SetNextTraderWindow(EntityTrader.TraderWindowState.Trade);
			}
			else
			{
				if (!base.CommandIs(entityTraderLockContext.Command, "remove"))
				{
					Log.Warning("[EntityTrader] Unexpected lock command '" + entityTraderLockContext.Command + "'.");
					LockManager.Instance.UnlockRequestLocal();
					return;
				}
				EntityPlayerLocal entityPlayer = uiforPrimaryPlayer.entityPlayer;
				Waypoint waypoint = ((entityPlayer != null) ? entityPlayer.Waypoints.GetLastKnownPositionWaypoint(this.entityId) : null);
				if (waypoint != null)
				{
					uiforPrimaryPlayer.entityPlayer.Waypoints.Collection.Remove(waypoint);
					NavObjectManager.Instance.UnRegisterNavObjectByPosition(waypoint.pos, "waypoint");
				}
				GameEventManager.Current.HandleAction("game_remove_entity", uiforPrimaryPlayer.entityPlayer, this, false, "", "", false, true, "", null);
			}
			this.TransitionToNextWindow();
		}
		if (_channel == 1)
		{
			LocalPlayerUI uiforPrimaryPlayer2 = LocalPlayerUI.GetUIForPrimaryPlayer();
			if (!_success)
			{
				GameManager.ShowTooltip(uiforPrimaryPlayer2.entityPlayer, Localization.Get("ttNoInteractPerson", false, null), string.Empty, "ui_denied", null, false, false, 0f);
				this.SetNextTraderWindow(EntityTrader.TraderWindowState.Close);
				this.TransitionToNextWindow();
				return;
			}
			EntityTrader.EntityTraderLockContext entityTraderLockContext2 = _context as EntityTrader.EntityTraderLockContext;
			if (entityTraderLockContext2 != null && entityTraderLockContext2.TraderData != null)
			{
				this.TraderData.CopyFrom(entityTraderLockContext2.TraderData);
			}
			uiforPrimaryPlayer2.xui.Trader.Trader = this;
			uiforPrimaryPlayer2.windowManager.CloseAllOpenModalWindows(null, false);
			uiforPrimaryPlayer2.windowManager.Open("trader", true);
		}
	}

	public void SetNextTraderWindow(EntityTrader.TraderWindowState _nextState)
	{
		this.nextWindow = _nextState;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TransitionToNextWindow()
	{
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		EntityPlayerLocal entityPlayer = uiforPrimaryPlayer.entityPlayer;
		uiforPrimaryPlayer.entityPlayer.OverrideFOV = 30f;
		uiforPrimaryPlayer.xui.Dialog.Respondent = this;
		uiforPrimaryPlayer.xui.Trader.Trader = this;
		uiforPrimaryPlayer.xui.Dialog.KeepZoomOnClose = true;
		EntityTrader.TraderWindowState traderWindowState = this.nextWindow;
		this.nextWindow = EntityTrader.TraderWindowState.Close;
		switch (traderWindowState)
		{
		case EntityTrader.TraderWindowState.Dialog:
			XUiC_DialogWindowGroup.Open(uiforPrimaryPlayer.xui, new Action(this.TransitionToNextWindow));
			return;
		case EntityTrader.TraderWindowState.Trade:
			LockManager.Instance.UnlockRequestLocal();
			LockManager.Instance.LockRequestLocal(this, new EntityTrader.EntityTraderLockContext("trade", this.TraderData), 1);
			return;
		case EntityTrader.TraderWindowState.QuestComplete:
			this.PlayVoiceSetEntry("quest_complete", entityPlayer, true, true);
			XUiC_QuestTurnInWindowGroup.Open(uiforPrimaryPlayer.xui, new Action(this.TransitionToNextWindow));
			return;
		case EntityTrader.TraderWindowState.Close:
			uiforPrimaryPlayer.xui.Dialog.Respondent = null;
			uiforPrimaryPlayer.entityPlayer.OverrideFOV = -1f;
			uiforPrimaryPlayer.xui.Dialog.KeepZoomOnClose = false;
			uiforPrimaryPlayer.xui.Trader.Trader = null;
			LockManager.Instance.UnlockRequestLocal();
			return;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public float eyeHeightHackMod = 1f;

	public bool ShowWornEquipment;

	public TraderArea traderArea;

	public Dictionary<EntityPlayer, float> GreetingDictionary = new Dictionary<EntityPlayer, float>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool firstTime = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float updateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool warningPlayed;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static float traderTalkDelayTime = 90f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string CommandTalk = "talk";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string CommandTrade = "trade";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string CommandRemove = "remove";

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int preferredDistanceIndex;

	public List<QuestEntry> specialQuestList;

	public Dictionary<int, List<QuestEntry>> questDictionary = new Dictionary<int, List<QuestEntry>>();

	public List<Quest> activeQuests;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Vector2> usedPOILocations = new List<Vector2>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<string> uniqueKeysUsed = new List<string>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public readonly int[] distanceIndices = new int[] { 0, 0, 0, 1, 2, 2, 2 };

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastVoiceTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityTrader.TraderWindowState nextWindow = EntityTrader.TraderWindowState.Close;

	public enum AnimReaction
	{
		Happy,
		Neutral,
		Angry
	}

	[Preserve]
	public class EntityTraderLockContext : ILockContext
	{
		public string Command
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public EntityTraderLockContext()
		{
		}

		public EntityTraderLockContext(string _command, TraderData _traderData = null)
		{
			this.Command = _command;
			this.TraderData = ((_traderData != null) ? _traderData.Clone() : null);
		}

		public void Read(PooledBinaryReader _br)
		{
			this.Command = _br.ReadString();
			if (_br.ReadBoolean())
			{
				if (this.TraderData == null)
				{
					this.TraderData = new TraderData();
				}
				this.TraderData.Read(_br);
				return;
			}
			this.TraderData = null;
		}

		public void Write(PooledBinaryWriter _bw)
		{
			_bw.Write(this.Command);
			_bw.Write(this.TraderData != null);
			if (this.TraderData != null)
			{
				this.TraderData.Write(_bw);
			}
		}

		public TraderData TraderData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum LockChannel
	{
		Interaction,
		Trade
	}

	public enum TraderWindowState
	{
		Dialog,
		Trade,
		QuestComplete,
		Close
	}
}
