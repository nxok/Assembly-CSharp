using System;

namespace Platform
{
	public interface IPlatformMemoryStat<T> : IPlatformMemoryStat
	{
		event PlatformMemoryColumnChangedHandler<T> ColumnSetAfter;

		PlatformMemoryRenderValue<T> RenderValue { get; set; }

		PlatformMemoryRenderDelta<T> RenderDelta { get; set; }

		void Set(MemoryStatColumn column, T value);

		bool TryGet(MemoryStatColumn column, out T value);

		bool TryGetLast(MemoryStatColumn column, out T value);
	}
}
