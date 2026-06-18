using System;
using System.IO;

public class PooledMemoryStream : MemoryStream, IMemoryPoolableObject
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public ~PooledMemoryStream()
	{
	}

	public void Reset()
	{
		this.SetLength(0L);
	}

	public void Cleanup()
	{
	}

	public static int InstanceCount;
}
