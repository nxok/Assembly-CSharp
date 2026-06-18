using System;
using UnityEngine;

public class AvatarControllerDummy : LegacyAvatarController
{
	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		if (this.modelTransform != null)
		{
			this.bipedTransform = this.modelTransform.Find(_modelName + (_bFPV ? "_FP" : ""));
			if (this.bipedTransform != null && this.entity != null)
			{
				this.rightHand = this.bipedTransform.FindInChilds(this.entity.GetRightHandTransformName(), false);
				base.SetAnimator(this.bipedTransform);
			}
		}
	}

	public override Transform GetRightHandTransform()
	{
		return this.rightHand;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void assignStates()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateSpineRotation()
	{
	}

	public override bool IsAnimationSpecialAttackPlaying()
	{
		return this.bSpecialAttackPlaying;
	}

	public override void StartAnimationSpecialAttack(bool _b, int _animType)
	{
		this.idleTime = 0f;
		this.bSpecialAttackPlaying = _b;
	}

	public override bool IsAnimationSpecialAttack2Playing()
	{
		return this.timeSpecialAttack2Playing > 0f;
	}

	public override void StartAnimationSpecialAttack2()
	{
		this.idleTime = 0f;
		this.timeSpecialAttack2Playing = 0.3f;
	}

	public override bool IsAnimationRagingPlaying()
	{
		return this.timeRagePlaying > 0f;
	}

	public override void StartAnimationRaging()
	{
		this.idleTime = 0f;
		this.ragingTicks = 3;
		this.timeRagePlaying = 0.3f;
	}

	public override bool IsAnimationWithMotionRunning()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (this.timeAttackAnimationPlaying > 0f)
		{
			this.timeAttackAnimationPlaying -= Time.deltaTime;
		}
		if (this.timeUseAnimationPlaying > 0f)
		{
			this.timeUseAnimationPlaying -= Time.deltaTime;
		}
		if (this.timeRagePlaying > 0f)
		{
			this.timeRagePlaying -= Time.deltaTime;
		}
		if (this.timeSpecialAttack2Playing > 0f)
		{
			this.timeSpecialAttack2Playing -= Time.deltaTime;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public new bool bSpecialAttackPlaying;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public new float timeSpecialAttack2Playing;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeRagePlaying;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int ragingTicks;
}
