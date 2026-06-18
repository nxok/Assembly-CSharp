using System;
using UnityEngine;

public class vp_WeaponShooter : vp_Shooter
{
	public vp_PlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_Player == null && base.EventHandler != null)
			{
				this.m_Player = (vp_PlayerEventHandler)base.EventHandler;
			}
			return this.m_Player;
		}
	}

	public vp_Weapon Weapon
	{
		get
		{
			if (this.m_Weapon == null)
			{
				this.m_Weapon = base.transform.GetComponent<vp_Weapon>();
			}
			return this.m_Weapon;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		if (this.m_ProjectileSpawnPoint == null && this.Weapon.Weapon3rdPersonModel != null)
		{
			this.m_ProjectileSpawnPoint = this.Weapon.Weapon3rdPersonModel;
		}
		if (this.GetFireSeed == null)
		{
			this.GetFireSeed = () => Random.Range(0, 100);
		}
		if (this.GetFirePosition == null)
		{
			this.GetFirePosition = () => this.FirePosition;
		}
		if (this.GetFireRotation == null)
		{
			this.GetFireRotation = delegate
			{
				Quaternion quaternion = Quaternion.identity;
				if (this.Player.LookPoint.Get() - this.FirePosition != Vector3.zero)
				{
					quaternion = vp_MathUtility.NaNSafeQuaternion(Quaternion.LookRotation(this.Player.LookPoint.Get() - this.FirePosition), default(Quaternion));
				}
				return quaternion;
			};
		}
		base.Awake();
		this.m_NextAllowedFireTime = Time.time;
		this.ProjectileSpawnDelay = Mathf.Min(this.ProjectileSpawnDelay, this.ProjectileFiringRate - 0.1f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
		if (this.Player == null)
		{
			return;
		}
		if (this.Player.IsFirstPerson == null)
		{
			return;
		}
		if (!this.Player.IsFirstPerson.Get() && this.m_3rdPersonFiredThisFrame)
		{
			this.m_3rdPersonFiredThisFrame = false;
		}
		this.m_WeaponWasInAttackStateLastFrame = this.Weapon.StateManager.IsEnabled("Attack");
		base.LateUpdate();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Fire()
	{
		if (vp_Gameplay.isMultiplayer && !this.Player.IsLocal.Get())
		{
			this.ProjectileSpawnDelay = 0f;
		}
		this.m_LastFireTime = Time.time;
		if (!this.Player.IsFirstPerson.Get())
		{
			this.m_3rdPersonFiredThisFrame = true;
		}
		if (this.MotionRecoilDelay == 0f)
		{
			this.ApplyRecoil();
		}
		else
		{
			vp_Timer.In(this.MotionRecoilDelay, new vp_Timer.Callback(this.ApplyRecoil), null);
		}
		base.Fire();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void ShowMuzzleFlash()
	{
		if (this.m_MuzzleFlash == null)
		{
			return;
		}
		if (this.MuzzleFlashFirstShotMaxDeviation == 180f || this.Player.IsFirstPerson.Get() || this.m_WeaponWasInAttackStateLastFrame)
		{
			base.ShowMuzzleFlash();
			return;
		}
		this.m_MuzzleFlashWeaponAngle = base.Transform.eulerAngles.x + 90f;
		this.m_MuzzleFlashFireAngle = this.m_CurrentFireRotation.eulerAngles.x + 90f;
		this.m_MuzzleFlashWeaponAngle = ((this.m_MuzzleFlashWeaponAngle >= 360f) ? (this.m_MuzzleFlashWeaponAngle - 360f) : this.m_MuzzleFlashWeaponAngle);
		this.m_MuzzleFlashFireAngle = ((this.m_MuzzleFlashFireAngle >= 360f) ? (this.m_MuzzleFlashFireAngle - 360f) : this.m_MuzzleFlashFireAngle);
		if (Mathf.Abs(this.m_MuzzleFlashWeaponAngle - this.m_MuzzleFlashFireAngle) > this.MuzzleFlashFirstShotMaxDeviation)
		{
			this.m_MuzzleFlash.SendMessage("ShootLightOnly", 1);
			return;
		}
		base.ShowMuzzleFlash();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ApplyRecoil()
	{
		if (this.MotionRotationRecoil.z == 0f)
		{
			this.Weapon.AddForce2(this.MotionPositionRecoil, this.MotionRotationRecoil);
			return;
		}
		this.Weapon.AddForce2(this.MotionPositionRecoil, Vector3.Scale(this.MotionRotationRecoil, Vector3.one + Vector3.back) + ((Random.value < 0.5f) ? Vector3.forward : Vector3.back) * Random.Range(this.MotionRotationRecoil.z * this.MotionRotationRecoilDeadZone, this.MotionRotationRecoil.z));
	}

	public virtual void DryFire()
	{
		if (base.Audio != null)
		{
			base.Audio.pitch = Time.timeScale;
			base.Audio.PlayOneShot(this.SoundDryFire);
		}
		this.DisableFiring(10000000f);
		this.m_LastFireTime = Time.time;
		this.Weapon.AddForce2(this.MotionPositionRecoil * this.MotionDryFireRecoil, this.MotionRotationRecoil * this.MotionDryFireRecoil);
	}

	public void OnMessage_DryFire()
	{
		this.DryFire();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Attack()
	{
		if (this.ProjectileFiringRate == 0f)
		{
			this.EnableFiring();
			return;
		}
		this.DisableFiring(this.ProjectileTapFiringRate - (Time.time - this.m_LastFireTime));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool OnAttempt_Fire()
	{
		if (Time.time < this.m_NextAllowedFireTime)
		{
			return false;
		}
		if (!this.Player.DepleteAmmo.Try())
		{
			this.DryFire();
			return false;
		}
		this.Fire();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Weapon m_Weapon;

	public float ProjectileTapFiringRate = 0.1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_LastFireTime;

	public Vector3 MotionPositionRecoil = new Vector3(0f, 0f, -0.035f);

	public Vector3 MotionRotationRecoil = new Vector3(-10f, 0f, 0f);

	public float MotionRotationRecoilDeadZone = 0.5f;

	public float MotionDryFireRecoil = -0.1f;

	public float MotionRecoilDelay;

	public float MuzzleFlashFirstShotMaxDeviation = 180f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_WeaponWasInAttackStateLastFrame;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_MuzzleFlashWeaponAngle;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_MuzzleFlashFireAngle;

	public AudioClip SoundDryFire;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Quaternion m_MuzzlePointRotation = Quaternion.identity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_3rdPersonFiredThisFrame;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_PlayerEventHandler m_Player;
}
