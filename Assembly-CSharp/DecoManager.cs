using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DecoManager
{
	public static DecoManager Instance
	{
		get
		{
			if (DecoManager.m_Instance == null)
			{
				DecoManager.m_Instance = new DecoManager();
			}
			return DecoManager.m_Instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DecoManager()
	{
	}

	public IEnumerator OnWorldLoaded(int _worldWidth, int _worldHeight, World _world, IChunkProvider _chunkProvider)
	{
		if (!this.IsEnabled)
		{
			yield break;
		}
		MicroStopwatch mswYields = new MicroStopwatch();
		this.world = _world;
		this.worldWidth = _worldWidth;
		this.worldHeight = _worldHeight;
		this.worldWidthHalf = this.worldWidth / 2;
		this.worldHeightHalf = this.worldHeight / 2;
		this.occupiedMap = new DecoOccupiedMap(this.worldWidth, this.worldHeight);
		this.bFixedSize = _world.ChunkCache.IsFixedSize;
		this.chunkProvider = _chunkProvider;
		this.terrainGenerator = ((this.chunkProvider != null) ? _chunkProvider.GetTerrainGenerator() : null);
		this.resourceNoise = new PerlinNoise(_world.Seed);
		yield return null;
		if (this.chunkProvider != null)
		{
			yield return this.chunkProvider.FillOccupiedMap(this.worldWidth, this.worldHeight, this.occupiedMap, this.overridePOIList);
		}
		yield return null;
		int num = DecoChunk.ToDecoChunkPos(-this.worldWidth / 2);
		int num2 = DecoChunk.ToDecoChunkPos(this.worldWidth / 2);
		int num3 = DecoChunk.ToDecoChunkPos(-this.worldHeight / 2);
		int num4 = DecoChunk.ToDecoChunkPos(this.worldHeight / 2);
		this.decoChunks = new Dictionary<int, DecoChunk>((num2 - num + 1) * (num4 - num3 + 1));
		for (int i = num; i <= num2; i++)
		{
			for (int j = num3; j <= num4; j++)
			{
				DecoChunk decoChunk = new DecoChunk(i, j, i, j);
				this.decoChunks.Add(DecoChunk.MakeKey16(i, j), decoChunk);
			}
		}
		yield return null;
		this.filenamePath = GameIO.GetSaveGameDir() + "/decoration.7dt";
		bool fileLoaded = this.TryLoad();
		string text = "[DECO] read {0}";
		object[] array = new object[1];
		int num5 = 0;
		HashSet<DecoObject> hashSet = this.loadedDecos;
		array[num5] = ((hashSet != null) ? new int?(hashSet.Count) : null);
		Log.Out(text, array);
		yield return null;
		int chunkStartX = -(_worldWidth / 2) / 128;
		int chunkEndX = _worldWidth / 2 / 128;
		int chunkStartZ = -(_worldHeight / 2) / 128;
		int chunkEndZ = _worldHeight / 2 / 128;
		int totalDecorated = 0;
		mswYields.ResetAndRestart();
		if (PlatformOptimizations.FileBackedArrays)
		{
			this.fileBackedOccupiedMap = new FileBackedDecoOccupiedMap(_worldWidth, _worldHeight);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !fileLoaded)
		{
			Log.Out("DecoManager chunks");
			int num6;
			for (int z = chunkStartZ; z <= chunkEndZ; z = num6 + 1)
			{
				for (int k = chunkStartX; k <= chunkEndX; k++)
				{
					GameRandom gameRandom = Utils.RandomFromSeedOnPos(k, z, this.world.Seed);
					DecoChunk decoChunk2 = this.decoChunks[DecoChunk.MakeKey16(k, z)];
					totalDecorated += this.decorateChunkRandom(decoChunk2, gameRandom);
					GameRandomManager.Instance.FreeGameRandom(gameRandom);
				}
				if (mswYields.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
				{
					yield return null;
					mswYields.ResetAndRestart();
				}
				if (PlatformOptimizations.FileBackedArrays && z > chunkStartZ)
				{
					this.fileBackedOccupiedMap.CopyDecoChunkRow(z - 1, this.occupiedMap.GetData());
				}
				num6 = z;
			}
			if (PlatformOptimizations.FileBackedArrays)
			{
				this.occupiedMap = null;
			}
		}
		yield return null;
		if (this.loadedDecos != null)
		{
			foreach (DecoObject decoObject in this.loadedDecos)
			{
				this.addLoadedDecoration(decoObject);
			}
			this.loadedDecos = null;
			for (int l = chunkStartZ; l <= chunkEndZ; l++)
			{
				for (int m = chunkStartX; m <= chunkEndX; m++)
				{
					this.decoChunks[DecoChunk.MakeKey16(m, l)].isDecorated = true;
				}
				if (PlatformOptimizations.FileBackedArrays && l > chunkStartZ)
				{
					this.fileBackedOccupiedMap.CopyDecoChunkRow(l - 1, this.occupiedMap.GetData());
				}
			}
			if (PlatformOptimizations.FileBackedArrays)
			{
				this.occupiedMap = null;
			}
		}
		this.bDirty = true;
		yield break;
	}

	public void OriginChanged(Vector3 _offset)
	{
		foreach (KeyValuePair<int, DecoChunk> keyValuePair in this.decoChunks)
		{
			GameObject rootObj = keyValuePair.Value.rootObj;
			if (rootObj)
			{
				rootObj.transform.position += _offset;
			}
		}
	}

	public void OnWorldUnloaded()
	{
		if (!this.IsEnabled)
		{
			return;
		}
		if (this.updateCoroutine != null)
		{
			ThreadManager.StopCoroutine(this.updateCoroutine);
			this.updateCoroutine = null;
		}
		foreach (KeyValuePair<int, DecoChunk> keyValuePair in this.decoChunks)
		{
			keyValuePair.Value.Destroy();
		}
		this.occupiedMap = null;
		if (PlatformOptimizations.FileBackedArrays)
		{
			FileBackedDecoOccupiedMap fileBackedDecoOccupiedMap = this.fileBackedOccupiedMap;
			if (fileBackedDecoOccupiedMap != null)
			{
				fileBackedDecoOccupiedMap.Dispose();
			}
			this.fileBackedOccupiedMap = null;
		}
		this.overridePOIList = null;
		this.checkDelayTicks = 0;
		this.decoChunks.Clear();
		this.visibleDecoChunks.Clear();
		this.loadedDecos = null;
		this.addDecosFromThread.Clear();
		this.removeDecosFromThread.Clear();
		if (this.writeTask != null)
		{
			this.writeTask.WaitForEnd();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryLoad()
	{
		if (GameManager.Instance.IsEditMode() || !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || !SdFile.Exists(this.filenamePath))
		{
			return false;
		}
		using (Stream stream = SdFile.OpenRead(this.filenamePath))
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(stream);
				byte b = pooledBinaryReader.ReadByte();
				if (b == 6)
				{
					this.Read(pooledBinaryReader, (int)b, true);
					return true;
				}
				Log.Warning(string.Format("Saved decoration data is out of date. Saved version is ({0}). Current version is ({1}). ", b, 6) + "Decorations will be regenerated for this map, but it is recommended to start a new game.");
			}
		}
		SdFile.Delete(this.filenamePath);
		return false;
	}

	public void Save()
	{
		if (!this.IsEnabled || GameManager.Instance.IsEditMode() || !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (!this.bDirty)
		{
			return;
		}
		if (this.writeTask != null)
		{
			this.writeTask.WaitForEnd();
		}
		using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
		{
			this.writeStream.Position = 0L;
			pooledBinaryWriter.SetBaseStream(this.writeStream);
			pooledBinaryWriter.Write(6);
			this.Write(pooledBinaryWriter, Block.nameIdMapping);
		}
		this.writeTask = ThreadManager.AddSingleTask(new ThreadManager.TaskFunctionDelegate(this.WriteTask), null, null, true);
		this.bDirty = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void WriteTask(ThreadManager.TaskInfo _taskInfo)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		using (Stream stream = SdFile.Open(this.filenamePath, FileMode.Create, FileAccess.Write, FileShare.Read))
		{
			this.writeStream.Position = 0L;
			StreamUtils.StreamCopy(this.writeStream, stream, null, true);
			this.writeStream.SetLength(0L);
		}
		Log.Out(string.Format("[DECO] write thread {0}ms", microStopwatch.ElapsedMilliseconds));
	}

	public void Read(BinaryReader _br, int _version = 2147483647, bool _resetExisting = true)
	{
		if (_resetExisting)
		{
			this.loadedDecos = new HashSet<DecoObject>();
		}
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			DecoObject decoObject = new DecoObject();
			decoObject.Read(_br);
			this.loadedDecos.Add(decoObject);
		}
	}

	public void Write(BinaryWriter _bw, NameIdMapping _blockMap)
	{
		MicroStopwatch microStopwatch = new MicroStopwatch();
		int num = 0;
		List<DecoObject> list = this.decoWriteList;
		lock (list)
		{
			this.GenerateDecoWriteList();
			num = this.decoWriteList.Count;
			_bw.Write(num);
			for (int i = 0; i < num; i++)
			{
				this.decoWriteList[i].Write(_bw, _blockMap);
			}
			this.decoWriteList.Clear();
		}
		Log.Out(string.Format("[DECO] written {0}, in {1}ms", num, microStopwatch.ElapsedMilliseconds));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateDecoWriteList()
	{
		List<DecoObject> list = this.decoWriteList;
		lock (list)
		{
			this.decoWriteList.Clear();
			foreach (KeyValuePair<int, DecoChunk> keyValuePair in this.decoChunks)
			{
				foreach (KeyValuePair<long, List<DecoObject>> keyValuePair2 in keyValuePair.Value.decosPerSmallChunks)
				{
					List<DecoObject> value = keyValuePair2.Value;
					for (int i = 0; i < value.Count; i++)
					{
						this.decoWriteList.Add(value[i]);
					}
				}
			}
		}
	}

	public void UpdateTick(World _world)
	{
		if (!this.IsEnabled)
		{
			return;
		}
		this.checkDelayTicks--;
		if (this.updateCoroutine != null)
		{
			return;
		}
		if (this.addDecosFromThread.Count > 0)
		{
			this.checkDelayTicks = 0;
			List<DecoManager.SAddDecoInfo> list = this.addDecosFromThread;
			lock (list)
			{
				for (int i = 0; i < this.addDecosFromThread.Count; i++)
				{
					this.AddDecorationAt(this.addDecosFromThread[i].world, this.addDecosFromThread[i].bv, this.addDecosFromThread[i].pos, this.addDecosFromThread[i].bForceBlockYPos);
				}
				this.addDecosFromThread.Clear();
			}
		}
		if (this.removeDecosFromThread.Count > 0)
		{
			this.checkDelayTicks = 0;
			List<Vector3i> list2 = this.removeDecosFromThread;
			lock (list2)
			{
				for (int j = 0; j < this.removeDecosFromThread.Count; j++)
				{
					this.RemoveDecorationAt(this.removeDecosFromThread[j]);
				}
				this.removeDecosFromThread.Clear();
			}
		}
		if (this.resetDecosInWorldRectFromThread.Count > 0)
		{
			this.checkDelayTicks = 0;
			List<Rect> list3 = this.resetDecosInWorldRectFromThread;
			lock (list3)
			{
				for (int k = 0; k < this.resetDecosInWorldRectFromThread.Count; k++)
				{
					this.ResetDecosInWorldRect(this.resetDecosInWorldRectFromThread[k]);
				}
				this.resetDecosInWorldRectFromThread.Clear();
			}
		}
		if (this.resetDecosForWorldChunkFromThread.Count > 0)
		{
			this.checkDelayTicks = 0;
			List<long> list4 = this.resetDecosForWorldChunkFromThread;
			lock (list4)
			{
				for (int l = 0; l < this.resetDecosForWorldChunkFromThread.Count; l++)
				{
					this.ResetDecosForWorldChunk(this.resetDecosForWorldChunkFromThread[l]);
				}
				this.resetDecosForWorldChunkFromThread.Clear();
			}
		}
		if (this.checkDelayTicks > 0)
		{
			return;
		}
		this.checkDelayTicks = 20;
		new MicroStopwatch();
		this.playersToCheck.Clear();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			this.playersToCheck.AddRange(_world.Players.list);
		}
		else
		{
			EntityPlayerLocal primaryPlayer = _world.GetPrimaryPlayer();
			if (primaryPlayer == null)
			{
				return;
			}
			this.playersToCheck.Add(primaryPlayer);
		}
		if (this.IsHidden)
		{
			this.playersToCheck.Clear();
		}
		this.chunksAroundPlayers.Clear();
		for (int m = this.playersToCheck.Count - 1; m >= 0; m--)
		{
			EntityPlayer entityPlayer = this.playersToCheck[m];
			Vector3i blockPosition = entityPlayer.GetBlockPosition();
			int num = DecoChunk.ToDecoChunkPos(blockPosition.x);
			int num2 = DecoChunk.ToDecoChunkPos(blockPosition.z);
			int num3 = (entityPlayer.isEntityRemote ? 1 : GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTreeDistance));
			int num4 = num - num3;
			int num5 = num + num3;
			int num6 = num2 - num3;
			int num7 = num2 + num3;
			for (int n = num4; n <= num5; n++)
			{
				for (int num8 = num6; num8 <= num7; num8++)
				{
					this.chunksAroundPlayers.Add(DecoChunk.MakeKey16(n, num8));
				}
			}
		}
		this.updateCoroutine = ThreadManager.StartCoroutine(this.UpdateDecorationsCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator UpdateDecorationsCo()
	{
		int waitCount = 0;
		this.msUpdate.ResetAndRestart();
		int num;
		for (int i = this.visibleDecoChunks.Count - 1; i >= 0; i = num - 1)
		{
			DecoChunk decoChunk = this.visibleDecoChunks[i];
			bool flag = this.chunksAroundPlayers.Contains(DecoChunk.MakeKey16(decoChunk.decoChunkX, decoChunk.decoChunkZ));
			decoChunk.SetVisible(flag);
			if (!flag)
			{
				decoChunk.Destroy();
			}
			if (this.msUpdate.ElapsedMicroseconds > 900L)
			{
				num = waitCount;
				waitCount = num + 1;
				yield return null;
				this.msUpdate.ResetAndRestart();
			}
			num = i;
		}
		this.visibleDecoChunks.Clear();
		foreach (int num2 in this.chunksAroundPlayers)
		{
			DecoChunk decoChunk;
			this.decoChunks.TryGetValue(num2, out decoChunk);
			if (decoChunk != null)
			{
				this.visibleDecoChunks.Add(decoChunk);
				DecoChunk decoChunk2 = decoChunk;
				lock (decoChunk2)
				{
					if (!decoChunk.isDecorated)
					{
						GameRandom gameRandom = Utils.RandomFromSeedOnPos(decoChunk.decoChunkX, decoChunk.decoChunkZ, this.world.Seed);
						this.decorateChunkRandom(decoChunk, gameRandom);
						GameRandomManager.Instance.FreeGameRandom(gameRandom);
					}
				}
				if (this.msUpdate.ElapsedMicroseconds > 900L)
				{
					num = waitCount;
					waitCount = num + 1;
					yield return null;
					this.msUpdate.ResetAndRestart();
				}
				if (decoChunk.decosPerSmallChunks.Count > 0 && !decoChunk.isGameObjectUpdated)
				{
					decoChunk.UpdateGameObject();
				}
				if (!decoChunk.isModelsUpdated)
				{
					yield return decoChunk.UpdateModels(this.world, this.msUpdate);
					if (this.msUpdate.ElapsedMicroseconds > 900L)
					{
						num = waitCount;
						waitCount = num + 1;
						yield return null;
						this.msUpdate.ResetAndRestart();
					}
				}
			}
		}
		HashSet<int>.Enumerator enumerator = default(HashSet<int>.Enumerator);
		this.updateCoroutine = null;
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DecoObject GetDecoObject()
	{
		return new DecoObject();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DecoChunk GetDecoChunkAt(int _x, int _z)
	{
		int num = DecoChunk.MakeKey16(_x, _z);
		DecoChunk decoChunk;
		if (!this.decoChunks.TryGetValue(num, out decoChunk))
		{
			return null;
		}
		return decoChunk;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int decorateChunkRandom(DecoChunk _decoChunk, GameRandom rnd)
	{
		if (this.bFixedSize)
		{
			_decoChunk.isDecorated = true;
			return 0;
		}
		if (this.chunkProvider == null)
		{
			return 0;
		}
		IBiomeProvider biomeProvider = this.chunkProvider.GetBiomeProvider();
		if (biomeProvider == null)
		{
			return 0;
		}
		int num = _decoChunk.decoChunkX * 128;
		int num2 = num + 128;
		int num3 = _decoChunk.decoChunkZ * 128;
		int num4 = num3 + 128;
		int num5 = 0;
		for (int i = 0; i < 1000; i++)
		{
			int num6 = rnd.RandomRange(num, num2 - 1);
			int num7 = rnd.RandomRange(num3, num4 - 1);
			if (this.occupiedMap.Get(num6, num7) <= EnumDecoOccupied.Stop_BigDeco && !this.occupiedMap.CheckArea(num6 - 2, num7 - 2, EnumDecoOccupied.POI, 5, 5))
			{
				BiomeDefinition biomeOrSubAt = biomeProvider.GetBiomeOrSubAt(num6, num7);
				if (biomeOrSubAt != null)
				{
					float num8 = -1f;
					BlockValue blockValue = BlockValue.Air;
					int num9 = 3;
					for (int j = biomeOrSubAt.m_DistantDecoBlocks.Count - 1; j >= 0; j--)
					{
						BiomeBlockDecoration biomeBlockDecoration = biomeOrSubAt.m_DistantDecoBlocks[j];
						if (rnd.RandomFloat <= biomeBlockDecoration.prob * 0.125f * 16f)
						{
							if (biomeBlockDecoration.checkResourceOffsetY < 2147483647)
							{
								if (num8 < 0f)
								{
									num8 = this.terrainGenerator.GetTerrainHeightAt(num6, num7) + 1f;
								}
								if (!GameUtils.CheckOreNoiseAt(this.resourceNoise, num6, (int)num8 + biomeBlockDecoration.checkResourceOffsetY, num7))
								{
									goto IL_0173;
								}
							}
							blockValue = biomeBlockDecoration.blockValues[0];
							num9 = biomeBlockDecoration.randomRotateMax;
							break;
						}
						IL_0173:;
					}
					Block block;
					if (!blockValue.isair && (block = blockValue.Block) != null && block.IsDistantDecoration)
					{
						BlockValue blockValue2 = new BlockValue((uint)blockValue.type);
						blockValue2.rotation = BiomeBlockDecoration.GetRandomRotation(rnd.RandomFloat, (block.isMultiBlock && num9 > 3) ? 3 : num9);
						if (this.TryAddToOccupiedMap(block, num6, num7, blockValue2.rotation, true))
						{
							if (num8 < 0f)
							{
								num8 = this.terrainGenerator.GetTerrainHeightAt(num6, num7) + 1f;
							}
							int num10 = (int)(num8 + 0.5f);
							DecoObject decoObject = this.GetDecoObject();
							decoObject.Init(new Vector3i(num6, num10, num7), num8, blockValue2, DecoState.GeneratedActive);
							_decoChunk.AddDecoObject(decoObject, false);
							this.bDirty = true;
							num5++;
						}
					}
				}
			}
		}
		_decoChunk.isDecorated = true;
		return num5;
	}

	public void GetDecorationsOnChunk(int _chunkX, int _chunkZ, List<SBlockPosValue> _multiBlockList)
	{
		int num = 0;
		try
		{
			_multiBlockList.Clear();
			num = 1;
			if (this.IsEnabled)
			{
				int num2 = DecoChunk.ToDecoChunkPos(_chunkX * 16);
				int num3 = DecoChunk.ToDecoChunkPos(_chunkZ * 16);
				DecoChunk decoChunk;
				if (this.decoChunks.TryGetValue(DecoChunk.MakeKey16(num2, num3), out decoChunk))
				{
					num = 2;
					DecoChunk decoChunk2 = decoChunk;
					lock (decoChunk2)
					{
						if (!decoChunk.isDecorated)
						{
							Log.Error("Decorating chunk, should not happen at this point!");
							GameRandom gameRandom = Utils.RandomFromSeedOnPos(decoChunk.decoChunkX, decoChunk.decoChunkZ, this.world.Seed);
							this.decorateChunkRandom(decoChunk, gameRandom);
							GameRandomManager.Instance.FreeGameRandom(gameRandom);
						}
					}
					num = 3;
					List<DecoObject> list;
					if (decoChunk.decosPerSmallChunks.TryGetValue(WorldChunkCache.MakeChunkKey(_chunkX, _chunkZ), out list))
					{
						num = 4;
						for (int i = 0; i < list.Count; i++)
						{
							DecoObject decoObject = list[i];
							if (decoObject == null)
							{
								Log.Warning("DecoManager decosInChunk #{0} null at {1}, {2}", new object[] { i, _chunkX, _chunkZ });
							}
							else if (decoObject.state != DecoState.GeneratedInactive)
							{
								_multiBlockList.Add(new SBlockPosValue(new Vector3i(decoObject.pos.x, decoObject.pos.y, decoObject.pos.z), decoObject.bv));
							}
						}
					}
				}
			}
		}
		catch (NullReferenceException ex)
		{
			Log.Error("Exception position: " + num.ToString());
			throw ex;
		}
	}

	public bool GetParentBlockOfDecoration(Transform _t, out Vector3i _blockPos, out DecoObject _decoObject)
	{
		_blockPos = Vector3i.zero;
		_decoObject = null;
		if (!this.IsEnabled)
		{
			return false;
		}
		Transform transform = RootTransformRefParent.FindRoot(_t);
		Vector3 position = transform.position;
		int num = Utils.Fastfloor(position.x - DecoManager.cDecoMiddleOffset.x + Origin.position.x);
		int num2 = Utils.Fastfloor(position.z - DecoManager.cDecoMiddleOffset.z + Origin.position.z);
		DecoChunk decoChunk;
		if (!this.decoChunks.TryGetValue(DecoChunk.MakeKey16(DecoChunk.ToDecoChunkPos(num), DecoChunk.ToDecoChunkPos(num2)), out decoChunk))
		{
			return false;
		}
		if (!decoChunk.isModelsUpdated)
		{
			return false;
		}
		List<DecoObject> list;
		if (!decoChunk.decosPerSmallChunks.TryGetValue(WorldChunkCache.MakeChunkKey(World.toChunkXZ(num), World.toChunkXZ(num2)), out list))
		{
			return false;
		}
		for (int i = list.Count - 1; i >= 0; i--)
		{
			DecoObject decoObject = list[i];
			if (decoObject.state != DecoState.GeneratedInactive && decoObject.go && decoObject.go.transform == transform)
			{
				_decoObject = decoObject;
				_blockPos = decoObject.pos;
				return true;
			}
		}
		return false;
	}

	public Transform GetDecorationTransform(Vector3i _blockPos, bool _bDetachTransform = false)
	{
		if (!this.IsEnabled)
		{
			return null;
		}
		DecoChunk decoChunk;
		if (!this.decoChunks.TryGetValue(DecoChunk.MakeKey16(DecoChunk.ToDecoChunkPos(_blockPos.x), DecoChunk.ToDecoChunkPos(_blockPos.z)), out decoChunk))
		{
			return null;
		}
		List<DecoObject> list;
		if (!decoChunk.decosPerSmallChunks.TryGetValue(WorldChunkCache.MakeChunkKey(World.toChunkXZ(_blockPos.x), World.toChunkXZ(_blockPos.z)), out list))
		{
			return null;
		}
		int i = list.Count - 1;
		while (i >= 0)
		{
			DecoObject decoObject = list[i];
			if (decoObject.state != DecoState.GeneratedInactive && !(decoObject.pos != _blockPos))
			{
				if (decoObject.go == null)
				{
					return null;
				}
				Transform transform = decoObject.go.transform;
				if (_bDetachTransform)
				{
					if (OcclusionManager.Instance.cullDecorations)
					{
						OcclusionManager.Instance.RemoveDeco(decoChunk, decoObject.go.transform);
					}
					decoObject.go = null;
				}
				return transform;
			}
			else
			{
				i--;
			}
		}
		return null;
	}

	public void AddDecorationAt(World _world, BlockValue _blockValue, Vector3i _blockPos, bool _bForceBlockYPos = false)
	{
		if (!ThreadManager.IsMainThread())
		{
			List<DecoManager.SAddDecoInfo> list = this.addDecosFromThread;
			lock (list)
			{
				this.addDecosFromThread.Add(new DecoManager.SAddDecoInfo
				{
					world = _world,
					bv = _blockValue,
					pos = _blockPos,
					bForceBlockYPos = _bForceBlockYPos
				});
				return;
			}
		}
		float num = (float)_blockPos.y;
		if (!_bForceBlockYPos && _blockPos.y > 0 && this.terrainGenerator != null)
		{
			BlockValue block = this.world.GetBlock(_blockPos - new Vector3i(0, 1, 0));
			if (block.Block != null && block.Block.shape.IsTerrain())
			{
				num = this.terrainGenerator.GetTerrainHeightAt(_blockPos.x, _blockPos.z) + 1f;
			}
		}
		this.bDirty = true;
		DecoChunk decoChunkAt = this.GetDecoChunkAt(DecoChunk.ToDecoChunkPos(_blockPos.x), DecoChunk.ToDecoChunkPos(_blockPos.z));
		if (decoChunkAt != null)
		{
			DecoObject decoObjectAt = decoChunkAt.GetDecoObjectAt(_blockPos);
			if (decoObjectAt != null)
			{
				if (decoObjectAt.realYPos == num && decoObjectAt.bv.Equals(_blockValue) && decoObjectAt.bv.rotation == _blockValue.rotation)
				{
					return;
				}
				decoChunkAt.RemoveDecoObject(decoObjectAt);
			}
		}
		DecoObject decoObject = this.GetDecoObject();
		decoObject.Init(_blockPos, num, _blockValue, DecoState.Dynamic);
		if (decoChunkAt == null)
		{
			if (this.loadedDecos == null)
			{
				this.loadedDecos = new HashSet<DecoObject>();
			}
			this.loadedDecos.Add(decoObject);
			return;
		}
		decoChunkAt.AddDecoObject(decoObject, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addLoadedDecoration(DecoObject _decoObject)
	{
		if (!this.IsEnabled)
		{
			return;
		}
		DecoChunk decoChunkAt = this.GetDecoChunkAt(DecoChunk.ToDecoChunkPos(_decoObject.pos.x), DecoChunk.ToDecoChunkPos(_decoObject.pos.z));
		if (decoChunkAt == null)
		{
			return;
		}
		decoChunkAt.AddDecoObject(_decoObject, false);
		if (_decoObject.state != DecoState.Dynamic)
		{
			this.TryAddToOccupiedMap(_decoObject.bv.Block, _decoObject.pos.x, _decoObject.pos.z, _decoObject.bv.rotation, false);
		}
		this.bDirty = true;
	}

	public static int CheckPosition(int worldWidth, int worldHeight, int _x, int _z)
	{
		int num = worldWidth / 2;
		int num2 = worldHeight / 2;
		if (_x < -num || _x >= num || _z < -num2 || _z >= num2)
		{
			return -1;
		}
		return _x + num + (_z + num2) * worldWidth;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool TryAddToOccupiedMap(Block block, int xWorld, int zWorld, byte rotationByte, bool enableStopBigDecoCheck)
	{
		int num = xWorld;
		int num2 = zWorld;
		int num3 = 1;
		int num4 = 1;
		if (!block.isMultiBlock)
		{
			this.occupiedMap.Set(xWorld, zWorld, EnumDecoOccupied.Deco);
		}
		else
		{
			switch (rotationByte)
			{
			case 1:
				num3 = block.multiBlockPos.dim.z;
				num4 = block.multiBlockPos.dim.x;
				break;
			case 2:
				num3 = block.multiBlockPos.dim.x;
				num4 = block.multiBlockPos.dim.z;
				break;
			case 3:
				num3 = block.multiBlockPos.dim.z;
				num4 = block.multiBlockPos.dim.x;
				break;
			default:
				num3 = block.multiBlockPos.dim.x;
				num4 = block.multiBlockPos.dim.z;
				break;
			}
			num = ((num3 % 2 == 0) ? (xWorld - num3 / 2 + 1) : (xWorld - num3 / 2));
			num2 = ((num4 % 2 == 0) ? (zWorld - num4 / 2 + 1) : (zWorld - num4 / 2));
			if (enableStopBigDecoCheck && this.occupiedMap.CheckArea(num, num2, EnumDecoOccupied.Stop_BigDeco, num3, num4))
			{
				return false;
			}
			this.occupiedMap.SetArea(num, num2, EnumDecoOccupied.Deco, num3, num4);
		}
		if (block.BigDecorationRadius > 0)
		{
			this.occupiedMap.SetArea(num - block.BigDecorationRadius, num2 - block.BigDecorationRadius, EnumDecoOccupied.Perimeter, block.BigDecorationRadius * 2 + num3, block.BigDecorationRadius * 2 + num4);
		}
		return true;
	}

	public bool RemoveDecorationAt(Vector3i _blockPos)
	{
		if (!this.IsEnabled)
		{
			return false;
		}
		int num = DecoChunk.MakeKey16(DecoChunk.ToDecoChunkPos(_blockPos.x), DecoChunk.ToDecoChunkPos(_blockPos.z));
		DecoChunk decoChunk;
		if (!this.decoChunks.TryGetValue(num, out decoChunk))
		{
			return false;
		}
		if (!ThreadManager.IsMainThread())
		{
			List<Vector3i> list = this.removeDecosFromThread;
			lock (list)
			{
				this.removeDecosFromThread.Add(_blockPos);
				return true;
			}
		}
		this.bDirty = true;
		return decoChunk.RemoveDecoObject(_blockPos);
	}

	public void ClearDecoObjectsInArea(Vector3i _boundsPosition, Vector3i _boundsSize)
	{
		if (!this.IsEnabled)
		{
			return;
		}
		int num = DecoChunk.ToDecoChunkPos(_boundsPosition.x);
		int num2 = DecoChunk.ToDecoChunkPos(_boundsPosition.z);
		int num3 = DecoChunk.ToDecoChunkPos(_boundsPosition.x + _boundsSize.x);
		int num4 = DecoChunk.ToDecoChunkPos(_boundsPosition.z + _boundsSize.z);
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				int num5 = DecoChunk.MakeKey16(i, j);
				DecoChunk decoChunk;
				if (this.decoChunks.TryGetValue(num5, out decoChunk))
				{
					decoChunk.ClearDecoObjectsInArea(_boundsPosition, _boundsSize);
					this.bDirty = true;
				}
			}
		}
	}

	public EnumDecoOccupied GetDecoOccupiedAt(int _x, int _z)
	{
		if (!this.IsEnabled)
		{
			return EnumDecoOccupied.Free;
		}
		if (this.occupiedMap == null && !PlatformOptimizations.FileBackedArrays)
		{
			return EnumDecoOccupied.NoneAllowed;
		}
		int num;
		if ((num = DecoManager.CheckPosition(this.worldWidth, this.worldHeight, _x, _z)) < 0)
		{
			return EnumDecoOccupied.NoneAllowed;
		}
		DecoChunk decoChunk;
		if (!this.decoChunks.TryGetValue(DecoChunk.MakeKey16(DecoChunk.ToDecoChunkPos(_x), DecoChunk.ToDecoChunkPos(_z)), out decoChunk))
		{
			return EnumDecoOccupied.NoneAllowed;
		}
		if (!decoChunk.isDecorated)
		{
			DecoChunk decoChunk2 = decoChunk;
			lock (decoChunk2)
			{
				if (!decoChunk.isDecorated)
				{
					Log.Error("Should not be decorating here!");
					GameRandom gameRandom = Utils.RandomFromSeedOnPos(decoChunk.decoChunkX, decoChunk.decoChunkZ, this.world.Seed);
					this.decorateChunkRandom(decoChunk, gameRandom);
					GameRandomManager.Instance.FreeGameRandom(gameRandom);
				}
			}
		}
		if (PlatformOptimizations.FileBackedArrays)
		{
			return this.fileBackedOccupiedMap.Get(num);
		}
		return this.occupiedMap.Get(num);
	}

	public void SetChunkDistance(int _distance)
	{
		GamePrefs.Set(EnumGamePrefs.OptionsGfxTreeDistance, _distance);
	}

	public void SetBlock(World _world, Vector3i _blockPos, BlockValue _bv)
	{
		if (_bv.isair)
		{
			this.RemoveDecorationAt(_blockPos);
			return;
		}
		this.RemoveDecorationAt(_blockPos);
		this.AddDecorationAt(_world, _bv, _blockPos, true);
	}

	public void SaveDebugTexture(string path, bool _includeFlatAreas = false)
	{
		if (PlatformOptimizations.FileBackedArrays)
		{
			this.fileBackedOccupiedMap.SaveAsTexture(path, true, null);
			return;
		}
		DecoOccupiedMap decoOccupiedMap = this.occupiedMap;
		if (decoOccupiedMap == null)
		{
			return;
		}
		decoOccupiedMap.SaveAsTexture(path, _includeFlatAreas, null);
	}

	public void SaveDebugTexture(string path, List<FlatArea> flatAreas)
	{
		if (PlatformOptimizations.FileBackedArrays)
		{
			this.fileBackedOccupiedMap.SaveAsTexture(path, true, flatAreas);
			return;
		}
		DecoOccupiedMap decoOccupiedMap = this.occupiedMap;
		if (decoOccupiedMap == null)
		{
			return;
		}
		decoOccupiedMap.SaveAsTexture(path, true, flatAreas);
	}

	public void SaveStateDebugTexture(string _filename)
	{
		Color32[] array = new Color32[this.worldWidth * this.worldHeight];
		int num = -this.worldWidth / 2;
		int num2 = -this.worldHeight / 2;
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Color.black;
			int num3 = i % this.worldWidth + num;
			int num4 = i / this.worldWidth + num2;
			int num5 = DecoChunk.MakeKey16(DecoChunk.ToDecoChunkPos(num3), DecoChunk.ToDecoChunkPos(num4));
			DecoChunk decoChunk;
			if (this.decoChunks.TryGetValue(num5, out decoChunk))
			{
				long num6 = WorldChunkCache.MakeChunkKey(World.toChunkXZ(num3), World.toChunkXZ(num4));
				List<DecoObject> list;
				if (decoChunk.decosPerSmallChunks.TryGetValue(num6, out list))
				{
					foreach (DecoObject decoObject in list)
					{
						if (decoObject.pos.x == num3 && decoObject.pos.z == num4)
						{
							switch (decoObject.state)
							{
							case DecoState.GeneratedActive:
								array[i] = Color.green;
								goto IL_0154;
							case DecoState.GeneratedInactive:
								array[i] = Color.yellow;
								goto IL_0154;
							case DecoState.Dynamic:
								array[i] = Color.blue;
								goto IL_0154;
							default:
								goto IL_0154;
							}
						}
					}
				}
			}
			IL_0154:;
		}
		Texture2D texture2D = new Texture2D(this.worldWidth, this.worldHeight);
		texture2D.SetPixels32(array);
		texture2D.Apply();
		TextureUtils.SaveTexture(texture2D, _filename);
		Object.Destroy(texture2D);
	}

	public void SendDecosToClient(ClientInfo _cInfo)
	{
		List<DecoObject> list = this.decoWriteList;
		lock (list)
		{
			this.GenerateDecoWriteList();
			int i = 0;
			while (i < this.decoWriteList.Count)
			{
				_cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageDecoUpdate>().Setup(this.decoWriteList, ref i));
			}
		}
	}

	public void ResetDecosInWorldRect(Rect worldRect)
	{
		if (!ThreadManager.IsMainThread())
		{
			List<Rect> list = this.resetDecosInWorldRectFromThread;
			lock (list)
			{
				this.resetDecosInWorldRectFromThread.Add(worldRect);
				return;
			}
		}
		this.bDirty = true;
		int num = DecoChunk.ToDecoChunkPos(worldRect.x);
		int num2 = DecoChunk.ToDecoChunkPos(worldRect.y);
		int num3 = DecoChunk.ToDecoChunkPos(worldRect.xMax);
		int num4 = DecoChunk.ToDecoChunkPos(worldRect.yMax);
		Predicate<DecoObject> <>9__0;
		for (int i = num; i <= num3; i++)
		{
			for (int j = num2; j <= num4; j++)
			{
				int num5 = DecoChunk.MakeKey16(i, j);
				DecoChunk decoChunk;
				if (this.decoChunks.TryGetValue(num5, out decoChunk))
				{
					DecoChunk decoChunk2 = decoChunk;
					Predicate<DecoObject> predicate;
					if ((predicate = <>9__0) == null)
					{
						predicate = (<>9__0 = (DecoObject deco) => worldRect.Contains(new Vector2((float)deco.pos.x, (float)deco.pos.z)));
					}
					decoChunk2.RestoreGeneratedDecos(predicate);
				}
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageDecoResetWorldRect>().Setup(worldRect), false, -1, -1, -1, null, 192, false);
		}
	}

	public void ResetDecosForWorldChunk(long worldChunkKey)
	{
		if (!ThreadManager.IsMainThread())
		{
			List<long> list = this.resetDecosForWorldChunkFromThread;
			lock (list)
			{
				this.resetDecosForWorldChunkFromThread.Add(worldChunkKey);
				return;
			}
		}
		this.bDirty = true;
		int num = DecoChunk.ToDecoChunkPos(WorldChunkCache.extractX(worldChunkKey) * 16);
		int num2 = DecoChunk.ToDecoChunkPos(WorldChunkCache.extractZ(worldChunkKey) * 16);
		int num3 = DecoChunk.MakeKey16(num, num2);
		DecoChunk decoChunk;
		if (this.decoChunks.TryGetValue(num3, out decoChunk))
		{
			decoChunk.RestoreGeneratedDecos(worldChunkKey, (DecoObject decoObject) => true);
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageDecoResetWorldChunk>().Setup(worldChunkKey), false, -1, -1, -1, null, 192, false);
		}
	}

	public EnumDecoOccupied GetDecoOccupiedFromMap(int xWorld, int zWorld)
	{
		if (!PlatformOptimizations.FileBackedArrays)
		{
			return this.occupiedMap.Get(xWorld, zWorld);
		}
		int num = DecoManager.CheckPosition(this.worldWidth, this.worldHeight, xWorld, zWorld);
		if (num < 0)
		{
			return EnumDecoOccupied.NoneAllowed;
		}
		return this.fileBackedOccupiedMap.Get(num);
	}

	public void ResetAll()
	{
		Rect rect;
		rect..ctor((float)(-(float)this.worldWidth / 2), (float)(-(float)this.worldHeight / 2), (float)this.worldWidth, (float)this.worldHeight);
		this.ResetDecosInWorldRect(rect);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int FILEVERSION = 6;

	public const int cChunkSize = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cUpdateDelay = 1f;

	public const int cUpdateCoMaxTimeUs = 900;

	public static Vector3 cDecoMiddleOffset = new Vector3(0.5f, 0f, 0.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static DecoManager m_Instance;

	public bool IsEnabled = true;

	public bool IsHidden;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, DecoChunk> decoChunks = new Dictionary<int, DecoChunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DecoChunk> visibleDecoChunks = new List<DecoChunk>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<DecoObject> loadedDecos;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldWidthHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public int worldHeightHalf;

	[PublicizedFrom(EAccessModifier.Private)]
	public DecoOccupiedMap occupiedMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public FileBackedDecoOccupiedMap fileBackedOccupiedMap;

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunkProvider chunkProvider;

	[PublicizedFrom(EAccessModifier.Private)]
	public ITerrainGenerator terrainGenerator;

	[PublicizedFrom(EAccessModifier.Private)]
	public World world;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bFixedSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public string filenamePath;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PrefabInstance> overridePOIList;

	[PublicizedFrom(EAccessModifier.Private)]
	public PerlinNoise resourceNoise;

	[PublicizedFrom(EAccessModifier.Private)]
	public int checkDelayTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bDirty;

	[PublicizedFrom(EAccessModifier.Private)]
	public MemoryStream writeStream = new MemoryStream();

	[PublicizedFrom(EAccessModifier.Private)]
	public ThreadManager.TaskInfo writeTask;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DecoManager.SAddDecoInfo> addDecosFromThread = new List<DecoManager.SAddDecoInfo>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Vector3i> removeDecosFromThread = new List<Vector3i>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<Rect> resetDecosInWorldRectFromThread = new List<Rect>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<long> resetDecosForWorldChunkFromThread = new List<long>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<DecoObject> decoWriteList = new List<DecoObject>(4096);

	[PublicizedFrom(EAccessModifier.Private)]
	public MicroStopwatch msUpdate = new MicroStopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	public Coroutine updateCoroutine;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<EntityPlayer> playersToCheck = new List<EntityPlayer>();

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> chunksAroundPlayers = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public struct SAddDecoInfo
	{
		public World world;

		public BlockValue bv;

		public Vector3i pos;

		public bool bForceBlockYPos;
	}
}
