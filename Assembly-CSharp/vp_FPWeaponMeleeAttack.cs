using System;
using System.Collections.Generic;
using UnityEngine;

public class vp_FPWeaponMeleeAttack : vp_Component
{
	public vp_FPPlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_Player == null && base.EventHandler != null)
			{
				this.m_Player = (vp_FPPlayerEventHandler)base.EventHandler;
			}
			return this.m_Player;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		this.m_Controller = (vp_FPController)base.Root.GetComponent(typeof(vp_FPController));
		this.m_Camera = (vp_FPCamera)base.Root.GetComponentInChildren(typeof(vp_FPCamera));
		this.m_Weapon = (vp_FPWeapon)base.Transform.GetComponent(typeof(vp_FPWeapon));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		this.UpdateAttack();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void UpdateAttack()
	{
		if (!this.Player.Attack.Active)
		{
			return;
		}
		if (this.Player.SetWeapon.Active)
		{
			return;
		}
		if (this.m_Weapon == null)
		{
			return;
		}
		if (!this.m_Weapon.Wielded)
		{
			return;
		}
		if (Time.time < this.m_NextAllowedSwingTime)
		{
			return;
		}
		this.m_NextAllowedSwingTime = Time.time + this.SwingRate;
		if (this.AttackPickRandomState)
		{
			this.PickAttack();
		}
		this.m_Weapon.SetState(this.WeaponStatePull, true, false, false);
		this.m_Weapon.Refresh();
		vp_Timer.In(this.SwingDelay, delegate
		{
			if (this.SoundSwing.Count > 0)
			{
				base.Audio.pitch = Random.Range(this.SoundSwingPitch.x, this.SoundSwingPitch.y) * Time.timeScale;
				base.Audio.clip = (AudioClip)this.SoundSwing[Random.Range(0, this.SoundSwing.Count)];
				base.Audio.Play();
			}
			this.m_Weapon.SetState(this.WeaponStatePull, false, false, false);
			this.m_Weapon.SetState(this.WeaponStateSwing, true, false, false);
			this.m_Weapon.Refresh();
			this.m_Weapon.AddSoftForce(this.SwingPositionSoftForce, this.SwingRotationSoftForce, this.SwingSoftForceFrames);
			vp_Timer.In(this.ImpactTime, delegate
			{
				RaycastHit raycastHit;
				Physics.SphereCast(new Ray(new Vector3(this.m_Controller.Transform.position.x, this.m_Camera.Transform.position.y, this.m_Controller.Transform.position.z), this.m_Camera.Transform.forward), this.DamageRadius, ref raycastHit, this.DamageRange, -538750989);
				if (raycastHit.collider != null)
				{
					this.SpawnImpactFX(raycastHit);
					this.ApplyDamage(raycastHit);
					this.ApplyRecoil();
					return;
				}
				vp_Timer.In(this.SwingDuration - this.ImpactTime, delegate
				{
					this.m_Weapon.StopSprings();
					this.Reset();
				}, this.SwingDurationTimer);
			}, this.ImpactTimer);
		}, this.SwingDelayTimer);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PickAttack()
	{
		int num = this.States.Count - 1;
		do
		{
			num = Random.Range(0, this.States.Count - 1);
		}
		while (this.States.Count > 1 && num == this.m_AttackCurrent && Random.value < 0.5f);
		this.m_AttackCurrent = num;
		base.SetState(this.States[this.m_AttackCurrent].Name, true, false, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Attack()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SpawnImpactFX(RaycastHit hit)
	{
		Quaternion quaternion = Quaternion.LookRotation(hit.normal);
		if (this.m_DustPrefab != null)
		{
			vp_Utility.Instantiate(this.m_DustPrefab, hit.point, quaternion);
		}
		if (this.m_SparkPrefab != null && Random.value < this.SparkFactor)
		{
			vp_Utility.Instantiate(this.m_SparkPrefab, hit.point, quaternion);
		}
		if (this.m_DebrisPrefab != null)
		{
			vp_Utility.Instantiate(this.m_DebrisPrefab, hit.point, quaternion);
		}
		if (this.SoundImpact.Count > 0)
		{
			base.Audio.pitch = Random.Range(this.SoundImpactPitch.x, this.SoundImpactPitch.y) * Time.timeScale;
			base.Audio.PlayOneShot((AudioClip)this.SoundImpact[Random.Range(0, this.SoundImpact.Count)]);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyDamage(RaycastHit hit)
	{
		hit.collider.SendMessage(this.DamageMethodName, this.Damage, 1);
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (attachedRigidbody != null && !attachedRigidbody.isKinematic)
		{
			attachedRigidbody.AddForceAtPosition(this.m_Camera.Transform.forward * this.DamageForce / Time.timeScale / vp_TimeUtility.AdjustedTimeScale, hit.point);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ApplyRecoil()
	{
		this.m_Weapon.StopSprings();
		this.m_Weapon.AddForce(this.ImpactPositionSpringRecoil, this.ImpactRotationSpringRecoil);
		this.m_Weapon.AddForce2(this.ImpactPositionSpring2Recoil, this.ImpactRotationSpring2Recoil);
		this.Reset();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Reset()
	{
		vp_Timer.In(0.05f, delegate
		{
			if (this.m_Weapon != null)
			{
				this.m_Weapon.SetState(this.WeaponStatePull, false, false, false);
				this.m_Weapon.SetState(this.WeaponStateSwing, false, false, false);
				this.m_Weapon.Refresh();
				if (this.AttackPickRandomState)
				{
					base.ResetState();
				}
			}
		}, this.ResetTimer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPWeapon m_Weapon;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPController m_Controller;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_FPCamera m_Camera;

	public string WeaponStatePull = "Pull";

	public string WeaponStateSwing = "Swing";

	public float SwingDelay = 0.5f;

	public float SwingDuration = 0.5f;

	public float SwingRate = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_NextAllowedSwingTime;

	public int SwingSoftForceFrames = 50;

	public Vector3 SwingPositionSoftForce = new Vector3(-0.5f, -0.1f, 0.3f);

	public Vector3 SwingRotationSoftForce = new Vector3(50f, -25f, 0f);

	public float ImpactTime = 0.11f;

	public Vector3 ImpactPositionSpringRecoil = new Vector3(0.01f, 0.03f, -0.05f);

	public Vector3 ImpactPositionSpring2Recoil = Vector3.zero;

	public Vector3 ImpactRotationSpringRecoil = Vector3.zero;

	public Vector3 ImpactRotationSpring2Recoil = new Vector3(0f, 0f, 10f);

	public string DamageMethodName = "Damage";

	public float Damage = 5f;

	public float DamageRadius = 0.3f;

	public float DamageRange = 2f;

	public float DamageForce = 1000f;

	public bool AttackPickRandomState = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int m_AttackCurrent;

	public float SparkFactor = 0.1f;

	public GameObject m_DustPrefab;

	public GameObject m_SparkPrefab;

	public GameObject m_DebrisPrefab;

	public List<Object> SoundSwing = new List<Object>();

	public List<Object> SoundImpact = new List<Object>();

	public Vector2 SoundSwingPitch = new Vector2(0.5f, 1.5f);

	public Vector2 SoundImpactPitch = new Vector2(1f, 1.5f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle SwingDelayTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle ImpactTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle SwingDurationTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_Timer.Handle ResetTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_FPPlayerEventHandler m_Player;
}
