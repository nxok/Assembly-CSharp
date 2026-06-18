using System;
using System.Runtime.CompilerServices;

public interface IBackedArrayView<[IsUnmanaged] T> : IDisposable where T : struct, ValueType
{
	int Length { get; }

	BackedArrayHandleMode Mode { get; }

	T this[int i] { get; set; }

	void Flush();
}
