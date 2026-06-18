using System;

public interface IMemoryPoolableObject
{
	void Reset();

	void Cleanup();
}
