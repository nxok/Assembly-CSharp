using System;

namespace SharpEXR.ColorSpace
{
	public static class Gamma
	{
		public static float Expand(float nonlinear)
		{
			return (float)Math.Pow((double)nonlinear, 2.2);
		}

		public static float Compress(float linear)
		{
			return (float)Math.Pow((double)linear, 0.45454545454545453);
		}

		public static void Expand(ref tVec3 pColor)
		{
			pColor.X = Gamma.Expand(pColor.X);
			pColor.Y = Gamma.Expand(pColor.Y);
			pColor.Z = Gamma.Expand(pColor.Z);
		}

		public static void Compress(ref tVec3 pColor)
		{
			pColor.X = Gamma.Compress(pColor.X);
			pColor.Y = Gamma.Compress(pColor.Y);
			pColor.Z = Gamma.Compress(pColor.Z);
		}

		public static void Expand(ref float r, ref float g, ref float b)
		{
			r = Gamma.Expand(r);
			g = Gamma.Expand(g);
			b = Gamma.Expand(b);
		}

		public static void Compress(ref float r, ref float g, ref float b)
		{
			r = Gamma.Compress(r);
			g = Gamma.Compress(g);
			b = Gamma.Compress(b);
		}

		public static tVec3 Expand(float r, float g, float b)
		{
			tVec3 tVec = new tVec3(r, g, b);
			Gamma.Expand(ref tVec);
			return tVec;
		}

		public static tVec3 Compress(float r, float g, float b)
		{
			tVec3 tVec = new tVec3(r, g, b);
			Gamma.Compress(ref tVec);
			return tVec;
		}

		public static float Expand_sRGB(float nonlinear)
		{
			if (nonlinear > 0.04045f)
			{
				return (float)Math.Pow((double)((nonlinear + 0.055f) / 1.055f), 2.4000000953674316);
			}
			return nonlinear / 12.92f;
		}

		public static float Compress_sRGB(float linear)
		{
			if (linear > 0.0031308f)
			{
				return 1.055f * (float)Math.Pow((double)linear, 0.4166666567325592) - 0.055f;
			}
			return 12.92f * linear;
		}

		public static void Expand_sRGB(ref tVec3 pColor)
		{
			pColor.X = Gamma.Expand_sRGB(pColor.X);
			pColor.Y = Gamma.Expand_sRGB(pColor.Y);
			pColor.Z = Gamma.Expand_sRGB(pColor.Z);
		}

		public static void Compress_sRGB(ref tVec3 pColor)
		{
			pColor.X = Gamma.Compress_sRGB(pColor.X);
			pColor.Y = Gamma.Compress_sRGB(pColor.Y);
			pColor.Z = Gamma.Compress_sRGB(pColor.Z);
		}

		public static void Expand_sRGB(ref float r, ref float g, ref float b)
		{
			r = Gamma.Expand_sRGB(r);
			g = Gamma.Expand_sRGB(g);
			b = Gamma.Expand_sRGB(b);
		}

		public static void Compress_sRGB(ref float r, ref float g, ref float b)
		{
			r = Gamma.Compress_sRGB(r);
			g = Gamma.Compress_sRGB(g);
			b = Gamma.Compress_sRGB(b);
		}

		public static tVec3 Expand_sRGB(float r, float g, float b)
		{
			tVec3 tVec = new tVec3(r, g, b);
			Gamma.Expand_sRGB(ref tVec);
			return tVec;
		}

		public static tVec3 Compress_sRGB(float r, float g, float b)
		{
			tVec3 tVec = new tVec3(r, g, b);
			Gamma.Compress_sRGB(ref tVec);
			return tVec;
		}
	}
}
