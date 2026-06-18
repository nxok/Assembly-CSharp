using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AvatarAnimalController : AvatarController
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.modelT = EModelBase.FindModel(base.transform);
		if (EntityClass.list[this.entity.entityClass].PainResistPerHit >= 0f)
		{
			this.hitLayerIndex = 1;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void assignBodyParts()
	{
		Transform transform = this.bipedT.FindInChilds("Hips", false);
		if (!transform)
		{
			return;
		}
		this.head = this.bipedT.FindInChilds("Head", false);
		this.leftUpperLeg = this.bipedT.FindInChilds("LeftUpLeg", false);
		this.rightUpperLeg = this.bipedT.FindInChilds("RightUpLeg", false);
		this.leftUpperArm = this.bipedT.FindInChilds("LeftArm", false);
		this.rightUpperArm = this.bipedT.FindInChilds("RightArm", false);
		this.limbScale = (this.head.position - transform.position).magnitude * 1.32f / this.head.lossyScale.x;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform SpawnLimbGore(Transform limbT, bool isLeft, string path, bool restoreState)
	{
		if (limbT == null)
		{
			return null;
		}
		CharacterJoint[] componentsInChildren = limbT.GetComponentsInChildren<CharacterJoint>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Object.Destroy(componentsInChildren[i]);
		}
		Rigidbody[] componentsInChildren2 = limbT.GetComponentsInChildren<Rigidbody>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			Object.Destroy(componentsInChildren2[j]);
		}
		Collider[] componentsInChildren3 = limbT.GetComponentsInChildren<Collider>();
		for (int k = 0; k < componentsInChildren3.Length; k++)
		{
			Object.Destroy(componentsInChildren3[k]);
		}
		Transform parent = limbT.parent;
		Transform transform = null;
		if (parent != null)
		{
			transform = this.SpawnLimbGore(parent, path, false);
			transform.localPosition = new Vector3(limbT.localPosition.x * 0.5f, limbT.localPosition.y, limbT.localPosition.z);
			transform.localRotation = limbT.localRotation;
			float num = this.limbScale;
			if (limbT != this.head)
			{
				num *= 0.7f;
				Vector3 vector;
				vector..ctor(0f, 0f, 90f);
				if (isLeft)
				{
					vector *= -1f;
				}
				vector += new Vector3(175f, 270f, 45f);
				transform.localEulerAngles += vector;
			}
			else
			{
				transform.localPosition = limbT.localPosition * 0.63f;
				Transform transform2 = new GameObject("scaleTarget").transform;
				transform2.position = limbT.position;
				for (int l = 0; l < limbT.childCount; l++)
				{
					limbT.GetChild(l).SetParent(transform2);
				}
				transform2.SetParent(limbT.parent);
				limbT.SetParent(transform2);
				transform2.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			}
			transform.localScale = limbT.localScale * num;
		}
		limbT.localScale = new Vector3(0.01f, 0.01f, 0.01f);
		return transform;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform SpawnLimbGore(Transform parent, string path, bool restoreState)
	{
		if (parent)
		{
			GameObject gameObject = (GameObject)Resources.Load(path);
			if (!gameObject)
			{
				Log.Out(this.entity.EntityName + " SpawnLimbGore prefab not found in resource. path: {0}", new object[] { path });
				string assetBundlePath = DismembermentManager.GetAssetBundlePath(path);
				GameObject gameObject2 = DataLoader.LoadAsset<GameObject>(assetBundlePath, false);
				if (!gameObject2)
				{
					Log.Warning(this.entity.EntityName + " SpawnLimbGore prefab not found in asset bundle. path: {0}", new object[] { assetBundlePath });
					return null;
				}
				gameObject = gameObject2;
			}
			GameObject gameObject3 = Object.Instantiate<GameObject>(gameObject, parent);
			GorePrefab component = gameObject3.GetComponent<GorePrefab>();
			if (component)
			{
				component.restoreState = restoreState;
			}
			return gameObject3.transform;
		}
		return null;
	}

	public override void RemoveLimb(BodyDamage _bodyDamage, bool restoreState)
	{
		EnumBodyPartHit bodyPartHit = _bodyDamage.bodyPartHit;
		EnumDamageTypes damageType = _bodyDamage.damageType;
		if (!this.headDismembered && (bodyPartHit & EnumBodyPartHit.Head) > EnumBodyPartHit.None)
		{
			this.MakeDismemberedPart(1U, damageType, this.head, false, restoreState);
			this.headDismembered = true;
		}
		if ((!this.leftUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.LeftUpperLeg) > EnumBodyPartHit.None) || (bodyPartHit & EnumBodyPartHit.LeftLowerLeg) > EnumBodyPartHit.None)
		{
			this.MakeDismemberedPart(32U, damageType, this.leftUpperLeg, true, restoreState);
			this.leftUpperLegDismembered = true;
		}
		if ((!this.rightUpperLegDismembered && (bodyPartHit & EnumBodyPartHit.RightUpperLeg) > EnumBodyPartHit.None) || (bodyPartHit & EnumBodyPartHit.RightLowerLeg) > EnumBodyPartHit.None)
		{
			this.MakeDismemberedPart(128U, damageType, this.rightUpperLeg, false, restoreState);
			this.rightUpperLegDismembered = true;
		}
		if ((!this.leftUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.LeftUpperArm) > EnumBodyPartHit.None) || (bodyPartHit & EnumBodyPartHit.LeftLowerArm) > EnumBodyPartHit.None)
		{
			this.MakeDismemberedPart(2U, damageType, this.leftUpperArm, true, restoreState);
			this.leftUpperArmDismembered = true;
		}
		if ((!this.rightUpperArmDismembered && (bodyPartHit & EnumBodyPartHit.RightUpperArm) > EnumBodyPartHit.None) || (bodyPartHit & EnumBodyPartHit.RightLowerArm) > EnumBodyPartHit.None)
		{
			this.MakeDismemberedPart(8U, damageType, this.rightUpperArm, false, restoreState);
			this.rightUpperArmDismembered = true;
		}
		if (this.entity.IsAlive())
		{
			int num = 0;
			if (this.leftUpperLegDismembered)
			{
				num++;
			}
			if (this.rightUpperLegDismembered)
			{
				num++;
			}
			if (this.leftUpperArmDismembered)
			{
				num++;
			}
			if (this.rightUpperArmDismembered)
			{
				num++;
			}
			if (this.missingMotorLimbs != num)
			{
				this.missingMotorLimbs = num;
			}
			float num2 = Mathf.Max(this.entity.moveSpeedAggroMax, this.entity.moveSpeedPanicMax) * (1f - (float)num / 5f);
			if (this.missingMotorLimbs >= 3)
			{
				num2 = 0f;
				this.entity.lastDamageResponse.Fatal = true;
				this.entity.Kill(this.entity.lastDamageResponse);
			}
			this.entity.moveSpeed = num2;
			this.entity.moveSpeedAggro = num2;
			this.entity.moveSpeedPanic = num2;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void MakeDismemberedPart(uint bodyDamageFlag, EnumDamageTypes damageType, Transform partT, bool isLeft, bool restoreState)
	{
		DismemberedPartData dismemberedPartData = DismembermentManager.DismemberPart(bodyDamageFlag, damageType, this.entity, true, false);
		if (dismemberedPartData == null)
		{
			return;
		}
		if (partT)
		{
			this.ProcDismemberedPart(partT, partT, dismemberedPartData);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ProcDismemberedPart(Transform t, Transform partT, DismemberedPartData part)
	{
		if (part.particlePaths != null)
		{
			for (int i = 0; i < part.particlePaths.Length; i++)
			{
				string text = part.particlePaths[i];
				if (!string.IsNullOrEmpty(text))
				{
					DismembermentManager.SpawnParticleEffect(new ParticleEffect(text, t.position + Origin.position, Quaternion.identity, 1f, Color.white), -1);
				}
			}
			return;
		}
		DismembermentManager.SpawnParticleEffect(new ParticleEffect("blood_impact", t.position + Origin.position, Quaternion.identity, 1f, Color.white), -1);
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		if (!this.bipedT)
		{
			this.bipedT = this.entity.emodel.GetModelTransform();
			this.rightHandT = this.FindTransform(this.entity.GetRightHandTransformName());
			this.assignBodyParts();
			this.assignStates();
			base.SetAnimator(this.bipedT);
			if (this.entity.RootMotion)
			{
				AvatarRootMotion avatarRootMotion = this.bipedT.GetComponent<AvatarRootMotion>();
				if (avatarRootMotion == null)
				{
					avatarRootMotion = this.bipedT.gameObject.AddComponent<AvatarRootMotion>();
				}
				avatarRootMotion.Init(this, this.anim);
			}
		}
		this.SetWalkType(this.entity.GetWalkType(), false);
		this._setBool(AvatarController.isDeadHash, this.entity.IsDead(), true);
		this._setBool(AvatarController.isAliveHash, this.entity.IsAlive(), true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform FindTransform(string _name)
	{
		return this.bipedT.FindInChildren(_name);
	}

	public override Transform GetRightHandTransform()
	{
		return this.rightHandT;
	}

	public override AvatarController.ActionState GetActionState()
	{
		if (this.attackPlayingTime > 0f)
		{
			return AvatarController.ActionState.Active;
		}
		int tagHash = this.baseStateInfo.tagHash;
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
		return this.attackPlayingTime > 0f || (!this.anim.IsInTransition(0) && this.baseStateInfo.tagHash == AvatarController.attackHash);
	}

	public override void StartAnimationAttack()
	{
		if (!this.entity.isEntityRemote)
		{
			this.attackPlayingTime = 0.5f;
		}
		this.idleTime = 0f;
		this._setTrigger(AvatarController.attackTriggerHash, true);
	}

	public override bool IsAnimationSpecialAttackPlaying()
	{
		return this.IsAnimationAttackPlaying();
	}

	public override bool IsAnimationSpecialAttack2Playing()
	{
		return this.IsAnimationAttackPlaying();
	}

	public override bool IsAnimationRagingPlaying()
	{
		return this.IsAnimationAttackPlaying();
	}

	public override void SetAlive()
	{
		this._setBool(AvatarController.isAliveHash, true, true);
		this._setBool(AvatarController.isDeadHash, false, true);
		this._setTrigger(AvatarController.triggerAliveHash, true);
	}

	public override void SetVisible(bool _b)
	{
		if (this.m_bVisible != _b || !this.visInit)
		{
			this.m_bVisible = _b;
			this.visInit = true;
			Transform transform = this.bipedT;
			if (transform != null)
			{
				Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = _b;
				}
			}
		}
	}

	public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
	{
		if (!base.CheckHit(_duration))
		{
			this.SetDataFloat(AvatarController.DataTypes.HitDuration, _duration, true);
			return;
		}
		this.idleTime = 0f;
		this._setInt(AvatarController.bodyPartHitHash, (int)_bodyPart, true);
		this._setInt(AvatarController.hitDirectionHash, _dir, true);
		this._setInt(AvatarController.hitDamageHash, _hitDamage, true);
		this._setBool(AvatarController.criticalHitHash, _criticalHit, true);
		this._setInt(AvatarController.movementStateHash, _movementState, true);
		this._setInt(AvatarController.randomHash, Mathf.FloorToInt(_random * 100f), true);
		this.SetDataFloat(AvatarController.DataTypes.HitDuration, _duration, true);
		this._setTrigger(AvatarController.painTriggerHash, true);
	}

	public override bool IsAnimationHitRunning()
	{
		if (this.hitLayerIndex < 0)
		{
			return false;
		}
		AnimatorStateInfo currentAnimatorStateInfo = this.anim.GetCurrentAnimatorStateInfo(this.hitLayerIndex);
		return (currentAnimatorStateInfo.tagHash == AvatarController.hitHash && currentAnimatorStateInfo.normalizedTime < 0.55f) || this.anim.IsInTransition(this.hitLayerIndex);
	}

	public override void StartAnimationJump(AnimJumpMode jumpMode)
	{
		this.idleTime = 0f;
		if (this.bipedT == null || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (this.anim != null)
		{
			if (jumpMode == AnimJumpMode.Start)
			{
				this._setTrigger(AvatarController.jumpStartHash, true);
				return;
			}
			this._setTrigger(AvatarController.jumpLandHash, true);
		}
	}

	public override void StartEating()
	{
		if (!this.isEating)
		{
			this._setTrigger(AvatarController.beginCorpseEatHash, true);
			this.isEating = true;
		}
	}

	public override void StopEating()
	{
		if (this.isEating)
		{
			this._setTrigger(AvatarController.endCorpseEatHash, true);
			this.isEating = false;
		}
	}

	public override void SetSwim(bool _enable)
	{
		this._setBool(AvatarController.isSwimHash, _enable, true);
	}

	public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float _random)
	{
		this.isInDeathAnim = true;
		this._setBool(AvatarController.isAliveHash, false, true);
		this._setBool(AvatarController.isDeadHash, true, true);
		this._setInt(AvatarController.randomHash, Mathf.FloorToInt(_random * 100f), true);
		this.idleTime = 0f;
		this._resetTrigger(AvatarController.attackTriggerHash, true);
		this._resetTrigger(AvatarController.painTriggerHash, true);
		this._setTrigger(AvatarController.deathTriggerHash, true);
	}

	public override Transform GetActiveModelRoot()
	{
		if (!this.modelT)
		{
			return this.bipedT;
		}
		return this.modelT;
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
		if (this.electrocuteTime > 0.3f && !this.entity.emodel.IsRagdollActive)
		{
			this._setTrigger(AvatarController.electrocuteTriggerHash, true);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if (!this.m_bVisible && (!this.entity || this.entity.isEntityRemote))
		{
			return;
		}
		if (!this.bipedT || !this.bipedT.gameObject.activeInHierarchy)
		{
			return;
		}
		if (!this.anim || !this.anim.enabled)
		{
			return;
		}
		this.attackPlayingTime -= Time.deltaTime;
		this.UpdateLayerStateInfo();
		float num = 0f;
		float num2 = 0f;
		if (!this.entity.IsFlyMode.Value)
		{
			num = this.entity.speedForward;
			num2 = this.entity.speedStrafe;
		}
		float num3 = num2;
		if (num3 >= 1234f)
		{
			num3 = 0f;
		}
		this._setFloat(AvatarController.forwardHash, num, false);
		this._setFloat(AvatarController.strafeHash, num3, false);
		if (!this.entity.IsDead())
		{
			float num4 = num * num + num3 * num3;
			this._setInt(AvatarController.movementStateHash, (num4 > this.entity.moveSpeedAggro * this.entity.moveSpeedAggro) ? 3 : ((num4 > this.entity.moveSpeed * this.entity.moveSpeed) ? 2 : ((num4 > 0.001f) ? 1 : 0)), false);
		}
		if (base.IsMoving(num, num2))
		{
			this.idleTime = 0f;
			this._setBool(AvatarController.isMovingHash, true, false);
		}
		else
		{
			this._setBool(AvatarController.isMovingHash, false, false);
		}
		if (this.isInDeathAnim && this.baseStateInfo.tagHash == AvatarController.deathHash && this.baseStateInfo.normalizedTime >= 1f && !this.anim.IsInTransition(0))
		{
			this.isInDeathAnim = false;
			if (this.entity.HasDeathAnim)
			{
				EModelBase emodel = this.entity.emodel;
				DamageResponse damageResponse = DamageResponse.New(true);
				emodel.DoRagdoll(in damageResponse, EModelBase.RagdollMode.Default, 999999f);
			}
		}
		this._setFloat(AvatarController.idleTimeHash, this.idleTime, false);
		this.idleTime += Time.deltaTime;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UpdateLayerStateInfo()
	{
		this.baseStateInfo = this.anim.GetCurrentAnimatorStateInfo(0);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool visInit;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_bVisible;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform modelT;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform bipedT;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightHandT;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform head;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform leftUpperArm;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightUpperArm;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform leftUpperLeg;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform rightUpperLeg;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float limbScale;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo baseStateInfo;

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
	public bool headDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool leftUpperArmDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool rightUpperArmDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool leftUpperLegDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool rightUpperLegDismembered;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isInDeathAnim;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int missingMotorLimbs;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isEating;
}
