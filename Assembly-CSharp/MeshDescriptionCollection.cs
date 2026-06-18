using System;
using System.Collections;
using UnityEngine;

public class MeshDescriptionCollection : MonoBehaviour
{
	public MeshDescription[] Meshes
	{
		get
		{
			return this.currentMeshes;
		}
	}

	public void Init()
	{
		MeshDescription meshDescription = this.meshes[0];
		if (meshDescription.TexDiffuse || meshDescription.TexNormal || meshDescription.TexSpecular)
		{
			Log.Error("MeshDescriptionCollection should not have MESH_OPAQUE textures");
		}
		meshDescription = this.meshes[5];
		if (meshDescription.TexDiffuse || meshDescription.TexNormal || meshDescription.TexSpecular)
		{
			Log.Error("MeshDescriptionCollection should not have MESH_TERRAIN textures");
		}
		this.currentMeshes = new MeshDescription[this.meshes.Length];
		for (int i = 0; i < this.meshes.Length; i++)
		{
			meshDescription = new MeshDescription(this.meshes[i]);
			this.currentMeshes[i] = meshDescription;
		}
	}

	public void UnloadTextureArrays(int _index)
	{
		this.currentMeshes[_index].UnloadTextureArrays(_index);
	}

	public IEnumerator LoadTextureArrays(bool _isReload = false)
	{
		MicroStopwatch ms = new MicroStopwatch(true);
		MeshDescription[] mds = this.currentMeshes;
		for (int i = 0; i < mds.Length; i++)
		{
			mds[i].UnloadTextureArrays(i);
		}
		if (_isReload)
		{
			Resources.UnloadUnusedAssets();
		}
		int num;
		for (int index = 0; index < mds.Length; index = num + 1)
		{
			MeshDescription meshDescription = mds[index];
			yield return meshDescription.LoadTextureArraysForQuality(this, index, this.quality, _isReload);
			yield return null;
			num = index;
		}
		Log.Out("LoadTextureArraysForQuality took {0}", new object[] { (float)ms.ElapsedMilliseconds * 0.001f });
		if (GameManager.Instance != null && GameManager.Instance.prefabLODManager != null)
		{
			GameManager.Instance.prefabLODManager.UpdateMaterials();
		}
		yield break;
	}

	public IEnumerator LoadTextureArraysForQuality(bool _isReload = false)
	{
		if (GameManager.IsDedicatedServer)
		{
			yield break;
		}
		int num = GameOptionsManager.GetTextureQuality(-1);
		if (num >= 3)
		{
			num = 2;
		}
		Log.Out("LoadTextureArraysForQuality quality {0} to {1}, reload {2}", new object[] { this.quality, num, _isReload });
		if (_isReload && num == this.quality)
		{
			yield break;
		}
		this.quality = num;
		yield return this.LoadTextureArrays(_isReload);
		yield break;
	}

	public void SetTextureArraysFilter()
	{
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		int textureFilter = GameOptionsManager.GetTextureFilter();
		int num = MeshDescriptionCollection.filterToAnisoLevel[textureFilter];
		Log.Out("SetTextureArraysFilter {0}, AF {1}", new object[] { textureFilter, num });
		MeshDescription[] array = this.currentMeshes;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetTextureFilter(i, num);
		}
	}

	public void Cleanup()
	{
	}

	public MeshDescription[] meshes;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MeshDescription[] currentMeshes;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int quality = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int[] filterToAnisoLevel = new int[] { 1, 2, 4, 8, 9 };
}
