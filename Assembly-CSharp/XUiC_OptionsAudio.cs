using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsAudio : XUiC_OptionsDialogBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public static void updateVoiceDeviceList(XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> _combo, IEnumerable<IPartyVoice.VoiceAudioDevice> _devices)
	{
		if (_combo == null)
		{
			return;
		}
		_combo.Elements.Clear();
		_combo.Elements.AddRange(_devices);
		if (_combo.Elements.Count == 0)
		{
			_combo.Elements.Add(XUiC_OptionsAudio.noDeviceEntry);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void updateVoiceDeviceSelection(XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> _combo, string _activeDeviceIdentifier)
	{
		if (_combo == null)
		{
			return;
		}
		if (_combo.Elements.Count == 1)
		{
			_combo.SelectedIndex = 0;
			return;
		}
		int num;
		if (string.IsNullOrEmpty(_activeDeviceIdentifier) || (num = _combo.Elements.FindIndex((IPartyVoice.VoiceAudioDevice _device) => _device.Identifier == _activeDeviceIdentifier)) < 0)
		{
			int num2 = _combo.Elements.FindIndex((IPartyVoice.VoiceAudioDevice _device) => _device.IsDefault);
			if (num2 < 0)
			{
				_combo.Elements.Insert(0, XUiC_OptionsAudio.defaultDeviceEntry);
				num2 = 0;
			}
			_combo.SelectedIndex = num2;
			return;
		}
		_combo.SelectedIndex = num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initEosVoiceDeviceOptions()
	{
		if (this.optionVoiceOutputDevice == null || this.comboVoiceOutputDevice == null || this.optionVoiceInputDevice == null || this.comboVoiceInputDevice == null)
		{
			return;
		}
		this.optionVoiceOutputDevice.GetSettingValue = delegate
		{
			this.<initEosVoiceDeviceOptions>g__UpdateVoiceDeviceLists|9_12();
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboVoiceOutputDevice, GamePrefs.GetString(EnumGamePrefs.OptionsVoiceOutputDevice));
		};
		this.optionVoiceOutputDevice.DiscardChanges = delegate
		{
			this.<initEosVoiceDeviceOptions>g__UpdateVoiceDeviceLists|9_12();
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboVoiceOutputDevice, GamePrefs.GetString(EnumGamePrefs.OptionsVoiceOutputDevice));
		};
		this.optionVoiceOutputDevice.ApplyChanges = delegate(bool _)
		{
			if (this.VoiceAvailable)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsVoiceOutputDevice, this.comboVoiceOutputDevice.Value.Identifier);
			}
		};
		this.optionVoiceOutputDevice.ResetDefaults = delegate
		{
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboVoiceOutputDevice, (string)GamePrefs.GetDefault(EnumGamePrefs.OptionsVoiceOutputDevice));
		};
		this.optionVoiceOutputDevice.IsChangedDelegate = delegate
		{
			string @string = GamePrefs.GetString(EnumGamePrefs.OptionsVoiceOutputDevice);
			IPartyVoice.VoiceAudioDevice value = this.comboVoiceOutputDevice.Value;
			return !@string.Equals((value != null) ? value.Identifier : null);
		};
		this.optionVoiceOutputDevice.IsDefaultDelegate = delegate
		{
			string text = (string)GamePrefs.GetDefault(EnumGamePrefs.OptionsVoiceOutputDevice);
			IPartyVoice.VoiceAudioDevice value2 = this.comboVoiceOutputDevice.Value;
			return text.Equals((value2 != null) ? value2.Identifier : null);
		};
		this.optionVoiceInputDevice.GetSettingValue = delegate
		{
			this.<initEosVoiceDeviceOptions>g__UpdateVoiceDeviceLists|9_12();
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboVoiceInputDevice, GamePrefs.GetString(EnumGamePrefs.OptionsVoiceInputDevice));
		};
		this.optionVoiceInputDevice.DiscardChanges = delegate
		{
			this.<initEosVoiceDeviceOptions>g__UpdateVoiceDeviceLists|9_12();
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboVoiceInputDevice, GamePrefs.GetString(EnumGamePrefs.OptionsVoiceInputDevice));
		};
		this.optionVoiceInputDevice.ApplyChanges = delegate(bool _)
		{
			if (this.VoiceAvailable)
			{
				GamePrefs.Set(EnumGamePrefs.OptionsVoiceInputDevice, this.comboVoiceInputDevice.Value.Identifier);
			}
		};
		this.optionVoiceInputDevice.ResetDefaults = delegate
		{
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboVoiceInputDevice, (string)GamePrefs.GetDefault(EnumGamePrefs.OptionsVoiceInputDevice));
		};
		this.optionVoiceInputDevice.IsChangedDelegate = delegate
		{
			string string2 = GamePrefs.GetString(EnumGamePrefs.OptionsVoiceInputDevice);
			IPartyVoice.VoiceAudioDevice value3 = this.comboVoiceInputDevice.Value;
			return !string2.Equals((value3 != null) ? value3.Identifier : null);
		};
		this.optionVoiceInputDevice.IsDefaultDelegate = delegate
		{
			string text2 = (string)GamePrefs.GetDefault(EnumGamePrefs.OptionsVoiceInputDevice);
			IPartyVoice.VoiceAudioDevice value4 = this.comboVoiceInputDevice.Value;
			return text2.Equals((value4 != null) ? value4.Identifier : null);
		};
	}

	public static event Action OnSettingsChanged;

	[XuiXmlBinding("voiceavailable")]
	public bool VoiceAvailable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PlatformManager.MultiPlatform.PartyVoice != null && PlatformManager.MultiPlatform.PartyVoice.Status == EPartyVoiceStatus.Ok;
		}
	}

	[XuiXmlBinding("commsallowed")]
	public bool IsCommunicationAllowed
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PermissionsManager.IsCommunicationAllowed();
		}
	}

	[XuiXmlBinding("multiplayerallowed")]
	public bool MultiplayerAllowed
	{
		get
		{
			return PermissionsManager.IsMultiplayerAllowed();
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_OptionsAudio.ID = base.WindowGroup.Id;
		this.initEosVoiceDeviceOptions();
		this.initDiscordVoiceDeviceOptions();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void afterChangesSaved()
	{
		base.afterChangesSaved();
		this.applyDiscordDisabledChanged();
		Action onSettingsChanged = XUiC_OptionsAudio.OnSettingsChanged;
		if (onSettingsChanged == null)
		{
			return;
		}
		onSettingsChanged();
	}

	public override void OnOpen()
	{
		this.onOpenDiscord();
		base.OnOpen();
	}

	public override void OnClose()
	{
		base.OnClose();
		this.onCloseDiscord();
	}

	public void SetActiveTab(string _tabKey)
	{
		XUiC_TabSelector tabSelector = this.TabSelector;
		if (tabSelector == null)
		{
			return;
		}
		tabSelector.SelectTabByName(_tabKey);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initDiscordVoiceDeviceOptions()
	{
		if (this.optionDiscordOutputDevice == null || this.comboDiscordOutputDevice == null || this.optionDiscordInputDevice == null || this.comboDiscordInputDevice == null)
		{
			return;
		}
		this.optionDiscordOutputDevice.GetSettingValue = delegate
		{
			if (!this.IsCommunicationAllowed)
			{
				return;
			}
			XUiC_OptionsAudio.updateVoiceDeviceList(this.comboDiscordOutputDevice, DiscordManager.Instance.AudioOutput.CurrentAudioDevices.Values);
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboDiscordOutputDevice, DiscordManager.Instance.AudioOutput.ActiveAudioDevice);
		};
		this.optionDiscordOutputDevice.DiscardChanges = delegate
		{
			XUiC_OptionsAudio.updateVoiceDeviceList(this.comboDiscordOutputDevice, DiscordManager.Instance.AudioOutput.CurrentAudioDevices.Values);
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboDiscordOutputDevice, DiscordManager.Instance.AudioOutput.ActiveAudioDevice);
		};
		this.optionDiscordOutputDevice.ApplyChanges = delegate(bool _)
		{
			if (this.VoiceAvailable)
			{
				DiscordManager.Instance.Settings.SelectedOutputDevice = this.comboDiscordOutputDevice.Value.Identifier;
			}
		};
		this.optionDiscordOutputDevice.ResetDefaults = delegate
		{
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboDiscordOutputDevice, (string)GamePrefs.GetDefault(EnumGamePrefs.DiscordSelectedOutputDevice));
		};
		this.optionDiscordOutputDevice.IsChangedDelegate = delegate
		{
			string @string = GamePrefs.GetString(EnumGamePrefs.DiscordSelectedOutputDevice);
			IPartyVoice.VoiceAudioDevice value = this.comboDiscordOutputDevice.Value;
			return !@string.Equals((value != null) ? value.Identifier : null);
		};
		this.optionDiscordOutputDevice.IsDefaultDelegate = delegate
		{
			string text = (string)GamePrefs.GetDefault(EnumGamePrefs.DiscordSelectedOutputDevice);
			IPartyVoice.VoiceAudioDevice value2 = this.comboDiscordOutputDevice.Value;
			return text.Equals((value2 != null) ? value2.Identifier : null);
		};
		this.optionDiscordInputDevice.GetSettingValue = delegate
		{
			if (!this.IsCommunicationAllowed)
			{
				return;
			}
			XUiC_OptionsAudio.updateVoiceDeviceList(this.comboDiscordInputDevice, DiscordManager.Instance.AudioInput.CurrentAudioDevices.Values);
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboDiscordInputDevice, DiscordManager.Instance.AudioInput.ActiveAudioDevice);
		};
		this.optionDiscordInputDevice.DiscardChanges = delegate
		{
			XUiC_OptionsAudio.updateVoiceDeviceList(this.comboDiscordInputDevice, DiscordManager.Instance.AudioInput.CurrentAudioDevices.Values);
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboDiscordInputDevice, DiscordManager.Instance.AudioInput.ActiveAudioDevice);
		};
		this.optionDiscordInputDevice.ApplyChanges = delegate(bool _)
		{
			if (this.VoiceAvailable)
			{
				DiscordManager.Instance.Settings.SelectedInputDevice = this.comboDiscordInputDevice.Value.Identifier;
			}
		};
		this.optionDiscordInputDevice.ResetDefaults = delegate
		{
			XUiC_OptionsAudio.updateVoiceDeviceSelection(this.comboDiscordInputDevice, (string)GamePrefs.GetDefault(EnumGamePrefs.DiscordSelectedInputDevice));
		};
		this.optionDiscordInputDevice.IsChangedDelegate = delegate
		{
			string string2 = GamePrefs.GetString(EnumGamePrefs.DiscordSelectedInputDevice);
			IPartyVoice.VoiceAudioDevice value3 = this.comboDiscordInputDevice.Value;
			return !string2.Equals((value3 != null) ? value3.Identifier : null);
		};
		this.optionDiscordInputDevice.IsDefaultDelegate = delegate
		{
			string text2 = (string)GamePrefs.GetDefault(EnumGamePrefs.DiscordSelectedInputDevice);
			IPartyVoice.VoiceAudioDevice value4 = this.comboDiscordInputDevice.Value;
			return text2.Equals((value4 != null) ? value4.Identifier : null);
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordVoiceDeviceListChanged(DiscordManager.AudioDeviceConfig _inOutConfig)
	{
		XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> xuiC_ComboBoxList = (_inOutConfig.IsOutput ? this.comboDiscordOutputDevice : this.comboDiscordInputDevice);
		XUiC_OptionsAudio.updateVoiceDeviceList(xuiC_ComboBoxList, _inOutConfig.CurrentAudioDevices.Values);
		XUiC_OptionsAudio.updateVoiceDeviceSelection(xuiC_ComboBoxList, _inOutConfig.ActiveAudioDevice);
	}

	[XuiXmlBinding("discord_ptt")]
	public bool DiscordVoiceButtonMode
	{
		get
		{
			XUiC_OptionEntryGamePrefBool xuiC_OptionEntryGamePrefBool = this.optionDiscordVoiceButtonMode;
			return xuiC_OptionEntryGamePrefBool != null && xuiC_OptionEntryGamePrefBool.CurrentValue;
		}
	}

	[XuiXmlBinding("discord_enabled")]
	public bool DiscordEnabled
	{
		get
		{
			return !DiscordManager.Instance.Settings.DiscordDisabled;
		}
	}

	[XuiXmlBinding("discordinitialized")]
	public bool DiscordInitialized
	{
		get
		{
			return DiscordManager.Instance.IsInitialized;
		}
	}

	[XuiXmlBinding("discord_is_ready")]
	public bool DiscordReady
	{
		get
		{
			return DiscordManager.Instance.IsReady;
		}
	}

	[XuiXmlBinding("discord_supports_full_accounts")]
	public bool DiscordSupportsFullAccounts
	{
		get
		{
			return DiscordManager.SupportsFullAccounts;
		}
	}

	[XuiXmlBinding("discord_supports_provisional_accounts")]
	public bool DiscordSupportsProvisionalAccounts
	{
		get
		{
			return DiscordManager.SupportsProvisionalAccounts;
		}
	}

	[XuiXmlBinding("discordaccountlinked")]
	public bool DiscordAccountLinked
	{
		get
		{
			DiscordManager.DiscordUser localUser = DiscordManager.Instance.LocalUser;
			return localUser == null || !localUser.IsProvisionalAccount;
		}
	}

	[XuiXmlBinding("discord_vad_threshold_manual")]
	public bool DiscordVoiceVadThresholdManual
	{
		get
		{
			return !DiscordManager.Instance.Settings.VoiceVadModeAuto;
		}
	}

	[XuiXmlBinding("discord_in_call")]
	public bool DiscordInCall
	{
		get
		{
			return DiscordManager.Instance.ActiveVoiceLobby != null;
		}
	}

	[XuiXmlBinding("discord_is_speaking")]
	public bool DiscordIsSpeaking
	{
		get
		{
			DiscordManager.LobbyInfo activeVoiceLobby = DiscordManager.Instance.ActiveVoiceLobby;
			return activeVoiceLobby != null && activeVoiceLobby.VoiceCall.IsSpeaking;
		}
	}

	[XuiBindEvent("OnPress", "btnDiscordInitialize")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordInitializeOnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_DiscordLogin.Open(delegate
		{
			this.OpenAtTab("xuiOptionsAudioDiscord");
		}, false, false, false, true, false);
		DiscordManager.Instance.AuthManager.AutoLogin();
	}

	[XuiBindEvent("OnPress", "btnDiscordLinkAccount")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordLinkAccountOnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_DiscordLogin.Open(delegate
		{
			this.OpenAtTab("xuiOptionsAudioDiscord");
		}, false, false, false, true, (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent());
		DiscordManager.Instance.AuthManager.LoginDiscordUser();
	}

	[XuiBindEvent("OnPress", "btnDiscordUnlinkAccount")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordUnlinkAccountOnPress(XUiController _sender, int _mouseButton)
	{
		XUiC_DiscordLogin.Open(delegate
		{
			this.OpenAtTab("xuiOptionsAudioDiscord");
		}, false, false, false, true, (DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent());
		DiscordManager.Instance.AuthManager.UnmergeAccount();
	}

	[XuiBindEvent("OnPress", "btnDiscordManageSocialSettings")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordManageSocialSettingsOnPress(XUiController _sender, int _mouseButton)
	{
		DiscordManager.Instance.OpenDiscordSocialSettings();
	}

	[XuiBindEvent("OnPress", "btnDiscordManageBlockedUsers")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void btnDiscordManageBlockedUsersOnPress(XUiController _sender, int _mouseButton)
	{
		GUIWindow window = this.xui.playerUI.windowManager.GetWindow(XUiC_DiscordBlockedUsers.ID);
		this.xui.playerUI.windowManager.Open(window, false);
	}

	public void OpenAtTab(string _tabName)
	{
		this.xui.playerUI.windowManager.Open(this.windowGroup, true);
		this.SetActiveTab(_tabName);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordStatusChanged(DiscordManager.EDiscordStatus _status)
	{
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordLocalUserChanged(bool _loggedIn)
	{
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discordVoiceStateChanged(bool _self, ulong _userId)
	{
		if (!_self)
		{
			return;
		}
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyDiscordDisabledChanged()
	{
		bool isInitialized = DiscordManager.Instance.IsInitialized;
		bool flag = !DiscordManager.Instance.Settings.DiscordDisabled;
		if (isInitialized == flag)
		{
			return;
		}
		if (!flag)
		{
			XUiC_MessageBoxWindowGroup.ShowOk(this.xui, Localization.Get("xuiOptionsAudioDiscordDisableRestartTitle", false, null), Localization.Get("xuiOptionsAudioDiscordDisableRestartText", false, null), null, false, false, false);
		}
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onOpenDiscord()
	{
		DiscordManager instance = DiscordManager.Instance;
		instance.AudioDevicesChanged += this.discordVoiceDeviceListChanged;
		instance.LocalUserChanged += this.discordLocalUserChanged;
		instance.StatusChanged += this.discordStatusChanged;
		instance.VoiceStateChanged += this.discordVoiceStateChanged;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void onCloseDiscord()
	{
		DiscordManager instance = DiscordManager.Instance;
		instance.AudioDevicesChanged -= this.discordVoiceDeviceListChanged;
		instance.LocalUserChanged -= this.discordLocalUserChanged;
		instance.StatusChanged -= this.discordStatusChanged;
		instance.VoiceStateChanged -= this.discordVoiceStateChanged;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <initEosVoiceDeviceOptions>g__UpdateVoiceDeviceLists|9_12()
	{
		int frameCount = Time.frameCount;
		if (this.lastDeviceListUpdateFrame == frameCount)
		{
			return;
		}
		this.lastDeviceListUpdateFrame = frameCount;
		if (!this.VoiceAvailable || !this.IsCommunicationAllowed)
		{
			return;
		}
		ValueTuple<IList<IPartyVoice.VoiceAudioDevice>, IList<IPartyVoice.VoiceAudioDevice>> devicesList = PlatformManager.MultiPlatform.PartyVoice.GetDevicesList();
		IList<IPartyVoice.VoiceAudioDevice> item = devicesList.Item1;
		IList<IPartyVoice.VoiceAudioDevice> item2 = devicesList.Item2;
		XUiC_OptionsAudio.updateVoiceDeviceList(this.comboVoiceOutputDevice, item2);
		XUiC_OptionsAudio.updateVoiceDeviceList(this.comboVoiceInputDevice, item);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly IPartyVoice.VoiceAudioDevice noDeviceEntry = new IPartyVoice.VoiceAudioDeviceNotFound();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly IPartyVoice.VoiceAudioDevice defaultDeviceEntry = new IPartyVoice.VoiceAudioDeviceDefault();

	[XuiBindComponent("OptionVoiceOutputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionVoiceOutputDevice;

	[XuiBindComponent("VoiceOutputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboVoiceOutputDevice;

	[XuiBindComponent("OptionVoiceInputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionVoiceInputDevice;

	[XuiBindComponent("VoiceInputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboVoiceInputDevice;

	[PublicizedFrom(EAccessModifier.Private)]
	public int lastDeviceListUpdateFrame;

	public static string ID = "";

	public const string DiscordTabName = "xuiOptionsAudioDiscord";

	[XuiBindComponent("OptionDiscordOutputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionDiscordOutputDevice;

	[XuiBindComponent("DiscordOutputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboDiscordOutputDevice;

	[XuiBindComponent("OptionDiscordInputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionDiscordInputDevice;

	[XuiBindComponent("DiscordInputDevice", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<IPartyVoice.VoiceAudioDevice> comboDiscordInputDevice;

	[XuiBindComponent("OptionDiscordVoiceButtonMode", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefBool optionDiscordVoiceButtonMode;

	[XuiBindComponent("btnDiscordInitialize", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordInitialize;

	[XuiBindComponent("btnDiscordLinkAccount", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordLinkAccount;

	[XuiBindComponent("btnDiscordUnlinkAccount", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordUnlinkAccount;

	[XuiBindComponent("btnDiscordManageSocialSettings", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordManageSocialSettings;

	[XuiBindComponent("btnDiscordManageBlockedUsers", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDiscordManageBlockedUsers;
}
