using System;

public class IntHashMap
{
	public IntHashMap()
	{
		this.threshold = 12;
		this.slots = new IntHashMapEntry[16];
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int computeHash(int _v)
	{
		_v ^= (_v >> 20) ^ (_v >> 12);
		return _v ^ (_v >> 7) ^ (_v >> 4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int getSlotIndex(int _p1, int _p2)
	{
		return _p1 & (_p2 - 1);
	}

	public object lookup(int _p)
	{
		int num = IntHashMap.computeHash(_p);
		for (IntHashMapEntry intHashMapEntry = this.slots[IntHashMap.getSlotIndex(num, this.slots.Length)]; intHashMapEntry != null; intHashMapEntry = intHashMapEntry.nextEntry)
		{
			if (intHashMapEntry.hashEntry == _p)
			{
				return intHashMapEntry.valueEntry;
			}
		}
		return null;
	}

	public bool containsItem(int _item)
	{
		return this.lookupEntry(_item) != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IntHashMapEntry lookupEntry(int _item)
	{
		int num = IntHashMap.computeHash(_item);
		for (IntHashMapEntry intHashMapEntry = this.slots[IntHashMap.getSlotIndex(num, this.slots.Length)]; intHashMapEntry != null; intHashMapEntry = intHashMapEntry.nextEntry)
		{
			if (intHashMapEntry.hashEntry == _item)
			{
				return intHashMapEntry;
			}
		}
		return null;
	}

	public void addKey(int _key, object _value)
	{
		int num = IntHashMap.computeHash(_key);
		int slotIndex = IntHashMap.getSlotIndex(num, this.slots.Length);
		for (IntHashMapEntry intHashMapEntry = this.slots[slotIndex]; intHashMapEntry != null; intHashMapEntry = intHashMapEntry.nextEntry)
		{
			if (intHashMapEntry.hashEntry == _key)
			{
				intHashMapEntry.valueEntry = _value;
			}
		}
		this.versionStamp++;
		this.insert(num, _key, _value, slotIndex);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void grow(int _size)
	{
		if (this.slots.Length == 1073741824)
		{
			this.threshold = int.MaxValue;
			return;
		}
		IntHashMapEntry[] array = new IntHashMapEntry[_size];
		this.copyTo(array);
		this.slots = array;
		this.threshold = (int)((float)_size * 0.75f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void copyTo(IntHashMapEntry[] _other)
	{
		IntHashMapEntry[] array = this.slots;
		int num = _other.Length;
		for (int i = 0; i < array.Length; i++)
		{
			IntHashMapEntry intHashMapEntry = array[i];
			if (intHashMapEntry != null)
			{
				array[i] = null;
				do
				{
					IntHashMapEntry nextEntry = intHashMapEntry.nextEntry;
					int slotIndex = IntHashMap.getSlotIndex(intHashMapEntry.slotHash, num);
					intHashMapEntry.nextEntry = _other[slotIndex];
					_other[slotIndex] = intHashMapEntry;
					intHashMapEntry = nextEntry;
				}
				while (intHashMapEntry != null);
			}
		}
	}

	public object removeObject(int _key)
	{
		IntHashMapEntry intHashMapEntry = this.removeEntry(_key);
		if (intHashMapEntry == null)
		{
			return null;
		}
		return intHashMapEntry.valueEntry;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IntHashMapEntry removeEntry(int _key)
	{
		int slotIndex = IntHashMap.getSlotIndex(IntHashMap.computeHash(_key), this.slots.Length);
		IntHashMapEntry intHashMapEntry = this.slots[slotIndex];
		IntHashMapEntry intHashMapEntry2;
		IntHashMapEntry nextEntry;
		for (intHashMapEntry2 = intHashMapEntry; intHashMapEntry2 != null; intHashMapEntry2 = nextEntry)
		{
			nextEntry = intHashMapEntry2.nextEntry;
			if (intHashMapEntry2.hashEntry == _key)
			{
				this.versionStamp++;
				this.count--;
				if (intHashMapEntry == intHashMapEntry2)
				{
					this.slots[slotIndex] = nextEntry;
				}
				else
				{
					intHashMapEntry.nextEntry = nextEntry;
				}
				return intHashMapEntry2;
			}
			intHashMapEntry = intHashMapEntry2;
		}
		return intHashMapEntry2;
	}

	public void clearMap()
	{
		this.versionStamp++;
		IntHashMapEntry[] array = this.slots;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = null;
		}
		this.count = 0;
	}

	public void map(Action<object> callback)
	{
		for (int i = 0; i < this.slots.Length; i++)
		{
			for (IntHashMapEntry intHashMapEntry = this.slots[i]; intHashMapEntry != null; intHashMapEntry = intHashMapEntry.nextEntry)
			{
				callback(intHashMapEntry.valueEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void insert(int _i1, int _i2, object _object, int _key)
	{
		IntHashMapEntry intHashMapEntry = this.slots[_key];
		this.slots[_key] = new IntHashMapEntry(_i1, _i2, _object, intHashMapEntry);
		int num = this.count;
		this.count = num + 1;
		if (num >= this.threshold)
		{
			this.grow(2 * this.slots.Length);
		}
	}

	public static int getHash(int _v)
	{
		return IntHashMap.computeHash(_v);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IntHashMapEntry[] slots;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count;

	[PublicizedFrom(EAccessModifier.Private)]
	public int threshold;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float growFactor = 0.75f;

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int versionStamp;
}
