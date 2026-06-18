using System;
using UnityEngine;

namespace GamePath
{
	public class PathInfoFleeTarget : PathInfoFleeRandom
	{
		public PathInfoFleeTarget(EntityAlive _entity, Vector3 _fleeTarget, int _searchLength, Vector3 _aimBias, float _aimStrength, float _speed, EAIBase _aiTask)
			: base(_entity, _searchLength, _aimBias, _aimStrength, _speed, _aiTask)
		{
			this.fleeTarget = _fleeTarget;
		}

		public Vector3 fleeTarget;
	}
}
