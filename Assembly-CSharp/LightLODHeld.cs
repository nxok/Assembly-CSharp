using System;
using UnityEngine;

public class LightLODHeld : LightLOD
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.startPos = this.selfT.localPosition;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void LateUpdate()
	{
		Transform selfT = this.selfT;
		if (!this.rootT)
		{
			this.rootT = RootTransformRefEntity.FindEntityUpwards(selfT);
			if (this.rootT)
			{
				this.player = this.rootT.GetComponent<EntityPlayerLocal>();
				this.parentStartPos = this.parentT.localPosition;
			}
			return;
		}
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		bool flag = this.player && this.player.bFirstPersonView;
		Vector3 vector;
		if (this.flickerRadius > 0f)
		{
			this.flickerTargetOffset = world.GetGameRandom().RandomOnUnitSphere;
			this.flickerOffset = Vector3.Lerp(this.flickerOffset, this.flickerTargetOffset, 0.2f);
			selfT.localPosition = this.startPos + this.flickerOffset * this.flickerRadius;
			vector = selfT.position;
			if (!flag)
			{
				vector.y += 0.2f;
			}
		}
		else if (flag)
		{
			selfT.localPosition = this.startPos;
			vector = selfT.position;
		}
		else
		{
			this.parentT.localPosition = Vector3.Lerp(this.parentStartPos, this.parentT.localPosition, 0.1f);
			selfT.localPosition = this.startPos;
			vector = selfT.position;
			vector.y += 0.2f;
		}
		Vector3 vector2 = this.rootT.position;
		vector2.y += 0.5f;
		RaycastHit raycastHit;
		if (Physics.Raycast(vector2, Vector3.up, ref raycastHit, 1.4f, -554734606))
		{
			vector2.y = raycastHit.point.y - 0.22f - 0.15f;
		}
		else
		{
			vector2.y += 0.95000005f;
		}
		if (vector2.y > vector.y + 0.25f)
		{
			vector2.y = vector.y + 0.25f;
		}
		Vector3 vector3 = vector - vector2;
		float num = vector3.magnitude;
		vector3 *= 1f / num;
		if (flag)
		{
			num *= 0.72f;
		}
		else
		{
			num *= 1.5f;
		}
		vector = vector2 + vector3 * num;
		vector2 += vector3 * 0.02f;
		if (Physics.SphereCast(vector2, 0.15f, vector3, ref raycastHit, num - 0.15f - 0.02f + 0.15f, -554734606))
		{
			vector2 = raycastHit.point;
			vector2 += raycastHit.normal * 0.15f;
			selfT.position = vector2;
			return;
		}
		selfT.position = vector;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cHeightCheckY = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cCastOffset = 0.02f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cCastRadius = 0.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cClosestDist = 0.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMask = -554734606;

	public float flickerRadius;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 flickerOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 flickerTargetOffset;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 startPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform rootT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 parentStartPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityPlayerLocal player;
}
