using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class ItemActionEntryPurchase : BaseItemActionEntry
{
	public ItemActionEntryPurchase(XUiController _controller)
		: base(_controller, "OVERRIDDEN BELOW", "OVERRIDDEN BELOW", BaseItemActionEntry.GamepadShortCut.None, "", "ui/ui_denied")
	{
		TileEntityVendingMachine tileEntityVendingMachine = _controller.xui.Trader.Trader as TileEntityVendingMachine;
		if (tileEntityVendingMachine != null)
		{
			TraderInfo traderInfo = _controller.xui.Trader.TraderData.TraderInfo;
			bool playerOwned = traderInfo.PlayerOwned;
			bool rentable = traderInfo.Rentable;
			this.isOwner = (playerOwned || rentable) && tileEntityVendingMachine.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
			this.isVending = true;
		}
		else
		{
			this.isOwner = false;
			this.isVending = false;
		}
		if (this.isOwner)
		{
			base.ActionName = Localization.Get("lblContextActionTake", false, null);
			base.IconName = "ui_game_symbol_hand";
			return;
		}
		base.ActionName = Localization.Get("lblContextActionBuy", false, null);
		base.IconName = "ui_game_symbol_coin";
	}

	public override void RefreshEnabled()
	{
		this.refreshBinding();
		XUiC_TraderItemEntry xuiC_TraderItemEntry = (XUiC_TraderItemEntry)base.ItemController;
		if (((xuiC_TraderItemEntry != null) ? xuiC_TraderItemEntry.Item : null) == null)
		{
			base.Enabled = false;
			return;
		}
		int count = xuiC_TraderItemEntry.InfoWindow.BuySellCounter.Count;
		if (this.isOwner)
		{
			base.Enabled = count > 0;
			return;
		}
		XUi xui = xuiC_TraderItemEntry.xui;
		XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
		ItemStack itemStack = xuiC_TraderItemEntry.Item.Clone();
		ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
		int buyPrice = XUiM_Trader.GetBuyPrice(xui, itemStack.itemValue, count, forId, xuiC_TraderItemEntry.SlotIndex);
		ItemValue item = ItemClass.GetItem(TraderInfo.CurrencyItem, false);
		base.Enabled = count > 0 && playerInventory.GetItemCount(item) >= buyPrice;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void refreshBinding()
	{
		XUiC_TraderItemEntry xuiC_TraderItemEntry = (XUiC_TraderItemEntry)base.ItemController;
		if (xuiC_TraderItemEntry == null)
		{
			return;
		}
		int slotIndex = xuiC_TraderItemEntry.SlotIndex;
		XUi xui = xuiC_TraderItemEntry.xui;
		List<TraderData.Entry> list;
		if (xui == null)
		{
			list = null;
		}
		else
		{
			XUiM_Trader trader = xui.Trader;
			if (trader == null)
			{
				list = null;
			}
			else
			{
				TraderData traderData = trader.TraderData;
				list = ((traderData != null) ? traderData.PrimaryInventory : null);
			}
		}
		List<TraderData.Entry> list2 = list;
		if (list2 != null && slotIndex < list2.Count)
		{
			xuiC_TraderItemEntry.Item = list2[slotIndex].Item;
			return;
		}
		xuiC_TraderItemEntry.Item = null;
	}

	public override void OnActivated()
	{
		this.refreshBinding();
		XUiC_TraderItemEntry xuiC_TraderItemEntry = (XUiC_TraderItemEntry)base.ItemController;
		TraderData traderData = base.ItemController.xui.Trader.TraderData;
		try
		{
			ItemStack itemStack = xuiC_TraderItemEntry.Item.Clone();
			ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
			ItemValue itemValue = itemStack.itemValue;
			int num = xuiC_TraderItemEntry.InfoWindow.BuySellCounter.Count;
			int buyPrice = XUiM_Trader.GetBuyPrice(base.ItemController.xui, itemStack.itemValue, num, forId, xuiC_TraderItemEntry.SlotIndex);
			XUi xui = base.ItemController.xui;
			XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
			int num2 = (forId.IsBlock() ? Block.list[itemValue.type].EconomicBundleSize : forId.EconomicBundleSize);
			int num3 = num % num2;
			if (num3 != 0)
			{
				GameManager.ShowTooltip(xui.playerUI.entityPlayer, string.Format(Localization.Get("ttItemCountNotBundleSize", false, null), num2), false, false, 0f);
				num -= num3;
				xuiC_TraderItemEntry.InfoWindow.BuySellCounter.Count = num;
				Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
			}
			else
			{
				int num4 = playerInventory.CountAvailableSpaceForItem(itemValue, true);
				if (num > num4)
				{
					GameManager.ShowTooltip(xui.playerUI.entityPlayer, string.Format(Localization.Get("ttItemCountMoreThanAvailable", false, null), num2), false, false, 0f);
					Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
				}
				else
				{
					if (num > itemStack.count)
					{
						num = itemStack.count;
					}
					if (this.isOwner)
					{
						int num5 = num;
						ItemStack itemStack2 = new ItemStack(itemStack.itemValue, num);
						if (playerInventory.AddItem(itemStack2))
						{
							ItemStack item = xuiC_TraderItemEntry.Item;
							item.count -= num;
							xuiC_TraderItemEntry.InfoWindow.BuySellCounter.MaxCount -= num;
							xuiC_TraderItemEntry.Refresh();
							if (item.count == 0)
							{
								traderData.PrimaryInventory.RemoveAt(xuiC_TraderItemEntry.SlotIndex);
							}
							Manager.PlayInsidePlayerHead("craft_take_item", -1, 0f, false, false);
						}
						else if (num5 != itemStack2.count)
						{
							ItemStack item2 = xuiC_TraderItemEntry.Item;
							if (itemStack2.count == 0)
							{
								itemStack2 = ItemStack.Empty;
								traderData.PrimaryInventory.RemoveAt(xuiC_TraderItemEntry.SlotIndex);
							}
							else
							{
								item2.count -= num5 - itemStack2.count;
								xuiC_TraderItemEntry.InfoWindow.BuySellCounter.MaxCount -= num;
								xuiC_TraderItemEntry.Refresh();
							}
							Manager.PlayInsidePlayerHead("craft_take_item", -1, 0f, false, false);
						}
					}
					else
					{
						ItemStack itemStack3 = new ItemStack(ItemClass.GetItem(TraderInfo.CurrencyItem, false), buyPrice);
						itemStack.count = num;
						if (playerInventory.CanSwapItems(itemStack3, itemStack, -1))
						{
							if (playerInventory.AddItem(itemStack, false))
							{
								playerInventory.RemoveItem(itemStack3);
								traderData.AvailableMoney += buyPrice;
								XUiC_TraderItemEntry xuiC_TraderItemEntry2 = xuiC_TraderItemEntry;
								ItemStack item3 = xuiC_TraderItemEntry2.Item;
								item3.count -= num;
								xuiC_TraderItemEntry2.InfoWindow.BuySellCounter.MaxCount -= num;
								xuiC_TraderItemEntry2.Refresh();
								if (item3.count == 0)
								{
									traderData.PrimaryInventory.RemoveAt(xuiC_TraderItemEntry2.SlotIndex);
								}
								EntityTrader entityTrader = base.ItemController.xui.Trader.Trader as EntityTrader;
								if (entityTrader != null)
								{
									Manager.PlayInsidePlayerHead("ui_trader_purchase", -1, 0f, false, false);
									QuestEventManager.Current.BoughtItems(entityTrader.EntityName, num);
								}
								else
								{
									Manager.PlayInsidePlayerHead("ui_vending_purchase", -1, 0f, false, false);
									QuestEventManager.Current.BoughtItems("", num);
								}
								GameSparksCollector.IncrementCounter(this.isVending ? GameSparksCollector.GSDataKey.VendingItemsBought : GameSparksCollector.GSDataKey.TraderItemsBought, forId.Name, num, true, GameSparksCollector.GSDataCollection.SessionUpdates);
								GameSparksCollector.IncrementCounter(this.isVending ? GameSparksCollector.GSDataKey.VendingMoneySpentOn : GameSparksCollector.GSDataKey.TraderMoneySpentOn, forId.Name, buyPrice, true, GameSparksCollector.GSDataCollection.SessionUpdates);
								GameSparksCollector.IncrementCounter(GameSparksCollector.GSDataKey.TotalMoneySpentOn, forId.Name, buyPrice, true, GameSparksCollector.GSDataCollection.SessionUpdates);
							}
						}
						else
						{
							GameManager.ShowTooltip(xui.playerUI.entityPlayer, Localization.Get("ttNoSpaceForBuying", false, null), false, false, 0f);
							Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
						}
					}
					base.ItemController.xui.Trader.TraderWindowGroup.RefreshTraderItems();
				}
			}
		}
		finally
		{
			traderData.SetModified(base.ItemController.xui.Trader.Trader);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool isOwner;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly bool isVending;
}
