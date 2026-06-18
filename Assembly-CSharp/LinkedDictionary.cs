using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LinkedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable, IReadOnlyDictionary<TKey, TValue>, IReadOnlyCollection<KeyValuePair<TKey, TValue>>
{
	public LinkedDictionary()
	{
		this.m_dictionary = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();
		this.m_list = new LinkedList<KeyValuePair<TKey, TValue>>();
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return this.m_list.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.Add(KeyValuePair<TKey, TValue> item)
	{
		this.Add(item.Key, item.Value);
	}

	public void Clear()
	{
		this.m_dictionary.Clear();
		this.m_list.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.Contains(KeyValuePair<TKey, TValue> item)
	{
		LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode;
		return this.m_dictionary.TryGetValue(item.Key, out linkedListNode) && EqualityComparer<TValue>.Default.Equals(linkedListNode.Value.Value, item.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0 || arrayIndex > array.Length)
		{
			throw new ArgumentOutOfRangeException("arrayIndex");
		}
		if (array.Length - arrayIndex < this.m_list.Count)
		{
			throw new ArgumentException("Not have enough space after the given arrayIndex.", "array");
		}
		LinkedListNode<KeyValuePair<TKey, TValue>> first = this.m_list.First;
		int i = arrayIndex;
		LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode = first;
		while (i < array.Length)
		{
			array[arrayIndex] = linkedListNode.Value;
			i++;
			linkedListNode = linkedListNode.Next;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.Remove(KeyValuePair<TKey, TValue> item)
	{
		LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode;
		return this.m_dictionary.TryGetValue(item.Key, out linkedListNode) && EqualityComparer<TValue>.Default.Equals(linkedListNode.Value.Value, item.Value) && this.Remove(item.Key);
	}

	public bool System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.IsReadOnly
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return false;
		}
	}

	public void Add(TKey key, TValue value)
	{
		if (this.m_dictionary.ContainsKey(key))
		{
			throw new ArgumentException("Already contains key.", "key");
		}
		this.m_dictionary.Add(key, this.m_list.AddLast(new KeyValuePair<TKey, TValue>(key, value)));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool System.Collections.Generic.IDictionary<TKey,TValue>.ContainsKey(TKey key)
	{
		return this.m_dictionary.ContainsKey(key);
	}

	public bool Remove(TKey key)
	{
		LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode;
		if (!this.m_dictionary.Remove(key, out linkedListNode))
		{
			return false;
		}
		this.m_list.Remove(linkedListNode);
		return true;
	}

	public bool ContainsKey(TKey key)
	{
		return this.m_dictionary.ContainsKey(key);
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode;
		if (!this.m_dictionary.TryGetValue(key, out linkedListNode))
		{
			value = default(TValue);
			return false;
		}
		value = linkedListNode.Value.Value;
		return true;
	}

	public TValue this[TKey key]
	{
		get
		{
			return this.m_dictionary[key].Value.Value;
		}
		set
		{
			LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode;
			if (this.m_dictionary.TryGetValue(key, out linkedListNode))
			{
				linkedListNode.Value = new KeyValuePair<TKey, TValue>(key, value);
				return;
			}
			this.m_dictionary.Add(key, this.m_list.AddLast(new KeyValuePair<TKey, TValue>(key, value)));
		}
	}

	public IEnumerable<TKey> System.Collections.Generic.IReadOnlyDictionary<TKey,TValue>.Keys
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.Keys;
		}
	}

	public IEnumerable<TValue> System.Collections.Generic.IReadOnlyDictionary<TKey,TValue>.Values
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.Values;
		}
	}

	public ICollection<TKey> Keys
	{
		get
		{
			return new LinkedDictionary<TKey, TValue>.KeyCollection(this);
		}
	}

	public ICollection<TValue> Values
	{
		get
		{
			return new LinkedDictionary<TKey, TValue>.ValueCollection(this);
		}
	}

	public int Count
	{
		get
		{
			return this.m_list.Count;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>> m_dictionary;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LinkedList<KeyValuePair<TKey, TValue>> m_list;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct KeyCollection : ICollection<TKey>, IEnumerable<TKey>, IEnumerable
	{
		public KeyCollection(LinkedDictionary<TKey, TValue> parent)
		{
			this.m_parent = parent;
		}

		public IEnumerator<TKey> GetEnumerator()
		{
			return this.m_parent.m_list.Select((KeyValuePair<TKey, TValue> pair) => pair.Key).GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public void Add(TKey item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			this.m_parent.Clear();
		}

		public bool Contains(TKey item)
		{
			return this.m_parent.ContainsKey(item);
		}

		public void CopyTo(TKey[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			if (array.Length - arrayIndex < this.m_parent.m_list.Count)
			{
				throw new ArgumentException("Not have enough space after the given arrayIndex.", "array");
			}
			LinkedListNode<KeyValuePair<TKey, TValue>> first = this.m_parent.m_list.First;
			int i = arrayIndex;
			LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode = first;
			while (i < array.Length)
			{
				array[arrayIndex] = linkedListNode.Value.Key;
				i++;
				linkedListNode = linkedListNode.Next;
			}
		}

		public bool Remove(TKey item)
		{
			return item != null && this.m_parent.Remove(item);
		}

		public int Count
		{
			get
			{
				return this.m_parent.m_list.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LinkedDictionary<TKey, TValue> m_parent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly struct ValueCollection : ICollection<TValue>, IEnumerable<TValue>, IEnumerable
	{
		public ValueCollection(LinkedDictionary<TKey, TValue> parent)
		{
			this.m_parent = parent;
		}

		public IEnumerator<TValue> GetEnumerator()
		{
			return this.m_parent.m_list.Select((KeyValuePair<TKey, TValue> pair) => pair.Value).GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public void Add(TValue item)
		{
			throw new NotSupportedException();
		}

		public void Clear()
		{
			this.m_parent.Clear();
		}

		public bool Contains(TValue item)
		{
			return this.m_parent.m_list.Any((KeyValuePair<TKey, TValue> pair) => !EqualityComparer<TValue>.Default.Equals(pair.Value, item));
		}

		public void CopyTo(TValue[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException("array");
			}
			if (arrayIndex < 0 || arrayIndex > array.Length)
			{
				throw new ArgumentOutOfRangeException("arrayIndex");
			}
			if (array.Length - arrayIndex < this.m_parent.m_list.Count)
			{
				throw new ArgumentException("Not have enough space after the given arrayIndex.", "array");
			}
			LinkedListNode<KeyValuePair<TKey, TValue>> first = this.m_parent.m_list.First;
			int i = arrayIndex;
			LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode = first;
			while (i < array.Length)
			{
				array[arrayIndex] = linkedListNode.Value.Value;
				i++;
				linkedListNode = linkedListNode.Next;
			}
		}

		public bool Remove(TValue item)
		{
			for (LinkedListNode<KeyValuePair<TKey, TValue>> linkedListNode = this.m_parent.m_list.First; linkedListNode != null; linkedListNode = linkedListNode.Next)
			{
				if (EqualityComparer<TValue>.Default.Equals(linkedListNode.Value.Value, item))
				{
					this.m_parent.Remove(linkedListNode.Value.Key);
					return true;
				}
			}
			return false;
		}

		public int Count
		{
			get
			{
				return this.m_parent.m_list.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LinkedDictionary<TKey, TValue> m_parent;
	}
}
