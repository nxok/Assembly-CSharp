using System;
using UnityEngine;

public class vp_Weapon : vp_Component
{
	public bool Wielded
	{
		get
		{
			return this.m_Wielded && base.Rendering;
		}
		set
		{
			this.m_Wielded = value;
		}
	}

	public vp_PlayerEventHandler Player
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Player == null && base.EventHandler != null)
			{
				this.m_Player = (vp_PlayerEventHandler)base.EventHandler;
			}
			return this.m_Player;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		this.RotationOffset = base.transform.localEulerAngles;
		this.PositionOffset = base.transform.position;
		base.Transform.localEulerAngles = this.RotationOffset;
		if (base.transform.parent == null)
		{
			Debug.LogError("Error (" + ((this != null) ? this.ToString() : null) + ") Must not be placed in scene root. Disabling self.");
			vp_Utility.Activate(base.gameObject, false);
			return;
		}
		if (base.GetComponent<Collider>() != null)
		{
			base.GetComponent<Collider>().enabled = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEnable()
	{
		this.RefreshWeaponModel();
		base.OnEnable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDisable()
	{
		this.RefreshWeaponModel();
		this.Activate3rdPersonModel(false);
		base.OnDisable();
	}

	public Vector3 RotationSpring2DefaultRotation
	{
		get
		{
			return this.m_RotationSpring2DefaultRotation;
		}
		set
		{
			this.m_RotationSpring2DefaultRotation = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		this.m_PositionSpring2 = new vp_Spring(base.transform, vp_Spring.UpdateMode.PositionAdditiveSelf, true);
		this.m_PositionSpring2.MinVelocity = 1E-05f;
		this.m_RotationSpring2 = new vp_Spring(base.transform, vp_Spring.UpdateMode.RotationAdditiveGlobal, true);
		this.m_RotationSpring2.MinVelocity = 1E-05f;
		this.SnapSprings();
		this.Refresh();
		base.CacheRenderers();
	}

	public Vector3 Recoil
	{
		get
		{
			return this.m_RotationSpring2.State;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (Time.timeScale == 0f)
		{
			return;
		}
		this.UpdateSprings();
	}

	public virtual void AddForce2(Vector3 positional, Vector3 angular)
	{
		this.m_PositionSpring2.AddForce(positional);
		this.m_RotationSpring2.AddForce(angular);
	}

	public virtual void AddForce2(float xPos, float yPos, float zPos, float xRot, float yRot, float zRot)
	{
		this.AddForce2(new Vector3(xPos, yPos, zPos), new Vector3(xRot, yRot, zRot));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void UpdateSprings()
	{
		base.Transform.localPosition = Vector3.up;
		base.Transform.localRotation = Quaternion.identity;
		this.m_PositionSpring2.FixedUpdate();
		this.m_RotationSpring2.FixedUpdate();
	}

	public override void Refresh()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		if (this.m_PositionSpring2 != null)
		{
			this.m_PositionSpring2.Stiffness = new Vector3(this.PositionSpring2Stiffness, this.PositionSpring2Stiffness, this.PositionSpring2Stiffness);
			this.m_PositionSpring2.Damping = Vector3.one - new Vector3(this.PositionSpring2Damping, this.PositionSpring2Damping, this.PositionSpring2Damping);
			this.m_PositionSpring2.RestState = Vector3.zero;
		}
		if (this.m_RotationSpring2 != null)
		{
			this.m_RotationSpring2.Stiffness = new Vector3(this.RotationSpring2Stiffness, this.RotationSpring2Stiffness, this.RotationSpring2Stiffness);
			this.m_RotationSpring2.Damping = Vector3.one - new Vector3(this.RotationSpring2Damping, this.RotationSpring2Damping, this.RotationSpring2Damping);
			this.m_RotationSpring2.RestState = this.m_RotationSpring2DefaultRotation;
		}
	}

	public override void Activate()
	{
		base.Activate();
		this.m_Wielded = true;
		base.Rendering = true;
	}

	public virtual void SnapSprings()
	{
		if (this.m_PositionSpring2 != null)
		{
			this.m_PositionSpring2.RestState = Vector3.zero;
			this.m_PositionSpring2.State = Vector3.zero;
			this.m_PositionSpring2.Stop(true);
		}
		if (this.m_RotationSpring2 != null)
		{
			this.m_RotationSpring2.RestState = this.m_RotationSpring2DefaultRotation;
			this.m_RotationSpring2.State = this.m_RotationSpring2DefaultRotation;
			this.m_RotationSpring2.Stop(true);
		}
	}

	public virtual void StopSprings()
	{
		if (this.m_PositionSpring2 != null)
		{
			this.m_PositionSpring2.Stop(true);
		}
		if (this.m_RotationSpring2 != null)
		{
			this.m_RotationSpring2.Stop(true);
		}
	}

	public virtual void Wield(bool isWielding = true)
	{
		this.m_Wielded = isWielding;
		this.Refresh();
		base.StateManager.CombineStates();
	}

	public virtual void RefreshWeaponModel()
	{
		if (this.Player == null)
		{
			return;
		}
		vp_Value<bool> isFirstPerson = this.Player.IsFirstPerson;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Activate3rdPersonModel(bool active = true)
	{
		if (this.Weapon3rdPersonModel == null)
		{
			return;
		}
		if (active)
		{
			this.Weapon3rdPersonModel.GetComponent<Renderer>().enabled = true;
			vp_Utility.Activate(this.Weapon3rdPersonModel, true);
			return;
		}
		this.Weapon3rdPersonModel.GetComponent<Renderer>().enabled = false;
		vp_Timer.In(0.1f, delegate
		{
			if (this.Weapon3rdPersonModel != null)
			{
				vp_Utility.Activate(this.Weapon3rdPersonModel, false);
			}
		}, this.m_Weapon3rdPersonModelWakeUpTimer);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStart_Dead()
	{
		if (this.Player.IsFirstPerson.Get())
		{
			return;
		}
		base.Rendering = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnStop_Dead()
	{
		if (this.Player.IsFirstPerson.Get())
		{
			return;
		}
		base.Rendering = true;
	}

	public virtual Vector3 OnValue_AimDirection
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return (this.Weapon3rdPersonModel.transform.position - this.Player.LookPoint.Get()).normalized;
		}
	}

	public GameObject Weapon3rdPersonModel;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameObject m_WeaponModel;

	public Vector3 PositionOffset = new Vector3(0.15f, -0.15f, -0.15f);

	public Vector3 AimingPositionOffset = new Vector3(0f, 0f, 0f);

	public float PositionSpring2Stiffness = 0.95f;

	public float PositionSpring2Damping = 0.25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Spring m_PositionSpring2;

	public Vector3 RotationOffset = Vector3.zero;

	public float RotationSpring2Stiffness = 0.95f;

	public float RotationSpring2Damping = 0.25f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Spring m_RotationSpring2;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Wielded = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_Weapon3rdPersonModelWakeUpTimer = new vp_Timer.Handle();

	public int AnimationType = 1;

	public int AnimationGrip = 1;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_PlayerEventHandler m_Player;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_RotationSpring2DefaultRotation = Vector3.zero;

	public new enum Type
	{
		Custom,
		Firearm,
		Melee,
		Thrown
	}

	public enum Grip
	{
		Custom,
		OneHanded,
		TwoHanded
	}
}
