using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class BlockGameEvent : Block
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
		base.Properties.ParseString(BlockGameEvent.PropOnActivateEvent, ref this.onActivateEvent);
		base.Properties.ParseString(BlockGameEvent.PropOnDamageEvent, ref this.onDamageEvent);
		base.Properties.ParseString(BlockGameEvent.PropOnTriggeredEvent, ref this.onTriggeredEvent);
		base.Properties.ParseString(BlockGameEvent.PropOnAddedEvent, ref this.onAddedEvent);
		base.Properties.ParseBool(BlockGameEvent.PropDestroyOnEvent, ref this.destroyOnEvent);
		base.Properties.ParseBool(BlockGameEvent.PropSendDamageUpdate, ref this.sendDamageUpdate);
		base.Properties.ParseFloat(BlockGameEvent.PropActivateTime, ref this.activateTime);
	}

	public override string GetActivationText(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		if (this.onActivateEvent == "" && !_world.IsEditor())
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
		GameEventManager.Current.HandleAction(this.onAddedEvent, null, null, false, _blockPos, "", "", false, true, "", null, null);
		if (this.destroyOnEvent)
		{
			this.DamageBlock(_world, _blockPos, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, -1, null, false, false);
		}
	}

	public override bool OnBlockActivated(string _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (_commandName == "activate")
		{
			if (this.onActivateEvent != "")
			{
				if (this.activateTime > 0f)
				{
					LocalPlayerUI playerUI = _player.PlayerUI;
					TimerEventData timerEventData = new TimerEventData();
					timerEventData.Data = new object[] { _blockPos, _player };
					timerEventData.CloseOnHit = true;
					timerEventData.FullTimeFinishEvent += this.EventData_Event;
					XUiC_Timer.OpenTimer(playerUI.xui, this.activateTime, timerEventData, -1f, "", true);
				}
				else
				{
					GameEventManager.Current.HandleAction(this.onActivateEvent, null, _player, false, _blockPos, "", "", false, true, "", null, null);
				}
				if (this.destroyOnEvent)
				{
					this.DamageBlock(_world, _blockPos, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, -1, null, false, false);
				}
			}
			return true;
		}
		if (!(_commandName == "trigger"))
		{
			return false;
		}
		XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _blockPos, false, true);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData timerData)
	{
		object[] array = (object[])timerData.Data;
		Vector3i vector3i = (Vector3i)array[0];
		EntityPlayerLocal entityPlayerLocal = array[1] as EntityPlayerLocal;
		GameEventManager.Current.HandleAction(this.onActivateEvent, null, entityPlayerLocal, false, vector3i, "", "", false, true, "", null, null);
	}

	public override bool HasBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		return true;
	}

	public override BlockActivationCommand[] GetBlockActivationCommands(WorldBase _world, BlockValue _blockValue, Vector3i _blockPos, EntityAlive _entityFocusing)
	{
		this.cmds[0].enabled = !_world.IsEditor() && this.onActivateEvent != "";
		this.cmds[1].enabled = _world.IsEditor() && !GameUtils.IsWorldEditor();
		return this.cmds;
	}

	public override int OnBlockDamaged(WorldBase _world, BlockValueRef _bvRef, BlockValue _blockValue, int _damagePoints, int _entityIdThatDamaged, ItemActionAttack.AttackHitInfo _attackHitInfo, bool _bUseHarvestTool, bool _bBypassMaxDamage, int _recDepth = 0)
	{
		if (this.onDamageEvent != "")
		{
			if (GameEventManager.Current.GetTargetType(this.onDamageEvent) != GameEventActionSequence.TargetTypes.Block)
			{
				Debug.LogError("Game Event Target Type must be set to 'Block' to be used in BlockGameEvent.");
			}
			else
			{
				Entity entity = _world.GetEntity(_entityIdThatDamaged);
				EntityVehicle entityVehicle = entity as EntityVehicle;
				if (entityVehicle != null)
				{
					entity = entityVehicle.GetFirstAttached();
				}
				GameEventManager.Current.HandleAction(this.onDamageEvent, null, entity as EntityPlayer, false, _bvRef, "", "", false, true, "", null, null);
				if (this.destroyOnEvent)
				{
					this.DamageBlock(_world, _bvRef, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, -1, null, false, false);
				}
			}
		}
		int num = base.OnBlockDamaged(_world, _bvRef, _blockValue, _damagePoints, _entityIdThatDamaged, _attackHitInfo, _bUseHarvestTool, _bBypassMaxDamage, _recDepth);
		if (num > 0 && this.sendDamageUpdate && GameManager.Instance.World.GetEntity(_entityIdThatDamaged) is EntityPlayer)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				GameEventManager.Current.SendBlockDamageUpdate(_bvRef);
				return num;
			}
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageGameEventResponse>().Setup(NetPackageGameEventResponse.ResponseTypes.BlockDamaged, _bvRef), false);
		}
		return num;
	}

	public override void OnTriggered(EntityPlayer _player, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		base.OnTriggered(_player, _world, _blockPos, _blockValue, _blockChanges, _triggeredBy);
		if (this.onTriggeredEvent != "")
		{
			if (GameEventManager.Current.GetTargetType(this.onTriggeredEvent) != GameEventActionSequence.TargetTypes.Block)
			{
				Debug.LogError("Game Event Target Type must be set to 'Block' to be used in BlockGameEvent.");
				return;
			}
			GameEventManager.Current.HandleAction(this.onTriggeredEvent, null, _player, false, _blockPos, "", "", false, true, "", null, null);
			if (this.destroyOnEvent)
			{
				this.DamageBlock(_world, _blockPos, _blockValue, _blockValue.Block.MaxDamage - _blockValue.damage, -1, null, false, false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOnActivateEvent = "ActivateEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOnDamageEvent = "DamageEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOnTriggeredEvent = "TriggeredEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropOnAddedEvent = "AddedEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropDestroyOnEvent = "DestroyOnEvent";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropSendDamageUpdate = "SendDamageUpdate";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropActivateTime = "ActivateTime";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string onActivateEvent = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string onDamageEvent = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string onTriggeredEvent = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string onAddedEvent = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool destroyOnEvent;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool sendDamageUpdate;

	[PublicizedFrom(EAccessModifier.Private)]
	public float activateTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public new BlockActivationCommand[] cmds = new BlockActivationCommand[]
	{
		new BlockActivationCommand("activate", "electric_switch", false, false, null),
		new BlockActivationCommand("trigger", "wrench", true, false, null)
	};
}
