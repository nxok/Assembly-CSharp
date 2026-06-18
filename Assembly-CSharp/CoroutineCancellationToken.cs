using System;

public class CoroutineCancellationToken
{
	public void Cancel()
	{
		this.cancelled = true;
	}

	public bool IsCancelled()
	{
		return this.cancelled;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool cancelled;
}
