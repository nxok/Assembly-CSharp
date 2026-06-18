using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectivePOIBlockActivate : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Number;
		}
	}

	public override bool UpdateUI
	{
		get
		{
			return base.ObjectiveState != BaseObjective.ObjectiveStates.Failed;
		}
	}

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag(QuestEventManager.restorePowerTag);
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveRestorePower_keyword", false, null);
		base.OwnerQuest.AddQuestTag(QuestEventManager.restorePowerTag);
	}

	public override void SetupDisplay()
	{
		base.Description = this.keyword;
		if (this.neededCount == -1)
		{
			this.StatusText = "";
			return;
		}
		this.StatusText = string.Format("{0}/{1}", base.CurrentValue, this.neededCount);
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		QuestEventManager.Current.BlockDestroy -= this.Current_BlockDestroy;
		QuestEventManager.Current.BlockDestroy += this.Current_BlockDestroy;
		QuestEventManager.Current.BlockActivate -= this.Current_BlockActivate;
		QuestEventManager.Current.BlockActivate += this.Current_BlockActivate;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockDestroy(Block block, Vector3i blockPos)
	{
		if (this.activateList == null)
		{
			return;
		}
		if (!this.activateList.Contains(blockPos))
		{
			return;
		}
		base.OwnerQuest.CloseQuest(Quest.QuestState.Failed, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockActivate(string blockname, Vector3i blockPos)
	{
		if (this.activateList == null)
		{
			return;
		}
		if (base.Complete)
		{
			return;
		}
		NavObjectManager.Instance.UnRegisterNavObjectByPosition(blockPos.ToVector3() + Vector3.one * 0.5f, this.NavObjectName);
		if (!this.activateList.Contains(blockPos))
		{
			return;
		}
		Block blockByName = Block.GetBlockByName(blockname, false);
		if ((this.ID == null || this.ID == "" || this.ID.EqualsCaseInsensitive(blockByName.IndexName)) && base.OwnerQuest.CheckRequirements())
		{
			byte currentValue = base.CurrentValue;
			base.CurrentValue = currentValue + 1;
			this.activateList.Remove(blockPos);
			this.Refresh();
		}
	}

	public void AddActivatedBlock(Vector3i blockPos)
	{
		NavObjectManager.Instance.UnRegisterNavObjectByPosition(blockPos.ToVector3() + Vector3.one * 0.5f, this.NavObjectName);
		byte currentValue = base.CurrentValue;
		base.CurrentValue = currentValue + 1;
		this.activateList.Remove(blockPos);
	}

	public override void RemoveHooks()
	{
		QuestEventManager questEventManager = QuestEventManager.Current;
		questEventManager.RemoveObjectiveToBeUpdated(this);
		questEventManager.BlockActivate -= this.Current_BlockActivate;
		questEventManager.BlockDestroy -= this.Current_BlockDestroy;
		this.ClearNavObjects();
		if (base.OwnerQuest != null && base.OwnerQuest.RallyMarkerActivated)
		{
			questEventManager.ActiveQuestBlocks.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearNavObjects()
	{
		if (this.activateList == null)
		{
			return;
		}
		for (int i = 0; i < this.activateList.Count; i++)
		{
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(this.activateList[i].ToVector3() + Vector3.one * 0.5f, this.NavObjectName);
		}
		this.activateList.Clear();
	}

	public override void Refresh()
	{
		if (this.neededCount == -1)
		{
			return;
		}
		if ((int)base.CurrentValue > this.neededCount)
		{
			base.CurrentValue = (byte)this.neededCount;
		}
		if (base.Complete)
		{
			return;
		}
		base.Complete = (int)base.CurrentValue >= this.neededCount;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
			base.HandleRemoveHooks();
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				QuestEventManager.Current.FinishManagedQuest(base.OwnerQuest.QuestCode, base.OwnerQuest.OwnerJournal.OwnerPlayer);
				return;
			}
			Vector3 zero = Vector3.zero;
			base.OwnerQuest.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition);
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.FinishManagedQuest, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, zero, base.OwnerQuest.QuestCode), false);
		}
	}

	public override void Update(float deltaTime)
	{
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 1f;
			if (this.neededCount == -1)
			{
				Vector3 zero = Vector3.zero;
				base.OwnerQuest.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition);
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					if (base.OwnerQuest.SharedOwnerID == -1)
					{
						this.activateList = new List<Vector3i>();
						QuestEventManager.Current.SetupActivateForMP(base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, base.OwnerQuest.QuestCode, this.completeEvent, this.activateList, GameManager.Instance.World, zero, this.ID, base.OwnerQuest.GetSharedWithIDList());
						this.SetupActivationList(zero, this.activateList);
						return;
					}
				}
				else
				{
					if (base.OwnerQuest.SharedOwnerID == -1)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageQuestEvent>().Setup(NetPackageQuestEvent.QuestEventTypes.SetupRestorePower, base.OwnerQuest.OwnerJournal.OwnerPlayer.entityId, base.OwnerQuest.QuestCode, this.completeEvent, zero, this.ID, base.OwnerQuest.GetSharedWithIDList()), false);
						return;
					}
					if (QuestEventManager.Current.ActiveQuestBlocks != null && QuestEventManager.Current.ActiveQuestBlocks.Count > 0)
					{
						this.SetupActivationList(zero, QuestEventManager.Current.ActiveQuestBlocks);
					}
				}
			}
		}
	}

	public override bool SetupActivationList(Vector3 prefabPos, List<Vector3i> newActivateList)
	{
		Vector3 zero = Vector3.zero;
		base.OwnerQuest.GetPositionData(out zero, Quest.PositionDataTypes.POIPosition);
		if (zero.x != prefabPos.x && zero.z != prefabPos.z)
		{
			return false;
		}
		base.CurrentValue = 0;
		this.neededCount = newActivateList.Count;
		byte b = 0;
		for (int i = 0; i < newActivateList.Count; i++)
		{
			NavObjectManager.Instance.RegisterNavObject(this.NavObjectName, newActivateList[i].ToVector3() + Vector3.one * 0.5f, "", false, -1, null);
		}
		base.CurrentValue = b;
		QuestEventManager.Current.ActiveQuestBlocks = newActivateList;
		this.activateList = newActivateList;
		this.Refresh();
		this.SetupDisplay();
		return true;
	}

	public override BaseObjective Clone()
	{
		ObjectivePOIBlockActivate objectivePOIBlockActivate = new ObjectivePOIBlockActivate();
		this.CopyValues(objectivePOIBlockActivate);
		objectivePOIBlockActivate.completeEvent = this.completeEvent;
		objectivePOIBlockActivate.neededCount = this.neededCount;
		objectivePOIBlockActivate.activateList = this.activateList;
		return objectivePOIBlockActivate;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectivePOIBlockActivate.PropBlockName))
		{
			this.ID = properties.Values[ObjectivePOIBlockActivate.PropBlockName];
		}
		properties.ParseString(ObjectivePOIBlockActivate.PropEventComplete, ref this.completeEvent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int neededCount = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public string completeEvent = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> activateList;

	public static string PropBlockName = "block_index";

	public static string PropEventComplete = "complete_event";

	[PublicizedFrom(EAccessModifier.Private)]
	public new float updateTime;
}
