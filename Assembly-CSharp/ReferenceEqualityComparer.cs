using System;
using System.Collections;
using System.Threading;

public class ReferenceEqualityComparer : IEqualityComparer
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public ReferenceEqualityComparer()
	{
	}

	public static ReferenceEqualityComparer Instance
	{
		get
		{
			if (ReferenceEqualityComparer.instance == null)
			{
				ReferenceEqualityComparer referenceEqualityComparer = new ReferenceEqualityComparer();
				Interlocked.CompareExchange<ReferenceEqualityComparer>(ref ReferenceEqualityComparer.instance, referenceEqualityComparer, null);
			}
			return ReferenceEqualityComparer.instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool System.Collections.IEqualityComparer.Equals(object _x, object _y)
	{
		return _x == _y;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int System.Collections.IEqualityComparer.GetHashCode(object _obj)
	{
		if (_obj != null)
		{
			return _obj.GetHashCode();
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ReferenceEqualityComparer instance;
}
