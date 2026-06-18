using System;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace GamePath
{
	public class PathInfoMultiTarget : PathInfo
	{
		public PathInfoMultiTarget(EntityAlive _entity, List<Vector3> _targetPositions, bool _canBreakBlocks, float _speed, EAIBase _aiTask)
			: base(_entity, _canBreakBlocks, _speed, _aiTask)
		{
			this.targetPositions = _targetPositions ?? new List<Vector3>();
		}

		public readonly List<Vector3> targetPositions;

		public bool pathsForAll;

		public OnPathDelegate[] OnTargetPathFinished;

		public MultiTargetPathSelection pathSelection = MultiTargetPathSelection.Closest;

		public List<Vector3>[] vectorPaths;
	}
}
