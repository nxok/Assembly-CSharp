using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarMultiBodyController : AvatarController
{
	public BodyAnimator PrimaryBody
	{
		get
		{
			return this.primaryBody;
		}
		set
		{
			this.primaryBody = value;
			this.SetInRightHand(this.heldItemTransform);
		}
	}

	public List<BodyAnimator> BodyAnimators
	{
		get
		{
			return this.bodyAnimators;
		}
	}

	public Animator HeldItemAnimator
	{
		get
		{
			return this.heldItemAnimator;
		}
	}

	public Transform HeldItemTransform
	{
		get
		{
			return this.heldItemTransform;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public BodyAnimator addBodyAnimator(BodyAnimator _body)
	{
		Animator animator = _body.Animator;
		if (animator)
		{
			animator.logWarnings = false;
		}
		this.bodyAnimators.Add(_body);
		base.SetAnimator(this.bodyAnimators[0].Animator);
		return _body;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void removeBodyAnimator(BodyAnimator _body)
	{
		this.bodyAnimators.Remove(_body);
	}

	public override void PlayPlayerFPRevive()
	{
		int count = this.bodyAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			Animator animator = this.bodyAnimators[i].Animator;
			if (animator)
			{
				animator.SetTrigger(AvatarController.reviveHash);
			}
		}
		this.reviveTime = Time.time;
	}

	public override bool IsAnimationPlayerFPRevivePlaying()
	{
		return Time.time - this.reviveTime < 4.5f;
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		if (this.heldItemTransform == null || this.entity == null || this.entity.inventory == null || this.entity.inventory.holdingItem == null)
		{
			return;
		}
		if (_bFPV)
		{
			this.heldItemTransform.localPosition = Vector3.zero;
			this.heldItemTransform.localEulerAngles = Vector3.zero;
			return;
		}
		this.heldItemTransform.localPosition = AnimationGunjointOffsetData.AnimationGunjointOffset[this.entity.inventory.holdingItem.HoldType.Value].position;
		this.heldItemTransform.localEulerAngles = AnimationGunjointOffsetData.AnimationGunjointOffset[this.entity.inventory.holdingItem.HoldType.Value].rotation;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnTrigger(int _id)
	{
		if (_id == AvatarController.weaponFireHash)
		{
			this.animationToDodgeTime = 1f;
		}
	}

	public override bool IsAnimationToDodge()
	{
		return this.animationToDodgeTime > 0f;
	}

	public override bool IsAnimationAttackPlaying()
	{
		return false;
	}

	public override void SetInAir(bool inAir)
	{
		this._setBool(AvatarController.inAirHash, inAir, true);
	}

	public override void StartAnimationAttack()
	{
		this._setBool(AvatarController.harvestingHash, false, true);
		int meta = this.entity.inventory.holdingItemItemValue.Meta;
		this._setInt(AvatarController.weaponAmmoRemaining, meta, true);
		this._setTrigger(AvatarController.weaponFireHash, true);
	}

	public override bool IsAnimationUsePlaying()
	{
		return this.timeUseAnimationPlaying > 0f;
	}

	public override void StartAnimationUse()
	{
		this._setTrigger(AvatarController.useItemHash, true);
	}

	public override bool IsAnimationSpecialAttackPlaying()
	{
		return this.bSpecialAttackPlaying;
	}

	public override void StartAnimationSpecialAttack(bool _b, int _animType)
	{
		this.idleTime = 0f;
		this.bSpecialAttackPlaying = _b;
		if (_b)
		{
			this._resetTrigger(AvatarController.weaponFireHash, true);
			this._resetTrigger(AvatarController.weaponPreFireCancelHash, true);
			this._setTrigger(AvatarController.weaponPreFireHash, true);
		}
	}

	public override bool IsAnimationSpecialAttack2Playing()
	{
		return this.timeSpecialAttack2Playing > 0f;
	}

	public override void StartAnimationSpecialAttack2()
	{
		this.idleTime = 0f;
		this.timeSpecialAttack2Playing = 0.3f;
		this._resetTrigger(AvatarController.weaponFireHash, true);
		this._resetTrigger(AvatarController.weaponPreFireHash, true);
		this._setTrigger(AvatarController.weaponPreFireCancelHash, true);
	}

	public override bool IsAnimationHarvestingPlaying()
	{
		return this.timeHarestingAnimationPlaying > 0f;
	}

	public override void StartAnimationHarvesting(float _length, bool _weaponFireTrigger)
	{
		this.timeHarestingAnimationPlaying = _length;
		this._setBool(AvatarController.harvestingHash, true, true);
		if (_weaponFireTrigger)
		{
			this._setTrigger(AvatarController.weaponFireHash, true);
		}
	}

	public override void SetDrunk(float _numBeers)
	{
		int count = this.bodyAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			BodyAnimator bodyAnimator = this.bodyAnimators[i];
			if (bodyAnimator.Animator)
			{
				bodyAnimator.SetDrunk(_numBeers);
			}
		}
	}

	public override void SetVehicleAnimation(int _animHash, int _pose)
	{
		int count = this.bodyAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			Animator animator = this.bodyAnimators[i].Animator;
			if (animator)
			{
				animator.SetInteger(_animHash, _pose);
			}
		}
	}

	public override void SetAiming(bool _bEnable)
	{
		this.idleTime = 0f;
		this._setBool(AvatarController.isAimingHash, _bEnable, true);
	}

	public override void SetCrouching(bool _bEnable)
	{
		this.idleTime = 0f;
		this._setBool(AvatarController.isCrouchingHash, _bEnable, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void avatarVisibilityChanged(BodyAnimator _body, bool _bVisible)
	{
		_body.State = (_bVisible ? BodyAnimator.EnumState.Visible : BodyAnimator.EnumState.Disabled);
	}

	public override void SetVisible(bool _b)
	{
		if (this.visible != _b)
		{
			int count = this.bodyAnimators.Count;
			for (int i = 0; i < count; i++)
			{
				BodyAnimator bodyAnimator = this.bodyAnimators[i];
				this.avatarVisibilityChanged(bodyAnimator, _b);
			}
			Transform holdingItemTransform = this.entity.inventory.GetHoldingItemTransform();
			if (holdingItemTransform != null)
			{
				MeshRenderer[] componentsInChildren = holdingItemTransform.GetComponentsInChildren<MeshRenderer>();
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].enabled = true;
				}
			}
			this.visible = _b;
		}
	}

	public override void SetRagdollEnabled(bool _b)
	{
		int count = this.bodyAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			this.bodyAnimators[i].RagdollActive = _b;
		}
	}

	public void SetStressLevel(float stress)
	{
		this._setFloat(AvatarController.stressLevel, stress, true);
	}

	public override void StartAnimationReloading()
	{
		this.idleTime = 0f;
		float value = EffectManager.GetValue(PassiveEffects.ReloadSpeedMultiplier, this.entity.inventory.holdingItemItemValue, 1f, this.entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		int count = this.bodyAnimators.Count;
		bool flag = this.entity as EntityPlayerLocal != null && (this.entity as EntityPlayerLocal).emodel.IsFPV;
		this._setBool(AvatarController.isFPVHash, flag, true);
		this._setBool(AvatarController.reloadHash, true, true);
		this._setFloat(AvatarController.reloadSpeedHash, value, true);
	}

	public override void StartAnimationJump(AnimJumpMode jumpMode)
	{
		this.idleTime = 0f;
		if (jumpMode == AnimJumpMode.Start)
		{
			this._setTrigger(AvatarController.jumpTriggerHash, true);
			this._setBool(AvatarController.inAirHash, true, true);
			return;
		}
		if (jumpMode != AnimJumpMode.Land)
		{
			return;
		}
		this._setTrigger(AvatarController.jumpLandHash, true);
		this._setInt(AvatarController.jumpLandResponseHash, 0, true);
		this._setBool(AvatarController.inAirHash, false, true);
	}

	public override void SetSwim(bool _enable)
	{
		int num = -1;
		if (!_enable)
		{
			num = this.entity.GetWalkType();
		}
		this.SetWalkType(num, true);
	}

	public override void StartAnimationFiring()
	{
		this.StartAnimationAttack();
	}

	public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
	{
		this.idleTime = 0f;
		this._setInt(AvatarController.movementStateHash, _movementState, true);
		this._setInt(AvatarController.hitDirectionHash, _dir, true);
		this._setInt(AvatarController.hitDamageHash, _hitDamage, true);
		this._setInt(AvatarController.hitBodyPartHash, (int)_bodyPart, true);
		this._setFloat(AvatarController.hitRandomValueHash, _random, true);
		this._setBool(AvatarController.isCriticalHash, _criticalHit, true);
		this._setTrigger(AvatarController.hitTriggerHash, true);
	}

	public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
		this.idleTime = 0f;
		int count = this.bodyAnimators.Count;
		for (int i = 0; i < count; i++)
		{
			this.bodyAnimators[i].StartDeathAnimation(_bodyPart, _movementState, random);
		}
	}

	public override void SetInRightHand(Transform _transform)
	{
		this.idleTime = 0f;
		if (_transform != null)
		{
			Quaternion quaternion = ((this.heldItemTransform != null) ? this.heldItemTransform.localRotation : Quaternion.identity);
			_transform.SetParent(this.GetRightHandTransform(), false);
			if ((!this.entity.emodel.IsFPV || this.entity.isEntityRemote) && this.entity.inventory != null && this.entity.inventory.holdingItem != null)
			{
				AnimationGunjointOffsetData.AnimationGunjointOffsets animationGunjointOffsets = AnimationGunjointOffsetData.AnimationGunjointOffset[this.entity.inventory.holdingItem.HoldType.Value];
				_transform.localPosition = animationGunjointOffsets.position;
				_transform.localRotation = Quaternion.Euler(animationGunjointOffsets.rotation);
			}
			else
			{
				_transform.localPosition = Vector3.zero;
				_transform.localRotation = quaternion;
			}
		}
		this.heldItemTransform = _transform;
		this.heldItemAnimator = ((_transform != null) ? _transform.GetComponent<Animator>() : null);
		if (this.heldItemAnimator != null)
		{
			this.heldItemAnimator.logWarnings = false;
			this.heldItemAnimator.runtimeAnimatorController = (this.entity.emodel.IsFPV ? GameManager.Instance.FirstPersonWeaponAnimatorController : GameManager.Instance.ThirdPersonWeaponAnimatorController);
		}
	}

	public override Transform GetRightHandTransform()
	{
		if (this.primaryBody == null)
		{
			return null;
		}
		return this.primaryBody.Parts.RightHandT;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		float deltaTime = Time.deltaTime;
		if (this.animationToDodgeTime > 0f)
		{
			this.animationToDodgeTime -= deltaTime;
		}
		if (this.timeUseAnimationPlaying > 0f)
		{
			this.timeUseAnimationPlaying -= deltaTime;
		}
		if (this.timeHarestingAnimationPlaying > 0f)
		{
			this.timeHarestingAnimationPlaying -= deltaTime;
		}
		if (this.timeSpecialAttack2Playing > 0f)
		{
			this.timeSpecialAttack2Playing -= deltaTime;
		}
		if (!this.IsAnimationUsePlaying())
		{
			int value = this.entity.inventory.holdingItem.HoldType.Value;
			this._setInt(AvatarController.weaponHoldTypeHash, value, true);
			int carry = AnimationDelayData.AnimationDelay[value].Carry;
			this._setInt(AvatarController.weaponCarryHash, carry, true);
		}
		float speedForward = this.entity.speedForward;
		float speedStrafe = this.entity.speedStrafe;
		float x = this.entity.rotation.x;
		bool flag = this.entity.IsDead();
		bool flag2 = base.IsMoving(speedForward, speedStrafe);
		if (flag2)
		{
			this.idleTime = 0f;
		}
		for (int i = 0; i < this.bodyAnimators.Count; i++)
		{
			this.bodyAnimators[i].Update();
		}
		float num = speedStrafe;
		if (num >= 1234f)
		{
			num = 0f;
		}
		this._setFloat(AvatarController.forwardHash, speedForward, false);
		this._setFloat(AvatarController.strafeHash, num, false);
		this._setBool(AvatarController.isMovingHash, flag2, false);
		this._setFloat(AvatarController.rotationPitchHash, x, false);
		if (!flag)
		{
			if (speedStrafe >= 1234f)
			{
				this._setInt(AvatarController.movementStateHash, 4, true);
			}
			else
			{
				float num2 = speedForward * speedForward + speedStrafe * speedStrafe;
				this._setInt(AvatarController.movementStateHash, (num2 > base.Entity.moveSpeedAggro * base.Entity.moveSpeedAggro) ? 3 : ((num2 > base.Entity.moveSpeed * base.Entity.moveSpeed) ? 2 : ((num2 > 0.001f) ? 1 : 0)), true);
			}
		}
		float num3 = this.idleTime - this.idleTimeSent;
		if (num3 * num3 > 0.25f)
		{
			this.idleTimeSent = this.idleTime;
			this._setFloat(AvatarController.idleTimeHash, this.idleTime, false);
		}
		this.idleTime += deltaTime;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if (base.Entity.inventory.holdingItem.Actions[0] != null)
		{
			base.Entity.inventory.holdingItem.Actions[0].UpdateNozzleParticlesPosAndRot(base.Entity.inventory.holdingItemData.actionData[0]);
		}
		if (base.Entity.inventory.holdingItem.Actions[1] != null)
		{
			base.Entity.inventory.holdingItem.Actions[1].UpdateNozzleParticlesPosAndRot(base.Entity.inventory.holdingItemData.actionData[1]);
		}
	}

	public override void NotifyAnimatorMove(Animator anim)
	{
	}

	public override Animator GetAnimator()
	{
		return this.primaryBody.Animator;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static bool animatorIsValid(Animator animator)
	{
		return animator && animator.enabled && animator.gameObject.activeInHierarchy;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setTrigger(int _propertyHash, bool _netsync = true)
	{
		this.changed = false;
		for (int i = 0; i < this.bodyAnimators.Count; i++)
		{
			Animator animator = this.bodyAnimators[i].Animator;
			if (AvatarMultiBodyController.animatorIsValid(animator) && !animator.GetBool(_propertyHash))
			{
				animator.SetTrigger(_propertyHash);
				this.changed = true;
			}
		}
		if (AvatarMultiBodyController.animatorIsValid(this.heldItemAnimator) && !this.heldItemAnimator.GetBool(_propertyHash))
		{
			this.heldItemAnimator.SetTrigger(_propertyHash);
			this.changed = true;
		}
		if (!this.entity.isEntityRemote && this.changed && _netsync)
		{
			this.changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Trigger, true));
		}
		if (this.changed)
		{
			this.OnTrigger(_propertyHash);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _resetTrigger(int _propertyHash, bool _netsync = true)
	{
		this.changed = false;
		for (int i = 0; i < this.bodyAnimators.Count; i++)
		{
			Animator animator = this.bodyAnimators[i].Animator;
			if (animator && animator.GetBool(_propertyHash))
			{
				animator.ResetTrigger(_propertyHash);
				this.changed = true;
			}
		}
		if (this.heldItemAnimator && this.heldItemAnimator.gameObject.activeInHierarchy && this.heldItemAnimator.GetBool(_propertyHash))
		{
			this.heldItemAnimator.ResetTrigger(_propertyHash);
			this.changed = true;
		}
		if (!this.entity.isEntityRemote && this.changed && _netsync)
		{
			this.changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Trigger, false));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setFloat(int _propertyHash, float _value, bool _netsync = true)
	{
		this.changed = false;
		for (int i = 0; i < this.bodyAnimators.Count; i++)
		{
			Animator animator = this.bodyAnimators[i].Animator;
			if (animator)
			{
				float num = animator.GetFloat(_propertyHash) - _value;
				if (num * num > 1.0000001E-06f)
				{
					animator.SetFloat(_propertyHash, _value);
					this.changed = true;
				}
			}
		}
		if (this.heldItemAnimator && this.heldItemAnimator.gameObject.activeInHierarchy && this.heldItemAnimator.GetFloat(_propertyHash) != _value)
		{
			this.heldItemAnimator.SetFloat(_propertyHash, _value);
			this.changed = true;
		}
		if (!this.entity.isEntityRemote && this.changed && _netsync)
		{
			this.changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Float, _value));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setBool(int _propertyHash, bool _value, bool _netsync = true)
	{
		this.changed = false;
		for (int i = 0; i < this.bodyAnimators.Count; i++)
		{
			Animator animator = this.bodyAnimators[i].Animator;
			if (animator && animator.GetBool(_propertyHash) != _value)
			{
				animator.SetBool(_propertyHash, _value);
				this.changed = true;
			}
		}
		if (this.heldItemAnimator && this.heldItemAnimator.gameObject.activeInHierarchy && this.heldItemAnimator.GetBool(_propertyHash) != _value)
		{
			this.heldItemAnimator.SetBool(_propertyHash, _value);
			this.changed = true;
		}
		if (!this.entity.isEntityRemote && this.changed && _propertyHash != AvatarController.isFPVHash && _netsync)
		{
			this.changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Bool, _value));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setInt(int _propertyHash, int _value, bool _netsync = true)
	{
		this.changed = false;
		for (int i = 0; i < this.bodyAnimators.Count; i++)
		{
			Animator animator = this.bodyAnimators[i].Animator;
			if (animator && animator.GetInteger(_propertyHash) != _value)
			{
				animator.SetInteger(_propertyHash, _value);
				this.changed = true;
			}
		}
		if (this.heldItemAnimator && this.heldItemAnimator.gameObject.activeInHierarchy && this.heldItemAnimator.GetInteger(_propertyHash) != _value)
		{
			this.heldItemAnimator.SetInteger(_propertyHash, _value);
			this.changed = true;
		}
		if (!this.entity.isEntityRemote && this.changed && _netsync)
		{
			this.changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Int, _value));
		}
	}

	public override bool TryGetTrigger(int _propertyHash, out bool _value)
	{
		_value = false;
		for (int i = 0; i < this.bodyAnimators.Count; i++)
		{
			Animator animator = this.bodyAnimators[i].Animator;
			if (animator)
			{
				_value |= animator.GetBool(_propertyHash);
				if (_value)
				{
					return true;
				}
			}
		}
		if (this.heldItemAnimator && this.heldItemAnimator.gameObject.activeInHierarchy)
		{
			_value |= this.heldItemAnimator.GetBool(_propertyHash);
		}
		return true;
	}

	public override bool TryGetFloat(int _propertyHash, out float _value)
	{
		_value = float.NaN;
		for (int i = 0; i < this.bodyAnimators.Count; i++)
		{
			Animator animator = this.bodyAnimators[i].Animator;
			if (animator)
			{
				_value = animator.GetFloat(_propertyHash);
				if (_value != float.NaN)
				{
					return true;
				}
			}
		}
		if (this.heldItemAnimator && this.heldItemAnimator.gameObject.activeInHierarchy)
		{
			_value = this.heldItemAnimator.GetFloat(_propertyHash);
		}
		return _value != float.NaN;
	}

	public override bool TryGetBool(int _propertyHash, out bool _value)
	{
		_value = false;
		for (int i = 0; i < this.bodyAnimators.Count; i++)
		{
			Animator animator = this.bodyAnimators[i].Animator;
			if (animator)
			{
				_value |= animator.GetBool(_propertyHash);
				if (_value)
				{
					return true;
				}
			}
		}
		if (this.heldItemAnimator && this.heldItemAnimator.gameObject.activeInHierarchy)
		{
			_value |= this.heldItemAnimator.GetBool(_propertyHash);
		}
		return true;
	}

	public override bool TryGetInt(int _propertyHash, out int _value)
	{
		_value = int.MinValue;
		for (int i = 0; i < this.bodyAnimators.Count; i++)
		{
			Animator animator = this.bodyAnimators[i].Animator;
			if (animator)
			{
				_value = animator.GetInteger(_propertyHash);
				if (_value != -2147483648)
				{
					return true;
				}
			}
		}
		if (this.heldItemAnimator && this.heldItemAnimator.gameObject.activeInHierarchy)
		{
			_value = this.heldItemAnimator.GetInteger(_propertyHash);
		}
		return _value != int.MinValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AvatarMultiBodyController()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<BodyAnimator> bodyAnimators = new List<BodyAnimator>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BodyAnimator primaryBody;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform heldItemTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator heldItemAnimator;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool visible = true;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float animationToDodgeTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeUseAnimationPlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeHarestingAnimationPlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bSpecialAttackPlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeSpecialAttack2Playing;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float idleTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float idleTimeSent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float reviveTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cReviveAnimLength = 4.5f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<int, AnimParamData> FullSyncAnimationParameters = new Dictionary<int, AnimParamData>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool changed;
}
