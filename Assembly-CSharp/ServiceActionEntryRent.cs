using System;
using UnityEngine.Scripting;

[Preserve]
public class ServiceActionEntryRent : BaseItemActionEntry
{
	public ServiceActionEntryRent(XUiController _controller, TileEntityVendingMachine _vending)
		: base(_controller, "lblContextActionRent", "ui_game_symbol_coin", BaseItemActionEntry.GamepadShortCut.None, "crafting/craft_click_craft", "ui/ui_denied")
	{
		this.vending = _vending;
	}

	public override void RefreshEnabled()
	{
		base.Enabled = this.vending.CanRent() == TileEntityVendingMachine.RentResult.Allowed;
	}

	public override void OnDisabledActivate()
	{
		string text;
		switch (this.vending.CanRent())
		{
		case TileEntityVendingMachine.RentResult.AlreadyRented:
			text = Localization.Get("ttVMAlreadyRented", false, null);
			break;
		case TileEntityVendingMachine.RentResult.AlreadyRentingVM:
			text = Localization.Get("ttAlreadyRentingVM", false, null);
			break;
		case TileEntityVendingMachine.RentResult.NotEnoughMoney:
			text = Localization.Get(this.vending.LocalPlayerIsOwner() ? "ttVMNotEnoughMoneyAddTime" : "ttVMNotEnoughMoneyRent", false, null);
			break;
		default:
			text = null;
			break;
		}
		string text2 = text;
		if (text2 != null)
		{
			GameManager.ShowTooltip(base.ItemController.xui.playerUI.entityPlayer, text2, false, false, 0f);
		}
	}

	public override void OnActivated()
	{
		if (this.vending.Rent())
		{
			((XUiC_TraderWindow)base.ItemController).Refresh();
		}
		this.RefreshEnabled();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly TileEntityVendingMachine vending;
}
