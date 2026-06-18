using System;
using System.Collections.Generic;
using System.Globalization;
using Audio;
using UnityEngine;

public class MiniTurretFireController : MonoBehaviour
{
	public bool IsOn
	{
		get
		{
			return this.entityTurret != null && this.entityTurret.IsOn;
		}
	}

	public Vector3 TurretPosition
	{
		get
		{
			return this.entityTurret.transform.position + Origin.position;
		}
	}

	public float CenteredYaw
	{
		get
		{
			return this.entityTurret.CenteredYaw;
		}
		set
		{
			this.entityTurret.CenteredYaw = value;
		}
	}

	public float CenteredPitch
	{
		get
		{
			return this.entityTurret.CenteredPitch;
		}
		set
		{
			this.entityTurret.CenteredPitch = value;
		}
	}

	public float MaxDistance
	{
		get
		{
			return this.maxDistance;
		}
	}

	public void Init(DynamicProperties _properties, EntityTurret _entity)
	{
		this.entityTurret = _entity;
		this.Cone = this.entityTurret.Cone;
		this.Laser = this.entityTurret.Laser;
		this.Muzzle = this.entityTurret.transform.FindInChilds("Muzzle", false);
		GameManager.Instance.World.GetEntity(this.entityTurret.belongsPlayerId);
		if (_properties.Values.ContainsKey("FireSound"))
		{
			this.fireSound = _properties.Values["FireSound"];
		}
		else
		{
			this.fireSound = "Electricity/Turret/turret_fire";
		}
		if (_properties.Values.ContainsKey("WakeUpSound"))
		{
			this.wakeUpSound = _properties.Values["WakeUpSound"];
		}
		else
		{
			this.wakeUpSound = "Electricity/Turret/turret_windup";
		}
		if (_properties.Values.ContainsKey("OverheatSound"))
		{
			this.overheatSound = _properties.Values["OverheatSound"];
		}
		else
		{
			this.overheatSound = "Electricity/Turret/turret_overheat_lp";
		}
		if (_properties.Values.ContainsKey("TargetingSound"))
		{
			this.targetingSound = _properties.Values["TargetingSound"];
		}
		else
		{
			this.targetingSound = "Electricity/Turret/turret_retarget_lp";
		}
		if (_properties.Values.ContainsKey("MaxDistance"))
		{
			this.maxDistance = StringParsers.ParseFloat(_properties.Values["MaxDistance"], 0, -1, NumberStyles.Any);
		}
		else
		{
			this.maxDistance = 16f;
		}
		if (_properties.Values.ContainsKey("YawRange"))
		{
			float num = StringParsers.ParseFloat(_properties.Values["YawRange"], 0, -1, NumberStyles.Any);
			num *= 0.5f;
			this.yawRange = new Vector2(-num, num);
		}
		else
		{
			this.yawRange = new Vector2(-22.5f, 22.5f);
		}
		if (_properties.Values.ContainsKey("PitchRange"))
		{
			float num2 = StringParsers.ParseFloat(_properties.Values["PitchRange"], 0, -1, NumberStyles.Any);
			num2 *= 0.5f;
			this.pitchRange = new Vector2(-num2, num2);
		}
		else
		{
			this.pitchRange = new Vector2(-22.5f, 22.5f);
		}
		if (_properties.Values.ContainsKey("RaySpread"))
		{
			float num3 = StringParsers.ParseFloat(_properties.Values["RaySpread"], 0, -1, NumberStyles.Any);
			num3 *= 0.5f;
			this.spreadHorizontal = new Vector2(-num3, num3);
		}
		else
		{
			this.spreadHorizontal = new Vector2(-1f, 1f);
		}
		if (_properties.Values.ContainsKey("RayCount"))
		{
			this.rayCount = int.Parse(_properties.Values["RayCount"]);
		}
		else
		{
			this.rayCount = 1;
		}
		if (_properties.Values.ContainsKey("WakeUpTime"))
		{
			this.wakeUpTimeMax = StringParsers.ParseFloat(_properties.Values["WakeUpTime"], 0, -1, NumberStyles.Any);
		}
		if (_properties.Values.ContainsKey("FallAsleepTime"))
		{
			this.fallAsleepTimeMax = StringParsers.ParseFloat(_properties.Values["FallAsleepTime"], 0, -1, NumberStyles.Any);
		}
		if (_properties.Values.ContainsKey("BurstRoundCount"))
		{
			this.burstRoundCountMax = int.Parse(_properties.Values["BurstRoundCount"]);
		}
		if (_properties.Values.ContainsKey("BurstFireRate"))
		{
			this.fireRateMax = StringParsers.ParseFloat(_properties.Values["BurstFireRate"], 0, -1, NumberStyles.Any);
		}
		if (_properties.Values.ContainsKey("CooldownTime"))
		{
			this.coolOffTimeMax = StringParsers.ParseFloat(_properties.Values["CooldownTime"], 0, -1, NumberStyles.Any);
		}
		if (_properties.Values.ContainsKey("OvershootTime"))
		{
			this.overshootTimeMax = StringParsers.ParseFloat(_properties.Values["OvershootTime"], 0, -1, NumberStyles.Any);
		}
		_properties.ParseString("ParticlesMuzzleFire", ref this.muzzleFireParticle);
		_properties.ParseString("ParticlesMuzzleSmoke", ref this.muzzleSmokeParticle);
		if (_properties.Values.ContainsKey("AmmoItem"))
		{
			this.ammoItemName = _properties.Values["AmmoItem"];
		}
		else
		{
			this.ammoItemName = "9mmBullet";
		}
		if (this.entityTurret.YawController != null)
		{
			this.entityTurret.YawController.Init(_properties);
		}
		if (this.entityTurret.PitchController != null)
		{
			this.entityTurret.PitchController.Init(_properties);
		}
		this.buffActions = new List<string>();
		if (_properties.Values.ContainsKey("Buff"))
		{
			string[] array = _properties.Values["Buff"].Replace(" ", "").Split(',', StringSplitOptions.None);
			this.buffActions.AddRange(array);
		}
		this.damageMultiplier = new DamageMultiplier(_properties);
		this.sorter = new MiniTurretFireController.TurretEntitySorter(this.TurretPosition);
		this.state = MiniTurretFireController.TurretState.Asleep;
		if (this.Cone != null)
		{
			this.Cone.localScale = new Vector3(this.Cone.localScale.x * (this.yawRange.y / 22.5f) * (this.maxDistance / 5.25f), this.Cone.localScale.y * (this.pitchRange.y / 22.5f) * (this.maxDistance / 5.25f), this.Cone.localScale.z * (this.maxDistance / 5.25f));
			this.Cone.gameObject.SetActive(false);
		}
		if (this.Laser != null)
		{
			this.Laser.localScale = new Vector3(this.Laser.localScale.x, this.Laser.localScale.y, this.Laser.localScale.z * this.maxDistance);
			this.Laser.gameObject.SetActive(false);
		}
		this.entityTurret.transform.GetComponent<SphereCollider>().enabled = true;
		this.burstFireStarted = false;
		this.burstFireFinished = false;
		this.burstRoundCount = 0;
		this.coolOffTime = 0f;
		this.waterCollisionParticles.Init(this.entityTurret.belongsPlayerId, "bullet", "water", 16);
	}

	public virtual float GetRange(EntityAlive owner)
	{
		return EffectManager.GetValue(PassiveEffects.MaxRange, this.entityTurret.OriginalItemValue, this.maxDistance, owner, null, default(FastTags<TagGroup.Global>), true, false, true, true, true, 1, true, false);
	}

	public virtual int GetBurstRoundCount(EntityAlive owner)
	{
		return (int)EffectManager.GetValue(PassiveEffects.BurstRoundCount, this.entityTurret.OriginalItemValue, 0f, owner, null, default(FastTags<TagGroup.Global>), true, false, true, true, true, 1, true, false);
	}

	public bool hasTarget
	{
		get
		{
			return this.currentEntityTarget != null;
		}
	}

	public void Update()
	{
		if (this.entityTurret == null)
		{
			return;
		}
		if (!this.entityTurret.IsOn)
		{
			if (this.entityTurret.YawController.Yaw != this.CenteredYaw)
			{
				this.entityTurret.YawController.Yaw = this.CenteredYaw;
			}
			if (this.entityTurret.PitchController.Pitch != this.CenteredPitch + 35f)
			{
				this.entityTurret.PitchController.Pitch = this.CenteredPitch + 35f;
			}
			if (this.turretSpinAudioHandle != null)
			{
				this.turretSpinAudioHandle.Stop(this.entityTurret.entityId);
				this.turretSpinAudioHandle = null;
			}
			this.entityTurret.YawController.UpdateYaw();
			this.entityTurret.PitchController.UpdatePitch();
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			if (!this.hasTarget)
			{
				this.findTarget();
			}
			else if (this.shouldIgnoreTarget(this.currentEntityTarget))
			{
				this.currentEntityTarget = null;
				this.entityTurret.TargetEntityId = -1;
			}
		}
		else if (this.entityTurret.TargetEntityId != -1)
		{
			this.currentEntityTarget = GameManager.Instance.World.GetEntity(this.entityTurret.TargetEntityId) as EntityAlive;
			if (this.currentEntityTarget == null || this.currentEntityTarget.IsDead())
			{
				this.currentEntityTarget = null;
				this.entityTurret.TargetEntityId = -1;
			}
		}
		else
		{
			this.currentEntityTarget = null;
			this.entityTurret.TargetEntityId = -1;
		}
		if (this.entityTurret.IsTurning)
		{
			if (this.turretSpinAudioHandle == null)
			{
				this.turretSpinAudioHandle = Manager.Play(this.entityTurret, this.targetingSound, 1f, true);
			}
		}
		else if (this.turretSpinAudioHandle != null)
		{
			this.turretSpinAudioHandle.Stop(this.entityTurret.entityId);
			this.turretSpinAudioHandle = null;
		}
		this.targetChestHeadDelay -= Time.deltaTime;
		if (this.targetChestHeadDelay <= 0f)
		{
			this.targetChestHeadDelay = 1f;
			this.targetChestHeadPercent = this.entityTurret.rand.RandomFloat;
		}
		this.fireRateTimer += Time.deltaTime;
		if (this.hasTarget)
		{
			this.entityTurret.YawController.IdleScan = false;
			float yaw = this.entityTurret.YawController.Yaw;
			float pitch = this.entityTurret.PitchController.Pitch;
			Vector3 vector;
			if (this.trackTarget(this.currentEntityTarget, ref yaw, ref pitch, out vector))
			{
				this.entityTurret.YawController.Yaw = yaw;
				this.entityTurret.PitchController.Pitch = pitch;
				EntityAlive entityAlive = GameManager.Instance.World.GetEntity(this.entityTurret.belongsPlayerId) as EntityAlive;
				this.burstRoundCountMax = this.GetBurstRoundCount(entityAlive);
				if (this.burstRoundCountMax == 0 || !this.burstFireStarted || (this.burstFireStarted && !this.burstFireFinished))
				{
					FastTags<TagGroup.Global> fastTags = this.entityTurret.OriginalItemValue.ItemClass.ItemTags | this.entityTurret.EntityClass.Tags;
					this.fireRateMax = 60f / (EffectManager.GetValue(PassiveEffects.RoundsPerMinute, this.entityTurret.OriginalItemValue, this.fireRateMax, entityAlive, null, fastTags, true, false, true, true, true, 1, true, false) + 1E-05f);
					if (this.fireRateTimer >= this.fireRateMax)
					{
						this.Fire();
						this.fireRateTimer = 0f;
						if (this.burstRoundCountMax > 0)
						{
							this.burstFireStarted = true;
							this.burstRoundCount++;
							if (this.burstRoundCount >= this.burstRoundCountMax)
							{
								this.burstFireFinished = true;
							}
						}
					}
				}
				else
				{
					this.coolOffTime += Time.deltaTime;
					if (this.coolOffTime >= this.coolOffTimeMax)
					{
						this.coolOffTime = 0f;
						this.burstRoundCount = 0;
						this.burstFireStarted = false;
						this.burstFireFinished = false;
					}
				}
			}
		}
		else
		{
			if (!this.entityTurret.YawController.IdleScan || (this.entityTurret.YawController.Yaw != this.yawRange.y && this.entityTurret.YawController.Yaw != this.yawRange.x))
			{
				this.entityTurret.YawController.IdleScan = true;
				this.entityTurret.YawController.Yaw = this.yawRange.y;
			}
			float num = ((this.yawRange.y > 0f) ? this.yawRange.y : (360f + this.yawRange.y));
			float num2 = ((this.yawRange.x > 0f) ? this.yawRange.x : (360f + this.yawRange.x));
			if (Mathf.Abs(this.entityTurret.YawController.CurrentYaw - num) < 1f || Mathf.Abs(this.entityTurret.YawController.CurrentYaw - this.yawRange.y) < 1f)
			{
				this.entityTurret.YawController.Yaw = this.yawRange.x;
			}
			else if (Mathf.Abs(this.entityTurret.YawController.CurrentYaw - num2) < 1f || Mathf.Abs(this.entityTurret.YawController.CurrentYaw - this.yawRange.x) < 1f)
			{
				this.entityTurret.YawController.Yaw = this.yawRange.y;
			}
			this.entityTurret.PitchController.Pitch = this.CenteredPitch;
		}
		this.entityTurret.YawController.UpdateYaw();
		this.entityTurret.PitchController.UpdatePitch();
		if (this.Laser != null)
		{
			this.updateLaser();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void findTarget()
	{
		Vector3 vector = base.transform.position;
		if (this.Cone != null)
		{
			vector = this.Cone.transform.position;
		}
		this.currentEntityTarget = null;
		this.entityTurret.TargetEntityId = -1;
		if (GameManager.Instance == null || GameManager.Instance.World == null)
		{
			return;
		}
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), new Bounds(this.TurretPosition, Vector3.one * (this.maxDistance * 2f + 1f)), new List<Entity>());
		entitiesInBounds.Sort(this.sorter);
		if (entitiesInBounds.Count > 0)
		{
			for (int i = 0; i < entitiesInBounds.Count; i++)
			{
				Entity entity = entitiesInBounds[i];
				if (!this.shouldIgnoreTarget(entity))
				{
					Vector3 zero = Vector3.zero;
					float centeredYaw = this.CenteredYaw;
					float centeredPitch = this.CenteredPitch;
					if (this.trackTarget(entity, ref centeredYaw, ref centeredPitch, out zero))
					{
						Vector3 normalized = (zero - vector).normalized;
						Ray ray;
						ray..ctor(vector + Origin.position - normalized * 0.05f, normalized);
						if (Voxel.Raycast(GameManager.Instance.World, ray, this.maxDistance + 0.05f, -538750989, 8, 0.05f) && Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
						{
							Transform hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
							if (!(hitRootTransform == null))
							{
								Entity component = hitRootTransform.GetComponent<Entity>();
								if (component != null)
								{
									if (component == entity)
									{
										this.currentEntityTarget = component as EntityAlive;
										this.entityTurret.TargetEntityId = this.currentEntityTarget.entityId;
										this.entityTurret.YawController.Yaw = centeredYaw;
										this.entityTurret.PitchController.Pitch = centeredPitch;
										return;
									}
									this.currentEntityTarget = null;
									this.entityTurret.TargetEntityId = -1;
								}
							}
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateLaser()
	{
		float num = this.maxDistance;
		Ray ray;
		ray..ctor(this.Laser.transform.position + Origin.position, -this.Laser.transform.forward);
		if (Voxel.Raycast(GameManager.Instance.World, ray, num, 1082195968, 128, 0.25f))
		{
			num = Vector3.Distance(Voxel.voxelRayHitInfo.hit.pos - Origin.position, ray.origin - Origin.position);
		}
		this.Laser.transform.localScale = new Vector3(1f, 1f, num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldIgnoreTarget(Entity _target)
	{
		if (_target == null)
		{
			return true;
		}
		Vector3 vector = base.transform.forward;
		if (this.Cone != null)
		{
			vector = this.Cone.transform.forward;
		}
		if (Vector3.Dot(_target.position - this.entityTurret.position, vector) > 0f)
		{
			return true;
		}
		if (!_target.IsAlive())
		{
			return true;
		}
		if (_target.entityId == this.entityTurret.entityId)
		{
			return true;
		}
		if (_target is EntityVehicle)
		{
			Entity attachedMainEntity = (_target as EntityVehicle).AttachedMainEntity;
			if (attachedMainEntity == null)
			{
				return true;
			}
			_target = attachedMainEntity;
		}
		if (_target is EntityPlayer)
		{
			bool flag = false;
			bool flag2 = false;
			EnumPlayerKillingMode @int = (EnumPlayerKillingMode)GamePrefs.GetInt(EnumGamePrefs.PlayerKillingMode);
			PersistentPlayerList persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
			if (this.entityTurret.belongsPlayerId == _target.entityId)
			{
				flag = true;
			}
			if (!flag && persistentPlayerList.EntityToPlayerMap.ContainsKey(this.entityTurret.belongsPlayerId))
			{
				PersistentPlayerData persistentPlayerData = persistentPlayerList.EntityToPlayerMap[this.entityTurret.belongsPlayerId];
				if (persistentPlayerData != null && persistentPlayerList.EntityToPlayerMap.ContainsKey(_target.entityId))
				{
					PersistentPlayerData persistentPlayerData2 = persistentPlayerList.EntityToPlayerMap[_target.entityId];
					if (persistentPlayerData.IsAlly(persistentPlayerData2))
					{
						flag2 = true;
					}
				}
				EntityPlayer entityPlayer = GameManager.Instance.World.GetEntity(this.entityTurret.belongsPlayerId) as EntityPlayer;
				if (!flag2 && entityPlayer != null && entityPlayer.Party != null && entityPlayer.Party.ContainsMember(_target as EntityPlayer))
				{
					flag2 = true;
				}
			}
			if (@int == EnumPlayerKillingMode.NoKilling)
			{
				return true;
			}
			if (flag && (!this.entityTurret.TargetOwner || @int != EnumPlayerKillingMode.KillEveryone))
			{
				return true;
			}
			if (flag2 && (!this.entityTurret.TargetAllies || (@int != EnumPlayerKillingMode.KillEveryone && @int != EnumPlayerKillingMode.KillAlliesOnly)))
			{
				return true;
			}
			if (!flag && !flag2 && (!this.entityTurret.TargetStrangers || (@int != EnumPlayerKillingMode.KillStrangersOnly && @int != EnumPlayerKillingMode.KillEveryone)))
			{
				return true;
			}
		}
		if (_target is EntityNPC)
		{
			if (_target is EntityTrader)
			{
				return true;
			}
			if (!this.entityTurret.TargetStrangers)
			{
				return true;
			}
		}
		if (_target is EntityEnemy && !this.entityTurret.TargetEnemies)
		{
			return true;
		}
		if (_target is EntityTurret)
		{
			return true;
		}
		if (_target is EntityDrone)
		{
			return true;
		}
		if (_target is EntitySupplyCrate)
		{
			return true;
		}
		float num = 0f;
		float num2 = 0f;
		Vector3 vector2;
		return _target as EntityAlive != null && !this.canHitEntity(_target, ref num, ref num2, out vector2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canHitEntity(Entity _targetEntity, ref float _yaw, ref float _pitch, out Vector3 targetPos)
	{
		Vector3 vector = base.transform.position;
		if (this.Cone != null)
		{
			vector = this.Cone.transform.position;
		}
		if (!this.trackTarget(_targetEntity, ref _yaw, ref _pitch, out targetPos))
		{
			return false;
		}
		Ray ray;
		ray..ctor(vector + Origin.position, (targetPos - vector).normalized);
		if (Voxel.Raycast(GameManager.Instance.World, ray, this.maxDistance, -538750989, 8, 0f) && Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
		{
			Transform hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
			if (hitRootTransform == null)
			{
				return false;
			}
			Entity component = hitRootTransform.GetComponent<Entity>();
			if (component != null && component.IsAlive() && _targetEntity == component)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool trackTarget(Entity _target, ref float _yaw, ref float _pitch, out Vector3 _targetPos)
	{
		_targetPos = Vector3.Lerp(_target.getChestPosition(), _target.getHeadPosition(), this.targetChestHeadPercent) - Origin.position;
		Vector3 vector = base.transform.position;
		if (this.Laser != null)
		{
			vector = this.Laser.transform.position;
		}
		Vector3 eulerAngles = Quaternion.LookRotation((_targetPos - vector).normalized).eulerAngles;
		float num = Mathf.DeltaAngle(this.entityTurret.transform.rotation.eulerAngles.y, eulerAngles.y);
		float num2 = eulerAngles.x;
		if (num2 > 180f)
		{
			num2 -= 360f;
		}
		float num3 = this.CenteredYaw % 360f;
		float num4 = this.CenteredPitch % 360f;
		if (num3 > 180f)
		{
			num3 -= 360f;
		}
		if (num4 > 180f)
		{
			num4 -= 360f;
		}
		if (num < num3 + this.yawRange.x || num > num3 + this.yawRange.y || num2 < num4 + this.pitchRange.x || num2 > num4 + this.pitchRange.y)
		{
			return false;
		}
		_yaw = num;
		_pitch = num2;
		return true;
	}

	public virtual void Fire()
	{
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || this.entityTurret == null || !this.entityTurret.IsOn)
		{
			return;
		}
		Vector3 position = this.Laser.transform.position;
		EntityAlive entityAlive = GameManager.Instance.World.GetEntity(this.entityTurret.belongsPlayerId) as EntityAlive;
		GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
		FastTags<TagGroup.Global> itemTags = this.entityTurret.OriginalItemValue.ItemClass.ItemTags;
		int num = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, this.entityTurret.OriginalItemValue, (float)this.rayCount, entityAlive, null, this.entityTurret.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false);
		this.maxDistance = EffectManager.GetValue(PassiveEffects.MaxRange, this.entityTurret.OriginalItemValue, this.MaxDistance, entityAlive, null, this.entityTurret.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false);
		for (int i = 0; i < num; i++)
		{
			Vector3 vector = this.Muzzle.transform.forward;
			this.spreadHorizontal.x = -(EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, this.entityTurret.OriginalItemValue, 22f, entityAlive, null, this.entityTurret.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false) * 0.5f);
			this.spreadHorizontal.y = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, this.entityTurret.OriginalItemValue, 22f, entityAlive, null, this.entityTurret.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false) * 0.5f;
			this.spreadVertical.x = -(EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, this.entityTurret.OriginalItemValue, 22f, entityAlive, null, this.entityTurret.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false) * 0.5f);
			this.spreadVertical.y = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, this.entityTurret.OriginalItemValue, 22f, entityAlive, null, this.entityTurret.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false) * 0.5f;
			vector = Quaternion.Euler(gameRandom.RandomRange(this.spreadHorizontal.x, this.spreadHorizontal.y), gameRandom.RandomRange(this.spreadVertical.x, this.spreadVertical.y), 0f) * vector;
			Ray ray;
			ray..ctor(position + Origin.position, vector);
			this.waterCollisionParticles.Reset();
			this.waterCollisionParticles.CheckCollision(ray.origin, ray.direction, this.maxDistance, -1);
			int num2 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, this.entityTurret.OriginalItemValue, 0f, entityAlive, null, this.entityTurret.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false));
			num2++;
			int num3 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.BlockPenetrationFactor, this.entityTurret.OriginalItemValue, 1f, entityAlive, null, this.entityTurret.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false));
			EntityAlive entityAlive2 = null;
			for (int j = 0; j < num2; j++)
			{
				if (Voxel.Raycast(GameManager.Instance.World, ray, this.maxDistance, -538750989, 8, 0f))
				{
					WorldRayHitInfo worldRayHitInfo = Voxel.voxelRayHitInfo.Clone();
					if (worldRayHitInfo.tag.StartsWith("E_"))
					{
						string text;
						EntityAlive entityAlive3 = ItemActionAttack.FindHitEntityNoTagCheck(worldRayHitInfo, out text) as EntityAlive;
						if (entityAlive2 == entityAlive3)
						{
							ray.origin = worldRayHitInfo.hit.pos + ray.direction * 0.1f;
							j--;
							goto IL_04E6;
						}
						entityAlive2 = entityAlive3;
					}
					else
					{
						j += Mathf.FloorToInt((float)ItemActionAttack.GetBlockHit(GameManager.Instance.World, worldRayHitInfo).Block.MaxDamage / (float)num3);
					}
					ItemActionAttack.Hit(worldRayHitInfo, this.entityTurret.belongsPlayerId, EnumDamageTypes.Piercing, this.GetDamageBlock(this.entityTurret.OriginalItemValue, BlockValue.Air, GameManager.Instance.World.GetEntity(this.entityTurret.belongsPlayerId) as EntityAlive, 1), this.GetDamageEntity(this.entityTurret.OriginalItemValue, GameManager.Instance.World.GetEntity(this.entityTurret.belongsPlayerId) as EntityAlive, 1), 1f, this.entityTurret.OriginalItemValue.PercentUsesLeft, 0f, 0f, "bullet", this.damageMultiplier, this.buffActions, new ItemActionAttack.AttackHitInfo(), 1, 0, 0f, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, this.entityTurret.entityId, this.entityTurret.OriginalItemValue, false, false, false, null);
				}
				IL_04E6:;
			}
		}
		if (!string.IsNullOrEmpty(this.muzzleFireParticle))
		{
			FireControllerUtils.SpawnParticleEffect(new ParticleEffect(this.muzzleFireParticle, this.Muzzle.position + Origin.position, this.Muzzle.rotation, 1f, Color.white, this.fireSound, null, 1f, ""), -1);
		}
		if (!string.IsNullOrEmpty(this.muzzleSmokeParticle))
		{
			float num4 = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(this.TurretPosition)) / 2f;
			FireControllerUtils.SpawnParticleEffect(new ParticleEffect(this.muzzleSmokeParticle, this.Muzzle.position + Origin.position, this.Muzzle.rotation, num4, new Color(1f, 1f, 1f, 0.3f), null, null, 1f, ""), -1);
		}
		if ((int)EffectManager.GetValue(PassiveEffects.MagazineSize, this.entityTurret.OriginalItemValue, 0f, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) > 0)
		{
			EntityTurret entityTurret = this.entityTurret;
			int ammoCount = entityTurret.AmmoCount;
			entityTurret.AmmoCount = ammoCount - 1;
		}
		this.entityTurret.OriginalItemValue.UseTimes += EffectManager.GetValue(PassiveEffects.DegradationPerUse, this.entityTurret.OriginalItemValue, 1f, entityAlive, null, this.entityTurret.OriginalItemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false) * ItemAction.ItemDegradationModifier;
	}

	public float GetDamageEntity(ItemValue _itemValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, (float)this.entityDamage, _holdingEntity, null, _itemValue.ItemClass.ItemTags, true, false, true, true, true, 1, true, false);
	}

	public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue, EntityAlive _holdingEntity = null, int actionIndex = 0)
	{
		this.tmpTag = _itemValue.ItemClass.ItemTags;
		this.tmpTag |= _blockValue.Block.Tags;
		float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, (float)this.blockDamage, _holdingEntity, null, this.tmpTag, true, false, true, true, true, 1, true, false);
		return Utils.FastMin((float)_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
	}

	public Transform Cone;

	public Transform Laser;

	public Transform Muzzle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float baseConeYaw = 22.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float baseConePitch = 22.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float baseConeDistance = 5.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float maxDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int entityDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int blockDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int rayCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float wakeUpTimeMax = 0.6522f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fallAsleepTimeMax = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool burstFireStarted;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool burstFireFinished;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int burstRoundCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int burstRoundCountMax = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fireRateTimer;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float fireRateMax = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float coolOffTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float coolOffTimeMax = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float overshootTimeMax = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MiniTurretFireController.TurretState state;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public string fireSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string wakeUpSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string overheatSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string targetingSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string muzzleFireParticle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string muzzleSmokeParticle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string ammoItemName;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public DamageMultiplier damageMultiplier;

	public List<string> buffActions;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 yawRange;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 pitchRange;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 spreadHorizontal;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 spreadVertical;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityAlive currentEntityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float targetChestHeadDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float targetChestHeadPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public MiniTurretFireController.TurretEntitySorter sorter;

	public EntityTurret entityTurret;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Handle turretSpinAudioHandle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cSeekRayRadius = 0.05f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public FastTags<TagGroup.Global> tmpTag;

	public static FastTags<TagGroup.Global> RangedTag = FastTags<TagGroup.Global>.Parse("ranged");

	public static FastTags<TagGroup.Global> MeleeTag = FastTags<TagGroup.Global>.Parse("melee");

	public static FastTags<TagGroup.Global> PrimaryTag = FastTags<TagGroup.Global>.Parse("primary");

	public static FastTags<TagGroup.Global> SecondaryTag = FastTags<TagGroup.Global>.Parse("secondary");

	public static FastTags<TagGroup.Global> TurretTag = FastTags<TagGroup.Global>.Parse("turret");

	[PublicizedFrom(EAccessModifier.Private)]
	public enum TurretState
	{
		Asleep,
		Awake,
		Overheated
	}

	public class TurretEntitySorter : IComparer<Entity>
	{
		public TurretEntitySorter(Vector3 _self)
		{
			this.self = _self;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int isNearer(Entity _e, Entity _other)
		{
			float num = this.DistanceSqr(this.self, _e.position);
			float num2 = this.DistanceSqr(this.self, _other.position);
			if (num < num2)
			{
				return -1;
			}
			if (num > num2)
			{
				return 1;
			}
			return 0;
		}

		public int Compare(Entity _obj1, Entity _obj2)
		{
			return this.isNearer(_obj1, _obj2);
		}

		public float DistanceSqr(Vector3 pointA, Vector3 pointB)
		{
			Vector3 vector = pointA - pointB;
			return vector.x * vector.x + vector.y * vector.y + vector.z * vector.z;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public Vector3 self;
	}
}
