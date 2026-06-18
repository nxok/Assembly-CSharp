using System;
using UnityEngine;

public class vp_FPWeaponThrower : vp_WeaponThrower
{
	public vp_FPWeapon FPWeapon
	{
		get
		{
			if (this.m_FPWeapon == null)
			{
				this.m_FPWeapon = (vp_FPWeapon)base.Transform.GetComponent(typeof(vp_FPWeapon));
			}
			return this.m_FPWeapon;
		}
	}

	public vp_FPWeaponShooter FPWeaponShooter
	{
		get
		{
			if (this.m_FPWeaponShooter == null)
			{
				this.m_FPWeaponShooter = (vp_FPWeaponShooter)base.Transform.GetComponent(typeof(vp_FPWeaponShooter));
			}
			return this.m_FPWeaponShooter;
		}
	}

	public Transform FirePosition
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_FirePosition == null)
			{
				GameObject gameObject = new GameObject("ThrownWeaponFirePosition");
				this.m_FirePosition = gameObject.transform;
				this.m_FirePosition.parent = Camera.main.transform;
			}
			return this.m_FirePosition;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		this.m_OriginalLookDownActive = this.FPWeapon.LookDownActive;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RewindAnimation()
	{
		if (!base.Player.IsFirstPerson.Get())
		{
			return;
		}
		if (this.FPWeapon == null)
		{
			return;
		}
		if (this.FPWeapon.WeaponModel == null)
		{
			return;
		}
		if (this.FPWeapon.WeaponModel.GetComponent<Animation>() == null)
		{
			return;
		}
		if (this.FPWeaponShooter == null)
		{
			return;
		}
		if (this.FPWeaponShooter.AnimationFire == null)
		{
			return;
		}
		this.FPWeapon.WeaponModel.GetComponent<Animation>()[this.FPWeaponShooter.AnimationFire.name].time = 0f;
		this.FPWeapon.WeaponModel.GetComponent<Animation>().Play();
		this.FPWeapon.WeaponModel.GetComponent<Animation>().Sample();
		this.FPWeapon.WeaponModel.GetComponent<Animation>().Stop();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnStart_Attack()
	{
		base.OnStart_Attack();
		if (base.Player.IsFirstPerson.Get())
		{
			base.Shooter.m_ProjectileSpawnPoint = this.FirePosition.gameObject;
			this.FirePosition.localPosition = this.FirePositionOffset;
			this.FirePosition.localEulerAngles = Vector3.zero;
		}
		else
		{
			base.Shooter.m_ProjectileSpawnPoint = base.Weapon.Weapon3rdPersonModel;
		}
		this.FPWeapon.LookDownActive = false;
		vp_Timer.In(base.Shooter.ProjectileSpawnDelay, delegate
		{
			if (!base.HaveAmmoForCurrentWeapon)
			{
				this.FPWeapon.SetState("ReWield", true, false, false);
				this.FPWeapon.Refresh();
				vp_Timer.In(1f, delegate
				{
					if (!base.Player.SetNextWeapon.Try())
					{
						vp_Timer.In(0.5f, delegate
						{
							this.RewindAnimation();
							base.Player.SetWeapon.Start(0f);
						}, this.m_Timer2);
					}
				}, null);
				return;
			}
			if (base.Player.IsFirstPerson.Get())
			{
				this.FPWeapon.SetState("ReWield", true, false, false);
				this.FPWeapon.Refresh();
				vp_Timer.In(1f, delegate
				{
					this.RewindAnimation();
					this.FPWeapon.Rendering = true;
					this.FPWeapon.SetState("ReWield", false, false, false);
					this.FPWeapon.Refresh();
				}, this.m_Timer3);
				return;
			}
			vp_Timer.In(0.5f, delegate
			{
				base.Player.Attack.Stop(0f);
			}, this.m_Timer4);
		}, this.m_Timer1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_SetWeapon()
	{
		this.RewindAnimation();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnStop_Attack()
	{
		base.OnStop_Attack();
		this.FPWeapon.LookDownActive = this.m_OriginalLookDownActive;
	}

	public Vector3 FirePositionOffset = new Vector3(0.35f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_OriginalLookDownActive;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_Timer1 = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_Timer2 = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_Timer3 = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_Timer4 = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPWeapon m_FPWeapon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPWeaponShooter m_FPWeaponShooter;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_FirePosition;
}
