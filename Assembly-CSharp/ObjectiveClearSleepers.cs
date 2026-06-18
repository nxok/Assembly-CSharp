using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveClearSleepers : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Boolean;
		}
	}

	public override bool RequiresZombies
	{
		get
		{
			return true;
		}
	}

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag(QuestEventManager.clearTag);
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveClearAreas_keyword", false, null);
		this.SetupIcon();
	}

	public override bool UpdateUI
	{
		get
		{
			return base.ObjectiveState != BaseObjective.ObjectiveStates.Failed;
		}
	}

	public override void SetupDisplay()
	{
		base.Description = this.keyword;
		this.StatusText = "";
	}

	public override void AddHooks()
	{
		this.GetPosition();
		Vector3 zero = Vector3.zero;
		Vector3 zero2 = Vector3.zero;
		base.OwnerQuest.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition);
		base.OwnerQuest.GetPositionData(out zero2, Quest.PositionDataTypes.POISize);
		QuestEventManager.Current.SleepersCleared += this.Current_SleepersCleared;
		QuestEventManager.Current.SleeperVolumePositionAdd += this.Current_SleeperVolumePositionAdd;
		QuestEventManager.Current.SleeperVolumePositionRemove += this.Current_SleeperVolumePositionRemove;
		QuestEventManager.Current.SubscribeToUpdateEvent(base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, zero);
		this.SetupZombieCompassBounds(zero, zero2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_SleeperVolumePositionAdd(Vector3 position)
	{
		if (this.NavObjectName == "")
		{
			if (!this.SleeperMapObjectList.ContainsKey(position))
			{
				MapObjectSleeperVolume mapObjectSleeperVolume = new MapObjectSleeperVolume(position);
				GameManager.Instance.World.ObjectOnMapAdd(mapObjectSleeperVolume);
				this.SleeperMapObjectList.Add(position, mapObjectSleeperVolume);
				return;
			}
		}
		else if (!this.SleeperNavObjectList.ContainsKey(position))
		{
			NavObject navObject = NavObjectManager.Instance.RegisterNavObject(this.NavObjectName, position, "", false, -1, null);
			this.SleeperNavObjectList.Add(position, navObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_SleeperVolumePositionRemove(Vector3 position)
	{
		if (this.NavObjectName == "")
		{
			if (this.SleeperMapObjectList.ContainsKey(position))
			{
				MapObject mapObject = this.SleeperMapObjectList[position];
				GameManager.Instance.World.ObjectOnMapRemove(mapObject.type, (int)mapObject.key);
				this.SleeperMapObjectList.Remove(position);
				return;
			}
		}
		else if (this.SleeperNavObjectList.ContainsKey(position))
		{
			NavObject navObject = this.SleeperNavObjectList[position];
			NavObjectManager.Instance.UnRegisterNavObject(navObject);
			this.SleeperNavObjectList.Remove(position);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveSleeperVolumeMapObjects()
	{
		if (this.NavObjectName == "")
		{
			GameManager.Instance.World.ObjectOnMapRemove(EnumMapObjectType.SleeperVolume);
			this.SleeperMapObjectList.Clear();
			return;
		}
		NavObjectManager.Instance.UnRegisterNavObjectByClass(this.NavObjectName);
		this.SleeperNavObjectList.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupZombieCompassBounds(Vector3 poiPos, Vector3 poiSize)
	{
		base.OwnerQuest.OwnerJournal.OwnerPlayer.ZombieCompassBounds = new Rect(poiPos.x, poiPos.z, poiSize.x, poiSize.z);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_SleepersCleared(Vector3 prefabPos)
	{
		Vector3 zero = Vector3.zero;
		base.OwnerQuest.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition);
		if (zero.x != prefabPos.x || zero.z != prefabPos.z)
		{
			return;
		}
		if (base.OwnerQuest.CheckRequirements())
		{
			base.Complete = true;
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.SleepersCleared -= this.Current_SleepersCleared;
		Vector3 zero = Vector3.zero;
		base.OwnerQuest.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition);
		QuestEventManager.Current.UnSubscribeToUpdateEvent(base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, zero);
		if (base.OwnerQuest.OwnerJournal.ActiveQuest == base.OwnerQuest)
		{
			base.OwnerQuest.OwnerJournal.OwnerPlayer.ZombieCompassBounds = default(Rect);
		}
		this.RemoveSleeperVolumeMapObjects();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetupIcon()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetPosition()
	{
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.POIPosition))
		{
			base.OwnerQuest.Position = this.position;
		}
		return Vector3.zero;
	}

	public void FinalizePoint(float offset, float x, float y, float z)
	{
		this.distanceOffset = offset;
		this.position = new Vector3(x, y, z);
		base.OwnerQuest.DataVariables.Add(this.locationVariable, string.Format("{0},{1},{2},{3}", new object[]
		{
			offset.ToCultureInvariantString(),
			x.ToCultureInvariantString(),
			y.ToCultureInvariantString(),
			z.ToCultureInvariantString()
		}));
		base.OwnerQuest.Position = this.position;
		base.CurrentValue = 1;
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveClearSleepers objectiveClearSleepers = new ObjectiveClearSleepers();
		this.CopyValues(objectiveClearSleepers);
		return objectiveClearSleepers;
	}

	public override bool SetLocation(Vector3 pos, Vector3 size)
	{
		this.FinalizePoint(this.distanceOffset, pos.x, pos.y, pos.z);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public float distanceOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	public string icon = "ui_game_symbol_quest";

	[PublicizedFrom(EAccessModifier.Private)]
	public string locationVariable = "gotolocation";

	public Dictionary<Vector3, MapObjectSleeperVolume> SleeperMapObjectList = new Dictionary<Vector3, MapObjectSleeperVolume>();

	public Dictionary<Vector3, NavObject> SleeperNavObjectList = new Dictionary<Vector3, NavObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	public enum GotoStates
	{
		NoPosition,
		TryRefresh,
		TryComplete,
		Completed
	}
}
