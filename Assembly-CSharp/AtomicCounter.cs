using System;
using System.Threading;

public sealed class AtomicCounter
{
	public int Value
	{
		get
		{
			return this.m_counter;
		}
	}

	public int Increment()
	{
		return Interlocked.Increment(ref this.m_counter);
	}

	public int Decrement()
	{
		return Interlocked.Decrement(ref this.m_counter);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_counter;
}
