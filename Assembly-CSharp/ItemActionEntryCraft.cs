using System;
using System.Collections.Generic;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryCraft : BaseItemActionEntry
{
	public ItemActionEntryCraft(XUiController _controller, XUiC_RecipeCraftCount _recipeCraftCount, int _craftingTier)
		: base(_controller, "lblContextActionCraft", "ui_game_symbol_hammer", BaseItemActionEntry.GamepadShortCut.DPadUp, "crafting/craft_click_craft", "ui/ui_denied")
	{
		this.craftCountControl = _recipeCraftCount;
		this.craftingTier = _craftingTier;
		WorkstationData workstationData = CraftingManager.GetWorkstationData(((XUiC_RecipeEntry)base.ItemController).Recipe.craftingArea);
		if (workstationData != null)
		{
			base.ActionName = workstationData.CraftActionName;
			base.IconName = workstationData.CraftIcon;
			base.SoundName = workstationData.CraftSound;
		}
	}

	public override void RefreshEnabled()
	{
		base.Enabled = true;
		XUiC_RecipeEntry xuiC_RecipeEntry = (XUiC_RecipeEntry)base.ItemController;
		Recipe recipe = xuiC_RecipeEntry.Recipe;
		if (xuiC_RecipeEntry.Recipe == null)
		{
			this.state = ItemActionEntryCraft.StateTypes.Other;
			base.Enabled = false;
			return;
		}
		if (!xuiC_RecipeEntry.IsCurrentWorkstation)
		{
			this.state = ItemActionEntryCraft.StateTypes.WrongWorkStation;
			base.Enabled = false;
			return;
		}
		if (!XUiM_Recipes.GetRecipeIsUnlocked(base.ItemController.xui, ((XUiC_RecipeEntry)base.ItemController).Recipe))
		{
			this.state = ItemActionEntryCraft.StateTypes.RecipeLocked;
			base.Enabled = false;
			return;
		}
		List<XUiC_CraftingWindowGroup> childrenByType = base.ItemController.xui.GetChildrenByType<XUiC_CraftingWindowGroup>();
		int i = 0;
		while (i < childrenByType.Count)
		{
			if (childrenByType[i].WindowGroup != null && childrenByType[i].WindowGroup.isShowing)
			{
				XUiC_CraftingWindowGroup xuiC_CraftingWindowGroup = childrenByType[i];
				if (!xuiC_CraftingWindowGroup.CraftingRequirementsValid(((XUiC_RecipeEntry)base.ItemController).Recipe))
				{
					this.state = ItemActionEntryCraft.StateTypes.Other;
					this.otherMessage = xuiC_CraftingWindowGroup.CraftingRequirementsInvalidMessage(((XUiC_RecipeEntry)base.ItemController).Recipe);
					base.Enabled = false;
					return;
				}
				break;
			}
			else
			{
				i++;
			}
		}
		if (!this.hasItems(xuiC_RecipeEntry.xui, recipe))
		{
			this.state = ItemActionEntryCraft.StateTypes.NotEnoughMaterials;
			base.Enabled = false;
			return;
		}
		if (this.craftingTier > recipe.GetCraftingTier(base.ItemController.xui.playerUI.entityPlayer))
		{
			this.state = ItemActionEntryCraft.StateTypes.RecipeLocked;
			base.Enabled = false;
			return;
		}
		ItemAction holdingPrimary = base.ItemController.xui.playerUI.entityPlayer.inventory.GetHoldingPrimary();
		if (holdingPrimary != null && holdingPrimary.IsActionRunning(base.ItemController.xui.playerUI.entityPlayer.inventory.holdingItemData.actionData[0]))
		{
			this.state = ItemActionEntryCraft.StateTypes.Other;
			base.Enabled = false;
			return;
		}
		if (base.ItemController.xui.IsUsingItemActionEntryUse)
		{
			this.state = ItemActionEntryCraft.StateTypes.Other;
			base.Enabled = false;
		}
	}

	public override void OnActivated()
	{
		Recipe recipe = ((XUiC_RecipeEntry)base.ItemController).Recipe;
		XUi xui = base.ItemController.xui;
		List<XUiC_CraftingWindowGroup> childrenByType = xui.GetChildrenByType<XUiC_CraftingWindowGroup>();
		XUiC_CraftingWindowGroup xuiC_CraftingWindowGroup = null;
		for (int i = 0; i < childrenByType.Count; i++)
		{
			if (childrenByType[i].WindowGroup != null && childrenByType[i].WindowGroup.isShowing)
			{
				xuiC_CraftingWindowGroup = childrenByType[i];
				break;
			}
		}
		if (xuiC_CraftingWindowGroup == null)
		{
			return;
		}
		if (!XUiM_Recipes.GetRecipeIsUnlocked(base.ItemController.xui, recipe))
		{
			this.craftCountControl.IsDirty = true;
			xuiC_CraftingWindowGroup.WindowGroup.Controller.SetAllChildrenDirty();
			return;
		}
		if (!xuiC_CraftingWindowGroup.CraftingRequirementsValid(recipe))
		{
			this.craftCountControl.IsDirty = true;
			xuiC_CraftingWindowGroup.WindowGroup.Controller.SetAllChildrenDirty();
			return;
		}
		Recipe recipe2 = new Recipe
		{
			itemValueType = recipe.itemValueType,
			count = XUiM_Recipes.GetRecipeCraftOutputCount(xui, recipe),
			craftingArea = recipe.craftingArea,
			craftExpGain = recipe.craftExpGain,
			craftingTime = XUiM_Recipes.GetRecipeCraftTime(xui, recipe),
			craftingToolType = recipe.craftingToolType,
			craftingTier = this.craftingTier,
			tags = recipe.tags
		};
		if (!this.hasItems(xui, recipe))
		{
			return;
		}
		bool flag = false;
		if (this.tempIngredientList.Count == 0)
		{
			if (!xuiC_CraftingWindowGroup.AddItemToQueue(recipe2))
			{
				this.warnQueueFull();
			}
			return;
		}
		for (int j = 0; j < recipe.ingredients.Count; j++)
		{
			flag |= recipe.ingredients[j].itemValue.HasQuality;
			if (flag || this.tempIngredientList[j].count != recipe.ingredients[j].count)
			{
				recipe2.scrapable = true;
			}
		}
		recipe2.AddIngredients(this.tempIngredientList);
		XUiC_WorkstationInputGrid childByType = this.craftCountControl.WindowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
		if (xuiC_CraftingWindowGroup.AddItemToQueue(recipe2))
		{
			if (flag)
			{
				this.tempIngredientList.Clear();
			}
			if (childByType != null)
			{
				childByType.RemoveItems(recipe2.ingredients, this.craftCountControl.Count, this.tempIngredientList);
			}
			else
			{
				xui.PlayerInventory.RemoveItems(recipe2.ingredients, this.craftCountControl.Count, this.tempIngredientList);
			}
			if (flag)
			{
				recipe2.ingredients.Clear();
				recipe2.AddIngredients(this.tempIngredientList);
			}
			if (recipe == xui.Recipes.TrackedRecipe)
			{
				xui.Recipes.TrackedRecipe = null;
				xui.Recipes.ResetToPreviousTracked(xui.playerUI.entityPlayer);
			}
		}
		else
		{
			this.warnQueueFull();
		}
		this.craftCountControl.IsDirty = true;
		xuiC_CraftingWindowGroup.WindowGroup.Controller.SetAllChildrenDirty();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasItems(XUi _xui, Recipe _recipe)
	{
		bool flag = false;
		List<ItemStack> allItemStacks = _xui.PlayerInventory.GetAllItemStacks();
		this.tempIngredientList.Clear();
		float craftingInputModifier = XUiM_Recipes.GetCraftingInputModifier(_recipe);
		if (craftingInputModifier == 0f)
		{
			return true;
		}
		for (int i = 0; i < _recipe.ingredients.Count; i++)
		{
			int num = _recipe.ingredients[i].count;
			int num2 = num;
			if (_recipe.UseIngredientModifier)
			{
				num = (int)EffectManager.GetValue(PassiveEffects.CraftingIngredientCount, null, (float)_recipe.ingredients[i].count, _xui.playerUI.entityPlayer, _recipe, FastTags<TagGroup.Global>.Parse(_recipe.ingredients[i].itemValue.ItemClass.GetItemName()), true, true, true, true, true, this.craftingTier, true, false);
				num2 = num;
				if (num > 0)
				{
					num = (int)((float)num * craftingInputModifier);
				}
			}
			if (!_recipe.ingredients[i].itemValue.HasQuality)
			{
				this.tempIngredientList.Add(new ItemStack(_recipe.ingredients[i].itemValue, num));
			}
			else
			{
				int num3 = ((num2 == 0) ? 1 : num2);
				this.tempIngredientList.Add(new ItemStack(_recipe.ingredients[i].itemValue.Clone(), num3));
				for (int j = 0; j < allItemStacks.Count; j++)
				{
					ItemStack itemStack = allItemStacks[j];
					if ((!itemStack.itemValue.HasModSlots || !itemStack.itemValue.HasMods()) && itemStack.itemValue.type == _recipe.ingredients[i].itemValue.type)
					{
						num3--;
						if (num3 == 0)
						{
							break;
						}
					}
				}
				if (num3 > 0)
				{
					return false;
				}
			}
		}
		flag |= _xui.PlayerInventory.HasItems(this.tempIngredientList, this.craftCountControl.Count);
		XUiC_WorkstationInputGrid childByType = this.craftCountControl.WindowGroup.Controller.GetChildByType<XUiC_WorkstationInputGrid>();
		if (childByType != null)
		{
			flag |= childByType.HasItems(this.tempIngredientList, this.craftCountControl.Count);
		}
		return flag;
	}

	public override void OnDisabledActivate()
	{
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		switch (this.state)
		{
		case ItemActionEntryCraft.StateTypes.RecipeLocked:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttMissingCraftingRecipe", false, null), false, false, 0f);
			return;
		case ItemActionEntryCraft.StateTypes.NotEnoughMaterials:
			GameManager.ShowTooltip(entityPlayer, Localization.Get("ttMissingCraftingResources", false, null), false, false, 0f);
			return;
		case ItemActionEntryCraft.StateTypes.WrongWorkStation:
			break;
		case ItemActionEntryCraft.StateTypes.Other:
			GameManager.ShowTooltip(entityPlayer, this.otherMessage, false, false, 0f);
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void warnQueueFull()
	{
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiCraftQueueFull", false, null), false, false, 0f);
		Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionEntryCraft.StateTypes state;

	[PublicizedFrom(EAccessModifier.Private)]
	public string otherMessage = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly int craftingTier;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_RecipeCraftCount craftCountControl;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<ItemStack> tempIngredientList = new List<ItemStack>();

	[PublicizedFrom(EAccessModifier.Private)]
	public enum StateTypes
	{
		Normal,
		RecipeLocked,
		NotEnoughMaterials,
		WrongWorkStation,
		Other
	}
}
