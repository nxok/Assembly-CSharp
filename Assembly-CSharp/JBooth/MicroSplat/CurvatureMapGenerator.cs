using System;
using UnityEngine;

namespace JBooth.MicroSplat
{
	public class CurvatureMapGenerator
	{
		[PublicizedFrom(EAccessModifier.Private)]
		public static float Horizontal(float dx, float dy, float dxx, float dyy, float dxy)
		{
			float num = -2f * (dy * dy * dxx + dx * dx * dyy - dx * dy * dxy);
			num /= dx * dx + dy * dy;
			if (float.IsInfinity(num) || float.IsNaN(num))
			{
				num = 0f;
			}
			if (num < -CurvatureMapGenerator.m_limit)
			{
				num = -CurvatureMapGenerator.m_limit;
			}
			if (num > CurvatureMapGenerator.m_limit)
			{
				num = CurvatureMapGenerator.m_limit;
			}
			num /= CurvatureMapGenerator.m_limit;
			return num * 0.5f + 0.5f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static float Vertical(float dx, float dy, float dxx, float dyy, float dxy)
		{
			float num = -2f * (dx * dx * dxx + dy * dy * dyy + dx * dy * dxy);
			num /= dx * dx + dy * dy;
			if (float.IsInfinity(num) || float.IsNaN(num))
			{
				num = 0f;
			}
			if (num < -CurvatureMapGenerator.m_limit)
			{
				num = -CurvatureMapGenerator.m_limit;
			}
			if (num > CurvatureMapGenerator.m_limit)
			{
				num = CurvatureMapGenerator.m_limit;
			}
			num /= CurvatureMapGenerator.m_limit;
			return num * 0.5f + 0.5f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static float Average(float dx, float dy, float dxx, float dyy, float dxy)
		{
			float num = CurvatureMapGenerator.Horizontal(dx, dy, dxx, dyy, dxy);
			float num2 = CurvatureMapGenerator.Vertical(dx, dy, dxx, dyy, dxy);
			return (num + num2) * 0.5f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void UpdateWaterMap(float[,] waterMap, float[,,] outFlow, int width, int height)
		{
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					float num = outFlow[j, i, 0] + outFlow[j, i, 1] + outFlow[j, i, 2] + outFlow[j, i, 3];
					float num2 = 0f;
					num2 += ((j == 0) ? 0f : outFlow[j - 1, i, CurvatureMapGenerator.RIGHT]);
					num2 += ((j == width - 1) ? 0f : outFlow[j + 1, i, CurvatureMapGenerator.LEFT]);
					num2 += ((i == 0) ? 0f : outFlow[j, i - 1, CurvatureMapGenerator.TOP]);
					num2 += ((i == height - 1) ? 0f : outFlow[j, i + 1, CurvatureMapGenerator.BOTTOM]);
					float num3 = waterMap[j, i] + (num2 - num) * CurvatureMapGenerator.TIME;
					if (num3 < 0f)
					{
						num3 = 0f;
					}
					waterMap[j, i] = num3;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void CalculateVelocityField(float[,] velocityMap, float[,,] outFlow, int width, int height)
		{
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					float num = ((j == 0) ? 0f : (outFlow[j - 1, i, CurvatureMapGenerator.RIGHT] - outFlow[j, i, CurvatureMapGenerator.LEFT]));
					float num2 = ((j == width - 1) ? 0f : (outFlow[j, i, CurvatureMapGenerator.RIGHT] - outFlow[j + 1, i, CurvatureMapGenerator.LEFT]));
					float num3 = ((i == height - 1) ? 0f : (outFlow[j, i + 1, CurvatureMapGenerator.BOTTOM] - outFlow[j, i, CurvatureMapGenerator.TOP]));
					float num4 = ((i == 0) ? 0f : (outFlow[j, i, CurvatureMapGenerator.BOTTOM] - outFlow[j, i - 1, CurvatureMapGenerator.TOP]));
					float num5 = (num + num2) * 0.5f;
					float num6 = (num4 + num3) * 0.5f;
					velocityMap[j, i] = Mathf.Sqrt(num5 * num5 + num6 * num6);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void NormalizeMap(float[,] map, int width, int height)
		{
			float num = float.PositiveInfinity;
			float num2 = float.NegativeInfinity;
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					float num3 = map[j, i];
					if (num3 < num)
					{
						num = num3;
					}
					if (num3 > num2)
					{
						num2 = num3;
					}
				}
			}
			float num4 = num2 - num;
			for (int k = 0; k < height; k++)
			{
				for (int l = 0; l < width; l++)
				{
					float num5 = map[l, k];
					if (num4 < 1E-12f)
					{
						num5 = 0f;
					}
					else
					{
						num5 = (num5 - num) / num4;
					}
					map[l, k] = num5;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void FillWaterMap(float amount, float[,] waterMap, int width, int height)
		{
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					waterMap[j, i] = amount;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static void ComputeOutflow(float[,] waterMap, float[,,] outFlow, float[,] heightMap, int width, int height)
		{
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					int num = ((j == 0) ? 0 : (j - 1));
					int num2 = ((j == width - 1) ? (width - 1) : (j + 1));
					int num3 = ((i == 0) ? 0 : (i - 1));
					int num4 = ((i == height - 1) ? (height - 1) : (i + 1));
					float num5 = waterMap[j, i];
					float num6 = waterMap[num, i];
					float num7 = waterMap[num2, i];
					float num8 = waterMap[j, num3];
					float num9 = waterMap[j, num4];
					float num10 = heightMap[j, i];
					float num11 = heightMap[num, i];
					float num12 = heightMap[num2, i];
					float num13 = heightMap[j, num3];
					float num14 = heightMap[j, num4];
					float num15 = num5 + num10 - (num6 + num11);
					float num16 = num5 + num10 - (num7 + num12);
					float num17 = num5 + num10 - (num8 + num13);
					float num18 = num5 + num10 - (num9 + num14);
					float num19 = Mathf.Max(0f, outFlow[j, i, 0] + num15);
					float num20 = Mathf.Max(0f, outFlow[j, i, 1] + num16);
					float num21 = Mathf.Max(0f, outFlow[j, i, 2] + num17);
					float num22 = Mathf.Max(0f, outFlow[j, i, 3] + num18);
					float num23 = num19 + num20 + num21 + num22;
					if (num23 > 0f)
					{
						float num24 = num5 / (num23 * CurvatureMapGenerator.TIME);
						if (num24 > 1f)
						{
							num24 = 1f;
						}
						if (num24 < 0f)
						{
							num24 = 0f;
						}
						outFlow[j, i, 0] = num19 * num24;
						outFlow[j, i, 1] = num20 * num24;
						outFlow[j, i, 2] = num21 * num24;
						outFlow[j, i, 3] = num22 * num24;
					}
					else
					{
						outFlow[j, i, 0] = 0f;
						outFlow[j, i, 1] = 0f;
						outFlow[j, i, 2] = 0f;
						outFlow[j, i, 3] = 0f;
					}
				}
			}
		}

		public static void CreateMap(float[,] heights, Texture2D curveMap)
		{
			int width = curveMap.width;
			int height = curveMap.height;
			float num = 1f / ((float)width - 1f);
			float num2 = 1f / ((float)height - 1f);
			float[,] array = new float[width, height];
			float[,,] array2 = new float[width, height, 4];
			CurvatureMapGenerator.FillWaterMap(0.0001f, array, width, height);
			for (int i = 0; i < CurvatureMapGenerator.m_iterations; i++)
			{
				CurvatureMapGenerator.ComputeOutflow(array, array2, heights, width, height);
				CurvatureMapGenerator.UpdateWaterMap(array, array2, width, height);
			}
			float[,] array3 = new float[width, height];
			CurvatureMapGenerator.CalculateVelocityField(array3, array2, width, height);
			CurvatureMapGenerator.NormalizeMap(array3, width, height);
			for (int j = 0; j < height; j++)
			{
				for (int k = width - 1; k >= 0; k--)
				{
					int num3 = ((k == width - 1) ? k : (k + 1));
					int num4 = ((k == 0) ? k : (k - 1));
					int num5 = ((j == height - 1) ? j : (j + 1));
					int num6 = ((j == 0) ? j : (j - 1));
					float num7 = heights[k, j];
					float num8 = heights[num4, j];
					float num9 = heights[num3, j];
					float num10 = heights[k, num6];
					float num11 = heights[k, num5];
					float num12 = heights[num4, num6];
					float num13 = heights[num4, num5];
					float num14 = heights[num3, num6];
					float num15 = heights[num3, num5];
					float num16 = (num9 - num8) / (2f * num);
					float num17 = (num11 - num10) / (2f * num2);
					float num18 = (num9 - 2f * num7 + num8) / (num * num);
					float num19 = (num11 - 2f * num7 + num10) / (num2 * num2);
					float num20 = (num15 - num14 - num13 + num12) / (4f * num * num2);
					float num21 = CurvatureMapGenerator.Average(num16, num17, num18, num19, num20);
					float num22 = array3[k, j];
					curveMap.SetPixel(j, k, new Color(0f, num21, 0f, num22));
				}
			}
			curveMap.Apply();
		}

		public static float m_limit = 10000f;

		[PublicizedFrom(EAccessModifier.Private)]
		public static int LEFT = 0;

		[PublicizedFrom(EAccessModifier.Private)]
		public static int RIGHT = 1;

		[PublicizedFrom(EAccessModifier.Private)]
		public static int BOTTOM = 2;

		[PublicizedFrom(EAccessModifier.Private)]
		public static int TOP = 3;

		[PublicizedFrom(EAccessModifier.Private)]
		public static float TIME = 0.2f;

		public static int m_iterations = 8;
	}
}
