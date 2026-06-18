using System;

public class LastAccessHelper
{
	public DateTime Time
	{
		get
		{
			object @lock = this.m_lock;
			DateTime dateTime;
			lock (@lock)
			{
				dateTime = ((this.m_numActive > 0) ? DateTime.Now : this.m_time);
			}
			return dateTime;
		}
	}

	public LastAccessHelper.Scope CreateScope()
	{
		return new LastAccessHelper.Scope(this);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Increment()
	{
		object @lock = this.m_lock;
		lock (@lock)
		{
			this.m_numActive++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Decrement()
	{
		object @lock = this.m_lock;
		lock (@lock)
		{
			this.m_numActive--;
			if (this.m_numActive == 0)
			{
				this.m_time = DateTime.Now;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly object m_lock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_numActive;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime m_time = DateTime.Now;

	public struct Scope : IDisposable
	{
		public Scope(LastAccessHelper parent)
		{
			this.m_parent = parent;
			this.m_parent.Increment();
		}

		public void Dispose()
		{
			LastAccessHelper parent = this.m_parent;
			if (parent != null)
			{
				parent.Decrement();
			}
			this.m_parent = null;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public LastAccessHelper m_parent;
	}
}
