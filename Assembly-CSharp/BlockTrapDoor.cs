using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockTrapDoor : Block
{
	public override bool AllowBlockTriggers
	{
		get
		{
			return true;
		}
	}

	public BlockTrapDoor()
	{
		this.IsCheckCollideWithEntity = true;
	}

	public override void Init()
	{
		base.Init();
		if (base.Properties.Values.ContainsKey(BlockTrapDoor.PropTriggerDelay))
		{
			this.TriggerDelay = StringParsers.ParseFloat(base.Properties.Values[BlockTrapDoor.PropTriggerDelay], 0, -1, NumberStyles.Any);
		}
		if (base.Properties.Values.ContainsKey(BlockTrapDoor.PropTriggerSound))
		{
			this.TriggerSound = base.Properties.Values[BlockTrapDoor.PropTriggerSound];
		}
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		this.cmds[0].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return this.cmds;
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _blockPos, true, true);
		return true;
	}

	public override bool OnEntityCollidedWithBlock(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, Entity _targetEntity)
	{
		if (_targetEntity as EntityAlive == null)
		{
			return false;
		}
		EntityAlive entityAlive = (EntityAlive)_targetEntity;
		if (entityAlive.IsDead())
		{
			return false;
		}
		if (_blockValue.meta == 1)
		{
			return false;
		}
		Vector3 vector = _blockPos.ToVector3() + new Vector3(0.5f, 0f, 0.5f);
		vector.y = 0f;
		Vector3 position = entityAlive.position;
		position.y = 0f;
		float num = Utils.FastAbs(vector.x - position.x);
		float num2 = Utils.FastAbs(vector.z - position.z);
		float num3 = ((_blockValue.Block != null && _blockValue.Block.isMultiBlock) ? ((float)_blockValue.Block.multiBlockPos.dim.x / 2f * 0.45f) : 0.45f);
		if (num > num3 || num2 > num3)
		{
			return false;
		}
		_blockValue.meta = 1;
		_world.SetBlockRPC(_blockPos, _blockValue);
		float value = EffectManager.GetValue(PassiveEffects.TrapDoorTriggerDelay, null, this.TriggerDelay, _targetEntity as EntityAlive, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		if (value > 0f)
		{
			GameManager.Instance.PlaySoundAtPositionServer(_blockPos.ToVector3(), this.TriggerSound, 1, 5, _targetEntity.entityId, 1f);
			GameManager.Instance.StartCoroutine(this.damageBlock(value, _world, _blockPos, _blockValue, _targetEntity as EntityPlayer, _targetEntity.entityId));
		}
		return true;
	}

	public override Block.DestroyedResult OnBlockDestroyedBy(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		EntityPlayer entityPlayer = ((World)_world).GetEntity(_entityId) as EntityPlayer;
		if (entityPlayer != null)
		{
			base.HandleTrigger(entityPlayer, (World)_world, _bvRef, _blockValue);
		}
		return base.OnBlockDestroyedBy(_world, _bvRef, _blockValue, _entityId, _bUseHarvestTool);
	}

	public override Block.DestroyedResult OnBlockDestroyedByExplosion(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _playerThatStartedExpl)
	{
		EntityPlayer entityPlayer = ((World)_world).GetEntity(_playerThatStartedExpl) as EntityPlayer;
		if (entityPlayer != null)
		{
			base.HandleTrigger(entityPlayer, (World)_world, _bvRef, _blockValue);
		}
		return base.OnBlockDestroyedByExplosion(_world, _bvRef, _blockValue, _playerThatStartedExpl);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator damageBlock(float time, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayer _player, int _entityId)
	{
		yield return new WaitForSeconds(time);
		if (_player != null)
		{
			base.HandleTrigger(_player, (World)_world, _blockPos, _blockValue);
		}
		this.DamageBlock(_world, _blockPos, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, _entityId, null, false, false);
		yield break;
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		this.DamageBlock(_world, _blockPos, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, -1, null, false, false);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTriggerDelay = "TriggerDelay";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropTriggerSound = "TriggerSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("trigger", "wrench", true, false, null)
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public float TriggerDelay = 0.6f;

	[PublicizedFrom(EAccessModifier.Private)]
	public string TriggerSound = "trapdoor_trigger";
}
