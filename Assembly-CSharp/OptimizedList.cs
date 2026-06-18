using System;
using System.Collections.Generic;

[Serializable]
public class OptimizedList<T>
{
	public OptimizedList()
		: this(10)
	{
		this.IsValueType = typeof(T).IsValueType;
	}

	public OptimizedList(int Capacity)
	{
		this.Count = 0;
		this.array = new T[Capacity];
		this.length = Capacity;
		this.DoubleSize = 0;
		this.IsValueType = typeof(T).IsValueType;
	}

	public OptimizedList(OptimizedList<T> L)
	{
		if (L.Count == 0)
		{
			this.Count = 0;
			this.array = new T[2];
			this.length = 2;
			return;
		}
		this.Count = 0;
		this.array = new T[L.Count];
		this.length = L.Count;
		this.DoubleSize = 0;
		this.IsValueType = typeof(T).IsValueType;
		this.AddRange(L);
	}

	public OptimizedList(T[] L)
	{
		if (L.Length == 0)
		{
			this.Count = 0;
			this.array = new T[2];
			this.length = 2;
			return;
		}
		this.Count = 0;
		this.array = new T[L.Length];
		this.length = L.Length;
		this.DoubleSize = 0;
		this.IsValueType = typeof(T).IsValueType;
		this.AddRange(L);
	}

	public OptimizedList(int Capacity, int DoubleSize)
	{
		this.Count = 0;
		this.array = new T[Capacity];
		this.length = Capacity;
		this.DoubleSize = DoubleSize;
		this.IsValueType = typeof(T).IsValueType;
	}

	public T Last()
	{
		if (this.Count > 0)
		{
			return this.array[this.Count - 1];
		}
		return default(T);
	}

	public void Add(T value)
	{
		if (this.length == this.Count)
		{
			this.length = ((this.DoubleSize == 0) ? (this.Count * 2) : (this.Count + this.DoubleSize));
			T[] array = new T[this.length];
			Array.Copy(this.array, 0, array, 0, this.Count);
			this.array = array;
		}
		T[] array2 = this.array;
		int count = this.Count;
		this.Count = count + 1;
		array2[count] = value;
	}

	public void Add(ref T value)
	{
		if (this.length == this.Count)
		{
			this.length = ((this.DoubleSize == 0) ? (this.Count * 2) : (this.Count + this.DoubleSize));
			T[] array = new T[this.length];
			Array.Copy(this.array, 0, array, 0, this.Count);
			this.array = array;
		}
		T[] array2 = this.array;
		int count = this.Count;
		this.Count = count + 1;
		array2[count] = value;
	}

	public void AddSafe(T value)
	{
		T[] array = this.array;
		int count = this.Count;
		this.Count = count + 1;
		array[count] = value;
	}

	public void AddSafe(T valueA, T valueB, T valueC, T valueD)
	{
		if (this.length - (this.Count + 4) <= 0)
		{
			this.length = ((this.DoubleSize == 0) ? (this.Count * 2 + 4) : (this.Count + this.DoubleSize + 4));
			T[] array = new T[this.length];
			Array.Copy(this.array, 0, array, 0, this.Count);
			this.array = array;
		}
		T[] array2 = this.array;
		int num = this.Count;
		this.Count = num + 1;
		array2[num] = valueA;
		T[] array3 = this.array;
		num = this.Count;
		this.Count = num + 1;
		array3[num] = valueB;
		T[] array4 = this.array;
		num = this.Count;
		this.Count = num + 1;
		array4[num] = valueC;
		T[] array5 = this.array;
		num = this.Count;
		this.Count = num + 1;
		array5[num] = valueD;
	}

	public void AddRange(T[] values)
	{
		int num = values.Length;
		if (this.length - (this.Count + num) <= 0)
		{
			T[] array = new T[this.Count * 2 + num];
			Array.Copy(this.array, 0, array, 0, this.Count);
			this.array = array;
			this.length = this.Count * 2 + num;
		}
		Array.Copy(values, 0, this.array, this.Count, num);
		this.Count += num;
	}

	public void AddRange(OptimizedList<T> values)
	{
		if (values == null || values.Count == 0)
		{
			return;
		}
		int num = values.Length;
		if (this.length - (this.Count + num) <= 0)
		{
			this.length = this.Count * 2 + num;
			T[] array = new T[this.length];
			Array.Copy(this.array, 0, array, 0, this.Count);
			this.array = array;
		}
		Array.Copy(values.array, 0, this.array, this.Count, num);
		this.Count += num;
	}

	public bool Remove(T obj)
	{
		int num = Array.IndexOf<T>(this.array, obj, 0, this.Count);
		if (num >= 0)
		{
			this.Count--;
			if (num < this.Count)
			{
				Array.Copy(this.array, num + 1, this.array, num, this.Count - num);
			}
			this.array[this.Count] = default(T);
			return true;
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		if (index >= this.Count)
		{
			return;
		}
		this.Count--;
		if (index < this.Count)
		{
			Array.Copy(this.array, index + 1, this.array, index, this.Count - index);
		}
		this.array[this.Count] = default(T);
	}

	public bool Contains(T obj)
	{
		return Array.IndexOf<T>(this.array, obj, 0, this.Count) >= 0;
	}

	public void Set(T[] values)
	{
		this.array = values;
		this.length = values.Length;
		this.Count = this.length;
	}

	public void Clear()
	{
		if (this.Count > 0)
		{
			Array.Clear(this.array, 0, this.array.Length);
		}
		this.Count = 0;
	}

	public T[] ToArray()
	{
		if (this.Count == 0)
		{
			return null;
		}
		T[] array = new T[this.Count];
		Array.Copy(this.array, array, this.Count);
		return array;
	}

	public void CheckArray(int Size)
	{
		if (this.length - (this.Count + Size) <= 0)
		{
			this.length = ((this.DoubleSize == 0) ? (this.Count * 2 + Size) : (this.Count + this.DoubleSize + Size));
			T[] array = new T[this.length];
			Array.Copy(this.array, 0, array, 0, this.Count);
			this.array = array;
		}
	}

	public void Sort(IComparer<T> comparer)
	{
		this.Sort(0, this.Count, comparer);
	}

	public void Sort(int index, int count, IComparer<T> comparer)
	{
		if (this.length - index < count)
		{
			return;
		}
		Array.Sort<T>(this.array, index, count, comparer);
	}

	public int Length
	{
		get
		{
			return this.Count;
		}
	}

	public T[] array;

	public int Count;

	public int length;

	public int DoubleSize;

	public bool IsValueType;
}
