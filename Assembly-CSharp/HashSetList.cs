using System;
using System.Collections.Generic;

public class HashSetList<T>
{
	public void Add(T _value)
	{
		if (this.hashSet.Add(_value))
		{
			this.list.Add(_value);
		}
	}

	public void Remove(T _value)
	{
		if (this.hashSet.Remove(_value))
		{
			this.list.Remove(_value);
		}
	}

	public void Clear()
	{
		this.list.Clear();
		this.hashSet.Clear();
	}

	public HashSet<T> hashSet = new HashSet<T>();

	public List<T> list = new List<T>();
}
