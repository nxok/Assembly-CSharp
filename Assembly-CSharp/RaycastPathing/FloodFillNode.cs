using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing
{
	[Preserve]
	public class FloodFillNode : RaycastNode
	{
		public FloodFillNode(Vector3 pos, float scale = 1f, int depth = 0)
			: base(pos, scale, depth)
		{
			this.score = new FloodFillNodeScore();
		}

		public FloodFillNode(Vector3 min, Vector3 max, float scale = 1f, int depth = 0)
			: base(min, max, scale, depth)
		{
			this.score = new FloodFillNodeScore();
		}

		public float G
		{
			get
			{
				return this.score.G;
			}
			set
			{
				this.score.G = value;
			}
		}

		public float Heuristic
		{
			get
			{
				return this.score.H;
			}
			set
			{
				this.score.H = value;
			}
		}

		public float F
		{
			get
			{
				return this.score.F;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public FloodFillNodeScore score;
	}
}
