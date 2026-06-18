using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockPlant : Block
{
	public BlockPlant()
	{
		this.IsRandomlyTick = true;
	}

	public override bool CanPlaceBlockAt(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		return base.CanPlaceBlockAt(_world, _blockPos, _blockValue, _bOmitCollideCheck) && this.CanGrowOn(_world, _blockPos - Vector3i.up, _blockValue);
	}

	public virtual bool CanGrowOn(WorldBase _world, Vector3i _blockPos, BlockValue _blockValueOfPlant)
	{
		return GameManager.Instance.IsEditMode() || this.fertileLevel == 0 || _world.GetBlock(_blockPos).Block.blockMaterial.FertileLevel >= this.fertileLevel;
	}

	public override void OnNeighborBlockChange(WorldBase _world, Vector3i _myBlockPos, BlockValue _myBlockValue, Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
	{
		base.OnNeighborBlockChange(_world, _myBlockPos, _myBlockValue, _blockPosThatChanged, _newNeighborBlockValue, _oldNeighborBlockValue);
		if (!_myBlockValue.ischild)
		{
			this.CheckPlantAlive(_world, _myBlockPos, _myBlockValue);
		}
	}

	public override bool UpdateTick(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		this.CheckPlantAlive(_world, _blockPos, _blockValue);
		return false;
	}

	public virtual bool CheckPlantAlive(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		if (GameManager.Instance.IsEditMode())
		{
			return true;
		}
		if (!this.CanPlantStay(_world, _blockPos, _blockValue))
		{
			_world.SetBlockRPC(_blockPos, BlockValue.Air);
			return false;
		}
		return true;
	}

	public override bool CanPlantStay(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		return GameManager.Instance.IsEditMode() || ((this.lightLevelStay == 0 || _world.GetBlockLightValue(_blockPos) >= this.lightLevelStay || _world.GetBlockLightValue(_blockPos + Vector3i.up) >= this.lightLevelStay || _world.IsOpenSkyAbove(_blockPos.x, _blockPos.y, _blockPos.z)) && this.CanGrowOn(_world, _blockPos - Vector3i.up, _blockValue));
	}

	public override BlockValue OnBlockPlaced(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, GameRandom _rnd)
	{
		_blockValue.rotation = (byte)_rnd.RandomRange(4);
		return _blockValue;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public int lightLevelStay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int fertileLevel = 1;
}
