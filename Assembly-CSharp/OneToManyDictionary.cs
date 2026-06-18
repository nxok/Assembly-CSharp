using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public sealed class OneToManyDictionary<TKey, TValue>
{
	public OneToManyDictionary()
	{
		this.m_keyToValues = new Dictionary<OneToManyDictionary<TKey, TValue>.Entry<TKey>, HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>>>();
		this.m_valueToKey = new Dictionary<OneToManyDictionary<TKey, TValue>.Entry<TValue>, OneToManyDictionary<TKey, TValue>.Entry<TKey>>();
	}

	public IEnumerable<TKey> Keys
	{
		get
		{
			foreach (OneToManyDictionary<TKey, TValue>.Entry<TKey> entry in this.m_keyToValues.Keys)
			{
				yield return entry.Value;
			}
			Dictionary<OneToManyDictionary<TKey, TValue>.Entry<TKey>, HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>>>.KeyCollection.Enumerator enumerator = default(Dictionary<OneToManyDictionary<TKey, TValue>.Entry<TKey>, HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>>>.KeyCollection.Enumerator);
			yield break;
			yield break;
		}
	}

	public IEnumerable<TValue> Values
	{
		get
		{
			foreach (OneToManyDictionary<TKey, TValue>.Entry<TValue> entry in this.m_valueToKey.Keys)
			{
				yield return entry.Value;
			}
			Dictionary<OneToManyDictionary<TKey, TValue>.Entry<TValue>, OneToManyDictionary<TKey, TValue>.Entry<TKey>>.KeyCollection.Enumerator enumerator = default(Dictionary<OneToManyDictionary<TKey, TValue>.Entry<TValue>, OneToManyDictionary<TKey, TValue>.Entry<TKey>>.KeyCollection.Enumerator);
			yield break;
			yield break;
		}
	}

	public void Add(TKey key, TValue value)
	{
		OneToManyDictionary<TKey, TValue>.Entry<TKey> entry = new OneToManyDictionary<TKey, TValue>.Entry<TKey>(key);
		OneToManyDictionary<TKey, TValue>.Entry<TValue> entry2 = new OneToManyDictionary<TKey, TValue>.Entry<TValue>(value);
		if (this.m_valueToKey.ContainsKey(entry2))
		{
			throw new ArgumentException("Value already in dictionary.", "value");
		}
		HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet;
		if (!this.m_keyToValues.TryGetValue(entry, out hashSet))
		{
			this.m_keyToValues.Add(entry, hashSet = new HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>>());
		}
		hashSet.Add(entry2);
		this.m_valueToKey.Add(entry2, entry);
	}

	public bool ContainsKey(TKey key)
	{
		return this.m_keyToValues.ContainsKey(new OneToManyDictionary<TKey, TValue>.Entry<TKey>(key));
	}

	public bool ContainsValue(TValue value)
	{
		return this.m_valueToKey.ContainsKey(new OneToManyDictionary<TKey, TValue>.Entry<TValue>(value));
	}

	public int CountKeys
	{
		get
		{
			return this.m_keyToValues.Count;
		}
	}

	public int CountValues
	{
		get
		{
			return this.m_valueToKey.Count;
		}
	}

	public bool TryGetByKey(TKey key, out IReadOnlyCollection<TValue> values)
	{
		HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet;
		if (!this.m_keyToValues.TryGetValue(new OneToManyDictionary<TKey, TValue>.Entry<TKey>(key), out hashSet))
		{
			values = null;
			return false;
		}
		values = new OneToManyDictionary<TKey, TValue>.ValuesView(hashSet);
		return true;
	}

	public int TryGetByKey(TKey key, ICollection<TValue> valuesOut)
	{
		HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet;
		if (!this.m_keyToValues.TryGetValue(new OneToManyDictionary<TKey, TValue>.Entry<TKey>(key), out hashSet))
		{
			return 0;
		}
		int num = 0;
		foreach (OneToManyDictionary<TKey, TValue>.Entry<TValue> entry in hashSet)
		{
			valuesOut.Add(entry.Value);
			num++;
		}
		return num;
	}

	public unsafe int TryGetByKey(TKey key, Span<TValue> valuesOut)
	{
		HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet;
		if (!this.m_keyToValues.TryGetValue(new OneToManyDictionary<TKey, TValue>.Entry<TKey>(key), out hashSet))
		{
			return 0;
		}
		int num = 0;
		foreach (OneToManyDictionary<TKey, TValue>.Entry<TValue> entry in hashSet)
		{
			if (num >= valuesOut.Length)
			{
				break;
			}
			*valuesOut[num] = entry.Value;
			num++;
		}
		return num;
	}

	public bool TryGetByValue(TValue value, out TKey key)
	{
		OneToManyDictionary<TKey, TValue>.Entry<TKey> entry;
		if (this.m_valueToKey.TryGetValue(new OneToManyDictionary<TKey, TValue>.Entry<TValue>(value), out entry))
		{
			key = entry.Value;
			return true;
		}
		key = default(TKey);
		return false;
	}

	public bool RemoveByKey(TKey key)
	{
		OneToManyDictionary<TKey, TValue>.Entry<TKey> entry = new OneToManyDictionary<TKey, TValue>.Entry<TKey>(key);
		HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet;
		if (!this.m_keyToValues.TryGetValue(entry, out hashSet))
		{
			return false;
		}
		foreach (OneToManyDictionary<TKey, TValue>.Entry<TValue> entry2 in hashSet)
		{
			this.m_valueToKey.Remove(entry2);
		}
		this.m_keyToValues.Remove(entry);
		return true;
	}

	public bool RemoveByValue(TValue value)
	{
		OneToManyDictionary<TKey, TValue>.Entry<TValue> entry = new OneToManyDictionary<TKey, TValue>.Entry<TValue>(value);
		OneToManyDictionary<TKey, TValue>.Entry<TKey> entry2;
		if (!this.m_valueToKey.TryGetValue(entry, out entry2))
		{
			return false;
		}
		HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet;
		if (!this.m_keyToValues.TryGetValue(entry2, out hashSet))
		{
			return false;
		}
		this.m_valueToKey.Remove(entry);
		hashSet.Remove(entry);
		if (hashSet.Count == 0)
		{
			this.m_keyToValues.Remove(entry2);
		}
		return true;
	}

	public void Clear()
	{
		this.m_keyToValues.Clear();
		this.m_valueToKey.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<OneToManyDictionary<TKey, TValue>.Entry<TKey>, HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>>> m_keyToValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<OneToManyDictionary<TKey, TValue>.Entry<TValue>, OneToManyDictionary<TKey, TValue>.Entry<TKey>> m_valueToKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Entry<T>
	{
		public Entry(T value)
		{
			this.Value = value;
			this.stableHash = (OneToManyDictionary<TKey, TValue>.Entry<T>.s_isRefType ? ((value == null) ? 0 : RuntimeHelpers.GetHashCode(value)) : EqualityComparer<T>.Default.GetHashCode(value));
		}

		public override int GetHashCode()
		{
			return this.stableHash;
		}

		public override bool Equals(object obj)
		{
			OneToManyDictionary<TKey, TValue>.Entry<T> entry = obj as OneToManyDictionary<TKey, TValue>.Entry<T>;
			if (entry == null)
			{
				return false;
			}
			if (OneToManyDictionary<TKey, TValue>.Entry<T>.s_isRefType)
			{
				return this.Value == entry.Value;
			}
			return EqualityComparer<T>.Default.Equals(this.Value, entry.Value);
		}

		public readonly T Value;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly int stableHash;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly bool s_isRefType = !typeof(T).IsValueType;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class ValuesView : IReadOnlyCollection<TValue>, IEnumerable<TValue>, IEnumerable
	{
		public ValuesView(HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>> set)
		{
			this.m_set = set;
		}

		public int Count
		{
			get
			{
				return this.m_set.Count;
			}
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			foreach (OneToManyDictionary<TKey, TValue>.Entry<TValue> entry in this.m_set)
			{
				yield return entry.Value;
			}
			HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>>.Enumerator enumerator = default(HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>>.Enumerator);
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<OneToManyDictionary<TKey, TValue>.Entry<TValue>> m_set;
	}
}
