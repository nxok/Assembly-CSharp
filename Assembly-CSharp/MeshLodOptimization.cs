using System;
using System.Collections.Generic;
using UnityEngine;

public static class MeshLodOptimization
{
	public static void Apply(ref Transform prefab)
	{
		if (prefab.GetComponentInChildren<Tree>())
		{
			return;
		}
		int instanceID = prefab.GetInstanceID();
		if (Application.isEditor)
		{
			if (MeshLodOptimization.editorCache == null)
			{
				MeshLodOptimization.editorCache = new MeshLodOptimization.EditorCloneCache();
			}
			prefab = MeshLodOptimization.editorCache.CacheClone(instanceID, prefab);
		}
		if (MeshLodOptimization.processed.Contains(instanceID))
		{
			return;
		}
		MeshLodOptimization.RemoveLod1(prefab);
		MeshLodOptimization.processed.Add(instanceID);
	}

	public static void RemoveLod1(Transform prefab)
	{
		MeshLodOptimization.lodGroupBuffer.Clear();
		prefab.GetComponentsInChildren<LODGroup>(MeshLodOptimization.lodGroupBuffer);
		foreach (LODGroup lodgroup in MeshLodOptimization.lodGroupBuffer)
		{
			if (lodgroup.lodCount > 2)
			{
				LOD[] lods = lodgroup.GetLODs();
				Renderer[] renderers = lods[1].renderers;
				Renderer[] renderers2 = lods[2].renderers;
				lods[1].renderers = lods[2].renderers;
				lodgroup.SetLODs(lods);
				foreach (Renderer renderer in renderers)
				{
					if (!(renderer == null))
					{
						bool flag = false;
						Renderer[] array2 = renderers2;
						for (int j = 0; j < array2.Length; j++)
						{
							if (array2[j] == renderer)
							{
								flag = true;
								break;
							}
						}
						if (!flag)
						{
							MeshFilter meshFilter;
							if (renderer.TryGetComponent<MeshFilter>(ref meshFilter))
							{
								Object.Destroy(meshFilter);
							}
							Object.Destroy(renderer);
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static MeshLodOptimization.EditorCloneCache editorCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public static HashSet<int> processed = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<LODGroup> lodGroupBuffer = new List<LODGroup>();

	public class EditorCloneCache
	{
		public EditorCloneCache()
		{
			this.cloneParent = new GameObject("LodCullingCache");
			this.cloneParent.SetActive(false);
			GameManager.Instance.OnWorldChanged += delegate(World world)
			{
				if (world == null)
				{
					NGUITools.DestroyChildren(this.cloneParent.transform);
					this.clones.Clear();
				}
			};
		}

		public Transform CacheClone(int id, Transform prefab)
		{
			Transform transform;
			if (this.clones.TryGetValue(id, out transform))
			{
				return transform;
			}
			string name = prefab.name;
			transform = Object.Instantiate<Transform>(prefab, this.cloneParent.transform);
			transform.name = name;
			this.clones.Add(id, transform);
			return transform;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public GameObject cloneParent;

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<int, Transform> clones = new Dictionary<int, Transform>();
	}
}
