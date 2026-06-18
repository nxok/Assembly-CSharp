using System;

namespace SharpEXR.AttributeTypes
{
	public struct Box2I
	{
		public Box2I(int xMin, int yMin, int xMax, int yMax)
		{
			this.XMin = xMin;
			this.YMin = yMin;
			this.XMax = xMax;
			this.YMax = yMax;
		}

		public override string ToString()
		{
			return string.Format("{0}: ({1}, {2})-({3}, {4})", new object[]
			{
				base.GetType().Name,
				this.XMin,
				this.YMin,
				this.XMax,
				this.YMax
			});
		}

		public int Width
		{
			get
			{
				return this.XMax - this.XMin + 1;
			}
		}

		public int Height
		{
			get
			{
				return this.YMax - this.YMin + 1;
			}
		}

		public readonly int XMin;

		public readonly int YMin;

		public readonly int XMax;

		public readonly int YMax;
	}
}
