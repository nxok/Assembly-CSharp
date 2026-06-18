using System;
using Pathfinding;
using UnityEngine;

namespace GamePath
{
	public class PathInfoSingleTarget : PathInfo
	{
		public PathInfoSingleTarget(EntityAlive _entity, Vector3 _targetPos, bool _canBreakBlocks, float _speed, EAIBase _aiTask)
			: base(_entity, _canBreakBlocks, _speed, _aiTask)
		{
			this.entity = _entity;
			this.hasStart = false;
			this.targetPos = _targetPos;
			this.canBreakBlocks = _canBreakBlocks;
			this.speed = _speed;
			this.aiTask = _aiTask;
			this.path = null;
			this.calculatePartial = true;
		}

		public Vector3 targetPos;

		public PathEndingCondition endingCondition;
	}
}
