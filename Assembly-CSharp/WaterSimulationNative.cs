using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

public class WaterSimulationNative
{
	public static WaterSimulationNative Instance
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	} = new WaterSimulationNative();

	public bool IsInitialized
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool IsPaused
	{
		get
		{
			return this.isPaused;
		}
	}

	public void Init(ChunkCluster _cc)
	{
		this.changeApplier = new WaterSimulationApplyChanges(_cc);
		if (!this.ShouldEnable)
		{
			return;
		}
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (this.waterDataHandles.IsCreated || this.modifiedChunks.IsCreated)
		{
			Debug.LogError("Last water simulation data was disposed of and may have leaked");
		}
		this.activeHandles = new UnsafeParallelHashSet<ChunkKey>(500, AllocatorManager.Persistent);
		this.waterDataHandles = new UnsafeParallelHashMap<ChunkKey, WaterDataHandle>(500, AllocatorManager.Persistent);
		this.modifiedChunks = new UnsafeParallelHashSet<ChunkKey>(500, AllocatorManager.Persistent);
		this.groundWaterHeightMap = new GroundWaterHeightMap(GameManager.Instance.World);
		this.IsInitialized = true;
		this.isPaused = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsChunkInWorldBounds(Chunk _c)
	{
		Vector3i vector3i;
		Vector3i vector3i2;
		GameManager.Instance.World.GetWorldExtent(out vector3i, out vector3i2);
		return _c.X * 16 >= vector3i.x && _c.X * 16 < vector3i2.x && _c.Z * 16 >= vector3i.z && _c.Z * 16 < vector3i2.z;
	}

	public void InitializeChunk(Chunk _c)
	{
		if (!this.IsInitialized)
		{
			return;
		}
		if (!this.IsChunkInWorldBounds(_c))
		{
			return;
		}
		NativeSafeHandle<WaterDataHandle> nativeSafeHandle;
		WaterDataHandle waterDataHandle;
		if (this.freeHandles.TryDequeue(out nativeSafeHandle))
		{
			waterDataHandle = nativeSafeHandle.Target;
			waterDataHandle.Clear();
		}
		else
		{
			waterDataHandle = WaterDataHandle.AllocateNew(4);
			nativeSafeHandle = new NativeSafeHandle<WaterDataHandle>(ref waterDataHandle, 4);
		}
		waterDataHandle.InitializeFromChunk(_c, this.groundWaterHeightMap);
		this.newInitializedHandles.Enqueue(new WaterSimulationNative.HandleInitRequest
		{
			chunkKey = new ChunkKey(_c),
			safeHandle = nativeSafeHandle
		});
		_c.AssignWaterSimHandle(new WaterSimulationNative.ChunkHandle(this, _c));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CopyInitializedChunksToNative()
	{
		WaterSimulationNative.HandleInitRequest handleInitRequest;
		while (this.newInitializedHandles.TryDequeue(out handleInitRequest))
		{
			ChunkKey chunkKey = handleInitRequest.chunkKey;
			NativeSafeHandle<WaterDataHandle> safeHandle = handleInitRequest.safeHandle;
			NativeSafeHandle<WaterDataHandle> nativeSafeHandle;
			if (this.usedHandles.TryGetValue(chunkKey, out nativeSafeHandle))
			{
				this.freeHandles.Enqueue(nativeSafeHandle);
				this.usedHandles[chunkKey] = safeHandle;
			}
			else
			{
				this.usedHandles.Add(chunkKey, safeHandle);
			}
			WaterDataHandle target = safeHandle.Target;
			this.waterDataHandles[chunkKey] = target;
			if (target.HasActiveWater)
			{
				this.activeHandles.Add(chunkKey);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessPendingRemoves()
	{
		ChunkKey chunkKey;
		while (this.handlesToRemove.TryDequeue(out chunkKey))
		{
			NativeSafeHandle<WaterDataHandle> nativeSafeHandle;
			if (this.usedHandles.TryGetValue(chunkKey, out nativeSafeHandle))
			{
				this.usedHandles.Remove(chunkKey);
				this.freeHandles.Enqueue(nativeSafeHandle);
			}
			this.waterDataHandles.Remove(chunkKey);
			this.activeHandles.Remove(chunkKey);
		}
	}

	public void SetPaused(bool _isPaused)
	{
		this.isPaused = _isPaused;
	}

	public void Step()
	{
		if (!this.isPaused)
		{
			this.SetPaused(true);
			return;
		}
		this.isPaused = false;
		this.Update();
		this.isPaused = true;
	}

	public unsafe void Update()
	{
		if (!this.IsInitialized)
		{
			return;
		}
		this.ProcessPendingRemoves();
		this.CopyInitializedChunksToNative();
		if (this.isPaused)
		{
			return;
		}
		if (this.changeApplier.HasNetWorkLimitBeenReached())
		{
			return;
		}
		WaterStats waterStats = default(WaterStats);
		if (!this.modifiedChunks.IsEmpty || !this.activeHandles.IsEmpty)
		{
			IJobExtensions.Run<WaterSimulationPreProcess>(new WaterSimulationPreProcess
			{
				activeChunks = this.activeHandles,
				waterDataHandles = this.waterDataHandles,
				modifiedChunks = this.modifiedChunks
			});
			if (!this.activeHandles.IsEmpty)
			{
				NativeArray<ChunkKey> nativeArray = this.activeHandles.ToNativeArray(3);
				NativeList<ChunkKey> nativeList = new NativeList<ChunkKey>(nativeArray.Length, AllocatorManager.TempJob);
				NativeArray<WaterStats> nativeArray2 = new NativeArray<WaterStats>(nativeArray.Length, 3, 1);
				WaterSimulationCalcFlows waterSimulationCalcFlows = new WaterSimulationCalcFlows
				{
					processingChunks = nativeArray,
					waterStats = nativeArray2,
					waterDataHandles = this.waterDataHandles
				};
				WaterSimulationApplyFlows waterSimulationApplyFlows = new WaterSimulationApplyFlows
				{
					processingChunks = nativeArray,
					nonFlowingChunks = nativeList.AsParallelWriter(),
					waterStats = nativeArray2,
					waterDataHandles = this.waterDataHandles,
					activeChunkSet = this.activeHandles.AsParallelWriter()
				};
				WaterSimulationPostProcess waterSimulationPostProcess = new WaterSimulationPostProcess
				{
					processingChunks = nativeArray,
					nonFlowingChunks = nativeList,
					activeChunks = this.activeHandles,
					waterDataHandles = this.waterDataHandles
				};
				int num = nativeArray.Length / JobsUtility.JobWorkerCount + 1;
				JobHandle jobHandle = IJobParallelForExtensions.Schedule<WaterSimulationCalcFlows>(waterSimulationCalcFlows, nativeArray.Length, num, default(JobHandle));
				JobHandle jobHandle2 = IJobParallelForExtensions.Schedule<WaterSimulationApplyFlows>(waterSimulationApplyFlows, nativeArray.Length, num, jobHandle);
				JobHandle jobHandle3 = IJobExtensions.Schedule<WaterSimulationPostProcess>(waterSimulationPostProcess, jobHandle2);
				JobHandle.ScheduleBatchedJobs();
				jobHandle3.Complete();
				waterStats += WaterStats.Sum(nativeArray2);
				nativeArray2.Dispose();
				nativeArray.Dispose();
				nativeList.Dispose();
			}
		}
		this.ProcessPendingRemoves();
		foreach (KeyValue<ChunkKey, WaterDataHandle> keyValue in this.waterDataHandles)
		{
			ChunkKey key = keyValue.Key;
			UnsafeParallelHashMap<ChunkKey, WaterDataHandle>.Enumerator enumerator;
			keyValue = enumerator.Current;
			WaterDataHandle waterDataHandle = *keyValue.Value;
			if (waterDataHandle.HasFlows)
			{
				using (WaterSimulationApplyChanges.ChangesForChunk.Writer changeWriter = this.changeApplier.GetChangeWriter(WorldChunkCache.MakeChunkKey(key.x, key.z)))
				{
					UnsafeParallelHashMap<int, int>.Enumerator flowVoxels = waterDataHandle.FlowVoxels;
					while (flowVoxels.MoveNext())
					{
						KeyValue<int, int> keyValue2 = flowVoxels.Current;
						int key2 = keyValue2.Key;
						int num2 = waterDataHandle.voxelData.Get(key2);
						WaterValue waterValue = new WaterValue(num2);
						changeWriter.RecordChange(key2, waterValue);
					}
					waterDataHandle.flowVoxels.Clear();
				}
			}
		}
		WaterStatsProfiler.SampleTick(waterStats);
	}

	public void Clear()
	{
		if (!this.IsInitialized)
		{
			return;
		}
		foreach (NativeSafeHandle<WaterDataHandle> nativeSafeHandle in this.usedHandles.Values)
		{
			this.freeHandles.Enqueue(nativeSafeHandle);
		}
		this.usedHandles.Clear();
		this.waterDataHandles.Clear();
		WaterSimulationNative.HandleInitRequest handleInitRequest;
		while (this.newInitializedHandles.TryDequeue(out handleInitRequest))
		{
			this.freeHandles.Enqueue(handleInitRequest.safeHandle);
		}
		this.handlesToRemove = new ConcurrentQueue<ChunkKey>();
		this.activeHandles.Clear();
		this.modifiedChunks.Clear();
	}

	public void Cleanup()
	{
		WaterSimulationApplyChanges waterSimulationApplyChanges = this.changeApplier;
		if (waterSimulationApplyChanges != null)
		{
			waterSimulationApplyChanges.Cleanup();
		}
		this.changeApplier = null;
		this.groundWaterHeightMap = null;
		if (!this.IsInitialized)
		{
			return;
		}
		this.Clear();
		NativeSafeHandle<WaterDataHandle> nativeSafeHandle;
		while (this.freeHandles.TryDequeue(out nativeSafeHandle))
		{
			nativeSafeHandle.Dispose();
		}
		foreach (NativeSafeHandle<WaterDataHandle> nativeSafeHandle2 in this.usedHandles.Values)
		{
			nativeSafeHandle2.Dispose();
		}
		this.usedHandles.Clear();
		if (this.activeHandles.IsCreated)
		{
			this.activeHandles.Dispose();
		}
		if (this.waterDataHandles.IsCreated)
		{
			this.waterDataHandles.Dispose();
		}
		if (this.modifiedChunks.IsCreated)
		{
			this.modifiedChunks.Dispose();
		}
		this.IsInitialized = false;
	}

	public string GetMemoryStats()
	{
		int count = this.usedHandles.Count;
		int count2 = this.freeHandles.Count;
		int count3 = this.newInitializedHandles.Count;
		int num = 0;
		foreach (NativeSafeHandle<WaterDataHandle> nativeSafeHandle in this.usedHandles.Values)
		{
			num += nativeSafeHandle.Target.CalculateOwnedBytes();
		}
		foreach (NativeSafeHandle<WaterDataHandle> nativeSafeHandle2 in this.freeHandles)
		{
			num += nativeSafeHandle2.Target.CalculateOwnedBytes();
		}
		foreach (WaterSimulationNative.HandleInitRequest handleInitRequest in this.newInitializedHandles)
		{
			int num2 = num;
			NativeSafeHandle<WaterDataHandle> safeHandle = handleInitRequest.safeHandle;
			num = num2 + safeHandle.Target.CalculateOwnedBytes();
		}
		int num3 = ProfilerUtils.CalculateUnsafeParallelHashSetBytes<ChunkKey>(this.activeHandles);
		num3 += ProfilerUtils.CalculateUnsafeParallelHashMapBytes<ChunkKey, WaterDataHandle>(this.waterDataHandles);
		return string.Format("Allocated Handles: {0}, Used Handles: {1}, Free Handles: {2}, Pending Handles: {3}, Handle Contents (MB): {4:F2}, Other Memory (MB): {5:F2}, Total Memory (MB): {6:F2}", new object[]
		{
			count + count2 + count3,
			count,
			count2,
			count3,
			(double)num * 9.5367431640625E-07,
			(double)num3 * 9.5367431640625E-07,
			(double)(num + num3) * 9.5367431640625E-07
		});
	}

	public string GetMemoryStatsDetailed()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Used Handles:");
		foreach (KeyValuePair<ChunkKey, NativeSafeHandle<WaterDataHandle>> keyValuePair in this.usedHandles)
		{
			stringBuilder.AppendFormat("Chunk ({0},{1}): {2}\n", keyValuePair.Key.x, keyValuePair.Key.z, keyValuePair.Value.Target.GetMemoryStats());
		}
		return stringBuilder.ToString();
	}

	public bool ShouldEnable = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isPaused;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<ChunkKey, NativeSafeHandle<WaterDataHandle>> usedHandles = new Dictionary<ChunkKey, NativeSafeHandle<WaterDataHandle>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<NativeSafeHandle<WaterDataHandle>> freeHandles = new ConcurrentQueue<NativeSafeHandle<WaterDataHandle>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<WaterSimulationNative.HandleInitRequest> newInitializedHandles = new ConcurrentQueue<WaterSimulationNative.HandleInitRequest>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ConcurrentQueue<ChunkKey> handlesToRemove = new ConcurrentQueue<ChunkKey>();

	[PublicizedFrom(EAccessModifier.Private)]
	public UnsafeParallelHashSet<ChunkKey> activeHandles;

	[PublicizedFrom(EAccessModifier.Private)]
	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	[PublicizedFrom(EAccessModifier.Private)]
	public UnsafeParallelHashSet<ChunkKey> modifiedChunks;

	[PublicizedFrom(EAccessModifier.Private)]
	public GroundWaterHeightMap groundWaterHeightMap;

	public WaterSimulationApplyChanges changeApplier;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct HandleInitRequest
	{
		public ChunkKey chunkKey;

		public NativeSafeHandle<WaterDataHandle> safeHandle;
	}

	public struct ChunkHandle
	{
		public bool IsValid
		{
			get
			{
				return this.sim != null;
			}
		}

		public ChunkHandle(WaterSimulationNative _sim, Chunk _chunk)
		{
			this.sim = _sim;
			this.chunkKey = new ChunkKey(_chunk);
		}

		public void SetWaterMass(int _x, int _y, int _z, int _mass)
		{
			if (!this.IsValid)
			{
				return;
			}
			WaterDataHandle waterDataHandle;
			if (this.sim.waterDataHandles.TryGetValue(this.chunkKey, ref waterDataHandle))
			{
				int voxelIndex = WaterDataHandle.GetVoxelIndex(_x, _y, _z);
				waterDataHandle.SetVoxelMass(voxelIndex, _mass);
				this.sim.activeHandles.Add(this.chunkKey);
			}
		}

		public void SetVoxelSolid(int _x, int _y, int _z, BlockFaceFlag _flags)
		{
			if (!this.IsValid)
			{
				return;
			}
			WaterDataHandle waterDataHandle;
			if (this.sim.waterDataHandles.TryGetValue(this.chunkKey, ref waterDataHandle))
			{
				waterDataHandle.SetVoxelSolid(_x, _y, _z, _flags);
				if (_flags != BlockFaceFlag.All)
				{
					this.WakeNeighbours(_x, _y, _z);
				}
			}
		}

		public void WakeNeighbours(int _x, int _y, int _z)
		{
			if (!this.IsValid)
			{
				return;
			}
			WaterDataHandle waterDataHandle;
			if (this.sim.waterDataHandles.TryGetValue(this.chunkKey, ref waterDataHandle))
			{
				waterDataHandle.EnqueueVoxelWakeup(_x, _y, _z);
			}
			this.sim.modifiedChunks.Add(this.chunkKey);
		}

		public void Reset()
		{
			if (this.IsValid && this.sim.IsInitialized)
			{
				this.sim.handlesToRemove.Enqueue(this.chunkKey);
			}
			this.sim = null;
			this.chunkKey = default(ChunkKey);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public WaterSimulationNative sim;

		[PublicizedFrom(EAccessModifier.Private)]
		public ChunkKey chunkKey;
	}
}
