using System;

namespace SharpEXR.AttributeTypes
{
	public struct M33F
	{
		public M33F(float v0, float v1, float v2, float v3, float v4, float v5, float v6, float v7, float v8)
		{
			this.Values = new float[9];
			this.Values[0] = v0;
			this.Values[1] = v1;
			this.Values[2] = v2;
			this.Values[3] = v3;
			this.Values[4] = v4;
			this.Values[5] = v5;
			this.Values[6] = v6;
			this.Values[7] = v7;
			this.Values[8] = v8;
		}

		public readonly float[] Values;
	}
}
