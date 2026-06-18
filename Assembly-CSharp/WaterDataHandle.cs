using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public struct WaterDataHandle : IDisposable
{
	public UnsafeBitArraySetIndicesEnumerator ActiveVoxelIndices
	{
		get
		{
			return new UnsafeBitArraySetIndicesEnumerator(this.activeVoxels);
		}
	}

	public UnsafeParallelHashMap<int, int>.Enumerator FlowVoxels
	{
		get
		{
			return this.flowVoxels.GetEnumerator();
		}
	}

	public bool HasActiveWater
	{
		get
		{
			return this.activeVoxels.TestAny(0, this.activeVoxels.Length);
		}
	}

	public bool HasFlows
	{
		get
		{
			return !this.flowVoxels.IsEmpty;
		}
	}

	public static WaterDataHandle AllocateNew(Allocator allocator)
	{
		WaterDataHandle waterDataHandle = default(WaterDataHandle);
		waterDataHandle.Allocate(allocator);
		return waterDataHandle;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Allocate(Allocator allocator)
	{
		this.voxelData = new UnsafeChunkData<int>(allocator);
		this.voxelState = new UnsafeChunkData<WaterVoxelState>(allocator);
		this.groundWaterHeights = new UnsafeChunkXZMap<GroundWaterBounds>(allocator);
		this.activeVoxels = new UnsafeBitArray(65536, allocator, 1);
		this.flowVoxels = new UnsafeParallelHashMap<int, int>(1000, allocator);
		this.flowsFromOtherChunks = new UnsafeFixedBuffer<WaterFlow>(16384, allocator);
		this.activationsFromOtherChunks = new UnsafeFixedBuffer<int>(16384, allocator);
		this.voxelsToWakeup = new UnsafeParallelHashSet<int>(256, allocator);
	}

	public bool IsInGroundWater(int _x, int _y, int _z)
	{
		GroundWaterBounds groundWaterBounds = this.groundWaterHeights.Get(_x, _z);
		return groundWaterBounds.IsGroundWater && _y >= (int)groundWaterBounds.bottom && _y <= (int)groundWaterBounds.waterHeight;
	}

	public void SetVoxelActive(int _x, int _y, int _z)
	{
		this.activeVoxels.Set(WaterDataHandle.GetVoxelIndex(_x, _y, _z), true);
	}

	public void SetVoxelActive(int _index)
	{
		this.activeVoxels.Set(_index, true);
	}

	public void EnqueueVoxelActive(int _x, int _y, int _z)
	{
		this.EnqueueVoxelActive(WaterDataHandle.GetVoxelIndex(_x, _y, _z));
	}

	public void EnqueueVoxelActive(int _index)
	{
		this.activationsFromOtherChunks.AddThreadSafe(_index);
	}

	public void EnqueueVoxelWakeup(int _x, int _y, int _z)
	{
		this.EnqueueVoxelWakeup(WaterDataHandle.GetVoxelIndex(_x, _y, _z));
	}

	public void EnqueueVoxelWakeup(int _index)
	{
		this.voxelsToWakeup.Add(_index);
	}

	public void ApplyEnqueuedActivations()
	{
		NativeArray<int> nativeArray = this.activationsFromOtherChunks.AsNativeArray();
		for (int i = 0; i < nativeArray.Length; i++)
		{
			int num = nativeArray[i];
			this.SetVoxelActive(num);
		}
		this.activationsFromOtherChunks.Clear();
	}

	public void SetVoxelInactive(int _index)
	{
		this.activeVoxels.Set(_index, false);
	}

	public void SetVoxelMass(int _x, int _y, int _z, int _mass)
	{
		int voxelIndex = WaterDataHandle.GetVoxelIndex(_x, _y, _z);
		this.SetVoxelMass(voxelIndex, _mass);
	}

	public void SetVoxelMass(int _index, int _mass)
	{
		if (_mass > 195)
		{
			this.activeVoxels.Set(_index, true);
		}
		else
		{
			this.activeVoxels.Set(_index, false);
		}
		this.voxelData.Set(_index, _mass);
	}

	public void SetVoxelSolid(int _x, int _y, int _z, BlockFaceFlag _flags)
	{
		int voxelIndex = WaterDataHandle.GetVoxelIndex(_x, _y, _z);
		WaterVoxelState waterVoxelState = this.voxelState.Get(voxelIndex);
		WaterVoxelState waterVoxelState2 = default(WaterVoxelState);
		waterVoxelState2.SetSolid(_flags);
		this.voxelState.Set(voxelIndex, waterVoxelState2);
		GroundWaterBounds groundWaterBounds = this.groundWaterHeights.Get(_x, _z);
		if (groundWaterBounds.IsGroundWater)
		{
			if (waterVoxelState.IsSolidYNeg() && !waterVoxelState2.IsSolidYNeg() && _y == (int)groundWaterBounds.bottom)
			{
				groundWaterBounds.bottom = (byte)this.FindGroundWaterBottom(voxelIndex);
				this.groundWaterHeights.Set(_x, _z, groundWaterBounds);
				return;
			}
			if (waterVoxelState.IsSolidYPos() && !waterVoxelState2.IsSolidYPos() && _y + 1 == (int)groundWaterBounds.bottom)
			{
				groundWaterBounds.bottom = (byte)this.FindGroundWaterBottom(voxelIndex);
				this.groundWaterHeights.Set(_x, _z, groundWaterBounds);
				return;
			}
			if (!waterVoxelState.IsSolidYNeg() && waterVoxelState2.IsSolidYNeg() && _y > (int)groundWaterBounds.bottom && _y <= (int)groundWaterBounds.waterHeight)
			{
				groundWaterBounds.bottom = (byte)_y;
				this.groundWaterHeights.Set(_x, _z, groundWaterBounds);
				return;
			}
			if (!waterVoxelState.IsSolidYPos() && waterVoxelState2.IsSolidYPos())
			{
				int num = _y + 1;
				if (num > (int)groundWaterBounds.bottom && num <= (int)groundWaterBounds.waterHeight)
				{
					groundWaterBounds.bottom = (byte)num;
					this.groundWaterHeights.Set(_x, _z, groundWaterBounds);
				}
			}
		}
	}

	public void ApplyFlow(int _x, int _y, int _z, int _flow)
	{
		this.ApplyFlow(WaterDataHandle.GetVoxelIndex(_x, _y, _z), _flow);
	}

	public void ApplyFlow(int _index, int _flow)
	{
		int num;
		if (this.flowVoxels.TryGetValue(_index, ref num))
		{
			_flow += num;
		}
		this.flowVoxels[_index] = _flow;
	}

	public void EnqueueFlow(int _voxelIndex, int _flow)
	{
		this.flowsFromOtherChunks.AddThreadSafe(new WaterFlow
		{
			voxelIndex = _voxelIndex,
			flow = _flow
		});
	}

	public void ApplyEnqueuedFlows()
	{
		NativeArray<WaterFlow> nativeArray = this.flowsFromOtherChunks.AsNativeArray();
		for (int i = 0; i < nativeArray.Length; i++)
		{
			WaterFlow waterFlow = nativeArray[i];
			this.ApplyFlow(waterFlow.voxelIndex, waterFlow.flow);
		}
		this.flowsFromOtherChunks.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int FindGroundWaterBottom(int _fromIndex)
	{
		for (int i = _fromIndex; i >= 0; i -= 256)
		{
			WaterVoxelState waterVoxelState = this.voxelState.Get(i);
			if (waterVoxelState.IsSolidYNeg())
			{
				return WaterDataHandle.GetVoxelY(i);
			}
			if (waterVoxelState.IsSolidYPos())
			{
				int num = math.min(i + 256, 255);
				if (num <= _fromIndex)
				{
					return WaterDataHandle.GetVoxelY(num);
				}
			}
		}
		return 0;
	}

	public void InitializeFromChunk(Chunk _chunk, GroundWaterHeightMap _groundWaterHeightMap)
	{
		if (!this.voxelData.IsCreated || !this.activeVoxels.IsCreated)
		{
			Debug.LogError("Could not initialize WaterDataHandle because it has not been allocated");
			return;
		}
		this.Clear();
		for (int i = 0; i < 256; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					WaterVoxelState waterVoxelState = default(WaterVoxelState);
					BlockValue blockNoDamage = _chunk.GetBlockNoDamage(k, i, j);
					Block block = blockNoDamage.Block;
					byte rotation = blockNoDamage.rotation;
					waterVoxelState.SetSolid(BlockFaceFlags.RotateFlags(block.WaterFlowMask, rotation));
					int voxelIndex = WaterDataHandle.GetVoxelIndex(k, i, j);
					int mass = _chunk.GetWater(k, i, j).GetMass();
					if (mass > 195)
					{
						this.activeVoxels.Set(voxelIndex, true);
						this.voxelData.Set(voxelIndex, mass);
					}
					if (!waterVoxelState.IsDefault())
					{
						this.voxelState.Set(voxelIndex, waterVoxelState);
					}
				}
			}
		}
		this.voxelState.CheckSameValues();
		if (_groundWaterHeightMap.TryInit())
		{
			for (int l = 0; l < 16; l++)
			{
				for (int m = 0; m < 16; m++)
				{
					Vector3i vector3i = _chunk.ToWorldPos(m, 0, l);
					int num;
					if (_groundWaterHeightMap.TryGetWaterHeightAt(vector3i.x, vector3i.z, out num))
					{
						int num2 = this.FindGroundWaterBottom(WaterDataHandle.GetVoxelIndex(m, num, l));
						this.groundWaterHeights.Set(m, l, new GroundWaterBounds(num2, num));
					}
				}
			}
		}
	}

	public void Clear()
	{
		if (this.voxelData.IsCreated)
		{
			this.voxelData.Clear();
		}
		if (this.voxelState.IsCreated)
		{
			this.voxelState.Clear();
		}
		if (this.groundWaterHeights.IsCreated)
		{
			this.groundWaterHeights.Clear();
		}
		if (this.activeVoxels.IsCreated)
		{
			this.activeVoxels.Clear();
		}
		if (this.flowVoxels.IsCreated)
		{
			this.flowVoxels.Clear();
		}
		if (this.flowsFromOtherChunks.IsCreated)
		{
			this.flowsFromOtherChunks.Clear();
		}
		if (this.activationsFromOtherChunks.IsCreated)
		{
			this.activationsFromOtherChunks.Clear();
		}
		if (this.voxelsToWakeup.IsCreated)
		{
			this.voxelsToWakeup.Clear();
		}
	}

	public void Dispose()
	{
		if (this.voxelData.IsCreated)
		{
			this.voxelData.Dispose();
		}
		if (this.voxelState.IsCreated)
		{
			this.voxelState.Dispose();
		}
		if (this.groundWaterHeights.IsCreated)
		{
			this.groundWaterHeights.Dispose();
		}
		if (this.activeVoxels.IsCreated)
		{
			this.activeVoxels.Dispose();
		}
		if (this.flowVoxels.IsCreated)
		{
			this.flowVoxels.Dispose();
		}
		if (this.flowsFromOtherChunks.IsCreated)
		{
			this.flowsFromOtherChunks.Dispose();
		}
		if (this.activationsFromOtherChunks.IsCreated)
		{
			this.activationsFromOtherChunks.Dispose();
		}
		if (this.voxelsToWakeup.IsCreated)
		{
			this.voxelsToWakeup.Dispose();
		}
	}

	public int CalculateOwnedBytes()
	{
		return 0 + this.voxelData.CalculateOwnedBytes() + this.voxelState.CalculateOwnedBytes() + this.groundWaterHeights.CalculateOwnedBytes() + ProfilerUtils.CalculateUnsafeBitArrayBytes(this.activeVoxels) + ProfilerUtils.CalculateUnsafeParallelHashMapBytes<int, int>(this.flowVoxels) + this.flowsFromOtherChunks.CalculateOwnedBytes() + this.activationsFromOtherChunks.CalculateOwnedBytes() + ProfilerUtils.CalculateUnsafeParallelHashSetBytes<int>(this.voxelsToWakeup);
	}

	public string GetMemoryStats()
	{
		return string.Format("voxelData: {0:F2} KB, voxelState: {1:F2} KB, groundWaterHeights: {2:F2} KB, activeVoxels: ({3:F2} KB), flowVoxels: ({4},{5},{6:F2} KB), flowsFromOtherChunks: {7:F2} KB, activationsFromOtherChunks: {8:F2} KB, voxelsToWakeup {9:F2} KB, Total: {10:F2} MB", new object[]
		{
			(double)this.voxelData.CalculateOwnedBytes() * 0.0009765625,
			(double)this.voxelState.CalculateOwnedBytes() * 0.0009765625,
			(double)this.groundWaterHeights.CalculateOwnedBytes() * 0.0009765625,
			(double)ProfilerUtils.CalculateUnsafeBitArrayBytes(this.activeVoxels) * 0.0009765625,
			this.flowVoxels.Count(),
			this.flowVoxels.Capacity,
			(double)ProfilerUtils.CalculateUnsafeParallelHashMapBytes<int, int>(this.flowVoxels) * 0.0009765625,
			(double)this.flowsFromOtherChunks.CalculateOwnedBytes() * 0.0009765625,
			(double)this.activationsFromOtherChunks.CalculateOwnedBytes() * 0.0009765625,
			(double)ProfilerUtils.CalculateUnsafeParallelHashSetBytes<int>(this.voxelsToWakeup) * 0.0009765625,
			(double)this.CalculateOwnedBytes() * 9.5367431640625E-07
		});
	}

	public static int GetVoxelIndex(int _x, int _y, int _z)
	{
		return _x + _y * 256 + _z * 16;
	}

	public static int3 GetVoxelCoords(int index)
	{
		int3 @int = default(int3);
		@int.y = index / 256;
		int num = index % 256;
		@int.z = num / 16;
		@int.x = num % 16;
		return @int;
	}

	public static int GetVoxelY(int _index)
	{
		return _index / 256;
	}

	public UnsafeChunkData<int> voxelData;

	public UnsafeChunkData<WaterVoxelState> voxelState;

	public UnsafeChunkXZMap<GroundWaterBounds> groundWaterHeights;

	public UnsafeBitArray activeVoxels;

	public UnsafeParallelHashMap<int, int> flowVoxels;

	public UnsafeFixedBuffer<WaterFlow> flowsFromOtherChunks;

	public UnsafeFixedBuffer<int> activationsFromOtherChunks;

	public UnsafeParallelHashSet<int> voxelsToWakeup;
}
