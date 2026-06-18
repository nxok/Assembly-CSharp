using System;

namespace SharpEXR.AttributeTypes
{
	public struct V3F
	{
		public V3F(float v0, float v1, float v2)
		{
			this.V0 = v0;
			this.V1 = v1;
			this.V2 = v2;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}, {3}", new object[]
			{
				base.GetType().Name,
				this.V0,
				this.V1,
				this.V2
			});
		}

		public float X
		{
			get
			{
				return this.V0;
			}
		}

		public float Y
		{
			get
			{
				return this.V1;
			}
		}

		public float Z
		{
			get
			{
				return this.V2;
			}
		}

		public float V0;

		public float V1;

		public float V2;
	}
}
