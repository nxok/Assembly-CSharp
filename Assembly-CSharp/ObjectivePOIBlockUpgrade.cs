using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectivePOIBlockUpgrade : BaseObjective
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
		this.keyword = Localization.Get("ObjectiveBlockUpgrade_keyword", false, null);
		this.localizedName = ((this.ID != "" && this.ID != null) ? Localization.Get(this.ID, false, null) : "Any Block");
		base.OwnerQuest.AddQuestTag(QuestEventManager.restorePowerTag);
	}

	public override void SetupDisplay()
	{
		base.Description = "TEST";
		this.StatusText = string.Format("{0}/{1}", base.CurrentValue, this.neededCount);
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
		QuestEventManager.Current.BlockUpgrade -= this.Current_BlockUpgrade;
		QuestEventManager.Current.BlockUpgrade += this.Current_BlockUpgrade;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		QuestEventManager.Current.BlockUpgrade -= this.Current_BlockUpgrade;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockUpgrade(string blockname, Vector3i blockPos)
	{
		if (base.Complete)
		{
			return;
		}
		NavObjectManager.Instance.UnRegisterNavObjectByPosition(blockPos.ToVector3() + Vector3.one * 0.5f, this.NavObjectName);
		Block blockByName = Block.GetBlockByName(blockname, false);
		if ((this.ID == null || this.ID == "" || this.ID.EqualsCaseInsensitive(blockByName.IndexName)) && base.OwnerQuest.CheckRequirements())
		{
			byte currentValue = base.CurrentValue;
			base.CurrentValue = currentValue + 1;
			this.Refresh();
		}
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
			QuestEventManager.Current.FinishManagedQuest(base.OwnerQuest.QuestCode, base.OwnerQuest.OwnerJournal.OwnerPlayer);
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
				if (base.OwnerQuest.SharedOwnerID == -1)
				{
					base.CurrentValue = 0;
					List<Vector3i> list = new List<Vector3i>();
					List<bool> list2 = new List<bool>();
					QuestEventManager.Current.SetupRepairForMP(list, list2, GameManager.Instance.World, zero);
					this.neededCount = list.Count;
					byte b = 0;
					for (int i = 0; i < list.Count; i++)
					{
						if (list2[i])
						{
							NavObjectManager.Instance.RegisterNavObject(this.NavObjectName, list[i].ToVector3() + Vector3.one * 0.5f, "", false, -1, null);
						}
						else
						{
							b += 1;
						}
					}
					base.CurrentValue = b;
					this.Refresh();
					this.SetupDisplay();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_WaitingForServer()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Update()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Completed()
	{
	}

	public override BaseObjective Clone()
	{
		ObjectivePOIBlockUpgrade objectivePOIBlockUpgrade = new ObjectivePOIBlockUpgrade();
		this.CopyValues(objectivePOIBlockUpgrade);
		return objectivePOIBlockUpgrade;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectivePOIBlockUpgrade.PropBlockName))
		{
			this.ID = properties.Values[ObjectivePOIBlockUpgrade.PropBlockName];
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int neededCount = -1;

	public static string PropBlockName = "block_index";

	[PublicizedFrom(EAccessModifier.Private)]
	public new float updateTime;
}
