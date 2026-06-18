using System;
using UnityEngine;

namespace KinematicCharacterController
{
	public struct OverlapResult
	{
		public OverlapResult(Vector3 normal, Collider collider)
		{
			this.Normal = normal;
			this.Collider = collider;
		}

		public Vector3 Normal;

		public Collider Collider;
	}
}
