using System;
using System.Collections.Generic;

namespace UniLinq
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class SortSequenceContext<TElement, TKey> : SortContext<TElement>
	{
		public SortSequenceContext(Func<TElement, TKey> selector, IComparer<TKey> comparer, SortDirection direction, SortContext<TElement> child_context)
			: base(direction, child_context)
		{
			this.selector = selector;
			this.comparer = comparer;
		}

		public override void Initialize(TElement[] elements)
		{
			if (this.child_context != null)
			{
				this.child_context.Initialize(elements);
			}
			this.keys = new TKey[elements.Length];
			for (int i = 0; i < this.keys.Length; i++)
			{
				this.keys[i] = this.selector(elements[i]);
			}
		}

		public override int Compare(int first_index, int second_index)
		{
			int num = this.comparer.Compare(this.keys[first_index], this.keys[second_index]);
			if (num == 0)
			{
				if (this.child_context != null)
				{
					return this.child_context.Compare(first_index, second_index);
				}
				num = ((this.direction == SortDirection.Descending) ? (second_index - first_index) : (first_index - second_index));
			}
			if (this.direction != SortDirection.Descending)
			{
				return num;
			}
			return -num;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Func<TElement, TKey> selector;

		[PublicizedFrom(EAccessModifier.Private)]
		public IComparer<TKey> comparer;

		[PublicizedFrom(EAccessModifier.Private)]
		public TKey[] keys;
	}
}
