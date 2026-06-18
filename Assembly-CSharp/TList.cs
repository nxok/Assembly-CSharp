using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

public class TList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
{
	public bool Disposed
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return Thread.VolatileRead(ref this.m_Disposed) == 1;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			Thread.VolatileWrite(ref this.m_Disposed, value ? 1 : 0);
		}
	}

	public TList()
	{
		this.m_TList = new List<T>();
	}

	public TList(int capacity)
	{
		this.m_TList = new List<T>(capacity);
	}

	public TList(IEnumerable<T> collection)
	{
		this.m_TList = new List<T>(collection);
	}

	public IEnumerator GetEnumerator()
	{
		this.LockList.EnterReadLock();
		List<T> list;
		try
		{
			list = new List<T>(this.m_TList);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		foreach (T t in list)
		{
			yield return t;
		}
		List<T>.Enumerator enumerator = default(List<T>.Enumerator);
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
	{
		this.LockList.EnterReadLock();
		List<T> list;
		try
		{
			list = new List<T>(this.m_TList);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		foreach (T t in list)
		{
			yield return t;
		}
		List<T>.Enumerator enumerator = default(List<T>.Enumerator);
		yield break;
		yield break;
	}

	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Dispose(bool disposing)
	{
		if (this.Disposed)
		{
			return;
		}
		this.Disposed = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~TList()
	{
		this.Dispose(false);
	}

	public void Add(T item)
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.Add(item);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public void AddRange(IEnumerable<T> collection)
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.AddRange(collection);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public bool AddIfNotExist(T item)
	{
		this.LockList.EnterWriteLock();
		bool flag;
		try
		{
			if (this.m_TList.Contains(item))
			{
				flag = false;
			}
			else
			{
				this.m_TList.Add(item);
				flag = true;
			}
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
		return flag;
	}

	public ReadOnlyCollection<T> AsReadOnly()
	{
		this.LockList.EnterReadLock();
		ReadOnlyCollection<T> readOnlyCollection;
		try
		{
			readOnlyCollection = this.m_TList.AsReadOnly();
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return readOnlyCollection;
	}

	public int BinarySearch(T item)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.BinarySearch(item);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public int BinarySearch(T item, IComparer<T> comparer)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.BinarySearch(item, comparer);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public int BinarySearch(int index, int count, T item, IComparer<T> comparer)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.BinarySearch(index, count, item, comparer);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public int Capacity
	{
		get
		{
			this.LockList.EnterReadLock();
			int capacity;
			try
			{
				capacity = this.m_TList.Capacity;
			}
			finally
			{
				this.LockList.ExitReadLock();
			}
			return capacity;
		}
		set
		{
			this.LockList.EnterWriteLock();
			try
			{
				this.m_TList.Capacity = value;
			}
			finally
			{
				this.LockList.ExitWriteLock();
			}
		}
	}

	public void Clear()
	{
		this.LockList.EnterReadLock();
		try
		{
			this.m_TList.Clear();
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
	}

	public bool Contains(T item)
	{
		this.LockList.EnterReadLock();
		bool flag;
		try
		{
			flag = this.m_TList.Contains(item);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return flag;
	}

	public List<TOutput> ConvertAll<TOutput>(Converter<T, TOutput> converter)
	{
		this.LockList.EnterReadLock();
		List<TOutput> list;
		try
		{
			list = this.m_TList.ConvertAll<TOutput>(converter);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return list;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		this.LockList.EnterReadLock();
		try
		{
			this.m_TList.CopyTo(array, arrayIndex);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
	}

	public int Count
	{
		get
		{
			this.LockList.EnterReadLock();
			int count;
			try
			{
				count = this.m_TList.Count;
			}
			finally
			{
				this.LockList.ExitReadLock();
			}
			return count;
		}
	}

	public bool Exists(Predicate<T> match)
	{
		this.LockList.EnterReadLock();
		bool flag;
		try
		{
			flag = this.m_TList.Exists(match);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return flag;
	}

	public T Find(Predicate<T> match)
	{
		this.LockList.EnterReadLock();
		T t;
		try
		{
			t = this.m_TList.Find(match);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return t;
	}

	public List<T> FindAll(Predicate<T> match)
	{
		this.LockList.EnterReadLock();
		List<T> list;
		try
		{
			list = this.m_TList.FindAll(match);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return list;
	}

	public int FindIndex(Predicate<T> match)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.FindIndex(match);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public int FindIndex(int startIndex, Predicate<T> match)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.FindIndex(startIndex, match);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public int FindIndex(int startIndex, int count, Predicate<T> match)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.FindIndex(startIndex, count, match);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public T FindLast(Predicate<T> match)
	{
		this.LockList.EnterReadLock();
		T t;
		try
		{
			t = this.m_TList.FindLast(match);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return t;
	}

	public int FindLastIndex(Predicate<T> match)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.FindLastIndex(match);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public int FindLastIndex(int startIndex, Predicate<T> match)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.FindLastIndex(startIndex, match);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public int FindLastIndex(int startIndex, int count, Predicate<T> match)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.FindLastIndex(startIndex, count, match);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public void ForEach(Action<T> action)
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.ForEach(action);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public List<T> GetRange(int index, int count)
	{
		this.LockList.EnterReadLock();
		List<T> range;
		try
		{
			range = this.m_TList.GetRange(index, count);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return range;
	}

	public int IndexOf(T item)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.IndexOf(item);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public int IndexOf(T item, int index)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.IndexOf(item, index);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public int IndexOf(T item, int index, int count)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.IndexOf(item, index, count);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public void Insert(int index, T item)
	{
		this.LockList.ExitWriteLock();
		try
		{
			this.m_TList.Insert(index, item);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public void InsertRange(int index, IEnumerable<T> range)
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.InsertRange(index, range);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public bool IsReadOnly
	{
		get
		{
			return false;
		}
	}

	public int LastIndexOf(T item)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.LastIndexOf(item);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public int LastIndexOf(T item, int index)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.LastIndexOf(item, index);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public int LastIndexOf(T item, int index, int count)
	{
		this.LockList.EnterReadLock();
		int num;
		try
		{
			num = this.m_TList.LastIndexOf(item, index, count);
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return num;
	}

	public bool Remove(T item)
	{
		this.LockList.EnterWriteLock();
		bool flag;
		try
		{
			flag = this.m_TList.Remove(item);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
		return flag;
	}

	public int RemoveAll(Predicate<T> match)
	{
		this.LockList.EnterWriteLock();
		int num;
		try
		{
			num = this.m_TList.RemoveAll(match);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
		return num;
	}

	public void RemoveAt(int index)
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.RemoveAt(index);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public void RemoveRange(int index, int count)
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.RemoveRange(index, count);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public void Reverse()
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.Reverse();
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public void Reverse(int index, int count)
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.Reverse(index, count);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public void Sort()
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.Sort();
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public void Sort(Comparison<T> comparison)
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.Sort(comparison);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public void Sort(IComparer<T> comparer)
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.Sort(comparer);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public void Sort(int index, int count, IComparer<T> comparer)
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.Sort(index, count, comparer);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public T[] ToArray()
	{
		this.LockList.EnterReadLock();
		T[] array;
		try
		{
			array = this.m_TList.ToArray();
		}
		finally
		{
			this.LockList.ExitReadLock();
		}
		return array;
	}

	public void TrimExcess()
	{
		this.LockList.EnterWriteLock();
		try
		{
			this.m_TList.TrimExcess();
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
	}

	public bool TrueForAll(Predicate<T> match)
	{
		this.LockList.EnterWriteLock();
		bool flag;
		try
		{
			flag = this.m_TList.TrueForAll(match);
		}
		finally
		{
			this.LockList.ExitWriteLock();
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<T> m_TList;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ReaderWriterLockSlim LockList = new ReaderWriterLockSlim();

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_Disposed;
}
