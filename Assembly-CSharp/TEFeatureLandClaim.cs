using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureLandClaim : TEFeatureAbs
{
	public bool ShowBounds
	{
		get
		{
			return this.showBounds;
		}
		set
		{
			this.showBounds = value;
			base.SetModified();
		}
	}

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		this.lockFeature = base.Parent.GetFeature<TEFeatureLockable>();
		this.lockpickFeature = base.Parent.GetFeature<TEFeatureLockPickable>();
		this.activePrompt = Localization.Get("activeBlockPrompt", false, null);
		this.inactivePrompt = Localization.Get("inactiveBlockPrompt", false, null);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		TEFeatureLandClaim tefeatureLandClaim;
		if (_other.TryGetSelfOrFeature(out tefeatureLandClaim))
		{
			this.showBounds = tefeatureLandClaim.showBounds;
		}
	}

	public override string GetActivationText(WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing, string _activateHotkeyMarkup, string _focusedTileEntityName)
	{
		base.GetActivationText(_world, _blockPos, _blockValue, _entityFocusing, _activateHotkeyMarkup, _focusedTileEntityName);
		if (this.lockpickFeature != null && this.lockpickFeature.NeedsLockpicking())
		{
			return string.Format(Localization.Get("tooltipLocked", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (this.lockFeature != null && this.lockFeature.IsLocked() && !this.lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return string.Format(Localization.Get("tooltipJammed", false, null), _activateHotkeyMarkup, _focusedTileEntityName);
		}
		if (this.IsPrimary())
		{
			string text = (base.Parent.LocalPlayerIsOwner ? ("\n" + Localization.Get("useWorkstation", false, null)) : "");
			return string.Format(this.activePrompt, _blockValue.Block.GetLocalizedBlockName()) + text;
		}
		return string.Format(this.inactivePrompt, _blockValue.Block.GetLocalizedBlockName());
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("show_bounds", "frames", false, false, null), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("hide_bounds", "frames", false, false, null), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("remove", "x", false, false, null), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
	}

	public override bool AllowBlockActivationCommand(ITileEntityFeature _module, ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _entityFocusing)
	{
		if (!base.AllowBlockActivationCommand(_module, _commandName, _world, _blockPos, _blockValue, _entityFocusing))
		{
			return false;
		}
		bool flag = base.Parent.LocalPlayerIsOwner && this.IsPrimary();
		if (!this.Equals(_module))
		{
			return true;
		}
		if (this.lockFeature != null && !GameManager.Instance.IsEditMode() && this.lockFeature.IsLocked() && !this.lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier))
		{
			return false;
		}
		if (base.CommandIs(_commandName, "show_bounds"))
		{
			return flag && !this.ShowBounds;
		}
		if (base.CommandIs(_commandName, "hide_bounds"))
		{
			return flag && this.ShowBounds;
		}
		base.CommandIs(_commandName, "remove");
		return flag;
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (base.CommandIs(_commandName, "show_bounds") || base.CommandIs(_commandName, "hide_bounds"))
		{
			this.ShowBounds = !this.ShowBounds;
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(base.ToWorldPos());
			if (boundsHelper != null)
			{
				this.BoundsHelper = boundsHelper;
				boundsHelper.gameObject.SetActive(this.ShowBounds);
			}
			return true;
		}
		if (base.CommandIs(_commandName, "remove"))
		{
			_world.SetBlockRPC(_blockPos, BlockValue.Air);
			_player.PlayOneShot("keystone_destroyed", false, false, false, null, 1f);
			return true;
		}
		return false;
	}

	public override void OnAdded(Vector3i _blockPos, BlockValue _blockValue)
	{
		base.OnAdded(_blockPos, _blockValue);
		GameManager.Instance.persistentPlayers.PlaceLandProtectionBlock(base.ToWorldPos(), base.Parent.Owner);
		if (base.Parent.LocalPlayerIsOwner)
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(base.ToWorldPos());
			if (boundsHelper != null)
			{
				this.BoundsHelper = boundsHelper;
				boundsHelper.gameObject.SetActive(this.ShowBounds);
			}
		}
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
		this.showBounds = _br.ReadBoolean();
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
		_bw.Write(this.showBounds);
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (this.BoundsHelper != null)
		{
			this.BoundsHelper.localPosition = base.ToWorldPos().ToVector3() - Origin.position + new Vector3(0.5f, 0.5f, 0.5f);
		}
	}

	public override void OnLoad()
	{
		base.OnLoad();
		Vector3i vector3i = base.ToWorldPos();
		if (!this.IsPrimary())
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				TEFeatureLandClaim.HandleDeactivateLandClaim(vector3i);
			}
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(vector3i, "land_claim");
			if (GameManager.Instance.persistentPlayers.m_lpBlockMap.ContainsKey(vector3i))
			{
				PersistentPlayerData persistentPlayerData = GameManager.Instance.persistentPlayers.m_lpBlockMap[vector3i];
				GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(vector3i);
				persistentPlayerData.LPBlocks.Remove(vector3i);
			}
			this.ShowBounds = false;
		}
		if (base.Parent.LocalPlayerIsOwner)
		{
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(vector3i);
			if (boundsHelper != null)
			{
				this.BoundsHelper = boundsHelper;
				boundsHelper.gameObject.SetActive(this.ShowBounds);
			}
		}
	}

	public override void OnUnload(World _world)
	{
		base.OnUnload(_world);
		LandClaimBoundsHelper.RemoveBoundsHelper(base.ToWorldPos());
	}

	public override void OnRemove(World _world)
	{
		base.OnRemove(_world);
		LandClaimBoundsHelper.RemoveBoundsHelper(base.ToWorldPos());
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		GameManager.Instance.persistentPlayers.RemoveLandProtectionBlock(base.ToWorldPos());
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, base.ToWorldPos()), false, -1, -1, -1, null, 192, false);
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		if (!this.IsPrimary())
		{
			this.ShowBounds = false;
			Transform boundsHelper = LandClaimBoundsHelper.GetBoundsHelper(base.ToWorldPos());
			if (boundsHelper != null)
			{
				this.BoundsHelper = null;
				boundsHelper.gameObject.SetActive(false);
				LandClaimBoundsHelper.RemoveBoundsHelper(base.ToWorldPos());
			}
		}
	}

	public bool IsPrimary()
	{
		return GameManager.Instance.persistentPlayers.GetLandProtectionBlockOwner(base.ToWorldPos()) != null;
	}

	public static void HandleDeactivateLandClaim(Vector3i _blockPos)
	{
		World world = GameManager.Instance.World;
		BlockValue block = world.GetBlock(_blockPos);
		if (!block.isair)
		{
			GameManager.Instance.persistentPlayers.m_lpBlockMap.Remove(_blockPos);
			NavObjectManager.Instance.UnRegisterNavObjectByPosition(_blockPos.ToVector3(), "land_claim");
			LandClaimBoundsHelper.RemoveBoundsHelper(_blockPos.ToVector3());
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				block.damage = block.Block.MaxDamage - 1;
				world.SetBlockRPC(_blockPos, block);
				SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>().Setup(EnumMapObjectType.LandClaim, _blockPos), false, -1, -1, -1, null, 192, false);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockPickable lockpickFeature;

	public Transform BoundsHelper;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool showBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string activePrompt;

	[PublicizedFrom(EAccessModifier.Private)]
	public string inactivePrompt;
}
