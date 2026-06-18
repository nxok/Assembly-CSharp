using System;
using System.Collections.Generic;
using Audio;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureLockable : TEFeatureAbs, ILockable, IFeatureTriggerCapability
{
	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		TEFeatureLockable tefeatureLockable;
		if (_other.TryGetSelfOrFeature(out tefeatureLockable))
		{
			this.locked = tefeatureLockable.IsLocked();
			this.allowedUserIds.AddRange(tefeatureLockable.GetUsers());
			this.passwordHash = tefeatureLockable.GetPasswordHash();
		}
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		TEFeatureLockable feature = _other.GetFeature<TEFeatureLockable>();
		if (feature != null)
		{
			this.locked = feature.IsLocked();
			this.allowedUserIds.AddRange(feature.GetUsers());
			this.passwordHash = feature.GetPasswordHash();
		}
	}

	public bool IsLocked()
	{
		return this.locked;
	}

	public void SetLocked(bool _isLocked)
	{
		this.locked = _isLocked;
		base.SetModified();
	}

	public void OnBlockTriggered(EntityPlayer _player, Vector3i _blockPos, BlockValue _blockValue, List<BlockChangeInfo> _blockChanges, BlockTrigger _triggeredBy)
	{
		if (_triggeredBy == null)
		{
			return;
		}
		if (_triggeredBy.Unlock)
		{
			this.SetLocked(false);
		}
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return base.Parent.Owner;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		base.Parent.SetOwner(_userIdentifier);
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		TEFeatureDoor tefeatureDoor;
		return this.IsOwner(_userIdentifier) || this.allowedUserIds.Contains(_userIdentifier) || (_userIdentifier.Equals(PlatformManager.InternalLocalUserIdentifier) && (!base.Parent.TryGetSelfOrFeature(out tefeatureDoor) && GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled)));
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return this.allowedUserIds;
	}

	public bool LocalPlayerIsOwner()
	{
		return this.IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(base.Parent.Owner);
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(this.passwordHash);
	}

	public bool SetPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (this.IsOwner(_userIdentifier) && _passwordHash != this.passwordHash)
		{
			this.passwordHash = _passwordHash;
			this.allowedUserIds.Clear();
			base.SetModified();
			return true;
		}
		return false;
	}

	public bool CheckPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (this.IsOwner(_userIdentifier) || !this.HasPassword())
		{
			return true;
		}
		if (_passwordHash == this.passwordHash)
		{
			this.allowedUserIds.Add(_userIdentifier);
			base.SetModified();
			return true;
		}
		return false;
	}

	public string GetPasswordHash()
	{
		return this.passwordHash;
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		if (!this.IsLocked())
		{
			return string.Format(Localization.Get("tooltipUnlocked", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (!this.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return string.Format(Localization.Get("tooltipJammed", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		return string.Format(Localization.Get("tooltipLocked", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("lock", "lock", false, false, null), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("unlock", "unlock", false, false, null), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("keypad", "keypad", false, false, null), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
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
		PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
		bool flag = !this.LocalPlayerIsOwner() && GameManager.Instance.persistentPlayers.Allies.IsAlly(base.Parent.Owner, internalLocalUserIdentifier);
		if (base.CommandIs(_commandName, "lock"))
		{
			return !this.IsLocked() && (this.LocalPlayerIsOwner() || flag || _world.IsEditor());
		}
		if (base.CommandIs(_commandName, "unlock"))
		{
			return this.IsLocked() && (this.LocalPlayerIsOwner() || _world.IsEditor());
		}
		return !base.CommandIs(_commandName, "keypad") || (!this.IsUserAllowed(internalLocalUserIdentifier) && this.HasPassword() && this.IsLocked()) || this.LocalPlayerIsOwner();
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (base.CommandIs(_commandName, "lock"))
		{
			this.SetLocked(true);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/locking");
			GameManager.ShowTooltip(_player, "containerLocked", false, false, 0f);
			return true;
		}
		if (base.CommandIs(_commandName, "unlock"))
		{
			this.SetLocked(false);
			Manager.BroadcastPlayByLocalPlayer(_blockPos.ToVector3() + Vector3.one * 0.5f, "Misc/unlocking");
			GameManager.ShowTooltip(_player, "containerUnlocked", false, false, 0f);
			return true;
		}
		if (base.CommandIs(_commandName, "keypad"))
		{
			LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(_player);
			if (uiforPlayer != null)
			{
				XUiC_KeypadWindow.Open(uiforPlayer, this, null, null);
			}
			return true;
		}
		return false;
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
		this.locked = _br.ReadBoolean();
		int num = _br.ReadInt32();
		this.allowedUserIds.Clear();
		for (int i = 0; i < num; i++)
		{
			this.allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br, false, false));
		}
		this.passwordHash = _br.ReadString();
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
		_bw.Write(this.locked);
		_bw.Write(this.allowedUserIds.Count);
		for (int i = 0; i < this.allowedUserIds.Count; i++)
		{
			this.allowedUserIds[i].ToStream(_bw, false);
		}
		_bw.Write(this.passwordHash);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool locked;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PlatformUserIdentifierAbs> allowedUserIds = new List<PlatformUserIdentifierAbs>();

	[PublicizedFrom(EAccessModifier.Private)]
	public string passwordHash = "";
}
