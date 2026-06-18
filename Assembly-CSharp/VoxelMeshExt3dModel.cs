using System;
using System.IO;
using UnityEngine;

public class VoxelMeshExt3dModel : VoxelMesh
{
	public VoxelMeshExt3dModel(int _meshIndex, int _minSize = 500)
		: base(_meshIndex, _minSize, VoxelMesh.CreateFlags.Default)
	{
	}

	public override void Write(BinaryWriter _bw)
	{
		base.Write(_bw);
		_bw.Write((uint)this.aabb.Length);
		foreach (Bounds bounds in this.aabb)
		{
			BoundsUtils.WriteBounds(_bw, bounds);
		}
		this.boundingBoxMesh.Write(_bw);
		this.lod1Mesh.Write(_bw);
	}

	public override void Read(BinaryReader _br)
	{
		base.Read(_br);
		uint num = _br.ReadUInt32();
		this.aabb = new Bounds[num];
		int num2 = 0;
		while ((long)num2 < (long)((ulong)num))
		{
			this.aabb[num2] = BoundsUtils.ReadBounds(_br);
			num2++;
		}
		this.boundingBoxMesh.ClearMesh();
		this.boundingBoxMesh.Read(_br);
		this.lod1Mesh.ClearMesh();
		this.lod1Mesh.Read(_br);
		this.lod1Mesh.ClearMesh();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addColliders(Vector3 _drawPos, VoxelMesh _colliderMesh)
	{
		if (_colliderMesh.Vertices.Count == 0)
		{
			return;
		}
		int count = this.boundsVertices.Count;
		for (int i = 0; i < _colliderMesh.Vertices.Count; i++)
		{
			this.boundsVertices.Add(_drawPos + _colliderMesh.Vertices[i]);
		}
		for (int j = 0; j < _colliderMesh.Indices.Count; j++)
		{
			this.boundsTriangles.Add(count + _colliderMesh.Indices[j]);
		}
	}

	public override void AddMesh(Vector3 _drawPos, int _count, Vector3[] _vertices, Vector3[] _normals, ArrayListMP<int> _indices, ArrayListMP<Vector2> _uvs, byte _sunlight, byte _blocklight, VoxelMesh _specialColliders, int damage)
	{
		if (_specialColliders != null)
		{
			this.addColliders(_drawPos, _specialColliders);
		}
		base.AddMesh(_drawPos, _count, _vertices, _normals, _indices, _uvs, _sunlight, _blocklight, null, damage);
	}

	public override int CopyToColliders(MeshCollider _meshCollider, out Mesh mesh)
	{
		if (this.boundsVertices == null || this.boundsVertices.Count == 0)
		{
			GameManager.Instance.World.m_ChunkManager.BakeDestroyCancel(_meshCollider);
			mesh = null;
			return 0;
		}
		mesh = base.ResetMesh(_meshCollider);
		MeshUnsafeCopyHelper.CopyVertices(this.boundsVertices, mesh);
		MeshUnsafeCopyHelper.CopyTriangles(this.boundsTriangles, mesh);
		_meshCollider.tag = "T_Mesh_B";
		return this.boundsTriangles.Count / 3;
	}

	public override void ClearMesh()
	{
		base.ClearMesh();
		this.lod1Mesh.ClearMesh();
		this.boundsVertices.Clear();
		this.boundsTriangles.Clear();
	}

	public override void Finished()
	{
		base.Finished();
		this.lod1Mesh.Finished();
	}

	public VoxelMesh boundingBoxMesh = new VoxelMesh(-1, 0, VoxelMesh.CreateFlags.Default);

	public Bounds[] aabb = new Bounds[0];

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayListMP<Vector3> boundsVertices = new ArrayListMP<Vector3>(MemoryPools.poolVector3, 0);

	[PublicizedFrom(EAccessModifier.Private)]
	public ArrayListMP<int> boundsTriangles = new ArrayListMP<int>(MemoryPools.poolInt, 0);

	public VoxelMesh lod1Mesh = new VoxelMesh(-1, 0, VoxelMesh.CreateFlags.Default);
}
