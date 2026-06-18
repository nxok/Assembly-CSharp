using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGameObject : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public ChunkGameObject()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
		this.blockEntitiesParentT = new GameObject("_BlockEntities").transform;
		this.blockEntitiesParentT.SetParent(base.transform, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnEnable()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void OnDisable()
	{
		if (this.chunk != null)
		{
			this.chunk.OnHide();
		}
		if (this.vml != null)
		{
			this.vml.TryFree();
			this.vml = null;
		}
	}

	public void SetStatic(bool _bStatic)
	{
		for (int i = 0; i < this.layers.Length; i++)
		{
			if (this.layers[i] != null)
			{
				for (int j = 0; j < MeshDescription.meshes.Length; j++)
				{
					this.layers[i].m_MeshesGO[j].isStatic = _bStatic;
				}
				for (int k = 0; k < this.layers[i].m_MeshCollider.Length; k++)
				{
					if (this.layers[i].m_MeshCollider[k] != null)
					{
						this.layers[i].m_MeshCollider[k].gameObject.isStatic = _bStatic;
					}
				}
			}
		}
		base.transform.gameObject.isStatic = _bStatic;
	}

	public Chunk GetChunk()
	{
		return this.chunk;
	}

	public void SetChunk(Chunk _chunk, ChunkCluster _chunkCluster)
	{
		if (this.chunk != null && this.chunk != _chunk)
		{
			this.chunk.OnHide();
			Chunk chunk = this.chunk;
			lock (chunk)
			{
				this.chunk.IsCollisionMeshGenerated = false;
				this.chunk.IsDisplayed = false;
			}
		}
		for (int i = 0; i < this.layers.Length; i++)
		{
			ChunkGameObjectLayer chunkGameObjectLayer = this.layers[i];
			if (chunkGameObjectLayer != null)
			{
				chunkGameObjectLayer.m_ParentGO.SetActive(false);
				chunkGameObjectLayer.m_ParentGO.transform.SetParent(null, false);
				MemoryPools.poolCGOL.FreeSync(chunkGameObjectLayer);
				this.layers[i] = null;
			}
		}
		this.chunk = _chunk;
		this.chunkCluster = _chunkCluster;
		Transform transform = base.transform;
		if (this.chunk != null)
		{
			this.chunk.IsDisplayed = true;
			transform.name = _chunk.ToString();
			transform.localPosition = new Vector3((float)(this.chunk.X * 16), 0f, (float)(this.chunk.Z * 16)) - Origin.position;
			GameManager.Instance.StartCoroutine(this.HandleWallVolumes(this.chunk, this.chunkCluster));
			return;
		}
		transform.name = "ChunkEmpty";
		this.RemoveWallVolumes();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RemoveWallVolumes()
	{
		if (this.wallVolumesParentT)
		{
			Object.Destroy(this.wallVolumesParentT.gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator HandleWallVolumes(Chunk _chunk, ChunkCluster _chunkCluster)
	{
		this.RemoveWallVolumes();
		List<int> wallVolumesId = _chunk.GetWallVolumes();
		WorldBase world = _chunkCluster.GetWorld();
		if (wallVolumesId.Count > 0)
		{
			while (!world.HasWallVolumes(wallVolumesId))
			{
				yield return new WaitForSeconds(1f);
			}
			if (!this.wallVolumesParentT)
			{
				this.wallVolumesParentT = new GameObject("_WallVolumes").transform;
				this.wallVolumesParentT.SetParent(base.transform, false);
			}
			Vector3 vector = _chunk.GetWorldPos();
			this.wallVolumes = new GameObject[wallVolumesId.Count];
			for (int i = 0; i < wallVolumesId.Count; i++)
			{
				int num = wallVolumesId[i];
				WallVolume wallVolume = world.GetWallVolume(num);
				GameObject gameObject = new GameObject(num.ToString());
				gameObject.layer = 16;
				Transform transform = gameObject.transform;
				transform.SetParent(this.wallVolumesParentT, false);
				transform.localPosition = wallVolume.Center - vector;
				gameObject.AddComponent<BoxCollider>().size = wallVolume.BoxMax - wallVolume.BoxMin;
				this.wallVolumes[i] = gameObject;
			}
		}
		yield break;
	}

	public int StartCopyMeshLayer()
	{
		this.currentlyCopiedMeshIdx = 0;
		this.isCopyCollidersThisCall = false;
		ChunkGameObjectLayer chunkGameObjectLayer;
		for (;;)
		{
			this.vml = this.chunk.GetMeshLayer();
			if (this.vml == null)
			{
				break;
			}
			chunkGameObjectLayer = this.layers[this.vml.idx];
			if (this.vml.HasContent())
			{
				goto IL_00A4;
			}
			if (chunkGameObjectLayer != null)
			{
				this.layers[this.vml.idx] = null;
				chunkGameObjectLayer.m_ParentGO.SetActive(false);
				chunkGameObjectLayer.m_ParentGO.transform.SetParent(null, false);
				MemoryPools.poolCGOL.FreeSync(chunkGameObjectLayer);
			}
			MemoryPools.poolVML.FreeSync(this.vml);
			this.vml = null;
		}
		return -1;
		IL_00A4:
		if (chunkGameObjectLayer == null)
		{
			chunkGameObjectLayer = MemoryPools.poolCGOL.AllocSync(false);
			chunkGameObjectLayer.Init(this.vml.idx, ChunkCluster.LayerMappingTable, base.transform, base.gameObject.isStatic);
			this.layers[this.vml.idx] = chunkGameObjectLayer;
		}
		this.vml.StartCopyMeshes();
		return this.vml.idx;
	}

	public void EndCopyMeshLayer()
	{
		if (this.vml == null)
		{
			return;
		}
		ChunkGameObjectLayer chunkGameObjectLayer = this.layers[this.vml.idx];
		if (chunkGameObjectLayer != null)
		{
			chunkGameObjectLayer.m_ParentGO.SetActive(true);
			Occludee.Refresh(chunkGameObjectLayer.m_ParentGO);
		}
		this.vml.EndCopyMeshes();
		this.vml = null;
	}

	public bool CreateFromChunkNext(out int _startIdx, out int _endIdx, out int _triangles, out int _colliderTriangles)
	{
		this.nextMS.ResetAndRestart();
		_startIdx = this.currentlyCopiedMeshIdx;
		_triangles = 0;
		_colliderTriangles = 0;
		while (this.currentlyCopiedMeshIdx < MeshDescription.meshes.Length)
		{
			if (!this.isCopyCollidersThisCall && !this.chunk.NeedsOnlyCollisionMesh)
			{
				_triangles += this.copyToMesh(this.currentlyCopiedMeshIdx);
				this.isCopyCollidersThisCall = true;
			}
			else
			{
				_colliderTriangles += this.copyToColliders(this.currentlyCopiedMeshIdx);
				this.isCopyCollidersThisCall = false;
				this.currentlyCopiedMeshIdx++;
			}
			if ((float)this.nextMS.ElapsedMilliseconds >= 0.5f)
			{
				break;
			}
		}
		_endIdx = this.currentlyCopiedMeshIdx - 1;
		return this.currentlyCopiedMeshIdx < MeshDescription.meshes.Length;
	}

	public void CreateMeshAll(out int triangles, out int colliderTriangles)
	{
		triangles = 0;
		colliderTriangles = 0;
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			colliderTriangles += this.copyToColliders(i);
		}
		if (!this.chunk.NeedsOnlyCollisionMesh)
		{
			for (int j = 0; j < MeshDescription.meshes.Length; j++)
			{
				triangles += this.copyToMesh(j);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int copyToMesh(int _meshIdx)
	{
		ChunkGameObjectLayer chunkGameObjectLayer = this.layers[this.vml.idx];
		if (chunkGameObjectLayer == null)
		{
			return 0;
		}
		MeshFilter meshFilter = chunkGameObjectLayer.m_MeshFilter[_meshIdx];
		int num = this.vml.CopyToMesh(_meshIdx, meshFilter, chunkGameObjectLayer.m_MeshRenderer[_meshIdx], 0);
		bool flag = num != 0;
		if (!GameManager.bShowPaintables && _meshIdx == 0)
		{
			flag = false;
		}
		meshFilter.gameObject.SetActive(flag);
		if (num > 0)
		{
			this.CheckLODs(_meshIdx);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int copyToColliders(int _meshIdx)
	{
		ChunkGameObjectLayer chunkGameObjectLayer = this.layers[this.vml.idx];
		if (chunkGameObjectLayer == null)
		{
			return 0;
		}
		MeshCollider meshCollider = chunkGameObjectLayer.m_MeshCollider[_meshIdx];
		if (meshCollider == null)
		{
			return 0;
		}
		Mesh mesh;
		int num = this.vml.meshes[_meshIdx].CopyToColliders(meshCollider, out mesh);
		if (num != 0)
		{
			GameManager.Instance.World.m_ChunkManager.BakeAdd(mesh, meshCollider);
		}
		bool flag = num != 0;
		if (!GameManager.bShowPaintables && _meshIdx == 0)
		{
			flag = false;
		}
		meshCollider.gameObject.SetActive(flag);
		return num;
	}

	public void Cleanup()
	{
		base.gameObject.SetActive(false);
		for (int i = 0; i < this.layers.Length; i++)
		{
			if (this.layers[i] != null)
			{
				this.layers[i].Cleanup();
			}
		}
		for (int j = base.transform.childCount - 1; j >= 0; j--)
		{
			Object.Destroy(base.transform.GetChild(j).gameObject);
		}
		for (int k = 0; k < this.layers.Length; k++)
		{
			if (this.layers[k] != null)
			{
				Object.Destroy(this.layers[k].m_ParentGO);
			}
		}
		this.RemoveWallVolumes();
	}

	public void CheckLODs(int _limitToMesh = -1)
	{
		if (this.chunk == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		if (!primaryPlayer)
		{
			return;
		}
		float num = primaryPlayer.position.x - (float)(this.chunk.X * 16 + 8);
		float num2 = primaryPlayer.position.z - (float)(this.chunk.Z * 16 + 8);
		float num3 = num * num + num2 * num2;
		bool @bool = GamePrefs.GetBool(EnumGamePrefs.OptionsDisableChunkLODs);
		if (_limitToMesh == -1 || _limitToMesh == 4)
		{
			this.SetLOD(4, (@bool || num3 < 1681f) ? 0 : 1);
		}
		if (_limitToMesh == -1 || _limitToMesh == 3)
		{
			float num4 = 48f;
			float num5 = 0f;
			int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxGrassDistance);
			int int2 = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
			switch (@int)
			{
			case 1:
				num4 = 64f;
				break;
			case 2:
				num4 = 96f;
				break;
			case 3:
				num4 = 112f;
				if (int2 >= 3)
				{
					num5 = ((int2 == 3) ? 2.3f : 3.6f) * 16f;
				}
				break;
			}
			bool flag = num3 < num4 * num4;
			bool flag2 = num3 < num5 * num5;
			for (int i = 0; i < this.layers.Length; i++)
			{
				ChunkGameObjectLayer chunkGameObjectLayer = this.layers[i];
				if (chunkGameObjectLayer != null)
				{
					GameObject gameObject = chunkGameObjectLayer.m_MeshesGO[3];
					if (gameObject)
					{
						if (flag)
						{
							if (!gameObject.activeSelf)
							{
								gameObject.SetActive(true);
							}
							if (flag2)
							{
								if (!chunkGameObjectLayer.isGrassCastShadows)
								{
									chunkGameObjectLayer.isGrassCastShadows = true;
									chunkGameObjectLayer.m_MeshRenderer[3].shadowCastingMode = 1;
								}
							}
							else if (chunkGameObjectLayer.isGrassCastShadows)
							{
								chunkGameObjectLayer.isGrassCastShadows = false;
								chunkGameObjectLayer.m_MeshRenderer[3].shadowCastingMode = 0;
							}
						}
						else if (gameObject.activeSelf)
						{
							gameObject.SetActive(false);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLOD(int _meshIdx, int _lodLevel)
	{
		if (_lodLevel == 0)
		{
			for (int i = 0; i < this.layers.Length; i++)
			{
				ChunkGameObjectLayer chunkGameObjectLayer = this.layers[i];
				if (chunkGameObjectLayer != null)
				{
					GameObject gameObject = chunkGameObjectLayer.m_MeshesGO[_meshIdx];
					if (gameObject && !gameObject.activeSelf)
					{
						gameObject.SetActive(true);
					}
				}
			}
			return;
		}
		for (int j = 0; j < this.layers.Length; j++)
		{
			ChunkGameObjectLayer chunkGameObjectLayer2 = this.layers[j];
			if (chunkGameObjectLayer2 != null)
			{
				GameObject gameObject2 = chunkGameObjectLayer2.m_MeshesGO[_meshIdx];
				if (gameObject2 && gameObject2.activeSelf)
				{
					gameObject2.SetActive(false);
				}
			}
		}
	}

	public ChunkGameObjectLayer GetLayer(int _layer)
	{
		return this.layers[_layer];
	}

	public Transform blockEntitiesParentT;

	public static int InstanceCount;

	public Chunk chunk;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public ChunkCluster chunkCluster;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public readonly ChunkGameObjectLayer[] layers = new ChunkGameObjectLayer[16];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public VoxelMeshLayer vml;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int currentlyCopiedMeshIdx;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isCopyCollidersThisCall;

	public Transform wallVolumesParentT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public GameObject[] wallVolumes;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MicroStopwatch nextMS = new MicroStopwatch(false);
}
