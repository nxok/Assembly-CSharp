using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Webserver.FileCache
{
	public abstract class AbstractCache
	{
		public abstract byte[] GetFileContent(string _filename);

		[return: TupleElementNames(new string[] { "filesDropped", "bytesDropped" })]
		public abstract ValueTuple<int, int> Invalidate();

		[PublicizedFrom(EAccessModifier.Protected)]
		public AbstractCache()
		{
			AbstractCache.caches.Add(this);
		}

		public static ValueTuple<int, int> InvalidateAllCaches()
		{
			int num = 0;
			int num2 = 0;
			foreach (AbstractCache abstractCache in AbstractCache.caches)
			{
				ValueTuple<int, int> valueTuple = abstractCache.Invalidate();
				int item = valueTuple.Item1;
				int item2 = valueTuple.Item2;
				num += item;
				num2 += item2;
			}
			return new ValueTuple<int, int>(num, num2);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly List<AbstractCache> caches = new List<AbstractCache>();
	}
}
