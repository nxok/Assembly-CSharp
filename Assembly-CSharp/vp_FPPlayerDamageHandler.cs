using System;
using UnityEngine;

[RequireComponent(typeof(vp_FPPlayerEventHandler))]
public class vp_FPPlayerDamageHandler : vp_PlayerDamageHandler
{
	public vp_FPPlayerEventHandler FPPlayer
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_FPPlayer == null)
			{
				this.m_FPPlayer = base.transform.GetComponent<vp_FPPlayerEventHandler>();
			}
			return this.m_FPPlayer;
		}
	}

	public vp_FPCamera FPCamera
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_FPCamera == null)
			{
				this.m_FPCamera = base.transform.GetComponentInChildren<vp_FPCamera>();
			}
			return this.m_FPCamera;
		}
	}

	public CharacterController CharacterController
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_CharacterController == null)
			{
				this.m_CharacterController = base.transform.root.GetComponentInChildren<CharacterController>();
			}
			return this.m_CharacterController;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		if (this.FPPlayer != null)
		{
			this.FPPlayer.Register(this);
		}
		this.RefreshColliders();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		if (this.FPPlayer != null)
		{
			this.FPPlayer.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Update()
	{
		if (this.FPPlayer.Dead.Active && Time.timeScale < 1f)
		{
			vp_TimeUtility.FadeTimeScale(1f, 0.05f);
		}
	}

	public override void Damage(float damage)
	{
		if (!base.enabled)
		{
			return;
		}
		if (!vp_Utility.IsActive(base.gameObject))
		{
			return;
		}
		base.Damage(damage);
		this.FPPlayer.HUDDamageFlash.Send(new vp_DamageInfo(damage, null));
		this.FPPlayer.HeadImpact.Send((Random.value < 0.5f) ? (damage * this.CameraShakeFactor) : (-(damage * this.CameraShakeFactor)));
	}

	public override void Damage(vp_DamageInfo damageInfo)
	{
		if (!base.enabled)
		{
			return;
		}
		if (!vp_Utility.IsActive(base.gameObject))
		{
			return;
		}
		base.Damage(damageInfo);
		this.FPPlayer.HUDDamageFlash.Send(damageInfo);
		if (damageInfo.Source != null)
		{
			this.m_DamageAngle = vp_3DUtility.LookAtAngleHorizontal(this.FPCamera.Transform.position, this.FPCamera.Transform.forward, damageInfo.Source.position);
			this.m_DamageAngleFactor = ((Mathf.Abs(this.m_DamageAngle) > 30f) ? 1f : Mathf.Lerp(0f, 1f, Mathf.Abs(this.m_DamageAngle) * 0.033f));
			this.FPPlayer.HeadImpact.Send(damageInfo.Damage * this.CameraShakeFactor * this.m_DamageAngleFactor * (float)((this.m_DamageAngle < 0f) ? 1 : (-1)));
		}
	}

	public override void Die()
	{
		base.Die();
		if (!base.enabled || !vp_Utility.IsActive(base.gameObject))
		{
			return;
		}
		this.FPPlayer.InputAllowGameplay.Set(false);
	}

	public virtual void RefreshColliders()
	{
		if (this.CharacterController != null && this.CharacterController.enabled)
		{
			foreach (Collider collider in base.Colliders)
			{
				if (collider.enabled)
				{
					Physics.IgnoreCollision(this.CharacterController, collider, true);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Reset()
	{
		base.Reset();
		if (!Application.isPlaying)
		{
			return;
		}
		this.FPPlayer.InputAllowGameplay.Set(true);
		this.FPPlayer.HUDDamageFlash.Send(null);
		this.RefreshColliders();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStart_Crouch()
	{
		this.RefreshColliders();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnStop_Crouch()
	{
		this.RefreshColliders();
	}

	public float CameraShakeFactor = 0.02f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_DamageAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float m_DamageAngleFactor = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_FPPlayer;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPCamera m_FPCamera;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CharacterController m_CharacterController;
}
