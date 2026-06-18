using System;
using System.Collections;
using System.Collections.Generic;

public sealed class DictionaryDebugWrapper<TKey, TValue> : CollectionDebugWrapper<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	public DictionaryDebugWrapper()
		: this(new Dictionary<TKey, TValue>())
	{
	}

	public DictionaryDebugWrapper(IDictionary<TKey, TValue> dictionary)
		: this(null, dictionary)
	{
	}

	public DictionaryDebugWrapper(DebugWrapper parent, IDictionary<TKey, TValue> dictionary)
		: base(parent, dictionary)
	{
		this.m_dictionary = dictionary;
		this.Keys = new CollectionDebugWrapper<TKey>(this, this.m_dictionary.Keys);
		this.Values = new CollectionDebugWrapper<TValue>(this, this.m_dictionary.Values);
	}

	public void Add(TKey key, TValue value)
	{
		using (base.DebugReadWriteScope())
		{
			this.m_dictionary.Add(key, value);
		}
	}

	public bool ContainsKey(TKey key)
	{
		bool flag;
		using (base.DebugReadScope())
		{
			flag = this.m_dictionary.ContainsKey(key);
		}
		return flag;
	}

	public bool Remove(TKey key)
	{
		bool flag;
		using (base.DebugReadWriteScope())
		{
			flag = this.m_dictionary.Remove(key);
		}
		return flag;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		bool flag;
		using (base.DebugReadScope())
		{
			flag = this.m_dictionary.TryGetValue(key, out value);
		}
		return flag;
	}

	public TValue this[TKey key]
	{
		get
		{
			TValue tvalue;
			using (base.DebugReadScope())
			{
				tvalue = this.m_dictionary[key];
			}
			return tvalue;
		}
		set
		{
			using (base.DebugReadWriteScope())
			{
				this.m_dictionary[key] = value;
			}
		}
	}

	public ICollection<TKey> Keys { get; }

	public ICollection<TValue> Values { get; }

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IDictionary<TKey, TValue> m_dictionary;
}
