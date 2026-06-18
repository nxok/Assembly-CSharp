using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryDrop : BaseItemActionEntry
{
	public ItemActionEntryDrop(XUiController _controller)
		: base(_controller, "lblContextActionDrop", "ui_game_symbol_drop", BaseItemActionEntry.GamepadShortCut.DPadDown, "crafting/craft_click_craft", "ui/ui_denied")
	{
	}

	public override void OnActivated()
	{
		LocalPlayerUI playerUI = base.ItemController.xui.playerUI;
		XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		base.ItemController.xui.CollectedItemList.RemoveItemStack(xuiC_ItemStack.ItemStack);
		GameManager.Instance.ItemDropServer(xuiC_ItemStack.ItemStack, playerUI.entityPlayer.GetDropPosition(), Vector3.zero, playerUI.entityPlayer.entityId, 60f, false);
		playerUI.entityPlayer.PlayOneShot("itemdropped", false, false, false, null, 1f);
		xuiC_ItemStack.ItemStack = ItemStack.Empty;
	}

	public override void RefreshEnabled()
	{
		XUiC_ItemStack xuiC_ItemStack = (XUiC_ItemStack)base.ItemController;
		base.Enabled = !xuiC_ItemStack.ItemStack.IsEmpty() && xuiC_ItemStack.ItemStack.itemValue.ItemClass.CanDrop(null) && !xuiC_ItemStack.StackLock;
	}

	public override void OnDisabledActivate()
	{
		GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, "This item cannot be dropped.", false, false, 0f);
	}
}
