using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveFetch : BaseObjective
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
		this.keyword = Localization.Get("ObjectiveFetch_keyword", false, null);
		this.expectedItem = ItemClass.GetItem(this.ID, false);
		this.expectedItemClass = ItemClass.GetItemClass(this.ID, false);
		this.itemCount = Convert.ToInt32(this.Value);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(this.keyword, this.expectedItemClass.GetLocalizedItemName());
		this.StatusText = string.Format("{0}/{1}", this.currentCount, this.itemCount);
	}

	public override void AddHooks()
	{
		LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		playerInventory.Backpack.OnBackpackItemsChangedInternal += this.Backpack_OnBackpackItemsChangedInternal;
		playerInventory.Toolbelt.OnToolbeltItemsChangedInternal += this.Toolbelt_OnToolbeltItemsChangedInternal;
		this.Refresh();
	}

	public override void RemoveHooks()
	{
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		if (playerInventory != null)
		{
			playerInventory.Backpack.OnBackpackItemsChangedInternal -= this.Backpack_OnBackpackItemsChangedInternal;
			playerInventory.Toolbelt.OnToolbeltItemsChangedInternal -= this.Toolbelt_OnToolbeltItemsChangedInternal;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_AddItem(ItemStack stack)
	{
		if (base.Complete)
		{
			return;
		}
		if (stack.itemValue.type == this.expectedItem.type)
		{
			if ((int)base.CurrentValue + stack.count > this.itemCount)
			{
				base.CurrentValue = (byte)this.itemCount;
			}
			else
			{
				base.CurrentValue += (byte)stack.count;
			}
			this.Refresh();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Backpack_OnBackpackItemsChangedInternal()
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		if (base.Complete || uiforPlayer.xui.PlayerInventory == null)
		{
			return;
		}
		this.Refresh();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Toolbelt_OnToolbeltItemsChangedInternal()
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer);
		if (base.Complete || uiforPlayer.xui.PlayerInventory == null)
		{
			return;
		}
		this.Refresh();
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		this.currentCount = playerInventory.Backpack.GetItemCount(this.expectedItem, -1, -1, true);
		this.currentCount += playerInventory.Toolbelt.GetItemCount(this.expectedItem, false, -1, -1, true);
		if (this.currentCount > this.itemCount)
		{
			this.currentCount = this.itemCount;
		}
		this.SetupDisplay();
		if (this.currentCount != (int)base.CurrentValue)
		{
			base.CurrentValue = (byte)this.currentCount;
		}
		base.Complete = this.currentCount >= this.itemCount && base.OwnerQuest.CheckRequirements();
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override void RemoveObjectives()
	{
		if (!this.KeepItems)
		{
			XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
			this.itemCount = playerInventory.Backpack.DecItem(this.expectedItem, this.itemCount, false, null);
			if (this.itemCount > 0)
			{
				playerInventory.Toolbelt.DecItem(this.expectedItem, this.itemCount, false, null);
			}
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveFetch objectiveFetch = new ObjectiveFetch();
		this.CopyValues(objectiveFetch);
		objectiveFetch.KeepItems = this.KeepItems;
		return objectiveFetch;
	}

	public override string ParseBinding(string bindingName)
	{
		string id = this.ID;
		string value = this.Value;
		if (!(bindingName == "items"))
		{
			if (!(bindingName == "itemswithcount"))
			{
				return "";
			}
			ItemClass itemClass = ItemClass.GetItemClass(id, false);
			int num = Convert.ToInt32(value);
			if (itemClass == null)
			{
				return "INVALID";
			}
			return num.ToString() + " " + itemClass.GetLocalizedItemName();
		}
		else
		{
			ItemClass itemClass2 = ItemClass.GetItemClass(id, false);
			if (itemClass2 == null)
			{
				return "INVALID";
			}
			return itemClass2.GetLocalizedItemName();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemValue expectedItem = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Private)]
	public int itemCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public int currentCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool KeepItems;
}
