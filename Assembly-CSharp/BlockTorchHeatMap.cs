using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockTorchHeatMap : BlockTorch
{
	public BlockTorchHeatMap()
	{
		this.IsRandomlyTick = true;
	}

	public override bool UpdateTick(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd)
	{
		base.UpdateTick(_world, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
		if (this.HeatMapStrength > 0f)
		{
			AIDirector aidirector = _world.GetAIDirector();
			if (aidirector != null)
			{
				float num = 1f;
				num *= 0.4f;
				aidirector.NotifyActivity(EnumAIDirectorChunkEvent.Torch, _blockPos, this.HeatMapStrength * num, 720f);
			}
		}
		return true;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return;
		}
		IChunk chunkSync = chunkCache.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
		if (chunkSync == null)
		{
			return;
		}
		BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return;
		}
		Transform transform = blockEntity.transform.FindInChildren("MainLight");
		if (transform)
		{
			LightLOD component = transform.GetComponent<LightLOD>();
			if (component)
			{
				component.SetBlockEntityData(blockEntity);
			}
		}
	}
}
