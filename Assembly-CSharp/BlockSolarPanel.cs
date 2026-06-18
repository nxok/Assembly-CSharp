using System;
using Audio;
using UnityEngine.Scripting;

[Preserve]
public class BlockSolarPanel : BlockPowerSource
{
	public override TileEntityPowerSource CreateTileEntity(Chunk chunk)
	{
		if (this.slotItem == null)
		{
			this.slotItem = ItemClass.GetItemClass(this.SlotItemName, false);
		}
		return new TileEntityPowerSource(chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.SolarPanel,
			SlotItem = this.slotItem
		};
	}

	public override bool CanPlaceBlockAt(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck = false)
	{
		if (!base.CanPlaceBlockAt(_world, _blockPos, _blockValue, _bOmitCollideCheck))
		{
			return false;
		}
		Vector3i vector3i = _blockPos + Vector3i.up;
		ChunkCluster chunkCache = _world.ChunkCache;
		return chunkCache == null || chunkCache.GetLight(vector3i, Chunk.LIGHT_TYPE.SUN) >= 15;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override string GetPowerSourceIcon()
	{
		return "electric_solar";
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		Manager.BroadcastStop(_blockPos.ToVector3(), this.runningSound);
	}

	public override void OnBlockUnloaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockUnloaded(_world, _blockPos, _blockValue);
		Manager.Stop(_blockPos.ToVector3(), this.runningSound);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string runningSound = "solarpanel_idle";
}
