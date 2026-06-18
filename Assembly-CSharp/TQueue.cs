using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class TQueue<T> : ICollection, IEnumerable
{
	public TQueue()
	{
		this.m_Queue = new Queue<T>();
	}

	public TQueue(int capacity)
	{
		this.m_Queue = new Queue<T>(capacity);
	}

	public TQueue(IEnumerable<T> collection)
	{
		this.m_Queue = new Queue<T>(collection);
	}

	public IEnumerator<T> GetEnumerator()
	{
		this.LockQ.EnterReadLock();
		Queue<T> queue;
		try
		{
			queue = new Queue<T>(this.m_Queue);
		}
		finally
		{
			this.LockQ.ExitReadLock();
		}
		foreach (T t in queue)
		{
			yield return t;
		}
		Queue<T>.Enumerator enumerator = default(Queue<T>.Enumerator);
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		this.LockQ.EnterReadLock();
		Queue<T> queue;
		try
		{
			queue = new Queue<T>(this.m_Queue);
		}
		finally
		{
			this.LockQ.ExitReadLock();
		}
		foreach (T t in queue)
		{
			yield return t;
		}
		Queue<T>.Enumerator enumerator = default(Queue<T>.Enumerator);
		yield break;
		yield break;
	}

	public void CopyTo(Array array, int index)
	{
		this.LockQ.EnterReadLock();
		try
		{
			this.m_Queue.ToArray().CopyTo(array, index);
		}
		finally
		{
			this.LockQ.ExitReadLock();
		}
	}

	public void CopyTo(T[] array, int index)
	{
		this.LockQ.EnterReadLock();
		try
		{
			this.m_Queue.CopyTo(array, index);
		}
		finally
		{
			this.LockQ.ExitReadLock();
		}
	}

	public int Count
	{
		get
		{
			this.LockQ.EnterReadLock();
			int count;
			try
			{
				count = this.m_Queue.Count;
			}
			finally
			{
				this.LockQ.ExitReadLock();
			}
			return count;
		}
	}

	public bool IsSynchronized
	{
		get
		{
			return true;
		}
	}

	public object SyncRoot
	{
		get
		{
			return this.objSyncRoot;
		}
	}

	public void Enqueue(T item)
	{
		this.LockQ.EnterWriteLock();
		try
		{
			this.m_Queue.Enqueue(item);
		}
		finally
		{
			this.LockQ.ExitWriteLock();
		}
	}

	public T Dequeue()
	{
		this.LockQ.EnterWriteLock();
		T t;
		try
		{
			t = this.m_Queue.Dequeue();
		}
		finally
		{
			this.LockQ.ExitWriteLock();
		}
		return t;
	}

	public void EnqueueAll(IEnumerable<T> ItemsToQueue)
	{
		this.LockQ.EnterWriteLock();
		try
		{
			foreach (T t in ItemsToQueue)
			{
				this.m_Queue.Enqueue(t);
			}
		}
		finally
		{
			this.LockQ.ExitWriteLock();
		}
	}

	public void EnqueueAll(TList<T> ItemsToQueue)
	{
		this.LockQ.EnterWriteLock();
		try
		{
			foreach (object obj in ItemsToQueue)
			{
				T t = (T)((object)obj);
				this.m_Queue.Enqueue(t);
			}
		}
		finally
		{
			this.LockQ.ExitWriteLock();
		}
	}

	public TList<T> DequeueAll()
	{
		this.LockQ.EnterWriteLock();
		TList<T> tlist2;
		try
		{
			TList<T> tlist = new TList<T>();
			while (this.m_Queue.Count > 0)
			{
				tlist.Add(this.m_Queue.Dequeue());
			}
			tlist2 = tlist;
		}
		finally
		{
			this.LockQ.ExitWriteLock();
		}
		return tlist2;
	}

	public void Clear()
	{
		this.LockQ.EnterWriteLock();
		try
		{
			this.m_Queue.Clear();
		}
		finally
		{
			this.LockQ.ExitWriteLock();
		}
	}

	public bool Contains(T item)
	{
		this.LockQ.EnterReadLock();
		bool flag;
		try
		{
			flag = this.m_Queue.Contains(item);
		}
		finally
		{
			this.LockQ.ExitReadLock();
		}
		return flag;
	}

	public T Peek()
	{
		this.LockQ.EnterReadLock();
		T t;
		try
		{
			t = this.m_Queue.Peek();
		}
		finally
		{
			this.LockQ.ExitReadLock();
		}
		return t;
	}

	public T[] ToArray()
	{
		this.LockQ.EnterReadLock();
		T[] array;
		try
		{
			array = this.m_Queue.ToArray();
		}
		finally
		{
			this.LockQ.ExitReadLock();
		}
		return array;
	}

	public void TrimExcess()
	{
		this.LockQ.EnterWriteLock();
		try
		{
			this.m_Queue.TrimExcess();
		}
		finally
		{
			this.LockQ.ExitWriteLock();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<T> m_Queue;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ReaderWriterLockSlim LockQ = new ReaderWriterLockSlim();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object objSyncRoot = new object();
}
