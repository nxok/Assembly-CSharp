using System;
using System.Collections;
using System.Collections.Generic;
using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveRallyPoint : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Distance;
		}
	}

	public override bool UpdateUI
	{
		get
		{
			return base.ObjectiveState != BaseObjective.ObjectiveStates.Failed;
		}
	}

	public override bool useUpdateLoop
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public override string StatusText
	{
		get
		{
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				return ValueDisplayFormatters.Distance(this.currentDistance);
			}
			if (base.OwnerQuest.CurrentState == Quest.QuestState.NotStarted)
			{
				return "";
			}
			if (base.ObjectiveState == BaseObjective.ObjectiveStates.Failed)
			{
				return Localization.Get("failed", false, null);
			}
			return Localization.Get("completed", false, null);
		}
	}

	public override void SetupObjective()
	{
		ObjectiveRallyPoint.textActivateRallyPoint = Localization.Get("ObjectiveRallyPointActivate", false, null);
		ObjectiveRallyPoint.textHeadToRallyPoint = Localization.Get("ObjectiveRallyPointHeadTo", false, null);
		ObjectiveRallyPoint.textWaitForActivate = Localization.Get("ObjectiveWaitForActivate_keyword", false, null);
		this.keyword = Localization.Get("ObjectiveBlockActivate_keyword", false, null);
		this.localizedName = ((this.ID != "" && this.ID != null) ? Localization.Get(this.ID, false, null) : "Any Block");
		if (base.OwnerQuest.SharedOwnerID != -1)
		{
			base.Description = ObjectiveRallyPoint.textWaitForActivate;
			return;
		}
		base.Description = ObjectiveRallyPoint.textActivateRallyPoint;
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.BlockActivate += this.Current_BlockActivate;
	}

	public static void SetupFlags(List<BaseObjective> objectives)
	{
		HashSet<ObjectiveRallyPointData> hashSet = new HashSet<ObjectiveRallyPointData>();
		foreach (BaseObjective baseObjective in objectives)
		{
			ObjectiveRallyPoint objectiveRallyPoint = baseObjective as ObjectiveRallyPoint;
			if (objectiveRallyPoint != null && objectiveRallyPoint.isRallyVisible)
			{
				Transform blockTransform = ObjectiveRallyPoint.getBlockTransform(objectiveRallyPoint.rallyPos);
				if (blockTransform != null)
				{
					ObjectiveRallyPointData component = blockTransform.gameObject.GetComponent<ObjectiveRallyPointData>();
					if (component != null)
					{
						Quest ownerQuest = objectiveRallyPoint.OwnerQuest;
						if (ownerQuest.CurrentPhase == objectiveRallyPoint.Phase)
						{
							if (!hashSet.Contains(component))
							{
								hashSet.Add(component);
								component.ClearAllFlags();
							}
							component.AddFlag(objectiveRallyPoint.rallyMarkerType, ownerQuest.SharedOwnerID == -1);
						}
					}
				}
			}
		}
		foreach (ObjectiveRallyPointData objectiveRallyPointData in hashSet)
		{
			objectiveRallyPointData.UpdateAllFlags();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform getBlockTransform(Vector3i rallyPos)
	{
		Transform transform = null;
		Chunk chunk = GameManager.Instance.World.GetChunkFromWorldPos(rallyPos) as Chunk;
		if (chunk != null)
		{
			BlockEntityData blockEntity = chunk.GetBlockEntity(rallyPos);
			if (blockEntity != null)
			{
				transform = blockEntity.transform;
			}
		}
		return transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setVisibility(Transform t, string childName, bool isVisible)
	{
		this.isRallyVisible = isVisible;
	}

	public override void RemoveObjectives()
	{
		base.RemoveObjectives();
		World world = GameManager.Instance.World;
		if (this.rallyPos != Vector3i.zero)
		{
			if (this.RallyStartType == ObjectiveRallyPoint.RallyStartTypes.Find)
			{
				Chunk chunk = world.GetChunkFromWorldPos(this.rallyPos) as Chunk;
				if (chunk != null)
				{
					BlockEntityData blockEntity = chunk.GetBlockEntity(this.rallyPos);
					if (blockEntity != null && blockEntity.transform != null)
					{
						this.setVisibility(blockEntity.transform, this.rallyMarkerType, false);
						return;
					}
				}
			}
			else
			{
				if (base.OwnerQuest.SharedOwnerID == -1)
				{
					GameManager.Instance.World.SetBlockRPC(this.rallyPos, BlockValue.Air, sbyte.MaxValue);
					return;
				}
				GameManager.Instance.StartCoroutine(this.setRallyPointVisibility(false));
			}
		}
	}

	public override void RemoveHooks()
	{
		World world = GameManager.Instance.World;
		QuestEventManager.Current.BlockActivate -= this.Current_BlockActivate;
		if (this.rallyPos != Vector3i.zero)
		{
			if (this.RallyStartType == ObjectiveRallyPoint.RallyStartTypes.Find)
			{
				bool flag = base.OwnerQuest.OwnerJournal.HasQuestAtRallyPosition(this.rallyPos.ToVector3(), true) != null && base.OwnerQuest.OwnerJournal.ActiveQuest == null;
				GameManager.Instance.StartCoroutine(this.setRallyPointVisibility(flag));
				return;
			}
			if (base.OwnerQuest.SharedOwnerID == -1)
			{
				world.SetBlockRPC(this.rallyPos, BlockValue.Air, sbyte.MaxValue);
				return;
			}
			GameManager.Instance.StartCoroutine(this.setRallyPointVisibility(false));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator setRallyPointVisibility(bool visible)
	{
		QuestEventManager.Current.BlockActivate -= this.Current_BlockActivate;
		if (visible)
		{
			QuestEventManager.Current.BlockActivate += this.Current_BlockActivate;
		}
		BlockValue bvRallyType = Block.GetBlockValue("questRallyMarker", false);
		World world = GameManager.Instance.World;
		Chunk chunk = world.GetChunkFromWorldPos(this.rallyPos) as Chunk;
		if (chunk != null)
		{
			BlockEntityData bd = chunk.GetBlockEntity(this.rallyPos);
			if (bd != null)
			{
				while (bd.transform == null)
				{
					yield return null;
				}
				if (!visible)
				{
					ObjectiveRallyPointData component = bd.transform.gameObject.GetComponent<ObjectiveRallyPointData>();
					if (component != null)
					{
						component.RemoveFlag(this.rallyMarkerType);
						component.UpdateAllFlags();
					}
				}
				if (bd.blockValue.EqualsExceptRotation(bvRallyType))
				{
					this.setVisibility(bd.transform, this.rallyMarkerType, visible);
				}
				else
				{
					world.SetBlockRPC(this.rallyPos, bvRallyType);
				}
			}
			else
			{
				world.SetBlockRPC(this.rallyPos, bvRallyType);
			}
			bd = null;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockActivate(string blockName, Vector3i blockPos)
	{
		if (base.OwnerQuest.SharedOwnerID != -1)
		{
			return;
		}
		if (base.Complete)
		{
			return;
		}
		if (base.OwnerQuest.OwnerJournal.ActiveQuest != null)
		{
			return;
		}
		if (this.rallyPos != blockPos)
		{
			return;
		}
		if (TwitchManager.HasInstance && TwitchManager.Current.IsVoting)
		{
			GameManager.ShowTooltip(base.OwnerQuest.OwnerJournal.OwnerPlayer, Localization.Get("ttWaitForVoteQuest", false, null), false, false, 0f);
			return;
		}
		Vector3 zero = Vector3.zero;
		int num = GameUtils.WorldTimeToHours(GameManager.Instance.World.worldTime);
		if (this.startTime != -1 && this.endTime != -1)
		{
			if (this.startTime < this.endTime)
			{
				if (num < this.startTime || num >= this.endTime)
				{
					GameManager.ShowTooltip(base.OwnerQuest.OwnerJournal.OwnerPlayer, string.Format(Localization.Get("ObjectiveRallyPointInvalidStartTime", false, null), this.startTime, this.endTime), false, false, 0f);
					return;
				}
			}
			else if (num < this.startTime && num >= this.endTime)
			{
				GameManager.ShowTooltip(base.OwnerQuest.OwnerJournal.OwnerPlayer, string.Format(Localization.Get("ObjectiveRallyPointInvalidStartTime", false, null), this.startTime, this.endTime), false, false, 0f);
				return;
			}
		}
		base.OwnerQuest.RemoveSharedNotInRange();
		if (!base.OwnerQuest.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition))
		{
			if (base.OwnerQuest.GetPositionData(out zero, Quest.PositionDataTypes.Location))
			{
				this.RallyPointActivate(zero, true, QuestEventManager.POILockoutReasonTypes.None, 0UL);
			}
			return;
		}
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			Vector2 vector;
			vector..ctor(zero.x, zero.z);
			ulong num2;
			QuestEventManager.POILockoutReasonTypes poilockoutReasonTypes = QuestEventManager.Current.CheckForPOILockouts(ownerPlayer.entityId, vector, out num2);
			this.RallyPointActivate(zero, poilockoutReasonTypes == QuestEventManager.POILockoutReasonTypes.None, poilockoutReasonTypes, num2);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.TryRallyMarker, ownerPlayer.entityId, zero, base.OwnerQuest.QuestCode), false);
	}

	public void RallyPointActivate(Vector3 prefabPos, bool activate, QuestEventManager.POILockoutReasonTypes lockoutReason, ulong extraData)
	{
		ObjectiveRallyPoint.<>c__DisplayClass39_0 CS$<>8__locals1 = new ObjectiveRallyPoint.<>c__DisplayClass39_0();
		CS$<>8__locals1.<>4__this = this;
		CS$<>8__locals1.prefabPos = prefabPos;
		bool flag = base.OwnerQuest.PositionData.ContainsKey(Quest.PositionDataTypes.POIPosition);
		if (activate)
		{
			if (base.OwnerQuest.CheckRequirements())
			{
				if (!base.OwnerQuest.QuestClass.CanActivate())
				{
					GameManager.ShowTooltip(base.OwnerQuest.OwnerJournal.OwnerPlayer, Localization.Get("questunavailable", false, null), false, false, 0f);
					return;
				}
				this.HandleParty();
				base.OwnerQuest.RemoveMapObject();
				base.OwnerQuest.RallyMarkerActivated = true;
				CS$<>8__locals1.player = base.OwnerQuest.OwnerJournal.OwnerPlayer;
				if (flag)
				{
					base.OwnerQuest.OwnerJournal.ActiveQuest = base.OwnerQuest;
					base.OwnerQuest.Tracked = true;
					base.OwnerQuest.OwnerJournal.TrackedQuest = base.OwnerQuest;
					base.OwnerQuest.OwnerJournal.RefreshTracked();
					if (base.OwnerQuest.PositionData.ContainsKey(Quest.PositionDataTypes.TraderPosition))
					{
						base.OwnerQuest.OwnerJournal.AddPOIToTraderData((int)base.OwnerQuest.QuestClass.DifficultyTier, base.OwnerQuest.PositionData[Quest.PositionDataTypes.TraderPosition], base.OwnerQuest.PositionData[Quest.PositionDataTypes.POIPosition]);
					}
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
					{
						ThreadManager.StartCoroutine(CS$<>8__locals1.<RallyPointActivate>g__RallyPointActivatedCoroutine|0());
					}
					else
					{
						ObjectiveRallyPoint.OutstandingRallyPoint = this;
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.LockPOI, CS$<>8__locals1.player.entityId, base.OwnerQuest.ID, base.OwnerQuest.QuestTags, CS$<>8__locals1.prefabPos, base.OwnerQuest.GetSharedWithIDList()), false);
					}
				}
				else
				{
					this.rallyPointActivated();
				}
				if (this.activateEvent != "")
				{
					GameEventManager.Current.HandleAction(this.activateEvent, null, CS$<>8__locals1.player, false, new Vector3i(CS$<>8__locals1.prefabPos), "", "", false, true, "", null, null);
					return;
				}
			}
		}
		else
		{
			EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
			switch (lockoutReason)
			{
			case QuestEventManager.POILockoutReasonTypes.PlayerInside:
				GameManager.ShowTooltip(ownerPlayer as EntityPlayerLocal, Localization.Get("poiLockoutPlayerInside", false, null), false, false, 0f);
				return;
			case QuestEventManager.POILockoutReasonTypes.Bedroll:
				GameManager.ShowTooltip(ownerPlayer as EntityPlayerLocal, Localization.Get("poiLockoutBedroll", false, null), false, false, 0f);
				return;
			case QuestEventManager.POILockoutReasonTypes.LandClaim:
				GameManager.ShowTooltip(ownerPlayer as EntityPlayerLocal, Localization.Get("poiLockoutLandClaim", false, null), false, false, 0f);
				return;
			case QuestEventManager.POILockoutReasonTypes.QuestLock:
			{
				if (extraData == 0UL)
				{
					GameManager.ShowTooltip(ownerPlayer as EntityPlayerLocal, Localization.Get("poiLockoutQuestActiveQuesters", false, null), false, false, 0f);
					return;
				}
				ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(extraData);
				int item = valueTuple.Item2;
				int item2 = valueTuple.Item3;
				GameManager.ShowTooltip(ownerPlayer as EntityPlayerLocal, Localization.Get("ttQuestLockedUntil", false, null), string.Format("{0:00}:{1:00}", item, item2 + 1), null, null, false, false, 0f);
				break;
			}
			default:
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void rallyPointActivated()
	{
		ObjectiveRallyPoint.OutstandingRallyPoint = null;
		base.CurrentValue = 1;
		this.Refresh();
	}

	public void RallyPointActivated()
	{
		this.rallyPointActivated();
	}

	public bool IsActivated()
	{
		return this.currentValue == 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleParty()
	{
		EntityPlayer ownerPlayer = base.OwnerQuest.OwnerJournal.OwnerPlayer;
		if (ownerPlayer.Party == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePartyQuestChange>().Setup(ownerPlayer.entityId, base.OwnerQuest.GetObjectiveIndex(this), true, base.OwnerQuest.QuestCode), true, -1, -1, -1, null, 192, false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePartyQuestChange>().Setup(ownerPlayer.entityId, base.OwnerQuest.GetObjectiveIndex(this), true, base.OwnerQuest.QuestCode), false);
	}

	public override void Refresh()
	{
		bool flag = base.CurrentValue == 1;
		base.Complete = flag;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public Vector3i RallyPos
	{
		get
		{
			return this.rallyPos;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 HandleRallyPoint()
	{
		BlockValue blockValue = Block.GetBlockValue("questRallyMarker", false);
		if (this.RallyStartType == ObjectiveRallyPoint.RallyStartTypes.Find)
		{
			if (!base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.POIPosition))
			{
				return Vector3.zero;
			}
			Vector3 vector;
			if (!base.OwnerQuest.GetPositionData(out vector, Quest.PositionDataTypes.POISize))
			{
				return Vector3.zero;
			}
			Vector3i vector3i = new Vector3i(this.position);
			int num = 32;
			this.outerRect = new Rect(this.position.x - (float)num, this.position.z - (float)num, vector.x + (float)(num * 2), vector.z + (float)(num * 2));
			Vector3 vector2;
			if (base.OwnerQuest.GetPositionData(out vector2, Quest.PositionDataTypes.Activate))
			{
				this.rallyPos = new Vector3i(vector2);
				this.position = vector2;
				this.positionSet = ObjectiveRallyPoint.PositionSetTypes.RallyMarkerPosition;
			}
			else
			{
				World world = GameManager.Instance.World;
				if ((this.rallyPos = this.GetRallyPosition(world, vector3i, new Vector3i(vector))) != Vector3i.zero)
				{
					BlockValue block = world.GetBlock(this.rallyPos);
					if (!(block.Block is BlockRallyMarker))
					{
						return Vector3.zero;
					}
					base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Activate, this.rallyPos.ToVector3());
					Chunk chunk = world.GetChunkFromWorldPos(this.rallyPos) as Chunk;
					if (chunk != null)
					{
						BlockEntityData blockEntity = chunk.GetBlockEntity(this.rallyPos);
						if (blockEntity == null || !(blockEntity.transform != null) || block.type != blockValue.type)
						{
							world.SetBlockRPC(this.rallyPos, blockValue);
							return Vector3.zero;
						}
						this.setVisibility(blockEntity.transform, this.rallyMarkerType, base.OwnerQuest.OwnerJournal.ActiveQuest == null);
						this.positionSet = ObjectiveRallyPoint.PositionSetTypes.RallyMarkerPosition;
					}
				}
				else
				{
					this.rallyPos = new Vector3i(this.position);
					this.positionSet = ObjectiveRallyPoint.PositionSetTypes.POIPosition;
				}
			}
		}
		else
		{
			base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.Location);
			Vector3 vector3;
			if (base.OwnerQuest.GetPositionData(out vector3, Quest.PositionDataTypes.Activate))
			{
				this.rallyPos = new Vector3i(vector3);
			}
			else
			{
				int num2 = (int)this.position.x;
				int num3 = (int)this.position.z;
				int height = (int)GameManager.Instance.World.GetHeight(num2, num3);
				this.rallyPos = new Vector3i(num2, height, num3);
			}
			World world2 = GameManager.Instance.World;
			Chunk chunk2 = world2.GetChunkFromWorldPos(this.rallyPos) as Chunk;
			if (chunk2 == null)
			{
				this.rallyPos = new Vector3i(this.position);
				this.positionSet = ObjectiveRallyPoint.PositionSetTypes.POIPosition;
			}
			else
			{
				BlockValue blockValue2 = chunk2.GetBlock(World.toBlock(this.rallyPos));
				if (blockValue2.ischild)
				{
					this.rallyPos = new Vector3i(this.rallyPos.x + blockValue2.parentx, this.rallyPos.y + blockValue2.parenty, this.rallyPos.z + blockValue2.parentz);
					blockValue2 = chunk2.GetBlock(World.toBlock(this.rallyPos));
					base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Activate, this.rallyPos.ToVector3());
				}
				if (!(blockValue2.Block is BlockRallyMarker))
				{
					if (!blockValue2.isair && !blockValue2.Block.IsTerrainDecoration)
					{
						this.rallyPos += Vector3i.up;
					}
					GameManager.Instance.World.SetBlockRPC(this.rallyPos, blockValue, sbyte.MaxValue);
					base.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Activate, this.rallyPos.ToVector3());
					return Vector3.zero;
				}
				BlockEntityData blockEntity2 = chunk2.GetBlockEntity(this.rallyPos);
				if (blockEntity2 == null || !(blockEntity2.transform != null) || blockValue2.type != blockValue.type)
				{
					world2.SetBlockRPC(this.rallyPos, blockValue);
					return Vector3.zero;
				}
				this.setVisibility(blockEntity2.transform, this.rallyMarkerType, true);
				this.positionSet = ObjectiveRallyPoint.PositionSetTypes.RallyMarkerPosition;
			}
		}
		this.position = this.rallyPos.ToVector3();
		base.OwnerQuest.HandleMapObject(Quest.PositionDataTypes.Activate, this.NavObjectName, -1);
		return this.rallyPos.ToVector3();
	}

	public Vector3i GetRallyPosition(World _world, Vector3i _prefabPosition, Vector3i _prefabSize)
	{
		int num = World.toChunkXZ(_prefabPosition.x - 1);
		int num2 = World.toChunkXZ(_prefabPosition.x + _prefabSize.x + 1);
		int num3 = World.toChunkXZ(_prefabPosition.z - 1);
		int num4 = World.toChunkXZ(_prefabPosition.z + _prefabSize.z + 1);
		new List<Vector3i>();
		Rect rect;
		rect..ctor((float)_prefabPosition.x, (float)_prefabPosition.z, (float)_prefabSize.x, (float)_prefabSize.z);
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				Chunk chunk = _world.GetChunkSync(i, j) as Chunk;
				if (chunk != null)
				{
					List<Vector3i> list = chunk.IndexedBlocks["Rally"];
					if (list != null)
					{
						for (int k = 0; k < list.Count; k++)
						{
							Vector3 vector = chunk.ToWorldPos(list[k]).ToVector3();
							if (rect.Contains(new Vector2(vector.x, vector.z)))
							{
								base.CurrentValue = 2;
								return chunk.ToWorldPos(list[k]);
							}
						}
					}
				}
			}
		}
		return Vector3i.zero;
	}

	public override BaseObjective Clone()
	{
		ObjectiveRallyPoint objectiveRallyPoint = new ObjectiveRallyPoint();
		this.CopyValues(objectiveRallyPoint);
		return objectiveRallyPoint;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveRallyPoint objectiveRallyPoint = (ObjectiveRallyPoint)objective;
		objectiveRallyPoint.RallyStartType = this.RallyStartType;
		objectiveRallyPoint.startTime = this.startTime;
		objectiveRallyPoint.endTime = this.endTime;
		objectiveRallyPoint.activateEvent = this.activateEvent;
		objectiveRallyPoint.rallyMarkerType = this.rallyMarkerType;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_NeedSetup()
	{
		this.HandleRallyPoint();
		if (this.positionSet != ObjectiveRallyPoint.PositionSetTypes.None)
		{
			base.CurrentValue = 2;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Update()
	{
		if (this.positionSet != ObjectiveRallyPoint.PositionSetTypes.RallyMarkerPosition)
		{
			this.HandleRallyPoint();
			if (this.positionSet == ObjectiveRallyPoint.PositionSetTypes.None)
			{
				return;
			}
		}
		if (this.positionSet == ObjectiveRallyPoint.PositionSetTypes.RallyMarkerPosition)
		{
			GameManager.Instance.StartCoroutine(this.setRallyPointVisibility(base.OwnerQuest.OwnerJournal.ActiveQuest == null));
		}
		Vector3 vector = base.OwnerQuest.OwnerJournal.OwnerPlayer.position;
		if (this.RallyStartType == ObjectiveRallyPoint.RallyStartTypes.Create)
		{
			this.currentDistance = Vector3.Distance(vector, this.position + new Vector3(0.5f, 0f, 0.5f));
			if (this.currentDistance > this.distanceNeeded)
			{
				if (base.Description == "" || this.lastDistance <= this.distanceNeeded)
				{
					base.Description = ((base.OwnerQuest.SharedOwnerID != -1) ? ObjectiveRallyPoint.textWaitForActivate : ObjectiveRallyPoint.textHeadToRallyPoint);
				}
			}
			else
			{
				if (this.lastDistance > this.distanceNeeded)
				{
					this.setRallyPointVisible = true;
				}
				if (base.Description == "" || this.lastDistance > this.distanceNeeded)
				{
					base.Description = ((base.OwnerQuest.SharedOwnerID != -1) ? ObjectiveRallyPoint.textWaitForActivate : ObjectiveRallyPoint.textActivateRallyPoint);
				}
			}
			this.lastDistance = this.currentDistance;
			return;
		}
		this.currentDistance = Vector3.Distance(vector, this.position + new Vector3(0.5f, 0f, 0.5f));
		vector.y = vector.z;
		if (this.outerRect.Contains(vector))
		{
			if (base.Description == "" || this.lastDistance > this.distanceNeeded)
			{
				base.Description = ((base.OwnerQuest.SharedOwnerID != -1) ? ObjectiveRallyPoint.textWaitForActivate : ObjectiveRallyPoint.textActivateRallyPoint);
			}
			if (this.lastDistance > this.distanceNeeded)
			{
				this.setRallyPointVisible = true;
			}
			this.lastDistance = -1f;
			return;
		}
		if (this.lastDistance == -1f)
		{
			this.setRallyPointVisible = true;
			this.lastDistance = 1f;
		}
		if (base.Description == "" || this.lastDistance <= this.distanceNeeded)
		{
			base.Description = ((base.OwnerQuest.SharedOwnerID != -1) ? ObjectiveRallyPoint.textWaitForActivate : ObjectiveRallyPoint.textHeadToRallyPoint);
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveRallyPoint.PropRallyStartMode))
		{
			this.RallyStartType = EnumUtils.Parse<ObjectiveRallyPoint.RallyStartTypes>(properties.Values[ObjectiveRallyPoint.PropRallyStartMode], false);
		}
		properties.ParseInt(ObjectiveRallyPoint.PropAllowedStartTime, ref this.startTime);
		properties.ParseInt(ObjectiveRallyPoint.PropAllowedEndTime, ref this.endTime);
		properties.ParseString(ObjectiveRallyPoint.PropActivateEvent, ref this.activateEvent);
		properties.ParseString(ObjectiveRallyPoint.PropRallyMarkerType, ref this.rallyMarkerType);
	}

	public void RallyPointRefresh()
	{
		GameManager.Instance.StartCoroutine(this.setRallyPointVisibility(true));
	}

	public ObjectiveRallyPoint.RallyStartTypes RallyStartType;

	public static string PropRallyStartMode = "start_mode";

	public static string PropAllowedStartTime = "allowed_start_hour";

	public static string PropAllowedEndTime = "allowed_end_hour";

	public static string PropActivateEvent = "activate_event";

	public static string PropRallyMarkerType = "rally_marker_type";

	public static ObjectiveRallyPoint OutstandingRallyPoint = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float currentDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect outerRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public int startTime = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public int endTime = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ObjectiveRallyPoint.PositionSetTypes positionSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string textActivateRallyPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string textHeadToRallyPoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string textWaitForActivate;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string rallyMarkerType = "questRallyMarker";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string activateEvent = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRallyVisible;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i rallyPos = Vector3i.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastDistance = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distanceNeeded = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool setRallyPointVisible;

	public enum RallyStartTypes
	{
		Find,
		Create
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum PositionSetTypes
	{
		None,
		POIPosition,
		RallyMarkerPosition
	}
}
