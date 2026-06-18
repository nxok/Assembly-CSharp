using System;
using System.Collections;
using UnityEngine;

public class MapVisitor
{
	public event MapVisitor.VisitChunkDelegate OnVisitChunk;

	public event MapVisitor.VisitMapDoneDelegate OnVisitMapDone;

	public Vector3i ChunkPosStart
	{
		get
		{
			return this.chunkPos1;
		}
	}

	public Vector3i ChunkPosEnd
	{
		get
		{
			return this.chunkPos2;
		}
	}

	public Vector3i WorldPosStart
	{
		get
		{
			return new Vector3i(this.chunkPos1.x << 4, 0, this.chunkPos1.z << 4);
		}
	}

	public Vector3i WorldPosEnd
	{
		get
		{
			return new Vector3i((this.chunkPos2.x + 1 << 4) - 1, 255, (this.chunkPos2.z + 1 << 4) - 1);
		}
	}

	public MapVisitor(Vector3i _worldPos1, Vector3i _worldPos2)
	{
		int x = _worldPos1.x;
		int z = _worldPos1.z;
		int x2 = _worldPos2.x;
		int z2 = _worldPos2.z;
		this.chunkPos1 = new Vector3i(World.toChunkXZ((x <= x2) ? x : x2), 0, World.toChunkXZ((z <= z2) ? z : z2));
		this.chunkPos2 = new Vector3i(World.toChunkXZ((x <= x2) ? x2 : x), 0, World.toChunkXZ((z <= z2) ? z2 : z));
	}

	public void Start()
	{
		if (!this.hasBeenStarted)
		{
			this.coroutine = ThreadManager.StartCoroutine(this.visitCo());
			this.hasBeenStarted = true;
		}
	}

	public void Stop()
	{
		if (this.hasBeenStarted && this.coroutine != null)
		{
			ThreadManager.StopCoroutine(this.coroutine);
			GameManager.Instance.RemoveChunkObserver(this.observer);
			this.observer = null;
			this.coroutine = null;
		}
	}

	public bool IsRunning()
	{
		return this.coroutine != null;
	}

	public bool HasBeenStarted()
	{
		return this.hasBeenStarted;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator visitCo()
	{
		int viewDim = 8;
		int num = this.chunkPos2.x - this.chunkPos1.x + 1;
		int num2 = this.chunkPos2.z - this.chunkPos1.z + 1;
		int chunksTotal = num * num2;
		int chunksDone = 0;
		float startTime = Time.time;
		int curChunkX = Math.Min(this.chunkPos1.x + viewDim, this.chunkPos2.x);
		int curChunkZ = Math.Min(this.chunkPos1.z + viewDim, this.chunkPos2.z);
		this.observer = GameManager.Instance.AddChunkObserver(this.chunkPosToBlockPos(curChunkX, curChunkZ), false, viewDim, -1);
		yield return null;
		while (curChunkX - viewDim <= this.chunkPos2.x && curChunkZ - viewDim <= this.chunkPos2.z)
		{
			this.observer.SetPosition(this.chunkPosToBlockPos(curChunkX, curChunkZ));
			int num3;
			for (int xOffset = -viewDim; xOffset <= viewDim; xOffset = num3 + 1)
			{
				for (int zOffset = -viewDim; zOffset <= viewDim; zOffset = num3 + 1)
				{
					if (curChunkX + xOffset >= this.chunkPos1.x && curChunkZ + zOffset >= this.chunkPos1.z && curChunkX + xOffset <= this.chunkPos2.x && curChunkZ + zOffset <= this.chunkPos2.z)
					{
						Chunk chunk;
						while ((chunk = GameManager.Instance.World.GetChunkSync(curChunkX + xOffset, curChunkZ + zOffset) as Chunk) == null || chunk.NeedsDecoration)
						{
							yield return null;
						}
						num3 = chunksDone;
						chunksDone = num3 + 1;
						if (this.OnVisitChunk != null)
						{
							float num4 = Time.time - startTime;
							this.OnVisitChunk(chunk, chunksDone, chunksTotal, num4);
						}
					}
					num3 = zOffset;
				}
				num3 = xOffset;
			}
			curChunkX += viewDim * 2 + 1;
			if (curChunkX - viewDim > this.chunkPos2.x)
			{
				curChunkX = Math.Min(this.chunkPos1.x + viewDim, this.chunkPos2.x);
				curChunkZ += viewDim * 2 + 1;
			}
		}
		yield return null;
		GameManager.Instance.RemoveChunkObserver(this.observer);
		this.observer = null;
		float num5 = Time.time - startTime;
		if (this.OnVisitMapDone != null)
		{
			this.OnVisitMapDone(chunksDone, num5);
		}
		this.coroutine = null;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 chunkPosToBlockPos(int _x, int _z)
	{
		return new Vector3((float)this.chunkXZtoBlockXZ(_x), 0f, (float)this.chunkXZtoBlockXZ(_z));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkXZtoBlockXZ(int _xz)
	{
		if (_xz >= 0)
		{
			return _xz * 16;
		}
		return _xz * 16 + 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine coroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkManager.ChunkObserver observer;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Vector3i chunkPos1;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Vector3i chunkPos2;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hasBeenStarted;

	public delegate void VisitChunkDelegate(Chunk _chunk, int _chunksVisited, int _chunksTotal, float _elapsedSeconds);

	public delegate void VisitMapDoneDelegate(int _chunks, float _elapsedSeconds);
}
