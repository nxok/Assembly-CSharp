using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

public class VoxelMesh
{
	public VoxelMesh(int _meshIndex, int _minSize = 1024, VoxelMesh.CreateFlags _flags = VoxelMesh.CreateFlags.Default)
	{
		this.meshIndex = _meshIndex;
		this.m_Vertices = new ArrayListMP<Vector3>(MemoryPools.poolVector3, _minSize);
		this.m_Indices = new ArrayListMP<int>(MemoryPools.poolInt, _minSize);
		this.m_Uvs = new ArrayListMP<Vector2>(MemoryPools.poolVector2, _minSize);
		if ((_flags & VoxelMesh.CreateFlags.Cracks) > VoxelMesh.CreateFlags.None)
		{
			this.UvsCrack = new ArrayListMP<Vector2>(MemoryPools.poolVector2, _minSize);
		}
		this.m_Normals = new ArrayListMP<Vector3>(MemoryPools.poolVector3, _minSize);
		this.m_Tangents = new ArrayListMP<Vector4>(MemoryPools.poolVector4, _minSize);
		this.m_ColorVertices = new ArrayListMP<Color>(MemoryPools.poolColor, _minSize);
		if ((_flags & VoxelMesh.CreateFlags.Collider) > VoxelMesh.CreateFlags.None)
		{
			this.m_CollVertices = new ArrayListMP<Vector3>(MemoryPools.poolVector3, _minSize);
			this.m_CollIndices = new ArrayListMP<int>(MemoryPools.poolInt, _minSize);
		}
	}

	public static VoxelMesh Create(int _meshIdx, VoxelMesh.EnumMeshType _meshType, int _minSize = 500)
	{
		if (_meshType == VoxelMesh.EnumMeshType.Terrain)
		{
			return new VoxelMeshTerrain(_meshIdx, _minSize);
		}
		if (_meshType == VoxelMesh.EnumMeshType.Decals)
		{
			return new VoxelMesh(_meshIdx, _minSize, VoxelMesh.CreateFlags.None);
		}
		return new VoxelMesh(_meshIdx, _minSize, VoxelMesh.CreateFlags.Default);
	}

	public void SetTemperature(float _temperature)
	{
		this.temperature = _temperature;
	}

	public virtual void Write(BinaryWriter _bw)
	{
		_bw.Write((uint)this.m_Vertices.Count);
		for (int i = 0; i < this.m_Vertices.Count; i++)
		{
			_bw.Write(this.m_Vertices[i].x);
			_bw.Write(this.m_Vertices[i].y);
			_bw.Write(this.m_Vertices[i].z);
		}
		_bw.Write((uint)this.m_Normals.Count);
		for (int j = 0; j < this.m_Normals.Count; j++)
		{
			_bw.Write(this.m_Normals[j].x);
			_bw.Write(this.m_Normals[j].y);
			_bw.Write(this.m_Normals[j].z);
		}
		_bw.Write((uint)this.m_Uvs.Count);
		for (int k = 0; k < this.m_Uvs.Count; k++)
		{
			_bw.Write(this.m_Uvs[k].x);
			_bw.Write(this.m_Uvs[k].y);
		}
		ArrayListMP<Vector2> uvsCrack = this.UvsCrack;
		int num = ((uvsCrack != null) ? uvsCrack.Count : 0);
		_bw.Write((uint)num);
		for (int l = 0; l < num; l++)
		{
			_bw.Write(this.UvsCrack[l].x);
			_bw.Write(this.UvsCrack[l].y);
		}
		_bw.Write((uint)this.m_ColorVertices.Count);
		for (int m = 0; m < this.m_ColorVertices.Count; m++)
		{
			_bw.Write(this.m_ColorVertices[m].r);
			_bw.Write(this.m_ColorVertices[m].g);
			_bw.Write(this.m_ColorVertices[m].b);
			_bw.Write(this.m_ColorVertices[m].a);
		}
		_bw.Write((uint)this.m_Indices.Count);
		for (int n = 0; n < this.m_Indices.Count; n++)
		{
			_bw.Write(this.m_Indices[n]);
		}
	}

	public virtual void Read(BinaryReader _br)
	{
		uint num = _br.ReadUInt32();
		this.m_Vertices.Clear();
		int num2 = this.m_Vertices.Alloc((int)num);
		int num3 = 0;
		while ((long)num3 < (long)((ulong)num))
		{
			this.m_Vertices[num2++] = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
			num3++;
		}
		num = _br.ReadUInt32();
		this.m_Normals.Clear();
		num2 = this.m_Normals.Alloc((int)num);
		int num4 = 0;
		while ((long)num4 < (long)((ulong)num))
		{
			this.m_Normals[num2++] = new Vector3(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
			num4++;
		}
		num = _br.ReadUInt32();
		this.m_Uvs.Clear();
		num2 = this.m_Uvs.Alloc((int)num);
		int num5 = 0;
		while ((long)num5 < (long)((ulong)num))
		{
			this.m_Uvs[num2++] = new Vector2(_br.ReadSingle(), _br.ReadSingle());
			num5++;
		}
		num = _br.ReadUInt32();
		ArrayListMP<Vector2> uvsCrack = this.UvsCrack;
		if (uvsCrack != null)
		{
			uvsCrack.Clear();
		}
		if (num > 0U)
		{
			num2 = this.UvsCrack.Alloc((int)num);
			int num6 = 0;
			while ((long)num6 < (long)((ulong)num))
			{
				this.UvsCrack[num2++] = new Vector2(_br.ReadSingle(), _br.ReadSingle());
				num6++;
			}
		}
		num = _br.ReadUInt32();
		this.m_ColorVertices.Clear();
		num2 = this.m_ColorVertices.Alloc((int)num);
		int num7 = 0;
		while ((long)num7 < (long)((ulong)num))
		{
			this.m_ColorVertices[num2++] = new Color(_br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle(), _br.ReadSingle());
			num7++;
		}
		num = _br.ReadUInt32();
		this.m_Indices.Clear();
		num2 = this.m_Indices.Alloc((int)num);
		int num8 = 0;
		while ((long)num8 < (long)((ulong)num))
		{
			this.m_Indices[num2++] = _br.ReadInt32();
			num8++;
		}
		this.m_Tangents.Clear();
	}

	public static float BlockFaceToColor(BlockFace _blockFace)
	{
		switch (_blockFace)
		{
		case BlockFace.Top:
			return VoxelMesh.COLOR_TOP;
		case BlockFace.Bottom:
			return VoxelMesh.COLOR_BOTTOM;
		case BlockFace.North:
			return VoxelMesh.COLOR_NORTH;
		case BlockFace.West:
			return VoxelMesh.COLOR_WEST;
		case BlockFace.South:
			return VoxelMesh.COLOR_SOUTH;
		case BlockFace.East:
			return VoxelMesh.COLOR_EAST;
		default:
			return 1f;
		}
	}

	public static void CreateMeshFilter(int _meshIndex, int _yOffset, GameObject _gameObject, string _meshTag, bool _bAllowLOD, out MeshFilter _mf, out MeshRenderer _mr, out MeshCollider _mc)
	{
		VoxelMesh.CreateMeshFilter(_meshIndex, _yOffset, _gameObject, _meshTag, _bAllowLOD, out _mf, out _mr);
		VoxelMesh.CreateMeshCollider(_meshIndex, _gameObject, _meshTag, out _mc);
	}

	public static void CreateMeshFilter(int _meshIndex, int _yOffset, GameObject _gameObject, string _meshTag, bool _bAllowLOD, out MeshFilter _mf, out MeshRenderer _mr)
	{
		_mf = null;
		_mr = null;
		MeshDescription meshDescription = MeshDescription.meshes[_meshIndex];
		_gameObject.transform.localPosition = Vector3.zero;
		if (!string.IsNullOrEmpty(_meshTag))
		{
			_gameObject.tag = _meshTag;
		}
		if (meshDescription.materials != null)
		{
			_mf = _gameObject.GetComponent<MeshFilter>();
			if (_mf == null)
			{
				_mf = _gameObject.AddComponent<MeshFilter>();
			}
			_mr = _gameObject.GetComponent<MeshRenderer>();
			if (_mr == null)
			{
				_mr = _gameObject.AddComponent<MeshRenderer>();
			}
			if (_meshIndex != 5)
			{
				if (meshDescription.materials.Length > 1)
				{
					_mr.sharedMaterials = meshDescription.materials;
				}
				else
				{
					_mr.sharedMaterial = meshDescription.materials[0];
				}
			}
			_mr.receiveShadows = meshDescription.bReceiveShadows;
			_mr.shadowCastingMode = (meshDescription.bCastShadows ? 1 : 0);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateMeshCollider(int _meshIndex, GameObject _gameObject, string _meshTag, out MeshCollider _mc)
	{
		_mc = null;
		MeshDescription meshDescription = MeshDescription.meshes[_meshIndex];
		if (meshDescription.ColliderLayerName != null && meshDescription.ColliderLayerName.Length > 0)
		{
			GameObject gameObject = new GameObject();
			gameObject.name = _gameObject.name + "Collider";
			if (!string.IsNullOrEmpty(_meshTag))
			{
				gameObject.tag = _meshTag;
			}
			gameObject.transform.parent = _gameObject.transform.parent;
			_mc = gameObject.AddComponent<MeshCollider>();
		}
	}

	public static Mesh GetPooledMesh()
	{
		int num = VoxelMesh.meshPool.Count;
		Mesh mesh;
		if (num > 0)
		{
			num--;
			mesh = VoxelMesh.meshPool[num];
			VoxelMesh.meshPool.RemoveAt(num);
		}
		else
		{
			mesh = new Mesh();
			mesh.name = "Pool";
		}
		return mesh;
	}

	public static void AddPooledMesh(Mesh mesh)
	{
		if (!mesh)
		{
			return;
		}
		if (VoxelMesh.meshPool.Count < 250)
		{
			mesh.Clear(false);
			VoxelMesh.meshPool.Add(mesh);
			return;
		}
		Object.Destroy(mesh);
	}

	public virtual int CopyToMesh(MeshFilter _mf, MeshRenderer _mr, int _lodLevel, Action _onCopyComplete = null)
	{
		VoxelMesh.<>c__DisplayClass39_0 CS$<>8__locals1 = new VoxelMesh.<>c__DisplayClass39_0();
		CS$<>8__locals1._mf = _mf;
		CS$<>8__locals1._onCopyComplete = _onCopyComplete;
		CS$<>8__locals1.filterState = VoxelMesh.m_meshFilterStates.GetOrCreateValue(CS$<>8__locals1._mf);
		int count = this.m_Vertices.Count;
		int num;
		if (count == 0)
		{
			VoxelMesh.MeshFilterState filterState = CS$<>8__locals1.filterState;
			VoxelMesh.MeshFilterState filterState2 = CS$<>8__locals1.filterState;
			num = filterState2.NextGeneration;
			filterState2.NextGeneration = num + 1;
			filterState.AppliedGeneration = num;
			Mesh sharedMesh = CS$<>8__locals1._mf.sharedMesh;
			if (sharedMesh)
			{
				CS$<>8__locals1._mf.sharedMesh = null;
				VoxelMesh.AddPooledMesh(sharedMesh);
			}
			Action onCopyComplete = CS$<>8__locals1._onCopyComplete;
			if (onCopyComplete != null)
			{
				onCopyComplete();
			}
			return 0;
		}
		if (count != this.m_ColorVertices.Count)
		{
			Log.Error("ERROR: VM.mesh[{2}].Vertices.Count ({0}) != VM.mesh[{2}].ColorSides.Count ({1})", new object[]
			{
				count,
				this.m_ColorVertices.Count,
				this.meshIndex
			});
			Action onCopyComplete2 = CS$<>8__locals1._onCopyComplete;
			if (onCopyComplete2 != null)
			{
				onCopyComplete2();
			}
			return this.m_Triangles;
		}
		if (count != this.m_Uvs.Count)
		{
			Log.Error("ERROR: VM.mesh.chunkMesh[{2}].Vertices.Count ({0}) != VM.mesh[{2}].Uvs.Count ({1})", new object[]
			{
				count,
				this.m_Uvs.Count,
				this.meshIndex
			});
			Action onCopyComplete3 = CS$<>8__locals1._onCopyComplete;
			if (onCopyComplete3 != null)
			{
				onCopyComplete3();
			}
			return this.m_Triangles;
		}
		VoxelMesh.<>c__DisplayClass39_0 CS$<>8__locals2 = CS$<>8__locals1;
		VoxelMesh.MeshFilterState filterState3 = CS$<>8__locals1.filterState;
		num = filterState3.NextGeneration;
		filterState3.NextGeneration = num + 1;
		CS$<>8__locals2.generation = num;
		CS$<>8__locals1.newMesh = VoxelMesh.GetPooledMesh();
		this.copyToMesh(CS$<>8__locals1.newMesh, this.m_Vertices, this.m_Indices, this.m_Uvs, this.UvsCrack, this.m_Normals, this.m_Tangents, this.m_ColorVertices, delegate
		{
			if (!CS$<>8__locals1._mf || CS$<>8__locals1.generation <= CS$<>8__locals1.filterState.AppliedGeneration)
			{
				VoxelMesh.AddPooledMesh(CS$<>8__locals1.newMesh);
				Action onCopyComplete4 = CS$<>8__locals1._onCopyComplete;
				if (onCopyComplete4 == null)
				{
					return;
				}
				onCopyComplete4();
				return;
			}
			else
			{
				CS$<>8__locals1.filterState.AppliedGeneration = CS$<>8__locals1.generation;
				Mesh sharedMesh2 = CS$<>8__locals1._mf.sharedMesh;
				CS$<>8__locals1._mf.sharedMesh = CS$<>8__locals1.newMesh;
				VoxelMesh.AddPooledMesh(sharedMesh2);
				Action onCopyComplete5 = CS$<>8__locals1._onCopyComplete;
				if (onCopyComplete5 == null)
				{
					return;
				}
				onCopyComplete5();
				return;
			}
		});
		return this.m_Triangles;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void copyToMesh(Mesh _mesh, ArrayListMP<Vector3> _vertices, ArrayListMP<int> _indices, ArrayListMP<Vector2> _uvs, ArrayListMP<Vector2> _uvCracks, ArrayListMP<Vector3> _normals, ArrayListMP<Vector4> _tangents, ArrayListMP<Color> _colorVertices, Action _onCopyComplete = null)
	{
		if (MeshDataManager.Enabled)
		{
			MeshDataManager.Instance.Add(_mesh, _vertices, _indices, _normals, _tangents, _colorVertices, _uvs, _uvCracks, PinnedBuffer.Ownership.Steal, true, true, false, false, _onCopyComplete);
			MeshDataManager.Instance.StartBatches();
			return;
		}
		MeshUnsafeCopyHelper.CopyVertices(_vertices, _mesh);
		if (_uvs.Count > 0)
		{
			MeshUnsafeCopyHelper.CopyUV(_uvs, _mesh);
			if (_uvCracks != null)
			{
				MeshUnsafeCopyHelper.CopyUV2(_uvCracks, _mesh);
			}
		}
		MeshUnsafeCopyHelper.CopyColors(_colorVertices, _mesh);
		MeshUnsafeCopyHelper.CopyTriangles(_indices, _mesh);
		if (_normals.Count == 0)
		{
			_mesh.RecalculateNormals();
		}
		else
		{
			if (_vertices.Count != _normals.Count)
			{
				Log.Error("ERROR: Vertices.Count ({0}) != Normals.Count ({1}), MeshIdx={2} TriIdx={3}", new object[] { _vertices.Count, _normals.Count, this.meshIndex, this.CurTriangleIndex });
			}
			MeshUnsafeCopyHelper.CopyNormals(_normals, _mesh);
		}
		if (_uvs.Count > 0)
		{
			if (_tangents.Count == 0)
			{
				Utils.CalculateMeshTangents(_vertices, _indices, _normals, _uvs, _tangents, _mesh, false);
			}
			if (_vertices.Count != _tangents.Count)
			{
				Log.Out("copyToMesh {0} verts #{1} != tangents #{2}, MeshIdx={3} TriIdx={4}", new object[] { _mesh.name, _vertices.Count, _tangents.Count, this.meshIndex, this.CurTriangleIndex });
			}
			else
			{
				MeshUnsafeCopyHelper.CopyTangents(_tangents, _mesh);
			}
		}
		_mesh.RecalculateUVDistributionMetrics(1E-09f);
		GameUtils.SetMeshVertexAttributes(_mesh, false);
		_mesh.UploadMeshData(false);
		if (_onCopyComplete != null)
		{
			_onCopyComplete();
		}
	}

	public virtual int CopyToColliders(MeshCollider _meshCollider, out Mesh mesh)
	{
		if (this.m_CollIndices == null)
		{
			mesh = null;
			return 0;
		}
		if (this.m_CollIndices.Count == 0)
		{
			GameManager.Instance.World.m_ChunkManager.BakeDestroyCancel(_meshCollider);
			mesh = null;
			return 0;
		}
		mesh = this.ResetMesh(_meshCollider);
		MeshUnsafeCopyHelper.CopyVertices(this.m_CollVertices, mesh);
		MeshUnsafeCopyHelper.CopyTriangles(this.m_CollIndices, mesh);
		return this.m_CollIndices.Count / 3;
	}

	public Mesh ResetMesh(MeshCollider _meshCollider)
	{
		Mesh mesh = GameManager.Instance.World.m_ChunkManager.BakeCancelAndGetMesh(_meshCollider);
		if (!mesh)
		{
			mesh = new Mesh();
		}
		else
		{
			mesh.Clear(false);
		}
		return mesh;
	}

	public virtual void GetColorForTextureId(int _subMeshIdx, ref Transvoxel.BuildVertex _data)
	{
		_data.color = Color.black;
		_data.uv = Vector2.zero;
		_data.uv2 = Vector2.zero;
		_data.uv3 = Vector2.zero;
		_data.uv4 = Vector2.zero;
	}

	public virtual int FindOrCreateSubMesh(int _t0, int _t1, int _t2)
	{
		return 0;
	}

	public virtual void AddIndices(int _i0, int _i1, int _i2, int _submesh)
	{
		this.m_Indices.Add(_i0);
		this.m_Indices.Add(_i1);
		this.m_Indices.Add(_i2);
	}

	public virtual void ClearMesh()
	{
		this.CurTriangleIndex = 0;
		this.m_Vertices.Clear();
		this.m_Indices.Clear();
		this.m_Uvs.Clear();
		if (this.UvsCrack != null)
		{
			this.UvsCrack.Clear();
		}
		if (this.m_Uvs3 != null)
		{
			this.m_Uvs3.Clear();
		}
		if (this.m_Uvs4 != null)
		{
			this.m_Uvs4.Clear();
		}
		this.m_ColorVertices.Clear();
		this.m_Normals.Clear();
		this.m_Tangents.Clear();
		if (this.m_CollVertices != null)
		{
			this.m_CollVertices.Clear();
			this.m_CollIndices.Clear();
		}
	}

	public virtual void SizeToChunkDefaults(int _meshIndex)
	{
		int num;
		int num2;
		int num3;
		VoxelMesh.GetDefaultSizes(_meshIndex, out num, out num2, out num3);
		this.m_Vertices.Grow(num);
		this.m_Indices.Grow(num2);
		this.m_Uvs.Grow(num);
		if (this.UvsCrack != null)
		{
			this.UvsCrack.Grow(num);
		}
		if (this.m_Uvs3 != null)
		{
			this.m_Uvs3.Grow(num);
			this.m_Uvs4.Grow(num);
		}
		this.m_ColorVertices.Grow(num);
		this.m_Normals.Grow(num);
		this.m_Tangents.Grow(num);
		if (this.m_CollVertices != null)
		{
			this.m_CollVertices.Grow(num3);
			this.m_CollIndices.Grow(num3);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void GetDefaultSizes(int _meshIndex, out int _vertMax, out int _indexMax, out int _colliderMax)
	{
		switch (_meshIndex)
		{
		case 0:
			_vertMax = 65536;
			_indexMax = 131072;
			_colliderMax = 32768;
			return;
		case 1:
			_vertMax = 16384;
			_indexMax = 32768;
			_colliderMax = 32768;
			return;
		case 2:
			_vertMax = 4096;
			_indexMax = 8192;
			_colliderMax = 4096;
			return;
		case 3:
			_vertMax = 16384;
			_indexMax = 32768;
			_colliderMax = 8192;
			return;
		case 4:
			_vertMax = 128;
			_indexMax = 128;
			_colliderMax = 0;
			return;
		case 5:
			_vertMax = 1024;
			_indexMax = 1024;
			_colliderMax = 1024;
			return;
		default:
			_vertMax = 512;
			_indexMax = 1024;
			_colliderMax = 512;
			return;
		}
	}

	public virtual void Finished()
	{
		this.m_Triangles = this.m_Indices.Count / 3;
	}

	public virtual Color[] UpdateColors(byte _suncolor, byte _blockcolor)
	{
		for (int i = 0; i < this.m_ColorVertices.Count; i++)
		{
			this.m_ColorVertices[i] = Lighting.ToColor((int)_suncolor, (int)_blockcolor, 1f);
		}
		return this.m_ColorVertices.ToArray();
	}

	public void AddBlockSide(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, BlockValue _blockValue, BlockFace blockFace, Lighting _lighting, int meshIndex)
	{
		Color color = _lighting.ToColor();
		this.AddQuadWithCracks(v1, color, v2, color, v3, color, v4, color, _blockValue.Block.getUVRectFromSideAndMetadata(meshIndex, blockFace, v1, _blockValue), WorldConstants.MapDamageToUVRect(_blockValue), false);
	}

	public void AddBlockSide(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, BlockValue _blockValue, float colorBlockFace, BlockFace blockFace, byte sunlight, byte blocklight, int meshIndex)
	{
		Color color = Lighting.ToColor((int)sunlight, (int)blocklight, colorBlockFace);
		this.AddQuadWithCracks(v1, color, v2, color, v3, color, v4, color, _blockValue.Block.getUVRectFromSideAndMetadata(meshIndex, blockFace, v1, _blockValue), WorldConstants.MapDamageToUVRect(_blockValue), false);
	}

	public void AddBlockSideTri(Vector3 v1, Vector3 v2, Vector3 v3, int _meshIdx, BlockValue _blockValue, float colorBlockFace, BlockFace blockFace, byte sunlight, byte blocklight)
	{
		Color color = Lighting.ToColor((int)sunlight, (int)blocklight, colorBlockFace);
		this.AddTriWithCracks(v1, color, v2, color, v3, color, WorldConstants.MapBlockToUVRect(_meshIdx, _blockValue, blockFace), WorldConstants.MapDamageToUVRect(_blockValue), false);
	}

	public void AddBlockSideTri(Vector3 v1, Color c1, Vector3 v2, Color c2, Vector3 v3, Color c3, Rect uv1, Rect uv2)
	{
		this.AddTriWithCracks(v1, c1, v2, c2, v3, c3, uv1, uv2, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void AddQuad(Vector3 _v0, Vector2 _uv0, Vector3 _v1, Vector2 _uv1, Vector3 _v2, Vector2 _uv3, Vector3 _v3, Vector2 _uv4, byte sunlight, byte blocklight, float sideColor)
	{
		if (this.m_Vertices.Count > 786428)
		{
			return;
		}
		this.m_Vertices.Add(_v0);
		this.m_Vertices.Add(_v1);
		this.m_Vertices.Add(_v2);
		this.m_Vertices.Add(_v3);
		int count = this.m_CollVertices.Count;
		this.m_CollVertices.Add(_v0);
		this.m_CollVertices.Add(_v1);
		this.m_CollVertices.Add(_v2);
		this.m_CollVertices.Add(_v3);
		this.m_Normals.Add(Vector3.Cross(_v3 - _v0, _v1 - _v0).normalized);
		this.m_Normals.Add(Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized);
		this.m_Normals.Add(Vector3.Cross(_v1 - _v2, _v3 - _v2).normalized);
		this.m_Normals.Add(Vector3.Cross(_v2 - _v3, _v0 - _v3).normalized);
		Color color = Lighting.ToColor((int)sunlight, (int)blocklight, sideColor);
		color.a = this.temperature;
		this.m_ColorVertices.Add(color);
		this.m_ColorVertices.Add(color);
		this.m_ColorVertices.Add(color);
		this.m_ColorVertices.Add(color);
		this.m_Indices.Add(this.CurTriangleIndex);
		this.m_Indices.Add(this.CurTriangleIndex + 2);
		this.m_Indices.Add(this.CurTriangleIndex + 1);
		this.m_Indices.Add(this.CurTriangleIndex + 3);
		this.m_Indices.Add(this.CurTriangleIndex + 2);
		this.m_Indices.Add(this.CurTriangleIndex);
		this.m_CollIndices.Add(count);
		this.m_CollIndices.Add(count + 2);
		this.m_CollIndices.Add(count + 1);
		this.m_CollIndices.Add(count + 3);
		this.m_CollIndices.Add(count + 2);
		this.m_CollIndices.Add(count);
		this.m_Uvs.Add(_uv0);
		this.m_Uvs.Add(_uv1);
		this.m_Uvs.Add(_uv3);
		this.m_Uvs.Add(_uv4);
		this.UvsCrack.Add(Vector2.zero);
		this.UvsCrack.Add(Vector2.zero);
		this.UvsCrack.Add(Vector2.zero);
		this.UvsCrack.Add(Vector2.zero);
		this.CurTriangleIndex += 4;
	}

	public void CreateFromQuadList(Vector3[] _vertices, Color _color)
	{
		this.m_Vertices.Add(_vertices[0]);
		this.m_Vertices.Add(_vertices[0] + new Vector3(1f, 0f, 0f));
		this.m_Vertices.Add(_vertices[0] + new Vector3(1f, 0f, 1f));
		this.m_Vertices.Add(_vertices[0] + new Vector3(0f, 0f, 1f));
		this.m_Vertices.Add(_vertices[0] + new Vector3(0.5f, 0.15f, 0.5f));
		this.m_CollVertices.Add(_vertices[0]);
		this.m_CollVertices.Add(_vertices[0] + new Vector3(1f, 0f, 0f));
		this.m_CollVertices.Add(_vertices[0] + new Vector3(1f, 0f, 1f));
		this.m_CollVertices.Add(_vertices[0] + new Vector3(0f, 0f, 1f));
		this.m_CollVertices.Add(_vertices[0] + new Vector3(0.5f, 0.15f, 0.5f));
		this.m_ColorVertices.Add(_color);
		this.m_ColorVertices.Add(_color);
		this.m_ColorVertices.Add(_color);
		this.m_ColorVertices.Add(_color);
		this.m_ColorVertices.Add(_color);
		this.m_Normals.Add(new Vector3(0f, 1f, 0f));
		this.m_Normals.Add(new Vector3(0f, 1f, 0f));
		this.m_Normals.Add(new Vector3(0f, 1f, 0f));
		this.m_Normals.Add(new Vector3(0f, 1f, 0f));
		this.m_Normals.Add(new Vector3(0f, 1f, 0f));
		this.Uvs.Add(Vector2.zero);
		this.Uvs.Add(Vector2.zero);
		this.Uvs.Add(Vector2.zero);
		this.Uvs.Add(Vector2.zero);
		this.Uvs.Add(Vector2.zero);
		this.UvsCrack.Add(Vector2.zero);
		this.UvsCrack.Add(Vector2.zero);
		this.UvsCrack.Add(Vector2.zero);
		this.UvsCrack.Add(Vector2.zero);
		this.UvsCrack.Add(Vector2.zero);
		this.m_Indices.Add(this.CurTriangleIndex + 4);
		this.m_Indices.Add(this.CurTriangleIndex);
		this.m_Indices.Add(this.CurTriangleIndex + 3);
		this.m_CollIndices.Add(this.CurTriangleIndex + 4);
		this.m_CollIndices.Add(this.CurTriangleIndex);
		this.m_CollIndices.Add(this.CurTriangleIndex + 3);
		this.m_Indices.Add(this.CurTriangleIndex + 4);
		this.m_Indices.Add(this.CurTriangleIndex + 1);
		this.m_Indices.Add(this.CurTriangleIndex);
		this.m_CollIndices.Add(this.CurTriangleIndex + 4);
		this.m_CollIndices.Add(this.CurTriangleIndex + 1);
		this.m_CollIndices.Add(this.CurTriangleIndex);
		this.m_Indices.Add(this.CurTriangleIndex + 4);
		this.m_Indices.Add(this.CurTriangleIndex + 2);
		this.m_Indices.Add(this.CurTriangleIndex + 1);
		this.m_CollIndices.Add(this.CurTriangleIndex + 4);
		this.m_CollIndices.Add(this.CurTriangleIndex + 2);
		this.m_CollIndices.Add(this.CurTriangleIndex + 1);
		this.m_Indices.Add(this.CurTriangleIndex + 4);
		this.m_Indices.Add(this.CurTriangleIndex + 3);
		this.m_Indices.Add(this.CurTriangleIndex + 2);
		this.m_CollIndices.Add(this.CurTriangleIndex + 4);
		this.m_CollIndices.Add(this.CurTriangleIndex + 3);
		this.m_CollIndices.Add(this.CurTriangleIndex + 2);
		this.CurTriangleIndex += 5;
	}

	public void AddBasicQuad(Vector3[] _vertices, Color _color, Vector2 _UVdata, bool bForceNormalsUp = false, bool bAlternateWinding = false)
	{
		if (this.m_Vertices.Count > 786428)
		{
			return;
		}
		if (_vertices.Length != 4)
		{
			return;
		}
		Vector3 vector = _vertices[0];
		Vector3 vector2 = _vertices[1];
		Vector3 vector3 = _vertices[2];
		Vector3 vector4 = _vertices[3];
		int num = this.m_Vertices.Alloc(4);
		this.m_Vertices[num] = vector;
		this.m_Vertices[num + 1] = vector2;
		this.m_Vertices[num + 2] = vector3;
		this.m_Vertices[num + 3] = vector4;
		int num2 = this.m_CollVertices.Alloc(4);
		this.m_CollVertices[num] = vector;
		this.m_CollVertices[num + 1] = vector2;
		this.m_CollVertices[num + 2] = vector3;
		this.m_CollVertices[num + 3] = vector4;
		num = this.m_Normals.Alloc(4);
		if (bForceNormalsUp)
		{
			Vector3 up = Vector3.up;
			this.m_Normals[num] = up;
			this.m_Normals[num + 1] = up;
			this.m_Normals[num + 2] = up;
			this.m_Normals[num + 3] = up;
		}
		else
		{
			this.m_Normals[num] = Vector3.Cross(vector4 - vector, vector2 - vector).normalized;
			this.m_Normals[num + 1] = Vector3.Cross(vector - vector2, vector3 - vector2).normalized;
			this.m_Normals[num + 2] = Vector3.Cross(vector2 - vector3, vector4 - vector3).normalized;
			this.m_Normals[num + 3] = Vector3.Cross(vector3 - vector4, vector - vector4).normalized;
		}
		num = this.m_ColorVertices.Alloc(4);
		this.m_ColorVertices[num] = _color;
		this.m_ColorVertices[num + 1] = _color;
		this.m_ColorVertices[num + 2] = _color;
		this.m_ColorVertices[num + 3] = _color;
		num = this.m_Indices.Alloc(6);
		this.m_Indices[num] = (bAlternateWinding ? (this.CurTriangleIndex + 3) : this.CurTriangleIndex);
		this.m_Indices[num + 1] = this.CurTriangleIndex + 2;
		this.m_Indices[num + 2] = this.CurTriangleIndex + 1;
		this.m_Indices[num + 3] = this.CurTriangleIndex + 3;
		this.m_Indices[num + 4] = (bAlternateWinding ? (this.CurTriangleIndex + 1) : (this.CurTriangleIndex + 2));
		this.m_Indices[num + 5] = this.CurTriangleIndex;
		num = this.m_CollIndices.Alloc(6);
		this.m_CollIndices[num] = num2;
		this.m_CollIndices[num + 1] = num2 + 2;
		this.m_CollIndices[num + 2] = num2 + 1;
		this.m_CollIndices[num + 3] = num2 + 3;
		this.m_CollIndices[num + 4] = num2 + 2;
		this.m_CollIndices[num + 5] = num2;
		num = this.m_Uvs.Alloc(4);
		this.m_Uvs.Items[num].x = 0f;
		this.m_Uvs.Items[num].y = 1f;
		this.m_Uvs.Items[++num].x = 1f;
		this.m_Uvs.Items[num].y = 1f;
		this.m_Uvs.Items[++num].x = 1f;
		this.m_Uvs.Items[num].y = 0f;
		this.m_Uvs.Items[++num].x = 0f;
		this.m_Uvs.Items[num].y = 0f;
		num = this.UvsCrack.Alloc(4);
		this.UvsCrack[num] = _UVdata;
		this.UvsCrack[num + 1] = _UVdata;
		this.UvsCrack[num + 2] = _UVdata;
		this.UvsCrack[num + 3] = _UVdata;
		this.CurTriangleIndex += 4;
	}

	public void AddQuadNoCollision(Vector3 _v0, Vector3 _v1, Vector3 _v2, Vector3 _v3, Color _color, Rect _uvTex)
	{
		if (this.m_Vertices.Count > 786428)
		{
			return;
		}
		int num = this.m_Vertices.Alloc(4);
		this.m_Vertices[num] = _v0;
		this.m_Vertices[num + 1] = _v1;
		this.m_Vertices[num + 2] = _v2;
		this.m_Vertices[num + 3] = _v3;
		num = this.m_Normals.Alloc(4);
		this.m_Normals[num] = Vector3.Cross(_v3 - _v0, _v1 - _v0).normalized;
		this.m_Normals[num + 1] = Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized;
		this.m_Normals[num + 2] = Vector3.Cross(_v1 - _v2, _v3 - _v2).normalized;
		this.m_Normals[num + 3] = Vector3.Cross(_v2 - _v3, _v0 - _v3).normalized;
		_color.a = this.temperature;
		num = this.m_ColorVertices.Alloc(4);
		this.m_ColorVertices[num] = _color;
		this.m_ColorVertices[num + 1] = _color;
		this.m_ColorVertices[num + 2] = _color;
		this.m_ColorVertices[num + 3] = _color;
		num = this.m_Indices.Alloc(6);
		this.m_Indices[num] = this.CurTriangleIndex;
		this.m_Indices[num + 1] = this.CurTriangleIndex + 2;
		this.m_Indices[num + 2] = this.CurTriangleIndex + 1;
		this.m_Indices[num + 3] = this.CurTriangleIndex + 3;
		this.m_Indices[num + 4] = this.CurTriangleIndex + 2;
		this.m_Indices[num + 5] = this.CurTriangleIndex;
		num = this.m_Uvs.Alloc(4);
		float x = _uvTex.x;
		float y = _uvTex.y;
		float width = _uvTex.width;
		float height = _uvTex.height;
		this.m_Uvs.Items[num].x = x;
		this.m_Uvs.Items[num].y = y;
		this.m_Uvs.Items[++num].x = x + width;
		this.m_Uvs.Items[num].y = y;
		this.m_Uvs.Items[++num].x = x + width;
		this.m_Uvs.Items[num].y = y + height;
		this.m_Uvs.Items[++num].x = x;
		this.m_Uvs.Items[num].y = y + height;
		this.CurTriangleIndex += 4;
	}

	public void AddQuadWithCracks(Vector3 _v0, Color _c0, Vector3 _v1, Color _c1, Vector3 _v2, Color _c2, Vector3 _v3, Color _c3, Rect _uvTex, Rect _uvOverlay, bool bSwitchUvHorizontal)
	{
		if (this.m_Vertices.Count > 786428)
		{
			return;
		}
		int num = this.m_Vertices.Alloc(4);
		this.m_Vertices[num] = _v0;
		this.m_Vertices[num + 1] = _v1;
		this.m_Vertices[num + 2] = _v2;
		this.m_Vertices[num + 3] = _v3;
		int count = this.m_CollVertices.Count;
		this.m_CollVertices.Add(_v0);
		this.m_CollVertices.Add(_v1);
		this.m_CollVertices.Add(_v2);
		this.m_CollVertices.Add(_v3);
		num = this.m_Normals.Alloc(4);
		this.m_Normals[num] = Vector3.Cross(_v3 - _v0, _v1 - _v0).normalized;
		this.m_Normals[num + 1] = Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized;
		this.m_Normals[num + 2] = Vector3.Cross(_v1 - _v2, _v3 - _v2).normalized;
		this.m_Normals[num + 3] = Vector3.Cross(_v2 - _v3, _v0 - _v3).normalized;
		float num2 = this.temperature;
		_c0.a = num2;
		_c1.a = num2;
		_c2.a = num2;
		_c3.a = num2;
		num = this.m_ColorVertices.Alloc(4);
		this.m_ColorVertices[num] = _c0;
		this.m_ColorVertices[num + 1] = _c1;
		this.m_ColorVertices[num + 2] = _c2;
		this.m_ColorVertices[num + 3] = _c3;
		num = this.m_Indices.Alloc(6);
		this.m_Indices[num] = this.CurTriangleIndex;
		this.m_Indices[num + 1] = this.CurTriangleIndex + 2;
		this.m_Indices[num + 2] = this.CurTriangleIndex + 1;
		this.m_Indices[num + 3] = this.CurTriangleIndex + 3;
		this.m_Indices[num + 4] = this.CurTriangleIndex + 2;
		this.m_Indices[num + 5] = this.CurTriangleIndex;
		this.m_CollIndices.Add(count);
		this.m_CollIndices.Add(count + 2);
		this.m_CollIndices.Add(count + 1);
		this.m_CollIndices.Add(count + 3);
		this.m_CollIndices.Add(count + 2);
		this.m_CollIndices.Add(count);
		num = this.m_Uvs.Alloc(4);
		float num3 = _uvTex.x;
		float num4 = _uvTex.y;
		float num5 = _uvTex.width;
		float num6 = _uvTex.height;
		if (!bSwitchUvHorizontal)
		{
			this.m_Uvs.Items[num].x = num3;
			this.m_Uvs.Items[num].y = num4;
			this.m_Uvs.Items[++num].x = num3 + num5;
			this.m_Uvs.Items[num].y = num4;
			this.m_Uvs.Items[++num].x = num3 + num5;
			this.m_Uvs.Items[num].y = num4 + num6;
			this.m_Uvs.Items[++num].x = num3;
			this.m_Uvs.Items[num].y = num4 + num6;
		}
		else
		{
			this.m_Uvs.Items[num].x = num3 + num5;
			this.m_Uvs.Items[num].y = num4;
			this.m_Uvs.Items[++num].x = num3;
			this.m_Uvs.Items[num].y = num4;
			this.m_Uvs.Items[++num].x = num3;
			this.m_Uvs.Items[num].y = num4 + num6;
			this.m_Uvs.Items[++num].x = num3 + num5;
			this.m_Uvs.Items[num].y = num4 + num6;
		}
		num = this.UvsCrack.Alloc(4);
		num3 = _uvOverlay.x;
		num4 = _uvOverlay.y;
		num5 = _uvOverlay.width;
		num6 = _uvOverlay.height;
		this.UvsCrack.Items[num].x = num3;
		this.UvsCrack.Items[num].y = num4;
		this.UvsCrack.Items[++num].x = num3;
		this.UvsCrack.Items[num].y = num4 + num6;
		this.UvsCrack.Items[++num].x = num3 + num5;
		this.UvsCrack.Items[num].y = num4 + num6;
		this.UvsCrack.Items[++num].x = num3 + num5;
		this.UvsCrack.Items[num].y = num4;
		this.CurTriangleIndex += 4;
	}

	public void AddRectangle(Vector3 _v0, Vector2 _uv0, Color _c0, Vector3 _v1, Vector2 _uv1, Color _c1, Vector3 _v2, Vector2 _uv2, Color _c2, Vector3 _v3, Vector2 _uv3, Color _c3, Rect _uvTex, Rect _uvOverlay)
	{
		if (this.m_Vertices.Count > 786428)
		{
			return;
		}
		this.m_Vertices.Add(_v0);
		this.m_Vertices.Add(_v1);
		this.m_Vertices.Add(_v2);
		this.m_Vertices.Add(_v3);
		int count = this.m_CollVertices.Count;
		this.m_CollVertices.Add(_v0);
		this.m_CollVertices.Add(_v1);
		this.m_CollVertices.Add(_v2);
		this.m_CollVertices.Add(_v3);
		this.m_Normals.Add(Vector3.Cross(_v3 - _v0, _v1 - _v0).normalized);
		this.m_Normals.Add(Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized);
		this.m_Normals.Add(Vector3.Cross(_v1 - _v2, _v3 - _v2).normalized);
		this.m_Normals.Add(Vector3.Cross(_v2 - _v3, _v0 - _v3).normalized);
		float num = this.temperature;
		_c0.a = num;
		_c1.a = num;
		_c2.a = num;
		_c3.a = num;
		this.m_ColorVertices.Add(_c0);
		this.m_ColorVertices.Add(_c1);
		this.m_ColorVertices.Add(_c2);
		this.m_ColorVertices.Add(_c3);
		this.m_Indices.Add(this.CurTriangleIndex);
		this.m_Indices.Add(this.CurTriangleIndex + 2);
		this.m_Indices.Add(this.CurTriangleIndex + 1);
		this.m_Indices.Add(this.CurTriangleIndex + 3);
		this.m_Indices.Add(this.CurTriangleIndex + 2);
		this.m_Indices.Add(this.CurTriangleIndex);
		this.m_CollIndices.Add(count);
		this.m_CollIndices.Add(count + 2);
		this.m_CollIndices.Add(count + 1);
		this.m_CollIndices.Add(count + 3);
		this.m_CollIndices.Add(count + 2);
		this.m_CollIndices.Add(count);
		this.m_Uvs.Add(new Vector2(_uvTex.x + _uv0.x * _uvTex.width, _uvTex.y + _uv0.y * _uvTex.height));
		this.m_Uvs.Add(new Vector2(_uvTex.x + _uv1.x * _uvTex.width, _uvTex.y + _uv1.y * _uvTex.height));
		this.m_Uvs.Add(new Vector2(_uvTex.x + _uv2.x * _uvTex.width, _uvTex.y + _uv2.y * _uvTex.height));
		this.m_Uvs.Add(new Vector2(_uvTex.x + _uv3.x * _uvTex.width, _uvTex.y + _uv3.y * _uvTex.height));
		this.UvsCrack.Add(new Vector2(_uvOverlay.x + _uv0.x * _uvOverlay.width, _uvOverlay.y + _uv0.y * _uvOverlay.height));
		this.UvsCrack.Add(new Vector2(_uvOverlay.x + _uv1.x * _uvOverlay.width, _uvOverlay.y + _uv1.y * _uvOverlay.height));
		this.UvsCrack.Add(new Vector2(_uvOverlay.x + _uv2.x * _uvOverlay.width, _uvOverlay.y + _uv2.y * _uvOverlay.height));
		this.UvsCrack.Add(new Vector2(_uvOverlay.x + _uv3.x * _uvOverlay.width, _uvOverlay.y + _uv3.y * _uvOverlay.height));
		this.CurTriangleIndex += 4;
	}

	public void AddRectangle(Vector3 _v0, Vector2 _uv0, Vector3 _v1, Vector2 _uv1, Vector3 _v2, Vector2 _uv2, Vector3 _v3, Vector2 _uv3, Vector3 _normal, Vector3 _normalTop, Vector4 _tangent, Rect _uvTex, byte _sunlight, byte _blocklight)
	{
		if (this.m_Vertices.Count > 786428)
		{
			return;
		}
		int num = this.m_Vertices.Alloc(4);
		this.m_Vertices[num] = _v0;
		this.m_Vertices[num + 1] = _v1;
		this.m_Vertices[num + 2] = _v2;
		this.m_Vertices[num + 3] = _v3;
		num = this.m_Normals.Alloc(4);
		this.m_Normals[num] = _normal;
		this.m_Normals[num + 1] = _normal;
		this.m_Normals[num + 2] = _normalTop;
		this.m_Normals[num + 3] = _normalTop;
		num = this.m_Tangents.Alloc(4);
		this.m_Tangents[num] = _tangent;
		this.m_Tangents[num + 1] = _tangent;
		this.m_Tangents[num + 2] = _tangent;
		this.m_Tangents[num + 3] = _tangent;
		Color color = Lighting.ToColor((int)_sunlight, (int)_blocklight);
		color.a = this.temperature;
		num = this.m_ColorVertices.Alloc(4);
		this.m_ColorVertices[num] = color;
		this.m_ColorVertices[num + 1] = color;
		this.m_ColorVertices[num + 2] = color;
		this.m_ColorVertices[num + 3] = color;
		num = this.m_Indices.Alloc(6);
		this.m_Indices[num] = this.CurTriangleIndex;
		this.m_Indices[num + 1] = this.CurTriangleIndex + 2;
		this.m_Indices[num + 2] = this.CurTriangleIndex + 1;
		this.m_Indices[num + 3] = this.CurTriangleIndex + 3;
		this.m_Indices[num + 4] = this.CurTriangleIndex + 2;
		this.m_Indices[num + 5] = this.CurTriangleIndex;
		num = this.m_Uvs.Alloc(4);
		float x = _uvTex.x;
		float y = _uvTex.y;
		float width = _uvTex.width;
		float height = _uvTex.height;
		this.m_Uvs.Items[num].x = x + _uv0.x * width;
		this.m_Uvs.Items[num].y = y + _uv0.y * height;
		this.m_Uvs.Items[++num].x = x + _uv1.x * width;
		this.m_Uvs.Items[num].y = y + _uv1.y * height;
		this.m_Uvs.Items[++num].x = x + _uv2.x * width;
		this.m_Uvs.Items[num].y = y + _uv2.y * height;
		this.m_Uvs.Items[++num].x = x + _uv3.x * width;
		this.m_Uvs.Items[num].y = y + _uv3.y * height;
		num = this.UvsCrack.Alloc(4);
		this.UvsCrack[num] = Vector2.zero;
		this.UvsCrack[num + 1] = Vector2.zero;
		this.UvsCrack[num + 2] = Vector2.zero;
		this.UvsCrack[num + 3] = Vector2.zero;
		this.CurTriangleIndex += 4;
	}

	public void AddRectangleColliderPair(Vector3 _v0, Vector3 _v1, Vector3 _v2, Vector3 _v3)
	{
		if (this.m_CollVertices.Count > 786428)
		{
			return;
		}
		int num = this.m_CollVertices.Alloc(4);
		this.m_CollVertices[num] = _v0;
		this.m_CollVertices[num + 1] = _v1;
		this.m_CollVertices[num + 2] = _v2;
		this.m_CollVertices[num + 3] = _v3;
		int num2 = this.m_CollIndices.Alloc(12);
		this.m_CollIndices[num2] = num;
		this.m_CollIndices[num2 + 1] = num + 1;
		this.m_CollIndices[num2 + 2] = num + 2;
		this.m_CollIndices[num2 + 3] = num;
		this.m_CollIndices[num2 + 4] = num + 2;
		this.m_CollIndices[num2 + 5] = num + 3;
		this.m_CollIndices[num2 + 6] = num;
		this.m_CollIndices[num2 + 7] = num + 2;
		this.m_CollIndices[num2 + 8] = num + 1;
		this.m_CollIndices[num2 + 9] = num + 3;
		this.m_CollIndices[num2 + 10] = num + 2;
		this.m_CollIndices[num2 + 11] = num;
	}

	public void AddColoredRectangle(Vector3 _v0, Color _c0, Vector3 _v1, Color _c1, Vector3 _v2, Color _c2, Vector3 _v3, Color _c3)
	{
		if (this.m_Vertices.Count > 786428)
		{
			return;
		}
		this.m_Vertices.Add(_v0);
		this.m_Vertices.Add(_v1);
		this.m_Vertices.Add(_v2);
		this.m_Vertices.Add(_v3);
		int count = this.m_CollVertices.Count;
		this.m_CollVertices.Add(_v0);
		this.m_CollVertices.Add(_v1);
		this.m_CollVertices.Add(_v2);
		this.m_CollVertices.Add(_v3);
		this.m_Normals.Add(Vector3.Cross(_v3 - _v0, _v1 - _v0).normalized);
		this.m_Normals.Add(Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized);
		this.m_Normals.Add(Vector3.Cross(_v1 - _v2, _v3 - _v2).normalized);
		this.m_Normals.Add(Vector3.Cross(_v2 - _v3, _v0 - _v3).normalized);
		float num = this.temperature;
		_c0.a = num;
		_c1.a = num;
		_c2.a = num;
		_c3.a = num;
		this.m_ColorVertices.Add(_c0);
		this.m_ColorVertices.Add(_c1);
		this.m_ColorVertices.Add(_c2);
		this.m_ColorVertices.Add(_c3);
		this.m_Indices.Add(this.CurTriangleIndex);
		this.m_Indices.Add(this.CurTriangleIndex + 2);
		this.m_Indices.Add(this.CurTriangleIndex + 1);
		this.m_Indices.Add(this.CurTriangleIndex + 3);
		this.m_Indices.Add(this.CurTriangleIndex + 2);
		this.m_Indices.Add(this.CurTriangleIndex);
		this.m_CollIndices.Add(count);
		this.m_CollIndices.Add(count + 2);
		this.m_CollIndices.Add(count + 1);
		this.m_CollIndices.Add(count + 3);
		this.m_CollIndices.Add(count + 2);
		this.m_CollIndices.Add(count);
		this.CurTriangleIndex += 4;
	}

	public void AddTriWithCracks(Vector3 _v0, Color _c0, Vector3 _v1, Color _c1, Vector3 _v2, Color _c2, Rect uvTex, Rect uvOverlay, bool bSwitchUvHorizontal)
	{
		if (this.m_Vertices.Count > 786429)
		{
			return;
		}
		this.m_Vertices.Add(_v0);
		this.m_Vertices.Add(_v1);
		this.m_Vertices.Add(_v2);
		int count = this.m_CollVertices.Count;
		this.m_CollVertices.Add(_v0);
		this.m_CollVertices.Add(_v1);
		this.m_CollVertices.Add(_v2);
		this.m_Normals.Add(Vector3.Cross(_v2 - _v0, _v1 - _v0).normalized);
		this.m_Normals.Add(Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized);
		this.m_Normals.Add(Vector3.Cross(_v1 - _v2, _v0 - _v2).normalized);
		float num = this.temperature;
		_c0.a = num;
		_c1.a = num;
		_c2.a = num;
		this.ColorVertices.Add(_c0);
		this.ColorVertices.Add(_c1);
		this.ColorVertices.Add(_c2);
		this.m_Indices.Add(this.CurTriangleIndex);
		this.m_Indices.Add(this.CurTriangleIndex + 2);
		this.m_Indices.Add(this.CurTriangleIndex + 1);
		this.m_CollIndices.Add(count);
		this.m_CollIndices.Add(count + 2);
		this.m_CollIndices.Add(count + 1);
		this.Uvs.Add(new Vector2(uvTex.x + 0f, uvTex.y + 0f));
		if (!bSwitchUvHorizontal)
		{
			this.Uvs.Add(new Vector2(uvTex.x + 0f, uvTex.y + uvTex.height - 0f));
		}
		else
		{
			this.Uvs.Add(new Vector2(uvTex.x + uvTex.width - 0f, uvTex.y + 0f));
		}
		this.Uvs.Add(new Vector2(uvTex.x + uvTex.width - 0f, uvTex.y + uvTex.height - 0f));
		this.UvsCrack.Add(new Vector2(uvOverlay.x + 0f, uvOverlay.y + 0f));
		if (!bSwitchUvHorizontal)
		{
			this.UvsCrack.Add(new Vector2(uvOverlay.x + 0f, uvOverlay.y + uvOverlay.height - 0f));
		}
		else
		{
			this.UvsCrack.Add(new Vector2(uvOverlay.x + uvOverlay.width - 0f, uvOverlay.y + 0f));
		}
		this.UvsCrack.Add(new Vector2(uvOverlay.x + uvOverlay.width - 0f, uvOverlay.y + uvOverlay.height - 0f));
		this.CurTriangleIndex += 3;
	}

	public void AddTriangle(Vector3 _v0, Vector2 _uv0, Color _c0, Vector3 _v1, Vector2 _uv1, Color _c1, Vector3 _v2, Vector2 _uv2, Color _c2, Rect uvTex, Rect uvOverlay)
	{
		if (this.m_Vertices.Count > 786429)
		{
			return;
		}
		this.m_Vertices.Add(_v0);
		this.m_Vertices.Add(_v1);
		this.m_Vertices.Add(_v2);
		int count = this.m_CollVertices.Count;
		this.m_CollVertices.Add(_v0);
		this.m_CollVertices.Add(_v1);
		this.m_CollVertices.Add(_v2);
		this.m_Normals.Add(Vector3.Cross(_v2 - _v0, _v1 - _v0).normalized);
		this.m_Normals.Add(Vector3.Cross(_v0 - _v1, _v2 - _v1).normalized);
		this.m_Normals.Add(Vector3.Cross(_v1 - _v2, _v0 - _v2).normalized);
		float num = this.temperature;
		_c0.a = num;
		_c1.a = num;
		_c2.a = num;
		this.ColorVertices.Add(_c0);
		this.ColorVertices.Add(_c1);
		this.ColorVertices.Add(_c2);
		this.m_Indices.Add(this.CurTriangleIndex);
		this.m_Indices.Add(this.CurTriangleIndex + 2);
		this.m_Indices.Add(this.CurTriangleIndex + 1);
		this.m_CollIndices.Add(count);
		this.m_CollIndices.Add(count + 2);
		this.m_CollIndices.Add(count + 1);
		this.Uvs.Add(new Vector2(uvTex.x + _uv0.x * uvTex.width, uvTex.y + _uv0.y * uvTex.height));
		this.Uvs.Add(new Vector2(uvTex.x + _uv1.x * uvTex.width, uvTex.y + _uv1.y * uvTex.height));
		this.Uvs.Add(new Vector2(uvTex.x + _uv2.x * uvTex.width, uvTex.y + _uv2.y * uvTex.height));
		this.UvsCrack.Add(new Vector2(uvOverlay.x + _uv0.x * uvOverlay.width, uvOverlay.y + _uv0.y * uvOverlay.height));
		this.UvsCrack.Add(new Vector2(uvOverlay.x + _uv1.x * uvOverlay.width, uvOverlay.y + _uv1.y * uvOverlay.height));
		this.UvsCrack.Add(new Vector2(uvOverlay.x + _uv2.x * uvOverlay.width, uvOverlay.y + _uv2.y * uvOverlay.height));
		this.CurTriangleIndex += 3;
	}

	public virtual void AddMesh(Vector3 _drawPos, int _count, Vector3[] _vertices, Vector3[] _normals, ArrayListMP<int> _indices, ArrayListMP<Vector2> _uvs, byte _sunlight, byte _blocklight, VoxelMesh _specialColliders, int damage)
	{
		if (_count + this.m_Vertices.Count > 786432)
		{
			return;
		}
		int curTriangleIndex = this.CurTriangleIndex;
		this.CurTriangleIndex += _count;
		this.m_Vertices.AddRange(_vertices, 0, _count);
		this.m_Normals.AddRange(_normals, 0, _count);
		Color color;
		color..ctor((float)_sunlight / 15f, 0f, 0f, this.temperature);
		int num = this.m_ColorVertices.Alloc(_count);
		for (int i = 0; i < _count; i++)
		{
			this.m_ColorVertices[num + i] = color;
		}
		num = this.m_Indices.Alloc(_indices.Count);
		for (int j = 0; j < _indices.Count; j++)
		{
			this.m_Indices[num + j] = _indices[j] + curTriangleIndex;
		}
		this.m_Uvs.AddRange(_uvs.Items, 0, _uvs.Count);
		Vector2 vector;
		vector..ctor((float)damage, 0f);
		num = this.UvsCrack.Alloc(_uvs.Count);
		for (int k = 0; k < _uvs.Count; k++)
		{
			this.UvsCrack[num + k] = vector;
		}
	}

	public ArrayListMP<int> Indices
	{
		get
		{
			return this.m_Indices;
		}
		set
		{
			this.m_Indices = value;
		}
	}

	public ArrayListMP<Vector2> Uvs
	{
		get
		{
			return this.m_Uvs;
		}
		set
		{
			this.m_Uvs = value;
		}
	}

	public ArrayListMP<Vector2> Uvs3
	{
		get
		{
			return this.m_Uvs3;
		}
		set
		{
			this.m_Uvs3 = value;
		}
	}

	public ArrayListMP<Vector2> Uvs4
	{
		get
		{
			return this.m_Uvs4;
		}
		set
		{
			this.m_Uvs4 = value;
		}
	}

	public ArrayListMP<Vector3> Vertices
	{
		get
		{
			return this.m_Vertices;
		}
		set
		{
			this.m_Vertices = value;
		}
	}

	public ArrayListMP<Vector3> Normals
	{
		get
		{
			return this.m_Normals;
		}
		set
		{
			this.m_Normals = value;
		}
	}

	public ArrayListMP<Vector4> Tangents
	{
		get
		{
			return this.m_Tangents;
		}
		set
		{
			this.m_Tangents = value;
		}
	}

	public ArrayListMP<Color> ColorVertices
	{
		get
		{
			return this.m_ColorVertices;
		}
		set
		{
			this.m_ColorVertices = value;
		}
	}

	public ArrayListMP<int> CollIndices
	{
		get
		{
			return this.m_CollIndices;
		}
	}

	public ArrayListMP<Vector3> CollVertices
	{
		get
		{
			return this.m_CollVertices;
		}
	}

	public int Size
	{
		get
		{
			int num = this.m_Indices.Count * 4 + this.m_Uvs.Count * 8 + this.m_Vertices.Count * 12 + this.m_ColorVertices.Count * 16 + this.m_Normals.Count * 12 + this.m_Tangents.Count * 16;
			if (this.UvsCrack != null)
			{
				num += this.UvsCrack.Count * 8;
			}
			return num;
		}
	}

	public int Triangles
	{
		get
		{
			return this.m_Triangles;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void calculateMeshTangentsDummy(Mesh mesh)
	{
		int num = mesh.vertices.Length;
		Vector4[] array = new Vector4[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = Vector4.one;
		}
		mesh.tangents = array;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void calculateMeshNormalsDummy(Mesh mesh)
	{
		int num = mesh.vertices.Length;
		Vector3[] array = new Vector3[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = Vector3.one;
		}
		mesh.normals = array;
	}

	public void CheckVertexLimit(int _count)
	{
	}

	public void AddRectXYFacingNorth(float _x, float _y, float _z, int _xAdd, int _yAdd)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y + (float)_yAdd, _z));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count);
	}

	public void AddRectXYFacingNorth(float _x, float _y, float _z, int _xAdd, int _yAdd, Color _c)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y + (float)_yAdd, _z));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		_c.a = this.temperature;
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count);
	}

	public void AddRectXYFacingSouth(float _x, float _y, float _z, int _xAdd, int _zAdd)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y + (float)_zAdd, _z));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_zAdd, _z));
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count);
	}

	public void AddRectXYFacingSouth(float _x, float _y, float _z, int _xAdd, int _zAdd, Color _c)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y + (float)_zAdd, _z));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_zAdd, _z));
		_c.a = this.temperature;
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count);
	}

	public void AddRectYZFacingWest(float _x, float _y, float _z, int _yAdd, int _zAdd)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count);
	}

	public void AddRectYZFacingWest(float _x, float _y, float _z, int _yAdd, int _zAdd, Color _c)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		_c.a = this.temperature;
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count);
	}

	public void AddRectYZFacingEast(float _x, float _y, float _z, int _yAdd, int _zAdd)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count);
	}

	public void AddRectYZFacingEast(float _x, float _y, float _z, int _yAdd, int _zAdd, Color _c)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y + (float)_yAdd, _z));
		_c.a = this.temperature;
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count);
	}

	public void AddRectXZFacingUp(float _x, float _y, float _z, int _xAdd, int _zAdd)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count);
	}

	public void AddRectXZFacingUp(float _x, float _y, float _z, int _xAdd, int _zAdd, Color _c)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		_c.a = this.temperature;
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count);
	}

	public void AddRectXZFacingDown(float _x, float _y, float _z, int _xAdd, int _zAdd)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count);
	}

	public void AddRectXZFacingDown(float _x, float _y, float _z, int _xAdd, int _zAdd, Color _c)
	{
		int count = this.m_Vertices.Count;
		this.m_Vertices.Add(new Vector3(_x, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z));
		this.m_Vertices.Add(new Vector3(_x + (float)_xAdd, _y, _z + (float)_zAdd));
		this.m_Vertices.Add(new Vector3(_x, _y, _z + (float)_zAdd));
		_c.a = this.temperature;
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_ColorVertices.Add(_c);
		this.m_Indices.Add(count);
		this.m_Indices.Add(count + 1);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 2);
		this.m_Indices.Add(count + 3);
		this.m_Indices.Add(count);
	}

	public static float GetTemperature(BiomeDefinition bd)
	{
		if (bd == null)
		{
			return -10f;
		}
		return 0f;
	}

	public void ClearTemperatureValues()
	{
		for (int i = 0; i < this.m_ColorVertices.Count; i++)
		{
			this.m_ColorVertices[i] = new Color(this.m_ColorVertices[i].r, this.m_ColorVertices[i].g, this.m_ColorVertices[i].b, 100f);
		}
	}

	public static float COLOR_SOUTH = 0.9f;

	public static float COLOR_WEST = 0.8f;

	public static float COLOR_NORTH = 0.7f;

	public static float COLOR_EAST = 0.85f;

	public static float COLOR_TOP = 1f;

	public static float COLOR_BOTTOM = 0.65f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ConditionalWeakTable<MeshFilter, VoxelMesh.MeshFilterState> m_meshFilterStates = new ConditionalWeakTable<MeshFilter, VoxelMesh.MeshFilterState>();

	public ArrayListMP<Vector3> m_Vertices;

	public ArrayListMP<int> m_Indices;

	public ArrayListMP<Vector2> m_Uvs;

	public ArrayListMP<Vector2> UvsCrack;

	public ArrayListMP<Vector2> m_Uvs3;

	public ArrayListMP<Vector2> m_Uvs4;

	public ArrayListMP<Vector3> m_Normals;

	public ArrayListMP<Vector4> m_Tangents;

	public ArrayListMP<Color> m_ColorVertices;

	public ArrayListMP<Vector3> m_CollVertices;

	public ArrayListMP<int> m_CollIndices;

	public int CurTriangleIndex;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_Triangles;

	public int meshIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cTemperatureDefault = -10f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float temperature;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cPoolMeshMax = 250;

	public static List<Mesh> meshPool = new List<Mesh>();

	public enum EnumMeshType
	{
		Blocks,
		Models,
		Terrain,
		Decals
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public class MeshFilterState
	{
		public int NextGeneration;

		public int AppliedGeneration = -1;
	}

	public enum CreateFlags
	{
		None,
		Collider,
		Cracks,
		Default
	}
}
