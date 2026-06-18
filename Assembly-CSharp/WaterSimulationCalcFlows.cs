using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
public struct WaterSimulationCalcFlows : IJobParallelFor
{
	public void Execute(int chunkIndex)
	{
		ChunkKey chunkKey = this.processingChunks[chunkIndex];
		this.stats = this.waterStats[chunkIndex];
		this.neighborCache = WaterNeighborCacheNative.InitializeCache(this.waterDataHandles);
		this.ProcessFlows(chunkKey);
		this.waterStats[chunkIndex] = this.stats;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcessFlows(ChunkKey chunkKey)
	{
		WaterDataHandle waterDataHandle;
		if (this.waterDataHandles.TryGetValue(chunkKey, ref waterDataHandle))
		{
			this.stats.NumChunksProcessed = this.stats.NumChunksProcessed + 1;
			if (!waterDataHandle.HasActiveWater)
			{
				return;
			}
			this.stats.NumChunksActive = this.stats.NumChunksActive + 1;
			this.neighborCache.SetChunk(chunkKey);
			UnsafeBitArraySetIndicesEnumerator activeVoxelIndices = waterDataHandle.ActiveVoxelIndices;
			while (activeVoxelIndices.MoveNext())
			{
				this.stats.NumVoxelsProcessed = this.stats.NumVoxelsProcessed + 1;
				int num = activeVoxelIndices.Current;
				int3 voxelCoords = WaterDataHandle.GetVoxelCoords(num);
				int num2 = waterDataHandle.voxelData.Get(num);
				this.neighborCache.SetVoxel(voxelCoords.x, voxelCoords.y, voxelCoords.z);
				if (waterDataHandle.IsInGroundWater(voxelCoords.x, voxelCoords.y, voxelCoords.z))
				{
					WaterVoxelState waterVoxelState = waterDataHandle.voxelState.Get(voxelCoords.x, voxelCoords.y, voxelCoords.z);
					if (waterVoxelState.IsSolid())
					{
						waterDataHandle.SetVoxelInactive(num);
						this.stats.NumVoxelsPutToSleep = this.stats.NumVoxelsPutToSleep + 1;
					}
					else if (num2 != 19500)
					{
						waterDataHandle.ApplyFlow(num, 19500);
					}
					else if (num2 > 195 && this.ProcessGroundWaterFlowSide(chunkKey, waterVoxelState, num2, WaterNeighborCacheNative.X_NEG) + this.ProcessGroundWaterFlowSide(chunkKey, waterVoxelState, num2, WaterNeighborCacheNative.X_POS) + this.ProcessGroundWaterFlowSide(chunkKey, waterVoxelState, num2, WaterNeighborCacheNative.Z_NEG) + this.ProcessGroundWaterFlowSide(chunkKey, waterVoxelState, num2, WaterNeighborCacheNative.Z_POS) < 195)
					{
						waterDataHandle.SetVoxelInactive(num);
						this.stats.NumVoxelsPutToSleep = this.stats.NumVoxelsPutToSleep + 1;
					}
				}
				else
				{
					int num3 = num2;
					if (num3 < 195)
					{
						waterDataHandle.SetVoxelInactive(num);
						this.stats.NumVoxelsPutToSleep = this.stats.NumVoxelsPutToSleep + 1;
					}
					else
					{
						int num4 = num3;
						num3 -= this.ProcessFlowBelow(waterDataHandle, num, voxelCoords.x, voxelCoords.y, voxelCoords.z, num3);
						if (num3 > 0)
						{
							num3 -= this.ProcessOverfull(waterDataHandle, num, voxelCoords.x, voxelCoords.y, voxelCoords.z, num3);
							int num5 = this.ProcessFlowSide(chunkKey, waterDataHandle, num, num3, WaterNeighborCacheNative.X_NEG);
							num5 += this.ProcessFlowSide(chunkKey, waterDataHandle, num, num3, WaterNeighborCacheNative.X_POS);
							num5 += this.ProcessFlowSide(chunkKey, waterDataHandle, num, num3, WaterNeighborCacheNative.Z_NEG);
							num5 += this.ProcessFlowSide(chunkKey, waterDataHandle, num, num3, WaterNeighborCacheNative.Z_POS);
							num3 -= num5;
							if (num3 > 0 && num4 - num3 < 195)
							{
								waterDataHandle.SetVoxelInactive(num);
								this.stats.NumVoxelsPutToSleep = this.stats.NumVoxelsPutToSleep + 1;
							}
						}
					}
				}
			}
			activeVoxelIndices.Dispose();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessFlowBelow(WaterDataHandle _chunkData, int _voxelIndex, int _x, int _y, int _z, int _mass)
	{
		int num = _y - 1;
		if (num < 0)
		{
			return 0;
		}
		if (_chunkData.voxelState.Get(_x, _y, _z).IsSolidYNeg() || _chunkData.voxelState.Get(_x, num, _z).IsSolidYPos())
		{
			return 0;
		}
		if (_chunkData.IsInGroundWater(_x, num, _z))
		{
			_chunkData.ApplyFlow(_voxelIndex, -_mass);
			this.stats.NumFlowEvents = this.stats.NumFlowEvents + 1;
			return _mass;
		}
		int num2;
		if (!this.TryGetMass(_chunkData, _x, num, _z, out num2))
		{
			return 0;
		}
		int num3 = WaterConstants.GetStableMassBelow(_mass, num2) - num2;
		if (num3 > 0)
		{
			num3 = (int)((float)num3 * 0.5f);
			num3 = math.clamp(num3, 1, _mass);
			_chunkData.ApplyFlow(_voxelIndex, -num3);
			_chunkData.ApplyFlow(_x, num, _z, num3);
			this.stats.NumFlowEvents = this.stats.NumFlowEvents + 1;
			return num3;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessOverfull(WaterDataHandle _chunkData, int _voxelIndex, int _x, int _y, int _z, int _mass)
	{
		if (_mass < 19500)
		{
			return 0;
		}
		int num = _y + 1;
		if (num > 255)
		{
			return 0;
		}
		if (_chunkData.voxelState.Get(_x, _y, _z).IsSolidYPos() || _chunkData.voxelState.Get(_x, num, _z).IsSolidYNeg())
		{
			return 0;
		}
		int num2;
		if (!this.TryGetMass(_chunkData, _x, num, _z, out num2))
		{
			return 0;
		}
		int num3 = math.min(_mass - 19500, 58500 - num2);
		if (num3 > 195)
		{
			num3 = math.clamp(num3, 1, _mass);
			_chunkData.ApplyFlow(_voxelIndex, -num3);
			_chunkData.ApplyFlow(_x, num, _z, num3);
			this.stats.NumFlowEvents = this.stats.NumFlowEvents + 1;
			return num3;
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessFlowSide(ChunkKey _chunkKey, WaterDataHandle _chunkData, int _voxelIndex, int _mass, int2 _xzOffset)
	{
		WaterVoxelState waterVoxelState = _chunkData.voxelState.Get(_voxelIndex);
		if (waterVoxelState.IsSolidXZ(_xzOffset))
		{
			return 0;
		}
		ChunkKey chunkKey;
		WaterDataHandle waterDataHandle;
		int num;
		int num2;
		int num3;
		if (this.neighborCache.TryGetNeighbor(_xzOffset, out chunkKey, out waterDataHandle, out num, out num2, out num3))
		{
			WaterVoxelState waterVoxelState2 = waterDataHandle.voxelState.Get(num, num2, num3);
			if (waterVoxelState2.IsSolidXZ(-_xzOffset))
			{
				return 0;
			}
			int num4;
			if (!this.TryGetMass(waterDataHandle, num, num2, num3, out num4))
			{
				return 0;
			}
			int num5 = num2 - 1;
			bool flag = true;
			bool flag2 = true;
			if (num5 >= 0)
			{
				flag = _chunkData.voxelState.Get(this.neighborCache.voxelX, num5, this.neighborCache.voxelZ).IsSolidYPos() || waterVoxelState.IsSolidYNeg();
				flag2 = waterDataHandle.voxelState.Get(num, num5, num3).IsSolidYPos() || waterVoxelState2.IsSolidYNeg();
			}
			int num6 = 195;
			if (flag == flag2)
			{
				if (_mass <= 4875)
				{
					return 0;
				}
			}
			else
			{
				num6 = 0;
			}
			int num7 = (int)((float)(_mass - num4) * 0.5f);
			num7 = math.clamp(num7, 0, (int)((float)_mass * 0.25f));
			if (num7 > num6)
			{
				num7 = (int)((float)num7 * 0.5f);
				num7 = math.clamp(num7, 1, _mass);
				_chunkData.ApplyFlow(_voxelIndex, -num7);
				if (_chunkKey.Equals(chunkKey))
				{
					waterDataHandle.ApplyFlow(num, num2, num3, num7);
				}
				else
				{
					waterDataHandle.EnqueueFlow(WaterDataHandle.GetVoxelIndex(num, num2, num3), num7);
				}
				this.stats.NumFlowEvents = this.stats.NumFlowEvents + 1;
				return num7;
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int ProcessGroundWaterFlowSide(ChunkKey _chunkKey, WaterVoxelState _fromVoxelState, int _mass, int2 _xzOffset)
	{
		if (_fromVoxelState.IsSolidXZ(_xzOffset))
		{
			return 0;
		}
		ChunkKey chunkKey;
		WaterDataHandle waterDataHandle;
		int num;
		int num2;
		int num3;
		if (this.neighborCache.TryGetNeighbor(_xzOffset, out chunkKey, out waterDataHandle, out num, out num2, out num3))
		{
			if (waterDataHandle.voxelState.Get(num, num2, num3).IsSolidXZ(-_xzOffset))
			{
				return 0;
			}
			int num4;
			if (!this.TryGetMass(waterDataHandle, num, num2, num3, out num4))
			{
				return 0;
			}
			int num5 = math.max(19500 - num4, 0);
			int num6 = math.min(_mass, (int)((float)num5 * 0.25f));
			if (num6 > 195)
			{
				num6 = (int)((float)num6 * 0.5f);
				num6 = math.clamp(num6, 1, _mass);
				if (_chunkKey.Equals(chunkKey))
				{
					waterDataHandle.ApplyFlow(num, num2, num3, num6);
				}
				else
				{
					waterDataHandle.EnqueueFlow(WaterDataHandle.GetVoxelIndex(num, num2, num3), num6);
				}
				this.stats.NumFlowEvents = this.stats.NumFlowEvents + 1;
				return num6;
			}
		}
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryGetMass(WaterDataHandle _chunkData, int _x, int _y, int _z, out int _mass)
	{
		if (_chunkData.IsInGroundWater(_x, _y, _z))
		{
			_mass = 19500;
			return false;
		}
		int num = _chunkData.voxelData.Get(_x, _y, _z);
		if (num > 195)
		{
			_mass = num;
			return true;
		}
		_mass = 0;
		return !_chunkData.voxelState.Get(_x, _y, _z).IsSolid();
	}

	public NativeArray<ChunkKey> processingChunks;

	public NativeArray<WaterStats> waterStats;

	public UnsafeParallelHashMap<ChunkKey, WaterDataHandle> waterDataHandles;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterStats stats;

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterNeighborCacheNative neighborCache;
}
