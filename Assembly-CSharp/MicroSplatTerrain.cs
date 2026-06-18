using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Terrain))]
[DisallowMultipleComponent]
public class MicroSplatTerrain : MicroSplatObject
{
	public static event MicroSplatTerrain.MaterialSyncAll OnMaterialSyncAll;

	public event MicroSplatTerrain.MaterialSync OnMaterialSync;

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.terrain = base.GetComponent<Terrain>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.terrain = base.GetComponent<Terrain>();
		MicroSplatTerrain.sInstances.Add(this);
		if (this.reenabled)
		{
			this.Sync();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.Sync();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		MicroSplatTerrain.sInstances.Remove(this);
		this.Cleanup();
		this.reenabled = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Cleanup()
	{
		if (this.matInstance != null && this.matInstance != this.templateMaterial)
		{
			Object.DestroyImmediate(this.matInstance);
			this.terrain.materialTemplate = null;
		}
	}

	public void Sync()
	{
		if (this.templateMaterial == null)
		{
			return;
		}
		Material material;
		if (this.terrain.materialTemplate == this.matInstance && this.matInstance != null)
		{
			this.terrain.materialTemplate.CopyPropertiesFromMaterial(this.templateMaterial);
			material = this.terrain.materialTemplate;
		}
		else
		{
			material = new Material(this.templateMaterial);
		}
		if (this.terrain.drawInstanced && this.keywordSO.IsKeywordEnabled("_TESSDISTANCE") && this.keywordSO.IsKeywordEnabled("_MSRENDERLOOP_SURFACESHADER"))
		{
			Debug.LogWarning("Disabling terrain instancing when tessellation is enabled, as Unity has not made surface shader tessellation compatible with terrain instancing");
			this.terrain.drawInstanced = false;
		}
		material.hideFlags = 61;
		this.terrain.materialTemplate = material;
		this.matInstance = material;
		base.ApplyMaps(material);
		if (this.keywordSO.IsKeywordEnabled("_CUSTOMSPLATTEXTURES"))
		{
			material.SetTexture("_CustomControl0", (this.customControl0 != null) ? this.customControl0 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl1", (this.customControl1 != null) ? this.customControl1 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl2", (this.customControl2 != null) ? this.customControl2 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl3", (this.customControl3 != null) ? this.customControl3 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl4", (this.customControl4 != null) ? this.customControl4 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl5", (this.customControl5 != null) ? this.customControl5 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl6", (this.customControl6 != null) ? this.customControl6 : Texture2D.blackTexture);
			material.SetTexture("_CustomControl7", (this.customControl7 != null) ? this.customControl7 : Texture2D.blackTexture);
		}
		else
		{
			if (this.terrain == null || this.terrain.terrainData == null)
			{
				Debug.LogError("Terrain or terrain data is null, cannot sync");
				return;
			}
			Texture2D[] alphamapTextures = this.terrain.terrainData.alphamapTextures;
			base.ApplyControlTextures(alphamapTextures, material);
		}
		base.ApplyBlendMap();
		if (this.OnMaterialSync != null)
		{
			this.OnMaterialSync(material);
		}
	}

	public override Bounds GetBounds()
	{
		return this.terrain.terrainData.bounds;
	}

	public new static void SyncAll()
	{
		for (int i = 0; i < MicroSplatTerrain.sInstances.Count; i++)
		{
			MicroSplatTerrain.sInstances[i].Sync();
		}
		if (MicroSplatTerrain.OnMaterialSyncAll != null)
		{
			MicroSplatTerrain.OnMaterialSyncAll();
		}
	}

	[HideInInspector]
	public Shader addPass;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<MicroSplatTerrain> sInstances = new List<MicroSplatTerrain>();

	public Terrain terrain;

	[HideInInspector]
	public Texture2D customControl0;

	[HideInInspector]
	public Texture2D customControl1;

	[HideInInspector]
	public Texture2D customControl2;

	[HideInInspector]
	public Texture2D customControl3;

	[HideInInspector]
	public Texture2D customControl4;

	[HideInInspector]
	public Texture2D customControl5;

	[HideInInspector]
	public Texture2D customControl6;

	[HideInInspector]
	public Texture2D customControl7;

	[HideInInspector]
	public bool reenabled;

	public delegate void MaterialSyncAll();

	public delegate void MaterialSync(Material m);
}
