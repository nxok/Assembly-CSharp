using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockMine : Block
{
	public ExplosionData Explosion
	{
		get
		{
			return this.explosion;
		}
	}

	public override void Init()
	{
		base.Init();
		this.explosion = new ExplosionData(base.Properties, null);
		this.BaseEntityDamage = this.explosion.EntityDamage;
		if (base.Properties.Values.ContainsKey(BlockMine.PropTriggerDelay))
		{
			this.TriggerDelay = StringParsers.ParseFloat(base.Properties.Values[BlockMine.PropTriggerDelay], 0, -1, NumberStyles.Any);
		}
		if (base.Properties.Values.ContainsKey(BlockMine.PropTriggerSound))
		{
			this.TriggerSound = base.Properties.Values[BlockMine.PropTriggerSound];
		}
		base.Properties.ParseBool(BlockMine.PropNoImmunity, ref this.NoImmunity);
	}

	public override void OnEntityWalking(WorldBase _world, int _x, int _y, int _z, BlockValue _blockValue, Entity entity)
	{
		if (this.NoImmunity || EffectManager.GetValue(PassiveEffects.LandMineImmunity, null, 0f, entity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false) == 0f)
		{
			if (entity as EntityPlayer != null)
			{
				if ((entity as EntityPlayer).IsSpectator)
				{
					return;
				}
				GameManager.Instance.PlaySoundAtPositionServer(new Vector3((float)_x, (float)_y, (float)_z), this.TriggerSound, 1, 5, entity.entityId, 1f);
			}
			float num = this.TriggerDelay;
			if (entity as EntityAlive != null)
			{
				num = EffectManager.GetValue(PassiveEffects.LandMineTriggerDelay, null, this.TriggerDelay, entity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
			this.explosion.EntityDamage = EffectManager.GetValue(PassiveEffects.TrapIncomingDamage, null, this.BaseEntityDamage, entity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			_world.GetWBT().AddScheduledBlockUpdate(new Vector3i(_x, _y, _z), this.blockID, (ulong)(num * 20f));
		}
	}

	public override int OnBlockDamaged(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (_damagePoints >= 0)
		{
			float num = (float)Utils.FastClamp(_damagePoints, 1, _blockValue.Block.MaxDamage - 1);
			if (_world.GetGameRandom().RandomFloat <= num / (float)_blockValue.Block.MaxDamage)
			{
				this.explode(_world, _bvRef, _entityIdThatDamaged);
			}
		}
		else
		{
			base.OnBlockDamaged(_world, _bvRef, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
		}
		return _blockValue.damage;
	}

	public override Block.DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _playerIdx)
	{
		if (_world.GetGameRandom().RandomFloat < 0.33f)
		{
			this.explode(_world, _bvRef, _playerIdx);
			return Block.DestroyedResult.Remove;
		}
		return Block.DestroyedResult.Keep;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void explode(WorldBase _world, BlockValueRef _bvRef, int _entityId)
	{
		Vector3 vector = _bvRef.ToVector3Center(_world);
		_world.GetGameManager().ExplosionServer(vector, World.worldToBlockPos(vector), Quaternion.identity, this.explosion, -1, 0.1f, true, null);
	}

	public override bool IsMovementBlocked(IBlockAccess _world, Vector3i _blockPos, BlockValue blockDef, BlockFace face)
	{
		return false;
	}

	public override bool UpdateTick(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		this.explode(_world, _blockPos, -1);
		return true;
	}

	public void TriggerMine(Entity _entity, WorldBase _world, Vector3i _blockPos, bool useTrigger)
	{
		if (useTrigger)
		{
			float num = this.TriggerDelay;
			EntityAlive entityAlive = _entity as EntityAlive;
			if (entityAlive != null)
			{
				GameManager.Instance.PlaySoundAtPositionServer(_blockPos, this.TriggerSound, 1, 5, 1f);
				num = EffectManager.GetValue(PassiveEffects.LandMineTriggerDelay, null, this.TriggerDelay, entityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
				this.explosion.EntityDamage = EffectManager.GetValue(PassiveEffects.TrapIncomingDamage, null, this.BaseEntityDamage, entityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
			else
			{
				this.explosion.EntityDamage = EffectManager.GetValue(PassiveEffects.TrapIncomingDamage, null, this.BaseEntityDamage, null, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
			}
			_world.GetWBT().AddScheduledBlockUpdate(_blockPos, this.blockID, (ulong)(num * 20f));
			return;
		}
		this.explode(_world, _blockPos, -1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTriggerDelay = "TriggerDelay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTriggerSound = "TriggerSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropNoImmunity = "NoImmunity";

	[PublicizedFrom(EAccessModifier.Protected)]
	public ExplosionData explosion;

	[PublicizedFrom(EAccessModifier.Private)]
	public float TriggerDelay = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string TriggerSound = "landmine_trigger";

	[PublicizedFrom(EAccessModifier.Private)]
	public float BaseEntityDamage;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool NoImmunity;
}
