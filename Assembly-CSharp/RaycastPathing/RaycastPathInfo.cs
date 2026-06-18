using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace RaycastPathing
{
	[Preserve]
	public class RaycastPathInfo
	{
		public RaycastPathInfo(Vector3 start, Vector3 target)
		{
			this.Start = start;
			this.Target = target;
			this.StartNode = new RaycastNode(this.Start, 1f, 0);
			this.TargetNode = new RaycastNode(this.Target, 1f, 0);
			this.PathStartsIndoors = RaycastPathWorldUtils.IsUnderground(start);
			this.PathEndsIndoors = RaycastPathWorldUtils.IsUnderground(target);
		}

		public Vector3i StartBlockPos
		{
			get
			{
				return this.StartNode.BlockPos;
			}
		}

		public Vector3i TargetBlockPos
		{
			get
			{
				return this.TargetNode.BlockPos;
			}
		}

		public bool PathStartsIndoors
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public bool PathEndsIndoors
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public static implicit operator bool(RaycastPathInfo exists)
		{
			return exists != null;
		}

		public readonly Vector3 Start;

		public readonly Vector3 Target;

		[PublicizedFrom(EAccessModifier.Private)]
		public RaycastNode StartNode;

		[PublicizedFrom(EAccessModifier.Private)]
		public RaycastNode TargetNode;
	}
}
