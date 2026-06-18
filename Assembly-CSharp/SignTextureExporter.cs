using System;
using System.IO;
using UnityEngine;

public sealed class SignTextureExporter
{
	public static SignTextureExporter Instance
	{
		get
		{
			return SignTextureExporter.m_instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public SignTextureExporter()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Initialize()
	{
		this.shader = GlobalAssets.FindShader("Game/SignTech/UI");
		if (this.shader == null)
		{
			throw new InvalidOperationException("Shader not found: Game/SignTech/UI");
		}
		this.material = new Material(this.shader)
		{
			hideFlags = 61
		};
		this.rt = new RenderTexture(4096, 4096, 0, 0, 2)
		{
			name = "SignTextureExporter_RT",
			useMipMap = false,
			autoGenerateMips = false,
			wrapMode = 1,
			filterMode = 1,
			antiAliasing = 1
		};
		this.rt.Create();
		this.readback = new Texture2D(4096, 4096, 4, false, false);
		this.readback.name = "SignTextureExporter_Readback";
		this.initialized = true;
	}

	public void Cleanup()
	{
		this.initialized = false;
		if (this.material != null)
		{
			Object.DestroyImmediate(this.material);
		}
		if (this.rt != null)
		{
			if (this.rt.IsCreated())
			{
				this.rt.Release();
			}
			Object.DestroyImmediate(this.rt);
		}
		if (this.readback != null)
		{
			Object.DestroyImmediate(this.readback);
		}
		this.shader = null;
		this.material = null;
		this.rt = null;
		this.readback = null;
	}

	public string ExportSignToTexture(string signName, GlobalSignId signId)
	{
		if (!this.initialized)
		{
			this.Initialize();
		}
		SignDataManager.Instance.TryApplyRenderingData(signId, 1f, this.material, null, SignUIStyle.Standard);
		RenderTexture active = RenderTexture.active;
		string text3;
		try
		{
			RenderTexture.active = this.rt;
			GL.Clear(true, true, Color.clear);
			Graphics.Blit(null, this.rt, this.material);
			this.readback.ReadPixels(new Rect(0f, 0f, 4096f, 4096f), 0, 0, false);
			this.readback.Apply(false, false);
			byte[] array = ImageConversion.EncodeToPNG(this.readback);
			if (string.IsNullOrEmpty(signName))
			{
				signName = "Unnamed Sign";
			}
			string text = string.Format("{0}_{1:yyyyMMdd_HHmmss}.png", signName, DateTime.Now);
			string text2 = Path.Combine(Application.dataPath + "/../", text);
			File.WriteAllBytes(text2, array);
			text3 = text2;
		}
		finally
		{
			RenderTexture.active = active;
		}
		return text3;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly SignTextureExporter m_instance = new SignTextureExporter();

	[PublicizedFrom(EAccessModifier.Private)]
	public const int kResolutionMultiplier = 4;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int kWidth = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int kHeight = 4096;

	[PublicizedFrom(EAccessModifier.Private)]
	public const string kShaderName = "Game/SignTech/UI";

	[PublicizedFrom(EAccessModifier.Private)]
	public Shader shader;

	[PublicizedFrom(EAccessModifier.Private)]
	public Material material;

	[PublicizedFrom(EAccessModifier.Private)]
	public RenderTexture rt;

	[PublicizedFrom(EAccessModifier.Private)]
	public Texture2D readback;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool initialized;
}
