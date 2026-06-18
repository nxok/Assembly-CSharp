using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AvatarLocalPlayerController : AvatarCharacterController
{
	public BodyAnimator FPSArms
	{
		get
		{
			return this.fpsArms;
		}
	}

	public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
	{
		base.SwitchModelAndView(_modelName, _bFPV, _bMale);
		if (this.entity is EntityPlayerLocal && (this.entity as EntityPlayerLocal).IsSpectator)
		{
			return;
		}
		if (this.fpsArms != null && this.isMale != _bMale)
		{
			if (this.fpsArms.Parts.BodyObj != null)
			{
				this.fpsArms.Parts.BodyObj.SetActive(false);
			}
			base.removeBodyAnimator(this.fpsArms);
			this.fpsArms = null;
		}
		if (this.fpsArms == null)
		{
			this.isMale = _bMale;
			EntityPlayerLocal entityPlayerLocal = this.entity as EntityPlayerLocal;
			Transform transform = ((entityPlayerLocal != null) ? entityPlayerLocal.cameraTransform : null);
			if (transform != null)
			{
				Transform transform2 = transform.FindInChildren((this.entity.emodel is EModelSDCS) ? "baseRigFP" : (this.isMale ? "maleArms_fp" : "femaleArms_fp"));
				if (transform2 != null)
				{
					transform2.gameObject.SetActive(true);
					this.fpsArms = base.addBodyAnimator(this.createFPSArms(transform2));
				}
			}
		}
		if (this.fpsArms != null)
		{
			this.initBodyAnimator(this.fpsArms, _bFPV, _bMale);
		}
		this.isFPV = _bFPV;
		if (_bFPV)
		{
			base.PrimaryBody = this.fpsArms;
			this.fpsArms.State = BodyAnimator.EnumState.Visible;
			base.CharacterBody.State = BodyAnimator.EnumState.OnlyColliders;
			return;
		}
		base.PrimaryBody = base.CharacterBody;
		if (this.fpsArms != null)
		{
			this.fpsArms.State = BodyAnimator.EnumState.Disabled;
		}
		base.CharacterBody.State = BodyAnimator.EnumState.Visible;
		if (base.HeldItemTransform != null)
		{
			Utils.SetLayerRecursively(base.HeldItemTransform.gameObject, 24, Utils.ExcludeLayerZoom);
		}
	}

	public void TPVResetAnimPose()
	{
		if (this.anim)
		{
			this.tpvDisableInFrames = 2;
			this.anim.enabled = true;
		}
	}

	public override void SetInRightHand(Transform _transform)
	{
		base.SetInRightHand(_transform);
		if (base.HeldItemTransform != null)
		{
			if (this.isFPV)
			{
				Utils.SetLayerRecursively(base.HeldItemTransform.gameObject, 10, Utils.ExcludeLayerZoom);
				return;
			}
			Utils.SetLayerRecursively(base.HeldItemTransform.gameObject, 24, Utils.ExcludeLayerZoom);
		}
	}

	public override Transform GetActiveModelRoot()
	{
		if (base.PrimaryBody == null || base.PrimaryBody.Parts == null)
		{
			return null;
		}
		return base.PrimaryBody.Parts.BodyObj.transform;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void avatarVisibilityChanged(BodyAnimator _body, bool _bVisible)
	{
		if (!_bVisible)
		{
			base.avatarVisibilityChanged(_body, _bVisible);
			return;
		}
		if (_body == this.fpsArms)
		{
			_body.State = (this.isFPV ? BodyAnimator.EnumState.Visible : BodyAnimator.EnumState.Disabled);
			return;
		}
		if (_body == base.CharacterBody)
		{
			_body.State = ((!this.isFPV) ? BodyAnimator.EnumState.Visible : BodyAnimator.EnumState.OnlyColliders);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
		base.LateUpdate();
		if (this.tpvDisableInFrames > 0)
		{
			int num = this.tpvDisableInFrames - 1;
			this.tpvDisableInFrames = num;
			if (num == 0 && this.anim && this.isFPV)
			{
				this.anim.enabled = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override BodyAnimator createCharacterBody(Transform _bodyTransform)
	{
		AvatarCharacterController.AnimationStates animationStates = new AvatarCharacterController.AnimationStates(this.getJumpStates(), this.getDeathStates(), this.getReloadStates(), this.getHitStates());
		return new UMACharacterBodyAnimator(base.Entity, animationStates, _bodyTransform, BodyAnimator.EnumState.Disabled);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void initBodyAnimator(BodyAnimator _body, bool _bFPV, bool _bMale)
	{
		base.initBodyAnimator(_body, _bFPV, _bMale);
		if (_body == this.fpsArms)
		{
			this.fpsArms.State = (_bFPV ? BodyAnimator.EnumState.Visible : BodyAnimator.EnumState.Disabled);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual BodyAnimator createFPSArms(Transform _fpsArmsTransform)
	{
		AvatarCharacterController.AnimationStates animationStates = new AvatarCharacterController.AnimationStates(this.getJumpStates(), this.getDeathStates(), this.getReloadStates(), this.getHitStates());
		return new FirstPersonAnimator(base.Entity, animationStates, _fpsArmsTransform, BodyAnimator.EnumState.Disabled);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setTrigger(int _pid, bool _netsync = true)
	{
		base._setTrigger(_pid, _netsync);
		if (base.HeldItemAnimator != null)
		{
			base.HeldItemAnimator.SetTrigger(_pid);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _resetTrigger(int _pid, bool _netsync = true)
	{
		base._resetTrigger(_pid, _netsync);
		if (base.HeldItemAnimator != null)
		{
			base.HeldItemAnimator.ResetTrigger(_pid);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setFloat(int _pid, float _value, bool _netsync = true)
	{
		base._setFloat(_pid, _value, _netsync);
		if (base.HeldItemAnimator != null)
		{
			base.HeldItemAnimator.SetFloat(_pid, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setBool(int _pid, bool _value, bool _netsync = true)
	{
		base._setBool(_pid, _value, _netsync);
		if (base.HeldItemAnimator != null)
		{
			base.HeldItemAnimator.SetBool(_pid, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void _setInt(int _pid, int _value, bool _netsync = true)
	{
		base._setInt(_pid, _value, _netsync);
		if (base.HeldItemAnimator != null)
		{
			base.HeldItemAnimator.SetInteger(_pid, _value);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public BodyAnimator fpsArms;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isMale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isFPV;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int tpvDisableInFrames;
}
