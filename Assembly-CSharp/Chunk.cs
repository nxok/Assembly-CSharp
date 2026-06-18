using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Platform;
using UnityEngine;

public class Chunk : IChunk, IBlockAccess, IMemoryPoolableObject
{
	public bool StopStabilityCalculation
	{
		get
		{
			return this.stopStabilityCalculation;
		}
		set
		{
			if (value)
			{
				World world = GameManager.Instance.World;
				if (world != null)
				{
					ChunkManager chunkManager = world.m_ChunkManager;
					if (chunkManager != null)
					{
						Action<Chunk> onChunkStabilityCalculationEnabled = chunkManager.OnChunkStabilityCalculationEnabled;
						if (onChunkStabilityCalculationEnabled != null)
						{
							onChunkStabilityCalculationEnabled(this);
						}
					}
				}
			}
			this.stopStabilityCalculation = value;
		}
	}

	public void AssignWaterSimHandle(WaterSimulationNative.ChunkHandle handle)
	{
		this.waterSimHandle = handle;
	}

	public void ResetWaterSimHandle()
	{
		this.waterSimHandle.Reset();
	}

	public void AssignWaterDebugRenderer(WaterDebugManager.RendererHandle handle)
	{
		this.waterDebugHandle = handle;
	}

	public void ResetWaterDebugHandle()
	{
	}

	public byte[] GetTopSoil()
	{
		return this.m_bTopSoilBroken;
	}

	public void SetTopSoil(IList<byte> soil)
	{
		for (int i = 0; i < this.m_bTopSoilBroken.Length; i++)
		{
			this.m_bTopSoilBroken[i] = soil[i];
		}
	}

	public Chunk()
	{
		this.m_X = 0;
		this.m_Y = 0;
		this.Z = 0;
		for (int i = 0; i < this.trisInMesh.GetLength(0); i++)
		{
			this.trisInMesh[i] = new int[MeshDescription.meshes.Length];
			this.sizeOfMesh[i] = new int[MeshDescription.meshes.Length];
		}
		for (int j = 0; j < 16; j++)
		{
			this.entityLists[j] = new List<Entity>();
		}
		this.NeedsLightCalculation = true;
		this.NeedsDecoration = true;
		this.hasEntities = false;
		this.isModified = false;
		this.m_BlockLayers = new ChunkBlockLayer[64];
		this.chnLight = new ChunkBlockChannel(0L, 1);
		this.chnDensity = new ChunkBlockChannel((long)((ulong)((byte)MarchingCubes.DensityAir)), 1);
		this.chnStability = new ChunkBlockChannel(0L, 1);
		this.chnDamage = new ChunkBlockChannel(0L, 2);
		this.chnTextures = new ChunkBlockChannel[1];
		for (int k = 0; k < 1; k++)
		{
			this.chnTextures[k] = new ChunkBlockChannel(0L, 6);
		}
		this.chnWater = new ChunkBlockChannel(0L, 2);
		this.m_HeightMap = new byte[256];
		this.m_TerrainHeight = new byte[256];
		this.m_bTopSoilBroken = new byte[32];
		this.m_Biomes = new byte[256];
		this.m_BiomeIntensities = new byte[1536];
		this.m_NormalX = new byte[256];
		this.m_NormalY = new byte[256];
		this.m_NormalZ = new byte[256];
		Chunk.InstanceCount++;
	}

	public Chunk(int _x, int _z)
		: this()
	{
		this.m_X = _x;
		this.m_Y = 0;
		this.m_Z = _z;
		this.ResetStability();
		this.RefreshSunlight();
		this.NeedsLightCalculation = true;
		this.NeedsDecoration = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public ~Chunk()
	{
		Chunk.InstanceCount--;
	}

	public void ResetLights(byte _lightValue = 0)
	{
		this.chnLight.Clear((long)((ulong)_lightValue));
	}

	public void Reset()
	{
		if (this.InProgressSaving)
		{
			Log.Warning("Unloading: chunk while saving " + ((this != null) ? this.ToString() : null));
		}
		this.cachedToString = null;
		this.m_X = 0;
		this.m_Y = 0;
		this.Z = 0;
		this.MeshLayerCount = 0;
		for (int i = 0; i < 16; i++)
		{
			this.entityLists[i].Clear();
		}
		this.entityStubs.Clear();
		this.pendingEntityCreateOps.Clear();
		this.blockEntityStubs.Clear();
		this.sleeperVolumes.Clear();
		this.triggerVolumes.Clear();
		this.tileEntities.Clear();
		this.IndexedBlocks.Clear();
		this.triggerData.Clear();
		this.insideDevices.Clear();
		this.insideDevicesHashSet.Clear();
		this.NeedsRegeneration = false;
		this.NeedsDecoration = true;
		this.NeedsLightDecoration = false;
		this.NeedsLightCalculation = true;
		this.hasEntities = false;
		this.isModified = false;
		this.InProgressRegeneration = false;
		this.InProgressSaving = false;
		this.InProgressCopying = false;
		this.InProgressDecorating = false;
		this.InProgressLighting = false;
		this.InProgressUnloading = false;
		this.NeedsOnlyCollisionMesh = false;
		this.IsCollisionMeshGenerated = false;
		this.SavedInWorldTicks = 0UL;
		MemoryPools.poolCBL.FreeSync(this.m_BlockLayers);
		this.chnDensity.FreeLayers();
		this.chnStability.FreeLayers();
		this.chnLight.FreeLayers();
		this.chnDamage.FreeLayers();
		for (int j = 0; j < 1; j++)
		{
			this.chnTextures[j].FreeLayers();
		}
		this.chnWater.FreeLayers();
		this.ResetLights(0);
		Array.Clear(this.m_HeightMap, 0, this.m_HeightMap.GetLength(0));
		Array.Clear(this.m_TerrainHeight, 0, this.m_TerrainHeight.GetLength(0));
		Array.Clear(this.m_bTopSoilBroken, 0, this.m_bTopSoilBroken.GetLength(0));
		Array.Clear(this.m_Biomes, 0, this.m_Biomes.GetLength(0));
		Array.Clear(this.m_NormalX, 0, this.m_NormalX.GetLength(0));
		Array.Clear(this.m_NormalY, 0, this.m_NormalY.GetLength(0));
		Array.Clear(this.m_NormalZ, 0, this.m_NormalZ.GetLength(0));
		this.ResetBiomeIntensity(BiomeIntensity.Default);
		this.DominantBiome = 0;
		this.AreaMasterDominantBiome = byte.MaxValue;
		this.biomeSpawnData = null;
		if (this.m_DecoBiomeArray != null)
		{
			Array.Clear(this.m_DecoBiomeArray, 0, this.m_DecoBiomeArray.GetLength(0));
		}
		this.ChunkCustomData.Clear();
		this.bMapDirty = true;
		DictionaryKeyList<Vector3i, int> dictionaryKeyList = this.tickedBlocks;
		lock (dictionaryKeyList)
		{
			this.tickedBlocks.Clear();
		}
		this.bEmptyDirty = true;
		this.StopStabilityCalculation = true;
		this.waterSimHandle.Reset();
	}

	public void Cleanup()
	{
		this.waterSimHandle.Reset();
	}

	public int X
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_X;
		}
		set
		{
			this.cachedToString = null;
			this.m_X = value;
			this.updateBounds();
		}
	}

	public int Y
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_Y;
		}
	}

	public int Z
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return this.m_Z;
		}
		set
		{
			this.cachedToString = null;
			this.m_Z = value;
			this.updateBounds();
		}
	}

	public Vector3i ChunkPos
	{
		get
		{
			return new Vector3i(this.m_X, this.m_Y, this.m_Z);
		}
		set
		{
			this.cachedToString = null;
			this.m_X = value.x;
			this.m_Z = value.z;
			this.updateBounds();
		}
	}

	public long Key
	{
		get
		{
			return WorldChunkCache.MakeChunkKey(this.m_X, this.m_Z);
		}
	}

	public bool IsLocked
	{
		get
		{
			return this.InProgressCopying || this.InProgressDecorating || this.InProgressLighting || this.InProgressRegeneration || this.InProgressUnloading || this.InProgressSaving || this.InProgressNetworking || this.InProgressWaterSim;
		}
	}

	public bool IsLockedExceptUnloading
	{
		get
		{
			return this.InProgressCopying || this.InProgressDecorating || this.InProgressLighting || this.InProgressRegeneration || this.InProgressSaving || this.InProgressNetworking || this.InProgressWaterSim;
		}
	}

	public bool IsInitialized
	{
		get
		{
			return !this.NeedsLightCalculation && !this.InProgressDecorating && !this.InProgressUnloading;
		}
	}

	public bool GetAvailable()
	{
		return this.IsCollisionMeshGenerated;
	}

	public bool NeedsRegeneration
	{
		get
		{
			bool flag2;
			lock (this)
			{
				flag2 = this.m_NeedsRegenerationAtY != 0;
			}
			return flag2;
		}
		set
		{
			Queue<int> layerIndexQueue = this.m_layerIndexQueue;
			lock (layerIndexQueue)
			{
				this.MeshLayerCount = 0;
				this.m_layerIndexQueue.Clear();
				MemoryPools.poolVML.FreeSync(this.m_meshLayers);
			}
			lock (this)
			{
				if (value)
				{
					this.m_NeedsRegenerationAtY = 65535;
				}
				else
				{
					this.m_NeedsRegenerationAtY = 0;
				}
			}
			this.NeedsRegenerationDebug = this.m_NeedsRegenerationAtY;
		}
	}

	public void ClearNeedsRegenerationAt(int _idx)
	{
		lock (this)
		{
			this.m_NeedsRegenerationAtY &= ~(1 << _idx);
			this.NeedsRegenerationDebug = this.m_NeedsRegenerationAtY;
		}
	}

	public bool NeedsCopying
	{
		get
		{
			return this.HasMeshLayer();
		}
	}

	public int NeedsRegenerationAt
	{
		get
		{
			int needsRegenerationAtY;
			lock (this)
			{
				needsRegenerationAtY = this.m_NeedsRegenerationAtY;
			}
			return needsRegenerationAtY;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			lock (this)
			{
				this.m_NeedsRegenerationAtY |= 1 << (value >> 4);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void NeedsRegenerationOrBits(int _v)
	{
		lock (this)
		{
			this.m_NeedsRegenerationAtY |= _v;
		}
	}

	public bool NeedsSaving
	{
		get
		{
			return this.isModified || this.hasEntities || this.tileEntities.Count > 0 || this.triggerData.Count > 0;
		}
	}

	public void load(PooledBinaryReader stream, uint _version)
	{
		this.read(stream, _version, false);
		this.isModified = false;
	}

	public void read(PooledBinaryReader stream, uint _version)
	{
		this.read(stream, _version, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void read(PooledBinaryReader _br, uint _version, bool _bNetworkRead)
	{
		this.cachedToString = null;
		this.m_X = _br.ReadInt32();
		this.m_Y = _br.ReadInt32();
		this.Z = _br.ReadInt32();
		this.SavedInWorldTicks = _br.ReadUInt64();
		this.LastTimeRandomTicked = this.SavedInWorldTicks;
		MemoryPools.poolCBL.FreeSync(this.m_BlockLayers);
		Array.Clear(this.m_HeightMap, 0, 256);
		if (_version < 32U)
		{
			throw new Exception("Chunk version " + _version.ToString() + " not supported!");
		}
		for (int i = 0; i < 64; i++)
		{
			if (_br.ReadBoolean())
			{
				ChunkBlockLayer chunkBlockLayer = MemoryPools.poolCBL.AllocSync(false);
				chunkBlockLayer.Read(_br, _version, _bNetworkRead);
				this.m_BlockLayers[i] = chunkBlockLayer;
				this.bEmptyDirty = true;
			}
		}
		if (!_bNetworkRead)
		{
			this.chnStability.Read(_br, _version, _bNetworkRead);
		}
		_br.Flush();
		this.recalcIndexedBlocks();
		_br.Read(this.m_HeightMap, 0, 256);
		_br.Read(this.m_TerrainHeight, 0, this.m_TerrainHeight.Length);
		if (_version > 41U)
		{
			_br.Read(this.m_bTopSoilBroken, 0, 32);
		}
		_br.Read(this.m_Biomes, 0, 256);
		_br.Read(this.m_BiomeIntensities, 0, 1536);
		this.DominantBiome = _br.ReadByte();
		this.AreaMasterDominantBiome = _br.ReadByte();
		int num = (int)_br.ReadUInt16();
		this.ChunkCustomData.Clear();
		for (int j = 0; j < num; j++)
		{
			ChunkCustomData chunkCustomData = new ChunkCustomData();
			chunkCustomData.Read(_br);
			this.ChunkCustomData.Set(chunkCustomData.key, chunkCustomData);
		}
		_br.Read(this.m_NormalX, 0, 256);
		_br.Read(this.m_NormalY, 0, 256);
		_br.Read(this.m_NormalZ, 0, 256);
		this.chnDensity.Read(_br, _version, _bNetworkRead);
		this.chnLight.Read(_br, _version, _bNetworkRead);
		if (_version >= 33U && _version < 36U)
		{
			ChunkBlockChannel chunkBlockChannel = new ChunkBlockChannel(0L, 1);
			chunkBlockChannel.Read(_br, _version, _bNetworkRead);
			chunkBlockChannel.Read(_br, _version, _bNetworkRead);
		}
		if (_version >= 36U)
		{
			this.chnDamage.Read(_br, _version, _bNetworkRead);
		}
		if (_version >= 47U)
		{
			for (int k = 0; k < 1; k++)
			{
				this.chnTextures[k].Read(_br, _version, _bNetworkRead);
			}
		}
		else if (_version >= 35U)
		{
			this.chnTextures[0].Read(_br, _version, _bNetworkRead);
		}
		if (_version >= 46U)
		{
			this.chnWater.Read(_br, _version, _bNetworkRead);
		}
		else if (WaterSimulationNative.Instance.IsInitialized)
		{
			throw new Exception("Serialized data incompatible with new water simulation");
		}
		this.NeedsDecoration = false;
		this.NeedsLightCalculation = _br.ReadBoolean();
		int num2 = _br.ReadInt32();
		for (int l = 0; l < 16; l++)
		{
			this.entityLists[l].Clear();
		}
		this.entityStubs.Clear();
		for (int m = 0; m < num2; m++)
		{
			EntityCreationData entityCreationData = new EntityCreationData();
			entityCreationData.read(_br, _bNetworkRead);
			this.entityStubs.Add(entityCreationData);
		}
		this.hasEntities = this.entityStubs.Count > 0;
		num2 = _br.ReadInt32();
		this.tileEntities.Clear();
		for (int n = 0; n < num2; n++)
		{
			TileEntityType tileEntityType = (TileEntityType)_br.ReadInt32();
			TileEntity.StreamModeRead streamModeRead = (_bNetworkRead ? TileEntity.StreamModeRead.FromServer : TileEntity.StreamModeRead.Persistency);
			TileEntity tileEntity = TileEntity.InstantiateFromRead(_br, streamModeRead, tileEntityType, this, null, new Func<int, int, int, BlockValue>(this.GetBlock));
			if (tileEntity != null)
			{
				tileEntity.OnReadComplete();
				this.tileEntities.Set(tileEntity.localChunkPos, tileEntity);
			}
		}
		if (_version > 10U && _version < 43U && !_bNetworkRead)
		{
			_br.ReadUInt16();
			_br.ReadByte();
		}
		if (_version > 33U && _br.ReadBoolean())
		{
			for (int num3 = 0; num3 < 16; num3++)
			{
				_br.ReadUInt16();
			}
		}
		if (!_bNetworkRead && _version == 37U)
		{
			byte b = _br.ReadByte();
			for (int num4 = 0; num4 < (int)b; num4++)
			{
				SleeperVolume.Read(_br);
			}
		}
		if (!_bNetworkRead && _version > 37U)
		{
			this.sleeperVolumes.Clear();
			int num5 = (int)_br.ReadByte();
			for (int num6 = 0; num6 < num5; num6++)
			{
				int num7 = _br.ReadInt32();
				if (num7 < 0)
				{
					Log.Error("chunk sleeper volumeId invalid {0}", new object[] { num7 });
				}
				else
				{
					this.AddSleeperVolumeId(num7);
				}
			}
		}
		if (!_bNetworkRead && _version >= 44U)
		{
			this.triggerVolumes.Clear();
			int num8 = (int)_br.ReadByte();
			for (int num9 = 0; num9 < num8; num9++)
			{
				int num10 = _br.ReadInt32();
				if (num10 < 0)
				{
					Log.Error("chunk trigger volumeId invalid {0}", new object[] { num10 });
				}
				else
				{
					this.AddTriggerVolumeId(num10);
				}
			}
		}
		if (_version >= 45U)
		{
			this.wallVolumes.Clear();
			int num11 = (int)_br.ReadByte();
			for (int num12 = 0; num12 < num11; num12++)
			{
				int num13 = _br.ReadInt32();
				if (num13 < 0)
				{
					Log.Error("chunk wall volumeId invalid {0}", new object[] { num13 });
				}
				else
				{
					this.AddWallVolumeId(num13);
				}
			}
		}
		if (_bNetworkRead)
		{
			_br.ReadBoolean();
		}
		DictionaryKeyList<Vector3i, int> dictionaryKeyList = this.tickedBlocks;
		lock (dictionaryKeyList)
		{
			this.tickedBlocks.Clear();
			for (int num14 = 0; num14 < 64; num14++)
			{
				ChunkBlockLayer chunkBlockLayer2 = this.m_BlockLayers[num14];
				if (chunkBlockLayer2 != null)
				{
					for (int num15 = 0; num15 < 1024; num15++)
					{
						int idAt = chunkBlockLayer2.GetIdAt(num15);
						if (idAt != 0 && Block.BlocksLoaded && idAt < Block.list.Length && Block.list[idAt] != null && Block.list[idAt].IsRandomlyTick && !chunkBlockLayer2.GetAt(num15).ischild)
						{
							int num16 = num15 % 256 % 16;
							int num17 = num14 * 4 + num15 / 256;
							int num18 = num15 % 256 / 16;
							this.tickedBlocks.Add(this.ToWorldPos(num16, num17, num18), 0);
						}
					}
				}
			}
		}
		this.insideDevices.Clear();
		if (_version > 39U)
		{
			int num19 = (int)_br.ReadInt16();
			this.insideDevices.Capacity = num19;
			byte b2 = 0;
			byte b3 = 0;
			int num20 = 0;
			for (int num21 = 0; num21 < num19; num21++)
			{
				if (num20 == 0)
				{
					b2 = _br.ReadByte();
					b3 = _br.ReadByte();
					num20 = (int)_br.ReadByte();
				}
				Vector3b vector3b = new Vector3b(b2, _br.ReadByte(), b3);
				this.insideDevices.Add(vector3b);
				this.insideDevicesHashSet.Add(vector3b.GetHashCode());
				num20--;
			}
		}
		if (_version > 40U)
		{
			this.IsInternalBlocksCulled = _br.ReadBoolean();
		}
		if (_version > 42U && !_bNetworkRead)
		{
			this.triggerData.Clear();
			int num22 = (int)_br.ReadInt16();
			for (int num23 = 0; num23 < num22; num23++)
			{
				Vector3i vector3i = StreamUtils.ReadVector3i(_br);
				BlockTrigger blockTrigger = new BlockTrigger(this);
				blockTrigger.LocalChunkPos = vector3i;
				blockTrigger.Read(_br);
				this.triggerData.Add(vector3i, blockTrigger);
			}
		}
		if (_bNetworkRead)
		{
			this.ResetStabilityToBottomMost();
			this.NeedsLightCalculation = true;
		}
		this.bMapDirty = true;
		this.StopStabilityCalculation = false;
	}

	public void save(PooledBinaryWriter stream)
	{
		this.saveBlockIds();
		this.write(stream, false);
		this.isModified = false;
		this.SavedInWorldTicks = GameTimer.Instance.ticks;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveBlockIds()
	{
		if (Block.nameIdMapping != null)
		{
			NameIdMapping nameIdMapping = Block.nameIdMapping;
			NameIdMapping nameIdMapping2 = nameIdMapping;
			lock (nameIdMapping2)
			{
				for (int i = 0; i < 256; i += 4)
				{
					int num = i >> 2;
					ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[num];
					if (chunkBlockLayer == null)
					{
						Block block = BlockValue.Air.Block;
						nameIdMapping.AddMapping(block.blockID, block.GetBlockName(), false);
					}
					else
					{
						chunkBlockLayer.SaveBlockMappings(nameIdMapping);
					}
				}
			}
		}
	}

	public void write(PooledBinaryWriter stream)
	{
		this.write(stream, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void write(PooledBinaryWriter _bw, bool _bNetworkWrite)
	{
		byte[] array = MemoryPools.poolByte.Alloc(256);
		_bw.Write(this.m_X);
		_bw.Write(this.m_Y);
		_bw.Write(this.m_Z);
		_bw.Write(this.SavedInWorldTicks);
		for (int i = 0; i < 64; i++)
		{
			bool flag = this.m_BlockLayers[i] != null;
			_bw.Write(flag);
			if (flag)
			{
				this.m_BlockLayers[i].Write(_bw, _bNetworkWrite);
			}
		}
		if (!_bNetworkWrite)
		{
			this.chnStability.Write(_bw, _bNetworkWrite, array);
		}
		_bw.Write(this.m_HeightMap);
		_bw.Write(this.m_TerrainHeight);
		_bw.Write(this.m_bTopSoilBroken);
		_bw.Write(this.m_Biomes);
		_bw.Write(this.m_BiomeIntensities);
		_bw.Write(this.DominantBiome);
		_bw.Write(this.AreaMasterDominantBiome);
		int num = 0;
		if (_bNetworkWrite)
		{
			for (int j = 0; j < this.ChunkCustomData.valueList.Count; j++)
			{
				if (this.ChunkCustomData.valueList[j].isSavedToNetwork)
				{
					num++;
				}
			}
		}
		else
		{
			num = this.ChunkCustomData.valueList.Count;
		}
		_bw.Write((ushort)num);
		for (int k = 0; k < this.ChunkCustomData.valueList.Count; k++)
		{
			if (!_bNetworkWrite || this.ChunkCustomData.valueList[k].isSavedToNetwork)
			{
				this.ChunkCustomData.valueList[k].Write(_bw);
			}
		}
		_bw.Write(this.m_NormalX);
		_bw.Write(this.m_NormalY);
		_bw.Write(this.m_NormalZ);
		this.chnDensity.Write(_bw, _bNetworkWrite, array);
		this.chnLight.Write(_bw, _bNetworkWrite, array);
		this.chnDamage.Write(_bw, _bNetworkWrite, array);
		for (int l = 0; l < 1; l++)
		{
			this.chnTextures[l].Write(_bw, _bNetworkWrite, array);
		}
		this.chnWater.Write(_bw, _bNetworkWrite, array);
		_bw.Write(this.NeedsLightCalculation);
		int num2 = 0;
		for (int m = 0; m < 16; m++)
		{
			List<Entity> list = this.entityLists[m];
			for (int n = 0; n < list.Count; n++)
			{
				Entity entity = list[n];
				if (!(entity is EntityVehicle) && !(entity is EntityDrone) && ((!_bNetworkWrite && entity.IsSavedToFile()) || (_bNetworkWrite && entity.IsSavedToNetwork())))
				{
					num2++;
				}
			}
		}
		_bw.Write(num2);
		for (int num3 = 0; num3 < 16; num3++)
		{
			List<Entity> list2 = this.entityLists[num3];
			for (int num4 = 0; num4 < list2.Count; num4++)
			{
				Entity entity2 = list2[num4];
				if (!(entity2 is EntityVehicle) && !(entity2 is EntityDrone) && ((!_bNetworkWrite && entity2.IsSavedToFile()) || (_bNetworkWrite && entity2.IsSavedToNetwork())))
				{
					new EntityCreationData(entity2, true).write(_bw, _bNetworkWrite);
				}
			}
		}
		_bw.Write(this.tileEntities.Count);
		for (int num5 = 0; num5 < this.tileEntities.list.Count; num5++)
		{
			_bw.Write((int)this.tileEntities.list[num5].GetTileEntityType());
			this.tileEntities.list[num5].write(_bw, _bNetworkWrite ? TileEntity.StreamModeWrite.ToClient : TileEntity.StreamModeWrite.Persistency);
		}
		_bw.Write(false);
		if (!_bNetworkWrite)
		{
			int count = this.sleeperVolumes.Count;
			_bw.Write((byte)count);
			for (int num6 = 0; num6 < count; num6++)
			{
				_bw.Write(this.sleeperVolumes[num6]);
			}
		}
		if (!_bNetworkWrite)
		{
			int count2 = this.triggerVolumes.Count;
			_bw.Write((byte)count2);
			for (int num7 = 0; num7 < count2; num7++)
			{
				_bw.Write(this.triggerVolumes[num7]);
			}
		}
		int count3 = this.wallVolumes.Count;
		_bw.Write((byte)count3);
		for (int num8 = 0; num8 < count3; num8++)
		{
			_bw.Write(this.wallVolumes[num8]);
		}
		if (_bNetworkWrite)
		{
			_bw.Write(false);
		}
		List<byte> list3 = new List<byte>();
		int num9 = int.MaxValue;
		int num10 = int.MaxValue;
		_bw.Write((short)this.insideDevices.Count);
		foreach (Vector3b vector3b in this.insideDevices)
		{
			if (list3.Count > 254 || num9 != (int)vector3b.x || num10 != (int)vector3b.z)
			{
				if (list3.Count > 0)
				{
					_bw.Write((byte)num9);
					_bw.Write((byte)num10);
					_bw.Write((byte)list3.Count);
					for (int num11 = 0; num11 < list3.Count; num11++)
					{
						_bw.Write(list3[num11]);
					}
					list3.Clear();
				}
				num9 = (int)vector3b.x;
				num10 = (int)vector3b.z;
			}
			list3.Add(vector3b.y);
		}
		if (list3.Count > 0)
		{
			_bw.Write((byte)num9);
			_bw.Write((byte)num10);
			_bw.Write((byte)list3.Count);
			for (int num12 = 0; num12 < list3.Count; num12++)
			{
				_bw.Write(list3[num12]);
			}
		}
		_bw.Write(this.IsInternalBlocksCulled);
		if (!_bNetworkWrite)
		{
			int count4 = this.triggerData.Count;
			_bw.Write((short)count4);
			for (int num13 = 0; num13 < count4; num13++)
			{
				StreamUtils.Write(_bw, this.triggerData.list[num13].LocalChunkPos);
				this.triggerData.list[num13].Write(_bw);
			}
		}
		MemoryPools.poolByte.Free(array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void recalcIndexedBlocks()
	{
		this.IndexedBlocks.Clear();
		for (int i = 0; i < 64; i++)
		{
			ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[i];
			if (chunkBlockLayer != null)
			{
				chunkBlockLayer.AddIndexedBlocks(i, this.IndexedBlocks);
			}
		}
	}

	public void AddEntityStub(EntityCreationData _ecd)
	{
		this.entityStubs.Add(_ecd);
	}

	public BlockEntityData GetBlockEntity(Vector3i _worldPos)
	{
		BlockEntityData blockEntityData;
		this.blockEntityStubs.dict.TryGetValue(GameUtils.Vector3iToUInt64(_worldPos), out blockEntityData);
		return blockEntityData;
	}

	public BlockEntityData GetBlockEntity(Transform _transform)
	{
		for (int i = 0; i < this.blockEntityStubs.list.Count; i++)
		{
			if (this.blockEntityStubs.list[i].transform == _transform)
			{
				return this.blockEntityStubs.list[i];
			}
		}
		return null;
	}

	public void AddEntityBlockStub(BlockEntityData _ecd)
	{
		ulong num = GameUtils.Vector3iToUInt64(_ecd.pos);
		BlockEntityData blockEntityData;
		if (this.blockEntityStubs.dict.TryGetValue(num, out blockEntityData))
		{
			this.blockEntityStubsToRemove.Add(blockEntityData);
		}
		this.blockEntityStubs.Set(num, _ecd);
	}

	public void RemoveEntityBlockStub(Vector3i _pos)
	{
		ulong num = GameUtils.Vector3iToUInt64(_pos);
		BlockEntityData blockEntityData;
		if (this.blockEntityStubs.dict.TryGetValue(num, out blockEntityData))
		{
			this.blockEntityStubsToRemove.Add(blockEntityData);
			this.blockEntityStubs.Remove(num);
			return;
		}
		string text = "Entity block on pos ";
		Vector3i vector3i = _pos;
		Log.Warning(text + vector3i.ToString() + " not found!");
	}

	public int EnableEntityBlocks(bool _on, string _name)
	{
		_name = _name.ToLower();
		int num = 0;
		for (int i = 0; i < this.blockEntityStubs.list.Count; i++)
		{
			BlockEntityData blockEntityData = this.blockEntityStubs.list[i];
			if (blockEntityData.transform)
			{
				string text = blockEntityData.transform.name.ToLower();
				if (_name.Length == 0 || text.Contains(_name))
				{
					blockEntityData.transform.gameObject.SetActive(_on);
					num++;
				}
			}
		}
		return num;
	}

	public void AddInsideDevicePosition(int _blockX, int _blockY, int _blockZ, BlockValue _bv)
	{
		Vector3b vector3b = new Vector3b(_blockX, _blockY, _blockZ);
		this.insideDevices.Add(vector3b);
		this.insideDevicesHashSet.Add(vector3b.GetHashCode());
		this.IsInternalBlocksCulled = true;
	}

	public int EnableInsideBlockEntities(bool _bOn)
	{
		int num = 0;
		foreach (Vector3b vector3b in this.insideDevices)
		{
			ulong num2 = GameUtils.Vector3iToUInt64(this.ToWorldPos(vector3b.ToVector3i()));
			BlockEntityData blockEntityData;
			if (this.blockEntityStubs.dict.TryGetValue(num2, out blockEntityData) && blockEntityData.bHasTransform)
			{
				blockEntityData.transform.gameObject.SetActive(_bOn);
				num++;
			}
		}
		return num;
	}

	public void ResetStability()
	{
		this.chnStability.Clear(-1L);
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 256; k++)
				{
					int blockId = this.GetBlockId(i, k, j);
					if (blockId == 0)
					{
						break;
					}
					if (!Block.list[blockId].StabilitySupport)
					{
						this.chnStability.Set(i, k, j, 1L);
						break;
					}
					this.chnStability.Set(i, k, j, 15L);
				}
			}
		}
	}

	public void ResetStabilityToBottomMost()
	{
		this.chnStability.Clear(-1L);
		for (int i = 0; i < 16; i++)
		{
			int j = 0;
			IL_0096:
			while (j < 16)
			{
				for (int k = 0; k < 256; k++)
				{
					int blockId = this.GetBlockId(j, k, i);
					if (blockId != 0 && Block.list[blockId].StabilitySupport)
					{
						IL_008A:
						while (k < 256)
						{
							int blockId2 = this.GetBlockId(j, k, i);
							if (blockId2 == 0)
							{
								break;
							}
							if (!Block.list[blockId2].StabilitySupport)
							{
								this.chnStability.Set(j, k, i, 1L);
								break;
							}
							this.chnStability.Set(j, k, i, 15L);
							k++;
						}
						j++;
						goto IL_0096;
					}
				}
				goto IL_008A;
			}
		}
	}

	public void RefreshSunlight()
	{
		this.chnLight.SetHalf(false, 15);
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				int num = 15;
				bool flag = true;
				int k = 255;
				while (k >= 0)
				{
					int blockId = this.GetBlockId(i, k, j);
					if (!flag)
					{
						goto IL_003F;
					}
					if (blockId != 0)
					{
						flag = false;
						goto IL_003F;
					}
					IL_008D:
					k--;
					continue;
					IL_003F:
					Block block = Block.list[blockId];
					bool flag2 = block.shape.IsTerrain();
					if (!flag2)
					{
						num -= block.lightOpacity;
						if (num <= 0)
						{
							break;
						}
					}
					this.chnLight.Set(i, k, j, (long)((ulong)((byte)num)));
					if (!flag2)
					{
						goto IL_008D;
					}
					num -= block.lightOpacity;
					if (num > 0)
					{
						goto IL_008D;
					}
					break;
				}
				for (k--; k >= 0; k--)
				{
					this.chnLight.Set(i, k, j, 0L);
				}
			}
		}
		this.isModified = true;
	}

	public void SetFullSunlight()
	{
		this.chnLight.SetHalf(false, 15);
	}

	public void CopyLightsFrom(Chunk _other)
	{
		this.chnLight.CopyFrom(_other.chnLight);
		this.isModified = true;
	}

	public bool CanMobsSpawnAtPos(int _x, int _y, int _z, bool _ignoreCanMobsSpawnOn = false, bool _checkWater = true)
	{
		if (_y < 2 || _y > 251)
		{
			return false;
		}
		if (this.IsTraderArea(_x, _z))
		{
			return false;
		}
		if (_checkWater || !this.IsWater(_x, _y - 1, _z))
		{
			Block block = this.GetBlockNoDamage(_x, _y - 1, _z).Block;
			if (!_ignoreCanMobsSpawnOn && !block.CanMobsSpawnOn)
			{
				return false;
			}
			if (!block.IsCollideMovement)
			{
				return false;
			}
		}
		Block block2 = this.GetBlockNoDamage(_x, _y, _z).Block;
		if (!block2.IsCollideMovement || !block2.shape.IsSolidSpace)
		{
			Block block3 = this.GetBlockNoDamage(_x, _y + 1, _z).Block;
			if ((!block3.IsCollideMovement || !block3.shape.IsSolidSpace) && (!_checkWater || !this.IsWater(_x, _y, _z)))
			{
				return true;
			}
		}
		return false;
	}

	public bool CanSleeperSpawnAtPos(int _x, int _y, int _z, bool _checkBelow)
	{
		if (_checkBelow && !this.GetBlockNoDamage(_x, _y - 1, _z).Block.IsCollideMovement)
		{
			return false;
		}
		Block block = this.GetBlockNoDamage(_x, _y, _z).Block;
		return !block.IsCollideMovement && !block.shape.IsSolidSpace;
	}

	public bool CanPlayersSpawnAtPos(int _x, int _y, int _z, bool _allowOnAirPos = false)
	{
		if (_y < 2 || _y > 251)
		{
			return false;
		}
		Block block = this.GetBlockNoDamage(_x, _y - 1, _z).Block;
		if (!block.CanPlayersSpawnOn)
		{
			return false;
		}
		Block block2 = this.GetBlockNoDamage(_x, _y, _z).Block;
		Block block3 = this.GetBlockNoDamage(_x, _y + 1, _z).Block;
		return ((_allowOnAirPos && block.blockID == 0) || block.IsCollideMovement) && (!block2.IsCollideMovement || !block2.shape.IsSolidSpace) && !this.IsWater(_x, _y, _z) && (!block3.IsCollideMovement || !block3.shape.IsSolidSpace);
	}

	public bool IsPositionOnTerrain(int _x, int _y, int _z)
	{
		return _y >= 1 && this.GetBlockNoDamage(_x, _y - 1, _z).Block.shape.IsTerrain();
	}

	public bool FindRandomTopSoilPoint(World _world, out int x, out int y, out int z, int numTrys)
	{
		x = 0;
		y = 0;
		z = 0;
		while (numTrys-- > 0)
		{
			x = _world.GetGameRandom().RandomRange(15);
			z = _world.GetGameRandom().RandomRange(15);
			y = (int)this.GetHeight(x, z);
			if (y >= 2 && this.CanMobsSpawnAtPos(x, y, z, false, true))
			{
				x += this.m_X * 16;
				y++;
				z += this.m_Z * 16;
				return true;
			}
		}
		return false;
	}

	public bool FindRandomCavePoint(World _world, out int x, out int y, out int z, int numTrys, int relMinY)
	{
		x = 0;
		y = 0;
		z = 0;
		while (numTrys-- > 0)
		{
			x = _world.GetGameRandom().RandomRange(15);
			z = _world.GetGameRandom().RandomRange(15);
			int height = (int)this.GetHeight(x, z);
			y = height;
			while (y > height - relMinY && y > 2)
			{
				if (this.CanMobsSpawnAtPos(x, y, z, false, true))
				{
					x += this.m_X * 16;
					y++;
					z += this.m_Z * 16;
					return true;
				}
				y--;
			}
		}
		return false;
	}

	public bool FindSpawnPointAtXZ(int x, int z, out int y, int _maxLightV, int _darknessV, int startY, int endY, bool _bIgnoreCanMobsSpawnOn = false)
	{
		endY = Utils.FastClamp(endY, 1, 255);
		startY = Utils.FastClamp(startY - 1, 1, 255);
		y = endY;
		while (y > startY)
		{
			if (this.GetLightValue(x, y, z, _darknessV) <= _maxLightV)
			{
				if (this.CanMobsSpawnAtPos(x, y, z, _bIgnoreCanMobsSpawnOn, true))
				{
					y++;
					return true;
				}
				y--;
			}
		}
		return false;
	}

	public float GetLightBrightness(int x, int y, int z, int _ss)
	{
		return (float)this.GetLightValue(x, y, z, _ss) / 15f;
	}

	public int GetLightValue(int x, int y, int z, int _darknessValue)
	{
		int num = (int)this.GetLight(x, y, z, Chunk.LIGHT_TYPE.SUN);
		num -= _darknessValue;
		if (num == 15)
		{
			return num;
		}
		int light = (int)this.GetLight(x, y, z, Chunk.LIGHT_TYPE.BLOCK);
		if (num > light)
		{
			return num;
		}
		return light;
	}

	public byte GetLight(int x, int y, int z, Chunk.LIGHT_TYPE type)
	{
		x &= 15;
		z &= 15;
		int @byte = (int)this.chnLight.GetByte(x, y, z);
		if (type == Chunk.LIGHT_TYPE.SUN)
		{
			return (byte)(@byte & 15);
		}
		return (byte)(@byte >> 4);
	}

	public void SetLight(int x, int y, int z, byte intensity, Chunk.LIGHT_TYPE type)
	{
		x &= 15;
		z &= 15;
		int @byte = (int)this.chnLight.GetByte(x, y, z);
		int num = (int)intensity;
		if (type == Chunk.LIGHT_TYPE.SUN)
		{
			num |= @byte & 240;
		}
		else if (type == Chunk.LIGHT_TYPE.BLOCK)
		{
			num = (num << 4) | (@byte & 15);
		}
		if (num != @byte)
		{
			this.chnLight.Set(x, y, z, (long)((ulong)((byte)num)));
			this.NeedsRegenerationAt = y;
		}
		this.isModified = true;
	}

	public void CheckSameLight()
	{
		this.chnLight.CheckSameValue();
	}

	public void CheckSameStability()
	{
		this.chnStability.CheckSameValue();
	}

	public static bool IsNeighbourChunksDecorated(Chunk[] _neighbours)
	{
		foreach (Chunk chunk in _neighbours)
		{
			if (chunk == null || chunk.NeedsDecoration)
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsNeighbourChunksLit(Chunk[] _neighbours)
	{
		foreach (Chunk chunk in _neighbours)
		{
			if (chunk == null || chunk.NeedsLightCalculation)
			{
				return false;
			}
		}
		return true;
	}

	public Vector3i GetWorldPos()
	{
		return new Vector3i(this.m_X << 4, this.m_Y << 8, this.m_Z << 4);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetBlockWorldPosX(int _x)
	{
		return (this.m_X << 4) + _x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetBlockWorldPosZ(int _z)
	{
		return (this.m_Z << 4) + _z;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetHeight(int _x, int _z)
	{
		return this.m_HeightMap[_x + _z * 16];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetHeight(int _x, int _z, byte _h)
	{
		this.m_HeightMap[_x + _z * 16] = _h;
	}

	public byte GetMaxHeight()
	{
		byte b = 0;
		for (int i = this.m_HeightMap.Length - 1; i >= 0; i--)
		{
			byte b2 = this.m_HeightMap[i];
			if (b2 > b)
			{
				b = b2;
			}
		}
		return b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetTerrainHeight(int _x, int _z)
	{
		return this.m_TerrainHeight[_x + _z * 16];
	}

	public void SetTerrainHeight(int _x, int _z, byte _h)
	{
		this.m_TerrainHeight[_x + _z * 16] = _h;
	}

	public byte GetTopMostTerrainHeight()
	{
		byte b = 0;
		for (int i = 0; i < this.m_TerrainHeight.Length; i++)
		{
			if (this.m_TerrainHeight[i] > b)
			{
				b = this.m_TerrainHeight[i];
			}
		}
		return b;
	}

	public bool IsTopSoil(int _x, int _z)
	{
		int num = (_x + _z * 16) / 8;
		int num2 = (_x + _z * 16) % 8;
		return ((int)this.m_bTopSoilBroken[num] & (1 << num2)) == 0;
	}

	public void SetTopSoilBroken(int _x, int _z)
	{
		int num = (_x + _z * 16) / 8;
		int num2 = (_x + _z * 16) % 8;
		int num3 = (int)this.m_bTopSoilBroken[num];
		num3 |= 1 << num2;
		this.m_bTopSoilBroken[num] = (byte)num3;
	}

	public BlockValue GetBlock(int _x, int _y, int _z)
	{
		if (this.IsInternalBlocksCulled && this.isInside(_x, _y, _z))
		{
			if (Chunk.bvPOIFiller.isair)
			{
				Chunk.bvPOIFiller = new BlockValue((uint)Block.GetBlockByName(Constants.cPOIFillerBlock, false).blockID);
			}
			return Chunk.bvPOIFiller;
		}
		BlockValue blockValue = BlockValue.Air;
		try
		{
			ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[_y >> 2];
			if (chunkBlockLayer != null)
			{
				blockValue = chunkBlockLayer.GetAt(_x, _y, _z);
			}
		}
		catch (IndexOutOfRangeException)
		{
			Log.Error(string.Concat(new string[]
			{
				"GetBlock failed: _y = ",
				_y.ToString(),
				", len = ",
				this.m_BlockLayers.Length.ToString(),
				" (chunk ",
				this.m_X.ToString(),
				"/",
				this.m_Z.ToString(),
				")"
			}));
			throw;
		}
		blockValue.damage = this.GetDamage(_x, _y, _z);
		return blockValue;
	}

	public BlockValue GetBlock(Vector3i pos)
	{
		return IBlockAccess.DefaultGetBlock(this, pos);
	}

	public BlockValue GetBlock(BlockValueRef bvRef)
	{
		return IBlockAccess.DefaultGetBlock(this, bvRef);
	}

	public BlockValue GetBlockNoDamage(int _x, int _y, int _z)
	{
		BlockValue blockValue = BlockValue.Air;
		try
		{
			ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[_y >> 2];
			if (chunkBlockLayer != null)
			{
				blockValue = chunkBlockLayer.GetAt(_x, _y, _z);
			}
		}
		catch (IndexOutOfRangeException)
		{
			Log.Error(string.Concat(new string[]
			{
				"GetBlockNoDamage failed: _y = ",
				_y.ToString(),
				", len = ",
				this.m_BlockLayers.Length.ToString(),
				" (chunk ",
				this.m_X.ToString(),
				"/",
				this.m_Z.ToString(),
				")"
			}));
			throw;
		}
		return blockValue;
	}

	public void GetBlockColumn(int _x, int _y, int _z, BlockValue[] _blocks)
	{
		try
		{
			int num = _blocks.Length;
			for (int i = 0; i < num; i++)
			{
				BlockValue blockValue = BlockValue.Air;
				ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[_y >> 2];
				if (chunkBlockLayer != null)
				{
					blockValue = chunkBlockLayer.GetAt(_x, _y, _z);
					blockValue.damage = this.GetDamage(_x, _y, _z);
				}
				_blocks[i] = blockValue;
				_y++;
			}
		}
		catch (IndexOutOfRangeException)
		{
			Log.Error(string.Concat(new string[]
			{
				"GetBlockColumn failed: _y = ",
				_y.ToString(),
				", len = ",
				this.m_BlockLayers.Length.ToString(),
				" (chunk ",
				this.m_X.ToString(),
				"/",
				this.m_Z.ToString(),
				")"
			}));
			throw;
		}
	}

	public int GetBlockId(int _x, int _y, int _z)
	{
		ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[_y >> 2];
		if (chunkBlockLayer != null)
		{
			return chunkBlockLayer.GetIdAt(_x, _y, _z);
		}
		return 0;
	}

	public void CopyMeshDataFrom(Chunk _other)
	{
		for (int i = 0; i < this.m_BlockLayers.Length; i++)
		{
			if (_other.m_BlockLayers[i] == null)
			{
				if (this.m_BlockLayers[i] != null)
				{
					MemoryPools.poolCBL.FreeSync(this.m_BlockLayers[i]);
					this.m_BlockLayers[i] = null;
				}
			}
			else
			{
				if (this.m_BlockLayers[i] == null)
				{
					this.m_BlockLayers[i] = MemoryPools.poolCBL.AllocSync(true);
				}
				this.m_BlockLayers[i].CopyFrom(_other.m_BlockLayers[i]);
			}
		}
		this.chnDensity.CopyFrom(_other.chnDensity);
		this.chnDamage.CopyFrom(_other.chnDamage);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetBiomeId(int _x, int _z)
	{
		return this.m_Biomes[_x + _z * 16];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetBiomeId(int _x, int _z, byte _biomeId)
	{
		this.m_Biomes[_x + _z * 16] = _biomeId;
	}

	public void FillBiomeId(byte _biomeId)
	{
		for (int i = 0; i < this.m_Biomes.Length; i++)
		{
			this.m_Biomes[i] = _biomeId;
		}
	}

	public BiomeIntensity GetBiomeIntensity(int _x, int _z)
	{
		if (this.m_BiomeIntensities == null)
		{
			return BiomeIntensity.Default;
		}
		return new BiomeIntensity(this.m_BiomeIntensities, (_x + _z * 16) * 6);
	}

	public void CalcBiomeIntensity(Chunk[] _neighbours)
	{
		int[] array = new int[50];
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				Array.Clear(array, 0, array.Length);
				for (int k = -16; k < 16; k++)
				{
					int num = i + k;
					int num2 = j + k;
					Chunk chunk = this;
					if (num < 0)
					{
						if (num2 < 0)
						{
							chunk = _neighbours[5];
						}
						else if (num2 >= 16)
						{
							chunk = _neighbours[6];
						}
						else
						{
							chunk = _neighbours[1];
						}
					}
					else if (num >= 16)
					{
						if (num2 < 0)
						{
							chunk = _neighbours[3];
						}
						else if (num2 >= 16)
						{
							chunk = _neighbours[4];
						}
						else
						{
							chunk = _neighbours[0];
						}
					}
					else if (num2 >= 16)
					{
						chunk = _neighbours[2];
					}
					else if (num2 < 0)
					{
						chunk = _neighbours[3];
					}
					int biomeId = (int)chunk.GetBiomeId(World.toBlockXZ(num), World.toBlockXZ(num2));
					if (biomeId >= 0 && biomeId < array.Length)
					{
						array[biomeId]++;
					}
				}
				BiomeIntensity.FromArray(array).ToArray(this.m_BiomeIntensities, (i + j * 16) * 6);
			}
		}
	}

	public void CalcDominantBiome()
	{
		int[] array = new int[50];
		for (int i = 0; i < this.m_Biomes.Length; i++)
		{
			array[(int)this.m_Biomes[i]]++;
		}
		int num = 0;
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j] > num)
			{
				this.DominantBiome = (byte)j;
				num = array[j];
			}
		}
	}

	public void ResetBiomeIntensity(BiomeIntensity _v)
	{
		for (int i = 0; i < this.m_BiomeIntensities.Length; i += 6)
		{
			_v.ToArray(this.m_BiomeIntensities, i);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetStability(int _x, int _y, int _z)
	{
		return (byte)this.chnStability.Get(_x, _y, _z);
	}

	public void SetStability(int _x, int _y, int _z, byte _v)
	{
		this.chnStability.Set(_x, _y, _z, (long)((ulong)_v));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetDensity(int _x, int _y, int _z, sbyte _density)
	{
		this.chnDensity.Set(_x, _y, _z, (long)((ulong)((byte)_density)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public sbyte GetDensity(int _x, int _y, int _z)
	{
		return (sbyte)this.chnDensity.Get(_x, _y, _z);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool HasSameDensityValue(int _y)
	{
		return this.chnDensity.HasSameValue(_y);
	}

	public sbyte GetSameDensityValue(int _y)
	{
		if (_y < 0)
		{
			return MarchingCubes.DensityTerrain;
		}
		if (_y >= 256)
		{
			return MarchingCubes.DensityAir;
		}
		return (sbyte)this.chnDensity.GetSameValue(_y);
	}

	public void CheckSameDensity()
	{
		this.chnDensity.CheckSameValue();
	}

	public bool IsOnlyTerrain(int _y)
	{
		int num = _y >> 2;
		return this.IsOnlyTerrainLayer(num);
	}

	public bool IsOnlyTerrainLayer(int _idx)
	{
		return _idx < 0 || _idx >= this.m_BlockLayers.Length || (this.m_BlockLayers[_idx] != null && this.m_BlockLayers[_idx].IsOnlyTerrain());
	}

	public void CheckOnlyTerrain()
	{
		for (int i = 0; i < this.m_BlockLayers.Length; i++)
		{
			ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[i];
			if (chunkBlockLayer != null)
			{
				chunkBlockLayer.CheckOnlyTerrain();
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public long GetTextureFull(int _x, int _y, int _z, int channel = 0)
	{
		if (!Chunk.IgnorePaintTextures)
		{
			return this.chnTextures[channel].Get(_x, _y, _z);
		}
		return 0L;
	}

	public TextureFullArray GetTextureFullArray(int _x, int _y, int _z, bool applyIgnore = true)
	{
		TextureFullArray textureFullArray;
		for (int i = 0; i < 1; i++)
		{
			textureFullArray[i] = ((applyIgnore && Chunk.IgnorePaintTextures) ? 0L : this.chnTextures[i].Get(_x, _y, _z));
		}
		return textureFullArray;
	}

	public void SetTextureFull(int _x, int _y, int _z, long _texturefull, int channel = 0)
	{
		this.chnTextures[channel].Set(_x, _y, _z, _texturefull);
		this.isModified = true;
	}

	public TextureFullArray GetSetTextureFullArray(int _x, int _y, int _z, TextureFullArray _texturefullArray)
	{
		TextureFullArray textureFullArray;
		for (int i = 0; i < 1; i++)
		{
			textureFullArray[i] = this.chnTextures[i].GetSet(_x, _y, _z, _texturefullArray[i]);
		}
		this.isModified = true;
		return textureFullArray;
	}

	public int GetBlockFaceTexture(int _x, int _y, int _z, BlockFace _face, int channel)
	{
		return (int)((this.chnTextures[channel].Get(_x, _y, _z) >> (int)(_face * (BlockFace)8)) & 255L);
	}

	public long SetBlockFaceTexture(int _x, int _y, int _z, BlockFace _face, int _texture, int channel = 0)
	{
		long num2;
		long num = (num2 = this.chnTextures[channel].Get(_x, _y, _z));
		int num3 = (int)(_face * (BlockFace)8);
		num2 &= ~(255L << num3);
		num2 |= (long)(_texture & 255) << num3;
		this.chnTextures[channel].Set(_x, _y, _z, num2);
		this.isModified = true;
		return num;
	}

	public static int Value64FullToIndex(long _valueFull, BlockFace _blockFace)
	{
		return (int)((_valueFull >> (int)(_blockFace * (BlockFace)8)) & 255L);
	}

	public static long TextureIdxToTextureFullValue64(int _idx)
	{
		long num = (long)_idx;
		return ((num & 255L) << 40) | ((num & 255L) << 32) | ((num & 255L) << 24) | ((num & 255L) << 16) | ((num & 255L) << 8) | (num & 255L);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetDamage(int _x, int _y, int _z, int _damage)
	{
		this.chnDamage.Set(_x, _y, _z, (long)_damage);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetDamage(int _x, int _y, int _z)
	{
		return (int)this.chnDamage.Get(_x, _y, _z);
	}

	public bool IsAir(int _x, int _y, int _z)
	{
		return !this.IsWater(_x, _y, _z) && this.GetBlockNoDamage(_x, _y, _z).isair;
	}

	public void ClearWater()
	{
		this.chnWater.Clear(0L);
	}

	public bool IsWater(int _x, int _y, int _z)
	{
		return this.GetWater(_x, _y, _z).HasMass();
	}

	public WaterValue GetWater(int _x, int _y, int _z)
	{
		return WaterValue.FromRawData(this.chnWater.Get(_x, _y, _z));
	}

	public void SetWater(int _x, int _y, int _z, WaterValue _data)
	{
		this.SetWaterRaw(_x, _y, _z, _data);
		this.waterSimHandle.WakeNeighbours(_x, _y, _z);
	}

	public void SetWaterRaw(int _x, int _y, int _z, WaterValue _data)
	{
		if (!WaterUtils.CanWaterFlowThrough(this.GetBlockNoDamage(_x, _y, _z)))
		{
			_data.SetMass(0);
		}
		this.chnWater.Set(_x, _y, _z, _data.RawData);
		this.bEmptyDirty = true;
		this.bMapDirty = true;
		this.isModified = true;
		this.waterSimHandle.SetWaterMass(_x, _y, _z, _data.GetMass());
		if (_data.HasMass())
		{
			int num = ChunkBlockLayerLegacy.CalcOffset(_x, _z);
			if ((int)this.m_HeightMap[num] < _y)
			{
				this.m_HeightMap[num] = (byte)_y;
			}
		}
	}

	public void SetWaterSimUpdate(int _x, int _y, int _z, WaterValue _data, out WaterValue _lastData)
	{
		if (!WaterUtils.CanWaterFlowThrough(this.GetBlockNoDamage(_x, _y, _z)))
		{
			_lastData = WaterValue.FromRawData(this.chnWater.Get(_x, _y, _z));
			return;
		}
		long set = this.chnWater.GetSet(_x, _y, _z, _data.RawData);
		_lastData = WaterValue.FromRawData(set);
		if (_lastData.GetMass() == _data.GetMass())
		{
			return;
		}
		GameManager.Instance.World.HandleWaterLevelChanged(this.ToWorldPos(_x, _y, _z), _data.GetMassPercent());
		this.bEmptyDirty = true;
		this.bMapDirty = true;
		this.isModified = true;
		if (_data.HasMass())
		{
			int num = ChunkBlockLayerLegacy.CalcOffset(_x, _z);
			if ((int)this.m_HeightMap[num] < _y)
			{
				this.m_HeightMap[num] = (byte)_y;
			}
		}
	}

	public bool IsEmpty()
	{
		if (this.bEmptyDirty)
		{
			this.bEmpty = true;
			for (int i = 0; i < this.m_BlockLayers.Length; i++)
			{
				if (this.m_BlockLayers[i] != null)
				{
					this.bEmpty = false;
					break;
				}
			}
			if (this.bEmpty)
			{
				this.bEmpty = this.chnWater.IsDefault();
			}
			this.bEmptyDirty = false;
		}
		return this.bEmpty;
	}

	public bool IsEmpty(int _y)
	{
		int num = _y >> 2;
		return this.IsEmptyLayer(num);
	}

	public bool IsEmptyLayer(int _idx)
	{
		return (ulong)_idx >= (ulong)((long)this.m_BlockLayers.Length) || (this.m_BlockLayers[_idx] == null && this.chnWater.IsDefaultLayer(_idx));
	}

	public int RecalcHeights()
	{
		int num = 0;
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				int num2 = ChunkBlockLayerLegacy.CalcOffset(j, i);
				this.m_HeightMap[num2] = 0;
				for (int k = 255; k >= 0; k--)
				{
					ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[k >> 2];
					if ((chunkBlockLayer != null && !chunkBlockLayer.GetAt(j, k, i).isair) || this.IsWater(j, k, i))
					{
						this.m_HeightMap[num2] = (byte)k;
						num = Utils.FastMax(num, k);
						break;
					}
				}
			}
		}
		return num;
	}

	public byte RecalcHeightAt(int _x, int _yMaxStart, int _z)
	{
		int num = ChunkBlockLayerLegacy.CalcOffset(_x, _z);
		for (int i = _yMaxStart; i >= 0; i--)
		{
			ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[i >> 2];
			if ((chunkBlockLayer != null && !chunkBlockLayer.GetAt(_x, i, _z).isair) || this.IsWater(_x, i, _z))
			{
				this.m_HeightMap[num] = (byte)i;
				return (byte)i;
			}
		}
		return 0;
	}

	public BlockValue SetBlock(WorldBase _world, int x, int y, int z, BlockValue _blockValue, bool _notifyAddChange = true, bool _notifyRemove = true, bool _fromReset = false, bool _poiOwned = false, int _changedByEntityId = -1)
	{
		Vector3i vector3i = new Vector3i((this.m_X << 4) + x, y, (this.m_Z << 4) + z);
		BlockValue blockValue = this.SetBlockRaw(x, y, z, _blockValue);
		bool flag = !blockValue.isair && _blockValue.isair;
		if (flag)
		{
			this.waterSimHandle.WakeNeighbours(x, y, z);
			if (blockValue.Block.StabilitySupport)
			{
				MultiBlockManager.Instance.SetOversizedStabilityDirty(vector3i);
			}
		}
		if (!_blockValue.ischild)
		{
			MultiBlockManager.Instance.UpdateTrackedBlockData(vector3i, _blockValue, _poiOwned);
		}
		_blockValue = this.GetBlock(x, y, z);
		if (_notifyRemove && !blockValue.isair && blockValue.type != _blockValue.type)
		{
			Block block = blockValue.Block;
			if (block != null)
			{
				block.OnBlockRemoved(_world, this, vector3i, blockValue);
			}
		}
		if (_notifyAddChange)
		{
			Block block2 = _blockValue.Block;
			if (block2 != null)
			{
				if (blockValue.type != _blockValue.type)
				{
					if (!_blockValue.isair)
					{
						PlatformUserIdentifierAbs platformUserIdentifierAbs = null;
						if (_changedByEntityId != -1)
						{
							platformUserIdentifierAbs = GameManager.Instance.persistentPlayers.GetPlayerDataFromEntityID(_changedByEntityId).PrimaryId;
						}
						block2.OnBlockAdded(_world, this, vector3i, _blockValue, platformUserIdentifierAbs);
					}
				}
				else
				{
					block2.OnBlockValueChanged(_world, this, vector3i, blockValue, _blockValue);
					if (_fromReset)
					{
						block2.OnBlockReset(_world, this, vector3i, _blockValue);
					}
				}
			}
		}
		if (flag)
		{
			this.RemoveBlockTrigger(new Vector3i(x, y, z));
			GameEventManager.Current.BlockRemoved(vector3i);
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent() && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !GameManager.Instance.IsEditMode() && BlockLimitTracker.instance != null && !blockValue.Equals(_blockValue))
		{
			BlockLimitTracker.instance.TryRemoveOrReplaceBlock(blockValue, _blockValue, vector3i);
			if (!flag)
			{
				BlockLimitTracker.instance.TryAddTrackedBlock(_blockValue, vector3i, _changedByEntityId);
			}
			BlockLimitTracker.instance.ServerUpdateClients();
		}
		return blockValue;
	}

	public BlockValue SetBlockRaw(int _x, int _y, int _z, BlockValue _blockValue)
	{
		if (_y >= 255)
		{
			return BlockValue.Air;
		}
		Block block = _blockValue.Block;
		if (block == null)
		{
			return BlockValue.Air;
		}
		if (_blockValue.isWater)
		{
			ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[_y >> 2];
			BlockValue blockValue = ((chunkBlockLayer != null) ? chunkBlockLayer.GetAt(_x, _y, _z) : BlockValue.Air);
			if (!WaterUtils.CanWaterFlowThrough(blockValue))
			{
				this.SetBlockRaw(_x, _y, _z, BlockValue.Air);
			}
			this.SetWater(_x, _y, _z, WaterValue.Full);
			return blockValue;
		}
		if (!WaterUtils.CanWaterFlowThrough(_blockValue))
		{
			this.SetWater(_x, _y, _z, WaterValue.Empty);
		}
		this.waterSimHandle.SetVoxelSolid(_x, _y, _z, BlockFaceFlags.RotateFlags(block.WaterFlowMask, _blockValue.rotation));
		BlockValue blockValue2 = BlockValue.Air;
		int num = _y >> 2;
		ChunkBlockLayer chunkBlockLayer2 = this.m_BlockLayers[num];
		if (chunkBlockLayer2 != null)
		{
			int num2 = ChunkBlockLayer.CalcOffset(_x, _y, _z);
			blockValue2 = chunkBlockLayer2.GetAt(num2);
			chunkBlockLayer2.SetAt(num2, _blockValue.rawData);
			if (!blockValue2.ischild)
			{
				blockValue2.damage = this.GetDamage(_x, _y, _z);
			}
		}
		else if (!_blockValue.isair)
		{
			chunkBlockLayer2 = MemoryPools.poolCBL.AllocSync(true);
			this.m_BlockLayers[num] = chunkBlockLayer2;
			chunkBlockLayer2.SetAt(_x, _y, _z, _blockValue.rawData);
		}
		if (!_blockValue.ischild)
		{
			this.SetDamage(_x, _y, _z, _blockValue.damage);
		}
		Block block2 = blockValue2.Block;
		if (blockValue2.type != _blockValue.type)
		{
			if (!blockValue2.ischild && block2.IndexName != null && this.IndexedBlocks.ContainsKey(block2.IndexName))
			{
				this.IndexedBlocks[block2.IndexName].Remove(new Vector3i(_x, _y, _z));
				if (this.IndexedBlocks[block2.IndexName].Count == 0)
				{
					this.IndexedBlocks.Remove(block2.IndexName);
				}
			}
			if (!_blockValue.ischild && block.IndexName != null && block.FilterIndexType(_blockValue))
			{
				if (!this.IndexedBlocks.ContainsKey(block.IndexName))
				{
					this.IndexedBlocks[block.IndexName] = new List<Vector3i>();
				}
				this.IndexedBlocks[block.IndexName].Add(new Vector3i(_x, _y, _z));
			}
		}
		int num3 = ChunkBlockLayerLegacy.CalcOffset(_x, _z);
		if (_blockValue.isair)
		{
			if ((int)this.m_HeightMap[num3] == _y)
			{
				this.RecalcHeightAt(_x, _y - 1, _z);
			}
		}
		else if ((int)this.m_HeightMap[num3] < _y)
		{
			this.m_HeightMap[num3] = (byte)_y;
		}
		if (blockValue2.isair && !_blockValue.isair && !_blockValue.ischild)
		{
			if (!block.IsRandomlyTick)
			{
				goto IL_03CF;
			}
			DictionaryKeyList<Vector3i, int> dictionaryKeyList = this.tickedBlocks;
			lock (dictionaryKeyList)
			{
				this.tickedBlocks.Replace(this.ToWorldPos(_x, _y, _z), 0);
				goto IL_03CF;
			}
		}
		if (!blockValue2.isair && _blockValue.isair && !blockValue2.ischild)
		{
			if (!block2.IsRandomlyTick)
			{
				goto IL_03CF;
			}
			DictionaryKeyList<Vector3i, int> dictionaryKeyList = this.tickedBlocks;
			lock (dictionaryKeyList)
			{
				this.tickedBlocks.Remove(this.ToWorldPos(_x, _y, _z));
				goto IL_03CF;
			}
		}
		if (block2.IsRandomlyTick && !block.IsRandomlyTick && !blockValue2.ischild)
		{
			DictionaryKeyList<Vector3i, int> dictionaryKeyList = this.tickedBlocks;
			lock (dictionaryKeyList)
			{
				this.tickedBlocks.Remove(this.ToWorldPos(_x, _y, _z));
				goto IL_03CF;
			}
		}
		if (!block2.IsRandomlyTick && block.IsRandomlyTick && !_blockValue.ischild)
		{
			DictionaryKeyList<Vector3i, int> dictionaryKeyList = this.tickedBlocks;
			lock (dictionaryKeyList)
			{
				this.tickedBlocks.Replace(this.ToWorldPos(_x, _y, _z), 0);
			}
		}
		IL_03CF:
		this.bMapDirty = true;
		this.isModified = true;
		this.bEmptyDirty = true;
		return blockValue2;
	}

	public bool FillBlockRaw(int _heightIncl, BlockValue _blockValue)
	{
		if (_heightIncl >= 255)
		{
			return false;
		}
		if (_blockValue.isair || _blockValue.ischild)
		{
			return false;
		}
		Block block = _blockValue.Block;
		if (block == null)
		{
			return false;
		}
		if (_blockValue.isWater)
		{
			return false;
		}
		if (!this.IsEmpty())
		{
			return false;
		}
		uint rawData = _blockValue.rawData;
		sbyte b = (block.shape.IsTerrain() ? MarchingCubes.DensityTerrain : MarchingCubes.DensityAir);
		int damage = _blockValue.damage;
		int i;
		for (i = 0; i <= _heightIncl - 4; i += 4)
		{
			int num = i >> 2;
			if (this.m_BlockLayers[num] == null)
			{
				this.m_BlockLayers[num] = MemoryPools.poolCBL.AllocSync(true);
			}
			this.m_BlockLayers[num].Fill(rawData);
		}
		while (i <= _heightIncl)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					int num2 = i >> 2;
					if (this.m_BlockLayers[num2] == null)
					{
						this.m_BlockLayers[num2] = MemoryPools.poolCBL.AllocSync(true);
					}
					this.m_BlockLayers[num2].SetAt(j, i, k, rawData);
				}
			}
			i++;
		}
		List<Vector3i> list = null;
		if (block.IndexName != null)
		{
			if (!this.IndexedBlocks.ContainsKey(block.IndexName))
			{
				this.IndexedBlocks[block.IndexName] = new List<Vector3i>();
			}
			list = this.IndexedBlocks[block.IndexName];
			list.Clear();
		}
		DictionaryKeyList<Vector3i, int> dictionaryKeyList = this.tickedBlocks;
		lock (dictionaryKeyList)
		{
			this.tickedBlocks.Clear();
			for (i = 0; i <= _heightIncl; i++)
			{
				for (int l = 0; l < 16; l++)
				{
					for (int m = 0; m < 16; m++)
					{
						this.SetDensity(l, i, m, b);
						this.SetDamage(l, i, m, damage);
						if (list != null)
						{
							list.Add(new Vector3i(l, i, m));
						}
						if (block.IsRandomlyTick)
						{
							this.tickedBlocks.Replace(this.ToWorldPos(l, i, m), 0);
						}
					}
				}
			}
		}
		for (int n = 0; n < 16; n++)
		{
			for (int num3 = 0; num3 < 16; num3++)
			{
				int num4 = ChunkBlockLayerLegacy.CalcOffset(n, num3);
				this.m_HeightMap[num4] = (byte)_heightIncl;
			}
		}
		this.bMapDirty = true;
		this.isModified = true;
		this.bEmptyDirty = true;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsInChunk(Vector3 position)
	{
		float x = position.x;
		if (x >= 0f && x < 16f)
		{
			float y = position.y;
			if (y >= 0f && y < 256f)
			{
				float z = position.z;
				if (z >= 0f)
				{
					return z < 16f;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PropValue AddProp(Vector3 position, Quaternion rotation, Vector3 scale, BlockValue blockValue)
	{
		if (blockValue.isair)
		{
			Log.Warning(string.Format("Can not create a prop out of an air block. BlockValue({0})", blockValue));
			return PropValue.AIR;
		}
		if (!this.IsInChunk(position))
		{
			Log.Warning(string.Format("Prop being created is not contained within this chunk. Position: {0}", position));
		}
		Quaternion normalized = rotation.normalized;
		PropValue propValue = new PropValue
		{
			transform = new PropTransform
			{
				position = position,
				rotation = normalized,
				scale = scale
			},
			blockValue = blockValue
		};
		int num = this.propIdNext;
		this.propIdNext = num + 1;
		int num2 = num;
		this.props[num2] = propValue;
		return propValue;
	}

	public PropValue GetProp(int chunkX, int chunkZ, int propId)
	{
		if (chunkX != this.X || chunkZ != this.Z)
		{
			return PropValue.AIR;
		}
		PropValue propValue;
		if (!this.props.TryGetValue(propId, out propValue))
		{
			return PropValue.AIR;
		}
		return propValue;
	}

	public PropValue GetProp(long chunkKey, int propId)
	{
		return this.GetProp(WorldChunkCache.extractX(chunkKey), WorldChunkCache.extractZ(chunkKey), propId);
	}

	public PropValue GetProp(Vector2i chunkPos, int propId)
	{
		return IBlockAccess.DefaultGetProp(this, chunkPos, propId);
	}

	public PropValue GetProp(PropRef propRef)
	{
		return IBlockAccess.DefaultGetProp(this, propRef);
	}

	public PropValue SetProp(int? propId = null, Vector3? position = null, Quaternion? rotation = null, Vector3? scale = null, BlockValue? blockValue = null)
	{
		if (propId == null)
		{
			if (position == null || rotation == null || scale == null || blockValue == null)
			{
				Log.Error(string.Format("Creating a new prop requires valid position, rotation, scale and block values. Position{0}, Rotation{1}, Scale{2}, BlockValue({3})", new object[] { position, rotation, scale, blockValue }));
				return PropValue.AIR;
			}
			return this.AddProp(position.Value, rotation.Value, scale.Value, blockValue.Value);
		}
		else
		{
			PropValue propValue;
			if (!this.props.TryGetValue(propId.Value, out propValue))
			{
				Log.Error(string.Format("Could not find prop with id {0}.", propId.Value));
				return PropValue.AIR;
			}
			bool flag = false;
			if (blockValue != null)
			{
				if (blockValue.Value.isair)
				{
					this.props.Remove(propId.Value);
					return PropValue.AIR;
				}
				if (propValue.blockValue.rawData != blockValue.Value.rawData)
				{
					propValue.blockValue.rawData = blockValue.Value.rawData;
					flag = true;
				}
				if (propValue.blockValue.damage != blockValue.Value.damage)
				{
					propValue.blockValue.damage = blockValue.Value.damage;
					flag = true;
				}
			}
			if (position != null)
			{
				if (!this.IsInChunk(position.Value))
				{
					Log.Warning(string.Format("Prop being updated is not contained within this chunk. Position: {0}", position.Value));
				}
				if (propValue.position != position.Value)
				{
					propValue.position = position.Value;
					flag = true;
				}
			}
			if (rotation != null)
			{
				Quaternion normalized = rotation.Value.normalized;
				if (propValue.rotation != normalized)
				{
					propValue.rotation = normalized;
					flag = true;
				}
			}
			if (scale != null)
			{
				Vector3 value = scale.Value;
				if (propValue.scale != value)
				{
					propValue.scale = value;
					flag = true;
				}
			}
			if (!flag)
			{
				return propValue;
			}
			this.props[propId.Value] = propValue;
			return propValue;
		}
	}

	public DictionaryKeyList<Vector3i, int> GetTickedBlocks()
	{
		return this.tickedBlocks;
	}

	public void RemoveTileEntityAt<T>(World world, Vector3i _posInChunk)
	{
		TileEntity tileEntity;
		if (this.tileEntities.dict.TryGetValue(_posInChunk, out tileEntity) && tileEntity is T)
		{
			tileEntity.IsRemoving = true;
			tileEntity.OnRemove(world);
			this.tileEntities.Remove(_posInChunk);
			tileEntity.IsRemoving = false;
		}
		this.isModified = true;
	}

	public void RemoveAllTileEntities()
	{
		this.isModified = this.tileEntities.Count > 0;
		this.tileEntities.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public byte GetHeight(int _blockOffset)
	{
		return this.m_HeightMap[_blockOffset];
	}

	public void AddTileEntity(TileEntity _te)
	{
		this.tileEntities.Set(_te.localChunkPos, _te);
	}

	public void RemoveTileEntity(World world, TileEntity _te)
	{
		TileEntity tileEntity;
		if (this.tileEntities.dict.TryGetValue(_te.localChunkPos, out tileEntity) && tileEntity != null)
		{
			tileEntity.IsRemoving = true;
			tileEntity.OnRemove(world);
			this.tileEntities.Remove(_te.localChunkPos);
			tileEntity.IsRemoving = false;
			this.isModified = true;
		}
	}

	public TileEntity GetTileEntity(Vector3i _blockPosInChunk)
	{
		TileEntity tileEntity;
		if (!this.tileEntities.dict.TryGetValue(_blockPosInChunk, out tileEntity))
		{
			return null;
		}
		return tileEntity;
	}

	public DictionaryList<Vector3i, TileEntity> GetTileEntities()
	{
		return this.tileEntities;
	}

	public void AddSleeperVolumeId(int id)
	{
		if (!this.sleeperVolumes.Contains(id))
		{
			if (this.sleeperVolumes.Count < 255)
			{
				this.sleeperVolumes.Add(id);
				return;
			}
			Log.Error("Chunk AddSleeperVolumeId at max");
		}
	}

	public List<int> GetSleeperVolumes()
	{
		return this.sleeperVolumes;
	}

	public void AddTriggerVolumeId(int id)
	{
		if (!this.triggerVolumes.Contains(id))
		{
			if (this.triggerVolumes.Count < 255)
			{
				this.triggerVolumes.Add(id);
				return;
			}
			Log.Error("Chunk AddTriggerVolumeId at max");
		}
	}

	public List<int> GetTriggerVolumes()
	{
		return this.triggerVolumes;
	}

	public void AddWallVolumeId(int id)
	{
		if (!this.wallVolumes.Contains(id))
		{
			if (this.wallVolumes.Count < 255)
			{
				this.wallVolumes.Add(id);
				return;
			}
			Log.Error("Chunk AddWallVolume at max");
		}
	}

	public List<int> GetWallVolumes()
	{
		return this.wallVolumes;
	}

	public int GetTickRefCount(int _layerIdx)
	{
		if (this.m_BlockLayers[_layerIdx] == null)
		{
			return 0;
		}
		return this.m_BlockLayers[_layerIdx].GetTickRefCount();
	}

	public DictionaryList<Vector3i, BlockTrigger> GetBlockTriggers()
	{
		return this.triggerData;
	}

	public void AddBlockTrigger(BlockTrigger _td)
	{
		this.triggerData.Set(_td.LocalChunkPos, _td);
		this.isModified = true;
	}

	public void RemoveBlockTrigger(BlockTrigger _td)
	{
		BlockTrigger blockTrigger;
		if (this.triggerData.dict.TryGetValue(_td.LocalChunkPos, out blockTrigger) && blockTrigger != null)
		{
			this.triggerData.Remove(_td.LocalChunkPos);
			this.isModified = true;
		}
	}

	public void RemoveBlockTrigger(Vector3i _blockPos)
	{
		if (this.triggerData.dict.ContainsKey(_blockPos))
		{
			this.triggerData.Remove(_blockPos);
			this.isModified = true;
		}
	}

	public BlockTrigger GetBlockTrigger(Vector3i _blockPosInChunk)
	{
		BlockTrigger blockTrigger;
		this.triggerData.dict.TryGetValue(_blockPosInChunk, out blockTrigger);
		return blockTrigger;
	}

	public void UpdateTick(World _world, bool _bSpawnEnemies)
	{
		this.ProfilerBegin("TeTick");
		for (int i = 0; i < this.tileEntities.list.Count; i++)
		{
			this.tileEntities.list[i].UpdateTick(_world);
		}
		this.ProfilerEnd();
	}

	public bool NeedsTicking
	{
		get
		{
			return this.tileEntities.Count > 0 || this.sleeperVolumes.Count > 0;
		}
	}

	public bool IsOpenSkyAbove(int _x, int _y, int _z)
	{
		return _y >= (int)this.GetHeight(_x, _z);
	}

	public void GetLivingEntitiesInBounds(EntityAlive _excludeEntity, Bounds _aabb, List<EntityAlive> _entityOutputList)
	{
		int num = Utils.Fastfloor((double)(_aabb.min.y - 5f) / 16.0);
		int num2 = Utils.Fastfloor((double)(_aabb.max.y + 5f) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = this.entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				EntityAlive entityAlive = list[j] as EntityAlive;
				if (!(entityAlive == null) && !(entityAlive == _excludeEntity) && !entityAlive.IsDead() && entityAlive.boundingBox.Intersects(_aabb) && (!(_excludeEntity != null) || _excludeEntity.CanCollideWith(entityAlive)))
				{
					_entityOutputList.Add(entityAlive);
				}
			}
		}
	}

	public void GetEntitiesInBounds(Entity _excludeEntity, Bounds _aabb, List<Entity> _entityOutputList, bool isAlive)
	{
		int num = Utils.Fastfloor((double)(_aabb.min.y - 5f) / 16.0);
		int num2 = Utils.Fastfloor((double)(_aabb.max.y + 5f) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = this.entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				Entity entity = list[j];
				if (!(entity == _excludeEntity) && isAlive == entity.IsAlive() && entity.boundingBox.Intersects(_aabb) && (!(_excludeEntity != null) || _excludeEntity.CanCollideWith(entity)))
				{
					_entityOutputList.Add(entity);
				}
			}
		}
	}

	public void GetEntitiesInBounds(FastTags<TagGroup.Global> _tags, Bounds _bb, List<Entity> _list)
	{
		int num = Utils.Fastfloor((double)(_bb.min.y - 5f) / 16.0);
		int num2 = Utils.Fastfloor((double)(_bb.max.y + 5f) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		else if (num >= 16)
		{
			num = 15;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		else if (num2 < 0)
		{
			num2 = 0;
		}
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = this.entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				Entity entity = list[j];
				if (entity.HasAnyTags(_tags) && entity.boundingBox.Intersects(_bb))
				{
					_list.Add(entity);
				}
			}
		}
	}

	public void GetEntitiesInBounds(Type _class, Bounds _bb, List<Entity> _list)
	{
		int num = Utils.Fastfloor((double)(_bb.min.y - 5f) / 16.0);
		int num2 = Utils.Fastfloor((double)(_bb.max.y + 5f) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		else if (num >= 16)
		{
			num = 15;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		else if (num2 < 0)
		{
			num2 = 0;
		}
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = this.entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				Entity entity = list[j];
				if (_class.IsAssignableFrom(entity.GetType()) && entity.boundingBox.Intersects(_bb))
				{
					_list.Add(entity);
				}
			}
		}
	}

	public void GetEntitiesAround(EntityFlags _mask, Vector3 _pos, float _radius, List<Entity> _list)
	{
		int num = Utils.Fastfloor((double)(_pos.y - _radius) / 16.0);
		int num2 = Utils.Fastfloor((double)(_pos.y + _radius) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		else if (num >= 16)
		{
			num = 15;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		else if (num2 < 0)
		{
			num2 = 0;
		}
		float num3 = _radius * _radius;
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = this.entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				Entity entity = list[j];
				if ((entity.entityFlags & _mask) != EntityFlags.None && (entity.position - _pos).sqrMagnitude <= num3)
				{
					_list.Add(entity);
				}
			}
		}
	}

	public void GetEntitiesAround(EntityFlags _flags, EntityFlags _mask, Vector3 _pos, float _radius, List<Entity> _list)
	{
		int num = Utils.Fastfloor((double)(_pos.y - _radius) / 16.0);
		int num2 = Utils.Fastfloor((double)(_pos.y + _radius) / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		else if (num >= 16)
		{
			num = 15;
		}
		if (num2 >= 16)
		{
			num2 = 15;
		}
		else if (num2 < 0)
		{
			num2 = 0;
		}
		float num3 = _radius * _radius;
		for (int i = num; i <= num2; i++)
		{
			List<Entity> list = this.entityLists[i];
			for (int j = 0; j < list.Count; j++)
			{
				Entity entity = list[j];
				if ((entity.entityFlags & _mask) == _flags && (entity.position - _pos).sqrMagnitude <= num3)
				{
					_list.Add(entity);
				}
			}
		}
	}

	public void RemoveEntityFromChunk(Entity _entity)
	{
		int y = _entity.chunkPosAddedEntityTo.y;
		this.entityLists[y].Remove(_entity);
		this.isModified = true;
		bool flag = false;
		for (int i = 0; i < 16; i++)
		{
			if (this.entityLists[i].Count > 0)
			{
				flag = true;
				break;
			}
		}
		this.hasEntities = flag;
	}

	public void AddEntityToChunk(Entity _entity)
	{
		this.hasEntities = true;
		int num = World.toChunkXZ(Utils.Fastfloor(_entity.position.x));
		int num2 = World.toChunkXZ(Utils.Fastfloor(_entity.position.z));
		if (num != this.m_X || num2 != this.m_Z)
		{
			Log.Error(string.Concat(new string[]
			{
				"Wrong entity chunk position! ",
				(_entity != null) ? _entity.ToString() : null,
				" x=",
				num.ToString(),
				" z=",
				num2.ToString(),
				"/",
				(this != null) ? this.ToString() : null
			}));
		}
		int num3 = Utils.Fastfloor((double)_entity.position.y / 16.0);
		if (num3 < 0)
		{
			num3 = 0;
		}
		if (num3 >= 16)
		{
			num3 = 15;
		}
		_entity.addedToChunk = true;
		_entity.chunkPosAddedEntityTo.x = this.m_X;
		_entity.chunkPosAddedEntityTo.y = num3;
		_entity.chunkPosAddedEntityTo.z = this.m_Z;
		this.entityLists[num3].Add(_entity);
	}

	public void AdJustEntityTracking(Entity _entity)
	{
		if (!_entity.addedToChunk)
		{
			return;
		}
		int num = Utils.Fastfloor((double)_entity.position.y / 16.0);
		if (num < 0)
		{
			num = 0;
		}
		if (num >= 16)
		{
			num = 15;
		}
		if (_entity.chunkPosAddedEntityTo.y != num)
		{
			this.entityLists[_entity.chunkPosAddedEntityTo.y].Remove(_entity);
			_entity.chunkPosAddedEntityTo.y = num;
			this.entityLists[num].Add(_entity);
			this.isModified = true;
		}
	}

	public Bounds GetAABB()
	{
		return this.boundingBox;
	}

	public static Bounds CalculateAABB(int _chunkX, int _chunkY, int _chunkZ)
	{
		return BoundsUtils.BoundsForMinMax((float)(_chunkX * 16), (float)(_chunkY * 256), (float)(_chunkZ * 16), (float)(_chunkX * 16 + 16), (float)(_chunkY * 256 + 256), (float)(_chunkZ * 16 + 16));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBounds()
	{
		this.boundingBox = Chunk.CalculateAABB(this.m_X, this.m_Y, this.m_Z);
		this.worldPosIMin.x = this.m_X << 4;
		this.worldPosIMin.y = this.m_Y << 8;
		this.worldPosIMin.z = this.m_Z << 4;
		this.worldPosIMax.x = this.worldPosIMin.x + 15;
		this.worldPosIMax.y = this.worldPosIMin.y + 255;
		this.worldPosIMax.z = this.worldPosIMin.z + 15;
	}

	public int GetTris()
	{
		return this.totalTris;
	}

	public int GetTrisInMesh(int _idx)
	{
		int num = 0;
		for (int i = 0; i < this.trisInMesh.GetLength(0); i++)
		{
			num += this.trisInMesh[i][_idx];
		}
		return num;
	}

	public int GetSizeOfMesh(int _idx)
	{
		int num = 0;
		for (int i = 0; i < this.trisInMesh.GetLength(0); i++)
		{
			num += this.sizeOfMesh[i][_idx];
		}
		return num;
	}

	public int GetUsedMem()
	{
		this.TotalMemory = 0;
		for (int i = 0; i < this.m_BlockLayers.Length; i++)
		{
			this.TotalMemory += ((this.m_BlockLayers[i] != null) ? this.m_BlockLayers[i].GetUsedMem() : 0);
		}
		this.TotalMemory += 12;
		this.TotalMemory += this.m_TerrainHeight.Length;
		this.TotalMemory += this.m_HeightMap.Length;
		this.TotalMemory += this.m_Biomes.Length;
		this.TotalMemory += this.m_BiomeIntensities.Length;
		this.TotalMemory += this.m_NormalX.Length;
		this.TotalMemory += this.m_NormalY.Length;
		this.TotalMemory += this.m_NormalZ.Length;
		this.TotalMemory += this.chnStability.GetUsedMem();
		this.TotalMemory += this.chnLight.GetUsedMem();
		this.TotalMemory += this.chnDensity.GetUsedMem();
		this.TotalMemory += this.chnDamage.GetUsedMem();
		for (int j = 0; j < 1; j++)
		{
			this.TotalMemory += this.chnTextures[j].GetUsedMem();
		}
		this.TotalMemory += this.chnWater.GetUsedMem();
		return this.TotalMemory;
	}

	public void GetTextureChannelMemory(out int[] texMem)
	{
		texMem = new int[1];
		for (int i = 0; i < 1; i++)
		{
			texMem[i] = this.chnTextures[i].GetUsedMem();
		}
	}

	public void OnLoadedFromCache()
	{
		this.NeedsRegeneration = true;
		this.isModified = true;
		this.InProgressRegeneration = false;
		this.InProgressSaving = false;
		this.InProgressCopying = false;
		this.InProgressDecorating = false;
		this.InProgressLighting = false;
		this.InProgressUnloading = false;
		this.NeedsOnlyCollisionMesh = false;
		this.IsCollisionMeshGenerated = false;
		this.entityStubs.Clear();
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < this.entityLists[i].Count; j++)
			{
				if (this.entityLists[i][j].IsSavedToFile())
				{
					this.entityStubs.Add(new EntityCreationData(this.entityLists[i][j], true));
				}
			}
			this.entityLists[i].Clear();
		}
	}

	public void OnLoad(World _world)
	{
		if (!_world.IsRemote())
		{
			for (int i = 0; i < this.entityStubs.Count; i++)
			{
				EntityCreationData entityCreationData = this.entityStubs[i];
				if (!(_world.GetEntity(entityCreationData.id) != null))
				{
					this.SpawnEntityAsync(_world, entityCreationData, null);
				}
			}
			this.removeExpiredCustomChunkDataEntries(_world.GetWorldTime());
		}
		if (!_world.IsEditor())
		{
			GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
		}
		for (int j = 0; j < this.m_BlockLayers.Length; j++)
		{
			if (this.m_BlockLayers[j] != null)
			{
				this.m_BlockLayers[j].OnLoad(_world, this.X * 16, j * 4, this.Z * 16);
			}
		}
		for (int k = 0; k < this.tileEntities.list.Count; k++)
		{
			this.tileEntities.list[k].OnLoad();
		}
	}

	public void OnUnload(WorldBase _world)
	{
		this.ProfilerBegin("Chunk OnUnload");
		this.InProgressUnloading = true;
		if (this.biomeParticles != null)
		{
			this.ProfilerBegin("biome particles");
			for (int i = 0; i < this.biomeParticles.Count; i++)
			{
				Object.Destroy(this.biomeParticles[i]);
			}
			this.biomeParticles = null;
			this.ProfilerEnd();
		}
		this.spawnedBiomeParticles = false;
		if (!_world.IsRemote())
		{
			this.ProfilerBegin("enities");
			if (this.pendingEntityCreateOps.Count > 0)
			{
				EntityAsyncManager.EntityCreateHandle[] array = new EntityAsyncManager.EntityCreateHandle[this.pendingEntityCreateOps.Count];
				this.pendingEntityCreateOps.CopyTo(array);
				EntityAsyncManager.EntityCreateHandle[] array2 = array;
				for (int j = 0; j < array2.Length; j++)
				{
					array2[j].WaitForComplete();
				}
				this.pendingEntityCreateOps.Clear();
			}
			for (int k = 0; k < 16; k++)
			{
				if (this.entityLists[k].Count != 0)
				{
					_world.UnloadEntities(this.entityLists[k], false);
				}
			}
			this.ProfilerEnd();
			this.removeExpiredCustomChunkDataEntries(_world.GetWorldTime());
		}
		this.ProfilerBegin("tile entities");
		for (int l = 0; l < this.tileEntities.list.Count; l++)
		{
			this.tileEntities.list[l].OnUnload(GameManager.Instance.World);
		}
		this.ProfilerEnd();
		this.RemoveBlockEntityTransforms();
		this.ProfilerBegin("block layers");
		for (int m = 0; m < this.m_BlockLayers.Length; m++)
		{
			if (this.m_BlockLayers[m] != null)
			{
				this.m_BlockLayers[m].OnUnload(_world, this.X * 16, m * 4, this.Z * 16);
			}
		}
		this.ProfilerEnd();
		this.ProfilerBegin("water");
		this.waterSimHandle.Reset();
		this.ProfilerEnd();
		this.ProfilerEnd();
	}

	public void SpawnEntityAsync(World _world, EntityCreationData _ecd, Action<Entity> _onEntityCreated = null)
	{
		if (this.InProgressUnloading)
		{
			Log.Error(string.Format("Spawning entity onto chunk ({0},{1}) which is unloading", this.X, this.Z));
			return;
		}
		EntityAsyncManager.EntityCreateHandle entityCreateHandle = _world.entityAsyncManager.StartCreateEntity(_ecd, delegate(EntityAsyncManager.EntityCreateHandle handle)
		{
			this.pendingEntityCreateOps.Remove(handle);
			Action<Entity> onEntityCreated = _onEntityCreated;
			if (onEntityCreated != null)
			{
				onEntityCreated(handle.Entity);
			}
			_world.SpawnEntityInWorld(handle.Entity);
		});
		this.pendingEntityCreateOps.Add(entityCreateHandle);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnBiomeParticles(Transform _parentForEntityBlocks)
	{
		if (!this.spawnedBiomeParticles)
		{
			this.biomeParticles = BiomeParticleManager.SpawnParticles(this, _parentForEntityBlocks);
			this.spawnedBiomeParticles = true;
		}
	}

	public void OnDisplay(World _world, Transform _entityBlocksParentT, ChunkCluster _chunkCluster)
	{
		this.ProfilerBegin("Chunk OnDisplay");
		this.SpawnBiomeParticles(_entityBlocksParentT);
		this.displayState = Chunk.DisplayState.BlockEntities;
		this.blockEntitiesIndex = 0;
		this.blockEntityStubs.list.Sort((BlockEntityData a, BlockEntityData b) => a.pos.y.CompareTo(b.pos.y));
		this.ProfilerEnd();
	}

	public void OnDisplayBlockEntities(World _world, Transform _entityBlocksParentT, ChunkCluster _chunkCluster)
	{
		this.ProfilerBegin("Chunk OnDisplayBlockEntities");
		Vector3 vector;
		vector..ctor((float)(this.X * 16), 0f, (float)(this.Z * 16));
		int num = ChunkCluster.LayerMappingTable["nocollision"];
		int num2 = ChunkCluster.LayerMappingTable["terraincollision"];
		int num3 = 0;
		int num4 = Utils.FastMax(50, this.blockEntityStubs.list.Count / 3 + 8);
		while (this.blockEntitiesIndex < this.blockEntityStubs.list.Count)
		{
			BlockEntityData blockEntityData = this.blockEntityStubs.list[this.blockEntitiesIndex];
			if (blockEntityData.bHasTransform)
			{
				if (!this.NeedsOnlyCollisionMesh && !blockEntityData.bRenderingOn)
				{
					this.SetBlockEntityRendering(blockEntityData, true);
				}
			}
			else
			{
				if (++num3 > num4)
				{
					this.ProfilerEnd();
					return;
				}
				BlockValue block = _chunkCluster.GetBlock(blockEntityData.pos);
				if (!this.IsInternalBlocksCulled || block.type == blockEntityData.blockValue.type)
				{
					Block block2 = blockEntityData.blockValue.Block;
					BlockShapeModelEntity blockShapeModelEntity = block2.shape as BlockShapeModelEntity;
					if (blockShapeModelEntity == null)
					{
						this.RemoveEntityBlockStub(blockEntityData.pos);
					}
					else
					{
						float num5 = 0f;
						if (block2.IsTerrainDecoration && _world.GetBlock(blockEntityData.pos - Vector3i.up).Block.shape.IsTerrain())
						{
							num5 = _world.GetDecorationOffsetY(blockEntityData.pos);
						}
						Quaternion rotation = blockShapeModelEntity.GetRotation(block);
						Vector3 rotatedOffset = blockShapeModelEntity.GetRotatedOffset(block2, rotation);
						rotatedOffset.x += 0.5f;
						rotatedOffset.z += 0.5f;
						rotatedOffset.y += num5;
						Vector3 vector2 = blockEntityData.pos.ToVector3() + rotatedOffset;
						GameObject objectForType = GameObjectPool.Instance.GetObjectForType(blockShapeModelEntity.modelName, out block2.defaultTintColor);
						if (objectForType)
						{
							this.ProfilerBegin("BE setup");
							Transform transform = objectForType.transform;
							blockEntityData.transform = transform;
							blockEntityData.bHasTransform = true;
							transform.SetParent(_entityBlocksParentT, false);
							transform.localScale = Vector3.one;
							transform.SetLocalPositionAndRotation(vector2 - vector, rotation);
							bool isCollideMovement = block2.IsCollideMovement;
							int num6 = num;
							if (isCollideMovement)
							{
								int layer = objectForType.layer;
								if (layer == 30)
								{
									num6 = ChunkCluster.LayerMappingTable["Glass"];
								}
								else if (layer != 4)
								{
									num6 = num2;
								}
							}
							Utils.SetColliderLayerRecursively(objectForType, num6);
							Vector3i vector3i = Chunk.ToLocalPosition(blockEntityData.pos);
							this.ProfilerBegin("BE TBA");
							block2.OnBlockEntityTransformBeforeActivated(_world, blockEntityData.pos, this.GetBlock(vector3i.x, vector3i.y, vector3i.z), blockEntityData);
							this.ProfilerEnd();
							objectForType.SetActive(true);
							this.ProfilerBegin("BE TAA");
							block2.OnBlockEntityTransformAfterActivated(_world, blockEntityData.pos, this.GetBlock(vector3i.x, vector3i.y, vector3i.z), blockEntityData);
							this.ProfilerEnd();
							if (this.NeedsOnlyCollisionMesh)
							{
								this.SetBlockEntityRendering(blockEntityData, false);
							}
							else
							{
								Chunk.occlusionTs.Add(blockEntityData.transform);
							}
							this.ProfilerEnd();
						}
					}
				}
			}
			this.blockEntitiesIndex++;
		}
		HashSet<int> hashSet = new HashSet<int>(this.propEntities.Keys);
		List<ValueTuple<int, PropValue>> list = new List<ValueTuple<int, PropValue>>();
		foreach (KeyValuePair<int, PropValue> keyValuePair in this.props)
		{
			int num7;
			PropValue propValue;
			keyValuePair.Deconstruct(out num7, out propValue);
			int num8 = num7;
			PropValue propValue2 = propValue;
			BlockValue blockValue = propValue2.blockValue;
			if (blockValue.Block.shape is BlockShapeModelEntity)
			{
				hashSet.Remove(num8);
				list.Add(new ValueTuple<int, PropValue>(num8, propValue2));
			}
		}
		foreach (int num9 in hashSet)
		{
			PropEntityData propEntityData;
			if (this.propEntities.Remove(num9, out propEntityData))
			{
				this.propEntitiesToRemove.Add(propEntityData);
			}
		}
		foreach (ValueTuple<int, PropValue> valueTuple in list)
		{
			int item = valueTuple.Item1;
			PropValue item2 = valueTuple.Item2;
			BlockValue blockValue2 = item2.blockValue;
			Block block3 = blockValue2.Block;
			BlockShapeModelEntity blockShapeModelEntity2 = block3.shape as BlockShapeModelEntity;
			if (blockShapeModelEntity2 != null)
			{
				PropEntityData propEntityData2;
				if (!this.propEntities.TryGetValue(item, out propEntityData2))
				{
					propEntityData2 = (this.propEntities[item] = new PropEntityData());
				}
				PropValue propValue3 = propEntityData2.propValue;
				if (!(item2 == propValue3))
				{
					propEntityData2.propValue = item2;
					if (propEntityData2.bHasTransform)
					{
						Chunk.occlusionTs.Add(propEntityData2.transform);
						if (OcclusionManager.Instance.cullChunkEntities)
						{
							OcclusionManager.Instance.RemoveChunkTransforms(this, Chunk.occlusionTs);
						}
						Chunk.occlusionTs.Clear();
						GameObjectPool.Instance.PoolObject(propEntityData2.transform.gameObject);
						propEntityData2.bHasTransform = false;
						propEntityData2.transform = null;
					}
					float num10 = 0f;
					Vector3 position = item2.position;
					Vector3i vector3i2 = Vector3i.Floor(position);
					if (block3.IsTerrainDecoration && _world.GetBlock(vector3i2 - Vector3i.up).Block.shape.IsTerrain())
					{
						num10 = _world.GetDecorationOffsetY(vector3i2);
					}
					Quaternion rotation2 = blockShapeModelEntity2.GetRotation(blockValue2);
					Vector3 rotatedOffset2 = blockShapeModelEntity2.GetRotatedOffset(block3, rotation2);
					rotatedOffset2.x += 0.5f;
					rotatedOffset2.z += 0.5f;
					rotatedOffset2.y += num10;
					Vector3 vector3 = position + rotatedOffset2;
					GameObject objectForType2 = GameObjectPool.Instance.GetObjectForType(blockShapeModelEntity2.modelName, out block3.defaultTintColor);
					if (objectForType2)
					{
						this.ProfilerBegin("BE setup");
						Transform transform2 = objectForType2.transform;
						propEntityData2.transform = transform2;
						propEntityData2.bHasTransform = true;
						transform2.SetParent(_entityBlocksParentT, false);
						transform2.localScale = Vector3.one;
						transform2.SetLocalPositionAndRotation(vector3, rotation2);
						PropReference propReference;
						if (!objectForType2.TryGetComponent<PropReference>(ref propReference))
						{
							propReference = objectForType2.AddComponent<PropReference>();
						}
						propReference.ChunkPos = new Vector2i(this.X, this.Z);
						propReference.PropId = item;
						bool isCollideMovement2 = block3.IsCollideMovement;
						int num11 = num;
						if (isCollideMovement2)
						{
							int layer2 = objectForType2.layer;
							if (layer2 == 30)
							{
								num11 = ChunkCluster.LayerMappingTable["Glass"];
							}
							else if (layer2 != 4)
							{
								num11 = num2;
							}
						}
						Utils.SetColliderLayerRecursively(objectForType2, num11);
						objectForType2.SetActive(true);
						Chunk.occlusionTs.Add(propEntityData2.transform);
						this.ProfilerEnd();
					}
				}
			}
		}
		if (Chunk.occlusionTs.Count > 0)
		{
			if (OcclusionManager.Instance.cullChunkEntities)
			{
				this.ProfilerBegin("BE occlusion");
				OcclusionManager.Instance.AddChunkTransforms(this, Chunk.occlusionTs);
				this.ProfilerEnd();
			}
			Chunk.occlusionTs.Clear();
		}
		this.removeBlockEntitesMarkedForRemoval();
		AstarManager.AddBoundsToUpdate(this.boundingBox);
		this.displayState = Chunk.DisplayState.Done;
		DynamicMeshThread.AddChunkGameObject(this);
		this.ProfilerEnd();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector3i ToLocalPosition(Vector3i _pos)
	{
		_pos.x &= 15;
		_pos.y &= 255;
		_pos.z &= 15;
		return _pos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeBlockEntitesMarkedForRemoval()
	{
		if (OcclusionManager.Instance.cullChunkEntities)
		{
			for (int i = 0; i < this.blockEntityStubsToRemove.Count; i++)
			{
				BlockEntityData blockEntityData = this.blockEntityStubsToRemove[i];
				if (blockEntityData.bHasTransform)
				{
					Chunk.occlusionTs.Add(blockEntityData.transform);
				}
			}
			foreach (PropEntityData propEntityData in this.propEntitiesToRemove)
			{
				if (propEntityData.bHasTransform)
				{
					Chunk.occlusionTs.Add(propEntityData.transform);
				}
			}
			if (Chunk.occlusionTs.Count > 0)
			{
				OcclusionManager.Instance.RemoveChunkTransforms(this, Chunk.occlusionTs);
				Chunk.occlusionTs.Clear();
			}
		}
		for (int j = 0; j < this.blockEntityStubsToRemove.Count; j++)
		{
			BlockEntityData blockEntityData2 = this.blockEntityStubsToRemove[j];
			blockEntityData2.Cleanup();
			if (blockEntityData2.bHasTransform)
			{
				this.poolBlockEntityTransform(blockEntityData2);
			}
		}
		this.blockEntityStubsToRemove.Clear();
		foreach (PropEntityData propEntityData2 in this.propEntitiesToRemove)
		{
			if (propEntityData2.bHasTransform)
			{
				if (propEntityData2.transform)
				{
					PropReference propReference;
					if (propEntityData2.transform.TryGetComponent<PropReference>(ref propReference))
					{
						Object.Destroy(propReference);
					}
					GameObjectPool.Instance.PoolObject(propEntityData2.transform.gameObject);
				}
				propEntityData2.bHasTransform = false;
				propEntityData2.transform = null;
			}
		}
		this.propEntitiesToRemove.Clear();
	}

	public void OnHide()
	{
		this.RemoveBlockEntityTransforms();
		AstarManager.AddBoundsToUpdate(this.boundingBox);
	}

	public void RemoveBlockEntityTransforms()
	{
		this.ProfilerBegin("RemoveBlockEntityTransforms");
		if (OcclusionManager.Instance.cullChunkEntities)
		{
			this.ProfilerBegin("OcclusionManager RemoveChunk");
			OcclusionManager.Instance.RemoveChunk(this);
			this.ProfilerEnd();
		}
		for (int i = 0; i < this.blockEntityStubs.list.Count; i++)
		{
			BlockEntityData blockEntityData = this.blockEntityStubs.list[i];
			if (blockEntityData.bHasTransform)
			{
				this.poolBlockEntityTransform(blockEntityData);
			}
		}
		this.ProfilerEnd();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void poolBlockEntityTransform(BlockEntityData _bed)
	{
		if (!_bed.bRenderingOn)
		{
			this.SetBlockEntityRendering(_bed, true);
		}
		if (_bed.transform)
		{
			GameObjectPool.Instance.PoolObject(_bed.transform.gameObject);
		}
		else
		{
			Log.Error("BlockEntity {0} at pos {1} null transform!", new object[]
			{
				_bed.ToString(),
				_bed.pos
			});
		}
		_bed.bHasTransform = false;
		_bed.transform = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBlockEntityRendering(BlockEntityData _bed, bool _bOn)
	{
		_bed.bRenderingOn = _bOn;
		if (!_bed.transform)
		{
			Log.Error(string.Format("2: {0} on pos {1} with empty transform/gameobject!", _bed.ToString(), _bed.pos));
			return;
		}
		this.ProfilerBegin("SetBlockEntityRendering set enable");
		_bed.transform.GetComponentsInChildren<MeshRenderer>(Chunk.tempMeshRenderers);
		for (int i = 0; i < Chunk.tempMeshRenderers.Count; i++)
		{
			Chunk.tempMeshRenderers[i].enabled = _bOn;
		}
		Chunk.tempMeshRenderers.Clear();
		this.ProfilerEnd();
		this.ProfilerBegin("SetBlockEntityRendering BroadcastMessage");
		if (_bOn)
		{
			_bed.transform.BroadcastMessage("SetRenderingOn", 1);
		}
		else
		{
			_bed.transform.BroadcastMessage("SetRenderingOff", 1);
		}
		this.ProfilerEnd();
	}

	public static void ToTerrain(Chunk _chunk, Chunk _terrainChunk)
	{
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				byte height = _chunk.GetHeight(i, j);
				for (int k = 0; k <= (int)height; k++)
				{
					if (!_chunk.GetBlock(i, k, j).isair)
					{
						_terrainChunk.SetBlockRaw(i, k, j, Constants.cTerrainBlockValue);
					}
				}
				for (int l = 0; l < 256; l++)
				{
					_terrainChunk.SetDensity(i, l, j, _chunk.GetDensity(i, l, j));
				}
				_terrainChunk.SetHeight(i, j, height);
				_terrainChunk.SetTerrainHeight(i, j, height);
			}
		}
		_terrainChunk.CopyLightsFrom(_chunk);
		_terrainChunk.isModified = true;
		_terrainChunk.NeedsLightCalculation = false;
	}

	public void AddMeshLayer(VoxelMeshLayer _vml)
	{
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			this.trisInMesh[_vml.idx][i] = _vml.GetTrisInMesh(i);
			this.sizeOfMesh[_vml.idx][i] = _vml.GetSizeOfMesh(i);
		}
		this.totalTris = 0;
		for (int j = 0; j < this.trisInMesh.GetLength(0); j++)
		{
			for (int k = 0; k < MeshDescription.meshes.Length; k++)
			{
				this.totalTris += this.trisInMesh[j][k];
			}
		}
		Queue<int> layerIndexQueue = this.m_layerIndexQueue;
		lock (layerIndexQueue)
		{
			VoxelMeshLayer voxelMeshLayer = this.m_meshLayers[_vml.idx];
			if (voxelMeshLayer == null)
			{
				this.MeshLayerCount++;
				this.m_layerIndexQueue.Enqueue(_vml.idx);
			}
			else
			{
				MemoryPools.poolVML.FreeSync(voxelMeshLayer);
			}
			this.m_meshLayers[_vml.idx] = _vml;
		}
	}

	public bool HasMeshLayer()
	{
		Queue<int> layerIndexQueue = this.m_layerIndexQueue;
		bool flag2;
		lock (layerIndexQueue)
		{
			flag2 = this.m_layerIndexQueue.Count > 0;
		}
		return flag2;
	}

	public VoxelMeshLayer GetMeshLayer()
	{
		Queue<int> layerIndexQueue = this.m_layerIndexQueue;
		VoxelMeshLayer voxelMeshLayer2;
		lock (layerIndexQueue)
		{
			if (this.m_layerIndexQueue.Count > 0)
			{
				this.MeshLayerCount--;
				int num = this.m_layerIndexQueue.Dequeue();
				VoxelMeshLayer voxelMeshLayer = this.m_meshLayers[num];
				this.m_meshLayers[num] = null;
				voxelMeshLayer2 = voxelMeshLayer;
			}
			else
			{
				voxelMeshLayer2 = null;
			}
		}
		return voxelMeshLayer2;
	}

	public EnumDecoAllowed GetDecoAllowedAt(int x, int z)
	{
		EnumDecoAllowed enumDecoAllowed = EnumDecoAllowed.Everything;
		if (this.m_DecoBiomeArray != null)
		{
			enumDecoAllowed = this.m_DecoBiomeArray[x + z * 16];
		}
		if (enumDecoAllowed.AllowBigDeco())
		{
			EnumDecoOccupied decoOccupiedAt = DecoManager.Instance.GetDecoOccupiedAt(x + this.m_X * 16, z + this.m_Z * 16);
			if (decoOccupiedAt > EnumDecoOccupied.Perimeter && decoOccupiedAt != EnumDecoOccupied.POI)
			{
				enumDecoAllowed = enumDecoAllowed.WithSize(EnumDecoAllowedSize.None);
			}
		}
		return enumDecoAllowed;
	}

	public EnumDecoAllowedSlope GetDecoAllowedSlopeAt(int x, int z)
	{
		return this.GetDecoAllowedAt(x, z).GetSlope();
	}

	public EnumDecoAllowedSize GetDecoAllowedSizeAt(int x, int z)
	{
		return this.GetDecoAllowedAt(x, z).GetSize();
	}

	public bool GetDecoAllowedStreetOnlyAt(int x, int z)
	{
		return this.GetDecoAllowedAt(x, z).GetStreetOnly();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EnsureDecoBiomeArray()
	{
		if (this.m_DecoBiomeArray == null)
		{
			this.m_DecoBiomeArray = new EnumDecoAllowed[256];
		}
	}

	public void SetDecoAllowedAt(int x, int z, EnumDecoAllowed _newVal)
	{
		this.EnsureDecoBiomeArray();
		int num = x + z * 16;
		EnumDecoAllowed enumDecoAllowed = this.m_DecoBiomeArray[num];
		EnumDecoAllowedSlope slope = enumDecoAllowed.GetSlope();
		if (slope > _newVal.GetSlope())
		{
			_newVal = _newVal.WithSlope(slope);
		}
		EnumDecoAllowedSize size = enumDecoAllowed.GetSize();
		if (size > _newVal.GetSize())
		{
			_newVal = _newVal.WithSize(size);
		}
		if (enumDecoAllowed.GetStreetOnly() && !_newVal.GetStreetOnly())
		{
			_newVal = _newVal.WithStreetOnly(true);
		}
		this.m_DecoBiomeArray[num] = _newVal;
	}

	public void SetDecoAllowedSlopeAt(int x, int z, EnumDecoAllowedSlope _newVal)
	{
		this.EnsureDecoBiomeArray();
		int num = x + z * 16;
		this.SetDecoAllowedAt(x, z, this.m_DecoBiomeArray[num].WithSlope(_newVal));
	}

	public void SetDecoAllowedSizeAt(int x, int z, EnumDecoAllowedSize _newVal)
	{
		this.EnsureDecoBiomeArray();
		int num = x + z * 16;
		this.SetDecoAllowedAt(x, z, this.m_DecoBiomeArray[num].WithSize(_newVal));
	}

	public void SetDecoAllowedStreetOnlyAt(int x, int z, bool _newVal)
	{
		this.EnsureDecoBiomeArray();
		int num = x + z * 16;
		this.SetDecoAllowedAt(x, z, this.m_DecoBiomeArray[num].WithStreetOnly(_newVal));
	}

	public Vector3 GetTerrainNormal(int _x, int _z)
	{
		int num = _x + _z * 16;
		Vector3 vector;
		vector.x = (float)((sbyte)this.m_NormalX[num]) / 127f;
		vector.y = (float)((sbyte)this.m_NormalY[num]) / 127f;
		vector.z = (float)((sbyte)this.m_NormalZ[num]) / 127f;
		return vector;
	}

	public float GetTerrainNormalY(int _x, int _z)
	{
		int num = _x + _z * 16;
		return (float)((sbyte)this.m_NormalY[num]) / 127f;
	}

	public void SetTerrainNormal(int x, int z, Vector3 _v)
	{
		int num = x + z * 16;
		this.m_NormalX[num] = (byte)Utils.FastClamp(_v.x * 127f, -128f, 127f);
		this.m_NormalY[num] = (byte)Utils.FastClamp(_v.y * 127f, -128f, 127f);
		this.m_NormalZ[num] = (byte)Utils.FastClamp(_v.z * 127f, -128f, 127f);
	}

	public Vector3i ToWorldPos()
	{
		return new Vector3i(this.m_X * 16, this.m_Y * 256, this.m_Z * 16);
	}

	public Vector3i ToWorldPos(int _x, int _y, int _z)
	{
		return new Vector3i(this.m_X * 16 + _x, this.m_Y * 256 + _y, this.m_Z * 16 + _z);
	}

	public Vector3i ToWorldPos(Vector3i _pos)
	{
		return new Vector3i(this.m_X * 16, this.m_Y * 256, this.m_Z * 16) + _pos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateFullMap()
	{
		if (this.mapColors == null)
		{
			this.mapColors = new ushort[256];
		}
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				int num = i + j * 16;
				int num2 = (int)this.m_HeightMap[num];
				int num3 = num2 >> 2;
				BlockValue blockValue = ((this.m_BlockLayers[num3] != null) ? this.m_BlockLayers[num3].GetAt(i, num2, j) : BlockValue.Air);
				WaterValue waterValue = this.GetWater(i, num2, j);
				while (num2 > 0 && (blockValue.isair || blockValue.Block.IsTerrainDecoration) && !waterValue.HasMass())
				{
					num2--;
					blockValue = ((this.m_BlockLayers[num3] != null) ? this.m_BlockLayers[num3].GetAt(i, num2, j) : BlockValue.Air);
					waterValue = this.GetWater(i, num2, j);
				}
				Color color = BlockLiquidv2.Color;
				if (!waterValue.HasMass())
				{
					float num4 = (float)((sbyte)this.m_NormalX[num]) / 127f;
					float num5 = (float)((sbyte)this.m_NormalY[num]) / 127f;
					float num6 = (float)((sbyte)this.m_NormalZ[num]) / 127f;
					color = blockValue.Block.GetMapColor(blockValue, new Vector3(num4, num5, num6), num2);
				}
				this.mapColors[num] = Utils.ToColor5(color);
			}
		}
		this.bMapDirty = false;
		ModEvents.SCalcChunkColorsDoneData scalcChunkColorsDoneData = new ModEvents.SCalcChunkColorsDoneData(this);
		ModEvents.CalcChunkColorsDone.Invoke(ref scalcChunkColorsDoneData);
	}

	public ushort[] GetMapColors()
	{
		if (this.mapColors == null || this.bMapDirty)
		{
			this.updateFullMap();
		}
		return this.mapColors;
	}

	public void OnDecorated()
	{
		this.CheckSameDensity();
		this.CheckOnlyTerrain();
	}

	public bool IsAreaMaster()
	{
		return this.m_X % 5 == 0 && this.m_Z % 5 == 0;
	}

	public bool IsAreaMasterCornerChunksLoaded(ChunkCluster _cc)
	{
		return _cc.GetChunkSync(this.m_X - 2, this.m_Z) != null && _cc.GetChunkSync(this.m_X, this.m_Z + 2) != null && _cc.GetChunkSync(this.m_X + 2, this.m_Z + 2) != null && _cc.GetChunkSync(this.m_X - 2, this.m_Z - 2) != null;
	}

	public static Vector3i ToAreaMasterChunkPos(Vector3i _worldBlockPos)
	{
		return new Vector3i(World.toChunkXZ(_worldBlockPos.x) / 5 * 5, World.toChunkY(_worldBlockPos.y), World.toChunkXZ(_worldBlockPos.z) / 5 * 5);
	}

	public bool IsAreaMasterDominantBiomeInitialized(ChunkCluster _cc)
	{
		if (this.AreaMasterDominantBiome != 255)
		{
			return true;
		}
		if (_cc == null)
		{
			return false;
		}
		for (int i = 0; i < 50; i++)
		{
			Chunk.biomeCnt[i] = 0;
		}
		for (int j = this.m_X - 2; j < this.m_X + 2; j++)
		{
			for (int k = this.m_Z - 2; k < this.m_Z + 2; k++)
			{
				Chunk chunkSync = _cc.GetChunkSync(j, k);
				if (chunkSync == null)
				{
					return false;
				}
				if (chunkSync.DominantBiome > 0)
				{
					Chunk.biomeCnt[(int)chunkSync.DominantBiome]++;
				}
			}
		}
		int num = 0;
		for (int l = 1; l < Chunk.biomeCnt.Length; l++)
		{
			if (Chunk.biomeCnt[l] > num)
			{
				this.AreaMasterDominantBiome = (byte)l;
				num = Chunk.biomeCnt[l];
			}
		}
		return true;
	}

	public ChunkAreaBiomeSpawnData GetChunkBiomeSpawnData()
	{
		if (this.AreaMasterDominantBiome == 255)
		{
			return null;
		}
		if (this.biomeSpawnData == null)
		{
			ChunkCustomData chunkCustomData;
			if (!this.ChunkCustomData.dict.TryGetValue("bspd.main", out chunkCustomData) || chunkCustomData == null)
			{
				chunkCustomData = new ChunkCustomData("bspd.main", ulong.MaxValue, false);
				this.ChunkCustomData.Set(chunkCustomData.key, chunkCustomData);
			}
			this.biomeSpawnData = new ChunkAreaBiomeSpawnData(this, this.AreaMasterDominantBiome, chunkCustomData);
		}
		return this.biomeSpawnData;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void removeExpiredCustomChunkDataEntries(ulong _worldTime)
	{
		List<string> list = null;
		for (int i = 0; i < this.ChunkCustomData.valueList.Count; i++)
		{
			if (this.ChunkCustomData.valueList[i].expiresInWorldTime <= _worldTime)
			{
				if (list == null)
				{
					list = new List<string>();
				}
				list.Add(this.ChunkCustomData.keyList[i]);
				this.ChunkCustomData.valueList[i].OnRemove(this);
			}
		}
		if (list != null)
		{
			for (int j = 0; j < list.Count; j++)
			{
				this.ChunkCustomData.Remove(list[j]);
			}
		}
	}

	public bool IsTraderArea(int _x, int _z)
	{
		Vector3i vector3i = this.worldPosIMin;
		vector3i.x += _x;
		vector3i.z += _z;
		return GameManager.Instance.World.IsWithinTraderArea(vector3i);
	}

	public override int GetHashCode()
	{
		return 31 * this.m_X + this.m_Z;
	}

	public void EnterReadLock()
	{
		this.sync.EnterReadLock();
	}

	public void EnterWriteLock()
	{
		this.sync.EnterWriteLock();
	}

	public void ExitReadLock()
	{
		this.sync.ExitReadLock();
	}

	public void ExitWriteLock()
	{
		this.sync.ExitWriteLock();
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj) && obj.GetHashCode() == this.GetHashCode();
	}

	public override string ToString()
	{
		if (this.cachedToString == null)
		{
			this.cachedToString = string.Format("Chunk_{0},{1}", this.m_X, this.m_Z);
		}
		return this.cachedToString;
	}

	public List<Chunk.DensityMismatchInformation> CheckDensities(bool _logAllMismatches = false)
	{
		Vector3i vector3i = new Vector3i(0, 0, 0);
		Vector3i vector3i2 = new Vector3i(16, 256, 16);
		int num = this.m_X << 4;
		int num2 = this.m_Y << 8;
		int num3 = this.m_Z << 4;
		bool flag = true;
		List<Chunk.DensityMismatchInformation> list = new List<Chunk.DensityMismatchInformation>();
		for (int i = vector3i.x; i < vector3i2.x; i++)
		{
			for (int j = vector3i.z; j < vector3i2.z; j++)
			{
				for (int k = vector3i.y; k < vector3i2.y; k++)
				{
					sbyte density = this.GetDensity(i, k, j);
					BlockValue block = this.GetBlock(i, k, j);
					bool flag2 = block.Block.shape.IsTerrain();
					bool flag3;
					if (flag2)
					{
						flag3 = density < 0;
					}
					else
					{
						flag3 = density >= 0;
					}
					if (!flag3)
					{
						Chunk.DensityMismatchInformation densityMismatchInformation = new Chunk.DensityMismatchInformation(num + i, num2 + k, num3 + j, density, block.type, flag2);
						list.Add(densityMismatchInformation);
						if (flag || _logAllMismatches)
						{
							Log.Warning(densityMismatchInformation.ToString());
							flag = false;
						}
					}
				}
			}
		}
		return list;
	}

	public bool RepairDensities()
	{
		Vector3i vector3i = new Vector3i(0, 0, 0);
		Vector3i vector3i2 = new Vector3i(16, 256, 16);
		bool flag = false;
		for (int i = vector3i.x; i < vector3i2.x; i++)
		{
			for (int j = vector3i.z; j < vector3i2.z; j++)
			{
				for (int k = vector3i.y; k < vector3i2.y; k++)
				{
					Block block = this.GetBlock(i, k, j).Block;
					sbyte density = this.GetDensity(i, k, j);
					if (block.shape.IsTerrain())
					{
						if (density >= 0)
						{
							this.SetDensity(i, k, j, -1);
							flag = true;
						}
					}
					else if (density < 0)
					{
						this.SetDensity(i, k, j, 1);
						flag = true;
					}
				}
			}
		}
		return flag;
	}

	public void LoopOverAllBlocks(ChunkBlockLayer.LoopBlocksDelegate _delegate, bool _bIncludeChilds = false, bool _bIncludeAirBlocks = false)
	{
		for (int i = 0; i < this.m_BlockLayers.Length; i++)
		{
			ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[i];
			if (chunkBlockLayer != null)
			{
				chunkBlockLayer.LoopOverAllBlocks(this, i << 2, _delegate, _bIncludeChilds, _bIncludeAirBlocks);
			}
		}
	}

	public IEnumerator LoopOverAllBlocksCoroutine(ChunkBlockLayer.LoopBlocksDelegate _delegate, bool _bIncludeChilds = false, bool _bIncludeAirBlocks = false)
	{
		int num;
		for (int i = 0; i < this.m_BlockLayers.Length; i = num + 1)
		{
			ChunkBlockLayer chunkBlockLayer = this.m_BlockLayers[i];
			if (chunkBlockLayer != null)
			{
				chunkBlockLayer.LoopOverAllBlocks(this, i << 2, _delegate, _bIncludeChilds, _bIncludeAirBlocks);
				yield return null;
			}
			num = i;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInside(int _x, int _y, int _z)
	{
		Vector3b vector3b = new Vector3b(_x, _y, _z);
		return this.insideDevicesHashSet.Contains(vector3b.GetHashCode());
	}

	public BlockFaceFlag RestoreCulledBlocks(World _world)
	{
		BlockFaceFlag blockFaceFlag = BlockFaceFlag.None;
		for (int i = this.insideDevices.Count - 1; i >= 0; i--)
		{
			Vector3b vector3b = this.insideDevices[i];
			if (vector3b.x == 0)
			{
				blockFaceFlag |= BlockFaceFlag.West;
			}
			else if (vector3b.x == 15)
			{
				blockFaceFlag |= BlockFaceFlag.East;
			}
			if (vector3b.z == 0)
			{
				blockFaceFlag |= BlockFaceFlag.North;
			}
			else if (vector3b.z == 15)
			{
				blockFaceFlag |= BlockFaceFlag.South;
			}
		}
		this.IsInternalBlocksCulled = false;
		return blockFaceFlag;
	}

	public bool HasFallingBlocks()
	{
		foreach (List<Entity> list in this.entityLists)
		{
			for (int j = 0; j < list.Count; j++)
			{
				if (list[j] is EntityFallingBlock || list[j] is EntityFallingBlocks)
				{
					return true;
				}
			}
		}
		return false;
	}

	[Conditional("DEBUG_CHUNK_PROFILE")]
	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ProfilerBegin(string _name)
	{
	}

	[Conditional("DEBUG_CHUNK_PROFILE")]
	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ProfilerEnd()
	{
	}

	[Conditional("DEBUG_CHUNK_RWCHECK")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void RWCheck(PooledBinaryReader stream)
	{
		if (stream.ReadInt32() != 1431655765)
		{
			Log.Error("Chunk !RWCheck");
		}
	}

	[Conditional("DEBUG_CHUNK_RWCHECK")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void RWCheck(PooledBinaryWriter stream)
	{
		stream.Write(1431655765);
	}

	[Conditional("DEBUG_CHUNK_TRIGGERLOG")]
	public void LogTrigger(string _format = "", params object[] _args)
	{
		_format = string.Format("{0} Chunk {1} trigger {2}", GameManager.frameCount, this.ChunkPos, _format);
		Log.Warning(_format, _args);
	}

	[Conditional("DEBUG_CHUNK_CHUNK")]
	public static void LogChunk(long _key, string _format = "", params object[] _args)
	{
		int num = WorldChunkCache.extractX(_key);
		int num2 = WorldChunkCache.extractZ(_key);
		if (num == 136 && num2 == 25)
		{
			_format = string.Format("{0} Chunk pos {1} {2}, {3}", new object[]
			{
				GameManager.frameCount,
				num,
				num2,
				_format
			});
			Log.Warning(_format, _args);
		}
	}

	[Conditional("DEBUG_CHUNK_ENTITY")]
	public void LogEntity(string _format = "", params object[] _args)
	{
		if (this.m_X == 136 && this.m_Z == 25)
		{
			_format = string.Format("{0} Chunk {1} entity {2}", GameManager.frameCount, this.ChunkPos, _format);
			Log.Warning(_format, _args);
		}
	}

	public void LogChunkState()
	{
		Log.Out(string.Concat(new string[]
		{
			string.Format("[FELLTHROUGHWORLD] Chunk {0} State\n", this.Key),
			string.Format("  Displayed: {0}\n", this.IsDisplayed),
			string.Format("  IsCollisionMeshGenerated: {0}\n", this.IsCollisionMeshGenerated),
			string.Format("  NeedsDecoration: {0}\n", this.NeedsDecoration),
			string.Format("  NeedsLightDecoration: {0}\n", this.NeedsLightDecoration),
			string.Format("  NeedsLightCalculation: {0}\n", this.NeedsLightCalculation),
			string.Format("  NeedsRegeneration: {0} {1}\n", this.NeedsRegeneration, this.FormatRegenerationLayers(this.m_NeedsRegenerationAtY)),
			string.Format("  NeedsCopying: {0} (layers: {1})", this.NeedsCopying, this.m_layerIndexQueue.Count)
		}));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string FormatRegenerationLayers(int mask)
	{
		if (mask == 0)
		{
			return "(none)";
		}
		if (mask == 65535)
		{
			return "(all layers)";
		}
		List<int> list = new List<int>();
		for (int i = 0; i < 16; i++)
		{
			if ((mask & (1 << i)) != 0)
			{
				list.Add(i);
			}
		}
		return "(Y layers: " + string.Join<int>(", ", list) + ")";
	}

	public const uint CurrentSaveVersion = 47U;

	public const uint SupportedSaveVersion = 32U;

	public const int cAreaMasterSizeChunks = 5;

	public const int cAreaMasterSizeBlocks = 80;

	public const int cTextureChannelCount = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockLayer[] m_BlockLayers;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel chnStability;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel chnDensity;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel chnLight;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel chnDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel[] chnTextures;

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkBlockChannel chnWater;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_X;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_Y;

	[PublicizedFrom(EAccessModifier.Private)]
	public int m_Z;

	public Vector3i worldPosIMin;

	public Vector3i worldPosIMax;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double cEntityListHeight = 16.0;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cEntityListCount = 16;

	public List<Entity>[] entityLists = new List<Entity>[16];

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryList<Vector3i, TileEntity> tileEntities = new DictionaryList<Vector3i, TileEntity>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> sleeperVolumes = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> triggerVolumes = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<int> wallVolumes = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_HeightMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_bTopSoilBroken;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_Biomes;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_BiomeIntensities;

	public byte DominantBiome;

	public byte AreaMasterDominantBiome = byte.MaxValue;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_NormalX;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_NormalY;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_NormalZ;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte[] m_TerrainHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityCreationData> entityStubs = new List<EntityCreationData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<EntityAsyncManager.EntityCreateHandle> pendingEntityCreateOps = new HashSet<EntityAsyncManager.EntityCreateHandle>();

	public DictionaryKeyValueList<string, ChunkCustomData> ChunkCustomData = new DictionaryKeyValueList<string, ChunkCustomData>();

	public ulong SavedInWorldTicks;

	public ulong LastTimeRandomTicked;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3b> insideDevices = new List<Vector3b>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> insideDevicesHashSet = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryList<Vector3i, BlockTrigger> triggerData = new DictionaryList<Vector3i, BlockTrigger>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly LinkedDictionary<int, PropValue> props = new LinkedDictionary<int, PropValue>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<int, PropEntityData> propEntities = new Dictionary<int, PropEntityData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PropEntityData> propEntitiesToRemove = new List<PropEntityData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public int propIdNext;

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryList<ulong, BlockEntityData> blockEntityStubs = new DictionaryList<ulong, BlockEntityData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<BlockEntityData> blockEntityStubsToRemove = new List<BlockEntityData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkAreaBiomeSpawnData biomeSpawnData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Queue<int> m_layerIndexQueue = new Queue<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public VoxelMeshLayer[] m_meshLayers = new VoxelMeshLayer[16];

	public volatile bool hasEntities;

	public bool isModified;

	[PublicizedFrom(EAccessModifier.Private)]
	public Bounds boundingBox;

	public DictionarySave<string, List<Vector3i>> IndexedBlocks = new DictionarySave<string, List<Vector3i>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public volatile int m_NeedsRegenerationAtY;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDecoAllowed[] m_DecoBiomeArray;

	[PublicizedFrom(EAccessModifier.Private)]
	public ushort[] mapColors;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bMapDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEmpty;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bEmptyDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public DictionaryKeyList<Vector3i, int> tickedBlocks = new DictionaryKeyList<Vector3i, int>();

	public bool IsInternalBlocksCulled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool stopStabilityCalculation;

	public OcclusionManager.OccludeeZone occludeeZone;

	public readonly ReaderWriterLockSlim sync = new ReaderWriterLockSlim();

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterSimulationNative.ChunkHandle waterSimHandle;

	public static int InstanceCount;

	public int TotalMemory;

	[PublicizedFrom(EAccessModifier.Private)]
	public int totalTris;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[][] trisInMesh = new int[16][];

	[PublicizedFrom(EAccessModifier.Private)]
	public int[][] sizeOfMesh = new int[16][];

	[PublicizedFrom(EAccessModifier.Private)]
	public WaterDebugManager.RendererHandle waterDebugHandle;

	public volatile bool InProgressCopying;

	public volatile bool InProgressDecorating;

	public volatile bool InProgressLighting;

	public volatile bool InProgressRegeneration;

	public volatile bool InProgressUnloading;

	public volatile bool InProgressSaving;

	public volatile bool InProgressNetworking;

	public volatile bool InProgressWaterSim;

	public volatile bool IsDisplayed;

	public volatile bool IsCollisionMeshGenerated;

	public volatile bool NeedsOnlyCollisionMesh;

	public int NeedsRegenerationDebug;

	public volatile bool NeedsDecoration;

	public volatile bool NeedsLightDecoration;

	public volatile bool NeedsLightCalculation;

	[PublicizedFrom(EAccessModifier.Private)]
	public static BlockValue bvPOIFiller;

	public static bool IgnorePaintTextures = false;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool spawnedBiomeParticles;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<GameObject> biomeParticles;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Transform> occlusionTs = new List<Transform>(200);

	public Chunk.DisplayState displayState;

	[PublicizedFrom(EAccessModifier.Private)]
	public int blockEntitiesIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<MeshRenderer> tempMeshRenderers = new List<MeshRenderer>();

	public int MeshLayerCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int[] biomeCnt = new int[50];

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedToString;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int dbChunkX = 136;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int dbChunkZ = 25;

	public enum LIGHT_TYPE
	{
		BLOCK,
		SUN
	}

	public enum DisplayState
	{
		Start,
		BlockEntities,
		Done
	}

	public struct DensityMismatchInformation
	{
		public DensityMismatchInformation(int _x, int _y, int _z, sbyte _density, int _bvType, bool _isTerrain)
		{
			this.x = _x;
			this.y = _y;
			this.z = _z;
			this.density = _density;
			this.bvType = _bvType;
			this.isTerrain = _isTerrain;
		}

		public string ToJsonString()
		{
			return string.Format("{{\"x\":{0}, \"y\":{1}, \"z\":{2}, \"density\":{3}, \"bvtype\":{4}, \"terrain\":{5}}}", new object[]
			{
				this.x,
				this.y,
				this.z,
				this.density,
				this.bvType,
				this.isTerrain.ToString().ToLower()
			});
		}

		public override string ToString()
		{
			return string.Format("DENSITYMISMATCH;{0};{1};{2};{3};{4};{5}", new object[] { this.x, this.y, this.z, this.density, this.isTerrain, this.bvType });
		}

		public int x;

		public int y;

		public int z;

		public sbyte density;

		public int bvType;

		public bool isTerrain;
	}
}
