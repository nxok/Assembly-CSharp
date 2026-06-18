using System;
using System.Collections.Generic;

namespace UniLinq
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class QuickSort<TElement>
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public QuickSort(IEnumerable<TElement> source, SortContext<TElement> context)
		{
			List<TElement> list = new List<TElement>();
			foreach (TElement telement in source)
			{
				list.Add(telement);
			}
			this.elements = list.ToArray();
			this.indexes = QuickSort<TElement>.CreateIndexes(this.elements.Length);
			this.context = context;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static int[] CreateIndexes(int length)
		{
			int[] array = new int[length];
			for (int i = 0; i < length; i++)
			{
				array[i] = i;
			}
			return array;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void PerformSort()
		{
			if (this.elements.Length <= 1)
			{
				return;
			}
			this.context.Initialize(this.elements);
			Array.Sort<int>(this.indexes, this.context);
		}

		public static IEnumerable<TElement> Sort(IEnumerable<TElement> source, SortContext<TElement> context)
		{
			QuickSort<TElement> sorter = new QuickSort<TElement>(source, context);
			sorter.PerformSort();
			int num;
			for (int i = 0; i < sorter.elements.Length; i = num + 1)
			{
				yield return sorter.elements[sorter.indexes[i]];
				num = i;
			}
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public TElement[] elements;

		[PublicizedFrom(EAccessModifier.Private)]
		public int[] indexes;

		[PublicizedFrom(EAccessModifier.Private)]
		public SortContext<TElement> context;
	}
}
