using System;
using UnityEngine;

public class WaterClippingVolume
{
	public void Prepare(Plane waterClipPlane)
	{
		this.waterClipPlane = waterClipPlane;
		this.isSliced = WaterClippingUtils.GetCubePlaneIntersectionEdgeLoop(waterClipPlane, ref this.intersectionPoints, out this.count);
	}

	public void ApplyClipping(ref Vector3 vertLocalPos)
	{
		if (!this.isSliced)
		{
			return;
		}
		if (this.waterClipPlane.GetDistanceToPoint(vertLocalPos) > 0f)
		{
			vertLocalPos = this.waterClipPlane.ClosestPointOnPlane(vertLocalPos);
			if (!WaterClippingUtils.CubeBounds.Contains(vertLocalPos))
			{
				vertLocalPos = GeometryUtils.NearestPointOnEdgeLoop(vertLocalPos, this.intersectionPoints, this.count);
			}
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Plane waterClipPlane;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3[] intersectionPoints = new Vector3[6];

	[PublicizedFrom(EAccessModifier.Private)]
	public int count = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isSliced;
}
