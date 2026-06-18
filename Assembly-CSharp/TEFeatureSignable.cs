using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class TEFeatureSignable : TEFeatureAbs, ITileEntitySignable, ITileEntity, ILockTarget, IFeatureSavedInPrefab
{
	public TEFeatureSignable()
	{
		PlatformUserManager.BlockedStateChanged += this.UserBlockedStateChanged;
	}

	public override void OnUnload(World _world)
	{
		PlatformUserManager.BlockedStateChanged -= this.UserBlockedStateChanged;
	}

	public override void OnDestroy()
	{
		PlatformUserManager.BlockedStateChanged -= this.UserBlockedStateChanged;
	}

	public override void Init(TileEntityComposite _parent, TileEntityFeatureData _featureData)
	{
		base.Init(_parent, _featureData);
		this.lockFeature = base.Parent.GetFeature<TEFeatureLockable>();
		this.lockpickFeature = base.Parent.GetFeature<TEFeatureLockPickable>();
		DynamicProperties props = _featureData.Props;
		props.ParseInt("LineCount", ref this.lineCount);
		props.ParseFloat("LineWidth", ref this.lineWidth);
		props.ParseInt("FontSize", ref this.fontSize);
		props.ParseFloat("LineSpacing", ref this.lineSpacing);
		if (this.lineWidth <= 0f || this.lineCount <= 0 || this.fontSize <= 0 || this.lineSpacing <= 0f)
		{
			Log.Error(string.Format("Block '{0}' has TEFeatureSignable with missing or invalid properties: LineWidth={1}, LineCount={2}, FontSize={3}, LineSpacing={4}", new object[]
			{
				base.Parent.TeData.Block.GetBlockName(),
				this.lineWidth,
				this.lineCount,
				this.fontSize,
				this.lineSpacing
			}));
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyFromInternal(TileEntityComposite _other)
	{
		TEFeatureSignable tefeatureSignable;
		if (_other.TryGetSelfOrFeature(out tefeatureSignable))
		{
			this.signText = tefeatureSignable.signText.Clone();
		}
	}

	public override void SetBlockEntityData(BlockEntityData _blockEntityData)
	{
		if (_blockEntityData == null || !_blockEntityData.bHasTransform)
		{
			return;
		}
		if (GameManager.IsDedicatedServer)
		{
			return;
		}
		if (this.lineWidth <= 0f || this.lineCount <= 0 || this.fontSize <= 0 || this.lineSpacing <= 0f)
		{
			return;
		}
		float num = this.lineWidth;
		TextMesh[] componentsInChildren = _blockEntityData.transform.GetComponentsInChildren<TextMesh>();
		if (componentsInChildren == null || componentsInChildren.Length == 0)
		{
			return;
		}
		this.smartTextMesh = new SmartTextMesh[componentsInChildren.Length];
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].fontSize = this.fontSize;
			componentsInChildren[i].characterSize = 0.01f;
			componentsInChildren[i].lineSpacing = this.lineSpacing;
			this.smartTextMesh[i] = componentsInChildren[i].gameObject.AddComponent<SmartTextMesh>();
			this.smartTextMesh[i].MaxWidthReal = num;
			this.smartTextMesh[i].MaxLines = this.lineCount;
			this.smartTextMesh[i].ConvertNewLines = true;
		}
		string text;
		if ((text = this.displayText) == null)
		{
			AuthoredText authoredText = this.signText;
			text = ((authoredText != null) ? authoredText.Text : null);
		}
		this.RefreshTextMesh(text);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void UserBlockedStateChanged(IPlatformUserData _userData, EBlockType _blockType, EUserBlockState _blockState)
	{
		if (!_userData.PrimaryId.Equals(this.signText.Author) || _blockType != EBlockType.TextChat)
		{
			return;
		}
		GeneratedTextManager.GetDisplayText(this.signText, new Action<string>(this.RefreshTextMesh), true, true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void RefreshTextMesh(string _text)
	{
		this.displayText = _text;
		if (GameManager.IsDedicatedServer || this.smartTextMesh == null || this.displayText == this.smartTextMesh[0].UnwrappedText)
		{
			return;
		}
		for (int i = 0; i < this.smartTextMesh.Length; i++)
		{
			this.smartTextMesh[i].UnwrappedText = this.displayText;
		}
	}

	public override void UpgradeDowngradeFrom(TileEntityComposite _other)
	{
		base.UpgradeDowngradeFrom(_other);
		ITileEntitySignable feature = _other.GetFeature<ITileEntitySignable>();
		if (feature != null)
		{
			this.signText = feature.GetAuthoredText();
			if (this.signText != null)
			{
				GeneratedTextManager.GetDisplayText(this.signText, new Action<string>(this.RefreshTextMesh), true, true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
			}
		}
	}

	public virtual void SetText(AuthoredText _authoredText, bool _syncData = true)
	{
		this.SetText((_authoredText != null) ? _authoredText.Text : null, _syncData, (_authoredText != null) ? _authoredText.Author : null);
	}

	public virtual void SetText(string _text, bool _syncData = true, PlatformUserIdentifierAbs _signingPlayer = null)
	{
		if (_signingPlayer == null)
		{
			_signingPlayer = PlatformManager.MultiPlatform.User.PlatformUserId;
		}
		PersistentPlayerList persistentPlayers = GameManager.Instance.persistentPlayers;
		if (((persistentPlayers != null) ? persistentPlayers.GetPlayerData(_signingPlayer) : null) == null)
		{
			_signingPlayer = null;
		}
		if (_text == this.signText.Text)
		{
			return;
		}
		this.signText.Update(_text, _signingPlayer);
		GeneratedTextManager.GetDisplayText(this.signText, new Action<string>(this.RefreshTextMesh), true, true, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.NotSupported);
		if (_syncData)
		{
			base.SetModified();
		}
	}

	public virtual AuthoredText GetAuthoredText()
	{
		return this.signText;
	}

	public virtual bool CanRenderString(string _text)
	{
		return this.smartTextMesh != null && this.smartTextMesh.Length != 0 && this.smartTextMesh[0].CanRenderString(_text);
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
		return Localization.Get("useWorkstation", false, null);
	}

	public override void InitBlockActivationCommands(Action<BlockActivationCommand, TileEntityComposite.EBlockCommandOrder, TileEntityFeatureData> _addCallback)
	{
		base.InitBlockActivationCommands(_addCallback);
		_addCallback(new BlockActivationCommand("edit", "pen", true, false, null), TileEntityComposite.EBlockCommandOrder.Normal, base.FeatureData);
		_addCallback(new BlockActivationCommand("report", "report", true, false, null), TileEntityComposite.EBlockCommandOrder.Last, base.FeatureData);
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
		if (base.CommandIs(_commandName, "edit"))
		{
			return this.lockFeature == null || GameManager.Instance.IsEditMode() || !this.lockFeature.IsLocked() || this.lockFeature.IsUserAllowed(PlatformManager.InternalLocalUserIdentifier);
		}
		if (base.CommandIs(_commandName, "report"))
		{
			PlatformUserIdentifierAbs internalLocalUserIdentifier = PlatformManager.InternalLocalUserIdentifier;
			bool flag = PlatformManager.MultiPlatform.PlayerReporting != null && !string.IsNullOrEmpty(this.signText.Text) && !internalLocalUserIdentifier.Equals(this.signText.Author);
			PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(this.signText.Author);
			bool flag2 = playerData != null && playerData.PlatformData.Blocked[EBlockType.TextChat].IsBlocked();
			return flag && !flag2;
		}
		return true;
	}

	public override bool OnBlockActivated(ReadOnlySpan<char> _commandName, WorldBase _world, Vector3i _blockPos, BlockValue _blockValue, EntityPlayerLocal _player)
	{
		base.OnBlockActivated(_commandName, _world, _blockPos, _blockValue, _player);
		if (base.CommandIs(_commandName, "edit"))
		{
			_player.AimingGun = false;
			LockManager.Instance.LockRequestLocal(this, null, 0);
			return true;
		}
		if (base.CommandIs(_commandName, "report"))
		{
			GeneratedTextManager.GetDisplayText(this.signText, delegate(string _filtered)
			{
				ThreadManager.AddSingleTaskMainThread("OpenReportWindow", delegate(object _)
				{
					PersistentPlayerData playerData = GameManager.Instance.persistentPlayers.GetPlayerData(this.signText.Author);
					XUiC_ReportPlayer.Open((playerData != null) ? playerData.PlayerData : null, EnumReportCategory.VerbalAbuse, string.Format(Localization.Get("xuiReportOffensiveTextMessage", false, null), _filtered));
				}, null);
			}, true, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
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
		this.SetText(AuthoredText.FromStream(_br), false);
	}

	public override void Write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.Write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
		if (GameManager.Instance.IsEditMode())
		{
			AuthoredText.ToStream(new AuthoredText(this.signText.Text, null), _bw);
			return;
		}
		AuthoredText.ToStream(this.signText, _bw);
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		TEFeatureSignable.ShowUI(this, _success);
	}

	public static void ShowUI(ITileEntitySignable _te, bool _lockGranted)
	{
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_lockGranted)
		{
			GameManager.ShowTooltip(uiforPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractItem", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			return;
		}
		((XUiWindowGroup)uiforPrimaryPlayer.windowManager.GetWindow("signMultiline")).Controller.GetChildByType<XUiC_SignWindow>().SetTileEntitySign(_te);
		uiforPrimaryPlayer.windowManager.Open("signMultiline", true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int Version = 18;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockable lockFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public TEFeatureLockPickable lockpickFeature;

	[PublicizedFrom(EAccessModifier.Private)]
	public AuthoredText signText = new AuthoredText();

	[PublicizedFrom(EAccessModifier.Private)]
	public int fontSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lineCount;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lineWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lineSpacing;

	[PublicizedFrom(EAccessModifier.Private)]
	public string displayText;

	[PublicizedFrom(EAccessModifier.Private)]
	public SmartTextMesh[] smartTextMesh;
}
