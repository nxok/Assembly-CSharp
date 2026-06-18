using System;

public interface ISharedChunkObserver : IDisposable
{
	void Reference();

	Vector2i ChunkPos { get; }

	SharedChunkObserverCache Owner { get; }
}
