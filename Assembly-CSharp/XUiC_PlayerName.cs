using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PlayerName : XUiController
{
	public Color Color
	{
		get
		{
			return this.lblName.Color;
		}
		set
		{
			this.lblName.Color = value;
			this.lblNameCrossplay.Color = value;
		}
	}

	public override void Init()
	{
		base.Init();
		this.rect.IsNavigatable = (this.rect.IsSnappable = false);
		base.OnPress += this.PlayerName_OnPress;
	}

	public override void Cleanup()
	{
		base.Cleanup();
		base.OnPress -= this.PlayerName_OnPress;
	}

	public void SetGenericName(string _name)
	{
		this.UpdatePlayerData(null, false, _name);
	}

	public void UpdatePlayerData(PlayerData _playerData, bool _showCrossplay, string _displayName = null)
	{
		this.playerData = _playerData;
		bool flag = false;
		if (_displayName != null)
		{
			XUiV_Label xuiV_Label = this.lblName;
			this.lblNameCrossplay.Text = _displayName;
			xuiV_Label.Text = _displayName;
			flag = true;
		}
		else if (this.playerData != null)
		{
			GeneratedTextManager.GetDisplayText(this.playerData.PlayerName, delegate(string _name)
			{
				XUiV_Label xuiV_Label2 = this.lblName;
				this.lblNameCrossplay.Text = _name;
				xuiV_Label2.Text = _name;
			}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
			flag = true;
		}
		this.rect.EventOnPress = flag && this.CanShowProfile();
		this.rect.IsNavigatable = (this.rect.IsSnappable = flag);
		if (_showCrossplay && this.playerData != null && this.playerData.PlayGroup != EPlayGroup.Unknown)
		{
			this.sprIconCrossplay.SpriteName = PlatformManager.NativePlatform.Utils.GetCrossplayPlayerIcon(this.playerData.PlayGroup, true, this.playerData.NativeId.PlatformIdentifier);
			this.sprIconCrossplay.UIAtlas = "SymbolAtlas";
			this.sprIconCrossplay.IsVisible = true;
			this.lblName.IsVisible = false;
			this.lblNameCrossplay.IsVisible = true;
		}
		else
		{
			this.sprIconCrossplay.IsVisible = false;
			this.lblName.IsVisible = true;
			this.lblNameCrossplay.IsVisible = false;
		}
		base.RefreshBindings();
	}

	public void ClearPlayerData()
	{
		this.playerData = null;
		this.lblName.Text = string.Empty;
		this.lblNameCrossplay.Text = string.Empty;
		this.sprIconCrossplay.IsVisible = false;
	}

	public bool CanShowProfile()
	{
		return this.playerData != null && ((this.playerData.NativeId != null && PlatformManager.MultiPlatform.User.CanShowProfile(this.playerData.NativeId)) || (this.playerData.PrimaryId != null && PlatformManager.MultiPlatform.User.CanShowProfile(this.playerData.PrimaryId)));
	}

	public void ShowProfile()
	{
		if (this.playerData == null)
		{
			return;
		}
		if (this.playerData.NativeId != null && PlatformManager.MultiPlatform.User.CanShowProfile(this.playerData.NativeId))
		{
			PlatformManager.MultiPlatform.User.ShowProfile(this.playerData.NativeId);
			return;
		}
		if (this.playerData.PrimaryId != null && PlatformManager.MultiPlatform.User.CanShowProfile(this.playerData.PrimaryId))
		{
			PlatformManager.MultiPlatform.User.ShowProfile(this.playerData.PrimaryId);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PlayerName_OnPress(XUiController _sender, int _mousebutton)
	{
		this.ShowProfile();
	}

	[XuiBindComponent("playerName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Rect rect;

	[XuiBindComponent("name", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblName;

	[XuiBindComponent("nameCrossplay", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Label lblNameCrossplay;

	[XuiBindComponent("iconCrossplay", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite sprIconCrossplay;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerData playerData;
}
