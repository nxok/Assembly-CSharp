using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;
using XMLData.Item;

[Preserve]
public class ItemActionEntryRepair : BaseItemActionEntry
{
	public ItemActionEntryRepair(XUiController _controller)
		: base(_controller, "lblContextActionRepair", "ui_game_symbol_wrench", BaseItemActionEntry.GamepadShortCut.DPadLeft, "crafting/craft_click_craft", "ui/ui_denied")
	{
		_controller.xui.PlayerInventory.OnBackpackItemsChanged += this.PlayerInventory_OnBackpackItemsChanged;
		_controller.xui.PlayerInventory.OnToolbeltItemsChanged += this.PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnToolbeltItemsChanged()
	{
		this.RefreshEnabled();
		XUiC_ItemActionEntry parentItem = base.ParentItem;
		if (parentItem == null)
		{
			return;
		}
		parentItem.MarkDirty();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerInventory_OnBackpackItemsChanged()
	{
		this.RefreshEnabled();
		XUiC_ItemActionEntry parentItem = base.ParentItem;
		if (parentItem == null)
		{
			return;
		}
		parentItem.MarkDirty();
	}

	public override void OnDisabledActivate()
	{
		switch (this.state)
		{
		case ItemActionEntryRepair.StateTypes.RecipeLocked:
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiRepairMustReadBook", false, null), true, false, 0f);
			return;
		case ItemActionEntryRepair.StateTypes.NotEnoughMaterials:
		{
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("xuiRepairMissingMats", false, null), true, false, 0f);
			ItemClass itemClass = null;
			XUiC_EquipmentStack xuiC_EquipmentStack = base.ItemController as XUiC_EquipmentStack;
			if (xuiC_EquipmentStack != null)
			{
				if (xuiC_EquipmentStack.ItemStack.IsEmpty())
				{
					return;
				}
				itemClass = xuiC_EquipmentStack.ItemValue.ItemClass;
			}
			else
			{
				XUiC_ItemStack xuiC_ItemStack = base.ItemController as XUiC_ItemStack;
				if (xuiC_ItemStack != null)
				{
					if (xuiC_ItemStack.ItemStack.IsEmpty() || xuiC_ItemStack.StackLock)
					{
						return;
					}
					itemClass = xuiC_ItemStack.ItemStack.itemValue.ItemClass;
				}
			}
			if (itemClass != null)
			{
				ItemData.DataItemArrayRepairTools repairTools = itemClass.RepairTools;
				int? num = ((repairTools != null) ? new int?(repairTools.Length) : null);
				int num2 = 0;
				if ((num.GetValueOrDefault() > num2) & (num != null))
				{
					ItemClass itemClass2 = ItemClass.GetItemClass(itemClass.RepairTools[0].Value, false);
					if (itemClass2 != null)
					{
						ItemStack itemStack = new ItemStack(new ItemValue(itemClass2.Id, false), 0);
						base.ItemController.xui.playerUI.entityPlayer.AddUIHarvestingItem(itemStack, true);
						return;
					}
				}
			}
			break;
		}
		case ItemActionEntryRepair.StateTypes.NoLongerRepairable:
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("ttItemCanNoLongerBeRepaired", false, null), true, false, 0f);
			return;
		case ItemActionEntryRepair.StateTypes.DurabilityWouldBeReduced:
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, Localization.Get("ttDurabilityWouldBeReduced", false, null), true, false, 0f);
			break;
		default:
			return;
		}
	}

	public override void RefreshEnabled()
	{
		base.RefreshEnabled();
		this.state = ItemActionEntryRepair.StateTypes.Normal;
		XUi xui = base.ItemController.xui;
		ItemClass itemClass = null;
		ItemStack itemStack = ItemStack.Empty;
		XUiC_EquipmentStack xuiC_EquipmentStack = base.ItemController as XUiC_EquipmentStack;
		if (xuiC_EquipmentStack != null)
		{
			if (xuiC_EquipmentStack.ItemStack.IsEmpty())
			{
				return;
			}
			itemClass = xuiC_EquipmentStack.ItemValue.ItemClass;
			itemStack = xuiC_EquipmentStack.ItemStack;
		}
		else
		{
			XUiC_ItemStack xuiC_ItemStack = base.ItemController as XUiC_ItemStack;
			if (xuiC_ItemStack != null)
			{
				if (xuiC_ItemStack.ItemStack.IsEmpty() || xuiC_ItemStack.StackLock)
				{
					return;
				}
				itemClass = xuiC_ItemStack.ItemStack.itemValue.ItemClass;
				itemStack = xuiC_ItemStack.ItemStack;
			}
		}
		base.Enabled = this.state == ItemActionEntryRepair.StateTypes.Normal;
		if (!base.Enabled)
		{
			base.IconName = "ui_game_symbol_book";
			return;
		}
		ItemValue itemValue = itemStack.itemValue;
		ItemAction.CanRepairValues canRepairValues = ItemAction.CanRepair(itemValue);
		if (canRepairValues == ItemAction.CanRepairValues.NoLongerRepairable)
		{
			this.state = ItemActionEntryRepair.StateTypes.NoLongerRepairable;
			base.Enabled = false;
			return;
		}
		if (canRepairValues == ItemAction.CanRepairValues.DurabilityWouldBeReduced)
		{
			this.state = ItemActionEntryRepair.StateTypes.DurabilityWouldBeReduced;
			base.Enabled = false;
			return;
		}
		if (itemClass != null)
		{
			ItemData.DataItemArrayRepairTools repairTools = itemClass.RepairTools;
			int? num = ((repairTools != null) ? new int?(repairTools.Length) : null);
			int num2 = 0;
			if ((num.GetValueOrDefault() > num2) & (num != null))
			{
				ItemClass itemClass2 = ItemClass.GetItemClass(itemClass.RepairTools[0].Value, false);
				if (itemClass2 == null)
				{
					return;
				}
				int num3 = Convert.ToInt32(Math.Ceiling((double)((float)Mathf.CeilToInt(itemValue.UseTimes) / (float)itemClass2.RepairAmount.Value)));
				if (Mathf.Min(xui.PlayerInventory.GetItemCount(new ItemValue(itemClass2.Id, false)), num3) * itemClass2.RepairAmount.Value > 0)
				{
					return;
				}
				this.state = ItemActionEntryRepair.StateTypes.NotEnoughMaterials;
				base.Enabled = this.state == ItemActionEntryRepair.StateTypes.Normal;
				return;
			}
		}
	}

	public override void OnActivated()
	{
		XUi xui = base.ItemController.xui;
		XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
		ItemStack itemStack = ItemStack.Empty;
		XUiC_EquipmentStack xuiC_EquipmentStack = base.ItemController as XUiC_EquipmentStack;
		if (xuiC_EquipmentStack != null)
		{
			itemStack = xuiC_EquipmentStack.ItemStack;
		}
		else
		{
			XUiC_ItemStack xuiC_ItemStack = base.ItemController as XUiC_ItemStack;
			if (xuiC_ItemStack != null)
			{
				xuiC_ItemStack.TimeIntervalElapsedEvent += this.ItemActionEntryRepair_TimeIntervalElapsedEvent;
				itemStack = xuiC_ItemStack.ItemStack;
			}
		}
		ItemValue itemValue = itemStack.itemValue;
		ItemClass forId = ItemClass.GetForId(itemValue.type);
		if (((forId != null) ? forId.RepairTools : null) == null || forId.RepairTools.Length <= 0)
		{
			return;
		}
		ItemClass itemClass = ItemClass.GetItemClass(forId.RepairTools[0].Value, false);
		if (itemClass == null)
		{
			return;
		}
		int num = Convert.ToInt32(Math.Ceiling((double)((float)Mathf.CeilToInt(itemValue.UseTimes) / (float)itemClass.RepairAmount.Value)));
		int num2 = Mathf.Min(playerInventory.GetItemCount(new ItemValue(itemClass.Id, false)), num);
		int num3 = num2 * itemClass.RepairAmount.Value;
		XUiC_CraftingWindowGroup childByType = xui.FindWindowGroupByName("crafting").GetChildByType<XUiC_CraftingWindowGroup>();
		if (childByType == null || num3 <= 0)
		{
			return;
		}
		Recipe recipe = new Recipe();
		recipe.count = 1;
		recipe.craftExpGain = Mathf.CeilToInt(forId.RepairExpMultiplier * (float)num2);
		recipe.ingredients.Add(new ItemStack(new ItemValue(itemClass.Id, false), num2));
		recipe.itemValueType = itemValue.type;
		recipe.craftingTime = itemClass.RepairTime.Value * (float)num2;
		num3 = (int)EffectManager.GetValue(PassiveEffects.RepairAmount, null, (float)num3, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()), true, true, true, true, true, 1, true, false);
		recipe.craftingTime = (float)((int)EffectManager.GetValue(PassiveEffects.CraftingTime, null, recipe.craftingTime, xui.playerUI.entityPlayer, recipe, FastTags<TagGroup.Global>.Parse(recipe.GetName()), true, true, true, true, true, 1, true, false));
		if (!childByType.AddRepairItemToQueue(recipe.craftingTime, itemValue.Clone(), num3))
		{
			this.warnQueueFull();
			return;
		}
		XUiC_EquipmentStack xuiC_EquipmentStack2 = base.ItemController as XUiC_EquipmentStack;
		if (xuiC_EquipmentStack2 != null)
		{
			xuiC_EquipmentStack2.ItemStack = ItemStack.Empty;
			xui.PlayerEquipment.Equipment.SetPreferredItemSlot(xuiC_EquipmentStack2.SlotNumber, itemValue);
		}
		else
		{
			XUiC_ItemStack xuiC_ItemStack2 = base.ItemController as XUiC_ItemStack;
			if (xuiC_ItemStack2 != null)
			{
				xuiC_ItemStack2.ItemStack = ItemStack.Empty;
			}
		}
		playerInventory.RemoveItems(recipe.ingredients, 1, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ItemActionEntryRepair_TimeIntervalElapsedEvent(float _timeLeft, XUiC_ItemStack _uiItemStack)
	{
		if (_timeLeft > 0f)
		{
			return;
		}
		ItemStack itemStack = _uiItemStack.ItemStack.Clone();
		itemStack.itemValue.UseTimes = Mathf.Max(0f, itemStack.itemValue.UseTimes - (float)_uiItemStack.RepairAmount);
		_uiItemStack.ItemStack = itemStack;
		_uiItemStack.TimeIntervalElapsedEvent -= this.ItemActionEntryRepair_TimeIntervalElapsedEvent;
		_uiItemStack.UnlockStack();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void warnQueueFull()
	{
		string text = "No room in queue!";
		if (Localization.Exists("wrnQueueFull", false))
		{
			text = Localization.Get("wrnQueueFull", false, null);
		}
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, text, false, false, 0f);
		Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
	}

	public override void DisableEvents()
	{
		base.ItemController.xui.PlayerInventory.OnBackpackItemsChanged -= this.PlayerInventory_OnBackpackItemsChanged;
		base.ItemController.xui.PlayerInventory.OnToolbeltItemsChanged -= this.PlayerInventory_OnToolbeltItemsChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemActionEntryRepair.StateTypes state;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum StateTypes
	{
		Normal,
		RecipeLocked,
		NotEnoughMaterials,
		NoLongerRepairable,
		DurabilityWouldBeReduced
	}
}
