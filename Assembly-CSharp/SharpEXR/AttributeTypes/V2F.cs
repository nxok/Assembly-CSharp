using System;

namespace SharpEXR.AttributeTypes
{
	public struct V2F
	{
		public V2F(float v0, float v1)
		{
			this.V0 = v0;
			this.V1 = v1;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}", base.GetType().Name, this.V0, this.V1);
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

		public float V0;

		public float V1;
	}
}
