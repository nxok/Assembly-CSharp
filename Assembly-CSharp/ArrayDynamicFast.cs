using System;
using System.Collections.Generic;

public class ArrayDynamicFast<T>
{
	public ArrayDynamicFast(int _size)
	{
		this.Size = _size;
		this.Data = new T[_size];
		this.DataAvail = new bool[_size];
		this.Count = 0;
	}

	public int Contains(T _v)
	{
		if (this.Count == 0)
		{
			return -1;
		}
		if (_v == null)
		{
			for (int i = 0; i < this.Data.Length; i++)
			{
				if (this.DataAvail[i] && this.Data[i] == null)
				{
					return i;
				}
			}
		}
		else
		{
			EqualityComparer<T> @default = EqualityComparer<T>.Default;
			for (int j = 0; j < this.Data.Length; j++)
			{
				if (this.DataAvail[j] && @default.Equals(this.Data[j], _v))
				{
					return j;
				}
			}
		}
		return -1;
	}

	public void Clear()
	{
		for (int i = 0; i < this.Data.Length; i++)
		{
			this.DataAvail[i] = false;
		}
	}

	public void Add(int _idx, T _texId)
	{
		if (_idx == -1)
		{
			for (int i = 0; i < this.Size; i++)
			{
				if (!this.DataAvail[i])
				{
					_idx = i;
					break;
				}
			}
		}
		if (_idx == -1)
		{
			return;
		}
		this.Data[_idx] = _texId;
		this.DataAvail[_idx] = true;
		this.Count++;
	}

	public T[] Data;

	public bool[] DataAvail;

	public int Count;

	public int Size;
}
