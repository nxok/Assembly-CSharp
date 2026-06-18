using System;
using System.Collections.Generic;
using UniLinq;

public static class StackSortUtil
{
	public static ItemStack[] CombineAndSortStacks(ItemStack[] _stacks, int _ignoreSlots = 0, PackedBoolArray _ignoredSlots = null)
	{
		for (int i = _ignoreSlots; i < _stacks.Length - 1; i++)
		{
			if (_stacks[i].IsEmpty() && !StackSortUtil.IsIgnoredSlot(_ignoreSlots, _ignoredSlots, i))
			{
				for (int j = i + 1; j < _stacks.Length; j++)
				{
					if (!_stacks[j].IsEmpty() && !StackSortUtil.IsIgnoredSlot(_ignoreSlots, _ignoredSlots, j))
					{
						_stacks[i] = _stacks[j];
						_stacks[j] = ItemStack.Empty;
						break;
					}
				}
			}
			if (!_stacks[i].IsEmpty())
			{
				ItemClass itemClass = _stacks[i].itemValue.ItemClass;
				int num = itemClass.Stacknumber.Value - _stacks[i].count;
				if (!itemClass.HasQuality && num != 0)
				{
					for (int k = i + 1; k < _stacks.Length; k++)
					{
						if (!StackSortUtil.IsIgnoredSlot(_ignoreSlots, _ignoredSlots, k))
						{
							if (_stacks[i].itemValue.type == _stacks[k].itemValue.type)
							{
								int num2 = Utils.FastMin(_stacks[k].count, num);
								_stacks[i].count += num2;
								_stacks[k].count -= num2;
								num -= num2;
								if (_stacks[k].count == 0)
								{
									_stacks[k] = ItemStack.Empty;
								}
							}
							if (num == 0)
							{
								break;
							}
						}
					}
				}
			}
		}
		_stacks = StackSortUtil.SortStacks(_stacks, _ignoreSlots, _ignoredSlots);
		return _stacks;
	}

	public static ItemStack[] SortStacks(ItemStack[] _stacks, int _ignoreSlots = 0, PackedBoolArray _ignoredSlots = null)
	{
		ItemStack[] array = new ItemStack[_stacks.Length];
		for (int i = 0; i < _stacks.Length; i++)
		{
			if (StackSortUtil.IsIgnoredSlot(_ignoreSlots, _ignoredSlots, i))
			{
				array[i] = _stacks[i];
				_stacks[i] = ItemStack.Empty;
			}
		}
		IEnumerator<ItemStack> enumerator = _stacks.OrderBy(new Func<ItemStack, string>(StackSortUtil.getGroup)).ThenBy(new Func<ItemStack, string>(StackSortUtil.getName)).ThenBy((ItemStack _stack) => _stack.itemValue.Quality)
			.ThenByDescending((ItemStack _stack) => _stack.itemValue.UseTimes)
			.GetEnumerator();
		bool flag = enumerator.MoveNext();
		int j = 0;
		while (j < array.Length && flag)
		{
			if (!StackSortUtil.IsIgnoredSlot(_ignoreSlots, _ignoredSlots, j))
			{
				array[j] = enumerator.Current;
				flag = enumerator.MoveNext();
			}
			j++;
		}
		while (j < array.Length)
		{
			array[j] = ItemStack.Empty;
			j++;
		}
		enumerator.Dispose();
		return array;
	}

	public static bool IsIgnoredSlot(int _ignoreSlots, PackedBoolArray _ignoredSlots, int _slot)
	{
		return _slot < _ignoreSlots || (_ignoredSlots != null && _slot < _ignoredSlots.Length && _ignoredSlots[_slot]);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getGroup(ItemStack _stack)
	{
		ItemClass forId = ItemClass.GetForId(_stack.itemValue.type);
		if (forId == null)
		{
			return "zzzzz";
		}
		if (forId.IsBlock())
		{
			return "aaaaa";
		}
		return forId.Groups[0];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getName(ItemStack _stack)
	{
		ItemClass forId = ItemClass.GetForId(_stack.itemValue.type);
		if (forId == null)
		{
			return "zzzzz";
		}
		return forId.GetLocalizedItemName();
	}
}
