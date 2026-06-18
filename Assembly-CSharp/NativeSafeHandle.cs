using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

[NativeContainer]
public struct NativeSafeHandle<T> : IDisposable where T : struct, IDisposable
{
	public T Target
	{
		get
		{
			return this.target;
		}
	}

	public NativeSafeHandle(ref T _target, Allocator allocator)
	{
		this.target = _target;
	}

	public void Dispose()
	{
		this.target.Dispose();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public T target;
}
