using System;
using System.Collections.Generic;
using UnityEngine;

public class MicroSplatProceduralTextureConfig : ScriptableObject
{
	public void ResetToDefault()
	{
		this.layers = new List<MicroSplatProceduralTextureConfig.Layer>(3);
		this.layers.Add(new MicroSplatProceduralTextureConfig.Layer());
		this.layers.Add(new MicroSplatProceduralTextureConfig.Layer());
		this.layers.Add(new MicroSplatProceduralTextureConfig.Layer());
		this.layers[1].textureIndex = 1;
		this.layers[1].slopeActive = true;
		this.layers[1].slopeCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0.03f, 0f),
			new Keyframe(0.06f, 1f),
			new Keyframe(0.16f, 1f),
			new Keyframe(0.2f, 0f)
		});
		this.layers[0].slopeActive = true;
		this.layers[0].textureIndex = 2;
		this.layers[0].slopeCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0.13f, 0f),
			new Keyframe(0.25f, 1f)
		});
	}

	public Texture2D GetHeightGradientTexture()
	{
		int num = 32;
		int num2 = 128;
		if (this.heightGradientTex == null)
		{
			this.heightGradientTex = new Texture2D(num2, num, 4, false);
			this.heightGradientTex.hideFlags = 61;
		}
		Color grey = Color.grey;
		for (int i = 0; i < this.heightGradients.Count; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				float num3 = (float)j / (float)num2;
				Color color = this.heightGradients[i].Evaluate(num3);
				this.heightGradientTex.SetPixel(j, i, color);
			}
		}
		for (int k = this.heightGradients.Count; k < 32; k++)
		{
			for (int l = 0; l < num2; l++)
			{
				this.heightGradientTex.SetPixel(l, k, grey);
			}
		}
		this.heightGradientTex.Apply(false, false);
		return this.heightGradientTex;
	}

	public Texture2D GetHeightHSVTexture()
	{
		int num = 32;
		int num2 = 128;
		if (this.heightHSVTex == null)
		{
			this.heightHSVTex = new Texture2D(num2, num, 4, false);
			this.heightHSVTex.hideFlags = 61;
		}
		Color grey = Color.grey;
		for (int i = 0; i < this.heightHSV.Count; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				Color color = grey;
				float num3 = (float)j / (float)num2;
				color.r = this.heightHSV[i].H.Evaluate(num3) * 0.5f + 0.5f;
				color.g = this.heightHSV[i].S.Evaluate(num3) * 0.5f + 0.5f;
				color.b = this.heightHSV[i].V.Evaluate(num3) * 0.5f + 0.5f;
				this.heightHSVTex.SetPixel(j, i, color);
			}
		}
		for (int k = this.heightHSV.Count; k < 32; k++)
		{
			for (int l = 0; l < num2; l++)
			{
				this.heightHSVTex.SetPixel(l, k, grey);
			}
		}
		this.heightHSVTex.Apply(false, false);
		return this.heightHSVTex;
	}

	public Texture2D GetSlopeGradientTexture()
	{
		int num = 32;
		int num2 = 128;
		if (this.slopeGradientTex == null)
		{
			this.slopeGradientTex = new Texture2D(num2, num, 4, false);
			this.slopeGradientTex.hideFlags = 61;
		}
		Color grey = Color.grey;
		for (int i = 0; i < this.slopeGradients.Count; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				float num3 = (float)j / (float)num2;
				Color color = this.slopeGradients[i].Evaluate(num3);
				this.slopeGradientTex.SetPixel(j, i, color);
			}
		}
		for (int k = this.slopeGradients.Count; k < 32; k++)
		{
			for (int l = 0; l < num2; l++)
			{
				this.slopeGradientTex.SetPixel(l, k, grey);
			}
		}
		this.slopeGradientTex.Apply(false, false);
		return this.slopeGradientTex;
	}

	public Texture2D GetSlopeHSVTexture()
	{
		int num = 32;
		int num2 = 128;
		if (this.slopeHSVTex == null)
		{
			this.slopeHSVTex = new Texture2D(num2, num, 4, false);
			this.slopeHSVTex.hideFlags = 61;
		}
		Color grey = Color.grey;
		for (int i = 0; i < this.slopeHSV.Count; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				Color color = grey;
				float num3 = (float)j / (float)num2;
				color.r = this.slopeHSV[i].H.Evaluate(num3) * 0.5f + 0.5f;
				color.g = this.slopeHSV[i].S.Evaluate(num3) * 0.5f + 0.5f;
				color.b = this.slopeHSV[i].V.Evaluate(num3) * 0.5f + 0.5f;
				this.slopeHSVTex.SetPixel(j, i, color);
			}
		}
		for (int k = this.slopeHSV.Count; k < 32; k++)
		{
			for (int l = 0; l < num2; l++)
			{
				this.slopeHSVTex.SetPixel(l, k, grey);
			}
		}
		this.slopeHSVTex.Apply(false, false);
		return this.slopeHSVTex;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float CompFilter(MicroSplatProceduralTextureConfig.Layer.Filter f, MicroSplatProceduralTextureConfig.Layer.CurveMode mode, float v)
	{
		float num = Mathf.Abs(v - f.center) * (1f / Mathf.Max(f.width, 0.0001f));
		num = Mathf.Clamp01(Mathf.Pow(num, f.contrast));
		switch (mode)
		{
		case MicroSplatProceduralTextureConfig.Layer.CurveMode.BoostFilter:
			return 1f - num;
		case MicroSplatProceduralTextureConfig.Layer.CurveMode.HighPass:
			if (v >= f.center)
			{
				return 1f;
			}
			return 1f - num;
		case MicroSplatProceduralTextureConfig.Layer.CurveMode.LowPass:
			if (v <= f.center)
			{
				return 1f;
			}
			return 1f - num;
		case MicroSplatProceduralTextureConfig.Layer.CurveMode.CutFilter:
			return num;
		default:
			Debug.LogError("Unhandled case in ProceduralTextureConfig::CompFilter");
			return 0f;
		}
	}

	public Texture2D GetCurveTexture()
	{
		int num = 32;
		int num2 = (int)this.proceduralCurveTextureSize;
		if (this.curveTex != null && this.curveTex.width != num2)
		{
			Object.DestroyImmediate(this.curveTex);
			this.curveTex = null;
		}
		if (this.curveTex == null)
		{
			this.curveTex = new Texture2D(num2, num, 4, false, true);
			this.curveTex.hideFlags = 61;
		}
		Color white = Color.white;
		for (int i = 0; i < this.layers.Count; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				Color color = white;
				float num3 = (float)j / (float)num2;
				if (this.layers[i].heightActive)
				{
					if (this.layers[i].heightCurveMode == MicroSplatProceduralTextureConfig.Layer.CurveMode.Curve)
					{
						color.r = this.layers[i].heightCurve.Evaluate(num3);
					}
					else
					{
						color.r = this.CompFilter(this.layers[i].heightFilter, this.layers[i].heightCurveMode, num3);
					}
				}
				if (this.layers[i].slopeActive)
				{
					if (this.layers[i].slopeCurveMode == MicroSplatProceduralTextureConfig.Layer.CurveMode.Curve)
					{
						color.g = this.layers[i].slopeCurve.Evaluate(num3);
					}
					else
					{
						color.g = this.CompFilter(this.layers[i].slopeFilter, this.layers[i].slopeCurveMode, num3);
					}
				}
				if (this.layers[i].cavityMapActive)
				{
					if (this.layers[i].cavityCurveMode == MicroSplatProceduralTextureConfig.Layer.CurveMode.Curve)
					{
						color.b = this.layers[i].cavityMapCurve.Evaluate(num3);
					}
					else
					{
						color.b = this.CompFilter(this.layers[i].cavityMapFilter, this.layers[i].cavityCurveMode, num3);
					}
				}
				if (this.layers[i].erosionMapActive)
				{
					if (this.layers[i].erosionCurveMode == MicroSplatProceduralTextureConfig.Layer.CurveMode.Curve)
					{
						color.a = this.layers[i].erosionMapCurve.Evaluate(num3);
					}
					else
					{
						color.a = this.CompFilter(this.layers[i].erosionFilter, this.layers[i].erosionCurveMode, num3);
					}
				}
				this.curveTex.SetPixel(j, i, color);
			}
		}
		this.curveTex.Apply(false, false);
		return this.curveTex;
	}

	public Texture2D GetParamTexture()
	{
		int num = 32;
		int num2 = 4;
		if (this.paramTex == null || this.paramTex.format != 17 || this.paramTex.width != num2)
		{
			this.paramTex = new Texture2D(num2, num, 17, false, true);
			this.paramTex.hideFlags = 61;
		}
		Color color;
		color..ctor(0f, 0f, 0f, 0f);
		for (int i = 0; i < this.layers.Count; i++)
		{
			Color color2 = color;
			Color color3 = color;
			if (this.layers[i].noiseActive)
			{
				color2.r = this.layers[i].noiseFrequency;
				color2.g = this.layers[i].noiseRange.x;
				color2.b = this.layers[i].noiseRange.y;
				color2.a = this.layers[i].noiseOffset;
			}
			color3.r = this.layers[i].weight;
			color3.g = (float)this.layers[i].textureIndex;
			this.paramTex.SetPixel(0, i, color2);
			this.paramTex.SetPixel(1, i, color3);
			Vector4 biomeWeights = this.layers[i].biomeWeights;
			this.paramTex.SetPixel(2, i, new Color(biomeWeights.x, biomeWeights.y, biomeWeights.z, biomeWeights.w));
			Vector4 biomeWeights2 = this.layers[i].biomeWeights2;
			this.paramTex.SetPixel(3, i, new Color(biomeWeights2.x, biomeWeights2.y, biomeWeights2.z, biomeWeights2.w));
		}
		this.paramTex.Apply(false, false);
		return this.paramTex;
	}

	public MicroSplatProceduralTextureConfig.TableSize proceduralCurveTextureSize = MicroSplatProceduralTextureConfig.TableSize.k256;

	public List<Gradient> heightGradients = new List<Gradient>();

	public List<MicroSplatProceduralTextureConfig.HSVCurve> heightHSV = new List<MicroSplatProceduralTextureConfig.HSVCurve>();

	public List<Gradient> slopeGradients = new List<Gradient>();

	public List<MicroSplatProceduralTextureConfig.HSVCurve> slopeHSV = new List<MicroSplatProceduralTextureConfig.HSVCurve>();

	[HideInInspector]
	public List<MicroSplatProceduralTextureConfig.Layer> layers = new List<MicroSplatProceduralTextureConfig.Layer>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D curveTex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D paramTex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D heightGradientTex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D heightHSVTex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D slopeGradientTex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D slopeHSVTex;

	public enum TableSize
	{
		k64 = 64,
		k128 = 128,
		k256 = 256,
		k512 = 512,
		k1024 = 1024,
		k2048 = 2048,
		k4096 = 4096
	}

	[Serializable]
	public class Layer
	{
		public MicroSplatProceduralTextureConfig.Layer Copy()
		{
			return new MicroSplatProceduralTextureConfig.Layer
			{
				weight = this.weight,
				textureIndex = this.textureIndex,
				noiseActive = this.noiseActive,
				noiseFrequency = this.noiseFrequency,
				noiseOffset = this.noiseOffset,
				noiseRange = this.noiseRange,
				biomeWeights = this.biomeWeights,
				biomeWeights2 = this.biomeWeights2,
				heightActive = this.heightActive,
				slopeActive = this.slopeActive,
				erosionMapActive = this.erosionMapActive,
				cavityMapActive = this.cavityMapActive,
				heightCurve = new AnimationCurve(this.heightCurve.keys),
				slopeCurve = new AnimationCurve(this.slopeCurve.keys),
				erosionMapCurve = new AnimationCurve(this.erosionMapCurve.keys),
				cavityMapCurve = new AnimationCurve(this.cavityMapCurve.keys),
				cavityMapFilter = this.cavityMapFilter,
				heightFilter = this.heightFilter,
				slopeFilter = this.slopeFilter,
				erosionFilter = this.erosionFilter,
				heightCurveMode = this.heightCurveMode,
				slopeCurveMode = this.slopeCurveMode,
				erosionCurveMode = this.erosionCurveMode,
				cavityCurveMode = this.cavityCurveMode
			};
		}

		public float weight = 1f;

		public int textureIndex;

		public bool noiseActive;

		public float noiseFrequency = 1f;

		public float noiseOffset;

		public Vector2 noiseRange = new Vector2(0f, 1f);

		public Vector4 biomeWeights = new Vector4(1f, 1f, 1f, 1f);

		public Vector4 biomeWeights2 = new Vector4(1f, 1f, 1f, 1f);

		public bool heightActive;

		public AnimationCurve heightCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public MicroSplatProceduralTextureConfig.Layer.Filter heightFilter = new MicroSplatProceduralTextureConfig.Layer.Filter();

		public bool slopeActive;

		public AnimationCurve slopeCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public MicroSplatProceduralTextureConfig.Layer.Filter slopeFilter = new MicroSplatProceduralTextureConfig.Layer.Filter();

		public bool erosionMapActive;

		public AnimationCurve erosionMapCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public MicroSplatProceduralTextureConfig.Layer.Filter erosionFilter = new MicroSplatProceduralTextureConfig.Layer.Filter();

		public bool cavityMapActive;

		public AnimationCurve cavityMapCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

		public MicroSplatProceduralTextureConfig.Layer.Filter cavityMapFilter = new MicroSplatProceduralTextureConfig.Layer.Filter();

		public MicroSplatProceduralTextureConfig.Layer.CurveMode heightCurveMode;

		public MicroSplatProceduralTextureConfig.Layer.CurveMode slopeCurveMode;

		public MicroSplatProceduralTextureConfig.Layer.CurveMode erosionCurveMode;

		public MicroSplatProceduralTextureConfig.Layer.CurveMode cavityCurveMode;

		[Serializable]
		public class Filter
		{
			public float center = 0.5f;

			public float width = 0.1f;

			public float contrast = 1f;
		}

		public enum CurveMode
		{
			Curve,
			BoostFilter,
			HighPass,
			LowPass,
			CutFilter
		}
	}

	[Serializable]
	public class HSVCurve
	{
		public AnimationCurve H = AnimationCurve.Linear(0f, 0.5f, 1f, 0.5f);

		public AnimationCurve S = AnimationCurve.Linear(0f, 0f, 1f, 0f);

		public AnimationCurve V = AnimationCurve.Linear(0f, 0f, 1f, 0f);
	}
}
