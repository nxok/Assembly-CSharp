using System;
using System.Collections.Generic;
using System.IO;

public class ItemStack
{
	public static ItemStack Empty
	{
		get
		{
			return new ItemStack(ItemValue.None, 0);
		}
	}

	public ItemStack()
	{
		this.itemValue = ItemValue.None;
		this.count = 0;
	}

	public ItemStack(ItemValue _itemValue, int _count)
	{
		this.itemValue = _itemValue;
		this.count = _count;
	}

	public ItemStack Clone()
	{
		if (this.itemValue != null)
		{
			return new ItemStack(this.itemValue.Clone(), this.count);
		}
		return new ItemStack(ItemValue.None, this.count);
	}

	public static ItemStack[] CreateArray(int _size)
	{
		ItemStack[] array = new ItemStack[_size];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = ItemStack.Empty;
		}
		return array;
	}

	public bool IsEmpty()
	{
		return this.count < 1 || this.itemValue.type == 0;
	}

	public void Clear()
	{
		this.itemValue.Clear();
		this.count = 0;
	}

	public static int AddToItemStackArray(ItemStack[] _itemStackArr, ItemStack _itemStack, int maxItemCount = -1)
	{
		int num = -1;
		int num2 = 0;
		while (num == -1 && num2 < _itemStackArr.Length)
		{
			if (_itemStackArr[num2].CanStackWith(_itemStack, false))
			{
				_itemStackArr[num2].count += _itemStack.count;
				_itemStack.count = 0;
				num = num2;
			}
			num2++;
		}
		int num3 = 0;
		while (num == -1 && num3 < _itemStackArr.Length && (maxItemCount == -1 || num3 != maxItemCount))
		{
			if (_itemStackArr[num3].IsEmpty())
			{
				_itemStackArr[num3] = _itemStack;
				num = num3;
			}
			num3++;
		}
		return num;
	}

	public ItemStack ReadOld(BinaryReader _br)
	{
		this.itemValue.ReadOld(_br);
		this.count = (int)_br.ReadInt16();
		return this;
	}

	public ItemStack Read(BinaryReader _br)
	{
		this.count = (int)_br.ReadUInt16();
		if (this.count > 0)
		{
			this.itemValue.Read(_br);
		}
		else
		{
			this.itemValue = ItemValue.None;
		}
		return this;
	}

	public ItemStack ReadDelta(BinaryReader _br, ItemStack _last)
	{
		this.itemValue.Read(_br);
		int num = (int)_br.ReadInt16();
		this.count = _last.count + num;
		return this;
	}

	public void Write(BinaryWriter _bw)
	{
		int num = this.count;
		if (num > 65535)
		{
			num = 65535;
		}
		_bw.Write((ushort)num);
		if (this.count != 0)
		{
			this.itemValue.Write(_bw);
		}
	}

	public void WriteDelta(BinaryWriter _bw, ItemStack _last)
	{
		this.itemValue.Write(_bw);
		_bw.Write((short)(this.count - _last.count));
		_last.count += this.count - _last.count;
	}

	public static ItemStack[] ReadArray(BinaryReader _br)
	{
		int num = (int)_br.ReadInt16();
		if (num == -1)
		{
			return null;
		}
		ItemStack[] array = ItemStack.CreateArray(num);
		for (int i = 0; i < num; i++)
		{
			array[i].Read(_br);
		}
		return array;
	}

	public static void WriteArray(BinaryWriter _bw, ItemStack[] _items)
	{
		if (_items == null)
		{
			_bw.Write(-1);
			return;
		}
		_bw.Write((short)_items.Length);
		for (int i = 0; i < _items.Length; i++)
		{
			_items[i].Write(_bw);
		}
	}

	public override bool Equals(object _other)
	{
		ItemStack itemStack = _other as ItemStack;
		return itemStack != null && itemStack.count == this.count && itemStack.itemValue.Equals(this.itemValue);
	}

	public override int GetHashCode()
	{
		return this.itemValue.GetHashCode() * 13 + this.count;
	}

	public int StackTransferCount(ItemStack _other)
	{
		if (_other.itemValue.type != this.itemValue.type)
		{
			return 0;
		}
		return Math.Min(ItemClass.GetForId(this.itemValue.type).Stacknumber.Value - this.count, _other.count);
	}

	public bool CanStackWith(ItemStack _other, bool allowPartialStack = false)
	{
		int num = _other.count;
		if (_other.itemValue == null || this.itemValue == null || _other.itemValue.type != this.itemValue.type || (this.itemValue.type < Block.ItemsStartHere && !(_other.itemValue.TextureFullArray == this.itemValue.TextureFullArray) && !this.itemValue.IsShapeHelperBlock))
		{
			return false;
		}
		if (!allowPartialStack)
		{
			return this.CanStack(_other.count);
		}
		return this.CanStackPartly(ref num);
	}

	public bool CanStack(int _count)
	{
		return this.itemValue.type == 0 || ItemClass.GetForId(this.itemValue.type).Stacknumber.Value >= _count + this.count;
	}

	public bool CanStackPartlyWith(ItemStack _other, out int _count)
	{
		_count = _other.count;
		return this.CanStackWith(_other, true) && this.CanStackPartly(ref _count);
	}

	public bool CanStackPartly(ref int _count)
	{
		if (this.itemValue.type == 0)
		{
			return false;
		}
		_count = Utils.FastMin(ItemClass.GetForId(this.itemValue.type).Stacknumber.Value - this.count, _count);
		return _count > 0;
	}

	public void Deactivate()
	{
		ItemClass forId = ItemClass.GetForId(this.itemValue.type);
		if (forId != null)
		{
			forId.Deactivate(this.itemValue);
		}
	}

	public static ItemStack[] Clone(IList<ItemStack> _itemStackArr)
	{
		if (_itemStackArr == null)
		{
			return null;
		}
		ItemStack[] array = new ItemStack[_itemStackArr.Count];
		for (int i = 0; i < array.Length; i++)
		{
			ItemStack itemStack = _itemStackArr[i];
			array[i] = ((itemStack != null) ? itemStack.Clone() : ItemStack.Empty);
		}
		return array;
	}

	public static ItemStack[] Clone(ItemStack[] _itemStackArr, int _startIdx, int _length)
	{
		if (_itemStackArr == null)
		{
			return null;
		}
		ItemStack[] array = new ItemStack[_length];
		for (int i = 0; i < _length; i++)
		{
			ItemStack itemStack = ((_startIdx + i < _itemStackArr.Length) ? _itemStackArr[_startIdx + i] : null);
			array[i] = ((itemStack != null) ? itemStack.Clone() : ItemStack.Empty);
		}
		return array;
	}

	public static ItemStack FromString(string _s)
	{
		ItemStack empty = ItemStack.Empty;
		int num = _s.IndexOf("=");
		if (num >= 0)
		{
			int num2 = 0;
			if (int.TryParse(_s.Substring(num + 1), out num2))
			{
				empty.count = num2;
			}
			_s = _s.Substring(0, num);
		}
		else
		{
			empty.count = 1;
		}
		empty.itemValue = ItemClass.GetItem(_s, false);
		return empty;
	}

	public override string ToString()
	{
		ItemValue itemValue = this.itemValue;
		return ((itemValue != null) ? itemValue.ToString() : null) + " cnt=" + this.count.ToString();
	}

	public static bool IsEmpty(ItemStack[] _slots)
	{
		if (_slots == null || _slots.Length == 0)
		{
			return true;
		}
		for (int i = 0; i < _slots.Length; i++)
		{
			if (!_slots[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public bool CanMoveTo(XUiC_ItemStack.StackLocationTypes locationType)
	{
		bool flag = true;
		ItemClass itemClass = this.itemValue.ItemClass;
		if (itemClass != null)
		{
			flag = itemClass.CanMoveToLocation(locationType);
		}
		return flag;
	}

	public ItemValue itemValue;

	public int count;

	public enum EnumDragType
	{
		DragTypeStart,
		DragTypeAdd,
		DragTypeExchange,
		DragTypeOther
	}
}
