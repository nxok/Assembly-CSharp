using System;

namespace Platform
{
	public static class IPlatformMemoryStatExtensions
	{
		public static IPlatformMemoryStat<T> AddColumnSetHandler<T>(this IPlatformMemoryStat<T> stat, PlatformMemoryColumnChangedHandler<T> handler)
		{
			stat.ColumnSetAfter += handler;
			return stat;
		}

		public static IPlatformMemoryStat<T> WithUpdatePeak<T>(this IPlatformMemoryStat<T> stat) where T : IComparable<T>
		{
			return stat.AddColumnSetHandler(delegate(MemoryStatColumn column, T value)
			{
				if (column != MemoryStatColumn.Current)
				{
					return;
				}
				T t;
				if (!stat.TryGet(MemoryStatColumn.Peak, out t) || value.CompareTo(t) > 0)
				{
					stat.Set(MemoryStatColumn.Peak, value);
				}
			});
		}

		public static IPlatformMemoryStat<T> WithUpdateMin<T>(this IPlatformMemoryStat<T> stat) where T : IComparable<T>
		{
			return stat.AddColumnSetHandler(delegate(MemoryStatColumn column, T value)
			{
				if (column != MemoryStatColumn.Current)
				{
					return;
				}
				T t;
				if (!stat.TryGet(MemoryStatColumn.Min, out t) || value.CompareTo(t) < 0)
				{
					stat.Set(MemoryStatColumn.Min, value);
				}
			});
		}

		public static bool TryGetCurrentAndLast<T>(this IPlatformMemoryStat<T> stat, MemoryStatColumn column, out T current, out T last)
		{
			if (!stat.TryGet(column, out current))
			{
				last = default(T);
				return false;
			}
			return stat.TryGetLast(column, out last);
		}

		public static bool HasColumnChanged<T>(this IPlatformMemoryStat<T> stat, MemoryStatColumn column, PlatformMemoryStatHasChangedSignificantly<T> checkCurrentVsLast)
		{
			T t;
			T t2;
			return stat.TryGetCurrentAndLast(column, out t, out t2) && checkCurrentVsLast(t, t2);
		}

		public static bool HasColumnIncreased<T>(this IPlatformMemoryStat<T> stat, MemoryStatColumn column) where T : IComparable<T>
		{
			return stat.HasColumnChanged(column, (T current, T last) => current.CompareTo(last) > 0);
		}

		public static bool HasColumnDecreased<T>(this IPlatformMemoryStat<T> stat, MemoryStatColumn column) where T : IComparable<T>
		{
			return stat.HasColumnChanged(column, (T current, T last) => current.CompareTo(last) < 0);
		}

		public static bool HasBytesChangedSignificantly(this IPlatformMemoryStat<long> stat, MemoryStatColumn column)
		{
			return stat.HasColumnChanged(column, delegate(long current, long last)
			{
				long num = Math.Abs(current - last);
				long num2;
				long num3;
				if (stat.TryGet(MemoryStatColumn.Limit, out num2) && last > num2 / 2L)
				{
					num3 = Math.Abs(num2 - last);
				}
				else
				{
					num3 = Math.Abs(last);
				}
				return num >= num3 / 128L;
			});
		}
	}
}
