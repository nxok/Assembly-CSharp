using System;
using System.Collections;
using System.Collections.Generic;

public class ReadOnlyDictionaryWrapper<TKey, TValueIn, TValueOut> : IReadOnlyDictionary<TKey, TValueOut>, IEnumerable<KeyValuePair<TKey, TValueOut>>, IEnumerable, IReadOnlyCollection<KeyValuePair<TKey, TValueOut>> where TValueIn : TValueOut
{
	public ReadOnlyDictionaryWrapper(IReadOnlyDictionary<TKey, TValueIn> dict)
	{
		this.m_dict = dict;
	}

	public IEnumerator<KeyValuePair<TKey, TValueOut>> GetEnumerator()
	{
		foreach (KeyValuePair<TKey, TValueIn> keyValuePair in this.m_dict)
		{
			TKey tkey;
			TValueIn tvalueIn;
			keyValuePair.Deconstruct(out tkey, out tvalueIn);
			TKey tkey2 = tkey;
			TValueIn tvalueIn2 = tvalueIn;
			yield return new KeyValuePair<TKey, TValueOut>(tkey2, (TValueOut)((object)tvalueIn2));
		}
		IEnumerator<KeyValuePair<TKey, TValueIn>> enumerator = null;
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return this.m_dict.GetEnumerator();
	}

	public int Count
	{
		get
		{
			return this.m_dict.Count;
		}
	}

	public bool ContainsKey(TKey key)
	{
		return this.m_dict.ContainsKey(key);
	}

	public bool TryGetValue(TKey key, out TValueOut value)
	{
		TValueIn tvalueIn;
		bool flag = this.m_dict.TryGetValue(key, out tvalueIn);
		value = (TValueOut)((object)tvalueIn);
		return flag;
	}

	public TValueOut this[TKey key]
	{
		get
		{
			return (TValueOut)((object)this.m_dict[key]);
		}
	}

	public IEnumerable<TKey> Keys
	{
		get
		{
			return this.m_dict.Keys;
		}
	}

	public IEnumerable<TValueOut> Values
	{
		get
		{
			foreach (TValueIn tvalueIn in this.m_dict.Values)
			{
				yield return (TValueOut)((object)tvalueIn);
			}
			IEnumerator<TValueIn> enumerator = null;
			yield break;
			yield break;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IReadOnlyDictionary<TKey, TValueIn> m_dict;
}
