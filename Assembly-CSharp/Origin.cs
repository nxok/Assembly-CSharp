using System;
using System.Collections.Generic;
using Audio;
using Unity.Collections;
using UnityEngine;

public class Origin : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		Origin.Instance = this;
		this.isAuto = true;
		Shader.SetGlobalVector("_OriginPos", Origin.position);
		this.particles = new NativeArray<ParticleSystem.Particle>(512, 4, 1);
		this.physicsCheckT = base.transform.GetChild(0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDestroy()
	{
		this.particles.Dispose();
	}

	public static void Cleanup()
	{
		Origin.position = Vector3.zero;
		Shader.SetGlobalVector("_OriginPos", Origin.position);
	}

	public static void Add(Transform _t, int _level)
	{
		Origin.RepositionObjects.Add(new Origin.TransformLevel(_t, _level));
	}

	public static void Remove(Transform _t)
	{
		for (int i = Origin.RepositionObjects.Count - 1; i >= 0; i--)
		{
			if (Origin.RepositionObjects[i].transform == _t)
			{
				Origin.RepositionObjects.RemoveAt(i);
			}
		}
	}

	public void Reposition(Vector3 _newOrigin)
	{
		this.DoReposition(_newOrigin);
		Physics.simulationMode = 2;
		Physics.Simulate(0.01f);
		Physics.simulationMode = 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoReposition(Vector3 _newOrigin)
	{
		_newOrigin.x = (float)((int)_newOrigin.x & -16);
		_newOrigin.y = (float)((int)_newOrigin.y & -16);
		_newOrigin.z = (float)((int)_newOrigin.z & -16);
		Log.Out("{0}+{1} Origin Reposition {2} to {3}", new object[]
		{
			GameManager.frameCount,
			GameManager.fixedUpdateCount,
			Origin.position.ToCultureInvariantString(),
			_newOrigin.ToCultureInvariantString()
		});
		Vector3 vector = Origin.position - _newOrigin;
		Origin.position = _newOrigin;
		this.OriginPos = _newOrigin;
		this.physicsCheckPos = -Origin.position;
		this.physicsCheckPos.y = this.physicsCheckPos.y - 256f;
		this.physicsCheckT.position = this.physicsCheckPos;
		this.checkRepositionDelay = 0;
		Shader.SetGlobalVector("_OriginPos", Origin.position);
		for (int i = 0; i < Origin.RepositionObjects.Count; i++)
		{
			Origin.RepositionTransform(vector, Origin.RepositionObjects[i].transform, Origin.RepositionObjects[i].level);
		}
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		EntityPlayerLocal primaryPlayer = world.GetPrimaryPlayer();
		if (primaryPlayer)
		{
			vp_FPController component = primaryPlayer.GetComponent<vp_FPController>();
			if (component)
			{
				component.Reposition(vector);
			}
			primaryPlayer.emodel.ClearClothMotion();
		}
		List<Entity> list = world.Entities.list;
		for (int j = list.Count - 1; j >= 0; j--)
		{
			list[j].OriginChanged(vector);
		}
		this.RepositionParticles(vector);
		if (AstarManager.Instance != null)
		{
			AstarManager.Instance.OriginChanged();
		}
		if (world.m_ChunkManager != null)
		{
			world.m_ChunkManager.OriginChanged(vector);
		}
		if (DecoManager.Instance != null)
		{
			DecoManager.Instance.OriginChanged(vector);
		}
		if (OcclusionManager.Instance)
		{
			OcclusionManager.Instance.OriginChanged(vector);
		}
		Manager.OriginChanged(vector);
		DynamicMeshManager.OriginUpdate();
		Action<Vector3> originChanged = Origin.OriginChanged;
		if (originChanged == null)
		{
			return;
		}
		originChanged(_newOrigin);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RepositionParticles(Vector3 _deltaV)
	{
		for (int i = Origin.particleSystemTs.Count - 1; i >= 0; i--)
		{
			Transform transform = Origin.particleSystemTs[i];
			if (!transform)
			{
				Origin.particleSystemTs.RemoveAt(i);
			}
			else
			{
				transform.GetComponentsInChildren<ParticleSystem>(this.particleSystems);
				for (int j = this.particleSystems.Count - 1; j >= 0; j--)
				{
					ParticleSystem particleSystem = this.particleSystems[j];
					if (particleSystem.isPlaying && particleSystem.main.simulationSpace == 1)
					{
						int num = particleSystem.GetParticles(this.particles);
						for (int k = 0; k < num; k++)
						{
							ParticleSystem.Particle particle = this.particles[k];
							particle.position += _deltaV;
							this.particles[k] = particle;
						}
						particleSystem.SetParticles(this.particles, num);
						particleSystem.Simulate(0f, false, false);
						particleSystem.Play(false);
					}
				}
				this.particleSystems.Clear();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void RepositionTransform(Vector3 _deltaV, Transform _t, int _level)
	{
		if (!_t)
		{
			return;
		}
		if (_level < 0)
		{
			_t.position += _deltaV;
			return;
		}
		int childCount = _t.childCount;
		if (_level == 0)
		{
			for (int i = 0; i < childCount; i++)
			{
				_t.GetChild(i).position += _deltaV;
			}
			return;
		}
		for (int j = 0; j < childCount; j++)
		{
			Transform child = _t.GetChild(j);
			Origin.RepositionTransform(_deltaV, child, _level - 1);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		if (GameManager.IsDedicatedServer || !GameManager.Instance.gameStateManager.IsGameStarted())
		{
			return;
		}
		World world = GameManager.Instance.World;
		if (world == null)
		{
			return;
		}
		if (this.isAuto)
		{
			List<EntityPlayerLocal> localPlayers = world.GetLocalPlayers();
			if (localPlayers.Count > 0)
			{
				EntityPlayerLocal entityPlayerLocal = localPlayers[0];
				this.UpdateLocalPlayer(entityPlayerLocal);
			}
		}
		else
		{
			if (this.timedMove > 0f)
			{
				this.timedMoveTime += Time.deltaTime;
				if (this.timedMoveTime >= this.timedMove)
				{
					this.timedMoveTime = 0f;
					this.timedMoveCount++;
					Vector3 vector;
					vector.x = (float)(this.timedMoveCount & 3) * this.timedMoveDistance.x;
					vector.y = (float)((this.timedMoveCount >> 2) & 1) * this.timedMoveDistance.y;
					vector.z = (float)((this.timedMoveCount >> 1) & 1) * this.timedMoveDistance.z;
					this.Reposition(vector);
				}
			}
			if (this.isMoveOriginNow)
			{
				this.Reposition(this.MoveOriginTo);
				this.MoveOriginTo = Vector3.zero;
				this.isMoveOriginNow = false;
			}
		}
		if (this.checkRepositionDelay >= 0)
		{
			int num = this.checkRepositionDelay - 1;
			this.checkRepositionDelay = num;
			if (num < 0)
			{
				for (int i = 0; i < 2; i++)
				{
					bool flag = true;
					Vector3 vector2 = this.physicsCheckPos;
					vector2.y += 10f;
					if (!Physics.Raycast(vector2, Vector3.down, 3.4028235E+38f, 65536))
					{
						flag = false;
						Log.Warning("{0}+{1} Origin ray fail {2}", new object[]
						{
							GameManager.frameCount,
							GameManager.fixedUpdateCount,
							vector2.ToCultureInvariantString()
						});
					}
					if (world != null)
					{
						List<EntityPlayerLocal> localPlayers2 = world.GetLocalPlayers();
						if (localPlayers2.Count > 0)
						{
							EntityPlayerLocal entityPlayerLocal2 = localPlayers2[0];
							if (entityPlayerLocal2.IsSpawned() && !entityPlayerLocal2.IsFlyMode.Value)
							{
								Vector3 vector3 = entityPlayerLocal2.transform.position;
								vector3.y += 1.5f;
								if (!Physics.Raycast(vector3, Vector3.down, 3.4028235E+38f, 65536))
								{
									flag = false;
									Log.Warning("{0}+{1} Origin player ray fail {2}", new object[]
									{
										GameManager.frameCount,
										GameManager.fixedUpdateCount,
										vector3.ToCultureInvariantString()
									});
								}
							}
						}
					}
					if (flag)
					{
						this.checkRepositionDelay = -1;
						break;
					}
					Vector3 vector4 = Origin.position;
					vector4.x += 16f;
					this.Reposition(vector4);
				}
				if (this.checkRepositionDelay >= 0)
				{
					this.checkRepositionDelay = 3;
				}
			}
		}
	}

	public void UpdateLocalPlayer(EntityPlayerLocal player)
	{
		if (!this.isAuto)
		{
			return;
		}
		if (player.IsSpawned() && (player.position - Origin.position).sqrMagnitude > 67600f)
		{
			this.Reposition(player.position);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cAutoRepositionDistanceSq = 67600f;

	public static Origin Instance;

	public static Action<Vector3> OriginChanged;

	public static Vector3 position;

	public bool isAuto;

	public Vector3 OriginPos;

	[Tooltip("Force a move every x seconds")]
	public float timedMove;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timedMoveTime;

	public Vector3 timedMoveDistance = new Vector3(16f, 0f, 0f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int timedMoveCount;

	public Vector3 MoveOriginTo;

	public bool isMoveOriginNow;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static readonly List<Origin.TransformLevel> RepositionObjects = new List<Origin.TransformLevel>();

	public static List<Transform> particleSystemTs = new List<Transform>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public NativeArray<ParticleSystem.Particle> particles;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform physicsCheckT;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 physicsCheckPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int checkRepositionDelay = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public struct TransformLevel
	{
		public TransformLevel(Transform _transform, int _level)
		{
			this.transform = _transform;
			this.level = _level;
			this.name = this.transform.name;
		}

		public readonly Transform transform;

		public readonly int level;

		public readonly string name;
	}
}
