using System;
using System.Collections.Generic;
using System.Linq;

public static class DictionaryExtension
{
	public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dic, Func<TValue, bool> predicate)
	{
		foreach (TKey tkey in dic.Keys.Where((TKey k) => predicate(dic[k])).ToList<TKey>())
		{
			dic.Remove(tkey);
		}
	}

	public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dic, Func<TKey, bool> predicate)
	{
		foreach (TKey tkey in dic.Keys.Where((TKey k) => predicate(k)).ToList<TKey>())
		{
			dic.Remove(tkey);
		}
	}

	public static void CopyTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, IDictionary<TKey, TValue> _dest, bool _overwriteExisting = false)
	{
		if (_overwriteExisting)
		{
			using (IEnumerator<KeyValuePair<TKey, TValue>> enumerator = _src.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<TKey, TValue> keyValuePair = enumerator.Current;
					_dest[keyValuePair.Key] = keyValuePair.Value;
				}
				return;
			}
		}
		foreach (KeyValuePair<TKey, TValue> keyValuePair2 in _src)
		{
			_dest.Add(keyValuePair2.Key, keyValuePair2.Value);
		}
	}

	public static void CopyKeysTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, ICollection<TKey> _dest)
	{
		foreach (KeyValuePair<TKey, TValue> keyValuePair in _src)
		{
			_dest.Add(keyValuePair.Key);
		}
	}

	public static void CopyKeysTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, TKey[] _dest)
	{
		if (_dest.Length != _src.Count)
		{
			throw new ArgumentOutOfRangeException("_dest", "Target array does not have the same size as the dictionary");
		}
		int num = 0;
		foreach (KeyValuePair<TKey, TValue> keyValuePair in _src)
		{
			_dest[num++] = keyValuePair.Key;
		}
	}

	public static void CopyValuesTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, IList<TValue> _dest)
	{
		foreach (KeyValuePair<TKey, TValue> keyValuePair in _src)
		{
			_dest.Add(keyValuePair.Value);
		}
	}

	public static void CopyValuesTo<TKey, TValue>(this IDictionary<TKey, TValue> _src, TValue[] _dest)
	{
		if (_dest.Length != _src.Count)
		{
			throw new ArgumentOutOfRangeException("_dest", "Target array does not have the same size as the dictionary");
		}
		int num = 0;
		foreach (KeyValuePair<TKey, TValue> keyValuePair in _src)
		{
			_dest[num++] = keyValuePair.Value;
		}
	}

	public static bool ValuesEquals<TKey, TValue>(this IDictionary<TKey, TValue> _src, IDictionary<TKey, TValue> _other)
	{
		if (_src.Count != _other.Count)
		{
			return false;
		}
		foreach (KeyValuePair<TKey, TValue> keyValuePair in _src)
		{
			TValue tvalue;
			if (!_other.TryGetValue(keyValuePair.Key, out tvalue))
			{
				return false;
			}
			if (!tvalue.Equals(keyValuePair.Value))
			{
				return false;
			}
		}
		return true;
	}
}
