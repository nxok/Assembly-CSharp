using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

public class Bag : IInventory
{
	public event XUiEvent_BackpackItemsChangedInternal OnBackpackItemsChangedInternal;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bag()
	{
	}

	public Bag(int _size)
	{
		this.items = ItemStack.CreateArray(_size);
	}

	public int SlotCount
	{
		get
		{
			if (this.items != null)
			{
				return this.items.Length;
			}
			return 0;
		}
	}

	public void Write(BinaryWriter bw)
	{
		bw.Write(1);
		ItemStack[] slots = this.GetSlots();
		bw.Write((ushort)slots.Length);
		for (int i = 0; i < slots.Length; i++)
		{
			slots[i].Write(bw);
		}
		bool flag = this.LockedSlots != null;
		bw.Write(flag);
		if (flag)
		{
			this.LockedSlots.Write(bw);
		}
		bw.Write(this.Touched);
		bool flag2 = this.preferences != null;
		bw.Write(flag2);
		if (!flag2)
		{
			return;
		}
		PooledBinaryWriter pooledBinaryWriter = bw as PooledBinaryWriter;
		if (pooledBinaryWriter != null)
		{
			this.preferences.Write(pooledBinaryWriter);
			return;
		}
		throw new InvalidOperationException("[Bag] Writing preferences requires PooledBinaryWriter.");
	}

	public static Bag Read(BinaryReader br)
	{
		return new Bag().ReadInto(br);
	}

	public Bag ReadInto(BinaryReader br)
	{
		byte b = br.ReadByte();
		int num = (int)br.ReadUInt16();
		if (this.items == null || this.SlotCount != num)
		{
			this.items = ItemStack.CreateArray(num);
		}
		for (int i = 0; i < num; i++)
		{
			this.items[i].Read(br);
		}
		if (br.ReadBoolean())
		{
			this.LockedSlots = this.LockedSlots ?? new PackedBoolArray(0);
			this.LockedSlots.Read(br);
		}
		else
		{
			this.LockedSlots = null;
		}
		if (b >= 1)
		{
			this.Touched = br.ReadBoolean();
			if (br.ReadBoolean())
			{
				if (this.preferences == null)
				{
					this.preferences = new PreferenceTracker(-1);
				}
				PooledBinaryReader pooledBinaryReader = br as PooledBinaryReader;
				if (pooledBinaryReader == null)
				{
					throw new InvalidOperationException("[Bag] Reading preferences requires PooledBinaryReader.");
				}
				this.preferences.Read(pooledBinaryReader);
			}
			else
			{
				this.preferences = null;
			}
		}
		else
		{
			this.preferences = null;
		}
		return this;
	}

	public Bag Clone()
	{
		Bag bag = new Bag();
		bag.items = ItemStack.Clone(this.items);
		PackedBoolArray lockedSlots = this.LockedSlots;
		bag.LockedSlots = ((lockedSlots != null) ? lockedSlots.Clone() : null);
		bag.Touched = this.Touched;
		PreferenceTracker preferenceTracker = this.preferences;
		bag.preferences = ((preferenceTracker != null) ? preferenceTracker.Clone() : null);
		return bag;
	}

	public void Clear()
	{
		ItemStack[] array = this.items;
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Clear();
			}
		}
		this.onBackpackChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onBackpackChanged()
	{
		XUiEvent_BackpackItemsChangedInternal onBackpackItemsChangedInternal = this.OnBackpackItemsChangedInternal;
		if (onBackpackItemsChangedInternal == null)
		{
			return;
		}
		onBackpackItemsChangedInternal();
	}

	public bool CanStackNoEmpty(ItemStack _itemStack)
	{
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			int num;
			if (slots[i].CanStackPartlyWith(_itemStack, out num))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEmpty()
	{
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (!slots[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public bool CanStack(ItemStack _itemStack)
	{
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].IsEmpty() || slots[i].CanStackWith(_itemStack, false))
			{
				return true;
			}
		}
		return false;
	}

	[return: TupleElementNames(new string[] { "anyMoved", "allMoved" })]
	public ValueTuple<bool, bool> TryStackItem(int startIndex, ItemStack _itemStack)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.Backpack))
		{
			return new ValueTuple<bool, bool>(false, false);
		}
		ItemStack[] slots = this.GetSlots();
		int num = 0;
		bool flag = false;
		for (int i = startIndex; i < slots.Length; i++)
		{
			num = _itemStack.count;
			if (_itemStack.itemValue.type == slots[i].itemValue.type && slots[i].CanStackPartly(ref num))
			{
				slots[i].count += num;
				_itemStack.count -= num;
				this.onBackpackChanged();
				flag = true;
				if (_itemStack.count == 0)
				{
					return new ValueTuple<bool, bool>(true, true);
				}
			}
		}
		return new ValueTuple<bool, bool>(flag, false);
	}

	public bool CanTakeItem(ItemStack _itemStack)
	{
		ItemStack[] slots = this.GetSlots();
		for (int i = 0; i < slots.Length; i++)
		{
			int num;
			if (slots[i].CanStackPartlyWith(_itemStack, out num))
			{
				return true;
			}
			if (slots[i].IsEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public ItemStack[] GetSlots()
	{
		return this.items;
	}

	public void SetSlots(ItemStack[] _slots)
	{
		this.items = _slots;
		this.onBackpackChanged();
	}

	public void SetSlot(int index, ItemStack _stack, bool callChangedEvent = true)
	{
		if (index >= this.items.Length)
		{
			return;
		}
		this.items[index] = _stack;
		if (callChangedEvent)
		{
			this.onBackpackChanged();
		}
	}

	public bool AddItem(ItemStack _itemStack)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.Backpack))
		{
			return false;
		}
		bool flag = ItemStack.AddToItemStackArray(this.items, _itemStack, -1) >= 0;
		if (flag)
		{
			this.onBackpackChanged();
		}
		return flag;
	}

	public int DecItem(ItemValue _itemValue, int _count, bool _ignoreModdedItems = false, IList<ItemStack> _removedItems = null)
	{
		int num = _count;
		ItemStack[] slots = this.GetSlots();
		int num2 = 0;
		while (_count > 0 && num2 < this.GetSlots().Length)
		{
			if (slots[num2].itemValue.type == _itemValue.type && (!_ignoreModdedItems || !slots[num2].itemValue.HasModSlots || !slots[num2].itemValue.HasMods()))
			{
				if (ItemClass.GetForId(slots[num2].itemValue.type).CanStack())
				{
					int count = slots[num2].count;
					int num3 = ((count >= _count) ? _count : count);
					if (_removedItems != null)
					{
						_removedItems.Add(new ItemStack(slots[num2].itemValue.Clone(), num3));
					}
					slots[num2].count -= num3;
					_count -= num3;
					if (slots[num2].count <= 0)
					{
						slots[num2].Clear();
					}
				}
				else
				{
					if (_removedItems != null)
					{
						_removedItems.Add(slots[num2].Clone());
					}
					slots[num2].Clear();
					_count--;
				}
			}
			num2++;
		}
		this.SetSlots(slots);
		return num - _count;
	}

	public int GetUsedSlotCount()
	{
		ItemStack[] slots = this.GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if (!slots[i].IsEmpty())
			{
				num++;
			}
		}
		return num;
	}

	public int GetItemCount(ItemValue _itemValue, int _seed = -1, int _meta = -1, bool _ignoreModdedItems = true)
	{
		ItemStack[] slots = this.GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if ((!_ignoreModdedItems || !slots[i].itemValue.HasModSlots || !slots[i].itemValue.HasMods()) && slots[i].itemValue.type == _itemValue.type && (_seed == -1 || _seed == (int)slots[i].itemValue.Seed) && (_meta == -1 || _meta == slots[i].itemValue.Meta))
			{
				num += slots[i].count;
			}
		}
		return num;
	}

	public int GetItemCount(FastTags<TagGroup.Global> itemTags, int _seed = -1, int _meta = -1, bool _ignoreModdedItems = true)
	{
		ItemStack[] slots = this.GetSlots();
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if ((!_ignoreModdedItems || !slots[i].itemValue.HasModSlots || !slots[i].itemValue.HasMods()) && !slots[i].itemValue.IsEmpty() && slots[i].itemValue.ItemClass.ItemTags.Test_AnySet(itemTags) && (_seed == -1 || _seed == (int)slots[i].itemValue.Seed) && (_meta == -1 || _meta == slots[i].itemValue.Meta))
			{
				num += slots[i].count;
			}
		}
		return num;
	}

	public bool HasItem(ItemValue _item)
	{
		return this.GetItemCount(_item, -1, -1, true) > 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const byte Version = 1;

	public bool Touched;

	public PackedBoolArray LockedSlots;

	public PreferenceTracker preferences;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;
}
