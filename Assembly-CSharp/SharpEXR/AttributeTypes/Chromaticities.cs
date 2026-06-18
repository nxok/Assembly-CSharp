using System;

namespace SharpEXR.AttributeTypes
{
	public struct Chromaticities
	{
		public Chromaticities(float redX, float redY, float greenX, float greenY, float blueX, float blueY, float whiteX, float whiteY)
		{
			this.RedX = redX;
			this.RedY = redY;
			this.GreenX = greenX;
			this.GreenY = greenY;
			this.BlueX = blueX;
			this.BlueY = blueY;
			this.WhiteX = whiteX;
			this.WhiteY = whiteY;
		}

		public readonly float RedX;

		public readonly float RedY;

		public readonly float GreenX;

		public readonly float GreenY;

		public readonly float BlueX;

		public readonly float BlueY;

		public readonly float WhiteX;

		public readonly float WhiteY;
	}
}
