using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

[BurstCompile(CompileSynchronously = true)]
public struct WaterSimulationPostProcess : IJob
{
	public void Execute()
	{
		for (int i = 0; i < this.nonFlowingChunks.Length; i++)
		{
			ChunkKey chunkKey = this.nonFlowingChunks[i];
			WaterDataHandle waterDataHandle;
			if (this.waterDataHandles.TryGetValue(chunkKey, ref waterDataHandle))
			{
				this.activeChunks.Remove(this.nonFlowingChunks[i]);
			}
		}
		for (int j = 0; j < this.processingChunks.Length; j++)
		{
			ChunkKey chunkKey2 = this.processingChunks[j];
			WaterDataHandle waterDataHandle2;
			if (this.waterDataHandles.TryGetValue(chunkKey2, ref waterDataHandle2) && waterDataHandle2.activationsFromOtherChunks.Count > 0)
			{
				waterDataHandle2.ApplyEnqueuedActivations();
				this.activeChunks.Add(chunkKey2);
			}
		}
	}

	public NativeArray<ChunkKey> processingChunks;

	public NativeList<ChunkKey> nonFlowingChunks;

	public UnsafeParallelHashSet<ChunkKey> activeChunks;

	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;
}
