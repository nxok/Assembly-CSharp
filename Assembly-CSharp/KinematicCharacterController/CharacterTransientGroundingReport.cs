using System;
using UnityEngine;

namespace KinematicCharacterController
{
	public struct CharacterTransientGroundingReport
	{
		public void CopyFrom(CharacterGroundingReport groundingReport)
		{
			this.FoundAnyGround = groundingReport.FoundAnyGround;
			this.IsStableOnGround = groundingReport.IsStableOnGround;
			this.SnappingPrevented = groundingReport.SnappingPrevented;
			this.GroundNormal = groundingReport.GroundNormal;
			this.InnerGroundNormal = groundingReport.InnerGroundNormal;
			this.OuterGroundNormal = groundingReport.OuterGroundNormal;
		}

		public bool FoundAnyGround;

		public bool IsStableOnGround;

		public bool SnappingPrevented;

		public Vector3 GroundNormal;

		public Vector3 InnerGroundNormal;

		public Vector3 OuterGroundNormal;
	}
}
