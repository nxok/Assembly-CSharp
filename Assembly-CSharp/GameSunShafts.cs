using System;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("Image Effects/Rendering/Game Sun Shafts")]
public class GameSunShafts : PostEffectsBase
{
	public override bool CheckResources()
	{
		base.CheckSupport(this.useDepthTexture);
		this.sunShaftsMaterial = base.CheckShaderAndCreateMaterial(this.sunShaftsShader, this.sunShaftsMaterial);
		this.simpleClearMaterial = base.CheckShaderAndCreateMaterial(this.simpleClearShader, this.simpleClearMaterial);
		if (!this.isSupported)
		{
			base.ReportAutoDisable();
		}
		return this.isSupported;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (!this.CheckResources())
		{
			Graphics.Blit(source, destination);
			return;
		}
		if (this.useDepthTexture)
		{
			base.GetComponent<Camera>().depthTextureMode |= 1;
		}
		int num = 4;
		if (this.resolution == GameSunShafts.SunShaftsResolution.Normal)
		{
			num = 2;
		}
		else if (this.resolution == GameSunShafts.SunShaftsResolution.High)
		{
			num = 1;
		}
		Vector3 vector = base.GetComponent<Camera>().WorldToViewportPoint(this.sunPosition);
		int num2 = source.width / num;
		int num3 = source.height / num;
		RenderTexture renderTexture = RenderTexture.GetTemporary(num2, num3, 0);
		this.sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(1f, 1f, 0f, 0f) * this.sunShaftBlurRadius);
		this.sunShaftsMaterial.SetVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, this.maxRadius));
		this.sunShaftsMaterial.SetVector("_SunThreshold", this.sunThreshold);
		if (!this.useDepthTexture)
		{
			RenderTextureFormat renderTextureFormat = (base.GetComponent<Camera>().allowHDR ? 9 : 7);
			RenderTexture temporary = RenderTexture.GetTemporary(source.width, source.height, 0, renderTextureFormat);
			RenderTexture.active = temporary;
			GL.ClearWithSkybox(false, base.GetComponent<Camera>());
			this.sunShaftsMaterial.SetTexture("_Skybox", temporary);
			Graphics.Blit(source, renderTexture, this.sunShaftsMaterial, 3);
			RenderTexture.ReleaseTemporary(temporary);
		}
		else
		{
			Graphics.Blit(source, renderTexture, this.sunShaftsMaterial, 2);
		}
		base.DrawBorder(renderTexture, this.simpleClearMaterial);
		this.radialBlurIterations = Mathf.Clamp(this.radialBlurIterations, 1, 4);
		float num4 = this.sunShaftBlurRadius * 0.0013020834f;
		this.sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num4, num4, 0f, 0f));
		this.sunShaftsMaterial.SetVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, this.maxRadius));
		for (int i = 0; i < this.radialBlurIterations; i++)
		{
			RenderTexture temporary2 = RenderTexture.GetTemporary(num2, num3, 0);
			Graphics.Blit(renderTexture, temporary2, this.sunShaftsMaterial, 1);
			RenderTexture.ReleaseTemporary(renderTexture);
			num4 = this.sunShaftBlurRadius * (((float)i * 2f + 1f) * 6f) / 768f;
			this.sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num4, num4, 0f, 0f));
			renderTexture = RenderTexture.GetTemporary(num2, num3, 0);
			Graphics.Blit(temporary2, renderTexture, this.sunShaftsMaterial, 1);
			RenderTexture.ReleaseTemporary(temporary2);
			num4 = this.sunShaftBlurRadius * (((float)i * 2f + 2f) * 6f) / 768f;
			this.sunShaftsMaterial.SetVector("_BlurRadius4", new Vector4(num4, num4, 0f, 0f));
		}
		if (vector.z >= 0f)
		{
			this.sunShaftsMaterial.SetVector("_SunColor", new Vector4(this.sunColor.r, this.sunColor.g, this.sunColor.b, this.sunColor.a) * this.sunShaftIntensity);
		}
		else
		{
			this.sunShaftsMaterial.SetVector("_SunColor", Vector4.zero);
		}
		this.sunShaftsMaterial.SetTexture("_ColorBuffer", renderTexture);
		Graphics.Blit(source, destination, this.sunShaftsMaterial, (this.screenBlendMode == GameSunShafts.ShaftsScreenBlendMode.Screen) ? 0 : 4);
		RenderTexture.ReleaseTemporary(renderTexture);
	}

	public GameSunShafts.SunShaftsResolution resolution = GameSunShafts.SunShaftsResolution.Normal;

	public GameSunShafts.ShaftsScreenBlendMode screenBlendMode;

	public Vector3 sunPosition;

	public int radialBlurIterations = 2;

	public Color sunColor = Color.white;

	public Color sunThreshold = new Color(0.87f, 0.74f, 0.65f);

	public float sunShaftBlurRadius = 2.5f;

	public float sunShaftIntensity = 1.15f;

	public float maxRadius = 0.75f;

	public bool useDepthTexture = true;

	public Shader sunShaftsShader;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material sunShaftsMaterial;

	public Shader simpleClearShader;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material simpleClearMaterial;

	public enum SunShaftsResolution
	{
		Low,
		Normal,
		High
	}

	public enum ShaftsScreenBlendMode
	{
		Screen,
		Add
	}
}
