using System;
using UnityEngine.Scripting;

[Preserve]
public class BlockRanged : BlockPowered
{
	public BlockRanged()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("AmmoItem"))
		{
			this.AmmoItemName = base.Properties.Values["AmmoItem"];
		}
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		bool flag2 = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		this.cmds[0].enabled = flag;
		this.cmds[1].enabled = flag2 && this.TakeDelay > 0f;
		return this.cmds;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false))
		{
			return "";
		}
		bool flag = _world.GetTileEntity(_blockPos) is TileEntityPoweredRangedTrap;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (!flag)
		{
			return "";
		}
		string text = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
		return string.Format(Localization.Get("vendingMachineActivate", false, null), text, localizedBlockName);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_commandName, _world, parentPos, block, _player);
		}
		TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap = _world.GetTileEntity(_blockPos) as TileEntityPoweredRangedTrap;
		if (tileEntityPoweredRangedTrap == null)
		{
			return false;
		}
		if (_commandName == "options")
		{
			_player.AimingGun = false;
			LockManager.Instance.LockRequestLocal(tileEntityPoweredRangedTrap, null, 0);
			return true;
		}
		if (!(_commandName == "take"))
		{
			return false;
		}
		base.takeItemWithTimer(_blockPos, _blockValue, _player, this.TakeDelay);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
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
		TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap = (TileEntityPoweredRangedTrap)_world.GetTileEntity(_blockPos);
		if (flag)
		{
			if (tileEntityPoweredRangedTrap != null)
			{
				PowerManager.Instance.ClientUpdateList.Add(tileEntityPoweredRangedTrap);
			}
		}
		else if (tileEntityPoweredRangedTrap != null)
		{
			PowerManager.Instance.ClientUpdateList.Remove(tileEntityPoweredRangedTrap);
		}
		AutoTurretController component = blockEntity.transform.gameObject.GetComponent<AutoTurretController>();
		if (component == null)
		{
			return false;
		}
		component.TileEntity = tileEntityPoweredRangedTrap;
		component.IsOn = flag;
		return true;
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
		this.updateState(_world, _blockPos, _newBlockValue);
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		AutoTurretController component = _ebcd.transform.gameObject.GetComponent<AutoTurretController>();
		if (component != null)
		{
			component.FireController.BlockPosition = _ebcd.pos.ToVector3();
			component.Init(base.Properties);
		}
		TileEntityPowered tileEntityPowered = _world.GetTileEntity(_blockPos) as TileEntityPowered;
		if (tileEntityPowered == null)
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
			tileEntityPowered = this.CreateTileEntity(chunk);
			tileEntityPowered.SetDisableModifiedCheck(true);
			tileEntityPowered.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowered.InitializePowerData();
			chunk.AddTileEntity(tileEntityPowered);
			tileEntityPowered.SetDisableModifiedCheck(false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				tileEntityPowered.SetModified();
			}
		}
		tileEntityPowered.BlockTransform = _ebcd.transform;
		tileEntityPowered.MarkWireDirty();
		if (tileEntityPowered.GetParent().y != -9999)
		{
			IPowered powered = _world.GetTileEntity(tileEntityPowered.GetParent()) as IPowered;
			if (powered != null)
			{
				powered.DrawWires();
			}
		}
	}

	public override bool ActivateBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _isOn, bool _isPowered)
	{
		byte b = (byte)(((int)_blockValue.meta & -3) | (_isOn ? 2 : 0));
		if (_blockValue.meta != b)
		{
			_blockValue.meta = b;
			_world.SetBlockRPC(_blockPos, _blockValue);
		}
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
		AutoTurretController component = blockEntity.transform.gameObject.GetComponent<AutoTurretController>();
		if (component == null)
		{
			return false;
		}
		TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap = (TileEntityPoweredRangedTrap)_world.GetTileEntity(_blockPos);
		component.TileEntity = tileEntityPoweredRangedTrap;
		component.IsOn = _isOn;
		return _isOn;
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!(_world.GetTileEntity(_blockPos) is TileEntityPoweredRangedTrap))
		{
			TileEntityPoweredRangedTrap tileEntityPoweredRangedTrap = (TileEntityPoweredRangedTrap)this.CreateTileEntity(_chunk);
			tileEntityPoweredRangedTrap.SetDisableModifiedCheck(true);
			tileEntityPoweredRangedTrap.localChunkPos = World.toBlock(_blockPos);
			if (_addedByPlayer != null)
			{
				tileEntityPoweredRangedTrap.SetOwner(_addedByPlayer);
			}
			tileEntityPoweredRangedTrap.InitializePowerData();
			_chunk.AddTileEntity(tileEntityPoweredRangedTrap);
			tileEntityPoweredRangedTrap.SetDisableModifiedCheck(false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				tileEntityPoweredRangedTrap.SetModified();
			}
		}
		BlockEntityData blockEntityData = new BlockEntityData(_blockValue, _blockPos)
		{
			bNeedsTemperature = true
		};
		_chunk.AddEntityBlockStub(blockEntityData);
	}

	public override void OnBlockLoaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockLoaded(_world, _blockPos, _blockValue);
		bool flag = (_blockValue.meta & 2) > 0;
		TileEntityPoweredBlock tileEntityPoweredBlock = _world.GetTileEntity(_blockPos) as TileEntityPoweredBlock;
		if (tileEntityPoweredBlock == null)
		{
			return;
		}
		if (flag)
		{
			PowerManager.Instance.ClientUpdateList.Add(tileEntityPoweredBlock);
			return;
		}
		PowerManager.Instance.ClientUpdateList.Remove(tileEntityPoweredBlock);
	}

	public override TileEntityPowered CreateTileEntity(Chunk _chunk)
	{
		return new TileEntityPoweredRangedTrap(_chunk)
		{
			PowerItemType = PowerItem.PowerItemTypes.RangedTrap
		};
	}

	public string AmmoItemName;

	[PublicizedFrom(EAccessModifier.Private)]
	public new readonly BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("options", "tool", true, false, null),
		new BlockActivationCommand("take", "hand", false, false, null)
	};
}
