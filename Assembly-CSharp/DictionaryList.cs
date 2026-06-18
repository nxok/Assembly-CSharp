using System;
using System.Collections.Generic;

public class DictionaryList<T, S>
{
	public DictionaryList()
	{
		this.dict = new Dictionary<T, S>();
	}

	public DictionaryList(IEqualityComparer<T> _comparer)
	{
		this.dict = new Dictionary<T, S>(_comparer);
	}

	public void Add(T _key, S _value)
	{
		this.dict.Add(_key, _value);
		this.list.Add(_value);
	}

	public void Set(T _key, S _value)
	{
		if (this.dict.ContainsKey(_key))
		{
			this.Remove(_key);
		}
		this.Add(_key, _value);
	}

	public bool Remove(T _key)
	{
		if (this.dict.ContainsKey(_key))
		{
			S s = this.dict[_key];
			this.dict.Remove(_key);
			this.list.Remove(s);
			return true;
		}
		return false;
	}

	public S Get(T _key)
	{
		S s;
		if (this.dict.TryGetValue(_key, out s))
		{
			return s;
		}
		return default(S);
	}

	public void Clear()
	{
		this.list.Clear();
		this.dict.Clear();
	}

	public int Count
	{
		get
		{
			return this.list.Count;
		}
	}

	public Dictionary<T, S> dict;

	public List<S> list = new List<S>();
}
