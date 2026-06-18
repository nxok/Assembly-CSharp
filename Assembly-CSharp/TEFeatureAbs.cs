using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public abstract class TEFeatureAbs : ITileEntityFeature, ITileEntity, ILockTarget
{
	public TileEntityFeatureData FeatureData
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public TileEntityComposite Parent
	{
		get; [PublicizedFrom(EAccessModifier.Protected)]
		set;
	}

	public virtual void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.Init (on " + base.GetType().Name + ")");
		}
		this.Parent = _parent;
		this.FeatureData = _featureData;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public abstract void CopyFromInternal(TileEntityComposite _other);

	public void CopyFrom(TileEntityComposite _other)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.CopyFrom (on " + base.GetType().Name + ")");
		}
		this.CopyFromInternal(_other);
	}

	public virtual void OnRemove(World _world)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnRemove (on " + base.GetType().Name + ")");
		}
	}

	public virtual void OnLoad()
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnLoad (on " + base.GetType().Name + ")");
		}
	}

	public virtual void OnUnload(World _world)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnUnload (on " + base.GetType().Name + ")");
		}
	}

	public virtual void OnDestroy()
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnDestroy (on " + base.GetType().Name + ")");
		}
	}

	public virtual void OnAdded(Vector3i _blockPos, BlockValue _blockValue)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnAdded (on " + base.GetType().Name + ")");
		}
	}

	public virtual void OnBlockValueChanged(Vector3i _blockPos, BlockValue _oldBlockValue, BlockValue _newBlockValue)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockValueChanged (on " + base.GetType().Name + ")");
		}
	}

	public virtual void OnBlockReset(Vector3i _blockPos, BlockValue _blockValue)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockReset (on " + base.GetType().Name + ")");
		}
	}

	public virtual void OnBlockStartsToFall(Vector3i _blockPos, BlockValue _blockValue)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockStartsToFall (on " + base.GetType().Name + ")");
		}
	}

	public virtual Block.DestroyedResult OnBlockDestroyedBy(Vector3i _blockPos, BlockValue _blockValue, int _entityId, bool _bUseHarvestTool)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockDestroyedBy (on " + base.GetType().Name + ")");
		}
		return Block.DestroyedResult.None;
	}

	public virtual Block.DestroyedResult OnBlockDestroyedByExplosion(Vector3i _blockPos, BlockValue _blockValue, int _playerThatStartedExpl)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockDestroyedByExplosion (on " + base.GetType().Name + ")");
		}
		return Block.DestroyedResult.None;
	}

	public virtual void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.SetBlockEntityData (on " + base.GetType().Name + ")");
		}
	}

	public virtual void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.UpgradeDowngradeFrom (on " + base.GetType().Name + ")");
		}
	}

	public virtual void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.ReplacedBy (on " + base.GetType().Name + ")");
		}
	}

	public virtual void Reset(FastTags<TagGroup.Global> _questTags)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.Reset (on " + base.GetType().Name + ")");
		}
	}

	public virtual void UpdateTick(World _world)
	{
	}

	public virtual string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		return null;
	}

	public virtual void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.InitBlockActivationCommands (on " + base.GetType().Name + ")");
		}
		if (this is IFeatureTriggerCapability)
		{
			_addCallback(new BlockActivationCommand("trigger", "wrench", false, false, null), TileEntityComposite.EBlockCommandOrder.Last, this.FeatureData);
		}
	}

	public virtual bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		return !this.Equals(_module) || !this.CommandIs(_commandName, "trigger") || (_world.IsEditor() && this is IFeatureTriggerCapability);
	}

	public virtual bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.OnBlockActivated (on " + base.GetType().Name + "), command " + _commandName.ToString());
		}
		if (this.CommandIs(_commandName, "trigger"))
		{
			XUiC_TriggerProperties.Show(_player.PlayerUI.xui, _blockPos, false, true);
			return true;
		}
		return false;
	}

	public virtual void Read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.Read (on " + base.GetType().Name + ")");
		}
	}

	public virtual void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		if (TEFeatureAbs.DebugLogCTE)
		{
			Log.Warning("TEFeatureAbs.Write (on " + base.GetType().Name + ")");
		}
	}

	public virtual LockTargetType LockTargetType
	{
		get
		{
			return LockTargetType.TEFeature;
		}
	}

	public virtual bool IsSharedLock(ushort _channel)
	{
		return false;
	}

	public virtual bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return true;
	}

	public virtual bool CanLockOnServer(int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		return true;
	}

	public virtual void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
	}

	public virtual void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
	}

	public virtual void OnUnlockedServer(int _unlockingPlayerID, ushort _channel)
	{
	}

	public event XUiEvent_TileEntityDestroyed Destroyed
	{
		add
		{
			this.Parent.Destroyed += value;
		}
		remove
		{
			this.Parent.Destroyed -= value;
		}
	}

	public List<ITileEntityChangedListener> listeners
	{
		get
		{
			return this.Parent.listeners;
		}
	}

	public void SetUserAccessing(bool _bUserAccessing)
	{
		this.Parent.SetUserAccessing(_bUserAccessing);
	}

	public bool IsUserAccessing()
	{
		return this.Parent.IsUserAccessing();
	}

	public void SetModified()
	{
		this.Parent.SetModified();
	}

	public Chunk GetChunk()
	{
		return this.Parent.GetChunk();
	}

	public Vector3i ToWorldPos()
	{
		return this.Parent.ToWorldPos();
	}

	public Vector3 ToWorldCenterPos()
	{
		return this.Parent.ToWorldCenterPos();
	}

	public BlockValue blockValue
	{
		get
		{
			return this.Parent.blockValue;
		}
	}

	public virtual bool IsRemoving
	{
		get
		{
			return this.Parent.IsRemoving;
		}
		set
		{
			this.Parent.IsRemoving = value;
		}
	}

	public LockTargetType GetLockTargetType()
	{
		return LockTargetType.TileEntity;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool CommandIs(ReadOnlySpan<char> _givenCommand, string _compareCommand)
	{
		return _givenCommand.Equals(_compareCommand, StringComparison.Ordinal);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public TEFeatureAbs()
	{
	}

	public static bool DebugLogCTE;
}
