using System;
using KinematicCharacterController;
using UnityEngine;

public class CC : ICharacterController
{
	public void Move()
	{
		this.collisionFlags = 0;
		this.hadWallOverlap = false;
		this.tickCount++;
		this.motor.UpdatePhase1(0.05f);
		this.motor.UpdatePhase2(0.05f);
		this.motor.Transform.SetPositionAndRotation(this.motor.TransientPosition, this.motor.TransientRotation);
	}

	public void BeforeCharacterUpdate(float deltaTime)
	{
	}

	public bool OnCollisionOverlap(int nbOverlaps, Collider[] _colliders)
	{
		Vector3 position = this.motor.Transform.position;
		bool flag;
		do
		{
			flag = false;
			for (int i = 0; i < nbOverlaps - 1; i++)
			{
				Collider collider = _colliders[i];
				Collider collider2 = _colliders[i + 1];
				if (collider.gameObject.layer == 15)
				{
					if (collider2.gameObject.layer != 15)
					{
						_colliders[i] = collider2;
						_colliders[i + 1] = collider;
						flag = true;
					}
					else
					{
						float sqrMagnitude = (collider.transform.position - position).sqrMagnitude;
						if ((collider2.transform.position - position).sqrMagnitude < sqrMagnitude)
						{
							_colliders[i] = collider2;
							_colliders[i + 1] = collider;
							flag = true;
						}
					}
				}
			}
		}
		while (flag);
		if (_colliders[0].gameObject.layer != 15)
		{
			this.hadWallOverlap = true;
		}
		else if (this.hadWallOverlap)
		{
			return false;
		}
		return true;
	}

	public float GetCollisionOverlapScale(Transform overlappedTransform)
	{
		if (overlappedTransform.gameObject.layer != 15)
		{
			return 1f;
		}
		if (((this.entity.entityId + this.tickCount) & 15) != 0)
		{
			return 0.1f;
		}
		return 0.5f;
	}

	public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
	{
	}

	public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
	{
		if (this.vel.y <= 0.001f)
		{
			if (this.motor.GroundingStatus.IsStableOnGround)
			{
				Vector3 groundNormal = this.motor.GroundingStatus.GroundNormal;
				this.vel.y = 0f;
				float magnitude = this.vel.magnitude;
				Vector3 vector = Vector3.Cross(this.vel, this.motor.CharacterUp);
				this.vel = Vector3.Cross(groundNormal, vector).normalized * magnitude;
				this.vel = this.vel * 0.5f + currentVelocity * 0.5f;
			}
			else
			{
				this.vel = this.vel * 0.5f + currentVelocity * 0.5f;
			}
		}
		currentVelocity = this.vel;
	}

	public void AfterCharacterUpdate(float deltaTime)
	{
	}

	public bool IsColliderValidForCollisions(Collider coll)
	{
		return true;
	}

	public void OnDiscreteCollisionDetected(Collider hitCollider)
	{
	}

	public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
	{
	}

	public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
	{
		if (hitNormal.y >= 0.64f)
		{
			this.collisionFlags |= 4;
			return;
		}
		if (hitNormal.y > -0.5f)
		{
			this.collisionFlags |= 1;
		}
	}

	public void PostGroundingUpdate(float deltaTime)
	{
	}

	public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
	{
	}

	public Entity entity;

	public KinematicCharacterMotor motor;

	public CollisionFlags collisionFlags;

	public Vector3 vel;

	[PublicizedFrom(EAccessModifier.Private)]
	public int tickCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hadWallOverlap;
}
