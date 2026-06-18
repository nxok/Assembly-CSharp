using System;
using System.Collections;
using System.Collections.Generic;

public class CollectionDebugWrapper<T> : EnumerableDebugWrapper<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	public CollectionDebugWrapper(ICollection<T> collection)
		: this(null, collection)
	{
	}

	public CollectionDebugWrapper(DebugWrapper parent, ICollection<T> collection)
		: base(parent, collection)
	{
		this.m_collection = collection;
	}

	public void Add(T item)
	{
		using (base.DebugReadWriteScope())
		{
			this.m_collection.Add(item);
		}
	}

	public void Clear()
	{
		using (base.DebugReadWriteScope())
		{
			this.m_collection.Clear();
		}
	}

	public bool Contains(T item)
	{
		bool flag;
		using (base.DebugReadScope())
		{
			flag = this.m_collection.Contains(item);
		}
		return flag;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		using (base.DebugReadScope())
		{
			this.m_collection.CopyTo(array, arrayIndex);
		}
	}

	public bool Remove(T item)
	{
		bool flag;
		using (base.DebugReadWriteScope())
		{
			flag = this.m_collection.Remove(item);
		}
		return flag;
	}

	public int Count
	{
		get
		{
			return this.m_collection.Count;
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return this.m_collection.IsReadOnly;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ICollection<T> m_collection;
}
