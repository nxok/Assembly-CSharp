using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing
{
	[Preserve]
	public class RaycastNodeInfo
	{
		public RaycastNodeInfo(Vector3 pos, float scale = 1f, int depth = 0)
		{
			this.Position = pos;
			this.BlockPos = World.worldToBlockPos(pos);
			this.Scale = scale;
			this.Depth = depth;
			this.Min = pos - Vector3.one * scale * 0.5f;
			this.Max = pos + Vector3.one * scale * 0.5f;
			this.Center = (this.Min + this.Max) * 0.5f;
		}

		public RaycastNodeInfo(Vector3 min, Vector3 max, float scale = 1f, int depth = 0)
		{
			this.Position = (min + max) * 0.5f;
			this.BlockPos = World.worldToBlockPos(this.Position);
			this.Scale = scale;
			this.Depth = depth;
			this.Min = min;
			this.Max = max;
			this.Center = this.Position;
		}

		public readonly Vector3 Position;

		public readonly Vector3i BlockPos;

		public readonly float Scale;

		public readonly int Depth;

		public readonly Vector3 Min;

		public readonly Vector3 Max;

		public readonly Vector3 Center;
	}
}
