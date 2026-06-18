using System;
using System.Collections;
using Platform;
using SandboxOptions;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ContinueGame : XUiC_NewContinueBase
{
	public override bool PlayIntroMovie
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return false;
		}
	}

	public override bool AllowChangingCreativeMode
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return !(DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent();
		}
	}

	[XuiXmlBinding("iscontinuegame")]
	public bool IsContinueGame
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return true;
		}
	}

	[XuiXmlBinding("saveselected")]
	public bool SaveSelected
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_SavegamesList xuiC_SavegamesList = this.savesList;
			return ((xuiC_SavegamesList != null) ? xuiC_SavegamesList.SelectedEntryData : null) != null;
		}
	}

	[XuiXmlBinding("saveplayable")]
	public bool SavePlayable
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_SavegamesList xuiC_SavegamesList = this.savesList;
			bool? flag;
			if (xuiC_SavegamesList == null)
			{
				flag = null;
			}
			else
			{
				XUiC_SavegamesList.ListEntry selectedEntryData = xuiC_SavegamesList.SelectedEntryData;
				flag = ((selectedEntryData != null) ? new bool?(selectedEntryData.Playable) : null);
			}
			bool? flag2 = flag;
			return flag2.GetValueOrDefault();
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_ContinueGame.ID = base.WindowGroup.Id;
	}

	public override void OnOpen()
	{
		this.IsDirty = true;
		this.Settings.WatchForServerEnabledChanges(false);
		base.OnOpen();
		if (this.savesList.EntryCount > 0)
		{
			this.savesList.SelectedEntryIndex = 0;
			this.savesList.SelectCursorElementForSelectedEntry();
			this.validateStartable();
		}
		else
		{
			string text = GamePrefs.GetString(EnumGamePrefs.GameMode);
			if (string.IsNullOrEmpty(text))
			{
				text = (string)GamePrefs.GetDefault(EnumGamePrefs.GameMode);
			}
			GameMode gameMode = GameMode.GetGameModeForName(text);
			if (gameMode == null)
			{
				text = (string)GamePrefs.GetDefault(EnumGamePrefs.GameMode);
				gameMode = GameMode.GetGameModeForName(text);
			}
			this.gameModeChanged(gameMode);
			this.validateStartable();
			base.GetChildById("btnBack").SelectCursorElement(true, false);
			this.updateBarSelectionValues();
		}
		this.Settings.ApplyInitialServerEnabledState();
		if (base.DataManagementBarEnabled)
		{
			SaveInfoProvider.Instance.SetDirty();
			this.DataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			this.DataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
			base.updateBarUsageAndAllowanceValues();
		}
		GamePrefs.SetObject(EnumGamePrefs.ResetUnprotectedChunks, GamePrefs.GetDefault(EnumGamePrefs.ResetUnprotectedChunks));
	}

	[XuiBindEvent("ListEntryDoubleClicked", "savesList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_OnEntryDoubleClicked(XUiC_List<XUiC_SavegamesList.ListEntry> _list, XUiC_SavegamesList.ListEntry _entry)
	{
		if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard && this.savesList.SelectedEntryIndex >= 0)
		{
			base.BtnStart_OnPressed(this, -1);
		}
	}

	[XuiBindEvent("SelectionChanged", "savesList")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void SavesList_OnSelectionChanged(XUiC_List<XUiC_SavegamesList.ListEntry> _list, XUiC_SavegamesList.ListEntry _previousEntry, XUiC_SavegamesList.ListEntry _newEntry)
	{
		this.IsDirty = true;
		if (_newEntry != null)
		{
			this.Settings.SetGamePrefsToDefaults();
			GamePrefs.Instance.Load(_newEntry.GetSaveDir() + "/gameOptions.sdf");
			GamePrefs.SetObject(EnumGamePrefs.ResetUnprotectedChunks, GamePrefs.GetDefault(EnumGamePrefs.ResetUnprotectedChunks));
			this.gameModeChanged(_newEntry.GameMode);
			GamePrefs.Set(EnumGamePrefs.GameName, _newEntry.SaveName);
			GamePrefs.Set(EnumGamePrefs.GameWorld, _newEntry.WorldName);
			GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, (int)_newEntry.StorageType);
			this.Settings.RefreshMultiplayerOptionStates(GamePrefs.GetBool(EnumGamePrefs.ServerEnabled));
		}
		this.updateBarSelectionValues();
		this.validateStartable();
	}

	[XuiBindEvent("OnPress", "btnDeleteSave")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.deleteSaveWithConfirmation();
	}

	[XuiBindEvent("OnHover", "btnDeleteSave")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDeleteSave_OnHover(XUiController _sender, bool _isOver)
	{
		if (base.DataManagementBarEnabled)
		{
			this.DataManagementBar.SetDeleteHovered(_isOver);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void deleteSaveWithConfirmation()
	{
		XUiC_SavegamesList.ListEntry saveEntry = this.savesList.SelectedEntryData;
		Action <>9__1;
		Action <>9__2;
		XUiC_MessageBoxWindowGroup.ShowCustom(this.xui, Localization.Get("xuiDeleteSaveGame", false, null), string.Format(Localization.Get("xuiSavegameDeleteConfirmation", false, null), saveEntry.SaveName), delegate(XUiC_MessageBoxWindowGroup _box)
		{
			XUiC_MessageBoxWindowGroup.MessageButton messageButton = _box.Buttons[0];
			string text = "btnConfirm";
			Action action;
			if ((action = <>9__1) == null)
			{
				action = (<>9__1 = delegate
				{
					this.btnDeleteSave.SelectCursorElement(false, false);
					string saveDir = saveEntry.GetSaveDir();
					if (SdDirectory.Exists(saveDir))
					{
						SdDirectory.Delete(saveDir, true);
						SaveDataUtils.SaveDataManager.CommitSync();
						SaveInfoProvider.Instance.SetDirty();
						this.updateBarUsageAndAllowanceValues();
					}
					this.savesList.RebuildList(false);
				});
			}
			messageButton.DefaultConfirm(text, action, true, 0f, 1.5f, false);
			XUiC_MessageBoxWindowGroup.MessageButton messageButton2 = _box.Buttons[2];
			string text2 = "xuiCancel";
			Action action2;
			if ((action2 = <>9__2) == null)
			{
				action2 = (<>9__2 = delegate
				{
					this.btnDeleteSave.SelectCursorElement(false, false);
				});
			}
			messageButton2.DefaultCancel(text2, action2, true, 0f, 0f, false);
		}, false, false, -1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnOpenSandboxSettingsRequested()
	{
		XUiC_SavegamesList.ListEntry selectedEntryData = this.savesList.SelectedEntryData;
		SandboxOptionManager.Current.SetWorldAndGame(selectedEntryData.WorldName, selectedEntryData.SaveName);
		base.OnOpenSandboxSettingsRequested();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDataManagementWindowClosed()
	{
		base.updateBarUsageAndAllowanceValues();
		if (base.DataManagementBarEnabled)
		{
			this.DataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Selection);
			this.DataManagementBar.SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion.None);
		}
		XUiC_SavegamesList.ListEntry selectedEntryData = this.savesList.SelectedEntryData;
		string text = null;
		string text2 = null;
		UserDataStorageType userDataStorageType = UserDataStorageType.DeviceLocal;
		if (selectedEntryData != null)
		{
			text = selectedEntryData.WorldName;
			text2 = selectedEntryData.SaveName;
			userDataStorageType = selectedEntryData.StorageType;
		}
		this.savesList.RebuildList(false);
		this.savesList.SelectEntry(text, text2, userDataStorageType);
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBarSelectionValues()
	{
		if (!base.DataManagementBarEnabled)
		{
			return;
		}
		XUiC_SavegamesList.ListEntry selectedEntryData = this.savesList.SelectedEntryData;
		SaveInfoProvider.SaveEntryInfo saveEntryInfo;
		if (selectedEntryData != null && SaveInfoProvider.Instance.TryGetLocalSaveEntry(selectedEntryData.WorldName, selectedEntryData.SaveName, selectedEntryData.StorageType, out saveEntryInfo) && selectedEntryData.StorageType.UsesDataLimit())
		{
			XUiC_DataManagementBar.BarRegion barRegion = new XUiC_DataManagementBar.BarRegion(saveEntryInfo.BarStartOffset, saveEntryInfo.SizeInfo.ReportedSize);
			this.DataManagementBar.SetSelectedByteRegion(barRegion);
			this.DataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
			return;
		}
		this.DataManagementBar.SetSelectedByteRegion(XUiC_DataManagementBar.BarRegion.None);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerator startGameCo()
	{
		XUiC_SavegamesList.ListEntry selectedSave = this.savesList.SelectedEntryData;
		if (selectedSave.SaveVersion.ReleaseType != Constants.cVersionInformation.ReleaseType || selectedSave.SaveVersion.Major != Constants.cVersionInformation.Major)
		{
			XUiC_ContinueGame.<>c__DisplayClass23_0 CS$<>8__locals1 = new XUiC_ContinueGame.<>c__DisplayClass23_0();
			CS$<>8__locals1.selection = null;
			XUiC_MessageBoxWindowGroup.ShowOkCancel(this.xui, Localization.Get("xuiSaveDifferentGameVersion", false, null), string.Format(Localization.Get("xuiSaveDifferentGameVersionText", false, null), selectedSave.SaveName, selectedSave.SaveVersion.ShortString, Constants.cVersionInformation.ShortString), delegate
			{
				CS$<>8__locals1.selection = new bool?(true);
			}, delegate
			{
				CS$<>8__locals1.selection = new bool?(false);
			}, false, false, false);
			while (CS$<>8__locals1.selection == null)
			{
				yield return null;
			}
			if (!CS$<>8__locals1.selection.Value)
			{
				yield break;
			}
			CS$<>8__locals1 = null;
		}
		SaveInfoProvider.SaveEntryInfo saveEntryInfo;
		if (selectedSave.WorldName.Equals("Playtesting") || selectedSave.WorldName.Equals("Empty"))
		{
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, 4);
		}
		else if (SaveInfoProvider.Instance.TryGetLocalSaveEntry(selectedSave.WorldName, selectedSave.SaveName, selectedSave.StorageType, out saveEntryInfo) && saveEntryInfo.WorldEntry.Location.Type != PathAbstractions.EAbstractedLocationType.None)
		{
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, (int)saveEntryInfo.WorldEntry.Location.Type);
			GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)saveEntryInfo.WorldEntry.Location.StorageType);
		}
		else
		{
			XUiC_ContinueGame.<>c__DisplayClass23_1 CS$<>8__locals2 = new XUiC_ContinueGame.<>c__DisplayClass23_1();
			CS$<>8__locals2.worldLocation = null;
			XUiC_WorldSelectionPopup.Open(this.xui, "xuiWorldConflict", selectedSave.WorldName, delegate
			{
				CS$<>8__locals2.worldLocation = new PathAbstractions.AbstractedLocation?(PathAbstractions.AbstractedLocation.None);
			}, delegate(PathAbstractions.AbstractedLocation _location)
			{
				CS$<>8__locals2.worldLocation = new PathAbstractions.AbstractedLocation?(_location);
			});
			while (CS$<>8__locals2.worldLocation == null)
			{
				yield return null;
			}
			if (CS$<>8__locals2.worldLocation.Value.Type == PathAbstractions.EAbstractedLocationType.None)
			{
				yield break;
			}
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, (int)CS$<>8__locals2.worldLocation.Value.Type);
			GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)CS$<>8__locals2.worldLocation.Value.StorageType);
			CS$<>8__locals2 = null;
		}
		if (GamePrefs.GetInt(EnumGamePrefs.ResetUnprotectedChunks) != 0)
		{
			XUiC_ContinueGame.<>c__DisplayClass23_2 CS$<>8__locals3 = new XUiC_ContinueGame.<>c__DisplayClass23_2();
			CS$<>8__locals3.resetConfirm = null;
			XUiC_MessageBoxWindowGroup.ShowOkCancel(this.xui, Localization.Get("goResetUnprotectedChunksMode", false, null), Localization.Get("goResetUnprotectedChunksModeConfirmation", false, null), delegate
			{
				CS$<>8__locals3.resetConfirm = new bool?(true);
			}, delegate
			{
				CS$<>8__locals3.resetConfirm = new bool?(false);
			}, false, false, false);
			while (CS$<>8__locals3.resetConfirm == null)
			{
				yield return null;
			}
			if (!CS$<>8__locals3.resetConfirm.Value)
			{
				yield break;
			}
			CS$<>8__locals3 = null;
		}
		GamePrefs.SetPersistent(EnumGamePrefs.GameMode, true);
		this.Settings.SaveGameOptions(false);
		if (PlatformOptimizations.RestartAfterRwg)
		{
			yield return PlatformApplicationManager.CheckRestartCoroutine(true);
		}
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		bool flag = !GamePrefs.GetBool(EnumGamePrefs.ServerEnabled);
		NetworkConnectionError networkConnectionError = SingletonMonoBehaviour<ConnectionManager>.Instance.StartServers(GamePrefs.GetString(EnumGamePrefs.ServerPassword), flag);
		if (networkConnectionError != NetworkConnectionError.NoError)
		{
			XUiC_MessageBoxWindowGroup.ShowNetworkError(this.xui, networkConnectionError, false);
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void gameModeChanged(GameMode _newMode)
	{
		GamePrefs.Set(EnumGamePrefs.GameMode, _newMode.GetTypeName());
		this.Settings.UpdateOptionVisibilityForGameMode(_newMode);
		this.Settings.UpdateOptionValuesFromGamePrefs();
		this.validateStartable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void validateStartable()
	{
		bool flag = this.savesList.SelectedEntryIndex >= 0;
		if (flag)
		{
			XUiC_SavegamesList.ListEntry selectedEntryData = this.savesList.SelectedEntryData;
			flag = selectedEntryData.Playable;
			if (flag && !selectedEntryData.WorldExists)
			{
				flag = false;
			}
		}
		flag &= this.Settings.PortValid;
		base.ValidStartableState = flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateInput()
	{
		if (!XUiUtils.HotkeysAllowedFor(this.viewComponent ?? this.children[0].ViewComponent, false))
		{
			return;
		}
		if (Input.GetKeyUp(127) && this.savesList.SelectedEntryData != null)
		{
			this.deleteSaveWithConfirmation();
		}
		if (this.xui.playerUI.playerInput.GUIActions.Apply.WasPressed)
		{
			base.BtnStart_OnPressed(null, 0);
		}
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		this.doLoadSaveGameAutomation();
		this.updateInput();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void doLoadSaveGameAutomation()
	{
		EPlatformLoadSaveGameState loadSaveGameState = PlatformApplicationManager.GetLoadSaveGameState();
		switch (loadSaveGameState)
		{
		case EPlatformLoadSaveGameState.NewGameSelect:
		case EPlatformLoadSaveGameState.ContinueGameOpen:
			break;
		case EPlatformLoadSaveGameState.NewGamePlay:
		case EPlatformLoadSaveGameState.ContinueGamePlay:
			if (!this.BtnStart.ViewComponent.Enabled)
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			base.BtnStart_OnPressed(this, -1);
			PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			break;
		case EPlatformLoadSaveGameState.ContinueGameSelect:
		{
			string @string = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			string string2 = GamePrefs.GetString(EnumGamePrefs.GameName);
			UserDataStorageType @int = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
			XUiC_SavegamesList.ListEntry listEntry = this.savesList.SelectedEntryData;
			if (listEntry == null || !listEntry.WorldName.EqualsCaseInsensitive(@string) || !listEntry.SaveName.EqualsCaseInsensitive(string2))
			{
				this.savesList.SelectEntry(@string, string2, @int);
			}
			listEntry = this.savesList.SelectedEntryData;
			if (listEntry == null || !listEntry.WorldName.EqualsCaseInsensitive(@string) || !listEntry.SaveName.EqualsCaseInsensitive(string2))
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			return;
		}
		default:
			return;
		}
	}

	public static string ID = "";

	[XuiBindComponent("saves", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_SavegamesList savesList;

	[XuiBindComponent("btnDeleteSave", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDeleteSave;
}
