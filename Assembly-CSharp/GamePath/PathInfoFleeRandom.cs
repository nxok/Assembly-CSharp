using System;
using UnityEngine;

namespace GamePath
{
	public class PathInfoFleeRandom : PathInfo
	{
		public PathInfoFleeRandom(EntityAlive _entity, int _searchLength, Vector3 _aimBias, float _aimStrength, float _speed, EAIBase _aiTask)
			: base(_entity, false, _speed, _aiTask)
		{
			this.searchLength = _searchLength;
			this.aimBias = _aimBias;
			this.aimStrength = _aimStrength;
		}

		public int searchLength;

		public Vector3 aimBias = Vector3.zero;

		public float aimStrength;
	}
}
