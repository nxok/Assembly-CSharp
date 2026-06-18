using System;
using UnityEngine;

public class BodyAnimator
{
	public Animator Animator
	{
		get
		{
			if (!this.bodyParts.BodyObj.activeInHierarchy)
			{
				return null;
			}
			return this.animator;
		}
		set
		{
			if (this.bodyParts.BodyObj.activeInHierarchy)
			{
				this.animator = value;
			}
		}
	}

	public BodyAnimator.EnumState State
	{
		set
		{
			if (this.state != value)
			{
				this.state = value;
				this.bodyParts.BodyObj.SetActive(this.state != BodyAnimator.EnumState.Disabled);
				this.updateVisibility();
				if (this.state != BodyAnimator.EnumState.Disabled)
				{
					this.animator = this.bodyParts.BodyObj.GetComponentInChildren<Animator>();
				}
			}
		}
	}

	public BodyAnimator.BodyParts Parts
	{
		get
		{
			return this.bodyParts;
		}
	}

	public bool RagdollActive
	{
		get
		{
			return this.isRagdoll;
		}
		set
		{
			if (this.isRagdoll != value)
			{
				this.isRagdoll = value;
				if (this.animator)
				{
					this.animator.cullingMode = (this.isRagdoll ? 0 : this.defaultCullingMode);
					this.animator.enabled = !this.isRagdoll;
				}
			}
		}
	}

	public virtual void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float random)
	{
		if (this.avatarController != null)
		{
			this.avatarController.UpdateInt(AvatarController.movementStateHash, _movementState, true);
			this.avatarController.UpdateBool(AvatarController.isAliveHash, false, true);
			this.avatarController.UpdateBool(AvatarController.isDeadHash, true, true);
			this.avatarController.UpdateInt(AvatarController.hitBodyPartHash, (int)_bodyPart.ToPrimary().LowerToUpperLimb(), true);
			this.avatarController.UpdateFloat("HitRandomValue", random, true);
			this.avatarController.TriggerEvent("DeathTrigger");
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void initBodyAnimator(EntityAlive _entity, BodyAnimator.BodyParts _bodyParts, BodyAnimator.EnumState _defaultState)
	{
		this.Entity = _entity;
		this.bodyParts = _bodyParts;
		this.state = _defaultState;
		this.animator = this.bodyParts.BodyObj.GetComponentInChildren<Animator>();
		this.defaultCullingMode = 0;
		this.meshes = this.bodyParts.BodyObj.GetComponentsInChildren<MeshRenderer>();
		this.skinnedMeshes = this.bodyParts.BodyObj.GetComponentsInChildren<SkinnedMeshRenderer>();
		if (this.Entity.emodel != null)
		{
			this.avatarController = this.Entity.emodel.avatarController;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void cacheLayerStateInfo()
	{
		if (this.animator && this.animator.gameObject.activeInHierarchy)
		{
			this.currentBaseState = this.animator.GetCurrentAnimatorStateInfo(0);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual AnimatorStateInfo getCachedLayerStateInfo(int _layer)
	{
		return this.currentBaseState;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateVisibility()
	{
		if (this.state != BodyAnimator.EnumState.Disabled)
		{
			bool flag = this.state == BodyAnimator.EnumState.Visible;
			if (this.meshes != null)
			{
				MeshRenderer[] array = this.meshes;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].enabled = flag;
				}
			}
			if (this.skinnedMeshes != null)
			{
				foreach (SkinnedMeshRenderer skinnedMeshRenderer in this.skinnedMeshes)
				{
					if (skinnedMeshRenderer)
					{
						skinnedMeshRenderer.enabled = flag;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void assignLayerWeights()
	{
	}

	public virtual void SetDrunk(float _numBeers)
	{
	}

	public virtual void Update()
	{
		this.assignLayerWeights();
		this.updateVisibility();
	}

	public virtual void LateUpdate()
	{
		this.cacheLayerStateInfo();
	}

	public EntityAlive Entity;

	[PublicizedFrom(EAccessModifier.Private)]
	public Animator animator;

	[PublicizedFrom(EAccessModifier.Private)]
	public AnimatorStateInfo currentBaseState;

	[PublicizedFrom(EAccessModifier.Protected)]
	public AvatarController avatarController;

	[PublicizedFrom(EAccessModifier.Private)]
	public BodyAnimator.BodyParts bodyParts;

	[PublicizedFrom(EAccessModifier.Private)]
	public BodyAnimator.EnumState state;

	[PublicizedFrom(EAccessModifier.Private)]
	public AnimatorCullingMode defaultCullingMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRagdoll;

	[PublicizedFrom(EAccessModifier.Private)]
	public SkinnedMeshRenderer[] skinnedMeshes;

	[PublicizedFrom(EAccessModifier.Private)]
	public MeshRenderer[] meshes;

	public enum EnumState
	{
		Visible,
		OnlyColliders,
		Disabled
	}

	public class BodyParts
	{
		public BodyParts(Transform _bodyTransform, Transform _rightHand)
		{
			this.BodyObj = _bodyTransform.gameObject;
			this.RightHandT = _rightHand;
		}

		public GameObject BodyObj;

		public Transform RightHandT;
	}
}
