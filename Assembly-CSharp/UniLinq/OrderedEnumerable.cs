using System;
using System.Collections;
using System.Collections.Generic;

namespace UniLinq
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public abstract class OrderedEnumerable<TElement> : IOrderedEnumerable<TElement>, IEnumerable<TElement>, IEnumerable
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public OrderedEnumerable(IEnumerable<TElement> source)
		{
			this.source = source;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public virtual IEnumerator<TElement> GetEnumerator()
		{
			return this.Sort(this.source).GetEnumerator();
		}

		public abstract SortContext<TElement> CreateContext(SortContext<TElement> current);

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract IEnumerable<TElement> Sort(IEnumerable<TElement> source);

		public IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> selector, IComparer<TKey> comparer, bool descending)
		{
			return new OrderedSequence<TElement, TKey>(this, this.source, selector, comparer, descending ? SortDirection.Descending : SortDirection.Ascending);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerable<TElement> source;
	}
}
