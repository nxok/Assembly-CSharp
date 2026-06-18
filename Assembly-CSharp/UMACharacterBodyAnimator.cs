using System;
using System.Collections.Generic;
using UnityEngine;

public class UMACharacterBodyAnimator : BodyAnimator
{
	public UMACharacterBodyAnimator(EntityAlive _entity, AvatarCharacterController.AnimationStates _animStates, Transform _bodyTransform, BodyAnimator.EnumState _defaultState)
	{
		Transform transform = _bodyTransform.FindInChilds((_entity.emodel is EModelSDCS) ? "RightWeapon" : "Gunjoint", false);
		BodyAnimator.BodyParts bodyParts = new BodyAnimator.BodyParts(_bodyTransform, transform);
		base.initBodyAnimator(_entity, bodyParts, _defaultState);
		this.deathStates = _animStates.DeathStates;
		this.hitStates = _animStates.HitStates;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void assignLayerWeights()
	{
		base.assignLayerWeights();
		Animator animator = base.Animator;
		if (animator)
		{
			if (this.Entity.IsDead())
			{
				animator.SetLayerWeight(1, 0f);
				animator.SetLayerWeight(2, 0f);
				animator.SetLayerWeight(3, 0f);
				return;
			}
			animator.SetLayerWeight(3, 1f);
			if (animator.GetInteger(AvatarController.vehiclePoseHash) >= 0)
			{
				animator.SetLayerWeight(1, 0f);
				animator.SetLayerWeight(2, 0f);
				return;
			}
			if (!animator.IsInTransition(1) && AnimationDelayData.AnimationDelay[this.Entity.inventory.holdingItem.HoldType.Value].TwoHanded)
			{
				animator.SetLayerWeight(1, 0f);
				animator.SetLayerWeight(2, 1f);
				return;
			}
			if (!animator.IsInTransition(2))
			{
				animator.SetLayerWeight(1, 1f);
				animator.SetLayerWeight(2, 0f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void cacheLayerStateInfo()
	{
		base.cacheLayerStateInfo();
		Animator animator = base.Animator;
		if (animator)
		{
			this.currentOverrideLayer = animator.GetCurrentAnimatorStateInfo(1);
			this.twoHandedLayer = animator.GetCurrentAnimatorStateInfo(2);
			this.painLayer = animator.GetCurrentAnimatorStateInfo(4);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override AnimatorStateInfo getCachedLayerStateInfo(int _layer)
	{
		if (_layer == 1)
		{
			return this.currentOverrideLayer;
		}
		if (_layer == 2)
		{
			return this.twoHandedLayer;
		}
		return base.getCachedLayerStateInfo(_layer);
	}

	public override void SetDrunk(float _numBeers)
	{
		if (base.Animator && this.Entity.AttachedToEntity == null && this.avatarController != null)
		{
			this.avatarController.UpdateFloat("drunk", _numBeers, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void updateSpineRotation()
	{
		if (!base.RagdollActive && !this.Entity.IsDead() && base.Animator && this.Entity.AttachedToEntity == null && this.avatarController != null)
		{
			float num;
			this.avatarController.TryGetFloat(AvatarController.yLookHash, out num);
			EntityPlayerLocal entityPlayerLocal = this.Entity as EntityPlayerLocal;
			if (entityPlayerLocal != null && !entityPlayerLocal.vp_FPCamera.Locked3rdPerson && entityPlayerLocal.IsCameraFacingCharacter())
			{
				this.avatarController.UpdateFloat(AvatarController.yLookHash, Mathf.Lerp(num, 0f, Time.deltaTime * 12f), false);
				return;
			}
			this.avatarController.UpdateFloat(AvatarController.yLookHash, Mathf.Lerp(num, -this.Entity.rotation.x / 90f, Time.deltaTime * 12f), false);
		}
	}

	public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
		base.StartDeathAnimation(_bodyPart, _movementState, random);
		this.isInDeathAnim = true;
		this.didDeathTransition = false;
	}

	public override void Update()
	{
		base.Update();
		this.updateSpineRotation();
	}

	public override void LateUpdate()
	{
		base.LateUpdate();
		Animator animator = base.Animator;
		if (animator)
		{
			AnimatorStateInfo cachedLayerStateInfo = this.getCachedLayerStateInfo(0);
			if (this.isInDeathAnim)
			{
				if (this.deathStates.Contains(cachedLayerStateInfo.fullPathHash) && !animator.IsInTransition(0))
				{
					this.didDeathTransition = true;
				}
				if (this.didDeathTransition && (cachedLayerStateInfo.normalizedTime >= 1f || animator.IsInTransition(0)))
				{
					this.isInDeathAnim = false;
					if (this.Entity.HasDeathAnim)
					{
						EModelBase emodel = this.Entity.emodel;
						DamageResponse damageResponse = DamageResponse.New(true);
						emodel.DoRagdoll(in damageResponse, EModelBase.RagdollMode.Default, 999999f);
					}
				}
			}
			if (animator.GetInteger(AvatarController.hitBodyPartHash) != 0 && this.avatarController != null && !animator.IsInTransition(4) && this.hitStates.Contains(this.painLayer.fullPathHash))
			{
				this.avatarController.UpdateInt(AvatarController.hitBodyPartHash, 0, true);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public AnimatorStateInfo currentOverrideLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public AnimatorStateInfo twoHandedLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public AnimatorStateInfo painLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> deathStates;

	[PublicizedFrom(EAccessModifier.Private)]
	public HashSet<int> hitStates;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isInDeathAnim;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool didDeathTransition;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cYLookUpdateSpeed = 12f;
}
