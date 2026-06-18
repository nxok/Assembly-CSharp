using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class GearBoneMap : MonoBehaviour
{
	public IReadOnlyList<Transform> GetPartBones(string partName)
	{
		if (string.IsNullOrEmpty(partName))
		{
			return Array.Empty<Transform>();
		}
		GearBoneMap.PartBones partBones = this.parts.FirstOrDefault((GearBoneMap.PartBones p) => string.Equals(p.partName, partName, StringComparison.Ordinal));
		if (partBones != null && partBones.bones != null)
		{
			return partBones.bones;
		}
		int num = partName.IndexOf("_");
		if (num < 0)
		{
			num = partName.Length;
		}
		partName = partName.Substring(0, num);
		partBones = this.parts.FirstOrDefault((GearBoneMap.PartBones p) => string.Equals(p.partName, partName, StringComparison.Ordinal));
		if (partBones != null && partBones.bones != null)
		{
			return partBones.bones;
		}
		return Array.Empty<Transform>();
	}

	public IReadOnlyList<string> GetPartNames()
	{
		return this.parts.Select((GearBoneMap.PartBones p) => p.partName).ToArray<string>();
	}

	public void SetBones(string partName, IEnumerable<Transform> newBones)
	{
		if (string.IsNullOrEmpty(partName))
		{
			return;
		}
		GearBoneMap.PartBones partBones = this.parts.FirstOrDefault((GearBoneMap.PartBones p) => string.Equals(p.partName, partName, StringComparison.Ordinal));
		if (partBones == null)
		{
			partBones = new GearBoneMap.PartBones
			{
				partName = partName
			};
			this.parts.Add(partBones);
		}
		List<Transform> list = (from t in newBones.Where((Transform t) => t != null).Distinct<Transform>()
			orderby GearBoneMap.GetHierarchyPath(t), t.name
			select t).ToList<Transform>();
		partBones.bones.Clear();
		partBones.bones.AddRange(list);
	}

	public void ClearAll()
	{
		this.parts.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string GetHierarchyPath(Transform t)
	{
		if (t == null)
		{
			return string.Empty;
		}
		Stack<string> stack = new Stack<string>();
		Transform transform = t;
		while (transform != null)
		{
			stack.Push(transform.name);
			transform = transform.parent;
		}
		return string.Join("/", stack);
	}

	public void Bake()
	{
		List<Transform> list = new List<Transform>();
		foreach (Transform transform in base.transform.GetComponentsInChildren<Transform>(true))
		{
			int num = transform.name.IndexOf("_");
			if (num < 0)
			{
				num = transform.name.Length;
			}
			if (transform.parent == base.transform && GearBoneMap.DefaultParts.Contains(transform.name.Substring(0, num)))
			{
				list.Add(transform);
			}
		}
		foreach (Transform transform2 in list)
		{
			SkinnedMeshRenderer[] componentsInChildren2 = transform2.GetComponentsInChildren<SkinnedMeshRenderer>(true);
			HashSet<Transform> hashSet = new HashSet<Transform>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in componentsInChildren2)
			{
				if (!(skinnedMeshRenderer == null))
				{
					Mesh sharedMesh = skinnedMeshRenderer.sharedMesh;
					if (sharedMesh == null)
					{
						Debug.LogWarning("[GearBoneMap] " + skinnedMeshRenderer.name + " has no sharedMesh.");
					}
					else
					{
						try
						{
							NativeArray<byte> bonesPerVertex = sharedMesh.GetBonesPerVertex();
							NativeArray<BoneWeight1> allBoneWeights = sharedMesh.GetAllBoneWeights();
							Transform[] bones = skinnedMeshRenderer.bones;
							int num2 = 0;
							for (int k = 0; k < bonesPerVertex.Length; k++)
							{
								int num3 = (int)bonesPerVertex[k];
								for (int l = 0; l < num3; l++)
								{
									BoneWeight1 boneWeight = allBoneWeights[num2++];
									if (boneWeight.weight > 0f)
									{
										int boneIndex = boneWeight.boneIndex;
										if (boneIndex >= 0 && bones != null && boneIndex < bones.Length)
										{
											Transform transform3 = bones[boneIndex];
											if (transform3 != null)
											{
												hashSet.Add(transform3);
											}
										}
									}
								}
							}
							if (skinnedMeshRenderer.rootBone != null)
							{
								hashSet.Add(skinnedMeshRenderer.rootBone);
							}
						}
						catch (Exception ex)
						{
							Debug.LogError("[GearBoneMap] Failed reading bone weights for " + skinnedMeshRenderer.name + ": " + ex.Message);
						}
					}
				}
			}
			this.SetBones(transform2.name, hashSet);
		}
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public List<GearBoneMap.PartBones> parts = new List<GearBoneMap.PartBones>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly string[] DefaultParts = new string[] { "head", "body", "feet", "hands" };

	[Serializable]
	public class PartBones
	{
		[Tooltip("e.g. head, body, feet, hands")]
		public string partName;

		[Tooltip("The unique bones actually referenced by skin weights for this part")]
		public List<Transform> bones = new List<Transform>();
	}
}
