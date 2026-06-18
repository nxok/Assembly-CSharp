using System;
using UnityEngine;

public abstract class EntityEnemy : EntityAlive
{
	public override void Init(int _entityClass, EntityInstanceAssets _assets, EModelInstanceAssets _eModelAssets)
	{
		base.Init(_entityClass, _assets, _eModelAssets);
	}

	public override void PostInit()
	{
		base.PostInit();
		if (!this.isEntityRemote)
		{
			this.IsBloodMoon = this.world.aiDirector.BloodMoonComponent.BloodMoonActive;
		}
	}

	public override bool IsDrawMapIcon()
	{
		return true;
	}

	public override Vector3 GetMapIconScale()
	{
		return new Vector3(0.75f, 0.75f, 1f);
	}

	public override bool IsSavedToFile()
	{
		return (base.GetSpawnerSource() != EnumSpawnerSource.Dynamic || this.IsDead()) && base.IsSavedToFile();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool canDespawn()
	{
		return (!this.IsHordeZombie || this.world.GetPlayers().Count == 0) && base.canDespawn();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isRadiationSensitive()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isDetailedHeadBodyColliders()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override bool isGameMessageOnDeath()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnEntityTargeted(EntityAlive target)
	{
		base.OnEntityTargeted(target);
		if (!this.isEntityRemote && base.GetSpawnerSource() != EnumSpawnerSource.Dynamic && target is EntityPlayer)
		{
			this.world.aiDirector.NotifyIntentToAttack(this, target as EntityPlayer);
		}
	}

	public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale)
	{
		return base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public EntityEnemy()
	{
	}

	public bool IsHordeZombie;
}
