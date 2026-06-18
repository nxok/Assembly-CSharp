using System;
using System.Collections.Generic;
using InControl;

public abstract class XUiC_OptionsDialogBase : XUiController, IXUiWindowConditionalClosing
{
	[XuiXmlBinding("unsaved_changes")]
	public bool UnsavedChanges
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return this.unsavedChanges;
		}
		[PublicizedFrom(EAccessModifier.Protected)]
		set
		{
			this.unsavedChanges = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("support_defaults")]
	public virtual bool SupportsDefaults
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	[XuiXmlBinding("allow_leaving_window")]
	public virtual bool AllowLeavingWindow
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	[XuiXmlBinding("hovered_custom_attributes")]
	[PublicizedFrom(EAccessModifier.Private)]
	public ObservableDictionary<string, object> bindingCustomAttributes()
	{
		XUiC_OptionEntryAbs xuiC_OptionEntryAbs = this.HoveredOption;
		return ((xuiC_OptionEntryAbs != null) ? xuiC_OptionEntryAbs.CustomAttributes : null) ?? this.CustomAttributes;
	}

	public XUiC_OptionEntryAbs HoveredOption
	{
		get
		{
			return this.hoveredOption;
		}
		set
		{
			if (this.hoveredOption == value)
			{
				return;
			}
			this.hoveredOption = value;
			this.IsDirty = true;
		}
	}

	public override void Init()
	{
		base.Init();
		GamePrefs.OnGamePrefChanged += this.OnGamePrefChanged;
		if (this.TabSelector != null)
		{
			foreach (XUiC_TabSelectorTab xuiC_TabSelectorTab in this.TabSelector.Tabs)
			{
				this.optionsByTab.Add(xuiC_TabSelectorTab, xuiC_TabSelectorTab.GetChildControllers<XUiC_OptionEntryAbs>("", null));
			}
		}
	}

	public override void Cleanup()
	{
		base.Cleanup();
		GamePrefs.OnGamePrefChanged -= this.OnGamePrefChanged;
	}

	public override void OnOpen()
	{
		this.unsavedChanges = false;
		base.OnOpen();
		XUi.InGameMenuOpen = true;
		XUiC_OptionsMenuNew.ParentSelector.SetSelected(this.windowGroup.Id);
		this.xui.playerUI.windowManager.Open(XUiC_OptionsMenuNew.ParentSelector.WindowGroup, false);
		this.windowGroup.isEscClosable = false;
	}

	public override void OnClose()
	{
		base.OnClose();
		XUi.InGameMenuOpen = false;
		this.xui.playerUI.windowManager.Close(XUiC_OptionsMenuNew.ParentSelector.WindowGroup, false);
		if (GameStats.GetInt(EnumGameStats.GameState) == 0)
		{
			this.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		if (this.IsDirty)
		{
			this.onDirtyUpdate();
			base.RefreshBindings();
			this.IsDirty = false;
		}
		if (XUiUtils.HotkeysAllowedFor(this.viewComponent, false))
		{
			if (this.xui.playerUI.playerInput.PermanentActions.Cancel.WasReleased)
			{
				this.TryClose(null, null);
			}
			if (this.UnsavedChanges && this.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
			{
				this.saveChanges();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void onDirtyUpdate()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnGamePrefChanged(EnumGamePrefs _pref)
	{
	}

	public virtual void TryClose(Action _onClosed, Action _onCancelled)
	{
		if (this.unsavedChanges)
		{
			Action <>9__1;
			Action <>9__2;
			Action <>9__3;
			XUiC_MessageBoxWindowGroup.ShowCustom(this.xui, Localization.Get("xuiUnsavedChangesTitle", false, null), Localization.Get("xuiOptionsUnsavedChangesText", false, null), delegate(XUiC_MessageBoxWindowGroup _box)
			{
				XUiC_MessageBoxWindowGroup.MessageButton messageButton = _box.Buttons[0];
				string text = "xuiDiscard";
				Action action;
				if ((action = <>9__1) == null)
				{
					action = (<>9__1 = delegate
					{
						this.closeOptions();
						Action onClosed2 = _onClosed;
						if (onClosed2 == null)
						{
							return;
						}
						onClosed2();
					});
				}
				messageButton.DefaultConfirm(text, action, true, 0f, 0f, false);
				XUiC_MessageBoxWindowGroup.MessageButton messageButton2 = _box.Buttons[1];
				string text2 = "xuiSaveAndClose";
				PlayerAction halfStack = this.xui.playerUI.playerInput.GUIActions.HalfStack;
				Action action2;
				if ((action2 = <>9__2) == null)
				{
					action2 = (<>9__2 = delegate
					{
						this.saveChanges();
						this.closeOptions();
						Action onClosed3 = _onClosed;
						if (onClosed3 == null)
						{
							return;
						}
						onClosed3();
					});
				}
				messageButton2.Set(text2, halfStack, action2, true, 0f, 0f, false);
				XUiC_MessageBoxWindowGroup.MessageButton messageButton3 = _box.Buttons[2];
				string text3 = "xuiCancel";
				Action action3;
				if ((action3 = <>9__3) == null)
				{
					action3 = (<>9__3 = delegate
					{
						Action onCancelled = _onCancelled;
						if (onCancelled == null)
						{
							return;
						}
						onCancelled();
					});
				}
				messageButton3.DefaultCancel(text3, action3, true, 0f, 0f, false);
			}, false, false, -1);
			return;
		}
		this.closeOptions();
		Action onClosed = _onClosed;
		if (onClosed == null)
		{
			return;
		}
		onClosed();
	}

	[XuiBindEvent("ValueChanged", "AllOptions")]
	public void Event_SettingChanged(XUiC_OptionEntryAbs _sender)
	{
		XUiC_OptionEntryAbs.DebugLog("SETTING CHANGED: " + _sender.GetXuiHierarchy());
		this.updateUnsavedState();
		base.RefreshBindingsSelfAndChildren();
	}

	[XuiBindEvent("OnTabChanged", "TabSelector")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnTabChanged(int _tabIndex, XUiC_TabSelectorTab _tab)
	{
		this.IsDirty = true;
	}

	[XuiBindEvent("OnPress", "btnApply")]
	public void Event_ApplyOnPress(XUiController _sender, int _mouseButton)
	{
		this.saveChanges();
	}

	[XuiBindEvent("OnPress", "btnDefaults")]
	public void Event_DefaultsOnPress(XUiController _sender, int _mouseButton)
	{
		this.resetToDefaults();
	}

	[XuiBindEvent("OnPress", "btnBack")]
	public void Event_BackOnPress(XUiController _sender, int _mouseButton)
	{
		this.TryClose(null, null);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateUnsavedState()
	{
		bool flag = false;
		XUiC_OptionEntryAbs[] array = this.AllOptions;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].IsChanged)
			{
				flag = true;
				break;
			}
		}
		this.UnsavedChanges = flag;
		if (this.TabSelector == null)
		{
			return;
		}
		foreach (KeyValuePair<XUiC_TabSelectorTab, XUiC_OptionEntryAbs[]> keyValuePair in this.optionsByTab)
		{
			XUiC_TabSelectorTab xuiC_TabSelectorTab;
			keyValuePair.Deconstruct(out xuiC_TabSelectorTab, out array);
			XUiC_TabSelectorTab xuiC_TabSelectorTab2 = xuiC_TabSelectorTab;
			XUiC_OptionEntryAbs[] array2 = array;
			bool flag2 = false;
			array = array2;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].IsChanged)
				{
					flag2 = true;
					break;
				}
			}
			xuiC_TabSelectorTab2.TabHighlight = flag2;
		}
	}

	public void SetChanged()
	{
		this.UnsavedChanges = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void closeOptions()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveChanges()
	{
		Log.Out("Save options " + base.GetType().Name);
		XUiC_OptionEntryAbs[] allOptions = this.AllOptions;
		for (int i = 0; i < allOptions.Length; i++)
		{
			allOptions[i].ApplySelection();
		}
		this.doSaveChangesInternal();
		GamePrefs.Instance.Save();
		this.afterChangesSaved();
		this.updateUnsavedState();
		if (this.unsavedChanges)
		{
			Log.Warning("UnsavedChanges after saving settings!");
		}
		this.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void doSaveChangesInternal()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void afterChangesSaved()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void discardChanges()
	{
		Log.Out("Discard changes " + base.GetType().Name);
		XUiC_OptionEntryAbs[] allOptions = this.AllOptions;
		for (int i = 0; i < allOptions.Length; i++)
		{
			allOptions[i].DiscardCurrentChange();
		}
		this.doDiscardChangesInternal();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void doDiscardChangesInternal()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void resetToDefaults()
	{
		Log.Out("Reset to defaults " + base.GetType().Name);
		foreach (XUiC_OptionEntryAbs xuiC_OptionEntryAbs in this.AllOptions)
		{
			if (xuiC_OptionEntryAbs.ViewComponent.IsActiveInHierarchy)
			{
				xuiC_OptionEntryAbs.ResetToDefault();
			}
		}
		this.doResetToDefaultsInternal();
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void doResetToDefaultsInternal()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public XUiC_OptionsDialogBase()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<XUiC_TabSelectorTab, XUiC_OptionEntryAbs[]> optionsByTab = new Dictionary<XUiC_TabSelectorTab, XUiC_OptionEntryAbs[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public bool unsavedChanges;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_TabSelector TabSelector;

	[XuiBindComponent("buttons.btnApply", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnApply;

	[XuiBindComponent("buttons.btnDefaults", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDefaults;

	[XuiBindComponent("buttons.btnBack", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnBack;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Protected)]
	public readonly XUiC_OptionEntryAbs[] AllOptions;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionEntryAbs hoveredOption;
}
