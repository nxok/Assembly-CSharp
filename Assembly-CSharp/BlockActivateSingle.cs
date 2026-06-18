using System;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockActivateSingle : Block
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
		base.Properties.ParseString(BlockActivateSingle.PropActivateSound, ref this.activateSound);
		base.Properties.ParseString(BlockActivateSingle.PropActivatedAnimBool, ref this.AnimActivatedBool);
		base.Properties.ParseString(BlockActivateSingle.PropActivatedAnimTrigger, ref this.AnimActivatedTrigger);
		base.Properties.ParseString(BlockActivateSingle.PropActivatedAnimState, ref this.AnimActivatedState);
	}

	public override void LateInit()
	{
		base.LateInit();
	}

	public override void OnBlockValueChanged(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		base.OnBlockValueChanged(_world, _chunk, _blockPos, _oldBlockValue, _newBlockValue);
		this.Refresh(_world, _chunk, _blockPos, _newBlockValue);
	}

	public override void OnBlockEntityTransformAfterActivated(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _blockValue, _ebcd);
		this.Refresh(_world, null, _blockPos, _blockValue);
	}

	public override void OnBlockRemoved(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		BlockUtilityNavIcon.RemoveNavObject(_blockPos);
		base.OnBlockRemoved(_world, _chunk, _blockPos, _blockValue);
	}

	public override void OnBlockUnloaded(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue)
	{
		BlockUtilityNavIcon.RemoveNavObject(_blockPos);
		base.OnBlockUnloaded(_world, _blockPos, _blockValue);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (!_world.IsEditor() && (_blockValue.meta & 2) != 0)
		{
			return "";
		}
		PlayerActionsLocal playerInput = ((EntityPlayerLocal)_entityFocusing).playerInput;
		string text = playerInput.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null) + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString(XUiUtils.EmptyBindingStyle.EmptyString, XUiUtils.DisplayStyle.Plain, null);
		string localizedBlockName = _blockValue.Block.GetLocalizedBlockName();
		return string.Format(Localization.Get("questBlockActivate", false, null), text, localizedBlockName);
	}

	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue, PlatformUserIdentifierAbs _addedByPlayer)
	{
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue, _addedByPlayer);
		_chunk.GetBlockTrigger(World.toBlock(_blockPos));
		this.Refresh(_world, _chunk, _blockPos, _blockValue);
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (!_world.IsEditor())
		{
			if ((_blockValue.meta & 2) != 0)
			{
				return false;
			}
			if (_player.prefab == null && SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return false;
			}
		}
		if (!(_commandName == "activate"))
		{
			if (_commandName == "trigger")
			{
				XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _blockPos, true, false);
			}
		}
		else if (!_world.IsEditor())
		{
			bool flag = (_blockValue.meta & 2) > 0;
			if (!flag)
			{
				base.HandleTrigger(_player, (World)_world, _blockPos, _blockValue);
				Manager.BroadcastPlay(_blockPos.ToVector3() + Vector3.one * 0.5f, this.activateSound, 0f);
				flag = !flag;
				_blockValue.meta = (byte)(((int)_blockValue.meta & -3) | (flag ? 2 : 0));
				_world.SetBlockRPC(_blockPos, _blockValue);
				this.Refresh(_world, null, _blockPos, _blockValue);
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
		this.cmds[0].enabled = !_world.IsEditor();
		this.cmds[1].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return this.cmds;
	}

	public override void Refresh(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue)
	{
		base.Refresh(_world, _chunk, _blockPos, _blockValue);
		bool flag = (_blockValue.meta & 2) > 0;
		BlockUtilityNavIcon.UpdateNavIcon(!flag, _blockPos);
		IChunk chunk = _chunk;
		if (chunk == null)
		{
			ChunkCluster chunkCache = _world.ChunkCache;
			if (chunkCache == null)
			{
				return;
			}
			chunk = chunkCache.GetChunkSync(World.toChunkXZ(_blockPos.x), World.toChunkY(_blockPos.y), World.toChunkXZ(_blockPos.z));
			if (chunk == null)
			{
				return;
			}
		}
		if (chunk == null)
		{
			return;
		}
		BlockEntityData blockEntity = chunk.GetBlockEntity(_blockPos);
		if (blockEntity == null || !blockEntity.bHasTransform)
		{
			return;
		}
		BlockSwitchSingleController component = blockEntity.transform.GetComponent<BlockSwitchSingleController>();
		if (component)
		{
			component.SetState(flag);
		}
		this.updateAnimState(blockEntity, flag);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateAnimState(BlockEntityData _ebcd, bool _bOpen)
	{
		if (this.AnimActivatedBool == "" || this.AnimActivatedTrigger == "")
		{
			return;
		}
		Animator[] componentsInChildren;
		if (_ebcd != null && _ebcd.bHasTransform && (componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>()) != null)
		{
			foreach (Animator animator in componentsInChildren)
			{
				animator.SetBool(this.AnimActivatedBool, _bOpen);
				animator.SetTrigger(this.AnimActivatedTrigger);
			}
		}
	}

	public override void ForceAnimationState(BlockValue _blockValue, BlockEntityData _ebcd)
	{
		if (this.AnimActivatedState == "" || this.AnimActivatedBool == "")
		{
			return;
		}
		Animator[] componentsInChildren;
		if (_ebcd != null && _ebcd.bHasTransform && (componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>(false)) != null)
		{
			bool flag = (_blockValue.meta & 2) > 0;
			foreach (Animator animator in componentsInChildren)
			{
				animator.SetBool(this.AnimActivatedBool, flag);
				if (flag)
				{
					animator.CrossFade(this.AnimActivatedState, 0f);
				}
				else
				{
					animator.CrossFade(this.AnimActivatedState, 0f);
				}
			}
		}
	}

	public const int cMetaOn = 2;

	[PublicizedFrom(EAccessModifier.Private)]
	public string activateSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string AnimActivatedBool = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string AnimActivatedTrigger = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string AnimActivatedState = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("activate", "electric_switch", true, false, null),
		new BlockActivationCommand("trigger", "wrench", true, false, null)
	};

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivateSound = "ActivateSound";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivatedAnimBool = "ActivatedAnimBool";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivatedAnimTrigger = "ActivatedAnimTrigger";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivatedAnimState = "ActivatedAnimState";
}
