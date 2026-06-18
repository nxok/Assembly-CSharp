using System;
using System.Collections;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryUse : BaseItemActionEntry
{
	public ItemActionEntryUse(XUiController _controller, ItemActionEntryUse.ConsumeType _consumeType)
		: base(_controller, "OVERRIDDEN BELOW", "OVERRIDDEN BELOW", BaseItemActionEntry.GamepadShortCut.DPadLeft, "crafting/craft_click_craft", "ui/ui_denied")
	{
		this.consumeType = _consumeType;
		switch (_consumeType)
		{
		case ItemActionEntryUse.ConsumeType.Eat:
			base.ActionName = Localization.Get("lblContextActionEat", false, null);
			base.IconName = "ui_game_symbol_fork";
			base.SoundName = "";
			break;
		case ItemActionEntryUse.ConsumeType.Drink:
			base.ActionName = Localization.Get("lblContextActionDrink", false, null);
			base.IconName = "ui_game_symbol_water";
			base.SoundName = "";
			break;
		case ItemActionEntryUse.ConsumeType.Heal:
			base.ActionName = Localization.Get("lblContextActionHeal", false, null);
			base.IconName = "ui_game_symbol_medical";
			break;
		case ItemActionEntryUse.ConsumeType.Read:
			base.ActionName = Localization.Get("lblContextActionRead", false, null);
			base.IconName = "ui_game_symbol_book";
			base.SoundName = "";
			break;
		case ItemActionEntryUse.ConsumeType.Quest:
			base.ActionName = Localization.Get("lblContextActionRead", false, null);
			base.IconName = "ui_game_symbol_quest";
			break;
		case ItemActionEntryUse.ConsumeType.Open:
			base.ActionName = Localization.Get("lblContextActionOpen", false, null);
			base.IconName = "ui_game_symbol_treasure";
			break;
		}
		this.RefreshEnabled();
	}

	public override void OnDisabledActivate()
	{
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		if (entityPlayer.AttachedToEntity)
		{
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttCannotUseWhileOnVehicle", false, null), false, false, 0f);
			return;
		}
		if (base.ItemController.xui.PlayerInventory.Toolbelt.IsHoldingItemActionRunning() || base.ItemController.xui.IsUsingItemActionEntryUse)
		{
			GameManager.ShowTooltip(entityPlayer, Localization.Get("isBusy", false, null), false, false, 0f);
			return;
		}
		if (XUiC_AssembleWindowGroup.GetWindowGroup(base.ItemController.xui).IsOpen)
		{
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttCannotUseWhileAssembling", false, null), false, false, 0f);
			return;
		}
		switch (this.consumeType)
		{
		case ItemActionEntryUse.ConsumeType.Eat:
			if (XUiM_Player.GetFoodPercent(entityPlayer) >= 1f)
			{
				GameManager.ShowTooltip(entityPlayer, Localization.Get("notHungry", false, null), false, false, 0f);
				return;
			}
			GameManager.ShowTooltip(entityPlayer, Localization.Get("isBusy", false, null), false, false, 0f);
			return;
		case ItemActionEntryUse.ConsumeType.Drink:
			if (XUiM_Player.GetWaterPercent(entityPlayer) >= 1f)
			{
				GameManager.ShowTooltip(entityPlayer, Localization.Get("notThirsty", false, null), false, false, 0f);
				return;
			}
			GameManager.ShowTooltip(entityPlayer, Localization.Get("isBusy", false, null), false, false, 0f);
			return;
		case ItemActionEntryUse.ConsumeType.Heal:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("notHurt", false, null), false, false, 0f);
			return;
		case ItemActionEntryUse.ConsumeType.Read:
			switch (this.state)
			{
			case ItemActionEntryUse.StateTypes.RecipeKnown:
				GameManager.ShowTooltip(entityPlayer, Localization.Get("alreadyKnown", false, null), false, false, 0f);
				return;
			case ItemActionEntryUse.StateTypes.SkillRequirementsNotMet:
				GameManager.ShowTooltip(entityPlayer, Localization.Get("ttSkillRequirementsNotMet", false, null), false, false, 0f);
				return;
			case ItemActionEntryUse.StateTypes.SkillKnown:
				GameManager.ShowTooltip(entityPlayer, Localization.Get("ttSkillMaxLevel", false, null), false, false, 0f);
				return;
			default:
				return;
			}
			break;
		case ItemActionEntryUse.ConsumeType.Quest:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("questunavailable", false, null), false, false, 0f);
			return;
		default:
			return;
		}
	}

	public override void RefreshEnabled()
	{
		this.state = ItemActionEntryUse.StateTypes.Normal;
		EntityPlayer entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		if (entityPlayer.AttachedToEntity)
		{
			base.Enabled = false;
			return;
		}
		if (entityPlayer.inventory.IsHoldingItemActionRunning())
		{
			base.Enabled = false;
			return;
		}
		if (XUiC_AssembleWindowGroup.GetWindowGroup(base.ItemController.xui).IsOpen)
		{
			base.Enabled = false;
			return;
		}
		ItemStack itemStack = ((XUiC_ItemStack)base.ItemController).ItemStack;
		switch (this.consumeType)
		{
		case ItemActionEntryUse.ConsumeType.Eat:
			base.Enabled = !base.ItemController.xui.IsUsingItemActionEntryUse && XUiM_Player.GetFoodPercent(entityPlayer) < 1f;
			break;
		case ItemActionEntryUse.ConsumeType.Drink:
			base.Enabled = !base.ItemController.xui.IsUsingItemActionEntryUse && XUiM_Player.GetWaterPercent(entityPlayer) < 1f;
			break;
		case ItemActionEntryUse.ConsumeType.Heal:
			base.Enabled = true;
			break;
		case ItemActionEntryUse.ConsumeType.Read:
			if (itemStack != null && !itemStack.IsEmpty())
			{
				ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
				bool flag = false;
				for (int i = 0; i < forId.Actions.Length; i++)
				{
					ItemActionLearnRecipe itemActionLearnRecipe = forId.Actions[i] as ItemActionLearnRecipe;
					if (itemActionLearnRecipe != null)
					{
						for (int j = 0; j < itemActionLearnRecipe.RecipesToLearn.Length; j++)
						{
							this.state = ItemActionEntryUse.StateTypes.RecipeKnown;
							if (!XUiM_Recipes.GetRecipeIsUnlocked(base.ItemController.xui, itemActionLearnRecipe.RecipesToLearn[j]))
							{
								this.state = ItemActionEntryUse.StateTypes.Normal;
								flag = true;
								break;
							}
						}
						if (flag)
						{
							break;
						}
					}
				}
				base.Enabled = flag;
			}
			else
			{
				base.Enabled = false;
			}
			break;
		case ItemActionEntryUse.ConsumeType.Quest:
			if (itemStack != null && !itemStack.IsEmpty())
			{
				ItemClass forId2 = ItemClass.GetForId(itemStack.itemValue.type);
				int k = 0;
				while (k < forId2.Actions.Length)
				{
					ItemActionQuest itemActionQuest = forId2.Actions[k] as ItemActionQuest;
					if (itemActionQuest != null)
					{
						if (!QuestClass.GetQuest(itemActionQuest.QuestGiven).CanActivate())
						{
							base.Enabled = false;
							break;
						}
						Quest quest = base.ItemController.xui.playerUI.entityPlayer.QuestJournal.FindQuest(itemActionQuest.QuestGiven, -1);
						bool flag2 = base.ItemController.xui.playerUI.entityPlayer.QuestJournal.FindActiveQuest(itemActionQuest.QuestGiven, -1) != null;
						base.Enabled = quest == null || (QuestClass.GetQuest(itemActionQuest.QuestGiven).Repeatable && !flag2);
						break;
					}
					else
					{
						k++;
					}
				}
			}
			break;
		case ItemActionEntryUse.ConsumeType.Open:
			base.Enabled = true;
			break;
		}
		Inventory toolbelt = base.ItemController.xui.PlayerInventory.Toolbelt;
		base.Enabled = base.Enabled && toolbelt.GetItem(toolbelt.DUMMY_SLOT_IDX).IsEmpty();
	}

	public override void OnActivated()
	{
		ItemActionEntryUse.<>c__DisplayClass6_0 CS$<>8__locals1 = new ItemActionEntryUse.<>c__DisplayClass6_0();
		CS$<>8__locals1.<>4__this = this;
		if (base.ItemController.xui.IsUsingItemActionEntryUse)
		{
			return;
		}
		CS$<>8__locals1.stackControl = (XUiC_ItemStack)base.ItemController;
		ItemClass itemClass = CS$<>8__locals1.stackControl.ItemStack.itemValue.ItemClass;
		if (!itemClass.CanExecuteAction(0, base.ItemController.xui.playerUI.entityPlayer, CS$<>8__locals1.stackControl.ItemStack.itemValue))
		{
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("ttCannotUseAtThisTime", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			return;
		}
		base.ItemController.xui.IsUsingItemActionEntryUse = true;
		CS$<>8__locals1.itemStack = new ItemStack(CS$<>8__locals1.stackControl.ItemStack.itemValue.Clone(), 1);
		CS$<>8__locals1.originalStack = new ItemStack(CS$<>8__locals1.stackControl.ItemStack.itemValue.Clone(), CS$<>8__locals1.stackControl.ItemStack.count);
		CS$<>8__locals1.inventory = base.ItemController.xui.PlayerInventory.Toolbelt;
		if (this.consumeType == ItemActionEntryUse.ConsumeType.Quest)
		{
			base.ItemController.xui.FindWindowGroupByName("questOffer").GetChildByType<XUiC_QuestOfferWindow>().ItemStackController = CS$<>8__locals1.stackControl;
			CS$<>8__locals1.stackControl.QuestLock = true;
		}
		else
		{
			CS$<>8__locals1.stackControl.HiddenLock = true;
		}
		CS$<>8__locals1.stackControl.WindowGroup.Controller.SetAllChildrenDirty();
		this.RefreshEnabled();
		CS$<>8__locals1.actionIdx = 0;
		for (int i = 0; i < itemClass.Actions.Length; i++)
		{
			bool flag = false;
			switch (this.consumeType)
			{
			case ItemActionEntryUse.ConsumeType.Eat:
			case ItemActionEntryUse.ConsumeType.Drink:
			case ItemActionEntryUse.ConsumeType.Heal:
				if (itemClass.Actions[i] != null)
				{
					flag = true;
				}
				break;
			case ItemActionEntryUse.ConsumeType.Read:
				if (itemClass.Actions[i] is ItemActionLearnRecipe)
				{
					flag = true;
				}
				break;
			case ItemActionEntryUse.ConsumeType.Quest:
				if (itemClass.Actions[i] is ItemActionQuest)
				{
					flag = true;
				}
				break;
			case ItemActionEntryUse.ConsumeType.Open:
			{
				ItemAction itemAction = itemClass.Actions[i];
				if (itemAction is ItemActionOpenBundle || itemAction is ItemActionOpenLootBundle)
				{
					flag = true;
				}
				break;
			}
			}
			if (flag)
			{
				CS$<>8__locals1.actionIdx = i;
				break;
			}
		}
		if (!(CS$<>8__locals1.itemStack.itemValue.ItemClass.Actions[CS$<>8__locals1.actionIdx] is ItemActionEat))
		{
			CS$<>8__locals1.originalStack.count--;
			if (CS$<>8__locals1.originalStack.count == 0)
			{
				CS$<>8__locals1.originalStack = ItemStack.Empty;
			}
		}
		if (this.consumeType != ItemActionEntryUse.ConsumeType.Quest)
		{
			CS$<>8__locals1.stackControl.ItemStack = CS$<>8__locals1.originalStack;
		}
		if (!CS$<>8__locals1.itemStack.itemValue.ItemClass.Actions[CS$<>8__locals1.actionIdx].UseAnimation && CS$<>8__locals1.itemStack.itemValue.ItemClass.Actions[CS$<>8__locals1.actionIdx].ExecuteInstantAction(base.ItemController.xui.playerUI.entityPlayer, CS$<>8__locals1.itemStack, false, CS$<>8__locals1.stackControl))
		{
			if (this.consumeType != ItemActionEntryUse.ConsumeType.Quest)
			{
				CS$<>8__locals1.stackControl.HiddenLock = false;
				CS$<>8__locals1.stackControl.WindowGroup.Controller.SetAllChildrenDirty();
			}
			base.ItemController.xui.IsUsingItemActionEntryUse = false;
			return;
		}
		ItemActionEat itemActionEat = CS$<>8__locals1.itemStack.itemValue.ItemClass.Actions[CS$<>8__locals1.actionIdx] as ItemActionEat;
		if (itemActionEat != null && itemActionEat.UsePrompt)
		{
			base.ItemController.xui.IsUsingItemActionEntryPromptComplete = true;
			XUiC_MessageBoxWindowGroup.ShowOkCancel(base.ItemController.xui, Localization.Get(itemActionEat.PromptTitle, false, null), Localization.Get(itemActionEat.PromptDescription, false, null), new Action(CS$<>8__locals1.<OnActivated>g__UseItemWithAnimation|1), new Action(CS$<>8__locals1.<OnActivated>g__SwitchBack|2), true, true, false);
			return;
		}
		CS$<>8__locals1.<OnActivated>g__UseItemWithAnimation|1();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SwitchBackCoroutine(Inventory _inventory)
	{
		while (_inventory.IsHolsterDelayActive())
		{
			yield return null;
		}
		((XUiC_ItemStack)base.ItemController).HiddenLock = false;
		base.ParentActionList.RefreshActionList();
		base.ItemController.xui.IsUsingItemActionEntryUse = false;
		this.RefreshEnabled();
		yield break;
	}

	public override void OnTimerCompleted()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ItemActionEntryUse.ConsumeType consumeType;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionEntryUse.StateTypes state;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum StateTypes
	{
		Normal,
		RecipeKnown,
		SkillRequirementsNotMet,
		SkillKnown
	}

	public enum ConsumeType
	{
		None,
		Eat,
		Drink,
		Heal,
		Read,
		Quest,
		Open
	}
}
