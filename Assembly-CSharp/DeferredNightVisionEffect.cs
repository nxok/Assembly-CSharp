using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DeferredNightVisionEffect : MonoBehaviour
{
	public Shader NightVisionShader
	{
		get
		{
			return this.m_Shader;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DestroyMaterial(Material mat)
	{
		if (mat)
		{
			Object.DestroyImmediate(mat);
			mat = null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CreateMaterials()
	{
		if (this.m_Shader == null)
		{
			this.m_Shader = Shader.Find("Custom/DeferredNightVisionShader");
		}
		if (this.m_Material == null && this.m_Shader != null && this.m_Shader.isSupported)
		{
			this.m_Material = this.CreateMaterial(this.m_Shader);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Material CreateMaterial(Shader shader)
	{
		if (!shader)
		{
			return null;
		}
		return new Material(shader)
		{
			hideFlags = 61
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDisable()
	{
		this.DestroyMaterial(this.m_Material);
		this.m_Material = null;
		this.m_Shader = null;
	}

	[ContextMenu("UpdateShaderValues")]
	public void UpdateShaderValues()
	{
		if (this.m_Material == null)
		{
			return;
		}
		this.m_Material.SetVector("_NVColor", this.m_NVColor);
		this.m_Material.SetVector("_TargetWhiteColor", this.m_TargetBleachColor);
		this.m_Material.SetFloat("_BaseLightingContribution", this.m_baseLightingContribution);
		this.m_Material.SetFloat("_LightSensitivityMultiplier", this.m_LightSensitivityMultiplier);
		this.m_Material.shaderKeywords = null;
		if (this.useVignetting)
		{
			Shader.EnableKeyword("USE_VIGNETTE");
			return;
		}
		Shader.DisableKeyword("USE_VIGNETTE");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.CreateMaterials();
		this.UpdateShaderValues();
	}

	public void ReloadShaders()
	{
		this.OnDisable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		this.UpdateShaderValues();
		this.CreateMaterials();
		Graphics.Blit(source, destination, this.m_Material);
	}

	[SerializeField]
	[Tooltip("The main color of the NV effect")]
	public Color m_NVColor = new Color(0f, 1f, 0.1724138f, 0f);

	[SerializeField]
	[Tooltip("The color that the NV effect will 'bleach' towards (white = default)")]
	public Color m_TargetBleachColor = new Color(1f, 1f, 1f, 0f);

	[Range(0f, 1f)]
	[Tooltip("How much base lighting does the NV effect pick up")]
	public float m_baseLightingContribution = 0.025f;

	[Range(0f, 128f)]
	[Tooltip("The higher this value, the more bright areas will get 'bleached out'")]
	public float m_LightSensitivityMultiplier = 100f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material m_Material;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Shader m_Shader;

	[Tooltip("Do we want to apply a vignette to the edges of the screen?")]
	public bool useVignetting = true;
}
