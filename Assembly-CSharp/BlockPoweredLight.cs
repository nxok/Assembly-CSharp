using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockPoweredLight : BlockPowered
{
	public BlockPoweredLight()
	{
		this.HasTileEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("RuntimeSwitch"))
		{
			this.isRuntimeSwitch = StringParsers.ParseBool(base.Properties.Values["RuntimeSwitch"], 0, -1, true);
		}
	}

	public override byte GetLightValue(BlockValue _blockValue)
	{
		if ((_blockValue.meta & 2) == 0)
		{
			return 0;
		}
		return base.GetLightValue(_blockValue);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (this.isRuntimeSwitch)
		{
			TileEntityPoweredBlock tileEntityPoweredBlock = (TileEntityPoweredBlock)_world.GetTileEntity(_blockPos);
			if (tileEntityPoweredBlock != null)
			{
				PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
				string text = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
				if (tileEntityPoweredBlock.IsToggled)
				{
					return string.Format(Localization.Get("useSwitchLightOff", false, null), text);
				}
				return string.Format(Localization.Get("useSwitchLightOn", false, null), text);
			}
		}
		else if (_world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false) && this.TakeDelay > 0f)
		{
			Block block = _blockValue.Block;
			return string.Format(Localization.Get("pickupPrompt", false, null), block.GetLocalizedBlockName());
		}
		return null;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "light"))
		{
			if (_commandName == "take")
			{
				base.takeItemWithTimer(_blockPos, _blockValue, _player, this.TakeDelay);
				return true;
			}
		}
		else
		{
			TileEntityPoweredBlock tileEntityPoweredBlock = (TileEntityPoweredBlock)_world.GetTileEntity(_blockPos);
			if (!_world.IsEditor() && tileEntityPoweredBlock != null)
			{
				tileEntityPoweredBlock.IsToggled = !tileEntityPoweredBlock.IsToggled;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool updateLightState(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool _bSwitchLight = false)
	{
		ChunkCluster chunkCache = _world.ChunkCache;
		if (chunkCache == null)
		{
			return false;
		}
		IChunk chunkFromWorldPos = chunkCache.GetChunkFromWorldPos(_blockPos);
		if (chunkFromWorldPos == null)
		{
			return false;
		}
		BlockEntityData blockEntity = chunkFromWorldPos.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return false;
		}
		bool flag = (_blockValue.meta & 2) > 0;
		TileEntityPoweredBlock tileEntityPoweredBlock = _world.GetTileEntity(_blockPos) as TileEntityPoweredBlock;
		if (tileEntityPoweredBlock != null)
		{
			flag = flag && tileEntityPoweredBlock.IsToggled;
		}
		if (_bSwitchLight)
		{
			flag = !flag;
			_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (flag ? 2 : 0));
			_world.SetBlockRPC(_blockPos, _blockValue);
		}
		Transform transform = blockEntity.transform.Find("MainLight");
		if (transform)
		{
			LightLOD component = transform.GetComponent<LightLOD>();
			if (component)
			{
				component.SwitchOnOff(flag, false);
				component.SetBlockEntityData(blockEntity);
			}
		}
		transform = blockEntity.transform.Find("Point light");
		if (transform != null)
		{
			LightLOD component2 = transform.GetComponent<LightLOD>();
			if (component2 != null)
			{
				component2.SwitchOnOff(flag, false);
				component2.SetBlockEntityData(blockEntity);
			}
		}
		return true;
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
		this.updateLightState(_world, _blockPos, _newBlockValue, false);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer(), false);
		this.cmds[0].enabled = _world.IsEditor() || this.isRuntimeSwitch;
		this.cmds[1].enabled = flag && this.TakeDelay > 0f;
		return this.cmds;
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		this.updateLightState(_world, _blockPos, _blockValue, false);
	}

	public override bool ActivateBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
	{
		_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (isOn ? 2 : 0));
		_world.SetBlockRPC(_blockPos, _blockValue);
		this.updateLightState(_world, _blockPos, _blockValue, false);
		return true;
	}

	public override TileEntityPowered CreateTileEntity(Chunk chunk)
	{
		PowerItem.PowerItemTypes powerItemTypes = PowerItem.PowerItemTypes.Consumer;
		if (this.isRuntimeSwitch)
		{
			powerItemTypes = PowerItem.PowerItemTypes.ConsumerToggle;
		}
		return new TileEntityPoweredBlock(chunk)
		{
			PowerItemType = powerItemTypes
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isRuntimeSwitch;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("light", "electric_switch", true, false, null),
		new BlockActivationCommand("take", "hand", false, false, null)
	};
}
