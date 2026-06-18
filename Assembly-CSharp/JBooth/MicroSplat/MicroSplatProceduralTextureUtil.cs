using System;
using UnityEngine;

namespace JBooth.MicroSplat
{
	public class MicroSplatProceduralTextureUtil
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static float PCFilter(int index, float height, float slope, float cavity, float flow, Vector3 worldPos, Vector2 uv, Color bMask, Color bMask2, out int texIndex, Vector3 pN, MicroSplatProceduralTextureConfig config, Texture2D procTexNoise, MicroSplatProceduralTextureUtil.NoiseUVMode noiseMode)
		{
			MicroSplatProceduralTextureConfig.Layer layer = config.layers[index];
			Color color;
			color..ctor(0f, 0f, 0f, 0f);
			if (noiseMode == MicroSplatProceduralTextureUtil.NoiseUVMode.Triplanar)
			{
				Vector2 vector = new Vector2(worldPos.z, worldPos.y) * 0.002f * layer.noiseFrequency + new Vector2(layer.noiseOffset, layer.noiseOffset);
				Vector2 vector2 = new Vector2(worldPos.x, worldPos.z) * 0.002f * layer.noiseFrequency + new Vector2(layer.noiseOffset + 0.31f, layer.noiseOffset + 0.31f);
				Vector2 vector3 = new Vector2(worldPos.x, worldPos.y) * 0.002f * layer.noiseFrequency + new Vector2(layer.noiseOffset + 0.71f, layer.noiseOffset + 0.71f);
				Color pixelBilinear = procTexNoise.GetPixelBilinear(vector.x, vector.y);
				Color pixelBilinear2 = procTexNoise.GetPixelBilinear(vector2.x, vector2.y);
				Color pixelBilinear3 = procTexNoise.GetPixelBilinear(vector3.x, vector3.y);
				color = pixelBilinear * pN.x + pixelBilinear2 * pN.y + pixelBilinear3 * pN.z;
			}
			else if (noiseMode == MicroSplatProceduralTextureUtil.NoiseUVMode.World)
			{
				color = procTexNoise.GetPixelBilinear(uv.x * layer.noiseFrequency + layer.noiseOffset, uv.y * layer.noiseFrequency + layer.noiseOffset);
			}
			else if (noiseMode == MicroSplatProceduralTextureUtil.NoiseUVMode.UV)
			{
				color *= procTexNoise.GetPixelBilinear(worldPos.x * 0.002f * layer.noiseFrequency + layer.noiseOffset, worldPos.z * 0.002f * layer.noiseFrequency + layer.noiseOffset);
			}
			color.r = color.r * 2f - 1f;
			color.g = color.g * 2f - 1f;
			float num = layer.heightCurve.Evaluate(height);
			float num2 = layer.slopeCurve.Evaluate(slope);
			float num3 = layer.cavityMapCurve.Evaluate(cavity);
			float num4 = layer.erosionMapCurve.Evaluate(flow);
			num *= 1f + Mathf.Lerp(layer.noiseRange.x, layer.noiseRange.y, color.r);
			num2 *= 1f + Mathf.Lerp(layer.noiseRange.x, layer.noiseRange.y, color.g);
			num3 *= 1f + Mathf.Lerp(layer.noiseRange.x, layer.noiseRange.y, color.b);
			num4 *= 1f + Mathf.Lerp(layer.noiseRange.x, layer.noiseRange.y, color.a);
			if (!layer.heightActive)
			{
				num = 1f;
			}
			if (!layer.slopeActive)
			{
				num2 = 1f;
			}
			if (!layer.cavityMapActive)
			{
				num3 = 1f;
			}
			if (!layer.erosionMapActive)
			{
				num4 = 1f;
			}
			bMask *= layer.biomeWeights;
			bMask2 *= layer.biomeWeights2;
			float num5 = Mathf.Max(Mathf.Max(Mathf.Max(bMask.r, bMask.g), bMask.b), bMask.a);
			float num6 = Mathf.Max(Mathf.Max(Mathf.Max(bMask2.r, bMask2.g), bMask2.b), bMask2.a);
			texIndex = layer.textureIndex;
			return Mathf.Clamp01(num * num2 * num3 * num4 * layer.weight * num5 * num6);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void PCProcessLayer(ref Vector4 weights, ref MicroSplatProceduralTextureUtil.Int4 indexes, ref float totalWeight, int curIdx, float height, float slope, float cavity, float flow, Vector3 worldPos, Vector2 uv, Color biomeMask, Color biomeMask2, Vector3 pN, MicroSplatProceduralTextureConfig config, Texture2D noiseMap, MicroSplatProceduralTextureUtil.NoiseUVMode noiseMode)
		{
			int num = 0;
			float num2 = MicroSplatProceduralTextureUtil.PCFilter(curIdx, height, slope, cavity, flow, worldPos, uv, biomeMask, biomeMask2, out num, pN, config, noiseMap, noiseMode);
			num2 = Mathf.Min(totalWeight, num2);
			totalWeight -= num2;
			if (num2 > weights.x)
			{
				weights.w = weights.z;
				weights.z = weights.y;
				weights.y = weights.x;
				indexes.w = indexes.z;
				indexes.z = indexes.y;
				indexes.y = indexes.x;
				weights.x = num2;
				indexes.x = num;
				return;
			}
			if (num2 > weights.y)
			{
				weights.w = weights.z;
				weights.z = weights.y;
				indexes.w = indexes.z;
				indexes.z = indexes.y;
				weights.y = num2;
				indexes.y = num;
				return;
			}
			if (num2 > weights.z)
			{
				weights.w = weights.z;
				indexes.w = indexes.z;
				weights.z = num2;
				indexes.z = num;
				return;
			}
			if (num2 > weights.w)
			{
				weights.w = num2;
				indexes.w = num;
			}
		}

		public static void Sample(Vector2 uv, Vector3 worldPos, Vector3 worldNormal, Vector3 up, MicroSplatProceduralTextureUtil.NoiseUVMode noiseUVMode, Material mat, MicroSplatProceduralTextureConfig config, out Vector4 weights, out MicroSplatProceduralTextureUtil.Int4 indexes)
		{
			weights = new Vector4(0f, 0f, 0f, 0f);
			int count = config.layers.Count;
			Vector2 vector = mat.GetVector("_WorldHeightRange");
			float num = Mathf.Clamp01((worldPos.y - vector.x) / Mathf.Max(0.1f, vector.y - vector.x));
			float num2 = 1f - Mathf.Clamp01(Vector3.Dot(worldNormal, up) * 0.5f + 0.49f);
			float num3 = 0.5f;
			float num4 = 0.5f;
			Texture2D texture2D = (mat.HasProperty("_CavityMap") ? ((Texture2D)mat.GetTexture("_CavityMap")) : null);
			if (texture2D != null)
			{
				Color pixelBilinear = texture2D.GetPixelBilinear(uv.x, uv.y);
				num3 = pixelBilinear.g;
				num4 = pixelBilinear.a;
			}
			indexes = default(MicroSplatProceduralTextureUtil.Int4);
			indexes.x = 0;
			indexes.y = 1;
			indexes.z = 2;
			indexes.w = 3;
			float num5 = 1f;
			Texture2D texture2D2 = (mat.HasProperty("_ProcTexBiomeMask") ? ((Texture2D)mat.GetTexture("_ProcTexBiomeMask")) : null);
			Object @object = (mat.HasProperty("_ProcTexBiomeMask2") ? ((Texture2D)mat.GetTexture("_ProcTexBiomeMask2")) : null);
			Color pixelBilinear2;
			pixelBilinear2..ctor(1f, 1f, 1f, 1f);
			Color pixelBilinear3;
			pixelBilinear3..ctor(1f, 1f, 1f, 1f);
			if (texture2D2 != null)
			{
				pixelBilinear2 = texture2D2.GetPixelBilinear(uv.x, uv.y);
			}
			if (@object != null)
			{
				pixelBilinear3 = texture2D2.GetPixelBilinear(uv.x, uv.y);
			}
			Vector3 vector2;
			vector2..ctor(0f, 0f, 0f);
			if (noiseUVMode == MicroSplatProceduralTextureUtil.NoiseUVMode.Triplanar)
			{
				Vector3 vector3 = worldNormal;
				vector3.x = Mathf.Abs(vector3.x);
				vector3.y = Mathf.Abs(vector3.y);
				vector3.z = Mathf.Abs(vector3.z);
				vector2.x = Mathf.Pow(vector3.x, 4f);
				vector2.y = Mathf.Pow(vector3.y, 4f);
				vector2.z = Mathf.Pow(vector3.z, 4f);
				float num6 = vector2.x + vector2.y + vector2.z;
				vector2.x /= num6;
				vector2.y /= num6;
				vector2.z /= num6;
			}
			Texture2D texture2D3 = (mat.HasProperty("_ProcTexNoise") ? ((Texture2D)mat.GetTexture("_ProcTexNoise")) : null);
			for (int i = 0; i < count; i++)
			{
				MicroSplatProceduralTextureUtil.PCProcessLayer(ref weights, ref indexes, ref num5, i, num, num2, num3, num4, worldPos, uv, pixelBilinear2, pixelBilinear3, vector2, config, texture2D3, noiseUVMode);
				if (num5 <= 0f)
				{
					break;
				}
			}
		}

		public enum NoiseUVMode
		{
			UV,
			World,
			Triplanar
		}

		public struct Int4
		{
			public int x;

			public int y;

			public int z;

			public int w;
		}
	}
}
