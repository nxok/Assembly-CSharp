using System;
using System.Collections.Generic;
using System.Diagnostics;
using Assets.DuckType.Jiggle;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AvatarBanditController : AvatarHumanController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.modelT = EModelBase.FindModel(base.transform);
		this.assignStates();
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		base.SwitchModelAndView(_modelName, _bFPV, _bMale);
		this.locomotionLayerIndex = this.anim.GetLayerIndex("Locomotion");
		this.jumpLayerIndex = this.anim.GetLayerIndex("Jump");
		this.hitLayerIndex = this.anim.GetLayerIndex("Hit");
		this.coverLayerIndex = this.anim.GetLayerIndex("Cover");
	}

	public override void SetCrouching(bool _bEnable)
	{
		this.idleTime = 0f;
		this._setBool(AvatarController.isCrouchingHash, _bEnable, true);
	}

	public override AvatarController.CoverMode CurrentCoverMode()
	{
		return this._currentCoverMode;
	}

	public override AvatarController.CoverType CurrentCoverType()
	{
		return this._currentCoverType;
	}

	public override AvatarController.CoverEngageType CurrentCoverEngageType()
	{
		return this._currentEngageType;
	}

	public override void SetCoverMode(AvatarController.CoverMode _coverMode)
	{
		this.idleTime = 0f;
		this._currentCoverMode = _coverMode;
		this._setInt(AvatarController.CoverModeHash, (int)_coverMode, true);
	}

	public override void SetCoverType(AvatarController.CoverType _coverType)
	{
		this.idleTime = 0f;
		this._currentCoverType = _coverType;
		this._setInt(AvatarController.CoverTypeHash, (int)_coverType, true);
	}

	public override void SetCoverHeight(float coverHeight)
	{
		this.idleTime = 0f;
		this._coverHeight = coverHeight;
		this._setFloat(AvatarController.CoverHeightHash, this._coverHeight, true);
	}

	public override void SetCoverEngageType(AvatarController.CoverEngageType _engage)
	{
		this.idleTime = 0f;
		this._currentEngageType = _engage;
		this._setInt(AvatarController.CoverEngageHash, (int)_engage, true);
	}

	public override void SetInRightHand(Transform _transform)
	{
		this.idleTime = 0f;
		if (_transform)
		{
			Quaternion identity = Quaternion.identity;
			_transform.SetParent(this.GetRightHandTransform(), false);
			if (this.entity.inventory != null && this.entity.inventory.holdingItem != null)
			{
				AnimationGunjointOffsetData.AnimationGunjointOffsets animationGunjointOffsets = AnimationGunjointOffsetData.AnimationGunjointOffset[this.entity.inventory.holdingItem.HoldType.Value];
				_transform.localPosition = animationGunjointOffsets.position;
				_transform.localRotation = Quaternion.Euler(animationGunjointOffsets.rotation);
				return;
			}
			_transform.localPosition = Vector3.zero;
			_transform.localRotation = identity;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		float deltaTime = Time.deltaTime;
		if (this.actionTimeActive > 0f)
		{
			this.actionTimeActive -= deltaTime;
		}
		if (this.attackPlayingTime > 0f)
		{
			this.attackPlayingTime -= deltaTime;
			if (this.attackPlayingTime <= 0f)
			{
				this.isAttackImpact = true;
			}
		}
		if (this.timeSpecialAttack2Playing > 0f)
		{
			this.timeSpecialAttack2Playing -= deltaTime;
		}
		if (this.timeRagePlaying > 0f)
		{
			this.timeRagePlaying -= deltaTime;
		}
		if (!this.isVisible && (!this.entity || !this.entity.RootMotion || this.entity.isEntityRemote))
		{
			return;
		}
		if (!this.bipedT || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!this.anim || !this.anim.avatar.isValid || !this.anim.enabled)
		{
			return;
		}
		this.UpdateLayerStateInfo();
		this.SetLayerWeights();
		int value = this.entity.inventory.holdingItem.HoldType.Value;
		this._setInt(AvatarController.weaponHoldTypeHash, value, true);
		int carry = AnimationDelayData.AnimationDelay[value].Carry;
		this._setInt(AvatarController.weaponCarryHash, carry, true);
		float num = this.entity.speedForward * 20f;
		float num2 = this.entity.speedStrafe * 20f;
		bool flag = num * num + num2 * num2 > 0.0001f;
		this._setFloat(AvatarController.forwardHash, num, false);
		this._setFloat(AvatarController.strafeHash, num2, false);
		if (!this.isMoving)
		{
			if (flag)
			{
				this.isMoving = true;
				this._setBool(AvatarController.isMovingHash, true, true);
			}
		}
		else if (!flag)
		{
			this.isMoving = false;
			this._setBool(AvatarController.isMovingHash, false, true);
		}
		if (!this.entity.IsDead())
		{
			if (this.movementStateOverride != -1)
			{
				this._setInt(AvatarController.movementStateHash, this.movementStateOverride, true);
				this.movementStateOverride = -1;
			}
			else
			{
				float num3 = num * num;
				this._setInt(AvatarController.movementStateHash, (num3 > this.entity.moveSpeedAggro * this.entity.moveSpeedAggro) ? 3 : ((num3 > this.entity.moveSpeed * this.entity.moveSpeed) ? 2 : ((num3 > 0.001f) ? 1 : 0)), false);
			}
		}
		Vector3 vector;
		if (this.entity.GetAimTarget(out vector))
		{
			Vector3 chestPosition = this.entity.emodel.GetChestPosition();
			Matrix4x4 matrix4x = Matrix4x4.TRS(chestPosition, this.entity.emodel.GetFlatHipRotation(), Vector3.one);
			Vector3 vector2 = matrix4x.inverse.MultiplyPoint(vector);
			float num4 = Mathf.Sqrt(vector2.x * vector2.x + vector2.z * vector2.z);
			float num5 = Mathf.Atan2(vector2.y, num4) * 57.29578f / 90f;
			float num6 = MathUtils.NormalizeAxis(MathUtils.YawForDirection(vector2)) / 90f;
			if (GameManager.Instance.DebugAIAim)
			{
				Vector3 vector3 = chestPosition - Origin.position;
				Debug.DrawLine(vector3, vector3 + matrix4x.GetColumn(0), Color.red, 0.1f);
				Debug.DrawLine(vector3, vector3 + matrix4x.GetColumn(1), Color.green, 0.1f);
				Debug.DrawLine(vector3, vector3 + matrix4x.GetColumn(2), Color.blue, 0.1f);
				Debug.DrawLine(vector3, vector - Origin.position, Color.cyan, 0.1f);
			}
			this._setFloat(AvatarController.aimPitchHash, Mathf.Clamp(num5, -1f, 1f), true);
			this._setFloat(AvatarController.aimYawHash, Mathf.Clamp(num6, -1f, 1f), true);
		}
		else
		{
			this._setFloat(AvatarController.aimPitchHash, 0f, true);
			this._setFloat(AvatarController.aimYawHash, 0f, true);
		}
		if (this.electrocuteTime > 0.3f && !this.entity.emodel.IsRagdollActive)
		{
			this._setTrigger(AvatarController.isElectrocutedHash, true);
		}
		if (!this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.entity.IsInElevator() || this.entity.Climbing)
		{
			this._setBool(AvatarController.isClimbingHash, true, true);
			return;
		}
		this._setBool(AvatarController.isClimbingHash, false, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void LateUpdate()
	{
		if (!this.entity || !this.bipedT || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!this.anim || !this.anim.enabled)
		{
			return;
		}
		this.UpdateLayerStateInfo();
		ItemClass holdingItem = this.entity.inventory.holdingItem;
		if (holdingItem.Actions[0] != null)
		{
			holdingItem.Actions[0].UpdateNozzleParticlesPosAndRot(this.entity.inventory.holdingItemData.actionData[0]);
		}
		if (holdingItem.Actions[1] != null)
		{
			holdingItem.Actions[1].UpdateNozzleParticlesPosAndRot(this.entity.inventory.holdingItemData.actionData[1]);
		}
		int fullPathHash = this.baseStateInfo.fullPathHash;
		bool flag = this.anim.IsInTransition(0);
		if (!flag)
		{
			this.isJumpStarted = false;
			if (fullPathHash == this.jumpState)
			{
				this._setBool(AvatarController.jumpHash, false, true);
			}
		}
		if (this.isInDeathAnim)
		{
			if (this.baseStateInfo.tagHash == AvatarController.deathHash && this.baseStateInfo.normalizedTime >= 1f && !flag)
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
		if (this.isCrawler && Time.time - this.crawlerTime > 2f)
		{
			this.isSuppressPain = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLayerStateInfo()
	{
		this.baseStateInfo = this.anim.GetCurrentAnimatorStateInfo(0);
		this.overrideStateInfo = this.anim.GetCurrentAnimatorStateInfo(1);
		this.fullBodyStateInfo = this.anim.GetCurrentAnimatorStateInfo(2);
		this.coverStateInfo = this.anim.GetCurrentAnimatorStateInfo(this.coverLayerIndex);
		if (this.hitLayerIndex >= 0)
		{
			this.hitStateInfo = this.anim.GetCurrentAnimatorStateInfo(this.hitLayerIndex);
		}
		if (this.locomotionLayerIndex >= 0)
		{
			this.locomotionStateInfo = this.anim.GetCurrentAnimatorStateInfo(this.locomotionLayerIndex);
		}
		if (this.jumpLayerIndex >= 0)
		{
			this.jumpStateInfo = this.anim.GetCurrentAnimatorStateInfo(this.jumpLayerIndex);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetLayerWeights()
	{
		this.isSuppressPain = this.isSuppressPain && (this.anim.IsInTransition(2) || this.fullBodyStateInfo.fullPathHash != 0);
		this.anim.SetLayerWeight(1, 1f);
		this.anim.SetLayerWeight(2, (float)((this.isSuppressPain || this.entity.bodyDamage.CurrentStun != EnumEntityStunType.None) ? 0 : 1));
	}

	public override void ResetAnimations()
	{
		base.ResetAnimations();
		this.anim.Play("None", 1, 0f);
		this.anim.Play("None", 2, 0f);
	}

	public override void SetMeleeAttackSpeed(float _speed)
	{
	}

	public override AvatarController.ActionState GetActionState()
	{
		if (this.attackPlayingTime > 0f || this.overrideStateInfo.tagHash == AvatarController.attackHash)
		{
			return AvatarController.ActionState.Active;
		}
		int tagHash = this.fullBodyStateInfo.tagHash;
		if (tagHash == AvatarController.attackStartHash || this.actionTimeActive > 0f)
		{
			return AvatarController.ActionState.Start;
		}
		if (tagHash == AvatarController.attackReadyHash)
		{
			return AvatarController.ActionState.Ready;
		}
		if (tagHash == AvatarController.attackHash)
		{
			return AvatarController.ActionState.Active;
		}
		return AvatarController.ActionState.None;
	}

	public override bool IsActionActive()
	{
		return this.GetActionState() > AvatarController.ActionState.None;
	}

	public override void StartAction(int _animType)
	{
		if (_animType < 3000)
		{
			this.StartAnimationAttack();
			return;
		}
		this.idleTime = 0f;
		this._setInt(AvatarController.attackHash, _animType, true);
		this._setTrigger(AvatarController.attackTriggerHash, true);
		this.actionTimeActive = 0.2f;
	}

	public override bool IsAnimationAttackPlaying()
	{
		return this.attackPlayingTime > 0f || this.overrideStateInfo.tagHash == AvatarController.attackHash || this.fullBodyStateInfo.tagHash == AvatarController.attackHash;
	}

	public override void StartAnimationAttack()
	{
		if (!this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		this.idleTime = 0f;
		this.isAttackImpact = false;
		this.attackPlayingTime = 2f;
		float randomFloat = this.entity.rand.RandomFloat;
		int num = -1;
		if (!this.rightArmDismembered)
		{
			num = 0;
			if (!this.leftArmDismembered)
			{
				num = this.entity.rand.RandomInt & 1;
			}
		}
		else if (!this.leftArmDismembered)
		{
			num = 1;
		}
		int num2 = 8;
		if (num >= 0)
		{
			num2 = num;
		}
		int walkType = this.entity.GetWalkType();
		if (walkType >= 20)
		{
			num2 += walkType * 100;
		}
		if (this.entity.IsBreakingDoors && num >= 0)
		{
			num2 += 10;
		}
		if (num2 <= 1)
		{
			if (walkType == 1)
			{
				num2 += 100;
			}
			else if (this.entity.rand.RandomFloat < 0.25f)
			{
				num2 += 4;
			}
		}
		this._setInt(AvatarController.attackHash, num2, true);
		this._setFloat(AvatarController.attackBlendHash, randomFloat, true);
		this._setTrigger(AvatarController.attackTriggerHash, true);
	}

	public override void SetAttackImpact()
	{
		if (!this.isAttackImpact)
		{
			this.isAttackImpact = true;
			this.attackPlayingTime = 0.1f;
		}
	}

	public override bool IsAttackImpact()
	{
		return this.isAttackImpact;
	}

	public override bool IsAnimationHitRunning()
	{
		if (this.hitWeight == 0f)
		{
			return false;
		}
		int tagHash = this.hitStateInfo.tagHash;
		return tagHash == AvatarController.hitStartHash || (tagHash == AvatarController.hitHash && this.hitStateInfo.normalizedTime < 0.55f) || this.anim.IsInTransition(this.hitLayerIndex);
	}

	public override bool IsAnimationSpecialAttackPlaying()
	{
		return false;
	}

	public override void StartAnimationSpecialAttack(bool _b, int _animType)
	{
	}

	public override bool IsAnimationSpecialAttack2Playing()
	{
		return this.timeSpecialAttack2Playing > 0f;
	}

	public override void StartAnimationSpecialAttack2()
	{
		this.idleTime = 0f;
		this.timeSpecialAttack2Playing = 0.3f;
		this._setTrigger(AvatarController.specialAttack2Hash, true);
	}

	public override bool IsAnimationRagingPlaying()
	{
		return this.timeRagePlaying > 0f;
	}

	public override void StartAnimationRaging()
	{
		this.idleTime = 0f;
		this._setTrigger(AvatarController.rageHash, true);
		this.timeRagePlaying = 0.3f;
	}

	public override void StartAnimationElectrocute(float _duration)
	{
		base.StartAnimationElectrocute(_duration);
		this.idleTime = 0f;
	}

	public override bool IsAnimationDigRunning()
	{
		return AvatarController.digHash == this.baseStateInfo.tagHash;
	}

	public override bool IsAttackProhibited()
	{
		return this.anim.IsInTransition(this.coverLayerIndex) || (this.anim.GetLayerWeight(this.coverLayerIndex) > 0f && AvatarController.readyToFireHash != this.coverStateInfo.tagHash);
	}

	public override void StartAnimationDodge(float _blend)
	{
		this._setFloat(AvatarController.dodgeBlendHash, _blend, true);
		this._setBool(AvatarController.dodgeTriggerHash, true, true);
	}

	public override void StartAnimationJumping()
	{
		this.idleTime = 0f;
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
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
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		this.isJumpStarted = true;
		if (this.anim != null)
		{
			if (jumpMode == AnimJumpMode.Start)
			{
				this._setTrigger(AvatarController.jumpStartHash, true);
				return;
			}
			this._setTrigger(AvatarController.jumpLandHash, true);
			this._setInt(AvatarController.jumpLandResponseHash, 0, true);
		}
	}

	public override bool IsAnimationJumpRunning()
	{
		return this.isJumpStarted || AvatarController.jumpHash == this.baseStateInfo.tagHash || (this.jumpLayerIndex >= 0 && this.jumpStateInfo.tagHash == AvatarController.jumpHash);
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

	public override void BeginStun(EnumEntityStunType stun, EnumBodyPartHit _bodyPart, Utils.EnumHitDirection _hitDirection, bool _criticalHit, float random)
	{
		this._setInt(AvatarController.stunTypeHash, (int)stun, true);
		this._setInt(AvatarController.stunBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb(), true);
		this._setInt(AvatarController.hitDirectionHash, (int)_hitDirection, true);
		this._setFloat(AvatarController.HitRandomValueHash, random, true);
		this._setTrigger(AvatarController.beginStunTriggerHash, true);
		this._resetTrigger(AvatarController.endStunTriggerHash, true);
	}

	public override void EndStun()
	{
		this._setTrigger(AvatarController.endStunTriggerHash, true);
	}

	public override bool IsAnimationStunRunning()
	{
		return this.baseStateInfo.tagHash == AvatarController.stunHash;
	}

	public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
		this.idleTime = 0f;
		this.isInDeathAnim = true;
		this.didDeathTransition = false;
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim != null)
		{
			this.movementStateOverride = _movementState;
			this._setInt(AvatarController.movementStateHash, _movementState, true);
			this._setBool(AvatarController.isAliveHash, false, true);
			this._setInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb(), true);
			this._setFloat(AvatarController.HitRandomValueHash, random, true);
			this.SetFallAndGround(false, this.entity.onGround);
		}
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim != null)
		{
			this._setTrigger(AvatarController.deathTriggerHash, true);
		}
	}

	public override void StartEating()
	{
		if (!this.isEating)
		{
			this._setInt(AvatarController.attackHash, 0, true);
			this._setTrigger(AvatarController.beginCorpseEatHash, true);
			this.isEating = true;
		}
	}

	public override void StopEating()
	{
		if (this.isEating)
		{
			this._setInt(AvatarController.attackHash, 0, true);
			this._setTrigger(AvatarController.endCorpseEatHash, true);
			this.isEating = false;
		}
	}

	public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
	{
		if (!this.isCrawler || Time.time - this.crawlerTime > 2f)
		{
			this.InternalStartAnimationHit(_bodyPart, _dir, _hitDamage, _criticalHit, _movementState, _random, _duration);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InternalStartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float random, float _duration)
	{
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!base.CheckHit(_duration))
		{
			this.SetDataFloat(AvatarController.DataTypes.HitDuration, _duration, true);
			return;
		}
		this.idleTime = 0f;
		if (this.anim)
		{
			this.movementStateOverride = _movementState;
			this._setInt(AvatarController.movementStateHash, _movementState, true);
			this._setInt(AvatarController.hitDirectionHash, _dir, true);
			this._setInt(AvatarController.hitDamageHash, _hitDamage, true);
			this._setFloat(AvatarController.HitRandomValueHash, random, true);
			this._setInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb(), true);
			this.SetDataFloat(AvatarController.DataTypes.HitDuration, _duration, true);
			this._setTrigger(AvatarController.hitTriggerHash, true);
		}
	}

	public bool rightArmDismembered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.rightUpperArmDismembered || this.rightLowerArmDismembered;
		}
	}

	public bool leftArmDismembered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.leftUpperArmDismembered || this.leftLowerArmDismembered;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCensoredContent()
	{
		EntityClass entityClass = this.entity.EntityClass;
		if ((entityClass != null && entityClass.censorMode == 0) || !GameManager.Instance.IsGoreCensored())
		{
			return false;
		}
		EntityClass entityClass2 = this.entity.EntityClass;
		if (entityClass2 == null || entityClass2.censorType != 2)
		{
			EntityClass entityClass3 = this.entity.EntityClass;
			return entityClass3 != null && entityClass3.censorType == 3;
		}
		return true;
	}

	public void CleanupDismemberedLimbs()
	{
		for (int i = 0; i < this.dismemberedParts.Count; i++)
		{
			this.dismemberedParts[i].ReadyForCleanup = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void _InitDismembermentMaterials()
	{
		if (!this.mainZombieMaterial)
		{
			EModelBase emodel = this.entity.emodel;
			if (emodel)
			{
				Transform meshTransform = emodel.meshTransform;
				if (meshTransform)
				{
					Renderer component = meshTransform.GetComponent<Renderer>();
					if (component)
					{
						this.mainZombieMaterial = component.sharedMaterial;
						bool flag = this.entity.HasAnyTags(DismembermentManager.radiatedTag) && (this.mainZombieMaterial.HasProperty("_IsRadiated") || this.mainZombieMaterial.HasProperty("_Irradiated"));
						DismembermentManager instance = DismembermentManager.Instance;
						this.gibCapMaterial = ((!flag) ? instance.GibCapsMaterial : instance.GibCapsRadMaterial);
					}
				}
			}
			this.isCensored = this.isCensoredContent();
		}
	}

	public override void RemoveLimb(BodyDamage _bodyDamage, bool restoreState)
	{
		if (DismembermentManager.DebugDontCreateParts)
		{
			return;
		}
		DismembermentManager instance = DismembermentManager.Instance;
		int num = ((instance != null) ? instance.parts.Count : 0);
		if (this.entity.isDisintegrated && num >= 25)
		{
			return;
		}
		this._InitDismembermentMaterials();
		EnumBodyPartHit bodyPartHit = _bodyDamage.bodyPartHit;
		EnumDamageTypes enumDamageTypes = _bodyDamage.damageType;
		bool flag = enumDamageTypes == EnumDamageTypes.Heat;
		if (this.isCensored)
		{
			List<string> bluntCensors = DismembermentManager.BluntCensors;
			EntityClass entityClass = this.entity.EntityClass;
			if (bluntCensors.Contains((entityClass != null) ? entityClass.entityClassName : null))
			{
				enumDamageTypes = EnumDamageTypes.Bashing;
			}
			else
			{
				enumDamageTypes = EnumDamageTypes.Piercing;
			}
		}
		int num2 = 0;
		if (!this.headDismembered && (bodyPartHit & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
		{
			this.headDismembered = true;
			num2++;
			if (flag || this.entity.OverrideHeadSize != 1f)
			{
				enumDamageTypes = EnumDamageTypes.Piercing;
			}
			Transform transform = base.FindTransform("Neck");
			this.MakeDismemberedPart(1U, enumDamageTypes, transform, restoreState);
			Transform transform2 = this.bipedT.Find("HeadAccessories");
			if (transform2)
			{
				transform2.gameObject.SetActive(false);
			}
		}
		if (!this.leftUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.LeftUpperLeg) > EnumBodyPartHit.None)
		{
			this.leftUpperLegDismembered = true;
			num2++;
			Transform transform3 = base.FindTransform("LeftUpLeg");
			this.MakeDismemberedPart(32U, enumDamageTypes, transform3, restoreState);
		}
		if (!this.leftLowerLegDismembered && !this.leftUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.LeftLowerLeg) > EnumBodyPartHit.None)
		{
			this.leftLowerLegDismembered = true;
			num2++;
			Transform transform4 = base.FindTransform("LeftLeg");
			this.MakeDismemberedPart(64U, enumDamageTypes, transform4, restoreState);
		}
		if (!this.rightUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.RightUpperLeg) > EnumBodyPartHit.None)
		{
			this.rightUpperLegDismembered = true;
			num2++;
			Transform transform5 = base.FindTransform("RightUpLeg");
			this.MakeDismemberedPart(128U, enumDamageTypes, transform5, restoreState);
		}
		if (!this.rightLowerLegDismembered && !this.rightUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.RightLowerLeg) > EnumBodyPartHit.None)
		{
			this.rightLowerLegDismembered = true;
			num2++;
			Transform transform6 = base.FindTransform("RightLeg");
			this.MakeDismemberedPart(256U, enumDamageTypes, transform6, restoreState);
		}
		if (!this.leftUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.LeftUpperArm) > EnumBodyPartHit.None)
		{
			this.leftUpperArmDismembered = true;
			num2++;
			Transform transform7 = base.FindTransform("LeftArm");
			this.MakeDismemberedPart(2U, enumDamageTypes, transform7, restoreState);
		}
		if (!this.leftLowerArmDismembered && !this.leftUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.LeftLowerArm) > EnumBodyPartHit.None)
		{
			this.leftLowerArmDismembered = true;
			num2++;
			Transform transform8 = base.FindTransform("LeftForeArm");
			this.MakeDismemberedPart(4U, enumDamageTypes, transform8, restoreState);
		}
		if (!this.rightUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.RightUpperArm) > EnumBodyPartHit.None)
		{
			this.rightUpperArmDismembered = true;
			num2++;
			Transform transform9 = base.FindTransform("RightArm");
			this.MakeDismemberedPart(8U, enumDamageTypes, transform9, restoreState);
		}
		if (!this.rightLowerArmDismembered && !this.rightUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.RightLowerArm) > EnumBodyPartHit.None)
		{
			this.rightLowerArmDismembered = true;
			num2++;
			Transform transform10 = base.FindTransform("RightForeArm");
			this.MakeDismemberedPart(16U, enumDamageTypes, transform10, restoreState);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform SpawnLimbGore(Transform parent, string path, bool restoreState)
	{
		if (!parent || string.IsNullOrEmpty(path))
		{
			return null;
		}
		string text = DismembermentManager.GetAssetBundlePath(path);
		LoadManager.AssetRequestTask<GameObject> assetRequestTask = null;
		if (this.isCensored)
		{
			string text2 = text.Replace(".", "_CGore.");
			assetRequestTask = LoadManager.LoadAsset<GameObject>(text2, null, null, false, true, false);
			if (assetRequestTask.Asset)
			{
				text = text2;
			}
		}
		if (assetRequestTask == null || !assetRequestTask.Asset)
		{
			assetRequestTask = LoadManager.LoadAsset<GameObject>(text, null, null, false, true, false);
		}
		if (!assetRequestTask.Asset)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate<GameObject>(assetRequestTask.Asset, parent);
		GorePrefab component = gameObject.GetComponent<GorePrefab>();
		if (component)
		{
			component.restoreState = restoreState;
		}
		ReleaseAssetsOnDestroy releaseAssetsOnDestroy;
		if (!gameObject.TryGetComponent<ReleaseAssetsOnDestroy>(ref releaseAssetsOnDestroy))
		{
			releaseAssetsOnDestroy = gameObject.AddComponent<ReleaseAssetsOnDestroy>();
		}
		releaseAssetsOnDestroy.AddAssetHandle(assetRequestTask.GetHandle());
		assetRequestTask.Release();
		return gameObject.transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcDismemberedPart(Transform t, Transform partT, DismemberedPartData part, uint bodyDamageFlag)
	{
		Transform transform = partT.FindRecursive(part.targetBone);
		if (transform)
		{
			if (!part.attachToParent)
			{
				Vector3 localScale = t.localScale;
				localScale.x /= Utils.FastMax(0.01f, transform.localScale.x);
				localScale.y /= Utils.FastMax(0.01f, transform.localScale.y);
				localScale.z /= Utils.FastMax(0.01f, transform.localScale.z);
				t.localScale = localScale;
			}
			if (!string.IsNullOrEmpty(part.childTargetObj))
			{
				Transform transform2 = new GameObject("scaleTarget").transform;
				transform2.position = transform.position;
				for (int i = 0; i < transform.childCount; i++)
				{
					transform.GetChild(i).SetParent(transform2);
				}
				transform2.SetParent(transform.parent);
				transform.SetParent(transform2);
				transform2.localScale = Vector3.zero;
			}
			if (!string.IsNullOrEmpty(part.insertBoneObj))
			{
				Transform transform3 = new GameObject("scaleTarget").transform;
				transform3.position = transform.position;
				for (int j = 0; j < transform.childCount; j++)
				{
					transform.GetChild(j).SetParent(transform3);
				}
				transform3.SetParent(transform);
				if (this.defaultHeadPos != Vector3.zero)
				{
					transform3.position = this.defaultHeadPos;
					Vector3 localPosition = transform3.localPosition;
					localPosition.z = -localPosition.y * 0.5f;
					transform3.localPosition = localPosition;
				}
				transform3.localScale = Vector3.zero;
			}
			if (!string.IsNullOrEmpty(part.addScalePoint))
			{
				this.addScalePoint(transform);
			}
			if (!string.IsNullOrEmpty(part.maskScaleBlend))
			{
				partT.FindRecursive(part.maskScaleBlend).localScale = part.scale;
			}
			if (!string.IsNullOrEmpty(part.setFixedValues))
			{
				Transform transform4 = partT.FindRecursive(part.setFixedValues);
				Transform transform5 = this.addScalePoint(transform4).transform;
				transform5.localPosition = part.pos;
				transform5.localScale = part.scale;
			}
		}
		if (part.hasRotOffset)
		{
			t.localEulerAngles = part.rot;
		}
		if (DismembermentManager.DebugShowArmRotations)
		{
			DismembermentManager.AddDebugArmObjects(partT, t);
		}
		if (part.offset != Vector3.zero)
		{
			Transform transform6 = t.FindRecursive("pos");
			if (transform6)
			{
				transform6.localPosition += part.offset;
			}
		}
		if (part.particlePaths != null)
		{
			for (int k = 0; k < part.particlePaths.Length; k++)
			{
				string text = part.particlePaths[k];
				if (!string.IsNullOrEmpty(text))
				{
					DismembermentManager.SpawnParticleEffect(new ParticleEffect(text, t.position + Origin.position, Quaternion.identity, 1f, Color.white), -1);
				}
			}
		}
		Transform transform7 = t.FindRecursive("pos");
		if (transform7)
		{
			Renderer[] componentsInChildren = transform7.GetComponentsInChildren<Renderer>(true);
			Material altMaterial = this.entity.emodel.AltMaterial;
			if (altMaterial)
			{
				this.altMatName = altMaterial.name;
				for (int l = 0; l < this.altMatName.Length; l++)
				{
					char c = this.altMatName[l];
					if (char.IsDigit(c))
					{
						this.altEntityMatId = int.Parse(c.ToString());
						break;
					}
				}
			}
			else
			{
				foreach (char c2 in this.mainZombieMaterial.name)
				{
					if (char.IsDigit(c2))
					{
						this.altEntityMatId = int.Parse(c2.ToString());
						break;
					}
				}
			}
			foreach (Renderer renderer in componentsInChildren)
			{
				if (!renderer.GetComponent<ParticleSystem>())
				{
					Material[] sharedMaterials = renderer.sharedMaterials;
					for (int num = 0; num < sharedMaterials.Length; num++)
					{
						Material material = sharedMaterials[num];
						string name2 = material.name;
						if ((!part.prefabPath.ContainsCaseInsensitive("head") || !name2.ContainsCaseInsensitive("hair")) && (!renderer.name.ContainsCaseInsensitive("eye") || material.HasProperty("_IsRadiated") || material.HasProperty("_Irradiated")))
						{
							bool flag = false;
							int num2 = 0;
							while (num2 < DismembermentManager.DefaultBundleGibs.Length)
							{
								flag = name2.ContainsCaseInsensitive(DismembermentManager.DefaultBundleGibs[num2]);
								if (flag)
								{
									if (name2.ContainsCaseInsensitive("ZombieGibs_caps"))
									{
										if (!this.gibCapMaterialCopy)
										{
											this.gibCapMaterialCopy = Object.Instantiate<Material>(this.gibCapMaterial);
											this.gibCapMaterialCopy.name = this.gibCapMaterial.name.Replace("(global)", "(local)");
										}
										sharedMaterials[num] = this.gibCapMaterialCopy;
										break;
									}
									break;
								}
								else
								{
									num2++;
								}
							}
							if (!flag && material.name.Contains("HD_"))
							{
								if (!this.mainZombieMaterialCopy)
								{
									this.mainZombieMaterialCopy = Object.Instantiate<Material>(this.mainZombieMaterial);
								}
								sharedMaterials[num] = this.mainZombieMaterialCopy;
							}
						}
					}
					renderer.materials = sharedMaterials;
				}
			}
		}
		if (this.entity.IsFeral && bodyDamageFlag == 1U)
		{
			this.setUpEyeMats(t);
			if (part.isDetachable)
			{
				Transform transform8 = t.FindRecursive("Detachable");
				if (transform8)
				{
					this.setUpEyeMats(transform8);
				}
			}
			Transform transform9 = t.FindRecursive("FeralFlame");
			if (transform9 && !this.entity.HasAnyTags(DismembermentManager.radiatedTag))
			{
				transform9.gameObject.SetActive(true);
				string text2 = "large_flames_LOD (3)";
				Transform transform10 = this.entity.transform.FindRecursive(text2);
				if (transform10)
				{
					transform10.gameObject.SetActive(false);
				}
				else
				{
					Log.Warning("entity {0} no longer has a child named {1}", new object[]
					{
						this.entity.name,
						text2
					});
				}
			}
		}
		if (!this.dismemberMat && !string.IsNullOrEmpty(this.subFolderDismemberEntityName))
		{
			string text3 = this.rootDismmemberDir + string.Format("/gibs_{0}", this.subFolderDismemberEntityName.ToLower());
			Material sharedMaterial = this.skinnedMeshRenderer.sharedMaterial;
			if (this.entity.HasAnyTags(DismembermentManager.radiatedTag) && (sharedMaterial.HasProperty("_IsRadiated") || sharedMaterial.HasProperty("_Irradiated")))
			{
				text3 += "_IsRadiated";
			}
			LoadManager.AssetRequestTask<Material> assetRequestTask = null;
			if (!string.IsNullOrEmpty(part.dismemberMatPath))
			{
				string text4 = this.rootDismmemberDir + "/" + part.dismemberMatPath + ".mat";
				assetRequestTask = LoadManager.LoadAsset<Material>(text4, null, null, false, true, false);
				if (assetRequestTask.Asset)
				{
					text3 = text4;
				}
			}
			string text5 = text3;
			object obj = ((this.altEntityMatId != -1) ? this.altEntityMatId : "");
			text3 = text5 + ((obj != null) ? obj.ToString() : null);
			if (this.isCensored)
			{
				string text6 = text3;
				text3 += "_CGore.mat";
				assetRequestTask = LoadManager.LoadAsset<Material>(text3, null, null, false, true, false);
				if (!assetRequestTask.Asset)
				{
					text3 = text6;
				}
			}
			if (assetRequestTask == null || !assetRequestTask.Asset)
			{
				text3 += ".mat";
			}
			assetRequestTask = LoadManager.LoadAsset<Material>(text3, null, null, false, true, false);
			if (!assetRequestTask.Asset)
			{
				bool useMask = part.useMask;
				return;
			}
			this.dismemberMat = Object.Instantiate<Material>(assetRequestTask.Asset);
			if (this.dismemberMat.HasColor("_EmissiveColor") && sharedMaterial.HasColor("_EmissiveColor"))
			{
				this.dismemberMat.SetColor("_EmissiveColor", sharedMaterial.GetColor("_EmissiveColor"));
			}
			this.skinnedMeshRenderer.material = this.dismemberMat;
			if (this.smrLODOne)
			{
				this.smrLODOne.material = this.dismemberMat;
			}
			if (this.smrLODTwo)
			{
				this.smrLODTwo.material = this.dismemberMat;
			}
			ReleaseAssetsOnDestroy releaseAssetsOnDestroy;
			if (!t.TryGetComponent<ReleaseAssetsOnDestroy>(ref releaseAssetsOnDestroy))
			{
				releaseAssetsOnDestroy = t.gameObject.AddComponent<ReleaseAssetsOnDestroy>();
			}
			releaseAssetsOnDestroy.AddAssetHandle(assetRequestTask.GetHandle());
			assetRequestTask.Release();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject addScalePoint(Transform boneT)
	{
		GameObject gameObject = new GameObject("scaleTarget");
		Transform transform = gameObject.transform;
		for (int i = 0; i < boneT.childCount; i++)
		{
			boneT.GetChild(i).SetParent(transform);
		}
		transform.SetParent(boneT);
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.zero;
		return gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setUpEyeMats(Transform t)
	{
		Transform transform = t.FindRecursive("NormalEye");
		Transform transform2 = t.FindRecursive("FeralEye");
		if (transform2 && !this.entity.HasAnyTags(DismembermentManager.radOrChargedTag))
		{
			if (transform)
			{
				transform.gameObject.SetActive(false);
			}
			transform2.gameObject.SetActive(true);
			return;
		}
		if (transform)
		{
			MeshRenderer component = transform.GetComponent<MeshRenderer>();
			if (component)
			{
				Material material = Object.Instantiate<Material>(component.material);
				if (material.HasProperty("_IsRadiated"))
				{
					material.SetFloat("_IsRadiated", 1f);
				}
				if (material.HasProperty("_Irradiated"))
				{
					material.SetFloat("_Irradiated", 1f);
				}
				material.DisableKeyword("_ELECTRIC_SHOCK_ON");
				component.material = material;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakeDismemberedPart(uint bodyDamageFlag, EnumDamageTypes damageType, Transform partT, bool restoreState)
	{
		DismemberedPartData dismemberedPartData = DismembermentManager.DismemberPart(bodyDamageFlag, damageType, this.entity, true, DismembermentManager.DebugUseLegacy);
		if (dismemberedPartData == null)
		{
			return;
		}
		DismemberedPart dismemberedPart = new DismemberedPart(dismemberedPartData, bodyDamageFlag, damageType);
		this.dismemberedParts.Add(dismemberedPart);
		if (partT)
		{
			Transform transform = null;
			if (!string.IsNullOrEmpty(dismemberedPartData.targetBone))
			{
				Transform transform2 = partT.FindRecursive(dismemberedPartData.targetBone);
				if (!transform2)
				{
					transform2 = partT.FindParent(dismemberedPartData.targetBone);
				}
				Transform transform3 = new GameObject("DynamicGore").transform;
				if (!dismemberedPartData.attachToParent)
				{
					transform3.SetParent(transform2);
				}
				else
				{
					transform3.SetParent(transform2.parent);
				}
				transform3.localPosition = Vector3.zero;
				transform3.localRotation = Quaternion.identity;
				transform3.localScale = Vector3.one;
				transform = transform3;
				this.defaultHeadPos = Vector3.zero;
				if (!dismemberedPartData.useMask)
				{
					if (!string.IsNullOrEmpty(dismemberedPartData.insertBoneObj))
					{
						Transform transform4 = transform2.FindRecursive(dismemberedPartData.insertBoneObj);
						this.defaultHeadPos = transform4.position;
					}
					transform2.localScale = dismemberedPartData.scale;
					this.scaleOutChildBones(transform2);
				}
				else
				{
					Collider component = transform2.GetComponent<Collider>();
					if (component)
					{
						component.enabled = false;
					}
					this.disableChildColliders(transform2);
				}
			}
			else
			{
				partT.localScale = dismemberedPartData.scale;
			}
			if (!string.IsNullOrEmpty(dismemberedPartData.prefabPath))
			{
				if (string.IsNullOrEmpty(this.rootDismmemberDir) && dismemberedPartData.prefabPath.Contains("/"))
				{
					this.subFolderDismemberEntityName = dismemberedPartData.prefabPath.Remove(dismemberedPartData.prefabPath.IndexOf("/"));
					this.rootDismmemberDir = "@:Entities/Zombies/" + this.subFolderDismemberEntityName + "/Dismemberment";
				}
				if (!transform)
				{
					string text = dismemberedPartData.propertyKey.Replace("DismemberTag_", "");
					transform = GameUtils.FindTagInChilds(this.bipedT, text);
				}
				Transform transform5 = this.SpawnLimbGore(transform, dismemberedPartData.prefabPath, restoreState);
				if (transform5 && !string.IsNullOrEmpty(dismemberedPartData.targetBone))
				{
					if (DismembermentManager.DebugLogEnabled)
					{
						foreach (MeshFilter meshFilter in transform5.GetComponentsInChildren<MeshFilter>())
						{
							Mesh sharedMesh = meshFilter.sharedMesh;
							if (!sharedMesh || (sharedMesh && sharedMesh.vertexCount == 0))
							{
								Log.Warning(string.Format("{0} prefabPath {1} partName {2} is missing a mesh.", base.GetType(), dismemberedPartData.prefabPath, meshFilter.transform.name));
							}
						}
					}
					if (!this.skinnedMeshRenderer)
					{
						this.skinnedMeshRenderer = this.entity.emodel.meshTransform.GetComponent<SkinnedMeshRenderer>();
						Transform parent = this.skinnedMeshRenderer.transform.parent;
						for (int j = 0; j < parent.childCount; j++)
						{
							Transform child = parent.GetChild(j);
							if (child.name.ContainsCaseInsensitive("LOD1"))
							{
								this.smrLODOne = child.GetComponent<SkinnedMeshRenderer>();
							}
							if (child.name.ContainsCaseInsensitive("LOD2"))
							{
								this.smrLODTwo = child.GetComponent<SkinnedMeshRenderer>();
							}
						}
					}
					Renderer[] componentsInChildren2 = transform5.GetComponentsInChildren<Renderer>();
					bool flag = false;
					foreach (Renderer renderer in componentsInChildren2)
					{
						if (!renderer.GetComponent<ParticleSystem>())
						{
							Material[] sharedMaterials = renderer.sharedMaterials;
							for (int l = 0; l < sharedMaterials.Length; l++)
							{
								Material material = sharedMaterials[l];
								string name = material.name;
								if ((!dismemberedPart.prefabPath.ContainsCaseInsensitive("head") || !name.ContainsCaseInsensitive("hair")) && material.shader.name == "Game/Character" && !DismembermentManager.IsDefaultGib(name))
								{
									sharedMaterials[l] = this.mainZombieMaterial;
									flag = true;
								}
							}
							if (flag)
							{
								renderer.sharedMaterials = sharedMaterials;
							}
						}
					}
					this.ProcDismemberedPart(transform5, partT, dismemberedPartData, bodyDamageFlag);
					dismemberedPart.prefabT = transform5;
					Transform transform6 = partT.FindRecursive(dismemberedPartData.targetBone);
					if (!transform6)
					{
						transform6 = partT.FindParent(dismemberedPartData.targetBone);
					}
					dismemberedPart.targetT = transform6;
					if (dismemberedPartData.useMask)
					{
						if (dismemberedPartData.scaleOutLimb)
						{
							Transform transform7 = partT.FindRecursive(dismemberedPartData.targetBone);
							if (!transform7)
							{
								transform7 = partT.FindParent(dismemberedPartData.targetBone);
							}
							if (!string.IsNullOrEmpty(dismemberedPartData.solTarget))
							{
								transform7 = partT.FindRecursive(dismemberedPartData.solTarget);
								if (!transform7)
								{
									transform7 = partT.FindParent(dismemberedPartData.solTarget);
								}
							}
							this.scaleOutChildBones(transform7);
							if (dismemberedPartData.hasSolScale)
							{
								transform7.localScale = dismemberedPartData.solScale;
							}
						}
						else
						{
							this.scaleOutChildBones(transform6);
						}
						EnumBodyPartHit bodyPartHit = DismembermentManager.GetBodyPartHit(dismemberedPart.bodyDamageFlag);
						Transform transform8 = this.modelT.FindTagInChildren("D_Accessory");
						if (transform8)
						{
							DismembermentAccessoryMan component2 = transform8.GetComponent<DismembermentAccessoryMan>();
							if (component2)
							{
								component2.HidePart(bodyPartHit);
							}
						}
						if (!dismemberedPartData.scaleOutLimb || !string.IsNullOrEmpty(dismemberedPartData.solTarget) || !string.IsNullOrEmpty(dismemberedPartData.maskScaleBlend))
						{
							this.setLimbShaderProps(bodyPartHit, dismemberedPart);
						}
					}
					if (dismemberedPartData.isDetachable)
					{
						this.ActivateDetachableLimbs(bodyDamageFlag, damageType, transform5, dismemberedPart);
					}
				}
			}
		}
	}

	[Conditional("DEBUG_DISMEMBERMENT")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void logDismemberment(string _log)
	{
		if (DismembermentManager.DebugLogEnabled)
		{
			Type type = base.GetType();
			Log.Out(((type != null) ? type.ToString() : null) + " " + _log);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void scaleOutChildBones(Transform _boneT)
	{
		if (_boneT.childCount > 0)
		{
			for (int i = 0; i < _boneT.childCount; i++)
			{
				Transform child = _boneT.GetChild(i);
				if (child && !child.name.Equals("DynamicGore"))
				{
					child.localScale = Vector3.zero;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void disableChildColliders(Transform _boneT)
	{
		foreach (Collider collider in _boneT.GetComponentsInChildren<Collider>())
		{
			if (collider)
			{
				collider.enabled = false;
			}
		}
		foreach (CharacterJoint characterJoint in _boneT.GetComponentsInChildren<CharacterJoint>())
		{
			if (characterJoint)
			{
				Rigidbody component = characterJoint.GetComponent<Rigidbody>();
				Object.Destroy(characterJoint);
				Object.Destroy(component);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ActivateDetachableLimbs(uint bodyDamageFlag, EnumDamageTypes damageType, Transform partT, DismemberedPart part)
	{
		Transform entitiesTransform = GameManager.Instance.World.EntitiesTransform;
		if (!entitiesTransform)
		{
			return;
		}
		Transform transform = entitiesTransform.Find("DismemberedLimbs");
		if (!transform)
		{
			transform = new GameObject("DismemberedLimbs").transform;
			transform.SetParent(entitiesTransform);
			transform.localPosition = Vector3.zero;
		}
		Transform transform2 = partT.FindRecursive("Detachable");
		if (transform2)
		{
			EnumBodyPartHit bodyPartHit = DismembermentManager.GetBodyPartHit(bodyDamageFlag);
			Transform transform3 = new GameObject(string.Format("{0}_{1}_{2}", this.entity.entityId, this.entity.EntityName, bodyPartHit)).transform;
			transform3.SetParent(transform);
			part.SetDetachedTransform(transform3, transform2);
			if (this.entity.IsBloodMoon)
			{
				part.lifeTime /= 3f;
			}
			if (this.leftLowerArmDismembered && bodyDamageFlag == 2U)
			{
				DismembermentManager.ActivateDetachable(transform2, "HalfArm");
				this.hideDismemberedPart(bodyDamageFlag);
			}
			if (this.leftLowerLegDismembered && bodyDamageFlag == 32U)
			{
				DismembermentManager.ActivateDetachable(transform2, "HalfLeg");
				this.hideDismemberedPart(bodyDamageFlag);
			}
			if (this.rightLowerArmDismembered && bodyDamageFlag == 8U)
			{
				DismembermentManager.ActivateDetachable(transform2, "HalfArm");
				this.hideDismemberedPart(bodyDamageFlag);
			}
			if (this.rightLowerLegDismembered && bodyDamageFlag == 128U)
			{
				DismembermentManager.ActivateDetachable(transform2, "HalfLeg");
				this.hideDismemberedPart(bodyDamageFlag);
			}
			if (!transform2.gameObject.activeSelf)
			{
				transform2.gameObject.SetActive(true);
			}
			if (this.entity.OverrideHeadSize != 1f && this.headDismembered && bodyDamageFlag == 1U)
			{
				float headBigSize = this.entity.emodel.HeadBigSize;
				part.overrideHeadSize = headBigSize;
				part.overrideHeadDismemberScaleTime = this.entity.OverrideHeadDismemberScaleTime;
				Transform transform4 = transform2.Find("Physics");
				Transform transform5 = new GameObject("pivot").transform;
				transform5.SetParent(transform4);
				transform5.localScale = Vector3.one;
				int i = 0;
				while (i < part.targetT.childCount)
				{
					Transform child = part.targetT.GetChild(i);
					if (child.CompareTag("E_BP_Head"))
					{
						Transform transform6 = transform2.FindRecursive(bodyPartHit.ToString());
						if (transform6)
						{
							Renderer component = transform6.GetComponent<Renderer>();
							transform5.position = child.position + (component.bounds.center - child.position);
							part.pivotT = transform5;
							break;
						}
						transform5.position = child.position;
						part.pivotT = transform5;
						break;
					}
					else
					{
						i++;
					}
				}
				List<Transform> list = new List<Transform>();
				for (int j = 0; j < transform4.childCount; j++)
				{
					Transform child2 = transform4.GetChild(j);
					if (child2 != transform4)
					{
						list.Add(child2);
					}
				}
				for (int k = 0; k < list.Count; k++)
				{
					list[k].SetParent(transform5);
				}
				transform5.localScale = new Vector3(headBigSize, headBigSize, headBigSize);
			}
			transform2.SetParent(transform3);
			DismembermentManager instance = DismembermentManager.Instance;
			if (instance != null)
			{
				instance.AddPart(part);
			}
			string text = string.Empty;
			foreach (Renderer renderer in transform2.GetComponentsInChildren<Renderer>())
			{
				if (renderer != null)
				{
					Material[] sharedMaterials = renderer.sharedMaterials;
					for (int m = 0; m < sharedMaterials.Length; m++)
					{
						Material material = sharedMaterials[m];
						if (material != null)
						{
							text = material.name;
							if ((!part.prefabPath.ContainsCaseInsensitive("head") || !text.ContainsCaseInsensitive("hair")) && (!renderer.name.ContainsCaseInsensitive("eye") || material.HasProperty("_IsRadiated") || material.HasProperty("_Irradiated")))
							{
								if (text.ContainsCaseInsensitive("ZombieGibs_caps"))
								{
									sharedMaterials[m] = this.gibCapMaterial;
								}
								if (text.Contains("HD_"))
								{
									sharedMaterials[m] = this.mainZombieMaterial;
								}
								sharedMaterials[m].DisableKeyword("_ELECTRIC_SHOCK_ON");
							}
						}
					}
					renderer.sharedMaterials = sharedMaterials;
				}
			}
			Jiggle[] componentsInChildren2 = transform2.GetComponentsInChildren<Jiggle>(true);
			for (int n = 0; n < componentsInChildren2.Length; n++)
			{
				componentsInChildren2[n].enabled = true;
			}
			Rigidbody componentInChildren = transform2.GetComponentInChildren<Rigidbody>();
			if (componentInChildren)
			{
				Vector3 vector = Vector3.up * this.entity.lastHitForce;
				float num = Vector3.Angle(this.entity.GetForwardVector(), this.entity.lastHitImpactDir);
				componentInChildren.AddTorque(Quaternion.FromToRotation(this.entity.GetForwardVector(), this.entity.lastHitImpactDir).eulerAngles * (1f + num / 90f), 1);
				componentInChildren.AddForce((this.entity.lastHitImpactDir + vector) * this.entity.lastHitForce, 1);
				string damageTag = DismembermentManager.GetDamageTag(damageType, this.entity.lastHitRanged);
				if (damageTag == "blunt")
				{
					if (damageType == EnumDamageTypes.Piercing)
					{
						componentInChildren.AddForce(this.entity.lastHitImpactDir + vector, 1);
					}
					else
					{
						componentInChildren.AddForce(this.entity.lastHitImpactDir * this.entity.lastHitForce * 1.5f + vector * 1.25f, 1);
					}
				}
				if (damageTag == "blade")
				{
					float num2 = Vector3.Dot(this.entity.GetForwardVector(), this.entity.lastHitImpactDir);
					float num3 = Vector3.Dot(this.entity.GetForwardVector(), this.entity.lastHitEntityFwd);
					componentInChildren.AddForce((num2 < num3) ? (-this.entity.transform.right * this.entity.lastHitForce + vector) : (this.entity.transform.right * this.entity.lastHitForce + vector), 1);
					componentInChildren.AddTorque(Quaternion.FromToRotation(this.entity.GetForwardVector(), this.entity.lastHitImpactDir).eulerAngles * (1f + num / 90f) * this.entity.lastHitForce, 1);
				}
				if (damageType == EnumDamageTypes.Heat)
				{
					float num4 = 2.67f;
					componentInChildren.AddForce(this.entity.lastHitImpactDir * num4 + Vector3.up * num4 * 0.67f, 1);
				}
			}
			ReleaseAssetsOnDestroy releaseAssetsOnDestroy;
			if (partT.TryGetComponent<ReleaseAssetsOnDestroy>(ref releaseAssetsOnDestroy))
			{
				ReleaseAssetsOnDestroy releaseAssetsOnDestroy2 = transform2.gameObject.AddComponent<ReleaseAssetsOnDestroy>();
				releaseAssetsOnDestroy.CopyTo(releaseAssetsOnDestroy2);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Material GetMainZombieBodyMaterial()
	{
		EModelBase emodel = this.entity.emodel;
		if (emodel)
		{
			Transform meshTransform = emodel.meshTransform;
			if (meshTransform)
			{
				return meshTransform.GetComponent<Renderer>().sharedMaterial;
			}
		}
		return null;
	}

	public override void Electrocute(bool enabled)
	{
		base.Electrocute(enabled);
		if (enabled)
		{
			Material mainZombieBodyMaterial = this.GetMainZombieBodyMaterial();
			if (mainZombieBodyMaterial)
			{
				mainZombieBodyMaterial.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if (this.dismemberMat)
			{
				this.dismemberMat.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if (this.mainZombieMaterialCopy)
			{
				this.mainZombieMaterialCopy.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			if (this.gibCapMaterialCopy)
			{
				this.gibCapMaterialCopy.EnableKeyword("_ELECTRIC_SHOCK_ON");
			}
			this.StartAnimationElectrocute(0.6f);
			return;
		}
		Material mainZombieBodyMaterial2 = this.GetMainZombieBodyMaterial();
		if (mainZombieBodyMaterial2)
		{
			mainZombieBodyMaterial2.DisableKeyword("_ELECTRIC_SHOCK_ON");
		}
		if (this.dismemberMat)
		{
			this.dismemberMat.DisableKeyword("_ELECTRIC_SHOCK_ON");
		}
		if (this.mainZombieMaterialCopy)
		{
			this.mainZombieMaterialCopy.DisableKeyword("_ELECTRIC_SHOCK_ON");
		}
		if (this.gibCapMaterialCopy)
		{
			this.gibCapMaterialCopy.DisableKeyword("_ELECTRIC_SHOCK_ON");
		}
		this.StartAnimationElectrocute(0f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setLimbShaderProps(EnumBodyPartHit partHit, DismemberedPart part)
	{
		DismemberedPartData data = part.Data;
		if (this.dismemberMat)
		{
			bool scaleOutLimb = data.scaleOutLimb;
			bool isLinked = data.isLinked;
			if (this.dismemberMat.HasProperty("_LeftLowerLeg") && (partHit & EnumBodyPartHit.LeftLowerLeg) > EnumBodyPartHit.None)
			{
				this.dismemberMat.SetFloat("_LeftLowerLeg", 1f);
				if (isLinked)
				{
					this.dismemberMat.SetFloat("_LeftUpperLeg", 1f);
				}
			}
			if (this.dismemberMat.HasProperty("_LeftUpperLeg") && (partHit & EnumBodyPartHit.LeftUpperLeg) > EnumBodyPartHit.None)
			{
				if (!isLinked)
				{
					this.dismemberMat.SetFloat("_LeftUpperLeg", 1f);
				}
				if (!scaleOutLimb)
				{
					this.dismemberMat.SetFloat("_LeftLowerLeg", 1f);
				}
			}
			if (this.dismemberMat.HasProperty("_RightLowerLeg") && (partHit & EnumBodyPartHit.RightLowerLeg) > EnumBodyPartHit.None)
			{
				this.dismemberMat.SetFloat("_RightLowerLeg", 1f);
				if (isLinked)
				{
					this.dismemberMat.SetFloat("_RightUpperLeg", 1f);
				}
			}
			if (this.dismemberMat.HasProperty("_RightUpperLeg") && (partHit & EnumBodyPartHit.RightUpperLeg) > EnumBodyPartHit.None)
			{
				if (!isLinked)
				{
					this.dismemberMat.SetFloat("_RightUpperLeg", 1f);
				}
				if (!scaleOutLimb)
				{
					this.dismemberMat.SetFloat("_RightLowerLeg", 1f);
				}
			}
			if (this.dismemberMat.HasProperty("_LeftLowerArm") && (partHit & EnumBodyPartHit.LeftLowerArm) > EnumBodyPartHit.None && !scaleOutLimb)
			{
				this.dismemberMat.SetFloat("_LeftLowerArm", 1f);
			}
			if (this.dismemberMat.HasProperty("_LeftUpperArm") && (partHit & EnumBodyPartHit.LeftUpperArm) > EnumBodyPartHit.None)
			{
				if (!isLinked)
				{
					this.dismemberMat.SetFloat("_LeftUpperArm", 1f);
				}
				if (!scaleOutLimb)
				{
					this.dismemberMat.SetFloat("_LeftLowerArm", 1f);
				}
			}
			if (this.dismemberMat.HasProperty("_RightLowerArm") && (partHit & EnumBodyPartHit.RightLowerArm) > EnumBodyPartHit.None && !scaleOutLimb)
			{
				this.dismemberMat.SetFloat("_RightLowerArm", 1f);
			}
			if (this.dismemberMat.HasProperty("_RightUpperArm") && (partHit & EnumBodyPartHit.RightUpperArm) > EnumBodyPartHit.None)
			{
				if (!isLinked)
				{
					this.dismemberMat.SetFloat("_RightUpperArm", 1f);
				}
				if (!scaleOutLimb)
				{
					this.dismemberMat.SetFloat("_RightLowerArm", 1f);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void hideDismemberedPart(uint bodyDamageFlag)
	{
		uint lowerBodyPart = 0U;
		if (bodyDamageFlag == 2U)
		{
			lowerBodyPart = 4U;
		}
		if (bodyDamageFlag == 8U)
		{
			lowerBodyPart = 16U;
		}
		if (bodyDamageFlag == 32U)
		{
			lowerBodyPart = 64U;
		}
		if (bodyDamageFlag == 128U)
		{
			lowerBodyPart = 256U;
		}
		if (lowerBodyPart != 0U)
		{
			DismemberedPart dismemberedPart = this.dismemberedParts.Find((DismemberedPart p) => p.bodyDamageFlag == lowerBodyPart);
			if (dismemberedPart != null)
			{
				dismemberedPart.Hide();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float idleTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float actionTimeActive;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float attackPlayingTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isAttackImpact;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeSpecialAttack2Playing;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float timeRagePlaying;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int locomotionLayerIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AnimatorStateInfo locomotionStateInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int jumpLayerIndex;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AnimatorStateInfo jumpStateInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isMoving;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int movementStateOverride = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isEating;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool headDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool leftUpperArmDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool leftLowerArmDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool rightUpperArmDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool rightLowerArmDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool leftUpperLegDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool leftLowerLegDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool rightUpperLegDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool rightLowerLegDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isInDeathAnim;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool didDeathTransition;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int coverLayerIndex;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo coverStateInfo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material mainZombieMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material mainZombieMaterialCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material gibCapMaterial;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material gibCapMaterialCopy;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AvatarController.CoverMode _currentCoverMode;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AvatarController.CoverType _currentCoverType;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AvatarController.CoverEngageType _currentEngageType;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float _coverHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Material dismemberMat;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SkinnedMeshRenderer skinnedMeshRenderer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SkinnedMeshRenderer smrLODOne;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public SkinnedMeshRenderer smrLODTwo;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string rootDismmemberDir;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string subFolderDismemberEntityName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int altEntityMatId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string altMatName;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<DismemberedPart> dismemberedParts = new List<DismemberedPart>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isCensored;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cEmissiveColor = "_EmissiveColor";

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 defaultHeadPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const string cElectrocuteKeyword = "_ELECTRIC_SHOCK_ON";
}
