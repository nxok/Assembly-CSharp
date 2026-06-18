using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ConcurrentCollections;
using UniLinq;
using UnityEngine;

public class DynamicMeshRegion : DynamicMeshContainer
{
	public Rect Rect { get; set; }

	public bool RegenRequired { get; set; }

	public int xIndex { get; set; }

	public int zIndex { get; set; }

	public List<PrefabInstance> Instances { get; set; }

	public GameObject RegionObject
	{
		get
		{
			return this._regionObject;
		}
		set
		{
			if (this._regionObject != null)
			{
				if (DynamicMeshManager.DoLog)
				{
					DynamicMeshManager.LogMsg(string.Concat(new string[]
					{
						"Removing old region mesh: ",
						base.ToDebugLocation(),
						" buff: ",
						this.InBuffer.ToString(),
						"  oldPos ",
						this._regionObject.transform.position.ToString(),
						" vs ",
						this._regionObject.transform.position.ToString()
					}));
				}
				DynamicMeshManager.MeshDestroy(this._regionObject);
			}
			this._regionObject = value;
		}
	}

	public bool IsMeshLoaded { get; set; }

	public DynamicMeshRegion(long key)
	{
		Vector3i vector3i = new Vector3i(WorldChunkCache.extractX(key) * 16, 0, WorldChunkCache.extractZ(key) * 16);
		this.WorldPosition = vector3i;
		this.Key = key;
		this.Rect = new Rect((float)vector3i.x, (float)vector3i.z, 160f, 160f);
		this.xIndex = (int)((double)vector3i.x / 160.0);
		this.zIndex = (int)((double)vector3i.z / 160.0);
	}

	public DynamicMeshRegion(Vector3i worldPos)
	{
		this.WorldPosition = DynamicMeshUnity.GetRegionPositionFromWorldPosition(worldPos);
		this.Key = WorldChunkCache.MakeChunkKey(World.toChunkXZ(this.WorldPosition.x), World.toChunkXZ(this.WorldPosition.z));
		this.Rect = new Rect((float)this.WorldPosition.x, (float)this.WorldPosition.z, 160f, 160f);
		this.xIndex = (int)((double)worldPos.x / 160.0);
		this.zIndex = (int)((double)worldPos.z / 160.0);
	}

	public void AddToLoadingQueue(DynamicMeshItem item)
	{
		this.OnLoadingQueue.Add(item);
		if (this.OnLoadingQueue.Count == 0)
		{
			this.SetVisibleNew(false, "LoadingQueueEmpty", true);
		}
	}

	public void RemoveFromLoadingQueue(DynamicMeshItem item)
	{
		this.OnLoadingQueue.Remove(item);
	}

	public override GameObject GetGameObject()
	{
		return this.RegionObject;
	}

	public bool IsInBuffer()
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		return !(primaryPlayer == null) && DynamicMeshUnity.IsInBuffer(primaryPlayer.position.x, primaryPlayer.position.z, DynamicMeshRegion.BufferIndexSize, this.xIndex, this.zIndex);
	}

	public static bool IsInBuffer(int x, int z)
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		return !(primaryPlayer == null) && Math.Abs((int)(primaryPlayer.position.x / 160f) - x / 160) <= DynamicMeshRegion.BufferIndexSize && Math.Abs((int)(primaryPlayer.position.x / 160f) - z / 160) <= DynamicMeshRegion.BufferIndexSize;
	}

	public bool IsInItemLoad()
	{
		if (GameManager.Instance == null)
		{
			return false;
		}
		if (GameManager.Instance.World == null)
		{
			return false;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		return !(primaryPlayer == null) && DynamicMeshUnity.IsInBuffer(primaryPlayer.position.x, primaryPlayer.position.z, DynamicMeshRegion.ItemLoadIndex, this.xIndex, this.zIndex);
	}

	public bool IsInItemLoad(float x, float z)
	{
		return DynamicMeshUnity.IsInBuffer(x, z, DynamicMeshRegion.ItemLoadIndex, this.xIndex, this.zIndex);
	}

	public bool IsInItemUnload()
	{
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		return !(primaryPlayer == null) && !DynamicMeshUnity.IsInBuffer(primaryPlayer.position.x, primaryPlayer.position.z, DynamicMeshRegion.ItemUnloadIndex, this.xIndex, this.zIndex);
	}

	public bool FileExists()
	{
		return SdFile.Exists(this.Path);
	}

	public string Path
	{
		get
		{
			return DynamicMeshFile.MeshLocation + this.Key.ToString() + ".group";
		}
	}

	public static DynamicMeshRegion GetRegionFromWorldPosition(Vector3i worldPos)
	{
		return DynamicMeshManager.Instance.GetRegion(worldPos);
	}

	public static DynamicMeshRegion GetRegionFromWorldPosition(float worldX, float worldZ)
	{
		return DynamicMeshRegion.GetRegionFromWorldPosition((int)worldX, (int)worldZ);
	}

	public static DynamicMeshRegion GetRegionFromWorldPosition(int worldX, int worldZ)
	{
		long regionKeyFromWorldPosition = DynamicMeshUnity.GetRegionKeyFromWorldPosition(worldX, worldZ);
		DynamicMeshRegion dynamicMeshRegion;
		DynamicMeshRegion.Regions.TryGetValue(regionKeyFromWorldPosition, out dynamicMeshRegion);
		return dynamicMeshRegion;
	}

	public bool AddItemToLoadedList(DynamicMeshItem item)
	{
		if (item == null)
		{
			return false;
		}
		for (int i = 0; i < this.UnloadedItems.Count; i++)
		{
			DynamicMeshItem dynamicMeshItem = this.UnloadedItems[i];
			long? num = ((dynamicMeshItem != null) ? new long?(dynamicMeshItem.Key) : null);
			long key = item.Key;
			if ((num.GetValueOrDefault() == key) & (num != null))
			{
				this.UnloadedItems.RemoveAt(i);
				this.LoadedItems.Add(item);
				return true;
			}
		}
		return false;
	}

	public bool HideIfAllLoaded()
	{
		if (this.RegionObject == null)
		{
			return false;
		}
		if (this.UnloadedItems.Count > 0)
		{
			if (this.LoadedItems.Count <= 0 || this.UnloadedItems.Count != 1 || this.UnloadedItems[0].WorldPosition.x != this.WorldPosition.x || this.UnloadedItems[0].WorldPosition.z != this.WorldPosition.z)
			{
				return false;
			}
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Override for single item " + base.ToDebugLocation());
			}
		}
		else
		{
			if (!this.RegionObject.activeSelf)
			{
				return false;
			}
			if (this.LoadedItems.Count == 0)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsVisible()
	{
		return this.RegionObject != null && this.RegionObject.activeSelf;
	}

	public bool AddChunk(int x, int z)
	{
		if (this.HasChunk(x, z))
		{
			return false;
		}
		this.LoadedChunks.Add(new Vector3i(x, 0, z));
		return true;
	}

	public bool AddChunk(Vector3i chunk)
	{
		if (this.HasChunk(chunk.x, chunk.z))
		{
			return false;
		}
		this.LoadedChunks.Add(chunk);
		return true;
	}

	public bool AddThreadedChunk(int x, int z)
	{
		this.AddChunksThreaded.Enqueue(new Vector3i(x, 0, z));
		return true;
	}

	public bool HasChunk(int x, int z)
	{
		for (int i = 0; i < this.LoadedChunks.Count; i++)
		{
			if (this.LoadedChunks[i].x == x && this.LoadedChunks[i].z == z)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasChunkAny(int x, int z)
	{
		for (int i = 0; i < this.LoadedItems.Count; i++)
		{
			Vector3i worldPosition = this.LoadedItems[i].WorldPosition;
			if (worldPosition.x == x && worldPosition.z == z)
			{
				return true;
			}
		}
		for (int j = 0; j < this.UnloadedItems.Count; j++)
		{
			Vector3i worldPosition2 = this.UnloadedItems[j].WorldPosition;
			if (worldPosition2.x == x && worldPosition2.z == z)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsRegionLoadedAndActive(bool doDebug)
	{
		if (DynamicMeshManager.Instance.PrefabCheck == PrefabCheckState.Run)
		{
			return true;
		}
		if (!(this.RegionObject == null) && this.LoadedItems.Count != 0)
		{
			if (!this.LoadedItems.Any((DynamicMeshItem d) => d.State != DynamicItemState.Loaded && d.State != DynamicItemState.ReadyToDelete && d.State != DynamicItemState.Empty))
			{
				return true;
			}
		}
		if (this.UnloadedItems.Count > 0)
		{
			this.LoadItems(true, !this.IsVisible(), true, "IsRegionLoadedAndActive");
			if (this.UnloadedItems.Count == 1)
			{
				DynamicMeshItem dynamicMeshItem = this.UnloadedItems[0];
				if (dynamicMeshItem.WorldPosition.x == this.WorldPosition.x && dynamicMeshItem.WorldPosition.z == this.WorldPosition.z)
				{
					return true;
				}
			}
		}
		if (doDebug)
		{
			if (this.RegionObject == null)
			{
				DynamicMeshRegion.LogMsg("LoadedAndActive Failed: regionObject null on " + base.ToDebugLocation());
			}
			if (this.UnloadedItems.Count > 0)
			{
				DynamicMeshRegion.LogMsg("LoadedAndActive Failed: unloaded items on " + base.ToDebugLocation());
			}
			if (this.LoadedItems.Count == 0)
			{
				DynamicMeshRegion.LogMsg("LoadedAndActive Failed: no loaded items on " + base.ToDebugLocation());
			}
			if (this.LoadedItems.Any((DynamicMeshItem d) => d.State != DynamicItemState.Loaded && d.State != DynamicItemState.Empty))
			{
				DynamicMeshRegion.LogMsg("LoadedAndActive Failed: loaded or empty");
			}
		}
		return false;
	}

	public bool ContainsPrefab(PrefabInstance p)
	{
		return this.Intersects(p.boundingBoxPosition.x, p.boundingBoxPosition.z, p.boundingBoxPosition.x + p.boundingBoxSize.x, p.boundingBoxPosition.z + p.boundingBoxSize.z) || this.Intersects(p.boundingBoxPosition.x, p.boundingBoxPosition.z + p.boundingBoxSize.z, p.boundingBoxPosition.x + p.boundingBoxSize.x, p.boundingBoxPosition.z);
	}

	public bool Intersects(int x1, int y1, int x2, int y2)
	{
		int num = Math.Min(x1, x2);
		int num2 = Math.Max(x1, x2);
		int num3 = Math.Min(y1, y2);
		int num4 = Math.Max(y1, y2);
		if (this.Rect.xMin > (float)num2 || this.Rect.xMax < (float)num)
		{
			return false;
		}
		if (this.Rect.yMin > (float)num4 || this.Rect.yMax < (float)num3)
		{
			return false;
		}
		if (this.Rect.xMin < (float)num && (float)num2 < this.Rect.xMax)
		{
			return true;
		}
		if (this.Rect.yMin < (float)num3 && (float)num4 < this.Rect.yMax)
		{
			return true;
		}
		Func<float, float> func = (float x) => (float)y1 - (x - (float)x1) * (float)((y1 - y2) / (x2 - x1));
		float num5 = func(this.Rect.xMin);
		float num6 = func(this.Rect.xMax);
		return (this.Rect.yMax >= num5 || this.Rect.yMax >= num6) && (this.Rect.yMin <= num5 || this.Rect.yMin <= num6);
	}

	public void OnChunkVisible(DynamicMeshItem item)
	{
		this.VisibleChunks += 1;
		this.ShowItems();
		this.HideRegion("onChunkVisible");
	}

	public void OnChunkUnloaded(DynamicMeshItem item)
	{
		if (this.VisibleChunks > 0)
		{
			this.VisibleChunks -= 1;
		}
		if (this.VisibleChunks == 0)
		{
			bool flag = true;
			string text = "All chunks unloaded on ";
			Vector3i worldPosition = this.WorldPosition;
			this.SetVisibleNew(flag, text + worldPosition.ToString(), true);
			this.HideItems();
		}
	}

	public void SetVisibleNew(bool active, string reason, bool updateItems = true)
	{
		if (this.RegionObject != null && active != this.RegionObject.activeSelf)
		{
			if (active && this.IsPlayerInRegion())
			{
				return;
			}
			if (DynamicMeshManager.DoLog)
			{
				Log.Out(string.Concat(new string[]
				{
					"Changing view state for ",
					base.ToDebugLocation(),
					" to visible: ",
					active.ToString(),
					"      Reason: ",
					reason
				}));
			}
			this.RegionObject.SetActive(active);
			if (DynamicMeshManager.DebugItemPositions)
			{
				this.RegionObject.name = base.ToDebugLocation() + ": " + reason;
			}
		}
	}

	public void HideItems()
	{
		foreach (DynamicMeshItem dynamicMeshItem in this.LoadedItems)
		{
			dynamicMeshItem.SetVisible(false, "Region hide");
		}
	}

	public void ShowItems()
	{
		foreach (DynamicMeshItem dynamicMeshItem in this.LoadedItems)
		{
			dynamicMeshItem.SetVisible(!dynamicMeshItem.IsChunkInGame, "Region show");
		}
		if (this.OnLoadingQueue.Count == 0)
		{
			this.SetVisibleNew(false, "ShowItems HideRegion", true);
		}
	}

	public bool LoadItems(bool urgent, bool visible, bool includeUnloaded, string reason)
	{
		if (!this.IsInItemLoad())
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Load items ignore as outside " + base.ToDebugLocation());
			}
			this.HideItems();
			return false;
		}
		bool flag = false;
		for (int i = 0; i < this.LoadedItems.Count; i++)
		{
			DynamicMeshItem dynamicMeshItem = this.LoadedItems[i];
			if (dynamicMeshItem != null)
			{
				flag = dynamicMeshItem.LoadIfEmpty("region load '" + reason + "'", urgent, this.InBuffer) || flag;
			}
		}
		if (includeUnloaded)
		{
			for (int j = 0; j < this.UnloadedItems.Count; j++)
			{
				DynamicMeshItem dynamicMeshItem2 = this.UnloadedItems[j];
				if (dynamicMeshItem2 != null)
				{
					flag = dynamicMeshItem2.LoadIfEmpty("region load unloaded", urgent, this.InBuffer) || flag;
				}
			}
		}
		return this.UnloadedItems.Count > 0 || flag;
	}

	public void ShowDebug()
	{
		if (DynamicMeshManager.DoLog)
		{
			string text = "Region: ";
			Vector3i vector3i = this.WorldPosition;
			DynamicMeshManager.LogMsg(text + vector3i.ToString() + "  Object: " + ((this.RegionObject == null) ? "null" : this.RegionObject.activeSelf.ToString()));
		}
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("Chunks: " + this.LoadedChunks.Count.ToString());
		}
		foreach (Vector3i vector3i2 in this.LoadedChunks)
		{
			Log.Out(vector3i2.ToString());
		}
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("Items: " + this.LoadedItems.Count.ToString() + " vs Unloaded: " + this.UnloadedItems.Count.ToString());
		}
		foreach (DynamicMeshItem dynamicMeshItem in this.LoadedItems)
		{
			if (DynamicMeshManager.DoLog)
			{
				string[] array = new string[5];
				int num = 0;
				Vector3i vector3i = dynamicMeshItem.WorldPosition;
				array[num] = vector3i.ToString();
				array[1] = "  Object: ";
				array[2] = ((dynamicMeshItem.ChunkObject == null) ? "null" : dynamicMeshItem.ChunkObject.activeSelf.ToString());
				array[3] = "  State: ";
				array[4] = dynamicMeshItem.State.ToString();
				DynamicMeshManager.LogMsg(string.Concat(array));
			}
		}
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg("--unloaded--");
		}
		foreach (DynamicMeshItem dynamicMeshItem2 in this.UnloadedItems)
		{
			if (DynamicMeshManager.DoLog)
			{
				string[] array2 = new string[5];
				int num2 = 0;
				Vector3i vector3i = dynamicMeshItem2.WorldPosition;
				array2[num2] = vector3i.ToString();
				array2[1] = "  Object: ";
				array2[2] = ((dynamicMeshItem2.ChunkObject == null) ? "null" : dynamicMeshItem2.ChunkObject.activeSelf.ToString());
				array2[3] = "  State: ";
				array2[4] = dynamicMeshItem2.State.ToString();
				DynamicMeshManager.LogMsg(string.Concat(array2));
			}
		}
	}

	public void OnCorrupted()
	{
		if (DynamicMeshManager.DoLog)
		{
			string text = "Corrupted region. Adding for regen ";
			Vector3i worldPosition = this.WorldPosition;
			DynamicMeshManager.LogMsg(text + worldPosition.ToString());
		}
		foreach (DynamicMeshItem dynamicMeshItem in this.LoadedItems)
		{
			DynamicMeshManager.Instance.AddChunk(dynamicMeshItem.WorldPosition, true);
		}
	}

	public int Triangles
	{
		get
		{
			int num = 0;
			if (this.RegionObject != null && this.RegionObject.GetComponent<MeshFilter>().mesh.isReadable)
			{
				num += this.RegionObject.GetComponent<MeshFilter>().mesh.triangles.Length;
				foreach (object obj in this.RegionObject.transform)
				{
					Transform transform = (Transform)obj;
					num += transform.gameObject.GetComponent<MeshFilter>().mesh.triangles.Length;
				}
			}
			return num;
		}
	}

	public int Vertices
	{
		get
		{
			int num = 0;
			if (this.RegionObject != null && this.RegionObject.GetComponent<MeshFilter>().mesh.isReadable)
			{
				num += this.RegionObject.GetComponent<MeshFilter>().mesh.vertexCount;
				foreach (object obj in this.RegionObject.transform)
				{
					Transform transform = (Transform)obj;
					num += transform.gameObject.GetComponent<MeshFilter>().mesh.vertexCount;
				}
			}
			return num;
		}
	}

	public int RegionObjects
	{
		get
		{
			int num = 0;
			if (this.RegionObject != null)
			{
				num = this.RegionObject.transform.childCount + 1;
			}
			return num;
		}
	}

	public void SetPosition()
	{
		if (this.RegionObject != null)
		{
			Vector3 vector = this.WorldPosition.ToVector3() - Origin.position;
			if (this.RegionObject.transform.position != vector)
			{
				this.RegionObject.transform.position = vector;
			}
		}
	}

	public void HideRegion(string debugReason)
	{
		this.SetVisibleNew(false, debugReason, true);
		this.ShowItems();
	}

	public void SetViewStats(bool inBuffer, bool shouldLoadItems, bool shouldUnloadItems, bool isOutsideMaxRegionArea)
	{
		this.OutsideLoadArea = isOutsideMaxRegionArea;
		if (shouldLoadItems)
		{
			this.LoadItems(false, true, true, "setViewStatsShouldLoadItem");
		}
		if (this.IsPlayerInRegion())
		{
			this.HideRegion("set view stats all items loaded");
		}
		if (!inBuffer && !isOutsideMaxRegionArea)
		{
			this.SetVisibleNew(true, "SetViewStats visible", true);
		}
		if (this.RegionObject == null && this.FileExists())
		{
			if (isOutsideMaxRegionArea)
			{
				this.SetState(DynamicRegionState.Unloaded, false);
			}
			else if (this.State != DynamicRegionState.StartLoad)
			{
				this.SetState(DynamicRegionState.StartLoad, false);
				DynamicMeshManager.AddRegionLoadMeshes(this.Key);
			}
		}
		if (inBuffer != this.InBuffer)
		{
			this.InBuffer = inBuffer;
			if (inBuffer)
			{
				if (!this.LoadItems(true, true, true, "setViewInBuffer"))
				{
					this.SetState(DynamicRegionState.Loaded, false);
				}
			}
			else
			{
				this.SetVisibleNew(true, "SetViewStats leftBuffer", true);
			}
		}
		if (shouldUnloadItems && this.LoadedItems.Count > 0)
		{
			this.ClearItems();
		}
	}

	public EntityPlayer GetPlayer
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (GameManager.Instance == null)
			{
				return null;
			}
			if (GameManager.Instance.World == null)
			{
				return null;
			}
			if (!GameManager.IsDedicatedServer)
			{
				return GameManager.Instance.World.GetPrimaryPlayer();
			}
			if (GameManager.Instance.World.Players.Count <= 0)
			{
				return null;
			}
			return GameManager.Instance.World.Players.list[0];
		}
	}

	public float DistanceToPlayer()
	{
		EntityPlayer getPlayer = this.GetPlayer;
		if (getPlayer == null)
		{
			return 999999f;
		}
		Vector3 position = getPlayer.position;
		int num = 80;
		return Math.Abs(Mathf.Sqrt(Mathf.Pow(position.x - (float)(this.WorldPosition.x + num), 2f) + Mathf.Pow(position.z - (float)(this.WorldPosition.z + num), 2f)));
	}

	public void SetState(DynamicRegionState newState, bool forceChange)
	{
		if (!forceChange && newState == DynamicRegionState.Unloading && this.State == DynamicRegionState.Unloaded)
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("Can't change state from unloading to unloaded");
			}
			return;
		}
		this.State = newState;
	}

	public void RemoveChunk(int x, int z, string reason, bool removedFromWorld)
	{
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg(string.Concat(new string[]
			{
				"Removing chunk ",
				x.ToString(),
				",",
				z.ToString(),
				": ",
				reason
			}));
		}
		DynamicMeshThread.ChunkDataQueue.MarkForDeletion(DynamicMeshUnity.GetRegionKeyFromWorldPosition(x, z));
		int i = 0;
		while (i < this.UnloadedItems.Count)
		{
			DynamicMeshItem dynamicMeshItem = this.UnloadedItems[i];
			if (dynamicMeshItem != null && dynamicMeshItem.WorldPosition.x == x && dynamicMeshItem.WorldPosition.z == z)
			{
				dynamicMeshItem.DestroyChunk();
				if (removedFromWorld)
				{
					this.UnloadedItems.RemoveAt(i);
					break;
				}
				dynamicMeshItem.State = DynamicItemState.ReadyToDelete;
				break;
			}
			else
			{
				i++;
			}
		}
		int j = 0;
		while (j < this.LoadedItems.Count)
		{
			DynamicMeshItem dynamicMeshItem2 = this.LoadedItems[j];
			if (dynamicMeshItem2 != null && dynamicMeshItem2.WorldPosition.x == x && dynamicMeshItem2.WorldPosition.z == z)
			{
				dynamicMeshItem2.DestroyChunk();
				if (removedFromWorld)
				{
					this.LoadedItems.RemoveAt(j);
					break;
				}
				dynamicMeshItem2.State = DynamicItemState.ReadyToDelete;
				break;
			}
			else
			{
				j++;
			}
		}
		this.LoadedChunks.Remove(new Vector3i(x, 0, z));
	}

	public void AddItem(DynamicMeshItem item)
	{
		int num = 0;
		int num2 = 0;
		try
		{
			num = 1;
			num2 = ((item == null) ? 1 : 0);
			if (item == null)
			{
				Log.Error("null item tried to be added");
			}
			else
			{
				num = 2;
				bool flag = false;
				for (int i = 0; i < this.LoadedItems.Count; i++)
				{
					DynamicMeshItem dynamicMeshItem = this.LoadedItems[i];
					num = 3;
					if (dynamicMeshItem != null)
					{
						num = 4;
						num2 = ((item == null) ? 1 : 0) + ((dynamicMeshItem == null) ? 10 : 0);
						if (dynamicMeshItem.Key == item.Key)
						{
							flag = true;
							break;
						}
					}
				}
				if (!flag)
				{
					num = 5;
					for (int j = 0; j < this.UnloadedItems.Count; j++)
					{
						num = 6;
						DynamicMeshItem dynamicMeshItem2 = this.UnloadedItems[j];
						num2 = ((item == null) ? 1 : 0);
						if (dynamicMeshItem2 != null)
						{
							num = 7;
							num2 = ((item == null) ? 1 : 0) + ((dynamicMeshItem2 == null) ? 10 : 0);
							if (dynamicMeshItem2.Key == item.Key)
							{
								flag = true;
								break;
							}
						}
					}
				}
				if (!flag)
				{
					num = 8;
					if (GameManager.IsDedicatedServer)
					{
						num = 9;
						this.LoadedItems.Add(item);
					}
					else
					{
						num = 10;
						this.UnloadedItems.Add(item);
					}
				}
				num = 11;
			}
		}
		catch (Exception)
		{
			Log.Error("Add Item error at stage: " + num.ToString() + " nulls: " + num2.ToString());
		}
	}

	public int GetStreamLength()
	{
		int num = 8 + this.LoadedChunks.Distinct<Vector3i>().Count<Vector3i>() * 8 + 8 + 12 + 1 + 4;
		return Math.Max(10240, num);
	}

	public void CleanUp()
	{
		if (this.RegionObject != null)
		{
			DynamicMeshManager.MeshDestroy(this.RegionObject);
			this.RegionObject = null;
		}
		this.ClearMeshes();
		this.State = DynamicRegionState.Unloaded;
		this.IsMeshLoaded = false;
	}

	public bool IsPlayerInRegion()
	{
		EntityPlayerLocal player = DynamicMeshManager.player;
		return !(player == null) && DynamicMeshManager.Instance.GetRegion((int)player.position.x, (int)player.position.z).WorldPosition == this.WorldPosition;
	}

	public void DistanceChecks()
	{
		float num = this.DistanceToPlayer();
		bool flag = this.IsInBuffer();
		bool flag2 = this.IsInItemLoad();
		bool flag3 = this.IsInItemUnload();
		bool flag4 = num >= (float)DynamicMeshSettings.MaxViewDistance || DynamicMeshManager.IsOutsideDistantTerrain(this);
		this.SetPosition();
		this.SetViewStats(flag, flag2, flag3, flag4);
		if (this.State == DynamicRegionState.Unloaded && num < (float)DynamicMeshSettings.MaxViewDistance && this.RegionObject == null)
		{
			bool outsideLoadArea = this.OutsideLoadArea;
		}
		if (!(this.RegionObject == null) || this.State == DynamicRegionState.Unloaded)
		{
		}
		if (this.RegionObject != null && flag4 && DynamicMeshFile.CurrentlyLoadingRegionPosition != this.WorldPosition)
		{
			this.CleanUp();
		}
	}

	public static void LogMsg(string msg)
	{
		if (DynamicMeshManager.DoLog)
		{
			DynamicMeshManager.LogMsg(msg);
		}
	}

	public void ClearItems()
	{
		foreach (DynamicMeshItem dynamicMeshItem in this.LoadedItems)
		{
			dynamicMeshItem.CleanUp();
			this.UnloadedItems.Add(dynamicMeshItem);
		}
		this.LoadedItems.Clear();
		this.SetVisibleNew(true, "itemsUnloaded", true);
	}

	public void ClearMeshes()
	{
	}

	public static ConcurrentDictionary<long, DynamicMeshRegion> Regions = new ConcurrentDictionary<long, DynamicMeshRegion>();

	public static int BufferIndexSize = 1;

	public static int ItemLoadIndex = 3;

	public static int ItemUnloadIndex = DynamicMeshRegion.ItemLoadIndex + 1;

	public byte VisibleChunks;

	public List<DynamicMeshItem> LoadedItems = new List<DynamicMeshItem>();

	public List<DynamicMeshItem> UnloadedItems = new List<DynamicMeshItem>();

	public HashSet<DynamicMeshItem> OnLoadingQueue = new HashSet<DynamicMeshItem>();

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject _regionObject;

	public ConcurrentQueue<Vector3i> AddChunksThreaded = new ConcurrentQueue<Vector3i>();

	public ConcurrentHashSet<Vector3i> LoadedChunksThreaded = new ConcurrentHashSet<Vector3i>();

	public List<Vector3i> LoadedChunks = new List<Vector3i>();

	public DateTime CreateDate = DateTime.Now;

	public DateTime NextLoadTime = DateTime.Now;

	public DynamicRegionState State;

	public bool InBuffer;

	public bool FastTrackLoaded;

	public bool MarkedForDeletion;

	public bool OutsideLoadArea = true;

	public bool IsThreadedRegion;
}
