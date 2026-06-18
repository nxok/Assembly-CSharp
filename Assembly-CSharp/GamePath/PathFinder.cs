using System;
using UnityEngine;

namespace GamePath
{
	public class PathFinder
	{
		public PathFinder(PathInfo _pathInfo, bool _bDrn, bool _canClimbLadders, bool _bCanClimbWalls)
		{
			this.pathInfo = _pathInfo;
			this.canEntityDrown = _bDrn;
			this.canClimbWalls = _bCanClimbWalls;
			this.canClimbLadders = _canClimbLadders;
		}

		public virtual void Calculate(Vector3 _fromPos)
		{
		}

		public virtual void Destruct()
		{
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public PathInfo pathInfo;

		public bool canClimbWalls;

		public bool canClimbLadders;

		public bool canEntityDrown;
	}
}
