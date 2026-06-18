using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public sealed class SunShaftsEffectRenderer : PostProcessEffectRenderer<SunShaftsEffect>
{
	public Mesh fullscreenTriangle
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_FullscreenTriangle != null)
			{
				return this.m_FullscreenTriangle;
			}
			this.m_FullscreenTriangle = new Mesh
			{
				name = "Fullscreen Triangle"
			};
			this.m_FullscreenTriangle.SetVertices(new List<Vector3>
			{
				new Vector3(-1f, -1f, 0f),
				new Vector3(-1f, 3f, 0f),
				new Vector3(3f, -1f, 0f)
			});
			this.m_FullscreenTriangle.SetIndices(new int[] { 0, 1, 2 }, 0, 0, false);
			this.m_FullscreenTriangle.UploadMeshData(false);
			return this.m_FullscreenTriangle;
		}
	}

	public override void Init()
	{
		base.Init();
		this.sunShaftsMaterial = new Material(base.settings.sunShaftsShader);
		this.sunShaftsMaterial.hideFlags = 52;
		this.simpleClearMaterial = new Material(base.settings.simpleClearShader);
		this.simpleClearMaterial.hideFlags = 52;
	}

	public override void Release()
	{
		base.Release();
		Object.Destroy(this.sunShaftsMaterial);
		Object.Destroy(this.simpleClearMaterial);
	}

	public void DrawBorder(CommandBuffer cmd, RenderTargetIdentifier dest, int width, int height, Material material, int borderWidth = 1)
	{
		cmd.SetRenderTarget(dest);
		Matrix4x4 matrix4x = Matrix4x4.Ortho(-1f, 1f, -1f, 1f, 0f, 1f);
		Matrix4x4 identity = Matrix4x4.identity;
		cmd.SetViewProjectionMatrices(identity, matrix4x);
		cmd.SetViewport(new Rect(0f, 0f, (float)borderWidth, (float)height));
		cmd.DrawMesh(this.fullscreenTriangle, Matrix4x4.identity, material);
		cmd.SetViewport(new Rect((float)(width - borderWidth), 0f, (float)borderWidth, (float)height));
		cmd.DrawMesh(this.fullscreenTriangle, Matrix4x4.identity, material);
		cmd.SetViewport(new Rect(0f, 0f, (float)width, (float)borderWidth));
		cmd.DrawMesh(this.fullscreenTriangle, Matrix4x4.identity, material);
		cmd.SetViewport(new Rect(0f, (float)(height - borderWidth), (float)width, (float)borderWidth));
		cmd.DrawMesh(this.fullscreenTriangle, Matrix4x4.identity, material);
	}

	public override void Render(PostProcessRenderContext context)
	{
		SunShaftsEffect.SunSettings sunSettings = (base.settings.autoUpdateSun ? SkyManager.GetSunShaftSettings() : base.settings.GetSunSettings());
		int num = 4;
		if (base.settings.resolution == SunShaftsEffect.SunShaftsResolution.Normal)
		{
			num = 2;
		}
		else if (base.settings.resolution == SunShaftsEffect.SunShaftsResolution.High)
		{
			num = 1;
		}
		Vector3 vector = context.camera.WorldToViewportPoint(sunSettings.sunPosition);
		int width = context.width;
		int height = context.height;
		int num2 = width / num;
		int num3 = height / num;
		int num4 = Shader.PropertyToID("_Temp1");
		context.command.GetTemporaryRT(num4, num2, num3, 0);
		context.command.SetGlobalVector("_BlurRadius4", new Vector4(1f, 1f, 0f, 0f) * base.settings.sunShaftBlurRadius);
		context.command.SetGlobalVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, base.settings.maxRadius));
		context.command.SetGlobalVector("_SunThreshold", sunSettings.sunThreshold);
		context.command.Blit(context.source, num4, this.sunShaftsMaterial, 2);
		this.DrawBorder(context.command, num4, num2, num3, this.simpleClearMaterial, 1);
		int num5 = Mathf.Clamp(base.settings.radialBlurIterations, 1, 4);
		float num6 = base.settings.sunShaftBlurRadius * 0.0013020834f;
		context.command.SetGlobalVector("_BlurRadius4", new Vector4(num6, num6, 0f, 0f));
		context.command.SetGlobalVector("_SunPosition", new Vector4(vector.x, vector.y, vector.z, base.settings.maxRadius));
		for (int i = 0; i < num5; i++)
		{
			int num7 = Shader.PropertyToID("_Temp3");
			context.command.GetTemporaryRT(num7, num2, num3, 0);
			context.command.Blit(num4, num7, this.sunShaftsMaterial, 1);
			context.command.ReleaseTemporaryRT(num4);
			num6 = base.settings.sunShaftBlurRadius * (((float)i * 2f + 1f) * 6f) / 768f;
			context.command.SetGlobalVector("_BlurRadius4", new Vector4(num6, num6, 0f, 0f));
			num4 = Shader.PropertyToID("_Temp4");
			context.command.GetTemporaryRT(num4, num2, num3, 0);
			context.command.Blit(num7, num4, this.sunShaftsMaterial, 1);
			context.command.ReleaseTemporaryRT(num7);
			num6 = base.settings.sunShaftBlurRadius * (((float)i * 2f + 2f) * 6f) / 768f;
			context.command.SetGlobalVector("_BlurRadius4", new Vector4(num6, num6, 0f, 0f));
		}
		if (vector.z >= 0f)
		{
			context.command.SetGlobalVector("_SunColor", sunSettings.sunColor * sunSettings.sunShaftIntensity);
		}
		else
		{
			context.command.SetGlobalVector("_SunColor", Vector4.zero);
		}
		context.command.SetGlobalTexture("_ColorBuffer", num4);
		context.command.Blit(context.source, context.destination, this.sunShaftsMaterial, (base.settings.screenBlendMode == SunShaftsEffect.ShaftsScreenBlendMode.Screen) ? 0 : 4);
		context.command.ReleaseTemporaryRT(num4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Material sunShaftsMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material simpleClearMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mesh m_FullscreenTriangle;
}
