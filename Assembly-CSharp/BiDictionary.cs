using System;
using System.Collections.Generic;

public sealed class BiDictionary<TKey, TValue>
{
	public void Add(TKey key, TValue value)
	{
		if (this.m_keyToValue.ContainsKey(key))
		{
			throw new ArgumentException("Key already in dictionary.", "key");
		}
		if (this.m_valueToKey.ContainsKey(value))
		{
			throw new ArgumentException("Value already in dictionary.", "value");
		}
		this.m_keyToValue.Add(key, value);
		this.m_valueToKey.Add(value, key);
	}

	public bool ContainsKey(TKey key)
	{
		return this.m_keyToValue.ContainsKey(key);
	}

	public bool ContainsValue(TValue value)
	{
		return this.m_valueToKey.ContainsKey(value);
	}

	public bool TryGetByKey(TKey key, out TValue value)
	{
		return this.m_keyToValue.TryGetValue(key, out value);
	}

	public bool TryGetByValue(TValue value, out TKey key)
	{
		return this.m_valueToKey.TryGetValue(value, out key);
	}

	public bool RemoveByKey(TKey key)
	{
		TValue tvalue;
		if (!this.m_keyToValue.Remove(key, out tvalue))
		{
			return false;
		}
		this.m_valueToKey.Remove(tvalue);
		return true;
	}

	public bool RemoveByValue(TValue value)
	{
		TKey tkey;
		if (!this.m_valueToKey.Remove(value, out tkey))
		{
			return false;
		}
		this.m_keyToValue.Remove(tkey);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<TKey, TValue> m_keyToValue = new Dictionary<TKey, TValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<TValue, TKey> m_valueToKey = new Dictionary<TValue, TKey>();
}
