using System;
using System.Collections.Generic;
using UnityEngine;

public class MeshLists
{
	public static MeshLists GetList()
	{
		if (MeshLists.MeshListCache.Count == 0)
		{
			return new MeshLists();
		}
		return MeshLists.MeshListCache.Pop();
	}

	public static void ReturnList(MeshLists list)
	{
		list.Reset();
		MeshLists.MeshListCache.Push(list);
		if (MeshLists.MeshListCache.Count > MeshLists.LastLargest)
		{
			MeshLists.LastLargest = MeshLists.MeshListCache.Count;
			Log.Out("Meshlist count is now " + MeshLists.MeshListCache.Count.ToString());
		}
	}

	public void Reset()
	{
		this.Vertices.Clear();
		this.Uvs.Clear();
		this.Uvs2.Clear();
		this.Uvs3.Clear();
		this.Uvs4.Clear();
		this.Triangles.Clear();
		foreach (List<int> list in this.TerrainTriangles)
		{
			list.Clear();
		}
		this.TerrainTriangles.Clear();
		this.Colours.Clear();
		this.Normals.Clear();
		this.Tangents.Clear();
	}

	public static Stack<MeshLists> MeshListCache = new Stack<MeshLists>();

	public static int LastLargest = 0;

	public List<Vector3> Vertices = new List<Vector3>();

	public List<Vector2> Uvs = new List<Vector2>();

	public List<Vector2> Uvs2 = new List<Vector2>();

	public List<Vector2> Uvs3 = new List<Vector2>();

	public List<Vector2> Uvs4 = new List<Vector2>();

	public List<int> Triangles = new List<int>();

	public List<List<int>> TerrainTriangles = new List<List<int>>();

	public List<Color> Colours = new List<Color>();

	public List<Vector3> Normals = new List<Vector3>();

	public List<Vector4> Tangents = new List<Vector4>();
}
