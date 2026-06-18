using System;
using System.Collections.Generic;
using System.Threading;

public sealed class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ReferenceEqualityComparer()
	{
	}

	public static ReferenceEqualityComparer<T> Instance
	{
		get
		{
			if (ReferenceEqualityComparer<T>.instance == null)
			{
				ReferenceEqualityComparer<T> referenceEqualityComparer = new ReferenceEqualityComparer<T>();
				Interlocked.CompareExchange<ReferenceEqualityComparer<T>>(ref ReferenceEqualityComparer<T>.instance, referenceEqualityComparer, null);
			}
			return ReferenceEqualityComparer<T>.instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool System.Collections.Generic.IEqualityComparer<T>.Equals(T _x, T _y)
	{
		return _x == _y;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int System.Collections.Generic.IEqualityComparer<T>.GetHashCode(T _obj)
	{
		if (_obj == null)
		{
			return 0;
		}
		return _obj.GetHashCode();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ReferenceEqualityComparer<T> instance;
}
