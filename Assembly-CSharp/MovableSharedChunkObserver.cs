using System;
using UnityEngine;

public class MovableSharedChunkObserver : IDisposable
{
	public MovableSharedChunkObserver(SharedChunkObserverCache _observerCache)
	{
		this.cache = _observerCache;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~MovableSharedChunkObserver()
	{
		this.Dispose();
	}

	public void SetPosition(Vector3 newPosition)
	{
		Vector2i vector2i = new Vector2i(World.toChunkXZ(Utils.Fastfloor(newPosition.x)), World.toChunkXZ(Utils.Fastfloor(newPosition.z)));
		if (this.observer == null || this.observer.ChunkPos != vector2i)
		{
			if (this.observer != null)
			{
				this.observer.Dispose();
			}
			this.observer = this.cache.GetSharedObserverForChunk(vector2i);
		}
	}

	public void Dispose()
	{
		if (this.observer != null)
		{
			this.observer.Dispose();
			this.observer = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ISharedChunkObserver observer;

	[PublicizedFrom(EAccessModifier.Private)]
	public SharedChunkObserverCache cache;
}
