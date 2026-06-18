using System;
using KinematicCharacterController;
using UnityEngine;

public class CharacterControllerKinematic : CharacterControllerAbstract
{
	public CharacterControllerKinematic(Entity _entity)
	{
		GameObject gameObject = _entity.PhysicsTransform.gameObject;
		KinematicCharacterSystem.EnsureCreation();
		this.cs = KinematicCharacterSystem.GetInstance();
		KinematicCharacterSystem.AutoSimulation = false;
		KinematicCharacterSystem.Interpolate = false;
		this.motor = gameObject.AddComponent<KinematicCharacterMotor>();
		this.motor.StepHandling = StepHandlingMethod.Extra;
		this.motor.AllowSteppingWithoutStableGrounding = true;
		this.motor.InteractiveRigidbodyHandling = false;
		this.motor.LedgeAndDenivelationHandling = false;
		this.motor.MaxStableSlopeAngle = 63.8f;
		this.cc = new CC();
		this.cc.entity = _entity;
		this.cc.motor = this.motor;
		this.motor.CharacterController = this.cc;
		this.motor.ForceUnground(0.1f);
	}

	public override void Enable(bool isEnabled)
	{
		this.motor.enabled = isEnabled;
	}

	public override void SetStepOffset(float _stepOffset)
	{
		this.motor.MaxStepHeight = _stepOffset + 0.01f;
	}

	public override float GetStepOffset()
	{
		return this.motor.MaxStepHeight;
	}

	public override void SetSize(Vector3 _center, float _height, float _radius)
	{
		this.motor.SetCapsuleDimensions(_radius, _height, _center.y);
	}

	public override void SetCenter(Vector3 _center)
	{
		this.motor.SetCapsuleDimensions(this.GetRadius(), this.GetHeight(), _center.y);
	}

	public override Vector3 GetCenter()
	{
		return this.motor.CharacterTransformToCapsuleCenter;
	}

	public override void SetRadius(float _radius)
	{
		this.motor.SetCapsuleDimensions(_radius, this.GetHeight(), this.GetCenter().y);
	}

	public override float GetRadius()
	{
		return this.motor.Capsule.radius;
	}

	public override void SetSkinWidth(float _width)
	{
	}

	public override float GetSkinWidth()
	{
		return 0.08f;
	}

	public override void SetHeight(float _height)
	{
		float radius = this.GetRadius();
		_height = Utils.FastMax(_height, radius * 2f);
		this.motor.SetCapsuleDimensions(radius, _height, _height * 0.5f);
	}

	public override float GetHeight()
	{
		return this.motor.Capsule.height;
	}

	public override bool IsGrounded()
	{
		return (this.cc.collisionFlags & 4) > 0;
	}

	public override Vector3 GroundNormal
	{
		get
		{
			return this.motor.GroundingStatus.GroundNormal;
		}
	}

	public override bool enableOverlapRecovery
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public override CollisionFlags Move(Vector3 _dir)
	{
		if (_dir.y >= 0.011f)
		{
			this.motor.ForceUnground(0.11f);
		}
		this.cc.vel = _dir / 0.05f;
		return this.Update();
	}

	public override CollisionFlags Update()
	{
		this.cc.Move();
		if (this.motor.GroundingStatus.FoundAnyGround)
		{
			this.cc.collisionFlags |= 4;
		}
		return this.cc.collisionFlags;
	}

	public override void Rotate(Quaternion _dir)
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public KinematicCharacterSystem cs;

	[PublicizedFrom(EAccessModifier.Private)]
	public KinematicCharacterMotor motor;

	[PublicizedFrom(EAccessModifier.Private)]
	public CC cc;
}
