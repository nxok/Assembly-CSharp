using System;
using SharpEXR.AttributeTypes;

namespace SharpEXR.ColorSpace
{
	public static class XYZ
	{
		public static tMat3x3 CalcColorSpaceConversion_RGB_to_XYZ(Chromaticities chromaticities)
		{
			return XYZ.CalcColorSpaceConversion_RGB_to_XYZ(new tVec2(chromaticities.RedX, chromaticities.RedY), new tVec2(chromaticities.GreenX, chromaticities.GreenY), new tVec2(chromaticities.BlueX, chromaticities.BlueY), new tVec2(chromaticities.WhiteX, chromaticities.WhiteY));
		}

		public static tMat3x3 CalcColorSpaceConversion_RGB_to_XYZ(tVec2 red_xy, tVec2 green_xy, tVec2 blue_xy, tVec2 white_xy)
		{
			tMat3x3 tMat3x = default(tMat3x3);
			tVec3 tVec = new tVec3(red_xy.X, red_xy.Y, 1f - (red_xy.X + red_xy.Y));
			tVec3 tVec2 = new tVec3(green_xy.X, green_xy.Y, 1f - (green_xy.X + green_xy.Y));
			tVec3 tVec3 = new tVec3(blue_xy.X, blue_xy.Y, 1f - (blue_xy.X + blue_xy.Y));
			tVec3 tVec4 = new tVec3(white_xy.X, white_xy.Y, 1f - (white_xy.X + white_xy.Y));
			tVec4.X /= white_xy.Y;
			tVec4.Y /= white_xy.Y;
			tVec4.Z /= white_xy.Y;
			tMat3x.SetCol(0, tVec);
			tMat3x.SetCol(1, tVec2);
			tMat3x.SetCol(2, tVec3);
			tMat3x3 tMat3x2;
			tMat3x.Invert(out tMat3x2);
			tVec3 tVec5 = tMat3x2 * tVec4;
			ref tMat3x3 ptr = ref tMat3x;
			ptr[0, 0] = ptr[0, 0] * tVec5.X;
			ptr = ref tMat3x;
			ptr[1, 0] = ptr[1, 0] * tVec5.X;
			ptr = ref tMat3x;
			ptr[2, 0] = ptr[2, 0] * tVec5.X;
			ptr = ref tMat3x;
			ptr[0, 1] = ptr[0, 1] * tVec5.Y;
			ptr = ref tMat3x;
			ptr[1, 1] = ptr[1, 1] * tVec5.Y;
			ptr = ref tMat3x;
			ptr[2, 1] = ptr[2, 1] * tVec5.Y;
			ptr = ref tMat3x;
			ptr[0, 2] = ptr[0, 2] * tVec5.Z;
			ptr = ref tMat3x;
			ptr[1, 2] = ptr[1, 2] * tVec5.Z;
			ptr = ref tMat3x;
			ptr[2, 2] = ptr[2, 2] * tVec5.Z;
			return tMat3x;
		}
	}
}
