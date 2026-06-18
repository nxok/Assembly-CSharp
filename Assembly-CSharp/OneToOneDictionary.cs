using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class OneToOneDictionary<TKey, TValue>
{
	public int Count
	{
		get
		{
			return this._forward.Count;
		}
	}

	public void Add(TKey key, TValue value)
	{
		OneToOneDictionary<TKey, TValue>.Entry<TKey> entry = new OneToOneDictionary<TKey, TValue>.Entry<TKey>(key);
		OneToOneDictionary<TKey, TValue>.Entry<TValue> entry2 = new OneToOneDictionary<TKey, TValue>.Entry<TValue>(value);
		if (this._forward.ContainsKey(entry))
		{
			throw new ArgumentException(string.Format("[TwoWayDictionary] Duplicate key: {0}", key));
		}
		if (this._reverse.ContainsKey(entry2))
		{
			throw new ArgumentException(string.Format("[TwoWayDictionary] Duplicate value: {0}", value));
		}
		this._forward[entry] = entry2;
		this._reverse[entry2] = entry;
	}

	public bool RemoveByKey(TKey key)
	{
		OneToOneDictionary<TKey, TValue>.Entry<TKey> entry = new OneToOneDictionary<TKey, TValue>.Entry<TKey>(key);
		OneToOneDictionary<TKey, TValue>.Entry<TValue> entry2;
		if (this._forward.Remove(entry, out entry2))
		{
			this._reverse.Remove(entry2);
			return true;
		}
		return false;
	}

	public bool RemoveByValue(TValue value)
	{
		OneToOneDictionary<TKey, TValue>.Entry<TValue> entry = new OneToOneDictionary<TKey, TValue>.Entry<TValue>(value);
		OneToOneDictionary<TKey, TValue>.Entry<TKey> entry2;
		if (this._reverse.Remove(entry, out entry2))
		{
			this._forward.Remove(entry2);
			return true;
		}
		return false;
	}

	public bool TryGetByKey(TKey key, out TValue value)
	{
		OneToOneDictionary<TKey, TValue>.Entry<TKey> entry = new OneToOneDictionary<TKey, TValue>.Entry<TKey>(key);
		OneToOneDictionary<TKey, TValue>.Entry<TValue> entry2;
		if (this._forward.TryGetValue(entry, out entry2))
		{
			value = entry2.Value;
			return true;
		}
		value = default(TValue);
		return false;
	}

	public bool TryGetByValue(TValue value, out TKey key)
	{
		OneToOneDictionary<TKey, TValue>.Entry<TValue> entry = new OneToOneDictionary<TKey, TValue>.Entry<TValue>(value);
		OneToOneDictionary<TKey, TValue>.Entry<TKey> entry2;
		if (this._reverse.TryGetValue(entry, out entry2))
		{
			key = entry2.Value;
			return true;
		}
		key = default(TKey);
		return false;
	}

	public TValue GetByKey(TKey key)
	{
		return this._forward[new OneToOneDictionary<TKey, TValue>.Entry<TKey>(key)].Value;
	}

	public TKey GetByValue(TValue value)
	{
		return this._reverse[new OneToOneDictionary<TKey, TValue>.Entry<TValue>(value)].Value;
	}

	public bool ContainsKey(TKey key)
	{
		return this._forward.ContainsKey(new OneToOneDictionary<TKey, TValue>.Entry<TKey>(key));
	}

	public bool ContainsValue(TValue value)
	{
		return this._reverse.ContainsKey(new OneToOneDictionary<TKey, TValue>.Entry<TValue>(value));
	}

	public void Clear()
	{
		this._forward.Clear();
		this._reverse.Clear();
	}

	public IEnumerable<TKey> Keys
	{
		get
		{
			foreach (OneToOneDictionary<TKey, TValue>.Entry<TKey> entry in this._forward.Keys)
			{
				yield return entry.Value;
			}
			Dictionary<OneToOneDictionary<TKey, TValue>.Entry<TKey>, OneToOneDictionary<TKey, TValue>.Entry<TValue>>.KeyCollection.Enumerator enumerator = default(Dictionary<OneToOneDictionary<TKey, TValue>.Entry<TKey>, OneToOneDictionary<TKey, TValue>.Entry<TValue>>.KeyCollection.Enumerator);
			yield break;
			yield break;
		}
	}

	public IEnumerable<TValue> Values
	{
		get
		{
			foreach (OneToOneDictionary<TKey, TValue>.Entry<TValue> entry in this._forward.Values)
			{
				yield return entry.Value;
			}
			Dictionary<OneToOneDictionary<TKey, TValue>.Entry<TKey>, OneToOneDictionary<TKey, TValue>.Entry<TValue>>.ValueCollection.Enumerator enumerator = default(Dictionary<OneToOneDictionary<TKey, TValue>.Entry<TKey>, OneToOneDictionary<TKey, TValue>.Entry<TValue>>.ValueCollection.Enumerator);
			yield break;
			yield break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<OneToOneDictionary<TKey, TValue>.Entry<TKey>, OneToOneDictionary<TKey, TValue>.Entry<TValue>> _forward = new Dictionary<OneToOneDictionary<TKey, TValue>.Entry<TKey>, OneToOneDictionary<TKey, TValue>.Entry<TValue>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<OneToOneDictionary<TKey, TValue>.Entry<TValue>, OneToOneDictionary<TKey, TValue>.Entry<TKey>> _reverse = new Dictionary<OneToOneDictionary<TKey, TValue>.Entry<TValue>, OneToOneDictionary<TKey, TValue>.Entry<TKey>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class Entry<T>
	{
		public Entry(T value)
		{
			this.Value = value;
			this.stableHash = (OneToOneDictionary<TKey, TValue>.Entry<T>.s_isRefType ? ((value == null) ? 0 : RuntimeHelpers.GetHashCode(value)) : EqualityComparer<T>.Default.GetHashCode(value));
		}

		public override int GetHashCode()
		{
			return this.stableHash;
		}

		public override bool Equals(object obj)
		{
			OneToOneDictionary<TKey, TValue>.Entry<T> entry = obj as OneToOneDictionary<TKey, TValue>.Entry<T>;
			if (entry == null)
			{
				return false;
			}
			if (OneToOneDictionary<TKey, TValue>.Entry<T>.s_isRefType)
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
}
