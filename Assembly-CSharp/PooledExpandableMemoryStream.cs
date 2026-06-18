using System;
using System.IO;

public class PooledExpandableMemoryStream : MemoryStream, IMemoryPoolableObject, IDisposable
{
	public override void Close()
	{
	}

	public void Reset()
	{
		this.SetLength(0L);
	}

	public void Cleanup()
	{
		this.Reset();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Dispose(bool _disposing)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void System.IDisposable.Dispose()
	{
		MemoryPools.poolMemoryStream.FreeSync(this);
	}
}
