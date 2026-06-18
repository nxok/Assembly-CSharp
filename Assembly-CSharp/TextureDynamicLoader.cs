using System;
using System.Collections.Generic;
using UnityEngine;

public class TextureDynamicLoader : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (!this.DistanceChecks)
		{
			return;
		}
		if (Time.time - this.lastTimeChecked < (float)(this.bLastTimeFarAwayCamera ? 5 : 1))
		{
			return;
		}
		this.lastTimeChecked = Time.time;
		if (this.mainCamera == null)
		{
			this.mainCamera = Camera.main;
		}
		if (this.mainCamera == null)
		{
			return;
		}
		float num = Vector3.Distance(this.mainCamera.transform.position, base.transform.position);
		bool flag = num > (float)this.LoResDistance;
		if (this.bHiResLoaded && flag)
		{
			this.bHiResLoaded = false;
			this.SetLoResTexture(false);
		}
		else if (!this.bHiResLoaded && !flag)
		{
			this.bHiResLoaded = true;
			this.SetHiResTexture();
		}
		this.bLastTimeFarAwayCamera = num > 50f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		if (!this.DistanceChecks && !this.bHiResLoaded)
		{
			this.bHiResLoaded = true;
			this.SetHiResTexture();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		this.mainCamera = null;
		if (this.bHiResLoaded)
		{
			this.SetLoResTexture(false);
			this.bHiResLoaded = false;
		}
	}

	public bool IsHiResTextureLoaded(out bool _bHires)
	{
		_bHires = false;
		this.checkMaterials();
		if (this.materials.Count == 0 || this.materials[0].Length == 0 || !this.materials[0][0])
		{
			return false;
		}
		Texture texture = this.materials[0][0].GetTexture(TextureDynamicLoader.textureTypes[0]);
		if (texture == null)
		{
			return false;
		}
		string name = texture.name;
		_bHires = !name.EndsWith("_LOW");
		return true;
	}

	public void SetHiResTexture()
	{
		this.checkMaterials();
		for (int i = 0; i < this.materials.Count; i++)
		{
			for (int j = 0; j < this.materials[i].Length; j++)
			{
				for (int k = 0; k < TextureDynamicLoader.textureTypes.Length; k++)
				{
					Material material = this.materials[i][j];
					if (material.HasProperty(TextureDynamicLoader.textureTypes[k]))
					{
						this.setHiResTexture(material, TextureDynamicLoader.textureTypes[k]);
					}
				}
			}
		}
	}

	public void SetLoResTexture(bool _bFindFolderAndCreateLoResTex = false)
	{
		this.checkMaterials();
		if (!Application.isPlaying && _bFindFolderAndCreateLoResTex)
		{
			string text = this.determineAssetsFolder();
			if (this.CreateLowResTexture)
			{
				for (int i = 0; i < this.materials.Count; i++)
				{
					for (int j = 0; j < this.materials[i].Length; j++)
					{
						for (int k = 0; k < TextureDynamicLoader.textureTypes.Length; k++)
						{
							Material material = this.materials[i][j];
							if (!(material == null) && material.HasProperty(TextureDynamicLoader.textureTypes[k]))
							{
								Texture texture = material.GetTexture(TextureDynamicLoader.textureTypes[k]);
								if (!(texture == null) && texture is Texture2D)
								{
									this.createLoResTexture(texture as Texture2D, text);
								}
							}
						}
					}
				}
			}
		}
		for (int l = 0; l < this.materials.Count; l++)
		{
			for (int m = 0; m < this.materials[l].Length; m++)
			{
				for (int n = 0; n < TextureDynamicLoader.textureTypes.Length; n++)
				{
					Material material2 = this.materials[l][m];
					if (!(material2 == null) && material2.HasProperty(TextureDynamicLoader.textureTypes[n]))
					{
						this.setLoResTexture(material2, TextureDynamicLoader.textureTypes[n]);
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createLoResTexture(Texture2D _tex, string _path)
	{
	}

	public static void SaveTexture(Texture2D _texture, string _fileName)
	{
		byte[] array = ImageConversion.EncodeToPNG(_texture);
		SdFile.WriteAllBytes(_fileName, array);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string determineAssetsFolder()
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkMaterials()
	{
		if (Application.isPlaying && this.bGotMaterials)
		{
			return;
		}
		this.bGotMaterials = true;
		base.GetComponentsInChildren<Renderer>(true, this.renderes);
		this.materials.Clear();
		this.materials.Capacity = this.renderes.Count;
		for (int i = 0; i < this.renderes.Count; i++)
		{
			Renderer renderer = this.renderes[i];
			bool flag = true;
			int num = 0;
			while (this.ExcludedRenderers != null && num < this.ExcludedRenderers.Length)
			{
				if (renderer == this.ExcludedRenderers[num])
				{
					flag = false;
					break;
				}
				num++;
			}
			if (flag)
			{
				if (Application.isPlaying && this.UseInstancedMaterial)
				{
					this.materials.Add(renderer.materials);
				}
				else
				{
					this.materials.Add(renderer.sharedMaterials);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setHiResTexture(Material _m, string _propName)
	{
		if (!_m.HasProperty(_propName))
		{
			return;
		}
		Texture texture = _m.GetTexture(_propName);
		if (texture == null)
		{
			return;
		}
		string text = texture.name;
		if (!text.EndsWith("_LOW"))
		{
			if (Application.isPlaying)
			{
				TextureLoadingManager.Instance.LoadTexture(_m, _propName, this.AssetPath, text, texture);
			}
			return;
		}
		text = text.Substring(0, text.Length - "_LOW".Length);
		if (Application.isPlaying)
		{
			TextureLoadingManager.Instance.LoadTexture(_m, _propName, this.AssetPath, text, texture);
			return;
		}
		Texture texture2 = Resources.Load<Texture2D>(this.AssetPath + text);
		_m.SetTexture(_propName, texture2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLoResTexture(Material _m, string _propName)
	{
		Texture texture = _m.GetTexture(_propName);
		if (texture == null)
		{
			return;
		}
		string text = texture.name;
		if (text.EndsWith("_LOW"))
		{
			if (Application.isPlaying)
			{
				TextureLoadingManager.Instance.UnloadTexture(this.AssetPath, text.Substring(0, text.Length - "_LOW".Length));
			}
			return;
		}
		text += "_LOW";
		bool flag = true;
		if (Application.isPlaying)
		{
			flag = TextureLoadingManager.Instance.UnloadTexture(this.AssetPath, texture.name);
		}
		if (flag || this.UseInstancedMaterial)
		{
			Texture texture2 = Resources.Load<Texture2D>(this.AssetPath + text);
			_m.SetTexture(_propName, texture2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cWidthLoRes = 64;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cDelayNearCamera = 1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cDelayFarAwayCamera = 5;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cDefaultDistanceLoRes = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cDistFarAwayCamera = 50;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cPrefixPath = "Assets/Resources";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cPostFixLoResTex = "_LOW";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static string[] textureTypes = new string[] { "_MainTex", "_BumpMap", "_MetallicGlossMap", "_SpecGlossMap", "_OcclusionMap", "_EmissionMap" };

	public int LoResDistance = 20;

	public string AssetPath;

	public bool DistanceChecks = true;

	public bool UseInstancedMaterial = true;

	public Renderer[] ExcludedRenderers;

	public bool CreateLowResTexture;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Material[]> materials = new List<Material[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Renderer> renderes = new List<Renderer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bGotMaterials;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastTimeChecked;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Camera mainCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bLastTimeFarAwayCamera;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bHiResLoaded;
}
