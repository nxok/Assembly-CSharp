using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EAIWander : EAIBase
{
	public override void Init(EntityAlive _theEntity)
	{
		base.Init(_theEntity);
		this.MutexBits = 1;
	}

	public override void SetData(Dictionary<string, string> data)
	{
		base.SetData(data);
		base.GetData(data, "exePer", ref this.executePercent);
		base.GetData(data, "fade", ref this.fade);
		base.GetData(data, "lookMin", ref this.lookMin);
		base.GetData(data, "lookMax", ref this.lookMax);
	}

	public override bool CanExecute()
	{
		if (this.theEntity.sleepingOrWakingUp)
		{
			return false;
		}
		if (this.manager.lookTime > 0f)
		{
			return false;
		}
		if (this.fade == 1f && this.theEntity.GetTicksNoPlayerAdjacent() >= 120)
		{
			return false;
		}
		if (this.theEntity.bodyDamage.CurrentStun != EnumEntityStunType.None)
		{
			return false;
		}
		bool isAlert = this.theEntity.IsAlert;
		if (!isAlert && this.executePercent * this.executeWaitTime <= base.RandomFloat)
		{
			return false;
		}
		int num = 1;
		int num2 = (int)this.manager.interestDistance;
		if (isAlert)
		{
			num = 2;
			num2 *= 2;
		}
		Vector3 vector = ((base.RandomFloat < 0.6f) ? this.theEntity.GetForwardVector() : base.Random.RandomOnUnitCircleXZ);
		Vector3 vector2 = RandomPositionGenerator.CalcInDir(this.theEntity, num, num2, num2, vector, 90f);
		if (vector2.y == 0f)
		{
			return false;
		}
		this.position = vector2;
		return true;
	}

	public override void Start()
	{
		this.time = 0f;
		this.theEntity.FindPath(this.position, this.theEntity.GetMoveSpeed(), false, this);
		this.theEntity.renderFadeMax = this.fade;
	}

	public override bool Continue()
	{
		return this.theEntity.bodyDamage.CurrentStun == EnumEntityStunType.None && this.theEntity.moveHelper.BlockedTime <= 0.3f && this.time <= 30f && !this.theEntity.navigator.noPathAndNotPlanningOne();
	}

	public override void Update()
	{
		this.time += 0.05f;
	}

	public override void Reset()
	{
		this.manager.lookTime = base.Random.RandomRange(this.lookMin, this.lookMax);
		this.theEntity.moveHelper.Stop();
		this.theEntity.renderFadeMax = 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float fade = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lookMin = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lookMax = 5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float executePercent = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public float time;
}
