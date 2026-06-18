using System;
using UnityEngine;

[ExecuteAlways]
public class MicroSplatObject : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public long GetOverrideHash()
	{
		long num = 3L * (long)(((this.propData == null) ? 3 : this.propData.GetHashCode()) * 3) * (((this.perPixelNormal == null) ? 7L : this.perPixelNormal.GetNativeTexturePtr().ToInt64()) * 7L) * (long)(((this.keywordSO == null) ? 11 : this.keywordSO.GetHashCode()) * 11) * (((this.procBiomeMask == null) ? 13L : this.procBiomeMask.GetNativeTexturePtr().ToInt64()) * 13L) * (((this.procBiomeMask2 == null) ? 81L : this.procBiomeMask2.GetNativeTexturePtr().ToInt64()) * 81L) * (((this.cavityMap == null) ? 17L : this.cavityMap.GetNativeTexturePtr().ToInt64()) * 17L) * (long)(((this.procTexCfg == null) ? 19 : this.procTexCfg.GetHashCode()) * 19) * (((this.streamTexture == null) ? 41L : this.streamTexture.GetNativeTexturePtr().ToInt64()) * 41L) * (((this.terrainDesc == null) ? 43L : this.terrainDesc.GetNativeTexturePtr().ToInt64()) * 43L) * (((this.geoTextureOverride == null) ? 47L : this.geoTextureOverride.GetNativeTexturePtr().ToInt64()) * 47L) * (((this.globalNormalOverride == null) ? 53L : this.globalNormalOverride.GetNativeTexturePtr().ToInt64()) * 53L) * (((this.globalSAOMOverride == null) ? 59L : this.globalSAOMOverride.GetNativeTexturePtr().ToInt64()) * 59L) * (((this.globalEmisOverride == null) ? 61L : this.globalEmisOverride.GetNativeTexturePtr().ToInt64()) * 61L) * (((this.tintMapOverride == null) ? 71L : this.tintMapOverride.GetNativeTexturePtr().ToInt64()) * 71L);
		if (num == 0L)
		{
			Debug.Log("Override hash returned 0, this should not happen");
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetMap(Material m, string name, Texture2D tex)
	{
		if (m.HasProperty(name) && tex != null)
		{
			m.SetTexture(name, tex);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ApplyMaps(Material m)
	{
		this.SetMap(m, "_PerPixelNormal", this.perPixelNormal);
		this.SetMap(m, "_StreamControl", this.streamTexture);
		this.SetMap(m, "_GeoTex", this.geoTextureOverride);
		this.SetMap(m, "_GlobalTintTex", this.tintMapOverride);
		this.SetMap(m, "_GlobalNormalTex", this.globalNormalOverride);
		this.SetMap(m, "_GlobalSAOMTex", this.globalSAOMOverride);
		this.SetMap(m, "_GlobalEmisTex", this.globalEmisOverride);
		if (m.HasProperty("_GeoCurve") && this.propData != null)
		{
			m.SetTexture("_GeoCurve", this.propData.GetGeoCurve());
		}
		if (m.HasProperty("_GeoSlopeTex") && this.propData != null)
		{
			m.SetTexture("_GeoSlopeTex", this.propData.GetGeoSlopeFilter());
		}
		if (m.HasProperty("_GlobalSlopeTex") && this.propData != null)
		{
			m.SetTexture("_GlobalSlopeTex", this.propData.GetGlobalSlopeFilter());
		}
		if (this.propData != null)
		{
			m.SetTexture("_PerTexProps", this.propData.GetTexture());
		}
		if (this.procTexCfg != null)
		{
			if (m.HasProperty("_ProcTexCurves"))
			{
				m.SetTexture("_ProcTexCurves", this.procTexCfg.GetCurveTexture());
				m.SetTexture("_ProcTexParams", this.procTexCfg.GetParamTexture());
				m.SetInt("_PCLayerCount", this.procTexCfg.layers.Count);
				if (this.procBiomeMask != null && m.HasProperty("_ProcTexBiomeMask"))
				{
					m.SetTexture("_ProcTexBiomeMask", this.procBiomeMask);
				}
				if (this.procBiomeMask2 != null && m.HasProperty("_ProcTexBiomeMask2"))
				{
					m.SetTexture("_ProcTexBiomeMask2", this.procBiomeMask2);
				}
			}
			if (m.HasProperty("_PCHeightGradients"))
			{
				m.SetTexture("_PCHeightGradients", this.procTexCfg.GetHeightGradientTexture());
			}
			if (m.HasProperty("_PCHeightHSV"))
			{
				m.SetTexture("_PCHeightHSV", this.procTexCfg.GetHeightHSVTexture());
			}
			if (m.HasProperty("_CavityMap"))
			{
				m.SetTexture("_CavityMap", this.cavityMap);
			}
			if (m.HasProperty("_PCSlopeGradients"))
			{
				m.SetTexture("_PCSlopeGradients", this.procTexCfg.GetSlopeGradientTexture());
			}
			if (m.HasProperty("_PCSlopeHSV"))
			{
				m.SetTexture("_PCSlopeHSV", this.procTexCfg.GetSlopeHSVTexture());
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ApplyControlTextures(Texture2D[] controls, Material m)
	{
		m.SetTexture("_Control0", (controls.Length != 0) ? controls[0] : Texture2D.blackTexture);
		m.SetTexture("_Control1", (controls.Length > 1) ? controls[1] : Texture2D.blackTexture);
		m.SetTexture("_Control2", (controls.Length > 2) ? controls[2] : Texture2D.blackTexture);
		m.SetTexture("_Control3", (controls.Length > 3) ? controls[3] : Texture2D.blackTexture);
		m.SetTexture("_Control4", (controls.Length > 4) ? controls[4] : Texture2D.blackTexture);
		m.SetTexture("_Control5", (controls.Length > 5) ? controls[5] : Texture2D.blackTexture);
		m.SetTexture("_Control6", (controls.Length > 6) ? controls[6] : Texture2D.blackTexture);
		m.SetTexture("_Control7", (controls.Length > 7) ? controls[7] : Texture2D.blackTexture);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SyncBlendMat(Vector3 size)
	{
		if (this.blendMatInstance != null && this.matInstance != null)
		{
			this.blendMatInstance.CopyPropertiesFromMaterial(this.matInstance);
			Vector4 vector = default(Vector4);
			vector.z = size.x;
			vector.w = size.z;
			vector.x = base.transform.position.x;
			vector.y = base.transform.position.z;
			this.blendMatInstance.SetVector("_TerrainBounds", vector);
			this.blendMatInstance.SetTexture("_TerrainDesc", this.terrainDesc);
		}
	}

	public virtual Bounds GetBounds()
	{
		return default(Bounds);
	}

	public Material GetBlendMatInstance()
	{
		if (this.blendMat != null && this.terrainDesc != null)
		{
			if (this.blendMatInstance == null)
			{
				this.blendMatInstance = new Material(this.blendMat);
				this.SyncBlendMat(this.GetBounds().size);
			}
			if (this.blendMatInstance.shader != this.blendMat.shader)
			{
				this.blendMatInstance.shader = this.blendMat.shader;
				this.SyncBlendMat(this.GetBounds().size);
			}
		}
		return this.blendMatInstance;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void ApplyBlendMap()
	{
		if (this.blendMat != null && this.terrainDesc != null)
		{
			if (this.blendMatInstance == null)
			{
				this.blendMatInstance = new Material(this.blendMat);
			}
			this.SyncBlendMat(this.GetBounds().size);
		}
	}

	public void RevisionFromMat()
	{
	}

	public static void SyncAll()
	{
		MicroSplatTerrain.SyncAll();
	}

	[HideInInspector]
	public Material templateMaterial;

	[HideInInspector]
	[NonSerialized]
	public Material matInstance;

	[HideInInspector]
	public Material blendMat;

	[HideInInspector]
	public Material blendMatInstance;

	[HideInInspector]
	public MicroSplatKeywords keywordSO;

	[HideInInspector]
	public Texture2D perPixelNormal;

	[HideInInspector]
	public Texture2D terrainDesc;

	public MicroSplatObject.DescriptorFormat descriptorFormat;

	[HideInInspector]
	public Texture2D streamTexture;

	[HideInInspector]
	public Texture2D cavityMap;

	[HideInInspector]
	public MicroSplatProceduralTextureConfig procTexCfg;

	[HideInInspector]
	public Texture2D procBiomeMask;

	[HideInInspector]
	public Texture2D procBiomeMask2;

	[HideInInspector]
	public Texture2D tintMapOverride;

	[HideInInspector]
	public Texture2D globalNormalOverride;

	[HideInInspector]
	public Texture2D globalSAOMOverride;

	[HideInInspector]
	public Texture2D globalEmisOverride;

	[HideInInspector]
	public Texture2D geoTextureOverride;

	[HideInInspector]
	public MicroSplatPropData propData;

	public enum DescriptorFormat
	{
		RGBAHalf,
		RGBAFloat
	}
}
