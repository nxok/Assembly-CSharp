using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class AvatarController : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		AvatarController.StaticInit();
		this.entity = base.GetComponent<EntityAlive>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void StaticInit()
	{
		if (AvatarController.initialized)
		{
			return;
		}
		AvatarController.initialized = true;
		AvatarController.hashNames = new Dictionary<int, string>();
		AvatarController.AssignAnimatorHash(ref AvatarController.attackHash, "Attack");
		AvatarController.AssignAnimatorHash(ref AvatarController.attackBlendHash, "AttackBlend");
		AvatarController.AssignAnimatorHash(ref AvatarController.attackStartHash, "AttackStart");
		AvatarController.AssignAnimatorHash(ref AvatarController.attackReadyHash, "AttackReady");
		AvatarController.AssignAnimatorHash(ref AvatarController.meleeAttackSpeedHash, "MeleeAttackSpeed");
		AvatarController.AssignAnimatorHash(ref AvatarController.deathHash, "Death");
		AvatarController.AssignAnimatorHash(ref AvatarController.digHash, "Dig");
		AvatarController.AssignAnimatorHash(ref AvatarController.hitStartHash, "HitStart");
		AvatarController.AssignAnimatorHash(ref AvatarController.hitHash, "Hit");
		AvatarController.AssignAnimatorHash(ref AvatarController.jumpHash, "Jump");
		AvatarController.AssignAnimatorHash(ref AvatarController.moveHash, "Move");
		AvatarController.AssignAnimatorHash(ref AvatarController.stunHash, "Stun");
		AvatarController.AssignAnimatorHash(ref AvatarController.readyToFireHash, "readyToFire");
		AvatarController.AssignAnimatorHash(ref AvatarController.beginCorpseEatHash, "BeginCorpseEat");
		AvatarController.AssignAnimatorHash(ref AvatarController.endCorpseEatHash, "EndCorpseEat");
		AvatarController.AssignAnimatorHash(ref AvatarController.forwardHash, "Forward");
		AvatarController.AssignAnimatorHash(ref AvatarController.hitBodyPartHash, "HitBodyPart");
		AvatarController.AssignAnimatorHash(ref AvatarController.idleTimeHash, "IdleTime");
		AvatarController.AssignAnimatorHash(ref AvatarController.aimPitchHash, "AimPitch");
		AvatarController.AssignAnimatorHash(ref AvatarController.aimYawHash, "AimYaw");
		AvatarController.AssignAnimatorHash(ref AvatarController.isAimingHash, "IsAiming");
		AvatarController.AssignAnimatorHash(ref AvatarController.itemUseHash, "ItemUse");
		AvatarController.AssignAnimatorHash(ref AvatarController.movementStateHash, "MovementState");
		AvatarController.AssignAnimatorHash(ref AvatarController.rotationPitchHash, "RotationPitch");
		AvatarController.AssignAnimatorHash(ref AvatarController.strafeHash, "Strafe");
		AvatarController.AssignAnimatorHash(ref AvatarController.swimSelectHash, "SwimSelect");
		AvatarController.AssignAnimatorHash(ref AvatarController.turnRateHash, "TurnRate");
		AvatarController.AssignAnimatorHash(ref AvatarController.walkTypeHash, "WalkType");
		AvatarController.AssignAnimatorHash(ref AvatarController.walkTypeBlendHash, "WalkTypeBlend");
		AvatarController.AssignAnimatorHash(ref AvatarController.weaponCarryHash, "WeaponCarry");
		AvatarController.AssignAnimatorHash(ref AvatarController.weaponHoldTypeHash, "WeaponHoldType");
		AvatarController.AssignAnimatorHash(ref AvatarController.isAliveHash, "IsAlive");
		AvatarController.AssignAnimatorHash(ref AvatarController.isDeadHash, "IsDead");
		AvatarController.AssignAnimatorHash(ref AvatarController.isFPVHash, "IsFPV");
		AvatarController.AssignAnimatorHash(ref AvatarController.isMovingHash, "IsMoving");
		AvatarController.AssignAnimatorHash(ref AvatarController.isSwimHash, "IsSwim");
		AvatarController.AssignAnimatorHash(ref AvatarController.attackTriggerHash, "AttackTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.deathTriggerHash, "DeathTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.hitTriggerHash, "HitTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.movementTriggerHash, "MovementTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.electrocuteTriggerHash, "ElectrocuteTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.painTriggerHash, "PainTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.itemHasChangedTriggerHash, "ItemHasChangedTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.itemThrownAwayTriggerHash, "ItemThrownAwayTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.dodgeBlendHash, "DodgeBlend");
		AvatarController.AssignAnimatorHash(ref AvatarController.dodgeTriggerHash, "DodgeTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.reactionTriggerHash, "ReactionTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.reactionTypeHash, "ReactionType");
		AvatarController.AssignAnimatorHash(ref AvatarController.sleeperPoseHash, "SleeperPose");
		AvatarController.AssignAnimatorHash(ref AvatarController.sleeperTriggerHash, "SleeperTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.jumpLandResponseHash, "JumpLandResponse");
		AvatarController.AssignAnimatorHash(ref AvatarController.forcedRootMotionHash, "ForcedRootMotion");
		AvatarController.AssignAnimatorHash(ref AvatarController.preventAttackHash, "PreventAttack");
		AvatarController.AssignAnimatorHash(ref AvatarController.canFallHash, "CanFall");
		AvatarController.AssignAnimatorHash(ref AvatarController.isOnGroundHash, "IsOnGround");
		AvatarController.AssignAnimatorHash(ref AvatarController.triggerAliveHash, "TriggerAlive");
		AvatarController.AssignAnimatorHash(ref AvatarController.bodyPartHitHash, "BodyPartHit");
		AvatarController.AssignAnimatorHash(ref AvatarController.hitDirectionHash, "HitDirection");
		AvatarController.AssignAnimatorHash(ref AvatarController.criticalHitHash, "CriticalHit");
		AvatarController.AssignAnimatorHash(ref AvatarController.hitDamageHash, "HitDamage");
		AvatarController.AssignAnimatorHash(ref AvatarController.randomHash, "Random");
		AvatarController.AssignAnimatorHash(ref AvatarController.jumpStartHash, "JumpStart");
		AvatarController.AssignAnimatorHash(ref AvatarController.jumpLandHash, "JumpLand");
		AvatarController.AssignAnimatorHash(ref AvatarController.isMaleHash, "IsMale");
		AvatarController.AssignAnimatorHash(ref AvatarController.specialAttack2Hash, "SpecialAttack2");
		AvatarController.AssignAnimatorHash(ref AvatarController.rageHash, "Rage");
		AvatarController.AssignAnimatorHash(ref AvatarController.stunTypeHash, "StunType");
		AvatarController.AssignAnimatorHash(ref AvatarController.stunBodyPartHash, "StunBodyPart");
		AvatarController.AssignAnimatorHash(ref AvatarController.isCriticalHash, "isCritical");
		AvatarController.AssignAnimatorHash(ref AvatarController.HitRandomValueHash, "HitRandomValue");
		AvatarController.AssignAnimatorHash(ref AvatarController.beginStunTriggerHash, "BeginStunTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.endStunTriggerHash, "EndStunTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.toCrawlerTriggerHash, "ToCrawlerTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.isElectrocutedHash, "IsElectrocuted");
		AvatarController.AssignAnimatorHash(ref AvatarController.isClimbingHash, "IsClimbing");
		AvatarController.AssignAnimatorHash(ref AvatarController.verticalSpeedHash, "VerticalSpeed");
		AvatarController.AssignAnimatorHash(ref AvatarController.reviveHash, "Revive");
		AvatarController.AssignAnimatorHash(ref AvatarController.harvestingHash, "Harvesting");
		AvatarController.AssignAnimatorHash(ref AvatarController.weaponFireHash, "WeaponFire");
		AvatarController.AssignAnimatorHash(ref AvatarController.weaponPreFireCancelHash, "WeaponPreFireCancel");
		AvatarController.AssignAnimatorHash(ref AvatarController.weaponPreFireHash, "WeaponPreFire");
		AvatarController.AssignAnimatorHash(ref AvatarController.weaponAmmoRemaining, "WeaponAmmoRemaining");
		AvatarController.AssignAnimatorHash(ref AvatarController.useItemHash, "UseItem");
		AvatarController.AssignAnimatorHash(ref AvatarController.itemActionIndexHash, "ItemActionIndex");
		AvatarController.AssignAnimatorHash(ref AvatarController.isCrouchingHash, "IsCrouching");
		AvatarController.AssignAnimatorHash(ref AvatarController.reloadHash, "Reload");
		AvatarController.AssignAnimatorHash(ref AvatarController.reloadSpeedHash, "ReloadSpeed");
		AvatarController.AssignAnimatorHash(ref AvatarController.jumpTriggerHash, "JumpTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.inAirHash, "InAir");
		AvatarController.AssignAnimatorHash(ref AvatarController.jumpLandHash, "JumpLand");
		AvatarController.AssignAnimatorHash(ref AvatarController.hitRandomValueHash, "HitRandomValue");
		AvatarController.AssignAnimatorHash(ref AvatarController.hitTriggerHash, "HitTrigger");
		AvatarController.AssignAnimatorHash(ref AvatarController.CoverModeHash, "CoverMode");
		AvatarController.AssignAnimatorHash(ref AvatarController.CoverTypeHash, "CoverType");
		AvatarController.AssignAnimatorHash(ref AvatarController.CoverEngageHash, "CoverEngage");
		AvatarController.AssignAnimatorHash(ref AvatarController.CoverHeightHash, "CoverHeight");
		AvatarController.AssignAnimatorHash(ref AvatarController.archetypeStanceHash, "ArchetypeStance");
		AvatarController.AssignAnimatorHash(ref AvatarController.yLookHash, "YLook");
		AvatarController.AssignAnimatorHash(ref AvatarController.vehiclePoseHash, "VehiclePose");
		AvatarController.AssignAnimatorHash(ref AvatarController.sleeperIdleBackHash, "SleeperIdleBack");
		AvatarController.AssignAnimatorHash(ref AvatarController.sleeperIdleSideLeftHash, "SleeperIdleSideLeft");
		AvatarController.AssignAnimatorHash(ref AvatarController.sleeperIdleSideRightHash, "SleeperIdleSideRight");
		AvatarController.AssignAnimatorHash(ref AvatarController.sleeperIdleSitHash, "SleeperIdleSit");
		AvatarController.AssignAnimatorHash(ref AvatarController.sleeperIdleStandHash, "SleeperIdleStand");
		AvatarController.AssignAnimatorHash(ref AvatarController.sleeperIdleStomachHash, "SleeperIdleStomach");
		AvatarController.AssignAnimatorHash(ref AvatarController.holsteredHash, "Holstered");
		AvatarController.AssignAnimatorHash(ref AvatarController.stressLevel, "ChickenStress");
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static void AssignAnimatorHash(ref int hash, string parameterName)
	{
		hash = Animator.StringToHash(parameterName);
		if (!AvatarController.hashNames.ContainsKey(hash))
		{
			AvatarController.hashNames.Add(hash, parameterName);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void assignStates()
	{
	}

	public virtual Animator GetAnimator()
	{
		return this.anim;
	}

	public void SetAnimator(Transform _animT)
	{
		this.SetAnimator(_animT.GetComponent<Animator>());
	}

	public void SetAnimator(Animator _anim)
	{
		if (this.anim == _anim)
		{
			return;
		}
		this.anim = _anim;
		if (this.anim)
		{
			this.anim.logWarnings = false;
			AnimatorControllerParameter[] parameters = this.anim.parameters;
			for (int i = 0; i < parameters.Length; i++)
			{
				if (parameters[i].nameHash == AvatarController.turnRateHash)
				{
					this.hasTurnRate = true;
				}
			}
		}
	}

	public EntityAlive Entity
	{
		get
		{
			return this.entity;
		}
	}

	public bool IsMoving(float forwardSpeed, float strafeSpeed)
	{
		return forwardSpeed * forwardSpeed + strafeSpeed * strafeSpeed > 0.0001f;
	}

	public virtual void NotifyAnimatorMove(Animator instigator)
	{
		this.entity.NotifyRootMotion(instigator);
	}

	public abstract Transform GetActiveModelRoot();

	public virtual Transform GetRightHandTransform()
	{
		return null;
	}

	public Texture2D GetTexture()
	{
		return null;
	}

	public virtual void ResetAnimations()
	{
		Animator animator = this.GetAnimator();
		if (animator)
		{
			animator.cullingMode = 0;
			animator.enabled = true;
		}
	}

	public virtual void SetMeleeAttackSpeed(float _speed)
	{
		this.UpdateFloat(AvatarController.meleeAttackSpeedHash, _speed, true);
	}

	public abstract bool IsAnimationAttackPlaying();

	public abstract void StartAnimationAttack();

	public virtual void SetInAir(bool inAir)
	{
	}

	public virtual void SetAttackImpact()
	{
	}

	public virtual bool IsAttackImpact()
	{
		return true;
	}

	public virtual bool IsAnimationWithMotionRunning()
	{
		return true;
	}

	public virtual AvatarController.ActionState GetActionState()
	{
		return AvatarController.ActionState.None;
	}

	public virtual bool IsActionActive()
	{
		return false;
	}

	public virtual void StartAction(int _animType)
	{
	}

	public virtual bool IsAnimationSpecialAttackPlaying()
	{
		return false;
	}

	public virtual void StartAnimationSpecialAttack(bool _b, int _animType)
	{
	}

	public virtual bool IsAnimationSpecialAttack2Playing()
	{
		return false;
	}

	public virtual void StartAnimationSpecialAttack2()
	{
	}

	public virtual bool IsAnimationRagingPlaying()
	{
		return false;
	}

	public virtual void StartAnimationRaging()
	{
	}

	public virtual void StartAnimationFiring()
	{
	}

	public virtual bool IsAnimationHitRunning()
	{
		return false;
	}

	public virtual void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CheckHit(float duration)
	{
		return this.hitWeight < 0.15f || duration > this.hitDuration || !this.IsAnimationHitRunning();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitHitDuration(float duration)
	{
		if (this.hitWeight > 0.15f)
		{
			float num = 0.2f;
			if (duration == 0f)
			{
				num = 0.1f;
			}
			if (this.hitWeightTarget > this.hitWeight)
			{
				this.hitWeightTarget += num;
				if (this.hitWeightTarget > this.hitWeightMax)
				{
					this.hitWeightTarget = this.hitWeightMax;
				}
			}
			this.hitWeight += num;
			if (this.hitWeight > this.hitWeightMax)
			{
				this.hitWeight = this.hitWeightMax;
			}
			return;
		}
		duration = Utils.FastMax(duration, 0.120000005f);
		this.hitDuration = duration;
		float num2 = Utils.FastMin(duration * 0.25f, 0.1f);
		this.hitDurationOut = duration - num2;
		this.hitWeightTarget = num2 / 0.1f;
		this.hitWeightTarget = Utils.FastClamp(this.hitWeightTarget, 0.2f, 0.8f);
		this.hitWeightDuration = num2 / Utils.FastMax(0.01f, this.hitWeightTarget - this.hitWeight);
		if (this.hitWeight == 0f)
		{
			this.anim.SetLayerWeight(this.hitLayerIndex, 0.01f);
		}
	}

	public virtual bool IsAnimationHarvestingPlaying()
	{
		return false;
	}

	public virtual void StartAnimationHarvesting(float _length, bool _weaponFireTrigger)
	{
	}

	public virtual bool IsAnimationDigRunning()
	{
		return false;
	}

	public virtual bool IsAttackProhibited()
	{
		return false;
	}

	public virtual void StartAnimationDodge(float _blend)
	{
	}

	public virtual bool IsAnimationToDodge()
	{
		return false;
	}

	public virtual void StartAnimationJumping()
	{
	}

	public virtual void StartAnimationJump(AnimJumpMode jumpMode)
	{
	}

	public virtual bool IsAnimationJumpRunning()
	{
		return false;
	}

	public virtual void SetSwim(bool _enable)
	{
	}

	public virtual float GetAnimationElectrocuteRemaining()
	{
		return this.electrocuteTime;
	}

	public virtual void StartAnimationElectrocute(float _duration)
	{
		this.electrocuteTime = _duration;
	}

	public virtual void Electrocute(bool enabled)
	{
	}

	public virtual void StartAnimationReloading()
	{
	}

	public void SetReloadBool(bool value)
	{
		this._setBool(AvatarController.reloadHash, value, true);
	}

	public void SetStress(float val)
	{
		this._setFloat(AvatarController.stressLevel, val, true);
	}

	public virtual void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
	}

	public virtual bool IsAnimationUsePlaying()
	{
		return false;
	}

	public virtual void StartAnimationUse()
	{
	}

	public virtual void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
	}

	public virtual void SetAiming(bool _bEnable)
	{
	}

	public virtual void SetAlive()
	{
		if (this.anim != null)
		{
			this._setBool(AvatarController.isAliveHash, true, true);
		}
	}

	public virtual void SetCrouching(bool _bEnable)
	{
	}

	public virtual void SetDrunk(float _numBeers)
	{
	}

	public virtual void SetInRightHand(Transform _transform)
	{
	}

	public virtual void SetLookPosition(Vector3 _pos)
	{
	}

	public virtual void SetVehicleAnimation(int _animHash, int _pose)
	{
	}

	public virtual int GetVehicleAnimation()
	{
		int num;
		if (this.TryGetInt(AvatarController.vehiclePoseHash, out num))
		{
			return num;
		}
		return -1;
	}

	public virtual AvatarController.CoverMode CurrentCoverMode()
	{
		return AvatarController.CoverMode.None;
	}

	public virtual AvatarController.CoverType CurrentCoverType()
	{
		return AvatarController.CoverType.Center;
	}

	public virtual AvatarController.CoverEngageType CurrentCoverEngageType()
	{
		return AvatarController.CoverEngageType.None;
	}

	public virtual void SetCoverMode(AvatarController.CoverMode _coverMode)
	{
	}

	public virtual void SetCoverType(AvatarController.CoverType _coverType)
	{
	}

	public virtual void SetCoverEngageType(AvatarController.CoverEngageType _engage)
	{
	}

	public virtual void SetCoverHeight(float coverHeight)
	{
	}

	public virtual void SetRagdollEnabled(bool _b)
	{
	}

	public virtual void SetWalkingSpeed(float _f)
	{
	}

	public virtual void SetWalkType(int _walkType, bool _trigger = false)
	{
		this._setInt(AvatarController.walkTypeHash, _walkType, true);
		if (_walkType >= 20)
		{
			this._setFloat(AvatarController.walkTypeBlendHash, 1f, true);
		}
		else if (_walkType > 0)
		{
			this._setFloat(AvatarController.walkTypeBlendHash, 0f, true);
		}
		if (_trigger)
		{
			this._setTrigger(AvatarController.movementTriggerHash, true);
		}
	}

	public virtual void SetHeadAngles(float _nick, float _yaw)
	{
	}

	public virtual void SetArmsAngles(float _rightArmAngle, float _leftArmAngle)
	{
	}

	public abstract void SetVisible(bool _b);

	public virtual void SetArchetypeStance(NPCInfo.StanceTypes stance)
	{
	}

	public virtual void TriggerReaction(int reaction)
	{
		if (this.anim != null)
		{
			this._setInt(AvatarController.reactionTypeHash, reaction, true);
			this._setTrigger(AvatarController.reactionTriggerHash, true);
		}
	}

	public virtual void TriggerSleeperPose(int pose, bool returningToSleep = false)
	{
		if (this.anim != null)
		{
			this._setInt(AvatarController.sleeperPoseHash, pose, true);
			this._setTrigger(AvatarController.sleeperTriggerHash, true);
		}
	}

	public virtual void RemoveLimb(BodyDamage _bodyDamage, bool restoreState)
	{
	}

	public virtual void DismemberLimb(BodyDamage _bodyDamage, bool restoreState)
	{
		if (_bodyDamage.bodyPartHit != EnumBodyPartHit.None)
		{
			this.RemoveLimb(_bodyDamage, restoreState);
		}
	}

	public virtual void TurnIntoCrawler()
	{
	}

	public virtual void BeginStun(EnumEntityStunType stun, EnumBodyPartHit _bodyPart, Utils.EnumHitDirection _hitDirection, bool _criticalHit, float random)
	{
	}

	public virtual void EndStun()
	{
	}

	public virtual bool IsAnimationStunRunning()
	{
		return false;
	}

	public virtual void StartEating()
	{
	}

	public virtual void StopEating()
	{
	}

	public virtual void PlayPlayerFPRevive()
	{
	}

	public virtual bool IsAnimationPlayerFPRevivePlaying()
	{
		return false;
	}

	public bool IsRootMotionForced()
	{
		return this.anim != null && this.anim.GetFloat(AvatarController.forcedRootMotionHash) > 0f;
	}

	public bool IsAttackPrevented()
	{
		return this.anim != null && this.anim.GetFloat(AvatarController.preventAttackHash) > 0f;
	}

	public virtual void SetFallAndGround(bool _canFall, bool _onGnd)
	{
		this._setBool(AvatarController.canFallHash, _canFall, false);
		this._setBool(AvatarController.isOnGroundHash, _onGnd, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void FixedUpdate()
	{
		if (this.hasTurnRate)
		{
			float y = this.entity.transform.eulerAngles.y;
			float num = Mathf.DeltaAngle(y, this.turnRateFacing) * 50f;
			if ((num > 5f && this.turnRate >= 0f) || (num < -5f && this.turnRate <= 0f))
			{
				float num2 = Utils.FastAbs(num) - Utils.FastAbs(this.turnRate);
				if (num2 > 0f)
				{
					this.turnRate = Utils.FastLerpUnclamped(this.turnRate, num, 0.2f);
				}
				else if (num2 < -50f)
				{
					this.turnRate = Utils.FastLerpUnclamped(this.turnRate, num, 0.05f);
				}
			}
			else
			{
				this.turnRate *= 0.92f;
				this.turnRate = Utils.FastMoveTowards(this.turnRate, 0f, 2f);
			}
			this.turnRateFacing = y;
			this._setFloat(AvatarController.turnRateHash, this.turnRate, false);
		}
		this.updateNetworkAnimData();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		float deltaTime = Time.deltaTime;
		if (this.electrocuteTime > 0f)
		{
			this.electrocuteTime -= deltaTime;
		}
		else if (this.electrocuteTime <= 0f)
		{
			this.Electrocute(false);
			this.electrocuteTime = 0f;
		}
		if (this.hitLayerIndex >= 0)
		{
			if (this.hitWeightTarget > 0f && this.hitWeight == this.hitWeightTarget)
			{
				if (this.hitDuration > 999f)
				{
					if (!this.IsAnimationHitRunning() || this.entity.IsDead() || this.entity.emodel.IsRagdollActive)
					{
						this.hitWeightDuration = 0.4f;
						this.hitWeightTarget = 0f;
					}
				}
				else if (this.hitWeightTarget > 0.15f)
				{
					this.hitWeightDuration = (this.hitDurationOut + 0.2f) / (this.hitWeight - 0.15f);
					this.hitWeightTarget = 0.15f;
				}
				else
				{
					this.hitWeightDuration = 4f;
					this.hitWeightTarget = 0f;
				}
			}
			if (this.hitWeight != this.hitWeightTarget)
			{
				this.hitWeight = Mathf.MoveTowards(this.hitWeight, this.hitWeightTarget, deltaTime / this.hitWeightDuration);
				this.anim.SetLayerWeight(this.hitLayerIndex, this.hitWeight);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void processAnimParamData(List<AnimParamData> animationParameterData)
	{
		for (int i = 0; i < animationParameterData.Count; i++)
		{
			int nameHash = animationParameterData[i].NameHash;
			switch (animationParameterData[i].ValueType)
			{
			case AnimParamData.ValueTypes.Bool:
				this.UpdateBool(nameHash, animationParameterData[i].IntValue != 0, true);
				break;
			case AnimParamData.ValueTypes.Trigger:
				if (animationParameterData[i].IntValue != 0)
				{
					this.TriggerEvent(nameHash);
				}
				else
				{
					this.CancelEvent(nameHash);
				}
				break;
			case AnimParamData.ValueTypes.Float:
				this.UpdateFloat(nameHash, animationParameterData[i].FloatValue, true);
				break;
			case AnimParamData.ValueTypes.Int:
				this.UpdateInt(nameHash, animationParameterData[i].IntValue, true);
				break;
			case AnimParamData.ValueTypes.DataFloat:
				this.SetDataFloat((AvatarController.DataTypes)nameHash, animationParameterData[i].FloatValue, true);
				break;
			}
		}
	}

	public void TriggerEvent(string _property)
	{
		this._setTrigger(_property, true);
	}

	public void TriggerEvent(int _pid)
	{
		this._setTrigger(_pid, true);
	}

	public void CancelEvent(string _property)
	{
		this._resetTrigger(_property, true);
	}

	public void CancelEvent(int _pid)
	{
		this._resetTrigger(_pid, true);
	}

	public void UpdateFloat(string _property, float _value, bool _netsync = true)
	{
		this._setFloat(_property, _value, _netsync);
	}

	public void UpdateFloat(int _pid, float _value, bool _netsync = true)
	{
		this._setFloat(_pid, _value, _netsync);
	}

	public void UpdateBool(string _property, bool _value, bool _netsync = true)
	{
		this._setBool(_property, _value, _netsync);
	}

	public void UpdateBool(int _pid, bool _value, bool _netsync = true)
	{
		this._setBool(_pid, _value, _netsync);
	}

	public void UpdateInt(string _property, int _value, bool _netsync = true)
	{
		this._setInt(_property, _value, _netsync);
	}

	public void UpdateInt(int _pid, int _value, bool _netsync = true)
	{
		this._setInt(_pid, _value, _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void _setTrigger(string _property, bool _netsync = true)
	{
		this._setTrigger(Animator.StringToHash(_property), _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void _setTrigger(int _pid, bool _netsync = true)
	{
		if (this.anim != null)
		{
			this.anim.SetTrigger(_pid);
			if (!this.entity.isEntityRemote && _netsync)
			{
				this.changedAnimationParameters.Add(new AnimParamData(_pid, AnimParamData.ValueTypes.Trigger, true));
			}
			this.OnTrigger(_pid);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnTrigger(int _id)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void _resetTrigger(string _property, bool _netsync = true)
	{
		this._resetTrigger(Animator.StringToHash(_property), _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void _resetTrigger(int _propertyHash, bool _netsync = true)
	{
		if (this.anim != null && this.anim.gameObject.activeSelf && this.anim.GetBool(_propertyHash))
		{
			this.anim.ResetTrigger(_propertyHash);
			if (!this.entity.isEntityRemote && _netsync)
			{
				this.changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Trigger, false));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void _setFloat(string _property, float _value, bool _netsync = true)
	{
		this._setFloat(Animator.StringToHash(_property), _value, _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void _setBool(string _property, bool _value, bool _netsync = true)
	{
		int num = Animator.StringToHash(_property);
		this._setBool(num, _value, _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void _setInt(string _property, int _value, bool _netsync = true)
	{
		this._setInt(Animator.StringToHash(_property), _value, _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void _setFloat(int _propertyHash, float _value, bool _netSync = true)
	{
		if (this.anim)
		{
			if (!_netSync)
			{
				this.anim.SetFloat(_propertyHash, _value);
				return;
			}
			float num = this.anim.GetFloat(_propertyHash) - _value;
			if (num * num > 0.0001f)
			{
				this.anim.SetFloat(_propertyHash, _value);
				if (!this.entity.isEntityRemote && _netSync)
				{
					this.changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Float, _value));
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void _setBool(int _propertyHash, bool _value, bool _netsync = true)
	{
		if (this.anim != null && this.anim.GetBool(_propertyHash) != _value)
		{
			this.anim.SetBool(_propertyHash, _value);
			if (_propertyHash == AvatarController.isFPVHash)
			{
				return;
			}
			if (!this.entity.isEntityRemote && _netsync)
			{
				this.changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Bool, _value));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void _setInt(int _propertyHash, int _value, bool _netsync = true)
	{
		if (this.anim != null && this.anim.GetInteger(_propertyHash) != _value)
		{
			this.anim.SetInteger(_propertyHash, _value);
			if (!this.entity.isEntityRemote && _netsync)
			{
				this.changedAnimationParameters.Add(new AnimParamData(_propertyHash, AnimParamData.ValueTypes.Int, _value));
			}
		}
	}

	public virtual void SetDataFloat(AvatarController.DataTypes _type, float _value, bool _netsync = true)
	{
		if (_type == AvatarController.DataTypes.HitDuration)
		{
			this.InitHitDuration(_value);
		}
		if (!this.entity.isEntityRemote && _netsync)
		{
			this.changedAnimationParameters.Add(new AnimParamData((int)_type, AnimParamData.ValueTypes.DataFloat, _value));
		}
	}

	public virtual bool TryGetTrigger(string _property, out bool _value)
	{
		return this.TryGetTrigger(Animator.StringToHash(_property), out _value);
	}

	public virtual bool TryGetFloat(string _property, out float _value)
	{
		return this.TryGetFloat(Animator.StringToHash(_property), out _value);
	}

	public virtual bool TryGetBool(string _property, out bool _value)
	{
		return this.TryGetBool(Animator.StringToHash(_property), out _value);
	}

	public virtual bool TryGetInt(string _property, out int _value)
	{
		return this.TryGetInt(Animator.StringToHash(_property), out _value);
	}

	public virtual bool TryGetTrigger(int _propertyHash, out bool _value)
	{
		if (this.anim == null)
		{
			return _value = false;
		}
		_value = this.anim.GetBool(_propertyHash);
		return true;
	}

	public virtual bool TryGetFloat(int _propertyHash, out float _value)
	{
		if (this.anim == null)
		{
			_value = 0f;
			return false;
		}
		_value = this.anim.GetFloat(_propertyHash);
		return true;
	}

	public virtual bool TryGetBool(int _propertyHash, out bool _value)
	{
		if (this.anim == null)
		{
			return _value = false;
		}
		_value = this.anim.GetBool(_propertyHash);
		return true;
	}

	public virtual bool TryGetInt(int _propertyHash, out int _value)
	{
		if (this.anim == null)
		{
			_value = 0;
			return false;
		}
		_value = this.anim.GetInteger(_propertyHash);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void updateNetworkAnimData()
	{
		if (this.entity == null)
		{
			return;
		}
		if (this.entity.isEntityRemote)
		{
			if (this.queuedAnimParams.Count > 0)
			{
				do
				{
					this.processAnimParamData(this.queuedAnimParams[0]);
					this.queuedAnimParams.RemoveAt(0);
				}
				while (this.queuedAnimParams.Count > 10);
				return;
			}
		}
		else
		{
			foreach (List<AnimParamData> list in this.changedAnimationParameters.GetParameterLists())
			{
				if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAnimationData>().Setup(this.entity.entityId, list), false, -1, this.entity.entityId, this.entity.entityId, null, 192, false);
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageEntityAnimationData>().Setup(this.entity.entityId, list), false);
				}
			}
		}
	}

	public void SyncAnimParameters(int _toEntityId)
	{
		if (!this.anim)
		{
			return;
		}
		Dictionary<int, AnimParamData> dictionary = new Dictionary<int, AnimParamData>();
		foreach (AnimatorControllerParameter animatorControllerParameter in this.anim.parameters)
		{
			switch (animatorControllerParameter.type)
			{
			case 1:
			{
				float @float = this.anim.GetFloat(animatorControllerParameter.nameHash);
				dictionary[animatorControllerParameter.nameHash] = new AnimParamData(animatorControllerParameter.nameHash, AnimParamData.ValueTypes.Float, @float);
				break;
			}
			case 3:
			{
				int integer = this.anim.GetInteger(animatorControllerParameter.nameHash);
				dictionary[animatorControllerParameter.nameHash] = new AnimParamData(animatorControllerParameter.nameHash, AnimParamData.ValueTypes.Int, integer);
				break;
			}
			case 4:
			{
				bool @bool = this.anim.GetBool(animatorControllerParameter.nameHash);
				dictionary[animatorControllerParameter.nameHash] = new AnimParamData(animatorControllerParameter.nameHash, AnimParamData.ValueTypes.Bool, @bool);
				break;
			}
			}
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityAnimationData>().Setup(this.entity.entityId, dictionary), false, _toEntityId, -1, -1, null, 192, false);
	}

	public virtual string GetParameterName(int _nameHash)
	{
		foreach (AnimatorControllerParameter animatorControllerParameter in this.anim.parameters)
		{
			if (animatorControllerParameter.nameHash == _nameHash)
			{
				return animatorControllerParameter.name;
			}
		}
		return "?";
	}

	public void SetAnimParameters(List<AnimParamData> animationParameterData)
	{
		this.queuedAnimParams.Add(animationParameterData);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AvatarController()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cMinFloatChangeSquared = 0.0001f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static bool initialized;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int attackTag;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int deathHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int digHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int hitStartHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int hitHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int jumpHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int moveHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int stunHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int readyToFireHash;

	public static int attackHash;

	public static int attackBlendHash;

	public static int attackStartHash;

	public static int attackReadyHash;

	public static int meleeAttackSpeedHash;

	public static int beginCorpseEatHash;

	public static int endCorpseEatHash;

	public static int forwardHash;

	public static int hitBodyPartHash;

	public static int idleTimeHash;

	public static int aimPitchHash;

	public static int aimYawHash;

	public static int isAimingHash;

	public static int itemUseHash;

	public static int movementStateHash;

	public static int rotationPitchHash;

	public static int strafeHash;

	public static int swimSelectHash;

	public static int turnRateHash;

	public static int weaponCarryHash;

	public static int weaponHoldTypeHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int walkTypeHash;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static int walkTypeBlendHash;

	public static int isAliveHash;

	public static int isDeadHash;

	public static int isFPVHash;

	public static int isMovingHash;

	public static int isSwimHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int attackTriggerHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int deathTriggerHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int hitTriggerHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int movementTriggerHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int electrocuteTriggerHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int painTriggerHash;

	public static int itemHasChangedTriggerHash;

	public static int itemThrownAwayTriggerHash;

	public static int reloadHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int dodgeBlendHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int dodgeTriggerHash;

	public static int reactionTypeHash;

	public static int reactionTriggerHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int sleeperPoseHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int sleeperTriggerHash;

	public static int jumpLandResponseHash;

	public static int forcedRootMotionHash;

	public static int preventAttackHash;

	public static int canFallHash;

	public static int isOnGroundHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int triggerAliveHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int bodyPartHitHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int hitDirectionHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int hitDamageHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int criticalHitHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int randomHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int jumpStartHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int jumpLandHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int isMaleHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int specialAttack2Hash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int rageHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int stunTypeHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int stunBodyPartHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int isCriticalHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int HitRandomValueHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int beginStunTriggerHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int endStunTriggerHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int toCrawlerTriggerHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int isElectrocutedHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int isClimbingHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int verticalSpeedHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int reviveHash;

	public static int harvestingHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int weaponFireHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int weaponPreFireCancelHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int weaponPreFireHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int weaponAmmoRemaining;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int useItemHash;

	public static int itemActionIndexHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int isCrouchingHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int reloadSpeedHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int jumpTriggerHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int inAirHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int jumpLandTriggerHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int hitRandomValueHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int CoverModeHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int CoverTypeHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int CoverEngageHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int CoverHeightHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int sleeperIdleSitHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int sleeperIdleSideRightHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int sleeperIdleSideLeftHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int sleeperIdleBackHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int sleeperIdleStomachHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int sleeperIdleStandHash;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static int archetypeStanceHash;

	public static int yLookHash;

	public static int vehiclePoseHash;

	public static int holsteredHash;

	public static int stressLevel;

	public const int cSleeperPoseMove = -2;

	public const int cSleeperPoseAwake = -1;

	public const int cSleeperPoseSit = 0;

	public const int cSleeperPoseSideRight = 1;

	public const int cSleeperPoseSideLeft = 2;

	public const int cSleeperPoseBack = 3;

	public const int cSleeperPoseStomach = 4;

	public const int cSleeperPoseStand = 5;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityAlive entity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Animator anim;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<List<AnimParamData>> queuedAnimParams = new List<List<AnimParamData>>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AvatarController.ChangedAnimationParameters changedAnimationParameters = new AvatarController.ChangedAnimationParameters();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float animSyncWaitTime = 0.5f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float electrocuteTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cHitBlendInTimeMax = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cHitBlendOutExtraTime = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cHitWeightFastTarget = 0.15f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cHitAgainWeightAdd = 0.2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cHitAgainWeightAddWeak = 0.1f;

	public float hitWeightMax = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float hitDuration;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float hitDurationOut;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int hitLayerIndex = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float hitWeight = 0.001f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float hitWeightTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float hitWeightDuration;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float forwardSpeedLerpMultiplier = 10f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float strafeSpeedLerpMultiplier = 10f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float targetSpeedForward;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float targetSpeedStrafe;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cPhysicsTicks = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool hasTurnRate;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float turnRateFacing;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float turnRate;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static Dictionary<int, string> hashNames;

	public const int cActionSpecial = 3000;

	public const int cActionEnd = 9999;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cMaxQueuedAnimData = 10;

	public enum DataTypes
	{
		HitDuration
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public class ChangedAnimationParameters
	{
		public void Add(AnimParamData apd)
		{
			int count = this.m_animationParameters.Count;
			List<AnimParamData> list;
			if (count < 1)
			{
				list = this.newPacket();
			}
			else
			{
				list = this.m_animationParameters[count - 1];
			}
			int num;
			if (this.m_animationParameterLookup.TryGetValue(apd.NameHash, out num))
			{
				list.RemoveAt(num);
			}
			this.m_animationParameterLookup[apd.NameHash] = list.Count;
			list.Add(apd);
			if (apd.ValueType == AnimParamData.ValueTypes.Trigger)
			{
				this.newPacket();
				this.m_hasAnyTriggers = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<AnimParamData> newPacket()
		{
			List<AnimParamData> list = new List<AnimParamData>();
			this.m_animationParameters.Add(list);
			this.m_animationParameterLookup.Clear();
			return list;
		}

		public List<List<AnimParamData>> GetParameterLists()
		{
			List<List<AnimParamData>> list = new List<List<AnimParamData>>();
			this.m_sendDelay -= Time.deltaTime;
			if (!this.m_hasAnyTriggers)
			{
				if (this.m_sendDelay > 0f)
				{
					return list;
				}
			}
			while (this.m_animationParameters.Count > 0)
			{
				List<AnimParamData> list2 = this.m_animationParameters[0];
				this.m_animationParameters.RemoveAt(0);
				if (list2.Count != 0)
				{
					list.Add(list2);
				}
			}
			this.m_hasAnyTriggers = false;
			this.m_sendDelay = 0.05f;
			return list;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float sendPeriodInSeconds = 0.05f;

		[PublicizedFrom(EAccessModifier.Private)]
		public float m_sendDelay;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<List<AnimParamData>> m_animationParameters = new List<List<AnimParamData>>();

		[PublicizedFrom(EAccessModifier.Private)]
		public Dictionary<int, int> m_animationParameterLookup = new Dictionary<int, int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public bool m_hasAnyTriggers;
	}

	public enum ActionState
	{
		None,
		Start,
		Ready,
		Active
	}

	public enum CoverMode
	{
		None,
		Stand,
		Crouch,
		Prone
	}

	public enum CoverType
	{
		Left = -1,
		Center,
		Right
	}

	public enum CoverEngageType
	{
		None,
		Peek,
		Lean
	}
}
