using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DynamicMeshVoxelRegionLoad
{
	public Vector3i WorldPosition { get; set; }

	public List<VoxelMesh> Meshes { get; set; }

	public static DynamicMeshVoxelRegionLoad Create(Vector3i position, List<VoxelMesh> meshes)
	{
		return new DynamicMeshVoxelRegionLoad
		{
			WorldPosition = position,
			Meshes = meshes
		};
	}

	public IEnumerator CreateMeshCoroutine(DynamicMeshRegion region)
	{
		DateTime start = DateTime.Now;
		GameObject newRegionObject = DynamicMeshItem.GetRegionMeshRendererFromPool();
		newRegionObject.transform.position = new Vector3(0f, 0f, 0f);
		newRegionObject.SetActive(false);
		start = DateTime.Now;
		foreach (VoxelMesh voxelMesh in this.Meshes)
		{
			MeshTiming meshTiming = new MeshTiming();
			if (voxelMesh is VoxelMeshTerrain)
			{
				this.CreateTerrainGo(voxelMesh as VoxelMeshTerrain, newRegionObject, meshTiming);
				if ((DateTime.Now - start).TotalMilliseconds > 3.0)
				{
					start = DateTime.Now;
					yield return null;
				}
			}
			else
			{
				yield return DynamicMeshManager.Instance.StartCoroutine(this.CreateOpaqueGo(voxelMesh, newRegionObject, meshTiming));
				start = DateTime.Now;
			}
		}
		List<VoxelMesh>.Enumerator enumerator = default(List<VoxelMesh>.Enumerator);
		region.RegionObject = newRegionObject;
		region.IsMeshLoaded = true;
		region.SetPosition();
		region.SetVisibleNew(region.VisibleChunks == 0 || !region.InBuffer, "vox region coroutine finished", true);
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator CreateOpaqueGo(VoxelMesh vm, GameObject parent, MeshTiming time)
	{
		if (vm.Vertices.Count > 0)
		{
			GameObject itemMeshRendererFromPool = DynamicMeshItem.GetItemMeshRendererFromPool();
			itemMeshRendererFromPool.name = "O";
			itemMeshRendererFromPool.transform.parent = parent.transform;
			itemMeshRendererFromPool.transform.localPosition = Vector3.zero;
			MeshFilter component = itemMeshRendererFromPool.GetComponent<MeshFilter>();
			Mesh mesh;
			if (component.sharedMesh == null)
			{
				mesh = new Mesh();
			}
			else
			{
				mesh = component.sharedMesh;
			}
			mesh.indexFormat = ((vm.Vertices.Count < 65535) ? 0 : 1);
			component.sharedMesh = mesh;
			yield return this.copyToMesh(mesh, vm.Vertices, vm.Indices, vm.Uvs, vm.UvsCrack, vm.Normals, vm.Tangents, vm.ColorVertices, time);
		}
		yield break;
	}

	public IEnumerator copyToMesh(Mesh _mesh, ArrayListMP<Vector3> _vertices, ArrayListMP<int> _indices, ArrayListMP<Vector2> _uvs, ArrayListMP<Vector2> _uvCracks, ArrayListMP<Vector3> _normals, ArrayListMP<Vector4> _tangents, ArrayListMP<Color> _colorVertices, MeshTiming time)
	{
		DateTime start = DateTime.Now;
		time.Reset();
		MeshUnsafeCopyHelper.CopyVertices(_vertices, _mesh);
		time.CopyVerts = time.GetTime();
		if ((DateTime.Now - start).TotalMilliseconds > 3.0)
		{
			start = DateTime.Now;
			yield return null;
		}
		if (_uvs.Count > 0)
		{
			time.Reset();
			MeshUnsafeCopyHelper.CopyUV(_uvs, _mesh);
			time.CopyUv = time.GetTime();
			if ((DateTime.Now - start).TotalMilliseconds > 3.0)
			{
				start = DateTime.Now;
				yield return null;
			}
			time.Reset();
			if (_uvCracks.Items != null)
			{
				MeshUnsafeCopyHelper.CopyUV2(_uvCracks, _mesh);
			}
			time.CopyUv2 = time.GetTime();
			if ((DateTime.Now - start).TotalMilliseconds > 3.0)
			{
				start = DateTime.Now;
				yield return null;
			}
		}
		time.Reset();
		MeshUnsafeCopyHelper.CopyColors(_colorVertices, _mesh);
		time.CopyColours = time.GetTime();
		if ((DateTime.Now - start).TotalMilliseconds > 3.0)
		{
			start = DateTime.Now;
			yield return null;
		}
		time.Reset();
		MeshUnsafeCopyHelper.CopyTriangles(_indices, _mesh);
		time.CopyTriangles = time.GetTime();
		if ((DateTime.Now - start).TotalMilliseconds > 3.0)
		{
			start = DateTime.Now;
			yield return null;
		}
		if (_normals.Count == 0)
		{
			Log.Error(this.WorldPosition.ToString() + " mesh normals zero on opaque");
			_mesh.RecalculateNormals();
		}
		else
		{
			if (_vertices.Count != _normals.Count)
			{
				Log.Error("ERROR: Vertices.Count ({0}) != Normals.Count ({1})", new object[] { _vertices.Count, _normals.Count });
			}
			time.Reset();
			MeshUnsafeCopyHelper.CopyNormals(_normals, _mesh);
			time.CopyNormals = time.GetTime();
			if ((DateTime.Now - start).TotalMilliseconds > 3.0)
			{
				start = DateTime.Now;
				yield return null;
			}
		}
		if (_uvs.Count > 0)
		{
			if (_tangents.Count == 0)
			{
				Log.Error(this.WorldPosition.ToString() + " Tangents empty for region!");
			}
			if (_vertices.Count != _tangents.Count)
			{
				Log.Error("ERROR: Vertices.Count ({0}) != tangents.Count ({1})", new object[] { _vertices.Count, _tangents.Count });
			}
			else
			{
				time.Reset();
				MeshUnsafeCopyHelper.CopyTangents(_tangents, _mesh);
				time.CopyTangents = time.GetTime();
				if ((DateTime.Now - start).TotalMilliseconds > 3.0)
				{
					start = DateTime.Now;
					yield return null;
				}
			}
		}
		time.Reset();
		GameUtils.SetMeshVertexAttributes(_mesh, true);
		_mesh.UploadMeshData(false);
		time.UploadMesh = time.GetTime();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateTerrainGo(VoxelMeshTerrain terrain, GameObject parent, MeshTiming time)
	{
		if (terrain == null || terrain.Vertices == null || terrain.Vertices.Count == 0)
		{
			return;
		}
		GameObject terrainMeshRendererFromPool = DynamicMeshItem.GetTerrainMeshRendererFromPool();
		terrainMeshRendererFromPool.name = "T";
		terrainMeshRendererFromPool.transform.parent = parent.transform;
		terrainMeshRendererFromPool.transform.localPosition = Vector3.zero;
		terrainMeshRendererFromPool.SetActive(true);
		MeshFilter component = terrainMeshRendererFromPool.GetComponent<MeshFilter>();
		Mesh mesh;
		if (component.sharedMesh == null)
		{
			mesh = new Mesh();
		}
		else
		{
			mesh = component.sharedMesh;
		}
		mesh.indexFormat = ((terrain.Vertices.Count < 65535) ? 0 : 1);
		component.sharedMesh = mesh;
		DynamicMeshVoxelLoad.CopyTerrain(terrain, mesh, component, time, null);
	}

	public static DynamicMeshLoadResult LoadRegionFromFile(BinaryReader reader, DyMeshRegionLoadRequest load)
	{
		DynamicMeshLoadResult dynamicMeshLoadResult = DynamicMeshVoxelRegionLoad.ReadVoxelMesh(reader, load.OpaqueMesh);
		if (dynamicMeshLoadResult != DynamicMeshLoadResult.Ok)
		{
			Log.Warning("Region file was corrupted. Adding for regeneration");
			Vector3i worldPosFromKey = DynamicMeshUnity.GetWorldPosFromKey(load.Key);
			DynamicMeshThread.AddRegionUpdateData(worldPosFromKey.x, worldPosFromKey.z, false);
			return dynamicMeshLoadResult;
		}
		dynamicMeshLoadResult = DynamicMeshVoxelRegionLoad.ReadTerrainMesh(reader, load.TerrainMesh);
		if (dynamicMeshLoadResult != DynamicMeshLoadResult.Ok)
		{
			Log.Warning("Region file was corrupted. Adding for regeneration");
			Vector3i worldPosFromKey2 = DynamicMeshUnity.GetWorldPosFromKey(load.Key);
			DynamicMeshThread.AddRegionUpdateData(worldPosFromKey2.x, worldPosFromKey2.z, false);
			return dynamicMeshLoadResult;
		}
		if (load.OpaqueMesh.Tangents.Count != load.OpaqueMesh.Vertices.Count)
		{
			MeshCalculations.CalculateMeshTangents(load.OpaqueMesh.Vertices, load.OpaqueMesh.Triangles, load.OpaqueMesh.Normals, load.OpaqueMesh.Uvs, load.OpaqueMesh.Tangents, false);
		}
		return DynamicMeshLoadResult.Ok;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DynamicMeshLoadResult ReadVoxelMesh(BinaryReader reader, MeshLists cache)
	{
		if (reader.ReadInt32() == 0)
		{
			return DynamicMeshLoadResult.Ok;
		}
		UVRectTiling[] uvMapping = MeshDescription.meshes[0].textureAtlas.uvMapping;
		List<Vector3> vertices = cache.Vertices;
		List<Vector2> uvs = cache.Uvs;
		List<Color> colours = cache.Colours;
		List<int> triangles = cache.Triangles;
		List<Vector3> normals = cache.Normals;
		List<Vector4> tangents = cache.Tangents;
		int num = 0;
		uint num2 = 0U;
		try
		{
			num2 = reader.ReadUInt32();
			num = 1;
			if (num2 == 0U)
			{
				return DynamicMeshLoadResult.Ok;
			}
			if (reader.BaseStream.Position + (long)((ulong)(num2 * 6U)) > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			vertices.Capacity = (int)Math.Max((long)vertices.Capacity, (long)((ulong)num2));
			for (uint num3 = 0U; num3 < num2; num3 += 1U)
			{
				vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			}
			num = 2;
			uint num4 = reader.ReadUInt32();
			uvs.Capacity = (int)Math.Max((long)uvs.Capacity, (long)((ulong)num4));
			colours.Capacity = (int)Math.Max((long)colours.Capacity, (long)((ulong)num4));
			if (reader.BaseStream.Position + (long)((ulong)(num4 * 8U)) > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			num = 3;
			for (uint num5 = 0U; num5 < num4; num5 += 1U)
			{
				int num6 = (int)reader.ReadInt16();
				int num7 = (int)reader.ReadByte();
				int num8 = ((num6 >= uvMapping.Length) ? 0 : (uvMapping[num6].index + num7));
				bool flag = reader.ReadBoolean();
				uvs.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
				colours.Add(new Color(0f, (float)num8, 0f, (float)((!flag) ? 0 : 1)));
			}
			num = 4;
			uint num9 = reader.ReadUInt32();
			if (reader.BaseStream.Position + (long)((ulong)(num9 * 4U)) > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			triangles.Capacity = (int)Math.Max((long)triangles.Capacity, (long)((ulong)num9));
			num = 5;
			for (uint num10 = 0U; num10 < num9; num10 += 1U)
			{
				triangles.Add(reader.ReadInt32());
			}
			num = 6;
			uint num11 = reader.ReadUInt32();
			if (reader.BaseStream.Position + (long)((ulong)(num11 * 6U)) > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			normals.Capacity = (int)Math.Max((long)normals.Capacity, (long)((ulong)num11));
			for (uint num12 = 0U; num12 < num11; num12 += 1U)
			{
				normals.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			}
			num = 7;
			uint num13 = reader.ReadUInt32();
			if (reader.BaseStream.Position + (long)((ulong)(num13 * 8U)) > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			tangents.Capacity = (int)Math.Max((long)tangents.Capacity, (long)((ulong)num13));
			for (uint num14 = 0U; num14 < num13; num14 += 1U)
			{
				tangents.Add(new Vector4((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			}
			return DynamicMeshLoadResult.Ok;
		}
		catch (Exception ex)
		{
			Log.Out(string.Concat(new string[]
			{
				"Read voxel mesh error:  verts: ",
				num2.ToString(),
				" stage:  ",
				num.ToString(),
				"  message: ",
				ex.Message
			}));
		}
		return DynamicMeshLoadResult.Error;
	}

	public static DynamicMeshLoadResult ReadTerrainMesh(BinaryReader reader, MeshLists cache)
	{
		if (reader.ReadInt32() == 0)
		{
			return DynamicMeshLoadResult.Ok;
		}
		List<Vector3> vertices = cache.Vertices;
		List<int> triangles = cache.Triangles;
		List<Color> colours = cache.Colours;
		List<Vector2> uvs = cache.Uvs;
		List<Vector2> uvs2 = cache.Uvs2;
		List<Vector2> uvs3 = cache.Uvs3;
		List<Vector2> uvs4 = cache.Uvs4;
		List<Vector3> normals = cache.Normals;
		List<Vector4> tangents = cache.Tangents;
		uint num = reader.ReadUInt32();
		if (reader.BaseStream.Position + (long)((ulong)(num * 6U)) > reader.BaseStream.Length)
		{
			return DynamicMeshLoadResult.WrongSize;
		}
		vertices.Capacity = (int)Math.Max((long)vertices.Capacity, (long)((ulong)num));
		for (uint num2 = 0U; num2 < num; num2 += 1U)
		{
			vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
		}
		uint num3 = reader.ReadUInt32();
		if (reader.BaseStream.Position + (long)((ulong)(num3 * 4U * 4U)) > reader.BaseStream.Length)
		{
			return DynamicMeshLoadResult.WrongSize;
		}
		uvs.Capacity = (int)Math.Max((long)uvs.Capacity, (long)((ulong)num3));
		uvs2.Capacity = (int)Math.Max((long)uvs2.Capacity, (long)((ulong)num3));
		uvs3.Capacity = (int)Math.Max((long)uvs3.Capacity, (long)((ulong)num3));
		uvs4.Capacity = (int)Math.Max((long)uvs4.Capacity, (long)((ulong)num3));
		for (uint num4 = 0U; num4 < num3; num4 += 1U)
		{
			uvs.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
		}
		num3 = reader.ReadUInt32();
		for (uint num5 = 0U; num5 < num3; num5 += 1U)
		{
			uvs2.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
		}
		num3 = reader.ReadUInt32();
		for (uint num6 = 0U; num6 < num3; num6 += 1U)
		{
			uvs3.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
		}
		num3 = reader.ReadUInt32();
		for (uint num7 = 0U; num7 < num3; num7 += 1U)
		{
			uvs4.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
		}
		uint num8 = reader.ReadUInt32();
		int num9 = 0;
		while ((long)num9 < (long)((ulong)num8))
		{
			uint num10 = reader.ReadUInt32();
			if (reader.BaseStream.Position + (long)((ulong)(num10 * 4U)) > reader.BaseStream.Length)
			{
				return DynamicMeshLoadResult.WrongSize;
			}
			for (uint num11 = 0U; num11 < num10; num11 += 1U)
			{
				triangles.Add(reader.ReadInt32());
			}
			num9++;
		}
		uint num12 = reader.ReadUInt32();
		if (reader.BaseStream.Position + (long)((ulong)(num12 * 8U)) > reader.BaseStream.Length)
		{
			return DynamicMeshLoadResult.WrongSize;
		}
		colours.Capacity = (int)Math.Max((long)colours.Capacity, (long)((ulong)num12));
		for (uint num13 = 0U; num13 < num12; num13 += 1U)
		{
			colours.Add(new Color((float)reader.ReadUInt16() / 10000f + 1f, (float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
		}
		uint num14 = reader.ReadUInt32();
		if (reader.BaseStream.Position + (long)((ulong)(num14 * 6U)) > reader.BaseStream.Length)
		{
			return DynamicMeshLoadResult.WrongSize;
		}
		normals.Capacity = (int)Math.Max((long)normals.Capacity, (long)((ulong)num14));
		for (uint num15 = 0U; num15 < num14; num15 += 1U)
		{
			normals.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
		}
		uint num16 = reader.ReadUInt32();
		if (reader.BaseStream.Position + (long)((ulong)(num16 * 8U)) > reader.BaseStream.Length)
		{
			return DynamicMeshLoadResult.WrongSize;
		}
		tangents.Capacity = (int)Math.Max((long)tangents.Capacity, (long)((ulong)num16));
		for (uint num17 = 0U; num17 < num16; num17 += 1U)
		{
			tangents.Add(new Vector4((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
		}
		return DynamicMeshLoadResult.Ok;
	}

	public static void SaveRegionToFile(BinaryWriter _bw, VoxelMesh meshes, VoxelMeshTerrain terrain, Vector3i worldPos, int updateTime, DynamicMeshChunkProcessor builder, DynamicMeshRegion region)
	{
		DynamicMeshVoxelRegionLoad.WriteOpaqueMesh(_bw, meshes);
		DynamicMeshVoxelRegionLoad.WriteTerrainVoxelMeshesToDisk(_bw, terrain);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteTerrainVoxelMeshesToDisk(BinaryWriter writer, VoxelMeshTerrain mesh)
	{
		int num = ((mesh.Vertices.Count == 0) ? 0 : 1);
		writer.Write(num);
		if (num == 0)
		{
			return;
		}
		int count = mesh.Vertices.Count;
		int count2 = mesh.Indices.Count;
		int num2 = mesh.submeshes.Sum((TerrainSubMesh e) => e.triangles.Count);
		int count3 = mesh.ColorVertices.Count;
		if (count != count3)
		{
			Log.Error("Invalid colours");
		}
		int count4 = mesh.Uvs.Count;
		int count5 = mesh.UvsCrack.Count;
		int count6 = mesh.Uvs3.Count;
		int count7 = mesh.Uvs4.Count;
		writer.Write((uint)count);
		ArrayListMP<Vector3> vertices = mesh.Vertices;
		for (int i = 0; i < vertices.Count; i++)
		{
			writer.Write((short)((double)vertices[i].x * 100.0));
			writer.Write((short)((double)vertices[i].y * 100.0));
			writer.Write((short)((double)vertices[i].z * 100.0));
		}
		writer.Write((uint)count4);
		ArrayListMP<Vector2> uvs = mesh.Uvs;
		for (int j = 0; j < uvs.Count; j++)
		{
			writer.Write((ushort)((double)uvs[j].x * 10000.0));
			writer.Write((ushort)((double)uvs[j].y * 10000.0));
		}
		writer.Write((uint)count5);
		ArrayListMP<Vector2> uvsCrack = mesh.UvsCrack;
		for (int k = 0; k < uvsCrack.Count; k++)
		{
			writer.Write((ushort)((double)uvsCrack[k].x * 10000.0));
			writer.Write((ushort)((double)uvsCrack[k].y * 10000.0));
		}
		writer.Write((uint)count6);
		ArrayListMP<Vector2> uvs2 = mesh.Uvs3;
		for (int l = 0; l < uvs2.Count; l++)
		{
			writer.Write((ushort)((double)uvs2[l].x * 10000.0));
			writer.Write((ushort)((double)uvs2[l].y * 10000.0));
		}
		writer.Write((uint)count7);
		ArrayListMP<Vector2> uvs3 = mesh.Uvs4;
		for (int m = 0; m < uvs3.Count; m++)
		{
			writer.Write((ushort)((double)uvs3[m].x * 10000.0));
			writer.Write((ushort)((double)uvs3[m].y * 10000.0));
		}
		int num3 = 1;
		writer.Write((uint)num3);
		writer.Write((uint)count2);
		int num4 = 0;
		if (count2 % 3 != 0 || num2 % 3 != 0)
		{
			Log.Out("Weird triangles");
		}
		if (mesh.submeshes.Count > 0)
		{
			using (List<TerrainSubMesh>.Enumerator enumerator = mesh.submeshes.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					TerrainSubMesh terrainSubMesh = enumerator.Current;
					for (int n = 0; n < terrainSubMesh.triangles.Count; n++)
					{
						writer.Write(terrainSubMesh.triangles[n] + num4);
					}
				}
				goto IL_0397;
			}
		}
		for (int num5 = 0; num5 < mesh.Indices.Count; num5++)
		{
			writer.Write(mesh.Indices[num5] + num4);
		}
		IL_0397:
		num4 += mesh.Vertices.Count;
		writer.Write((uint)count);
		ArrayListMP<Color> colorVertices = mesh.ColorVertices;
		for (int num6 = 0; num6 < colorVertices.Count; num6++)
		{
			Color color = colorVertices[num6];
			writer.Write((ushort)(color.r * 10000f));
			writer.Write((ushort)(color.g * 10000f));
			writer.Write((ushort)(color.b * 10000f));
			writer.Write((ushort)(color.a * 10000f));
		}
		int count8 = mesh.Normals.Count;
		writer.Write((uint)count8);
		ArrayListMP<Vector3> normals = mesh.Normals;
		for (int num7 = 0; num7 < normals.Count; num7++)
		{
			Vector3 vector = normals[num7];
			writer.Write((short)((double)vector.x * 100.0));
			writer.Write((short)((double)vector.y * 100.0));
			writer.Write((short)((double)vector.z * 100.0));
		}
		int count9 = mesh.Tangents.Count;
		writer.Write((uint)count9);
		ArrayListMP<Vector4> tangents = mesh.Tangents;
		for (int num8 = 0; num8 < tangents.Count; num8++)
		{
			Vector4 vector2 = tangents[num8];
			writer.Write((short)((double)vector2.x * 100.0));
			writer.Write((short)((double)vector2.y * 100.0));
			writer.Write((short)((double)vector2.z * 100.0));
			writer.Write((short)((double)vector2.w * 100.0));
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteOpaqueMesh(BinaryWriter writer, VoxelMesh mesh)
	{
		int num = ((mesh.Vertices.Count == 0) ? 0 : 1);
		writer.Write(num);
		if (num == 0)
		{
			return;
		}
		UVRectTiling[] uvMapping = MeshDescription.meshes[0].textureAtlas.uvMapping;
		if (!DynamicMeshManager.Allow32BitMeshes)
		{
			Log.Error("Can't write combined meshes to disk unless in 32 bit mode");
			return;
		}
		int count = mesh.Vertices.Count;
		int count2 = mesh.Indices.Count;
		writer.Write((uint)count);
		ArrayListMP<Vector3> vertices = mesh.Vertices;
		for (int i = 0; i < vertices.Count; i++)
		{
			writer.Write((short)((double)vertices[i].x * 100.0));
			writer.Write((short)((double)vertices[i].y * 100.0));
			writer.Write((short)((double)vertices[i].z * 100.0));
		}
		writer.Write((uint)count);
		ArrayListMP<Color> colorVertices = mesh.ColorVertices;
		ArrayListMP<Vector2> uvs = mesh.Uvs;
		for (int j = 0; j < colorVertices.Count; j++)
		{
			int num2 = (int)colorVertices[j].g;
			int num3 = -1;
			if (!DynamicMeshVoxelRegionLoad.TileLinks.TryGetValue(num2, out num3))
			{
				for (int k = 0; k < uvMapping.Length; k++)
				{
					UVRectTiling uvrectTiling = uvMapping[k];
					if (uvrectTiling.index == num2 || k + 1 >= uvMapping.Length || (float)uvrectTiling.index + uvrectTiling.uv.width * uvrectTiling.uv.height > (float)num2)
					{
						num3 = k;
						DynamicMeshVoxelRegionLoad.TileLinks.TryAdd(num2, num3);
						break;
					}
				}
				if (num3 == -1)
				{
					num3 = 0;
					DynamicMeshVoxelRegionLoad.TileLinks.TryAdd(num2, num3);
				}
			}
			if (num3 == -1)
			{
				num3 = 0;
			}
			writer.Write((short)num3);
			writer.Write((byte)(num2 - uvMapping[num3].index));
			bool flag = (double)colorVertices[j].a > 0.5;
			writer.Write(flag);
			writer.Write((ushort)((double)uvs[j].x * 10000.0));
			writer.Write((ushort)((double)uvs[j].y * 10000.0));
		}
		writer.Write((uint)count2);
		ArrayListMP<int> indices = mesh.Indices;
		for (int l = 0; l < indices.Count; l++)
		{
			writer.Write(indices[l]);
		}
		int count3 = mesh.Normals.Count;
		writer.Write((uint)count3);
		ArrayListMP<Vector3> normals = mesh.Normals;
		for (int m = 0; m < normals.Count; m++)
		{
			Vector3 vector = normals[m];
			writer.Write((short)((double)vector.x * 100.0));
			writer.Write((short)((double)vector.y * 100.0));
			writer.Write((short)((double)vector.z * 100.0));
		}
		int count4 = mesh.Tangents.Count;
		writer.Write((uint)count4);
		ArrayListMP<Vector4> tangents = mesh.Tangents;
		for (int n = 0; n < tangents.Count; n++)
		{
			Vector4 vector2 = tangents[n];
			writer.Write((short)((double)vector2.x * 100.0));
			writer.Write((short)((double)vector2.y * 100.0));
			writer.Write((short)((double)vector2.z * 100.0));
			writer.Write((short)((double)vector2.w * 100.0));
		}
	}

	public static ConcurrentDictionary<int, int> TileLinks = new ConcurrentDictionary<int, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Bounds> bounds = new List<Bounds>(10);
}
