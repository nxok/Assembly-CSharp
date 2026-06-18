using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlacementPineLeaves : BlockPlacement
{
	public override BlockPlacement.Result OnPlaceBlock(BlockPlacement.EnumPlacement _placement, BlockPlacement.EnumRotationMode _mode, int _localRot, WorldBase _world, BlockValue _blockValue, PropTransform _propTransform, HitInfoDetails _hitInfo, Vector3 _entityPos)
	{
		if (_mode != BlockPlacement.EnumRotationMode.Auto)
		{
			return base.OnPlaceBlock(_placement, _mode, _localRot, _world, _blockValue, _propTransform, _hitInfo, _entityPos);
		}
		BlockPlacement.Result result = new BlockPlacement.Result(_placement, _blockValue, _propTransform, _hitInfo);
		result.blockValue.rotation = 0;
		return result;
	}
}
