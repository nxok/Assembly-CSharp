using System;
using System.Collections.Generic;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SandboxOptions : XUiController
{
	[XuiXmlBinding("gamename")]
	public string GameName
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SandboxOptionManager sandboxOptionManager = this.sandboxManager;
			if (!string.IsNullOrEmpty((sandboxOptionManager != null) ? sandboxOptionManager.GameName : null))
			{
				return this.sandboxManager.GameName;
			}
			return "New World";
		}
	}

	[XuiXmlBinding("worldname")]
	public string WorldName
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			SandboxOptionManager sandboxOptionManager = this.sandboxManager;
			return ((sandboxOptionManager != null) ? sandboxOptionManager.WorldName : null) ?? "";
		}
	}

	[XuiXmlBinding("is_custom")]
	public bool CurrentIsCustom
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.currentPreset.IsCustomPreset;
		}
	}

	[XuiXmlBinding("is_userpreset")]
	public bool CurrentIsUserPreset
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.currentPreset.IsUserPreset;
		}
	}

	[XuiXmlBinding("has_changes")]
	public bool HasChanges
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			foreach (KeyValuePair<global::SandboxOptions.SandboxOptions, XUiC_SandBoxOptionEntry> keyValuePair in this.sandboxOptionDict)
			{
				global::SandboxOptions.SandboxOptions sandboxOptions;
				XUiC_SandBoxOptionEntry xuiC_SandBoxOptionEntry;
				keyValuePair.Deconstruct(out sandboxOptions, out xuiC_SandBoxOptionEntry);
				if (xuiC_SandBoxOptionEntry.IsChanged)
				{
					return true;
				}
			}
			return false;
		}
	}

	[XuiXmlBinding("sandboxcode")]
	public string SandBoxCode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (string.IsNullOrEmpty(this.currentPreset.SandboxCode))
			{
				return "";
			}
			return this.currentPreset.SandboxCode;
		}
	}

	public override void Init()
	{
		base.Init();
		this.setupOptions();
	}

	[XuiBindEvent("OnTabChanged", "tabSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TabSelector_OnTabChanged(int _tabIndex, XUiC_TabSelectorTab _tab)
	{
		this.resetTabHeaders();
	}

	[XuiBindEvent("OnPress", "btnCopyCode")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCopyCode_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.updateCode();
		GUIUtility.systemCopyBuffer = this.currentPreset.SandboxCode;
	}

	[XuiBindEvent("OnPress", "btnPasteCode")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnPasteCode_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.sandboxManager.LoadOptionsFromCode(GUIUtility.systemCopyBuffer, SandboxOptionManager.CustomPreset))
		{
			return;
		}
		this.presetSelector.SelectPresetByName("Custom", null, false);
		this.IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnCopy")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCopy_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.duplicateCurrentSettingsToCustomPreset();
		this.IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnCreatePreset")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnCreatePreset_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.sandboxManager.ResetAllToDefault();
		this.sandboxManager.SaveCurrentToPreset(SandboxOptionManager.CustomPreset);
		this.presetSelector.SelectPresetByName("Custom", null, false);
		this.IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnSavePreset")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (!this.btnSavePreset.ViewComponent.Enabled)
		{
			return;
		}
		this.applyEntryChanges(true);
		if (this.currentPreset.IsCustomPreset)
		{
			SandboxOptionPreset sandboxOptionPreset = this.sandboxManager.SaveCurrentToNewPreset("tempPreset", "", true);
			XUiC_SandboxSettingsSaveAsPreset.Open(this.xui, sandboxOptionPreset.SandboxCode, "", delegate(string _presetName)
			{
				this.presetSelector.SelectPresetByName(_presetName, null, false);
				this.IsDirty = true;
			});
			return;
		}
		SandboxOptionPreset preset = this.sandboxManager.GetPreset(this.currentPreset.InternalName);
		this.sandboxManager.SaveCurrentToPreset(preset);
		this.sandboxManager.SavePreset(preset, true, true);
		this.updateOptionsToPreset(preset);
		this.currentPreset.SandboxCode = preset.SandboxCode;
		this.applyPresetSettings();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyPresetSettings()
	{
		GamePrefs.Set(EnumGamePrefs.SandboxPreset, this.currentPreset.InternalName);
		GamePrefs.Set(EnumGamePrefs.SandboxCode, this.currentPreset.IsCustomPreset ? this.currentPreset.SandboxCode : "");
	}

	[XuiBindEvent("OnPress", "btnDelete")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDelete_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_MessageBoxWindowGroup.ShowCustom(this.xui, Localization.Get("xuiDeleteSandboxPreset", false, null), string.Format(Localization.Get("xuiDeleteSandboxConfirmation", false, null), this.currentPreset.FormattedName), delegate(XUiC_MessageBoxWindowGroup _box)
		{
			_box.Buttons[0].DefaultConfirm("btnConfirm", delegate
			{
				this.sandboxManager.DeletePreset(this.currentPreset.InternalName);
				this.presetSelector.SelectPresetByName("", "User", false);
				this.IsDirty = true;
			}, true, 0f, 1.5f, false);
			_box.Buttons[2].DefaultCancel("xuiCancel", delegate
			{
			}, true, 0f, 0f, false);
		}, false, false, -1);
	}

	[XuiBindEvent("OnPress", "btnDefaults")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnOnPressed(XUiController _sender, int _mouseButton)
	{
		foreach (KeyValuePair<global::SandboxOptions.SandboxOptions, XUiC_SandBoxOptionEntry> keyValuePair in this.sandboxOptionDict)
		{
			global::SandboxOptions.SandboxOptions sandboxOptions;
			XUiC_SandBoxOptionEntry xuiC_SandBoxOptionEntry;
			keyValuePair.Deconstruct(out sandboxOptions, out xuiC_SandBoxOptionEntry);
			XUiC_SandBoxOptionEntry xuiC_SandBoxOptionEntry2 = xuiC_SandBoxOptionEntry;
			if (!(xuiC_SandBoxOptionEntry2.Option.CategoryName != this.tabSelector.SelectedTab.TabKey))
			{
				xuiC_SandBoxOptionEntry2.ResetToDefault(false);
			}
		}
		this.IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnBack")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnBack_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[XuiBindEvent("SandboxPresetSelectionChanged", "presetSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Presets_OnValueChanged(SandboxPresetInfo _oldValue, SandboxPresetInfo _newValue)
	{
		SandboxOptionPreset preset = this.sandboxManager.GetPreset(_newValue.InternalName);
		if (_oldValue.IsCustomPreset)
		{
			this.applyEntryChanges(false);
			this.sandboxManager.SaveCurrentToPreset(SandboxOptionManager.CustomPreset);
		}
		this.updateOptionsToPreset(preset);
		this.currentPreset = _newValue;
		this.updateCode();
		this.applyPresetSettings();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SandboxOptionEntry_OnChangeHandler(XUiC_SandBoxOptionEntry _entry, global::SandboxOptions.SandboxOptions _option)
	{
		if (!this.currentPreset.IsUserPreset && !this.currentPreset.IsCustomPreset)
		{
			XUiC_SandBoxOptionEntry.SandboxOptionValue originalValue = _entry.OriginalValue;
			this.duplicateCurrentSettingsToCustomPreset();
			_entry.ForceOriginalValue(originalValue);
		}
		this.setEntriesDirty();
		this.IsDirty = true;
		this.resetTabHeaders();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void duplicateCurrentSettingsToCustomPreset()
	{
		this.applyEntryChanges(true);
		this.sandboxManager.SaveCurrentToPreset(SandboxOptionManager.CustomPreset);
		this.presetSelector.SelectPresetByName("Custom", null, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCode()
	{
		this.applyEntryChanges(true);
		SandboxOptionPreset sandboxOptionPreset = this.sandboxManager.SaveCurrentToNewPreset("temppreset", "temp", false);
		this.currentPreset.SandboxCode = sandboxOptionPreset.SandboxCode;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setupOptions()
	{
		DictionaryList<string, List<BaseSandboxOption>> optionsByCategory = this.sandboxManager.OptionsByCategory;
		for (int i = 0; i < this.tabs.Length; i++)
		{
			XUiC_TabSelectorTab xuiC_TabSelectorTab = this.tabs[i];
			if (i >= optionsByCategory.Count)
			{
				xuiC_TabSelectorTab.TabVisible = false;
			}
			else
			{
				List<BaseSandboxOption> list = optionsByCategory.list[i];
				if (list.Count != 0)
				{
					string categoryName = list[0].CategoryName;
					xuiC_TabSelectorTab.TabKey = categoryName;
					XUiC_SandBoxOptionEntry[] childControllers = xuiC_TabSelectorTab.GetChildControllers<XUiC_SandBoxOptionEntry>("", null);
					this.tabHelpers.Add(categoryName, new XUiC_SandboxOptions.TabChangeHelper(xuiC_TabSelectorTab, childControllers, "sandboxOptionCategory" + categoryName));
					int num = -1;
					XUiV_Grid xuiV_Grid;
					if (xuiC_TabSelectorTab.TryGetChildView<XUiV_Grid>("", out xuiV_Grid) && xuiV_Grid.Arrangement == null)
					{
						num = xuiV_Grid.Columns;
					}
					if (list.Count > childControllers.Length)
					{
						Log.Error(string.Format("[XUi] More sandbox options ({0}) in category '{1}' than XUiC_SandBoxOptionEntries in respective tab ({2})!", list.Count, categoryName, childControllers.Length));
					}
					int j = 0;
					for (int k = 0; k < list.Count; k++)
					{
						if (j >= childControllers.Length)
						{
							IL_0218:
							while (j < childControllers.Length)
							{
								childControllers[j].setupOption(null);
								j++;
							}
							goto IL_0220;
						}
						BaseSandboxOption baseSandboxOption = list[k];
						if (baseSandboxOption.NewUISection)
						{
							if (num <= 0)
							{
								childControllers[j++].setupSeparator();
							}
							else
							{
								int num2 = j % num;
								if (num2 > 0)
								{
									int num3 = num - num2;
									for (int l = 0; l < num3; l++)
									{
										childControllers[j++].setupSeparator();
									}
								}
								for (int m = 0; m < num; m++)
								{
									childControllers[j++].setupSeparator();
								}
							}
						}
						XUiC_SandBoxOptionEntry xuiC_SandBoxOptionEntry = childControllers[j++];
						XUiC_SandBoxOptionEntry xuiC_SandBoxOptionEntry2 = xuiC_SandBoxOptionEntry;
						xuiC_SandBoxOptionEntry2.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Remove(xuiC_SandBoxOptionEntry2.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(this.SandboxOptionEntry_OnChangeHandler));
						xuiC_SandBoxOptionEntry.setupOption(baseSandboxOption);
						XUiC_SandBoxOptionEntry xuiC_SandBoxOptionEntry3 = xuiC_SandBoxOptionEntry;
						xuiC_SandBoxOptionEntry3.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Combine(xuiC_SandBoxOptionEntry3.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(this.SandboxOptionEntry_OnChangeHandler));
						this.sandboxOptionDict.Add(xuiC_SandBoxOptionEntry.Option.Option, xuiC_SandBoxOptionEntry);
					}
					goto IL_0218;
				}
				xuiC_TabSelectorTab.TabVisible = false;
			}
			IL_0220:;
		}
		this.resetTabHeaders();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void applyEntryChanges(bool _forceChange)
	{
		foreach (XUiC_SandBoxOptionEntry xuiC_SandBoxOptionEntry in this.sandboxOptionDict.Values)
		{
			xuiC_SandBoxOptionEntry.ApplyChange(_forceChange);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void setEntriesDirty()
	{
		foreach (XUiC_SandBoxOptionEntry xuiC_SandBoxOptionEntry in this.sandboxOptionDict.Values)
		{
			xuiC_SandBoxOptionEntry.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateOptionsToPreset(SandboxOptionPreset _preset)
	{
		foreach (XUiC_SandBoxOptionEntry xuiC_SandBoxOptionEntry in this.sandboxOptionDict.Values)
		{
			xuiC_SandBoxOptionEntry.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Remove(xuiC_SandBoxOptionEntry.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(this.SandboxOptionEntry_OnChangeHandler));
			xuiC_SandBoxOptionEntry.ResetToDefault(true);
			xuiC_SandBoxOptionEntry.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Combine(xuiC_SandBoxOptionEntry.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(this.SandboxOptionEntry_OnChangeHandler));
		}
		foreach (global::SandboxOptions.SandboxOptions sandboxOptions in _preset.PresetValues.Keys)
		{
			XUiC_SandBoxOptionEntry xuiC_SandBoxOptionEntry2 = this.sandboxOptionDict[sandboxOptions];
			xuiC_SandBoxOptionEntry2.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Remove(xuiC_SandBoxOptionEntry2.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(this.SandboxOptionEntry_OnChangeHandler));
			xuiC_SandBoxOptionEntry2.SetComboBoxIndex(_preset.PresetValues[sandboxOptions]);
			xuiC_SandBoxOptionEntry2.ApplyChange(true);
			xuiC_SandBoxOptionEntry2.OnValueChanged = (Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>)Delegate.Combine(xuiC_SandBoxOptionEntry2.OnValueChanged, new Action<XUiC_SandBoxOptionEntry, global::SandboxOptions.SandboxOptions>(this.SandboxOptionEntry_OnChangeHandler));
		}
		this.resetTabHeaders();
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetTabHeaders()
	{
		foreach (XUiC_SandboxOptions.TabChangeHelper tabChangeHelper in this.tabHelpers.Values)
		{
			tabChangeHelper.UpdateTab();
		}
	}

	public override void OnOpen()
	{
		this.windowGroup.isEscClosable = false;
		string @string = GamePrefs.GetString(EnumGamePrefs.SandboxPreset);
		base.OnOpen();
		this.presetSelector.SelectPresetByName(@string, null, true);
		this.resetTabHeaders();
		this.tabSelector.SelectedTabIndex = 0;
	}

	public override void OnClose()
	{
		base.OnClose();
		Action action = this.onClose;
		if (action != null)
		{
			action();
		}
		this.onClose = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent, false))
		{
			if (this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				this.BtnBack_OnPressed(this, -1);
			}
			if (this.xui.playerUI.playerInput.GUIActions.Apply.WasReleased)
			{
				this.BtnSave_OnPressed(null, 0);
			}
		}
		if (this.IsDirty)
		{
			this.updateCode();
		}
		base.handleDirtyUpdateDefault();
	}

	public static void Open(XUi _xui, Action _onClose)
	{
		XUiC_SandboxOptions childByType = _xui.GetChildByType<XUiC_SandboxOptions>();
		childByType.onClose = _onClose;
		_xui.playerUI.windowManager.Open(childByType.windowGroup, false);
	}

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TabSelector tabSelector;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TabSelectorTab[] tabs;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SandboxPresetSelector presetSelector;

	[XuiBindComponent("btnSave", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnSavePreset;

	[XuiBindComponent("btnCopyCode", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCopyCode;

	[XuiBindComponent("btnCopy", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCopy;

	[XuiBindComponent("btnCreatePreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnCreatePreset;

	[XuiBindComponent("btnDelete", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDelete;

	[XuiBindComponent("btnPasteCode", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnPasteCode;

	[XuiBindComponent("btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent("btnDefaults", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDefaults;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<global::SandboxOptions.SandboxOptions, XUiC_SandBoxOptionEntry> sandboxOptionDict = new Dictionary<global::SandboxOptions.SandboxOptions, XUiC_SandBoxOptionEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly SandboxOptionManager sandboxManager = SandboxOptionManager.Current;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<string, XUiC_SandboxOptions.TabChangeHelper> tabHelpers = new Dictionary<string, XUiC_SandboxOptions.TabChangeHelper>();

	[PublicizedFrom(EAccessModifier.Private)]
	public SandboxPresetInfo currentPreset;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action onClose;

	[PublicizedFrom(EAccessModifier.Private)]
	public class TabChangeHelper
	{
		public TabChangeHelper(XUiC_TabSelectorTab _tab, XUiC_SandBoxOptionEntry[] _tabOptions, string _localizationKey)
		{
			this.tab = _tab;
			this.tabOptions = _tabOptions;
			this.localizationKey = _localizationKey;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasAnyNonDefault()
		{
			for (int i = 0; i < this.tabOptions.Length; i++)
			{
				if (!this.tabOptions[i].IsDefaultValue())
				{
					return true;
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool hasAnyUnsavedChange()
		{
			for (int i = 0; i < this.tabOptions.Length; i++)
			{
				if (this.tabOptions[i].IsChanged)
				{
					return true;
				}
			}
			return false;
		}

		public void UpdateTab()
		{
			this.tab.TabHighlight = this.hasAnyNonDefault();
			this.tab.TabHeaderText = Localization.Get(this.localizationKey, false, null) + (this.hasAnyUnsavedChange() ? "[FF]" : "[00]") + "*[FF]";
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_TabSelectorTab tab;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_SandBoxOptionEntry[] tabOptions;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string localizationKey;
	}
}
