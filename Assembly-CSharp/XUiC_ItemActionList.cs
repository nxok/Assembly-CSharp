using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemActionList : XUiController
{
	public override void Init()
	{
		base.Init();
		for (int i = 0; i < this.children.Count; i++)
		{
			if (this.children[i] is XUiC_ItemActionEntry)
			{
				this.entryList.Add((XUiC_ItemActionEntry)this.children[i]);
			}
		}
		this.craftCountControl = this.windowGroup.Controller.GetChildByType<XUiC_RecipeCraftCount>();
	}

	public override void Update(float _dt)
	{
		ItemAction holdingPrimary = this.xui.playerUI.entityPlayer.inventory.GetHoldingPrimary();
		bool flag = holdingPrimary != null && holdingPrimary.IsActionRunning(this.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[0]);
		if (!flag && this.actionWasRunning)
		{
			this.IsDirty = true;
		}
		this.actionWasRunning = flag;
		if (this.IsDirty)
		{
			this.SortActionList();
			for (int i = 0; i < this.entryList.Count; i++)
			{
				XUiC_ItemActionEntry xuiC_ItemActionEntry = this.entryList[i];
				if (xuiC_ItemActionEntry != null)
				{
					if (i < this.itemActionEntries.Count)
					{
						xuiC_ItemActionEntry.ItemActionEntry = this.itemActionEntries[i];
					}
					else
					{
						xuiC_ItemActionEntry.ItemActionEntry = null;
					}
				}
			}
			this.IsDirty = false;
		}
		if (base.ViewComponent.IsActiveInHierarchy)
		{
			PlayerActionsGUI guiactions = this.xui.playerUI.playerInput.GUIActions;
			if (PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard || !this.xui.playerUI.windowManager.IsInputActive())
			{
				for (int j = 0; j < this.entryList.Count; j++)
				{
					if (this.entryList[j].ItemActionEntry != null && (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard || guiactions.Inspect.IsPressed) && ((guiactions.DPad_Up.WasPressed && this.entryList[j].ItemActionEntry.ShortCut == BaseItemActionEntry.GamepadShortCut.DPadUp) || (guiactions.DPad_Right.WasPressed && this.entryList[j].ItemActionEntry.ShortCut == BaseItemActionEntry.GamepadShortCut.DPadRight) || (guiactions.DPad_Down.WasPressed && this.entryList[j].ItemActionEntry.ShortCut == BaseItemActionEntry.GamepadShortCut.DPadDown) || (guiactions.DPad_Left.WasPressed && this.entryList[j].ItemActionEntry.ShortCut == BaseItemActionEntry.GamepadShortCut.DPadLeft)))
					{
						this.entryList[j].Background.Pressed(-1);
						break;
					}
				}
			}
		}
		base.Update(_dt);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SortActionList()
	{
		if (this.itemActionEntries.Count > 0)
		{
			List<BaseItemActionEntry> list = new List<BaseItemActionEntry>();
			List<BaseItemActionEntry.GamepadShortCut> list2 = new List<BaseItemActionEntry.GamepadShortCut>();
			List<BaseItemActionEntry> list3 = new List<BaseItemActionEntry>();
			for (int i = 0; i < 5; i++)
			{
				BaseItemActionEntry.GamepadShortCut shortcut = (BaseItemActionEntry.GamepadShortCut)i;
				List<BaseItemActionEntry> list4 = this.itemActionEntries.FindAll((BaseItemActionEntry itemEntry) => itemEntry.ShortCut == shortcut);
				if (shortcut == BaseItemActionEntry.GamepadShortCut.None)
				{
					list3.AddRange(list4);
				}
				else if (list4.Count == 0)
				{
					list2.Add(shortcut);
				}
				else if (list4.Count > 1)
				{
					list3.AddRange(list4.GetRange(1, list4.Count - 1));
					list.Add(list4[0]);
				}
				else
				{
					list.AddRange(list4);
				}
			}
			for (int j = 0; j < list3.Count; j++)
			{
				if (list2.Count == 0)
				{
					list3[j].ShortCut = BaseItemActionEntry.GamepadShortCut.None;
				}
				else
				{
					BaseItemActionEntry.GamepadShortCut gamepadShortCut = list2[0];
					list2.RemoveAt(0);
					list3[j].ShortCut = gamepadShortCut;
				}
				list.Add(list3[j]);
			}
			list.Sort((BaseItemActionEntry x, BaseItemActionEntry y) => x.ShortCut - y.ShortCut);
			this.itemActionEntries = list;
		}
	}

	public void AddActionListEntry(BaseItemActionEntry actionEntry)
	{
		this.itemActionEntries.Add(actionEntry);
		actionEntry.ParentActionList = this;
	}

	public void RefreshActionList()
	{
		this.IsDirty = true;
	}

	public void SetCraftingActionList(XUiC_ItemActionList.ItemActionListTypes _actionListType, XUiController itemController)
	{
		for (int i = 0; i < this.itemActionEntries.Count; i++)
		{
			this.itemActionEntries[i].DisableEvents();
		}
		this.itemActionEntries.Clear();
		switch (_actionListType)
		{
		case XUiC_ItemActionList.ItemActionListTypes.Crafting:
		{
			XUiC_RecipeEntry xuiC_RecipeEntry = (XUiC_RecipeEntry)itemController;
			List<XUiC_CraftingWindowGroup> childrenByType = this.xui.GetChildrenByType<XUiC_CraftingWindowGroup>();
			XUiC_CraftingWindowGroup xuiC_CraftingWindowGroup = null;
			for (int j = 0; j < childrenByType.Count; j++)
			{
				if (childrenByType[j].WindowGroup != null && childrenByType[j].WindowGroup.isShowing)
				{
					xuiC_CraftingWindowGroup = childrenByType[j];
					break;
				}
			}
			bool flag = xuiC_CraftingWindowGroup == null || (xuiC_CraftingWindowGroup.Workstation == "" && xuiC_RecipeEntry.Recipe.craftingArea == "");
			int num = -1;
			if (xuiC_CraftingWindowGroup != null)
			{
				num = xuiC_CraftingWindowGroup.GetChildByType<XUiC_CraftingInfoWindow>().SelectedCraftingTier;
				Block blockByName = Block.GetBlockByName(xuiC_CraftingWindowGroup.Workstation, false);
				if (blockByName != null && blockByName.Properties.Contains("Workstation", "CraftingAreaRecipes"))
				{
					string @string = blockByName.Properties.GetString("Workstation", "CraftingAreaRecipes");
					string[] array = new string[] { @string };
					if (@string.Contains(","))
					{
						array = @string.Replace(", ", ",").Replace(" ,", ",").Replace(" , ", ",")
							.Split(',', StringSplitOptions.None);
					}
					for (int k = 0; k < array.Length; k++)
					{
						if (array[k].EqualsCaseInsensitive("player"))
						{
							flag |= string.IsNullOrEmpty(xuiC_RecipeEntry.Recipe.craftingArea);
						}
						else
						{
							flag |= array[k].EqualsCaseInsensitive(xuiC_RecipeEntry.Recipe.craftingArea);
						}
					}
				}
				else
				{
					flag |= xuiC_CraftingWindowGroup.Workstation.EqualsCaseInsensitive(xuiC_RecipeEntry.Recipe.craftingArea);
				}
			}
			if (flag)
			{
				if (XUiM_Recipes.GetRecipeIsUnlocked(this.xui, xuiC_RecipeEntry.Recipe))
				{
					this.AddActionListEntry(new ItemActionEntryCraft(itemController, this.craftCountControl, num));
					this.AddActionListEntry(new ItemActionEntryFavorite(itemController, xuiC_RecipeEntry.Recipe));
				}
				else
				{
					this.HandleUnlockedBy(itemController, xuiC_RecipeEntry);
				}
			}
			else if (!XUiM_Recipes.GetRecipeIsUnlocked(this.xui, xuiC_RecipeEntry.Recipe))
			{
				this.HandleUnlockedBy(itemController, xuiC_RecipeEntry);
			}
			if (xuiC_RecipeEntry.Recipe.IsTrackable && XUiM_Recipes.GetCraftingInputModifier(xuiC_RecipeEntry.Recipe) != 0f)
			{
				this.AddActionListEntry(new ItemActionEntryTrackRecipe(itemController, num));
			}
			break;
		}
		case XUiC_ItemActionList.ItemActionListTypes.Item:
		{
			XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)itemController;
			ItemStack itemStack = xuiC_ItemStack.ItemStack;
			ItemValue itemValue = itemStack.itemValue;
			ItemClass itemClass = itemStack.itemValue.ItemClass;
			if (itemClass != null)
			{
				if (xuiC_ItemStack is XUiC_Creative2Stack)
				{
					this.AddActionListEntry(new ItemActionEntryTake(itemController));
					bool flag2 = true;
					this.AddActionActions(itemValue, itemClass, xuiC_ItemStack, ref flag2);
					if (XUiM_Recipes.FilterRecipesByIngredient(itemStack, XUiM_Recipes.GetRecipes()).Count > 0)
					{
						this.AddActionListEntry(new ItemActionEntryRecipes(itemController));
					}
					this.AddActionListEntry(new CreativeActionEntryFavorite(itemController, itemStack.itemValue.type));
				}
				else if (!itemStack.IsEmpty())
				{
					if (xuiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.LootContainer || xuiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Vehicle)
					{
						this.AddActionListEntry(new ItemActionEntryTake(itemController));
					}
					if (!(xuiC_ItemStack is XUiC_RequiredItemStack))
					{
						if ((xuiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || xuiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) && xuiC_ItemStack == this.xui.AssembleItem.CurrentItemStackController)
						{
							this.AddActionListEntry(new ItemActionEntryAssemble(itemController));
						}
						else
						{
							TraderData traderData = this.xui.Trader.TraderData;
							if (traderData != null)
							{
								TraderInfo traderInfo = traderData.TraderInfo;
								TileEntityVendingMachine tileEntityVendingMachine = this.xui.Trader.Trader as TileEntityVendingMachine;
								if (tileEntityVendingMachine != null)
								{
									if (traderInfo.AllowSell || (!traderInfo.AllowSell && tileEntityVendingMachine.LocalPlayerIsOwner()) || tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
									{
										this.AddActionListEntry(new ItemActionEntrySell(itemController));
									}
								}
								else if (traderInfo.AllowSell)
								{
									this.AddActionListEntry(new ItemActionEntrySell(itemController));
								}
							}
							else
							{
								if (itemClass.CanEquip() && this.xui.AssembleItem.CurrentItem == null)
								{
									this.AddActionListEntry(new ItemActionEntryWear(itemController));
								}
								bool flag3 = false;
								if (itemClass is ItemClassBlock && xuiC_ItemStack.StackLocation != XUiC_ItemStack.StackLocationTypes.ToolBelt && !xuiC_ItemStack.AssembleLock)
								{
									ItemActionEntryEquip itemActionEntryEquip = new ItemActionEntryEquip(itemController);
									this.AddActionListEntry(itemActionEntryEquip);
									flag3 = true;
								}
								this.AddActionActions(itemValue, itemClass, xuiC_ItemStack, ref flag3);
								if ((ItemAction.RepairType == ItemAction.RepairTypes.Both || ItemAction.RepairType == ItemAction.RepairTypes.RepairOnly) && itemValue.MaxUseTimes > 1 && itemValue.UseTimes > 0f && itemClass.RepairTools != null && itemClass.RepairTools.Length > 0 && itemClass.RepairTools[0].Value.Length > 0)
								{
									this.AddActionListEntry(new ItemActionEntryRepair(itemController));
								}
								if ((xuiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || xuiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) && (itemValue.Modifications.Length != 0 || itemValue.CosmeticMods.Length != 0))
								{
									this.AddActionListEntry(new ItemActionEntryAssemble(itemController));
								}
								Recipe scrapableRecipe = CraftingManager.GetScrapableRecipe(itemStack.itemValue, itemStack.count);
								if (scrapableRecipe != null && XUiM_Recipes.ScrappingOutputModifier > 0f && scrapableRecipe.CanCraft(new ItemStack[] { itemStack }, null, -1))
								{
									this.AddActionListEntry(new ItemActionEntryScrap(itemController));
								}
								if ((xuiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || xuiC_ItemStack.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) && this.xui.AssembleItem.CurrentItemStackController == null && XUiM_Recipes.FilterRecipesByIngredient(itemStack, XUiM_Recipes.GetRecipes()).Count > 0)
								{
									this.AddActionListEntry(new ItemActionEntryRecipes(itemController));
								}
								this.AddActionListEntry(new ItemActionEntryDrop(itemController));
							}
						}
					}
				}
			}
			break;
		}
		case XUiC_ItemActionList.ItemActionListTypes.Equipment:
		case XUiC_ItemActionList.ItemActionListTypes.Part:
		{
			XUiC_VehiclePartStack xuiC_VehiclePartStack = itemController as XUiC_VehiclePartStack;
			if (xuiC_VehiclePartStack != null)
			{
				if (xuiC_VehiclePartStack.SlotType != "chassis")
				{
					this.AddActionListEntry(new ItemActionEntryTake(itemController));
				}
			}
			else
			{
				XUiC_ItemPartStack xuiC_ItemPartStack = itemController as XUiC_ItemPartStack;
				if (xuiC_ItemPartStack != null)
				{
					ItemStack itemStack2 = xuiC_ItemPartStack.ItemStack;
					if (itemStack2 != null && (itemStack2.itemValue.ItemClass as ItemClassModifier).Type == ItemClassModifier.ModifierTypes.Attachment)
					{
						this.AddActionListEntry(new ItemActionEntryTake(itemController));
					}
				}
				else if (itemController is XUiC_ItemCosmeticStack)
				{
					this.AddActionListEntry(new ItemActionEntryTake(itemController));
				}
				else
				{
					XUiC_EquipmentStack xuiC_EquipmentStack = (XUiC_EquipmentStack)itemController;
					if (xuiC_EquipmentStack == this.xui.AssembleItem.CurrentEquipmentStackController)
					{
						this.AddActionListEntry(new ItemActionEntryAssemble(itemController));
					}
					else
					{
						ItemStack itemStack3 = xuiC_EquipmentStack.ItemStack;
						if (!itemStack3.IsEmpty())
						{
							ItemValue itemValue2 = itemStack3.itemValue;
							ItemClass itemClass2 = itemValue2.ItemClass;
							ItemClassArmor itemClassArmor = itemClass2 as ItemClassArmor;
							if (itemClassArmor == null || itemClassArmor.AllowUnEquip)
							{
								this.AddActionListEntry(new ItemActionEntryTake(itemController));
							}
							if (itemValue2.Modifications.Length != 0 || itemValue2.CosmeticMods.Length != 0)
							{
								this.AddActionListEntry(new ItemActionEntryAssemble(itemController));
							}
							if ((ItemAction.RepairType == ItemAction.RepairTypes.Both || ItemAction.RepairType == ItemAction.RepairTypes.RepairOnly) && itemValue2.MaxUseTimes > 1 && itemValue2.UseTimes > 0f && itemClass2.RepairTools != null && itemClass2.RepairTools.Length > 0 && itemClass2.RepairTools[0].Value.Length > 0)
							{
								this.AddActionListEntry(new ItemActionEntryRepair(itemController));
							}
							if (itemClassArmor != null && itemClassArmor.IsCosmetic)
							{
								this.AddActionListEntry(new ItemActionEntryShowCosmetics(itemController));
							}
						}
					}
				}
			}
			break;
		}
		case XUiC_ItemActionList.ItemActionListTypes.Trader:
		{
			if (this.xui.Trader.TraderData.TraderInfo.AllowBuy)
			{
				this.AddActionListEntry(new ItemActionEntryPurchase(itemController));
			}
			TileEntityVendingMachine tileEntityVendingMachine2 = this.xui.Trader.Trader as TileEntityVendingMachine;
			if (tileEntityVendingMachine2 != null && tileEntityVendingMachine2.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
			{
				this.AddActionListEntry(new ItemActionEntryMarkup(itemController));
				this.AddActionListEntry(new ItemActionEntryMarkdown(itemController));
				this.AddActionListEntry(new ItemActionEntryResetPrice(itemController));
			}
			break;
		}
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AddActionActions(ItemValue itemValue, ItemClass itemClass, XUiC_ItemStack stackController, ref bool equipFound)
	{
		for (int i = 0; i < itemClass.Actions.Length; i++)
		{
			ItemAction itemAction = itemClass.Actions[i];
			ItemActionEat itemActionEat = itemAction as ItemActionEat;
			if (itemActionEat != null && (!itemActionEat.UsePrompt || stackController.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || stackController.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt))
			{
				this.AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Heal));
			}
			if (itemAction is ItemActionLearnRecipe)
			{
				this.AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Read));
			}
			if (itemAction is ItemActionGainSkill)
			{
				this.AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Read));
			}
			if ((stackController.StackLocation == XUiC_ItemStack.StackLocationTypes.Backpack || stackController.StackLocation == XUiC_ItemStack.StackLocationTypes.ToolBelt) && itemAction is ItemActionQuest)
			{
				this.AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Quest));
			}
			if (itemAction is ItemActionOpenBundle)
			{
				this.AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Open));
			}
			if (itemAction is ItemActionOpenLootBundle)
			{
				this.AddActionListEntry(new ItemActionEntryUse(stackController, ItemActionEntryUse.ConsumeType.Open));
			}
			if (!equipFound && stackController.StackLocation != XUiC_ItemStack.StackLocationTypes.ToolBelt && !stackController.AssembleLock && (itemAction is ItemActionMelee || itemAction is ItemActionDynamicMelee || itemAction is ItemActionRanged || itemAction is ItemActionLauncher || itemAction is ItemActionPlaceAsBlock || itemAction is ItemActionExchangeBlock || itemAction is ItemActionBailLiquid || itemAction is ItemActionActivate || itemAction is ItemActionConnectPower || itemAction is ItemActionThrowAway))
			{
				ItemValue holdingItemItemValue = this.xui.playerUI.entityPlayer.inventory.holdingItemItemValue;
				this.AddActionListEntry(new ItemActionEntryEquip(stackController)
				{
					Enabled = (itemValue.type != holdingItemItemValue.type || itemValue.Quality != holdingItemItemValue.Quality || !Mathf.Approximately(itemValue.UseTimes, holdingItemItemValue.UseTimes))
				});
				equipFound = true;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleUnlockedBy(XUiController itemController, XUiC_RecipeEntry entry)
	{
		ItemClass forId = ItemClass.GetForId(entry.Recipe.itemValueType);
		RecipeUnlockData[] array = (forId.IsBlock() ? forId.GetBlock().UnlockedBy : forId.UnlockedBy);
		RecipeUnlockData recipeUnlockData = null;
		RecipeUnlockData recipeUnlockData2 = null;
		RecipeUnlockData recipeUnlockData3 = null;
		RecipeUnlockData recipeUnlockData4 = null;
		RecipeUnlockData recipeUnlockData5 = null;
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				switch (array[i].UnlockType)
				{
				case RecipeUnlockData.UnlockTypes.Perk:
					recipeUnlockData = array[i];
					break;
				case RecipeUnlockData.UnlockTypes.Book:
					recipeUnlockData2 = array[i];
					break;
				case RecipeUnlockData.UnlockTypes.Skill:
					recipeUnlockData3 = array[i];
					break;
				case RecipeUnlockData.UnlockTypes.Schematic:
				{
					RecipeUnlockData recipeUnlockData6 = array[i];
					break;
				}
				case RecipeUnlockData.UnlockTypes.ChallengeGroup:
					recipeUnlockData4 = array[i];
					break;
				case RecipeUnlockData.UnlockTypes.Challenge:
					recipeUnlockData5 = array[i];
					break;
				}
			}
		}
		if (recipeUnlockData != null)
		{
			this.AddActionListEntry(new ItemActionEntryShowPerk(itemController, recipeUnlockData));
		}
		if (recipeUnlockData2 != null)
		{
			this.AddActionListEntry(new ItemActionEntryShowPerk(itemController, recipeUnlockData2));
		}
		if (recipeUnlockData3 != null)
		{
			this.AddActionListEntry(new ItemActionEntryShowPerk(itemController, recipeUnlockData3));
		}
		EntityPlayerLocal entityPlayer = this.xui.playerUI.entityPlayer;
		if (recipeUnlockData4 != null && recipeUnlockData4.ChallengeGroup.IsVisible(entityPlayer))
		{
			this.AddActionListEntry(new ItemActionEntryShowChallenge(itemController, recipeUnlockData4));
		}
		if (recipeUnlockData5 != null && recipeUnlockData5.Challenge.ChallengeGroup.IsVisible(entityPlayer))
		{
			this.AddActionListEntry(new ItemActionEntryShowChallenge(itemController, recipeUnlockData5));
		}
	}

	public void SetServiceActionList(InGameService service, XUiController itemController)
	{
		this.itemActionEntries.Clear();
		if (service.ServiceType == InGameService.InGameServiceTypes.VendingRent)
		{
			this.AddActionListEntry(new ServiceActionEntryRent(itemController, this.xui.Trader.Trader as TileEntityVendingMachine));
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiC_ItemActionEntry> entryList = new List<XUiC_ItemActionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BaseItemActionEntry> itemActionEntries = new List<BaseItemActionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ItemActionList.ItemActionListTypes itemActionListType = XUiC_ItemActionList.ItemActionListTypes.Crafting;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeEntry recipeEntry;

	[PublicizedFrom(EAccessModifier.Private)]
	public int length;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_RecipeCraftCount craftCountControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool actionWasRunning;

	public enum ItemActionListTypes
	{
		None,
		Buff,
		Crafting,
		Forge,
		Item,
		Loot,
		Equipment,
		Creative,
		Skill,
		Part,
		Trader,
		QuestReward
	}
}
