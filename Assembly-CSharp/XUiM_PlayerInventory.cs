using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Audio;
using UnityEngine;

public class XUiM_PlayerInventory : XUiModel, IInventory
{
	public event XUiEvent_BackpackItemsChanged OnBackpackItemsChanged;

	public event XUiEvent_ToolbeltItemsChanged OnToolbeltItemsChanged;

	public event XUiEvent_CurrencyChanged OnCurrencyChanged;

	public Bag Backpack
	{
		get
		{
			return this.backpack;
		}
	}

	public Inventory Toolbelt
	{
		get
		{
			return this.toolbelt;
		}
	}

	public int CurrencyAmount
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int QuickSwapSlot
	{
		get
		{
			return this.toolbelt.GetBestQuickSwapSlot();
		}
	}

	public XUiM_PlayerInventory(XUi _xui, EntityPlayerLocal _player)
	{
		if (_player == null)
		{
			return;
		}
		this.xui = _xui;
		this.localPlayer = _player;
		this.backpack = this.localPlayer.bag;
		this.toolbelt = this.localPlayer.inventory;
		this.backpack.OnBackpackItemsChangedInternal += this.dispatchBackpackItemsChanged;
		this.toolbelt.OnToolbeltItemsChangedInternal += this.dispatchToolbeltItemsChanged;
		this.localPlayer.PlayerUI.OnUIShutdown += this.HandleUIShutdown;
		this.currencyItem = ItemClass.GetItem(TraderInfo.CurrencyItem, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dispatchBackpackItemsChanged()
	{
		XUiEvent_BackpackItemsChanged onBackpackItemsChanged = this.OnBackpackItemsChanged;
		if (onBackpackItemsChanged != null)
		{
			onBackpackItemsChanged();
		}
		this.RefreshCurrency();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dispatchToolbeltItemsChanged()
	{
		XUiEvent_ToolbeltItemsChanged onToolbeltItemsChanged = this.OnToolbeltItemsChanged;
		if (onToolbeltItemsChanged != null)
		{
			onToolbeltItemsChanged();
		}
		this.RefreshCurrency();
	}

	public bool AddItemToPreferredToolbeltSlot(ItemStack _itemStack, int _slot)
	{
		return this.toolbelt.AddItemAtSlot(_itemStack, _slot);
	}

	public bool AddItemNoPartial(ItemStack _itemStack, bool _playCollectSound = true)
	{
		return (this.backpack.CanStack(_itemStack) || this.toolbelt.CanStack(_itemStack)) && this.AddItem(_itemStack, _playCollectSound);
	}

	public bool CanSwapItems(ItemStack _removedStack, ItemStack _addedStack, int _slotNumber = -1)
	{
		List<ItemStack> allItemStacks = this.GetAllItemStacks();
		int num = _removedStack.count;
		int num2 = _addedStack.count;
		int value = _removedStack.itemValue.ItemClass.Stacknumber.Value;
		int value2 = _addedStack.itemValue.ItemClass.Stacknumber.Value;
		for (int i = 0; i < allItemStacks.Count - 1; i++)
		{
			if (num > 0 && allItemStacks[i].itemValue.type == _removedStack.itemValue.type && (_slotNumber == -1 || _slotNumber == i))
			{
				int count = allItemStacks[i].count;
				if (count > num)
				{
					num = 0;
				}
				else
				{
					num -= count;
					num2 -= value2;
				}
			}
			else if (num2 > 0 && allItemStacks[i].itemValue.type == _addedStack.itemValue.type)
			{
				int num3 = value2 - allItemStacks[i].count;
				if (num3 >= num2)
				{
					num2 = 0;
				}
				else
				{
					num2 -= num3;
				}
			}
			else if (allItemStacks[i].IsEmpty())
			{
				num2 -= value2;
			}
			if (num <= 0 && num2 <= 0)
			{
				return true;
			}
		}
		return false;
	}

	public void SortStacks(int _ignoreSlots = 0, PackedBoolArray _ignoredSlots = null)
	{
		if (EffectManager.GetValue(PassiveEffects.ShuffledBackpack, null, 0f, this.localPlayer, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) == 0f)
		{
			ItemStack[] array = StackSortUtil.CombineAndSortStacks(this.GetBackpackItemStacks(), _ignoreSlots, _ignoredSlots);
			this.backpack.SetSlots(array);
		}
	}

	public bool AddItem(ItemStack _itemStack, bool _playCollectSound)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.ToolBelt))
		{
			return false;
		}
		bool flag = false;
		ItemStack itemStack = _itemStack.Clone();
		ItemClassArmor itemClassArmor = _itemStack.itemValue.ItemClass as ItemClassArmor;
		if (itemClassArmor != null && itemClassArmor.AutoEquip)
		{
			Equipment equipment = this.xui.playerUI.entityPlayer.equipment;
			int equipSlot = (int)itemClassArmor.EquipSlot;
			if (itemClassArmor.AllowUnEquip)
			{
				ItemValue slotItem = equipment.GetSlotItem(equipSlot);
				equipment.SetSlotItem(equipSlot, itemStack.itemValue, true);
				QuestEventManager.Current.WoreItem(itemStack.itemValue);
				itemStack.count = 0;
				itemStack = new ItemStack(slotItem, 1);
			}
			else
			{
				equipment.SetSlotItem(equipSlot, itemStack.itemValue, true);
				QuestEventManager.Current.WoreItem(itemStack.itemValue);
				flag = true;
				itemStack.count = 0;
			}
			this.xui.PlayerEquipment.RefreshEquipment();
		}
		if (!flag)
		{
			if (this.backpack.CanStackNoEmpty(itemStack))
			{
				flag = this.backpack.TryStackItem(0, itemStack).Item2;
			}
			else if (this.toolbelt.CanStackNoEmpty(itemStack))
			{
				flag = this.toolbelt.TryStackItem(0, itemStack);
			}
		}
		if (!flag)
		{
			if (this.backpack.CanStack(itemStack))
			{
				flag = this.backpack.TryStackItem(0, itemStack).Item2;
			}
			else if (this.toolbelt.CanStack(itemStack))
			{
				flag = this.toolbelt.TryStackItem(0, itemStack);
			}
		}
		if (!flag)
		{
			ItemClass itemClass = itemStack.itemValue.ItemClass;
			int num = 1;
			if (((itemClass != null) ? itemClass.Stacknumber : null) != null)
			{
				num = itemClass.Stacknumber.Value;
			}
			if (itemStack.count > num)
			{
				for (int i = itemStack.count; i > 0; i -= num)
				{
					bool flag2 = false;
					int num2 = Math.Min(i, num);
					num2 = Math.Max(0, num2);
					ItemStack itemStack2 = itemStack.Clone();
					itemStack2.count = num2;
					ItemClassArmor itemClassArmor2 = itemStack.itemValue.ItemClass as ItemClassArmor;
					if (itemClassArmor2 != null && !this.xui.PlayerEquipment.IsEquipmentTypeWorn(itemClassArmor2.EquipSlot) && this.localPlayer.equipment.ReturnItem(itemStack2, true))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
						this.xui.PlayerEquipment.RefreshEquipment();
					}
					else if (this.toolbelt.ReturnItem(itemStack2))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
					}
					else if (this.backpack.AddItem(itemStack2))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
					}
					else if (this.toolbelt.AddItem(itemStack2))
					{
						flag = true;
						flag2 = true;
						itemStack.count -= itemStack2.count;
					}
					if (!flag2)
					{
						if (_itemStack.count != itemStack.count)
						{
							XUiC_CollectedItemList collectedItemList = this.xui.CollectedItemList;
							if (collectedItemList != null)
							{
								collectedItemList.AddItemStack(new ItemStack(itemStack.itemValue, _itemStack.count - itemStack.count), false);
							}
							if (_playCollectSound)
							{
								Manager.PlayInsidePlayerHead("item_pickup", -1, 0f, false, false);
							}
							_itemStack.count = itemStack.count;
						}
						return false;
					}
				}
			}
			else
			{
				ItemClassArmor itemClassArmor3 = itemStack.itemValue.ItemClass as ItemClassArmor;
				if (itemClassArmor3 != null && !this.xui.PlayerEquipment.IsEquipmentTypeWorn(itemClassArmor3.EquipSlot) && this.localPlayer.equipment.ReturnItem(itemStack, true))
				{
					flag = true;
					itemStack = itemStack.Clone();
					itemStack.count = 0;
					this.xui.PlayerEquipment.RefreshEquipment();
				}
				else if (this.toolbelt.ReturnItem(itemStack))
				{
					flag = true;
					itemStack = itemStack.Clone();
					itemStack.count = 0;
					this.dispatchToolbeltItemsChanged();
				}
				else if (this.backpack.AddItem(itemStack))
				{
					flag = true;
					itemStack = itemStack.Clone();
					itemStack.count = 0;
					this.dispatchBackpackItemsChanged();
				}
				else if (this.toolbelt.AddItem(itemStack))
				{
					flag = true;
					itemStack = itemStack.Clone();
					itemStack.count = 0;
					this.dispatchToolbeltItemsChanged();
				}
			}
		}
		if (itemStack.count != _itemStack.count)
		{
			ItemStack itemStack3 = new ItemStack(itemStack.itemValue, _itemStack.count - itemStack.count);
			QuestEventManager.Current.ItemAdded(itemStack3);
			XUiC_CollectedItemList collectedItemList2 = this.xui.CollectedItemList;
			if (collectedItemList2 != null)
			{
				collectedItemList2.AddItemStack(itemStack3, false);
			}
			if (_playCollectSound)
			{
				Manager.PlayInsidePlayerHead("item_pickup", -1, 0f, false, false);
			}
		}
		if (itemStack.count == 0)
		{
			itemStack = ItemStack.Empty;
		}
		_itemStack.count = itemStack.count;
		return flag;
	}

	public int CountAvailableSpaceForItem(ItemValue _itemValue, bool _limitToOneStack = true)
	{
		ItemStack itemStack = new ItemStack(_itemValue, 1);
		int value = _itemValue.ItemClass.Stacknumber.Value;
		int num = 0;
		ItemStack[] slots = this.backpack.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].IsEmpty())
			{
				num += value;
			}
			else if (itemStack.CanStackWith(slots[i], false))
			{
				num += value - slots[i].count;
			}
			if (_limitToOneStack && num >= value)
			{
				return value;
			}
		}
		ItemStack[] slots2 = this.toolbelt.GetSlots();
		for (int j = 0; j < this.toolbelt.PUBLIC_SLOTS; j++)
		{
			if (slots2[j].IsEmpty())
			{
				num += value;
			}
			else if (itemStack.CanStackWith(slots2[j], false))
			{
				num += value - slots2[j].count;
			}
			if (_limitToOneStack && num > value)
			{
				return value;
			}
		}
		return num;
	}

	public void DropItem(ItemStack _stack)
	{
		GameManager instance = GameManager.Instance;
		if (instance)
		{
			instance.ItemDropServer(_stack, this.localPlayer.GetDropPosition(), Vector3.zero, this.localPlayer.entityId, 60f, false);
			Manager.BroadcastPlay("itemdropped");
		}
		XUiC_CollectedItemList collectedItemList = this.xui.CollectedItemList;
		if (collectedItemList == null)
		{
			return;
		}
		collectedItemList.RemoveItemStack(_stack);
	}

	public bool AddItems(ItemStack[] _itemStacks)
	{
		bool flag = true;
		for (int i = 0; i < _itemStacks.Length; i++)
		{
			flag &= this.AddItem(_itemStacks[i]);
		}
		return flag;
	}

	public bool AddItemsUsingPreferenceTracker(XUiC_ItemStackGrid _srcGrid, PreferenceTracker _preferences)
	{
		if (!_preferences.AnyPreferences)
		{
			return false;
		}
		if (this.localPlayer.entityId != _preferences.PlayerID)
		{
			return false;
		}
		bool flag = false;
		XUiC_ItemStack[] itemStackControllers = _srcGrid.GetItemStackControllers();
		HashSet<int> hashSet = new HashSet<int>();
		for (int j = 0; j < itemStackControllers.Length; j++)
		{
			ValueTuple<bool, bool> valueTuple = this.TryStackItem(0, itemStackControllers[j].ItemStack);
			bool item = valueTuple.Item1;
			bool item2 = valueTuple.Item2;
			flag = flag || item;
			if (item)
			{
				if (item2)
				{
					itemStackControllers[j].ItemStack = ItemStack.Empty;
				}
				else
				{
					hashSet.Add(itemStackControllers[j].ItemStack.itemValue.type);
				}
				_srcGrid.HandleSlotChangedEvent(j, itemStackControllers[j].ItemStack);
			}
		}
		if (_preferences.toolbelt != null)
		{
			ItemStack[] slots = this.toolbelt.GetSlots();
			int k = 0;
			while (k < _preferences.toolbelt.Length && k < slots.Length)
			{
				if (slots[k].IsEmpty())
				{
					int type2 = _preferences.toolbelt[k].itemValue.type;
					XUiC_ItemStack xuiC_ItemStack;
					if (hashSet.Contains(type2))
					{
						xuiC_ItemStack = itemStackControllers.FirstOrDefault((XUiC_ItemStack _stack) => _stack.ItemStack.itemValue.type == type2);
					}
					else
					{
						xuiC_ItemStack = itemStackControllers.FirstOrDefault((XUiC_ItemStack _stack) => _stack.ItemStack.Equals(_preferences.toolbelt[k]));
					}
					if (((xuiC_ItemStack != null) ? xuiC_ItemStack.ItemStack : null) != null && !xuiC_ItemStack.ItemStack.IsEmpty())
					{
						this.toolbelt.SetItem(k, xuiC_ItemStack.ItemStack);
						xuiC_ItemStack.ItemStack = ItemStack.Empty;
						_srcGrid.HandleSlotChangedEvent(xuiC_ItemStack.SlotNumber, ItemStack.Empty);
						flag = true;
					}
				}
				int num = k;
				k = num + 1;
			}
		}
		if (_preferences.equipment != null)
		{
			int num2 = Utils.FastMin(this.localPlayer.equipment.GetSlotCount(), _preferences.equipment.Length);
			int num;
			int l;
			for (l = 0; l < num2; l = num + 1)
			{
				if (this.localPlayer.equipment.GetSlotItem(l) == null)
				{
					XUiC_ItemStack xuiC_ItemStack2 = itemStackControllers.FirstOrDefault((XUiC_ItemStack _stack) => _stack.ItemStack.itemValue.Equals(_preferences.equipment[l]));
					if (((xuiC_ItemStack2 != null) ? xuiC_ItemStack2.ItemStack : null) != null && !xuiC_ItemStack2.ItemStack.IsEmpty())
					{
						this.localPlayer.equipment.SetSlotItem(l, xuiC_ItemStack2.ItemStack.itemValue, true);
						xuiC_ItemStack2.ItemStack.count--;
						if (xuiC_ItemStack2.ItemStack.IsEmpty())
						{
							xuiC_ItemStack2.ItemStack = ItemStack.Empty;
							_srcGrid.HandleSlotChangedEvent(xuiC_ItemStack2.SlotNumber, ItemStack.Empty);
						}
						flag = true;
					}
				}
				num = l;
			}
		}
		this.xui.PlayerEquipment.RefreshEquipment();
		if (_preferences.bag != null)
		{
			ItemStack[] slots2 = this.backpack.GetSlots();
			int i = 0;
			while (i < _preferences.bag.Length && i < slots2.Length)
			{
				if (slots2[i].IsEmpty())
				{
					int type = _preferences.bag[i].itemValue.type;
					XUiC_ItemStack xuiC_ItemStack3;
					if (hashSet.Contains(type))
					{
						xuiC_ItemStack3 = itemStackControllers.FirstOrDefault((XUiC_ItemStack _stack) => _stack.ItemStack.itemValue.type == type);
					}
					else
					{
						xuiC_ItemStack3 = itemStackControllers.FirstOrDefault((XUiC_ItemStack _stack) => _stack.ItemStack.Equals(_preferences.bag[i]));
					}
					if (((xuiC_ItemStack3 != null) ? xuiC_ItemStack3.ItemStack : null) != null && !xuiC_ItemStack3.ItemStack.IsEmpty())
					{
						this.backpack.SetSlot(i, xuiC_ItemStack3.ItemStack, true);
						xuiC_ItemStack3.ItemStack = ItemStack.Empty;
						_srcGrid.HandleSlotChangedEvent(xuiC_ItemStack3.SlotNumber, ItemStack.Empty);
						flag = true;
					}
				}
				int num = i;
				i = num + 1;
			}
		}
		return flag;
	}

	public bool AddItemToBackpack(ItemStack _itemStack)
	{
		this.backpack.TryStackItem(0, _itemStack);
		return _itemStack.count > 0 && this.backpack.AddItem(_itemStack);
	}

	public bool AddItemToToolbelt(ItemStack _itemStack)
	{
		this.toolbelt.TryStackItem(0, _itemStack);
		return _itemStack.count > 0 && this.toolbelt.AddItem(_itemStack);
	}

	public bool HasItem(ItemStack _itemStack)
	{
		return this.HasItems(new ItemStack[] { _itemStack }, 1);
	}

	public bool HasItems(IList<ItemStack> _itemStacks, int _multiplier = 1)
	{
		for (int i = 0; i < _itemStacks.Count; i++)
		{
			int num = _itemStacks[i].count * _multiplier;
			num -= this.backpack.GetItemCount(_itemStacks[i].itemValue, -1, -1, true);
			if (num > 0)
			{
				num -= this.toolbelt.GetItemCount(_itemStacks[i].itemValue, false, -1, -1, true);
			}
			if (num > 0)
			{
				return false;
			}
		}
		return true;
	}

	public void RemoveItem(ItemStack _itemStack)
	{
		this.RemoveItems(new ItemStack[] { _itemStack }, 1, null);
	}

	public void RemoveItems(IList<ItemStack> _itemStacks, int _multiplier = 1, IList<ItemStack> _removedItems = null)
	{
		if (!this.HasItems(_itemStacks, 1))
		{
			return;
		}
		for (int i = 0; i < _itemStacks.Count; i++)
		{
			int num = _itemStacks[i].count * _multiplier;
			num -= this.backpack.DecItem(_itemStacks[i].itemValue, num, true, _removedItems);
			if (num > 0)
			{
				this.toolbelt.DecItem(_itemStacks[i].itemValue, num, true, _removedItems);
			}
		}
		this.dispatchBackpackItemsChanged();
		this.dispatchToolbeltItemsChanged();
	}

	public int GetItemCount(ItemValue _itemValue)
	{
		return 0 + this.backpack.GetItemCount(_itemValue, -1, -1, true) + this.toolbelt.GetItemCount(_itemValue, false, -1, -1, true);
	}

	public int GetItemCountWithMods(ItemValue _itemValue)
	{
		return 0 + this.backpack.GetItemCount(_itemValue, -1, -1, false) + this.toolbelt.GetItemCount(_itemValue, false, -1, -1, false);
	}

	public int GetItemCount(int _itemId)
	{
		ItemValue itemValue = new ItemValue(_itemId, false);
		return 0 + this.backpack.GetItemCount(itemValue, -1, -1, true) + this.toolbelt.GetItemCount(itemValue, false, -1, -1, true);
	}

	public List<ItemStack> GetAllItemStacks()
	{
		List<ItemStack> list = new List<ItemStack>();
		list.AddRange(this.GetBackpackItemStacks());
		list.AddRange(this.GetToolbeltItemStacks());
		return list;
	}

	public ItemStack[] GetBackpackItemStacks()
	{
		return this.backpack.GetSlots();
	}

	public ItemStack[] GetToolbeltItemStacks()
	{
		return this.toolbelt.GetSlots();
	}

	public void SetBackpackItemStacks(ItemStack[] _itemStacks)
	{
		this.backpack.SetSlots(_itemStacks);
		this.dispatchBackpackItemsChanged();
	}

	public void SetToolbeltItemStacks(ItemStack[] _itemStacks)
	{
		this.toolbelt.SetSlots(_itemStacks, false);
		this.dispatchToolbeltItemsChanged();
	}

	public void RefreshCurrency()
	{
		int itemCount = this.GetItemCount(this.currencyItem);
		if (itemCount != this.CurrencyAmount)
		{
			this.CurrencyAmount = itemCount;
			XUiEvent_CurrencyChanged onCurrencyChanged = this.OnCurrencyChanged;
			if (onCurrencyChanged == null)
			{
				return;
			}
			onCurrencyChanged();
		}
	}

	public void HandleUIShutdown()
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(this.localPlayer);
		if (uiforPlayer != null)
		{
			uiforPlayer.OnUIShutdown -= this.HandleUIShutdown;
		}
		this.backpack.OnBackpackItemsChangedInternal -= this.dispatchBackpackItemsChanged;
		this.toolbelt.OnToolbeltItemsChangedInternal -= this.dispatchToolbeltItemsChanged;
	}

	public bool AddItem(ItemStack _itemStack)
	{
		return this.AddItem(_itemStack, true);
	}

	[return: TupleElementNames(new string[] { "anyMoved", "allMoved" })]
	public ValueTuple<bool, bool> TryStackItem(int _startIndex, ItemStack _itemStack)
	{
		int count = _itemStack.count;
		bool flag = this.toolbelt.TryStackItem(_startIndex, _itemStack);
		if (!flag)
		{
			flag = this.backpack.TryStackItem(_startIndex, _itemStack).Item2;
		}
		if (count != _itemStack.count)
		{
			ItemStack itemStack = new ItemStack(_itemStack.itemValue, count - _itemStack.count);
			QuestEventManager.Current.ItemAdded(itemStack);
			XUiC_CollectedItemList collectedItemList = this.xui.CollectedItemList;
			if (collectedItemList != null)
			{
				collectedItemList.AddItemStack(itemStack, false);
			}
			Manager.PlayInsidePlayerHead("item_pickup", -1, 0f, false, false);
		}
		return new ValueTuple<bool, bool>(count != _itemStack.count, flag);
	}

	public bool HasItem(ItemValue _item)
	{
		return this.GetItemCount(_item) > 0;
	}

	public static bool TryStackItem(int _startIndex, ItemStack _itemStack, ItemStack[] _items)
	{
		for (int i = _startIndex; i < _items.Length; i++)
		{
			int count = _itemStack.count;
			if (_itemStack.itemValue.type == _items[i].itemValue.type && _items[i].CanStackPartly(ref count))
			{
				_items[i].count += count;
				_itemStack.count -= count;
				if (_itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUi xui;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly EntityPlayerLocal localPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Bag backpack;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Inventory toolbelt;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ItemValue currencyItem;
}
