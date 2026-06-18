using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Audio;
using UnityEngine;

public class AutoTurretFireController : MonoBehaviour
{
	public Vector3 BlockPosition
	{
		get
		{
			return this.blockPos - Origin.position;
		}
		set
		{
			this.blockPos = value;
		}
	}

	public float CenteredYaw
	{
		get
		{
			return this.TileEntity.CenteredYaw;
		}
		set
		{
			this.TileEntity.CenteredYaw = value;
		}
	}

	public float CenteredPitch
	{
		get
		{
			return this.TileEntity.CenteredPitch;
		}
		set
		{
			this.TileEntity.CenteredPitch = value;
		}
	}

	public float MaxDistance
	{
		get
		{
			return this.maxDistance;
		}
	}

	public void Init(DynamicProperties _properties, AutoTurretController _atc)
	{
		this.atc = _atc;
		this.IsOn = false;
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
		if (_properties.Values.ContainsKey("IdleSound"))
		{
			this.idleSound = _properties.Values["IdleSound"];
		}
		else
		{
			this.idleSound = "Electricity/Turret/turret_idle_lp";
		}
		if (_properties.Values.ContainsKey("EntityDamage"))
		{
			this.entityDamage = int.Parse(_properties.Values["EntityDamage"]);
		}
		if (_properties.Values.ContainsKey("BlockDamage"))
		{
			this.blockDamage = int.Parse(_properties.Values["BlockDamage"]);
		}
		else
		{
			this.blockDamage = 0;
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
			this.spread = new Vector2(-num3, num3);
		}
		else
		{
			this.spread = new Vector2(-1f, 1f);
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
			this.burstFireRateMax = StringParsers.ParseFloat(_properties.Values["BurstFireRate"], 0, -1, NumberStyles.Any);
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
		this.buffActions = new List<string>();
		if (_properties.Values.ContainsKey("Buff"))
		{
			string[] array = _properties.Values["Buff"].Replace(" ", "").Split(',', StringSplitOptions.None);
			this.buffActions.AddRange(array);
		}
		this.targetingBounds = this.Cone.GetComponent<MeshRenderer>().bounds;
		this.damageMultiplier = new DamageMultiplier(_properties);
		this.sorter = new AutoTurretFireController.TurretEntitySorter(this.BlockPosition);
		this.state = AutoTurretFireController.TurretState.Asleep;
		this.Cone.localScale = new Vector3(this.Cone.localScale.x * (this.yawRange.y / 22.5f) * (this.maxDistance / 5.25f), this.Cone.localScale.y * (this.pitchRange.y / 22.5f) * (this.maxDistance / 5.25f), this.Cone.localScale.z * (this.maxDistance / 5.25f));
		this.Cone.gameObject.SetActive(false);
		this.Laser.localScale = new Vector3(this.Laser.localScale.x, this.Laser.localScale.y, this.Laser.localScale.z * (this.maxDistance / 5.25f));
		this.Laser.gameObject.SetActive(false);
	}

	public void OnDestroy()
	{
		this.OnPoweredOff();
	}

	public bool hasTarget
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.currentEntityTarget != null;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.atc == null || this.TileEntity == null)
		{
			return;
		}
		if (!this.IsOn || this.atc.UserAccessingId != -1 || this.TileEntity.IsUserAccessing())
		{
			if (this.atc.UserAccessingId != -1)
			{
				this.atc.YawController.Yaw = this.CenteredYaw;
				this.atc.YawController.UpdateYaw();
				this.atc.PitchController.Pitch = this.CenteredPitch;
				this.atc.PitchController.UpdatePitch();
				switch (this.state)
				{
				case AutoTurretFireController.TurretState.Asleep:
					this.state = AutoTurretFireController.TurretState.Awake;
					return;
				case AutoTurretFireController.TurretState.Awake:
					if (this.burstRoundCount >= this.burstRoundCountMax)
					{
						this.state = AutoTurretFireController.TurretState.Overheated;
						this.burstRoundCount = 0;
						return;
					}
					break;
				case AutoTurretFireController.TurretState.Overheated:
					if (this.coolOffTime == 0f)
					{
						this.broadcastPlay(this.overheatSound);
					}
					if (this.coolOffTime < this.coolOffTimeMax)
					{
						this.coolOffTime += Time.deltaTime;
						return;
					}
					this.state = AutoTurretFireController.TurretState.Awake;
					this.coolOffTime = 0f;
					this.broadcastStop(this.overheatSound);
					return;
				default:
					return;
				}
			}
			else if (!this.IsOn)
			{
				if (this.atc.YawController.Yaw != this.CenteredYaw)
				{
					this.atc.YawController.Yaw = this.CenteredYaw;
					this.atc.YawController.UpdateYaw();
				}
				if (this.atc.PitchController.Pitch != this.CenteredPitch)
				{
					this.atc.PitchController.Pitch = this.CenteredPitch;
					this.atc.PitchController.UpdatePitch();
					return;
				}
			}
			else
			{
				if (this.atc.YawController.Yaw != this.CenteredYaw)
				{
					this.atc.YawController.Yaw = this.CenteredYaw;
					this.atc.YawController.UpdateYaw();
				}
				if (this.atc.PitchController.Pitch != this.CenteredPitch)
				{
					this.atc.PitchController.Pitch = this.CenteredPitch;
					this.atc.PitchController.UpdatePitch();
				}
			}
			return;
		}
		if (!this.hasTarget)
		{
			this.findTarget();
		}
		else if (this.shouldIgnoreTarget(this.currentEntityTarget))
		{
			this.currentEntityTarget = null;
			if (!this.state.Equals(AutoTurretFireController.TurretState.Overheated))
			{
				this.state = AutoTurretFireController.TurretState.Asleep;
				this.wakeUpTime = 0f;
			}
		}
		if (this.atc.IsTurning)
		{
			this.broadcastPlay(this.targetingSound);
			this.broadcastStop(this.idleSound);
		}
		else
		{
			this.broadcastStop(this.targetingSound);
			this.broadcastPlay(this.idleSound);
		}
		switch (this.state)
		{
		case AutoTurretFireController.TurretState.Asleep:
			if (this.hasTarget)
			{
				if (this.wakeUpTime == 0f)
				{
					this.broadcastPlay(this.wakeUpSound);
				}
				float num = this.wakeUpTime;
				PassiveEffects passiveEffects = PassiveEffects.TurretWakeUp;
				ItemValue itemValue = null;
				EntityAlive entityAlive = this.currentEntityTarget;
				if (num < EffectManager.GetValue(passiveEffects, itemValue, this.wakeUpTimeMax, entityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false))
				{
					this.wakeUpTime += Time.deltaTime;
				}
				else
				{
					this.state = AutoTurretFireController.TurretState.Awake;
					this.wakeUpTime = 0f;
				}
			}
			else
			{
				this.atc.YawController.Yaw = this.CenteredYaw;
				this.atc.PitchController.Pitch = this.CenteredPitch;
			}
			break;
		case AutoTurretFireController.TurretState.Awake:
			if (this.hasTarget)
			{
				float yaw = this.atc.YawController.Yaw;
				float pitch = this.atc.PitchController.Pitch;
				Vector3 zero = Vector3.zero;
				if (!this.canHitEntity(ref yaw, ref pitch, out zero))
				{
					this.overshootTime += Time.deltaTime;
				}
				if (this.overshootTime >= this.overshootTimeMax)
				{
					this.currentEntityTarget = null;
					this.overshootTime = 0f;
					return;
				}
				this.fallAsleepTime = 0f;
				this.atc.YawController.Yaw = yaw;
				this.atc.PitchController.Pitch = pitch;
				if (this.burstRoundCount < this.burstRoundCountMax)
				{
					if (this.burstFireRate < this.burstFireRateMax)
					{
						this.burstFireRate += Time.deltaTime;
					}
					else
					{
						this.Fire();
						this.burstFireRate = 0f;
					}
				}
				else
				{
					this.state = AutoTurretFireController.TurretState.Overheated;
					this.burstRoundCount = 0;
				}
			}
			else if (this.currentEntityTarget != null && this.fallAsleepTime < this.fallAsleepTimeMax)
			{
				this.fallAsleepTime += Time.deltaTime;
			}
			else
			{
				this.currentEntityTarget = null;
				this.state = AutoTurretFireController.TurretState.Asleep;
				this.fallAsleepTime = 0f;
			}
			break;
		case AutoTurretFireController.TurretState.Overheated:
			if (this.coolOffTime == 0f)
			{
				this.broadcastPlay(this.overheatSound);
			}
			if (this.coolOffTime < this.coolOffTimeMax)
			{
				this.coolOffTime += Time.deltaTime;
			}
			else
			{
				this.state = AutoTurretFireController.TurretState.Awake;
				this.coolOffTime = 0f;
				this.broadcastStop(this.overheatSound);
			}
			break;
		}
		this.dispatchSoundCommandsThrottle(Time.deltaTime);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void findTarget()
	{
		Vector3 position = this.Cone.transform.position;
		this.currentEntityTarget = null;
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), new Bounds(this.blockPos, Vector3.one * (this.maxDistance * 2f)), new List<Entity>());
		entitiesInBounds.Sort(this.sorter);
		bool flag = false;
		Collider[] array = Physics.OverlapSphere(position + Origin.position, 0.05f);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].gameObject != this.atc.gameObject)
			{
				flag = true;
				break;
			}
		}
		if (entitiesInBounds.Count > 0 && !flag)
		{
			for (int j = 0; j < entitiesInBounds.Count; j++)
			{
				if (!this.shouldIgnoreTarget(entitiesInBounds[j]))
				{
					Vector3 zero = Vector3.zero;
					float centeredYaw = this.CenteredYaw;
					float centeredPitch = this.CenteredPitch;
					if (this.trackTarget(entitiesInBounds[j], ref centeredYaw, ref centeredPitch, out zero))
					{
						Vector3 normalized = (zero - position).normalized;
						Ray ray;
						ray..ctor(position + Origin.position - normalized * 0.05f, normalized);
						if (Voxel.Raycast(GameManager.Instance.World, ray, this.maxDistance + 0.05f, -538750989, 8, 0f) && Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
						{
							if (Voxel.voxelRayHitInfo.tag == "E_Vehicle")
							{
								EntityVehicle entityVehicle = EntityVehicle.FindCollisionEntity(Voxel.voxelRayHitInfo.transform);
								if (entityVehicle != null && entityVehicle.IsAttached(entitiesInBounds[j]))
								{
									this.currentEntityTarget = entitiesInBounds[j] as EntityAlive;
									return;
								}
								this.currentEntityTarget = null;
							}
							else
							{
								Transform hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
								if (!(hitRootTransform == null))
								{
									Entity component = hitRootTransform.GetComponent<Entity>();
									if (component != null)
									{
										if (component == entitiesInBounds[j])
										{
											this.currentEntityTarget = component as EntityAlive;
											return;
										}
										this.currentEntityTarget = null;
									}
								}
							}
						}
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool shouldIgnoreTarget(Entity _target)
	{
		if (Vector3.Dot(_target.position - this.TileEntity.ToWorldPos().ToVector3(), this.Cone.transform.forward) > 0f)
		{
			if (_target == this.currentEntityTarget)
			{
				this.currentEntityTarget = null;
			}
			return true;
		}
		if (!_target.IsAlive())
		{
			return true;
		}
		if (_target is EntitySupplyCrate)
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
			if (persistentPlayerList != null && persistentPlayerList.EntityToPlayerMap.ContainsKey(_target.entityId) && this.TileEntity.IsOwner(persistentPlayerList.EntityToPlayerMap[_target.entityId].PrimaryId))
			{
				flag = true;
			}
			if (!flag)
			{
				PersistentPlayerData playerData = persistentPlayerList.GetPlayerData(this.TileEntity.GetOwner());
				if (playerData != null && persistentPlayerList.EntityToPlayerMap.ContainsKey(_target.entityId))
				{
					PersistentPlayerData persistentPlayerData = persistentPlayerList.EntityToPlayerMap[_target.entityId];
					if (playerData.IsAlly(persistentPlayerData))
					{
						flag2 = true;
					}
				}
			}
			if (@int == EnumPlayerKillingMode.NoKilling)
			{
				return true;
			}
			if (flag && !this.TileEntity.TargetSelf)
			{
				return true;
			}
			if (flag2 && (!this.TileEntity.TargetAllies || (@int != EnumPlayerKillingMode.KillEveryone && @int != EnumPlayerKillingMode.KillAlliesOnly)))
			{
				return true;
			}
			if (!flag && !flag2 && (!this.TileEntity.TargetStrangers || (@int != EnumPlayerKillingMode.KillStrangersOnly && @int != EnumPlayerKillingMode.KillEveryone)))
			{
				return true;
			}
		}
		return _target is EntityTurret || _target is EntityDrone || (_target is EntityNPC && !this.TileEntity.TargetStrangers) || (_target is EntityEnemy && !this.TileEntity.TargetZombies) || (_target is EntityAnimal && !_target.EntityClass.bIsEnemyEntity);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool canHitEntity(ref float _yaw, ref float _pitch, out Vector3 targetPos)
	{
		Vector3 vector = this.Cone.transform.position - Origin.position;
		if (!this.trackTarget(this.currentEntityTarget, ref _yaw, ref _pitch, out targetPos))
		{
			return false;
		}
		Ray ray;
		ray..ctor(vector, (targetPos - this.Cone.transform.position).normalized);
		if (Voxel.Raycast(GameManager.Instance.World, ray, this.maxDistance, -538750989, 8, 0f) && Voxel.voxelRayHitInfo.tag.StartsWith("E_"))
		{
			Transform hitRootTransform = GameUtils.GetHitRootTransform(Voxel.voxelRayHitInfo.tag, Voxel.voxelRayHitInfo.transform);
			if (hitRootTransform == null)
			{
				return false;
			}
			Entity component = hitRootTransform.GetComponent<Entity>();
			if (component != null && component.IsAlive() && this.currentEntityTarget == component)
			{
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool trackTarget(Entity _target, ref float _yaw, ref float _pitch, out Vector3 _targetPos)
	{
		if (GameManager.Instance.World.GetGameRandom().RandomFloat < 0.05f)
		{
			_targetPos = _target.getHeadPosition() - Origin.position;
		}
		else
		{
			_targetPos = _target.getChestPosition() - Origin.position;
		}
		Vector3 normalized = (_targetPos - this.atc.YawController.transform.position).normalized;
		Vector3 normalized2 = (_targetPos - this.atc.PitchController.transform.position).normalized;
		float num = Quaternion.LookRotation(normalized).eulerAngles.y - this.atc.transform.rotation.eulerAngles.y;
		float num2 = Quaternion.LookRotation(normalized2).eulerAngles.x - this.atc.transform.rotation.z;
		if (num > 180f)
		{
			num -= 360f;
		}
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

	public void PlayerFire(bool buttonPressed)
	{
		if (this.state == AutoTurretFireController.TurretState.Awake)
		{
			if (this.burstFireRate < this.burstFireRateMax)
			{
				this.burstFireRate += Time.deltaTime;
				return;
			}
			if (buttonPressed)
			{
				if (this.TileEntity.ClientData != null)
				{
					this.TileEntity.ClientData.SendSlots = true;
				}
				this.Fire();
				if (this.TileEntity.ClientData != null)
				{
					this.TileEntity.ClientData.SendSlots = false;
				}
				this.burstFireRate = 0f;
			}
		}
	}

	public void Fire()
	{
		ItemClass itemClass = null;
		if (this.TileEntity != null)
		{
			if (!this.TileEntity.IsLocked)
			{
				return;
			}
			if ((SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer && !this.TileEntity.IsUserAccessing()) || (this.atc != null && this.atc.UserAccessingId != -1))
			{
				if (!this.TileEntity.DecrementAmmo(out itemClass))
				{
					this.TileEntity.IsLocked = false;
					this.TileEntity.SetModified();
					return;
				}
				this.burstRoundCount++;
			}
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || (this.atc != null && this.atc.UserAccessingId != -1))
		{
			ItemValue itemValue = null;
			if (itemClass != null)
			{
				itemValue = new ItemValue(itemClass.Id, false);
			}
			Vector3 vector = this.Cone.position + Origin.position;
			Ray ray;
			ray..ctor(vector, Vector3.forward);
			Vector3 vector2 = this.Cone.forward * -1f;
			GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
			int num = this.rayCount;
			if (itemValue != null)
			{
				num = (int)EffectManager.GetValue(PassiveEffects.RoundRayCount, itemValue, (float)num, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
			Vector2 vector3 = this.getSpread(itemValue);
			float range = this.GetRange(itemValue);
			int num2 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.EntityPenetrationCount, itemValue, 0f, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
			num2++;
			int num3 = Mathf.FloorToInt(EffectManager.GetValue(PassiveEffects.BlockPenetrationFactor, itemValue, 251f, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
			for (int i = 0; i < num; i++)
			{
				this.<Fire>g__fireSingleDirectionBullet|73_0(gameRandom, itemValue, ray, vector2, vector3, range, num2, num3);
			}
			if (!string.IsNullOrEmpty(this.muzzleFireParticle))
			{
				FireControllerUtils.SpawnParticleEffect(new ParticleEffect(this.muzzleFireParticle, this.Muzzle.position + Origin.position, this.Muzzle.rotation, 1f, Color.white, this.fireSound, null, 1f, ""), -1);
			}
			if (!string.IsNullOrEmpty(this.muzzleSmokeParticle))
			{
				float num4 = GameManager.Instance.World.GetLightBrightness(World.worldToBlockPos(this.BlockPosition)) / 2f;
				FireControllerUtils.SpawnParticleEffect(new ParticleEffect(this.muzzleSmokeParticle, this.Muzzle.position + Origin.position, this.Muzzle.rotation, num4, new Color(1f, 1f, 1f, 0.3f), null, null, 1f, ""), -1);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetRange(ItemValue _itemValue)
	{
		return EffectManager.GetValue(PassiveEffects.MaxRange, _itemValue, this.maxDistance, null, null, FastTags<TagGroup.Global>.none, true, true, true, true, true, 1, true, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector2 getSpread(ItemValue _itemValue)
	{
		float value = EffectManager.GetValue(PassiveEffects.SpreadDegreesHorizontal, _itemValue, this.spread.y * 2f, null, null, FastTags<TagGroup.Global>.none, true, true, true, true, true, 1, true, false);
		float value2 = EffectManager.GetValue(PassiveEffects.SpreadDegreesVertical, _itemValue, this.spread.y * 2f, null, null, FastTags<TagGroup.Global>.none, true, true, true, true, true, 1, true, false);
		return new Vector2(value, value2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetDamageEntity(ItemValue _itemValue)
	{
		return EffectManager.GetValue(PassiveEffects.EntityDamage, _itemValue, (float)this.entityDamage, null, null, FastTags<TagGroup.Global>.none, true, true, true, true, true, 1, true, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float GetDamageBlock(ItemValue _itemValue, BlockValue _blockValue)
	{
		FastTags<TagGroup.Global> fastTags = FastTags<TagGroup.Global>.none;
		fastTags |= _blockValue.Block.Tags;
		float value = EffectManager.GetValue(PassiveEffects.BlockDamage, _itemValue, (float)this.blockDamage, null, null, fastTags, true, true, true, true, true, 1, true, false);
		return Utils.FastMin((float)_blockValue.Block.blockMaterial.MaxIncomingDamage, value);
	}

	public void OnPoweredOff()
	{
		this.broadcastStop(this.targetingSound);
		this.broadcastStop(this.overheatSound);
		this.broadcastStop(this.idleSound);
		this.dispatchSoundCommands();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void broadcastPlay(string name)
	{
		this.broadcastSoundAction(name, true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void broadcastStop(string name)
	{
		this.broadcastSoundAction(name, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void broadcastSoundAction(string name, bool play)
	{
		if (!string.IsNullOrEmpty(name))
		{
			this.soundCommandDictionary[name] = play;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dispatchSoundCommands()
	{
		foreach (KeyValuePair<string, bool> keyValuePair in this.soundCommandDictionary)
		{
			if (keyValuePair.Value)
			{
				Manager.BroadcastPlay(this.blockPos, keyValuePair.Key, 0f);
			}
			else
			{
				Manager.BroadcastStop(this.blockPos, keyValuePair.Key);
			}
		}
		this.soundCommandDictionary.Clear();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void dispatchSoundCommandsThrottle(float deltaTime)
	{
		this.timeSinceDispatchSounds += Time.deltaTime;
		if (this.timeSinceDispatchSounds > 1f)
		{
			this.timeSinceDispatchSounds %= 1f;
			this.dispatchSoundCommands();
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 <Fire>g__fireSingleDirectionBullet|73_0(GameRandom _random, ItemValue _ammo, Ray ray, Vector3 _turretLookDirection, Vector2 _localSpread, float range, int penCount, int blockPenFactor)
	{
		ray.direction = Quaternion.Euler((float)_random.RandomRange(-1, 1) * _localSpread.x, (float)_random.RandomRange(-1, 1) * _localSpread.y, 0f) * _turretLookDirection;
		this.waterCollisionParticles.Init(this.TileEntity.OwnerEntityID, "bullet", "water", 16);
		this.waterCollisionParticles.CheckCollision(ray.origin, ray.direction, range, -1);
		int num = 8;
		EntityAlive entityAlive = null;
		for (int i = 0; i < penCount; i++)
		{
			if (Voxel.Raycast(GameManager.Instance.World, ray, range, -538751005, num, 0f))
			{
				WorldRayHitInfo worldRayHitInfo = Voxel.voxelRayHitInfo.Clone();
				if (worldRayHitInfo.hit.distanceSq > range * range)
				{
					return ray.direction;
				}
				ray.origin = worldRayHitInfo.hit.pos;
				if (worldRayHitInfo.tag.StartsWith("E_"))
				{
					string text;
					EntityAlive entityAlive2 = ItemActionAttack.FindHitEntityNoTagCheck(worldRayHitInfo, out text) as EntityAlive;
					if (entityAlive == entityAlive2)
					{
						ray.origin = worldRayHitInfo.hit.pos + ray.direction * 0.1f;
						i--;
						goto IL_0336;
					}
					entityAlive = entityAlive2;
				}
				else
				{
					i += Mathf.FloorToInt((float)ItemActionAttack.GetBlockHit(GameManager.Instance.World, worldRayHitInfo).Block.MaxDamage / (float)blockPenFactor);
				}
				float num2 = 1f;
				float value = EffectManager.GetValue(PassiveEffects.DamageFalloffRange, _ammo, range, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				if (worldRayHitInfo.hit.distanceSq > value * value)
				{
					num2 = 1f - (worldRayHitInfo.hit.distanceSq - value * value) / (range * range - value * value);
				}
				float num3 = 1f;
				World world = GameManager.Instance.World;
				Vector3i vector3i = World.worldToBlockPos(worldRayHitInfo.hit.pos);
				WaterValue water = world.GetWater(vector3i);
				if (water.HasMass())
				{
					Vector3i vector3i2 = new Vector3i(vector3i.x, vector3i.y + 1, vector3i.z);
					if (world.GetWater(vector3i2).GetMassPercent() > 0f)
					{
						num3 = 0.25f;
					}
					else
					{
						float num4 = worldRayHitInfo.hit.pos.y - (float)vector3i.y;
						float num5 = water.GetMassPercent() * 0.6f - num4;
						if (num5 > 0f)
						{
							num3 = 1f - 0.75f * num5;
						}
					}
				}
				bool flag = this.atc != null && this.atc.UserAccessingId != -1;
				ItemActionAttack.Hit(worldRayHitInfo, this.TileEntity.OwnerEntityID, EnumDamageTypes.Piercing, this.GetDamageBlock(_ammo, ItemActionAttack.GetBlockHit(GameManager.Instance.World, worldRayHitInfo)) * num2 * num3, this.GetDamageEntity(_ammo) * num2 * num3, 1f, 1f, 0.5f, 0.05f, "bullet", this.damageMultiplier, this.buffActions, new ItemActionAttack.AttackHitInfo(), 3, 0, 0f, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, flag ? this.atc.UserAccessingId : (-2), _ammo, false, false, flag, null);
			}
			IL_0336:;
		}
		return ray.direction;
	}

	public bool IsOn;

	public Transform Cone;

	public Transform Laser;

	public Transform Muzzle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 blockPos;

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
	public float fireRateMax = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float findTargetDelayMax = 0.5f;

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
	public int raySpread;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float wakeUpTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float wakeUpTimeMax = 0.6522f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fallAsleepTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fallAsleepTimeMax = 10f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int burstRoundCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int burstRoundCountMax = 20;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float burstFireRate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float burstFireRateMax = 0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float coolOffTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float coolOffTimeMax = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float overshootTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float overshootTimeMax = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float retargetSoundTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float retargetSoundTimeMax = 0.874f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Bounds targetingBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AutoTurretFireController.TurretState state;

	[PublicizedFrom(EAccessModifier.Private)]
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
	public string idleSound;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string muzzleFireParticle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string muzzleSmokeParticle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public string ammoItemName;

	[PublicizedFrom(EAccessModifier.Private)]
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
	public Vector2 spread;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float fireRate = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float findTargetDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive currentEntityTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AutoTurretController atc;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public AutoTurretFireController.TurretEntitySorter sorter;

	public TileEntityPoweredRangedTrap TileEntity;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cTimeBetweenSoundDispatch = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float timeSinceDispatchSounds;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Dictionary<string, bool> soundCommandDictionary = new Dictionary<string, bool>();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<string> soundsPlayOrder = new List<string>();

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
