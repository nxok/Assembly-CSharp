using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockDamage : Block
{
	public BlockDamage()
	{
		this.IsCheckCollideWithEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey(Block.PropDamage))
		{
			int.TryParse(base.Properties.Values[Block.PropDamage], out this.damage);
		}
		else
		{
			Log.Error("Block " + base.GetBlockName() + " is a BlockDamage but does not specify a damage value");
			this.damage = 0;
		}
		if (base.Properties.Values.ContainsKey(BlockDamage.PropDamageReceived))
		{
			int.TryParse(base.Properties.Values[BlockDamage.PropDamageReceived], out this.damageReceived);
		}
		else
		{
			this.damageReceived = 0;
		}
		base.Properties.ParseEnum<EnumDamageTypes>(BlockDamage.PropDamageType, ref this.damageType);
	}

	public override void GetCollisionAABB(BlockValue _blockValue, int _x, int _y, int _z, float _distortedY, List<Bounds> _result)
	{
		base.GetCollisionAABB(_blockValue, _x, _y, _z, _distortedY, _result);
		Vector3 vector;
		vector..ctor(0.05f, 0.05f, 0.05f);
		for (int i = 0; i < _result.Count; i++)
		{
			Bounds bounds = _result[i];
			bounds.SetMinMax(bounds.min - vector, bounds.max + vector);
			_result[i] = bounds;
		}
	}

	public override IList<Bounds> GetClipBoundsList(BlockValue _blockValue, Vector3 _blockPos)
	{
		Block.staticList_IntersectRayWithBlockList.Clear();
		this.GetCollisionAABB(_blockValue, (int)_blockPos.x, (int)_blockPos.y, (int)_blockPos.z, 0f, Block.staticList_IntersectRayWithBlockList);
		return Block.staticList_IntersectRayWithBlockList;
	}

	public override bool OnEntityCollidedWithBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, Entity _targetEntity)
	{
		if (!(_targetEntity is EntityAlive))
		{
			return false;
		}
		EntityAlive entityAlive = (EntityAlive)_targetEntity;
		if (entityAlive.IsDead())
		{
			return false;
		}
		DamageSourceEntity damageSourceEntity = new DamageSourceEntity(EnumDamageSource.External, this.damageType, -1);
		damageSourceEntity.AttackingItem = _blockValue.ToItemValue();
		damageSourceEntity.BlockPosition = _blockPos;
		damageSourceEntity.SetIgnoreConsecutiveDamages(true);
		bool flag;
		if (entityAlive is EntityHuman)
		{
			damageSourceEntity.hitTransformName = entityAlive.emodel.GetHitTransform(BodyPrimaryHit.Torso).name;
			flag = _targetEntity.DamageEntity(damageSourceEntity, this.damage, false, 1f) > 0;
		}
		else
		{
			flag = _targetEntity.DamageEntity(damageSourceEntity, this.damage, false, 1f) > 0;
		}
		bool flag2 = false;
		int num = entityAlive.CalculateBlockDamage(this, this.damageReceived, out flag2);
		if (this.MovementFactor != 1f)
		{
			entityAlive.SetMotionMultiplier(EffectManager.GetValue(PassiveEffects.MovementFactorMultiplier, null, this.MovementFactor, entityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false));
		}
		if (flag && num > 0 && (World.SandboxUseTraderArea != TraderAreaStates.Default || !((World)_world).IsWithinTraderArea(_blockPos)))
		{
			this.DamageBlock(_world, _blockPos, _blockValue, num, (_targetEntity != null) ? _targetEntity.entityId : (-1), null, false, flag2);
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDamageReceived = "Damage_received";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDamageType = "DamageType";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int damage;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int damageReceived;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDamageTypes damageType = EnumDamageTypes.Piercing;
}
