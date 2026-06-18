using System;

public class ArrayListMP<T> where T : new()
{
	public ArrayListMP(MemoryPooledArray<T> _pool, int _minSize = 0)
	{
		this.pool = _pool;
		this.Count = 0;
		if (_minSize > 0)
		{
			this.Items = this.pool.Alloc(_minSize);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~ArrayListMP()
	{
		if (this.Items != null)
		{
			this.pool.Free(this.Items);
			this.Items = null;
		}
	}

	public void Add(T _item)
	{
		if (this.Items == null)
		{
			this.Items = this.pool.Alloc(0);
		}
		if (this.Count >= this.Items.Length)
		{
			this.Items = this.pool.Grow(this.Items);
		}
		T[] items = this.Items;
		int count = this.Count;
		this.Count = count + 1;
		items[count] = _item;
	}

	public T this[int idx]
	{
		get
		{
			return this.Items[idx];
		}
		set
		{
			this.Items[idx] = value;
		}
	}

	public void Clear()
	{
		this.Count = 0;
		if (this.Items != null)
		{
			this.pool.Free(this.Items);
			this.Items = null;
		}
	}

	public T[] ToArray()
	{
		if (this.Items == null)
		{
			return new T[0];
		}
		T[] array = new T[this.Count];
		Array.Copy(this.Items, array, this.Count);
		return array;
	}

	public void AddRange(T[] _range)
	{
		this.AddRange(_range, 0, _range.Length);
	}

	public void AddRange(T[] _range, int _offs, int _count)
	{
		if (_range == null || _range.Length == 0)
		{
			return;
		}
		if (this.Items == null)
		{
			this.Items = this.pool.Alloc(_count);
		}
		if (this.Count + _count >= this.Items.Length)
		{
			this.Items = this.pool.Grow(this.Items, this.Count + _count);
		}
		Array.Copy(_range, _offs, this.Items, this.Count, _count);
		this.Count += _count;
	}

	public int Alloc(int _count)
	{
		if (this.Items == null)
		{
			this.Items = this.pool.Alloc(_count);
		}
		else if (this.Count + _count > this.Items.Length)
		{
			this.Items = this.pool.Grow(this.Items, this.Count + _count);
		}
		int count = this.Count;
		this.Count += _count;
		return count;
	}

	public void Grow(int newSize)
	{
		if (this.Items == null)
		{
			this.Items = this.pool.Alloc(newSize);
			return;
		}
		if (newSize > this.Items.Length)
		{
			this.Items = this.pool.Grow(this.Items, newSize);
		}
	}

	public readonly MemoryPooledArray<T> pool;

	public T[] Items;

	public int Count;
}
