using System;
using UnityEngine;

public class AnimatorDrawbridgeState : AnimatorDoorState
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.mask = LayerMask.GetMask(new string[] { "Physics", "CC Physics", "CC Local Physics" });
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool CheckForObstacles()
	{
		if (this.colliders == null)
		{
			return false;
		}
		for (int i = 0; i < this.colliders.Length; i++)
		{
			EntityCollisionRules entityCollisionRules = this.rules[i];
			if (!entityCollisionRules || !entityCollisionRules.IsStatic)
			{
				Vector3 vector = this.colliders[i].bounds.extents;
				if (this.colliders[i] is MeshCollider)
				{
					vector = Vector3.Scale(((MeshCollider)this.colliders[i]).sharedMesh.bounds.extents, this.colliders[i].transform.localScale);
				}
				if (Physics.OverlapBoxNonAlloc(this.colliders[i].bounds.center, vector, AnimatorDrawbridgeState.overlapBoxHits, this.colliders[i].transform.rotation, this.mask) > 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void PushPlayers(float _normalizedTime)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static Collider[] overlapBoxHits = new Collider[20];

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public LayerMask mask;
}
