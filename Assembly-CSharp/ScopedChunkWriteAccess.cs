using System;

public struct ScopedChunkWriteAccess : IDisposable
{
	public Chunk Chunk
	{
		readonly get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ScopedChunkWriteAccess(Chunk chunk)
	{
		this.Chunk = chunk;
		Chunk chunk2 = this.Chunk;
		if (chunk2 == null)
		{
			return;
		}
		chunk2.EnterWriteLock();
	}

	public void Dispose()
	{
		Chunk chunk = this.Chunk;
		if (chunk == null)
		{
			return;
		}
		chunk.ExitWriteLock();
	}
}
