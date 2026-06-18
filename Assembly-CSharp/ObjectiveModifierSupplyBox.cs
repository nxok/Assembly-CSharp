using System;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveModifierSupplyBox : BaseObjectiveModifier
{
	public override void AddHooks()
	{
		QuestEventManager.Current.ContainerOpened += this.Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed += this.Current_ContainerClosed;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.ContainerOpened -= this.Current_ContainerOpened;
		QuestEventManager.Current.ContainerClosed -= this.Current_ContainerClosed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerOpened(Vector3i containerLocation, ITileEntityLootable tileEntity)
	{
		if (this.expectedItemClass == null)
		{
			this.SetupExpectedItem();
		}
		int num = this.GetItemCount();
		if (num >= this.itemCount)
		{
			return;
		}
		if (tileEntity.blockValue.Block.GetBlockName() == this.defaultContainer && !tileEntity.HasItem(this.expectedItem))
		{
			tileEntity.AddItem(new ItemStack(this.expectedItem, this.itemCount - num));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_ContainerClosed(Vector3i containerLocation, ITileEntityLootable tileEntity)
	{
		if (tileEntity.blockValue.Block.GetBlockName() == this.defaultContainer)
		{
			tileEntity.RemoveItem(this.expectedItem);
			tileEntity.SetModified();
		}
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveModifierSupplyBox.PropExpectedItemClassID))
		{
			this.expectedItemClassID = properties.Values[ObjectiveModifierSupplyBox.PropExpectedItemClassID];
		}
		if (properties.Values.ContainsKey(ObjectiveModifierSupplyBox.PropQuestItemID))
		{
			this.expectedQuestItemID = properties.Values[ObjectiveModifierSupplyBox.PropQuestItemID];
		}
		if (properties.Values.ContainsKey(ObjectiveModifierSupplyBox.PropItemCount))
		{
			this.itemCount = StringParsers.ParseSInt32(properties.Values[ObjectiveModifierSupplyBox.PropItemCount], 0, -1, NumberStyles.Integer);
		}
		if (properties.Values.ContainsKey(ObjectiveModifierSupplyBox.PropDefaultContainer))
		{
			this.defaultContainer = properties.Values[ObjectiveModifierSupplyBox.PropDefaultContainer];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupExpectedItem()
	{
		if (base.OwnerObjective.OwnerQuest.QuestCode == 0)
		{
			base.OwnerObjective.OwnerQuest.SetupQuestCode();
		}
		this.expectedItemClass = ItemClass.GetItemClass(this.expectedItemClassID, false);
		this.expectedItem = new ItemValue(this.expectedItemClass.Id, false);
		if (this.expectedItemClass is ItemClassQuest)
		{
			ushort num = StringParsers.ParseUInt16(this.expectedQuestItemID, 0, -1, NumberStyles.Integer);
			this.expectedItemClass = ItemClassQuest.GetItemQuestById(num);
			this.expectedItem.Seed = num;
		}
		this.expectedItem.Meta = base.OwnerObjective.OwnerQuest.QuestCode;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void RemoveFetchItems()
	{
		if (this.expectedItemClass == null)
		{
			this.SetupExpectedItem();
		}
		XUi xui = LocalPlayerUI.GetUIForPlayer(base.OwnerObjective.OwnerQuest.OwnerJournal.OwnerPlayer).xui;
		XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
		int num = 1;
		int num2 = 1;
		num2 -= playerInventory.Backpack.DecItem(this.expectedItem, num2, false, null);
		if (num2 > 0)
		{
			playerInventory.Toolbelt.DecItem(this.expectedItem, num2, false, null);
		}
		if (num != num2)
		{
			ItemStack itemStack = new ItemStack(this.expectedItem.Clone(), num - num2);
			xui.CollectedItemList.AddRemoveItemQueueEntry(itemStack);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetItemCount()
	{
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerObjective.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		this.expectedItem.Meta = base.OwnerObjective.OwnerQuest.QuestCode;
		return playerInventory.Backpack.GetItemCount(this.expectedItem, -1, base.OwnerObjective.OwnerQuest.QuestCode, true) + playerInventory.Toolbelt.GetItemCount(this.expectedItem, false, -1, base.OwnerObjective.OwnerQuest.QuestCode, true);
	}

	public override BaseObjectiveModifier Clone()
	{
		return new ObjectiveModifierSupplyBox
		{
			expectedItemClassID = this.expectedItemClassID,
			expectedQuestItemID = this.expectedQuestItemID,
			itemCount = this.itemCount,
			defaultContainer = this.defaultContainer
		};
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemValue expectedItem = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int currentCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int itemCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string defaultContainer = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string expectedItemClassID = "questItem";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string expectedQuestItemID = "";

	public static string PropExpectedItemClassID = "item_class";

	public static string PropQuestItemID = "quest_item_ID";

	public static string PropItemCount = "item_count";

	public static string PropDefaultContainer = "container";
}
