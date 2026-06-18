using System;
using System.Collections;
using System.Collections.Generic;

namespace UniLinq
{
	[PublicizedFrom(EAccessModifier.Internal)]
	public class Grouping<K, T> : IGrouping<K, T>, IEnumerable<T>, IEnumerable
	{
		public Grouping(K key, IEnumerable<T> group)
		{
			this.group = group;
			this.key = key;
		}

		public K Key
		{
			get
			{
				return this.key;
			}
			set
			{
				this.key = value;
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.group.GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.group.GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public K key;

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerable<T> group;
	}
}
