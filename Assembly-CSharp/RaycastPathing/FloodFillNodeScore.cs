using System;
using UnityEngine.Scripting;

namespace RaycastPathing
{
	[Preserve]
	public class FloodFillNodeScore
	{
		public float F
		{
			get
			{
				return this.G + this.H;
			}
		}

		public float G;

		public float H;
	}
}
