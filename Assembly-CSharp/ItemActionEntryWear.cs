using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryWear : BaseItemActionEntry
{
	public ItemActionEntryWear(XUiController _controller)
		: base(_controller, "lblContextActionWear", "ui_game_symbol_shirt", BaseItemActionEntry.GamepadShortCut.DPadUp, "crafting/craft_click_craft", "ui/ui_denied")
	{
	}

	public override void OnActivated()
	{
		XUiM_PlayerEquipment playerEquipment = base.ItemController.xui.PlayerEquipment;
		ItemStack itemStack = ((XUiC_ItemStack)base.ItemController).ItemStack.Clone();
		((XUiC_ItemStack)base.ItemController).ItemStack = playerEquipment.EquipItem(itemStack);
	}
}
