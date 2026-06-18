using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarCharacterController : AvatarMultiBodyController
{
	public BodyAnimator CharacterBody
	{
		get
		{
			return this.characterBody;
		}
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		if (this.characterBody != null && this.modelName != _modelName)
		{
			if (this.characterBody.Parts.BodyObj != null)
			{
				this.characterBody.Parts.BodyObj.SetActive(false);
			}
			base.removeBodyAnimator(this.characterBody);
			this.characterBody = null;
		}
		if (this.characterBody == null)
		{
			this.modelName = _modelName;
			Transform transform = EModelBase.FindModel(base.transform);
			if (transform != null)
			{
				Transform transform2 = transform.Find(_modelName);
				if (transform2)
				{
					transform2.gameObject.SetActive(true);
					this.characterBody = base.addBodyAnimator(this.createCharacterBody(transform2));
				}
			}
		}
		if (this.characterBody != null)
		{
			this.initBodyAnimator(this.characterBody, _bFPV, _bMale);
		}
		base.SwitchModelAndView(_modelName, _bFPV, _bMale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void initBodyAnimator(BodyAnimator _body, bool _bFPV, bool _bMale)
	{
		base._setBool("IsMale", _bMale, true);
		base._setFloat("IsMaleFloat", _bMale ? 1f : 0f, true);
		this.SetWalkType(this.entity.GetWalkType(), false);
		this._setBool(AvatarController.isDeadHash, this.entity.IsDead(), true);
		this._setBool(AvatarController.isFPVHash, _bFPV, true);
		this._setBool(AvatarController.isAliveHash, this.entity.IsAlive(), true);
		if (_body == this.characterBody)
		{
			this.characterBody.State = (_bFPV ? BodyAnimator.EnumState.OnlyColliders : BodyAnimator.EnumState.Visible);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract BodyAnimator createCharacterBody(Transform _bodyTransform);

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual HashSet<int> getJumpStates()
	{
		return new HashSet<int> { Animator.StringToHash("Base Layer.Jump") };
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual HashSet<int> getDeathStates()
	{
		HashSet<int> hashSet = new HashSet<int>();
		AvatarCharacterController.GetFirstPersonDeathStates(hashSet);
		AvatarCharacterController.GetThirdPersonDeathStates(hashSet);
		return hashSet;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual HashSet<int> getReloadStates()
	{
		HashSet<int> hashSet = new HashSet<int>();
		AvatarCharacterController.GetFirstPersonReloadStates(hashSet);
		AvatarCharacterController.GetThirdPersonReloadStates(hashSet);
		return hashSet;
	}

	public override Animator GetAnimator()
	{
		return this.characterBody.Animator;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual HashSet<int> getHitStates()
	{
		HashSet<int> hashSet = new HashSet<int>();
		AvatarCharacterController.GetThirdPersonHitStates(hashSet);
		return hashSet;
	}

	public static void GetFirstPersonReloadStates(HashSet<int> hashSet)
	{
		hashSet.Add(Animator.StringToHash("Base Layer.fpvBlunderbussReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvSawedOffShotgunReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvPistolReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvMP5Reload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvSniperRifleReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvM136Reload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvCrossbowReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvHuntingRifleReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvAugerReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvChainsawReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvSawedOffShotgunReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvMagnumReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvBowReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvNailGunReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvAK47Reload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvBowReload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvAK47Reload"));
		hashSet.Add(Animator.StringToHash("Base Layer.fpvCompoundBowReload"));
	}

	public static void GetThirdPersonReloadStates(HashSet<int> hashSet)
	{
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleSawedOffShotgunReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleMP5Reload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleSniperRifleReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleM136Reload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleCrossbowReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleHuntingRifleReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleAugerReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleChainsawReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleSawedOffShotgunReloadIntro"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleSawedOffShotgunReloadExit"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.Female44MagnumReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemaleNailGunReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleBowReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleBlunderbussReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleBowReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.FemalePistolReload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleAk47Reload"));
		hashSet.Add(Animator.StringToHash("TwoHandedOverlays.femaleCompoundBowReload"));
	}

	public static void GetFirstPersonHitStates(HashSet<int> hashSet)
	{
	}

	public static void GetThirdPersonHitStates(HashSet<int> hashSet)
	{
		hashSet.Add(Animator.StringToHash("PainOverlays.femaleTwitchHeadLeft"));
		hashSet.Add(Animator.StringToHash("PainOverlays.femaleTwitchHeadRight"));
		hashSet.Add(Animator.StringToHash("PainOverlays.femaleTwitchChestLeft"));
		hashSet.Add(Animator.StringToHash("PainOverlays.femaleTwitchChestRight"));
	}

	public static void GetFirstPersonDeathStates(HashSet<int> hashSet)
	{
	}

	public static void GetThirdPersonDeathStates(HashSet<int> hashSet)
	{
		hashSet.Add(Animator.StringToHash("Base Layer.generic"));
		hashSet.Add(Animator.StringToHash("Base Layer.FemaleDeath01"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingHead"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingChest"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingLeftArm"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingRightArm"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingLeftLeg"));
		hashSet.Add(Animator.StringToHash("Base Layer.idlingRightLeg"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadFront"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadLeft"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadLeftA"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadLeftB"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadLeftC"));
		hashSet.Add(Animator.StringToHash("Base Layer.meleeHeadRight"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningChestA"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningChestB"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningHeadA"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningHeadB"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningLeftArmA"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningLeftArmB"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningRightArmA"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningRightArmB"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningLeftLeg"));
		hashSet.Add(Animator.StringToHash("Base Layer.runningRightLeg"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingChestA"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingChestB"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingHeadA"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingHeadB"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingLeftArm"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingRightArm"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingLeftLeg"));
		hashSet.Add(Animator.StringToHash("Base Layer.walkingRightLeg"));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setTrigger(int _pid, bool _netsync = true)
	{
		base._setTrigger(_pid, _netsync);
		if (this.characterBody != null)
		{
			Animator animator = this.characterBody.Animator;
			if (AvatarMultiBodyController.animatorIsValid(animator))
			{
				animator.SetTrigger(_pid);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _resetTrigger(int _pid, bool _netsync = true)
	{
		base._resetTrigger(_pid, _netsync);
		if (this.characterBody != null)
		{
			Animator animator = this.characterBody.Animator;
			if (animator)
			{
				animator.ResetTrigger(_pid);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setFloat(int _pid, float _value, bool _netsync = true)
	{
		base._setFloat(_pid, _value, _netsync);
		if (this.characterBody != null)
		{
			Animator animator = this.characterBody.Animator;
			if (animator)
			{
				animator.SetFloat(_pid, _value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setBool(int _pid, bool _value, bool _netsync = true)
	{
		base._setBool(_pid, _value, _netsync);
		if (this.characterBody != null)
		{
			Animator animator = this.characterBody.Animator;
			if (animator)
			{
				animator.SetBool(_pid, _value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setInt(int _pid, int _value, bool _netsync = true)
	{
		base._setInt(_pid, _value, _netsync);
		if (this.characterBody != null)
		{
			Animator animator = this.characterBody.Animator;
			if (animator)
			{
				animator.SetInteger(_pid, _value);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AvatarCharacterController()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BodyAnimator characterBody;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string modelName;

	public class AnimationStates
	{
		public AnimationStates(HashSet<int> _jumpStates, HashSet<int> _deathStates, HashSet<int> _reloadStates, HashSet<int> _hitStates)
		{
			this.JumpStates = _jumpStates;
			this.DeathStates = _deathStates;
			this.ReloadStates = _reloadStates;
			this.HitStates = _hitStates;
		}

		public readonly HashSet<int> JumpStates;

		public readonly HashSet<int> DeathStates;

		public readonly HashSet<int> ReloadStates;

		public readonly HashSet<int> HitStates;
	}
}
