using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile(CompileSynchronously = true)]
public struct WaterSimulationPreProcess : IJob
{
	public void Execute()
	{
		this.neighborCache = WaterNeighborCacheNative.InitializeCache(this.waterDataHandles);
		using (UnsafeParallelHashSet<ChunkKey>.Enumerator enumerator = this.modifiedChunks.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				ChunkKey chunkKey = enumerator.Current;
				WaterDataHandle waterDataHandle;
				if (this.waterDataHandles.TryGetValue(chunkKey, ref waterDataHandle))
				{
					this.neighborCache.SetChunk(chunkKey);
					int num = 0;
					using (UnsafeParallelHashSet<int>.Enumerator enumerator2 = waterDataHandle.voxelsToWakeup.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							if (++num > 65536)
							{
								Debug.LogError(string.Format("[WaterSimulationPreProcess] Number of wakeups for chunk ({0}, {1}) has exceeded the volume of a chunk {2}.", chunkKey.x, chunkKey.z, 65536));
								break;
							}
							int3 voxelCoords = WaterDataHandle.GetVoxelCoords(enumerator2.Current);
							waterDataHandle.SetVoxelActive(voxelCoords.x, voxelCoords.y, voxelCoords.z);
							this.neighborCache.SetVoxel(voxelCoords.x, voxelCoords.y, voxelCoords.z);
							this.WakeNeighbor(WaterNeighborCacheNative.X_NEG);
							this.WakeNeighbor(WaterNeighborCacheNative.X_POS);
							this.WakeNeighbor(1);
							this.WakeNeighbor(-1);
							this.WakeNeighbor(WaterNeighborCacheNative.Z_NEG);
							this.WakeNeighbor(WaterNeighborCacheNative.Z_POS);
						}
						waterDataHandle.voxelsToWakeup.Clear();
						this.activeChunks.Add(chunkKey);
					}
				}
			}
			this.modifiedChunks.Clear();
			using (NativeArray<ChunkKey> nativeArray = this.activeChunks.ToNativeArray(2))
			{
				for (int i = 0; i < nativeArray.Length; i++)
				{
					ChunkKey chunkKey2 = nativeArray[i];
					this.TryTrackChunk(chunkKey2.x + 1, chunkKey2.z);
					this.TryTrackChunk(chunkKey2.x - 1, chunkKey2.z);
					this.TryTrackChunk(chunkKey2.x, chunkKey2.z + 1);
					this.TryTrackChunk(chunkKey2.x, chunkKey2.z - 1);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WakeNeighbor(int _yOffset)
	{
		int num = this.neighborCache.voxelY + _yOffset;
		if (num < 0 || num > 255)
		{
			return;
		}
		this.neighborCache.center.SetVoxelActive(this.neighborCache.voxelX, num, this.neighborCache.voxelZ);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WakeNeighbor(int2 _xzOffset)
	{
		ChunkKey chunkKey;
		WaterDataHandle waterDataHandle;
		int num;
		int num2;
		int num3;
		if (this.neighborCache.TryGetNeighbor(_xzOffset, out chunkKey, out waterDataHandle, out num, out num2, out num3))
		{
			waterDataHandle.SetVoxelActive(num, num2, num3);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TryTrackChunk(int _chunkX, int _chunkZ)
	{
		ChunkKey chunkKey = new ChunkKey(_chunkX, _chunkZ);
		if (this.waterDataHandles.ContainsKey(chunkKey))
		{
			this.activeChunks.Add(chunkKey);
		}
	}

	public UnsafeParallelHashSet<ChunkKey> activeChunks;

	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	public UnsafeParallelHashSet<ChunkKey> modifiedChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterNeighborCacheNative neighborCache;
}
