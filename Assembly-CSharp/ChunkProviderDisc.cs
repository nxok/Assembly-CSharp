using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class ChunkProviderDisc : ChunkProviderAbstract
{
	public ChunkProviderDisc(ChunkCluster _cc, string _worldName, PathAbstractions.AbstractedLocation _worldLocation)
	{
		this.cc = _cc;
		this.worldName = _worldName;
		this.worldLocation = _worldLocation;
	}

	public override IEnumerator Init(World _world)
	{
		this.world = _world;
		this.prefabDecorator = new DynamicPrefabDecorator(_world.m_PrefabCache);
		if (GameUtils.IsPlaytesting())
		{
			if (SdDirectory.Exists(GameIO.GetSaveGameRegionDir()))
			{
				SdDirectory.Delete(GameIO.GetSaveGameRegionDir(), true);
			}
			if (SdFile.Exists(GameIO.GetSaveGameDir() + "/decoration.7dt"))
			{
				SdFile.Delete(GameIO.GetSaveGameDir() + "/decoration.7dt");
			}
		}
		bool flag = false;
		if (_world.IsEditor() || !SdDirectory.Exists(GameIO.GetSaveGameRegionDir()))
		{
			flag = true;
		}
		string text = this.worldLocation.FullPath + "/Region";
		this.m_RegionFileManager = new RegionFileManager(text, (!_world.IsEditor()) ? GameIO.GetSaveGameRegionDir() : text, 0, false);
		this.cc.IsFixedSize = true;
		long[] allChunkKeys = this.m_RegionFileManager.GetAllChunkKeys();
		for (int i = 0; i < allChunkKeys.Length; i++)
		{
			Chunk chunkSync = this.m_RegionFileManager.GetChunkSync(allChunkKeys[i]);
			if (chunkSync != null)
			{
				chunkSync.FillBiomeId(3);
				chunkSync.NeedsRegeneration = false;
				chunkSync.NeedsLightCalculation = false;
				this.cc.AddChunkSync(chunkSync, false);
			}
		}
		if (flag)
		{
			yield return this.prefabDecorator.Load(this.worldLocation.FullPath, false);
			this.prefabDecorator.CopyAllPrefabsIntoWorld(this.world, false);
		}
		this.spawnPointManager = new SpawnPointManager(true);
		this.spawnPointManager.Load(this.worldLocation.FullPath);
		if (GameUtils.IsPlaytesting())
		{
			Prefab prefab = new Prefab();
			prefab.Load(GamePrefs.GetString(EnumGamePrefs.GameName), true, true, false, false);
			this.world.m_ChunkManager.RemoveAllChunks();
			int num = -1 * prefab.size.x / 2;
			int num2 = -1 * prefab.size.z / 2;
			int num3 = num + prefab.size.x;
			int num4 = num2 + prefab.size.z;
			this.cc.ChunkMinPos = new Vector2i((num - 1) / 16 - 1, (num2 - 1) / 16 - 1);
			this.cc.ChunkMinPos -= new Vector2i(2, 5);
			this.cc.ChunkMaxPos = new Vector2i(num3 / 16 + 1, num4 / 16 + 1);
			this.cc.ChunkMaxPos += new Vector2i(2, 2);
			for (int j = this.cc.ChunkMinPos.x; j <= this.cc.ChunkMaxPos.x; j++)
			{
				for (int k = this.cc.ChunkMinPos.y; k <= this.cc.ChunkMaxPos.y; k++)
				{
					Chunk chunk = MemoryPools.PoolChunks.AllocSync(true);
					chunk.X = j;
					chunk.Z = k;
					chunk.FillBiomeId(3);
					chunk.FillBlockRaw((int)WorldConstants.WaterLevel - 1, Block.GetBlockValue("terrainFiller", false));
					chunk.SetFullSunlight();
					chunk.NeedsLightCalculation = false;
					chunk.NeedsDecoration = false;
					chunk.NeedsRegeneration = false;
					this.cc.AddChunkSync(chunk, true);
				}
			}
			Vector3i vector3i = new Vector3i((float)(-(float)prefab.size.x / 2), WorldConstants.WaterLevel + (float)prefab.yOffset, (float)(-(float)prefab.size.z / 2));
			PrefabInstance prefabInstance = new PrefabInstance(1, prefab.location, vector3i, 0, prefab, 0);
			this.prefabDecorator.AddWorldPrefab(prefabInstance, true);
			this.prefabDecorator.CopyAllPrefabsIntoWorld(this.world, false);
			int num5 = -prefab.size.z / 2 - 11;
			this.world.SetBlock(new Vector3i(2, (int)(this.world.GetHeight(0, num5) + 1), num5), Block.GetBlockValue("cntQuestTestLoot", false), false, false);
			Vector3 vector;
			vector..ctor(-2f, (float)(this.world.GetHeight(0, num5) + 2), (float)num5);
			IChunk chunkFromWorldPos = this.world.GetChunkFromWorldPos(new Vector3i(vector));
			if (chunkFromWorldPos != null)
			{
				EntityCreationData entityCreationData = new EntityCreationData();
				entityCreationData.pos = vector;
				entityCreationData.rot = new Vector3(0f, 180f, 0f);
				entityCreationData.entityClass = EntityClass.FromString("npcTraderTest");
				((Chunk)chunkFromWorldPos).AddEntityStub(entityCreationData);
			}
			foreach (Chunk chunk2 in this.world.ChunkCache.GetChunkArrayCopySync())
			{
				chunk2.ResetStability();
				chunk2.ResetLights(0);
				chunk2.RefreshSunlight();
				chunk2.NeedsLightCalculation = true;
			}
		}
		if (this.worldName == "Empty")
		{
			this.spawnPointManager.spawnPointList = new SpawnPointList
			{
				new SpawnPoint(new Vector3i(0, 10, 0))
			};
		}
		yield return null;
		yield break;
	}

	public override DynamicPrefabDecorator GetDynamicPrefabDecorator()
	{
		return this.prefabDecorator;
	}

	public override SpawnPointList GetSpawnPointList()
	{
		return this.spawnPointManager.spawnPointList;
	}

	public override void SetSpawnPointList(SpawnPointList _spawnPointList)
	{
		this.spawnPointManager.spawnPointList = _spawnPointList;
	}

	public override bool GetOverviewMap(Vector2i _startPos, Vector2i _size, Color[] mapColors)
	{
		return false;
	}

	public override EnumChunkProviderId GetProviderId()
	{
		return EnumChunkProviderId.Disc;
	}

	public override void Cleanup()
	{
		Thread.Sleep(200);
		this.m_RegionFileManager.Cleanup();
	}

	public override void SaveAll()
	{
		if (this.world.IsEditor())
		{
			PathAbstractions.AbstractedLocation location = this.worldLocation;
			if (this.worldLocation.Type == PathAbstractions.EAbstractedLocationType.None)
			{
				SdDirectory.CreateDirectory(GameIO.GetGameDir("Data/Worlds") + "/" + this.worldName);
				location = PathAbstractions.WorldsSearchPaths.GetLocation(this.worldName, null, null);
			}
			this.prefabDecorator.Save(location.FullPath);
			this.prefabDecorator.CleanAllPrefabsFromWorld(this.world);
			this.m_RegionFileManager.MakePersistent(this.cc, true);
			this.m_RegionFileManager.WaitSaveDone();
			this.prefabDecorator.CopyAllPrefabsIntoWorld(this.world, false);
			this.spawnPointManager.Save(location.FullPath);
			return;
		}
		this.m_RegionFileManager.MakePersistent(this.world.ChunkCache, false);
		this.m_RegionFileManager.WaitSaveDone();
	}

	public override void SaveRandomChunks(int count, ulong _curWorldTimeInTicks, ArraySegment<long> _activeChunkSet)
	{
	}

	public override bool GetWorldExtent(out Vector3i _minSize, out Vector3i _maxSize)
	{
		_minSize = new Vector3i(this.world.ChunkCache.ChunkMinPos.x * 16, 0, this.world.ChunkCache.ChunkMinPos.y * 16);
		_maxSize = new Vector3i(this.world.ChunkCache.ChunkMaxPos.x * 16, 255, this.world.ChunkCache.ChunkMaxPos.y * 16);
		return true;
	}

	public override void RebuildTerrain(HashSetLong _chunks, Vector3i _areaStart, Vector3i _areaSize, bool _bStopStabilityUpdate, bool _bRegenerateChunk, bool _bFillEmptyBlocks, bool _isReset)
	{
		ThreadManager.RunCoroutineSync(this.prefabDecorator.Load(this.worldLocation.FullPath, false));
		this.prefabDecorator.CopyAllPrefabsIntoWorld(this.world, true);
		Chunk[] array = new Chunk[8];
		foreach (Chunk chunk in this.world.ChunkCache.GetChunkArrayCopySync())
		{
			chunk.ResetStability();
			chunk.ResetLights(0);
			chunk.RefreshSunlight();
			if (this.cc.GetNeighborChunks(chunk, array))
			{
				this.cc.LightChunk(chunk, array);
			}
			List<TileEntity> list = chunk.GetTileEntities().list;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Reset(FastTags<TagGroup.Global>.none);
			}
		}
		List<EntityPlayerLocal> localPlayers = this.world.GetLocalPlayers();
		foreach (long num in _chunks)
		{
			Chunk chunkSync = this.cc.GetChunkSync(num);
			if (chunkSync != null)
			{
				for (int j = 0; j < this.world.m_ChunkManager.m_ObservedEntities.Count; j++)
				{
					if (!this.world.m_ChunkManager.m_ObservedEntities[j].bBuildVisualMeshAround)
					{
						bool flag = false;
						int num2 = 0;
						while (!flag && num2 < localPlayers.Count)
						{
							if (this.world.m_ChunkManager.m_ObservedEntities[j].entityIdToSendChunksTo == localPlayers[num2].entityId)
							{
								flag = true;
							}
							num2++;
						}
						if (!flag && this.world.m_ChunkManager.m_ObservedEntities[j].chunksLoaded.Contains(num))
						{
							SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunkSync, true), false, this.world.m_ChunkManager.m_ObservedEntities[j].entityIdToSendChunksTo, -1, -1, null, 192, false);
						}
					}
				}
			}
		}
	}

	public override ChunkProtectionLevel GetChunkProtectionLevel(Vector3i worldPos)
	{
		return this.m_RegionFileManager.GetChunkProtectionLevelForWorldPos(worldPos);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public World world;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly PathAbstractions.AbstractedLocation worldLocation;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly string worldName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly string gameName;

	[PublicizedFrom(EAccessModifier.Protected)]
	public WorldState m_HeaderInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	public DynamicPrefabDecorator prefabDecorator;

	[PublicizedFrom(EAccessModifier.Protected)]
	public RegionFileManager m_RegionFileManager;

	[PublicizedFrom(EAccessModifier.Private)]
	public IChunkProviderIndicator chunkManager;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ChunkCluster cc;

	[PublicizedFrom(EAccessModifier.Protected)]
	public SpawnPointManager spawnPointManager;
}
