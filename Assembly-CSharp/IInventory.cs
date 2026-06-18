using System;
using System.Runtime.CompilerServices;

public interface IInventory
{
	bool AddItem(ItemStack _itemStack);

	[return: TupleElementNames(new string[] { "anyMoved", "allMoved" })]
	ValueTuple<bool, bool> TryStackItem(int _startIndex, ItemStack _itemStack);

	bool HasItem(ItemValue _item);
}
