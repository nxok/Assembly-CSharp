using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class vp_DamageHandler : MonoBehaviour
{
	public static Dictionary<Collider, vp_DamageHandler> DamageHandlersByCollider
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (vp_DamageHandler.m_DamageHandlersByCollider == null)
			{
				vp_DamageHandler.m_DamageHandlersByCollider = new Dictionary<Collider, vp_DamageHandler>(100);
			}
			return vp_DamageHandler.m_DamageHandlersByCollider;
		}
	}

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

	public vp_Respawner Respawner
	{
		get
		{
			if (this.m_Respawner == null)
			{
				this.m_Respawner = base.GetComponent<vp_Respawner>();
			}
			return this.m_Respawner;
		}
	}

	public Transform Source
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Source == null)
			{
				this.m_Source = this.Transform;
			}
			return this.m_Source;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_Source = value;
		}
	}

	public Transform OriginalSource
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_OriginalSource == null)
			{
				this.m_OriginalSource = this.Transform;
			}
			return this.m_OriginalSource;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.m_OriginalSource = value;
		}
	}

	[Obsolete("This property will be removed in an upcoming release.")]
	public Transform Sender
	{
		get
		{
			return this.Source;
		}
		set
		{
			this.Source = value;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.m_Audio = base.GetComponent<AudioSource>();
		this.CurrentHealth = this.MaxHealth;
		this.CheckForObsoleteParams();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.NotifyLevelWasLoaded);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.NotifyLevelWasLoaded);
	}

	public virtual void Damage(float damage)
	{
		this.Damage(new vp_DamageInfo(damage, null));
	}

	public virtual void Damage(vp_DamageInfo damageInfo)
	{
		if (!base.enabled)
		{
			return;
		}
		if (!vp_Utility.IsActive(base.gameObject))
		{
			return;
		}
		if (this.CurrentHealth <= 0f)
		{
			return;
		}
		if (damageInfo != null)
		{
			if (damageInfo.Source != null)
			{
				this.Source = damageInfo.Source;
			}
			if (damageInfo.OriginalSource != null)
			{
				this.OriginalSource = damageInfo.OriginalSource;
			}
		}
		this.CurrentHealth = Mathf.Min(this.CurrentHealth - damageInfo.Damage, this.MaxHealth);
		if (vp_Gameplay.isMaster)
		{
			if (vp_Gameplay.isMultiplayer && damageInfo.Source != null)
			{
				vp_GlobalEvent<Transform, Transform, float>.Send("Damage", this.Transform.root, damageInfo.OriginalSource, damageInfo.Damage, vp_GlobalEventMode.REQUIRE_LISTENER);
			}
			if (this.CurrentHealth <= 0f)
			{
				if (this.m_InstaKill)
				{
					base.SendMessage("Die");
					return;
				}
				vp_Timer.In(Random.Range(this.MinDeathDelay, this.MaxDeathDelay), delegate
				{
					base.SendMessage("Die");
				}, null);
			}
		}
	}

	public virtual void DieBySources(Transform[] sourceAndOriginalSource)
	{
		if (sourceAndOriginalSource.Length != 2)
		{
			Debug.LogWarning("Warning (" + ((this != null) ? this.ToString() : null) + ") 'DieBySources' argument must contain 2 transforms.");
			return;
		}
		this.Source = sourceAndOriginalSource[0];
		this.OriginalSource = sourceAndOriginalSource[1];
		this.Die();
	}

	public virtual void DieBySource(Transform source)
	{
		this.Source = source;
		this.OriginalSource = source;
		this.Die();
	}

	public virtual void Die()
	{
		if (!base.enabled || !vp_Utility.IsActive(base.gameObject))
		{
			return;
		}
		if (this.m_Audio != null)
		{
			this.m_Audio.pitch = Time.timeScale;
			this.m_Audio.PlayOneShot(this.DeathSound);
		}
		foreach (GameObject gameObject in this.DeathSpawnObjects)
		{
			if (gameObject != null)
			{
				GameObject gameObject2 = (GameObject)vp_Utility.Instantiate(gameObject, this.Transform.position, this.Transform.rotation);
				if (this.Source != null && gameObject2 != null)
				{
					vp_TargetEvent<Transform>.Send(gameObject2.transform, "SetSource", this.OriginalSource, vp_TargetEventOptions.DontRequireReceiver);
				}
			}
		}
		if (this.Respawner == null)
		{
			vp_Utility.Destroy(base.gameObject);
		}
		else
		{
			this.RemoveBulletHoles();
			vp_Utility.Activate(base.gameObject, false);
		}
		this.m_InstaKill = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Reset()
	{
		this.CurrentHealth = this.MaxHealth;
		this.Source = null;
		this.OriginalSource = null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RemoveBulletHoles()
	{
		vp_HitscanBullet[] componentsInChildren = base.GetComponentsInChildren<vp_HitscanBullet>(true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			vp_Utility.Destroy(componentsInChildren[i].gameObject);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnCollisionEnter(Collision collision)
	{
		float num = collision.relativeVelocity.sqrMagnitude * 0.1f;
		float num2 = ((num > this.ImpactDamageThreshold) ? (num * this.ImpactDamageMultiplier) : 0f);
		if (num2 <= 0f)
		{
			return;
		}
		if (this.CurrentHealth - num2 <= 0f)
		{
			this.m_InstaKill = true;
		}
		this.Damage(num2);
	}

	public static vp_DamageHandler GetDamageHandlerOfCollider(Collider col)
	{
		if (!vp_DamageHandler.DamageHandlersByCollider.TryGetValue(col, out vp_DamageHandler.m_GetDamageHandlerOfColliderResult))
		{
			vp_DamageHandler.m_GetDamageHandlerOfColliderResult = col.transform.root.GetComponentInChildren<vp_DamageHandler>();
			vp_DamageHandler.DamageHandlersByCollider.Add(col, vp_DamageHandler.m_GetDamageHandlerOfColliderResult);
		}
		return vp_DamageHandler.m_GetDamageHandlerOfColliderResult;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void NotifyLevelWasLoaded(Scene scene, LoadSceneMode mode)
	{
		vp_DamageHandler.DamageHandlersByCollider.Clear();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Respawn()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Reactivate()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CheckForObsoleteParams()
	{
		if (this.DeathEffect != null)
		{
			Debug.LogWarning(((this != null) ? this.ToString() : null) + "'DeathEffect' is obsolete! Please use the 'DeathSpawnObjects' array instead.");
		}
		string text = "";
		if (this.Respawns)
		{
			text += "Respawns, ";
		}
		if (this.MinRespawnTime != -99999f)
		{
			text += "MinRespawnTime, ";
		}
		if (this.MaxRespawnTime != -99999f)
		{
			text += "MaxRespawnTime, ";
		}
		if (this.RespawnCheckRadius != -99999f)
		{
			text += "RespawnCheckRadius, ";
		}
		if (this.RespawnSound != null)
		{
			text += "RespawnSound, ";
		}
		if (text != "")
		{
			text = text.Remove(text.LastIndexOf(", "));
			Debug.LogWarning(string.Format("Warning + (" + ((this != null) ? this.ToString() : null) + ") The following parameters are obsolete: \"{0}\". Creating a temp vp_Respawner component. To remove this warning, see the UFPS menu -> Wizards -> Convert Old DamageHandlers.", text));
			this.CreateTempRespawner();
		}
	}

	public bool CreateTempRespawner()
	{
		if (base.GetComponent<vp_Respawner>() || base.GetComponent<vp_PlayerRespawner>())
		{
			this.DisableOldParams();
			return false;
		}
		vp_DamageHandler.CreateRespawnerForDamageHandler(this);
		this.DisableOldParams();
		return true;
	}

	public static int GenerateRespawnersForAllDamageHandlers()
	{
		vp_PlayerDamageHandler[] array = Object.FindObjectsByType<vp_PlayerDamageHandler>(0);
		if (array != null && array.Length != 0)
		{
			foreach (vp_PlayerDamageHandler vp_PlayerDamageHandler in array)
			{
				if (!(vp_PlayerDamageHandler.transform.GetComponent<vp_FPPlayerEventHandler>() == null))
				{
					vp_FPPlayerDamageHandler vp_FPPlayerDamageHandler = vp_PlayerDamageHandler.gameObject.AddComponent<vp_FPPlayerDamageHandler>();
					vp_FPPlayerDamageHandler.AllowFallDamage = vp_PlayerDamageHandler.AllowFallDamage;
					vp_FPPlayerDamageHandler.DeathEffect = vp_PlayerDamageHandler.DeathEffect;
					vp_FPPlayerDamageHandler.DeathSound = vp_PlayerDamageHandler.DeathSound;
					vp_FPPlayerDamageHandler.DeathSpawnObjects = vp_PlayerDamageHandler.DeathSpawnObjects;
					vp_FPPlayerDamageHandler.FallImpactPitch = vp_PlayerDamageHandler.FallImpactPitch;
					vp_FPPlayerDamageHandler.FallImpactSounds = vp_PlayerDamageHandler.FallImpactSounds;
					vp_FPPlayerDamageHandler.FallImpactThreshold = vp_PlayerDamageHandler.FallImpactThreshold;
					vp_FPPlayerDamageHandler.ImpactDamageMultiplier = vp_PlayerDamageHandler.ImpactDamageMultiplier;
					vp_FPPlayerDamageHandler.ImpactDamageThreshold = vp_PlayerDamageHandler.ImpactDamageThreshold;
					vp_FPPlayerDamageHandler.m_Audio = vp_PlayerDamageHandler.m_Audio;
					vp_FPPlayerDamageHandler.CurrentHealth = vp_PlayerDamageHandler.CurrentHealth;
					vp_FPPlayerDamageHandler.m_StartPosition = vp_PlayerDamageHandler.m_StartPosition;
					vp_FPPlayerDamageHandler.m_StartRotation = vp_PlayerDamageHandler.m_StartRotation;
					vp_FPPlayerDamageHandler.MaxDeathDelay = vp_PlayerDamageHandler.MaxDeathDelay;
					vp_FPPlayerDamageHandler.MaxHealth = vp_PlayerDamageHandler.MaxHealth;
					vp_FPPlayerDamageHandler.MaxRespawnTime = vp_PlayerDamageHandler.MaxRespawnTime;
					vp_FPPlayerDamageHandler.MinDeathDelay = vp_PlayerDamageHandler.MinDeathDelay;
					vp_FPPlayerDamageHandler.MinRespawnTime = vp_PlayerDamageHandler.MinRespawnTime;
					vp_FPPlayerDamageHandler.RespawnCheckRadius = vp_PlayerDamageHandler.RespawnCheckRadius;
					vp_FPPlayerDamageHandler.Respawns = vp_PlayerDamageHandler.Respawns;
					vp_FPPlayerDamageHandler.RespawnSound = vp_PlayerDamageHandler.RespawnSound;
					Object.DestroyImmediate(vp_PlayerDamageHandler);
				}
			}
		}
		vp_DamageHandler[] array3 = Object.FindObjectsByType<vp_DamageHandler>(0);
		vp_DamageHandler[] array4 = Object.FindObjectsByType<vp_FPPlayerDamageHandler>(0);
		vp_DamageHandler[] array5 = array4;
		int num = 0;
		array4 = array3;
		for (int i = 0; i < array4.Length; i++)
		{
			if (array4[i].CreateTempRespawner())
			{
				num++;
			}
		}
		array4 = array5;
		for (int i = 0; i < array4.Length; i++)
		{
			if (array4[i].CreateTempRespawner())
			{
				num++;
			}
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DisableOldParams()
	{
		this.Respawns = false;
		this.MinRespawnTime = -99999f;
		this.MaxRespawnTime = -99999f;
		this.RespawnCheckRadius = -99999f;
		this.RespawnSound = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void CreateRespawnerForDamageHandler(vp_DamageHandler damageHandler)
	{
		if (damageHandler.gameObject.GetComponent<vp_Respawner>() || damageHandler.gameObject.GetComponent<vp_PlayerRespawner>())
		{
			return;
		}
		vp_Respawner vp_Respawner;
		if (damageHandler is vp_FPPlayerDamageHandler)
		{
			vp_Respawner = damageHandler.gameObject.AddComponent<vp_PlayerRespawner>();
		}
		else
		{
			vp_Respawner = damageHandler.gameObject.AddComponent<vp_Respawner>();
		}
		if (vp_Respawner == null)
		{
			return;
		}
		if (damageHandler.MinRespawnTime != -99999f)
		{
			vp_Respawner.MinRespawnTime = damageHandler.MinRespawnTime;
		}
		if (damageHandler.MaxRespawnTime != -99999f)
		{
			vp_Respawner.MaxRespawnTime = damageHandler.MaxRespawnTime;
		}
		if (damageHandler.RespawnCheckRadius != -99999f)
		{
			vp_Respawner.ObstructionRadius = damageHandler.RespawnCheckRadius;
		}
		if (damageHandler.RespawnSound != null)
		{
			vp_Respawner.SpawnSound = damageHandler.RespawnSound;
		}
	}

	public float MaxHealth = 1f;

	public GameObject[] DeathSpawnObjects;

	public float MinDeathDelay;

	public float MaxDeathDelay;

	public float CurrentHealth;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_InstaKill;

	public AudioClip DeathSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioSource m_Audio;

	public float ImpactDamageThreshold = 10f;

	public float ImpactDamageMultiplier;

	[HideInInspector]
	public bool Respawns;

	[HideInInspector]
	public float MinRespawnTime = -99999f;

	[HideInInspector]
	public float MaxRespawnTime = -99999f;

	[HideInInspector]
	public float RespawnCheckRadius = -99999f;

	[HideInInspector]
	public AudioClip RespawnSound;

	[HideInInspector]
	public GameObject DeathEffect;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static Dictionary<Collider, vp_DamageHandler> m_DamageHandlersByCollider;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static vp_DamageHandler m_GetDamageHandlerOfColliderResult;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_StartPosition;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Quaternion m_StartRotation;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Respawner m_Respawner;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Source;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_OriginalSource;
}
