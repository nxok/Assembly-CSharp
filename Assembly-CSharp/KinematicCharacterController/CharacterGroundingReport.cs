using System;
using UnityEngine;

namespace KinematicCharacterController
{
	public struct CharacterGroundingReport
	{
		public void CopyFrom(CharacterTransientGroundingReport transientGroundingReport)
		{
			this.FoundAnyGround = transientGroundingReport.FoundAnyGround;
			this.IsStableOnGround = transientGroundingReport.IsStableOnGround;
			this.SnappingPrevented = transientGroundingReport.SnappingPrevented;
			this.GroundNormal = transientGroundingReport.GroundNormal;
			this.InnerGroundNormal = transientGroundingReport.InnerGroundNormal;
			this.OuterGroundNormal = transientGroundingReport.OuterGroundNormal;
			this.GroundCollider = null;
			this.GroundPoint = Vector3.zero;
		}

		public bool FoundAnyGround;

		public bool IsStableOnGround;

		public bool SnappingPrevented;

		public Vector3 GroundNormal;

		public Vector3 InnerGroundNormal;

		public Vector3 OuterGroundNormal;

		public Collider GroundCollider;

		public Vector3 GroundPoint;
	}
}
