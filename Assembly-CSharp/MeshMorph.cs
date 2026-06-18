using System;
using UnityEngine;

[PreferBinarySerialization]
public class MeshMorph : ScriptableObject
{
	public Vector3[] Vertices
	{
		get
		{
			return this.vertices;
		}
	}

	public void Init(SkinnedMeshRenderer source, Vector3[] vertices)
	{
		this.source = source;
		this.vertices = vertices;
	}

	public GameObject GetMorphedSkinnedMesh()
	{
		if (this.source == null || this.source.sharedMesh == null || this.vertices == null || this.vertices.Length == 0)
		{
			Debug.LogError("MeshMorph: source or vertices are null or empty", this);
			return null;
		}
		Mesh mesh = Object.Instantiate<Mesh>(this.source.sharedMesh);
		mesh.name = MeshMorph.cMeshPrefix + this.source.gameObject.name;
		mesh.SetVertices(this.vertices);
		mesh.RecalculateBounds();
		GameObject gameObject = new GameObject(this.source.gameObject.name);
		gameObject.transform.localPosition = this.source.transform.localPosition;
		gameObject.transform.localRotation = this.source.transform.localRotation;
		gameObject.transform.localScale = this.source.transform.localScale;
		SkinnedMeshRenderer skinnedMeshRenderer = gameObject.AddComponent<SkinnedMeshRenderer>();
		skinnedMeshRenderer.sharedMesh = mesh;
		skinnedMeshRenderer.sharedMaterials = this.source.sharedMaterials;
		skinnedMeshRenderer.rootBone = this.source.rootBone;
		skinnedMeshRenderer.bones = this.source.bones;
		return gameObject;
	}

	public static bool IsInstance(Mesh _mesh)
	{
		return _mesh.name.StartsWith(MeshMorph.cMeshPrefix);
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public SkinnedMeshRenderer source;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] vertices;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static string cMeshPrefix = "MeshMorph-";
}
