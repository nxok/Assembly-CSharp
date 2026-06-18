using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryScrap : BaseItemActionEntry
{
	public ItemActionEntryScrap(XUiController _controller)
		: base(_controller, "lblContextActionScrap", "ui_game_symbol_scrap", BaseItemActionEntry.GamepadShortCut.DPadRight, "crafting/craft_click_craft", "ui/ui_denied")
	{
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		xuiC_ItemStack.LockChangedEvent += this.ItemStackController_LockChangedEvent;
		ItemStack itemStack = xuiC_ItemStack.ItemStack.Clone();
		Recipe scrapableRecipe = CraftingManager.GetScrapableRecipe(itemStack.itemValue, itemStack.count);
		if (scrapableRecipe == null)
		{
			return;
		}
		this.recipe = scrapableRecipe;
		XUiController xuiController = base.ItemController.xui.FindWindowGroupByName("workstation_workbench");
		if (xuiController == null || !xuiController.WindowGroup.isShowing)
		{
			xuiController = xui.FindWindowGroupByName("crafting");
		}
		XUiC_CraftingWindowGroup childByType = xuiController.GetChildByType<XUiC_CraftingWindowGroup>();
		if (childByType == null)
		{
			return;
		}
		ItemClass forId = ItemClass.GetForId(this.recipe.itemValueType);
		ItemClass forId2 = ItemClass.GetForId(itemStack.itemValue.type);
		int num = forId2.GetWeight() * itemStack.count;
		int weight = forId.GetWeight();
		int num2 = num / weight;
		if (num2 <= 0)
		{
			return;
		}
		int num3 = (int)((float)(num2 * weight) / (float)forId2.GetWeight() + 0.5f);
		Recipe recipe = new Recipe();
		float num4 = ((XUiM_Recipes.DisableSmelter && forId2.HasAnyTags(ItemActionEntryScrap.scrap100Tag)) ? 1f : 0.75f);
		num2 = (int)((float)num2 * (num4 * XUiM_Recipes.ScrappingOutputModifier));
		if (num2 <= 0)
		{
			num2 = 1;
		}
		recipe.count = num2;
		recipe.craftExpGain = this.recipe.craftExpGain;
		recipe.ingredients.Add(new ItemStack(itemStack.itemValue, num3));
		recipe.itemValueType = forId.Id;
		recipe.craftingTime = ((forId2.ScrapTimeOverride > 0f) ? forId2.ScrapTimeOverride : EffectManager.GetValue(PassiveEffects.ScrappingTime, null, forId.CraftComponentTime * (float)num2, xui.playerUI.entityPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
		recipe.scrapable = true;
		recipe.IsScrap = true;
		if (!childByType.AddItemToQueue(recipe, 1))
		{
			this.warnQueueFull();
			return;
		}
		itemStack.count -= num3;
		itemStack = ItemActionEntryScrap.HandleRemoveAmmo(itemStack, base.ItemController.xui);
		((XUiC_ItemStack)base.ItemController).ItemStack = ((itemStack.count <= 0) ? ItemStack.Empty : itemStack.Clone());
		((XUiC_ItemStack)base.ItemController).WindowGroup.Controller.SetAllChildrenDirty();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void warnQueueFull()
	{
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiCraftQueueFull", false, null), false, false, 0f);
		Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemStackController_TimeIntervalElapsedEvent(float _timeLeft, XUiC_ItemStack _uiItemStack)
	{
		EntityPlayerLocal entityPlayer = base.ItemController.xui.playerUI.entityPlayer;
		XUiM_PlayerInventory playerInventory = base.ItemController.xui.PlayerInventory;
		XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		ItemStack itemStack = xuiC_ItemStack.ItemStack.Clone();
		itemStack.count -= (int)this.numberOfCurrentItemsNeededFor1StackOfOutputItem;
		xuiC_ItemStack.ItemStack = itemStack;
		ItemStack itemStack2 = new ItemStack(new ItemValue(this.recipe.itemValueType, false), (int)this.scrapItemCount);
		if (!playerInventory.AddItem(itemStack2))
		{
			GameManager.Instance.ItemDropServer(itemStack2, entityPlayer.GetPosition(), new Vector3(0.5f, 0f, 0.5f), -1, 60f, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemStackController_LockChangedEvent(XUiC_ItemStack.LockTypes _lockType, XUiC_ItemStack _uiItemStack)
	{
		XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		if (xuiC_ItemStack.ItemStack.count == 0)
		{
			xuiC_ItemStack.ItemStack = ItemStack.Empty;
		}
		xuiC_ItemStack.TimeIntervalElapsedEvent -= this.ItemStackController_TimeIntervalElapsedEvent;
		xuiC_ItemStack.UnlockStack();
		xuiC_ItemStack.LockChangedEvent -= this.ItemStackController_LockChangedEvent;
	}

	public override void RefreshEnabled()
	{
		ItemStack itemStack = ((XUiC_ItemStack)base.ItemController).ItemStack;
		if (itemStack.itemValue.Modifications.Length != 0)
		{
			for (int i = 0; i < itemStack.itemValue.Modifications.Length; i++)
			{
				ItemValue itemValue = itemStack.itemValue.Modifications[i];
				if (itemValue != null && !itemValue.IsEmpty() && ((ItemClassModifier)itemValue.ItemClass).Type == ItemClassModifier.ModifierTypes.Attachment)
				{
					base.Enabled = false;
					return;
				}
			}
		}
		base.Enabled = true;
	}

	public override void OnDisabledActivate()
	{
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("ttCannotScrapWithAttachments", false, null), false, false, 0f);
	}

	public static ItemStack HandleRemoveAmmo(ItemStack _stack, XUi _xui)
	{
		if (_stack.itemValue.Meta <= 0)
		{
			return _stack;
		}
		ItemClass forId = ItemClass.GetForId(_stack.itemValue.type);
		for (int i = 0; i < forId.Actions.Length; i++)
		{
			ItemActionRanged itemActionRanged = forId.Actions[i] as ItemActionRanged;
			if (itemActionRanged != null && !(forId.Actions[i] is ItemActionTextureBlock) && itemActionRanged.MagazineItemNames != null && (int)_stack.itemValue.SelectedAmmoTypeIndex < itemActionRanged.MagazineItemNames.Length)
			{
				ItemStack itemStack = new ItemStack(ItemClass.GetItem(itemActionRanged.MagazineItemNames[(int)_stack.itemValue.SelectedAmmoTypeIndex], false), _stack.itemValue.Meta);
				if (!_xui.PlayerInventory.AddItem(itemStack))
				{
					_xui.PlayerInventory.DropItem(itemStack);
				}
				_stack.itemValue.Meta = 0;
			}
		}
		return _stack;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Recipe recipe;

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> scrap100Tag = FastTags<TagGroup.Global>.Parse("scrap100");

	[PublicizedFrom(EAccessModifier.Private)]
	public float scrapItemCount = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float numberOfCurrentItemsNeededFor1StackOfOutputItem = -1f;
}
