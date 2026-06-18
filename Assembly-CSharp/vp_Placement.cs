using System;
using UnityEngine;

public class vp_Placement
{
	public static bool AdjustPosition(vp_Placement p, float physicsRadius, int attempts = 1000)
	{
		attempts--;
		if (attempts > 0)
		{
			if (p.IsObstructed(physicsRadius))
			{
				Vector3 insideUnitSphere = Random.insideUnitSphere;
				p.Position.x = p.Position.x + insideUnitSphere.x;
				p.Position.z = p.Position.z + insideUnitSphere.z;
				vp_Placement.AdjustPosition(p, physicsRadius, attempts);
			}
			return true;
		}
		Debug.LogWarning("(vp_Placement.AdjustPosition) Failed to find valid placement.");
		return false;
	}

	public virtual bool IsObstructed(float physicsRadius = 1f)
	{
		return Physics.CheckSphere(this.Position, physicsRadius, 2261000);
	}

	public static void SnapToGround(vp_Placement p, float radius, float snapDistance)
	{
		if (snapDistance == 0f)
		{
			return;
		}
		RaycastHit raycastHit;
		Physics.SphereCast(new Ray(p.Position + Vector3.up * snapDistance, Vector3.down), radius, ref raycastHit, snapDistance * 2f, 1084850184);
		if (raycastHit.collider != null)
		{
			p.Position.y = raycastHit.point.y + 0.05f;
		}
	}

	public Vector3 Position = Vector3.zero;

	public Quaternion Rotation = Quaternion.identity;
}
