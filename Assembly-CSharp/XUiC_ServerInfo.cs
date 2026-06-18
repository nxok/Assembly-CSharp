using System;
using System.Collections.Generic;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ServerInfo : XUiController
{
	[XuiXmlBinding("serverurl")]
	public string ServerUrl
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.serverUrl ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.serverUrl = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("serverurlhovered")]
	public bool ServerUrlHovered
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.serverUrlHovered;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.serverUrlHovered = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("serverdescription")]
	public string ServerDescription
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.serverDescription ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.serverDescription = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("serversandboxcode")]
	public string ServerSandboxCode
	{
		get
		{
			return this.serverSandboxCode ?? "";
		}
		set
		{
			this.serverSandboxCode = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("sandboxpreset")]
	public string SandboxPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SandboxOptionPreset presetByCode = SandboxOptionManager.Current.GetPresetByCode(this.ServerSandboxCode);
			if (presetByCode == null)
			{
				return Localization.Get("sandboxPresetGroupUser", false, null);
			}
			if (presetByCode != null && !presetByCode.IsUserPreset && !presetByCode.IsCustomPreset && !presetByCode.IsModded)
			{
				return presetByCode.DisplayName;
			}
			return Localization.Get("sandboxPresetGroupUser", false, null);
		}
	}

	[XuiXmlBinding("showip")]
	public bool ShowIp
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.displayMode == XUiC_ServerInfo.DisplayMode.Dedicated;
		}
	}

	public override void Init()
	{
		base.Init();
		foreach (XUiC_ServerBrowserGamePrefInfo xuiC_ServerBrowserGamePrefInfo in base.GetChildrenByType<XUiC_ServerBrowserGamePrefInfo>(null))
		{
			this.infoFields.Add(xuiC_ServerBrowserGamePrefInfo);
			if (xuiC_ServerBrowserGamePrefInfo.ValueType == GamePrefs.EnumType.Int)
			{
				GameInfoInt gameInfoInt = xuiC_ServerBrowserGamePrefInfo.GameInfoInt;
				if (gameInfoInt != GameInfoInt.CurrentServerTime)
				{
					if (gameInfoInt == GameInfoInt.AirDropFrequency)
					{
						xuiC_ServerBrowserGamePrefInfo.CustomIntValueFormatter = (GameServerInfo _, int _value) => string.Format(Localization.Get("goAirDropValue" + ((_value == 1) ? "" : "s"), false, null), _value);
					}
				}
				else
				{
					xuiC_ServerBrowserGamePrefInfo.CustomIntValueFormatter = (GameServerInfo _, int _worldTime) => ValueDisplayFormatters.WorldTime((ulong)((long)_worldTime), Localization.Get("xuiDayTimeLong", false, null));
				}
			}
			else if (xuiC_ServerBrowserGamePrefInfo.ValueType == GamePrefs.EnumType.String && xuiC_ServerBrowserGamePrefInfo.GameInfoString == GameInfoString.ServerVersion)
			{
				xuiC_ServerBrowserGamePrefInfo.CustomStringValueFormatter = delegate(GameServerInfo _info, string _s)
				{
					if (_info.Version.Build != 0)
					{
						return (_info.IsCompatibleVersion ? "" : "[ff0000]") + _info.Version.LongString;
					}
					return "[ff0000]" + _s;
				};
			}
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
	}

	public void InitializeForListFilter(XUiC_ServersList.EnumServerLists _mode)
	{
		if (_mode == XUiC_ServersList.EnumServerLists.Peer || _mode == XUiC_ServersList.EnumServerLists.Friends || _mode == XUiC_ServersList.EnumServerLists.History)
		{
			this.setDisplayMode(XUiC_ServerInfo.DisplayMode.Peer);
			return;
		}
		this.setDisplayMode(XUiC_ServerInfo.DisplayMode.Dedicated);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setDisplayMode(XUiC_ServerInfo.DisplayMode _mode)
	{
		if (this.displayMode != _mode)
		{
			this.displayMode = _mode;
			this.IsDirty = true;
		}
	}

	public void SetServerInfo(GameServerInfo _gameInfo)
	{
		foreach (XUiC_ServerBrowserGamePrefInfo xuiC_ServerBrowserGamePrefInfo in this.infoFields)
		{
			xuiC_ServerBrowserGamePrefInfo.SetCurrentValue(_gameInfo);
		}
		this.ServerDescription = "";
		this.ServerUrl = "";
		this.unfilteredUrlText = "";
		string text = ((_gameInfo == null) ? "" : GeneratedTextManager.GetDisplayTextImmediately(_gameInfo.ServerDisplayName, false, GeneratedTextManager.TextFilteringMode.Filter, GeneratedTextManager.BbCodeSupportMode.NotSupported));
		GameStats.Set(EnumGameStats.SandboxPreset, text);
		this.ServerSandboxCode = ((_gameInfo == null) ? "" : _gameInfo.GetValue(GameInfoString.SandboxCode));
		if (_gameInfo == null)
		{
			return;
		}
		AuthoredText serverDisplayName = _gameInfo.ServerDescription;
		if (_gameInfo.IsDedicated || _gameInfo.IsLAN)
		{
			this.setDisplayMode(XUiC_ServerInfo.DisplayMode.Dedicated);
		}
		else
		{
			this.setDisplayMode(XUiC_ServerInfo.DisplayMode.Peer);
			if (string.IsNullOrEmpty(serverDisplayName.Text))
			{
				serverDisplayName = _gameInfo.ServerDisplayName;
			}
		}
		this.unfilteredUrlText = _gameInfo.ServerURL.Text;
		GeneratedTextManager.GetDisplayText(serverDisplayName, delegate(string _desc)
		{
			this.ServerDescription = _desc;
		}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.Supported);
		GeneratedTextManager.GetDisplayText(_gameInfo.ServerURL, delegate(string _url)
		{
			this.ServerUrl = _url;
		}, true, false, GeneratedTextManager.TextFilteringMode.FilterWithSafeString, GeneratedTextManager.BbCodeSupportMode.SupportedAndAddEscapes);
	}

	[XuiBindEvent("OnHover", "lblServerUrl")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void UrlController_OnHover(XUiController _sender, bool _isOver)
	{
		this.ServerUrlHovered = _isOver;
	}

	[XuiBindEvent("OnPress", "lblServerUrl")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void UrlController_OnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_MessageBoxWindowGroup.ShowUrlConfirmationDialog(this.xui, this.unfilteredUrlText, false, null, null, null, this.ServerUrl, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_ServerBrowserGamePrefInfo> infoFields = new List<XUiC_ServerBrowserGamePrefInfo>();

	[XuiBindComponent("ServerWebsiteURL", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiController lblServerUrl;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_ServerInfo.DisplayMode displayMode;

	[PublicizedFrom(EAccessModifier.Private)]
	public string unfilteredUrlText;

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverUrl;

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverDescription;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool serverUrlHovered;

	[PublicizedFrom(EAccessModifier.Private)]
	public string serverSandboxCode;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum DisplayMode
	{
		Dedicated,
		Peer
	}
}
