using System;
using System.Collections;
using System.Globalization;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPowerSource : Block
{
	public BlockPowerSource()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("SlotItem"))
		{
			this.SlotItemName = base.Properties.Values["SlotItem"];
		}
		else
		{
			this.SlotItemName = "carBattery";
		}
		if (base.Properties.Values.ContainsKey("OutputPerStack"))
		{
			this.OutputPerStack = Convert.ToInt32(base.Properties.Values["OutputPerStack"]);
		}
		else
		{
			this.OutputPerStack = 25;
		}
		if (base.Properties.Values.ContainsKey("TakeDelay"))
		{
			this.TakeDelay = StringParsers.ParseFloat(base.Properties.Values["TakeDelay"], 0, -1, NumberStyles.Any);
			return;
		}
		this.TakeDelay = 2f;
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false))
		{
			return "";
		}
		bool flag = _world.GetTileEntity(_blockPos) is TileEntityPowerSource;
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		if (flag)
		{
			string text = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
			string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
			return string.Format(Localization.Get("vendingMachineActivate", false, null), text, localizedBlockName);
		}
		return "";
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return _world.GetTileEntity(_blockPos) is TileEntityPowerSource;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		TileEntityPowerSource tileEntityPowerSource = _world.GetTileEntity(_blockPos) as TileEntityPowerSource;
		if (tileEntityPowerSource == null)
		{
			return BlockActivationCommand.Empty;
		}
		bool flag = _world.CanPlaceBlockAt(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		bool flag2 = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		this.cmds[0].icon = this.GetPowerSourceIcon();
		this.cmds[0].enabled = flag;
		this.cmds[1].enabled = flag;
		bool flag3 = false;
		if (tileEntityPowerSource != null)
		{
			flag3 = tileEntityPowerSource.IsPlayerPlaced;
		}
		this.cmds[2].enabled = flag2 && flag3 && this.TakeDelay > 0f;
		return this.cmds;
	}

	public override void OnBlockAdded(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		if (_blockValue.ischild)
		{
			return;
		}
		if (!(world.GetTileEntity(_blockPos) is TileEntityPowerSource))
		{
			TileEntityPowerSource tileEntityPowerSource = this.CreateTileEntity(_chunk);
			tileEntityPowerSource.SetDisableModifiedCheck(true);
			tileEntityPowerSource.localChunkPos = World.toBlock(_blockPos);
			if (_addedByPlayer != null)
			{
				tileEntityPowerSource.SetOwner(_addedByPlayer);
				tileEntityPowerSource.IsPlayerPlaced = true;
			}
			tileEntityPowerSource.InitializePowerData();
			_chunk.AddTileEntity(tileEntityPowerSource);
			tileEntityPowerSource.SetDisableModifiedCheck(false);
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				tileEntityPowerSource.SetModified();
			}
		}
		_chunk.AddEntityBlockStub(new BlockEntityData(_blockValue, _blockPos)
		{
			bNeedsTemperature = true
		});
	}

	public override void OnBlockRemoved(WorldBase world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnBlockRemoved(world, _chunk, _blockPos, _blockValue);
		TileEntityPowered tileEntityPowered = _chunk.GetTileEntity(World.toBlock(_blockPos)) as TileEntityPowered;
		if (tileEntityPowered != null)
		{
			if (!GameManager.IsDedicatedServer)
			{
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				if (primaryPlayer.inventory.holdingItem.Actions[1] is ItemActionConnectPower)
				{
					(primaryPlayer.inventory.holdingItem.Actions[1] as ItemActionConnectPower).CheckForWireRemoveNeeded(primaryPlayer, _blockPos);
				}
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				PowerManager.Instance.RemovePowerNode(tileEntityPowered.GetPowerItem());
			}
			if (tileEntityPowered.GetParent().y != -9999)
			{
				IPowered powered = world.GetTileEntity(tileEntityPowered.GetParent()) as IPowered;
				if (powered != null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
				{
					powered.SendWireData();
				}
			}
			tileEntityPowered.RemoveWires();
		}
		_chunk.RemoveTileEntityAt<TileEntityPowerSource>((World)world, World.toBlock(_blockPos));
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		TileEntityPowerSource tileEntityPowerSource = _world.GetTileEntity(_bvRef) as TileEntityPowerSource;
		if (tileEntityPowerSource != null)
		{
			tileEntityPowerSource.OnDestroy();
		}
		return Block.DestroyedResult.Downgrade;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_blockValue.ischild)
		{
			Vector3i parentPos = _blockValue.Block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			BlockValue block = _world.GetBlock(parentPos);
			return this.OnBlockActivated(_commandName, _world, parentPos, block, _player);
		}
		TileEntityPowerSource tileEntityPowerSource = _world.GetTileEntity(_blockPos) as TileEntityPowerSource;
		if (tileEntityPowerSource == null)
		{
			return false;
		}
		if (_commandName == "open")
		{
			_player.AimingGun = false;
			LockManager.Instance.LockRequestLocal(tileEntityPowerSource, null, 0);
			return true;
		}
		if (!(_commandName == "light"))
		{
			if (!(_commandName == "take"))
			{
				return false;
			}
			base.takeItemWithTimer(_blockPos, _blockValue, _player, 4f);
			return true;
		}
		else
		{
			if (tileEntityPowerSource.MaxOutput == 0)
			{
				Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
				GameManager.ShowTooltip(_player, Localization.Get("ttRequiresOneComponent", false, null), false, false, 0f);
				return false;
			}
			if (tileEntityPowerSource.PowerItemType == PowerItem.PowerItemTypes.Generator && tileEntityPowerSource.CurrentFuel == 0)
			{
				Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
				GameManager.ShowTooltip(_player, Localization.Get("ttGeneratorRequiresFuel", false, null), false, false, 0f);
				return false;
			}
			bool flag = (_blockValue.meta & 2) > 0;
			if (!flag && (false | _world.IsWater(_blockPos.x, _blockPos.y + 1, _blockPos.z) | _world.IsWater(_blockPos.x + 1, _blockPos.y, _blockPos.z) | _world.IsWater(_blockPos.x - 1, _blockPos.y, _blockPos.z) | _world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z + 1) | _world.IsWater(_blockPos.x, _blockPos.y, _blockPos.z - 1)))
			{
				Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
				GameManager.ShowTooltip(_player, Localization.Get("ttPowerSourceUnderwater", false, null), false, false, 0f);
				return false;
			}
			_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | ((!flag) ? 2 : 0));
			_world.SetBlockRPC(_blockPos, _blockValue);
			return true;
		}
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		TileEntityPowerSource tileEntityPowerSource = (TileEntityPowerSource)_world.GetTileEntity(_blockPos);
		if (tileEntityPowerSource == null)
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
			tileEntityPowerSource = this.CreateTileEntity(chunk);
			tileEntityPowerSource.localChunkPos = World.toBlock(_blockPos);
			tileEntityPowerSource.InitializePowerData();
			chunk.AddTileEntity(tileEntityPowerSource);
		}
		tileEntityPowerSource.BlockTransform = _ebcd.transform;
		GameManager.Instance.StartCoroutine(this.drawWiresLater(tileEntityPowerSource));
		if (tileEntityPowerSource.GetParent().y != -9999)
		{
			IPowered powered = _world.GetTileEntity(tileEntityPowerSource.GetParent()) as IPowered;
			if (powered != null)
			{
				GameManager.Instance.StartCoroutine(this.drawWiresLater(powered));
			}
		}
		this.updateState(_world, _blockPos, _blockValue, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator drawWiresLater(IPowered powered)
	{
		yield return new WaitForSeconds(0.5f);
		powered.DrawWires();
		yield break;
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			TileEntityPowerSource tileEntityPowerSource = _world.GetTileEntity(_blockPos) as TileEntityPowerSource;
			if (tileEntityPowerSource == null)
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
				tileEntityPowerSource = this.CreateTileEntity(chunk);
				tileEntityPowerSource.localChunkPos = World.toBlock(_blockPos);
				chunk.AddTileEntity(tileEntityPowerSource);
				string text = "TileEntityPowerSource not found (";
				Vector3i vector3i = _blockPos;
				Log.Out(text + vector3i.ToString() + ")");
			}
			PowerSource powerSource = tileEntityPowerSource.GetPowerItem() as PowerSource;
			if (powerSource == null)
			{
				powerSource = PowerManager.Instance.GetPowerItemByWorldPos(tileEntityPowerSource.ToWorldPos()) as PowerSource;
				if (powerSource == null)
				{
					powerSource = tileEntityPowerSource.CreatePowerItemForTileEntity((ushort)_newBlockValue.type) as PowerSource;
					tileEntityPowerSource.SetModified();
					powerSource.AddTileEntity(tileEntityPowerSource);
					string text2 = "PowerSource not found (";
					Vector3i vector3i = _blockPos;
					Log.Out(text2 + vector3i.ToString() + ")");
				}
			}
			bool flag = (_newBlockValue.meta & 2) > 0;
			powerSource.IsOn = flag;
		}
		this.updateState(_world, _blockPos, _newBlockValue, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateState(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bSwitchLight = false)
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
		if (_bSwitchLight)
		{
			flag = !flag;
			_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (flag ? 2 : 0));
			_world.SetBlockRPC(_blockPos, _blockValue);
		}
		Transform transform = blockEntity.transform.Find("Activated");
		if (transform != null)
		{
			transform.gameObject.SetActive(flag);
		}
		return true;
	}

	public override bool ActivateBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		if ((_blockValue.meta & 2) > 0 != isOn)
		{
			_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (isOn ? 2 : 0));
			_world.SetBlockRPC(_blockPos, _blockValue);
		}
		this.updateState(_world, _blockPos, _blockValue, false);
		return true;
	}

	public virtual TileEntityPowerSource CreateTileEntity(Chunk chunk)
	{
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual string GetPowerSourceIcon()
	{
		return "";
	}

	public string SlotItemName;

	public int OutputPerStack;

	[PublicizedFrom(EAccessModifier.Private)]
	public float TakeDelay = 2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public ItemClass slotItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("open", "hand", false, false, null),
		new BlockActivationCommand("light", "electric_switch", false, false, null),
		new BlockActivationCommand("take", "hand", false, false, null)
	};
}
