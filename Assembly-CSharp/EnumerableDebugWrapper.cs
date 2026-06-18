using System;
using System.Collections;
using System.Collections.Generic;

public class EnumerableDebugWrapper<T> : EnumerableDebugWrapper, IEnumerable<T>, IEnumerable
{
	public EnumerableDebugWrapper(IEnumerable<T> enumerable)
		: this(null, enumerable)
	{
	}

	public EnumerableDebugWrapper(DebugWrapper parent, IEnumerable<T> enumerable)
		: base(parent, enumerable)
	{
		this.m_enumerable = enumerable;
	}

	public new IEnumerator<T> GetEnumerator()
	{
		return base.DebugEnumerator<T>(this.m_enumerable.GetEnumerator());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new readonly IEnumerable<T> m_enumerable;
}
