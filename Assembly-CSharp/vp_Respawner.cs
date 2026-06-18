using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[Serializable]
public class vp_Respawner : MonoBehaviour
{
	public static Dictionary<Collider, vp_Respawner> RespawnersByCollider
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (vp_Respawner.m_RespawnersByCollider == null)
			{
				vp_Respawner.m_RespawnersByCollider = new Dictionary<Collider, vp_Respawner>(100);
			}
			return vp_Respawner.m_RespawnersByCollider;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.m_Transform = base.transform;
		this.m_Audio = base.GetComponent<AudioSource>();
		this.Placement.Position = (this.m_InitialPosition = this.m_Transform.position);
		this.Placement.Rotation = (this.m_InitialRotation = this.m_Transform.rotation);
		if (this.m_SpawnMode == vp_Respawner.SpawnMode.SamePosition)
		{
			this.SpawnPointTag = "";
		}
		if (this.SpawnOnAwake)
		{
			this.m_IsInitialSpawnOnAwake = true;
			vp_Utility.Activate(base.gameObject, false);
			this.PickSpawnPoint();
		}
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

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void SpawnFX()
	{
		if (!this.m_IsInitialSpawnOnAwake)
		{
			if (this.m_Audio != null)
			{
				this.m_Audio.pitch = Time.timeScale;
				this.m_Audio.PlayOneShot(this.SpawnSound);
			}
			if (this.SpawnFXPrefabs != null && this.SpawnFXPrefabs.Length != 0)
			{
				foreach (GameObject gameObject in this.SpawnFXPrefabs)
				{
					if (gameObject != null)
					{
						vp_Utility.Instantiate(gameObject, this.m_Transform.position, this.m_Transform.rotation);
					}
				}
			}
		}
		this.m_IsInitialSpawnOnAwake = false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Die()
	{
		vp_Timer.In(Random.Range(this.MinRespawnTime, this.MaxRespawnTime), new vp_Timer.Callback(this.PickSpawnPoint), this.m_RespawnTimer);
	}

	public virtual void PickSpawnPoint()
	{
		if (this == null)
		{
			return;
		}
		if (this.m_SpawnMode == vp_Respawner.SpawnMode.SamePosition || vp_SpawnPoint.SpawnPoints.Count < 1)
		{
			this.Placement.Position = this.m_InitialPosition;
			this.Placement.Rotation = this.m_InitialRotation;
			if (this.Placement.IsObstructed(this.ObstructionRadius))
			{
				vp_Respawner.ObstructionSolver obstructionSolver = this.m_ObstructionSolver;
				if (obstructionSolver == vp_Respawner.ObstructionSolver.Wait)
				{
					vp_Timer.In(Random.Range(this.MinRespawnTime, this.MaxRespawnTime), new vp_Timer.Callback(this.PickSpawnPoint), this.m_RespawnTimer);
					return;
				}
				if (obstructionSolver == vp_Respawner.ObstructionSolver.AdjustPlacement)
				{
					if (!vp_Placement.AdjustPosition(this.Placement, this.ObstructionRadius, 1000))
					{
						vp_Timer.In(Random.Range(this.MinRespawnTime, this.MaxRespawnTime), new vp_Timer.Callback(this.PickSpawnPoint), this.m_RespawnTimer);
						return;
					}
				}
			}
		}
		else
		{
			vp_Respawner.ObstructionSolver obstructionSolver = this.m_ObstructionSolver;
			if (obstructionSolver != vp_Respawner.ObstructionSolver.Wait)
			{
				if (obstructionSolver == vp_Respawner.ObstructionSolver.AdjustPlacement)
				{
					this.Placement = vp_SpawnPoint.GetRandomPlacement(this.ObstructionRadius, this.SpawnPointTag);
					if (this.Placement == null)
					{
						vp_Timer.In(Random.Range(this.MinRespawnTime, this.MaxRespawnTime), new vp_Timer.Callback(this.PickSpawnPoint), this.m_RespawnTimer);
						return;
					}
				}
			}
			else
			{
				this.Placement = vp_SpawnPoint.GetRandomPlacement(0f, this.SpawnPointTag);
				if (this.Placement == null)
				{
					this.Placement = new vp_Placement();
					this.m_SpawnMode = vp_Respawner.SpawnMode.SamePosition;
					this.PickSpawnPoint();
				}
				if (this.Placement.IsObstructed(this.ObstructionRadius))
				{
					vp_Timer.In(Random.Range(this.MinRespawnTime, this.MaxRespawnTime), new vp_Timer.Callback(this.PickSpawnPoint), this.m_RespawnTimer);
					return;
				}
			}
		}
		this.Respawn();
	}

	public virtual void PickSpawnPoint(Vector3 position, Quaternion rotation)
	{
		this.Placement.Position = position;
		this.Placement.Rotation = rotation;
		this.Respawn();
	}

	public virtual void Respawn()
	{
		this.LastRespawnTime = Time.time;
		vp_Utility.Activate(base.gameObject, true);
		this.SpawnFX();
		if (vp_Gameplay.isMaster)
		{
			vp_GlobalEvent<Transform, vp_Placement>.Send("Respawn", base.transform.root, this.Placement);
		}
		base.SendMessage("Reset");
		this.Placement.Position = this.m_InitialPosition;
		this.Placement.Rotation = this.m_InitialRotation;
	}

	public virtual void Reset()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		this.m_Transform.position = this.Placement.Position;
		if (base.GetComponent<Rigidbody>() != null && !base.GetComponent<Rigidbody>().isKinematic)
		{
			base.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
			base.GetComponent<Rigidbody>().velocity = Vector3.zero;
		}
	}

	public static vp_Respawner GetRespawnerOfCollider(Collider col)
	{
		if (!vp_Respawner.RespawnersByCollider.TryGetValue(col, out vp_Respawner.m_GetRespawnerOfColliderResult))
		{
			vp_Respawner.m_GetRespawnerOfColliderResult = col.transform.root.GetComponentInChildren<vp_Respawner>();
			vp_Respawner.RespawnersByCollider.Add(col, vp_Respawner.m_GetRespawnerOfColliderResult);
		}
		return vp_Respawner.m_GetRespawnerOfColliderResult;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void NotifyLevelWasLoaded(Scene scene, LoadSceneMode mode)
	{
		vp_Respawner.RespawnersByCollider.Clear();
	}

	public vp_Respawner.SpawnMode m_SpawnMode;

	public string SpawnPointTag = "";

	public vp_Respawner.ObstructionSolver m_ObstructionSolver;

	public float ObstructionRadius = 1f;

	public float MinRespawnTime = 3f;

	public float MaxRespawnTime = 3f;

	public float LastRespawnTime;

	public bool SpawnOnAwake;

	public AudioClip SpawnSound;

	public GameObject[] SpawnFXPrefabs;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 m_InitialPosition = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Quaternion m_InitialRotation;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Placement Placement = new vp_Placement();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public AudioSource m_Audio;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_IsInitialSpawnOnAwake;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_Timer.Handle m_RespawnTimer = new vp_Timer.Handle();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static Dictionary<Collider, vp_Respawner> m_RespawnersByCollider;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public static vp_Respawner m_GetRespawnerOfColliderResult;

	public enum SpawnMode
	{
		SamePosition,
		SpawnPoint
	}

	public enum ObstructionSolver
	{
		Wait,
		AdjustPlacement
	}
}
