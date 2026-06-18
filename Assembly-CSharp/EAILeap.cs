using System;
using System.Collections.Generic;
using GamePath;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAILeap : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 3;
		this.executeDelay = 1f + base.RandomFloat;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		base.GetData(data, "legs", ref this.legCount);
	}

	public override bool CanExecute()
	{
		if (this.theEntity.IsDancing)
		{
			return false;
		}
		if (!this.theEntity.GetAttackTarget())
		{
			return false;
		}
		if (this.theEntity.Jumping)
		{
			return false;
		}
		if ((this.legCount <= 2) ? this.theEntity.bodyDamage.IsAnyLegMissing : this.theEntity.bodyDamage.IsAnyArmOrLegMissing)
		{
			return false;
		}
		if (this.theEntity.moveHelper.BlockedFlags > 0)
		{
			return false;
		}
		PathEntity path = this.theEntity.navigator.getPath();
		if (path == null)
		{
			return false;
		}
		float jumpMaxDistance = this.theEntity.jumpMaxDistance;
		this.leapV = path.GetEndPos() - this.theEntity.position;
		if (this.leapV.y < -5f || this.leapV.y > 0.5f + jumpMaxDistance * 0.5f)
		{
			return false;
		}
		this.leapDist = Mathf.Sqrt(this.leapV.x * this.leapV.x + this.leapV.z * this.leapV.z);
		if (this.leapDist < 2.8f || this.leapDist > jumpMaxDistance)
		{
			return false;
		}
		Vector3 position = this.theEntity.position;
		position.y += 1.5f;
		RaycastHit raycastHit;
		return !Physics.Raycast(position - Origin.position, this.leapV, ref raycastHit, this.leapDist - 0.5f, 1082195968);
	}

	public override void Start()
	{
		this.abortTime = 5f;
		this.theEntity.moveHelper.Stop();
		this.leapYaw = Mathf.Atan2(this.leapV.x, this.leapV.z) * 57.29578f;
	}

	public override bool Continue()
	{
		if (this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		if (this.abortTime <= 0f)
		{
			return false;
		}
		EntityMoveHelper moveHelper = this.theEntity.moveHelper;
		this.theEntity.SeekYaw(this.leapYaw, 0f, 10f);
		if (Utils.FastAbs(Mathf.DeltaAngle(this.theEntity.rotation.y, this.leapYaw)) < 1f)
		{
			moveHelper.StartJump(false, this.leapDist, this.leapV.y);
			return false;
		}
		return true;
	}

	public override void Update()
	{
		this.abortTime -= 0.05f;
	}

	public override void Reset()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cCollisionMask = 1082195968;

	[PublicizedFrom(EAccessModifier.Private)]
	public int legCount = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 leapV;

	[PublicizedFrom(EAccessModifier.Private)]
	public float leapDist;

	[PublicizedFrom(EAccessModifier.Private)]
	public float leapYaw;

	[PublicizedFrom(EAccessModifier.Private)]
	public float abortTime;
}
