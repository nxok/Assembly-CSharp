using System;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class Morphable : MonoBehaviour
{
	public void MorphHeadgear(Archetype archetype, bool _ignoreDlcEntitlements)
	{
		SkinnedMeshRenderer component = base.GetComponent<SkinnedMeshRenderer>();
		if (component == null || component.sharedMesh == null)
		{
			Log.Error("Morphable: SkinnedMeshRenderer not found", new object[] { this });
			return;
		}
		LoadManager.AssetRequestTask<MeshMorph> assetRequestTask = LoadManager.LoadAsset<MeshMorph>(string.Concat(new string[]
		{
			this.MorphSetPath,
			"/",
			archetype.Race,
			archetype.Variant.ToString("00"),
			"/",
			this.MorphName,
			".asset"
		}), null, null, false, true, _ignoreDlcEntitlements);
		MeshMorph asset = assetRequestTask.Asset;
		if (asset == null || asset.Vertices == null || asset.Vertices.Length == 0)
		{
			Log.Error("Morphable: MeshMorph not found", new object[] { this });
			return;
		}
		Mesh mesh = Object.Instantiate<Mesh>(component.sharedMesh);
		mesh.name = Morphable.cMeshPrefix + component.gameObject.name;
		mesh.SetVertices(asset.Vertices);
		mesh.RecalculateBounds();
		component.sharedMesh = mesh;
		assetRequestTask.Release();
	}

	public string MorphSetPath;

	public string MorphName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static string cMeshPrefix = "MeshMorph-";
}
