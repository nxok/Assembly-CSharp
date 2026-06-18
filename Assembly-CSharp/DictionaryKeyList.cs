using System;
using System.Collections.Generic;

public class DictionaryKeyList<T, S>
{
	public void Add(T _key, S _value)
	{
		this.dict.Add(_key, _value);
		this.list.Add(_key);
	}

	public void Remove(T _key)
	{
		this.list.Remove(_key);
		this.dict.Remove(_key);
	}

	public void Replace(T _key, S _value)
	{
		if (this.dict.ContainsKey(_key))
		{
			this.Remove(_key);
		}
		this.Add(_key, _value);
	}

	public void Clear()
	{
		this.list.Clear();
		this.dict.Clear();
	}

	public Dictionary<T, S> dict = new Dictionary<T, S>();

	public List<T> list = new List<T>();
}
