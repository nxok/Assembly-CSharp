using System;
using System.Collections.Generic;
using System.Threading;

public class BlockingQueue<T>
{
	public void Enqueue(T item)
	{
		Queue<T> queue = this.queue;
		lock (queue)
		{
			this.queue.Enqueue(item);
			Monitor.PulseAll(this.queue);
		}
	}

	public T Dequeue()
	{
		Queue<T> queue = this.queue;
		T t;
		lock (queue)
		{
			while (this.queue.Count == 0)
			{
				if (this.closing)
				{
					t = default(T);
					return t;
				}
				Monitor.Wait(this.queue);
			}
			t = this.queue.Dequeue();
		}
		return t;
	}

	public bool HasData()
	{
		Queue<T> queue = this.queue;
		bool flag2;
		lock (queue)
		{
			flag2 = this.queue.Count > 0;
		}
		return flag2;
	}

	public void Close()
	{
		Queue<T> queue = this.queue;
		lock (queue)
		{
			this.closing = true;
			Monitor.PulseAll(this.queue);
		}
	}

	public void Clear()
	{
		Queue<T> queue = this.queue;
		lock (queue)
		{
			this.queue.Clear();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool closing;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Queue<T> queue = new Queue<T>();
}
