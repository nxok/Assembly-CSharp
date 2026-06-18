using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ProjectileMoveScript : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	static ProjectileMoveScript()
	{
		GameManager.Instance.OnWorldChanged += ProjectileMoveScript.OnWorldChanged;
		ProjectileMoveScript.OnWorldChanged(GameManager.Instance.World);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void Awake()
	{
	}

	public void Fire(Vector3 _idealStartPos, Vector3 _dirOrPos, Entity _firingEntity, int _hmOverride = 0, float _radius = 0f, bool _isBallistic = false)
	{
		Transform transform = base.transform;
		this.isOnIdealPos = true;
		this.previousPosition = transform.position + Origin.position;
		if (_idealStartPos.y != 0f)
		{
			this.idealPosition = _idealStartPos;
			this.isOnIdealPos = false;
		}
		this.firingEntity = _firingEntity;
		this.hitMask = ((_hmOverride == 0) ? 80 : _hmOverride);
		this.radius = ((_radius >= 0f) ? _radius : this.itemActionProjectile.collisionRadius);
		if (this.itemActionProjectile.FlyTime < 0f)
		{
			EntityAlive entityAlive = _firingEntity as EntityAlive;
			if (entityAlive != null)
			{
				if (_isBallistic)
				{
					Vector3 vector = _dirOrPos - _idealStartPos;
					float magnitude = vector.magnitude;
					this.flyDirection = vector / magnitude;
					float num = Utils.FastLerp(0.4f, -this.itemActionProjectile.FlyTime, magnitude / this.itemActionProjectile.Velocity);
					this.velocity = vector / num;
					this.velocity.y = this.velocity.y + this.itemActionProjectile.Gravity * -0.5f * num;
					Vector3 vector2 = this.velocity * 0.015f;
					this.previousPosition += vector2;
					this.idealPosition += vector2;
				}
				else
				{
					this.flyDirection = entityAlive.GetForwardVector();
					this.velocity = this.flyDirection * 2f;
					this.explosionDisabled = true;
				}
			}
			this.gravity = this.itemActionProjectile.Gravity;
		}
		else
		{
			this.flyDirection = _dirOrPos.normalized;
			this.velocity = this.flyDirection * EffectManager.GetValue(PassiveEffects.ProjectileVelocity, this.itemValueLauncher, this.itemActionProjectile.Velocity, _firingEntity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			this.gravity = EffectManager.GetValue(PassiveEffects.ProjectileGravity, this.itemValueLauncher, this.itemActionProjectile.Gravity, _firingEntity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		}
		this.waterCollisionParticles.Init(this.ProjectileOwnerID, this.itemProjectile.MadeOfMaterial.SurfaceCategory, "water", 16);
		OnActivateItemGameObjectReference component = transform.GetComponent<OnActivateItemGameObjectReference>();
		if (component)
		{
			component.ActivateItem(true);
		}
		if (transform.parent)
		{
			transform.SetParent(null);
			Utils.SetLayerRecursively(transform.gameObject, 0);
		}
		transform.position = this.previousPosition - Origin.position;
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
		this.SetState(ProjectileMoveScript.State.Active);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetState(ProjectileMoveScript.State _state)
	{
		this.state = _state;
		this.stateTime = 0f;
		if (this.state == ProjectileMoveScript.State.Dead)
		{
			Transform transform = base.transform;
			Transform transform2 = transform.Find("MeshExplode");
			if (transform2)
			{
				transform2.gameObject.SetActive(false);
			}
			Light componentInChildren = transform.GetComponentInChildren<Light>();
			if (componentInChildren)
			{
				componentInChildren.gameObject.SetActive(false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnWorldChanged(World _world)
	{
		if (ProjectileMoveScript.s_lastWorld == _world)
		{
			return;
		}
		if (ProjectileMoveScript.s_lastWorld != null)
		{
			ProjectileMoveScript.s_lastWorld.EntityUnloadedDelegates -= ProjectileMoveScript.OnEntityUnloaded;
		}
		if (_world != null)
		{
			_world.EntityUnloadedDelegates += ProjectileMoveScript.OnEntityUnloaded;
		}
		ProjectileMoveScript.s_lastWorld = _world;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		this.stateTime += fixedDeltaTime;
		if (this.state == ProjectileMoveScript.State.Active)
		{
			GameManager instance = GameManager.Instance;
			if (!instance || instance.World == null)
			{
				return;
			}
			if (this.stateTime > this.itemActionProjectile.FlyTime)
			{
				this.velocity.y = this.velocity.y + this.gravity * fixedDeltaTime;
			}
			Transform transform = base.transform;
			Vector3 vector = transform.position;
			Vector3 vector2 = this.velocity * fixedDeltaTime;
			transform.LookAt(vector + vector2);
			vector += vector2;
			if (!this.isOnIdealPos)
			{
				this.idealPosition += vector2;
				vector = Vector3.Lerp(vector, this.idealPosition - Origin.position, this.stateTime * 5f);
				this.isOnIdealPos = this.stateTime >= 0.2f;
			}
			transform.position = vector;
			if (this.stateTime >= this.itemActionProjectile.LifeTime)
			{
				this.SetState(ProjectileMoveScript.State.Dead);
			}
		}
		else if (this.state == ProjectileMoveScript.State.Sticky)
		{
			GameManager instance2 = GameManager.Instance;
			World world = ((instance2 != null) ? instance2.World : null);
			if (this.stateTime >= 180f)
			{
				this.SetState(ProjectileMoveScript.State.StickyDestroyed);
				Object.Destroy(base.gameObject);
			}
			else if (world != null)
			{
				bool flag = this.stickyForceRevive;
				if (!flag && this.stickyRay != null && this.stickyRay.Value.direction != Vector3.zero && this.stickyRayDistance > 0f && (!Voxel.Raycast(world, this.stickyRay.Value, this.stickyRayDistance, -538751005, this.hitMask, this.radius) || (!GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) && !Voxel.voxelRayHitInfo.tag.StartsWith("E_"))))
				{
					flag = true;
				}
				if (flag)
				{
					this.DoRevive();
				}
			}
		}
		else if (this.state == ProjectileMoveScript.State.Dead && this.stateTime > this.itemActionProjectile.DeadTime)
		{
			this.SetState(ProjectileMoveScript.State.Destroyed);
			Object.Destroy(base.gameObject);
		}
		if (this.state == ProjectileMoveScript.State.Active)
		{
			this.checkCollision();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DoRevive()
	{
		if (this.ProjectileID == -1)
		{
			return;
		}
		GameManager instance = GameManager.Instance;
		GameRandom gameRandom;
		if (instance == null)
		{
			gameRandom = null;
		}
		else
		{
			World world = instance.World;
			gameRandom = ((world != null) ? world.GetGameRandom() : null);
		}
		GameRandom gameRandom2 = gameRandom;
		if (gameRandom2 == null)
		{
			return;
		}
		if (!ProjectileManager.RemoveProjectile(this.ProjectileID, true))
		{
			return;
		}
		Component transform = base.transform;
		this.stickyRay = null;
		this.stickyRayDistance = 0f;
		this.stickyForceRevive = false;
		Quaternion quaternion = Quaternion.FromToRotation(Vector3.forward, this.velocity.normalized);
		Vector3 normalized = new Vector3(gameRandom2.RandomGaussian * 0.4f, gameRandom2.RandomGaussian * 0.4f, 1f).normalized;
		float num = gameRandom2.RandomFloat * 0.9f;
		this.velocity = quaternion * normalized * num;
		if (this.velocity.y > 0f)
		{
			this.velocity.y = 0f;
		}
		Utils.SetLayerRecursively(transform.gameObject, 0);
		this.SetState(ProjectileMoveScript.State.Active);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void checkCollision()
	{
		GameManager instance = GameManager.Instance;
		if (!instance)
		{
			return;
		}
		World world = instance.World;
		if (world == null)
		{
			return;
		}
		Vector3 vector;
		if (this.isOnIdealPos)
		{
			vector = base.transform.position + Origin.position;
		}
		else
		{
			vector = this.idealPosition;
		}
		Vector3 vector2 = vector - this.previousPosition;
		float magnitude = vector2.magnitude;
		if (magnitude < 0.04f)
		{
			return;
		}
		Ray ray;
		ray..ctor(this.previousPosition, vector2);
		this.previousPosition = vector;
		EntityAlive entityAlive = (EntityAlive)this.firingEntity;
		this.waterCollisionParticles.CheckCollision(ray.origin, ray.direction, magnitude, entityAlive ? entityAlive.entityId : (-1));
		int num = -1;
		if (entityAlive && entityAlive.emodel)
		{
			num = entityAlive.GetModelLayer();
			entityAlive.SetModelLayer(2, false, null);
		}
		float num2 = this.radius + this.collisionStartBack;
		ray.origin -= ray.direction * num2;
		bool flag = Voxel.Raycast(world, ray, magnitude + num2, -538751005, this.hitMask, this.radius);
		this.collisionStartBack = 0.1f;
		if (num >= 0)
		{
			entityAlive.SetModelLayer(num, false, null);
		}
		if (flag && (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag) || Voxel.voxelRayHitInfo.tag.StartsWith("E_")))
		{
			if (this.firingEntity && !this.firingEntity.isEntityRemote)
			{
				EntityAlive entityAlive2 = ItemActionAttack.FindHitEntity(Voxel.voxelRayHitInfo) as EntityAlive;
				EntityDrone entityDrone = entityAlive2 as EntityDrone;
				EntityAlive entityAlive3 = this.firingEntity as EntityAlive;
				if (entityDrone && entityAlive3 && entityDrone.isAlly(entityAlive3))
				{
					return;
				}
				entityAlive.MinEventContext.Other = entityAlive2;
				ItemActionAttack.AttackHitInfo attackHitInfo = new ItemActionAttack.AttackHitInfo
				{
					WeaponTypeTag = ItemActionAttack.RangedTag
				};
				MinEventParams.CachedEventParam.Self = entityAlive;
				MinEventParams.CachedEventParam.Position = Voxel.voxelRayHitInfo.hit.pos;
				MinEventParams.CachedEventParam.ItemValue = this.itemValueProjectile;
				MinEventParams.CachedEventParam.Other = entityAlive2;
				MinEventParams.CachedEventParam.ItemActionData = this.actionData;
				this.itemProjectile.FireEvent(MinEventTypes.onProjectilePreImpact, MinEventParams.CachedEventParam);
				float num3 = Utils.FastLerp(1f, this.itemActionProjectile.GetDamageBlock(this.itemValueLauncher, ItemActionAttack.GetBlockHit(world, Voxel.voxelRayHitInfo), entityAlive, 0), this.actionData.strainPercent);
				float num4 = Utils.FastLerp(1f, this.itemActionProjectile.GetDamageEntity(this.itemValueLauncher, entityAlive, 0), this.actionData.strainPercent);
				bool flag2;
				if (entityAlive3 is EntityPlayer)
				{
					num4 *= ItemActionAttack.RangedDamagePercent;
					flag2 = ItemActionAttack.RangedDamagePercent == 0f;
				}
				else
				{
					num4 *= ItemActionAttack.EntityPlayerDamagePercent;
					flag2 = ItemActionAttack.EntityPlayerDamagePercent == 0f;
				}
				ItemActionAttack.Hit(Voxel.voxelRayHitInfo, this.ProjectileOwnerID, EnumDamageTypes.Piercing, num3, num4, 1f, 1f, EffectManager.GetValue(PassiveEffects.CriticalChance, this.itemValueLauncher, this.itemProjectile.CritChance.Value, entityAlive, null, this.itemProjectile.ItemTags, true, true, true, true, true, 1, true, false), ItemAction.GetDismemberChance(this.actionData, Voxel.voxelRayHitInfo), this.itemProjectile.MadeOfMaterial.SurfaceCategory, this.itemActionProjectile.GetDamageMultiplier(), this.getBuffActions(), attackHitInfo, 1, this.itemActionProjectile.ActionExp, this.itemActionProjectile.ActionExpBonusMultiplier, null, null, ItemActionAttack.EnumAttackMode.RealNoHarvesting, null, -1, this.itemValueLauncher, false, flag2, true, null);
				if (!entityAlive2)
				{
					entityAlive.FireEvent(MinEventTypes.onSelfPrimaryActionMissEntity, true);
				}
				entityAlive.FireEvent(MinEventTypes.onProjectileImpact, false);
				MinEventParams.CachedEventParam.Self = entityAlive;
				MinEventParams.CachedEventParam.Position = Voxel.voxelRayHitInfo.hit.pos;
				MinEventParams.CachedEventParam.ItemValue = this.itemValueProjectile;
				MinEventParams.CachedEventParam.Other = entityAlive2;
				MinEventParams.CachedEventParam.ItemActionData = this.actionData;
				this.itemProjectile.FireEvent(MinEventTypes.onProjectileImpact, MinEventParams.CachedEventParam);
				if (this.itemActionProjectile.Explosion.ParticleIndex > 0 && !this.explosionDisabled)
				{
					Vector3 vector3 = Voxel.voxelRayHitInfo.hit.pos - vector2.normalized * 0.1f;
					Vector3i vector3i = World.worldToBlockPos(vector3);
					if (!world.GetBlock(vector3i).isair)
					{
						BlockFace blockFace;
						vector3i = Voxel.OneVoxelStep(vector3i, vector3, -vector2.normalized, out vector3, out blockFace);
					}
					instance.ExplosionServer(vector3, vector3i, Quaternion.identity, this.itemActionProjectile.Explosion, this.ProjectileOwnerID, 0f, false, this.itemValueLauncher);
					this.SetState(ProjectileMoveScript.State.Dead);
					return;
				}
				if (!(entityAlive2 is EntitySwarm))
				{
					if (!this.itemProjectile.IsSticky)
					{
						this.SetState(ProjectileMoveScript.State.Dead);
						return;
					}
					GameRandom gameRandom = world.GetGameRandom();
					if (GameUtils.IsBlockOrTerrain(Voxel.voxelRayHitInfo.tag))
					{
						float num5 = this.stickyRoll.GetValueOrDefault();
						float num6;
						if (this.stickyRoll == null)
						{
							num5 = gameRandom.RandomFloat;
							this.stickyRoll = new float?(num5);
							num6 = num5;
						}
						else
						{
							num6 = num5;
						}
						if (num6 < EffectManager.GetValue(PassiveEffects.ProjectileStickChance, this.itemValueLauncher, 0.5f, entityAlive, null, this.itemProjectile.ItemTags | FastTags<TagGroup.Global>.Parse(Voxel.voxelRayHitInfo.fmcHit.blockValue.Block.blockMaterial.SurfaceCategory), true, true, true, true, true, 1, true, false))
						{
							this.ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, vector2.normalized, this.itemValueProjectile.type);
							this.stickyRay = new Ray?(ray);
							this.stickyRayDistance = magnitude + num2;
							this.SetState(ProjectileMoveScript.State.Sticky);
							return;
						}
						instance.SpawnParticleEffectServer(new ParticleEffect("impact_metal_on_wood", Voxel.voxelRayHitInfo.hit.pos, Utils.BlockFaceToRotation(Voxel.voxelRayHitInfo.fmcHit.blockFace), 1f, Color.white, string.Format("{0}hit{1}", Voxel.voxelRayHitInfo.fmcHit.blockValue.Block.blockMaterial.SurfaceCategory, this.itemProjectile.MadeOfMaterial.SurfaceCategory), null, 1f, ""), this.firingEntity.entityId, false, false);
						this.SetState(ProjectileMoveScript.State.Dead);
						return;
					}
					else
					{
						float num5 = this.stickyRoll.GetValueOrDefault();
						float num7;
						if (this.stickyRoll == null)
						{
							num5 = gameRandom.RandomFloat;
							this.stickyRoll = new float?(num5);
							num7 = num5;
						}
						else
						{
							num7 = num5;
						}
						if (num7 < EffectManager.GetValue(PassiveEffects.ProjectileStickChance, this.itemValueLauncher, 0.5f, entityAlive, null, this.itemProjectile.ItemTags, true, true, true, true, true, 1, true, false))
						{
							this.ProjectileID = ProjectileManager.AddProjectileItem(base.transform, -1, Voxel.voxelRayHitInfo.hit.pos, vector2.normalized, this.itemValueProjectile.type);
							Utils.SetLayerRecursively(ProjectileManager.GetProjectile(this.ProjectileID).gameObject, 14);
							this.SetState(ProjectileMoveScript.State.Sticky);
							return;
						}
						instance.SpawnParticleEffectServer(new ParticleEffect("impact_metal_on_wood", Voxel.voxelRayHitInfo.hit.pos, Utils.BlockFaceToRotation(Voxel.voxelRayHitInfo.fmcHit.blockFace), 1f, Color.white, "bullethitwood", null, 1f, ""), this.firingEntity.entityId, false, false);
						this.SetState(ProjectileMoveScript.State.Dead);
						return;
					}
				}
			}
			else
			{
				this.SetState(ProjectileMoveScript.State.Dead);
			}
		}
	}

	public static void OnEntityUnloaded(Entity _entity, EnumRemoveEntityReason _reason)
	{
		ProjectileMoveScript.<>c__DisplayClass39_0 CS$<>8__locals1;
		CS$<>8__locals1.toUnload = new List<ProjectileMoveScript>();
		ProjectileMoveScript.<OnEntityUnloaded>g__Search|39_0(_entity.RootTransform, ref CS$<>8__locals1);
		foreach (ProjectileMoveScript projectileMoveScript in CS$<>8__locals1.toUnload)
		{
			projectileMoveScript.OnUnloaded();
		}
	}

	public void OnUnloaded()
	{
		if (this.state == ProjectileMoveScript.State.Sticky)
		{
			this.idealPosition = (this.previousPosition = base.transform.position + Origin.position);
			base.transform.SetParent(null);
			base.transform.position = this.previousPosition - Origin.position;
			this.stickyForceRevive = true;
		}
	}

	public void OnDestroy()
	{
		GameManager instance = GameManager.Instance;
		if (!instance || instance.World == null || !this.firingEntity || this.itemValueProjectile == null)
		{
			return;
		}
		if (this.state == ProjectileMoveScript.State.StickyDestroyed)
		{
			return;
		}
		if (this.ProjectileID != -1 && !this.firingEntity.isEntityRemote)
		{
			Vector3 position = base.transform.position;
			if (instance.World.IsChunkAreaLoaded(Mathf.CeilToInt(position.x + Origin.position.x), Mathf.CeilToInt(position.y + Origin.position.y), Mathf.CeilToInt(position.z + Origin.position.z)))
			{
				instance.ItemDropServer(new ItemStack(this.itemValueProjectile, 1), position + Origin.position, Vector3.zero, this.ProjectileOwnerID, 60f, false);
			}
		}
	}

	public bool TryCollect(EntityPlayerLocal _player)
	{
		if (!this.itemProjectile.IsSticky)
		{
			return false;
		}
		ItemStack itemStack = new ItemStack(this.itemValueProjectile, 1);
		if (_player.PlayerUI.xui.PlayerInventory.AddItem(itemStack))
		{
			this.ProjectileID = -1;
			Object.Destroy(base.gameObject);
			return true;
		}
		GameManager.ShowTooltip(_player, Localization.Get("xuiInventoryFullForPickup", false, null), string.Empty, "ui_denied", null, false, false, 0f);
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public List<string> getBuffActions()
	{
		return this.itemActionProjectile.BuffActions;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static void <OnEntityUnloaded>g__Search|39_0(Transform parent, ref ProjectileMoveScript.<>c__DisplayClass39_0 A_1)
	{
		foreach (object obj in parent)
		{
			ProjectileMoveScript.<OnEntityUnloaded>g__Search|39_0((Transform)obj, ref A_1);
		}
		ProjectileMoveScript projectileMoveScript;
		if (parent.TryGetComponent<ProjectileMoveScript>(ref projectileMoveScript))
		{
			A_1.toUnload.Add(projectileMoveScript);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float ReviveSpread = 0.4f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float ReviveVelocity = 0.9f;

	public const int InvalidID = -1;

	public static World s_lastWorld;

	public int ProjectileID = -1;

	public int ProjectileOwnerID;

	public ItemActionProjectile itemActionProjectile;

	public ItemClass itemProjectile;

	public ItemValue itemValueProjectile;

	public ItemValue itemValueLauncher;

	public ItemActionLauncher.ItemActionDataLauncher actionData;

	public Vector3 FinalPosition;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 flyDirection;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 idealPosition;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 velocity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public ProjectileMoveScript.State state;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float stateTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Entity firingEntity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector3 previousPosition;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float gravity;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool isOnIdealPos;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int hitMask;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float radius;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float collisionStartBack;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool explosionDisabled;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public CollisionParticleController waterCollisionParticles = new CollisionParticleController();

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float? stickyRoll;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Ray? stickyRay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stickyRayDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool stickyForceRevive;

	public enum State
	{
		Idle,
		Active,
		Sticky,
		StickyDestroyed,
		Dead,
		Destroyed
	}
}
