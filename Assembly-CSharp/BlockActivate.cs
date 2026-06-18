using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockActivate : Block
{
	public override bool AllowBlockTriggers
	{
		get
		{
			return true;
		}
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey("ActivateSound"))
		{
			this.activateSound = base.Properties.Values["ActivateSound"];
		}
	}

	public override void LateInit()
	{
		base.LateInit();
		if (base.Properties.Values.ContainsKey(BlockActivate.PropBlockChangeTo))
		{
			this.blockChangeTo = Block.GetBlockValue(base.Properties.Values[BlockActivate.PropBlockChangeTo], false);
			this.useChangeTo = true;
		}
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string text = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
		return string.Format(Localization.Get("questBlockActivate", false, null), text, localizedBlockName);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!(_commandName == "activate"))
		{
			if (_commandName == "trigger")
			{
				XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _blockPos, true, false);
			}
		}
		else if (!_world.IsEditor())
		{
			base.HandleTrigger(_player, (World)_world, _blockPos, _blockValue);
			Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound, 0f);
			if (this.useChangeTo)
			{
				this.blockChangeTo.rotation = _blockValue.rotation;
			}
			return true;
		}
		return false;
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		((Chunk)_world.ChunkCache.GetChunkSync(World.toChunkXZ(_blockPos.x), _blockPos.y, World.toChunkXZ(_blockPos.z))).GetBlockTrigger(World.toBlock(_blockPos));
		this.cmds[0].enabled = true;
		this.cmds[1].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return this.cmds;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string activateSound;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue blockChangeTo = BlockValue.Air;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool useChangeTo;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("activate", "electric_switch", true, false, null),
		new BlockActivationCommand("trigger", "wrench", true, false, null)
	};

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropBlockChangeTo = "BlockChangeTo";
}
