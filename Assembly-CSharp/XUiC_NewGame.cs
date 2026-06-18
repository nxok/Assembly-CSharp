using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform;
using SandboxOptions;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_NewGame : XUiC_NewContinueBase
{
	[XuiXmlBinding("gamenametooltip")]
	public string GameNameTooltip
	{
		get
		{
			return this.gameNameTooltip;
		}
		set
		{
			if (this.gameNameTooltip == value)
			{
				return;
			}
			this.gameNameTooltip = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("gamenamevalid")]
	public bool ValidGameName
	{
		get
		{
			return this.validGameName;
		}
		set
		{
			if (this.validGameName == value)
			{
				return;
			}
			this.validGameName = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("countynamevalid")]
	public bool CountyNameValid
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_WorldGenerationWindow xuiC_WorldGenerationWindow = this.worldGenerationControls;
			return xuiC_WorldGenerationWindow == null || xuiC_WorldGenerationWindow.ValidCountyName;
		}
	}

	public GameMode SelectedGameMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<GameMode> xuiC_ComboBoxList = this.cbxGameMode;
			if (xuiC_ComboBoxList == null)
			{
				return null;
			}
			return xuiC_ComboBoxList.Value;
		}
	}

	[XuiXmlBinding("iscontinuegame")]
	public bool IsContinueGame
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return false;
		}
	}

	[XuiXmlBinding("worldtype")]
	public XUiC_NewGame.EWorldType WorldType
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<XUiC_NewGame.EWorldType> xuiC_ComboBoxEnum = this.cbxWorldType;
			if (xuiC_ComboBoxEnum == null)
			{
				return XUiC_NewGame.EWorldType.Handmade;
			}
			return xuiC_ComboBoxEnum.Value;
		}
	}

	[XuiXmlBinding("saveStorageTooltip")]
	public string SaveStorageTooltipKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.saveStorageTooltipKey;
		}
	}

	[XuiXmlBinding("saveDataLimitTooltip")]
	public string SaveDataLimitTooltipKey
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.saveDataLimitTooltipKey;
		}
	}

	public XUiC_NewGame.GameWorldInfo? SelectedWorld
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<XUiC_NewGame.GameWorldInfo> xuiC_ComboBoxList = this.cbxWorldName;
			return new XUiC_NewGame.GameWorldInfo?((xuiC_ComboBoxList != null) ? xuiC_ComboBoxList.Value : default(XUiC_NewGame.GameWorldInfo));
		}
	}

	public GameUtils.WorldInfo SelectedWorldInfo
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (this.SelectedWorld == null)
			{
				return null;
			}
			XUiC_NewGame.GameWorldInfo? gameWorldInfo;
			return gameWorldInfo.GetValueOrDefault().WorldInfo;
		}
	}

	[XuiXmlBinding("worldgameversion")]
	public string WorldGameVersion
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			GameUtils.WorldInfo selectedWorldInfo = this.SelectedWorldInfo;
			if (selectedWorldInfo == null)
			{
				return "";
			}
			if (!selectedWorldInfo.GameVersionCreated.IsValid)
			{
				return "";
			}
			return selectedWorldInfo.GameVersionCreated.ReleaseType.ToStringCached<VersionInformation.EGameReleaseType>() + " " + selectedWorldInfo.GameVersionCreated.Major.ToString();
		}
	}

	[XuiXmlBinding("differentworldversion")]
	public bool DifferentWorldVersion
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			GameUtils.WorldInfo selectedWorldInfo = this.SelectedWorldInfo;
			return selectedWorldInfo != null && selectedWorldInfo.GameVersionCreated.IsValid && !selectedWorldInfo.GameVersionCreated.EqualsMajor(Constants.cVersionInformation);
		}
	}

	[XuiXmlBinding("mapsize")]
	public string MapSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			GameUtils.WorldInfo selectedWorldInfo = this.SelectedWorldInfo;
			if (selectedWorldInfo == null)
			{
				return "";
			}
			return string.Format("{0} x {1}", selectedWorldInfo.WorldSize.x, selectedWorldInfo.WorldSize.y);
		}
	}

	[XuiXmlBinding("isgenerateworld")]
	public bool IsGenerateWorld
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.WorldType == XUiC_NewGame.EWorldType.CreateRandom;
		}
	}

	[XuiXmlBinding("worldstorageusesdatalimit")]
	public bool WorldStorageUsesDataLimit
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_NewGame.GameWorldInfo? gameWorldInfo;
			return this.SelectedWorld != null && gameWorldInfo.GetValueOrDefault().Location.StorageType.UsesDataLimit();
		}
	}

	public override bool PlayIntroMovie
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return GamePrefs.GetBool(EnumGamePrefs.OptionsIntroMovieEnabled);
		}
	}

	public override bool AllowChangingCreativeMode
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_NewGame.ID = base.WindowGroup.Id;
		this.txtGameName.UIInput.onValidate = new UIInput.OnValidate(GameUtils.ValidateGameNameInput);
		SaveDataLimitUIHelper.AddComboBox(this.cbxSaveDataLimit);
	}

	public override void OnOpen()
	{
		using (this.pmOnOpen.Auto())
		{
			this.IsDirty = true;
			this.Settings.WatchForServerEnabledChanges(false);
			this.cbxGameMode.Elements.Clear();
			this.createWorldList();
			foreach (GameMode gameMode in GameMode.AvailGameModes)
			{
				if (this.worldsPerMode.ContainsKey((EnumGameMode)gameMode.GetID()))
				{
					this.cbxGameMode.Elements.Add(gameMode);
				}
			}
			if (this.cbxGameMode.Elements.Count == 0)
			{
				Log.Error("No supported GameMode found in any world!");
			}
			string modeName = GamePrefs.GetString(EnumGamePrefs.GameMode);
			int num = this.cbxGameMode.Elements.FindIndex((GameMode _mode) => _mode.GetTypeName().EqualsCaseInsensitive(modeName));
			if (num < 0)
			{
				num = 0;
			}
			this.cbxGameMode.SelectedIndex = num;
			this.gameModeChanged(this.SelectedGameMode);
			GamePrefs.Instance.Load(GameIO.GetSaveGameRootDir() + "/newGameOptions.sdf");
			this.Settings.UpdateOptionValuesFromGamePrefs();
			this.Settings.ResetSandboxPresetToDefault();
			if (GamePrefs.GetInt(EnumGamePrefs.MaxChunkAge) == 0)
			{
				GamePrefs.Set(EnumGamePrefs.MaxChunkAge, (int)GamePrefs.GetDefault(EnumGamePrefs.MaxChunkAge));
			}
			if (GamePrefs.GetString(EnumGamePrefs.GameName).Trim().Length < 1)
			{
				GamePrefs.Set(EnumGamePrefs.GameName, "MyGame");
			}
			this.txtGameName.Text = GamePrefs.GetString(EnumGamePrefs.GameName).Trim();
			this.validateStartable();
			base.OnOpen();
			this.Settings.ApplyMaxPlayerCountOptions();
			this.Settings.UpdateOptionVisibilityForGameMode(this.SelectedGameMode);
			base.GetChildById("txtGameName").SelectCursorElement(true, false);
			this.Settings.ApplyInitialServerEnabledState();
			if (base.DataManagementBarEnabled)
			{
				SaveInfoProvider.Instance.SetDirty();
				this.DataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
				this.DataManagementBar.SetSelectionDepth(XUiC_DataManagementBar.SelectionDepth.Primary);
				base.updateBarUsageAndAllowanceValues();
				this.updateBarPendingValue();
				SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Combine(SaveDataLimitUIHelper.OnValueChanged, new Action(this.CbxSaveDataLimit_OnValueChanged));
				this.worldGenerationControls.OnWorldSizeChanged += this.updateBarPendingValue;
			}
			this.cbxGameSaveStorage.Value = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		}
	}

	public override void OnClose()
	{
		base.OnClose();
		SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Remove(SaveDataLimitUIHelper.OnValueChanged, new Action(this.CbxSaveDataLimit_OnValueChanged));
		this.worldGenerationControls.OnWorldSizeChanged -= this.updateBarPendingValue;
	}

	[XuiBindEvent("OnPress", "btnGenerateWorld")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnRwgPreviewerOnOnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_WorldGenerationWindow.Open(this.xui, XUiC_NewGame.ID);
	}

	[XuiBindEvent("OnValueChanged", "cbxGameMode")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxGameMode_OnValueChanged(XUiController _sender, GameMode _oldValue, GameMode _newValue)
	{
		this.gameModeChanged(_newValue);
	}

	[XuiBindEvent("OnValueChanged", "cbxWorldType")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxWorldType_OnValueChanged(XUiController _sender, XUiC_NewGame.EWorldType _oldValue, XUiC_NewGame.EWorldType _newValue)
	{
		this.IsDirty = true;
		this.updateWorlds();
		this.validateStartable();
	}

	[XuiBindEvent("OnValueChanged", "cbxWorldName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxWorldName_OnValueChanged(XUiController _sender, XUiC_NewGame.GameWorldInfo _oldValue, XUiC_NewGame.GameWorldInfo _newValue)
	{
		this.IsDirty = true;
		GamePrefs.Set(EnumGamePrefs.GameWorld, _newValue.RealName);
		if (this.IsGenerateWorld)
		{
			GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType));
			this.worldGenerationControls.ValidateNewRwg();
		}
		else
		{
			GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)_newValue.Location.StorageType);
		}
		this.updateBarPendingValue();
		this.validateStartable();
	}

	[XuiBindEvent("OnChangeHandler", "txtGameName")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void TxtGameName_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		GamePrefs.Set(EnumGamePrefs.GameName, _text);
		this.validateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxSaveDataLimit_OnValueChanged()
	{
		this.IsDirty = true;
		this.updateBarPendingValue();
	}

	[XuiBindEvent("OnValueChanged", "cbxGameSaveStorage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void CbxGameSaveStorage_OnValueChanged(XUiController _sender, UserDataStorageType _oldValue, UserDataStorageType _newValue)
	{
		this.IsDirty = true;
		GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, (int)_newValue);
		this.worldGenerationControls.SetGameSaveStorageValue(_newValue);
		this.updateBarPendingValue();
		this.validateStartable();
	}

	[XuiBindEvent("OnGameSaveStorageChanged", "worldGenerationControls")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void worldGenerationGameSaveStorageChanged(UserDataStorageType _newValue)
	{
		this.IsDirty = true;
		this.cbxGameSaveStorage.Value = _newValue;
		this.updateBarPendingValue();
		this.validateStartable();
	}

	[XuiBindEvent("OnWorldStorageChanged", "worldGenerationControls")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void worldGenerationWorldSaveStorageChanged()
	{
		this.updateBarPendingValue();
		this.validateStartable();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void createWorldList()
	{
		this.worldsPerMode.Clear();
		List<PathAbstractions.AbstractedLocation> availablePathsList = PathAbstractions.WorldsSearchPaths.GetAvailablePathsList(null, false, null);
		if (availablePathsList.Count == 0)
		{
			Log.Error("No worlds found!");
			return;
		}
		foreach (PathAbstractions.AbstractedLocation abstractedLocation in availablePathsList)
		{
			XUiC_NewGame.GameWorldInfo gameWorldInfo = new XUiC_NewGame.GameWorldInfo
			{
				RealName = abstractedLocation.Name,
				Location = abstractedLocation
			};
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(abstractedLocation);
			if (worldInfo == null)
			{
				gameWorldInfo.Description = "No Description Found";
				if (!this.worldsPerMode.ContainsKey(EnumGameMode.Creative))
				{
					this.worldsPerMode.Add(EnumGameMode.Creative, new List<XUiC_NewGame.GameWorldInfo>());
				}
				this.worldsPerMode[EnumGameMode.Creative].Add(gameWorldInfo);
			}
			else
			{
				gameWorldInfo.CustName = worldInfo.Name;
				gameWorldInfo.Description = worldInfo.Description;
				gameWorldInfo.WorldInfo = worldInfo;
				string[] modes = worldInfo.Modes;
				if (modes != null)
				{
					foreach (string text in modes)
					{
						EnumGameMode enumGameMode;
						if (EnumUtils.TryParse<EnumGameMode>(text, out enumGameMode, true))
						{
							if (!this.worldsPerMode.ContainsKey(enumGameMode))
							{
								this.worldsPerMode.Add(enumGameMode, new List<XUiC_NewGame.GameWorldInfo>());
							}
							this.worldsPerMode[enumGameMode].Add(gameWorldInfo);
						}
						else
						{
							Log.Warning(string.Concat(new string[] { "World \"", gameWorldInfo.RealName, "\" has unknown game mode \"", text, "\"." }));
						}
					}
				}
			}
		}
		if (this.worldsPerMode.Count == 0)
		{
			Log.Error("No world with any valid GameMode found!");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateWorlds()
	{
		EnumGameMode id = (EnumGameMode)this.SelectedGameMode.GetID();
		this.cbxWorldName.Elements.Clear();
		List<XUiC_NewGame.GameWorldInfo> list;
		if (this.WorldType != XUiC_NewGame.EWorldType.CreateRandom && this.worldsPerMode.TryGetValue(id, out list))
		{
			foreach (XUiC_NewGame.GameWorldInfo gameWorldInfo in list)
			{
				if (this.<updateWorlds>g__CanAddWorld|66_0(gameWorldInfo))
				{
					this.cbxWorldName.Elements.Add(gameWorldInfo);
				}
			}
		}
		this.cbxWorldName.SelectedIndex = 0;
		this.CbxWorldName_OnValueChanged(this.cbxWorldName, default(XUiC_NewGame.GameWorldInfo), this.SelectedWorld.Value);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void selectWorld(string _worldName)
	{
		int num = this.cbxWorldName.Elements.FindIndex((XUiC_NewGame.GameWorldInfo _info) => _info.RealName.EqualsCaseInsensitive(_worldName));
		if (num < 0)
		{
			EnumGameMode id = (EnumGameMode)this.SelectedGameMode.GetID();
			List<XUiC_NewGame.GameWorldInfo> list;
			if (this.worldsPerMode.TryGetValue(id, out list))
			{
				num = list.FindIndex((XUiC_NewGame.GameWorldInfo _info) => _info.RealName.EqualsCaseInsensitive(_worldName));
				if (num >= 0)
				{
					XUiC_NewGame.GameWorldInfo gameWorldInfo = list[num];
					this.cbxWorldType.Value = (gameWorldInfo.IsRandomWorld ? XUiC_NewGame.EWorldType.ExistingRandom : XUiC_NewGame.EWorldType.Handmade);
					this.CbxWorldType_OnValueChanged(this.cbxWorldType, XUiC_NewGame.EWorldType.Handmade, this.cbxWorldType.Value);
				}
			}
			num = this.cbxWorldName.Elements.FindIndex((XUiC_NewGame.GameWorldInfo _info) => _info.RealName.EqualsCaseInsensitive(_worldName));
		}
		this.cbxWorldName.SelectedIndex = num;
		this.CbxWorldName_OnValueChanged(this.cbxWorldName, default(XUiC_NewGame.GameWorldInfo), this.SelectedWorld.Value);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnOpenSandboxSettingsRequested()
	{
		SandboxOptionManager.Current.SetWorldAndGame("", "");
		base.OnOpenSandboxSettingsRequested();
	}

	[XuiBindEvent("OnPress", "btnDefaults")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnDefaults_OnPressed(XUiController _sender, int _mouseButton)
	{
		GamePrefs.Set(EnumGamePrefs.ServerEnabled, (bool)GamePrefs.GetDefault(EnumGamePrefs.ServerEnabled));
		this.SelectedGameMode.ResetGamePrefs();
		this.Settings.UpdateOptionValuesFromGamePrefs();
		this.Settings.UpdateServerEnabledState();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void OnDataManagementWindowClosed()
	{
		base.updateBarUsageAndAllowanceValues();
		string @string = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		this.createWorldList();
		this.updateWorlds();
		this.selectWorld(@string);
		if (base.DataManagementBarEnabled)
		{
			this.DataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
			this.updateBarPendingValue();
		}
		this.worldGenerationControls.RefreshCountyName();
		this.validateStartable();
		this.worldGenerationControls.RefreshBindings();
		base.RefreshBindings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBarPendingValue()
	{
		if (!base.DataManagementBarEnabled)
		{
			return;
		}
		UserDataStorageType @int = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		this.pendingPreviewSize = (@int.UsesDataLimit() ? this.<updateBarPendingValue>g__CalculatePendingSaveSize|71_0() : 0L);
		this.DataManagementBar.SetPendingBytes(this.pendingPreviewSize);
		this.validateStartable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override IEnumerator startGameCo()
	{
		string worldName;
		Vector2i worldSize;
		if (this.IsGenerateWorld)
		{
			worldName = null;
			yield return this.worldGenerationControls.GenerateCo(delegate(string _name)
			{
				worldName = _name;
			}, null);
			if (worldName == null)
			{
				this.xui.playerUI.windowManager.Open(this.windowGroup, true);
				yield break;
			}
			GamePrefs.Set(EnumGamePrefs.GameWorld, worldName);
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, 2);
			worldSize.y = (worldSize.x = this.worldGenerationControls.WorldSize);
		}
		else
		{
			worldSize = this.SelectedWorldInfo.WorldSize;
			worldName = this.SelectedWorldInfo.Name;
			XUiC_NewGame.GameWorldInfo? gameWorldInfo;
			GamePrefs.Set(EnumGamePrefs.GameWorldLocationType, (int)((this.SelectedWorld != null) ? new PathAbstractions.EAbstractedLocationType?(gameWorldInfo.GetValueOrDefault().Location.Type) : null).Value);
		}
		PathAbstractions.AbstractedLocation abstractedLocation = PathAbstractions.Contextual.FindActiveWorldLocation();
		UserDataStorageType @int = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		long saveDataSize = SaveDataLimitUIHelper.CurrentValue.CalculateTotalSize(worldSize);
		XUiC_SaveSpaceNeeded confirmationWindow = XUiC_SaveSpaceNeeded.Open(saveDataSize, new string[]
		{
			abstractedLocation.FullPath,
			GameIO.GetSaveGameDir(worldName, GamePrefs.GetString(EnumGamePrefs.GameName), (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType))
		}, @int, true, true, false, null, "xuiDmCreateSave", null, null, "xuiCreate", null);
		if (confirmationWindow != null)
		{
			while (confirmationWindow.IsOpen || confirmationWindow.Result == XUiC_SaveSpaceNeeded.ConfirmationResult.Pending)
			{
				yield return null;
			}
			if (confirmationWindow.Result != XUiC_SaveSpaceNeeded.ConfirmationResult.Confirmed)
			{
				this.xui.playerUI.windowManager.Open(this.windowGroup, true);
				yield break;
			}
		}
		SaveDataLimit.SetLimitToPref(saveDataSize);
		GamePrefs.SetPersistent(EnumGamePrefs.GameMode, true);
		this.Settings.SaveGameOptions(true);
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
		string @string = GamePrefs.GetString(EnumGamePrefs.GameWorld);
		this.updateWorlds();
		this.selectWorld(@string);
		this.Settings.UpdateOptionVisibilityForGameMode(_newMode);
		this.Settings.UpdateOptionValuesFromGamePrefs();
		this.Settings.ResetSandboxPresetToDefault();
		this.validateStartable();
	}

	[XuiBindEvent("OnCountyNameChanged", "worldGenerationControls")]
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void validateStartable()
	{
		string text;
		bool flag = this.validateWorld(out text);
		flag &= this.validateGameName(text);
		flag &= this.validateStorage();
		if (base.DataManagementBarEnabled)
		{
			flag &= SaveInfoProvider.Instance.TotalAvailableBytes >= this.pendingPreviewSize;
		}
		flag &= this.Settings.PortValid;
		base.ValidStartableState = flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateGameName(string _worldName)
	{
		string text = this.txtGameName.Text;
		UserDataStorageType @int = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		bool flag = GameUtils.ValidateGameName(text);
		bool flag2 = text.Trim().Length > 0;
		bool flag3 = !text.EqualsCaseInsensitive("Region");
		bool flag4 = !SdFile.Exists(GameIO.GetSaveGameDir(_worldName, text, @int) + "/main.ttw");
		this.ValidGameName = flag && flag2 && flag3 && flag4;
		if (!flag2)
		{
			this.GameNameTooltip = Localization.Get("mmLblNameErrorEmpty", false, null);
		}
		else if (!flag)
		{
			this.GameNameTooltip = Localization.Get("mmLblNameErrorInvalid", false, null);
		}
		else if (!flag3)
		{
			this.GameNameTooltip = Localization.Get("mmLblNameErrorDefault", false, null);
		}
		else if (!flag4)
		{
			this.GameNameTooltip = Localization.Get("mmLblNameErrorExists", false, null);
		}
		else
		{
			this.GameNameTooltip = "";
		}
		return this.ValidGameName;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateWorld(out string _worldName)
	{
		XUiC_NewGame.GameWorldInfo value = this.SelectedWorld.Value;
		bool flag = true;
		bool flag2;
		if (!this.IsGenerateWorld)
		{
			flag2 = value.Location.Type != PathAbstractions.EAbstractedLocationType.None;
			if (this.SelectedWorldInfo != null)
			{
				bool flag3 = this.SelectedWorldInfo.GameVersionCreated.IsValid && !this.SelectedWorldInfo.GameVersionCreated.EqualsMajor(Constants.cVersionInformation);
				flag &= !flag3;
			}
			_worldName = value.RealName;
		}
		else
		{
			flag2 = this.worldGenerationControls.ValidCountyName;
			_worldName = this.worldGenerationControls.CountyName;
		}
		return flag2 && flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validateStorage()
	{
		UserDataStorageType @int = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		XUiC_NewGame.GameWorldInfo value = this.SelectedWorld.Value;
		bool flag = this.IsGenerateWorld || value.Location.Type == PathAbstractions.EAbstractedLocationType.GameData || value.Location.StorageType != UserDataStorageType.DeviceLocal || @int == UserDataStorageType.DeviceLocal;
		if (flag)
		{
			this.cbxGameSaveStorage.TextColor = Color.white;
			this.saveStorageTooltipKey = string.Empty;
		}
		else
		{
			this.cbxGameSaveStorage.TextColor = Color.red;
			this.saveStorageTooltipKey = "mmLblNameErrorSaveStorageInvalid";
		}
		bool flag2 = !@int.UsesDataLimit() || this.cbxSaveDataLimit.Value > SaveDataLimitType.Unlimited;
		if (flag2)
		{
			this.cbxSaveDataLimit.TextColor = Color.white;
			this.saveDataLimitTooltipKey = string.Empty;
		}
		else
		{
			this.cbxSaveDataLimit.TextColor = Color.red;
			this.saveDataLimitTooltipKey = "mmLblNameErrorSaveDataLimitNoUnlimited";
		}
		return flag && flag2;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateInput()
	{
		if (!XUiUtils.HotkeysAllowedFor(this.viewComponent ?? this.children[0].ViewComponent, false))
		{
			return;
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
		{
			string @string = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			if (!this.SelectedWorld.Value.RealName.EqualsCaseInsensitive(@string))
			{
				this.selectWorld(@string);
			}
			if (!this.SelectedWorld.Value.RealName.EqualsCaseInsensitive(@string))
			{
				PlatformApplicationManager.SetFailedLoadSaveGame();
				return;
			}
			PlatformApplicationManager.AdvanceLoadSaveGameStateFrom(loadSaveGameState);
			return;
		}
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
		case EPlatformLoadSaveGameState.ContinueGameOpen:
		case EPlatformLoadSaveGameState.ContinueGameSelect:
			break;
		default:
			return;
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool <updateWorlds>g__CanAddWorld|66_0(XUiC_NewGame.GameWorldInfo _worldInfo)
	{
		if (_worldInfo.WorldInfo != null && PlatformOptimizations.EnforceMaxWorldSizeHost)
		{
			Vector2i worldSize = _worldInfo.WorldInfo.WorldSize;
			if (worldSize.x > PlatformOptimizations.MaxWorldSizeHost || worldSize.y > PlatformOptimizations.MaxWorldSizeHost)
			{
				return false;
			}
		}
		XUiC_NewGame.EWorldType worldType = this.WorldType;
		if (worldType != XUiC_NewGame.EWorldType.Handmade)
		{
			if (worldType == XUiC_NewGame.EWorldType.ExistingRandom)
			{
				if (!_worldInfo.IsRandomWorld)
				{
					return false;
				}
			}
		}
		else if (_worldInfo.IsRandomWorld)
		{
			return false;
		}
		return true;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public long <updateBarPendingValue>g__CalculatePendingSaveSize|71_0()
	{
		Vector2i worldSize;
		if (this.IsGenerateWorld)
		{
			worldSize.y = (worldSize.x = Math.Max(1, this.worldGenerationControls.WorldSize));
		}
		else
		{
			worldSize = this.SelectedWorldInfo.WorldSize;
		}
		if (SaveDataLimitUIHelper.CurrentValue == SaveDataLimitType.Unlimited)
		{
			return Math.Max(SaveDataLimitType.Short.CalculateTotalSize(worldSize), SaveInfoProvider.Instance.TotalAvailableBytes);
		}
		return SaveDataLimitUIHelper.CurrentValue.CalculateTotalSize(worldSize);
	}

	public static string ID = "";

	[XuiBindComponent("cbxGameMode", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<GameMode> cbxGameMode;

	[XuiBindComponent("cbxWorldType", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<XUiC_NewGame.EWorldType> cbxWorldType;

	[XuiBindComponent("cbxWorldName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_NewGame.GameWorldInfo> cbxWorldName;

	[XuiBindComponent("txtGameName", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtGameName;

	[XuiBindComponent("btnDefaults", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnDefaults;

	[XuiBindComponent("btnGenerateWorld", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnGenerateWorld;

	[XuiBindComponent("cbxSaveDataLimit", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<SaveDataLimitType> cbxSaveDataLimit;

	[PublicizedFrom(EAccessModifier.Private)]
	public string saveDataLimitTooltipKey = string.Empty;

	[XuiBindComponent("cbxGameSaveStorage", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<UserDataStorageType> cbxGameSaveStorage;

	[PublicizedFrom(EAccessModifier.Private)]
	public string saveStorageTooltipKey = string.Empty;

	[XuiBindComponent(true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldGenerationWindow worldGenerationControls;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<EnumGameMode, List<XUiC_NewGame.GameWorldInfo>> worldsPerMode = new EnumDictionary<EnumGameMode, List<XUiC_NewGame.GameWorldInfo>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public long pendingPreviewSize;

	[PublicizedFrom(EAccessModifier.Private)]
	public string gameNameTooltip = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validGameName;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly ProfilerMarker pmOnOpen = new ProfilerMarker("OnOpen");

	public enum EWorldType
	{
		Handmade,
		ExistingRandom,
		CreateRandom
	}

	public struct GameWorldInfo
	{
		public bool IsRandomWorld
		{
			get
			{
				GameUtils.WorldInfo worldInfo = this.WorldInfo;
				return worldInfo != null && worldInfo.RandomGeneratedWorld;
			}
		}

		public override string ToString()
		{
			if (PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional && this.Location.Type == PathAbstractions.EAbstractedLocationType.UserDataPath)
			{
				return this.RealName + " [808080][i](" + this.Location.StorageType.LocalizedName() + ")[/i][-]";
			}
			return this.RealName;
		}

		public string RealName;

		public string CustName;

		public string Description;

		public GameUtils.WorldInfo WorldInfo;

		public PathAbstractions.AbstractedLocation Location;
	}
}
