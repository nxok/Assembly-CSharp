using System;
using Unity.Collections;
using UnityEngine;

public class ColorSpectrum
{
	public static ColorSpectrum FromTexture(string _filename)
	{
		Texture2D texture2D = DataLoader.LoadAsset<Texture2D>(_filename, false);
		if (texture2D == null)
		{
			return null;
		}
		ColorSpectrum colorSpectrum = new ColorSpectrum(_filename, texture2D);
		DataLoader.UnloadAsset(_filename, texture2D);
		return colorSpectrum;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsSupportedRawTextureFormat(TextureFormat _format)
	{
		return _format - 3 <= 2 || _format == 14;
	}

	public ColorSpectrum(string name, Texture2D _tex)
	{
		int width = _tex.width;
		this.values = new Color[width];
		if (!this.IsSupportedRawTextureFormat(_tex.format))
		{
			Log.Warning("Color Spectrum texture " + name + " is not in a format supported for non-allocating GetRawTextureData access. Falling back to GetPixels.");
			Color[] pixels = _tex.GetPixels();
			for (int i = 0; i < width; i++)
			{
				this.values[i] = pixels[i].linear;
			}
			return;
		}
		TextureFormat format = _tex.format;
		switch (format)
		{
		case 3:
		{
			NativeArray<TextureUtils.ColorRGB24> rawTextureData = _tex.GetRawTextureData<TextureUtils.ColorRGB24>();
			for (int j = 0; j < width; j++)
			{
				this.values[j] = TextureUtils.GetLinearColor(rawTextureData[j]);
			}
			return;
		}
		case 4:
		{
			NativeArray<Color32> rawTextureData2 = _tex.GetRawTextureData<Color32>();
			for (int k = 0; k < width; k++)
			{
				this.values[k] = TextureUtils.GetLinearColor(rawTextureData2[k]);
			}
			return;
		}
		case 5:
		{
			NativeArray<TextureUtils.ColorARGB32> rawTextureData3 = _tex.GetRawTextureData<TextureUtils.ColorARGB32>();
			for (int l = 0; l < width; l++)
			{
				this.values[l] = TextureUtils.GetLinearColor(rawTextureData3[l]);
			}
			return;
		}
		default:
		{
			if (format != 14)
			{
				return;
			}
			NativeArray<TextureUtils.ColorBGRA32> rawTextureData4 = _tex.GetRawTextureData<TextureUtils.ColorBGRA32>();
			for (int m = 0; m < width; m++)
			{
				this.values[m] = TextureUtils.GetLinearColor(rawTextureData4[m]);
			}
			return;
		}
		}
	}

	public Color GetValue(float _v)
	{
		int num = this.values.Length;
		float num2 = (float)num * _v;
		int num3 = (int)num2;
		Color color = this.values[num3];
		Color color2 = this.values[(num3 + 1) % num];
		return Color.LerpUnclamped(color, color2, num2 - (float)num3);
	}

	public static bool Exists(string _filename)
	{
		return Resources.Load(_filename) != null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Color[] values;
}
