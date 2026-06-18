using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

public class ChunkPreviewManager
{
	public Prefab Prefab
	{
		get
		{
			return this.PreviewData.PrefabData;
		}
	}

	public Vector3i WorldPosition
	{
		get
		{
			return this.PreviewData.WorldPosition;
		}
	}

	public ChunkPreviewManager()
	{
		ChunkPreviewManager.Instance = this;
		this.PreviewData = new ChunkPreviewData();
		this.PreviewChunksContainer = GameObject.Find("PreviewChunks");
		if (this.PreviewChunksContainer == null)
		{
			this.PreviewChunksContainer = new GameObject("PreviewChunks");
		}
		DynamicMeshThread.SetDefaultThreads();
		this.ThreadData = new DynamicMeshPrefabPreviewThread();
		this.ThreadData.PreviewData = this.PreviewData;
		this.ThreadData.StartThread();
		GameManager.Instance.StartCoroutine(this.LoadPreviewMesh());
	}

	public void AddChunkPreviewLoadData(DynamicMeshVoxelLoad loadData)
	{
		this.ChunkPreviewMeshData.Enqueue(loadData);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator LoadPreviewMesh()
	{
		while (!this.StopRequested)
		{
			DynamicMeshVoxelLoad voxelData;
			if (!this.ChunkPreviewMeshData.TryDequeue(out voxelData))
			{
				yield return null;
			}
			else
			{
				while (voxelData.Item.State == DynamicItemState.Loading)
				{
					Log.Out("delaying load");
					yield return null;
				}
				voxelData.Item.DestroyChunk();
				yield return GameManager.Instance.StartCoroutine(voxelData.Item.CreateMeshFromVoxelCoroutine(true, null, voxelData));
				this.AddPreviewChunk(voxelData);
				if (voxelData.Item.ChunkObject != null)
				{
					voxelData.Item.ChunkObject.transform.parent = this.PreviewChunksContainer.transform;
				}
				voxelData.DisposeMeshes();
				voxelData = null;
			}
		}
		yield break;
	}

	public void ClearAll()
	{
		foreach (DynamicMeshItem dynamicMeshItem in this.Items)
		{
			dynamicMeshItem.DestroyChunk();
		}
		this.Items.Clear();
	}

	public void AddPreviewChunk(DynamicMeshVoxelLoad loadData)
	{
		DynamicMeshItem item = loadData.Item;
		if (this.IsPositionInArea(loadData.Item.WorldPosition))
		{
			this.SetChunkGoVisiblity(item.Key, false);
		}
		else
		{
			this.SetChunkGoVisiblity(item.Key, true);
			loadData.Item.DestroyChunk();
		}
		this.CheckItems();
	}

	public bool IsPositionInArea(Vector3 pos)
	{
		return this.IsPositionInArea(new Vector3i(pos));
	}

	public void ActivationChanged(PrefabInstance pi)
	{
		if (pi == null)
		{
			this.SetWorldPosition(new Vector3i(this.WorldPosition.x, -512, this.WorldPosition.z));
			this.CheckItems();
			return;
		}
		this.SetPrefab(pi.prefab, pi, pi.boundingBoxPosition);
	}

	public bool IsActivePrefab(PrefabInstance pi)
	{
		return pi.prefab == this.PreviewData.PrefabData && this.IsPositionInArea(pi.boundingBoxPosition);
	}

	public bool IsPositionInArea(Vector3i fullposition)
	{
		if (this.WorldPosition.y < -256)
		{
			return false;
		}
		Vector3i chunkPositionFromWorldPosition = DynamicMeshUnity.GetChunkPositionFromWorldPosition(fullposition);
		Vector3i chunkPositionFromWorldPosition2 = DynamicMeshUnity.GetChunkPositionFromWorldPosition(this.PreviewData.WorldPosition);
		Vector3i chunkPositionFromWorldPosition3 = DynamicMeshUnity.GetChunkPositionFromWorldPosition(this.PreviewData.WorldPosition + this.PreviewData.PrefabData.size);
		int x = chunkPositionFromWorldPosition2.x;
		int x2 = chunkPositionFromWorldPosition3.x;
		int z = chunkPositionFromWorldPosition2.z;
		int z2 = chunkPositionFromWorldPosition3.z;
		return chunkPositionFromWorldPosition.x >= x && chunkPositionFromWorldPosition.x <= x2 && chunkPositionFromWorldPosition.z >= z && chunkPositionFromWorldPosition.z <= z2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckItems()
	{
		for (int i = this.Items.Count - 1; i >= 0; i--)
		{
			DynamicMeshItem dynamicMeshItem = this.Items[i];
			if (!this.IsPositionInArea(dynamicMeshItem.WorldPosition))
			{
				this.SetChunkGoVisiblity(dynamicMeshItem.Key, true);
				dynamicMeshItem.DestroyChunk();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetChunkGoVisiblity(long chunkKey, bool isVisible)
	{
		string text = string.Format("Chunk_{0},{1}", DynamicMeshUnity.GetChunkSectionX(chunkKey), DynamicMeshUnity.GetChunkSectionZ(chunkKey));
		foreach (ChunkGameObject chunkGameObject in GameManager.Instance.World.m_ChunkManager.GetUsedChunkGameObjects())
		{
			Transform transform = chunkGameObject.transform;
			if (transform.name == text)
			{
				if (isVisible)
				{
					transform.gameObject.transform.localScale = Vector3.one;
				}
				else
				{
					transform.gameObject.transform.localScale = Vector3.zero;
				}
			}
		}
	}

	public void CleanUp()
	{
		DynamicMeshPrefabPreviewThread threadData = this.ThreadData;
		if (threadData == null)
		{
			return;
		}
		threadData.StopThread();
	}

	public void SetWorldPosition(Vector3i worldPosition)
	{
		this.PreviewData.WorldPosition = worldPosition;
		if (this.Prefab != null)
		{
			int num = DynamicMeshUnity.RoundChunk(worldPosition.x + this.Prefab.size.x) + 16;
			int num2 = DynamicMeshUnity.RoundChunk(worldPosition.z + this.Prefab.size.z) + 16;
			for (int i = worldPosition.x; i <= num; i += 16)
			{
				for (int j = worldPosition.z; j <= num2; j += 16)
				{
					this.StartChunkPreview(new Vector3i(i, 0, j));
				}
			}
		}
		this.CheckItems();
	}

	public void StartChunkPreview(Vector3i chunkPos)
	{
		DynamicMeshItem dynamicMeshItem = this.Get(chunkPos);
		this.ThreadData.AddChunk(dynamicMeshItem);
	}

	public void SetPrefab(PrefabInstance prefab)
	{
		this.SetPrefab(prefab.prefab, prefab, this.WorldPosition);
	}

	public void SetPrefab(Prefab prefab)
	{
		this.SetPrefab(prefab, null, this.WorldPosition);
	}

	public void SetPrefab(Prefab prefab, PrefabInstance instance, Vector3i worldPosition)
	{
		PrefabInstance prefabInstance = this.PreviewData.PrefabInstance;
		if (prefabInstance != null)
		{
			PrefabLODManager.PrefabGameObject instance2 = GameManager.Instance.prefabLODManager.GetInstance(prefabInstance.id);
			if (instance2 != null)
			{
				instance2.go.SetActive(true);
			}
		}
		this.PreviewData.PrefabData = prefab;
		this.PreviewData.PrefabInstance = instance;
		if (instance != null)
		{
			PrefabLODManager.PrefabGameObject instance3 = GameManager.Instance.prefabLODManager.GetInstance(instance.id);
			if (instance3 != null)
			{
				instance3.go.SetActive(false);
			}
		}
		this.SetWorldPosition(worldPosition);
	}

	public DynamicMeshItem Get(Vector3i worldPos)
	{
		worldPos = DynamicMeshUnity.GetChunkPositionFromWorldPosition(worldPos);
		foreach (DynamicMeshItem dynamicMeshItem in this.Items)
		{
			if (dynamicMeshItem.WorldPosition.x == worldPos.x && dynamicMeshItem.WorldPosition.z == worldPos.z)
			{
				return dynamicMeshItem;
			}
		}
		DynamicMeshItem dynamicMeshItem2 = new DynamicMeshItem(worldPos);
		this.Items.Add(dynamicMeshItem2);
		return dynamicMeshItem2;
	}

	public void Update()
	{
		if (this.NextUpdate > DateTime.Now)
		{
			return;
		}
		this.NextUpdate = DateTime.Now.AddSeconds(1.0);
		foreach (DynamicMeshItem dynamicMeshItem in this.Items)
		{
			this.SetChunkGoVisiblity(dynamicMeshItem.Key, dynamicMeshItem.ChunkObject == null || !this.IsPositionInArea(dynamicMeshItem.WorldPosition));
		}
	}

	public static ChunkPreviewManager Instance;

	public List<DynamicMeshItem> Items = new List<DynamicMeshItem>();

	public ChunkPreviewData PreviewData;

	[PublicizedFrom(EAccessModifier.Private)]
	public DynamicMeshPrefabPreviewThread ThreadData;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject PreviewChunksContainer;

	public ConcurrentQueue<DynamicMeshVoxelLoad> ChunkPreviewMeshData = new ConcurrentQueue<DynamicMeshVoxelLoad>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool StopRequested;

	[PublicizedFrom(EAccessModifier.Private)]
	public DateTime NextUpdate = DateTime.Now;
}
