using System;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryShowCosmetics : BaseItemActionEntry
{
	public ItemActionEntryShowCosmetics(XUiController _controller)
		: base(_controller, "xuiCosmetics", "ui_game_symbol_wardrobe", BaseItemActionEntry.GamepadShortCut.DPadUp, "crafting/craft_click_craft", "ui/ui_denied")
	{
	}

	public override void RefreshEnabled()
	{
		base.Enabled = true;
	}

	public override void OnActivated()
	{
		XUiC_EquipmentStack xuiC_EquipmentStack = base.ItemController as XUiC_EquipmentStack;
		if (xuiC_EquipmentStack == null)
		{
			return;
		}
		ItemClassArmor itemClassArmor = xuiC_EquipmentStack.ItemValue.ItemClass as ItemClassArmor;
		if (itemClassArmor == null)
		{
			return;
		}
		XUiC_CharacterCosmeticWindowGroup.Open(base.ItemController.xui, itemClassArmor.EquipSlot);
	}
}
