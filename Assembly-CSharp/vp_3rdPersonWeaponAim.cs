using System;
using UnityEngine;

public class vp_3rdPersonWeaponAim : MonoBehaviour
{
	public Transform Transform
	{
		get
		{
			if (this.m_Transform == null)
			{
				this.m_Transform = base.transform;
			}
			return this.m_Transform;
		}
	}

	public vp_PlayerEventHandler Player
	{
		get
		{
			if (this.m_Player == null)
			{
				this.m_Player = (vp_PlayerEventHandler)this.Root.GetComponentInChildren(typeof(vp_PlayerEventHandler));
			}
			return this.m_Player;
		}
	}

	public vp_WeaponHandler WeaponHandler
	{
		get
		{
			if (this.m_WeaponHandler == null)
			{
				this.m_WeaponHandler = (vp_WeaponHandler)this.Root.GetComponentInChildren(typeof(vp_WeaponHandler));
			}
			return this.m_WeaponHandler;
		}
	}

	public Animator Animator
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Animator == null)
			{
				this.m_Animator = this.Root.GetComponentInChildren<Animator>();
			}
			return this.m_Animator;
		}
	}

	public Transform Root
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_Root == null)
			{
				this.m_Root = this.Transform.root;
			}
			return this.m_Root;
		}
	}

	public Transform LowerArmObj
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_LowerArmObj == null)
			{
				this.m_LowerArmObj = this.HandObj.parent;
			}
			return this.m_LowerArmObj;
		}
	}

	public Transform HandObj
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.m_HandObj == null)
			{
				if (this.Hand != null)
				{
					this.m_HandObj = this.Hand.transform;
				}
				else
				{
					this.m_HandObj = vp_Utility.GetTransformByNameInAncestors(this.Transform, "hand", true, true);
					if (this.m_HandObj == null && this.Transform.parent != null)
					{
						this.m_HandObj = this.Transform.parent;
					}
					if (this.m_HandObj != null)
					{
						this.Hand = this.m_HandObj.gameObject;
					}
				}
			}
			return this.m_HandObj;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		if (this.Player != null)
		{
			this.Player.Register(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		if (this.Player != null)
		{
			this.Player.Unregister(this);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.m_DefaultRotation = this.Transform.localRotation;
		if (this.LowerArmObj == null || this.HandObj == null)
		{
			Debug.LogError("Hierarchy Error (" + ((this != null) ? this.ToString() : null) + ") This script should be placed on a 3rd person weapon gameobject childed to a hand bone in a rigged character.");
			base.enabled = false;
			return;
		}
		Quaternion quaternion = Quaternion.Inverse(this.LowerArmObj.rotation);
		this.m_ReferenceLookDir = quaternion * this.Root.rotation * Vector3.forward;
		this.m_ReferenceUpDir = quaternion * this.Root.rotation * Vector3.up;
		Quaternion rotation = this.HandObj.rotation;
		this.HandObj.rotation = this.Root.rotation;
		Quaternion rotation2 = this.HandObj.rotation;
		this.HandObj.rotation = rotation;
		this.m_HandBoneRotDif = Quaternion.Inverse(rotation2) * rotation;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void LateUpdate()
	{
		if (Time.timeScale == 0f)
		{
			return;
		}
		this.UpdateAiming();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateAiming()
	{
		if (this.Animator == null)
		{
			return;
		}
		if ((!this.Animator.GetBool("IsAttacking") && !this.Animator.GetBool("IsZooming")) || this.Animator.GetBool("IsReloading") || this.Animator.GetBool("IsOutOfControl") || this.Player.CurrentWeaponIndex.Get() == 0)
		{
			this.Transform.localRotation = this.m_DefaultRotation;
			return;
		}
		Quaternion rotation = this.Transform.rotation;
		this.Transform.rotation = Quaternion.LookRotation(this.Player.AimDirection.Get());
		this.m_WorldDir = this.Transform.forward;
		this.Transform.rotation = rotation;
		this.HandObj.rotation = vp_3DUtility.GetBoneLookRotationInWorldSpace(this.HandObj.rotation, this.LowerArmObj.rotation, this.m_WorldDir, 1f, this.m_ReferenceUpDir, this.m_ReferenceLookDir, this.m_HandBoneRotDif);
		this.HandObj.Rotate(this.Transform.forward, this.AngleAdjustZ + this.WeaponHandler.CurrentWeapon.Recoil.z * this.RecoilFactorZ, 0);
		this.HandObj.Rotate(this.Transform.up, this.AngleAdjustY + this.WeaponHandler.CurrentWeapon.Recoil.y * this.RecoilFactorY, 0);
		this.HandObj.Rotate(this.Transform.right, this.AngleAdjustX + this.WeaponHandler.CurrentWeapon.Recoil.x * this.RecoilFactorX, 0);
	}

	public GameObject Hand;

	[Range(0f, 360f)]
	public float AngleAdjustX;

	[Range(0f, 360f)]
	public float AngleAdjustY;

	[Range(0f, 360f)]
	public float AngleAdjustZ;

	[Range(0f, 5f)]
	public float RecoilFactorX = 1f;

	[Range(0f, 5f)]
	public float RecoilFactorY = 1f;

	[Range(0f, 5f)]
	public float RecoilFactorZ = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Quaternion m_DefaultRotation;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_ReferenceUpDir;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_ReferenceLookDir;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Quaternion m_HandBoneRotDif;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_WorldDir = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_PlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public vp_WeaponHandler m_WeaponHandler;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Animator m_Animator;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform m_Root;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform m_LowerArmObj;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform m_HandObj;
}
