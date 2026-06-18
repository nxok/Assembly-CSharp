using System;
using System.Collections;

public class EnumerableDebugWrapper : DebugWrapper, IEnumerable
{
	public EnumerableDebugWrapper(IEnumerable enumerable)
		: this(null, enumerable)
	{
	}

	public EnumerableDebugWrapper(DebugWrapper parent, IEnumerable enumerable)
		: base(parent)
	{
		this.m_enumerable = enumerable;
	}

	public IEnumerator GetEnumerator()
	{
		return base.DebugEnumerator(this.m_enumerable.GetEnumerator());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IEnumerable m_enumerable;
}
