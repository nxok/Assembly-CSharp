using System;
using System.Collections;
using System.Collections.Generic;

public class ReadOnlyListWrapper<TIn, TOut> : IReadOnlyList<TOut>, IEnumerable<TOut>, IEnumerable, IReadOnlyCollection<TOut> where TIn : TOut
{
	public ReadOnlyListWrapper(IList<TIn> list)
	{
		this.m_list = list;
	}

	public IEnumerator<TOut> GetEnumerator()
	{
		return (IEnumerator<TOut>)this.m_list.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator System.Collections.IEnumerable.GetEnumerator()
	{
		return this.m_list.GetEnumerator();
	}

	public int Count
	{
		get
		{
			return this.m_list.Count;
		}
	}

	public TOut this[int index]
	{
		get
		{
			return (TOut)((object)this.m_list[index]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly IList<TIn> m_list;
}
