using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityDistantTerrainWaterPlane
{
	public UnityDistantTerrainWaterPlane(UnityDistantTerrain.Config _terrainConfig, Material _waterMaterial)
	{
		this.terrainConfig = _terrainConfig;
		this.waterMaterial = _waterMaterial;
	}

	public void createDynamicWaterPlane_Step1(int _xStart, int _zStart, int _tileSize, int _waterChunks16x16Width, byte[] _waterChunks16x16)
	{
		int num = this.terrainConfig.ChunkWorldSize / 16 + 1;
		int num2 = num * num;
		if (this.dynamiceWaterPlaneChunkArr == null || this.dynamiceWaterPlaneChunkArr.Length != num2 * num2)
		{
			this.dynamiceWaterPlaneChunkArr = new int[num2];
		}
		else
		{
			Array.Clear(this.dynamiceWaterPlaneChunkArr, 0, this.dynamiceWaterPlaneChunkArr.Length);
		}
		int num3 = _tileSize / 16;
		int num4 = _tileSize / 16;
		int num5 = _xStart / 16 + _zStart / 16 * _waterChunks16x16Width;
		for (int i = 0; i < num3; i++)
		{
			for (int j = 0; j < num4; j++)
			{
				int num6 = (int)_waterChunks16x16[num5 + i + j * _waterChunks16x16Width];
				if (num6 != 0)
				{
					this.dynamiceWaterPlaneChunkArr[i + j * num] = 16842752 | num6;
				}
			}
		}
		if (this.dynamicWaterPlane == null)
		{
			this.dynamicWaterPlane = new UnityDistantTerrainWaterPlane.DynamicMeshData(num * num);
		}
		else
		{
			this.dynamicWaterPlane.Clear();
		}
		for (int k = 0; k < this.dynamiceWaterPlaneChunkArr.Length; k++)
		{
			int num7 = this.dynamiceWaterPlaneChunkArr[k] >> 16;
			if (num7 != 0)
			{
				int num8 = k % num;
				int num9 = k / num;
				int num10 = num7 >> 8;
				int num11 = num7 & 255;
				int num12 = this.dynamiceWaterPlaneChunkArr[k] & 65535;
				this.dynamicWaterPlane.AddQuad(num8, num12, num9, num10, num11, 16, num);
			}
		}
	}

	public GameObject createDynamicWaterPlane_Step2(GameObject _existingWaterPlane, Transform _parent, string _tag = null)
	{
		if (_existingWaterPlane == null)
		{
			_existingWaterPlane = new GameObject("WaterHi");
			_existingWaterPlane.transform.parent = _parent;
			_existingWaterPlane.transform.localPosition = new Vector3(0f, 0f, 0f);
			_existingWaterPlane.layer = 14;
		}
		if (this.dynamicWaterPlane == null || this.dynamicWaterPlane.vertices.Count == 0)
		{
			return _existingWaterPlane;
		}
		MeshFilter meshFilter = _existingWaterPlane.GetComponent<MeshFilter>();
		if (meshFilter == null)
		{
			meshFilter = _existingWaterPlane.AddComponent<MeshFilter>();
		}
		else
		{
			meshFilter.mesh.Clear();
		}
		Mesh mesh = meshFilter.mesh;
		mesh.SetVertices(this.dynamicWaterPlane.vertices);
		mesh.SetNormals(this.dynamicWaterPlane.normals);
		mesh.SetTangents(this.dynamicWaterPlane.tangents);
		mesh.SetTriangles(this.dynamicWaterPlane.triangles, 0);
		MeshRenderer meshRenderer = _existingWaterPlane.GetComponent<MeshRenderer>();
		if (meshRenderer == null)
		{
			meshRenderer = _existingWaterPlane.AddComponent<MeshRenderer>();
		}
		meshRenderer.material = this.waterMaterial;
		meshRenderer.shadowCastingMode = 0;
		if (_tag != null)
		{
			_existingWaterPlane.tag = _tag;
		}
		this.dynamicWaterPlane.Clear();
		_existingWaterPlane.SetActive(true);
		return _existingWaterPlane;
	}

	public void Cleanup()
	{
		this.dynamicWaterPlane = null;
		this.fixedWaterPlaneHiRes = null;
		this.fixedWaterPlaneLoRes = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cWaterPlaneChunkSize = 16;

	[PublicizedFrom(EAccessModifier.Private)]
	public UnityDistantTerrainWaterPlane.FixedMeshData fixedWaterPlaneHiRes;

	[PublicizedFrom(EAccessModifier.Private)]
	public UnityDistantTerrainWaterPlane.FixedMeshData fixedWaterPlaneLoRes;

	[PublicizedFrom(EAccessModifier.Private)]
	public UnityDistantTerrainWaterPlane.DynamicMeshData dynamicWaterPlane;

	[PublicizedFrom(EAccessModifier.Private)]
	public int[] dynamiceWaterPlaneChunkArr;

	[PublicizedFrom(EAccessModifier.Private)]
	public UnityDistantTerrain.Config terrainConfig;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material waterMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public class FixedMeshData
	{
		public FixedMeshData(int _vertCount, int _triCount)
		{
			this.count = _vertCount;
			this.vertices = new Vector3[this.count];
			this.normals = new Vector3[this.count];
			this.tangents = new Vector4[this.count];
			this.uvs = new Vector2[this.count];
			this.triangles = new int[_triCount];
		}

		public int count;

		public Vector3[] vertices;

		public Vector3[] normals;

		public Vector4[] tangents;

		public Vector2[] uvs;

		public int[] triangles;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class DynamicMeshData
	{
		public DynamicMeshData(int _triCapacity)
		{
			this.vertices = new List<Vector3>(_triCapacity / 2);
			this.normals = new List<Vector3>(_triCapacity / 2);
			this.tangents = new List<Vector4>(_triCapacity / 2);
			this.uvs = new List<Vector2>(_triCapacity / 2);
			this.triangles = new List<int>(_triCapacity);
		}

		public void Clear()
		{
			this.vertices.Clear();
			this.normals.Clear();
			this.tangents.Clear();
			this.uvs.Clear();
			this.triangles.Clear();
		}

		public void AddQuad(int _x, int _y, int _z, int _width, int _height, int _scale, int _totalCountP1)
		{
			int num = this.vertices.Count;
			_x *= _scale;
			_z *= _scale;
			_width *= _scale;
			_height *= _scale;
			this.vertices.Add(new Vector3((float)_x, (float)_y + 0.77f, (float)_z));
			this.vertices.Add(new Vector3((float)(_x + _width), (float)_y + 0.77f, (float)(_z + _height)));
			this.vertices.Add(new Vector3((float)(_x + _width), (float)_y + 0.77f, (float)_z));
			this.normals.Add(Vector3.up);
			this.normals.Add(Vector3.up);
			this.normals.Add(Vector3.up);
			this.tangents.Add(UnityDistantTerrainWaterPlane.DynamicMeshData.defaultWaterPlaneTangent);
			this.tangents.Add(UnityDistantTerrainWaterPlane.DynamicMeshData.defaultWaterPlaneTangent);
			this.tangents.Add(UnityDistantTerrainWaterPlane.DynamicMeshData.defaultWaterPlaneTangent);
			this.triangles.Add(num);
			this.triangles.Add(1 + num);
			this.triangles.Add(2 + num);
			num = this.vertices.Count;
			this.vertices.Add(new Vector3((float)_x, (float)_y + 0.77f, (float)_z));
			this.vertices.Add(new Vector3((float)_x, (float)_y + 0.77f, (float)(_z + _height)));
			this.vertices.Add(new Vector3((float)(_x + _width), (float)_y + 0.77f, (float)(_z + _height)));
			this.normals.Add(Vector3.up);
			this.normals.Add(Vector3.up);
			this.normals.Add(Vector3.up);
			this.tangents.Add(UnityDistantTerrainWaterPlane.DynamicMeshData.defaultWaterPlaneTangent);
			this.tangents.Add(UnityDistantTerrainWaterPlane.DynamicMeshData.defaultWaterPlaneTangent);
			this.tangents.Add(UnityDistantTerrainWaterPlane.DynamicMeshData.defaultWaterPlaneTangent);
			this.triangles.Add(num);
			this.triangles.Add(1 + num);
			this.triangles.Add(2 + num);
		}

		public List<Vector3> vertices;

		public List<Vector3> normals;

		public List<Vector4> tangents;

		public List<Vector2> uvs;

		public List<int> triangles;

		[PublicizedFrom(EAccessModifier.Private)]
		public static Vector4 defaultWaterPlaneTangent = new Vector4(1f, 0f, 0f, 1f);
	}
}
