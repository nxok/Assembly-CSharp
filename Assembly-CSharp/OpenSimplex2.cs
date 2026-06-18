using System;
using System.Runtime.CompilerServices;

public static class OpenSimplex2
{
	public static float Noise2(long seed, double x, double y)
	{
		double num = 0.366025403784439 * (x + y);
		double num2 = x + num;
		double num3 = y + num;
		return OpenSimplex2.Noise2_UnskewedBase(seed, num2, num3);
	}

	public static float Noise2_ImproveX(long seed, double x, double y)
	{
		double num = x * 0.7071067811865476;
		double num2 = y * 1.2247448713915896;
		return OpenSimplex2.Noise2_UnskewedBase(seed, num2 + num, num2 - num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float Noise2_UnskewedBase(long seed, double xs, double ys)
	{
		int num = OpenSimplex2.FastFloor(xs);
		int num2 = OpenSimplex2.FastFloor(ys);
		float num3 = (float)(xs - (double)num);
		float num4 = (float)(ys - (double)num2);
		long num5 = (long)num * 5910200641878280303L;
		long num6 = (long)num2 * 6452764530575939509L;
		float num7 = (num3 + num4) * -0.21132487f;
		float num8 = num3 + num7;
		float num9 = num4 + num7;
		float num10 = 0f;
		float num11 = 0.5f - num8 * num8 - num9 * num9;
		if (num11 > 0f)
		{
			num10 = num11 * num11 * (num11 * num11) * OpenSimplex2.Grad(seed, num5, num6, num8, num9);
		}
		float num12 = -3.1547005f * num7 + (-0.6666667f + num11);
		if (num12 > 0f)
		{
			float num13 = num8 - 0.57735026f;
			float num14 = num9 - 0.57735026f;
			num10 += num12 * num12 * (num12 * num12) * OpenSimplex2.Grad(seed, num5 + 5910200641878280303L, num6 + 6452764530575939509L, num13, num14);
		}
		if (num9 > num8)
		{
			float num15 = num8 - -0.21132487f;
			float num16 = num9 - 0.7886751f;
			float num17 = 0.5f - num15 * num15 - num16 * num16;
			if (num17 > 0f)
			{
				num10 += num17 * num17 * (num17 * num17) * OpenSimplex2.Grad(seed, num5, num6 + 6452764530575939509L, num15, num16);
			}
		}
		else
		{
			float num18 = num8 - 0.7886751f;
			float num19 = num9 - -0.21132487f;
			float num20 = 0.5f - num18 * num18 - num19 * num19;
			if (num20 > 0f)
			{
				num10 += num20 * num20 * (num20 * num20) * OpenSimplex2.Grad(seed, num5 + 5910200641878280303L, num6, num18, num19);
			}
		}
		return num10;
	}

	public static float Noise3_ImproveXY(long seed, double x, double y, double z)
	{
		double num = x + y;
		double num2 = num * -0.21132486540518713;
		double num3 = z * 0.577350269189626;
		double num4 = x + num2 + num3;
		double num5 = y + num2 + num3;
		double num6 = num * -0.577350269189626 + num3;
		return OpenSimplex2.Noise3_UnrotatedBase(seed, num4, num5, num6);
	}

	public static float Noise3_ImproveXZ(long seed, double x, double y, double z)
	{
		double num = x + z;
		double num2 = num * -0.21132486540518713;
		double num3 = y * 0.577350269189626;
		double num4 = x + num2 + num3;
		double num5 = z + num2 + num3;
		double num6 = num * -0.577350269189626 + num3;
		return OpenSimplex2.Noise3_UnrotatedBase(seed, num4, num6, num5);
	}

	public static float Noise3_Fallback(long seed, double x, double y, double z)
	{
		double num = 0.6666666666666666 * (x + y + z);
		double num2 = num - x;
		double num3 = num - y;
		double num4 = num - z;
		return OpenSimplex2.Noise3_UnrotatedBase(seed, num2, num3, num4);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float Noise3_UnrotatedBase(long seed, double xr, double yr, double zr)
	{
		int num = OpenSimplex2.FastRound(xr);
		int num2 = OpenSimplex2.FastRound(yr);
		int num3 = OpenSimplex2.FastRound(zr);
		float num4 = (float)(xr - (double)num);
		float num5 = (float)(yr - (double)num2);
		float num6 = (float)(zr - (double)num3);
		int num7 = (int)(-1f - num4) | 1;
		int num8 = (int)(-1f - num5) | 1;
		int num9 = (int)(-1f - num6) | 1;
		float num10 = (float)num7 * -num4;
		float num11 = (float)num8 * -num5;
		float num12 = (float)num9 * -num6;
		long num13 = (long)num * 5910200641878280303L;
		long num14 = (long)num2 * 6452764530575939509L;
		long num15 = (long)num3 * 6614699811220273867L;
		float num16 = 0f;
		float num17 = 0.6f - num4 * num4 - (num5 * num5 + num6 * num6);
		int num18 = 0;
		for (;;)
		{
			if (num17 > 0f)
			{
				num16 += num17 * num17 * (num17 * num17) * OpenSimplex2.Grad(seed, num13, num14, num15, num4, num5, num6);
			}
			if (num10 >= num11 && num10 >= num12)
			{
				float num19 = num17 + num10 + num10;
				if (num19 > 1f)
				{
					num19 -= 1f;
					num16 += num19 * num19 * (num19 * num19) * OpenSimplex2.Grad(seed, num13 - (long)num7 * 5910200641878280303L, num14, num15, num4 + (float)num7, num5, num6);
				}
			}
			else if (num11 > num10 && num11 >= num12)
			{
				float num20 = num17 + num11 + num11;
				if (num20 > 1f)
				{
					num20 -= 1f;
					num16 += num20 * num20 * (num20 * num20) * OpenSimplex2.Grad(seed, num13, num14 - (long)num8 * 6452764530575939509L, num15, num4, num5 + (float)num8, num6);
				}
			}
			else
			{
				float num21 = num17 + num12 + num12;
				if (num21 > 1f)
				{
					num21 -= 1f;
					num16 += num21 * num21 * (num21 * num21) * OpenSimplex2.Grad(seed, num13, num14, num15 - (long)num9 * 6614699811220273867L, num4, num5, num6 + (float)num9);
				}
			}
			if (num18 == 1)
			{
				break;
			}
			num10 = 0.5f - num10;
			num11 = 0.5f - num11;
			num12 = 0.5f - num12;
			num4 = (float)num7 * num10;
			num5 = (float)num8 * num11;
			num6 = (float)num9 * num12;
			num17 += 0.75f - num10 - (num11 + num12);
			num13 += (long)(num7 >> 1) & 5910200641878280303L;
			num14 += (long)(num8 >> 1) & 6452764530575939509L;
			num15 += (long)(num9 >> 1) & 6614699811220273867L;
			num7 = -num7;
			num8 = -num8;
			num9 = -num9;
			seed ^= -5968755714895566377L;
			num18++;
		}
		return num16;
	}

	public static float Noise4_ImproveXYZ_ImproveXY(long seed, double x, double y, double z, double w)
	{
		double num = x + y;
		double num2 = num * -0.211324865405187;
		double num3 = z * 0.2886751345948129;
		double num4 = w * 0.2236067977499788;
		double num5 = x + (num3 + num4 + num2);
		double num6 = y + (num3 + num4 + num2);
		double num7 = num * -0.577350269189626 + (num3 + num4);
		double num8 = z * -0.866025403784439 + num4;
		return OpenSimplex2.Noise4_UnskewedBase(seed, num5, num6, num7, num8);
	}

	public static float Noise4_ImproveXYZ_ImproveXZ(long seed, double x, double y, double z, double w)
	{
		double num = x + z;
		double num2 = num * -0.211324865405187;
		double num3 = y * 0.2886751345948129;
		double num4 = w * 0.2236067977499788;
		double num5 = x + (num3 + num4 + num2);
		double num6 = z + (num3 + num4 + num2);
		double num7 = num * -0.577350269189626 + (num3 + num4);
		double num8 = y * -0.866025403784439 + num4;
		return OpenSimplex2.Noise4_UnskewedBase(seed, num5, num7, num6, num8);
	}

	public static float Noise4_ImproveXYZ(long seed, double x, double y, double z, double w)
	{
		double num = x + y + z;
		double num2 = w * 0.2236067977499788;
		double num3 = num * -0.16666666666666666 + num2;
		double num4 = x + num3;
		double num5 = y + num3;
		double num6 = z + num3;
		double num7 = -0.5 * num + num2;
		return OpenSimplex2.Noise4_UnskewedBase(seed, num4, num5, num6, num7);
	}

	public static float Noise4_Fallback(long seed, double x, double y, double z, double w)
	{
		double num = -0.13819660246372223 * (x + y + z + w);
		double num2 = x + num;
		double num3 = y + num;
		double num4 = z + num;
		double num5 = w + num;
		return OpenSimplex2.Noise4_UnskewedBase(seed, num2, num3, num4, num5);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static float Noise4_UnskewedBase(long seed, double xs, double ys, double zs, double ws)
	{
		int num = OpenSimplex2.FastFloor(xs);
		int num2 = OpenSimplex2.FastFloor(ys);
		int num3 = OpenSimplex2.FastFloor(zs);
		int num4 = OpenSimplex2.FastFloor(ws);
		float num5 = (float)(xs - (double)num);
		float num6 = (float)(ys - (double)num2);
		float num7 = (float)(zs - (double)num3);
		float num8 = (float)(ws - (double)num4);
		float num9 = num5 + num6 + (num7 + num8);
		int num10 = (int)((double)num9 * 1.25);
		seed += (long)num10 * 1045921697555224141L;
		float num11 = (float)num10 * -0.2f;
		num5 += num11;
		num6 += num11;
		num7 += num11;
		num8 += num11;
		float num12 = (num9 + num11 * 4f) * 0.309017f;
		long num13 = (long)num * 5910200641878280303L;
		long num14 = (long)num2 * 6452764530575939509L;
		long num15 = (long)num3 * 6614699811220273867L;
		long num16 = (long)num4 * 6254464313819354443L;
		float num17 = 0f;
		int num18 = 0;
		for (;;)
		{
			double num19 = 1.0 + (double)num12 * -3.2360678915486614;
			if (num5 >= num6 && num5 >= num7 && num5 >= num8 && (double)num5 >= num19)
			{
				num13 += 5910200641878280303L;
				num5 -= 1f;
				num12 -= 0.309017f;
			}
			else if (num6 > num5 && num6 >= num7 && num6 >= num8 && (double)num6 >= num19)
			{
				num14 += 6452764530575939509L;
				num6 -= 1f;
				num12 -= 0.309017f;
			}
			else if (num7 > num5 && num7 > num6 && num7 >= num8 && (double)num7 >= num19)
			{
				num15 += 6614699811220273867L;
				num7 -= 1f;
				num12 -= 0.309017f;
			}
			else if (num8 > num5 && num8 > num6 && num8 > num7 && (double)num8 >= num19)
			{
				num16 += 6254464313819354443L;
				num8 -= 1f;
				num12 -= 0.309017f;
			}
			float num20 = num5 + num12;
			float num21 = num6 + num12;
			float num22 = num7 + num12;
			float num23 = num8 + num12;
			float num24 = num20 * num20 + num21 * num21 + (num22 * num22 + num23 * num23);
			if (num24 < 0.6f)
			{
				num24 -= 0.6f;
				num24 *= num24;
				num17 += num24 * num24 * OpenSimplex2.Grad(seed, num13, num14, num15, num16, num20, num21, num22, num23);
			}
			if (num18 == 4)
			{
				break;
			}
			num5 += 0.2f;
			num6 += 0.2f;
			num7 += 0.2f;
			num8 += 0.2f;
			num12 += 0.2472136f;
			seed -= 1045921697555224141L;
			if (num18 == num10)
			{
				num13 -= 5910200641878280303L;
				num14 -= 6452764530575939509L;
				num15 -= 6614699811220273867L;
				num16 -= 6254464313819354443L;
				seed += 5229608487776120705L;
			}
			num18++;
		}
		return num17;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Grad(long seed, long xsvp, long ysvp, float dx, float dy)
	{
		long num = (seed ^ xsvp ^ ysvp) * 6026932503003350773L;
		int num2 = (int)(num ^ (num >> 58)) & 254;
		return OpenSimplex2.GRADIENTS_2D[num2 | 0] * dx + OpenSimplex2.GRADIENTS_2D[num2 | 1] * dy;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Grad(long seed, long xrvp, long yrvp, long zrvp, float dx, float dy, float dz)
	{
		long num = (seed ^ xrvp ^ (yrvp ^ zrvp)) * 6026932503003350773L;
		int num2 = (int)(num ^ (num >> 58)) & 1020;
		return OpenSimplex2.GRADIENTS_3D[num2 | 0] * dx + OpenSimplex2.GRADIENTS_3D[num2 | 1] * dy + OpenSimplex2.GRADIENTS_3D[num2 | 2] * dz;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Grad(long seed, long xsvp, long ysvp, long zsvp, long wsvp, float dx, float dy, float dz, float dw)
	{
		long num = (seed ^ (xsvp ^ ysvp) ^ (zsvp ^ wsvp)) * 6026932503003350773L;
		int num2 = (int)(num ^ (num >> 57)) & 2044;
		return OpenSimplex2.GRADIENTS_4D[num2 | 0] * dx + OpenSimplex2.GRADIENTS_4D[num2 | 1] * dy + (OpenSimplex2.GRADIENTS_4D[num2 | 2] * dz + OpenSimplex2.GRADIENTS_4D[num2 | 3] * dw);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastFloor(double x)
	{
		int num = (int)x;
		if (x >= (double)num)
		{
			return num;
		}
		return num - 1;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FastRound(double x)
	{
		if (x >= 0.0)
		{
			return (int)(x + 0.5);
		}
		return (int)(x - 0.5);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	static OpenSimplex2()
	{
		float[] array = new float[]
		{
			0.38268343f, 0.9238795f, 0.9238795f, 0.38268343f, 0.9238795f, -0.38268343f, 0.38268343f, -0.9238795f, -0.38268343f, -0.9238795f,
			-0.9238795f, -0.38268343f, -0.9238795f, 0.38268343f, -0.38268343f, 0.9238795f, 0.13052619f, 0.9914449f, 0.6087614f, 0.7933533f,
			0.7933533f, 0.6087614f, 0.9914449f, 0.13052619f, 0.9914449f, -0.13052619f, 0.7933533f, -0.6087614f, 0.6087614f, -0.7933533f,
			0.13052619f, -0.9914449f, -0.13052619f, -0.9914449f, -0.6087614f, -0.7933533f, -0.7933533f, -0.6087614f, -0.9914449f, -0.13052619f,
			-0.9914449f, 0.13052619f, -0.7933533f, 0.6087614f, -0.6087614f, 0.7933533f, -0.13052619f, 0.9914449f
		};
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = (float)((double)array[i] / 0.01001634121365712);
		}
		int j = 0;
		int num = 0;
		while (j < OpenSimplex2.GRADIENTS_2D.Length)
		{
			if (num == array.Length)
			{
				num = 0;
			}
			OpenSimplex2.GRADIENTS_2D[j] = array[num];
			j++;
			num++;
		}
		OpenSimplex2.GRADIENTS_3D = new float[1024];
		float[] array2 = new float[]
		{
			2.2247448f, 2.2247448f, -1f, 0f, 2.2247448f, 2.2247448f, 1f, 0f, 3.0862665f, 1.1721513f,
			0f, 0f, 1.1721513f, 3.0862665f, 0f, 0f, -2.2247448f, 2.2247448f, -1f, 0f,
			-2.2247448f, 2.2247448f, 1f, 0f, -1.1721513f, 3.0862665f, 0f, 0f, -3.0862665f, 1.1721513f,
			0f, 0f, -1f, -2.2247448f, -2.2247448f, 0f, 1f, -2.2247448f, -2.2247448f, 0f,
			0f, -3.0862665f, -1.1721513f, 0f, 0f, -1.1721513f, -3.0862665f, 0f, -1f, -2.2247448f,
			2.2247448f, 0f, 1f, -2.2247448f, 2.2247448f, 0f, 0f, -1.1721513f, 3.0862665f, 0f,
			0f, -3.0862665f, 1.1721513f, 0f, -2.2247448f, -2.2247448f, -1f, 0f, -2.2247448f, -2.2247448f,
			1f, 0f, -3.0862665f, -1.1721513f, 0f, 0f, -1.1721513f, -3.0862665f, 0f, 0f,
			-2.2247448f, -1f, -2.2247448f, 0f, -2.2247448f, 1f, -2.2247448f, 0f, -1.1721513f, 0f,
			-3.0862665f, 0f, -3.0862665f, 0f, -1.1721513f, 0f, -2.2247448f, -1f, 2.2247448f, 0f,
			-2.2247448f, 1f, 2.2247448f, 0f, -3.0862665f, 0f, 1.1721513f, 0f, -1.1721513f, 0f,
			3.0862665f, 0f, -1f, 2.2247448f, -2.2247448f, 0f, 1f, 2.2247448f, -2.2247448f, 0f,
			0f, 1.1721513f, -3.0862665f, 0f, 0f, 3.0862665f, -1.1721513f, 0f, -1f, 2.2247448f,
			2.2247448f, 0f, 1f, 2.2247448f, 2.2247448f, 0f, 0f, 3.0862665f, 1.1721513f, 0f,
			0f, 1.1721513f, 3.0862665f, 0f, 2.2247448f, -2.2247448f, -1f, 0f, 2.2247448f, -2.2247448f,
			1f, 0f, 1.1721513f, -3.0862665f, 0f, 0f, 3.0862665f, -1.1721513f, 0f, 0f,
			2.2247448f, -1f, -2.2247448f, 0f, 2.2247448f, 1f, -2.2247448f, 0f, 3.0862665f, 0f,
			-1.1721513f, 0f, 1.1721513f, 0f, -3.0862665f, 0f, 2.2247448f, -1f, 2.2247448f, 0f,
			2.2247448f, 1f, 2.2247448f, 0f, 1.1721513f, 0f, 3.0862665f, 0f, 3.0862665f, 0f,
			1.1721513f, 0f
		};
		for (int k = 0; k < array2.Length; k++)
		{
			array2[k] = (float)((double)array2[k] / 0.07969837668935331);
		}
		int l = 0;
		int num2 = 0;
		while (l < OpenSimplex2.GRADIENTS_3D.Length)
		{
			if (num2 == array2.Length)
			{
				num2 = 0;
			}
			OpenSimplex2.GRADIENTS_3D[l] = array2[num2];
			l++;
			num2++;
		}
		OpenSimplex2.GRADIENTS_4D = new float[2048];
		float[] array3 = new float[]
		{
			-0.6740059f, -0.32398477f, -0.32398477f, 0.5794685f, -0.7504884f, -0.40046722f, 0.15296486f, 0.502986f, -0.7504884f, 0.15296486f,
			-0.40046722f, 0.502986f, -0.8828162f, 0.08164729f, 0.08164729f, 0.4553054f, -0.4553054f, -0.08164729f, -0.08164729f, 0.8828162f,
			-0.502986f, -0.15296486f, 0.40046722f, 0.7504884f, -0.502986f, 0.40046722f, -0.15296486f, 0.7504884f, -0.5794685f, 0.32398477f,
			0.32398477f, 0.6740059f, -0.6740059f, -0.32398477f, 0.5794685f, -0.32398477f, -0.7504884f, -0.40046722f, 0.502986f, 0.15296486f,
			-0.7504884f, 0.15296486f, 0.502986f, -0.40046722f, -0.8828162f, 0.08164729f, 0.4553054f, 0.08164729f, -0.4553054f, -0.08164729f,
			0.8828162f, -0.08164729f, -0.502986f, -0.15296486f, 0.7504884f, 0.40046722f, -0.502986f, 0.40046722f, 0.7504884f, -0.15296486f,
			-0.5794685f, 0.32398477f, 0.6740059f, 0.32398477f, -0.6740059f, 0.5794685f, -0.32398477f, -0.32398477f, -0.7504884f, 0.502986f,
			-0.40046722f, 0.15296486f, -0.7504884f, 0.502986f, 0.15296486f, -0.40046722f, -0.8828162f, 0.4553054f, 0.08164729f, 0.08164729f,
			-0.4553054f, 0.8828162f, -0.08164729f, -0.08164729f, -0.502986f, 0.7504884f, -0.15296486f, 0.40046722f, -0.502986f, 0.7504884f,
			0.40046722f, -0.15296486f, -0.5794685f, 0.6740059f, 0.32398477f, 0.32398477f, 0.5794685f, -0.6740059f, -0.32398477f, -0.32398477f,
			0.502986f, -0.7504884f, -0.40046722f, 0.15296486f, 0.502986f, -0.7504884f, 0.15296486f, -0.40046722f, 0.4553054f, -0.8828162f,
			0.08164729f, 0.08164729f, 0.8828162f, -0.4553054f, -0.08164729f, -0.08164729f, 0.7504884f, -0.502986f, -0.15296486f, 0.40046722f,
			0.7504884f, -0.502986f, 0.40046722f, -0.15296486f, 0.6740059f, -0.5794685f, 0.32398477f, 0.32398477f, -0.753341f, -0.3796829f,
			-0.3796829f, -0.3796829f, -0.78216845f, -0.43214726f, -0.43214726f, 0.121284805f, -0.78216845f, -0.43214726f, 0.121284805f, -0.43214726f,
			-0.78216845f, 0.121284805f, -0.43214726f, -0.43214726f, -0.85865086f, -0.5086297f, 0.04480237f, 0.04480237f, -0.85865086f, 0.04480237f,
			-0.5086297f, 0.04480237f, -0.85865086f, 0.04480237f, 0.04480237f, -0.5086297f, -0.9982829f, -0.033819415f, -0.033819415f, -0.033819415f,
			-0.3796829f, -0.753341f, -0.3796829f, -0.3796829f, -0.43214726f, -0.78216845f, -0.43214726f, 0.121284805f, -0.43214726f, -0.78216845f,
			0.121284805f, -0.43214726f, 0.121284805f, -0.78216845f, -0.43214726f, -0.43214726f, -0.5086297f, -0.85865086f, 0.04480237f, 0.04480237f,
			0.04480237f, -0.85865086f, -0.5086297f, 0.04480237f, 0.04480237f, -0.85865086f, 0.04480237f, -0.5086297f, -0.033819415f, -0.9982829f,
			-0.033819415f, -0.033819415f, -0.3796829f, -0.3796829f, -0.753341f, -0.3796829f, -0.43214726f, -0.43214726f, -0.78216845f, 0.121284805f,
			-0.43214726f, 0.121284805f, -0.78216845f, -0.43214726f, 0.121284805f, -0.43214726f, -0.78216845f, -0.43214726f, -0.5086297f, 0.04480237f,
			-0.85865086f, 0.04480237f, 0.04480237f, -0.5086297f, -0.85865086f, 0.04480237f, 0.04480237f, 0.04480237f, -0.85865086f, -0.5086297f,
			-0.033819415f, -0.033819415f, -0.9982829f, -0.033819415f, -0.3796829f, -0.3796829f, -0.3796829f, -0.753341f, -0.43214726f, -0.43214726f,
			0.121284805f, -0.78216845f, -0.43214726f, 0.121284805f, -0.43214726f, -0.78216845f, 0.121284805f, -0.43214726f, -0.43214726f, -0.78216845f,
			-0.5086297f, 0.04480237f, 0.04480237f, -0.85865086f, 0.04480237f, -0.5086297f, 0.04480237f, -0.85865086f, 0.04480237f, 0.04480237f,
			-0.5086297f, -0.85865086f, -0.033819415f, -0.033819415f, -0.033819415f, -0.9982829f, -0.32398477f, -0.6740059f, -0.32398477f, 0.5794685f,
			-0.40046722f, -0.7504884f, 0.15296486f, 0.502986f, 0.15296486f, -0.7504884f, -0.40046722f, 0.502986f, 0.08164729f, -0.8828162f,
			0.08164729f, 0.4553054f, -0.08164729f, -0.4553054f, -0.08164729f, 0.8828162f, -0.15296486f, -0.502986f, 0.40046722f, 0.7504884f,
			0.40046722f, -0.502986f, -0.15296486f, 0.7504884f, 0.32398477f, -0.5794685f, 0.32398477f, 0.6740059f, -0.32398477f, -0.32398477f,
			-0.6740059f, 0.5794685f, -0.40046722f, 0.15296486f, -0.7504884f, 0.502986f, 0.15296486f, -0.40046722f, -0.7504884f, 0.502986f,
			0.08164729f, 0.08164729f, -0.8828162f, 0.4553054f, -0.08164729f, -0.08164729f, -0.4553054f, 0.8828162f, -0.15296486f, 0.40046722f,
			-0.502986f, 0.7504884f, 0.40046722f, -0.15296486f, -0.502986f, 0.7504884f, 0.32398477f, 0.32398477f, -0.5794685f, 0.6740059f,
			-0.32398477f, -0.6740059f, 0.5794685f, -0.32398477f, -0.40046722f, -0.7504884f, 0.502986f, 0.15296486f, 0.15296486f, -0.7504884f,
			0.502986f, -0.40046722f, 0.08164729f, -0.8828162f, 0.4553054f, 0.08164729f, -0.08164729f, -0.4553054f, 0.8828162f, -0.08164729f,
			-0.15296486f, -0.502986f, 0.7504884f, 0.40046722f, 0.40046722f, -0.502986f, 0.7504884f, -0.15296486f, 0.32398477f, -0.5794685f,
			0.6740059f, 0.32398477f, -0.32398477f, -0.32398477f, 0.5794685f, -0.6740059f, -0.40046722f, 0.15296486f, 0.502986f, -0.7504884f,
			0.15296486f, -0.40046722f, 0.502986f, -0.7504884f, 0.08164729f, 0.08164729f, 0.4553054f, -0.8828162f, -0.08164729f, -0.08164729f,
			0.8828162f, -0.4553054f, -0.15296486f, 0.40046722f, 0.7504884f, -0.502986f, 0.40046722f, -0.15296486f, 0.7504884f, -0.502986f,
			0.32398477f, 0.32398477f, 0.6740059f, -0.5794685f, -0.32398477f, 0.5794685f, -0.6740059f, -0.32398477f, -0.40046722f, 0.502986f,
			-0.7504884f, 0.15296486f, 0.15296486f, 0.502986f, -0.7504884f, -0.40046722f, 0.08164729f, 0.4553054f, -0.8828162f, 0.08164729f,
			-0.08164729f, 0.8828162f, -0.4553054f, -0.08164729f, -0.15296486f, 0.7504884f, -0.502986f, 0.40046722f, 0.40046722f, 0.7504884f,
			-0.502986f, -0.15296486f, 0.32398477f, 0.6740059f, -0.5794685f, 0.32398477f, -0.32398477f, 0.5794685f, -0.32398477f, -0.6740059f,
			-0.40046722f, 0.502986f, 0.15296486f, -0.7504884f, 0.15296486f, 0.502986f, -0.40046722f, -0.7504884f, 0.08164729f, 0.4553054f,
			0.08164729f, -0.8828162f, -0.08164729f, 0.8828162f, -0.08164729f, -0.4553054f, -0.15296486f, 0.7504884f, 0.40046722f, -0.502986f,
			0.40046722f, 0.7504884f, -0.15296486f, -0.502986f, 0.32398477f, 0.6740059f, 0.32398477f, -0.5794685f, 0.5794685f, -0.32398477f,
			-0.6740059f, -0.32398477f, 0.502986f, -0.40046722f, -0.7504884f, 0.15296486f, 0.502986f, 0.15296486f, -0.7504884f, -0.40046722f,
			0.4553054f, 0.08164729f, -0.8828162f, 0.08164729f, 0.8828162f, -0.08164729f, -0.4553054f, -0.08164729f, 0.7504884f, -0.15296486f,
			-0.502986f, 0.40046722f, 0.7504884f, 0.40046722f, -0.502986f, -0.15296486f, 0.6740059f, 0.32398477f, -0.5794685f, 0.32398477f,
			0.5794685f, -0.32398477f, -0.32398477f, -0.6740059f, 0.502986f, -0.40046722f, 0.15296486f, -0.7504884f, 0.502986f, 0.15296486f,
			-0.40046722f, -0.7504884f, 0.4553054f, 0.08164729f, 0.08164729f, -0.8828162f, 0.8828162f, -0.08164729f, -0.08164729f, -0.4553054f,
			0.7504884f, -0.15296486f, 0.40046722f, -0.502986f, 0.7504884f, 0.40046722f, -0.15296486f, -0.502986f, 0.6740059f, 0.32398477f,
			0.32398477f, -0.5794685f, 0.033819415f, 0.033819415f, 0.033819415f, 0.9982829f, -0.04480237f, -0.04480237f, 0.5086297f, 0.85865086f,
			-0.04480237f, 0.5086297f, -0.04480237f, 0.85865086f, -0.121284805f, 0.43214726f, 0.43214726f, 0.78216845f, 0.5086297f, -0.04480237f,
			-0.04480237f, 0.85865086f, 0.43214726f, -0.121284805f, 0.43214726f, 0.78216845f, 0.43214726f, 0.43214726f, -0.121284805f, 0.78216845f,
			0.3796829f, 0.3796829f, 0.3796829f, 0.753341f, 0.033819415f, 0.033819415f, 0.9982829f, 0.033819415f, -0.04480237f, 0.04480237f,
			0.85865086f, 0.5086297f, -0.04480237f, 0.5086297f, 0.85865086f, -0.04480237f, -0.121284805f, 0.43214726f, 0.78216845f, 0.43214726f,
			0.5086297f, -0.04480237f, 0.85865086f, -0.04480237f, 0.43214726f, -0.121284805f, 0.78216845f, 0.43214726f, 0.43214726f, 0.43214726f,
			0.78216845f, -0.121284805f, 0.3796829f, 0.3796829f, 0.753341f, 0.3796829f, 0.033819415f, 0.9982829f, 0.033819415f, 0.033819415f,
			-0.04480237f, 0.85865086f, -0.04480237f, 0.5086297f, -0.04480237f, 0.85865086f, 0.5086297f, -0.04480237f, -0.121284805f, 0.78216845f,
			0.43214726f, 0.43214726f, 0.5086297f, 0.85865086f, -0.04480237f, -0.04480237f, 0.43214726f, 0.78216845f, -0.121284805f, 0.43214726f,
			0.43214726f, 0.78216845f, 0.43214726f, -0.121284805f, 0.3796829f, 0.753341f, 0.3796829f, 0.3796829f, 0.9982829f, 0.033819415f,
			0.033819415f, 0.033819415f, 0.85865086f, -0.04480237f, -0.04480237f, 0.5086297f, 0.85865086f, -0.04480237f, 0.5086297f, -0.04480237f,
			0.78216845f, -0.121284805f, 0.43214726f, 0.43214726f, 0.85865086f, 0.5086297f, -0.04480237f, -0.04480237f, 0.78216845f, 0.43214726f,
			-0.121284805f, 0.43214726f, 0.78216845f, 0.43214726f, 0.43214726f, -0.121284805f, 0.753341f, 0.3796829f, 0.3796829f, 0.3796829f
		};
		for (int m = 0; m < array3.Length; m++)
		{
			array3[m] = (float)((double)array3[m] / 0.0220065933241897);
		}
		int n = 0;
		int num3 = 0;
		while (n < OpenSimplex2.GRADIENTS_4D.Length)
		{
			if (num3 == array3.Length)
			{
				num3 = 0;
			}
			OpenSimplex2.GRADIENTS_4D[n] = array3[num3];
			n++;
			num3++;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PRIME_X = 5910200641878280303L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PRIME_Y = 6452764530575939509L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PRIME_Z = 6614699811220273867L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long PRIME_W = 6254464313819354443L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long HASH_MULTIPLIER = 6026932503003350773L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long SEED_FLIP_3D = -5968755714895566377L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const long SEED_OFFSET_4D = 1045921697555224141L;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double ROOT2OVER2 = 0.7071067811865476;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double SKEW_2D = 0.366025403784439;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double UNSKEW_2D = -0.21132486540518713;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double ROOT3OVER3 = 0.577350269189626;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double FALLBACK_ROTATE_3D = 0.6666666666666666;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double ROTATE_3D_ORTHOGONALIZER = -0.21132486540518713;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float SKEW_4D = -0.1381966f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float UNSKEW_4D = 0.309017f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float LATTICE_STEP_4D = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_2D_EXPONENT = 7;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_3D_EXPONENT = 8;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_4D_EXPONENT = 9;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_2D = 128;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_3D = 256;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int N_GRADS_4D = 512;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double NORMALIZER_2D = 0.01001634121365712;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double NORMALIZER_3D = 0.07969837668935331;

	[PublicizedFrom(EAccessModifier.Private)]
	public const double NORMALIZER_4D = 0.0220065933241897;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float RSQUARED_2D = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float RSQUARED_3D = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float RSQUARED_4D = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] GRADIENTS_2D = new float[256];

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] GRADIENTS_3D;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly float[] GRADIENTS_4D;
}
