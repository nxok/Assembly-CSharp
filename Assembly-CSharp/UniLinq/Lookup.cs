using System;
using System.Collections;
using System.Collections.Generic;

namespace UniLinq
{
	public class Lookup<TKey, TElement> : IEnumerable<IGrouping<TKey, TElement>>, IEnumerable, ILookup<TKey, TElement>
	{
		public int Count
		{
			get
			{
				if (this.nullGrouping != null)
				{
					return this.groups.Count + 1;
				}
				return this.groups.Count;
			}
		}

		public IEnumerable<TElement> this[TKey key]
		{
			get
			{
				if (key == null && this.nullGrouping != null)
				{
					return this.nullGrouping;
				}
				IGrouping<TKey, TElement> grouping;
				if (key != null && this.groups.TryGetValue(key, out grouping))
				{
					return grouping;
				}
				return new TElement[0];
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public Lookup(Dictionary<TKey, List<TElement>> lookup, IEnumerable<TElement> nullKeyElements)
		{
			this.groups = new Dictionary<TKey, IGrouping<TKey, TElement>>(lookup.Comparer);
			foreach (KeyValuePair<TKey, List<TElement>> keyValuePair in lookup)
			{
				this.groups.Add(keyValuePair.Key, new Grouping<TKey, TElement>(keyValuePair.Key, keyValuePair.Value));
			}
			if (nullKeyElements != null)
			{
				this.nullGrouping = new Grouping<TKey, TElement>(default(TKey), nullKeyElements);
			}
		}

		public IEnumerable<TResult> ApplyResultSelector<TResult>(Func<TKey, IEnumerable<TElement>, TResult> resultSelector)
		{
			if (this.nullGrouping != null)
			{
				yield return resultSelector(this.nullGrouping.Key, this.nullGrouping);
			}
			foreach (KeyValuePair<TKey, IGrouping<TKey, TElement>> keyValuePair in this.groups)
			{
				yield return resultSelector(keyValuePair.Value.Key, keyValuePair.Value);
			}
			Dictionary<TKey, IGrouping<TKey, TElement>>.Enumerator enumerator = default(Dictionary<TKey, IGrouping<TKey, TElement>>.Enumerator);
			yield break;
			yield break;
		}

		public bool Contains(TKey key)
		{
			if (key == null)
			{
				return this.nullGrouping != null;
			}
			return this.groups.ContainsKey(key);
		}

		public IEnumerator<IGrouping<TKey, TElement>> GetEnumerator()
		{
			if (this.nullGrouping != null)
			{
				yield return this.nullGrouping;
			}
			foreach (KeyValuePair<TKey, IGrouping<TKey, TElement>> keyValuePair in this.groups)
			{
				yield return keyValuePair.Value;
			}
			Dictionary<TKey, IGrouping<TKey, TElement>>.Enumerator enumerator = default(Dictionary<TKey, IGrouping<TKey, TElement>>.Enumerator);
			yield break;
			yield break;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public IGrouping<TKey, TElement> nullGrouping;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<TKey, IGrouping<TKey, TElement>> groups;
	}
}
