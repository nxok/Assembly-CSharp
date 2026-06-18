using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockBarbed : BlockDamage
{
	public override bool OnEntityCollidedWithBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, Entity _targetEntity)
	{
		if (!base.OnEntityCollidedWithBlock(_world, _blockPos, _blockValue, _targetEntity))
		{
			return false;
		}
		byte meta = _blockValue.meta;
		_blockValue.meta = meta + 1;
		if (_blockValue.meta == 15)
		{
			this.DamageBlock(_world, _blockPos, _blockValue, _blockValue.Block.MaxDamage, (_targetEntity != null) ? _targetEntity.entityId : (-1), null, false, false);
		}
		else
		{
			_world.SetBlockRPC(_blockPos, _blockValue);
		}
		return true;
	}
}
