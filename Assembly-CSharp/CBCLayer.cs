using System;

public class CBCLayer : IMemoryPoolableObject
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public ~CBCLayer()
	{
	}

	public void Reset()
	{
	}

	public void Cleanup()
	{
	}

	public void CopyFrom(CBCLayer _other)
	{
		Array.Copy(_other.data, this.data, this.data.Length);
	}

	public readonly byte[] data = new byte[1024];

	public static int InstanceCount;
}
