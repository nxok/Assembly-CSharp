using System;
using System.Globalization;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveBaseFetchContainer : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			if (base.CurrentValue != 3)
			{
				return BaseObjective.ObjectiveValueTypes.Number;
			}
			return BaseObjective.ObjectiveValueTypes.Boolean;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void RemoveFetchItems()
	{
		if (base.CurrentValue != 3)
		{
			return;
		}
		if (this.expectedItemClass == null)
		{
			this.SetupExpectedItem();
		}
		XUi xui = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui;
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

	public override void SetupQuestTag()
	{
		base.OwnerQuest.AddQuestTag(QuestEventManager.fetchTag);
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveFetchContainer_keyword", false, null);
		if (this.expectedItemClass == null)
		{
			this.SetupExpectedItem();
		}
		this.SetupQuestTag();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupExpectedItem()
	{
		if (base.OwnerQuest.QuestCode == 0)
		{
			base.OwnerQuest.SetupQuestCode();
		}
		this.expectedItemClass = ItemClass.GetItemClass(this.questItemClassID, false);
		this.expectedItem = new ItemValue(this.expectedItemClass.Id, false);
		if (this.expectedItemClass is ItemClassQuest)
		{
			ushort num = StringParsers.ParseUInt16(this.ID, 0, -1, NumberStyles.Integer);
			this.expectedItemClass = ItemClassQuest.GetItemQuestById(num);
			this.expectedItem.Seed = num;
		}
		this.expectedItem.Meta = base.OwnerQuest.QuestCode;
	}

	public override void HandleCompleted()
	{
		base.HandleCompleted();
		this.RemoveFetchItems();
	}

	public override void HandleFailed()
	{
		base.HandleFailed();
		this.RemoveFetchItems();
	}

	public override void ResetObjective()
	{
		this.RemoveFetchItems();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int GetItemCount(int _expectedMeta = -2)
	{
		XUiM_PlayerInventory playerInventory = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory;
		if (_expectedMeta == -2)
		{
			_expectedMeta = base.OwnerQuest.QuestCode;
		}
		this.expectedItem.Meta = _expectedMeta;
		return playerInventory.Backpack.GetItemCount(this.expectedItem, -1, _expectedMeta, true) + playerInventory.Toolbelt.GetItemCount(this.expectedItem, false, -1, _expectedMeta, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		((ObjectiveBaseFetchContainer)objective).questItemClassID = this.questItemClassID;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveBaseFetchContainer.PropQuestItemClass))
		{
			this.questItemClassID = properties.Values[ObjectiveBaseFetchContainer.PropQuestItemClass];
		}
		if (properties.Values.ContainsKey(ObjectiveBaseFetchContainer.PropQuestItemID))
		{
			this.ID = properties.Values[ObjectiveBaseFetchContainer.PropQuestItemID];
		}
		if (properties.Values.ContainsKey(ObjectiveBaseFetchContainer.PropItemCount))
		{
			this.Value = properties.Values[ObjectiveBaseFetchContainer.PropItemCount];
		}
		if (properties.Values.ContainsKey(ObjectiveBaseFetchContainer.PropDefaultContainer))
		{
			this.defaultContainer = properties.Values[ObjectiveBaseFetchContainer.PropDefaultContainer];
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemValue expectedItem = ItemValue.None;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemClass expectedItemClass;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float distance;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int currentCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string defaultContainer = "";

	public string questItemClassID = "questItem";

	public static string PropQuestItemClass = "quest_item";

	public static string PropQuestItemID = "quest_item_ID";

	public static string PropItemCount = "item_count";

	public static string PropDefaultContainer = "default_container";

	[PublicizedFrom(EAccessModifier.Protected)]
	public World world;
}
