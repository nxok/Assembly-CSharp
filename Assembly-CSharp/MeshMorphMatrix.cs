using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MeshMorphMatrix", menuName = "Mesh Morphing/MeshMorphMatrix", order = 1)]
public class MeshMorphMatrix : ScriptableObject
{
	public SkinnedMeshRenderer MorphTargetsSource
	{
		get
		{
			return this.morphTargetsSource;
		}
	}

	public MeshMorphMatrix.MorphTarget[] MorphTargets
	{
		get
		{
			return this.morphTargets;
		}
	}

	public MeshMorphMatrix.MeshData[] Meshes
	{
		get
		{
			return this.meshes;
		}
	}

	public MeshMorph[] MorphedMeshes
	{
		get
		{
			return this.morphedMeshes;
		}
	}

	public float MaxDistance
	{
		get
		{
			return this.maxDistance;
		}
	}

	public float NormalBias
	{
		get
		{
			return this.normalBias;
		}
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshMorphMatrix.MeshMorphMatrixType matrixType;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public SkinnedMeshRenderer morphTargetsSource;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float maxDistance = 0.1f;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public float normalBias;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshMorphMatrix.MorphTarget[] morphTargets;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshMorphMatrix.MeshData[] meshes;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public MeshMorph[] morphedMeshes;

	[HideInInspector]
	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;

	public enum MeshMorphMatrixType
	{
		Hair,
		Headgear
	}

	[Serializable]
	public struct MorphTarget
	{
		public int blendshapeIndex;

		public string name;
	}

	[Serializable]
	public struct MeshData
	{
		public string MeshName
		{
			get
			{
				if (!(this.gameObject != null))
				{
					return "null";
				}
				return this.gameObject.name;
			}
		}

		public int blendshapeIndex;

		public string typeName;

		public GameObject gameObject;
	}
}
