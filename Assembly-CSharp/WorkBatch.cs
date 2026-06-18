using System;
using System.Collections.Generic;

public class WorkBatch<T>
{
	public WorkBatch()
	{
		this.queuingList = new List<T>();
		this.workingList = new List<T>();
		this.sync = new object();
	}

	public int Count()
	{
		int num = this.workingList.Count;
		object obj = this.sync;
		lock (obj)
		{
			num += this.queuingList.Count;
		}
		return num;
	}

	public void Clear()
	{
		object obj = this.sync;
		lock (obj)
		{
			this.queuingList.Clear();
		}
		this.workingList.Clear();
	}

	public void DoWork(Action<T> _action)
	{
		this.FlipLists();
		this.workingList.ForEach(_action);
		this.workingList.Clear();
	}

	public void Add(T _item)
	{
		object obj = this.sync;
		lock (obj)
		{
			this.queuingList.Add(_item);
		}
	}

	public bool Remove(T _item)
	{
		object obj = this.sync;
		bool flag2;
		lock (obj)
		{
			flag2 = this.queuingList.Remove(_item);
		}
		return flag2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FlipLists()
	{
		object obj = this.sync;
		lock (obj)
		{
			List<T> list = this.workingList;
			this.workingList = this.queuingList;
			this.queuingList = list;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T> queuingList;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<T> workingList;

	[PublicizedFrom(EAccessModifier.Private)]
	public object sync;
}
