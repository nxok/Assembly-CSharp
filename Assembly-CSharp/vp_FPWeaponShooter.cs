using System;
using UnityEngine;

[RequireComponent(typeof(vp_FPWeapon))]
public class vp_FPWeaponShooter : vp_WeaponShooter
{
	public new vp_FPPlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Player == null && base.EventHandler != null)
			{
				this.m_Player = (vp_FPPlayerEventHandler)base.EventHandler;
			}
			return (vp_FPPlayerEventHandler)this.m_Player;
		}
	}

	public new vp_FPWeapon Weapon
	{
		get
		{
			if (this.m_FPWeapon == null)
			{
				this.m_FPWeapon = base.transform.GetComponent<vp_FPWeapon>();
			}
			return this.m_FPWeapon;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.m_FPCamera = base.transform.root.GetComponentInChildren<vp_FPCamera>();
		if (this.m_ProjectileSpawnPoint == null)
		{
			this.m_ProjectileSpawnPoint = this.m_FPCamera.gameObject;
		}
		this.m_ProjectileDefaultSpawnpoint = this.m_ProjectileSpawnPoint;
		this.m_NextAllowedFireTime = Time.time;
		this.ProjectileSpawnDelay = Mathf.Min(this.ProjectileSpawnDelay, this.ProjectileFiringRate - 0.1f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		this.RefreshFirePoint();
		base.OnEnable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		base.OnDisable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		if (this.ProjectileFiringRate == 0f && this.AnimationFire != null)
		{
			this.ProjectileFiringRate = this.AnimationFire.length;
		}
		if (this.ProjectileFiringRate == 0f && this.AnimationFire != null)
		{
			this.ProjectileFiringRate = this.AnimationFire.length;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Fire()
	{
		this.m_LastFireTime = Time.time;
		if (this.AnimationFire != null)
		{
			if (this.Weapon.WeaponModel.GetComponent<Animation>()[this.AnimationFire.name] == null)
			{
				Debug.LogError(string.Concat(new string[]
				{
					"Error (",
					(this != null) ? this.ToString() : null,
					") No animation named '",
					this.AnimationFire.name,
					"' is listed in this prefab. Make sure the prefab has an 'Animation' component which references all the clips you wish to play on the weapon."
				}));
			}
			else
			{
				this.Weapon.WeaponModel.GetComponent<Animation>()[this.AnimationFire.name].time = 0f;
				this.Weapon.WeaponModel.GetComponent<Animation>().Sample();
				this.Weapon.WeaponModel.GetComponent<Animation>().Play(this.AnimationFire.name);
			}
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
	public override void ApplyRecoil()
	{
		this.Weapon.ResetSprings(this.MotionPositionReset, this.MotionRotationReset, this.MotionPositionPause, this.MotionRotationPause);
		if (this.MotionRotationRecoil.z == 0f)
		{
			this.Weapon.AddForce2(this.MotionPositionRecoil, this.MotionRotationRecoil);
			if (this.MotionPositionRecoilCameraFactor != 0f)
			{
				this.m_FPCamera.AddForce2(this.MotionPositionRecoil * this.MotionPositionRecoilCameraFactor);
				return;
			}
		}
		else
		{
			this.Weapon.AddForce2(this.MotionPositionRecoil, Vector3.Scale(this.MotionRotationRecoil, Vector3.one + Vector3.back) + ((Random.value < 0.5f) ? Vector3.forward : Vector3.back) * Random.Range(this.MotionRotationRecoil.z * this.MotionRotationRecoilDeadZone, this.MotionRotationRecoil.z));
			if (this.MotionPositionRecoilCameraFactor != 0f)
			{
				this.m_FPCamera.AddForce2(this.MotionPositionRecoil * this.MotionPositionRecoilCameraFactor);
			}
			if (this.MotionRotationRecoilCameraFactor != 0f)
			{
				this.m_FPCamera.AddRollForce(Random.Range(this.MotionRotationRecoil.z * this.MotionRotationRecoilDeadZone, this.MotionRotationRecoil.z) * this.MotionRotationRecoilCameraFactor * ((Random.value < 0.5f) ? 1f : (-1f)));
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnMessage_CameraToggle3rdPerson()
	{
		this.RefreshFirePoint();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshFirePoint()
	{
		if (this.Player.IsFirstPerson == null)
		{
			return;
		}
		if (this.Player.IsFirstPerson.Get())
		{
			this.m_ProjectileSpawnPoint = this.m_FPCamera.gameObject;
			if (base.MuzzleFlash != null)
			{
				base.MuzzleFlash.layer = 10;
			}
			this.m_MuzzleFlashSpawnPoint = null;
			this.m_ShellEjectSpawnPoint = null;
			this.Refresh();
		}
		else
		{
			this.m_ProjectileSpawnPoint = this.m_ProjectileDefaultSpawnpoint;
			if (base.MuzzleFlash != null)
			{
				base.MuzzleFlash.layer = 0;
			}
			this.m_MuzzleFlashSpawnPoint = null;
			this.m_ShellEjectSpawnPoint = null;
			this.Refresh();
		}
		if (this.Player.CurrentWeaponName.Get() != base.name)
		{
			this.m_ProjectileSpawnPoint = this.m_FPCamera.gameObject;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPWeapon m_FPWeapon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPCamera m_FPCamera;

	public float MotionPositionReset = 0.5f;

	public float MotionRotationReset = 0.5f;

	public float MotionPositionPause = 1f;

	public float MotionRotationPause = 1f;

	public float MotionRotationRecoilCameraFactor;

	public float MotionPositionRecoilCameraFactor;

	public AnimationClip AnimationFire;
}
