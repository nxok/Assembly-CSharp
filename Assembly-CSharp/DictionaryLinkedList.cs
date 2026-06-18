using System;
using System.Collections.Generic;

public class DictionaryLinkedList<T, S>
{
	public DictionaryLinkedList()
	{
		this.dict = new Dictionary<T, S>();
	}

	public DictionaryLinkedList(IEqualityComparer<T> _comparer)
	{
		this.dict = new Dictionary<T, S>(_comparer);
	}

	public void Add(T _key, S _value)
	{
		this.dict.Add(_key, _value);
		LinkedListNode<S> linkedListNode = this.list.AddLast(_value);
		this.indices.Add(_key, linkedListNode);
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
		if (this.dict.ContainsKey(_key))
		{
			S s = this.dict[_key];
			this.dict.Remove(_key);
			LinkedListNode<S> linkedListNode = this.indices[_key];
			this.list.Remove(linkedListNode);
			this.indices.Remove(_key);
		}
	}

	public void Clear()
	{
		this.list.Clear();
		this.dict.Clear();
		this.indices.Clear();
	}

	public int Count
	{
		get
		{
			return this.list.Count;
		}
	}

	public Dictionary<T, S> dict;

	public LinkedList<S> list = new LinkedList<S>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<T, LinkedListNode<S>> indices = new Dictionary<T, LinkedListNode<S>>();
}
