using System;

namespace SharpEXR.AttributeTypes
{
	public struct V2I
	{
		public V2I(int v0, int v1)
		{
			this.V0 = v0;
			this.V1 = v1;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1}, {2}", base.GetType().Name, this.V0, this.V1);
		}

		public int X
		{
			get
			{
				return this.V0;
			}
		}

		public int Y
		{
			get
			{
				return this.V1;
			}
		}

		public int V0;

		public int V1;
	}
}
