using System;
using UnityEngine;

public class CharacterControllerUnity : CharacterControllerAbstract
{
	public CharacterControllerUnity(CharacterController _cc)
	{
		this.cc = _cc;
	}

	public override void Enable(bool isEnabled)
	{
		this.cc.enabled = isEnabled;
	}

	public override void SetStepOffset(float _stepOffset)
	{
		this.cc.stepOffset = _stepOffset;
	}

	public override float GetStepOffset()
	{
		return this.cc.stepOffset;
	}

	public override void SetSize(Vector3 _center, float _height, float _radius)
	{
		this.cc.center = _center;
		this.cc.height = _height;
		this.cc.radius = _radius;
	}

	public override void SetCenter(Vector3 _center)
	{
		this.cc.center = _center;
	}

	public override Vector3 GetCenter()
	{
		return this.cc.center;
	}

	public override void SetRadius(float _radius)
	{
		this.cc.radius = _radius;
	}

	public override float GetRadius()
	{
		return this.cc.radius;
	}

	public override void SetSkinWidth(float _width)
	{
		this.cc.skinWidth = _width;
	}

	public override float GetSkinWidth()
	{
		return this.cc.skinWidth;
	}

	public override void SetHeight(float _height)
	{
		this.cc.height = _height;
	}

	public override float GetHeight()
	{
		return this.cc.height;
	}

	public override bool IsGrounded()
	{
		return this.cc.isGrounded;
	}

	public override CollisionFlags Move(Vector3 _dir)
	{
		return this.cc.Move(_dir);
	}

	public override void Rotate(Quaternion _dir)
	{
	}

	public override bool enableOverlapRecovery
	{
		get
		{
			return this.cc.enableOverlapRecovery;
		}
		set
		{
			this.cc.enableOverlapRecovery = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public CharacterController cc;
}
