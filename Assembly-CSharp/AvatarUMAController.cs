using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AvatarUMAController : LegacyAvatarController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void assignStates()
	{
		this.jumpState = Animator.StringToHash("Base Layer.Jump");
		this.fpvJumpState = Animator.StringToHash("Base Layer.FPVFemaleJump");
		this.deathStates = new HashSet<int>();
		AvatarCharacterController.GetThirdPersonDeathStates(this.deathStates);
		this.reloadStates = new HashSet<int>();
		AvatarCharacterController.GetThirdPersonReloadStates(this.reloadStates);
		this.hitStates = new HashSet<int>();
		AvatarCharacterController.GetThirdPersonHitStates(this.hitStates);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void assignParts(bool _bFPV)
	{
		if (!_bFPV)
		{
			this.pelvis = this.bipedTransform.FindInChilds("Hips", false);
			this.spine = this.pelvis.Find("LowerBack");
			this.spine1 = this.spine.Find("Spine");
			this.spine2 = this.spine1.Find("Spine1");
			this.spine3 = this.spine2.Find("Neck");
			this.head = this.spine3.Find("Head");
			this.cameraNode = this.head.Find("CameraNode");
			this.cameraRootTransform = this.spine3;
			this.rightHand = this.bipedTransform.FindInChilds(this.entity.GetRightHandTransformName(), false);
		}
		else
		{
			this.bNewModel = this.bipedTransform.FindInChilds("Origin", false) != null;
			if (!this.bNewModel)
			{
				this.pelvis = this.bipedTransform.Find("Bip001/Bip001 Pelvis");
				this.spine = this.pelvis.Find("Bip001 Spine");
				this.spine1 = this.spine.Find("Bip001 Spine1");
				this.spine2 = this.spine1.Find("Bip001 Spine2");
				this.spine3 = this.spine2.Find("Bip001 Spine3");
				this.head = this.spine3.Find("Bip001 Neck/Bip001 Head");
				this.cameraNode = this.head.Find("CameraNode");
				this.cameraNode = this.spine3;
				this.rightHand = this.bipedTransform.FindInChilds(this.entity.GetRightHandTransformName(), false);
			}
			else
			{
				this.pelvis = null;
				this.spine = null;
				this.spine1 = null;
				this.spine2 = null;
				this.spine3 = null;
				this.head = null;
				this.cameraNode = null;
				this.cameraRootTransform = this.spine;
				this.rightHand = this.bipedTransform.FindInChilds("Gunjoint", false);
			}
		}
		this.meshTransform = this.bipedTransform.FindInChilds("body", false);
		if (this.meshTransform == null)
		{
			this.meshTransform = this.bipedTransform.FindInChilds("TraderBob", false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		if (this.anim == null && this.m_bVisible)
		{
			base.SetAnimator(base.GetComponentInChildren<Animator>());
		}
		base.Update();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void setLayerWeights()
	{
		if (this.anim != null)
		{
			if (this.entity.IsDead())
			{
				this.anim.SetLayerWeight(1, 0f);
				this.anim.SetLayerWeight(2, 0f);
				this.anim.SetLayerWeight(3, 0f);
				return;
			}
			this.anim.SetLayerWeight(3, 1f);
			if (this.anim.GetInteger(AvatarController.vehiclePoseHash) >= 0)
			{
				this.anim.SetLayerWeight(1, 0f);
				this.anim.SetLayerWeight(2, 0f);
				return;
			}
			if (!this.anim.IsInTransition(1) && AnimationDelayData.AnimationDelay[this.entity.inventory.holdingItem.HoldType.Value].TwoHanded)
			{
				this.anim.SetLayerWeight(1, 0f);
				this.anim.SetLayerWeight(2, 1f);
				return;
			}
			if (!this.anim.IsInTransition(2))
			{
				this.anim.SetLayerWeight(1, 1f);
				this.anim.SetLayerWeight(2, 0f);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateLayerStateInfo()
	{
		if (this.anim != null)
		{
			this.baseStateInfo = this.anim.GetCurrentAnimatorStateInfo(0);
			this.currentWeaponHoldLayer = this.anim.GetCurrentAnimatorStateInfo((this.entity.inventory.holdingItem.HoldType != 0 && AnimationDelayData.AnimationDelay[this.entity.inventory.holdingItem.HoldType.Value].TwoHanded) ? 2 : 1);
			this.painLayer = this.anim.GetCurrentAnimatorStateInfo(4);
		}
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		string text = (_bFPV ? (_bMale ? "maleArms_fp" : "femaleArms_fp") : _modelName);
		Transform transform = this.modelTransform.Find(text);
		if (transform == null && _bFPV)
		{
			transform = this.modelTransform.Find(_modelName);
		}
		if (this.bipedTransform != null && this.bipedTransform != transform)
		{
			this.bipedTransform.gameObject.SetActive(false);
		}
		this.bipedTransform = transform;
		this.bipedTransform.gameObject.SetActive(true);
		this.modelName = _modelName;
		this.bMale = _bMale;
		this.bFPV = _bFPV;
		this.assignParts(this.bFPV);
		this.anim = base.GetComponentInChildren<Animator>();
		base._setBool("IsMale", _bMale, true);
		if (this.anim != null)
		{
			this.anim.logWarnings = false;
			this.anim.GetBool(AvatarController.isDeadHash);
			this.anim.GetInteger(AvatarController.weaponHoldTypeHash);
		}
		if (this.rightHandItemTransform != null)
		{
			this.rightHandItemTransform.parent = this.rightHand;
			Vector3 position = AnimationGunjointOffsetData.AnimationGunjointOffset[this.entity.inventory.holdingItem.HoldType.Value].position;
			Vector3 rotation = AnimationGunjointOffsetData.AnimationGunjointOffset[this.entity.inventory.holdingItem.HoldType.Value].rotation;
			this.rightHandItemTransform.localPosition = position;
			this.rightHandItemTransform.localEulerAngles = rotation;
		}
		this.SetWalkType(this.entity.GetWalkType(), false);
		this._setBool(AvatarController.isDeadHash, this.entity.IsDead(), true);
		this._setBool(AvatarController.isFPVHash, this.bFPV, true);
		this._setBool(AvatarController.isAliveHash, this.entity.IsAlive(), true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateSpineRotation()
	{
	}

	public override Transform GetRightHandTransform()
	{
		return this.rightHand;
	}

	public override Transform GetMeshTransform()
	{
		if (!(this.meshTransform != null))
		{
			return this.bipedTransform;
		}
		return this.meshTransform;
	}

	public override bool IsAnimationHitRunning()
	{
		return base.IsAnimationHitRunning() || this.hitStates.Contains(this.painLayer.fullPathHash);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
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
		if (this.anim.GetBool(AvatarController.isDeadHash) && !this.anim.IsInTransition(0))
		{
			this._setBool(AvatarController.isDeadHash, false, true);
		}
		if (!this.anim.IsInTransition(2) && this.anim.GetBool("Reload") && this.reloadStates.Contains(this.currentWeaponHoldLayer.fullPathHash) && this.rightHandAnimator != null)
		{
			this.rightHandAnimator.SetBool("Reload", false);
		}
		bool flag = this.anim.IsInTransition(4);
		int integer = this.anim.GetInteger(AvatarController.hitBodyPartHash);
		bool flag2 = this.IsAnimationHitRunning();
		if (!flag && integer != 0 && flag2)
		{
			this._setInt(AvatarController.hitBodyPartHash, 0, true);
			base._setBool("isCritical", false, true);
		}
		if (this.anim != null && this.anim.GetBool(AvatarController.itemUseHash))
		{
			int num = this.itemUseTicks - 1;
			this.itemUseTicks = num;
			if (num <= 0)
			{
				this._setBool(AvatarController.itemUseHash, false, true);
				if (this.rightHandAnimator != null)
				{
					this.rightHandAnimator.SetBool(AvatarController.itemUseHash, false);
				}
			}
		}
		if (this.isInDeathAnim && this.deathStates.Contains(this.baseStateInfo.fullPathHash) && !this.anim.IsInTransition(0))
		{
			this.didDeathTransition = true;
		}
		if (this.isInDeathAnim && this.didDeathTransition && (this.baseStateInfo.normalizedTime >= 1f || this.anim.IsInTransition(0)))
		{
			this.isInDeathAnim = false;
			if (this.entity.HasDeathAnim)
			{
				EModelBase emodel = this.entity.emodel;
				DamageResponse damageResponse = DamageResponse.New(true);
				emodel.DoRagdoll(in damageResponse, EModelBase.RagdollMode.Default, 999999f);
			}
		}
		if (this.isInDeathAnim && this.entity.HasDeathAnim && this.entity.RootMotion && this.entity.isCollidedHorizontally)
		{
			this.isInDeathAnim = false;
			EModelBase emodel2 = this.entity.emodel;
			DamageResponse damageResponse = DamageResponse.New(true);
			emodel2.DoRagdoll(in damageResponse, EModelBase.RagdollMode.Default, 999999f);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform meshTransform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform cameraRootTransform;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public HashSet<int> hitStates;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AnimatorStateInfo painLayer;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool bNewModel;
}
