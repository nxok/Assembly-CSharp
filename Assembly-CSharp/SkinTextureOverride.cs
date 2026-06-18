using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class SkinTextureOverride : MonoBehaviour
{
	public void ApplyOverrides()
	{
		Texture2D texture2D = this.skinAlbedo.LoadAssetAsync<Texture2D>().WaitForCompletion();
		Texture2D texture2D2 = this.skinNormal.LoadAssetAsync<Texture2D>().WaitForCompletion();
		Texture2D texture2D3 = this.skinRMOE.LoadAssetAsync<Texture2D>().WaitForCompletion();
		SkinnedMeshRenderer component = base.transform.GetComponent<SkinnedMeshRenderer>();
		Material[] materials = component.materials;
		for (int i = 0; i < materials.Length; i++)
		{
			if (materials[i] != null && materials[i].shader.name == "Game/SDCS/Skin")
			{
				string name = materials[i].name;
				if (name.Contains("_Body") || name.Contains("_Head") || name.Contains("_Hand"))
				{
					materials[i] = new Material(materials[i]);
					materials[i].SetTexture("_Albedo", texture2D);
					materials[i].SetTexture("_NormalMap", texture2D2);
					materials[i].SetTexture("_Masks", texture2D3);
				}
			}
		}
		component.materials = materials;
	}

	public AssetReference skinAlbedo;

	public AssetReference skinNormal;

	public AssetReference skinRMOE;
}
