using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public sealed class ManyToManyDictionary<TKey, TValue>
{
	public ManyToManyDictionary()
	{
		this.m_keyToValues = new Dictionary<ManyToManyDictionary<TKey, TValue>.Entry<TKey>, HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>>>();
		this.m_valueToKeys = new Dictionary<ManyToManyDictionary<TKey, TValue>.Entry<TValue>, HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>>>();
	}

	public IEnumerable<TKey> Keys
	{
		get
		{
			foreach (ManyToManyDictionary<TKey, TValue>.Entry<TKey> entry in this.m_keyToValues.Keys)
			{
				yield return entry.Value;
			}
			Dictionary<ManyToManyDictionary<TKey, TValue>.Entry<TKey>, HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>>>.KeyCollection.Enumerator enumerator = default(Dictionary<ManyToManyDictionary<TKey, TValue>.Entry<TKey>, HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>>>.KeyCollection.Enumerator);
			yield break;
			yield break;
		}
	}

	public IEnumerable<TValue> Values
	{
		get
		{
			foreach (ManyToManyDictionary<TKey, TValue>.Entry<TValue> entry in this.m_valueToKeys.Keys)
			{
				yield return entry.Value;
			}
			Dictionary<ManyToManyDictionary<TKey, TValue>.Entry<TValue>, HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>>>.KeyCollection.Enumerator enumerator = default(Dictionary<ManyToManyDictionary<TKey, TValue>.Entry<TValue>, HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>>>.KeyCollection.Enumerator);
			yield break;
			yield break;
		}
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
			return this.m_valueToKeys.Count;
		}
	}

	public void Clear()
	{
		this.m_keyToValues.Clear();
		this.m_valueToKeys.Clear();
	}

	public void Add(TKey key, TValue value)
	{
		ManyToManyDictionary<TKey, TValue>.Entry<TKey> entry = new ManyToManyDictionary<TKey, TValue>.Entry<TKey>(key);
		ManyToManyDictionary<TKey, TValue>.Entry<TValue> entry2 = new ManyToManyDictionary<TKey, TValue>.Entry<TValue>(value);
		HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet;
		if (!this.m_keyToValues.TryGetValue(entry, out hashSet))
		{
			this.m_keyToValues.Add(entry, hashSet = new HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>>());
		}
		HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>> hashSet2;
		if (!this.m_valueToKeys.TryGetValue(entry2, out hashSet2))
		{
			this.m_valueToKeys.Add(entry2, hashSet2 = new HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>>());
		}
		hashSet.Add(entry2);
		hashSet2.Add(entry);
	}

	public bool Contains(TKey key, TValue value)
	{
		HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet;
		return this.m_keyToValues.TryGetValue(new ManyToManyDictionary<TKey, TValue>.Entry<TKey>(key), out hashSet) && hashSet.Contains(new ManyToManyDictionary<TKey, TValue>.Entry<TValue>(value));
	}

	public bool ContainsKey(TKey key)
	{
		return this.m_keyToValues.ContainsKey(new ManyToManyDictionary<TKey, TValue>.Entry<TKey>(key));
	}

	public bool ContainsValue(TValue value)
	{
		return this.m_valueToKeys.ContainsKey(new ManyToManyDictionary<TKey, TValue>.Entry<TValue>(value));
	}

	public bool Remove(TKey key, TValue value)
	{
		ManyToManyDictionary<TKey, TValue>.Entry<TKey> entry = new ManyToManyDictionary<TKey, TValue>.Entry<TKey>(key);
		ManyToManyDictionary<TKey, TValue>.Entry<TValue> entry2 = new ManyToManyDictionary<TKey, TValue>.Entry<TValue>(value);
		HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet;
		if (!this.m_keyToValues.TryGetValue(entry, out hashSet))
		{
			return false;
		}
		HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>> hashSet2;
		if (!this.m_valueToKeys.TryGetValue(entry2, out hashSet2))
		{
			return false;
		}
		bool flag = hashSet.Remove(entry2);
		bool flag2 = hashSet2.Remove(entry);
		if (hashSet.Count == 0)
		{
			this.m_keyToValues.Remove(entry);
		}
		if (hashSet2.Count == 0)
		{
			this.m_valueToKeys.Remove(entry2);
		}
		return flag && flag2;
	}

	public bool RemoveByKey(TKey key)
	{
		ManyToManyDictionary<TKey, TValue>.Entry<TKey> entry = new ManyToManyDictionary<TKey, TValue>.Entry<TKey>(key);
		HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet;
		if (!this.m_keyToValues.Remove(entry, out hashSet))
		{
			return false;
		}
		foreach (ManyToManyDictionary<TKey, TValue>.Entry<TValue> entry2 in hashSet)
		{
			HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>> hashSet2;
			if (this.m_valueToKeys.TryGetValue(entry2, out hashSet2))
			{
				hashSet2.Remove(entry);
				if (hashSet2.Count == 0)
				{
					this.m_valueToKeys.Remove(entry2);
				}
			}
		}
		return true;
	}

	public bool RemoveByValue(TValue value)
	{
		ManyToManyDictionary<TKey, TValue>.Entry<TValue> entry = new ManyToManyDictionary<TKey, TValue>.Entry<TValue>(value);
		HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>> hashSet;
		if (!this.m_valueToKeys.Remove(entry, out hashSet))
		{
			return false;
		}
		foreach (ManyToManyDictionary<TKey, TValue>.Entry<TKey> entry2 in hashSet)
		{
			HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet2;
			if (this.m_keyToValues.TryGetValue(entry2, out hashSet2))
			{
				hashSet2.Remove(entry);
				if (hashSet2.Count == 0)
				{
					this.m_keyToValues.Remove(entry2);
				}
			}
		}
		return true;
	}

	public bool TryGetByKey(TKey key, out IReadOnlyCollection<TValue> values)
	{
		HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>> hashSet;
		if (!this.m_keyToValues.TryGetValue(new ManyToManyDictionary<TKey, TValue>.Entry<TKey>(key), out hashSet))
		{
			values = null;
			return false;
		}
		values = new ManyToManyDictionary<TKey, TValue>.ValuesView(hashSet);
		return true;
	}

	public bool TryGetByValue(TValue value, out IReadOnlyCollection<TKey> keys)
	{
		HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>> hashSet;
		if (!this.m_valueToKeys.TryGetValue(new ManyToManyDictionary<TKey, TValue>.Entry<TValue>(value), out hashSet))
		{
			keys = null;
			return false;
		}
		keys = new ManyToManyDictionary<TKey, TValue>.KeysView(hashSet);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<ManyToManyDictionary<TKey, TValue>.Entry<TKey>, HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>>> m_keyToValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<ManyToManyDictionary<TKey, TValue>.Entry<TValue>, HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>>> m_valueToKeys;

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Entry<T>
	{
		public Entry(T value)
		{
			this.Value = value;
			this.stableHash = (ManyToManyDictionary<TKey, TValue>.Entry<T>.s_isRefType ? ((value == null) ? 0 : RuntimeHelpers.GetHashCode(value)) : EqualityComparer<T>.Default.GetHashCode(value));
		}

		public override int GetHashCode()
		{
			return this.stableHash;
		}

		public override bool Equals(object obj)
		{
			ManyToManyDictionary<TKey, TValue>.Entry<T> entry = obj as ManyToManyDictionary<TKey, TValue>.Entry<T>;
			if (entry == null)
			{
				return false;
			}
			if (ManyToManyDictionary<TKey, TValue>.Entry<T>.s_isRefType)
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
		public ValuesView(HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>> set)
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
			foreach (ManyToManyDictionary<TKey, TValue>.Entry<TValue> entry in this.m_set)
			{
				yield return entry.Value;
			}
			HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>>.Enumerator enumerator = default(HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>>.Enumerator);
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TValue>> m_set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class KeysView : IReadOnlyCollection<TKey>, IEnumerable<TKey>, IEnumerable
	{
		public KeysView(HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>> set)
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

		public IEnumerator<TKey> GetEnumerator()
		{
			foreach (ManyToManyDictionary<TKey, TValue>.Entry<TKey> entry in this.m_set)
			{
				yield return entry.Value;
			}
			HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>>.Enumerator enumerator = default(HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>>.Enumerator);
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<ManyToManyDictionary<TKey, TValue>.Entry<TKey>> m_set;
	}
}
