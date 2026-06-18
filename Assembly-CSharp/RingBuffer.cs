using System;

public class RingBuffer<T>
{
	public RingBuffer(int _count)
	{
		this.data = new T[_count];
	}

	public void Add(T _el)
	{
		T[] array = this.data;
		int num = this.idx;
		this.idx = num + 1;
		array[num] = _el;
		if (this.idx >= this.data.Length)
		{
			this.idx = 0;
		}
		this.count++;
		if (this.count > this.data.Length)
		{
			this.count = this.data.Length;
		}
	}

	public int Count
	{
		get
		{
			return this.count;
		}
	}

	public void Clear()
	{
		this.count = 0;
		this.idx = 0;
	}

	public void SetToLast()
	{
		this.readIdx = this.idx - 1;
		if (this.readIdx < 0)
		{
			this.readIdx = this.data.Length - 1;
		}
	}

	public T Peek()
	{
		return this.data[this.readIdx];
	}

	public T GetPrev()
	{
		T[] array = this.data;
		int num = this.readIdx;
		this.readIdx = num - 1;
		T t = array[num];
		if (this.readIdx < 0)
		{
			this.readIdx = this.data.Length - 1;
		}
		return t;
	}

	public T GetNext()
	{
		T[] array = this.data;
		int num = this.readIdx;
		this.readIdx = num + 1;
		T t = array[num];
		if (this.readIdx >= this.data.Length)
		{
			this.readIdx = 0;
		}
		return t;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T[] data;

	[PublicizedFrom(EAccessModifier.Private)]
	public int idx;

	[PublicizedFrom(EAccessModifier.Private)]
	public int count;

	[PublicizedFrom(EAccessModifier.Private)]
	public int readIdx;
}
