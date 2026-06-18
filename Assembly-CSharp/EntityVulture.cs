using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityVulture : EntityFlying
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Awake()
	{
		BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
		if (component)
		{
			component.center = new Vector3(0f, 0.35f, 0f);
			component.size = new Vector3(0.4f, 0.4f, 0.4f);
		}
		base.Awake();
		this.state = EntityVulture.State.WanderStart;
	}

	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		base.Init(_entityClass, _assets, _eModelAssets);
		this.Init();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Init()
	{
		if (this.navigator != null)
		{
			this.navigator.setCanDrown(true);
		}
		this.battleFatigueSeconds = this.rand.RandomRange(30f, 60f);
	}

	public override void SetSleeper()
	{
		base.SetSleeper();
		this.sorter = new EAISetNearestEntityAsTargetSorter(this);
		base.setHomeArea(new Vector3i(this.position), (int)this.sleeperSightRange + 1);
		this.battleFatigueSeconds = float.MaxValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void updateTasks()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.DebugStopEnemiesMoving))
		{
			this.aiManager.UpdateDebugName();
			return;
		}
		if (GameStats.GetInt(EnumGameStats.GameState) == 2)
		{
			return;
		}
		base.CheckDespawn();
		base.GetEntitySenses().ClearIfExpired();
		if (this.IsSleeperPassive)
		{
			return;
		}
		if (this.IsSleeping)
		{
			float seeDistance = this.GetSeeDistance();
			this.world.GetEntitiesInBounds(typeof(EntityPlayer), BoundsUtils.ExpandBounds(this.boundingBox, seeDistance, seeDistance, seeDistance), EntityVulture.list);
			EntityVulture.list.Sort(this.sorter);
			EntityPlayer entityPlayer = null;
			float num = float.MaxValue;
			if (this.noisePlayer != null && this.noisePlayerVolume >= this.sleeperNoiseToWake)
			{
				entityPlayer = this.noisePlayer;
				num = this.noisePlayerDistance;
			}
			for (int i = 0; i < EntityVulture.list.Count; i++)
			{
				EntityPlayer entityPlayer2 = (EntityPlayer)EntityVulture.list[i];
				if (base.CanSee(entityPlayer2))
				{
					float distance = base.GetDistance(entityPlayer2);
					if (base.GetSleeperDisturbedLevel(distance, entityPlayer2.Stealth.lightLevel) >= 2 && distance < num)
					{
						entityPlayer = entityPlayer2;
						num = distance;
					}
				}
			}
			EntityVulture.list.Clear();
			if (entityPlayer == null)
			{
				return;
			}
			base.ConditionalTriggerSleeperWakeUp();
			base.SetAttackTarget(entityPlayer, 1200);
		}
		bool flag = this.Buffs.HasBuff("buffShocked");
		if (flag)
		{
			this.SetState(EntityVulture.State.Stun);
		}
		else
		{
			EntityAlive revengeTarget = base.GetRevengeTarget();
			if (revengeTarget)
			{
				this.battleDuration = 0f;
				this.isBattleFatigued = false;
				base.SetRevengeTarget(null);
				if (revengeTarget != this.attackTarget && (!this.attackTarget || this.rand.RandomFloat < 0.5f))
				{
					base.SetAttackTarget(revengeTarget, 1200);
				}
			}
			if (this.attackTarget != this.currentTarget)
			{
				this.currentTarget = this.attackTarget;
				if (this.currentTarget)
				{
					this.SetState(EntityVulture.State.Attack);
					this.waypoint = this.position;
					this.moveUpdateDelay = 0;
					this.homeCheckDelay = 400;
				}
				else
				{
					this.SetState(EntityVulture.State.AttackStop);
				}
			}
		}
		float sqrMagnitude = (this.waypoint - this.position).sqrMagnitude;
		this.stateTime += 0.05f;
		int num2;
		switch (this.state)
		{
		case EntityVulture.State.Attack:
			this.battleDuration += 0.05f;
			break;
		case EntityVulture.State.AttackReposition:
			if (sqrMagnitude < 2.25f || this.stateTime >= this.stateMaxTime)
			{
				this.SetState(EntityVulture.State.Attack);
				this.motion *= -0.2f;
				this.motion.y = 0f;
			}
			break;
		case EntityVulture.State.AttackStop:
			this.ClearTarget();
			this.SetState(EntityVulture.State.WanderStart);
			break;
		case EntityVulture.State.Home:
			if (sqrMagnitude < 4f || this.stateTime > 30f)
			{
				this.SetState(EntityVulture.State.WanderStart);
			}
			else
			{
				num2 = this.homeSeekDelay - 1;
				this.homeSeekDelay = num2;
				if (num2 <= 0)
				{
					this.homeSeekDelay = 40;
					int num3 = 10;
					if (this.stateTime > 20f)
					{
						num3 = -20;
					}
					int maximumHomeDistance = base.getMaximumHomeDistance();
					Vector3 vector = RandomPositionGenerator.CalcTowards(this, num3, 30, maximumHomeDistance / 2, base.getHomePosition().position.ToVector3());
					if (!vector.Equals(Vector3.zero))
					{
						this.waypoint = vector;
						this.AdjustWaypoint();
					}
				}
			}
			break;
		case EntityVulture.State.Stun:
		{
			Animator componentInChildren = this.ModelTransform.GetComponentInChildren<Animator>();
			if (flag)
			{
				this.motion = this.rand.RandomOnUnitSphere * -0.075f;
				this.motion.y = this.motion.y + -0.060000002f;
				if (componentInChildren)
				{
					componentInChildren.enabled = false;
				}
				return;
			}
			if (componentInChildren)
			{
				componentInChildren.enabled = true;
			}
			this.SetState(EntityVulture.State.WanderStart);
			break;
		}
		case EntityVulture.State.WanderStart:
			this.homeCheckDelay = 60;
			if (!base.isWithinHomeDistanceCurrentPosition())
			{
				this.StartHome(base.getHomePosition().position.ToVector3());
			}
			else
			{
				this.SetState(EntityVulture.State.Wander);
				this.isCircling = !this.IsSleeper && this.rand.RandomFloat < 0.4f;
				float num4 = this.position.y;
				RaycastHit raycastHit;
				if (Physics.Raycast(this.position - Origin.position, Vector3.down, ref raycastHit, 999f, 65536))
				{
					float num5 = this.rand.RandomRange(this.wanderHeightRange.x, this.wanderHeightRange.y);
					if (this.IsSleeper)
					{
						num5 *= 0.4f;
					}
					num4 += -raycastHit.distance + num5;
				}
				else
				{
					this.isCircling = false;
				}
				bool flag2 = false;
				EntityPlayer entityPlayer3 = null;
				if (!this.isBattleFatigued)
				{
					entityPlayer3 = this.world.GetClosestPlayerSeen(this, 80f, 1f);
					if (entityPlayer3 && base.GetDistanceSq(entityPlayer3) > 400f)
					{
						flag2 = true;
					}
				}
				if (this.isCircling)
				{
					this.wanderChangeDelay = 120;
					Vector3 right = base.transform.right;
					right.y = 0f;
					this.circleReverseScale = 1f;
					if (this.rand.RandomFloat < 0.5f)
					{
						this.circleReverseScale = -1f;
						right.x = -right.x;
						right.z = -right.z;
					}
					this.circleCenter = this.position + right * (3f + this.rand.RandomFloat * 7f);
					this.circleCenter.y = num4;
					if (flag2)
					{
						this.circleCenter.x = this.circleCenter.x * 0.6f + entityPlayer3.position.x * 0.4f;
						this.circleCenter.z = this.circleCenter.z * 0.6f + entityPlayer3.position.z * 0.4f;
					}
				}
				else
				{
					this.wanderChangeDelay = 400;
					this.waypoint = this.position;
					this.waypoint.x = this.waypoint.x + (this.rand.RandomFloat * 16f - 8f);
					this.waypoint.y = num4;
					this.waypoint.z = this.waypoint.z + (this.rand.RandomFloat * 16f - 8f);
					if (flag2)
					{
						this.waypoint.x = this.waypoint.x * 0.6f + entityPlayer3.position.x * 0.4f;
						this.waypoint.z = this.waypoint.z * 0.6f + entityPlayer3.position.z * 0.4f;
					}
					this.AdjustWaypoint();
				}
			}
			break;
		case EntityVulture.State.Wander:
			if (this.isBattleFatigued)
			{
				this.battleDuration -= 0.05f;
				if (this.battleDuration <= 0f)
				{
					this.isBattleFatigued = false;
				}
			}
			num2 = this.wanderChangeDelay - 1;
			this.wanderChangeDelay = num2;
			if (num2 <= 0)
			{
				this.SetState(EntityVulture.State.WanderStart);
			}
			if (this.isCircling)
			{
				Vector3 vector2 = this.circleCenter - this.position;
				float x = vector2.x;
				vector2.x = -vector2.z * this.circleReverseScale;
				vector2.z = x * this.circleReverseScale;
				vector2.y = 0f;
				this.waypoint = this.position + vector2;
			}
			else if (sqrMagnitude < 1f)
			{
				this.SetState(EntityVulture.State.WanderStart);
			}
			num2 = this.targetSwitchDelay - 1;
			this.targetSwitchDelay = num2;
			if (num2 <= 0)
			{
				this.targetSwitchDelay = 40;
				if (this.IsSleeper || this.rand.RandomFloat >= 0.5f)
				{
					EntityPlayer entityPlayer4 = this.FindTarget();
					if (entityPlayer4)
					{
						base.SetAttackTarget(entityPlayer4, 1200);
					}
				}
			}
			break;
		}
		if (this.state != EntityVulture.State.Home)
		{
			num2 = this.homeCheckDelay - 1;
			this.homeCheckDelay = num2;
			if (num2 <= 0)
			{
				this.homeCheckDelay = 60;
				if (!base.isWithinHomeDistanceCurrentPosition())
				{
					this.SetState(EntityVulture.State.AttackStop);
				}
			}
		}
		num2 = this.moveUpdateDelay - 1;
		this.moveUpdateDelay = num2;
		if (num2 <= 0)
		{
			this.moveUpdateDelay = 4 + this.rand.RandomRange(5);
			if (this.currentTarget && this.state == EntityVulture.State.Attack)
			{
				this.waypoint = this.currentTarget.getHeadPosition();
				this.waypoint.y = this.waypoint.y + -0.1f;
				if (this.currentTarget.AttachedToEntity)
				{
					this.waypoint += this.currentTarget.GetVelocityPerSecond() * 0.3f;
				}
				else
				{
					this.waypoint += this.currentTarget.GetVelocityPerSecond() * 0.1f;
				}
				Vector3 vector3 = this.waypoint - this.position;
				vector3.y = 0f;
				vector3.Normalize();
				this.waypoint += vector3 * -0.6f;
			}
			float num6;
			if (!this.IsCourseTraversable(this.waypoint, out num6))
			{
				this.waypoint.y = this.waypoint.y + 2f;
				if (this.state == EntityVulture.State.Attack)
				{
					if (this.rand.RandomFloat < 0.1f)
					{
						this.StartAttackReposition();
					}
				}
				else if (this.state != EntityVulture.State.Home && this.state != EntityVulture.State.AttackReposition)
				{
					this.SetState(EntityVulture.State.WanderStart);
				}
			}
		}
		Vector3 vector4 = this.waypoint - this.position;
		float magnitude = vector4.magnitude;
		Vector3 vector5 = vector4 * (1f / magnitude);
		this.glidingPercent = 0f;
		if (vector5.y > 0.57f)
		{
			this.accel = 0.35f;
		}
		else if (vector5.y < -0.34f)
		{
			this.accel = 0.95f;
			this.glidingPercent = 1f;
		}
		else
		{
			this.accel = 0.55f;
			if (this.state == EntityVulture.State.Home || this.state == EntityVulture.State.Wander)
			{
				this.accel = 0.8f;
				if (this.isCircling)
				{
					this.glidingPercent = 1f;
				}
			}
		}
		if (this.attackDelay > 0)
		{
			this.glidingPercent = 0f;
		}
		if (this.currentTarget && this.currentTarget.AttachedToEntity && !this.ignoreTargetAttached)
		{
			if (this.IsBloodMoon && this.accel > 0.5f)
			{
				this.accel = 2.5f;
			}
			this.accel *= this.moveSpeedAggro;
		}
		else
		{
			this.accel *= this.moveSpeed;
		}
		this.motion = this.motion * 0.9f + vector5 * (this.accel * 0.1f);
		if (this.emodel.avatarController)
		{
			this.glidingCurrentPercent = Mathf.MoveTowards(this.glidingCurrentPercent, this.glidingPercent, 0.060000002f);
			this.emodel.avatarController.UpdateFloat("Gliding", this.glidingCurrentPercent, true);
		}
		if (this.attackDelay > 0)
		{
			this.attackDelay--;
		}
		if (this.attack2Delay > 0)
		{
			this.attack2Delay--;
		}
		float num7 = Mathf.Atan2(this.motion.x * this.motionReverseScale, this.motion.z * this.motionReverseScale) * 57.29578f;
		if (this.currentTarget)
		{
			num2 = this.targetSwitchDelay - 1;
			this.targetSwitchDelay = num2;
			if (num2 <= 0)
			{
				this.targetSwitchDelay = 60;
				if (this.state != EntityVulture.State.AttackStop)
				{
					EntityPlayer entityPlayer5 = this.FindTarget();
					if (entityPlayer5 && entityPlayer5 != this.attackTarget)
					{
						base.SetAttackTarget(entityPlayer5, 400);
					}
				}
				float num8 = (this.currentTarget.AttachedToEntity ? 0.1f : 0.25f);
				if (this.state != EntityVulture.State.AttackReposition && this.rand.RandomFloat < num8)
				{
					this.StartAttackReposition();
				}
			}
		}
		if (this.currentTarget)
		{
			Vector3 vector6 = this.currentTarget.getHeadPosition();
			vector6 += this.currentTarget.GetVelocityPerSecond() * 0.1f;
			Vector3 vector7 = vector6 - this.position;
			float sqrMagnitude2 = vector7.sqrMagnitude;
			if ((sqrMagnitude2 > 6400f && !this.IsBloodMoon) || this.currentTarget.IsDead())
			{
				this.SetState(EntityVulture.State.AttackStop);
			}
			else if (this.state != EntityVulture.State.AttackReposition)
			{
				if (sqrMagnitude2 < 4f)
				{
					num7 = Mathf.Atan2(vector7.x, vector7.z) * 57.29578f;
				}
				if (this.attackDelay <= 0 && !this.isAttack2On)
				{
					if (sqrMagnitude2 < 0.80999994f && this.position.y >= this.currentTarget.position.y && this.position.y < vector6.y + 0.1f)
					{
						this.AttackAndAdjust(false);
					}
					else if (this.checkBlockedDelay > 0)
					{
						this.checkBlockedDelay--;
					}
					else
					{
						this.checkBlockedDelay = 6;
						Vector3 normalized = vector7.normalized;
						Ray ray;
						ray..ctor(this.position + new Vector3(0f, 0.22f, 0f) - normalized * 0.13f, normalized);
						if (Voxel.Raycast(this.world, ray, 0.83f, 1082195968, 128, 0.13f))
						{
							this.AttackAndAdjust(true);
						}
					}
				}
				bool flag3 = false;
				ItemActionVomit.ItemActionDataVomit itemActionDataVomit = this.inventory.holdingItemData.actionData[1] as ItemActionVomit.ItemActionDataVomit;
				if (itemActionDataVomit != null && this.attack2Delay <= 0 && sqrMagnitude2 >= 9f)
				{
					float range = ((ItemActionRanged)this.inventory.holdingItem.Actions[1]).GetRange(itemActionDataVomit);
					if (sqrMagnitude2 < range * range && Utils.FastAbs(Utils.DeltaAngle(num7, this.rotation.y)) < 20f && Utils.FastAbs(Vector3.SignedAngle(vector7, base.transform.forward, Vector3.right)) < 25f)
					{
						flag3 = true;
					}
				}
				if (!this.isAttack2On && flag3)
				{
					this.isAttack2On = true;
					itemActionDataVomit.muzzle = this.emodel.GetHeadTransform();
					itemActionDataVomit.numWarningsPlayed = 999;
				}
				if (this.isAttack2On)
				{
					if (!flag3)
					{
						this.isAttack2On = false;
					}
					else
					{
						this.motion *= 0.7f;
						base.SetLookPosition(vector6);
						this.UseHoldingItem(1, false);
						if (!itemActionDataVomit.isActive)
						{
							this.isAttack2On = false;
						}
					}
					if (!this.isAttack2On)
					{
						if (itemActionDataVomit.numVomits > 0)
						{
							this.StartAttackReposition();
						}
						this.UseHoldingItem(1, true);
						this.attack2Delay = 60;
						base.SetLookPosition(Vector3.zero);
					}
				}
			}
		}
		float magnitude2 = this.motion.magnitude;
		if (magnitude2 < 0.02f)
		{
			this.motion *= 1f / magnitude2 * 0.02f;
		}
		base.SeekYaw(num7, 0f, 20f);
		this.aiManager.UpdateDebugName();
	}

	public override string MakeDebugNameInfo()
	{
		return string.Format("\n{0} {1}\nWaypoint {2}\nTarget {3}, AtkDelay {4}, BtlTime {5}\nSpeed {6}, Motion {7}, Accel {8}", new object[]
		{
			this.state.ToStringCached<EntityVulture.State>(),
			this.stateTime.ToCultureInvariantString("0.00"),
			this.waypoint.ToCultureInvariantString(),
			this.currentTarget ? this.currentTarget.name : "",
			this.attackDelay,
			this.battleDuration.ToCultureInvariantString("0.00"),
			this.motion.magnitude.ToCultureInvariantString("0.000"),
			this.motion.ToCultureInvariantString("0.000"),
			this.accel.ToCultureInvariantString("0.000")
		});
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetState(EntityVulture.State newState)
	{
		this.state = newState;
		this.stateTime = 0f;
		this.motionReverseScale = 1f;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AdjustWaypoint()
	{
		int num = 255;
		Vector3i vector3i = new Vector3i(this.waypoint);
		while (!this.world.GetBlock(vector3i).isair && --num >= 0)
		{
			this.waypoint.y = this.waypoint.y + 1f;
			vector3i.y++;
		}
		this.waypoint.y = Mathf.Min(this.waypoint.y, 250f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void StartAttackReposition()
	{
		if (!this.IsBloodMoon && this.battleDuration >= this.battleFatigueSeconds)
		{
			this.ClearTarget();
			this.battleDuration = this.rand.RandomRange(80f, 180f);
			this.isBattleFatigued = true;
			this.SetState(EntityVulture.State.Wander);
			return;
		}
		this.SetState(EntityVulture.State.AttackReposition);
		this.stateMaxTime = this.rand.RandomRange(0.8f, 5f);
		this.attackCount = 0;
		this.waypoint = this.position;
		this.waypoint.x = this.waypoint.x + (this.rand.RandomFloat * 8f - 4f);
		this.waypoint.y = this.waypoint.y + (this.rand.RandomFloat * 4f + 3f);
		this.waypoint.z = this.waypoint.z + (this.rand.RandomFloat * 8f - 4f);
		this.moveUpdateDelay = 0;
		this.motion = -this.motion;
		if (this.rand.RandomFloat < 0.5f)
		{
			this.motionReverseScale = -1f;
			this.motion.y = 0.2f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void StartHome(Vector3 _homePos)
	{
		this.SetState(EntityVulture.State.Home);
		this.homeSeekDelay = 0;
		this.waypoint = _homePos;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ClearTarget()
	{
		base.SetAttackTarget(null, 0);
		base.SetRevengeTarget(null);
		this.currentTarget = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayer FindTarget()
	{
		EntityPlayer entityPlayer;
		if (this.IsBloodMoon)
		{
			entityPlayer = this.world.GetClosestPlayerSeen(this, -1f, 0f);
			if (!entityPlayer)
			{
				entityPlayer = this.world.GetClosestPlayer(this, -1f, false);
			}
			return entityPlayer;
		}
		float num = 26f;
		entityPlayer = this.world.GetClosestPlayerSeen(this, 80f, num);
		if (!entityPlayer || entityPlayer.inWaterPercent >= 0.6f)
		{
			entityPlayer = this.noisePlayer;
		}
		if (entityPlayer)
		{
			if (this.isBattleFatigued)
			{
				return null;
			}
			float num2 = (float)entityPlayer.Health / entityPlayer.Stats.Health.ModifiedMax;
			if (this.IsSleeper || num2 <= this.targetAttackHealthPercent)
			{
				return entityPlayer;
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void fallHitGround(float _v, Vector3 _fallMotion)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDetailedHeadBodyColliders()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isRadiationSensitive()
	{
		return false;
	}

	public override float GetEyeHeight()
	{
		return 0.3f;
	}

	public override Vector3 GetLookVector()
	{
		if (this.lookAtPosition.Equals(Vector3.zero))
		{
			return base.GetLookVector();
		}
		return this.lookAtPosition - this.getHeadPosition();
	}

	public override bool CanDamageEntity(int _sourceEntityId)
	{
		Entity entity = this.world.GetEntity(_sourceEntityId);
		return !entity || entity.entityClass != this.entityClass;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void AttackAndAdjust(bool isBlock)
	{
		if (this.UseHoldingItem(0, false))
		{
			this.UseHoldingItem(0, true);
			this.attackDelay = 18;
			this.isCircling = false;
			if (this.currentTarget.AttachedToEntity)
			{
				this.motion *= 0.7f;
			}
			else
			{
				this.motion *= 0.6f;
			}
			int num = this.attackCount + 1;
			this.attackCount = num;
			if (num >= 5 || this.rand.RandomFloat < 0.25f)
			{
				this.StartAttackReposition();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool IsCourseTraversable(Vector3 _pos, out float _distance)
	{
		float num = _pos.x - this.position.x;
		float num2 = _pos.y - this.position.y;
		float num3 = _pos.z - this.position.z;
		_distance = Mathf.Sqrt(num * num + num2 * num2 + num3 * num3);
		if (_distance < 1.5f)
		{
			return true;
		}
		num /= _distance;
		num2 /= _distance;
		num3 /= _distance;
		Bounds boundingBox = this.boundingBox;
		this.collBB.Clear();
		int num4 = 1;
		while ((float)num4 < _distance - 1f)
		{
			boundingBox.center += new Vector3(num, num2, num3);
			this.world.GetCollidingBounds(this, boundingBox, this.collBB);
			if (this.collBB.Count > 0)
			{
				return false;
			}
			num4++;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cFlyingMinimumSpeed = 0.02f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cTargetDistanceClose = 0.9f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cTargetDistanceMax = 80f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cTargetAttackOffsetY = -0.1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cVomitMinRange = 3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cAttackDelay = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int cCollisionMask = 1082195968;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cBattleFatigueMin = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cBattleFatigueMax = 60f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cBattleFatigueCooldownMin = 80f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const float cBattleFatigueCooldownMax = 180f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int moveUpdateDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float motionReverseScale = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 waypoint;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isCircling;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 circleCenter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float circleReverseScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float glidingCurrentPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float glidingPercent;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float accel;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Vector2 wanderHeightRange = new Vector2(10f, 30f);

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EntityAlive currentTarget;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public float targetAttackHealthPercent = 0.8f;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool ignoreTargetAttached;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int targetSwitchDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int homeCheckDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int homeSeekDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int wanderChangeDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int checkBlockedDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float battleDuration;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float battleFatigueSeconds;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isBattleFatigued;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int attackDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int attackCount;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public int attack2Delay;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public bool isAttack2On;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public EAISetNearestEntityAsTargetSorter sorter;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public static List<Entity> list = new List<Entity>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public EntityVulture.State state;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float stateMaxTime;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public List<Bounds> collBB = new List<Bounds>();

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum State
	{
		Attack,
		AttackReposition,
		AttackStop,
		Home,
		Stun,
		WanderStart,
		Wander
	}
}
