using System;
using System.Runtime.CompilerServices;

public abstract class PinnedBuffer : IDisposable
{
	public static PinnedBuffer<T> Create<[IsUnmanaged] T>(ArrayListMP<T> list, PinnedBuffer.Ownership ownership) where T : struct, ValueType
	{
		if (list == null || list.Count <= 0)
		{
			return new PinnedBuffer<T>();
		}
		switch (ownership)
		{
		case PinnedBuffer.Ownership.Pin:
			return new PinnedBuffer<T>(list.Items, list.Count);
		case PinnedBuffer.Ownership.Clone:
		{
			PinnedBuffer<T> pinnedBuffer = new PinnedBuffer<T>(list.pool, list.Count);
			list.Items.AsSpan(0, list.Count).CopyTo(pinnedBuffer.AsSpan());
			return pinnedBuffer;
		}
		case PinnedBuffer.Ownership.Steal:
		{
			T[] items = list.Items;
			int count = list.Count;
			list.Items = null;
			list.Count = 0;
			return new PinnedBuffer<T>(list.pool, items, count);
		}
		default:
			throw new ArgumentOutOfRangeException("ownership", ownership, null);
		}
	}

	public abstract void Dispose();

	[PublicizedFrom(EAccessModifier.Protected)]
	public PinnedBuffer()
	{
	}

	public enum Ownership
	{
		Pin,
		Clone,
		Steal
	}
}
