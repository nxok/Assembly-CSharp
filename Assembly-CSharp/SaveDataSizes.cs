using System;

public readonly struct SaveDataSizes
{
	public SaveDataSizes(long total, long remaining)
	{
		this.m_total = total;
		this.m_remaining = remaining;
	}

	public long Total
	{
		get
		{
			return this.m_total;
		}
	}

	public long Used
	{
		get
		{
			return this.m_total - this.m_remaining;
		}
	}

	public long Remaining
	{
		get
		{
			return this.m_remaining;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly long m_total;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly long m_remaining;
}
