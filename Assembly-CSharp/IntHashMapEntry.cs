using System;

[PublicizedFrom(EAccessModifier.Internal)]
public class IntHashMapEntry
{
	public IntHashMapEntry(int _i1, int _i2, object _obj, IntHashMapEntry _entry)
	{
		this.valueEntry = _obj;
		this.nextEntry = _entry;
		this.hashEntry = _i2;
		this.slotHash = _i1;
	}

	public int getHash()
	{
		return this.hashEntry;
	}

	public object getValue()
	{
		return this.valueEntry;
	}

	public bool equals(object _obj)
	{
		if (!(_obj is IntHashMapEntry))
		{
			return false;
		}
		IntHashMapEntry intHashMapEntry = (IntHashMapEntry)_obj;
		int hash = this.getHash();
		int hash2 = intHashMapEntry.getHash();
		if (hash == hash2)
		{
			object value = this.getValue();
			object value2 = intHashMapEntry.getValue();
			if (value == value2 || (value != null && value.Equals(value2)))
			{
				return true;
			}
		}
		return false;
	}

	public int hashCode()
	{
		return IntHashMap.getHash(this.hashEntry);
	}

	public string toString()
	{
		string text = this.getHash().ToString();
		string text2 = "=";
		object value = this.getValue();
		return text + text2 + ((value != null) ? value.ToString() : null);
	}

	public int hashEntry;

	public object valueEntry;

	public IntHashMapEntry nextEntry;

	public int slotHash;
}
