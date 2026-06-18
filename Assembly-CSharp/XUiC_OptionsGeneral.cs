using System;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsGeneral : XUiC_OptionsDialogBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void initLanguageOptions()
	{
		if (this.optionLanguage == null || this.comboLanguage == null)
		{
			return;
		}
		this.optionLanguage.GetSettingValue = delegate
		{
			this.comboLanguage.Elements.Clear();
			bool flag = false;
			foreach (string text in Localization.KnownLanguages)
			{
				if ((flag || (flag = text.EqualsCaseInsensitive("english"))) && text.IndexOf(' ') < 0)
				{
					this.comboLanguage.Elements.Add(new XUiC_OptionsGeneral.LanguageInfo(text));
				}
			}
			this.comboLanguage.Elements.Sort();
			this.comboLanguage.Elements.Insert(0, new XUiC_OptionsGeneral.LanguageInfo(""));
			string @string = GamePrefs.GetString(EnumGamePrefs.Language);
			for (int j = 0; j < this.comboLanguage.Elements.Count; j++)
			{
				if (this.comboLanguage.Elements[j].LanguageKey.EqualsCaseInsensitive(@string))
				{
					this.comboLanguage.SelectedIndex = j;
				}
			}
		};
		this.optionLanguage.DiscardChanges = delegate
		{
			string string2 = GamePrefs.GetString(EnumGamePrefs.Language);
			for (int k = 0; k < this.comboLanguage.Elements.Count; k++)
			{
				if (this.comboLanguage.Elements[k].LanguageKey.EqualsCaseInsensitive(string2))
				{
					this.comboLanguage.SelectedIndex = k;
				}
			}
		};
		this.optionLanguage.ApplyChanges = delegate(bool _)
		{
			this.languageChanged = this.comboLanguage.Value.LanguageKey != GamePrefs.GetString(EnumGamePrefs.Language);
			this.IsDirty |= this.languageChanged;
			GamePrefs.Set(EnumGamePrefs.Language, this.comboLanguage.Value.LanguageKey);
		};
		this.optionLanguage.ResetDefaults = delegate
		{
			this.comboLanguage.SelectedIndex = 0;
		};
		this.optionLanguage.IsChangedDelegate = () => this.comboLanguage.Value.LanguageKey != GamePrefs.GetString(EnumGamePrefs.Language);
		this.optionLanguage.IsDefaultDelegate = () => this.comboLanguage.Value.LanguageKey.EqualsCaseInsensitive((string)GamePrefs.GetDefault(EnumGamePrefs.Language));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void languageSavedOptions()
	{
		if (!this.languageChanged)
		{
			return;
		}
		this.languageChanged = false;
		Log.Out("Language selection changed: " + GamePrefs.GetString(EnumGamePrefs.Language));
		if ((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent())
		{
			XUiC_MessageBoxWindowGroup.ShowOkCancel(this.xui, Localization.Get("xuiConfirmRestartLanguageTitle", false, null), Localization.Get("xuiConfirmRestartLanguageText", false, null), delegate
			{
				Utils.RestartGame(Utils.ERestartAntiCheatMode.KeepAntiCheatMode);
			}, delegate
			{
				this.xui.playerUI.windowManager.Open(this.windowGroup, true);
			}, false, true, false);
			return;
		}
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			XUiC_MessageBoxWindowGroup.ShowOk(this.xui, Localization.Get("xuiLanguageChangedTitle", false, null), Localization.Get("xuiLanguageChangedText", false, null), delegate
			{
				this.xui.playerUI.windowManager.Open(this.windowGroup, true);
			}, false, true, false);
		}
	}

	[XuiXmlBinding("crosshairs_enabled")]
	public bool CrosshairsEnabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GamePrefs.GetBool(EnumGamePrefs.OptionsCrosshairEnabled);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initCrosshairOptions()
	{
		this.comboCrosshairsColor.Min = 0L;
		this.comboCrosshairsColor.Max = (long)(EntityPlayerLocal.crosshairColors.Count - 1);
		this.crosshairDrawer = this.viewComponent.UiTransform.gameObject.AddComponent<CrosshairDrawer>();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref - EnumGamePrefs.OptionsCrosshairEnabled <= 3 || _pref - EnumGamePrefs.OptionsCrosshairThickness <= 1)
		{
			this.updateCrosshairElements();
		}
	}

	[XuiBindEvent("OnTabChanged", "TabSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public new void OnTabChanged(int _tabIndex, XUiC_TabSelectorTab _tab)
	{
		this.updateCrosshairElements();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateRangedCrosshairDrwaing()
	{
		bool @bool = GamePrefs.GetBool(EnumGamePrefs.OptionsCrosshairEnabled);
		bool bool2 = GamePrefs.GetBool(EnumGamePrefs.OptionsCrosshairRangedEnabled);
		this.crosshairDrawer.draw = this.TabSelector.SelectedTabIndex == 1 && @bool && bool2 && !XUiC_FullScreenCollider.IsBlocked(this.viewComponent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCrosshairPosition()
	{
		Vector3 vector = this.xui.playerUI.uiCamera.cachedCamera.WorldToViewportPoint(this.crosshairRangedSample.UiTransform.position);
		vector.x *= (float)Screen.width;
		vector.y *= (float)Screen.height;
		vector.y = (float)Screen.height - vector.y;
		this.crosshairDrawer.centerX = vector.x;
		this.crosshairDrawer.centerY = vector.y;
		this.updateRangedCrosshairDrwaing();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCrosshairElements()
	{
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsCrosshairScale);
		float float2 = GamePrefs.GetFloat(EnumGamePrefs.OptionsCrosshairThickness);
		float float3 = GamePrefs.GetFloat(EnumGamePrefs.OptionsCrosshairOpacity);
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsCrosshairColor);
		Color color = EntityPlayerLocal.crosshairColors[@int];
		color.a = float3;
		this.crosshairSample.Color = color;
		this.crosshairSample.Size = new Vector2i((int)(@float * 100f), (int)(@float * 100f));
		this.updateRangedCrosshairDrwaing();
		if (!this.crosshairDrawer.draw)
		{
			return;
		}
		this.crosshairDrawer.thickness = float2;
		this.crosshairDrawer.length = @float * (float)Screen.height * 0.025f;
		this.crosshairDrawer.openAreaX = (this.crosshairDrawer.openAreaY = (float)Screen.height * 0.02f);
		this.crosshairDrawer.opacity = float3;
		this.crosshairDrawer.color = color;
		base.RefreshBindings();
	}

	public static EUserPerms UserPermissions
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PermissionsManager.GetPermissions(PermissionsManager.PermissionSources.Platform | PermissionsManager.PermissionSources.LaunchPrefs | PermissionsManager.PermissionSources.DebugMask | PermissionsManager.PermissionSources.TitleStorage);
		}
	}

	[XuiXmlBinding("has_communication_perm")]
	public bool HasCommunicationPermission
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_OptionsGeneral.UserPermissions.HasCommunication();
		}
	}

	[XuiXmlBinding("has_crossplay_perm")]
	public bool HasCrossplayPermission
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_OptionsGeneral.UserPermissions.HasCrossplay();
		}
	}

	[XuiXmlBinding("crossplayTooltip")]
	public string CrossplayTooltip
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return PermissionsManager.GetPermissionDenyReason(EUserPerms.Crossplay, PermissionsManager.PermissionSources.Platform | PermissionsManager.PermissionSources.LaunchPrefs | PermissionsManager.PermissionSources.DebugMask | PermissionsManager.PermissionSources.TitleStorage) ?? Localization.Get("xuiOptionsGeneralCrossplayTooltip", false, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initPermissionsBasedOptions()
	{
		if (this.optionChatComms != null)
		{
			this.optionChatComms.OverrideSettingValue = delegate(bool _, out bool _overrideValue)
			{
				_overrideValue = false;
				return !XUiC_OptionsGeneral.UserPermissions.HasCommunication();
			};
			this.optionChatComms.DoSaveOverride = () => XUiC_OptionsGeneral.UserPermissions.HasCommunication();
		}
		if (this.optionCrossplay != null)
		{
			this.optionCrossplay.OverrideSettingValue = delegate(bool _, out bool _overrideValue)
			{
				_overrideValue = false;
				return !XUiC_OptionsGeneral.UserPermissions.HasCrossplay();
			};
			this.optionCrossplay.DoSaveOverride = () => XUiC_OptionsGeneral.UserPermissions.HasCrossplay();
		}
	}

	[XuiXmlBinding("bug_reporting")]
	public bool SupportsBugReporting
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return BacktraceUtils.BugReportFeature;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_OptionsGeneral.ID = base.WindowGroup.Id;
		this.initLanguageOptions();
		this.initCrosshairOptions();
		this.initPermissionsBasedOptions();
	}

	[XuiBindEvent("OnPress", "btnEula")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnEula_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_EulaWindow.Open(this.xui, GameManager.HasAcceptedLatestEula());
	}

	[XuiBindEvent("OnPress", "btnBugReport")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBugReport_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_BugReportWindow.Open();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void afterChangesSaved()
	{
		base.afterChangesSaved();
		this.languageSavedOptions();
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.updateCrosshairPosition();
	}

	public override void OnOpen()
	{
		this.updateCrosshairElements();
		base.OnOpen();
	}

	[XuiBindComponent("OptionLanguage", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionLanguage;

	[XuiBindComponent("Language", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_OptionsGeneral.LanguageInfo> comboLanguage;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool languageChanged;

	[XuiBindComponent("CrosshairsColor", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxInt comboCrosshairsColor;

	[XuiBindComponent("CrosshairsSample", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiV_Sprite crosshairSample;

	[XuiBindComponent("rangedCrosshairSample", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView crosshairRangedSample;

	[PublicizedFrom(EAccessModifier.Private)]
	public CrosshairDrawer crosshairDrawer;

	[XuiBindComponent("OptionChatComms", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefBool optionChatComms;

	[XuiBindComponent("OptionCrossplay", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefBool optionCrossplay;

	public static string ID = "";

	[XuiBindComponent("btnEula", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnEula;

	[XuiBindComponent("btnBugReport", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBugReport;

	public readonly struct LanguageInfo : IComparable<XUiC_OptionsGeneral.LanguageInfo>, IEquatable<XUiC_OptionsGeneral.LanguageInfo>, IComparable
	{
		public LanguageInfo(string _languageKey)
		{
			this.LanguageKey = _languageKey;
			if (_languageKey == "")
			{
				this.NameEnglish = null;
				this.NameNative = null;
				return;
			}
			this.NameEnglish = Localization.Get("languageNameEnglish", false, _languageKey);
			this.NameNative = Localization.Get("languageNameNative", false, _languageKey);
		}

		public override string ToString()
		{
			if (!(this.LanguageKey == ""))
			{
				return this.NameEnglish + " / " + this.NameNative;
			}
			return "-Auto-";
		}

		public bool Equals(XUiC_OptionsGeneral.LanguageInfo _other)
		{
			return this.LanguageKey == _other.LanguageKey;
		}

		public override bool Equals(object _obj)
		{
			if (_obj is XUiC_OptionsGeneral.LanguageInfo)
			{
				XUiC_OptionsGeneral.LanguageInfo languageInfo = (XUiC_OptionsGeneral.LanguageInfo)_obj;
				return this.Equals(languageInfo);
			}
			return false;
		}

		public override int GetHashCode()
		{
			if (this.LanguageKey == null)
			{
				return 0;
			}
			return this.LanguageKey.GetHashCode();
		}

		public int CompareTo(XUiC_OptionsGeneral.LanguageInfo _other)
		{
			return string.Compare(this.NameEnglish, _other.NameEnglish, StringComparison.OrdinalIgnoreCase);
		}

		public int CompareTo(object _obj)
		{
			if (_obj == null)
			{
				return 1;
			}
			if (_obj is XUiC_OptionsGeneral.LanguageInfo)
			{
				XUiC_OptionsGeneral.LanguageInfo languageInfo = (XUiC_OptionsGeneral.LanguageInfo)_obj;
				return this.CompareTo(languageInfo);
			}
			throw new ArgumentException("Object must be of type LanguageInfo");
		}

		public readonly string NameEnglish;

		public readonly string NameNative;

		public readonly string LanguageKey;
	}
}
