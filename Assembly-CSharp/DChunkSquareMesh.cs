using System;
using UnityEngine;

public class DChunkSquareMesh
{
	public DChunkSquareMesh(DistantChunkMap DCMap, int LODLevel)
	{
		DistantChunkMapInfo distantChunkMapInfo = DCMap.ChunkMapInfoArray[LODLevel];
		this.Init(distantChunkMapInfo.BaseMesh.Vertices.Length, LODLevel, distantChunkMapInfo.ChunkResolution, distantChunkMapInfo.ColliderResolution, DCMap.NbResLevel, distantChunkMapInfo.BaseMesh.Triangles.Length);
	}

	public void Init(int NbVertices, int ResLevel, int Resolution, int ColliderResolution, int MaxNbResLevel, int NbTriangles)
	{
		this.Normals = new Vector3[NbVertices];
		this.Tangents = new Vector4[NbVertices];
		this.EdgeCorNormals = new Vector3[Resolution * 4];
		this.Colors = new Color[NbVertices];
		this.TextureId = new int[NbVertices];
		this.IsWater = new bool[NbVertices];
		this.ChunkBound = default(Bounds);
		this.ColVertices = new Vector3[ColliderResolution * ColliderResolution];
		this.ColVerticesHeight = new float[ColliderResolution * ColliderResolution];
	}

	public Vector3[] Normals;

	public Vector4[] Tangents;

	public Vector3[] EdgeCorNormals;

	public Color[] Colors;

	public int[] TextureId;

	public Bounds ChunkBound;

	public int WaterPlaneBlockId;

	public Vector3[] ColVertices;

	public float[] ColVerticesHeight;

	public VoxelMeshTerrain VoxelMesh = new VoxelMeshTerrain(0, 500);

	public bool[] IsWater;
}
