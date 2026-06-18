using System;
using System.Collections.Generic;
using Platform;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_NewContinueGameSettings : XUiController
{
	public event Action SettingsChanged;

	public event Action OpenSandboxSettingsRequested;

	[XuiXmlBinding("crossplayTooltip")]
	public string CrossplayTooltip
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			string permissionDenyReason = PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay, PermissionsManager.PermissionSources.All);
			if (!string.IsNullOrEmpty(permissionDenyReason))
			{
				return permissionDenyReason;
			}
			if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent() && this.gameOptions.Count > 0)
			{
				this.gameOptions[EnumGamePrefs.ServerEACPeerToPeer].CheckDefaultValue();
				return string.Format(Localization.Get("xuiOptionsGeneralCrossplayTooltipPC", false, null), 8);
			}
			return Localization.Get("xuiOptionsGeneralCrossplayTooltip", false, null);
		}
	}

	[XuiXmlBinding("port_valid")]
	public bool PortValid
	{
		get
		{
			return this.portValid;
		}
		set
		{
			if (this.portValid == value)
			{
				return;
			}
			this.portValid = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_GamePrefSelector[] childControllers = base.GetChildControllers<XUiC_GamePrefSelector>("", null);
		int i = 0;
		while (i < childControllers.Length)
		{
			XUiC_GamePrefSelector xuiC_GamePrefSelector = childControllers[i];
			XUiC_GamePrefSelector xuiC_GamePrefSelector2 = xuiC_GamePrefSelector;
			xuiC_GamePrefSelector2.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xuiC_GamePrefSelector2.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(this.Option_OnValueChanged));
			EnumGamePrefs gamePref = xuiC_GamePrefSelector.GamePref;
			if (gamePref <= EnumGamePrefs.ServerEACPeerToPeer)
			{
				if (gamePref == EnumGamePrefs.ServerPort)
				{
					xuiC_GamePrefSelector.ControlText.UIInput.validation = 1;
					xuiC_GamePrefSelector.ControlText.OnChangeHandler += this.TxtPort_OnChangeHandler;
					goto IL_0116;
				}
				if (gamePref != EnumGamePrefs.ServerEACPeerToPeer)
				{
					goto IL_0116;
				}
				XUiC_GamePrefSelector xuiC_GamePrefSelector3 = xuiC_GamePrefSelector;
				xuiC_GamePrefSelector3.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xuiC_GamePrefSelector3.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(delegate(XUiC_GamePrefSelector _, EnumGamePrefs _)
				{
					this.RefreshMultiplayerOptionStates(GamePrefs.GetBool(EnumGamePrefs.ServerEnabled));
				}));
				goto IL_0116;
			}
			else
			{
				if (gamePref == EnumGamePrefs.BloodMoonFrequency)
				{
					XUiC_GamePrefSelector xuiC_GamePrefSelector4 = xuiC_GamePrefSelector;
					xuiC_GamePrefSelector4.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xuiC_GamePrefSelector4.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(this.CmbBloodMoonFrequency_OnChangeHandler));
					goto IL_0116;
				}
				if (gamePref == EnumGamePrefs.AirDropFrequency)
				{
					xuiC_GamePrefSelector.ValuePreDisplayModifierFunc = (int _n) => _n / 24;
					goto IL_0116;
				}
				if (gamePref != EnumGamePrefs.ServerEnabled)
				{
					goto IL_0116;
				}
				this.serverEnabledSelector = xuiC_GamePrefSelector;
			}
			IL_0123:
			i++;
			continue;
			IL_0116:
			this.gameOptions.Add(gamePref, xuiC_GamePrefSelector);
			goto IL_0123;
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty && this.tabSelector != null)
		{
			foreach (XUiC_TabSelectorTab xuiC_TabSelectorTab in this.tabSelector.Tabs)
			{
				xuiC_TabSelectorTab.GetChildControllers<XUiC_GamePrefSelector>("", this.tabPrefsTmp);
				bool flag = false;
				foreach (XUiC_GamePrefSelector xuiC_GamePrefSelector in this.tabPrefsTmp)
				{
					flag |= xuiC_GamePrefSelector.ViewComponent.IsVisible && xuiC_GamePrefSelector.Enabled && !xuiC_GamePrefSelector.IsDefaultValueForGameMode();
				}
				xuiC_TabSelectorTab.TabHighlight = flag;
				this.tabPrefsTmp.Clear();
			}
		}
		if (base.handleDirtyUpdateDefault())
		{
			ThreadManager.RunTaskAfterFrames(new Action(this.sandboxPresetValues.ResetPosition), 3);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Option_OnValueChanged(XUiC_GamePrefSelector _uiElement, EnumGamePrefs _gamePref)
	{
		this.IsDirty = true;
	}

	[XuiXmlAttribute("sandbox_values_columns", false)]
	public int SandboxValuesColumns
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("sandbox_values_template", false)]
	public string SandBoxValuesTemplate
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.sandBoxValuesTemplate;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == null)
			{
				this.sandBoxValuesTemplate = "";
				return;
			}
			this.sandBoxValuesTemplate = value.Replace("%0", "{0}").Replace("%1", "{1}");
		}
	}

	[XuiXmlAttribute("sandbox_values_template_defaults", false)]
	public string SandBoxValuesTemplateDefaults
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.sandBoxValuesTemplateDefaults;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (value == null)
			{
				this.sandBoxValuesTemplateDefaults = "";
				return;
			}
			this.sandBoxValuesTemplateDefaults = value.Replace("%0", "{0}").Replace("%1", "{1}");
		}
	}

	[XuiXmlBinding("sandboxvalues")]
	public Dictionary<string, string> SandboxValues
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
	} = new Dictionary<string, string>();

	[XuiBindEvent("OnPress", "btnSandboxOptions")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSandboxOptions_OnPressed(XUiController _sender, int _mouseButton)
	{
		Action openSandboxSettingsRequested = this.OpenSandboxSettingsRequested;
		if (openSandboxSettingsRequested == null)
		{
			return;
		}
		openSandboxSettingsRequested();
	}

	public void ResetSandboxPresetToDefault()
	{
		this.sandboxPresetSelector.SelectPresetByName("", null, false);
	}

	public void UpdateSandboxPresetGroups()
	{
		string @string = GamePrefs.GetString(EnumGamePrefs.SandboxPreset);
		if (@string == "Custom" || this.sandboxManager.GetPreset(@string) == null)
		{
			this.sandboxManager.LoadOptionsFromCode(GamePrefs.GetString(EnumGamePrefs.SandboxCode), SandboxOptionManager.CustomPreset);
			this.sandboxPresetSelector.SelectPresetByName("Custom", null, false);
		}
		else
		{
			this.sandboxPresetSelector.SelectPresetByName(@string, null, false);
		}
		this.updateSandboxData();
	}

	[XuiBindEvent("SandboxPresetSelectionChanged", "sandboxPresetSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void SandboxPresetSelector_OnValueChanged(SandboxPresetInfo _oldPreset, SandboxPresetInfo _newPreset)
	{
		this.updateSandboxData();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateSandboxData()
	{
		SandboxPresetInfo selectedPreset = this.sandboxPresetSelector.SelectedPreset;
		GamePrefs.Set(EnumGamePrefs.SandboxPreset, selectedPreset.InternalName);
		GamePrefs.Set(EnumGamePrefs.SandboxCode, selectedPreset.SandboxCode);
		if (selectedPreset.IsCustomPreset)
		{
			this.sandboxManager.LoadOptionsFromCode(selectedPreset.SandboxCode);
			this.sandboxDisplay.SandboxCode = selectedPreset.SandboxCode;
		}
		else
		{
			this.sandboxDisplay.SandboxName = selectedPreset.InternalName;
		}
		this.sandboxDisplay.IsDirty = true;
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbBloodMoonFrequency_OnChangeHandler(XUiC_GamePrefSelector _prefSelector, EnumGamePrefs _gamePref)
	{
		this.gameOptions[EnumGamePrefs.BloodMoonRange].Enabled = GamePrefs.GetInt(_gamePref) != 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtPort_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		XUiC_TextInput xuiC_TextInput = (XUiC_TextInput)_sender;
		if (_text.Length < 1)
		{
			xuiC_TextInput.Text = "0";
		}
		else if (_text.Length > 1 && _text[0] == '0')
		{
			xuiC_TextInput.Text = _text.Substring(1);
		}
		this.validatePort(xuiC_TextInput);
		Action settingsChanged = this.SettingsChanged;
		if (settingsChanged == null)
		{
			return;
		}
		settingsChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void validatePort(XUiC_TextInput _txtPort)
	{
		int num;
		this.PortValid = StringParsers.TryParseSInt32(_txtPort.Text, out num) && num >= 1024 && num < 65533;
		if (!this.PortValid)
		{
			_txtPort.ActiveTextColor = Color.red;
		}
	}

	public void WatchForServerEnabledChanges(bool _doWatch)
	{
		if (_doWatch)
		{
			XUiC_GamePrefSelector xuiC_GamePrefSelector = this.serverEnabledSelector;
			xuiC_GamePrefSelector.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Combine(xuiC_GamePrefSelector.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(this.CmbServerEnabled_OnChangeHandler));
			return;
		}
		XUiC_GamePrefSelector xuiC_GamePrefSelector2 = this.serverEnabledSelector;
		xuiC_GamePrefSelector2.OnValueChanged = (Action<XUiC_GamePrefSelector, EnumGamePrefs>)Delegate.Remove(xuiC_GamePrefSelector2.OnValueChanged, new Action<XUiC_GamePrefSelector, EnumGamePrefs>(this.CmbServerEnabled_OnChangeHandler));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CmbServerEnabled_OnChangeHandler(XUiC_GamePrefSelector _prefSelector, EnumGamePrefs _gamePref)
	{
		bool @bool = GamePrefs.GetBool(EnumGamePrefs.ServerEnabled);
		this.RefreshMultiplayerOptionStates(@bool);
		if (@bool && (!PermissionsManager.IsMultiplayerAllowed() || !PermissionsManager.CanHostMultiplayer()))
		{
			XUiC_MultiplayerPrivilegeNotification.ResolvePrivilegesWithDialog(EUserPerms.HostMultiplayer, delegate(bool _result)
			{
				GamePrefs.Set(EnumGamePrefs.ServerEnabled, _result);
				this.serverEnabledSelector.SetCurrentValue();
			}, -1f, false, null);
		}
	}

	public void RefreshMultiplayerOptionStates(bool _enabled)
	{
		IPlatform crossplatformPlatform = PlatformManager.CrossplatformPlatform;
		bool? flag;
		if (crossplatformPlatform == null)
		{
			flag = null;
		}
		else
		{
			IAntiCheatServer antiCheatServer = crossplatformPlatform.AntiCheatServer;
			flag = ((antiCheatServer != null) ? new bool?(antiCheatServer.ServerEacAvailable()) : null);
		}
		bool? flag2 = flag;
		bool flag3 = flag2.GetValueOrDefault() && _enabled;
		IPlatform crossplatformPlatform2 = PlatformManager.CrossplatformPlatform;
		bool? flag4;
		if (crossplatformPlatform2 == null)
		{
			flag4 = null;
		}
		else
		{
			IAntiCheatServer antiCheatServer2 = crossplatformPlatform2.AntiCheatServer;
			flag4 = ((antiCheatServer2 != null) ? new bool?(antiCheatServer2.ServerEacEnabled()) : null);
		}
		flag2 = flag4;
		bool flag5 = flag2.GetValueOrDefault() && _enabled;
		bool flag6 = PermissionsManager.IsCrossplayAllowed() && (flag5 || !Submission.Enabled) && _enabled;
		bool @bool = GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer);
		bool bool2 = GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay);
		bool flag7 = (DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent() || !Submission.Enabled;
		GamePrefs.Set(EnumGamePrefs.ServerEACPeerToPeer, !flag7 || flag5);
		this.gameOptions[EnumGamePrefs.ServerEACPeerToPeer].ViewComponent.IsVisible = flag7;
		this.gameOptions[EnumGamePrefs.ServerEACPeerToPeer].Enabled = flag3;
		if (@bool != GamePrefs.GetBool(EnumGamePrefs.ServerEACPeerToPeer))
		{
			this.gameOptions[EnumGamePrefs.ServerEACPeerToPeer].SetCurrentValue();
		}
		this.gameOptions[EnumGamePrefs.ServerEACPeerToPeer].CheckDefaultValue();
		if (_enabled)
		{
			GamePrefs.Set(EnumGamePrefs.ServerAllowCrossplay, bool2 && flag6);
		}
		this.gameOptions[EnumGamePrefs.ServerAllowCrossplay].Enabled = flag6;
		this.gameOptions[EnumGamePrefs.ServerAllowCrossplay].SetCurrentValue();
		this.gameOptions[EnumGamePrefs.ServerAllowCrossplay].CheckDefaultValue();
		this.gameOptions[EnumGamePrefs.Region].Enabled = _enabled;
		this.gameOptions[EnumGamePrefs.ServerVisibility].Enabled = _enabled;
		this.gameOptions[EnumGamePrefs.ServerPassword].Enabled = _enabled;
		this.gameOptions[EnumGamePrefs.ServerMaxPlayerCount].Enabled = _enabled;
		this.gameOptions[EnumGamePrefs.Region].CheckDefaultValue();
		this.gameOptions[EnumGamePrefs.ServerVisibility].CheckDefaultValue();
		this.gameOptions[EnumGamePrefs.ServerPassword].CheckDefaultValue();
		this.gameOptions[EnumGamePrefs.ServerMaxPlayerCount].CheckDefaultValue();
	}

	public void ApplyMaxPlayerCountOptions()
	{
		if (GamePrefs.GetString(EnumGamePrefs.GameMode) != "GameMode" + EnumGameMode.Survival.ToStringCached<EnumGameMode>() || GameModeSurvival.OverrideMaxPlayerCount < 2)
		{
			return;
		}
		List<string> list = new List<string>();
		for (int i = 2; i <= GameModeSurvival.OverrideMaxPlayerCount; i++)
		{
			list.Add(i.ToString());
		}
		this.gameOptions[EnumGamePrefs.ServerMaxPlayerCount].OverrideValues(list);
	}

	public void ApplyInitialServerEnabledState()
	{
		bool flag = GamePrefs.GetBool(EnumGamePrefs.ServerEnabled) && PermissionsManager.IsMultiplayerAllowed() && PermissionsManager.CanHostMultiplayer();
		GamePrefs.Set(EnumGamePrefs.ServerEnabled, flag);
		this.serverEnabledSelector.SetCurrentValue();
		if (PlatformManager.MultiPlatform.User.UserStatus == EUserStatus.OfflineMode)
		{
			this.serverEnabledSelector.Enabled = false;
		}
		else
		{
			this.WatchForServerEnabledChanges(true);
		}
		this.RefreshMultiplayerOptionStates(flag);
	}

	public void UpdateServerEnabledState()
	{
		this.serverEnabledSelector.SetCurrentValue();
	}

	public void UpdateCrossplayEnabledState()
	{
		this.gameOptions[EnumGamePrefs.ServerAllowCrossplay].SetCurrentValue();
	}

	public void ApplyCreativeModeChangeAllowed(bool _allowed)
	{
		this.gameOptions[EnumGamePrefs.BuildCreate].Enabled = _allowed;
	}

	public void SaveGameOptions(bool _saveAsLastUsed)
	{
		List<EnumGamePrefs> list = new List<EnumGamePrefs>(this.gameOptions.Count);
		this.gameOptions.CopyKeysTo(list);
		list.Add(EnumGamePrefs.SandboxCode);
		list.Add(EnumGamePrefs.SandboxPreset);
		GamePrefs.Instance.Save(GameIO.GetSaveGameDir() + "/gameOptions.sdf", list);
		if (_saveAsLastUsed)
		{
			GamePrefs.Instance.Save(GameIO.GetSaveGameRootDir() + "/newGameOptions.sdf", list);
		}
	}

	public void UpdateOptionVisibilityForGameMode(GameMode _gameMode)
	{
		foreach (KeyValuePair<EnumGamePrefs, XUiC_GamePrefSelector> keyValuePair in this.gameOptions)
		{
			keyValuePair.Value.SetCurrentGameMode(_gameMode);
		}
	}

	public void UpdateOptionValuesFromGamePrefs()
	{
		this.UpdateSandboxPresetGroups();
		foreach (KeyValuePair<EnumGamePrefs, XUiC_GamePrefSelector> keyValuePair in this.gameOptions)
		{
			keyValuePair.Value.SetCurrentValue();
		}
	}

	public void SetGamePrefsToDefaults()
	{
		foreach (KeyValuePair<EnumGamePrefs, XUiC_GamePrefSelector> keyValuePair in this.gameOptions)
		{
			GamePrefs.SetObject(keyValuePair.Key, GamePrefs.GetDefault(keyValuePair.Key));
		}
	}

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TabSelector tabSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumGamePrefs, XUiC_GamePrefSelector> gameOptions = new EnumDictionary<EnumGamePrefs, XUiC_GamePrefSelector>();

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_GamePrefSelector serverEnabledSelector;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool portValid;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<XUiC_GamePrefSelector> tabPrefsTmp = new List<XUiC_GamePrefSelector>();

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SandboxPresetSelector sandboxPresetSelector;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SandboxSettingsDisplay sandboxDisplay;

	[XuiBindComponent("btnSandboxOptions", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSandboxOptions;

	[XuiBindComponent("sandboxPresetValues.", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_ScrollView sandboxPresetValues;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SandboxOptionManager sandboxManager = SandboxOptionManager.Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxValuesTemplate;

	[PublicizedFrom(EAccessModifier.Private)]
	public string sandBoxValuesTemplateDefaults;
}
