using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Noemax.GZip;
using UnityEngine;

public class DynamicMeshFile
{
	public static void CleanUp()
	{
		MeshLists.MeshListCache.Clear();
		MeshLists.LastLargest = 0;
	}

	public static string DisabledImpostersFile
	{
		get
		{
			return DynamicMeshFile.MeshLocation + "DisabledImposters.list";
		}
	}

	public static Bounds GetBoundsFromVerts(ArrayListMP<Vector3> verts)
	{
		float num = verts[0].x;
		float num2 = verts[0].x;
		float num3 = verts[0].y;
		float num4 = verts[0].y;
		float num5 = verts[0].z;
		float num6 = verts[0].z;
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 vector = verts[i];
			num = Math.Min(num, vector.x);
			num2 = Math.Max(num2, vector.x);
			num3 = Math.Min(num3, vector.y);
			num4 = Math.Max(num4, vector.y);
			num5 = Math.Min(num5, vector.z);
			num6 = Math.Max(num6, vector.z);
		}
		Vector3 vector2;
		vector2..ctor(num, num3, num5);
		Vector3 vector3;
		vector3..ctor(num2, num4, num6);
		Bounds bounds = default(Bounds);
		bounds.size = vector3 - vector2;
		bounds.center = bounds.size / 2f + vector2;
		return bounds;
	}

	public static Bounds GetBoundsFromVerts(List<Vector3> verts)
	{
		float num = verts[0].x;
		float num2 = verts[0].x;
		float num3 = verts[0].y;
		float num4 = verts[0].y;
		float num5 = verts[0].z;
		float num6 = verts[0].z;
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 vector = verts[i];
			num = Math.Min(num, vector.x);
			num2 = Math.Max(num2, vector.x);
			num3 = Math.Min(num3, vector.y);
			num4 = Math.Max(num4, vector.y);
			num5 = Math.Min(num5, vector.z);
			num6 = Math.Max(num6, vector.z);
		}
		Vector3 vector2;
		vector2..ctor(num, num3, num5);
		Vector3 vector3;
		vector3..ctor(num2, num4, num6);
		Bounds bounds = default(Bounds);
		bounds.size = vector3 - vector2;
		bounds.center = bounds.size / 2f + vector2;
		return bounds;
	}

	public static Bounds GetBoundsFromVerts(Vector3[] verts)
	{
		float num = verts[0].x;
		float num2 = verts[0].x;
		float num3 = verts[0].y;
		float num4 = verts[0].y;
		float num5 = verts[0].z;
		float num6 = verts[0].z;
		foreach (Vector3 vector in verts)
		{
			num = Math.Min(num, vector.x);
			num2 = Math.Max(num2, vector.x);
			num3 = Math.Min(num3, vector.y);
			num4 = Math.Max(num4, vector.y);
			num5 = Math.Min(num5, vector.z);
			num6 = Math.Max(num6, vector.z);
		}
		Vector3 vector2;
		vector2..ctor(num, num3, num5);
		Vector3 vector3;
		vector3..ctor(num2, num4, num6);
		Bounds bounds = default(Bounds);
		bounds.size = vector3 - vector2;
		bounds.center = bounds.size / 2f + vector2;
		return bounds;
	}

	public static Bounds GetBoundsFromVertsJustY(ArrayListMP<Vector3> verts, Bounds bounds)
	{
		float num = verts[0].y;
		float num2 = verts[0].y;
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 vector = verts[i];
			num = Math.Min(num, vector.y);
			num2 = Math.Max(num2, vector.y);
		}
		Vector3 vector2;
		vector2..ctor(0f, num, 0f);
		Vector3 vector3;
		vector3..ctor(0f, num2, 0f);
		bounds.SetMinMax(vector2, vector3);
		return bounds;
	}

	public static Bounds GetBoundsFromVerts(ArrayListMP<Vector3> verts, List<Bounds> boundsList, int index)
	{
		if (index == boundsList.Count)
		{
			boundsList.Add(default(Bounds));
		}
		while (index >= boundsList.Count)
		{
			boundsList.Add(default(Bounds));
		}
		Bounds bounds = boundsList[index];
		float num = verts[0].x;
		float num2 = verts[0].x;
		float num3 = verts[0].y;
		float num4 = verts[0].y;
		float num5 = verts[0].z;
		float num6 = verts[0].z;
		for (int i = 0; i < verts.Count; i++)
		{
			Vector3 vector = verts[i];
			num = Math.Min(num, vector.x);
			num2 = Math.Max(num2, vector.x);
			num3 = Math.Min(num3, vector.y);
			num4 = Math.Max(num4, vector.y);
			num5 = Math.Min(num5, vector.z);
			num6 = Math.Max(num6, vector.z);
		}
		Vector3 vector2;
		vector2..ctor(num, num3, num5);
		Vector3 vector3;
		vector3..ctor(num2, num4, num6);
		bounds.size = vector3 - vector2;
		bounds.center = bounds.size / 2f + vector2;
		return bounds;
	}

	public static VoxelMesh GetMeshFromPool()
	{
		return DynamicMeshFile.GetMeshFromPool(500);
	}

	public static VoxelMesh GetMeshFromPool(int size)
	{
		VoxelMesh voxelMesh;
		if (DynamicMeshFile.VoxelMeshPool.TryDequeue(out voxelMesh))
		{
			return voxelMesh;
		}
		return new VoxelMesh(0, size, VoxelMesh.CreateFlags.Default);
	}

	public static VoxelMeshTerrain GetTerrainMeshFromPool()
	{
		return DynamicMeshFile.GetTerrainMeshFromPool(500);
	}

	public static VoxelMeshTerrain GetTerrainMeshFromPool(int size)
	{
		VoxelMeshTerrain voxelMeshTerrain;
		if (DynamicMeshFile.VoxelTerrainMeshPool.TryDequeue(out voxelMeshTerrain))
		{
			return voxelMeshTerrain;
		}
		voxelMeshTerrain = new VoxelMeshTerrain(5, size);
		return voxelMeshTerrain;
	}

	public static void AddMeshToPool(VoxelMesh mesh)
	{
		if (mesh is VoxelMeshTerrain)
		{
			DynamicMeshFile.AddMeshToPool((VoxelMeshTerrain)mesh);
			return;
		}
		mesh.ClearMesh();
		DynamicMeshFile.VoxelMeshPool.Enqueue(mesh);
	}

	public static void AddMeshToPool(VoxelMeshTerrain mesh)
	{
		mesh.ClearMesh();
		DynamicMeshFile.VoxelTerrainMeshPool.Enqueue(mesh);
	}

	public static GameObject CreateMeshObject(string name, bool isRegion)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = 0;
		meshRenderer.material = DynamicMeshFile.GetOpaqueMaterial(isRegion);
		gameObject.isStatic = true;
		return gameObject;
	}

	public static GameObject CreateTerrainMeshObject(string name)
	{
		GameObject gameObject = new GameObject(name);
		gameObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.shadowCastingMode = 0;
		gameObject.isStatic = true;
		return gameObject;
	}

	public static void LoadRegionGameObjectSync(string path, DynamicMeshRegion region)
	{
		if (!SdFile.Exists(path))
		{
			return;
		}
		DateTime now = DateTime.Now;
		while ((DateTime.Now - now).TotalSeconds < 10.0)
		{
			bool flag = false;
			try
			{
				using (SdFile.OpenRead(path))
				{
					flag = true;
				}
				break;
			}
			catch (Exception ex)
			{
				Log.Warning("Access file error: " + ex.Message);
			}
			if (!flag)
			{
				return;
			}
		}
		using (Stream readStream = DynamicMeshFile.GetReadStream(path))
		{
			using (PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false))
			{
				pooledBinaryReader.SetBaseStream(readStream);
				int num = pooledBinaryReader.ReadInt32();
				int num2 = pooledBinaryReader.ReadInt32();
				for (int i = 0; i < num2; i++)
				{
					Vector3i vector3i = new Vector3i(pooledBinaryReader.ReadInt32(), 0, pooledBinaryReader.ReadInt32());
					region.AddChunk(vector3i.x, vector3i.z);
					DynamicMeshManager.Instance.AddChunkStub(vector3i, region);
				}
				long num3 = pooledBinaryReader.ReadInt64();
				region.CreateDate = new DateTime(num3);
				int num4 = pooledBinaryReader.ReadInt32();
				int num5 = pooledBinaryReader.ReadInt32();
				int num6 = pooledBinaryReader.ReadInt32();
				region.Rect = new Rect((float)num4, (float)num6, (float)num, (float)num);
				region.WorldPosition = new Vector3i(num4, num5, num6);
				int num7 = pooledBinaryReader.ReadInt32();
				if (num7 == 0)
				{
					if (region.MarkedForDeletion)
					{
						Log.Warning("Removing region. Delete file? " + region.ToDebugLocation());
					}
					else
					{
						region.MarkedForDeletion = true;
					}
				}
				else
				{
					GameObject regionMeshRendererFromPool = DynamicMeshItem.GetRegionMeshRendererFromPool();
					Vector3 vector = ((region.RegionObject == null) ? Vector3.zero : region.RegionObject.transform.position);
					for (int j = 0; j < num7; j++)
					{
						GameObject gameObject = ((j == 0) ? regionMeshRendererFromPool : DynamicMeshItem.GetRegionMeshRendererFromPool());
						MeshFilter component = gameObject.GetComponent<MeshFilter>();
						Mesh mesh = component.mesh ?? new Mesh();
						if (j != 0)
						{
							gameObject.transform.parent = regionMeshRendererFromPool.transform;
						}
						gameObject.SetActive(false);
						DynamicMeshFile.ReadMesh(5, pooledBinaryReader, MeshDescription.meshes[0].textureAtlas.uvMapping, mesh, true, null);
						component.mesh = mesh;
						if (j != 0)
						{
							gameObject.SetActive(true);
						}
					}
					regionMeshRendererFromPool.transform.position = vector;
					if (region.RegionObject != null)
					{
						DynamicMeshItem.AddToMeshPool(region.RegionObject);
					}
					region.RegionObject = regionMeshRendererFromPool;
					region.SetPosition();
					region.SetVisibleNew(region.VisibleChunks == 0 && !region.IsPlayerInRegion(), "load region sync finished", true);
					region.IsMeshLoaded = true;
					DynamicMeshManager.Instance.UpdateDynamicPrefabDecoratorRegions(region);
					if (pooledBinaryReader.BaseStream.Position != pooledBinaryReader.BaseStream.Length)
					{
						num7 = pooledBinaryReader.ReadInt32();
						if (num7 > 0)
						{
							for (int k = 0; k < num7; k++)
							{
								GameObject terrainMeshRendererFromPool = DynamicMeshItem.GetTerrainMeshRendererFromPool();
								MeshFilter component2 = terrainMeshRendererFromPool.GetComponent<MeshFilter>();
								MeshRenderer component3 = terrainMeshRendererFromPool.GetComponent<MeshRenderer>();
								Mesh mesh2 = component2.mesh ?? new Mesh();
								terrainMeshRendererFromPool.transform.parent = region.RegionObject.transform;
								terrainMeshRendererFromPool.transform.localPosition = Vector3.zero;
								terrainMeshRendererFromPool.name = "T";
								DynamicMeshFile.ReadMeshTerrain(pooledBinaryReader, mesh2, component3, true);
								component2.mesh = mesh2;
								GameUtils.SetMeshVertexAttributes(mesh2, true);
								mesh2.UploadMeshData(true);
								terrainMeshRendererFromPool.SetActive(true);
							}
						}
					}
				}
			}
		}
	}

	public static Stream GetCreateStream(string path)
	{
		Stream stream = SdFile.Create(path);
		if (DynamicMeshManager.CompressFiles)
		{
			return new DeflateOutputStream(stream, 3, false);
		}
		return stream;
	}

	public static Stream GetReadStream(string path)
	{
		Stream stream = SdFile.OpenRead(path);
		if (DynamicMeshManager.CompressFiles)
		{
			return new DeflateInputStream(stream);
		}
		return stream;
	}

	public static void WriteRegionHeaderData(DynamicMeshRegion region, int tryCount)
	{
		if (tryCount > 10)
		{
			Log.Error("Could not save region header: " + region.ToDebugLocation());
			return;
		}
		string meshLocation = DynamicMeshFile.MeshLocation;
		if (!SdDirectory.Exists(meshLocation))
		{
			SdDirectory.CreateDirectory(meshLocation);
		}
		string path = region.Path;
		try
		{
			using (Stream createStream = DynamicMeshFile.GetCreateStream(path))
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
				{
					pooledBinaryWriter.SetBaseStream(createStream);
					pooledBinaryWriter.Write(160);
					Vector3i[] array = region.LoadedChunks.Distinct<Vector3i>().ToArray<Vector3i>();
					pooledBinaryWriter.Write(array.Length);
					foreach (Vector3i vector3i in array)
					{
						pooledBinaryWriter.Write(vector3i.x);
						pooledBinaryWriter.Write(vector3i.z);
					}
					pooledBinaryWriter.Write(region.CreateDate.Ticks);
					pooledBinaryWriter.Write(region.WorldPosition.x);
					pooledBinaryWriter.Write(region.WorldPosition.y);
					pooledBinaryWriter.Write(region.WorldPosition.z);
					pooledBinaryWriter.Write(0);
					pooledBinaryWriter.Write(0);
					pooledBinaryWriter.Write(0);
				}
			}
		}
		catch (Exception ex)
		{
			tryCount++;
			Log.Warning("Write region header error: " + ex.Message + " attempt: " + tryCount.ToString());
			Thread.Sleep(500 * tryCount);
			DynamicMeshFile.WriteRegionHeaderData(region, tryCount);
		}
	}

	public static void WriteRegion(DynamicMeshRegion region, int tryCount)
	{
		if (tryCount > 5)
		{
			string text = "Could not save region: ";
			Vector3i worldPosition = region.WorldPosition;
			Log.Error(text + worldPosition.ToString());
			return;
		}
		string meshLocation = DynamicMeshFile.MeshLocation;
		if (!SdDirectory.Exists(meshLocation))
		{
			SdDirectory.CreateDirectory(meshLocation);
		}
		int num = region.GetStreamLength();
		num *= tryCount;
		byte[] fromPool = DynamicMeshThread.ChunkDataQueue.GetFromPool(num);
		try
		{
			using (MemoryStream memoryStream = new MemoryStream(fromPool))
			{
				using (PooledBinaryWriter pooledBinaryWriter = MemoryPools.poolBinaryWriter.AllocSync(false))
				{
					pooledBinaryWriter.SetBaseStream(memoryStream);
					pooledBinaryWriter.Write(160);
					Vector3i[] array = region.LoadedChunks.Distinct<Vector3i>().ToArray<Vector3i>();
					pooledBinaryWriter.Write(array.Length);
					foreach (Vector3i vector3i in array)
					{
						pooledBinaryWriter.Write(vector3i.x);
						pooledBinaryWriter.Write(vector3i.z);
					}
					region.CreateDate = DateTime.Now;
					pooledBinaryWriter.Write(region.CreateDate.Ticks);
					long position = pooledBinaryWriter.BaseStream.Position;
				}
			}
		}
		catch (Exception ex)
		{
			if (!(ex.Message == "Memory stream is not expandable") || tryCount > 1)
			{
				Log.Warning(string.Concat(new string[]
				{
					"Write region error: ",
					region.ToDebugLocation(),
					"  ",
					ex.Message,
					"  attempt: ",
					tryCount.ToString()
				}));
			}
			tryCount++;
			Thread.Sleep(100 * tryCount);
			DynamicMeshFile.WriteRegion(region, tryCount);
		}
	}

	public static void WriteVoxelMeshes(BinaryWriter _bw, List<VoxelMesh> meshes, Vector3i worldPos, int updateTime, DynamicMeshChunkProcessor builder)
	{
		if (DynamicMeshManager.Allow32BitMeshes)
		{
			DynamicMeshFile.Write32BitVoxelMeshes(_bw, meshes, worldPos, updateTime, builder);
			return;
		}
		DynamicMeshFile.Write16BitVoxelMeshes(_bw, meshes, worldPos, updateTime, builder);
	}

	public static void Write32BitVoxelMeshes(BinaryWriter _bw, List<VoxelMesh> meshes, Vector3i worldPos, int updateTime, DynamicMeshChunkProcessor builder)
	{
		int num = 1;
		bool allow32BitMeshes = DynamicMeshManager.Allow32BitMeshes;
		if (meshes.Sum((VoxelMesh d) => d.Vertices.Count) < 65535)
		{
			DynamicMeshFile.Write16BitVoxelMeshes(_bw, meshes, worldPos, updateTime, builder);
			return;
		}
		_bw.Write(worldPos.x);
		if (builder != null)
		{
			_bw.Write(builder.yOffset);
		}
		else
		{
			_bw.Write(worldPos.y);
		}
		_bw.Write(worldPos.z);
		if (builder != null)
		{
			_bw.Write(updateTime);
		}
		_bw.Write(num);
		for (int i = 0; i < meshes.Count; i++)
		{
			VoxelMesh voxelMesh = meshes[i];
			if (DynamicMeshFile.bounds.Count < i + 1)
			{
				DynamicMeshFile.bounds.Add(default(Bounds));
			}
			if (voxelMesh.Vertices.Count == 0)
			{
				DynamicMeshFile.bounds[i].SetMinMax(Vector3.zero, Vector3.zero);
			}
			else
			{
				DynamicMeshFile.bounds.Add(DynamicMeshFile.GetBoundsFromVertsJustY(voxelMesh.Vertices, DynamicMeshFile.bounds[i]));
			}
		}
		DynamicMeshFile.WriteVoxelMeshesToDisk(_bw, meshes);
	}

	public static void Write16BitVoxelMeshes(BinaryWriter _bw, List<VoxelMesh> meshes, Vector3i worldPos, int updateTime, DynamicMeshChunkProcessor builder)
	{
		int num = 0;
		ushort maxValue = ushort.MaxValue;
		foreach (VoxelMesh voxelMesh in meshes)
		{
			if (voxelMesh.Vertices.Count > 0)
			{
				num += (int)Math.Ceiling((double)voxelMesh.Vertices.Count / (double)maxValue);
			}
		}
		_bw.Write(worldPos.x);
		if (builder != null)
		{
			_bw.Write(builder.yOffset);
		}
		else
		{
			_bw.Write(worldPos.y);
		}
		_bw.Write(worldPos.z);
		if (builder != null)
		{
			_bw.Write(updateTime);
		}
		_bw.Write(num);
		int num2 = 0;
		float num3 = 0f;
		for (int i = 0; i < meshes.Count; i++)
		{
			VoxelMesh voxelMesh2 = meshes[i];
			if (DynamicMeshFile.bounds.Count < i + 1)
			{
				DynamicMeshFile.bounds.Add(default(Bounds));
			}
			if (voxelMesh2.Vertices.Count == 0)
			{
				DynamicMeshFile.bounds[i].SetMinMax(Vector3.zero, Vector3.zero);
			}
			else
			{
				DynamicMeshFile.bounds.Add(DynamicMeshFile.GetBoundsFromVertsJustY(voxelMesh2.Vertices, DynamicMeshFile.bounds[i]));
			}
		}
		for (int j = 0; j < meshes.Count; j++)
		{
			VoxelMesh voxelMesh3 = meshes[j];
			if (voxelMesh3.Vertices.Count != 0)
			{
				DynamicMeshFile.WriteVoxelMeshToDisk(_bw, voxelMesh3, MeshDescription.meshes[0].textureAtlas.uvMapping, num3, builder, 0);
				num3 -= DynamicMeshFile.bounds[j].size.y;
				num2 += voxelMesh3.Vertices.Count;
			}
		}
	}

	public static void WriteVoxelMeshesWithTerrain(BinaryWriter _bw, List<VoxelMesh> meshes, List<VoxelMeshTerrain> terrain, Vector3i worldPos, int updateTime, DynamicMeshChunkProcessor builder, DynamicMeshRegion region)
	{
		DynamicMeshFile.WriteVoxelMeshes(_bw, meshes, worldPos, updateTime, builder);
		if (terrain == null)
		{
			return;
		}
		int num = 0;
		int num2 = (DynamicMeshManager.Allow32BitMeshes ? int.MaxValue : 65535);
		foreach (VoxelMeshTerrain voxelMeshTerrain in terrain)
		{
			if (voxelMeshTerrain.Vertices.Count > 0)
			{
				num += (int)Math.Ceiling((double)voxelMeshTerrain.Vertices.Count / (double)num2);
			}
		}
		_bw.Write(num);
		int num3 = 0;
		float num4 = 0f;
		for (int i = 0; i < terrain.Count; i++)
		{
			VoxelMeshTerrain voxelMeshTerrain2 = terrain[i];
			if (DynamicMeshFile.bounds.Count < i + 1)
			{
				DynamicMeshFile.bounds.Add(default(Bounds));
			}
			if (voxelMeshTerrain2.Vertices.Count == 0)
			{
				DynamicMeshFile.bounds[i].SetMinMax(Vector3.zero, Vector3.zero);
			}
			else
			{
				DynamicMeshFile.bounds.Add(DynamicMeshFile.GetBoundsFromVertsJustY(voxelMeshTerrain2.Vertices, DynamicMeshFile.bounds[i]));
			}
		}
		if (terrain.Sum((VoxelMeshTerrain d) => d.Vertices.Count) < 65535 || !DynamicMeshManager.Allow32BitMeshes)
		{
			for (int j = 0; j < terrain.Count; j++)
			{
				VoxelMeshTerrain voxelMeshTerrain3 = terrain[j];
				if (voxelMeshTerrain3.Vertices.Count != 0)
				{
					DynamicMeshFile.WriteTerrainVoxelMeshToDisk(_bw, voxelMeshTerrain3, num4, builder, region, 0);
					num4 -= DynamicMeshFile.bounds[j].size.y;
					num3 += voxelMeshTerrain3.Vertices.Count;
				}
			}
			return;
		}
		DynamicMeshFile.WriteTerrainVoxelMeshesToDisk(_bw, terrain);
	}

	public static void InitTiles()
	{
		DynamicMeshFile.TileLinks.Clear();
		DynamicMeshFile.TileLinks.TryAdd(0, 0);
		DynamicMeshFile.TileLinks.TryAdd(1, 7);
		DynamicMeshFile.TileLinks.TryAdd(10, 11);
		DynamicMeshFile.TileLinks.TryAdd(100, 170);
		DynamicMeshFile.TileLinks.TryAdd(101, 170);
		DynamicMeshFile.TileLinks.TryAdd(102, 170);
		DynamicMeshFile.TileLinks.TryAdd(103, 170);
		DynamicMeshFile.TileLinks.TryAdd(104, 171);
		DynamicMeshFile.TileLinks.TryAdd(105, 172);
		DynamicMeshFile.TileLinks.TryAdd(106, 173);
		DynamicMeshFile.TileLinks.TryAdd(107, 173);
		DynamicMeshFile.TileLinks.TryAdd(108, 173);
		DynamicMeshFile.TileLinks.TryAdd(109, 173);
		DynamicMeshFile.TileLinks.TryAdd(11, 11);
		DynamicMeshFile.TileLinks.TryAdd(110, 174);
		DynamicMeshFile.TileLinks.TryAdd(112, 190);
		DynamicMeshFile.TileLinks.TryAdd(113, 191);
		DynamicMeshFile.TileLinks.TryAdd(114, 192);
		DynamicMeshFile.TileLinks.TryAdd(115, 192);
		DynamicMeshFile.TileLinks.TryAdd(116, 192);
		DynamicMeshFile.TileLinks.TryAdd(117, 192);
		DynamicMeshFile.TileLinks.TryAdd(118, 193);
		DynamicMeshFile.TileLinks.TryAdd(119, 193);
		DynamicMeshFile.TileLinks.TryAdd(12, 11);
		DynamicMeshFile.TileLinks.TryAdd(120, 193);
		DynamicMeshFile.TileLinks.TryAdd(121, 193);
		DynamicMeshFile.TileLinks.TryAdd(122, 194);
		DynamicMeshFile.TileLinks.TryAdd(123, 194);
		DynamicMeshFile.TileLinks.TryAdd(124, 194);
		DynamicMeshFile.TileLinks.TryAdd(125, 194);
		DynamicMeshFile.TileLinks.TryAdd(126, 196);
		DynamicMeshFile.TileLinks.TryAdd(127, 214);
		DynamicMeshFile.TileLinks.TryAdd(128, 217);
		DynamicMeshFile.TileLinks.TryAdd(129, 217);
		DynamicMeshFile.TileLinks.TryAdd(13, 12);
		DynamicMeshFile.TileLinks.TryAdd(130, 217);
		DynamicMeshFile.TileLinks.TryAdd(131, 217);
		DynamicMeshFile.TileLinks.TryAdd(132, 226);
		DynamicMeshFile.TileLinks.TryAdd(133, 226);
		DynamicMeshFile.TileLinks.TryAdd(134, 226);
		DynamicMeshFile.TileLinks.TryAdd(135, 226);
		DynamicMeshFile.TileLinks.TryAdd(136, 227);
		DynamicMeshFile.TileLinks.TryAdd(137, 227);
		DynamicMeshFile.TileLinks.TryAdd(138, 227);
		DynamicMeshFile.TileLinks.TryAdd(139, 227);
		DynamicMeshFile.TileLinks.TryAdd(14, 12);
		DynamicMeshFile.TileLinks.TryAdd(140, 229);
		DynamicMeshFile.TileLinks.TryAdd(141, 229);
		DynamicMeshFile.TileLinks.TryAdd(142, 229);
		DynamicMeshFile.TileLinks.TryAdd(143, 229);
		DynamicMeshFile.TileLinks.TryAdd(144, 241);
		DynamicMeshFile.TileLinks.TryAdd(145, 241);
		DynamicMeshFile.TileLinks.TryAdd(146, 241);
		DynamicMeshFile.TileLinks.TryAdd(147, 241);
		DynamicMeshFile.TileLinks.TryAdd(148, 245);
		DynamicMeshFile.TileLinks.TryAdd(149, 245);
		DynamicMeshFile.TileLinks.TryAdd(15, 12);
		DynamicMeshFile.TileLinks.TryAdd(150, 245);
		DynamicMeshFile.TileLinks.TryAdd(151, 245);
		DynamicMeshFile.TileLinks.TryAdd(152, 246);
		DynamicMeshFile.TileLinks.TryAdd(153, 247);
		DynamicMeshFile.TileLinks.TryAdd(154, 248);
		DynamicMeshFile.TileLinks.TryAdd(155, 261);
		DynamicMeshFile.TileLinks.TryAdd(156, 262);
		DynamicMeshFile.TileLinks.TryAdd(157, 263);
		DynamicMeshFile.TileLinks.TryAdd(158, 265);
		DynamicMeshFile.TileLinks.TryAdd(159, 266);
		DynamicMeshFile.TileLinks.TryAdd(16, 12);
		DynamicMeshFile.TileLinks.TryAdd(160, 267);
		DynamicMeshFile.TileLinks.TryAdd(161, 268);
		DynamicMeshFile.TileLinks.TryAdd(162, 269);
		DynamicMeshFile.TileLinks.TryAdd(163, 269);
		DynamicMeshFile.TileLinks.TryAdd(164, 269);
		DynamicMeshFile.TileLinks.TryAdd(165, 269);
		DynamicMeshFile.TileLinks.TryAdd(166, 270);
		DynamicMeshFile.TileLinks.TryAdd(167, 272);
		DynamicMeshFile.TileLinks.TryAdd(168, 276);
		DynamicMeshFile.TileLinks.TryAdd(169, 282);
		DynamicMeshFile.TileLinks.TryAdd(17, 13);
		DynamicMeshFile.TileLinks.TryAdd(170, 282);
		DynamicMeshFile.TileLinks.TryAdd(171, 282);
		DynamicMeshFile.TileLinks.TryAdd(172, 282);
		DynamicMeshFile.TileLinks.TryAdd(173, 284);
		DynamicMeshFile.TileLinks.TryAdd(174, 299);
		DynamicMeshFile.TileLinks.TryAdd(175, 299);
		DynamicMeshFile.TileLinks.TryAdd(176, 299);
		DynamicMeshFile.TileLinks.TryAdd(177, 299);
		DynamicMeshFile.TileLinks.TryAdd(178, 302);
		DynamicMeshFile.TileLinks.TryAdd(179, 302);
		DynamicMeshFile.TileLinks.TryAdd(18, 13);
		DynamicMeshFile.TileLinks.TryAdd(180, 302);
		DynamicMeshFile.TileLinks.TryAdd(181, 302);
		DynamicMeshFile.TileLinks.TryAdd(182, 303);
		DynamicMeshFile.TileLinks.TryAdd(183, 307);
		DynamicMeshFile.TileLinks.TryAdd(184, 311);
		DynamicMeshFile.TileLinks.TryAdd(185, 312);
		DynamicMeshFile.TileLinks.TryAdd(186, 314);
		DynamicMeshFile.TileLinks.TryAdd(187, 315);
		DynamicMeshFile.TileLinks.TryAdd(188, 319);
		DynamicMeshFile.TileLinks.TryAdd(189, 320);
		DynamicMeshFile.TileLinks.TryAdd(19, 13);
		DynamicMeshFile.TileLinks.TryAdd(190, 321);
		DynamicMeshFile.TileLinks.TryAdd(191, 328);
		DynamicMeshFile.TileLinks.TryAdd(192, 328);
		DynamicMeshFile.TileLinks.TryAdd(193, 328);
		DynamicMeshFile.TileLinks.TryAdd(194, 328);
		DynamicMeshFile.TileLinks.TryAdd(195, 329);
		DynamicMeshFile.TileLinks.TryAdd(196, 329);
		DynamicMeshFile.TileLinks.TryAdd(197, 329);
		DynamicMeshFile.TileLinks.TryAdd(198, 329);
		DynamicMeshFile.TileLinks.TryAdd(199, 330);
		DynamicMeshFile.TileLinks.TryAdd(2, 7);
		DynamicMeshFile.TileLinks.TryAdd(20, 13);
		DynamicMeshFile.TileLinks.TryAdd(200, 331);
		DynamicMeshFile.TileLinks.TryAdd(201, 332);
		DynamicMeshFile.TileLinks.TryAdd(202, 332);
		DynamicMeshFile.TileLinks.TryAdd(203, 332);
		DynamicMeshFile.TileLinks.TryAdd(204, 332);
		DynamicMeshFile.TileLinks.TryAdd(206, 335);
		DynamicMeshFile.TileLinks.TryAdd(209, 340);
		DynamicMeshFile.TileLinks.TryAdd(21, 14);
		DynamicMeshFile.TileLinks.TryAdd(210, 341);
		DynamicMeshFile.TileLinks.TryAdd(211, 342);
		DynamicMeshFile.TileLinks.TryAdd(212, 344);
		DynamicMeshFile.TileLinks.TryAdd(213, 344);
		DynamicMeshFile.TileLinks.TryAdd(214, 344);
		DynamicMeshFile.TileLinks.TryAdd(215, 344);
		DynamicMeshFile.TileLinks.TryAdd(216, 345);
		DynamicMeshFile.TileLinks.TryAdd(217, 346);
		DynamicMeshFile.TileLinks.TryAdd(218, 352);
		DynamicMeshFile.TileLinks.TryAdd(219, 355);
		DynamicMeshFile.TileLinks.TryAdd(22, 14);
		DynamicMeshFile.TileLinks.TryAdd(220, 356);
		DynamicMeshFile.TileLinks.TryAdd(221, 358);
		DynamicMeshFile.TileLinks.TryAdd(222, 361);
		DynamicMeshFile.TileLinks.TryAdd(223, 372);
		DynamicMeshFile.TileLinks.TryAdd(226, 379);
		DynamicMeshFile.TileLinks.TryAdd(227, 380);
		DynamicMeshFile.TileLinks.TryAdd(228, 382);
		DynamicMeshFile.TileLinks.TryAdd(229, 384);
		DynamicMeshFile.TileLinks.TryAdd(23, 14);
		DynamicMeshFile.TileLinks.TryAdd(230, 385);
		DynamicMeshFile.TileLinks.TryAdd(231, 385);
		DynamicMeshFile.TileLinks.TryAdd(232, 385);
		DynamicMeshFile.TileLinks.TryAdd(233, 385);
		DynamicMeshFile.TileLinks.TryAdd(234, 391);
		DynamicMeshFile.TileLinks.TryAdd(235, 407);
		DynamicMeshFile.TileLinks.TryAdd(236, 408);
		DynamicMeshFile.TileLinks.TryAdd(237, 408);
		DynamicMeshFile.TileLinks.TryAdd(238, 408);
		DynamicMeshFile.TileLinks.TryAdd(239, 408);
		DynamicMeshFile.TileLinks.TryAdd(24, 14);
		DynamicMeshFile.TileLinks.TryAdd(240, 410);
		DynamicMeshFile.TileLinks.TryAdd(241, 413);
		DynamicMeshFile.TileLinks.TryAdd(246, 422);
		DynamicMeshFile.TileLinks.TryAdd(247, 427);
		DynamicMeshFile.TileLinks.TryAdd(248, 428);
		DynamicMeshFile.TileLinks.TryAdd(249, 429);
		DynamicMeshFile.TileLinks.TryAdd(25, 15);
		DynamicMeshFile.TileLinks.TryAdd(250, 429);
		DynamicMeshFile.TileLinks.TryAdd(251, 429);
		DynamicMeshFile.TileLinks.TryAdd(252, 429);
		DynamicMeshFile.TileLinks.TryAdd(253, 430);
		DynamicMeshFile.TileLinks.TryAdd(254, 435);
		DynamicMeshFile.TileLinks.TryAdd(255, 435);
		DynamicMeshFile.TileLinks.TryAdd(256, 435);
		DynamicMeshFile.TileLinks.TryAdd(257, 435);
		DynamicMeshFile.TileLinks.TryAdd(258, 436);
		DynamicMeshFile.TileLinks.TryAdd(259, 442);
		DynamicMeshFile.TileLinks.TryAdd(26, 15);
		DynamicMeshFile.TileLinks.TryAdd(260, 443);
		DynamicMeshFile.TileLinks.TryAdd(261, 443);
		DynamicMeshFile.TileLinks.TryAdd(262, 443);
		DynamicMeshFile.TileLinks.TryAdd(263, 443);
		DynamicMeshFile.TileLinks.TryAdd(264, 445);
		DynamicMeshFile.TileLinks.TryAdd(265, 446);
		DynamicMeshFile.TileLinks.TryAdd(266, 519);
		DynamicMeshFile.TileLinks.TryAdd(267, 525);
		DynamicMeshFile.TileLinks.TryAdd(268, 531);
		DynamicMeshFile.TileLinks.TryAdd(269, 532);
		DynamicMeshFile.TileLinks.TryAdd(27, 15);
		DynamicMeshFile.TileLinks.TryAdd(270, 534);
		DynamicMeshFile.TileLinks.TryAdd(271, 534);
		DynamicMeshFile.TileLinks.TryAdd(272, 534);
		DynamicMeshFile.TileLinks.TryAdd(273, 534);
		DynamicMeshFile.TileLinks.TryAdd(274, 535);
		DynamicMeshFile.TileLinks.TryAdd(275, 535);
		DynamicMeshFile.TileLinks.TryAdd(276, 535);
		DynamicMeshFile.TileLinks.TryAdd(277, 535);
		DynamicMeshFile.TileLinks.TryAdd(278, 536);
		DynamicMeshFile.TileLinks.TryAdd(279, 536);
		DynamicMeshFile.TileLinks.TryAdd(28, 15);
		DynamicMeshFile.TileLinks.TryAdd(280, 536);
		DynamicMeshFile.TileLinks.TryAdd(281, 536);
		DynamicMeshFile.TileLinks.TryAdd(282, 537);
		DynamicMeshFile.TileLinks.TryAdd(283, 537);
		DynamicMeshFile.TileLinks.TryAdd(284, 537);
		DynamicMeshFile.TileLinks.TryAdd(285, 537);
		DynamicMeshFile.TileLinks.TryAdd(286, 538);
		DynamicMeshFile.TileLinks.TryAdd(287, 538);
		DynamicMeshFile.TileLinks.TryAdd(288, 538);
		DynamicMeshFile.TileLinks.TryAdd(289, 538);
		DynamicMeshFile.TileLinks.TryAdd(29, 21);
		DynamicMeshFile.TileLinks.TryAdd(290, 539);
		DynamicMeshFile.TileLinks.TryAdd(291, 539);
		DynamicMeshFile.TileLinks.TryAdd(292, 539);
		DynamicMeshFile.TileLinks.TryAdd(293, 539);
		DynamicMeshFile.TileLinks.TryAdd(294, 540);
		DynamicMeshFile.TileLinks.TryAdd(295, 540);
		DynamicMeshFile.TileLinks.TryAdd(296, 540);
		DynamicMeshFile.TileLinks.TryAdd(297, 540);
		DynamicMeshFile.TileLinks.TryAdd(298, 541);
		DynamicMeshFile.TileLinks.TryAdd(299, 542);
		DynamicMeshFile.TileLinks.TryAdd(3, 7);
		DynamicMeshFile.TileLinks.TryAdd(30, 22);
		DynamicMeshFile.TileLinks.TryAdd(300, 543);
		DynamicMeshFile.TileLinks.TryAdd(301, 544);
		DynamicMeshFile.TileLinks.TryAdd(302, 544);
		DynamicMeshFile.TileLinks.TryAdd(303, 544);
		DynamicMeshFile.TileLinks.TryAdd(304, 544);
		DynamicMeshFile.TileLinks.TryAdd(305, 545);
		DynamicMeshFile.TileLinks.TryAdd(306, 545);
		DynamicMeshFile.TileLinks.TryAdd(307, 545);
		DynamicMeshFile.TileLinks.TryAdd(308, 545);
		DynamicMeshFile.TileLinks.TryAdd(309, 546);
		DynamicMeshFile.TileLinks.TryAdd(31, 23);
		DynamicMeshFile.TileLinks.TryAdd(310, 546);
		DynamicMeshFile.TileLinks.TryAdd(311, 546);
		DynamicMeshFile.TileLinks.TryAdd(312, 546);
		DynamicMeshFile.TileLinks.TryAdd(313, 547);
		DynamicMeshFile.TileLinks.TryAdd(314, 548);
		DynamicMeshFile.TileLinks.TryAdd(315, 548);
		DynamicMeshFile.TileLinks.TryAdd(316, 548);
		DynamicMeshFile.TileLinks.TryAdd(317, 548);
		DynamicMeshFile.TileLinks.TryAdd(318, 549);
		DynamicMeshFile.TileLinks.TryAdd(319, 549);
		DynamicMeshFile.TileLinks.TryAdd(32, 23);
		DynamicMeshFile.TileLinks.TryAdd(320, 549);
		DynamicMeshFile.TileLinks.TryAdd(321, 549);
		DynamicMeshFile.TileLinks.TryAdd(322, 552);
		DynamicMeshFile.TileLinks.TryAdd(323, 553);
		DynamicMeshFile.TileLinks.TryAdd(324, 553);
		DynamicMeshFile.TileLinks.TryAdd(325, 553);
		DynamicMeshFile.TileLinks.TryAdd(326, 553);
		DynamicMeshFile.TileLinks.TryAdd(327, 554);
		DynamicMeshFile.TileLinks.TryAdd(328, 555);
		DynamicMeshFile.TileLinks.TryAdd(329, 571);
		DynamicMeshFile.TileLinks.TryAdd(33, 23);
		DynamicMeshFile.TileLinks.TryAdd(330, 571);
		DynamicMeshFile.TileLinks.TryAdd(331, 571);
		DynamicMeshFile.TileLinks.TryAdd(332, 571);
		DynamicMeshFile.TileLinks.TryAdd(333, 572);
		DynamicMeshFile.TileLinks.TryAdd(334, 580);
		DynamicMeshFile.TileLinks.TryAdd(335, 581);
		DynamicMeshFile.TileLinks.TryAdd(336, 582);
		DynamicMeshFile.TileLinks.TryAdd(337, 582);
		DynamicMeshFile.TileLinks.TryAdd(338, 582);
		DynamicMeshFile.TileLinks.TryAdd(339, 582);
		DynamicMeshFile.TileLinks.TryAdd(34, 23);
		DynamicMeshFile.TileLinks.TryAdd(340, 583);
		DynamicMeshFile.TileLinks.TryAdd(341, 584);
		DynamicMeshFile.TileLinks.TryAdd(342, 585);
		DynamicMeshFile.TileLinks.TryAdd(343, 585);
		DynamicMeshFile.TileLinks.TryAdd(344, 585);
		DynamicMeshFile.TileLinks.TryAdd(345, 585);
		DynamicMeshFile.TileLinks.TryAdd(346, 586);
		DynamicMeshFile.TileLinks.TryAdd(347, 587);
		DynamicMeshFile.TileLinks.TryAdd(348, 588);
		DynamicMeshFile.TileLinks.TryAdd(349, 589);
		DynamicMeshFile.TileLinks.TryAdd(35, 24);
		DynamicMeshFile.TileLinks.TryAdd(350, 590);
		DynamicMeshFile.TileLinks.TryAdd(351, 590);
		DynamicMeshFile.TileLinks.TryAdd(352, 590);
		DynamicMeshFile.TileLinks.TryAdd(353, 590);
		DynamicMeshFile.TileLinks.TryAdd(354, 591);
		DynamicMeshFile.TileLinks.TryAdd(355, 591);
		DynamicMeshFile.TileLinks.TryAdd(356, 591);
		DynamicMeshFile.TileLinks.TryAdd(357, 591);
		DynamicMeshFile.TileLinks.TryAdd(358, 592);
		DynamicMeshFile.TileLinks.TryAdd(359, 592);
		DynamicMeshFile.TileLinks.TryAdd(36, 25);
		DynamicMeshFile.TileLinks.TryAdd(360, 592);
		DynamicMeshFile.TileLinks.TryAdd(361, 592);
		DynamicMeshFile.TileLinks.TryAdd(362, 593);
		DynamicMeshFile.TileLinks.TryAdd(364, 597);
		DynamicMeshFile.TileLinks.TryAdd(365, 598);
		DynamicMeshFile.TileLinks.TryAdd(37, 43);
		DynamicMeshFile.TileLinks.TryAdd(38, 43);
		DynamicMeshFile.TileLinks.TryAdd(39, 43);
		DynamicMeshFile.TileLinks.TryAdd(4, 8);
		DynamicMeshFile.TileLinks.TryAdd(40, 43);
		DynamicMeshFile.TileLinks.TryAdd(41, 44);
		DynamicMeshFile.TileLinks.TryAdd(42, 46);
		DynamicMeshFile.TileLinks.TryAdd(43, 50);
		DynamicMeshFile.TileLinks.TryAdd(44, 50);
		DynamicMeshFile.TileLinks.TryAdd(45, 50);
		DynamicMeshFile.TileLinks.TryAdd(46, 50);
		DynamicMeshFile.TileLinks.TryAdd(47, 51);
		DynamicMeshFile.TileLinks.TryAdd(48, 51);
		DynamicMeshFile.TileLinks.TryAdd(49, 51);
		DynamicMeshFile.TileLinks.TryAdd(5, 8);
		DynamicMeshFile.TileLinks.TryAdd(50, 51);
		DynamicMeshFile.TileLinks.TryAdd(51, 52);
		DynamicMeshFile.TileLinks.TryAdd(52, 52);
		DynamicMeshFile.TileLinks.TryAdd(53, 52);
		DynamicMeshFile.TileLinks.TryAdd(54, 52);
		DynamicMeshFile.TileLinks.TryAdd(55, 53);
		DynamicMeshFile.TileLinks.TryAdd(56, 53);
		DynamicMeshFile.TileLinks.TryAdd(57, 53);
		DynamicMeshFile.TileLinks.TryAdd(58, 53);
		DynamicMeshFile.TileLinks.TryAdd(59, 54);
		DynamicMeshFile.TileLinks.TryAdd(6, 8);
		DynamicMeshFile.TileLinks.TryAdd(60, 55);
		DynamicMeshFile.TileLinks.TryAdd(61, 56);
		DynamicMeshFile.TileLinks.TryAdd(62, 57);
		DynamicMeshFile.TileLinks.TryAdd(63, 57);
		DynamicMeshFile.TileLinks.TryAdd(64, 57);
		DynamicMeshFile.TileLinks.TryAdd(65, 57);
		DynamicMeshFile.TileLinks.TryAdd(66, 58);
		DynamicMeshFile.TileLinks.TryAdd(67, 59);
		DynamicMeshFile.TileLinks.TryAdd(68, 61);
		DynamicMeshFile.TileLinks.TryAdd(69, 62);
		DynamicMeshFile.TileLinks.TryAdd(7, 8);
		DynamicMeshFile.TileLinks.TryAdd(70, 64);
		DynamicMeshFile.TileLinks.TryAdd(71, 65);
		DynamicMeshFile.TileLinks.TryAdd(72, 67);
		DynamicMeshFile.TileLinks.TryAdd(73, 73);
		DynamicMeshFile.TileLinks.TryAdd(74, 74);
		DynamicMeshFile.TileLinks.TryAdd(75, 74);
		DynamicMeshFile.TileLinks.TryAdd(76, 74);
		DynamicMeshFile.TileLinks.TryAdd(77, 74);
		DynamicMeshFile.TileLinks.TryAdd(78, 75);
		DynamicMeshFile.TileLinks.TryAdd(79, 75);
		DynamicMeshFile.TileLinks.TryAdd(8, 9);
		DynamicMeshFile.TileLinks.TryAdd(80, 75);
		DynamicMeshFile.TileLinks.TryAdd(81, 75);
		DynamicMeshFile.TileLinks.TryAdd(82, 76);
		DynamicMeshFile.TileLinks.TryAdd(83, 77);
		DynamicMeshFile.TileLinks.TryAdd(84, 77);
		DynamicMeshFile.TileLinks.TryAdd(85, 77);
		DynamicMeshFile.TileLinks.TryAdd(86, 77);
		DynamicMeshFile.TileLinks.TryAdd(87, 78);
		DynamicMeshFile.TileLinks.TryAdd(88, 79);
		DynamicMeshFile.TileLinks.TryAdd(89, 81);
		DynamicMeshFile.TileLinks.TryAdd(9, 11);
		DynamicMeshFile.TileLinks.TryAdd(90, 84);
		DynamicMeshFile.TileLinks.TryAdd(91, 116);
		DynamicMeshFile.TileLinks.TryAdd(92, 168);
		DynamicMeshFile.TileLinks.TryAdd(93, 168);
		DynamicMeshFile.TileLinks.TryAdd(94, 168);
		DynamicMeshFile.TileLinks.TryAdd(95, 168);
		DynamicMeshFile.TileLinks.TryAdd(96, 169);
		DynamicMeshFile.TileLinks.TryAdd(97, 169);
		DynamicMeshFile.TileLinks.TryAdd(98, 169);
		DynamicMeshFile.TileLinks.TryAdd(99, 169);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteVoxelMeshesToDisk(BinaryWriter writer, List<VoxelMesh> meshes)
	{
		UVRectTiling[] uvMapping = MeshDescription.meshes[0].textureAtlas.uvMapping;
		if (!DynamicMeshManager.Allow32BitMeshes)
		{
			Log.Error("Can't write combined meshes to disk unless in 32 bit mode");
			return;
		}
		int num = meshes.Sum((VoxelMesh d) => d.Vertices.Count);
		int num2 = meshes.Sum((VoxelMesh d) => d.Indices.Count);
		writer.Write((uint)num);
		for (int i = 0; i < meshes.Count; i++)
		{
			ArrayListMP<Vector3> vertices = meshes[i].Vertices;
			for (int j = 0; j < vertices.Count; j++)
			{
				writer.Write((short)((double)vertices[j].x * 100.0));
				writer.Write((short)((double)vertices[j].y * 100.0));
				writer.Write((short)((double)vertices[j].z * 100.0));
			}
		}
		writer.Write((uint)num);
		for (int k = 0; k < meshes.Count; k++)
		{
			VoxelMesh voxelMesh = meshes[k];
			ArrayListMP<Color> colorVertices = voxelMesh.ColorVertices;
			ArrayListMP<Vector2> uvs = voxelMesh.Uvs;
			for (int l = 0; l < colorVertices.Count; l++)
			{
				int num3 = (int)colorVertices[l].g;
				int num4 = -1;
				if (!DynamicMeshFile.TileLinks.TryGetValue(num3, out num4))
				{
					for (int m = 0; m < uvMapping.Length; m++)
					{
						UVRectTiling uvrectTiling = uvMapping[m];
						if (uvrectTiling.index == num3 || m + 1 >= uvMapping.Length || (float)uvrectTiling.index + uvrectTiling.uv.width * uvrectTiling.uv.height > (float)num3)
						{
							num4 = m;
							DynamicMeshFile.TileLinks.TryAdd(num3, num4);
							break;
						}
					}
					if (num4 == -1)
					{
						num4 = 0;
						DynamicMeshFile.TileLinks.TryAdd(num3, num4);
					}
				}
				if (num4 == -1)
				{
					num4 = 0;
				}
				writer.Write((short)num4);
				writer.Write((byte)(num3 - uvMapping[num4].index));
				bool flag = (double)colorVertices[l].a > 0.5;
				writer.Write(flag);
				writer.Write((ushort)((double)uvs[l].x * 10000.0));
				writer.Write((ushort)((double)uvs[l].y * 10000.0));
			}
		}
		writer.Write((uint)num2);
		int num5 = 0;
		for (int n = 0; n < meshes.Count; n++)
		{
			ArrayListMP<int> indices = meshes[n].Indices;
			for (int num6 = 0; num6 < indices.Count; num6++)
			{
				writer.Write(indices[num6] + num5);
			}
			num5 += meshes[n].Vertices.Count;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int WriteVoxelMeshToDisk(BinaryWriter writer, VoxelMesh mesh, UVRectTiling[] tiling, float yOff, DynamicMeshChunkProcessor builder, int vertOffset)
	{
		ArrayListMP<Vector3> vertices = mesh.Vertices;
		ArrayListMP<int> indices = mesh.Indices;
		ArrayListMP<Vector2> uvs = mesh.Uvs;
		ArrayListMP<Color> colorVertices = mesh.ColorVertices;
		int num = vertices.Count - vertOffset;
		int num2 = (DynamicMeshManager.Allow32BitMeshes ? int.MaxValue : 65535);
		if (num > num2)
		{
			Log.Warning("OVER MAX VERTS: ATTEMPTING SPLIT: " + num.ToString() + "   triangles: " + indices.Count.ToString());
			num = num2;
		}
		int num3 = vertOffset + num;
		writer.Write((uint)num);
		for (int i = vertOffset; i < num3; i++)
		{
			writer.Write((short)((double)vertices[i].x * 100.0));
			writer.Write((short)((double)vertices[i].y * 100.0));
			writer.Write((short)((double)vertices[i].z * 100.0));
		}
		writer.Write((uint)num);
		for (int j = vertOffset; j < num3; j++)
		{
			int num4 = (int)colorVertices[j].g;
			int num5 = -1;
			if (!DynamicMeshFile.TileLinks.TryGetValue(num4, out num5))
			{
				for (int k = 0; k < tiling.Length; k++)
				{
					UVRectTiling uvrectTiling = tiling[k];
					if (uvrectTiling.index == num4 || k + 1 >= tiling.Length || (float)uvrectTiling.index + uvrectTiling.uv.width * uvrectTiling.uv.height > (float)num4)
					{
						num5 = k;
						DynamicMeshFile.TileLinks.TryAdd(num4, num5);
						break;
					}
				}
				if (num5 == -1)
				{
					num5 = 0;
					DynamicMeshFile.TileLinks.TryAdd(num4, num5);
				}
			}
			if (num5 == -1)
			{
				num5 = 0;
			}
			writer.Write((short)num5);
			writer.Write((byte)(num4 - tiling[num5].index));
			bool flag = (double)colorVertices[j].a > 0.5;
			writer.Write(flag);
			writer.Write((ushort)((double)uvs[j].x * 10000.0));
			writer.Write((ushort)((double)uvs[j].y * 10000.0));
		}
		int num6 = indices.Count - vertOffset;
		if (num6 > num2 && num == num2)
		{
			num6 = num2;
		}
		writer.Write((uint)num6);
		for (int l = vertOffset; l < vertOffset + num6; l++)
		{
			writer.Write((ushort)(indices[l] - vertOffset));
		}
		if (vertOffset + num < vertices.Count)
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("WRITING SECOND MESH");
			}
			if (vertOffset > 0)
			{
				throw new NotImplementedException("Can't write more than two dynamic meshes per VoxelMesh");
			}
			num += DynamicMeshFile.WriteVoxelMeshToDisk(writer, mesh, tiling, yOff, builder, vertOffset + num);
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteTerrainVoxelMeshesToDisk(BinaryWriter writer, List<VoxelMeshTerrain> meshes)
	{
		if (!DynamicMeshManager.Allow32BitMeshes)
		{
			Log.Error("Can't write combined meshes to disk unless in 32 bit mode");
			return;
		}
		int num = meshes.Sum((VoxelMeshTerrain d) => d.Vertices.Count);
		int num2 = meshes.Sum((VoxelMeshTerrain d) => d.Indices.Count);
		int num3 = meshes.Sum((VoxelMeshTerrain d) => d.submeshes.Sum((TerrainSubMesh e) => e.triangles.Count));
		int num4 = meshes.Sum((VoxelMeshTerrain d) => d.ColorVertices.Count);
		if (num != num4)
		{
			Log.Error("Invalid colours");
		}
		int num5 = meshes.Sum((VoxelMeshTerrain d) => d.Uvs.Count);
		int num6 = meshes.Sum((VoxelMeshTerrain d) => d.UvsCrack.Count);
		int num7 = meshes.Sum((VoxelMeshTerrain d) => d.Uvs3.Count);
		int num8 = meshes.Sum((VoxelMeshTerrain d) => d.Uvs4.Count);
		writer.Write((uint)num);
		for (int i = 0; i < meshes.Count; i++)
		{
			ArrayListMP<Vector3> vertices = meshes[i].Vertices;
			for (int j = 0; j < vertices.Count; j++)
			{
				writer.Write((short)((double)vertices[j].x * 100.0));
				writer.Write((short)((double)vertices[j].y * 100.0));
				writer.Write((short)((double)vertices[j].z * 100.0));
			}
		}
		writer.Write((uint)num5);
		for (int k = 0; k < meshes.Count; k++)
		{
			ArrayListMP<Vector2> uvs = meshes[k].Uvs;
			for (int l = 0; l < uvs.Count; l++)
			{
				writer.Write((ushort)((double)uvs[l].x * 10000.0));
				writer.Write((ushort)((double)uvs[l].y * 10000.0));
			}
		}
		writer.Write((uint)num6);
		for (int m = 0; m < meshes.Count; m++)
		{
			ArrayListMP<Vector2> uvsCrack = meshes[m].UvsCrack;
			for (int n = 0; n < uvsCrack.Count; n++)
			{
				writer.Write((ushort)((double)uvsCrack[n].x * 10000.0));
				writer.Write((ushort)((double)uvsCrack[n].y * 10000.0));
			}
		}
		writer.Write((uint)num7);
		for (int num9 = 0; num9 < meshes.Count; num9++)
		{
			ArrayListMP<Vector2> uvs2 = meshes[num9].Uvs3;
			for (int num10 = 0; num10 < uvs2.Count; num10++)
			{
				writer.Write((ushort)((double)uvs2[num10].x * 10000.0));
				writer.Write((ushort)((double)uvs2[num10].y * 10000.0));
			}
		}
		writer.Write((uint)num8);
		for (int num11 = 0; num11 < meshes.Count; num11++)
		{
			ArrayListMP<Vector2> uvs3 = meshes[num11].Uvs4;
			for (int num12 = 0; num12 < uvs3.Count; num12++)
			{
				writer.Write((ushort)((double)uvs3[num12].x * 10000.0));
				writer.Write((ushort)((double)uvs3[num12].y * 10000.0));
			}
		}
		int num13 = 1;
		writer.Write((uint)num13);
		writer.Write((uint)num2);
		int num14 = 0;
		if (num2 % 3 != 0 || num3 % 3 != 0)
		{
			Log.Out("Weird triangles");
		}
		int num15 = 0;
		while (num15 < meshes.Count)
		{
			VoxelMeshTerrain voxelMeshTerrain = meshes[num15];
			if (voxelMeshTerrain.submeshes.Count > 0)
			{
				using (List<TerrainSubMesh>.Enumerator enumerator = voxelMeshTerrain.submeshes.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						TerrainSubMesh terrainSubMesh = enumerator.Current;
						for (int num16 = 0; num16 < terrainSubMesh.triangles.Count; num16++)
						{
							writer.Write(terrainSubMesh.triangles[num16] + num14);
						}
					}
					goto IL_04E3;
				}
				goto IL_04B1;
			}
			goto IL_04B1;
			IL_04E3:
			num14 += voxelMeshTerrain.Vertices.Count;
			num15++;
			continue;
			IL_04B1:
			for (int num17 = 0; num17 < voxelMeshTerrain.Indices.Count; num17++)
			{
				writer.Write(voxelMeshTerrain.Indices[num17] + num14);
			}
			goto IL_04E3;
		}
		writer.Write((uint)num);
		for (int num18 = 0; num18 < meshes.Count; num18++)
		{
			ArrayListMP<Color> colorVertices = meshes[num18].ColorVertices;
			for (int num19 = 0; num19 < colorVertices.Count; num19++)
			{
				Color color = colorVertices[num19];
				writer.Write((ushort)(color.r * 10000f));
				writer.Write((ushort)(color.g * 10000f));
				writer.Write((ushort)(color.b * 10000f));
				writer.Write((ushort)(color.a * 10000f));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void WriteTerrainVoxelMeshToDisk(BinaryWriter writer, VoxelMeshTerrain mesh, float yOff, DynamicMeshChunkProcessor builder, DynamicMeshRegion region, int vertOffset)
	{
		ArrayListMP<Vector3> vertices = mesh.Vertices;
		ArrayListMP<int> indices = mesh.Indices;
		ArrayListMP<Vector2> uvs = mesh.Uvs;
		ArrayListMP<Vector2> uvsCrack = mesh.UvsCrack;
		ArrayListMP<Vector2> uvs2 = mesh.Uvs3;
		ArrayListMP<Vector2> uvs3 = mesh.Uvs4;
		ArrayListMP<Color> colorVertices = mesh.ColorVertices;
		int num = vertices.Count - vertOffset;
		int num2 = (DynamicMeshManager.Allow32BitMeshes ? int.MaxValue : 65535);
		if (num > num2)
		{
			Log.Warning("OVER MAX VERTS: ATTEMPTING SPLIT: " + num.ToString() + "   triangles: " + indices.Count.ToString());
			num = num2;
		}
		int num3 = vertOffset + num;
		writer.Write((uint)num);
		for (int i = vertOffset; i < num3; i++)
		{
			writer.Write((short)((double)vertices[i].x * 100.0));
			writer.Write((short)((double)vertices[i].y * 100.0));
			writer.Write((short)((double)vertices[i].z * 100.0));
		}
		writer.Write((uint)num);
		for (int j = vertOffset; j < num3; j++)
		{
			writer.Write((ushort)((double)uvs[j].x * 10000.0));
			writer.Write((ushort)((double)uvs[j].y * 10000.0));
		}
		writer.Write((uint)uvsCrack.Count);
		for (int k = 0; k < uvsCrack.Count; k++)
		{
			writer.Write((ushort)((double)uvsCrack[k].x * 10000.0));
			writer.Write((ushort)((double)uvsCrack[k].y * 10000.0));
		}
		writer.Write((uint)uvs2.Count);
		for (int l = 0; l < uvs2.Count; l++)
		{
			writer.Write((ushort)((double)uvs2[l].x * 10000.0));
			writer.Write((ushort)((double)uvs2[l].y * 10000.0));
		}
		writer.Write((uint)uvs3.Count);
		for (int m = 0; m < uvs3.Count; m++)
		{
			writer.Write((ushort)((double)uvs3[m].x * 10000.0));
			writer.Write((ushort)((double)uvs3[m].y * 10000.0));
		}
		int num4 = 1;
		writer.Write((uint)num4);
		ArrayListMP<int> indices2 = mesh.Indices;
		int count = indices2.Count;
		writer.Write((uint)count);
		for (int n = 0; n < count; n++)
		{
			writer.Write((ushort)(indices2[n] - vertOffset));
		}
		int num5 = colorVertices.Count - vertOffset;
		if (num5 > num2 && num == num2)
		{
			num5 = num2;
		}
		writer.Write((uint)num5);
		for (int num6 = vertOffset; num6 < vertOffset + num5; num6++)
		{
			Color color = colorVertices[num6];
			writer.Write((ushort)(color.r * 10000f));
			writer.Write((ushort)(color.g * 10000f));
			writer.Write((ushort)(color.b * 10000f));
			writer.Write((ushort)(color.a * 10000f));
		}
		if (vertOffset + num < vertices.Count)
		{
			if (DynamicMeshManager.DoLog)
			{
				DynamicMeshManager.LogMsg("WRITING SECOND MESH");
			}
			if (vertOffset > 0)
			{
				throw new NotImplementedException("Can't write more than two dynamic meshes per VoxelMesh");
			}
			DynamicMeshFile.WriteTerrainVoxelMeshToDisk(writer, mesh, yOff, builder, region, vertOffset + num);
		}
	}

	public static void ReadItemMesh(BinaryReader reader, DynamicMeshItem item, bool isVisible)
	{
		if (reader.BaseStream.Position == reader.BaseStream.Length)
		{
			Log.Out("ReadItemMesh Zero length: " + item.ToDebugLocation());
			return;
		}
		int num = reader.ReadInt32();
		int num2 = reader.ReadInt32();
		int num3 = reader.ReadInt32();
		item.WorldPosition.x = num;
		item.WorldPosition.y = num2;
		item.WorldPosition.z = num3;
		int num4 = reader.ReadInt32();
		item.UpdateTime = num4;
		GameObject gameObject = DynamicMeshItem.GetItemMeshRendererFromPool();
		int num5 = reader.ReadInt32();
		for (int i = 0; i < num5; i++)
		{
			GameObject gameObject2 = ((i == 0) ? gameObject : DynamicMeshItem.GetItemMeshRendererFromPool());
			MeshFilter component = gameObject2.GetComponent<MeshFilter>();
			Mesh mesh = component.mesh ?? new Mesh();
			if (i != 0)
			{
				gameObject2.transform.parent = gameObject.transform;
			}
			mesh = DynamicMeshFile.ReadMesh(3, reader, MeshDescription.meshes[0].textureAtlas.uvMapping, mesh, false, item);
			if (item.State == DynamicItemState.Invalid)
			{
				DynamicMeshItem.AddToMeshPool(gameObject2);
				if (i == 0)
				{
					DynamicMeshManager.Instance.RemoveItem(item, true);
					gameObject = null;
					IL_022A:
					DynamicMeshItem.AddToMeshPool(item.ChunkObject);
					item.ChunkObject = gameObject;
					item.State = DynamicItemState.Loaded;
					item.SetVisible(isVisible, "read item mesh");
					return;
				}
			}
			else if (mesh.vertexCount == 0)
			{
				DynamicMeshItem.AddToMeshPool(gameObject2);
			}
			else
			{
				gameObject2.SetActive(true);
				mesh.RecalculateBounds();
				mesh.RecalculateNormals();
				mesh.RecalculateTangents();
				component.mesh = mesh;
			}
		}
		if (reader.BaseStream.Position != reader.BaseStream.Length)
		{
			int num6 = reader.ReadInt32();
			for (int j = 0; j < num6; j++)
			{
				GameObject itemMeshRendererFromPool = DynamicMeshItem.GetItemMeshRendererFromPool();
				MeshFilter component2 = itemMeshRendererFromPool.GetComponent<MeshFilter>();
				Mesh mesh2 = component2.mesh ?? new Mesh();
				MeshRenderer component3 = itemMeshRendererFromPool.GetComponent<MeshRenderer>();
				itemMeshRendererFromPool.transform.parent = gameObject.transform;
				if (DynamicMeshManager.DebugItemPositions)
				{
					itemMeshRendererFromPool.name = "Terrain";
				}
				mesh2 = DynamicMeshFile.ReadMeshTerrain(reader, mesh2, component3, false);
				if (mesh2.vertexCount == 0)
				{
					DynamicMeshItem.AddToMeshPool(itemMeshRendererFromPool);
				}
				else
				{
					itemMeshRendererFromPool.SetActive(true);
					mesh2.RecalculateBounds();
					mesh2.RecalculateNormals();
					mesh2.RecalculateTangents();
					component2.mesh = mesh2;
				}
			}
			goto IL_022A;
		}
		goto IL_022A;
	}

	public static Material GetOpaqueMaterial(bool isRegion)
	{
		Material material = (isRegion ? DynamicMeshFile._RegionMaterial : DynamicMeshFile._itemMaterial);
		if (material != null)
		{
			return material;
		}
		MeshDescription meshDescription = MeshDescription.meshes[0];
		material = Resources.Load<Material>("Materials/DistantPOI_TA_DM");
		material = Object.Instantiate<Material>(material);
		material.SetTexture("_MainTex", meshDescription.TexDiffuse);
		material.SetTexture("_Normal", meshDescription.TexNormal);
		material.SetTexture("_MetallicGlossMap", meshDescription.TexSpecular);
		material.SetTexture("_OcclusionMap", meshDescription.TexOcclusion);
		return material;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static VoxelMesh ReadVoxelMesh(BinaryReader reader, UVRectTiling[] tilings, Vector3i worldPos)
	{
		if (reader.BaseStream.Length == reader.BaseStream.Position)
		{
			Log.Out("Read voxel mesh error: end of stream");
			return DynamicMeshFile.GetMeshFromPool(1);
		}
		int num = 0;
		uint num2 = 0U;
		try
		{
			num2 = reader.ReadUInt32();
			bool flag = num2 > 65535U;
			num = 1;
			VoxelMesh meshFromPool = DynamicMeshFile.GetMeshFromPool((int)num2);
			if (num2 == 0U)
			{
				return meshFromPool;
			}
			for (uint num3 = 0U; num3 < num2; num3 += 1U)
			{
				meshFromPool.Vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			}
			num = 2;
			uint num4 = reader.ReadUInt32();
			num = 3;
			for (uint num5 = 0U; num5 < num4; num5 += 1U)
			{
				int num6 = (int)reader.ReadInt16();
				int num7 = (int)reader.ReadByte();
				int num8 = ((num6 >= tilings.Length) ? 0 : (tilings[num6].index + num7));
				bool flag2 = reader.ReadBoolean();
				meshFromPool.Uvs.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
				meshFromPool.ColorVertices.Add(new Color(0f, (float)num8, 0f, (float)((!flag2) ? 0 : 1)));
			}
			num = 4;
			uint num9 = reader.ReadUInt32();
			num = 5;
			for (uint num10 = 0U; num10 < num9; num10 += 1U)
			{
				if (flag)
				{
					meshFromPool.Indices.Add(reader.ReadInt32());
				}
				else
				{
					meshFromPool.Indices.Add((int)reader.ReadUInt16());
				}
			}
			num = 6;
			return meshFromPool;
		}
		catch (Exception ex)
		{
			string[] array = new string[8];
			array[0] = "Read voxel mesh error: ";
			int num11 = 1;
			Vector3i vector3i = worldPos;
			array[num11] = vector3i.ToString();
			array[2] = " verts: ";
			array[3] = num2.ToString();
			array[4] = " stage:  ";
			array[5] = num.ToString();
			array[6] = "  message: ";
			array[7] = ex.Message;
			Log.Out(string.Concat(array));
		}
		return DynamicMeshFile.GetMeshFromPool(1);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static VoxelMeshTerrain ReadVoxelMeshTerrain(BinaryReader reader, Vector3i worldPos, out bool cancelLoad)
	{
		cancelLoad = false;
		if (reader.BaseStream.Length == reader.BaseStream.Position)
		{
			string text = "Read voxel mesh terrain error: end of stream: ";
			Vector3i vector3i = worldPos;
			Log.Out(text + vector3i.ToString());
			cancelLoad = true;
			return DynamicMeshFile.GetTerrainMeshFromPool(1);
		}
		int num = 0;
		uint num2 = 0U;
		try
		{
			num2 = reader.ReadUInt32();
			bool flag = num2 > 65535U;
			num = 1;
			VoxelMeshTerrain terrainMeshFromPool = DynamicMeshFile.GetTerrainMeshFromPool((int)num2);
			for (uint num3 = 0U; num3 < num2; num3 += 1U)
			{
				terrainMeshFromPool.Vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			}
			num = 2;
			uint num4 = reader.ReadUInt32();
			for (uint num5 = 0U; num5 < num4; num5 += 1U)
			{
				terrainMeshFromPool.Uvs.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			}
			num4 = reader.ReadUInt32();
			for (uint num6 = 0U; num6 < num4; num6 += 1U)
			{
				terrainMeshFromPool.UvsCrack.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			}
			num4 = reader.ReadUInt32();
			for (uint num7 = 0U; num7 < num4; num7 += 1U)
			{
				terrainMeshFromPool.Uvs3.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			}
			num4 = reader.ReadUInt32();
			for (uint num8 = 0U; num8 < num4; num8 += 1U)
			{
				terrainMeshFromPool.Uvs4.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			}
			num = 4;
			uint num9 = reader.ReadUInt32();
			int num10 = 0;
			while ((long)num10 < (long)((ulong)num9))
			{
				terrainMeshFromPool.submeshes.Add(new TerrainSubMesh(terrainMeshFromPool.submeshes, 0));
				TerrainSubMesh terrainSubMesh = terrainMeshFromPool.submeshes[num10];
				uint num11 = reader.ReadUInt32();
				num = 5;
				for (uint num12 = 0U; num12 < num11; num12 += 1U)
				{
					if (flag)
					{
						terrainSubMesh.triangles.Add(reader.ReadInt32());
					}
					else
					{
						terrainSubMesh.triangles.Add((int)reader.ReadUInt16());
					}
				}
				num10++;
			}
			num = 6;
			uint num13 = reader.ReadUInt32();
			for (uint num14 = 0U; num14 < num13; num14 += 1U)
			{
				terrainMeshFromPool.ColorVertices.Add(new Color((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			}
			return terrainMeshFromPool;
		}
		catch (Exception ex)
		{
			string[] array = new string[8];
			array[0] = "Read voxel terrain mesh error: ";
			int num15 = 1;
			Vector3i vector3i = worldPos;
			array[num15] = vector3i.ToString();
			array[2] = " verts: ";
			array[3] = num2.ToString();
			array[4] = " stage:  ";
			array[5] = num.ToString();
			array[6] = "  message: ";
			array[7] = ex.Message;
			Log.Out(string.Concat(array));
		}
		cancelLoad = true;
		return DynamicMeshFile.GetTerrainMeshFromPool(1);
	}

	public static void SetMeshMax(int max, bool orMore)
	{
		if (orMore && DynamicMeshFile.ReadMeshMax >= max)
		{
			return;
		}
		DynamicMeshFile.ReadMeshMax = max;
	}

	public static IEnumerator ReadMeshCoroutine(int version, BinaryReader reader, UVRectTiling[] tilings, Mesh mesh, DynamicMeshRegion region, DynamicMeshItem item, int maxTime)
	{
		MeshLists cache = MeshLists.GetList();
		List<Vector3> vertices = cache.Vertices;
		List<int> triangles = cache.Triangles;
		List<Vector2> uvs = cache.Uvs;
		List<Color> colours = cache.Colours;
		if (reader.BaseStream.Position == reader.BaseStream.Length)
		{
			Log.Warning("Corrupted mesh file");
			MeshLists.ReturnList(cache);
			yield break;
		}
		uint vertCount = reader.ReadUInt32();
		bool is32Bit = vertCount > 65535U && DynamicMeshManager.Allow32BitMeshes;
		mesh.indexFormat = (is32Bit ? 1 : 0);
		if (!DynamicMeshManager.CompressFiles && reader.BaseStream.Position + (long)((ulong)(vertCount * 6U)) > reader.BaseStream.Length)
		{
			Log.Error(string.Concat(new string[]
			{
				"Not enough data in file for verts. Position: ",
				reader.BaseStream.Position.ToString(),
				" expecting: ",
				(vertCount * 6U).ToString(),
				" Length: ",
				reader.BaseStream.Length.ToString()
			}));
			if (region != null)
			{
				region.OnCorrupted();
			}
			if (item != null)
			{
				item.OnCorrupted();
			}
			MeshLists.ReturnList(cache);
			yield break;
		}
		for (uint vCount = 0U; vCount < vertCount; vCount += 1U)
		{
			vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
			{
				yield return DynamicMeshFile.ReadMeshWait;
				DynamicMeshFile.stop.ResetAndRestart();
			}
		}
		uint uvCount = reader.ReadUInt32();
		if (!DynamicMeshManager.CompressFiles && reader.BaseStream.Position + (long)((ulong)(uvCount * 8U)) > reader.BaseStream.Length)
		{
			Log.Error(string.Concat(new string[]
			{
				"Not enough data in file for uv.Position: ",
				reader.BaseStream.Position.ToString(),
				" expecting: ",
				(uvCount * 8U).ToString(),
				" Length: ",
				reader.BaseStream.Length.ToString()
			}));
			if (region != null)
			{
				region.OnCorrupted();
			}
			if (item != null)
			{
				item.OnCorrupted();
			}
			MeshLists.ReturnList(cache);
			yield break;
		}
		for (uint vCount = 0U; vCount < uvCount; vCount += 1U)
		{
			int num = (int)reader.ReadInt16();
			int num2 = (int)reader.ReadByte();
			int num3 = ((num >= tilings.Length) ? 0 : (tilings[num].index + num2));
			bool flag = reader.ReadBoolean();
			uvs.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			colours.Add(new Color(0f, (float)num3, 0f, (float)((!flag) ? 0 : 1)));
			if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
			{
				yield return DynamicMeshFile.ReadMeshWait;
				DynamicMeshFile.stop.ResetAndRestart();
			}
		}
		uint triangleCount = reader.ReadUInt32();
		if (reader.BaseStream.Position + (long)((ulong)(triangleCount * 2U)) > reader.BaseStream.Length)
		{
			Log.Error(string.Concat(new string[]
			{
				"Not enough data in file for triangles.Position: ",
				reader.BaseStream.Position.ToString(),
				" expecting: ",
				(triangleCount * 2U).ToString(),
				" Length: ",
				reader.BaseStream.Length.ToString()
			}));
			if (region != null)
			{
				region.OnCorrupted();
			}
			if (item != null)
			{
				item.OnCorrupted();
			}
			MeshLists.ReturnList(cache);
			yield break;
		}
		for (uint vCount = 0U; vCount < triangleCount; vCount += 1U)
		{
			if (is32Bit)
			{
				triangles.Add(reader.ReadInt32());
			}
			else
			{
				triangles.Add((int)reader.ReadUInt16());
			}
			if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
			{
				yield return DynamicMeshFile.ReadMeshWait;
				DynamicMeshFile.stop.ResetAndRestart();
			}
		}
		if (vertices.Count != colours.Count)
		{
			string text = ((item == null) ? ("R" + region.ToDebugLocation()) : ("I" + item.ToDebugLocation()));
			Log.Warning(string.Concat(new string[]
			{
				"Count mismatch in mesh ",
				text,
				"     ",
				vertices.Count.ToString(),
				" vs ",
				colours.Count.ToString()
			}));
		}
		if (triangles.Count % 3 != 0)
		{
			string text2 = ((item == null) ? ("R" + region.ToDebugLocation()) : ("I" + item.ToDebugLocation()));
			Log.Warning(string.Concat(new string[]
			{
				"Triangle mismatch in mesh ",
				text2,
				"     ",
				vertices.Count.ToString(),
				" vs ",
				colours.Count.ToString()
			}));
		}
		GameUtils.SetMeshVertexAttributes(mesh, true);
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.RecalculateNormals();
		mesh.SetColors(colours);
		if (region != null)
		{
			mesh.UploadMeshData(true);
		}
		if (item != null)
		{
			Bounds boundsFromVerts = DynamicMeshFile.GetBoundsFromVerts(vertices);
			bool flag2 = boundsFromVerts.size.z > 0f;
			bool flag3 = boundsFromVerts.size.x > 0f;
			if (!flag3 || !flag2)
			{
				item.State = DynamicItemState.Invalid;
				if (DynamicMeshManager.DoLog)
				{
					Log.Out(string.Format("Invalid mesh at {0} depth :{1} width: {2}. Marking for deletion", item.ToDebugLocation(), flag2, flag3));
				}
			}
		}
		MeshLists.ReturnList(cache);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mesh ReadMesh(int version, BinaryReader reader, UVRectTiling[] tilings, Mesh mesh, bool lockMesh, DynamicMeshItem item)
	{
		uint num = reader.ReadUInt32();
		bool flag = num > 65535U && DynamicMeshManager.Allow32BitMeshes;
		mesh.indexFormat = (flag ? 1 : 0);
		MeshLists list = MeshLists.GetList();
		List<Vector3> vertices = list.Vertices;
		List<Color> colours = list.Colours;
		List<int> triangles = list.Triangles;
		List<Vector2> uvs = list.Uvs;
		for (uint num2 = 0U; num2 < num; num2 += 1U)
		{
			vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
		}
		uint num3 = reader.ReadUInt32();
		for (uint num4 = 0U; num4 < num3; num4 += 1U)
		{
			int num5 = (int)reader.ReadInt16();
			int num6 = (int)reader.ReadByte();
			int num7 = ((num5 >= tilings.Length) ? 0 : (tilings[num5].index + num6));
			bool flag2 = reader.ReadBoolean();
			uvs.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			colours.Add(new Color(0f, (float)num7, 0f, (float)((!flag2) ? 0 : 1)));
		}
		uint num8 = reader.ReadUInt32();
		for (uint num9 = 0U; num9 < num8; num9 += 1U)
		{
			if (flag)
			{
				triangles.Add(reader.ReadInt32());
			}
			else
			{
				triangles.Add((int)reader.ReadUInt16());
			}
		}
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		if (item != null)
		{
			Bounds boundsFromVerts = DynamicMeshFile.GetBoundsFromVerts(vertices);
			bool flag3 = boundsFromVerts.size.z > 0f;
			bool flag4 = boundsFromVerts.size.x > 0f;
			if (!flag4 || !flag3)
			{
				item.State = DynamicItemState.Invalid;
				if (DynamicMeshManager.DoLog)
				{
					Log.Out(string.Format("Invalid mesh at {0} depth :{1} width: {2}. Marking for deletion", item.ToDebugLocation(), flag3, flag4));
				}
				MeshLists.ReturnList(list);
				return null;
			}
		}
		GameUtils.SetMeshVertexAttributes(mesh, true);
		mesh.SetVertices(vertices);
		mesh.SetTriangles(triangles, 0);
		mesh.SetUVs(0, uvs);
		mesh.RecalculateNormals();
		mesh.SetColors(colours);
		if (lockMesh)
		{
			mesh.UploadMeshData(true);
		}
		MeshLists.ReturnList(list);
		return mesh;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mesh ReadMeshTerrain(BinaryReader reader, Mesh mesh, MeshRenderer rend, bool lockMesh)
	{
		uint num = reader.ReadUInt32();
		bool flag = num > 65535U && DynamicMeshManager.Allow32BitMeshes;
		mesh.indexFormat = (flag ? 1 : 0);
		MeshLists list = MeshLists.GetList();
		List<Vector3> vertices = list.Vertices;
		List<Color> colours = list.Colours;
		List<List<int>> terrainTriangles = list.TerrainTriangles;
		List<Vector2> uvs = list.Uvs;
		List<Vector2> uvs2 = list.Uvs2;
		List<Vector2> uvs3 = list.Uvs3;
		List<Vector2> uvs4 = list.Uvs4;
		for (uint num2 = 0U; num2 < num; num2 += 1U)
		{
			vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
		}
		uint num3 = reader.ReadUInt32();
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
			if (terrainTriangles.Count < num9 + 1)
			{
				terrainTriangles.Add(new List<int>());
			}
			List<int> list2 = terrainTriangles[num9];
			for (uint num11 = 0U; num11 < num10; num11 += 1U)
			{
				if (flag)
				{
					list2.Add(reader.ReadInt32());
				}
				else
				{
					list2.Add((int)reader.ReadUInt16());
				}
			}
			num9++;
		}
		uint num12 = reader.ReadUInt32();
		for (uint num13 = 0U; num13 < num12; num13 += 1U)
		{
			colours.Add(new Color((float)reader.ReadUInt16() / 10000f + 1f, (float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
		}
		int num14 = rend.sharedMaterials.Length;
		if (!DynamicMeshFile.TerrainSharedMaterials.ContainsKey(num14))
		{
			Material[] array = new Material[num14];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = MeshDescription.meshes[5].material;
			}
			DynamicMeshFile.TerrainSharedMaterials.Add(num14, array);
		}
		rend.sharedMaterials = DynamicMeshFile.TerrainSharedMaterials[num14];
		mesh.SetVertices(vertices);
		mesh.subMeshCount = (int)num8;
		int num15 = 0;
		while ((long)num15 < (long)((ulong)num8))
		{
			mesh.SetTriangles(terrainTriangles[num15], num15);
			num15++;
		}
		GameUtils.SetMeshVertexAttributes(mesh, true);
		mesh.SetUVs(0, uvs);
		mesh.SetUVs(1, uvs2);
		mesh.SetUVs(2, uvs3);
		mesh.SetUVs(3, uvs4);
		mesh.RecalculateNormals();
		mesh.SetColors(colours);
		if (lockMesh)
		{
			mesh.UploadMeshData(true);
		}
		MeshLists.ReturnList(list);
		return mesh;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator ReadMeshTerrainCoroutine(BinaryReader reader, Mesh mesh, MeshRenderer rend, bool lockMesh, int maxTime, DynamicMeshRegion region, DynamicMeshItem item)
	{
		DynamicMeshFile.CurrentlyLoadingItem = ((region == null) ? item : region);
		MeshLists cache = MeshLists.GetList();
		List<Vector3> vertices = cache.Vertices;
		List<List<int>> triangles = cache.TerrainTriangles;
		List<Color> colours = cache.Colours;
		List<Vector2> uvs = cache.Uvs;
		List<Vector2> uvs2 = cache.Uvs2;
		List<Vector2> uvs3 = cache.Uvs3;
		List<Vector2> uvs4 = cache.Uvs4;
		uint vertCount = reader.ReadUInt32();
		bool is32Bit = vertCount > 65535U && DynamicMeshManager.Allow32BitMeshes;
		mesh.indexFormat = (is32Bit ? 1 : 0);
		for (uint vCount = 0U; vCount < vertCount; vCount += 1U)
		{
			vertices.Add(new Vector3((float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f, (float)reader.ReadInt16() / 100f));
			if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
			{
				yield return DynamicMeshFile.ReadMeshWait;
				DynamicMeshFile.stop.ResetAndRestart();
			}
		}
		uint uvCount = reader.ReadUInt32();
		for (uint vCount = 0U; vCount < uvCount; vCount += 1U)
		{
			uvs.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
			{
				yield return DynamicMeshFile.ReadMeshWait;
				DynamicMeshFile.stop.ResetAndRestart();
			}
		}
		uvCount = reader.ReadUInt32();
		for (uint vCount = 0U; vCount < uvCount; vCount += 1U)
		{
			uvs2.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
			{
				yield return DynamicMeshFile.ReadMeshWait;
				DynamicMeshFile.stop.ResetAndRestart();
			}
		}
		uvCount = reader.ReadUInt32();
		for (uint vCount = 0U; vCount < uvCount; vCount += 1U)
		{
			uvs3.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
			{
				yield return DynamicMeshFile.ReadMeshWait;
				DynamicMeshFile.stop.ResetAndRestart();
			}
		}
		uvCount = reader.ReadUInt32();
		for (uint vCount = 0U; vCount < uvCount; vCount += 1U)
		{
			uvs4.Add(new Vector2((float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
			{
				yield return DynamicMeshFile.ReadMeshWait;
				DynamicMeshFile.stop.ResetAndRestart();
			}
		}
		uint submeshCount = reader.ReadUInt32();
		int x = 0;
		while ((long)x < (long)((ulong)submeshCount))
		{
			uint num = reader.ReadUInt32();
			if (triangles.Count < x + 1)
			{
				triangles.Add(new List<int>());
			}
			List<int> list = triangles[x];
			for (uint num2 = 0U; num2 < num; num2 += 1U)
			{
				if (is32Bit)
				{
					list.Add(reader.ReadInt32());
				}
				else
				{
					list.Add((int)reader.ReadUInt16());
				}
			}
			if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
			{
				yield return DynamicMeshFile.ReadMeshWait;
				DynamicMeshFile.stop.ResetAndRestart();
			}
			int num3 = x;
			x = num3 + 1;
		}
		uint colourCount = reader.ReadUInt32();
		for (uint vCount = 0U; vCount < colourCount; vCount += 1U)
		{
			colours.Add(new Color((float)reader.ReadUInt16() / 10000f + 1f, (float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f, (float)reader.ReadUInt16() / 10000f));
			if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
			{
				yield return DynamicMeshFile.ReadMeshWait;
				DynamicMeshFile.stop.ResetAndRestart();
			}
		}
		try
		{
			int num4 = rend.sharedMaterials.Length;
			if (!DynamicMeshFile.TerrainSharedMaterials.ContainsKey(num4))
			{
				Material[] array = new Material[num4];
				for (int i = 0; i < array.Length; i++)
				{
					array[i] = MeshDescription.meshes[5].material;
				}
				DynamicMeshFile.TerrainSharedMaterials.Add(num4, array);
			}
			rend.sharedMaterials = DynamicMeshFile.TerrainSharedMaterials[num4];
		}
		catch (Exception)
		{
			bool flag = region != null;
			string text = (flag ? "R:" : "C:") + (((item != null) ? item.ToDebugLocation() : null) ?? region.ToDebugLocation());
			Log.Warning("Setting shared material failed on terrain material. Reloading " + text);
			if (!flag)
			{
				DynamicMeshManager.Instance.AddItemLoadRequest(item, true);
			}
			MeshLists.ReturnList(cache);
			DynamicMeshFile.CurrentlyLoadingItem = null;
			yield break;
		}
		mesh.SetVertices(vertices);
		if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
		{
			yield return DynamicMeshFile.ReadMeshWait;
			DynamicMeshFile.stop.ResetAndRestart();
		}
		mesh.subMeshCount = (int)submeshCount;
		int num5 = 0;
		while ((long)num5 < (long)((ulong)submeshCount))
		{
			mesh.SetTriangles(triangles[num5], num5);
			num5++;
		}
		if (DynamicMeshFile.stop.ElapsedMilliseconds > (long)maxTime)
		{
			yield return DynamicMeshFile.ReadMeshWait;
			DynamicMeshFile.stop.ResetAndRestart();
		}
		GameUtils.SetMeshVertexAttributes(mesh, true);
		mesh.SetUVs(0, uvs);
		mesh.SetUVs(1, uvs2);
		mesh.SetUVs(2, uvs3);
		mesh.SetUVs(3, uvs4);
		mesh.RecalculateNormals();
		mesh.SetColors(colours);
		mesh.UploadMeshData(lockMesh);
		MeshLists.ReturnList(cache);
		DynamicMeshFile.CurrentlyLoadingItem = null;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static MicroStopwatch stop = new MicroStopwatch();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<Bounds> bounds = new List<Bounds>(10);

	public static ConcurrentDictionary<int, int> TileLinks = new ConcurrentDictionary<int, int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Vector3 TerrainMeshOffset = new Vector3(0f, -0.2f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	public static Material _itemMaterial = null;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Material _RegionMaterial = null;

	public static int ReadMeshMax = 2;

	public static WaitForSeconds ReadMeshWait = null;

	public static string MeshLocation;

	public static DateTime ItemMin = new DateTime(2000, 1, 1, 0, 0, 0);

	public static WaitForSeconds WaitForSecond = new WaitForSeconds(1f);

	public static WaitForSeconds WaitForPoint1Second = new WaitForSeconds(0.1f);

	public static ConcurrentQueue<VoxelMesh> VoxelMeshPool = new ConcurrentQueue<VoxelMesh>();

	public static ConcurrentQueue<VoxelMeshTerrain> VoxelTerrainMeshPool = new ConcurrentQueue<VoxelMeshTerrain>();

	public static WaitForSeconds WaitPointOne = new WaitForSeconds(0.1f);

	public static WaitForSeconds WaitPointTwo = new WaitForSeconds(0.2f);

	public static WaitForSeconds WaitOne = new WaitForSeconds(1f);

	public static Vector3i CurrentlyLoadingRegionPosition = new Vector3i(int.MaxValue, 0, 0);

	public static Dictionary<int, Material[]> TerrainSharedMaterials = new Dictionary<int, Material[]>();

	public static DynamicMeshContainer CurrentlyLoadingItem;
}
