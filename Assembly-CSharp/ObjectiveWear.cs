using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveWear : BaseObjective
{
	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveWear_keyword", false, null);
		this.expectedItem = ItemClass.GetItem(this.ID, false);
		this.expectedItemClass = ItemClass.GetItemClass(this.ID, false);
	}

	public override void SetupDisplay()
	{
		byte currentValue = base.CurrentValue;
		base.Description = string.Format(this.keyword, this.expectedItemClass.GetLocalizedItemName());
		this.StatusText = "";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.WearItem += this.Current_WearItem;
		XUi xui = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui;
		XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
		if (xui.PlayerEquipment.IsWearing(this.expectedItem) && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 1;
			this.Refresh();
		}
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.WearItem -= this.Current_WearItem;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_WearItem(ItemValue itemValue)
	{
		if (itemValue.type == this.expectedItem.type && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 1;
			this.Refresh();
		}
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		bool flag = base.CurrentValue == 1;
		base.Complete = flag;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveWear objectiveWear = new ObjectiveWear();
		this.CopyValues(objectiveWear);
		return objectiveWear;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;
}
