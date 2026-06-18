using System;
using System.Collections.Generic;
using UnityEngine;
using WorldGenerationEngineFinal;

public static class WorldPreviewTerrain
{
	public static void Init(WorldBuilder _worldBuilder, Transform _parentTransform)
	{
		WorldPreviewTerrain.worldBuilder = _worldBuilder;
		WorldPreviewTerrain.parentTransform = _parentTransform;
		WorldPreviewTerrain.terrainPreviewMaterial = Resources.Load("Materials/TerrainPreview", typeof(Material)) as Material;
	}

	public static void SetTexture(Texture2D _texture)
	{
		if (WorldPreviewTerrain.terrainPreviewMaterial)
		{
			WorldPreviewTerrain.terrainPreviewMaterial.mainTexture = _texture;
		}
	}

	public static void GenerateTerrain()
	{
		MicroStopwatch microStopwatch = new MicroStopwatch(true);
		WorldPreviewTerrain.parentTransform.localPosition = new Vector3(-((float)WorldPreviewTerrain.worldBuilder.WorldSize * 0.5f), 0f, -((float)WorldPreviewTerrain.worldBuilder.WorldSize * 0.5f));
		if (XUiC_WorldGenerationWindow.Instance.PreviewQualityLevel == XUiC_WorldGenerationWindow.PreviewQuality.NoPreview)
		{
			return;
		}
		int num;
		switch (XUiC_WorldGenerationWindow.Instance.PreviewQualityLevel)
		{
		case XUiC_WorldGenerationWindow.PreviewQuality.Lowest:
			num = 16;
			break;
		case XUiC_WorldGenerationWindow.PreviewQuality.Low:
			num = 8;
			break;
		case XUiC_WorldGenerationWindow.PreviewQuality.Default:
			num = 4;
			break;
		case XUiC_WorldGenerationWindow.PreviewQuality.High:
		case XUiC_WorldGenerationWindow.PreviewQuality.Highest:
			num = 2;
			break;
		default:
			num = 1;
			break;
		}
		int num2 = num;
		int num3 = WorldPreviewTerrain.worldBuilder.WorldSize / 512;
		int num4 = num3 * num3;
		if (WorldPreviewTerrain.objects == null || WorldPreviewTerrain.objects.Length != num4)
		{
			WorldPreviewTerrain.destroyTerrain();
			WorldPreviewTerrain.objects = new GameObject[num4];
			for (int i = 0; i < num3; i++)
			{
				for (int j = 0; j < num3; j++)
				{
					Vector2i vector2i = new Vector2i(j, i);
					GameObject gameObject = new GameObject(vector2i.ToString());
					WorldPreviewTerrain.objects[j + i * num3] = gameObject;
					gameObject.transform.SetParent(WorldPreviewTerrain.parentTransform);
					WorldPreviewTerrain.createMesh(gameObject, vector2i, num2);
				}
			}
		}
		else
		{
			for (int k = 0; k < num3; k++)
			{
				for (int l = 0; l < num3; l++)
				{
					Vector2i vector2i2 = new Vector2i(l, k);
					WorldPreviewTerrain.updateMesh(WorldPreviewTerrain.objects[l + k * num3], vector2i2, num2);
				}
			}
		}
		Log.Warning("GenerateTerrain in {0}", new object[] { (float)microStopwatch.ElapsedMilliseconds * 0.001f });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void destroyTerrain()
	{
		if (!WorldPreviewTerrain.parentTransform)
		{
			return;
		}
		int childCount = WorldPreviewTerrain.parentTransform.childCount;
		if (childCount > 0)
		{
			MeshFilter[] componentsInChildren = WorldPreviewTerrain.parentTransform.GetComponentsInChildren<MeshFilter>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Object.Destroy(componentsInChildren[i].sharedMesh);
			}
			Renderer[] componentsInChildren2 = WorldPreviewTerrain.parentTransform.GetComponentsInChildren<Renderer>();
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				Object.Destroy(componentsInChildren2[j]);
			}
			for (int k = 0; k < childCount; k++)
			{
				Object.Destroy(WorldPreviewTerrain.parentTransform.GetChild(k).gameObject);
			}
		}
		WorldPreviewTerrain.objects = null;
	}

	public static void Cleanup()
	{
		WorldPreviewTerrain.destroyTerrain();
		if (!WorldPreviewTerrain.terrainPreviewMaterial)
		{
			return;
		}
		Resources.UnloadAsset(WorldPreviewTerrain.terrainPreviewMaterial);
		WorldPreviewTerrain.terrainPreviewMaterial = null;
		GCUtils.UnloadAndCollectStart();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void createMesh(GameObject _obj, Vector2i _terrainSectorIndex, int _metersPerDetailUnit)
	{
		MeshFilter meshFilter = _obj.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = _obj.AddComponent<MeshRenderer>();
		meshRenderer.receiveShadows = false;
		meshRenderer.lightProbeUsage = 0;
		meshRenderer.reflectionProbeUsage = 0;
		meshRenderer.shadowCastingMode = 0;
		meshRenderer.sharedMaterial = WorldPreviewTerrain.terrainPreviewMaterial;
		Mesh mesh = new Mesh();
		meshFilter.sharedMesh = mesh;
		_obj.layer = 11;
		_obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		Vector2i vector2i = new Vector2i(_terrainSectorIndex.x * 512, _terrainSectorIndex.y * 512);
		int worldSize = WorldPreviewTerrain.worldBuilder.WorldSize;
		for (int i = vector2i.y; i <= vector2i.y + 512; i += _metersPerDetailUnit)
		{
			Vector3 vector;
			vector.z = (float)i;
			for (int j = vector2i.x; j <= vector2i.x + 512; j += _metersPerDetailUnit)
			{
				vector.x = (float)j;
				vector.y = WorldPreviewTerrain.worldBuilder.GetHeight(j, i);
				WorldPreviewTerrain.vertices.Add(vector);
				Vector2 vector2;
				vector2.x = (float)j / (float)worldSize;
				vector2.y = (float)i / (float)worldSize;
				WorldPreviewTerrain.uvs.Add(vector2);
			}
		}
		int num = 0;
		int num2 = 512 / _metersPerDetailUnit;
		for (int k = 0; k < 512; k += _metersPerDetailUnit)
		{
			for (int l = 0; l < 512; l += _metersPerDetailUnit)
			{
				WorldPreviewTerrain.triangles.Add(num);
				WorldPreviewTerrain.triangles.Add(num + num2 + 1);
				WorldPreviewTerrain.triangles.Add(num + 1);
				WorldPreviewTerrain.triangles.Add(num + 1);
				WorldPreviewTerrain.triangles.Add(num + num2 + 1);
				WorldPreviewTerrain.triangles.Add(num + num2 + 2);
				num++;
			}
			num++;
		}
		mesh.indexFormat = 1;
		mesh.SetVertices(WorldPreviewTerrain.vertices);
		mesh.SetUVs(0, WorldPreviewTerrain.uvs);
		mesh.SetTriangles(WorldPreviewTerrain.triangles, 0);
		mesh.RecalculateNormals();
		WorldPreviewTerrain.vertices.Clear();
		WorldPreviewTerrain.triangles.Clear();
		WorldPreviewTerrain.uvs.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void updateMesh(GameObject _obj, Vector2i _terrainSectorIndex, int _metersPerDetailUnit)
	{
		Vector2i vector2i = new Vector2i(_terrainSectorIndex.x * 512, _terrainSectorIndex.y * 512);
		for (int i = vector2i.y; i <= vector2i.y + 512; i += _metersPerDetailUnit)
		{
			Vector3 vector;
			vector.z = (float)i;
			for (int j = vector2i.x; j <= vector2i.x + 512; j += _metersPerDetailUnit)
			{
				vector.x = (float)j;
				vector.y = WorldPreviewTerrain.worldBuilder.GetHeight(j, i);
				WorldPreviewTerrain.vertices.Add(vector);
			}
		}
		Mesh sharedMesh = _obj.GetComponent<MeshFilter>().sharedMesh;
		sharedMesh.SetVertices(WorldPreviewTerrain.vertices);
		WorldPreviewTerrain.vertices.Clear();
		sharedMesh.RecalculateNormals();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int TerrainSectorSize = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public static WorldBuilder worldBuilder;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Transform parentTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Material terrainPreviewMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject[] objects;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Vector3> vertices = new List<Vector3>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<int> triangles = new List<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Vector2> uvs = new List<Vector2>();
}
