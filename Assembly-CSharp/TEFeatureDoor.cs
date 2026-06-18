using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureDoor : TEFeatureAbs, IFeatureTriggerCapability, IFeaturePhysicalCapabilities, IFeatureSavedInPrefab
{
	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		this.lockFeature = base.Parent.GetFeature<TEFeatureLockable>();
		this.lockpickFeature = base.Parent.GetFeature<TEFeatureLockPickable>();
		this.isDrawBridge = false;
		DynamicProperties props = _featureData.Props;
		if (base.Parent.TeData.CompositeProps.Values.ContainsKey(Block.PropMultiBlockDim))
		{
			Log.Error(string.Concat(new string[]
			{
				"Block with name ",
				base.Parent.TeData.Block.GetBlockName(),
				" requires a ",
				Block.PropMultiBlockDim,
				" property when including a Door feature"
			}));
		}
		props.ParseString(TEFeatureDoor.PropOpenSound, ref this.openSound);
		props.ParseString(TEFeatureDoor.PropCloseSound, ref this.closeSound);
		props.ParseString(TEFeatureDoor.PropLockedSound, ref this.lockedSound);
		props.ParseBool(TEFeatureDoor.PropIsDrawBridge, ref this.isDrawBridge);
		props.ParseFloat(TEFeatureDoor.PropAutoCloseTime, ref this.autoCloseTime);
		if (props.Values.ContainsKey("TintableMaterials"))
		{
			foreach (string text in props.Values["TintableMaterials"].Split(',', StringSplitOptions.None))
			{
				this.tintableMaterials.Add(text + " (Instance)");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		TEFeatureDoor tefeatureDoor;
		if (_other.TryGetSelfOrFeature(out tefeatureDoor))
		{
			this.isOpen = tefeatureDoor.isOpen;
			this.openSound = tefeatureDoor.openSound;
			this.closeSound = tefeatureDoor.closeSound;
			this.lockedSound = tefeatureDoor.lockedSound;
			this.isDrawBridge = tefeatureDoor.isDrawBridge;
		}
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		TEFeatureDoor feature = _other.GetFeature<TEFeatureDoor>();
		if (feature != null)
		{
			this.isOpen = feature.isOpen;
			this.openSound = feature.openSound;
			this.closeSound = feature.closeSound;
			this.lockedSound = feature.lockedSound;
			this.isDrawBridge = feature.isDrawBridge;
		}
	}

	public void HandleOpenCloseSound(Vector3i _blockPos)
	{
		if (!ThreadManager.IsMainThread())
		{
			return;
		}
		string text = (this.isOpen ? this.openSound : this.closeSound);
		Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, text);
	}

	public void UpdateAnimState(BlockEntityData _ebcd)
	{
		if (_ebcd != null && _ebcd.bHasTransform)
		{
			Animator[] componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>();
			if (componentsInChildren != null)
			{
				for (int i = componentsInChildren.Length - 1; i >= 0; i--)
				{
					Animator animator = componentsInChildren[i];
					animator.enabled = true;
					animator.SetBool(AnimatorDoorState.IsOpenHash, this.isOpen);
					animator.SetTrigger(AnimatorDoorState.OpenTriggerHash);
				}
			}
		}
	}

	public void ForceAnimationState(BlockEntityData _ebcd)
	{
		if (_ebcd != null && _ebcd.bHasTransform)
		{
			Animator[] componentsInChildren = _ebcd.transform.GetComponentsInChildren<Animator>();
			if (componentsInChildren != null)
			{
				for (int i = componentsInChildren.Length - 1; i >= 0; i--)
				{
					Animator animator = componentsInChildren[i];
					animator.enabled = true;
					animator.keepAnimatorStateOnDisable = true;
					animator.SetBool(AnimatorDoorState.IsOpenHash, this.isOpen);
					animator.Play(this.isOpen ? AnimatorDoorState.OpenHash : AnimatorDoorState.CloseHash, 0, 1f);
				}
			}
		}
	}

	public bool IsOpen()
	{
		return this.isOpen;
	}

	public void SetOpen(bool _open, bool _animate = false)
	{
		if (this.isOpen == _open)
		{
			return;
		}
		this.isOpen = _open;
		this.animateOnSync = _animate;
		if (this.isOpen && this.autoCloseTime > 0f)
		{
			this.autoCloseAtTickTime = GameTimer.Instance.ticks + (ulong)(this.autoCloseTime * 20f);
		}
		else
		{
			this.autoCloseAtTickTime = 0UL;
		}
		this.HandleDoorAnimation(_animate);
		if (_animate)
		{
			this.HandleOpenCloseSound(base.ToWorldPos());
		}
		GameManager.Instance.World.SetBlockRPC(base.ToWorldPos(), base.Parent.blockValue);
		base.SetModified();
	}

	public bool CanOpen(out bool canPickToOpen)
	{
		canPickToOpen = false;
		if (this.IsOpen())
		{
			return true;
		}
		TEFeatureLockPickable tefeatureLockPickable = this.lockpickFeature;
		canPickToOpen = tefeatureLockPickable != null && tefeatureLockPickable.NeedsLockpicking();
		return this.lockFeature == null || !this.lockFeature.IsLocked();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void HandleDoorAnimation(bool _animate)
	{
		BlockEntityData blockEntity = GameManager.Instance.World.ChunkCache.GetBlockEntity(base.Parent.ToWorldPos());
		if (_animate)
		{
			this.UpdateAnimState(blockEntity);
			return;
		}
		this.ForceAnimationState(blockEntity);
	}

	public override void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (_blockEntityData == null || !_blockEntityData.bHasTransform)
		{
			return;
		}
		this.ForceAnimationState(_blockEntityData);
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer || !this.isOpen || this.autoCloseTime <= 0f)
		{
			return;
		}
		if (this.autoCloseAtTickTime == 0UL || GameTimer.Instance.ticks < this.autoCloseAtTickTime)
		{
			return;
		}
		this.SetOpen(false, true);
	}

	public override void OnBlockValueChanged(Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		if (_newBlockValue.Block.shape is BlockShapeModelEntity && _oldBlockValue.type != _newBlockValue.type && !_newBlockValue.ischild)
		{
			if (VehicleManager.Instance != null)
			{
				VehicleManager.Instance.PhysicsWakeNear(_blockPos.ToVector3());
			}
			BlockEntityData blockEntity = GameManager.Instance.World.ChunkCache.GetBlockEntity(_blockPos);
			this.ForceAnimationState(blockEntity);
		}
	}

	public override void OnBlockReset(Vector3i _blockPos, BlockValue _blockValue)
	{
		this.SetOpen(false, false);
	}

	public void OnBlockTriggered(EntityPlayer _player, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		if (this.lockFeature != null && _triggeredBy != null && _triggeredBy.Unlock)
		{
			this.lockFeature.SetLocked(false);
		}
		this.SetOpen(!this.isOpen, true);
	}

	public bool IsMovementBlocked(Vector3i _blockPos, BlockValue _blockValue, BlockFace _face)
	{
		if (!this.isDrawBridge)
		{
			return !this.isOpen;
		}
		Block block = _blockValue.Block;
		if (!block.isMultiBlock || !_blockValue.ischild)
		{
			return true;
		}
		if (!this.isOpen)
		{
			return true;
		}
		Vector3i parentPos = block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
		return _blockPos.y == parentPos.y;
	}

	public bool IsMovementBlocked(Vector3i _blockPos, BlockValue _blockValue, BlockFaceFlag _sides)
	{
		if (!this.isDrawBridge)
		{
			return !this.isOpen;
		}
		Block block = _blockValue.Block;
		if (!block.isMultiBlock || !_blockValue.ischild)
		{
			return true;
		}
		if (!this.isOpen)
		{
			return true;
		}
		Vector3i parentPos = block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
		return _blockPos.y == parentPos.y;
	}

	public bool IsSeeThrough(Vector3i _blockPos, BlockValue _blockValue)
	{
		return this.isOpen;
	}

	public float GetStepHeight(Vector3i _blockPos, BlockValue _blockValue, BlockFace crossingFace)
	{
		if (!this.isDrawBridge)
		{
			return 0f;
		}
		if (!this.isOpen)
		{
			return 1f;
		}
		Block block = _blockValue.Block;
		if (block.isMultiBlock && _blockValue.ischild)
		{
			Vector3i parentPos = block.multiBlockPos.GetParentPos(_blockPos, _blockValue);
			if (_blockPos.y == parentPos.y)
			{
				return 1f;
			}
		}
		return 0f;
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		string text = Localization.Get("door", false, null);
		if (this.lockpickFeature != null && this.lockpickFeature.NeedsLockpicking())
		{
			return string.Format(Localization.Get("tooltipLocked", false, null), _activateHotkeyMarkup, text);
		}
		if (this.lockFeature != null && this.lockFeature.IsLocked())
		{
			return string.Format(Localization.Get("tooltipLocked", false, null), _activateHotkeyMarkup, text);
		}
		return string.Format(Localization.Get("tooltipUnlocked", false, null), _activateHotkeyMarkup, text);
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("open", "door", false, false, null), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("close", "door", false, false, null), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing))
		{
			return false;
		}
		if (!this.Equals(_module))
		{
			return true;
		}
		if (base.CommandIs(_commandName, "open"))
		{
			return !this.isOpen;
		}
		return !base.CommandIs(_commandName, "close") || this.isOpen;
	}

	public override void OnAdded(Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnAdded(_blockPos, _blockValue);
		if (this.isDrawBridge)
		{
			this.isOpen = true;
			base.SetModified();
		}
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (!base.CommandIs(_commandName, "close") && !base.CommandIs(_commandName, "open"))
		{
			return false;
		}
		bool flag = false;
		if (this.lockFeature != null && this.lockFeature.IsLocked() && !this.lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			flag = true;
		}
		if (flag)
		{
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, this.lockedSound);
			TraderArea traderAreaAt = ((World)_world).GetTraderAreaAt(_blockPos);
			if (traderAreaAt != null)
			{
				EntityTrader trader = traderAreaAt.GetTrader();
				GameManager.ShowTooltip(_player, trader.GetNextTimeMessage(), string.Empty, null, null, true, false, 0f);
			}
			return false;
		}
		this.SetOpen(!this.isOpen, true);
		_blockValue.Block.HandleTrigger(_player, (World)_world, _blockPos, _blockValue);
		return true;
	}

	public override void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.Read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			if (base.Parent.UseLocalVersioning())
			{
				_br.ReadUInt16();
			}
			else
			{
				base.Parent.GetLegacyForkVersion();
			}
		}
		bool flag = this.isOpen;
		this.isOpen = _br.ReadBoolean();
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency && this.autoCloseTime > 0f)
		{
			this.isOpen = false;
		}
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			bool flag2 = _br.ReadBoolean();
			if (flag != this.isOpen)
			{
				this.HandleDoorAnimation(flag2);
			}
		}
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
		_bw.Write(this.isOpen);
		if (_eStreamMode != TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(this.animateOnSync);
			this.animateOnSync = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropOpenSound = "OpenSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropCloseSound = "CloseSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropLockedSound = "LockedSound";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropIsDrawBridge = "IsDrawBridge";

	[PublicizedFrom(EAccessModifier.Private)]
	public static string PropAutoCloseTime = "AutoCloseTime";

	[PublicizedFrom(EAccessModifier.Protected)]
	public string openSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string closeSound;

	[PublicizedFrom(EAccessModifier.Protected)]
	public string lockedSound = "Misc/locked";

	[PublicizedFrom(EAccessModifier.Protected)]
	public HashSet<string> tintableMaterials = new HashSet<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockPickable lockpickFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isOpen;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isDrawBridge;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool animateOnSync;

	[PublicizedFrom(EAccessModifier.Private)]
	public float autoCloseTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong autoCloseAtTickTime;
}
