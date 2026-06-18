using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Audio;
using Quests;
using Quests.Requirements;
using UnityEngine;

public class Quest
{
	public Quest.QuestState CurrentState
	{
		get
		{
			return this._currentState;
		}
		set
		{
			if (this._currentState != value)
			{
				this._currentState = value;
				PrefabInstance.RefreshSwitchesInContainingPoi(this);
			}
		}
	}

	public string ID
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public byte CurrentQuestVersion { get; set; }

	public byte CurrentFileVersion { get; set; }

	public string PreviousQuest { get; set; }

	public bool OptionalComplete
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ulong FinishTime { get; set; }

	public byte CurrentPhase { get; set; }

	public int SharedOwnerID
	{
		get
		{
			return this.sharedOwnerID;
		}
		set
		{
			if (this.sharedOwnerID != value)
			{
				this.sharedOwnerID = value;
			}
		}
	}

	public static int QuestsPerDay
	{
		get
		{
			return GameStats.GetInt(EnumGameStats.QuestProgressionDailyLimit);
		}
	}

	public void AddQuestTag(FastTags<TagGroup.Global> tag)
	{
		this.QuestTags |= tag;
	}

	public bool Active
	{
		get
		{
			return this.CurrentState == Quest.QuestState.InProgress || this.CurrentState == Quest.QuestState.ReadyForTurnIn;
		}
	}

	public Vector3 Position
	{
		get
		{
			return this.position;
		}
		set
		{
			this.position = value;
		}
	}

	public bool HasPosition
	{
		get
		{
			return this.MapObject != null || this.NavObject != null;
		}
	}

	public string RequirementsString
	{
		get
		{
			if (this.Requirements.Count > 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				for (int i = 0; i < this.Requirements.Count; i++)
				{
					stringBuilder.Append(this.Requirements[i].CheckRequirement() ? "[DEFAULT_COLOR]" : "[MISSING_COLOR]");
					stringBuilder.Append(this.Requirements[i].Description);
					stringBuilder.Append("[-]");
					stringBuilder.Append((i < this.Requirements.Count - 1) ? ", " : "");
				}
				return stringBuilder.ToString();
			}
			return "";
		}
	}

	public bool Tracked
	{
		get
		{
			return this.tracked;
		}
		set
		{
			if (this.tracked)
			{
				this.SetMapObjectSelected(false);
			}
			this.tracked = value;
			if (this.tracked)
			{
				this.SetMapObjectSelected(true);
			}
		}
	}

	public byte GetActionIndex(BaseQuestAction action)
	{
		for (int i = 0; i < this.Actions.Count; i++)
		{
			if (action == this.Actions[i])
			{
				return (byte)i;
			}
		}
		return 0;
	}

	public byte GetObjectiveIndex(BaseObjective objective)
	{
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			if (objective == this.Objectives[i])
			{
				return (byte)i;
			}
		}
		return 0;
	}

	public int ActiveObjectives
	{
		get
		{
			int num = 0;
			for (int i = 0; i < this.Objectives.Count; i++)
			{
				if ((this.Objectives[i].Phase == 0 || this.Objectives[i].Phase == this.CurrentPhase) && !this.Objectives[i].HiddenObjective)
				{
					num++;
				}
			}
			return num;
		}
	}

	public QuestClass QuestClass
	{
		get
		{
			if (this.questClass == null)
			{
				this.questClass = QuestClass.GetQuest(this.ID);
			}
			return this.questClass;
		}
	}

	public bool IsShareable
	{
		get
		{
			return this.SharedOwnerID == -1 && this.QuestClass.Shareable && !this.RallyMarkerActivated && this.CurrentState == Quest.QuestState.InProgress;
		}
	}

	public MapObject MapObject
	{
		get
		{
			return this.mapObject;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.mapObject != null)
			{
				GameManager.Instance.World.ObjectOnMapRemove(this.mapObject.type, (int)this.mapObject.key);
			}
			this.mapObject = value;
		}
	}

	public bool AddsProgression
	{
		get
		{
			return this.QuestProgressDay > 0;
		}
	}

	public void HandleMapObject(Quest.PositionDataTypes dataType, string navObjectName, int defaultTreasureRadius = -1)
	{
		if (this.OwnerJournal == null)
		{
			return;
		}
		this.RemoveMapObject();
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		float num = -1f;
		bool flag = false;
		switch (dataType)
		{
		case Quest.PositionDataTypes.QuestGiver:
			if (this.GetPositionData(out zero, Quest.PositionDataTypes.QuestGiver))
			{
				this.Position = zero;
				if (navObjectName == "")
				{
					this.MapObject = new MapObjectQuest(zero, "ui_game_symbol_quest");
					GameManager.Instance.World.ObjectOnMapAdd(this.MapObject);
				}
				flag = true;
			}
			break;
		case Quest.PositionDataTypes.Location:
			if (this.GetPositionData(out zero, Quest.PositionDataTypes.Location))
			{
				this.Position = zero;
				if (navObjectName == "")
				{
					this.MapObject = new MapObjectQuest(zero, "ui_game_symbol_quest");
					GameManager.Instance.World.ObjectOnMapAdd(this.MapObject);
				}
				new Vector3(0.5f, 0f, 0.5f);
				flag = true;
			}
			break;
		case Quest.PositionDataTypes.POIPosition:
			if (this.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition))
			{
				Vector3 zero3 = Vector3.zero;
				if (this.GetPositionData(out zero3, Quest.PositionDataTypes.POISize))
				{
					Vector3 vector;
					vector..ctor(zero.x + zero3.x / 2f, zero.y, zero.z + zero3.z / 2f);
					vector.y = (float)((int)GameManager.Instance.World.GetHeightAt(vector.x, vector.y));
					this.Position = vector;
					if (navObjectName == "")
					{
						this.MapObject = new MapObjectQuest(vector, "ui_game_symbol_quest");
						GameManager.Instance.World.ObjectOnMapAdd(this.MapObject);
					}
				}
				new Vector3(0.5f, 0f, 0.5f);
				flag = true;
			}
			break;
		case Quest.PositionDataTypes.TreasurePoint:
			if (this.GetPositionData(out zero, Quest.PositionDataTypes.TreasurePoint))
			{
				if (defaultTreasureRadius == -1)
				{
					defaultTreasureRadius = ObjectiveTreasureChest.TreasureRadiusInitial;
				}
				float num2 = EffectManager.GetValue(PassiveEffects.TreasureRadius, null, (float)defaultTreasureRadius, this.OwnerJournal.OwnerPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				num2 = Mathf.Clamp(num2, 0f, (float)defaultTreasureRadius);
				World world = GameManager.Instance.World;
				Vector3 vector2;
				this.GetPositionData(out vector2, Quest.PositionDataTypes.TreasureOffset);
				this.Position = zero + vector2 * num2;
				if (navObjectName == "")
				{
					if (this.MapObject is MapObjectTreasureChest)
					{
						(this.MapObject as MapObjectTreasureChest).SetPosition(this.Position);
					}
					else
					{
						this.MapObject = new MapObjectTreasureChest(this.Position, this.QuestCode, defaultTreasureRadius);
						GameManager.Instance.World.ObjectOnMapAdd(this.MapObject);
					}
				}
				else
				{
					num = (float)defaultTreasureRadius;
				}
				flag = true;
			}
			break;
		case Quest.PositionDataTypes.FetchContainer:
			if (this.GetPositionData(out zero, Quest.PositionDataTypes.FetchContainer))
			{
				this.Position = zero;
				if (navObjectName == "")
				{
					this.MapObject = new MapObjectFetchItem(zero + Vector3.one * 0.5f);
					GameManager.Instance.World.ObjectOnMapAdd(this.MapObject);
				}
				new Vector3(0.5f, 0f, 0.5f);
				flag = true;
			}
			break;
		case Quest.PositionDataTypes.HiddenCache:
			if (this.GetPositionData(out zero, Quest.PositionDataTypes.HiddenCache))
			{
				this.Position = zero;
				if (navObjectName == "")
				{
					this.MapObject = new MapObjectHiddenCache(zero + Vector3.one * 0.5f);
					GameManager.Instance.World.ObjectOnMapAdd(this.MapObject);
				}
				new Vector3(0.5f, 0f, 0.5f);
				flag = true;
			}
			break;
		case Quest.PositionDataTypes.Activate:
			if (this.GetPositionData(out zero, Quest.PositionDataTypes.Activate))
			{
				this.Position = zero;
				if (navObjectName == "")
				{
					this.MapObject = new MapObjectQuest(zero + Vector3.one * 0.5f, "ui_game_symbol_quest");
					GameManager.Instance.World.ObjectOnMapAdd(this.MapObject);
				}
				new Vector3(0.5f, 0f, 0.5f);
				flag = true;
			}
			break;
		}
		if (navObjectName != "" && flag)
		{
			World world2 = GameManager.Instance.World;
			EntityPlayer entityPlayer = world2.GetEntity(this.sharedOwnerID) as EntityPlayer;
			if (entityPlayer == null)
			{
				entityPlayer = world2.GetPrimaryPlayer();
			}
			this.NavObject = NavObjectManager.Instance.RegisterNavObject(navObjectName, this.Position + new Vector3(0.5f, 0f, 0.5f), "", false, -1, entityPlayer);
			this.NavObject.IsActive = false;
			this.NavObject.ExtraData = num;
			QuestClass questClass = this.QuestClass;
			this.NavObject.name = string.Format("{0} ({1})", questClass.Name, entityPlayer.PlayerDisplayName);
		}
		this.SetMapObjectSelected(this.tracked);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetMapObjectSelected(bool isSelected)
	{
		if (this.NavObject != null)
		{
			this.NavObject.IsActive = isSelected;
		}
		if (this.MapObject != null)
		{
			if (this.MapObject is MapObjectQuest)
			{
				((MapObjectQuest)this.MapObject).IsSelected = isSelected;
				return;
			}
			if (this.MapObject is MapObjectTreasureChest)
			{
				((MapObjectTreasureChest)this.MapObject).IsSelected = isSelected;
				return;
			}
			if (this.MapObject is MapObjectFetchItem)
			{
				((MapObjectFetchItem)this.MapObject).IsSelected = isSelected;
				return;
			}
			if (this.MapObject is MapObjectHiddenCache)
			{
				((MapObjectHiddenCache)this.MapObject).IsSelected = isSelected;
				return;
			}
			if (this.MapObject is MapObjectRestorePower)
			{
				((MapObjectRestorePower)this.MapObject).IsSelected = isSelected;
			}
		}
	}

	public void SetupQuestCode()
	{
		if (this.QuestCode == 0)
		{
			this.QuestCode = string.Concat(new string[]
			{
				Time.unscaledTime.ToString(),
				"_",
				this.ID,
				"_",
				this.OwnerJournal.OwnerPlayer.entityId.ToString(),
				"_",
				this.QuestGiverID.ToString()
			}).GetHashCode();
		}
	}

	public void RemoveMapObject()
	{
		if (this.MapObject != null)
		{
			this.MapObject = null;
		}
		if (this.NavObject != null)
		{
			NavObjectManager.Instance.UnRegisterNavObject(this.NavObject);
			this.NavObject = null;
		}
		Vector3 zero = Vector3.zero;
		if (this.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition))
		{
			Vector3 zero2 = Vector3.zero;
			if (this.GetPositionData(out zero2, Quest.PositionDataTypes.POISize))
			{
				Vector3 vector;
				vector..ctor(zero.x + zero2.x / 2f, this.OwnerJournal.OwnerPlayer.position.y, zero.z + zero2.z / 2f);
				GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.Quest, vector);
				return;
			}
		}
		else
		{
			if (this.GetPositionData(out zero, Quest.PositionDataTypes.Location))
			{
				GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.Quest, zero);
				return;
			}
			if (this.GetPositionData(out zero, Quest.PositionDataTypes.TreasurePoint))
			{
				GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.TreasureChest, this.QuestCode);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void UnhookQuest()
	{
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			this.Objectives[i].HandleRemoveHooks();
		}
		for (int j = 0; j < this.Objectives.Count; j++)
		{
			this.Objectives[j].RemoveObjectives();
		}
		this.RemoveMapObject();
	}

	public Quest(string id)
	{
		this.ID = id;
		this.CurrentPhase = 1;
		this.CurrentState = Quest.QuestState.InProgress;
	}

	public void SetupTags()
	{
		this.NeedsNPCSetPosition = false;
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			this.Objectives[i].OwnerQuest = this;
			this.Objectives[i].HandleVariables();
			this.Objectives[i].SetupQuestTag();
			if (this.Objectives[i].NeedsNPCSetPosition)
			{
				this.NeedsNPCSetPosition = true;
			}
		}
	}

	public bool SetupPosition(EntityNPC ownerNPC, EntityPlayer player = null, List<Vector2> usedPOILocations = null, int entityIDforQuests = -1)
	{
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			if (this.Objectives[i].SetupPosition(ownerNPC, player, usedPOILocations, entityIDforQuests))
			{
				return true;
			}
		}
		return false;
	}

	public void SetPosition(EntityNPC ownerNPC, Vector3 position, Vector3 size)
	{
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			this.Objectives[i].OwnerQuest = this;
			this.Objectives[i].HandleVariables();
			this.Objectives[i].SetupQuestTag();
		}
		for (int j = 0; j < this.Objectives.Count; j++)
		{
			this.Objectives[j].SetPosition(position, size);
		}
	}

	public void SetObjectivePosition(Quest.PositionDataTypes dataType, Vector3i position)
	{
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			this.Objectives[i].OwnerQuest = this;
			this.Objectives[i].HandleVariables();
			this.Objectives[i].SetupQuestTag();
		}
		for (int j = 0; j < this.Objectives.Count; j++)
		{
			this.Objectives[j].SetPosition(dataType, position);
		}
	}

	public bool HandleRallyMarkerActivation(Vector3 prefabPos, bool rallyMarkerActivated, QuestEventManager.POILockoutReasonTypes lockoutReason, ulong extraData)
	{
		Vector3 zero = Vector3.zero;
		if (!this.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition))
		{
			return false;
		}
		if (zero != prefabPos)
		{
			return false;
		}
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			this.Objectives[i].OwnerQuest = this;
			this.Objectives[i].HandleVariables();
			this.Objectives[i].SetupQuestTag();
		}
		for (int j = 0; j < this.Objectives.Count; j++)
		{
			ObjectiveRallyPoint objectiveRallyPoint = this.Objectives[j] as ObjectiveRallyPoint;
			if (objectiveRallyPoint != null)
			{
				objectiveRallyPoint.RallyPointActivate(prefabPos, rallyMarkerActivated, lockoutReason, extraData);
				return true;
			}
		}
		return false;
	}

	public void RefreshRallyMarker()
	{
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			if (this.Objectives[i] is ObjectiveRallyPoint && this.Objectives[i].Phase == this.CurrentPhase)
			{
				(this.Objectives[i] as ObjectiveRallyPoint).RallyPointRefresh();
				return;
			}
		}
	}

	public bool CheckIsQuestGiver(int entityID)
	{
		Entity entity = GameManager.Instance.World.GetEntity(entityID);
		return this.QuestGiverID == entityID || (entity != null && (entity.position - this.GetQuestGiverLocation()).magnitude < 3f);
	}

	public Vector3 GetQuestGiverLocation()
	{
		Vector3 zero = Vector3.zero;
		if (this.GetPositionData(out zero, Quest.PositionDataTypes.QuestGiver))
		{
			return zero;
		}
		return Vector3.zero;
	}

	public Vector3 GetLocation()
	{
		Vector3 zero = Vector3.zero;
		if (this.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition))
		{
			return zero;
		}
		if (this.GetPositionData(out zero, Quest.PositionDataTypes.TreasurePoint))
		{
			return zero;
		}
		if (this.GetPositionData(out zero, Quest.PositionDataTypes.Location))
		{
			return zero;
		}
		return Vector3.zero;
	}

	public Vector3 GetLocationSize()
	{
		Vector3 zero = Vector3.zero;
		if (this.GetPositionData(out zero, Quest.PositionDataTypes.POISize))
		{
			return zero;
		}
		return Vector3.zero;
	}

	public Rect GetLocationRect()
	{
		int num = 5;
		Vector3 zero = Vector3.zero;
		if (this.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition))
		{
			Vector3 zero2 = Vector3.zero;
			if (this.GetPositionData(out zero2, Quest.PositionDataTypes.POISize))
			{
				return new Rect(zero.x - (float)num, zero.z - (float)num, zero2.x + (float)(num * 2), zero2.z + (float)(num * 2));
			}
		}
		return Rect.zero;
	}

	public void StartQuest(bool newQuest = true, bool notify = true)
	{
		if (newQuest)
		{
			this.CurrentState = Quest.QuestState.InProgress;
		}
		for (int i = 0; i < this.Actions.Count; i++)
		{
			this.Actions[i].OwnerQuest = this;
			this.Actions[i].HandleVariables();
			this.Actions[i].SetupAction();
			if (newQuest && this.Actions[i].Phase == (int)this.CurrentPhase && !this.Actions[i].OnComplete)
			{
				this.Actions[i].HandlePerformAction();
			}
		}
		for (int j = 0; j < this.Requirements.Count; j++)
		{
			this.Requirements[j].OwnerQuest = this;
			this.Requirements[j].HandleVariables();
			this.Requirements[j].SetupRequirement();
		}
		for (int k = 0; k < this.Objectives.Count; k++)
		{
			this.Objectives[k].OwnerQuest = this;
			this.Objectives[k].HandleVariables();
			this.Objectives[k].SetupQuestTag();
		}
		for (int l = 0; l < this.Objectives.Count; l++)
		{
			this.Objectives[l].SetupObjective();
			this.Objectives[l].SetupDisplay();
		}
		for (int m = 0; m < this.Objectives.Count; m++)
		{
			if (this.Objectives[m].Phase == this.CurrentPhase)
			{
				if (this.Objectives[m].ObjectiveState == BaseObjective.ObjectiveStates.NotStarted)
				{
					this.Objectives[m].ObjectiveState = BaseObjective.ObjectiveStates.InProgress;
				}
				if (this.CurrentState == Quest.QuestState.InProgress && this.Objectives[m].Phase == this.CurrentPhase)
				{
					this.Objectives[m].HandleRemoveHooks();
					this.Objectives[m].HandleAddHooks();
					this.Objectives[m].Refresh();
				}
			}
		}
		bool flag = false;
		for (int n = 0; n < this.Rewards.Count; n++)
		{
			this.Rewards[n].OwnerQuest = this;
			this.Rewards[n].HandleVariables();
			if (this.Rewards[n].ReceiveStage == BaseReward.ReceiveStages.QuestStart && newQuest)
			{
				this.Rewards[n].GiveReward();
			}
			if (this.Rewards[n].RewardIndex > 0 && !flag)
			{
				flag = true;
			}
		}
		if (!flag)
		{
			this.SetupRewards();
		}
		if (newQuest && notify)
		{
			QuestClass quest = QuestClass.GetQuest(this.ID);
			string text = ((quest.Name == quest.SubTitle) ? quest.Name : string.Format("{0} - {1}", quest.Name, quest.SubTitle));
			GameManager.ShowTooltip(this.OwnerJournal.OwnerPlayer, string.Format("{0} {1}: {2}", quest.Category, Localization.Get("started", false, null), text), false, false, 0f);
			Manager.PlayInsidePlayerHead("quest_started", -1, 0f, false, false);
			GameManager.Instance.StartCoroutine(this.trackLater(this));
		}
		this.SetupQuestCode();
		this.TrackingHelper.LocalPlayer = this.OwnerJournal.OwnerPlayer;
		this.TrackingHelper.QuestCode = this.QuestCode;
		this.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, false, null);
	}

	public void SetupSharedQuest()
	{
		this.CurrentState = Quest.QuestState.NotStarted;
		for (int i = 0; i < this.Actions.Count; i++)
		{
			this.Actions[i].OwnerQuest = this;
			this.Actions[i].HandleVariables();
			this.Actions[i].SetupAction();
		}
		for (int j = 0; j < this.Requirements.Count; j++)
		{
			this.Requirements[j].OwnerQuest = this;
			this.Requirements[j].HandleVariables();
			this.Requirements[j].SetupRequirement();
		}
		for (int k = 0; k < this.Objectives.Count; k++)
		{
			this.Objectives[k].OwnerQuest = this;
			this.Objectives[k].HandleVariables();
			this.Objectives[k].SetupObjective();
			this.Objectives[k].SetupDisplay();
		}
		for (int l = 0; l < this.Rewards.Count; l++)
		{
			this.Rewards[l].OwnerQuest = this;
			this.Rewards[l].HandleVariables();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AdvancePhase()
	{
		byte currentPhase = this.CurrentPhase;
		this.CurrentPhase = currentPhase + 1;
		for (int i = 0; i < this.Actions.Count; i++)
		{
			if (this.Actions[i].Phase == (int)this.CurrentPhase && !this.Actions[i].OnComplete)
			{
				this.Actions[i].HandlePerformAction();
			}
		}
		for (int j = 0; j < this.Objectives.Count; j++)
		{
			if (this.CurrentState == Quest.QuestState.InProgress)
			{
				if (this.Objectives[j].Phase == this.CurrentPhase - 1)
				{
					this.Objectives[j].HandlePhaseCompleted();
				}
				if (this.Objectives[j].Phase == this.CurrentPhase || this.Objectives[j].Phase == 0)
				{
					if (this.Objectives[j].ObjectiveState == BaseObjective.ObjectiveStates.NotStarted)
					{
						this.Objectives[j].ObjectiveState = BaseObjective.ObjectiveStates.InProgress;
					}
					this.Objectives[j].HandleRemoveHooks();
					this.Objectives[j].HandleAddHooks();
				}
				else
				{
					this.Objectives[j].HandleRemoveHooks();
				}
			}
			else
			{
				this.Objectives[j].HandleRemoveHooks();
			}
		}
	}

	public void SetupRewards()
	{
		int num = 0;
		for (int i = 0; i < this.Rewards.Count; i++)
		{
			BaseReward baseReward = this.Rewards[i];
			baseReward.RewardIndex = (byte)i;
			if (!baseReward.isChainReward && baseReward.isChosenReward && !baseReward.isFixedLocation)
			{
				num++;
			}
		}
		if (num > 1)
		{
			World world = this.OwnerJournal.OwnerPlayer.world;
			for (int j = 0; j < 100; j++)
			{
				int num2 = world.GetGameRandom().RandomRange(this.Rewards.Count);
				int num3 = world.GetGameRandom().RandomRange(this.Rewards.Count);
				if (num2 != num3)
				{
					BaseReward baseReward2 = this.Rewards[num2];
					BaseReward baseReward3 = this.Rewards[num3];
					if (!baseReward2.isFixedLocation && baseReward2.isChosenReward && !baseReward3.isFixedLocation && baseReward3.isChosenReward)
					{
						byte rewardIndex = this.Rewards[num2].RewardIndex;
						this.Rewards[num2].RewardIndex = this.Rewards[num3].RewardIndex;
						this.Rewards[num3].RewardIndex = rewardIndex;
					}
				}
			}
		}
	}

	public bool HandleActivateListReceived(Vector3 prefabPos, List<Vector3i> activateList)
	{
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			if (this.Objectives[i].SetupActivationList(prefabPos, activateList))
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TrackQuest_Event(object obj)
	{
		Quest quest = (Quest)obj;
		GameManager.Instance.StartCoroutine(this.trackLater(quest));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator trackLater(Quest q)
	{
		yield return new WaitForSeconds(0.5f);
		if (XUi.IsGameRunning())
		{
			if (q.CurrentState == Quest.QuestState.InProgress)
			{
				if (this.OwnerJournal != null && null != this.OwnerJournal.OwnerPlayer)
				{
					LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(this.OwnerJournal.OwnerPlayer);
					if (null != uiforPlayer && null != uiforPlayer.xui && uiforPlayer.xui.QuestTracker != null)
					{
						XUiM_Quest questTracker = uiforPlayer.xui.QuestTracker;
						if (uiforPlayer.xui.Recipes.TrackedRecipe == null && questTracker.TrackedChallenge == null && questTracker.TrackedQuest == null)
						{
							q.Tracked = uiforPlayer.xui.QuestTracker.TrackedQuest == null || uiforPlayer.xui.QuestTracker.TrackedQuest == q;
						}
					}
					else
					{
						q.Tracked = false;
					}
					this.OwnerJournal.RefreshTracked();
				}
				else
				{
					q.Tracked = false;
				}
			}
			else
			{
				q.Tracked = false;
			}
		}
		yield break;
	}

	public bool CheckRequirements()
	{
		for (int i = 0; i < this.Requirements.Count; i++)
		{
			if ((this.Requirements[i].Phase == 0 || this.Requirements[i].Phase == (int)this.CurrentPhase) && !this.Requirements[i].CheckRequirement())
			{
				return false;
			}
		}
		return true;
	}

	public Quest Clone()
	{
		Quest quest = new Quest(this.ID);
		quest.ID = this.ID;
		quest.OwnerJournal = this.OwnerJournal;
		quest.CurrentQuestVersion = this.CurrentQuestVersion;
		quest.CurrentState = this.CurrentState;
		quest.FinishTime = this.FinishTime;
		quest.SharedOwnerID = this.SharedOwnerID;
		quest.QuestGiverID = this.QuestGiverID;
		quest.CurrentPhase = this.CurrentPhase;
		quest.QuestCode = this.QuestCode;
		quest.RallyMarkerActivated = this.RallyMarkerActivated;
		quest.Tracked = this.Tracked;
		quest.OptionalComplete = this.OptionalComplete;
		quest.QuestTags = this.QuestTags;
		quest.TrackingHelper = this.TrackingHelper;
		quest.QuestFaction = this.QuestFaction;
		quest.QuestProgressDay = this.QuestProgressDay;
		for (int i = 0; i < this.Actions.Count; i++)
		{
			BaseQuestAction baseQuestAction = this.Actions[i].Clone();
			baseQuestAction.OwnerQuest = quest;
			quest.Actions.Add(baseQuestAction);
		}
		for (int j = 0; j < this.Requirements.Count; j++)
		{
			BaseRequirement baseRequirement = this.Requirements[j].Clone();
			baseRequirement.OwnerQuest = quest;
			quest.Requirements.Add(baseRequirement);
		}
		for (int k = 0; k < this.Objectives.Count; k++)
		{
			BaseObjective baseObjective = this.Objectives[k].Clone();
			baseObjective.OwnerQuest = quest;
			quest.Objectives.Add(baseObjective);
		}
		for (int l = 0; l < this.Rewards.Count; l++)
		{
			BaseReward baseReward = this.Rewards[l].Clone();
			baseReward.OwnerQuest = quest;
			quest.Rewards.Add(baseReward);
		}
		foreach (KeyValuePair<string, string> keyValuePair in this.DataVariables)
		{
			quest.DataVariables.Add(keyValuePair.Key, keyValuePair.Value);
		}
		foreach (KeyValuePair<Quest.PositionDataTypes, Vector3> keyValuePair2 in this.PositionData)
		{
			quest.PositionData.Add(keyValuePair2.Key, keyValuePair2.Value);
		}
		return quest;
	}

	public void ResetObjectives()
	{
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			this.Objectives[i].ResetObjective();
		}
	}

	public void ResetToRallyPointObjective()
	{
		if (this.CurrentPhase == this.QuestClass.HighestPhase || !this.QuestClass.LoginRallyReset)
		{
			return;
		}
		this.RallyMarkerActivated = false;
		int num = -1;
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			if (this.Objectives[i] is ObjectiveRallyPoint)
			{
				num = (int)this.Objectives[i].Phase;
			}
			if (num != -1 && (int)this.Objectives[i].Phase >= num && this.Objectives[i].Phase <= this.CurrentPhase)
			{
				this.Objectives[i].ResetObjective();
			}
		}
		if (num != -1 && num < (int)this.CurrentPhase)
		{
			this.CurrentPhase = (byte)num;
		}
	}

	public void RefreshQuestCompletion(QuestClass.CompletionTypes currentCompletionType = QuestClass.CompletionTypes.AutoComplete, List<BaseReward> rewardChoice = null, bool playObjectiveComplete = true, EntityNPC turnInNPC = null)
	{
		this.refreshQuestCompletion(currentCompletionType, rewardChoice, playObjectiveComplete, turnInNPC);
		PrefabInstance.RefreshSwitchesInContainingPoi(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshQuestCompletion(QuestClass.CompletionTypes currentCompletionType, List<BaseReward> rewardChoice, bool playObjectiveComplete, EntityNPC turnInNPC)
	{
		if (this.CurrentState != Quest.QuestState.InProgress && this.CurrentState != Quest.QuestState.ReadyForTurnIn)
		{
			return;
		}
		if (this.OwnerJournal == null)
		{
			return;
		}
		if (this.CurrentState == Quest.QuestState.InProgress)
		{
			this.OwnerJournal.RefreshQuest(this);
			this.OptionalComplete = true;
			bool flag = false;
			bool flag2 = false;
			for (int i = 0; i < this.Objectives.Count; i++)
			{
				if (this.Objectives[i].Phase == this.CurrentPhase || this.Objectives[i].Phase == 0)
				{
					if (this.Objectives[i].Optional)
					{
						if (!this.Objectives[i].Complete)
						{
							this.OptionalComplete = false;
						}
					}
					else if (!this.Objectives[i].Complete && !this.Objectives[i].AlwaysComplete)
					{
						flag = true;
					}
					else if (this.Objectives[i].ForcePhaseFinish)
					{
						flag2 = true;
					}
				}
			}
			if (flag)
			{
				if (flag2)
				{
					this.CloseQuest(Quest.QuestState.Failed, null);
					return;
				}
				return;
			}
			else if (this.CurrentPhase < this.QuestClass.HighestPhase)
			{
				this.AdvancePhase();
				if (playObjectiveComplete)
				{
					Manager.PlayInsidePlayerHead("quest_objective_complete", -1, 0f, false, false);
				}
				if (this.CurrentPhase == this.QuestClass.HighestPhase && this.OwnerJournal.ActiveQuest == this)
				{
					this.OwnerJournal.ActiveQuest = null;
					this.OwnerJournal.RefreshRallyMarkerPositions();
				}
				return;
			}
		}
		if (currentCompletionType != this.QuestClass.CompletionType)
		{
			this.CurrentState = Quest.QuestState.ReadyForTurnIn;
			return;
		}
		QuestEventManager.Current.QuestCompleted(this.QuestTags, this.questClass);
		this.CloseQuest(Quest.QuestState.Completed, rewardChoice);
		if (this.QuestClass.ResetTraderQuests)
		{
			EntityTrader entityTrader = turnInNPC as EntityTrader;
			if (entityTrader != null)
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					EntityPlayerLocal ownerPlayer = this.OwnerJournal.OwnerPlayer;
					entityTrader.ClearActiveQuests(ownerPlayer.entityId);
					entityTrader.SetupActiveQuestsForPlayer(ownerPlayer, -1);
					return;
				}
				EntityPlayer ownerPlayer2 = this.OwnerJournal.OwnerPlayer;
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ResetTraderQuests, ownerPlayer2.entityId, this.QuestGiverID, entityTrader.GetQuestFactionPoints(ownerPlayer2)), false);
			}
		}
	}

	public void CloseQuest(Quest.QuestState finalState, List<BaseReward> rewardChoice = null)
	{
		if (finalState != Quest.QuestState.Completed && finalState != Quest.QuestState.Failed)
		{
			Log.Warning(string.Format("Ending a quest in a state {0}. Should be {1} or {2}", finalState, Quest.QuestState.Completed, Quest.QuestState.Failed));
		}
		if (this.OwnerJournal == null)
		{
			return;
		}
		this.OwnerJournal.RefreshQuest(this);
		this.CurrentState = finalState;
		bool flag = finalState == Quest.QuestState.Completed;
		this.HandleUnlockPOI(null);
		bool flag2 = !string.IsNullOrEmpty(this.PreviousQuest);
		bool flag3 = flag && flag2;
		if (flag3)
		{
			for (int i = 0; i < this.Rewards.Count; i++)
			{
				if (this.Rewards[i] is RewardQuest && (this.Rewards[i] as RewardQuest).IsChainQuest)
				{
					flag3 = false;
				}
			}
		}
		ToolTipEvent toolTipEvent = new ToolTipEvent();
		for (int j = 0; j < this.Rewards.Count; j++)
		{
			if (this.Rewards[j].ReceiveStage == BaseReward.ReceiveStages.AfterCompleteNotification)
			{
				toolTipEvent.EventHandler += this.QuestRewardsLater_Event;
				toolTipEvent.Parameter = this;
				break;
			}
		}
		EntityPlayerLocal ownerPlayer = this.OwnerJournal.OwnerPlayer;
		string text = ((this.questClass.Name == this.questClass.SubTitle) ? this.questClass.Name : string.Format("{0} - {1}", this.questClass.Name, this.questClass.SubTitle));
		string text2 = (flag ? Localization.Get("completed", false, null) : Localization.Get("failed", false, null));
		string text3 = (flag ? "quest_subtask_complete" : "quest_failed");
		ToolTipEvent toolTipEvent2 = ((flag && !flag3) ? toolTipEvent : null);
		GameManager.ShowTooltip(ownerPlayer, string.Format("{0} {1}: {2}", this.questClass.Category, text2, text), string.Empty, text3, toolTipEvent2, false, false, 0f);
		if (flag3)
		{
			GameManager.ShowTooltip(ownerPlayer, string.Format("{0} {1}: {2}", Localization.Get("questChain", false, null), Localization.Get("completed", false, null), this.questClass.GroupName), string.Empty, "quest_master_complete", toolTipEvent, false, false, 0f);
		}
		if (this.OwnerJournal.TrackedQuest == this)
		{
			this.OwnerJournal.TrackedQuest = null;
		}
		for (int k = 0; k < this.Objectives.Count; k++)
		{
			this.Objectives[k].HandleRemoveHooks();
		}
		for (int l = 0; l < this.Objectives.Count; l++)
		{
			this.Objectives[l].RemoveObjectives();
			if (flag)
			{
				this.Objectives[l].HandleCompleted();
			}
			else
			{
				this.Objectives[l].HandleFailed();
			}
		}
		this.RemoveMapObject();
		if (flag)
		{
			if (this.AddsProgression)
			{
				QuestEventManager.Current.HandleNewCompletedQuest(this.OwnerJournal.OwnerPlayer, this.QuestFaction, (int)this.QuestClass.DifficultyTier, this.QuestClass.AddsToTierComplete);
			}
			for (int m = 0; m < this.Rewards.Count; m++)
			{
				if (this.Rewards[m].ReceiveStage == BaseReward.ReceiveStages.QuestCompletion && (!this.Rewards[m].Optional || (this.Rewards[m].Optional && this.OptionalComplete)) && (!this.Rewards[m].isChosenReward || (this.Rewards[m].isChosenReward && rewardChoice != null && rewardChoice.Contains(this.Rewards[m]))))
				{
					this.Rewards[m].GiveReward();
				}
			}
			this.OwnerJournal.CompleteQuest(this);
			for (int n = 0; n < this.Actions.Count; n++)
			{
				BaseQuestAction baseQuestAction = this.Actions[n];
				if (baseQuestAction.OnComplete)
				{
					baseQuestAction.HandlePerformAction();
				}
			}
		}
		else
		{
			this.OptionalComplete = false;
			this.tracked = false;
			this.OwnerJournal.FailedQuest(this);
		}
		if (this.OwnerJournal.ActiveQuest == this)
		{
			this.OwnerJournal.ActiveQuest = null;
			this.OwnerJournal.RefreshRallyMarkerPositions();
		}
		if (this.QuestClass.ResetTraderQuests)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				QuestEventManager.Current.AddTraderResetQuestsForPlayer(this.OwnerJournal.OwnerPlayer.entityId, this.QuestGiverID);
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.ResetTraderQuests, this.OwnerJournal.OwnerPlayer.entityId, this.QuestGiverID, -1), false);
		}
	}

	public void HandleUnlockPOI(EntityPlayer player = null)
	{
		Vector3 zero = Vector3.zero;
		if (this.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition))
		{
			if (player == null)
			{
				player = this.OwnerJournal.OwnerPlayer;
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				QuestEventManager.Current.QuestUnlockPOI(player.entityId, zero);
				return;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.UnlockPOI, player.entityId, zero), false);
		}
	}

	public void HandleQuestEvent(Quest ownerQuest, string eventType)
	{
		for (int i = 0; i < this.questClass.Events.Count; i++)
		{
			if (this.questClass.Events[i].EventType == eventType)
			{
				this.questClass.Events[i].HandleEvent(ownerQuest);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void QuestRewardsLater_Event(object obj)
	{
		Quest quest = (Quest)obj;
		GameManager.Instance.StartCoroutine(this.GiveRewardsLater(quest));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public IEnumerator GiveRewardsLater(Quest q)
	{
		yield return new WaitForSeconds(3f);
		if (XUi.IsGameRunning() && q.CurrentState == Quest.QuestState.Completed)
		{
			for (int i = 0; i < this.Rewards.Count; i++)
			{
				if (this.Rewards[i].ReceiveStage == BaseReward.ReceiveStages.AfterCompleteNotification && (!this.Rewards[i].Optional || (this.Rewards[i].Optional && q.OptionalComplete)))
				{
					this.Rewards[i].GiveReward();
				}
			}
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseQuestAction AddAction(BaseQuestAction action)
	{
		if (action != null)
		{
			this.Actions.Add(action);
		}
		return action;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseRequirement AddRequirement(BaseRequirement requirement)
	{
		if (requirement != null)
		{
			this.Requirements.Add(requirement);
		}
		return requirement;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseObjective AddObjective(BaseObjective objective)
	{
		if (objective != null)
		{
			this.Objectives.Add(objective);
		}
		return objective;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public BaseReward AddReward(BaseReward reward)
	{
		if (reward != null)
		{
			this.Rewards.Add(reward);
		}
		return reward;
	}

	public string ParseVariable(string value)
	{
		if (value != null && value.Contains("{"))
		{
			int num = value.IndexOf("{") + 1;
			int num2 = value.IndexOf("}", num);
			if (num2 != -1)
			{
				string text = value.Substring(num, num2 - num);
				if (this.DataVariables.ContainsKey(text))
				{
					value = this.DataVariables[text];
				}
			}
		}
		return value;
	}

	public void Read(PooledBinaryReader _br)
	{
		bool flag = true;
		this.CurrentFileVersion = _br.ReadByte();
		this.CurrentState = (Quest.QuestState)_br.ReadByte();
		this.SharedOwnerID = _br.ReadInt32();
		this.QuestGiverID = _br.ReadInt32();
		if (this.CurrentState == Quest.QuestState.InProgress)
		{
			this.tracked = _br.ReadBoolean();
			this.CurrentPhase = _br.ReadByte();
			this.QuestCode = _br.ReadInt32();
		}
		else if (this.CurrentState == Quest.QuestState.Completed)
		{
			this.CurrentPhase = this.QuestClass.HighestPhase;
		}
		PooledBinaryReader.StreamReadSizeMarker streamReadSizeMarker = default(PooledBinaryReader.StreamReadSizeMarker);
		if (this.CurrentFileVersion >= 7)
		{
			streamReadSizeMarker = _br.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
		}
		try
		{
			for (int i = 0; i < this.Objectives.Count; i++)
			{
				this.Objectives[i].Read(_br);
			}
		}
		catch (Exception ex)
		{
			Log.Error(ex.ToString());
		}
		finally
		{
			uint num;
			if (this.CurrentFileVersion >= 7 && !_br.ValidateSizeMarker(ref streamReadSizeMarker, out num, true))
			{
				this.Objectives.Clear();
				Log.Error("Loading player quests: Quest with ID " + this.ID + ": Failed loading objectives");
				flag = false;
			}
		}
		int num2 = (int)_br.ReadByte();
		for (int j = 0; j < num2; j++)
		{
			string text = _br.ReadString();
			string text2 = _br.ReadString();
			if (!this.DataVariables.ContainsKey(text))
			{
				this.DataVariables.Add(text, text2);
			}
			else
			{
				this.DataVariables[text] = text2;
			}
		}
		if (this.CurrentState == Quest.QuestState.InProgress)
		{
			this.PositionData.Clear();
			int num3 = (int)_br.ReadByte();
			for (int k = 0; k < num3; k++)
			{
				Quest.PositionDataTypes positionDataTypes = (Quest.PositionDataTypes)_br.ReadByte();
				Vector3 vector = StreamUtils.ReadVector3(_br);
				this.SetPositionData(positionDataTypes, vector);
			}
			this.RallyMarkerActivated = _br.ReadBoolean();
		}
		else
		{
			this.FinishTime = _br.ReadUInt64();
		}
		if (this.CurrentState == Quest.QuestState.InProgress || this.CurrentState == Quest.QuestState.ReadyForTurnIn)
		{
			PooledBinaryReader.StreamReadSizeMarker streamReadSizeMarker2 = default(PooledBinaryReader.StreamReadSizeMarker);
			if (this.CurrentFileVersion >= 7)
			{
				streamReadSizeMarker2 = _br.ReadSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
			}
			try
			{
				if (this.CurrentFileVersion <= 5)
				{
					for (int l = 0; l < this.Rewards.Count; l++)
					{
						this.Rewards[l].Read(_br);
					}
				}
				else
				{
					int num4 = _br.ReadInt32();
					for (int m = 0; m < num4; m++)
					{
						this.Rewards[m].Read(_br);
					}
				}
			}
			catch (Exception ex2)
			{
				Log.Error(ex2.ToString());
			}
			finally
			{
				uint num;
				if (this.CurrentFileVersion >= 7 && !_br.ValidateSizeMarker(ref streamReadSizeMarker2, out num, true))
				{
					this.Rewards.Clear();
					Log.Error("Loading player quests: Quest with ID " + this.ID + ": Failed loading rewards");
					flag = false;
				}
			}
		}
		if (this.CurrentFileVersion > 4)
		{
			this.QuestFaction = _br.ReadByte();
		}
		if (!flag && this.CurrentState != Quest.QuestState.Completed)
		{
			this.CurrentState = Quest.QuestState.Failed;
		}
		if (this.CurrentFileVersion >= 8)
		{
			this.QuestProgressDay = _br.ReadInt32();
		}
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write(this.ID);
		_bw.Write(this.CurrentQuestVersion);
		_bw.Write(Quest.FileVersion);
		_bw.Write((byte)this.CurrentState);
		_bw.Write(this.SharedOwnerID);
		_bw.Write(this.QuestGiverID);
		if (this.CurrentState == Quest.QuestState.InProgress)
		{
			_bw.Write(this.Tracked);
			_bw.Write(this.CurrentPhase);
			_bw.Write(this.QuestCode);
		}
		PooledBinaryWriter.StreamWriteSizeMarker streamWriteSizeMarker = _bw.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			this.Objectives[i].Write(_bw);
		}
		_bw.FinalizeSizeMarker(ref streamWriteSizeMarker);
		_bw.Write((byte)this.DataVariables.Count);
		foreach (KeyValuePair<string, string> keyValuePair in this.DataVariables)
		{
			_bw.Write(keyValuePair.Key);
			_bw.Write(keyValuePair.Value);
		}
		if (this.CurrentState == Quest.QuestState.InProgress)
		{
			_bw.Write((byte)this.PositionData.Count);
			foreach (KeyValuePair<Quest.PositionDataTypes, Vector3> keyValuePair2 in this.PositionData)
			{
				_bw.Write((byte)keyValuePair2.Key);
				StreamUtils.Write(_bw, keyValuePair2.Value);
			}
			_bw.Write(this.RallyMarkerActivated);
		}
		else
		{
			_bw.Write(this.FinishTime);
		}
		if (this.CurrentState == Quest.QuestState.InProgress || this.CurrentState == Quest.QuestState.ReadyForTurnIn)
		{
			PooledBinaryWriter.StreamWriteSizeMarker streamWriteSizeMarker2 = _bw.ReserveSizeMarker(PooledBinaryWriter.EMarkerSize.UInt16);
			_bw.Write(this.Rewards.Count);
			for (int j = 0; j < this.Rewards.Count; j++)
			{
				this.Rewards[j].Write(_bw);
			}
			_bw.FinalizeSizeMarker(ref streamWriteSizeMarker2);
		}
		_bw.Write(this.QuestFaction);
		_bw.Write(this.QuestProgressDay);
	}

	public void AddSharedLocation(Vector3 pos, Vector3 size)
	{
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			if (this.Objectives[i].Phase == this.CurrentPhase && this.Objectives[i].SetLocation(pos, size))
			{
				return;
			}
		}
	}

	public void AddSharedKill(string enemyType)
	{
		for (int i = 0; i < this.Objectives.Count; i++)
		{
			if (this.Objectives[i].Phase == this.CurrentPhase && this.Objectives[i].ID == enemyType)
			{
				BaseObjective baseObjective = this.Objectives[i];
				byte currentValue = baseObjective.CurrentValue;
				baseObjective.CurrentValue = currentValue + 1;
				this.Objectives[i].Refresh();
			}
		}
	}

	public int GetSharedWithCount()
	{
		if (this.sharedWithList == null)
		{
			return 0;
		}
		return this.sharedWithList.Count;
	}

	public int GetSharedWithCountNotInRange()
	{
		if (this.sharedWithList == null)
		{
			return 0;
		}
		EntityPlayer ownerPlayer = this.OwnerJournal.OwnerPlayer;
		int num = 0;
		Rect locationRect = this.GetLocationRect();
		for (int i = 0; i < this.sharedWithList.Count; i++)
		{
			EntityPlayer entityPlayer = this.sharedWithList[i];
			if (locationRect != Rect.zero)
			{
				Vector3 vector = entityPlayer.position;
				vector.y = vector.z;
				if (!locationRect.Contains(vector))
				{
					num++;
				}
			}
			else if (Vector3.Distance(ownerPlayer.position, entityPlayer.position) >= 15f)
			{
				num++;
			}
		}
		return num;
	}

	public List<EntityPlayer> GetSharedWithListNotInRange()
	{
		if (this.sharedWithList == null)
		{
			return null;
		}
		EntityPlayer ownerPlayer = this.OwnerJournal.OwnerPlayer;
		Rect locationRect = this.GetLocationRect();
		List<EntityPlayer> list = new List<EntityPlayer>();
		for (int i = 0; i < this.sharedWithList.Count; i++)
		{
			EntityPlayer entityPlayer = this.sharedWithList[i];
			if (locationRect != Rect.zero)
			{
				Vector3 vector = entityPlayer.position;
				vector.y = vector.z;
				if (!locationRect.Contains(vector))
				{
					list.Add(entityPlayer);
				}
			}
			else if (Vector3.Distance(ownerPlayer.position, entityPlayer.position) >= 15f)
			{
				list.Add(entityPlayer);
			}
		}
		return list;
	}

	public void RemoveSharedNotInRange()
	{
		if (this.sharedWithList == null)
		{
			return;
		}
		EntityPlayer ownerPlayer = this.OwnerJournal.OwnerPlayer;
		Rect locationRect = this.GetLocationRect();
		for (int i = this.sharedWithList.Count - 1; i >= 0; i--)
		{
			EntityPlayer entityPlayer = this.sharedWithList[i];
			if (locationRect != Rect.zero)
			{
				Vector3 vector = entityPlayer.position;
				vector.y = vector.z;
				if (!locationRect.Contains(vector))
				{
					this.sharedWithList.RemoveAt(i);
				}
			}
			else if (Vector3.Distance(ownerPlayer.position, entityPlayer.position) >= 15f)
			{
				this.sharedWithList.RemoveAt(i);
			}
		}
	}

	public int[] GetSharedWithIDList()
	{
		if (this.sharedWithList == null)
		{
			return null;
		}
		int[] array = new int[this.sharedWithList.Count];
		for (int i = 0; i < this.sharedWithList.Count; i++)
		{
			array[i] = this.sharedWithList[i].entityId;
		}
		return array;
	}

	public bool HasSharedWith(EntityPlayer player)
	{
		if (this.sharedWithList == null)
		{
			return false;
		}
		for (int i = 0; i < this.sharedWithList.Count; i++)
		{
			if (this.sharedWithList[i] == player)
			{
				return true;
			}
		}
		return false;
	}

	public void AddSharedWith(EntityPlayer player)
	{
		if (this.sharedWithList == null)
		{
			this.sharedWithList = new List<EntityPlayer>();
		}
		if (!this.sharedWithList.Contains(player))
		{
			this.sharedWithList.Add(player);
		}
	}

	public bool RemoveSharedWith(EntityPlayer player)
	{
		bool flag = false;
		if (this.sharedWithList == null)
		{
			return false;
		}
		for (int i = this.sharedWithList.Count - 1; i >= 0; i--)
		{
			if (this.sharedWithList[i].entityId == player.entityId)
			{
				flag = true;
				this.sharedWithList.RemoveAt(i);
			}
		}
		if (this.sharedWithList.Count == 0)
		{
			this.sharedWithList = null;
		}
		return flag;
	}

	public void SetPositionData(Quest.PositionDataTypes dataType, Vector3 value)
	{
		if (!this.PositionData.ContainsKey(dataType))
		{
			this.PositionData.Add(dataType, value);
		}
		else
		{
			this.PositionData[dataType] = value;
		}
		if (this.OwnerJournal != null)
		{
			PersistentPlayerData playerDataFromEntityID = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(this.OwnerJournal.OwnerPlayer.entityId);
			if (playerDataFromEntityID != null)
			{
				playerDataFromEntityID.AddQuestPosition(this.QuestCode, dataType, value);
			}
		}
	}

	public void RemovePositionData(Quest.PositionDataTypes dataType)
	{
		if (this.PositionData.ContainsKey(dataType))
		{
			this.PositionData.Remove(dataType);
		}
	}

	public bool GetPositionData(out Vector3 pos, Quest.PositionDataTypes dataType)
	{
		if (this.PositionData.ContainsKey(dataType))
		{
			pos = this.PositionData[dataType];
			return true;
		}
		pos = Vector3.zero;
		return false;
	}

	public string GetParsedText(string text)
	{
		if (text.Contains("{"))
		{
			text = this.ParseBindingVariables(text);
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string ParseBindingVariables(string response)
	{
		if (string.IsNullOrEmpty(response))
		{
			return response;
		}
		int num = response.IndexOf('{');
		while (num != -1)
		{
			int num2 = response.IndexOf('}', num);
			if (num2 != -1)
			{
				string text = response.Substring(num, num2 - num + 1);
				string[] array = text.Substring(1, text.Length - 2).Split(new char[] { '_', '.' });
				if (array.Length == 2)
				{
					response = response.Replace(text, this.GetVariableText(array[0], -1, array[1]));
				}
				if (array.Length == 3)
				{
					response = response.Replace(text, this.GetVariableText(array[0], Convert.ToInt32(array[1]), array[2]));
				}
			}
			if (num + 1 < response.Length)
			{
				num = response.IndexOf('{', num + 1);
			}
			else
			{
				num = -1;
			}
		}
		return response;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string GetVariableText(string field, int index, string variableName)
	{
		int num = 0;
		if (!(field == "fetch"))
		{
			if (!(field == "buff"))
			{
				if (!(field == "kill"))
				{
					if (!(field == "goto"))
					{
						if (!(field == "poi"))
						{
							if (field == "treasure")
							{
								for (int i = 0; i < this.Objectives.Count; i++)
								{
									if (this.Objectives[i] is ObjectiveTreasureChest && (++num == index || index == -1))
									{
										return this.Objectives[i].ParseBinding(variableName);
									}
								}
							}
						}
						else
						{
							for (int j = 0; j < this.Objectives.Count; j++)
							{
								if (this.Objectives[j] is ObjectiveRandomPOIGoto && (++num == index || index == -1))
								{
									return this.Objectives[j].ParseBinding(variableName);
								}
							}
						}
					}
					else
					{
						for (int k = 0; k < this.Objectives.Count; k++)
						{
							if (this.Objectives[k] is ObjectiveGoto && (++num == index || index == -1))
							{
								return this.Objectives[k].ParseBinding(variableName);
							}
						}
					}
				}
				else
				{
					for (int l = 0; l < this.Objectives.Count; l++)
					{
						if (this.Objectives[l] is ObjectiveEntityKill && (++num == index || index == -1))
						{
							return this.Objectives[l].ParseBinding(variableName);
						}
					}
				}
			}
			else
			{
				for (int m = 0; m < this.Objectives.Count; m++)
				{
					if (this.Objectives[m] is ObjectiveBuff && (++num == index || index == -1))
					{
						return this.Objectives[m].ParseBinding(variableName);
					}
				}
			}
		}
		else
		{
			for (int n = 0; n < this.Objectives.Count; n++)
			{
				if ((this.Objectives[n] is ObjectiveFetch || this.Objectives[n] is ObjectiveFetchKeep) && (++num == index || index == -1))
				{
					return this.Objectives[n].ParseBinding(variableName);
				}
			}
		}
		return field;
	}

	public string GetPOIName()
	{
		if (this.DataVariables.ContainsKey("POIName"))
		{
			return this.DataVariables["POIName"];
		}
		return "";
	}

	public bool CanTurnInQuest(List<BaseReward> rewardChoice)
	{
		EntityPlayerLocal ownerPlayer = this.OwnerJournal.OwnerPlayer;
		ItemStack[] array = ItemStack.Clone(this.OwnerJournal.OwnerPlayer.bag.GetSlots());
		ItemStack[] array2 = ItemStack.Clone(this.OwnerJournal.OwnerPlayer.inventory.GetSlots());
		ItemStack[] array3 = new ItemStack[array.Length + array2.Length];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			array3[num++] = array[i];
		}
		for (int j = 0; j < array2.Length; j++)
		{
			array3[num++] = array2[j];
		}
		for (int k = 0; k < this.Rewards.Count; k++)
		{
			if (this.Rewards[k].ReceiveStage == BaseReward.ReceiveStages.QuestCompletion && (!this.Rewards[k].Optional || (this.Rewards[k].Optional && this.OptionalComplete)) && (!this.Rewards[k].isChosenReward || (this.Rewards[k].isChosenReward && rewardChoice != null && rewardChoice.Contains(this.Rewards[k]))))
			{
				ItemStack rewardItem = this.Rewards[k].GetRewardItem();
				if (!rewardItem.IsEmpty())
				{
					XUiM_PlayerInventory.TryStackItem(0, rewardItem, array3);
					if (rewardItem.count > 0 && ItemStack.AddToItemStackArray(array3, rewardItem, -1) == -1)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public QuestJournal OwnerJournal;

	public static byte FileVersion = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest.QuestState _currentState;

	[PublicizedFrom(EAccessModifier.Private)]
	public int sharedOwnerID = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayer> sharedWithList;

	public int QuestCode;

	public int QuestGiverID = -1;

	public static int MaxQuestTier = 5;

	public static int QuestsPerTier = 10;

	public static string StarterQuest = "quest_whiteRiverCitizen1";

	public byte QuestFaction;

	public bool RallyMarkerActivated;

	public FastTags<TagGroup.Global> QuestTags = FastTags<TagGroup.Global>.none;

	public bool NeedsNPCSetPosition;

	public int QuestProgressDay = int.MinValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool tracked;

	[PublicizedFrom(EAccessModifier.Private)]
	public QuestClass questClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public MapObject mapObject;

	public PrefabInstance QuestPrefab;

	public NavObject NavObject;

	public Dictionary<Quest.PositionDataTypes, Vector3> PositionData = new EnumDictionary<Quest.PositionDataTypes, Vector3>();

	public Dictionary<string, string> DataVariables = new Dictionary<string, string>();

	public List<BaseQuestAction> Actions = new List<BaseQuestAction>();

	public List<BaseRequirement> Requirements = new List<BaseRequirement>();

	public List<BaseObjective> Objectives = new List<BaseObjective>();

	public List<BaseReward> Rewards = new List<BaseReward>();

	public TrackingHandler TrackingHelper = new TrackingHandler();

	public enum QuestState
	{
		NotStarted,
		InProgress,
		ReadyForTurnIn,
		Completed,
		Failed
	}

	public enum PositionDataTypes
	{
		QuestGiver,
		Location,
		POIPosition,
		POISize,
		TreasurePoint,
		FetchContainer,
		HiddenCache,
		Activate,
		TreasureOffset,
		TraderPosition
	}
}
