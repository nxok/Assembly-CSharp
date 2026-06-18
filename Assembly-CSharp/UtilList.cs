using System;
using System.Collections.Generic;

public class UtilList<T> where T : class
{
	public int Capacity
	{
		get
		{
			return this.capacity;
		}
	}

	public int Count
	{
		get
		{
			return this.count;
		}
	}

	public UtilList(int _capacity, T _NullValue)
	{
		this.capacity = _capacity;
		this.StartId = 0;
		this.EndId = 0;
		this.count = 0;
		this.IsStandardState = true;
		this.IsFull = false;
		this.InternArray = new T[_capacity];
		this.NullValue = _NullValue;
	}

	public void Clear()
	{
		this.StartId = 0;
		this.EndId = 0;
		this.count = 0;
		this.IsFull = false;
		this.IsStandardState = true;
	}

	public void Add(T NewItem)
	{
		if (this.IsFull)
		{
			throw new ArgumentException("Overflow : UtilList is full");
		}
		this.InternArray[this.EndId] = NewItem;
		int num = this.EndId + 1;
		this.EndId = num;
		if (num == this.capacity)
		{
			this.EndId = 0;
			this.IsStandardState = false;
		}
		if (this.EndId == this.StartId)
		{
			this.IsFull = true;
		}
		this.count++;
	}

	public T Peek()
	{
		return this.InternArray[this.StartId];
	}

	public T Dequeue()
	{
		if (this.StartId == this.EndId && !this.IsFull)
		{
			throw new ArgumentException("UtilList is EMPTY");
		}
		int startId = this.StartId;
		this.StartId++;
		if (this.StartId == this.capacity)
		{
			this.StartId = 0;
			this.IsStandardState = true;
		}
		this.IsFull = false;
		this.count--;
		return this.InternArray[startId];
	}

	public void RemoveNFirst(int N)
	{
		if (this.count < N)
		{
			throw new ArgumentException("UtilList Out of range");
		}
		this.StartId += N;
		if (this.StartId >= this.capacity)
		{
			this.StartId -= this.capacity;
			this.IsStandardState = true;
		}
		this.count -= N;
		this.IsFull = this.count == this.capacity;
	}

	public void RemoveAt(List<int> IdList)
	{
		int num = IdList.Count;
		for (int i = 0; i < num; i++)
		{
			this[IdList[i]] = this.NullValue;
		}
		num = 0;
		for (int j = 0; j < this.Count; j++)
		{
			if (this[j] != null)
			{
				this[num++] = this[j];
			}
		}
		this.count = num;
		this.EndId = this.StartId + this.count;
		if (this.EndId >= this.capacity)
		{
			this.EndId -= this.capacity;
			this.IsStandardState = false;
		}
		else
		{
			this.IsStandardState = true;
		}
		this.IsFull = this.count == this.capacity;
	}

	public T this[int Id]
	{
		get
		{
			if (this.Count == 0)
			{
				throw new ArgumentException("UtilList is EMPTY");
			}
			int num = this.StartId + Id;
			if (this.IsStandardState)
			{
				if (num >= this.EndId)
				{
					throw new ArgumentException("UtilList index is out of range");
				}
			}
			else if (num >= this.capacity)
			{
				num -= this.capacity;
				if (num >= this.EndId)
				{
					throw new ArgumentException("UtilList Index is out of range");
				}
			}
			return this.InternArray[num];
		}
		set
		{
			if (this.Count == 0)
			{
				throw new ArgumentException("UtilList is EMPTY");
			}
			int num = this.StartId + Id;
			if (this.IsStandardState)
			{
				if (num >= this.EndId)
				{
					throw new ArgumentException("UtilList : Index is out of range");
				}
			}
			else if (num >= this.capacity)
			{
				num -= this.capacity;
				if (num >= this.EndId)
				{
					throw new ArgumentException("UtilList : Index is out of range");
				}
			}
			this.InternArray[num] = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int capacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public int StartId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int EndId;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsStandardState;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsFull;

	[PublicizedFrom(EAccessModifier.Private)]
	public T[] InternArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public T NullValue;
}
