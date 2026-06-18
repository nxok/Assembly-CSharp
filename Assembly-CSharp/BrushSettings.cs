using System;
using UnityEngine;

[Serializable]
public class BrushSettings
{
	public string MainTexName { get; set; } = "_MainTex";

	public string DirectionMapName { get; set; } = "_DirectionMap";

	public string RMOLMapName { get; set; } = "_RMOL";

	public float Size
	{
		get
		{
			return this.size;
		}
		set
		{
			this.UpdateBrushPreview();
			this.size = Mathf.Max(1f, value);
		}
	}

	public float Strength
	{
		get
		{
			return this.strength;
		}
		set
		{
			this.isDirty = true;
			this.strength = Mathf.Clamp01(value);
		}
	}

	public float Falloff
	{
		get
		{
			return this.falloff;
		}
		set
		{
			this.isDirty = true;
			this.falloff = Mathf.Clamp01(value);
		}
	}

	public float Matting
	{
		get
		{
			return this.matting;
		}
		set
		{
			this.matting = Mathf.Clamp01(value);
		}
	}

	public float Length
	{
		get
		{
			return this.length;
		}
		set
		{
			this.length = Mathf.Clamp01(value);
		}
	}

	public float Roughness
	{
		get
		{
			return this.roughness;
		}
		set
		{
			this.roughness = Mathf.Clamp01(value);
		}
	}

	public float Metallic
	{
		get
		{
			return this.metallic;
		}
		set
		{
			this.metallic = Mathf.Clamp01(value);
		}
	}

	public float Occlusion
	{
		get
		{
			return this.occlusion;
		}
		set
		{
			this.occlusion = Mathf.Clamp01(value);
		}
	}

	public Color Color
	{
		get
		{
			return this.color;
		}
		set
		{
			this.color = value;
		}
	}

	public Texture2D BrushPreview
	{
		get
		{
			this.UpdateBrushPreview();
			return this.brushPreview;
		}
		set
		{
			this.brushPreview = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateBrushPreview()
	{
		if (!this.isDirty)
		{
			return;
		}
		this.isDirty = false;
		if (this.brushPreview == null)
		{
			this.brushPreview = new Texture2D(100, 100);
		}
		Color[] array = new Color[this.brushPreview.width * this.brushPreview.height];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = Color.clear;
		}
		int num = Mathf.FloorToInt(this.size * 0.5f);
		for (int j = 0; j < 100; j++)
		{
			for (int k = 0; k < 100; k++)
			{
				Vector2 vector = new Vector2((float)j, (float)k) - new Vector2(50f, 50f);
				float num2 = Vector3.Dot(vector, vector);
				if (num2 <= (float)(num * num))
				{
					float num3 = Mathf.Pow(Mathf.Clamp01(1f - MathF.Sqrt(num2) / (float)num), this.falloff * 4f) * this.strength;
					array[k * this.brushPreview.width + j] = new Color(num3, num3, num3, 1f);
				}
				else
				{
					array[k * this.brushPreview.width + j] = Color.black;
				}
			}
		}
		this.brushPreview.SetPixels(array);
		this.brushPreview.Apply();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Texture2D brushPreview;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float size = 100f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float strength = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float falloff = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float matting = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float length = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float roughness = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float metallic = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float occlusion = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Color color = Color.white;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isDirty = true;
}
