using System;
using UnityEngine;

public class MicroSplatPropData : ScriptableObject
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void RevisionData()
	{
		if (this.values.Length == 256)
		{
			Color[] array = new Color[1024];
			for (int i = 0; i < 16; i++)
			{
				for (int j = 0; j < 16; j++)
				{
					array[j * 32 + i] = this.values[j * 32 + i];
				}
			}
			this.values = array;
			return;
		}
		if (this.values.Length == 512)
		{
			Color[] array2 = new Color[1024];
			for (int k = 0; k < 32; k++)
			{
				for (int l = 0; l < 16; l++)
				{
					array2[l * 32 + k] = this.values[l * 32 + k];
				}
			}
			this.values = array2;
		}
	}

	public Color GetValue(int x, int y)
	{
		this.RevisionData();
		return this.values[y * 32 + x];
	}

	public void SetValue(int x, int y, Color c)
	{
		this.RevisionData();
		this.values[y * 32 + x] = c;
	}

	public void SetValue(int x, int y, int channel, float value)
	{
		this.RevisionData();
		int num = y * 32 + x;
		Color color = this.values[num];
		color[channel] = value;
		this.values[num] = color;
	}

	public void SetValue(int x, int y, int channel, Vector2 value)
	{
		this.RevisionData();
		int num = y * 32 + x;
		Color color = this.values[num];
		if (channel == 0)
		{
			color.r = value.x;
			color.g = value.y;
		}
		else
		{
			color.b = value.x;
			color.a = value.y;
		}
		this.values[num] = color;
	}

	public void SetValue(int textureIndex, MicroSplatPropData.PerTexFloat channel, float value)
	{
		float num = (float)channel / 4f;
		int num2 = (int)num;
		int num3 = Mathf.RoundToInt((num - (float)num2) * 4f);
		this.SetValue(textureIndex, num2, num3, value);
	}

	public void SetValue(int textureIndex, MicroSplatPropData.PerTexColor channel, Color value)
	{
		int num = (int)((float)channel / 4f);
		this.SetValue(textureIndex, num, value);
	}

	public void SetValue(int textureIndex, MicroSplatPropData.PerTexVector2 channel, Vector2 value)
	{
		float num = (float)channel / 4f;
		int num2 = (int)num;
		int num3 = Mathf.RoundToInt((num - (float)num2) * 4f);
		this.SetValue(textureIndex, num2, num3, value);
	}

	public Texture2D GetTexture()
	{
		this.RevisionData();
		if (this.tex == null)
		{
			if (SystemInfo.SupportsTextureFormat(20))
			{
				this.tex = new Texture2D(32, 32, 20, false, true);
			}
			else if (SystemInfo.SupportsTextureFormat(17))
			{
				this.tex = new Texture2D(32, 32, 17, false, true);
			}
			else
			{
				Debug.LogError("Could not create RGBAFloat or RGBAHalf format textures, per texture properties will be clamped to 0-1 range, which will break things");
				this.tex = new Texture2D(32, 32, 4, false, true);
			}
			this.tex.hideFlags = 61;
			this.tex.wrapMode = 1;
			this.tex.filterMode = 0;
		}
		this.tex.SetPixels(this.values);
		this.tex.Apply();
		return this.tex;
	}

	public Texture2D GetGeoCurve()
	{
		if (this.geoTex == null)
		{
			this.geoTex = new Texture2D(256, 1, 15, false, true);
			this.geoTex.hideFlags = 61;
		}
		for (int i = 0; i < 256; i++)
		{
			float num = this.geoCurve.Evaluate((float)i / 255f);
			this.geoTex.SetPixel(i, 0, new Color(num, num, num, num));
		}
		this.geoTex.Apply();
		return this.geoTex;
	}

	public Texture2D GetGeoSlopeFilter()
	{
		if (this.geoSlopeTex == null)
		{
			this.geoSlopeTex = new Texture2D(256, 1, 1, false, true);
			this.geoSlopeTex.hideFlags = 61;
		}
		for (int i = 0; i < 256; i++)
		{
			float num = this.geoSlopeFilter.Evaluate((float)i / 255f);
			this.geoSlopeTex.SetPixel(i, 0, new Color(num, num, num, num));
		}
		this.geoSlopeTex.Apply();
		return this.geoSlopeTex;
	}

	public Texture2D GetGlobalSlopeFilter()
	{
		if (this.globalSlopeTex == null)
		{
			this.globalSlopeTex = new Texture2D(256, 1, 1, false, true);
			this.globalSlopeTex.hideFlags = 61;
		}
		for (int i = 0; i < 256; i++)
		{
			float num = this.globalSlopeFilter.Evaluate((float)i / 255f);
			this.globalSlopeTex.SetPixel(i, 0, new Color(num, num, num, num));
		}
		this.globalSlopeTex.Apply();
		return this.globalSlopeTex;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int sMaxTextures = 32;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int sMaxAttributes = 32;

	public Color[] values = new Color[1024];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D tex;

	[HideInInspector]
	public AnimationCurve geoCurve = AnimationCurve.Linear(0f, 0f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D geoTex;

	[HideInInspector]
	public AnimationCurve geoSlopeFilter = AnimationCurve.Linear(0f, 0.2f, 0.4f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D geoSlopeTex;

	[HideInInspector]
	public AnimationCurve globalSlopeFilter = AnimationCurve.Linear(0f, 0.2f, 0.4f, 1f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D globalSlopeTex;

	public enum PerTexVector2
	{
		SplatUVScale,
		SplatUVOffset = 2
	}

	public enum PerTexColor
	{
		Tint = 4,
		SSSRTint = 72
	}

	public enum PerTexFloat
	{
		InterpolationContrast = 5,
		NormalStrength = 8,
		Smoothness,
		AO,
		Metallic,
		Brightness,
		Contrast,
		Porosity,
		Foam,
		DetailNoiseStrength,
		DistanceNoiseStrength,
		DistanceResample,
		DisplacementMip,
		GeoTexStrength,
		GeoTintStrength,
		GeoNormalStrength,
		GlobalSmoothMetalAOStength,
		DisplacementStength,
		DisplacementBias,
		DisplacementOffset,
		GlobalEmisStength,
		NoiseNormal0Strength,
		NoiseNormal1Strength,
		NoiseNormal2Strength,
		WindParticulateStrength,
		SnowAmount,
		GlitterAmount,
		GeoHeightFilter,
		GeoHeightFilterStrength,
		TriplanarMode,
		TriplanarContrast,
		StochatsicEnabled,
		Saturation,
		TextureClusterContrast,
		TextureClusterBoost,
		HeightOffset,
		HeightContrast,
		AntiTileArrayNormalStrength = 56,
		AntiTileArrayDetailStrength,
		AntiTileArrayDistanceStrength,
		DisplaceShaping,
		UVRotation = 64,
		TriplanarRotationX,
		TriplanarRotationY,
		FuzzyShadingCore = 68,
		FuzzyShadingEdge,
		FuzzyShadingPower,
		SSSThickness = 75
	}
}
