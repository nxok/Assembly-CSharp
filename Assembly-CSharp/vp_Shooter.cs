using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class vp_Shooter : vp_Component
{
	public GameObject ProjectileSpawnPoint
	{
		get
		{
			return this.m_ProjectileSpawnPoint;
		}
	}

	public GameObject MuzzleFlash
	{
		get
		{
			if (this.m_MuzzleFlash == null && this.MuzzleFlashPrefab != null && this.ProjectileSpawnPoint != null)
			{
				this.m_MuzzleFlash = (GameObject)vp_Utility.Instantiate(this.MuzzleFlashPrefab, this.ProjectileSpawnPoint.transform.position, this.ProjectileSpawnPoint.transform.rotation);
				this.m_MuzzleFlash.name = base.transform.name + "MuzzleFlash";
				this.m_MuzzleFlash.transform.parent = this.ProjectileSpawnPoint.transform;
			}
			return this.m_MuzzleFlash;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		base.Awake();
		if (this.m_ProjectileSpawnPoint == null)
		{
			this.m_ProjectileSpawnPoint = base.gameObject;
		}
		this.m_ProjectileDefaultSpawnpoint = this.m_ProjectileSpawnPoint;
		if (this.GetFirePosition == null)
		{
			this.GetFirePosition = () => this.FirePosition;
		}
		if (this.GetFireRotation == null)
		{
			this.GetFireRotation = () => this.m_ProjectileSpawnPoint.transform.rotation;
		}
		if (this.GetFireSeed == null)
		{
			this.GetFireSeed = () => Random.Range(0, 100);
		}
		this.m_CharacterController = this.m_ProjectileSpawnPoint.transform.root.GetComponentInChildren<CharacterController>();
		this.m_NextAllowedFireTime = Time.time;
		this.ProjectileSpawnDelay = Mathf.Min(this.ProjectileSpawnDelay, this.ProjectileFiringRate - 0.1f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Start()
	{
		base.Start();
		base.Audio.playOnAwake = false;
		base.Audio.dopplerLevel = 0f;
		base.RefreshDefaultState();
		this.Refresh();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void LateUpdate()
	{
		this.FirePosition = this.m_ProjectileSpawnPoint.transform.position;
	}

	public virtual bool CanFire()
	{
		return Time.time >= this.m_NextAllowedFireTime;
	}

	public virtual bool TryFire()
	{
		if (Time.time < this.m_NextAllowedFireTime)
		{
			return false;
		}
		this.Fire();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Fire()
	{
		this.m_NextAllowedFireTime = Time.time + this.ProjectileFiringRate;
		if (this.SoundFireDelay == 0f)
		{
			this.PlayFireSound();
		}
		else
		{
			vp_Timer.In(this.SoundFireDelay, new vp_Timer.Callback(this.PlayFireSound), null);
		}
		if (this.ProjectileSpawnDelay == 0f)
		{
			this.SpawnProjectiles();
		}
		else
		{
			vp_Timer.In(this.ProjectileSpawnDelay, delegate
			{
				this.SpawnProjectiles();
			}, null);
		}
		if (this.ShellEjectDelay == 0f)
		{
			this.EjectShell();
		}
		else
		{
			vp_Timer.In(this.ShellEjectDelay, new vp_Timer.Callback(this.EjectShell), null);
		}
		if (this.MuzzleFlashDelay == 0f)
		{
			this.ShowMuzzleFlash();
			return;
		}
		vp_Timer.In(this.MuzzleFlashDelay, new vp_Timer.Callback(this.ShowMuzzleFlash), null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void PlayFireSound()
	{
		if (base.Audio == null)
		{
			return;
		}
		base.Audio.pitch = Random.Range(this.SoundFirePitch.x, this.SoundFirePitch.y) * Time.timeScale;
		base.Audio.clip = this.SoundFire;
		base.Audio.Play();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SpawnProjectiles()
	{
		if (this.ProjectilePrefab == null)
		{
			return;
		}
		if (this.m_SendFireEventToNetworkFunc != null)
		{
			this.m_SendFireEventToNetworkFunc();
		}
		this.m_CurrentFirePosition = this.GetFirePosition();
		this.m_CurrentFireRotation = this.GetFireRotation();
		this.m_CurrentFireSeed = this.GetFireSeed();
		for (int i = 0; i < this.ProjectileCount; i++)
		{
			GameObject gameObject = (GameObject)vp_Utility.Instantiate(this.ProjectilePrefab, this.m_CurrentFirePosition, this.m_CurrentFireRotation);
			gameObject.SendMessage("SetSource", this.ProjectileSourceIsRoot ? base.Root : base.Transform, 1);
			gameObject.transform.localScale = new Vector3(this.ProjectileScale, this.ProjectileScale, this.ProjectileScale);
			this.SetSpread(this.m_CurrentFireSeed * (i + 1), gameObject.transform);
		}
	}

	public void SetSpread(int seed, Transform target)
	{
		Random.InitState(seed);
		target.Rotate(0f, 0f, (float)Random.Range(0, 360));
		target.Rotate(0f, Random.Range(-this.ProjectileSpread, this.ProjectileSpread), 0f);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void ShowMuzzleFlash()
	{
		if (this.MuzzleFlash == null)
		{
			return;
		}
		if (this.m_MuzzleFlashSpawnPoint != null && this.ProjectileSpawnPoint != null)
		{
			this.MuzzleFlash.transform.position = this.m_MuzzleFlashSpawnPoint.transform.position;
			this.MuzzleFlash.transform.rotation = this.ProjectileSpawnPoint.transform.rotation;
		}
		this.MuzzleFlash.SendMessage("Shoot", 1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void EjectShell()
	{
		if (this.ShellPrefab == null)
		{
			return;
		}
		GameObject gameObject = (GameObject)vp_Utility.Instantiate(this.ShellPrefab, (this.m_ShellEjectSpawnPoint == null) ? (this.FirePosition + this.m_ProjectileSpawnPoint.transform.TransformDirection(this.ShellEjectPosition)) : this.m_ShellEjectSpawnPoint.transform.position, this.m_ProjectileSpawnPoint.transform.rotation);
		gameObject.transform.localScale = new Vector3(this.ShellScale, this.ShellScale, this.ShellScale);
		vp_Layer.Set(gameObject.gameObject, 29, false);
		if (gameObject.GetComponent<Rigidbody>())
		{
			Vector3 vector = ((this.m_ShellEjectSpawnPoint == null) ? (base.transform.TransformDirection(this.ShellEjectDirection).normalized * this.ShellEjectVelocity) : (this.m_ShellEjectSpawnPoint.transform.forward.normalized * this.ShellEjectVelocity));
			gameObject.GetComponent<Rigidbody>().AddForce(vector, 1);
		}
		if (this.m_CharacterController)
		{
			Vector3 velocity = this.m_CharacterController.velocity;
			gameObject.GetComponent<Rigidbody>().AddForce(velocity, 2);
		}
		if (this.ShellEjectSpin > 0f)
		{
			if (Random.value > 0.5f)
			{
				gameObject.GetComponent<Rigidbody>().AddRelativeTorque(-Random.rotation.eulerAngles * this.ShellEjectSpin);
				return;
			}
			gameObject.GetComponent<Rigidbody>().AddRelativeTorque(Random.rotation.eulerAngles * this.ShellEjectSpin);
		}
	}

	public virtual void DisableFiring(float seconds = 10000000f)
	{
		this.m_NextAllowedFireTime = Time.time + seconds;
	}

	public virtual void EnableFiring()
	{
		this.m_NextAllowedFireTime = Time.time;
	}

	public override void Refresh()
	{
		if (this.MuzzleFlash != null)
		{
			if (this.m_MuzzleFlashSpawnPoint == null)
			{
				if (this.ProjectileSpawnPoint == this.m_ProjectileDefaultSpawnpoint)
				{
					this.m_MuzzleFlashSpawnPoint = vp_Utility.GetTransformByNameInChildren(this.ProjectileSpawnPoint.transform, "muzzle", false, false);
				}
				else
				{
					this.m_MuzzleFlashSpawnPoint = vp_Utility.GetTransformByNameInChildren(base.Transform, "muzzle", false, false);
				}
			}
			if (this.m_MuzzleFlashSpawnPoint != null)
			{
				if (this.ProjectileSpawnPoint == this.m_ProjectileDefaultSpawnpoint)
				{
					this.m_MuzzleFlash.transform.parent = this.ProjectileSpawnPoint.transform.parent.parent.parent;
				}
				else
				{
					this.m_MuzzleFlash.transform.parent = this.ProjectileSpawnPoint.transform;
				}
			}
			else
			{
				this.m_MuzzleFlash.transform.parent = this.ProjectileSpawnPoint.transform;
				this.MuzzleFlash.transform.localPosition = this.MuzzleFlashPosition;
				this.MuzzleFlash.transform.rotation = this.ProjectileSpawnPoint.transform.rotation;
			}
			this.MuzzleFlash.transform.localScale = this.MuzzleFlashScale;
			this.MuzzleFlash.SendMessage("SetFadeSpeed", this.MuzzleFlashFadeSpeed, 1);
		}
		if (this.ShellPrefab != null && this.m_ShellEjectSpawnPoint == null && this.ProjectileSpawnPoint != null)
		{
			if (this.ProjectileSpawnPoint == this.m_ProjectileDefaultSpawnpoint)
			{
				this.m_ShellEjectSpawnPoint = vp_Utility.GetTransformByNameInChildren(this.ProjectileSpawnPoint.transform, "shell", false, false);
				return;
			}
			this.m_ShellEjectSpawnPoint = vp_Utility.GetTransformByNameInChildren(base.Transform, "shell", false, false);
		}
	}

	public override void Activate()
	{
		base.Activate();
		if (this.MuzzleFlash != null)
		{
			vp_Utility.Activate(this.MuzzleFlash, true);
		}
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if (this.MuzzleFlash != null)
		{
			vp_Utility.Activate(this.MuzzleFlash, false);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void DrawProjectileDebugInfo(int projectileIndex)
	{
		GameObject gameObject = vp_3DUtility.DebugPointer(null);
		gameObject.transform.rotation = this.GetFireRotation();
		gameObject.transform.position = this.GetFirePosition();
		GameObject gameObject2 = vp_3DUtility.DebugBall(null);
		RaycastHit raycastHit;
		if (Physics.Linecast(gameObject.transform.position, gameObject.transform.position + gameObject.transform.forward * 1000f, ref raycastHit, 1084850184) && !raycastHit.collider.isTrigger && base.Root.InverseTransformPoint(raycastHit.point).z > 0f)
		{
			gameObject2.transform.position = raycastHit.point;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CharacterController m_CharacterController;

	public GameObject m_ProjectileSpawnPoint;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameObject m_ProjectileDefaultSpawnpoint;

	public GameObject ProjectilePrefab;

	public float ProjectileScale = 1f;

	public float ProjectileFiringRate = 0.3f;

	public float ProjectileSpawnDelay;

	public int ProjectileCount = 1;

	public float ProjectileSpread;

	public bool ProjectileSourceIsRoot = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float m_NextAllowedFireTime;

	public Vector3 MuzzleFlashPosition = Vector3.zero;

	public Vector3 MuzzleFlashScale = Vector3.one;

	public float MuzzleFlashFadeSpeed = 0.075f;

	public GameObject MuzzleFlashPrefab;

	public float MuzzleFlashDelay;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public GameObject m_MuzzleFlash;

	public Transform m_MuzzleFlashSpawnPoint;

	public GameObject ShellPrefab;

	public float ShellScale = 1f;

	public Vector3 ShellEjectDirection = new Vector3(1f, 1f, 1f);

	public Vector3 ShellEjectPosition = new Vector3(1f, 0f, 1f);

	public float ShellEjectVelocity = 0.2f;

	public float ShellEjectDelay;

	public float ShellEjectSpin;

	public Transform m_ShellEjectSpawnPoint;

	public AudioClip SoundFire;

	public float SoundFireDelay;

	public Vector2 SoundFirePitch = new Vector2(1f, 1f);

	public vp_Shooter.NetworkFunc m_SendFireEventToNetworkFunc;

	public vp_Shooter.FirePositionFunc GetFirePosition;

	public vp_Shooter.FireRotationFunc GetFireRotation;

	public vp_Shooter.FireSeedFunc GetFireSeed;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_CurrentFirePosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Quaternion m_CurrentFireRotation = Quaternion.identity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int m_CurrentFireSeed;

	public Vector3 FirePosition = Vector3.zero;

	public delegate void NetworkFunc();

	public delegate Vector3 FirePositionFunc();

	public delegate Quaternion FireRotationFunc();

	public delegate int FireSeedFunc();
}
