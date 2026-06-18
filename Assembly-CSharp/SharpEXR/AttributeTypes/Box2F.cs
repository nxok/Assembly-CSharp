using System;

namespace SharpEXR.AttributeTypes
{
	public struct Box2F
	{
		public Box2F(float xMin, float yMin, float xMax, float yMax)
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

		public float Width
		{
			get
			{
				return this.XMax - this.XMin + 1f;
			}
		}

		public float Height
		{
			get
			{
				return this.YMax - this.YMin + 1f;
			}
		}

		public readonly float XMin;

		public readonly float YMin;

		public readonly float XMax;

		public readonly float YMax;
	}
}
