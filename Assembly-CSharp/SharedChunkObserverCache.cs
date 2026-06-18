using System;
using System.Collections.Generic;
using UnityEngine;

public class SharedChunkObserverCache
{
	public IThreadingSemantics ThreadingSemantics
	{
		get
		{
			return this.threadingSemantics;
		}
	}

	public SharedChunkObserverCache(ChunkManager _chunkManager, int _viewDim, IThreadingSemantics _threadingSemantics)
	{
		this.chunkManager = _chunkManager;
		this.viewDim = _viewDim;
		this.threadingSemantics = _threadingSemantics;
	}

	public ISharedChunkObserver GetSharedObserverForChunk(Vector2i chunkPos)
	{
		return this.threadingSemantics.Synchronize<SharedChunkObserverCache.SharedChunkObserver>(delegate
		{
			SharedChunkObserverCache.SharedChunkObserver sharedChunkObserver;
			if (this.observers.TryGetValue(chunkPos, out sharedChunkObserver) && sharedChunkObserver.refCount < 1)
			{
				sharedChunkObserver = null;
			}
			if (sharedChunkObserver != null)
			{
				sharedChunkObserver.Reference();
			}
			else
			{
				sharedChunkObserver = new SharedChunkObserverCache.SharedChunkObserver(this, this.chunkManager.AddChunkObserver(new Vector3((float)(chunkPos.x << 4), 0f, (float)(chunkPos.y << 4)), false, this.viewDim, -1), new SharedChunkObserverCache.SharedChunkObserver.RemoveObserver(this.removeChunkObserver), chunkPos);
				this.observers[chunkPos] = sharedChunkObserver;
			}
			return sharedChunkObserver;
		});
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeChunkObserver(SharedChunkObserverCache.SharedChunkObserver observer)
	{
		this.threadingSemantics.Synchronize(delegate
		{
			if (this.observers[observer.chunkPos] == observer)
			{
				this.observers.Remove(observer.chunkPos);
			}
		});
		this.chunkManager.RemoveChunkObserver(observer.chunkRef);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IThreadingSemantics threadingSemantics;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<Vector2i, SharedChunkObserverCache.SharedChunkObserver> observers = new Dictionary<Vector2i, SharedChunkObserverCache.SharedChunkObserver>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkManager chunkManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public int viewDim;

	[PublicizedFrom(EAccessModifier.Private)]
	public class SharedChunkObserver : ISharedChunkObserver, IDisposable
	{
		public SharedChunkObserver(SharedChunkObserverCache _cache, ChunkManager.ChunkObserver _chunkRef, SharedChunkObserverCache.SharedChunkObserver.RemoveObserver _removeObserverDelegate, Vector2i _chunkPos)
		{
			this.cache = _cache;
			this.chunkRef = _chunkRef;
			this.removeObserver = _removeObserverDelegate;
			this.chunkPos = _chunkPos;
			this.refCount = 1;
		}

		public void Reference()
		{
			if (this.cache.ThreadingSemantics.InterlockedAdd(ref this.refCount, 1) < 2)
			{
				throw new Exception("Synchronization error: shared chunk observer was already disposed with a ref count of zero!");
			}
		}

		public void Dispose()
		{
			if (this.cache.ThreadingSemantics.InterlockedAdd(ref this.refCount, -1) == 0)
			{
				this.removeObserver(this);
			}
		}

		public Vector2i ChunkPos
		{
			get
			{
				return this.chunkPos;
			}
		}

		public SharedChunkObserverCache Owner
		{
			get
			{
				return this.cache;
			}
		}

		public Vector2i chunkPos;

		public int refCount;

		public ChunkManager.ChunkObserver chunkRef;

		[PublicizedFrom(EAccessModifier.Private)]
		public SharedChunkObserverCache cache;

		[PublicizedFrom(EAccessModifier.Private)]
		public SharedChunkObserverCache.SharedChunkObserver.RemoveObserver removeObserver;

		public delegate void RemoveObserver(SharedChunkObserverCache.SharedChunkObserver observer);
	}
}
