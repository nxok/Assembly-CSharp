using System;
using System.IO;

public class InventoryOperation
{
	public static InventoryOperation CreateSetAbsolute(ItemStack _stack, int _index)
	{
		return new InventoryOperation
		{
			Operation = InventoryOperation.EnumOperation.SetAbsolute,
			Stack = _stack.Clone(),
			Index = _index
		};
	}

	public static InventoryOperation CreateSetRelative(ItemStack _stack, int _index)
	{
		return new InventoryOperation
		{
			Operation = InventoryOperation.EnumOperation.SetRelative,
			Stack = _stack.Clone(),
			Index = _index
		};
	}

	public static InventoryOperation CreateSetAll(ItemStack[] _newStacks)
	{
		return new InventoryOperation
		{
			Operation = InventoryOperation.EnumOperation.SetAll,
			NewStacks = ItemStack.Clone(_newStacks)
		};
	}

	public static InventoryOperation Read(BinaryReader _br)
	{
		switch (_br.ReadInt16())
		{
		case 0:
		{
			ItemStack itemStack = ItemStack.Empty.Read(_br);
			int num = _br.ReadInt32();
			return InventoryOperation.CreateSetAbsolute(itemStack, num);
		}
		case 1:
		{
			ItemStack itemStack2 = ItemStack.Empty.Read(_br);
			int num2 = _br.ReadInt32();
			return InventoryOperation.CreateSetRelative(itemStack2, num2);
		}
		case 2:
			return InventoryOperation.CreateSetAll(ItemStack.ReadArray(_br));
		default:
			return null;
		}
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write((short)this.Operation);
		InventoryOperation.EnumOperation operation = this.Operation;
		if (operation <= InventoryOperation.EnumOperation.SetRelative)
		{
			this.Stack.Write(_bw);
			_bw.Write(this.Index);
			return;
		}
		if (operation != InventoryOperation.EnumOperation.SetAll)
		{
			return;
		}
		ItemStack.WriteArray(_bw, this.NewStacks);
	}

	public InventoryOperation.EnumOperation Operation;

	public ItemStack Stack;

	public int Index;

	public ItemStack[] NewStacks;

	public enum EnumOperation
	{
		SetAbsolute,
		SetRelative,
		SetAll
	}
}
