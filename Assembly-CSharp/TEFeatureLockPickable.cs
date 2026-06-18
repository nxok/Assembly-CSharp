using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Audio;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureLockPickable : TEFeatureAbs, IFeatureTriggerCapability
{
	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		DynamicProperties props = _featureData.Props;
		if (!props.Values.ContainsKey(TEFeatureLockPickable.PropLockPickItem))
		{
			Log.Error(string.Concat(new string[]
			{
				"Block with name ",
				base.Parent.TeData.Block.GetBlockName(),
				" does not have a ",
				TEFeatureLockPickable.PropLockPickItem,
				" property for LockPickable feature"
			}));
		}
		props.ParseString(TEFeatureLockPickable.PropLockPickItem, ref this.lockPickItem);
		props.ParseFloat(TEFeatureLockPickable.PropLockPickTime, ref this.lockPickTime);
		props.ParseFloat(TEFeatureLockPickable.PropLockPickBreakChance, ref this.lockPickBreakChance);
		props.ParseString(TEFeatureLockPickable.PropOnLockPickSuccessEvent, ref this.lockPickSuccessEvent);
		props.ParseString(TEFeatureLockPickable.PropOnLockPickFailedEvent, ref this.lockPickFailedEvent);
		if (props.Values.ContainsKey(TEFeatureLockPickable.PropLockpickDowngradeBlock))
		{
			string text = props.Values[TEFeatureLockPickable.PropLockpickDowngradeBlock];
			if (!string.IsNullOrEmpty(text))
			{
				this.lockpickDowngradeBlock = Block.GetBlockValue(text, false);
				if (this.lockpickDowngradeBlock.isair)
				{
					throw new Exception("Block with name '" + text + "' not found in block " + base.Parent.TeData.Block.GetBlockName());
				}
			}
		}
		else
		{
			this.lockpickDowngradeBlock = base.Parent.TeData.Block.DowngradeBlock;
		}
		if (this.lockPickTime > 0f)
		{
			this.unlockCompletion = 0f;
			return;
		}
		this.unlockCompletion = 1f;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			if (base.Parent.UseLocalVersioning())
			{
				_br.ReadUInt16();
				return;
			}
			base.Parent.GetLegacyForkVersion();
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
	}

	public bool NeedsLockpicking()
	{
		return this.unlockCompletion < 1f;
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		this.ShowUI(_success);
	}

	public void ShowUI(bool _lockGranted)
	{
		TEFeatureLockPickable.<>c__DisplayClass23_0 CS$<>8__locals1;
		CS$<>8__locals1.<>4__this = this;
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_lockGranted)
		{
			GameManager.ShowTooltip(uiforPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractItem", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			return;
		}
		CS$<>8__locals1.player = uiforPrimaryPlayer.entityPlayer;
		if (this.nthPercentile < 0f)
		{
			this.nthPercentile = CS$<>8__locals1.player.rand.RandomRange(0f, 0.999999f);
			this.lockPicksUsed = 0;
			this.unlockCompletion = 0f;
		}
		float num = -1f;
		float value = EffectManager.GetValue(PassiveEffects.LockPickTime, CS$<>8__locals1.player.inventory.holdingItemItemValue, this.lockPickTime, CS$<>8__locals1.player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		float num2 = value * (1f - this.unlockCompletion);
		float value2 = EffectManager.GetValue(PassiveEffects.LockPickBreakChance, CS$<>8__locals1.player.inventory.holdingItemItemValue, this.lockPickBreakChance, CS$<>8__locals1.player, null, default(FastTags<TagGroup.Global>), true, true, true, true, true, 1, true, false);
		int num3 = this.<ShowUI>g__CalculateLockpicksNeeded|23_0(1f - value2, this.nthPercentile, 15, ref CS$<>8__locals1) - this.lockPicksUsed;
		if (num3 > 1)
		{
			num = value - num2 + this.<ShowUI>g__RandomizeNextBreak|23_1(num3 - 1, ref CS$<>8__locals1) * num2;
		}
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.CloseEvent += this.EventData_CloseEvent;
		timerEventData.Data = CS$<>8__locals1.player;
		timerEventData.FullTimeFinishEvent += this.EventData_Event;
		timerEventData.AlternateTime = num;
		timerEventData.AlternateEvent += this.EventData_BreakEvent;
		XUiC_Timer.OpenTimer(uiforPrimaryPlayer.xui, value, timerEventData, num2, "", true);
		Manager.BroadcastPlayByLocalPlayer(base.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_CloseEvent(TimerEventData _timerData)
	{
		EntityPlayerLocal entityPlayerLocal = (EntityPlayerLocal)_timerData.Data;
		Manager.BroadcastPlayByLocalPlayer(base.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "Misc/locked");
		LockManager.Instance.UnlockRequestLocal();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_BreakEvent(TimerEventData _timerData)
	{
		EntityPlayerLocal entityPlayerLocal = (EntityPlayerLocal)_timerData.Data;
		Vector3i vector3i = base.ToWorldPos();
		ItemValue item = ItemClass.GetItem(this.lockPickItem, false);
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/locked");
		ItemStack itemStack = new ItemStack(item, 1);
		uiforPlayer.xui.PlayerInventory.RemoveItem(itemStack);
		GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttLockpickBroken", false, null), false, false, 0f);
		uiforPlayer.xui.CollectedItemList.RemoveItemStack(itemStack);
		this.unlockCompletion = Utils.FastMax(this.unlockCompletion, _timerData.Completion);
		this.lockPicksUsed++;
		if (this.lockPickFailedEvent != null)
		{
			GameEventManager.Current.HandleAction(this.lockPickFailedEvent, null, entityPlayerLocal, false, vector3i, "", "", false, true, "", null, null);
		}
		LockManager.Instance.UnlockRequestLocal();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void EventData_Event(TimerEventData _timerData)
	{
		EntityPlayerLocal entityPlayerLocal = (EntityPlayerLocal)_timerData.Data;
		Vector3i vector3i = base.ToWorldPos();
		LockManager.Instance.UnlockRequestLocal();
		this.unlockCompletion = 1f;
		this.lockPicksUsed++;
		this.DowngradeToUnlockedVariant(vector3i);
		Manager.BroadcastPlayByLocalPlayer(vector3i.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
		if (this.lockPickSuccessEvent != null)
		{
			GameEventManager.Current.HandleAction(this.lockPickSuccessEvent, null, entityPlayerLocal, false, vector3i, "", "", false, true, "", null, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DowngradeToUnlockedVariant(Vector3i _blockPos)
	{
		if (this.lockpickDowngradeBlock.isair)
		{
			return;
		}
		World world = GameManager.Instance.World;
		BlockValue block = world.GetBlock(_blockPos);
		BlockValue blockValue = this.lockpickDowngradeBlock;
		blockValue = BlockPlaceholderMap.Instance.Replace(blockValue, world.GetGameRandom(), _blockPos.x, _blockPos.z, false);
		blockValue.rotation = block.rotation;
		blockValue.meta = block.meta;
		world.SetBlockRPC(_blockPos, blockValue, blockValue.Block.Density);
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		if (this.unlockCompletion < 1f)
		{
			return string.Format(Localization.Get("tooltipLocked", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		return null;
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("pick", "unlock", false, false, null), TileEntityComposite.EBlockCommandOrder.First, base.FeatureData);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing))
		{
			return false;
		}
		if (!this.Equals(_module))
		{
			return this.unlockCompletion == 1f || base.Parent.LocalPlayerIsOwner;
		}
		return !base.CommandIs(_commandName, "pick") || !base.Parent.LocalPlayerIsOwner;
	}

	public void OnBlockTriggered(EntityPlayer _player, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		if (_triggeredBy == null)
		{
			return;
		}
		if (_triggeredBy.Unlock)
		{
			this.DowngradeToUnlockedVariant(_blockPos);
		}
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (!base.CommandIs(_commandName, "pick"))
		{
			return false;
		}
		LocalPlayerUI playerUI = _player.PlayerUI;
		ItemValue item = ItemClass.GetItem(this.lockPickItem, false);
		if (playerUI.xui.PlayerInventory.GetItemCount(item) == 0)
		{
			playerUI.xui.CollectedItemList.AddItemStack(new ItemStack(item, 0), true);
			GameManager.ShowTooltip(_player, Localization.Get("ttLockpickMissing", false, null), false, false, 0f);
			return true;
		}
		_player.AimingGun = false;
		LockManager.Instance.LockRequestLocal(this, null, 0);
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public int <ShowUI>g__CalculateLockpicksNeeded|23_0(float _lpSuccess, float _nthPercentile, int _maxTries = 15, ref TEFeatureLockPickable.<>c__DisplayClass23_0 A_4)
	{
		if (_lpSuccess <= 0f || _lpSuccess > 1f || _maxTries <= 0)
		{
			return 1;
		}
		float num = 1f - _lpSuccess;
		return Utils.FastMin(Utils.Fastfloor(Math.Log(1.0 - (double)this.nthPercentile) / Math.Log((double)num)) + 1, _maxTries);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public float <ShowUI>g__RandomizeNextBreak|23_1(int _breaksRemaining, ref TEFeatureLockPickable.<>c__DisplayClass23_0 A_2)
	{
		if (_breaksRemaining <= 0)
		{
			return -1f;
		}
		float num = 1f;
		for (int i = 0; i < _breaksRemaining; i++)
		{
			float num2 = A_2.player.rand.RandomRange(0f, 1f);
			if (num2 < num)
			{
				num = num2;
			}
		}
		return Utils.FastClamp(num, 0.05f, 0.95f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropLockPickTime = "LockPickTime";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropLockPickItem = "LockPickItem";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropLockPickBreakChance = "LockPickBreakChance";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropOnLockPickSuccessEvent = "LockPickSuccessEvent";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropOnLockPickFailedEvent = "LockPickFailedEvent";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropLockpickDowngradeBlock = "LockPickDowngradeBlock";

	[PublicizedFrom(EAccessModifier.Private)]
	public string lockPickItem;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lockPickTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lockPickBreakChance;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lockPickSuccessEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lockPickFailedEvent;

	[PublicizedFrom(EAccessModifier.Private)]
	public BlockValue lockpickDowngradeBlock;

	[PublicizedFrom(EAccessModifier.Private)]
	public float unlockCompletion = -1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lockPicksUsed = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public float nthPercentile = -1f;
}
