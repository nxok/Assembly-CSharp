using System;

namespace SharpEXR.AttributeTypes
{
	public struct V3I
	{
		public V3I(int v0, int v1, int v2)
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

		public int Z
		{
			get
			{
				return this.V2;
			}
		}

		public int V0;

		public int V1;

		public int V2;
	}
}
