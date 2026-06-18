using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockTNT : Block
{
	public override void Init()
	{
		base.Init();
		this.explosion = new ExplosionData(base.Properties, null);
	}

	public override int OnBlockDamaged(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _damagePoints, int _entityId, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (_world.GetGameRandom().RandomFloat <= (float)_damagePoints / (float)_blockValue.Block.MaxDamage)
		{
			this.explode(_world, _bvRef, _entityId, 0.1f);
		}
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache != null)
		{
			chunkCache.InvokeOnBlockDamagedDelegates(_bvRef, _blockValue, _damagePoints, _entityId);
		}
		return _blockValue.damage;
	}

	public override Block.DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, BlockValueRef _pos, BlockValue _blockValue, int _playerIdx)
	{
		base.OnBlockDestroyedByExplosion(_world, _pos, _blockValue, _playerIdx);
		this.explode(_world, _pos, _playerIdx, _world.GetGameRandom().RandomFloat * 0.5f + 0.3f);
		return Block.DestroyedResult.Remove;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void explode(WorldBase _world, BlockValueRef _bvRef, int _entityId, float _delay)
	{
		Vector3 vector = _bvRef.ToVector3Center(_world);
		_world.GetGameManager().ExplosionServer(vector, _bvRef, Quaternion.identity, this.explosion, _entityId, _delay, true, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ExplosionData explosion;
}
