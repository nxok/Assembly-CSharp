using System;
using UnityEngine;

public class FurCombPainter
{
	public void Paint(Texture2D texture, Vector2 uv, Vector3 tangentDirection, BrushSettings brushSettings, FurCombPainter.PaintMode paintMode)
	{
		if (this.texturePaintComputeShader == null)
		{
			this.texturePaintComputeShader = Resources.Load<ComputeShader>("Shaders/TexturePaintCompute");
		}
		if (this.texturePaintComputeShader == null)
		{
			Debug.LogError("TexturePaintCompute.compute not found in Resources folder");
			return;
		}
		int num = this.texturePaintComputeShader.FindKernel("ChannelWrite");
		int num2 = Mathf.FloorToInt(uv.x * (float)texture.width);
		int num3 = Mathf.FloorToInt(uv.y * (float)texture.height);
		RenderTexture renderTexture = new RenderTexture(texture.width, texture.height, 0, 0);
		renderTexture.enableRandomWrite = true;
		renderTexture.useMipMap = texture.mipmapCount > 1;
		renderTexture.Create();
		Graphics.CopyTexture(texture, renderTexture);
		Vector3 vector = tangentDirection * 0.5f + Vector3.one * 0.5f;
		this.texturePaintComputeShader.SetInts("PaintPosition", new int[] { num2, num3 });
		this.texturePaintComputeShader.SetInt("BrushRadius", (int)brushSettings.Size);
		this.texturePaintComputeShader.SetFloat("BrushStrength", brushSettings.Strength);
		this.texturePaintComputeShader.SetFloat("BrushFalloff", brushSettings.Falloff);
		this.texturePaintComputeShader.SetVector("BrushColor", brushSettings.Color);
		switch (paintMode)
		{
		case FurCombPainter.PaintMode.Albedo:
			this.texturePaintComputeShader.SetVector("BrushValue", new Vector4(brushSettings.Color.r, brushSettings.Color.g, brushSettings.Color.b, brushSettings.Color.a));
			this.texturePaintComputeShader.SetVector("ChannelMask", new Vector4(1f, 1f, 1f, 0f));
			this.texturePaintComputeShader.SetTexture(num, "Result", renderTexture);
			this.texturePaintComputeShader.Dispatch(num, texture.width / 8, texture.height / 8, 1);
			break;
		case FurCombPainter.PaintMode.Direction:
			this.texturePaintComputeShader.SetVector("BrushValue", new Vector4(vector.x, vector.y, 0f, 0f));
			this.texturePaintComputeShader.SetVector("ChannelMask", new Vector4(1f, 1f, 0f, 0f));
			this.texturePaintComputeShader.SetTexture(num, "Result", renderTexture);
			this.texturePaintComputeShader.Dispatch(num, texture.width / 8, texture.height / 8, 1);
			break;
		case FurCombPainter.PaintMode.Matting:
			this.texturePaintComputeShader.SetVector("ChannelMask", new Vector4(0f, 0f, 0f, 1f));
			this.texturePaintComputeShader.SetVector("BrushValue", new Vector4(0f, 0f, 0f, brushSettings.Matting));
			this.texturePaintComputeShader.SetTexture(num, "Result", renderTexture);
			this.texturePaintComputeShader.Dispatch(num, texture.width / 8, texture.height / 8, 1);
			break;
		case FurCombPainter.PaintMode.DirectionAndMatting:
			this.texturePaintComputeShader.SetVector("ChannelMask", new Vector4(1f, 1f, 0f, 1f));
			this.texturePaintComputeShader.SetVector("BrushValue", new Vector4(vector.x, vector.y, 0f, brushSettings.Matting));
			this.texturePaintComputeShader.SetTexture(num, "Result", renderTexture);
			this.texturePaintComputeShader.Dispatch(num, texture.width / 8, texture.height / 8, 1);
			break;
		case FurCombPainter.PaintMode.Length:
			this.texturePaintComputeShader.SetVector("ChannelMask", new Vector4(0f, 0f, 1f, 0f));
			this.texturePaintComputeShader.SetVector("BrushValue", new Vector4(0f, 0f, brushSettings.Length, 0f));
			this.texturePaintComputeShader.SetTexture(num, "Result", renderTexture);
			this.texturePaintComputeShader.Dispatch(num, texture.width / 8, texture.height / 8, 1);
			break;
		case FurCombPainter.PaintMode.Roughness:
			this.texturePaintComputeShader.SetVector("ChannelMask", new Vector4(1f, 0f, 0f, 0f));
			this.texturePaintComputeShader.SetVector("BrushValue", new Vector4(brushSettings.Roughness, 0f, 0f, 0f));
			this.texturePaintComputeShader.SetTexture(num, "Result", renderTexture);
			this.texturePaintComputeShader.Dispatch(num, texture.width / 8, texture.height / 8, 1);
			break;
		case FurCombPainter.PaintMode.Metallic:
			this.texturePaintComputeShader.SetVector("ChannelMask", new Vector4(0f, 1f, 0f, 0f));
			this.texturePaintComputeShader.SetVector("BrushValue", new Vector4(0f, brushSettings.Metallic, 0f, 0f));
			this.texturePaintComputeShader.SetTexture(num, "Result", renderTexture);
			this.texturePaintComputeShader.Dispatch(num, texture.width / 8, texture.height / 8, 1);
			break;
		case FurCombPainter.PaintMode.Occlusion:
			this.texturePaintComputeShader.SetVector("ChannelMask", new Vector4(0f, 0f, 1f, 0f));
			this.texturePaintComputeShader.SetVector("BrushValue", new Vector4(0f, 0f, brushSettings.Occlusion, 0f));
			this.texturePaintComputeShader.SetTexture(num, "Result", renderTexture);
			this.texturePaintComputeShader.Dispatch(num, texture.width / 8, texture.height / 8, 1);
			break;
		}
		RenderTexture.active = renderTexture;
		texture.ReadPixels(new Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height), 0, 0);
		texture.Apply();
		RenderTexture.active = null;
		renderTexture.Release();
	}

	public ComputeShader texturePaintComputeShader;

	public enum PaintMode
	{
		Albedo,
		Direction,
		Matting,
		DirectionAndMatting,
		Length,
		Roughness,
		Metallic,
		Occlusion
	}
}
