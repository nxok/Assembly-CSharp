using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPoweredTrap : BlockPowered
{
	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey(BlockPoweredTrap.PropDamage))
		{
			int.TryParse(base.Properties.Values[BlockPoweredTrap.PropDamage], out this.damage);
		}
		else
		{
			this.damage = 0;
		}
		if (base.Properties.Values.ContainsKey(BlockPoweredTrap.PropDamageReceived))
		{
			int.TryParse(base.Properties.Values[BlockPoweredTrap.PropDamageReceived], out this.damageReceived);
			return;
		}
		this.damageReceived = 0;
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

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateTrapState(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bSwitchTrap = false)
	{
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return false;
		}
		IChunk chunkSync = chunkCache.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
		if (chunkSync == null)
		{
			return false;
		}
		BlockEntityData blockEntity = chunkSync.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return false;
		}
		bool flag = (_blockValue.meta & 2) > 0;
		if (_bSwitchTrap)
		{
			flag = !flag;
			_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (flag ? 2 : 0));
			_world.SetBlockRPC(_blockPos, _blockValue);
		}
		this.ActivateTrap(blockEntity, flag);
		TileEntityPoweredMeleeTrap tileEntityPoweredMeleeTrap = (TileEntityPoweredMeleeTrap)_world.GetTileEntity(_blockPos);
		if (tileEntityPoweredMeleeTrap != null)
		{
			SpinningBladeTrapController component = blockEntity.transform.gameObject.GetComponent<SpinningBladeTrapController>();
			if (component != null)
			{
				component.BladeController.OwnerTE = tileEntityPoweredMeleeTrap;
			}
		}
		return true;
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
		if (_newBlockValue.ischild)
		{
			return;
		}
		this.updateTrapState(_world, _blockPos, _newBlockValue, false);
		((World)_world).ChunkCache.GetBlockEntity(_blockPos);
	}

	public static bool IsOn(byte _metadata)
	{
		return (_metadata & 2) > 0;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		SpinningBladeTrapController component = _ebcd.transform.gameObject.GetComponent<SpinningBladeTrapController>();
		if (component != null)
		{
			component.Cleanup();
		}
		this.ActivateTrap(_ebcd, false);
		TileEntityPoweredMeleeTrap tileEntityPoweredMeleeTrap = (TileEntityPoweredMeleeTrap)_world.GetTileEntity(_blockPos);
		if (tileEntityPoweredMeleeTrap == null)
		{
			ChunkCluster chunkCache = _world.ChunkCache;
			if (chunkCache == null)
			{
				return;
			}
			Chunk chunk = (Chunk)chunkCache.GetChunkFromWorldPos(_blockPos);
			if (chunk == null)
			{
				return;
			}
			tileEntityPoweredMeleeTrap = (TileEntityPoweredMeleeTrap)this.CreateTileEntity(chunk);
			tileEntityPoweredMeleeTrap.localChunkPos = World.toBlock(_blockPos);
			tileEntityPoweredMeleeTrap.InitializePowerData();
			chunk.AddTileEntity(tileEntityPoweredMeleeTrap);
		}
		if (tileEntityPoweredMeleeTrap != null)
		{
			bool flag = (_blockValue.meta & 2) > 0;
			_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (flag ? 2 : 0));
			this.updateTrapState(_world, _blockPos, _blockValue, false);
			if (component != null)
			{
				component.BladeController.OwnerTE = tileEntityPoweredMeleeTrap;
			}
		}
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!(world.GetTileEntity(_blockPos) is TileEntityPoweredMeleeTrap))
		{
			TileEntityPoweredMeleeTrap tileEntityPoweredMeleeTrap = this.CreateTileEntity(_chunk) as TileEntityPoweredMeleeTrap;
			tileEntityPoweredMeleeTrap.SetDisableModifiedCheck(true);
			tileEntityPoweredMeleeTrap.localChunkPos = World.toBlock(_blockPos);
			if (_addedByPlayer != null)
			{
				tileEntityPoweredMeleeTrap.SetOwner(_addedByPlayer);
			}
			tileEntityPoweredMeleeTrap.InitializePowerData();
			_chunk.AddTileEntity(tileEntityPoweredMeleeTrap);
			tileEntityPoweredMeleeTrap.SetDisableModifiedCheck(false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				tileEntityPoweredMeleeTrap.SetModified();
			}
		}
		_chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
		{
			bNeedsTemperature = true
		});
	}

	public override bool ActivateBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		if (_blockValue.ischild)
		{
			return false;
		}
		_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (isOn ? 2 : 0));
		_world.SetBlockRPC(_blockPos, _blockValue);
		this.updateTrapState(_world, _blockPos, _blockValue, false);
		return true;
	}

	public virtual bool ActivateTrap(BlockEntityData blockEntity, bool isOn)
	{
		return false;
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		return new TileEntityPoweredMeleeTrap(chunk);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public new static string PropDamage = "Damage";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDamageReceived = "Damage_received";

	[PublicizedFrom(EAccessModifier.Protected)]
	public int damage;

	[PublicizedFrom(EAccessModifier.Protected)]
	public int damageReceived;
}
