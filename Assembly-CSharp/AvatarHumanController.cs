using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class AvatarHumanController : AvatarController
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
		if (!this.bipedT)
		{
			this.hitLayerIndex = 3;
			this.bipedT = this.entity.emodel.GetModelTransform();
			this.rightHandT = this.FindTransform(this.entity.GetRightHandTransformName());
			base.SetAnimator(this.bipedT);
			if (this.entity.RootMotion)
			{
				this.rootMotion = this.bipedT.gameObject.AddComponent<AvatarRootMotion>();
				this.rootMotion.Init(this, this.anim);
			}
		}
		this.SetWalkType(this.entity.GetWalkType(), false);
		this._setBool(AvatarController.isDeadHash, this.entity.IsDead(), true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public Transform FindTransform(string _name)
	{
		return this.bipedT.FindInChildren(_name);
	}

	public override void SetVisible(bool _b)
	{
		if (this.isVisible != _b || !this.isVisibleInit)
		{
			this.isVisible = _b;
			this.isVisibleInit = true;
			Transform transform = this.bipedT;
			if (transform)
			{
				Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>(true);
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].enabled = _b;
				}
			}
		}
	}

	public override Transform GetActiveModelRoot()
	{
		return this.modelT;
	}

	public override Transform GetRightHandTransform()
	{
		return this.rightHandT;
	}

	public override void TriggerSleeperPose(int pose, bool returningToSleep = false)
	{
		if (returningToSleep)
		{
			base.TriggerSleeperPose(pose, returningToSleep);
			return;
		}
		if (this.anim)
		{
			this._setInt(AvatarController.sleeperPoseHash, pose, true);
			switch (pose)
			{
			case -2:
				this.anim.CrossFadeInFixedTime("Crouch Walk 8", 0.25f);
				return;
			case -1:
				this._setTrigger(AvatarController.sleeperTriggerHash, true);
				break;
			case 0:
				this.anim.Play(AvatarController.sleeperIdleSitHash);
				return;
			case 1:
				this.anim.Play(AvatarController.sleeperIdleSideRightHash);
				return;
			case 2:
				this.anim.Play(AvatarController.sleeperIdleSideLeftHash);
				return;
			case 3:
				this.anim.Play(AvatarController.sleeperIdleBackHash);
				return;
			case 4:
				this.anim.Play(AvatarController.sleeperIdleStomachHash);
				return;
			case 5:
				this.anim.Play(AvatarController.sleeperIdleStandHash);
				return;
			default:
				return;
			}
		}
	}

	public override void TurnIntoCrawler()
	{
		this.isCrawler = true;
		this.crawlerTime = Time.time;
		this.isSuppressPain = true;
		this._setInt(AvatarController.hitBodyPartHash, 0, true);
		this.SetWalkType(21, false);
		this._setTrigger(AvatarController.toCrawlerTriggerHash, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setInt(int _propertyHash, int _value, bool _netsync = true)
	{
		if (_propertyHash == AvatarController.walkTypeHash && _value == 5 && this.entity.GetWalkType() == 21)
		{
			return;
		}
		base._setInt(_propertyHash, _value, _netsync);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public AvatarHumanController()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int cOverrideLayerIndex = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int cFullBodyLayerIndex = 2;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int cHitLayerIndex = 3;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AvatarRootMotion rootMotion;

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
	public AnimatorStateInfo baseStateInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo overrideStateInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo fullBodyStateInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AnimatorStateInfo hitStateInfo;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isVisibleInit;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isVisible;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int jumpState;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isJumpStarted;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isCrawler;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float crawlerTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isSuppressPain;
}
