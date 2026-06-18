using System;
using System.Collections.Generic;

public class DictionaryKeyValueList<T, S>
{
	public void Add(T _key, S _value)
	{
		this.dict.Add(_key, _value);
		this.keyList.Add(_key);
		this.valueList.Add(_value);
	}

	public void Set(T _key, S _value)
	{
		if (this.dict.ContainsKey(_key))
		{
			this.Remove(_key);
		}
		this.Add(_key, _value);
	}

	public void Remove(T _key)
	{
		int num = this.keyList.IndexOf(_key);
		if (num >= 0)
		{
			this.keyList.RemoveAt(num);
			this.valueList.RemoveAt(num);
			this.dict.Remove(_key);
		}
	}

	public void Clear()
	{
		this.keyList.Clear();
		this.valueList.Clear();
		this.dict.Clear();
	}

	public Dictionary<T, S> dict = new Dictionary<T, S>();

	public List<S> valueList = new List<S>();

	public List<T> keyList = new List<T>();
}
