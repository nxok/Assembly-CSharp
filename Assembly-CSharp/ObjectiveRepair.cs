using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveRepair : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Number;
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveRepair_keyword", false, null);
		this.expectedItem = ItemClass.GetItem(this.ID, false);
		this.expectedItemClass = ItemClass.GetItemClass(this.ID, false);
		this.repairCount = Convert.ToInt32(this.Value);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(this.keyword, this.expectedItemClass.GetLocalizedItemName());
		this.StatusText = string.Format("{0}/{1}", base.CurrentValue, this.repairCount);
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.RepairItem += this.Current_RepairItem;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RepairItem -= this.Current_RepairItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_RepairItem(ItemValue itemValue)
	{
		if (base.Complete)
		{
			return;
		}
		if (itemValue.type == this.expectedItem.type && base.OwnerQuest.CheckRequirements())
		{
			byte currentValue = base.CurrentValue;
			base.CurrentValue = currentValue + 1;
			this.Refresh();
		}
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		base.Complete = (int)base.CurrentValue >= this.repairCount;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveRepair objectiveRepair = new ObjectiveRepair();
		this.CopyValues(objectiveRepair);
		return objectiveRepair;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public int repairCount;
}
