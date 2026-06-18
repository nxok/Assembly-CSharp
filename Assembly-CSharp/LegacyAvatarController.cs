using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class LegacyAvatarController : AvatarController
{
	public Transform HeldItemTransform
	{
		get
		{
			return this.rightHandItemTransform;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		if (this.entity is EntityPlayerLocal)
		{
			this.modelTransform = base.transform.Find("Camera");
		}
		else
		{
			this.modelTransform = EModelBase.FindModel(base.transform);
		}
		this.assignStates();
	}

	public override Transform GetActiveModelRoot()
	{
		return this.modelTransform;
	}

	public override void SetInRightHand(Transform _transform)
	{
		this.idleTime = 0f;
		if (_transform != null)
		{
			_transform.SetParent(this.rightHand, false);
		}
		this.rightHandItemTransform = _transform;
		this.rightHandAnimator = ((_transform != null) ? _transform.GetComponent<Animator>() : null);
		if (this.rightHandAnimator != null)
		{
			this.rightHandAnimator.logWarnings = false;
			this.rightHandAnimator.runtimeAnimatorController = GameManager.Instance.ThirdPersonWeaponAnimatorController;
		}
		if (this.rightHandItemTransform != null)
		{
			Utils.SetLayerRecursively(this.rightHandItemTransform.gameObject, 0);
		}
	}

	public override Transform GetRightHandTransform()
	{
		return this.rightHandItemTransform;
	}

	public override bool IsAnimationAttackPlaying()
	{
		return this.timeAttackAnimationPlaying > 0f;
	}

	public override void StartAnimationAttack()
	{
		this.idleTime = 0f;
		this.isAttackImpact = false;
		this.timeAttackAnimationPlaying = 0.3f;
		if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		this._setTrigger(AvatarController.weaponFireHash, true);
	}

	public override void SetAttackImpact()
	{
		if (!this.isAttackImpact)
		{
			this.isAttackImpact = true;
			this.timeAttackAnimationPlaying = 0.1f;
		}
	}

	public override bool IsAttackImpact()
	{
		return this.isAttackImpact;
	}

	public override bool IsAnimationUsePlaying()
	{
		return this.timeUseAnimationPlaying > 0f;
	}

	public override void StartAnimationUse()
	{
		this.idleTime = 0f;
		this.itemUseTicks = 3;
		this.timeUseAnimationPlaying = 0.3f;
		if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		this._setBool(AvatarController.itemUseHash, true, true);
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
		return this.timeSpecialAttack2Playing > 0.3f;
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

	public override void StartAnimationFiring()
	{
		this.idleTime = 0f;
		this.timeAttackAnimationPlaying = 0.3f;
		if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		this._setTrigger(AvatarController.weaponFireHash, true);
	}

	public override void StartAnimationReloading()
	{
		this.idleTime = 0f;
		if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		float value = EffectManager.GetValue(PassiveEffects.ReloadSpeedMultiplier, this.entity.inventory.holdingItemItemValue, 1f, this.entity, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		this._setBool(AvatarController.reloadHash, true, true);
		this._setFloat(AvatarController.reloadSpeedHash, value, true);
	}

	public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float random, float _duration)
	{
		this.InternalStartAnimationHit(_bodyPart, _dir, _hitDamage, _criticalHit, _movementState, random, _duration);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void InternalStartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float random, float _duration)
	{
		if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!base.CheckHit(_duration))
		{
			this.SetDataFloat(AvatarController.DataTypes.HitDuration, _duration, true);
			return;
		}
		this.idleTime = 0f;
		if (this.anim != null)
		{
			this.movementStateOverride = _movementState;
			this._setInt(AvatarController.movementStateHash, _movementState, true);
			this._setBool(AvatarController.isCriticalHash, _criticalHit, true);
			this._setInt(AvatarController.hitDirectionHash, _dir, true);
			this._setInt(AvatarController.hitDamageHash, _hitDamage, true);
			this._setFloat(AvatarController.hitRandomValueHash, random, true);
			this._setInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb(), true);
			this._setTrigger(AvatarController.hitTriggerHash, true);
			this.SetDataFloat(AvatarController.DataTypes.HitDuration, _duration, true);
		}
	}

	public override void SetVisible(bool _b)
	{
		this.m_bVisible = _b;
		Transform meshTransform = this.GetMeshTransform();
		if (meshTransform != null && meshTransform.gameObject.activeSelf != _b)
		{
			meshTransform.gameObject.SetActive(_b);
			if (_b)
			{
				this.SwitchModelAndView(this.modelName, this.bFPV, this.bMale);
			}
		}
	}

	public override void SetVehicleAnimation(int _animHash, int _pose)
	{
		if (this.anim)
		{
			this._setInt(_animHash, _pose, true);
		}
	}

	public override void SetAiming(bool _bEnable)
	{
		this.idleTime = 0f;
		if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim != null)
		{
			this._setBool(AvatarController.isAimingHash, _bEnable, true);
		}
	}

	public override void SetCrouching(bool _bEnable)
	{
		this.idleTime = 0f;
		if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim != null)
		{
			this._setBool(AvatarController.isCrouchingHash, _bEnable, true);
		}
	}

	public override bool IsAnimationDigRunning()
	{
		return AvatarController.digHash == this.baseStateInfo.tagHash;
	}

	public override void StartAnimationJumping()
	{
		this.idleTime = 0f;
		if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim != null)
		{
			this._setBool(AvatarController.jumpHash, true, true);
		}
	}

	public override void StartAnimationJump(AnimJumpMode jumpMode)
	{
		this.idleTime = 0f;
		if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		this.isJumpStarted = true;
		if (this.anim != null)
		{
			if (jumpMode == AnimJumpMode.Start)
			{
				this._setTrigger(AvatarController.jumpStartHash, true);
				this._setBool(AvatarController.inAirHash, true, true);
				return;
			}
			this._setTrigger(AvatarController.jumpLandHash, true);
			this._setInt(AvatarController.jumpLandResponseHash, 0, true);
			this._setBool(AvatarController.inAirHash, false, true);
		}
	}

	public override bool IsAnimationJumpRunning()
	{
		return this.isJumpStarted || AvatarController.jumpHash == this.baseStateInfo.tagHash;
	}

	public override bool IsAnimationWithMotionRunning()
	{
		int tagHash = this.baseStateInfo.tagHash;
		return tagHash == AvatarController.jumpHash || tagHash == AvatarController.moveHash;
	}

	public override void SetSwim(bool _enable)
	{
		int num = -1;
		if (!_enable)
		{
			num = this.entity.GetWalkType();
		}
		else
		{
			this._setFloat(AvatarController.swimSelectHash, this.entity.rand.RandomFloat, true);
		}
		this.SetWalkType(num, true);
	}

	public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
		this.idleTime = 0f;
		this.isInDeathAnim = true;
		this.didDeathTransition = false;
		if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim != null)
		{
			this.movementStateOverride = _movementState;
			this._setInt(AvatarController.movementStateHash, _movementState, true);
			this._setBool(AvatarController.isAliveHash, false, true);
			this._setInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb(), true);
			this._setFloat(AvatarController.hitRandomValueHash, random, true);
			this.SetFallAndGround(false, this.entity.onGround);
		}
	}

	public override void SetRagdollEnabled(bool _b)
	{
		this.bIsRagdoll = _b;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateLayerStateInfo()
	{
		if (this.anim != null)
		{
			this.baseStateInfo = this.anim.GetCurrentAnimatorStateInfo(0);
			if (this.anim.layerCount > 1)
			{
				this.currentWeaponHoldLayer = this.anim.GetCurrentAnimatorStateInfo(1);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateSpineRotation()
	{
		if (this.modelTransform.parent != null && !this.bIsRagdoll && !this.entity.IsDead())
		{
			if (this.bFPV)
			{
				this.spine3.transform.localEulerAngles = new Vector3(this.spine1.transform.localEulerAngles.x, this.spine1.transform.localEulerAngles.y, this.spine1.transform.localEulerAngles.z + 1f * this.entity.rotation.x);
				return;
			}
			float num = 1f * this.entity.rotation.x / 3f;
			if (Time.timeScale > 0.001f)
			{
				this.spine1.transform.localEulerAngles = new Vector3(this.spine1.transform.localEulerAngles.x, this.spine1.transform.localEulerAngles.y, this.spine1.transform.localEulerAngles.z + num);
				this.spine2.transform.localEulerAngles = new Vector3(this.spine2.transform.localEulerAngles.x, this.spine2.transform.localEulerAngles.y, this.spine2.transform.localEulerAngles.z + num);
				this.spine3.transform.localEulerAngles = new Vector3(this.spine3.transform.localEulerAngles.x, this.spine3.transform.localEulerAngles.y, this.spine3.transform.localEulerAngles.z + num);
				return;
			}
			this.spine1.transform.localEulerAngles = new Vector3(this.spine1.transform.localEulerAngles.x, this.spine1.transform.localEulerAngles.y, num);
			this.spine2.transform.localEulerAngles = new Vector3(this.spine2.transform.localEulerAngles.x, this.spine2.transform.localEulerAngles.y, num);
			this.spine3.transform.localEulerAngles = new Vector3(this.spine3.transform.localEulerAngles.x, this.spine3.transform.localEulerAngles.y, num);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if (this.entity == null || this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim == null || !this.anim.enabled)
		{
			return;
		}
		this.updateLayerStateInfo();
		this.updateSpineRotation();
		if (this.entity.inventory.holdingItem.Actions[0] != null)
		{
			this.entity.inventory.holdingItem.Actions[0].UpdateNozzleParticlesPosAndRot(this.entity.inventory.holdingItemData.actionData[0]);
		}
		if (this.entity.inventory.holdingItem.Actions[1] != null)
		{
			this.entity.inventory.holdingItem.Actions[1].UpdateNozzleParticlesPosAndRot(this.entity.inventory.holdingItemData.actionData[1]);
		}
		int fullPathHash = this.baseStateInfo.fullPathHash;
		bool flag = this.anim.IsInTransition(0);
		if (!flag)
		{
			this.isJumpStarted = false;
			if (fullPathHash == this.jumpState || fullPathHash == this.fpvJumpState)
			{
				this._setBool(AvatarController.jumpHash, false, true);
			}
			if (this.anim.GetBool(AvatarController.reloadHash) && this.reloadStates.Contains(this.currentWeaponHoldLayer.fullPathHash))
			{
				this._setBool(AvatarController.reloadHash, false, true);
			}
		}
		if (this.anim.GetBool(AvatarController.itemUseHash))
		{
			int num = this.itemUseTicks - 1;
			this.itemUseTicks = num;
			if (num <= 0)
			{
				this._setBool(AvatarController.itemUseHash, false, true);
			}
		}
		if (this.isInDeathAnim)
		{
			if ((this.baseStateInfo.tagHash == AvatarController.deathHash || this.deathStates.Contains(fullPathHash)) && this.baseStateInfo.normalizedTime >= 1f && !flag)
			{
				this.isInDeathAnim = false;
				if (this.entity.HasDeathAnim)
				{
					EModelBase emodel = this.entity.emodel;
					DamageResponse damageResponse = DamageResponse.New(true);
					emodel.DoRagdoll(in damageResponse, EModelBase.RagdollMode.Default, 999999f);
				}
			}
			if (this.entity.HasDeathAnim && this.entity.RootMotion && this.entity.isCollidedHorizontally)
			{
				this.isInDeathAnim = false;
				EModelBase emodel2 = this.entity.emodel;
				DamageResponse damageResponse = DamageResponse.New(true);
				emodel2.DoRagdoll(in damageResponse, EModelBase.RagdollMode.Default, 999999f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void setLayerWeights()
	{
		if (this.anim == null || this.anim.layerCount <= 1)
		{
			return;
		}
		if (this.entity.IsDead())
		{
			this.anim.SetLayerWeight(1, 1f);
			if (!this.bFPV)
			{
				this.anim.SetLayerWeight(2, 1f);
				return;
			}
		}
		else
		{
			if (this.entity.inventory.holdingItem.HoldType == 0)
			{
				this.anim.SetLayerWeight(1, 0f);
			}
			else
			{
				this.anim.SetLayerWeight(1, 1f);
			}
			if (!this.bFPV)
			{
				this.anim.SetLayerWeight(2, 1f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		if (this.timeAttackAnimationPlaying > 0f)
		{
			this.timeAttackAnimationPlaying -= Time.deltaTime;
			if (this.timeAttackAnimationPlaying <= 0f)
			{
				this.isAttackImpact = true;
			}
		}
		if (this.timeUseAnimationPlaying > 0f)
		{
			this.timeUseAnimationPlaying -= Time.deltaTime;
		}
		if (this.timeHarestingAnimationPlaying > 0f)
		{
			this.timeHarestingAnimationPlaying -= Time.deltaTime;
			if (this.timeHarestingAnimationPlaying <= 0f && this.anim != null)
			{
				this._setBool(AvatarController.harvestingHash, false, true);
			}
		}
		if (this.timeSpecialAttack2Playing > 0f)
		{
			this.timeSpecialAttack2Playing -= Time.deltaTime;
		}
		if (!this.m_bVisible && (!this.entity || !this.entity.RootMotion || this.entity.isEntityRemote))
		{
			return;
		}
		if (this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim == null || !this.anim.avatar.isValid || !this.anim.enabled)
		{
			return;
		}
		this.updateLayerStateInfo();
		this.setLayerWeights();
		int value = this.entity.inventory.holdingItem.HoldType.Value;
		this._setInt(AvatarController.weaponHoldTypeHash, value, true);
		float speedForward = this.entity.speedForward;
		float speedStrafe = this.entity.speedStrafe;
		float num = speedStrafe;
		if (num >= 1234f)
		{
			num = 0f;
		}
		this._setFloat(AvatarController.forwardHash, speedForward, false);
		this._setFloat(AvatarController.strafeHash, num, false);
		if (!this.entity.IsDead())
		{
			if (this.movementStateOverride != -1)
			{
				this._setInt(AvatarController.movementStateHash, this.movementStateOverride, true);
				this.movementStateOverride = -1;
			}
			else if (speedStrafe >= 1234f)
			{
				this._setInt(AvatarController.movementStateHash, 4, true);
			}
			else
			{
				float num2 = speedForward * speedForward + num * num;
				this._setInt(AvatarController.movementStateHash, (num2 > this.entity.moveSpeedAggro * this.entity.moveSpeedAggro) ? 3 : ((num2 > this.entity.moveSpeed * this.entity.moveSpeed) ? 2 : ((num2 > 0.001f) ? 1 : 0)), true);
			}
		}
		if (base.IsMoving(speedForward, speedStrafe))
		{
			this.idleTime = 0f;
			this._setBool(AvatarController.isMovingHash, true, true);
		}
		else
		{
			this._setBool(AvatarController.isMovingHash, false, true);
		}
		if (this.useIdle)
		{
			float num3 = this.idleTime - this.idleTimeSent;
			if (num3 * num3 > 0.25f)
			{
				this.idleTimeSent = this.idleTime;
				this._setFloat(AvatarController.idleTimeHash, this.idleTime, false);
			}
			this.idleTime += Time.deltaTime;
		}
		float num4;
		this.TryGetFloat(AvatarController.rotationPitchHash, out num4);
		float num5 = Mathf.Lerp(num4, this.entity.rotation.x, Time.deltaTime * 12f);
		this._setFloat(AvatarController.rotationPitchHash, num5, false);
		float num6;
		this.TryGetFloat(AvatarController.yLookHash, out num6);
		base.UpdateFloat(AvatarController.yLookHash, Mathf.Lerp(num6, -base.Entity.rotation.x / 90f, Time.deltaTime * 12f), false);
	}

	public virtual Transform GetMeshTransform()
	{
		return this.bipedTransform;
	}

	public override void SetArchetypeStance(NPCInfo.StanceTypes stance)
	{
		if (this.anim != null && this.anim.avatar.isValid && this.anim.enabled)
		{
			this._setInt(AvatarController.archetypeStanceHash, (int)stance, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setTrigger(int _propertyHash, bool _netsync = true)
	{
		base._setTrigger(_propertyHash, _netsync);
		if (this.rightHandAnimator != null && this.rightHandAnimator.runtimeAnimatorController != null)
		{
			this.rightHandAnimator.SetTrigger(_propertyHash);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _resetTrigger(int _propertyHash, bool _netsync = true)
	{
		base._resetTrigger(_propertyHash, _netsync);
		if (this.rightHandAnimator != null && this.rightHandAnimator.runtimeAnimatorController != null && this.rightHandAnimator.GetBool(_propertyHash))
		{
			this.rightHandAnimator.ResetTrigger(_propertyHash);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setFloat(int _propertyHash, float _value, bool _netsync = true)
	{
		base._setFloat(_propertyHash, _value, _netsync);
		if (this.rightHandAnimator != null && this.rightHandAnimator.runtimeAnimatorController != null)
		{
			this.rightHandAnimator.SetFloat(_propertyHash, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setBool(int _propertyHash, bool _value, bool _netsync = true)
	{
		base._setBool(_propertyHash, _value, _netsync);
		if (this.rightHandAnimator != null && this.rightHandAnimator.runtimeAnimatorController != null)
		{
			this.rightHandAnimator.SetBool(_propertyHash, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setInt(int _propertyHash, int _value, bool _netsync = true)
	{
		base._setInt(_propertyHash, _value, _netsync);
		if (this.rightHandAnimator != null && this.rightHandAnimator.runtimeAnimatorController != null)
		{
			this.rightHandAnimator.SetInteger(_propertyHash, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public LegacyAvatarController()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cPitchUpdateSpeed = 12f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int jumpState;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int fpvJumpState;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isJumpStarted;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public HashSet<int> reloadStates = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public HashSet<int> deathStates = new HashSet<int>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_bVisible;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bFPV;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bMale;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo baseStateInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo currentWeaponHoldLayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo currentUpperBodyState;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float lastAbsMotionX;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float lastAbsMotionZ;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float lastAbsMotion;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform bipedTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform modelTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightHand;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform pelvis;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform spine;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform spine1;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform spine2;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform spine3;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform head;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform cameraNode;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeAttackAnimationPlaying;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isAttackImpact;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeUseAnimationPlaying;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeHarestingAnimationPlaying;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightHandItemTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Animator rightHandAnimator;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool bIsRagdoll;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool useIdle = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float idleTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float idleTimeSent;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int itemUseTicks;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int movementStateOverride = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string modelName;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isInDeathAnim;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool didDeathTransition;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bSpecialAttackPlaying;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeSpecialAttack2Playing;
}
