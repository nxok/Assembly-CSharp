using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
public struct WaterSimulationApplyFlows : IJobParallelFor
{
	public unsafe void Execute(int chunkIndex)
	{
		this.neighborCache = WaterNeighborCacheNative.InitializeCache(this.waterDataHandles);
		ChunkKey chunkKey = this.processingChunks[chunkIndex];
		this.stats = this.waterStats[chunkIndex];
		WaterDataHandle waterDataHandle;
		if (this.waterDataHandles.TryGetValue(chunkKey, ref waterDataHandle))
		{
			waterDataHandle.ApplyEnqueuedFlows();
			if (waterDataHandle.flowVoxels.IsEmpty)
			{
				this.nonFlowingChunks.AddNoResize(chunkKey);
			}
			else
			{
				this.neighborCache.SetChunk(chunkKey);
				foreach (KeyValue<int, int> keyValue in waterDataHandle.flowVoxels)
				{
					int key = keyValue.Key;
					UnsafeParallelHashMap<int, int>.Enumerator enumerator;
					keyValue = enumerator.Current;
					int num = *keyValue.Value;
					int3 voxelCoords = WaterDataHandle.GetVoxelCoords(key);
					int num2 = waterDataHandle.voxelData.Get(key);
					if (waterDataHandle.IsInGroundWater(voxelCoords.x, voxelCoords.y, voxelCoords.z))
					{
						num2 = math.min(num, 19500);
					}
					else
					{
						if (num == 0)
						{
							continue;
						}
						num2 += num;
					}
					waterDataHandle.voxelData.Set(key, num2);
					waterDataHandle.SetVoxelActive(key);
					this.neighborCache.SetVoxel(voxelCoords.x, voxelCoords.y, voxelCoords.z);
					this.WakeNeighbor(WaterNeighborCacheNative.X_NEG);
					this.WakeNeighbor(WaterNeighborCacheNative.X_POS);
					this.WakeNeighbor(1);
					this.WakeNeighbor(-1);
					this.WakeNeighbor(WaterNeighborCacheNative.Z_NEG);
					this.WakeNeighbor(WaterNeighborCacheNative.Z_POS);
				}
				this.activeChunkSet.Add(chunkKey);
			}
		}
		this.waterStats[chunkIndex] = this.stats;
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
		this.stats.NumVoxelsWokeUp = this.stats.NumVoxelsWokeUp + 1;
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
			if (chunkKey.Equals(this.neighborCache.chunkKey))
			{
				waterDataHandle.SetVoxelActive(num, num2, num3);
			}
			else
			{
				waterDataHandle.EnqueueVoxelActive(num, num2, num3);
				this.activeChunkSet.Add(chunkKey);
			}
			this.stats.NumVoxelsWokeUp = this.stats.NumVoxelsWokeUp + 1;
		}
	}

	public NativeArray<ChunkKey> processingChunks;

	public NativeList<ChunkKey>.ParallelWriter nonFlowingChunks;

	public UnsafeParallelHashSet<ChunkKey>.ParallelWriter activeChunkSet;

	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	public NativeArray<WaterStats> waterStats;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterNeighborCacheNative neighborCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterStats stats;
}
