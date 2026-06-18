using System;
using System.Collections;
using System.Collections.Generic;

public sealed class ListDebugWrapper<T> : CollectionDebugWrapper<T>, IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	public ListDebugWrapper()
		: this(new List<T>())
	{
	}

	public ListDebugWrapper(IList<T> list)
		: this(null, list)
	{
	}

	public ListDebugWrapper(DebugWrapper parent, IList<T> list)
		: base(parent, list)
	{
		this.m_list = list;
	}

	public int IndexOf(T item)
	{
		int num;
		using (base.DebugReadScope())
		{
			num = this.m_list.IndexOf(item);
		}
		return num;
	}

	public void Insert(int index, T item)
	{
		using (base.DebugReadWriteScope())
		{
			this.m_list.Insert(index, item);
		}
	}

	public void RemoveAt(int index)
	{
		using (base.DebugReadWriteScope())
		{
			this.m_list.RemoveAt(index);
		}
	}

	public T this[int index]
	{
		get
		{
			T t;
			using (base.DebugReadScope())
			{
				t = this.m_list[index];
			}
			return t;
		}
		set
		{
			using (base.DebugReadWriteScope())
			{
				this.m_list[index] = value;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IList<T> m_list;
}
