using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ConcurrentCollections;
using UniLinq;
using UnityEngine;
using UnityEngine.Rendering;

public class DynamicMeshManager : MonoBehaviour
{
	public static Transform ParentTransform
	{
		get
		{
			if (!(DynamicMeshManager.Instance == null))
			{
				return DynamicMeshManager.Instance.transform;
			}
			return null;
		}
	}

	public void ForceOrphanChecks()
	{
		base.StartCoroutine(this.DoubleOrphanCheck());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator DoubleOrphanCheck()
	{
		yield return base.StartCoroutine(this.CheckForOrphans(false));
		yield return base.StartCoroutine(this.CheckForOrphans(false));
		yield break;
	}

	public IEnumerator CheckForOrphans(bool debugLogOnly)
	{
		int returns = 0;
		DateTime dateTime = DateTime.Now.AddMilliseconds(2.0);
		this.checkForOrphans.Clear();
		for (int i = 0; i < DynamicMeshManager.ParentTransform.childCount; i++)
		{
			Transform child = DynamicMeshManager.ParentTransform.GetChild(i);
			this.checkForOrphans.Add(child.gameObject);
		}
		foreach (DynamicMeshItem dynamicMeshItem in this.ItemsDictionary.Values)
		{
			if (!(dynamicMeshItem.ChunkObject == null))
			{
				this.checkForOrphans.Remove(dynamicMeshItem.ChunkObject);
				this.potentialOrphans.Remove(dynamicMeshItem.ChunkObject);
				if (dateTime < DateTime.Now)
				{
					int num = returns;
					returns = num + 1;
					yield return null;
					dateTime = DateTime.Now.AddMilliseconds(2.0);
				}
			}
		}
		IEnumerator<DynamicMeshItem> enumerator = null;
		foreach (DynamicMeshRegion dynamicMeshRegion in DynamicMeshRegion.Regions.Values)
		{
			if (!(dynamicMeshRegion.RegionObject == null))
			{
				this.checkForOrphans.Remove(dynamicMeshRegion.RegionObject);
				this.potentialOrphans.Remove(dynamicMeshRegion.RegionObject);
				if (dateTime < DateTime.Now)
				{
					int num = returns;
					returns = num + 1;
					yield return null;
					dateTime = DateTime.Now.AddMilliseconds(2.0);
				}
			}
		}
		IEnumerator<DynamicMeshRegion> enumerator2 = null;
		using (HashSet<GameObject>.Enumerator enumerator3 = this.checkForOrphans.GetEnumerator())
		{
			while (enumerator3.MoveNext())
			{
				GameObject gameObject = enumerator3.Current;
				if (this.potentialOrphans.Contains(gameObject))
				{
					Log.Warning("Found orphaned mesh in dymesh parent " + gameObject.name);
					if (!debugLogOnly)
					{
						Object.Destroy(gameObject);
					}
					this.potentialOrphans.Remove(gameObject);
				}
				else
				{
					this.potentialOrphans.Add(gameObject);
				}
			}
			yield break;
		}
		yield break;
		yield break;
	}

	public static void EnabledChanged(bool newvalue)
	{
		if (DynamicMeshManager.CONTENT_ENABLED == newvalue)
		{
			return;
		}
		if (GameManager.Instance == null)
		{
			return;
		}
		if (GameManager.Instance.World == null)
		{
			return;
		}
		DynamicMeshManager.OnWorldUnload();
		if (newvalue)
		{
			DynamicMeshManager.Init();
		}
	}

	public void AddObjectForDestruction(GameObject go)
	{
		this.ToBeDestroyed.Enqueue(go);
	}

	public static void AddRegionLoadMeshes(long key)
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		using (LinkedList<long>.Enumerator enumerator = DynamicMeshManager.Instance.RegionsAvailableToLoad.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current == key)
				{
					return;
				}
			}
		}
		DynamicMeshManager.Instance.RegionsAvailableToLoad.AddLast(key);
	}

	public void AddChunkLoadData(DynamicMeshVoxelLoad loadData)
	{
		object chunkMeshDataLock = this._chunkMeshDataLock;
		lock (chunkMeshDataLock)
		{
			DynamicMeshManager.Instance.ChunkMeshData.AddLast(loadData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static DynamicMeshManager()
	{
		GamePrefs.OnGamePrefChanged += DynamicMeshManager.OnGamePrefChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref == EnumGamePrefs.DebugMenuEnabled)
		{
			DynamicMeshManager.EnabledChanged(GamePrefs.GetBool(_pref));
		}
	}

	public static bool IsOutsideDistantTerrain(DynamicMeshRegion region)
	{
		return DynamicMeshManager.CONTENT_ENABLED && !(DynamicMeshManager.Instance == null) && (region.Rect.xMin < (float)DynamicMeshManager.dtMinX || region.Rect.xMax > (float)DynamicMeshManager.dtMaxX || region.Rect.yMin < (float)DynamicMeshManager.dtMinZ || region.Rect.yMax > (float)DynamicMeshManager.dtMaxZ);
	}

	public static bool IsOutsideDistantTerrain(float minx, float maxx, float minz, float maxz)
	{
		return DynamicMeshManager.CONTENT_ENABLED && !(DynamicMeshManager.Instance == null) && (minx < (float)DynamicMeshManager.dtMinX || maxx > (float)DynamicMeshManager.dtMaxX || minz < (float)DynamicMeshManager.dtMinZ || maxz > (float)DynamicMeshManager.dtMaxZ);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public static void UpdateDistantTerrainBounds(TileArea<UnityDistantTerrain.TerrainAndWater> data, UnityDistantTerrain.Config terrainConfig)
	{
		if (DynamicMeshManager.Instance == null)
		{
			return;
		}
		int num = int.MaxValue;
		int num2 = int.MaxValue;
		int num3 = int.MinValue;
		int num4 = int.MinValue;
		if (data.Data.Count == 0)
		{
			DynamicMeshManager.dtMinX = int.MinValue;
			DynamicMeshManager.dtMinZ = int.MinValue;
			DynamicMeshManager.dtMaxX = -2147482624;
			DynamicMeshManager.dtMaxZ = -2147482624;
			return;
		}
		foreach (uint num5 in data.Data.Keys)
		{
			int tileXPos = TileAreaUtils.GetTileXPos(num5);
			int tileZPos = TileAreaUtils.GetTileZPos(num5);
			num = Math.Min(num, tileXPos);
			num2 = Math.Min(num2, tileZPos);
			num3 = Math.Max(num3, tileXPos);
			num4 = Math.Max(num4, tileZPos);
		}
		int dataTileSize = terrainConfig.DataTileSize;
		num *= dataTileSize;
		num2 *= dataTileSize;
		num3 *= dataTileSize;
		num4 *= dataTileSize;
		num3 += dataTileSize;
		num4 += dataTileSize;
		if (num != DynamicMeshManager.dtMinX || num3 != DynamicMeshManager.dtMaxX || DynamicMeshManager.dtMinZ != num2 || DynamicMeshManager.dtMaxZ != num4)
		{
			DynamicMeshManager.dtMinX = num;
			DynamicMeshManager.dtMaxX = num3;
			DynamicMeshManager.dtMinZ = num2;
			DynamicMeshManager.dtMaxZ = num4;
			DynamicMeshManager.Instance.ShowOrHidePrefabs();
			GameManager.Instance.prefabLODManager.TriggerUpdate();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyObjects()
	{
		GameObject gameObject;
		while (this.ToBeDestroyed.TryDequeue(out gameObject))
		{
			if (!(gameObject == null))
			{
				DynamicMeshContainer currentlyLoadingItem = DynamicMeshFile.CurrentlyLoadingItem;
				if (((currentlyLoadingItem != null) ? currentlyLoadingItem.GetGameObject() : null) == gameObject)
				{
					Log.Warning("Object in use when destroying... delaying");
					this.ToBeDestroyed.Enqueue(gameObject);
					return;
				}
				gameObject.SetActive(false);
				DynamicMeshManager.MeshDestroy(gameObject);
			}
		}
	}

	public static void WarmUp()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator SendClientMessage()
	{
		while (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsConnected)
		{
			yield return new WaitForSeconds(1f);
		}
		this.ClientMessage = DynamicMeshServerStatus.ClientMessageSent;
		NetPackageDynamicClientArrive package = NetPackageManager.GetPackage<NetPackageDynamicClientArrive>();
		package.BuildData();
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(package, false);
		DynamicMeshManager.LogMsg("Sending client arrive message. Items: " + package.Items.Count.ToString());
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ProcessChunkRegionRequests()
	{
		this.ProcessRegionReady = false;
		this.ForceNextRegion = DateTime.Now.AddSeconds(60.0);
		DyMeshRegionLoadRequest load = null;
		if (this.RegionFileLoadRequests.Count != 0 && this.RegionFileLoadRequests.TryDequeue(out load))
		{
			if (this.PrefabCheck == PrefabCheckState.Waiting)
			{
				this.PrefabCheck = PrefabCheckState.Ready;
			}
			DynamicMeshRegion region = this.GetRegion(load.Key);
			if (DynamicMeshManager.DoLog)
			{
				Log.Out("Loading region go-process " + region.ToDebugLocation());
			}
			if (region != null)
			{
				if (region.OutsideLoadArea)
				{
					region.SetState(DynamicRegionState.Unloaded, false);
				}
				else
				{
					yield return base.StartCoroutine(load.CreateMeshCoroutine(region));
					if (region.RegionObject != null)
					{
						region.SetState(DynamicRegionState.Loaded, false);
						bool flag = !region.InBuffer && region.LoadedItems.Count > 0 && region.UnloadedItems.Count == 0 && region.RegionObject != null && region.RegionObject.activeSelf;
						region.LoadItems(false, !flag, false, "regionLoadRequest");
						this.UpdateDynamicPrefabDecoratorRegions(region);
					}
					region = null;
				}
			}
		}
		if (load != null)
		{
			MeshLists.ReturnList(load.OpaqueMesh);
			MeshLists.ReturnList(load.TerrainMesh);
		}
		this.AvailableRegionLoadRequests = Math.Min(DynamicMeshSettings.MaxRegionMeshData, this.AvailableRegionLoadRequests + 1);
		this.ProcessRegionReady = true;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator ProcessItemMeshGeneration()
	{
		this.ProcessItemLoadReady = false;
		this.ForceNextItemLoad = DateTime.Now.AddSeconds(30.0);
		DynamicMeshVoxelLoad voxelData = null;
		if (this.ChunkMeshData.Count != 0)
		{
			bool flag = false;
			Monitor.TryEnter(this._chunkMeshDataLock, 1, ref flag);
			if (flag)
			{
				float num = 9999999f;
				foreach (DynamicMeshVoxelLoad dynamicMeshVoxelLoad in this.ChunkMeshData)
				{
					float num2 = dynamicMeshVoxelLoad.Item.DistanceToPlayer();
					if (num2 < num)
					{
						voxelData = dynamicMeshVoxelLoad;
						num = num2;
						if (num2 < 50f)
						{
							break;
						}
					}
				}
				if (voxelData != null)
				{
					this.ChunkMeshData.Remove(voxelData);
				}
				Monitor.Exit(this._chunkMeshDataLock);
				if (voxelData != null)
				{
					DynamicMeshItem item = this.GetItemOrNull(voxelData.Item.Key);
					if (item != null)
					{
						DynamicMeshRegion region = this.GetRegion(item);
						region.RemoveFromLoadingQueue(item);
						if (!region.IsInItemLoad())
						{
							item.State = DynamicItemState.Waiting;
						}
						else
						{
							item.State = DynamicItemState.Loading;
							if (DynamicMeshManager.DoLog)
							{
								Log.Out("Item " + item.ToDebugLocation() + " loading request");
							}
							DynamicMeshManager.ItemLoadDistance = item.DistanceToPlayer();
							yield return base.StartCoroutine(item.CreateMeshFromVoxelCoroutine(false, DynamicMeshManager.MeshLoadStop, voxelData));
							region.AddChunk(item.WorldPosition.x, item.WorldPosition.z);
							if (region.WorldPosition.x != DynamicMeshUnity.RoundRegion(item.WorldPosition.x))
							{
								string[] array = new string[6];
								array[0] = "Region mismatch: ";
								int num3 = 1;
								Vector3i vector3i = region.WorldPosition;
								array[num3] = vector3i.ToString();
								array[2] = " vs ";
								array[3] = (item.WorldPosition.x / 16 * 16).ToString();
								array[4] = " for  ";
								int num4 = 5;
								vector3i = item.WorldPosition;
								array[num4] = vector3i.ToString();
								Log.Error(string.Concat(array));
							}
							if (item.State != DynamicItemState.ReadyToDelete)
							{
								region.AddItemToLoadedList(item);
								bool flag2 = region.IsVisible();
								item.SetVisible(!flag2 && !item.IsChunkInView, "loadItemRequest");
								item.State = DynamicItemState.Loaded;
							}
						}
					}
				}
			}
		}
		this.ProcessItemLoadReady = true;
		if (voxelData != null)
		{
			voxelData.DisposeMeshes();
		}
		yield break;
	}

	public void ForceLoadDataAroundPosition(Vector3i pos, int regionRadius)
	{
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			return;
		}
		DateTime now = DateTime.Now;
		DynamicMeshManager.CONTENT_ENABLED = false;
		foreach (DynamicMeshRegion dynamicMeshRegion in DynamicMeshRegion.Regions.Values)
		{
			if (dynamicMeshRegion != null)
			{
				dynamicMeshRegion.DistanceChecks();
			}
		}
		DynamicMeshManager.CONTENT_ENABLED = true;
		DyMeshRegionLoadRequest dyMeshRegionLoadRequest = DyMeshRegionLoadRequest.Create(0L);
		foreach (long num in this.RegionsAvailableToLoad)
		{
			dyMeshRegionLoadRequest.Key = num;
			DynamicMeshRegion region = this.GetRegion(dyMeshRegionLoadRequest.Key);
			if (region != null)
			{
				if (region.OutsideLoadArea || region.DistanceToPlayer() > 1000f)
				{
					region.SetState(DynamicRegionState.Unloaded, false);
				}
				else
				{
					dyMeshRegionLoadRequest.OpaqueMesh.Reset();
					dyMeshRegionLoadRequest.TerrainMesh.Reset();
					DynamicMeshThread.RegionStorage.LoadRegion(dyMeshRegionLoadRequest);
					dyMeshRegionLoadRequest.CreateMeshSync(region);
					if (region.RegionObject != null)
					{
						region.SetState(DynamicRegionState.Loaded, false);
						bool flag = !region.InBuffer && region.LoadedItems.Count > 0 && region.UnloadedItems.Count == 0 && region.RegionObject != null && region.RegionObject.activeSelf;
						region.LoadItems(false, !flag, false, "regionLoadRequest");
						this.UpdateDynamicPrefabDecoratorRegions(region);
					}
				}
			}
		}
		MeshLists.ReturnList(dyMeshRegionLoadRequest.OpaqueMesh);
		MeshLists.ReturnList(dyMeshRegionLoadRequest.TerrainMesh);
		this.RegionsAvailableToLoad.Clear();
		foreach (ChunkGameObject chunkGameObject in GameManager.Instance.World.ChunkCache.DisplayedChunkGameObjects.Dict.Values)
		{
			if (!DynamicMeshManager.ChunkGameObjects.Contains(chunkGameObject.chunk.Key))
			{
				DynamicMeshThread.AddChunkGameObject(chunkGameObject.chunk);
			}
		}
		Log.Out("Force load took " + ((int)(DateTime.Now - now).TotalSeconds).ToString() + " seconds");
	}

	public void ClearPrefabs()
	{
		if (base.gameObject != null)
		{
			foreach (object obj in base.gameObject.transform)
			{
				Object.Destroy(((Transform)obj).gameObject);
			}
		}
		if (this.ItemsDictionary != null)
		{
			this.ItemsDictionary.Clear();
			DynamicMeshRegion.Regions.Clear();
		}
	}

	public void Cleanup()
	{
		DynamicMeshThread.StopThreadRequest();
		this.ClearPrefabs();
		base.StopAllCoroutines();
		Vector2i vector2i;
		while (this.ChunksToRemove.TryDequeue(out vector2i))
		{
		}
		foreach (DynamicMeshRegion dynamicMeshRegion in DynamicMeshRegion.Regions.Values)
		{
			dynamicMeshRegion.CleanUp();
		}
		this.ItemsDictionary.Clear();
		this.UpdateData.Clear();
		this.ChunkMeshData.Clear();
		foreach (object obj in base.transform)
		{
			Transform transform = (Transform)obj;
			this.ToBeDestroyed.Enqueue(transform.gameObject);
		}
		this.DestroyObjects();
		this.disabledImposterChunkManager.Dispose();
	}

	public bool HalEventChunkChanged(object chunkObject)
	{
		if (chunkObject == null || !(chunkObject is Chunk))
		{
			return false;
		}
		DynamicMeshManager.ChunkChanged(((Chunk)chunkObject).GetWorldPos(), -1, 1);
		return true;
	}

	public static bool IsValidGameMode()
	{
		GameManager instance = GameManager.Instance;
		if (((instance != null) ? instance.World : null) == null)
		{
			Log.Out("GM or World is not initialised yet for dynamic mesh");
			return false;
		}
		EnumGameMode gameMode = (EnumGameMode)GameManager.Instance.World.GetGameMode();
		return !GameManager.Instance.IsEditMode() && !GameUtils.IsPlaytesting() && gameMode != EnumGameMode.Creative && gameMode != EnumGameMode.EditWorld;
	}

	public void Awake()
	{
		DynamicMeshManager.LogMsg("Awake");
		DynamicMeshManager.CONTENT_ENABLED = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			Log.Out("Dynamic mesh disabled");
			return;
		}
		if (DynamicMeshManager.backgroundTexture == null)
		{
			DynamicMeshManager.backgroundTexture = Texture2D.blackTexture;
		}
		DynamicMeshFile.TerrainSharedMaterials.Clear();
		if (DynamicMeshManager.Instance != null)
		{
			DynamicMeshManager.Instance.StopAllCoroutines();
			this.ClearPrefabs();
			if (DynamicMeshManager.Instance != this)
			{
				Object.Destroy(DynamicMeshManager.Instance);
			}
			DynamicMeshManager.Instance = null;
		}
		if (!DynamicMeshManager.IsValidGameMode())
		{
			Log.Out("Dynamic Mesh will not run in this game mode");
			return;
		}
		DynamicMeshManager.Instance = this;
		DynamicMeshFile.MeshLocation = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? (GameIO.GetSaveGameDir() + "/DynamicMeshes/") : (GameIO.GetSaveGameLocalDir() + "/DynamicMeshes/"));
		DynamicMeshManager.LogMsg("Mesh location: " + DynamicMeshFile.MeshLocation);
		this.BufferRegionLoadRequests.Clear();
		this.AvailableRegionLoadRequests = DynamicMeshSettings.MaxRegionMeshData;
		DyMeshData.ActiveItems = 0;
		this.ChunkMeshData.Clear();
		this.ChunkMeshLoadRequests.Clear();
		ConnectionManager.OnClientDisconnected -= DynamicMeshServer.OnClientDisconnect;
		ConnectionManager.OnClientDisconnected += DynamicMeshServer.OnClientDisconnect;
		this.UpdateData.Clear();
		this.ItemsDictionary = new ConcurrentDictionary<long, DynamicMeshItem>();
		DynamicMeshThread.StopThreadForce();
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			DynamicMeshManager.LogMsg("Disabled");
			return;
		}
		DateTime now = DateTime.Now;
		this.LoadItemsDedicated();
		DynamicMeshManager.LogMsg("Loading all items took: " + (DateTime.Now - now).TotalSeconds.ToString() + " seconds.");
		this.disabledImposterChunkManager = new DynamicMeshManager.DisabledImposterChunkManager(this);
		if (!GameManager.IsDedicatedServer)
		{
			if (DynamicMeshManager.player != null)
			{
				DynamicMeshThread.PlayerPositionX = DynamicMeshManager.player.position.x;
				DynamicMeshThread.PlayerPositionZ = DynamicMeshManager.player.position.z;
				this.ForceLoadDataAroundPosition(new Vector3i(DynamicMeshManager.player.position), DynamicMeshSettings.MaxViewDistance / 2);
			}
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg("Saying hello to server. Regions: " + DynamicMeshRegion.Regions.Values.Count.ToString());
				}
				this.ShowOrHidePrefabs();
			}
		}
		DynamicMeshThread.StartThread();
		this.PrefabCheck = (SdFile.Exists(DynamicMeshFile.MeshLocation + "!!ChunksChecked.info") ? PrefabCheckState.Run : PrefabCheckState.Waiting);
		if (this.PrefabCheck != PrefabCheckState.Run)
		{
			this.CheckPrefabs("thread Started", false);
		}
		MeshDescription.meshes[0].bTextureArray = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ShowErrorStackTraces(string _msg, string _trace, LogType _type)
	{
		if (_type == null || _type == 4)
		{
			Log.Out(string.Concat(new string[]
			{
				"Callback ",
				_type.ToString(),
				":",
				_msg,
				" | ",
				_trace
			}));
		}
	}

	public static void OnWorldUnload()
	{
		DynamicMeshThread.StopThreadRequest();
		DateTime dateTime = DateTime.Now.AddSeconds(1.0);
		while (DynamicMeshThread.RequestThreadStop && DateTime.Now < dateTime)
		{
			Thread.Sleep(100);
		}
		DynamicMeshThread.CleanUp();
		DynamicMeshManager.ChunkGameObjects.Clear();
		if (DynamicMeshManager.Instance != null)
		{
			DynamicMeshManager.Instance.Cleanup();
		}
		DynamicMeshFile.CleanUp();
		DynamicMeshFile.MeshLocation = null;
	}

	public static bool IsServer
	{
		get
		{
			return SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer;
		}
	}

	public void CheckPrefabsInRegion(int x, int z)
	{
		int xMin = DynamicMeshUnity.RoundRegion(x);
		int zMin = DynamicMeshUnity.RoundRegion(z);
		int xMax = xMin + 160;
		int zMax = zMin + 160;
		DynamicMeshManager.LogMsg("Checking prefabs in region");
		this.PrefabCheck = PrefabCheckState.Warming;
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		EntityPlayerLocal player = DynamicMeshManager.player;
		if (dynamicPrefabDecorator == null)
		{
			return;
		}
		if (player == null)
		{
			return;
		}
		Vector3 playerPos = new Vector3((float)x, 0f, (float)z);
		this.dynamicPrefabsBuffer.Clear();
		dynamicPrefabDecorator.GetAllPrefabs(this.dynamicPrefabsBuffer);
		List<PrefabInstance> list = (from d in this.dynamicPrefabsBuffer
			where d.boundingBoxPosition.x >= xMin && d.boundingBoxPosition.x + d.boundingBoxSize.x < xMax && d.boundingBoxPosition.z >= zMin && d.boundingBoxPosition.z + d.boundingBoxSize.z < zMax
			orderby Math.Abs(Vector3.Distance(playerPos, d.boundingBoxPosition.ToVector3()))
			select d).ToList<PrefabInstance>();
		DynamicMeshManager.LogMsg("Found prefabs: " + list.Count.ToString() + "   Already loaded: " + this.ItemsDictionary.Count.ToString());
		Dictionary<Vector3i, List<Vector3i>> dictionary = new Dictionary<Vector3i, List<Vector3i>>();
		foreach (PrefabInstance prefabInstance in list)
		{
			DynamicMeshManager.Instance.CheckPrefab(prefabInstance, dictionary);
		}
		List<Vector3i> list2 = dictionary.Keys.OrderBy((Vector3i d) => DynamicMeshUnity.Distance(d, playerPos)).ToList<Vector3i>();
		DynamicMeshThread.RegionsToCheck = new List<List<Vector3i>>();
		int num = 0;
		foreach (Vector3i vector3i in list2)
		{
			using (List<Vector3i>.Enumerator enumerator3 = dictionary[vector3i].GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					Vector3i p = enumerator3.Current;
					DynamicMeshRegion regionFromWorldPosition = DynamicMeshRegion.GetRegionFromWorldPosition(p.x, p.z);
					if (regionFromWorldPosition == null || !regionFromWorldPosition.LoadedChunks.Any((Vector3i d) => d.x == p.x && d.z == p.z))
					{
						num++;
						this.AddChunk(p, false);
					}
				}
			}
		}
		DynamicMeshManager.LogMsg("Keys: " + list2.Count.ToString() + " loaded: " + num.ToString());
		this.PrefabCheck = PrefabCheckState.WaitingForCompleteCheck;
	}

	public void CheckPrefabs(string source, bool forceRegen = false)
	{
		if (!DynamicMeshManager.IsServer)
		{
			return;
		}
		if (!forceRegen && (DynamicMeshSettings.OnlyPlayerAreas || !DynamicMeshSettings.NewWorldFullRegen))
		{
			return;
		}
		DynamicMeshManager.LogMsg("Checking prefabs " + source);
		if (GameManager.Instance.World.ChunkCache == null)
		{
			DynamicMeshManager.LogMsg("ChunkCache null");
			this.PrefabCheck = PrefabCheckState.WaitingForCompleteCheck;
			return;
		}
		this.PrefabCheck = PrefabCheckState.Warming;
		DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
		EntityPlayerLocal player = DynamicMeshManager.player;
		if (dynamicPrefabDecorator == null)
		{
			DynamicMeshManager.LogMsg("No deco found");
			return;
		}
		Vector3 playerPos = (DynamicMeshManager.IsServer ? Vector3.zero : DynamicMeshManager.player.GetPosition());
		this.dynamicPrefabsBuffer.Clear();
		dynamicPrefabDecorator.GetAllPrefabs(this.dynamicPrefabsBuffer);
		List<PrefabInstance> list = this.dynamicPrefabsBuffer.OrderBy((PrefabInstance d) => Math.Abs(Vector3.Distance(playerPos, d.boundingBoxPosition.ToVector3()))).ToList<PrefabInstance>();
		DynamicMeshManager.LogMsg("Prefabs: " + list.Count.ToString());
		DynamicMeshManager.LogMsg("Found prefabs: " + list.Count.ToString() + "   Already loaded: " + this.ItemsDictionary.Count.ToString());
		Dictionary<Vector3i, List<Vector3i>> dictionary = new Dictionary<Vector3i, List<Vector3i>>();
		foreach (PrefabInstance prefabInstance in list)
		{
			DynamicMeshManager.Instance.CheckPrefab(prefabInstance, dictionary);
		}
		List<Vector3i> list2 = dictionary.Keys.OrderBy((Vector3i d) => DynamicMeshUnity.Distance(d, playerPos)).ToList<Vector3i>();
		DynamicMeshThread.RegionsToCheck = new List<List<Vector3i>>();
		int num = 0;
		foreach (Vector3i vector3i in list2)
		{
			using (List<Vector3i>.Enumerator enumerator3 = dictionary[vector3i].GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					Vector3i p = enumerator3.Current;
					DynamicMeshRegion regionFromWorldPosition = DynamicMeshRegion.GetRegionFromWorldPosition(p.x, p.z);
					if (regionFromWorldPosition == null || !regionFromWorldPosition.LoadedChunks.Any((Vector3i d) => d.x == p.x && d.z == p.z))
					{
						num++;
						this.AddChunk(p, false);
					}
				}
			}
		}
		DynamicMeshManager.LogMsg("Keys: " + list2.Count.ToString() + " loaded: " + num.ToString());
		this.PrefabCheck = PrefabCheckState.WaitingForCompleteCheck;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckPrefab(PrefabInstance p, Dictionary<Vector3i, List<Vector3i>> positions)
	{
		Vector3i boundingBoxPosition = p.boundingBoxPosition;
		Vector3i boundingBoxSize = p.boundingBoxSize;
		int itemPosition = DynamicMeshUnity.GetItemPosition(boundingBoxPosition.x);
		int num = DynamicMeshUnity.GetItemPosition(boundingBoxPosition.x + boundingBoxSize.x) + 16;
		int itemPosition2 = DynamicMeshUnity.GetItemPosition(boundingBoxPosition.z);
		int num2 = DynamicMeshUnity.GetItemPosition(boundingBoxPosition.z + boundingBoxSize.z) + 16;
		for (int i = itemPosition; i <= num; i += 16)
		{
			for (int j = itemPosition2; j <= num2; j += 16)
			{
				long itemKey = DynamicMeshUnity.GetItemKey(i, j);
				DynamicMeshItem dynamicMeshItem;
				this.ItemsDictionary.TryGetValue(itemKey, out dynamicMeshItem);
				if (dynamicMeshItem == null || !dynamicMeshItem.FileExists())
				{
					Vector3i regionPositionFromWorldPosition = DynamicMeshUnity.GetRegionPositionFromWorldPosition(i, j);
					if (!positions.ContainsKey(regionPositionFromWorldPosition))
					{
						positions.Add(regionPositionFromWorldPosition, new List<Vector3i>());
					}
					List<Vector3i> list = positions[regionPositionFromWorldPosition];
					bool flag = false;
					for (int k = 0; k < list.Count; k++)
					{
						if (list[k].x == i && list[k].z == j)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						Vector3i vector3i = new Vector3i(i, 0, j);
						if (DynamicMeshManager.DoLog)
						{
							DynamicMeshManager.LogMsg("Adding chunk " + i.ToString() + "," + j.ToString());
						}
						list.Add(vector3i);
					}
				}
			}
		}
	}

	public void RefreshAll()
	{
		foreach (DynamicMeshItem dynamicMeshItem in this.ItemsDictionary.Values)
		{
			DynamicMeshThread.ToGenerate.Enqueue(dynamicMeshItem);
		}
	}

	public void AddChunkStub(Vector3i worldPos, DynamicMeshRegion region)
	{
		long num = WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z));
		this.AddChunk(num, false, false, region);
	}

	public static void AddDataFromServer(int x, int z)
	{
		if (DynamicMeshManager.Instance == null)
		{
			return;
		}
		Vector3i vector3i = new Vector3i(x, 0, z);
		DynamicMeshRegion region = DynamicMeshManager.Instance.GetRegion(x, z);
		bool flag = region.IsInItemLoad();
		DynamicMeshManager.Instance.AddChunkStub(vector3i, region);
		DynamicMeshItem itemFromWorldPosition = DynamicMeshManager.Instance.GetItemFromWorldPosition(x, z);
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg(string.Concat(new string[]
			{
				"Add data from server ",
				x.ToString(),
				",",
				z.ToString(),
				" itemLoad: ",
				flag.ToString(),
				"  state: ",
				itemFromWorldPosition.State.ToString()
			}));
		}
		itemFromWorldPosition.State = DynamicItemState.UpdateRequired;
		if (flag)
		{
			itemFromWorldPosition.Load("data from server", true, region.InBuffer);
		}
	}

	public DynamicMeshItem AddChunk(Vector3i worldPos, bool primary)
	{
		long num = WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z));
		return this.AddChunk(num, true, primary, null);
	}

	public DynamicMeshItem AddChunk(long key, bool addToThread, bool primary, DynamicMeshRegion region)
	{
		Vector3i vector3i = new Vector3i(WorldChunkCache.extractX(key) * 16, 0, WorldChunkCache.extractZ(key) * 16);
		DynamicMeshThread.AddRegionChunk(vector3i.x, vector3i.z, key);
		DynamicMeshItem dynamicMeshItem;
		if (!this.ItemsDictionary.TryGetValue(key, out dynamicMeshItem))
		{
			dynamicMeshItem = new DynamicMeshItem(vector3i);
			if (region == null)
			{
				region = this.GetRegion(dynamicMeshItem);
			}
			if (region == null)
			{
				return dynamicMeshItem;
			}
			region.AddItem(dynamicMeshItem);
			this.disabledImposterChunksDirty |= this.ItemsDictionary.TryAdd(key, dynamicMeshItem);
		}
		DynamicMeshUnity.AddDisabledImposterChunk(dynamicMeshItem.Key);
		if (addToThread)
		{
			if (primary)
			{
				DynamicMeshThread.RequestPrimaryQueue(dynamicMeshItem);
			}
			else
			{
				DynamicMeshThread.RequestSecondaryQueue(dynamicMeshItem);
			}
		}
		return dynamicMeshItem;
	}

	public static void LogMsg(string msg)
	{
		Log.Out("Dymesh: {0}", new object[] { msg });
	}

	public static void MeshDestroy(GameObject go)
	{
		MeshFilter component = go.GetComponent<MeshFilter>();
		if (component != null)
		{
			Object.Destroy(component.sharedMesh);
			component.sharedMesh = null;
		}
		MeshRenderer component2 = go.GetComponent<MeshRenderer>();
		if (component2 != null)
		{
			component2.sharedMaterial = null;
			Object.Destroy(component2);
		}
		foreach (object obj in go.transform)
		{
			DynamicMeshManager.MeshDestroy(((Transform)obj).gameObject);
		}
		Object.Destroy(go);
	}

	public void UpdateDynamicPrefabDecoratorRegions(DynamicMeshRegion region)
	{
		this.UpdateDynamicPrefabDecoratorRegion(region);
		DynamicMeshRegion dynamicMeshRegion = this.GetRegion(region.WorldPosition + new Vector3i(160, 0, -160));
		this.UpdateDynamicPrefabDecoratorRegion(dynamicMeshRegion);
		dynamicMeshRegion = this.GetRegion(region.WorldPosition + new Vector3i(-160, 0, 160));
		this.UpdateDynamicPrefabDecoratorRegion(dynamicMeshRegion);
		dynamicMeshRegion = this.GetRegion(region.WorldPosition + new Vector3i(160, 0, 160));
		this.UpdateDynamicPrefabDecoratorRegion(dynamicMeshRegion);
		dynamicMeshRegion = this.GetRegion(region.WorldPosition + new Vector3i(-160, 0, -160));
		this.UpdateDynamicPrefabDecoratorRegion(dynamicMeshRegion);
	}

	public void UpdateDynamicPrefabDecoratorRegion(DynamicMeshRegion region)
	{
		if (DynamicMeshManager.DisableLOD)
		{
			return;
		}
		if (region == null)
		{
			return;
		}
		if (region.RegionObject == null)
		{
			return;
		}
		if (region.LoadedItems.Any((DynamicMeshItem d) => d.State != DynamicItemState.Loaded && d.State != DynamicItemState.Empty && d.State != DynamicItemState.ReadyToDelete))
		{
			return;
		}
		DateTime now = DateTime.Now;
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = null;
			if (GameManager.Instance == null)
			{
				DynamicMeshManager.LogMsg("GM Null");
			}
			else if (GameManager.Instance.World == null)
			{
				DynamicMeshManager.LogMsg("world Null");
			}
			else if (GameManager.Instance.World.ChunkCache == null)
			{
				DynamicMeshManager.LogMsg("ChunkCache Null");
			}
			else if (GameManager.Instance.World.ChunkCache.ChunkProvider == null)
			{
				DynamicMeshManager.LogMsg("Provider Null");
			}
			else
			{
				dynamicPrefabDecorator = GameManager.Instance.World.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
			}
			if (dynamicPrefabDecorator == null)
			{
				DynamicMeshManager.LogMsg("dec Null");
				return;
			}
			if (region.Instances == null)
			{
				region.Instances = new List<PrefabInstance>();
				this.dynamicPrefabsBuffer.Clear();
				dynamicPrefabDecorator.GetAllPrefabs(this.dynamicPrefabsBuffer);
				if (this.dynamicPrefabsBuffer.Count == 0)
				{
					DynamicMeshManager.LogMsg("prefabs Null or empty");
					return;
				}
				for (int i = this.dynamicPrefabsBuffer.Count - 1; i >= 0; i--)
				{
					PrefabInstance prefabInstance = this.dynamicPrefabsBuffer[i];
					if (region.ContainsPrefab(prefabInstance))
					{
						region.Instances.Add(prefabInstance);
					}
				}
			}
		}
	}

	public bool PointInRectAndRegionExists(int topLeftX, int topLeftY, int bottomRightX, int bottomRightY, int x, int y)
	{
		bool flag = x >= topLeftX && x <= bottomRightX && y >= bottomRightY && y <= topLeftY;
		if (flag)
		{
			DynamicMeshRegion regionFromWorldPosition = DynamicMeshRegion.GetRegionFromWorldPosition(x, y);
			if (regionFromWorldPosition == null || regionFromWorldPosition.RegionObject == null)
			{
				return false;
			}
		}
		return flag;
	}

	public bool IsRegionLoadedAndActive(int x, int y, bool doDebug)
	{
		if (DynamicMeshManager.Instance.PrefabCheck == PrefabCheckState.Run)
		{
			return true;
		}
		DynamicMeshRegion regionFromWorldPosition = DynamicMeshRegion.GetRegionFromWorldPosition(x, y);
		if (regionFromWorldPosition != null)
		{
			return regionFromWorldPosition.IsRegionLoadedAndActive(doDebug);
		}
		if (doDebug)
		{
			Log.Out("LoadedAndActive Failed: region is null");
		}
		return false;
	}

	public void ArrangeChunkRemoval(int x, int z)
	{
		this.ChunksToRemove.Enqueue(new Vector2i(x, z));
	}

	public void DisableLodGO(GameObject go)
	{
		if (go == null)
		{
			return;
		}
		Object.Destroy(go.GetComponent<MeshRenderer>());
		Object.Destroy(go.GetComponent<MeshFilter>());
		foreach (object obj in go.transform)
		{
			Object.Destroy(((Transform)obj).gameObject);
		}
	}

	public bool StartObserver(Vector3 pos, Vector3 next)
	{
		this.ObserverRequestInfo = ObserverRequest.Start;
		this.ObserverPos = pos;
		this.ObserverPosNext = next;
		return true;
	}

	public void StopObserver()
	{
		if (this.Observer != null && this.Observer.Observer != null && this.ObserverRequestInfo != ObserverRequest.Stop)
		{
			this.ObserverRequestInfo = ObserverRequest.Stop;
		}
	}

	public void HideChunk(Vector3i worldPos)
	{
		long num = WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z));
		DynamicMeshItem dynamicMeshItem;
		if (this.ItemsDictionary.TryGetValue(num, out dynamicMeshItem))
		{
			dynamicMeshItem.SetVisible(false, "Hide chunk world pos");
		}
	}

	public void HideChunk(long key)
	{
		DynamicMeshItem dynamicMeshItem;
		if (this.ItemsDictionary.TryGetValue(key, out dynamicMeshItem))
		{
			dynamicMeshItem.SetVisible(false, "Hide chunk world pos");
		}
	}

	public void ShowChunk(long key)
	{
		DynamicMeshItem dynamicMeshItem;
		if (this.ItemsDictionary.TryGetValue(key, out dynamicMeshItem))
		{
			dynamicMeshItem.GetRegion().OnChunkUnloaded(dynamicMeshItem);
			if (dynamicMeshItem.ChunkObject != null)
			{
				dynamicMeshItem.SetVisible(true, "Show chunk force");
			}
		}
	}

	public void LoadItemsDedicated()
	{
		DynamicMeshManager.LogMsg("Loading Items: " + DynamicMeshFile.MeshLocation);
		string[] files = SdDirectory.GetFiles(DynamicMeshFile.MeshLocation);
		SdFileInfo[] array = (from fi in new SdDirectoryInfo(DynamicMeshFile.MeshLocation).GetFiles("*.*")
			where fi.Length == 0L
			select fi).ToArray<SdFileInfo>();
		if (array.Length != 0)
		{
			DynamicMeshManager.LogMsg("Found " + array.Length.ToString() + " files @ zero size. Deleting...");
			SdFileInfo[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i].Delete();
			}
		}
		foreach (string text in files)
		{
			if (text.EndsWith(".update"))
			{
				long num = long.Parse(Path.GetFileNameWithoutExtension(text));
				DynamicMeshItem dynamicMeshItem = this.AddChunk(num, false, false, null);
				dynamicMeshItem.UpdateTime = dynamicMeshItem.ReadUpdateTimeFromFile();
			}
		}
		DynamicMeshManager.LogMsg("Loaded Items: " + this.ItemsDictionary.Count.ToString());
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			base.StartCoroutine(this.SendClientMessage());
		}
		if (files.Length == 0)
		{
			this.CheckPrefabs(" (load items dedicated)", false);
		}
	}

	public void LoadItemsChunksDedicated()
	{
		DynamicMeshManager.LogMsg("Loading Items: " + DynamicMeshFile.MeshLocation);
		string[] files = SdDirectory.GetFiles(DynamicMeshFile.MeshLocation, "*.chunk");
		new Dictionary<string, Vector3>();
		string[] array = files;
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Path.GetFileNameWithoutExtension(array[i]).Split(',', StringSplitOptions.None);
			float num = float.Parse(array2[0]);
			float num2 = float.Parse(array2[1]);
			this.AddChunkStub(new Vector3i(num, 0f, num2), null);
		}
		if (this.ItemsDictionary.Count == 0)
		{
			this.CheckPrefabs("LoadItemsChunksDedicated", false);
		}
		DynamicMeshManager.LogMsg("Loaded Items: " + this.ItemsDictionary.Count.ToString());
	}

	public static int GetViewSize(EntityPlayer _player)
	{
		if (_player.ChunkObserver == null)
		{
			throw new Exception();
		}
		return DynamicMeshManager.player.ChunkObserver.viewDim * 16;
	}

	public static int GetBufferSize(EntityPlayer _player)
	{
		return ((_player.ChunkObserver != null) ? DynamicMeshManager.player.ChunkObserver.viewDim : GamePrefs.GetInt(EnumGamePrefs.OptionsGfxViewDistance)) * 16 * 3;
	}

	public static bool RectContainsRect(Rect r1, Rect r2)
	{
		float xMin = r1.xMin;
		float xMax = r1.xMax;
		float yMin = r1.yMin;
		float yMax = r1.yMax;
		float xMin2 = r2.xMin;
		float xMax2 = r2.xMax;
		float yMin2 = r2.yMin;
		float yMax2 = r2.yMax;
		return xMax2 >= xMin && xMin2 <= xMax && yMax2 >= yMin && yMin2 <= yMax;
	}

	public static bool RectContainsPoint(Rect r1, int x, int z)
	{
		float xMin = r1.xMin;
		float xMax = r1.xMax;
		float yMin = r1.yMin;
		float yMax = r1.yMax;
		return (float)x >= xMin && (float)x <= xMax && (float)z >= yMin && (float)z <= yMax;
	}

	public static void OriginUpdate()
	{
		if (DynamicMeshManager.Instance == null)
		{
			return;
		}
		DynamicMeshManager.Instance.ShowOrHidePrefabs();
		DynamicMeshManager.Instance.SetItemPositions();
	}

	public void SetItemPositions()
	{
		foreach (long num in this.ItemsDictionary.Keys.ToArray<long>())
		{
			this.ItemsDictionary[num].SetPosition();
		}
	}

	public void ShowOrHidePrefabs()
	{
		if (DynamicMeshManager.player == null)
		{
			return;
		}
		if (DynamicMeshRegion.Regions == null)
		{
			return;
		}
		if (DynamicMeshManager.DebugReport)
		{
			DynamicMeshManager.LogMsg("Regions to process: " + DynamicMeshRegion.Regions.Count.ToString());
			string text = "Player Position: ";
			Vector3 position = DynamicMeshManager.player.position;
			DynamicMeshManager.LogMsg(text + position.ToString());
		}
		Vector3i vector3i = new Vector3i(DynamicMeshManager.player.position);
		IChunk chunkFromWorldPos = GameManager.Instance.World.GetChunkFromWorldPos(vector3i);
		if (chunkFromWorldPos == null)
		{
			return;
		}
		Vector3i worldPos = chunkFromWorldPos.GetWorldPos();
		int viewSize = DynamicMeshManager.GetViewSize(DynamicMeshManager.player);
		GameManager instance = GameManager.Instance;
		DictionarySave<long, ChunkGameObject> dictionarySave = ((instance != null) ? instance.World.ChunkCache.DisplayedChunkGameObjects : null);
		if (dictionarySave == null)
		{
			return;
		}
		for (int i = worldPos.x - viewSize; i <= worldPos.x + viewSize; i += 16)
		{
			for (int j = worldPos.z - viewSize; j <= worldPos.z + viewSize; j += 16)
			{
				Vector3i vector3i2 = new Vector3i(i, 0, j);
				DynamicMeshItem itemOrNull = this.GetItemOrNull(vector3i2);
				if (itemOrNull != null && dictionarySave.ContainsKey(itemOrNull.Key))
				{
					itemOrNull.SetVisible(false, "showHide");
				}
				if (DynamicMeshManager.DebugReport)
				{
					string text2 = "World pos: ";
					Vector3i vector3i3 = vector3i2;
					DynamicMeshManager.LogMsg(text2 + vector3i3.ToString());
					DynamicMeshManager.LogMsg("Chunk Item: " + ((itemOrNull == null) ? "null" : itemOrNull.IsVisible.ToString()));
				}
			}
		}
		DynamicMeshItem itemOrNull2 = this.GetItemOrNull(vector3i);
		DynamicMeshRegion dynamicMeshRegion = ((itemOrNull2 != null) ? itemOrNull2.GetRegion() : null);
		if (dynamicMeshRegion != null)
		{
			dynamicMeshRegion.SetVisibleNew(false, "showHide", true);
			foreach (DynamicMeshItem dynamicMeshItem in dynamicMeshRegion.LoadedItems)
			{
				dynamicMeshItem.SetVisible(!dynamicMeshItem.IsChunkInGame, "Region updateItems");
			}
		}
		foreach (DynamicMeshRegion dynamicMeshRegion2 in DynamicMeshRegion.Regions.Values)
		{
			if (dynamicMeshRegion2 != null)
			{
				dynamicMeshRegion2.DistanceChecks();
			}
		}
		DynamicMeshManager.DebugReport = false;
	}

	public bool AddRegionChecks()
	{
		if (DynamicMeshThread.RegionsToCheck == null)
		{
			return false;
		}
		if (DynamicMeshThread.RegionsToCheck.Count == 0)
		{
			return false;
		}
		DynamicMeshThread.AddRegionChecks = false;
		List<Vector3i> list = DynamicMeshThread.RegionsToCheck[0];
		DynamicMeshThread.RegionsToCheck.RemoveAt(0);
		DynamicMeshManager.LogMsg(string.Concat(new string[]
		{
			"Adding new region to check: ",
			list.Count.ToString(),
			"  at ",
			Time.time.ToString(),
			"  remaining: ",
			DynamicMeshThread.RegionsToCheck.Count.ToString()
		}));
		foreach (Vector3i vector3i in list)
		{
			this.AddChunk(vector3i, false);
		}
		if (DynamicMeshThread.RegionsToCheck.Count == 0)
		{
			DynamicMeshThread.RegionsToCheck = null;
		}
		return true;
	}

	public void OnGUI()
	{
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			return;
		}
		if (!DynamicMeshManager.ShowGui || GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		if (DynamicMeshManager.player == null)
		{
			return;
		}
		Vector3i regionPositionFromWorldPosition = DynamicMeshUnity.GetRegionPositionFromWorldPosition(DynamicMeshManager.player.position);
		Vector3i vector3i = new Vector3i(World.toChunkXZ((int)DynamicMeshManager.player.position.x) * 16, 0, World.toChunkXZ((int)DynamicMeshManager.player.position.z) * 16);
		string text = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? "SP /p2p Host" : "Dedi / p2p");
		try
		{
			string[] array = new string[71];
			array[0] = "p: ";
			array[1] = DynamicMeshManager.player.position.ToString();
			array[2] = " - ";
			array[3] = DynamicMeshManager.player.transform.position.ToString();
			array[4] = "\nc: ";
			array[5] = vector3i.x.ToString();
			array[6] = ",";
			array[7] = vector3i.z.ToString();
			array[8] = "\nr:";
			array[9] = regionPositionFromWorldPosition.x.ToString();
			array[10] = ",";
			array[11] = regionPositionFromWorldPosition.z.ToString();
			array[12] = "\n Buff : ";
			array[13] = this.BufferRegionLoadRequests.Count.ToString();
			array[14] = "\n Items: ";
			array[15] = this.ChunkMeshData.Count.ToString();
			array[16] = "\n ItemGen: ";
			array[17] = DynamicMeshThread.MeshGenCount.ToString();
			array[18] = "\n Reg  : ";
			array[19] = this.RegionsAvailableToLoad.Count.ToString();
			array[20] = string.Format(" ({0})", this.AvailableRegionLoadRequests);
			array[21] = "\n Server : ";
			array[22] = SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer.ToString();
			array[23] = " / ";
			array[24] = SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient.ToString();
			array[25] = "\n Thread P/S : ";
			array[26] = DynamicMeshThread.PrimaryQueue.Count.ToString();
			array[27] = " / ";
			array[28] = DynamicMeshThread.SecondaryQueue.Count.ToString();
			array[29] = "\n Game: ";
			array[30] = text;
			array[31] = "\n Packets: ";
			array[32] = NetPackageDynamicMesh.Count.ToString();
			array[33] = "\n ThreadDistance: ";
			array[34] = DynamicMeshManager.ThreadDistance.ToString();
			array[35] = "\n ObserverDistance: ";
			array[36] = DynamicMeshManager.ObserverDistance.ToString();
			array[37] = "\n ItemLoadDistance: ";
			array[38] = DynamicMeshManager.ItemLoadDistance.ToString();
			array[39] = "\n ItemCache: ";
			array[40] = DynamicMeshThread.ChunkDataQueue.ChunkData.Count.ToString();
			array[41] = " (";
			array[42] = DynamicMeshThread.ChunkDataQueue.LiveItems.ToString();
			array[43] = " live)\n WorldChunks: ";
			array[44] = GameManager.Instance.World.ChunkCache.chunks.list.Count.ToString();
			array[45] = "\n ThreadNext: ";
			array[46] = DynamicMeshThread.nextChunks.Count.ToString();
			array[47] = "\n ThreadQueue: ";
			array[48] = DynamicMeshThread.Queue;
			array[49] = "\n RegionUpdates: ";
			array[50] = DynamicMeshThread.RegionUpdates.Count.ToString();
			array[51] = "\n RegionUpdatesDebug: ";
			array[52] = DynamicMeshThread.RegionUpdatesDebug;
			array[53] = "\n SyncPackets: ";
			array[54] = DynamicMeshServer.SyncRequests.Count.ToString();
			array[55] = " (";
			array[56] = DynamicMeshServer.ActiveSyncs.Count.ToString();
			array[57] = ")\n DataSaveQueue: ";
			array[58] = DynamicMeshThread.ChunkDataQueue.ChunkData.Count.ToString();
			array[59] = "\n DataCache: ";
			array[60] = DynamicMeshChunkData.ActiveDataItems.ToString();
			array[61] = "/";
			array[62] = DynamicMeshChunkData.Cache.Count.ToString();
			array[63] = " (";
			array[64] = DynamicMeshChunkData.Cache.Sum((DynamicMeshChunkData d) => (double)d.GetStreamSize() / 1024.0 / 1024.0).ToString();
			array[65] = "MB)\n vMeshCache: ";
			array[66] = DynamicMeshVoxelLoad.LayerCache.Count.ToString();
			array[67] = "\n DymeshData: ";
			array[68] = DyMeshData.ActiveItems.ToString();
			array[69] = "/";
			array[70] = DyMeshData.TotalItems.ToString();
			string text2 = string.Concat(array);
			foreach (DynamicMeshChunkProcessor dynamicMeshChunkProcessor in DynamicMeshThread.BuilderManager.BuilderThreads.ToList<DynamicMeshChunkProcessor>())
			{
				if (dynamicMeshChunkProcessor != null)
				{
					string[] array2 = new string[9];
					array2[0] = text2;
					array2[1] = "\n Thread: ";
					int num = 2;
					DynamicMeshItem item = dynamicMeshChunkProcessor.Item;
					array2[num] = ((item != null) ? item.ToDebugLocation() : null);
					array2[3] = " Data ";
					array2[4] = ((int)dynamicMeshChunkProcessor.MeshDataTime).ToString();
					array2[5] = "ms Mesh: ";
					array2[6] = ((int)dynamicMeshChunkProcessor.ExportTime).ToString();
					array2[7] = " Inactive: ";
					array2[8] = ((int)dynamicMeshChunkProcessor.InactiveTime).ToString();
					text2 = string.Concat(array2);
				}
			}
			Color contentColor = GUI.contentColor;
			Color color = GUI.color;
			int depth = GUI.depth;
			GUI.depth = 0;
			GUI.DrawTexture(new Rect(0f, (float)Screen.height, 500f, 500f), Texture2D.blackTexture, 0);
			GUI.color = DynamicMeshManager.DebugStyle.normal.textColor;
			GUI.contentColor = DynamicMeshManager.DebugStyle.normal.textColor;
			GUI.Label(new Rect(0f, (float)DynamicMeshManager.GuiY, 500f, 500f), text2, DynamicMeshManager.DebugStyle);
			GUI.contentColor = contentColor;
			GUI.color = color;
			GUI.depth = depth;
		}
		catch (Exception)
		{
		}
	}

	public DynamicMeshRegion GetNearestUnloadedRegion()
	{
		if (this.PrimaryLocation != null)
		{
			this.NearestRegionWithUnloaded = this.GetRegion(this.PrimaryLocation.Value);
		}
		if (this.NearestRegionWithUnloaded == null)
		{
			DynamicMeshRegion dynamicMeshRegion = (from d in DynamicMeshRegion.Regions.Values
				where !d.FastTrackLoaded && d.UnloadedItems.Any<DynamicMeshItem>()
				orderby d.DistanceToPlayer()
				select d).ToList<DynamicMeshRegion>().FirstOrDefault<DynamicMeshRegion>();
			(from d in DynamicMeshRegion.Regions.Values
				where d.UnloadedItems.Any<DynamicMeshItem>()
				orderby d.DistanceToPlayer()
				select d).FirstOrDefault<DynamicMeshRegion>();
			this.NearestRegionWithUnloaded = dynamicMeshRegion;
			if (this.NearestRegionWithUnloaded != null)
			{
				this.NearestRegionWithUnloaded.FastTrackLoaded = true;
			}
		}
		this.PrimaryLocation = null;
		this.FindNearestUnloadedItems = false;
		return this.NearestRegionWithUnloaded;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MonitorGC()
	{
		for (int i = 0; i < 3; i++)
		{
			int num = GC.CollectionCount(i);
			if (num != this.gcGenCount[i])
			{
				this.gcGenCount[i] = num;
				Log.Out(string.Format("Gen {0} has fired: ", i) + num.ToString());
			}
		}
	}

	public void Update()
	{
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			return;
		}
		if (DynamicMeshManager.Instance == null)
		{
			return;
		}
		this.time = Time.time;
		if (this.FindNearestUnloadedItems)
		{
			this.GetNearestUnloadedRegion();
		}
		while (this.ChunksToRemove.Count > 0)
		{
			Vector2i vector2i;
			if (this.ChunksToRemove.TryDequeue(out vector2i))
			{
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg("Remove chunk from region " + vector2i.x.ToString() + "," + vector2i.y.ToString());
				}
				DynamicMeshItem itemFromWorldPosition = this.GetItemFromWorldPosition(vector2i.x, vector2i.y);
				this.RemoveItem(itemFromWorldPosition, true);
			}
		}
		if (this.ProcessItemLoadReady || this.ForceNextItemLoad < DateTime.Now)
		{
			if (this.ForceNextItemLoad < DateTime.Now)
			{
				Log.Warning("Forcing mesh processing after large delay");
			}
			base.StartCoroutine(this.ProcessItemMeshGeneration());
		}
		if (this.RegionFileLoadRequests.Count > 0)
		{
			base.StartCoroutine(this.ProcessChunkRegionRequests());
		}
		this.DestroyObjects();
		this.CheckFallingObservers();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			DynamicMeshServer.Update();
		}
		if (this.Observer.StopTime < Time.time)
		{
			this.Observer.Stop();
			this.ObserverPrep.Stop();
		}
		if (this.nextShowHide < Time.time)
		{
			this.ShowOrHidePrefabs();
			this.nextShowHide = Time.time + (float)(DynamicMeshManager.ShowHideCheckTime / 1000);
			this.CheckGameObjects();
		}
		if (this.ObserverRequestInfo != ObserverRequest.None)
		{
			if (this.ObserverRequestInfo == ObserverRequest.Start)
			{
				this.Observer.Start(this.ObserverPos);
			}
			else if (this.ObserverRequestInfo == ObserverRequest.Stop)
			{
				this.Observer.StopTime = Time.time + 3f;
			}
			this.ObserverRequestInfo = ObserverRequest.None;
		}
		while (DynamicMeshThread.ReadyForCollection.Count > 0)
		{
			DynamicMeshData dynamicMeshData;
			if (DynamicMeshThread.ReadyForCollection.TryDequeue(out dynamicMeshData))
			{
				DynamicMeshItem itemFromWorldPosition2 = this.GetItemFromWorldPosition(dynamicMeshData.X, dynamicMeshData.Z);
				DynamicMeshRegion dynamicMeshRegion = itemFromWorldPosition2.GetRegion();
				if (itemFromWorldPosition2.ChunkObject != null || (dynamicMeshRegion != null && dynamicMeshRegion.IsInItemLoad()))
				{
					this.AddItemLoadRequest(itemFromWorldPosition2, false);
				}
			}
		}
		while (DynamicMeshThread.ChunkReadyForCollection.Count > 0)
		{
			Vector2i vector2i2;
			if (DynamicMeshThread.ChunkReadyForCollection.TryRemoveFirst(out vector2i2))
			{
				DynamicMeshItem itemFromWorldPosition3 = this.GetItemFromWorldPosition(vector2i2.x, vector2i2.y);
				DynamicMeshRegion dynamicMeshRegion = itemFromWorldPosition3.GetRegion();
				if (itemFromWorldPosition3.ChunkObject != null || (dynamicMeshRegion != null && dynamicMeshRegion.IsInItemLoad()))
				{
					DynamicMeshThread.AddChunkGenerationRequest(itemFromWorldPosition3);
				}
			}
		}
		while (this.AvailableRegionLoadRequests > 0 && this.RegionsAvailableToLoad.Count > 0)
		{
			LinkedListNode<long> linkedListNode = null;
			float num = 99999f;
			for (LinkedListNode<long> linkedListNode2 = this.RegionsAvailableToLoad.First; linkedListNode2 != null; linkedListNode2 = linkedListNode2.Next)
			{
				long value = linkedListNode2.Value;
				DynamicMeshRegion dynamicMeshRegion = this.GetRegion(value);
				if (dynamicMeshRegion.OutsideLoadArea)
				{
					if (linkedListNode == linkedListNode2)
					{
						linkedListNode = null;
					}
					this.RegionsAvailableToLoad.Remove(linkedListNode2);
				}
				else
				{
					float num2 = dynamicMeshRegion.DistanceToPlayer();
					if (num2 < num)
					{
						linkedListNode = linkedListNode2;
						num = num2;
						if (num < 100f)
						{
							break;
						}
					}
				}
			}
			if (linkedListNode == null)
			{
				break;
			}
			long value2 = linkedListNode.Value;
			this.RegionsAvailableToLoad.Remove(linkedListNode);
			DynamicMeshThread.AddRegionLoadRequest(DyMeshRegionLoadRequest.Create(value2));
			this.AvailableRegionLoadRequests--;
		}
		List<DynamicMeshUpdateData> list = new List<DynamicMeshUpdateData>();
		for (int i = 0; i < this.UpdateData.Count; i++)
		{
			DynamicMeshUpdateData dynamicMeshUpdateData = this.UpdateData[i];
			if (dynamicMeshUpdateData.UpdateTime < Time.time || dynamicMeshUpdateData.IsUrgent || dynamicMeshUpdateData.MaxTime < Time.time)
			{
				this.AddChunk(dynamicMeshUpdateData.Key, dynamicMeshUpdateData.AddToThread, true, null);
				list.Add(dynamicMeshUpdateData);
			}
		}
		foreach (DynamicMeshUpdateData dynamicMeshUpdateData2 in list)
		{
			this.UpdateData.Remove(dynamicMeshUpdateData2);
		}
		if (this.nextUpdate > Time.time)
		{
			return;
		}
		if (DynamicMeshThread.RegionsToCheck != null && DynamicMeshThread.AddRegionChecks)
		{
			this.AddRegionChecks();
		}
		this.nextUpdate = Time.time + 1f;
		EntityPlayerLocal player = DynamicMeshManager.player;
		if (player != null)
		{
			DynamicMeshThread.PlayerPositionX = player.position.x;
			DynamicMeshThread.PlayerPositionZ = player.position.z;
		}
		if (DynamicMeshChunkProcessor.DebugOnMainThread)
		{
			DynamicMeshThread.BuilderManager.MainThreadRunJobs();
		}
		if (this.nextOrphanCheck < DateTime.Now)
		{
			this.nextOrphanCheck = DateTime.Now.AddSeconds(10.0);
			base.StartCoroutine(this.CheckForOrphans(true));
		}
		if (this.disabledImposterChunksDirty)
		{
			this.disabledImposterChunkManager.Update();
			this.disabledImposterChunksDirty = false;
		}
	}

	public void AddItemLoadRequest(DynamicMeshItem item, bool urgent)
	{
		item.GetRegion().AddToLoadingQueue(item);
		DynamicMeshThread.AddChunkGenerationRequest(item);
	}

	public bool IsInLoadableArea(long key)
	{
		return !DynamicMeshSettings.OnlyPlayerAreas || this.HandlePlayerOnlyAreas(key, DynamicMeshUnity.GetWorldPosFromKey(key));
	}

	public DynamicMeshItem GetItemOrNull(Vector3i worldPos)
	{
		long itemKey = DynamicMeshUnity.GetItemKey(worldPos.x, worldPos.z);
		DynamicMeshItem dynamicMeshItem;
		if (this.ItemsDictionary.TryGetValue(itemKey, out dynamicMeshItem))
		{
			return dynamicMeshItem;
		}
		return null;
	}

	public DynamicMeshItem GetItemOrNull(long key)
	{
		DynamicMeshItem dynamicMeshItem;
		if (this.ItemsDictionary.TryGetValue(key, out dynamicMeshItem))
		{
			return dynamicMeshItem;
		}
		return null;
	}

	public DynamicMeshItem GetItemFromWorldPosition(int x, int z)
	{
		long itemKey = DynamicMeshUnity.GetItemKey(x, z);
		DynamicMeshItem dynamicMeshItem;
		if (this.ItemsDictionary.TryGetValue(itemKey, out dynamicMeshItem))
		{
			return dynamicMeshItem;
		}
		dynamicMeshItem = this.AddChunk(itemKey, false, false, null);
		return dynamicMeshItem;
	}

	public DynamicMeshRegion GetRegion(int x, int z)
	{
		return this.GetRegion(DynamicMeshUnity.GetRegionKeyFromWorldPosition(x, z));
	}

	public DynamicMeshRegion GetRegion(BlockValueRef bvRef)
	{
		switch (bvRef.Type)
		{
		case BlockValueRefType.None:
			return null;
		case BlockValueRefType.Block:
			return this.GetRegion(bvRef.BlockPosition);
		case BlockValueRefType.Prop:
		{
			Vector2i vector2i = World.toChunkXZ(bvRef.PropReference.ChunkPos);
			return this.GetRegion(new Vector3i(vector2i.x, 0, vector2i.y));
		}
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	public DynamicMeshRegion GetRegion(Vector3i worldPos)
	{
		return this.GetRegion(DynamicMeshUnity.GetRegionKeyFromWorldPosition(worldPos.x, worldPos.z));
	}

	public DynamicMeshRegion GetRegion(DynamicMeshItem item)
	{
		return this.GetRegion(item.GetRegionKey());
	}

	public DynamicMeshRegion GetRegion(long key)
	{
		DynamicMeshRegion dynamicMeshRegion;
		DynamicMeshRegion.Regions.TryGetValue(key, out dynamicMeshRegion);
		if (dynamicMeshRegion == null)
		{
			dynamicMeshRegion = new DynamicMeshRegion(key);
			if (!DynamicMeshRegion.Regions.TryAdd(key, dynamicMeshRegion))
			{
				DynamicMeshRegion.Regions.TryGetValue(key, out dynamicMeshRegion);
			}
		}
		return dynamicMeshRegion;
	}

	public static void Init()
	{
		DynamicMeshManager.CONTENT_ENABLED = GamePrefs.GetBool(EnumGamePrefs.DynamicMeshEnabled);
		DynamicMeshManager.DisabledImposterChunkManager.DisableShaderKeyword();
		GameManager.Instance.StopCoroutine(DynamicMeshManager.DelayStartForWorldLoad());
		GameManager.Instance.StartCoroutine(DynamicMeshManager.DelayStartForWorldLoad());
	}

	public static EntityPlayerLocal player
	{
		get
		{
			if (GameManager.Instance.World != null)
			{
				return GameManager.Instance.World.GetPrimaryPlayer();
			}
			return null;
		}
	}

	public static void EnableErrorCallstackLogs()
	{
		Log.LogCallbacks -= new Log.LogCallbackDelegate(DynamicMeshManager.ShowErrorStackTraces);
		Log.LogCallbacks += new Log.LogCallbackDelegate(DynamicMeshManager.ShowErrorStackTraces);
	}

	public static void DisableErrorCallstackLogs()
	{
		Log.LogCallbacks -= new Log.LogCallbackDelegate(DynamicMeshManager.ShowErrorStackTraces);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator DelayStartForWorldLoad()
	{
		while (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			Log.Out("Dynamic mesh waiting for world");
			yield return DynamicMeshFile.WaitOne;
		}
		if (!DynamicMeshManager.CONTENT_ENABLED)
		{
			Log.Out("Dynamic mesh disabled on world start");
			yield break;
		}
		DynamicMeshManager.LogMsg("Prepping dynamic mesh. Resend Default: " + DynamicMeshServer.ResendPackages.ToString());
		DynamicMeshFile.MeshLocation = (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer ? (GameIO.GetSaveGameDir() + "/DynamicMeshes/") : (GameIO.GetSaveGameLocalDir() + "/DynamicMeshes/"));
		DynamicMeshManager.LogMsg("Mesh location: " + DynamicMeshFile.MeshLocation);
		if (!SdDirectory.Exists(DynamicMeshFile.MeshLocation))
		{
			SdDirectory.CreateDirectory(DynamicMeshFile.MeshLocation);
		}
		yield return new WaitForSeconds(1f);
		if (DynamicMeshManager.Parent != null)
		{
			Object.Destroy(DynamicMeshManager.Parent);
			DynamicMeshManager.Parent = null;
		}
		DynamicMeshManager.DebugStyle.fontSize = ((Screen.width > 3000) ? 30 : 14);
		DynamicMeshManager.DebugStyle.normal.textColor = Color.magenta;
		if (DynamicMeshManager.Instance)
		{
			DynamicMeshManager.OnWorldUnload();
			DynamicMeshManager.Instance.ClearPrefabs();
		}
		DynamicMeshManager.LogMsg("Warming dynamic mesh");
		if (DynamicMeshManager.Instance == null || DynamicMeshManager.Parent == null)
		{
			DynamicMeshManager.LogMsg("Creating dynamic mesh manager");
			DynamicMeshManager.Parent = new GameObject("DynamicMeshes");
			DynamicMeshManager.Parent.AddComponent<DynamicMeshManager>();
		}
		DynamicMeshBlockSwap.Init();
		DynamicMeshSettings.Validate();
		if (!GameManager.IsDedicatedServer && Application.isEditor && SdDirectory.Exists("D:\\7DaysToDie\\trunkCode"))
		{
			DynamicMeshConsoleCmd.DebugAll();
		}
		yield break;
	}

	public void ReorderGameObjects()
	{
		List<Transform> regions = new List<Transform>();
		List<Transform> list = new List<Transform>();
		foreach (object obj in DynamicMeshManager.Parent.transform)
		{
			Transform transform = (Transform)obj;
			list.Add(transform);
		}
		regions = (from d in list
			where !d.gameObject.name.StartsWith("C")
			orderby d.gameObject.name
			select d).ToList<Transform>();
		list.RemoveAll((Transform d) => regions.Contains(d));
		int num = 0;
		foreach (Transform transform2 in regions)
		{
			transform2.SetSiblingIndex(num++);
			if (!(transform2.gameObject.name == string.Empty))
			{
				string[] array = transform2.gameObject.name.Replace("(sync)", "").Replace("R ", "").Split(',', StringSplitOptions.None);
				int num2 = int.Parse(array[0]);
				int num3 = int.Parse(array[1]);
				foreach (Transform transform3 in list)
				{
					string[] array2 = transform3.gameObject.name.Substring(2, transform3.gameObject.name.IndexOf(":") - 2).Split(',', StringSplitOptions.None);
					int num4 = int.Parse(array2[0]);
					int num5 = int.Parse(array2[1]);
					if (num4 >= num2 && num5 >= num3 && num4 < num2 + 160 && num5 < num3 + 160)
					{
						transform3.SetSiblingIndex(num++);
					}
				}
			}
		}
	}

	public void RemoveItem(DynamicMeshItem item, bool removedFromWorld)
	{
		if (item == null)
		{
			return;
		}
		this.GetRegion(item).RemoveChunk(item.WorldPosition.x, item.WorldPosition.z, "removeItem", removedFromWorld);
		if (removedFromWorld && !GameManager.IsDedicatedServer)
		{
			DynamicMeshItem dynamicMeshItem;
			this.disabledImposterChunksDirty |= this.ItemsDictionary.TryRemove(item.Key, out dynamicMeshItem);
		}
		DynamicMeshThread.RemoveRegionChunk(item.WorldPosition.x, item.WorldPosition.z, item.Key);
		DynamicMeshUnity.RemoveDisabledImposterChunk(item.Key);
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("Item removed: " + item.ToDebugLocation());
		}
	}

	public static void ChunkChanged(Vector3i blockPos, int entityId, int blockType)
	{
		DynamicMeshManager.ChunkChanged(new BlockValueRef(blockPos), entityId, blockType);
	}

	public static void ChunkChanged(BlockValueRef bvRef, int entityId, int blockType)
	{
		if (DynamicMeshManager.Instance == null)
		{
			return;
		}
		if (blockType != -1 && !DynamicMeshBlockSwap.IsValidBlock(blockType))
		{
			return;
		}
		if (ThreadManager.IsMainThread() && DynamicMeshManager.player != null && DynamicMeshManager.player.entityId == entityId)
		{
			DynamicMeshRegion region = DynamicMeshManager.Instance.GetRegion(bvRef);
			if (region.IsInItemLoad())
			{
				region.SetVisibleNew(false, DynamicMeshManager.ChunkChangedInItemLoad, true);
			}
		}
		Vector3i vector3i;
		if (!DynamicMeshManager.IsServer || !bvRef.TryGetBlockPos(out vector3i))
		{
			return;
		}
		DynamicMeshManager.Instance.AddUpdateData(vector3i, false, true);
		int num = vector3i.x & 15;
		if (num == 0)
		{
			DynamicMeshManager.Instance.AddUpdateData(vector3i + new Vector3i(-16, 0, 0), false, true);
		}
		else if (num == 15)
		{
			DynamicMeshManager.Instance.AddUpdateData(vector3i + new Vector3i(16, 0, 0), false, true);
		}
		if ((vector3i.z & 15) == 0)
		{
			DynamicMeshManager.Instance.AddUpdateData(vector3i + new Vector3i(0, 0, -16), false, true);
			return;
		}
		if (num == 15)
		{
			DynamicMeshManager.Instance.AddUpdateData(vector3i + new Vector3i(0, 0, 16), false, true);
		}
	}

	public bool AddUpdateData(Vector3i worldPos, bool isUrgent, bool addToThread)
	{
		long num = WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldPos.x), World.toChunkXZ(worldPos.z));
		return this.AddUpdateData(num, isUrgent, addToThread, true, 1);
	}

	public bool AddUpdateData(int worldX, int worldZ, bool isUrgent, bool addToThread)
	{
		long num = WorldChunkCache.MakeChunkKey(World.toChunkXZ(worldX), World.toChunkXZ(worldZ));
		return this.AddUpdateData(num, isUrgent, addToThread, true, 1);
	}

	public bool AddUpdateData(long key, bool isUrgent, bool addToThread, bool checkPlayerArea, int delayScale = 1)
	{
		if (DynamicMeshThread.ChunkDataQueue == null || DynamicMeshManager.Instance == null)
		{
			return false;
		}
		DynamicMeshUpdateData dynamicMeshUpdateData = null;
		Vector3i vector3i = new Vector3i(WorldChunkCache.extractX(key) * 16, 0, WorldChunkCache.extractZ(key) * 16);
		Vector3 vector = vector3i.ToVector3();
		if (checkPlayerArea && !this.HandlePlayerOnlyAreas(key, vector))
		{
			return false;
		}
		int i = 0;
		while (i < this.UpdateData.Count)
		{
			DynamicMeshUpdateData dynamicMeshUpdateData2 = this.UpdateData[i++];
			if (dynamicMeshUpdateData2.Key == key)
			{
				dynamicMeshUpdateData = dynamicMeshUpdateData2;
				break;
			}
		}
		if (dynamicMeshUpdateData == null)
		{
			dynamicMeshUpdateData = new DynamicMeshUpdateData();
			dynamicMeshUpdateData.ChunkPosition = vector3i;
			dynamicMeshUpdateData.Key = key;
			dynamicMeshUpdateData.MaxTime = this.time + DynamicMeshManager.MaxRebuildTime;
			dynamicMeshUpdateData.AddToThread = addToThread;
			this.UpdateData.Add(dynamicMeshUpdateData);
		}
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg(string.Concat(new string[]
			{
				"Adding update ",
				key.ToString(),
				"  Time: ",
				dynamicMeshUpdateData.UpdateTime.ToString(),
				" pos: ",
				dynamicMeshUpdateData.ToDebugLocation()
			}));
		}
		dynamicMeshUpdateData.UpdateTime = this.time + (float)(this.QueueDelay * delayScale);
		dynamicMeshUpdateData.IsUrgent = dynamicMeshUpdateData.IsUrgent || isUrgent;
		dynamicMeshUpdateData.AddToThread = dynamicMeshUpdateData.AddToThread || addToThread;
		return true;
	}

	public bool HandlePlayerOnlyAreas(long key, Vector3 position)
	{
		return !DynamicMeshSettings.OnlyPlayerAreas || this.IsPositionInRange(position);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsPositionInRange(Vector3 _position)
	{
		float x = _position.x;
		float z = _position.z;
		int num = Math.Max(16, (DynamicMeshSettings.PlayerAreaChunkBuffer - 1) / 2 * 16);
		DictionaryList<int, EntityPlayer> players = GameManager.Instance.World.Players;
		for (int i = 0; i < players.list.Count; i++)
		{
			for (int j = 0; j < players.list[i].SpawnPoints.Count; j++)
			{
				Vector3i vector3i = players.list[i].SpawnPoints[j];
				int num2 = vector3i.x - num;
				int num3 = vector3i.x + num + 16;
				int num4 = vector3i.z - num;
				int num5 = vector3i.z + num + 16;
				if (x >= (float)num2 && x < (float)num3 && z >= (float)num4 && z < (float)num5)
				{
					return true;
				}
			}
		}
		foreach (PersistentPlayerData persistentPlayerData in GameManager.Instance.persistentPlayers.m_lpBlockMap.Values)
		{
			for (int k = 0; k < persistentPlayerData.LPBlocks.Count; k++)
			{
				Vector3i vector3i2 = persistentPlayerData.LPBlocks[k];
				int num2 = DynamicMeshUnity.GetItemPosition(vector3i2.x) - num;
				int num3 = DynamicMeshUnity.GetItemPosition(vector3i2.x) + num + 16;
				int num4 = DynamicMeshUnity.GetItemPosition(vector3i2.z) - num;
				int num5 = DynamicMeshUnity.GetItemPosition(vector3i2.z) + num + 16;
				if (x >= (float)num2 && x < (float)num3 && z >= (float)num4 && z < (float)num5)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void ImportVox(string name, Vector3 pos, int blockId)
	{
		if (blockId == 0)
		{
			blockId = 502;
		}
		if (DynamicMeshManager.Instance == null || GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		GameManager.bPhysicsActive = false;
		string text = DynamicMeshFile.MeshLocation + name + ".vox";
		if (!SdFile.Exists(text))
		{
			Log.Out("File " + text + " does not exist. Cancelling import");
			return;
		}
		byte[] array = Convert.FromBase64String(SdFile.ReadAllText(text));
		BlockValue blockValue = default(BlockValue);
		blockValue.type = blockId;
		BlockValue blockValue2 = default(BlockValue);
		blockValue2.type = 1;
		HashSet<Vector3i> hashSet = new HashSet<Vector3i>();
		int num;
		int num3;
		List<BlockChangeInfo> list;
		using (MemoryStream memoryStream = new MemoryStream(array))
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(memoryStream);
				num = (int)pooledBinaryReader.ReadByte();
				int num2 = (int)pooledBinaryReader.ReadByte();
				num3 = (int)pooledBinaryReader.ReadByte();
				list = new List<BlockChangeInfo>(num * num2 * num3 / 2);
				while (pooledBinaryReader.BaseStream.Position != pooledBinaryReader.BaseStream.Length)
				{
					byte b = pooledBinaryReader.ReadByte();
					byte b2 = pooledBinaryReader.ReadByte();
					byte b3 = pooledBinaryReader.ReadByte();
					int num4 = (int)(pos.x + (float)b);
					int num5 = (int)(pos.y + (float)b2);
					int num6 = (int)(pos.z + (float)b3);
					BlockChangeInfo blockChangeInfo = new BlockChangeInfo(new BlockValueRef(num4, num5, num6), blockValue, 0);
					if (!hashSet.Contains(blockChangeInfo.blockValueRef.BlockPosition))
					{
						list.Add(blockChangeInfo);
						hashSet.Add(blockChangeInfo.blockValueRef.BlockPosition);
					}
					blockChangeInfo = new BlockChangeInfo(new BlockValueRef(num4 + 1, num5, num6), blockValue2, 0);
					if (!hashSet.Contains(blockChangeInfo.blockValueRef.BlockPosition))
					{
						list.Add(blockChangeInfo);
					}
					blockChangeInfo = new BlockChangeInfo(new BlockValueRef(num4 - 1, num5, num6), blockValue2, 0);
					if (!hashSet.Contains(blockChangeInfo.blockValueRef.BlockPosition))
					{
						list.Add(blockChangeInfo);
					}
					blockChangeInfo = new BlockChangeInfo(new BlockValueRef(num4, num5, num6 + 1), blockValue2, 0);
					if (!hashSet.Contains(blockChangeInfo.blockValueRef.BlockPosition))
					{
						list.Add(blockChangeInfo);
					}
					blockChangeInfo = new BlockChangeInfo(new BlockValueRef(num4, num5, num6 - 1), blockValue2, 0);
					if (!hashSet.Contains(blockChangeInfo.blockValueRef.BlockPosition))
					{
						list.Add(blockChangeInfo);
					}
				}
			}
		}
		Log.Out("Setting " + list.Count.ToString() + " blocks");
		GameManager.Instance.ChangeBlocks(null, list);
		Log.Out(name + " imported");
		GameManager.bPhysicsActive = true;
		int num7 = (num + 32) / 2;
		int num8 = (num3 + 32) / 2;
		int num9 = (int)pos.x - num7;
		while ((float)num9 < pos.x + (float)num7)
		{
			int num10 = (int)pos.z - num8;
			while ((float)num10 < pos.z + (float)num8)
			{
				DynamicMeshManager.Instance.AddChunk(new Vector3i(num9, 0, num10), true);
				num10 += 16;
			}
			num9 += 16;
		}
	}

	public static void AddFallingBlockObserver(Vector3i pos)
	{
		if (DynamicMeshManager.Instance == null)
		{
			return;
		}
		if (!DynamicMeshManager.IsServer)
		{
			return;
		}
		using (List<DynamicObserver>.Enumerator enumerator = DynamicMeshManager.Instance.Observers.GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				if (enumerator.Current.ContainsPoint(pos))
				{
					return;
				}
			}
		}
		DynamicObserver dynamicObserver = new DynamicObserver();
		dynamicObserver.Start(pos.ToVector3());
		DynamicMeshManager.Instance.Observers.Add(dynamicObserver);
		DynamicMeshManager.Instance.NextFallingCheck = DateTime.Now.AddSeconds(5.0);
	}

	public void CheckGameObjects()
	{
		int num = 0;
		foreach (object obj in DynamicMeshManager.ParentTransform)
		{
			GameObject gameObject = ((Transform)obj).gameObject;
			if (gameObject.name.StartsWith("C ") && !(gameObject.name == ""))
			{
				string[] array = gameObject.name.Substring(2, gameObject.name.IndexOf(" ", 3) - 3).Split(',', StringSplitOptions.None);
				int num2 = int.Parse(array[0]);
				int num3 = int.Parse(array[1]);
				DynamicMeshItem itemFromWorldPosition = this.GetItemFromWorldPosition(num2, num3);
				if (itemFromWorldPosition.ChunkObject == null)
				{
					this.AddObjectForDestruction(gameObject);
					num++;
				}
				if (!itemFromWorldPosition.GetRegion().IsInItemLoad())
				{
					num++;
					this.AddObjectForDestruction(gameObject);
				}
				if (itemFromWorldPosition.ChunkObject != gameObject)
				{
					num++;
					this.AddObjectForDestruction(gameObject);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckFallingObservers()
	{
		if (this.NextFallingCheck > DateTime.Now)
		{
			return;
		}
		this.NextFallingCheck = DateTime.Now.AddSeconds(5.0);
		for (int i = this.Observers.Count - 1; i >= 0; i--)
		{
			DynamicObserver dynamicObserver = this.Observers[i];
			if (!dynamicObserver.HasFallingBlocks())
			{
				dynamicObserver.Stop();
				this.Observers.RemoveAt(i);
			}
		}
	}

	public int DebugOption;

	public float DebugY = -0.7f;

	public float TestZ = -1f;

	public static int DebugX = 1840;

	public static int DebugZ = 672;

	public static bool DebugItemPositions = false;

	public static bool DebugReleases = false;

	public int QueueDelay = 5;

	public static bool ForceMeshGeneration;

	public static string FileMissing = "FM";

	public static bool Allow32BitMeshes = true;

	public static GUIStyle DebugStyle = new GUIStyle();

	public static bool CONTENT_ENABLED = true;

	public static bool CompressFiles = true;

	public static bool DisableScopeTexture = true;

	public static int GuiY = 0;

	public static float MaxRebuildTime = 300f;

	public PrefabCheckState PrefabCheck;

	public static Rect ViewRect = default(Rect);

	public int QuadSize = 10000;

	public ConcurrentDictionary<long, DynamicMeshItem> ItemsDictionary;

	public static bool DisableLOD = false;

	public DynamicObserver Observer = new DynamicObserver();

	public DynamicObserver ObserverPrep = new DynamicObserver();

	public Vector3 ObserverPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 ObserverPosNext;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ObserverRequest ObserverRequestInfo;

	public static DynamicMeshManager Instance;

	public static bool DisableMoveVerts = false;

	public static bool ShowDebug = true;

	public Queue ToRemove = new Queue();

	public static int CombineType = 1;

	public static HashSet<long> ChunkGameObjects = new HashSet<long>();

	public List<DynamicObserver> Observers = new List<DynamicObserver>();

	public DynamicMeshServerStatus ClientMessage;

	public ConcurrentHashSet<DynamicMeshVoxelLoad> BufferRegionLoadRequests = new ConcurrentHashSet<DynamicMeshVoxelLoad>();

	public LinkedList<DynamicMeshItem> ChunkMeshLoadRequests = new LinkedList<DynamicMeshItem>();

	public object _chunkMeshDataLock = new object();

	public LinkedList<DynamicMeshVoxelLoad> ChunkMeshData = new LinkedList<DynamicMeshVoxelLoad>();

	public LinkedList<long> RegionsAvailableToLoad = new LinkedList<long>();

	public int AvailableRegionLoadRequests;

	public ConcurrentQueue<DyMeshRegionLoadRequest> RegionFileLoadRequests = new ConcurrentQueue<DyMeshRegionLoadRequest>();

	public static MicroStopwatch MeshLoadStop = new MicroStopwatch();

	public int LongestRegionLoad;

	public static bool DebugReport = false;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float nextUpdate;

	public static bool ShowGui = false;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float nextShowHide;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool ProcessItemLoadReady = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DateTime ForceNextItemLoad = DateTime.Now.AddDays(1.0);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool ProcessRegionReady = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DateTime ForceNextRegion = DateTime.Now.AddDays(1.0);

	public static bool testMessage = true;

	public static GameObject Parent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LinkedListNode<DynamicMeshItem> CachedItem;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<PrefabInstance> dynamicPrefabsBuffer = new List<PrefabInstance>();

	public float time;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Texture2D backgroundTexture = null;

	public static float ThreadDistance;

	public static float ObserverDistance;

	public static float ItemLoadDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DynamicMeshManager.DisabledImposterChunkManager disabledImposterChunkManager;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool disabledImposterChunksDirty;

	public List<DynamicMeshUpdateData> UpdateData = new List<DynamicMeshUpdateData>(10);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly int PointerSize = 8;

	public ConcurrentQueue<GameObject> ToBeDestroyed = new ConcurrentQueue<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DateTime nextOrphanCheck;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public HashSet<GameObject> potentialOrphans = new HashSet<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public HashSet<GameObject> checkForOrphans = new HashSet<GameObject>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int dtMinX;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int dtMaxX;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int dtMinZ;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int dtMaxZ;

	public static bool DoLog = false;

	public static bool DoLogNet = false;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ConcurrentQueue<Vector2i> ChunksToRemove = new ConcurrentQueue<Vector2i>();

	public DynamicMeshRegion NearestRegionWithUnloaded;

	public bool FindNearestUnloadedItems;

	public Vector3i? PrimaryLocation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<long, HashSet<long>> RequestKeys = new Dictionary<long, HashSet<long>>();

	public static int ShowHideCheckTime = 3000;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int[] gcGenCount = new int[3];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static string ChunkChangedInItemLoad = "ChunkChangedInItemLoad";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public DateTime NextFallingCheck = DateTime.Now;

	[PublicizedFrom(EAccessModifier.Private)]
	public class DisabledImposterChunkManager : IDisposable
	{
		public DisabledImposterChunkManager(DynamicMeshManager dynamicMeshManager)
		{
			this.dynamicMeshManager = dynamicMeshManager;
			Vector2i worldSize = GameManager.Instance.World.ChunkCache.ChunkProvider.GetWorldSize();
			this.worldChunkDimensions = worldSize.x / 16;
			int num = this.worldChunkDimensions * this.worldChunkDimensions;
			this.chunkClipValues = new ComputeBuffer(num, 4);
			this.chunkClipValuesArray = new int[num];
			Shader.SetGlobalInteger(DynamicMeshManager.DisabledImposterChunkManager.chunkDimensionsID, this.worldChunkDimensions);
			Shader.EnableKeyword(ref DynamicMeshManager.DisabledImposterChunkManager.featureKeyword);
			this.Update();
		}

		public void Update()
		{
			if (this.chunkClipValuesArray != null)
			{
				int num = this.worldChunkDimensions / 2;
				for (int i = 0; i < this.chunkClipValuesArray.Length; i++)
				{
					int num2 = i % this.worldChunkDimensions - num;
					int num3 = i / this.worldChunkDimensions - num;
					long num4 = WorldChunkCache.MakeChunkKey(num2, num3);
					if (this.dynamicMeshManager.ItemsDictionary.ContainsKey(num4))
					{
						this.chunkClipValuesArray[i] = -1;
					}
					else
					{
						this.chunkClipValuesArray[i] = 1;
					}
				}
				this.chunkClipValues.SetData(this.chunkClipValuesArray);
				Shader.SetGlobalBuffer(DynamicMeshManager.DisabledImposterChunkManager.chunkBufferID, this.chunkClipValues);
			}
		}

		public static void DisableShaderKeyword()
		{
			Shader.DisableKeyword(ref DynamicMeshManager.DisabledImposterChunkManager.featureKeyword);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public virtual void Dispose(bool disposing)
		{
			if (!this.disposedValue)
			{
				if (disposing)
				{
					DynamicMeshManager.DisabledImposterChunkManager.DisableShaderKeyword();
					this.chunkClipValues.Dispose();
				}
				this.chunkClipValuesArray = null;
				this.disposedValue = true;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const string ClippingOnKeyword = "PREFAB_CLIPPING_ON";

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly int chunkBufferID = Shader.PropertyToID("_ChunkClipValues");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly int chunkDimensionsID = Shader.PropertyToID("_WorldChunkDimensions");

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly GlobalKeyword featureKeyword = GlobalKeyword.Create("PREFAB_CLIPPING_ON");

		[PublicizedFrom(EAccessModifier.Private)]
		public DynamicMeshManager dynamicMeshManager;

		[PublicizedFrom(EAccessModifier.Private)]
		public ComputeBuffer chunkClipValues;

		[PublicizedFrom(EAccessModifier.Private)]
		public int[] chunkClipValuesArray;

		[PublicizedFrom(EAccessModifier.Private)]
		public int worldChunkDimensions;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool disposedValue;
	}
}
