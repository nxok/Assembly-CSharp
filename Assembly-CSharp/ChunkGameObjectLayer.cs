using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGameObjectLayer : IMemoryPoolableObject
{
	public ChunkGameObjectLayer()
	{
		int num = MeshDescription.meshes.Length;
		this.m_MeshFilter = new MeshFilter[num];
		this.m_MeshRenderer = new MeshRenderer[num];
		this.m_MeshCollider = new MeshCollider[num];
		this.m_MeshesGO = new GameObject[num];
		this.m_ParentGO = new GameObject("CLayer");
		Transform transform = this.m_ParentGO.transform;
		for (int i = 0; i < num; i++)
		{
			MeshDescription meshDescription = MeshDescription.meshes[i];
			GameObject gameObject = new GameObject(meshDescription.Name);
			this.m_MeshesGO[i] = gameObject;
			gameObject.transform.SetParent(transform, false);
			VoxelMesh.CreateMeshFilter(i, 0, gameObject, meshDescription.Tag, true, out this.m_MeshFilter[i], out this.m_MeshRenderer[i], out this.m_MeshCollider[i]);
		}
		if (OcclusionManager.Instance.cullChunkLayers)
		{
			Occludee.Add(this.m_ParentGO);
		}
		this.m_ParentGO.SetActive(false);
	}

	public void Init(int _chunkLayerIdx, IReadOnlyDictionary<string, int> _layerMappingTable, Transform _chunkT, bool _bStatic)
	{
		for (int i = 0; i < MeshDescription.meshes.Length; i++)
		{
			MeshDescription meshDescription = MeshDescription.meshes[i];
			GameObject gameObject = this.m_MeshesGO[i];
			gameObject.isStatic = _bStatic;
			if (!string.IsNullOrEmpty(meshDescription.MeshLayerName))
			{
				gameObject.layer = _layerMappingTable[meshDescription.MeshLayerName];
			}
			else
			{
				gameObject.layer = 0;
			}
			MeshCollider meshCollider = this.m_MeshCollider[i];
			if (meshCollider)
			{
				if (meshCollider.sharedMesh)
				{
					Log.Warning("ChunkGameObjectLayer Init collider '{0}' should be null", new object[] { meshCollider.sharedMesh.name });
				}
				GameObject gameObject2 = meshCollider.gameObject;
				gameObject2.isStatic = _bStatic;
				gameObject2.layer = _layerMappingTable[meshDescription.ColliderLayerName];
			}
		}
		this.m_ParentGO.name = "CLayer" + _chunkLayerIdx.ToString("00");
		this.m_ParentGO.transform.SetParent(_chunkT, false);
	}

	public void Reset()
	{
		foreach (MeshFilter meshFilter2 in this.m_MeshFilter)
		{
			if (meshFilter2)
			{
				Mesh sharedMesh = meshFilter2.sharedMesh;
				if (sharedMesh)
				{
					meshFilter2.sharedMesh = null;
					VoxelMesh.AddPooledMesh(sharedMesh);
				}
			}
		}
		foreach (MeshCollider meshCollider2 in this.m_MeshCollider)
		{
			if (meshCollider2)
			{
				Mesh sharedMesh2 = meshCollider2.sharedMesh;
				if (sharedMesh2)
				{
					meshCollider2.sharedMesh = null;
					Object.Destroy(sharedMesh2);
				}
			}
		}
	}

	public void Cleanup()
	{
		Span<MeshFilter> span = this.m_MeshFilter.AsSpan<MeshFilter>();
		for (int i = 0; i < span.Length; i++)
		{
			ref MeshFilter ptr = ref span[i];
			if (ptr)
			{
				Mesh sharedMesh = ptr.sharedMesh;
				if (sharedMesh)
				{
					ptr.sharedMesh = null;
					Object.Destroy(sharedMesh);
				}
				GameUtils.NullThenDestroy<MeshFilter>(ref ptr);
			}
		}
		Span<MeshRenderer> span2 = this.m_MeshRenderer.AsSpan<MeshRenderer>();
		for (int i = 0; i < span2.Length; i++)
		{
			ref MeshRenderer ptr2 = ref span2[i];
			if (ptr2)
			{
				GameUtils.NullThenDestroy<MeshRenderer>(ref ptr2);
			}
		}
		Span<MeshCollider> span3 = this.m_MeshCollider.AsSpan<MeshCollider>();
		for (int i = 0; i < span3.Length; i++)
		{
			ref MeshCollider ptr3 = ref span3[i];
			if (ptr3)
			{
				Mesh sharedMesh2 = ptr3.sharedMesh;
				if (sharedMesh2)
				{
					ptr3.sharedMesh = null;
					Object.Destroy(sharedMesh2);
				}
				GameUtils.NullThenDestroy<MeshCollider>(ref ptr3);
			}
		}
		Span<GameObject> span4 = this.m_MeshesGO.AsSpan<GameObject>();
		for (int i = 0; i < span4.Length; i++)
		{
			ref GameObject ptr4 = ref span4[i];
			if (ptr4)
			{
				GameUtils.NullThenDestroy<GameObject>(ref ptr4);
			}
		}
		GameUtils.NullThenDestroy<GameObject>(ref this.m_ParentGO);
	}

	public GameObject m_ParentGO;

	public MeshFilter[] m_MeshFilter;

	public MeshRenderer[] m_MeshRenderer;

	public MeshCollider[] m_MeshCollider;

	public GameObject[] m_MeshesGO;

	public bool isGrassCastShadows;

	public static int InstanceCount;
}
