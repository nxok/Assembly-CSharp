using System;

namespace MusicUtils
{
	public struct Pair<T1, T2> where T1 : IComparable<T1> where T2 : IComparable<T2>
	{
		public Pair(T1 _item1, T2 _item2)
		{
			this.item1 = _item1;
			this.item2 = _item2;
		}

		public T1 item1;

		public T2 item2;
	}
}
