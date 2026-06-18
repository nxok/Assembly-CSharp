using System;
using System.Collections.Generic;
using Audio;
using GameEvent.GameEventHelpers;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class EntityHomerunGoal : Entity
{
	public override bool CanCollideWithBlocks()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isEntityStatic()
	{
		return true;
	}

	public override bool CanBePushed()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void Update()
	{
		base.Update();
		World world = GameManager.Instance.World;
		if (world == null)
		{
			this.ReadyForDelete = true;
			return;
		}
		if (this.Owner == null)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				world.RemoveEntity(this.entityId, EnumRemoveEntityReason.Killed);
			}
			return;
		}
		this.TimeRemaining -= Time.deltaTime;
		if (this.TimeRemaining <= 0f)
		{
			this.ReadyForDelete = true;
			return;
		}
		if (this.IsMoving)
		{
			switch (this.direction)
			{
			case EntityHomerunGoal.Direction.YPositive:
				this.SetPosition(this.StartPosition + Vector3.up * Mathf.PingPong(Time.time, 2f) * 2f, true);
				break;
			case EntityHomerunGoal.Direction.XPositive:
				this.SetPosition(this.StartPosition + Vector3.right * Mathf.PingPong(Time.time, 2f) * 2f, true);
				break;
			case EntityHomerunGoal.Direction.XNegative:
				this.SetPosition(this.StartPosition + Vector3.left * Mathf.PingPong(Time.time, 2f) * 2f, true);
				break;
			case EntityHomerunGoal.Direction.ZPositive:
				this.SetPosition(this.StartPosition + Vector3.forward * Mathf.PingPong(Time.time, 2f) * 2f, true);
				break;
			case EntityHomerunGoal.Direction.ZNegative:
				this.SetPosition(this.StartPosition + Vector3.back * Mathf.PingPong(Time.time, 2f) * 2f, true);
				break;
			}
		}
		if (Vector3.Distance(this.position, this.Owner.Player.position) > 50f)
		{
			this.ReadyForDelete = true;
			return;
		}
		List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(this.position, Vector3.one * this.Size));
		for (int i = 0; i < entitiesInBounds.Count; i++)
		{
			EntityAlive entityAlive = entitiesInBounds[i] as EntityAlive;
			if (entityAlive != null && entityAlive != null && entityAlive.IsAlive() && !(entityAlive is EntityPlayer) && entityAlive.emodel != null && entityAlive.emodel.transform != null && entityAlive.emodel.IsRagdollActive)
			{
				float lightBrightness = world.GetLightBrightness(entityAlive.GetBlockPosition());
				world.GetGameManager().SpawnParticleEffectServer(new ParticleEffect("twitch_fireworks", entityAlive.position, lightBrightness, Color.white, null, null, false, 1f, ""), entityAlive.entityId, false, true);
				Manager.BroadcastPlayByLocalPlayer(entityAlive.position, "twitch_baseball_balloon_pop");
				entityAlive.DamageEntity(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Suicide), 99999, false, 1f);
				world.RemoveEntity(entityAlive.entityId, EnumRemoveEntityReason.Killed);
				if (!this.ReadyForDelete)
				{
					this.Owner.Score += this.ScoreAdded;
					this.ReadyForDelete = true;
				}
				this.Owner.AddScoreDisplay(this.position);
				return;
			}
		}
	}

	public override void CopyPropertiesFromEntityClass()
	{
		base.CopyPropertiesFromEntityClass();
		EntityClass entityClass = EntityClass.list[this.entityClass];
		entityClass.Properties.ParseInt("ScoreAdded", ref this.ScoreAdded);
		entityClass.Properties.ParseFloat("Size", ref this.Size);
		entityClass.Properties.ParseBool("IsMoving", ref this.IsMoving);
	}

	public HomerunData Owner;

	public bool ReadyForDelete;

	public int ScoreAdded = 1;

	public float Size = 2f;

	public Vector3 StartPosition;

	public float TimeRemaining = 20f;

	public bool IsMoving = true;

	public EntityHomerunGoal.Direction direction;

	public enum Direction
	{
		YPositive,
		XPositive,
		XNegative,
		ZPositive,
		ZNegative,
		Max
	}
}
