using System;
using System.Collections;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;
using WorldGenerationEngineFinal;

[Preserve]
public class XUiC_WorldGenerationWindow : XUiController
{
	public event Action OnCountyNameChanged;

	public event Action OnWorldSizeChanged;

	public event Action OnWorldStorageChanged;

	public event Action<UserDataStorageType> OnGameSaveStorageChanged;

	public XUiC_WorldGenerationWindow.PreviewQuality PreviewQualityLevel
	{
		get
		{
			XUiC_ComboBoxEnum<XUiC_WorldGenerationWindow.PreviewQuality> xuiC_ComboBoxEnum = this.cbxPreviewQuality;
			if (xuiC_ComboBoxEnum == null)
			{
				return XUiC_WorldGenerationWindow.PreviewQuality.Default;
			}
			return xuiC_ComboBoxEnum.Value;
		}
	}

	public WorldBuilder.BiomeLayout BiomeLayout
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<WorldBuilder.BiomeLayout> xuiC_ComboBoxEnum = this.cbxBiomeLayout;
			if (xuiC_ComboBoxEnum == null)
			{
				return WorldBuilder.BiomeLayout.CenterForest;
			}
			return xuiC_ComboBoxEnum.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxBiomeLayout != null)
			{
				this.cbxBiomeLayout.Value = value;
			}
		}
	}

	public int PlainsWeight
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxInt xuiC_ComboBoxInt = this.cbxPlainsWeight;
			return (int)((xuiC_ComboBoxInt != null) ? xuiC_ComboBoxInt.Value : 4L);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxPlainsWeight != null)
			{
				this.cbxPlainsWeight.Value = (long)value;
			}
		}
	}

	public int HillsWeight
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxInt xuiC_ComboBoxInt = this.cbxHillsWeight;
			return (int)((xuiC_ComboBoxInt != null) ? xuiC_ComboBoxInt.Value : 4L);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxHillsWeight != null)
			{
				this.cbxHillsWeight.Value = (long)value;
			}
		}
	}

	public int MountainsWeight
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxInt xuiC_ComboBoxInt = this.cbxMountainsWeight;
			return (int)((xuiC_ComboBoxInt != null) ? xuiC_ComboBoxInt.Value : 2L);
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxMountainsWeight != null)
			{
				this.cbxMountainsWeight.Value = (long)value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Rivers
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> xuiC_ComboBoxEnum = this.cbxRivers;
			if (xuiC_ComboBoxEnum == null)
			{
				return WorldBuilder.GenerationSelections.Default;
			}
			return xuiC_ComboBoxEnum.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxRivers != null)
			{
				this.cbxRivers.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Craters
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> xuiC_ComboBoxEnum = this.cbxCraters;
			if (xuiC_ComboBoxEnum == null)
			{
				return WorldBuilder.GenerationSelections.Default;
			}
			return xuiC_ComboBoxEnum.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxCraters != null)
			{
				this.cbxCraters.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Canyons
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> xuiC_ComboBoxEnum = this.cbxCanyons;
			if (xuiC_ComboBoxEnum == null)
			{
				return WorldBuilder.GenerationSelections.Default;
			}
			return xuiC_ComboBoxEnum.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxCanyons != null)
			{
				this.cbxCanyons.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Lakes
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> xuiC_ComboBoxEnum = this.cbxLakes;
			if (xuiC_ComboBoxEnum == null)
			{
				return WorldBuilder.GenerationSelections.Default;
			}
			return xuiC_ComboBoxEnum.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxLakes != null)
			{
				this.cbxLakes.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Rural
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> xuiC_ComboBoxEnum = this.cbxRural;
			if (xuiC_ComboBoxEnum == null)
			{
				return WorldBuilder.GenerationSelections.Default;
			}
			return xuiC_ComboBoxEnum.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxRural != null)
			{
				this.cbxRural.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Town
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> xuiC_ComboBoxEnum = this.cbxTown;
			if (xuiC_ComboBoxEnum == null)
			{
				return WorldBuilder.GenerationSelections.Default;
			}
			return xuiC_ComboBoxEnum.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxTown != null)
			{
				this.cbxTown.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections City
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> xuiC_ComboBoxEnum = this.cbxCity;
			if (xuiC_ComboBoxEnum == null)
			{
				return WorldBuilder.GenerationSelections.Default;
			}
			return xuiC_ComboBoxEnum.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxCity != null)
			{
				this.cbxCity.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Towns
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> xuiC_ComboBoxEnum = this.cbxTowns;
			if (xuiC_ComboBoxEnum == null)
			{
				return WorldBuilder.GenerationSelections.Default;
			}
			return xuiC_ComboBoxEnum.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxTowns != null)
			{
				this.cbxTowns.Value = value;
			}
		}
	}

	public WorldBuilder.GenerationSelections Wilderness
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> xuiC_ComboBoxEnum = this.cbxWilderness;
			if (xuiC_ComboBoxEnum == null)
			{
				return WorldBuilder.GenerationSelections.Default;
			}
			return xuiC_ComboBoxEnum.Value;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			if (this.cbxWilderness != null)
			{
				this.cbxWilderness.Value = value;
			}
		}
	}

	public bool HasSufficientSpace
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return !SaveInfoProvider.DataLimitEnabled || this.pendingBytes <= this.totalAvailableBytes;
		}
	}

	public bool IsAdvancedUi
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.windowGroup.Id == XUiC_WorldGenerationWindowGroup.ID;
		}
	}

	public int WorldSize
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("countynamevalid")]
	public bool ValidCountyName
	{
		get
		{
			return this.validCountyName;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.validCountyName = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("countyname")]
	public string CountyName
	{
		get
		{
			return this.countyName ?? "";
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.countyName = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("statustext")]
	public string StatusText
	{
		get
		{
			return this.statusText ?? "";
		}
		set
		{
			this.statusText = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("showbar")]
	public bool HasDataManagementBar
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.dataManagementBar != null && SaveInfoProvider.DataLimitEnabled;
		}
	}

	[XuiXmlBinding("canstartnewgame")]
	public bool CanStartNewGame
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			if (!this.IsGenerating)
			{
				WorldBuilder worldBuilder = this.WorldBuilder;
				if (worldBuilder != null && worldBuilder.IsFinished && !worldBuilder.IsCanceled && this.WorldBuilder.CanSaveData())
				{
					return this.HasSufficientSpace;
				}
			}
			return false;
		}
	}

	[XuiXmlBinding("isroamingoptional")]
	[PublicizedFrom(EAccessModifier.Private)]
	public bool bindingIsRoamingOptional()
	{
		return PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional;
	}

	public WorldBuilder WorldBuilder
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlBinding("isgenerating")]
	public bool IsGenerating
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.isGenerating;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this.isGenerating = value;
			this.IsDirty = true;
		}
	}

	[XuiXmlBinding("iscancelling")]
	public bool IsCancelling
	{
		get
		{
			return this.isCancelling;
		}
		set
		{
			this.isCancelling = value;
			this.IsDirty = true;
		}
	}

	public static bool WorldBuilderIsGenerating
	{
		get
		{
			XUiC_WorldGenerationWindow instance = XUiC_WorldGenerationWindow.Instance;
			WorldBuilder worldBuilder = ((instance != null) ? instance.WorldBuilder : null);
			return worldBuilder != null && !worldBuilder.IsFinished;
		}
	}

	public static void CancelGeneration()
	{
		XUiC_WorldGenerationWindow instance = XUiC_WorldGenerationWindow.Instance;
		WorldBuilder worldBuilder = ((instance != null) ? instance.WorldBuilder : null);
		if (worldBuilder != null)
		{
			worldBuilder.IsCanceled = true;
			XUiC_WorldGenerationWindow.Instance.IsCancelling = true;
		}
	}

	[XuiBindEvent("OnPress", "btnNewGame")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void NewGameButton_OnPressed(XUiController _sender, int _mouseButton)
	{
		this.xui.StartCoroutine(this.saveAndNewGameCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator saveAndNewGameCo()
	{
		if (this.isClosing)
		{
			yield break;
		}
		this.isClosing = true;
		bool shouldClose = false;
		WorldBuilder worldBuilder = this.WorldBuilder;
		WorldBuilder.SaveDataPromptMode saveDataPromptMode = WorldBuilder.SaveDataPromptMode.OnInsufficientSpace;
		XUiV_Window parentWindow = base.GetParentWindow();
		yield return worldBuilder.SaveData(saveDataPromptMode, ((parentWindow != null) ? parentWindow.Controller : null) ?? this, delegate
		{
			shouldClose = false;
		}, null, delegate
		{
			shouldClose = true;
		});
		if (!shouldClose)
		{
			this.isClosing = false;
			yield break;
		}
		GamePrefs.Set(EnumGamePrefs.GameWorld, this.CountyName);
		this.checkProfile(XUiC_NewGame.ID);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void checkProfile(string _windowToOpen)
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		if (ProfileSDF.CurrentProfileName().Length == 0)
		{
			XUiC_PlayerProfile.Open(this.xui, delegate
			{
				this.xui.playerUI.windowManager.Open(_windowToOpen, true);
			});
			return;
		}
		this.xui.playerUI.windowManager.Open(_windowToOpen, true);
	}

	public override void OnOpen()
	{
		XUiC_WorldGenerationWindow.Instance = this;
		base.OnOpen();
		this.IsGenerating = false;
		this.IsCancelling = false;
		this.isClosing = false;
		PathAbstractions.CacheEnabled = true;
		if (this.previewWindow != null)
		{
			this.prefabPreviewManagerInstance = new PrefabPreviewManager(this.previewWindow);
		}
		if (this.cbxWorldSize != null)
		{
			if (PlatformOptimizations.EnforceMaxWorldSizeHost)
			{
				int num = this.cbxWorldSize.Elements.FindLastIndex((int _element) => _element <= PlatformOptimizations.MaxWorldSizeHost);
				if (num >= 0)
				{
					this.cbxWorldSize.MinIndex = 0;
					this.cbxWorldSize.MaxIndex = num;
					this.cbxWorldSize.SelectedIndex = num;
				}
			}
			if (this.cbxWorldSize.Elements.Contains(8192))
			{
				this.cbxWorldSize.Value = 8192;
			}
		}
		if (this.cbxWorldSaveStorage != null)
		{
			this.cbxWorldSaveStorage.Value = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.UserWorldStorageType);
		}
		if (this.cbxGameSaveStorage != null)
		{
			this.cbxGameSaveStorage.Value = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType);
		}
		SaveDataLimitUIHelper.AddComboBox(this.cbxSaveDataLimit);
		SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Combine(SaveDataLimitUIHelper.OnValueChanged, new Action(this.SaveDataLimit_OnChanged));
		this.Rivers = WorldBuilder.GenerationSelections.Default;
		this.Craters = WorldBuilder.GenerationSelections.Default;
		this.Canyons = WorldBuilder.GenerationSelections.Default;
		this.Lakes = WorldBuilder.GenerationSelections.Default;
		this.Rural = WorldBuilder.GenerationSelections.Default;
		this.Town = WorldBuilder.GenerationSelections.Default;
		this.City = WorldBuilder.GenerationSelections.Default;
		this.Towns = WorldBuilder.GenerationSelections.Default;
		this.Wilderness = WorldBuilder.GenerationSelections.Default;
		this.PlainsWeight = 4;
		this.HillsWeight = 4;
		this.MountainsWeight = 2;
		this.BiomeLayout = WorldBuilder.BiomeLayout.CenterForest;
		for (int i = 0; i < this.biomeBoxes.Length; i++)
		{
			this.biomeBoxes[i].BiomeIdx = i;
		}
		this.updateTerrainPercentages(false);
		this.updateBiomePercentages();
		if (this.cbxPreviewQuality != null)
		{
			this.cbxPreviewQuality.Value = XUiC_WorldGenerationWindow.PreviewQuality.Default;
			this.cbxPreviewQuality.Max = new XUiC_WorldGenerationWindow.PreviewQuality?(PlatformOptimizations.MaxRWGPreviewQuality);
		}
		if (this.txtSeedInput != null)
		{
			this.SeedInput_OnChangeHandler(this.txtSeedInput, this.txtSeedInput.Text, true);
		}
		if (this.cbxWorldSize != null)
		{
			this.WorldSizeComboBox_OnValueChanged(this.cbxWorldSize, this.cbxWorldSize.Value - 1, this.cbxWorldSize.Value);
		}
		this.oldFogDensity = RenderSettings.fogDensity;
		RenderSettings.fogDensity = 0f;
		this.updateBarValues();
	}

	[XuiBindEvent("OnValueChanged", "cbxPreviewQuality")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void Quality_OnValueChanged(XUiController _sender, XUiC_WorldGenerationWindow.PreviewQuality _oldValue, XUiC_WorldGenerationWindow.PreviewQuality _newValue)
	{
		if (this.previewWindow != null)
		{
			this.previewWindow.PreviewInit();
			this.previewWindow.PreviewTextureDraw();
		}
		if (this.prefabPreviewManagerInstance == null || !this.prefabPreviewManagerInstance.Initialized)
		{
			return;
		}
		if (_oldValue >= XUiC_WorldGenerationWindow.PreviewQuality.Low && _oldValue <= XUiC_WorldGenerationWindow.PreviewQuality.High && _newValue >= XUiC_WorldGenerationWindow.PreviewQuality.Low && _newValue <= XUiC_WorldGenerationWindow.PreviewQuality.High)
		{
			return;
		}
		this.prefabPreviewManagerInstance.RemovePrefabs();
		this.prefabPreviewManagerInstance.InitPrefabs();
		this.prefabPreviewManagerInstance.ForceUpdate();
	}

	[XuiBindEvent("ValueChanged", "biomeBoxes")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BiomeWeight_OnValueChanged()
	{
		this.updateBiomePercentages();
	}

	[XuiBindEvent("OnValueChanged", "cbxPlainsWeight")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void PlainsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		this.updateTerrainPercentages(false);
	}

	[XuiBindEvent("OnValueChanged", "cbxHillsWeight")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void HillsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		this.updateTerrainPercentages(false);
	}

	[XuiBindEvent("OnValueChanged", "cbxMountainsWeight")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void MountainsWeight_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
	{
		this.updateTerrainPercentages(true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateTerrainPercentages(bool _isMountainsChanged = false)
	{
		if (this.cbxPlainsWeight == null || this.cbxHillsWeight == null || this.cbxMountainsWeight == null)
		{
			return;
		}
		int num2;
		int num3;
		int num4;
		for (;;)
		{
			float num = (float)(this.PlainsWeight + this.HillsWeight + this.MountainsWeight);
			if (num <= 0f)
			{
				num = 1f;
			}
			num2 = Mathf.RoundToInt((float)this.PlainsWeight / num * 100f);
			num3 = Mathf.RoundToInt((float)this.HillsWeight / num * 100f);
			num4 = Mathf.RoundToInt((float)this.MountainsWeight / num * 100f);
			if (num4 <= 50)
			{
				break;
			}
			if (_isMountainsChanged)
			{
				this.PlainsWeight++;
			}
			else
			{
				this.MountainsWeight--;
			}
		}
		if (num2 + num3 + num4 == 0)
		{
			num2 = 100;
		}
		this.cbxPlainsWeight.ValueTextOverride = string.Format("{0}%", Mathf.Max(0, num2));
		this.cbxHillsWeight.ValueTextOverride = string.Format("{0}%", Mathf.Max(0, num3));
		this.cbxMountainsWeight.ValueTextOverride = string.Format("{0}%", Mathf.Max(0, num4));
		this.cbxPlainsWeight.IsDirty = true;
		this.cbxHillsWeight.IsDirty = true;
		this.cbxMountainsWeight.IsDirty = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBiomePercentages()
	{
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		for (int i = 0; i < 5; i++)
		{
			XUiC_WorldGenerationWindow.XUiC_RwgBiome xuiC_RwgBiome = ((i < this.biomeBoxes.Length) ? this.biomeBoxes[i] : null);
			if (xuiC_RwgBiome != null)
			{
				float num4 = (float)xuiC_RwgBiome.Value;
				num += num4;
				if (num4 > num2)
				{
					num2 = num4;
					num3 = i;
				}
			}
		}
		int num5 = 0;
		for (int j = 0; j < 5; j++)
		{
			XUiC_WorldGenerationWindow.XUiC_RwgBiome xuiC_RwgBiome2 = ((j < this.biomeBoxes.Length) ? this.biomeBoxes[j] : null);
			if (xuiC_RwgBiome2 != null)
			{
				int num6 = Mathf.RoundToInt((float)xuiC_RwgBiome2.Value / num * 100f);
				num6 = Utils.FastMax(5, num6);
				num5 += num6;
				if (j == 4)
				{
					num6 += 100 - num5;
					if (num6 < 5)
					{
						XUiC_WorldGenerationWindow.XUiC_RwgBiome xuiC_RwgBiome3 = this.biomeBoxes[num3];
						int num7 = Mathf.RoundToInt((float)xuiC_RwgBiome3.Value / num * 100f);
						xuiC_RwgBiome3.SetLabel(string.Format("{0}%", num7 + 5 - num6));
						num6 = 5;
					}
				}
				xuiC_RwgBiome2.SetLabel(string.Format("{0}%", num6));
				xuiC_RwgBiome2.IsDirty = true;
			}
		}
	}

	[XuiBindEvent("OnValueChanged", "cbxWorldSize")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldSizeComboBox_OnValueChanged(XUiController _sender, int _oldValue, int _newValue)
	{
		this.WorldSize = _newValue;
		this.RefreshCountyName();
		Action onWorldSizeChanged = this.OnWorldSizeChanged;
		if (onWorldSizeChanged == null)
		{
			return;
		}
		onWorldSizeChanged();
	}

	[XuiBindEvent("OnChangeHandler", "txtSeedInput")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void SeedInput_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
	{
		this.RefreshCountyName();
	}

	[XuiBindEvent("OnValueChanged", "cbxGameSaveStorage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void GameSaveStorage_OnValueChanged(XUiController _sender, UserDataStorageType _oldValue, UserDataStorageType _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.GameSaveStorageType, (int)_newValue);
		GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)_newValue);
		this.ValidateNewRwg();
		Action<UserDataStorageType> onGameSaveStorageChanged = this.OnGameSaveStorageChanged;
		if (onGameSaveStorageChanged == null)
		{
			return;
		}
		onGameSaveStorageChanged(_newValue);
	}

	[XuiBindEvent("OnValueChanged", "cbxWorldSaveStorage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void WorldSaveStorage_OnValueChanged(XUiController _sender, UserDataStorageType _oldValue, UserDataStorageType _newValue)
	{
		GamePrefs.Set(EnumGamePrefs.UserWorldStorageType, (int)_newValue);
		WorldBuilder worldBuilder = this.WorldBuilder;
		if (worldBuilder != null)
		{
			worldBuilder.SetStorageType(_newValue);
		}
		this.updateBarValues();
		this.ValidateNewRwg();
		Action onWorldStorageChanged = this.OnWorldStorageChanged;
		if (onWorldStorageChanged == null)
		{
			return;
		}
		onWorldStorageChanged();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SaveDataLimit_OnChanged()
	{
		this.validateDataLimit();
	}

	public void SetGameSaveStorageValue(UserDataStorageType _newValue)
	{
		this.cbxGameSaveStorage.Value = _newValue;
	}

	public void RefreshCountyName()
	{
		this.CountyName = WorldBuilder.GetGeneratedWorldName(this.txtSeedInput.Text, this.WorldSize);
		this.ValidateNewRwg();
		Action onCountyNameChanged = this.OnCountyNameChanged;
		if (onCountyNameChanged == null)
		{
			return;
		}
		onCountyNameChanged();
	}

	public void ValidateNewRwg()
	{
		string text = this.CountyName;
		bool flag = PathAbstractions.WorldsSearchPaths.GetLocation(text, null, null).Type != PathAbstractions.EAbstractedLocationType.None;
		this.ValidCountyName = !flag;
		this.validateDataLimit();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void validateDataLimit()
	{
		if (this.cbxSaveDataLimit == null)
		{
			return;
		}
		bool flag = !((UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.GameSaveStorageType)).UsesDataLimit() || this.cbxSaveDataLimit.Value > SaveDataLimitType.Unlimited;
		this.cbxSaveDataLimit.TextColor = (flag ? Color.white : Color.red);
	}

	[XuiBindEvent("OnPress", "btnManage")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void BtnManage_OnPressed(XUiController _sender, int _mouseButton)
	{
		XUiC_DataManagement.OpenDataManagementWindow(this, new Action(this.OnDataManagementWindowClosed));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnDataManagementWindowClosed()
	{
		this.updateBarValues();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateBarValues()
	{
		if (!this.HasDataManagementBar)
		{
			base.RefreshBindings();
			return;
		}
		SaveInfoProvider instance = SaveInfoProvider.Instance;
		this.dataManagementBar.SetUsedBytes(instance.TotalUsedBytes);
		this.dataManagementBar.SetAllowanceBytes(instance.TotalAllowanceBytes);
		this.dataManagementBar.SetDisplayMode(XUiC_DataManagementBar.DisplayMode.Preview);
		WorldBuilder worldBuilder = this.WorldBuilder;
		this.pendingBytes = ((worldBuilder != null) ? worldBuilder.SerializedSize : 0L);
		this.totalAvailableBytes = instance.TotalAvailableBytes;
		this.dataManagementBar.SetPendingBytes(this.pendingBytes);
		base.RefreshBindings();
	}

	[XuiBindEvent("OnPress", "btnGenerate")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void GenerateButton_OnPressed(XUiController _sender, int _mouseButton)
	{
		if (this.IsGenerating)
		{
			if (this.WorldBuilder != null)
			{
				this.WorldBuilder.IsCanceled = true;
			}
			return;
		}
		XUiC_WorldGenerationPreview xuiC_WorldGenerationPreview = this.previewWindow;
		if (xuiC_WorldGenerationPreview != null)
		{
			xuiC_WorldGenerationPreview.TerrainCleanup();
		}
		PrefabPreviewManager prefabPreviewManager = this.prefabPreviewManagerInstance;
		if (prefabPreviewManager != null)
		{
			prefabPreviewManager.ClearOldPreview();
		}
		ThreadManager.StartCoroutine(this.GenerateCo(null, null));
	}

	public IEnumerator GenerateCo(Action<string> _onSuccess = null, Action _onFailure = null)
	{
		this.IsGenerating = true;
		this.IsCancelling = false;
		this.updateBarValues();
		this.WorldSize = this.cbxWorldSize.Value;
		this.destroyBuilder();
		UserDataStorageType @int = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.UserWorldStorageType);
		this.WorldBuilder = new WorldBuilder(this.txtSeedInput.Text, this.WorldSize, @int)
		{
			PreviewWindow = this.previewWindow
		};
		XUiC_WorldGenerationPreview.WorldBuilder = this.WorldBuilder;
		this.WorldBuilder.Towns = this.Towns;
		this.WorldBuilder.Wilderness = this.Wilderness;
		this.WorldBuilder.Rivers = this.Rivers;
		this.WorldBuilder.Craters = this.Craters;
		this.WorldBuilder.Canyons = this.Canyons;
		this.WorldBuilder.Lakes = this.Lakes;
		this.WorldBuilder.Plains = this.PlainsWeight;
		this.WorldBuilder.Hills = this.HillsWeight;
		this.WorldBuilder.Mountains = this.MountainsWeight;
		this.WorldBuilder.biomeLayout = this.BiomeLayout;
		for (int i = 0; i < 5; i++)
		{
			XUiC_WorldGenerationWindow.XUiC_RwgBiome xuiC_RwgBiome = ((i < this.biomeBoxes.Length) ? this.biomeBoxes[i] : null);
			if (xuiC_RwgBiome != null)
			{
				this.WorldBuilder.SetBiomeWeight((BiomeType)i, xuiC_RwgBiome.Value);
			}
		}
		if (this.prefabPreviewManagerInstance != null)
		{
			this.prefabPreviewManagerInstance.ReadyToDisplay = false;
		}
		this.updateBarValues();
		yield return GCUtils.WaitForIdle();
		yield return this.WorldBuilder.GenerateFromUI();
		if (this.previewWindow != null)
		{
			yield return this.WorldBuilder.FinishForPreview();
			if (!this.WorldBuilder.IsCanceled)
			{
				yield return this.previewWindow.ShowPreview(XUiC_WorldGenerationPreview.PreviewStep.Done);
				yield return new WaitForSeconds(2f);
			}
		}
		else
		{
			XUiC_WorldGenerationWindow.<>c__DisplayClass152_0 CS$<>8__locals1 = new XUiC_WorldGenerationWindow.<>c__DisplayClass152_0();
			XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
			CS$<>8__locals1.success = false;
			WorldBuilder worldBuilder = this.WorldBuilder;
			WorldBuilder.SaveDataPromptMode saveDataPromptMode = WorldBuilder.SaveDataPromptMode.OnInsufficientSpace;
			XUiV_Window parentWindow = base.GetParentWindow();
			yield return worldBuilder.SaveData(saveDataPromptMode, ((parentWindow != null) ? parentWindow.Controller : null) ?? this, null, delegate
			{
				CS$<>8__locals1.success = false;
			}, delegate
			{
				CS$<>8__locals1.success = true;
			});
			if (CS$<>8__locals1.success)
			{
				if (_onSuccess != null)
				{
					_onSuccess(this.WorldBuilder.WorldName);
				}
			}
			else if (_onFailure != null)
			{
				_onFailure();
			}
			this.destroyBuilder();
			CS$<>8__locals1 = null;
		}
		XUiC_ProgressWindow.Close(LocalPlayerUI.primaryUI);
		if (this.prefabPreviewManagerInstance != null)
		{
			this.prefabPreviewManagerInstance.ReadyToDisplay = true;
		}
		this.IsCancelling = false;
		this.IsGenerating = false;
		this.StatusText = "";
		this.updateBarValues();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void destroyBuilder()
	{
		if (this.WorldBuilder == null)
		{
			return;
		}
		this.WorldBuilder.Cleanup();
		this.WorldBuilder = null;
	}

	public void StartClose()
	{
		if (this.IsGenerating)
		{
			return;
		}
		if (this.WorldBuilder == null || !this.WorldBuilder.IsFinished || !this.WorldBuilder.CanSaveData())
		{
			this.close();
			return;
		}
		this.xui.StartCoroutine(this.startCloseCo());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public IEnumerator startCloseCo()
	{
		if (this.isClosing)
		{
			yield break;
		}
		this.isClosing = true;
		bool shouldClose = false;
		WorldBuilder worldBuilder = this.WorldBuilder;
		WorldBuilder.SaveDataPromptMode saveDataPromptMode = WorldBuilder.SaveDataPromptMode.On;
		XUiV_Window parentWindow = base.GetParentWindow();
		yield return worldBuilder.SaveData(saveDataPromptMode, ((parentWindow != null) ? parentWindow.Controller : null) ?? this, delegate
		{
			shouldClose = false;
		}, delegate
		{
			shouldClose = true;
		}, delegate
		{
			shouldClose = true;
		});
		if (!shouldClose)
		{
			this.updateBarValues();
			this.isClosing = false;
			yield break;
		}
		this.close();
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void close()
	{
		this.xui.playerUI.windowManager.Close(this.windowGroup, false);
		this.xui.playerUI.windowManager.Open(this.lastWindowID, true);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.destroyBuilder();
		PathAbstractions.CacheEnabled = false;
		this.CleanupPreviewManager();
		RenderSettings.fogDensity = this.oldFogDensity;
		SaveDataLimitUIHelper.OnValueChanged = (Action)Delegate.Remove(SaveDataLimitUIHelper.OnValueChanged, new Action(this.SaveDataLimit_OnChanged));
	}

	public void CleanupPreviewManager()
	{
		if (this.prefabPreviewManagerInstance == null)
		{
			return;
		}
		this.prefabPreviewManagerInstance.Cleanup();
		this.prefabPreviewManagerInstance = null;
	}

	public override void Update(float _dt)
	{
		base.Update(_dt);
		base.handleDirtyUpdateDefault();
		if (!this.IsAdvancedUi)
		{
			return;
		}
		if (this.progressView != null && XUiUtils.HotkeysAllowedFor(this.progressView, false) && PlatformManager.NativePlatform.Input.PrimaryPlayer.GUIActions.Cancel.WasReleased)
		{
			XUiC_WorldGenerationWindow.CancelGeneration();
		}
		if (this.prefabPreviewManagerInstance != null && this.prefabPreviewManagerInstance.ReadyToDisplay)
		{
			this.prefabPreviewManagerInstance.Update();
		}
	}

	public static void Open(XUi _xui, string _previousWindowId)
	{
		XUiC_WorldGenerationWindow childByType = _xui.GetChildByType<XUiC_WorldGenerationWindowGroup>().GetChildByType<XUiC_WorldGenerationWindow>();
		childByType.lastWindowID = _previousWindowId;
		_xui.playerUI.windowManager.Open(childByType.windowGroup, true);
	}

	public static bool IsWindowOpen(XUi _xui)
	{
		XUiC_WorldGenerationWindow childByType = _xui.GetChildByType<XUiC_WorldGenerationWindowGroup>().GetChildByType<XUiC_WorldGenerationWindow>();
		return _xui.playerUI.windowManager.IsWindowOpen(childByType.windowGroup);
	}

	public static XUiC_WorldGenerationWindow Instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lastWindowID = string.Empty;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldGenerationPreview previewWindow;

	[XuiBindComponent("progress", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiView progressView;

	[XuiBindComponent("seedInput", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_TextInput txtSeedInput;

	[XuiBindComponent("generate", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnGenerate;

	[XuiBindComponent("btnNewGame", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnNewGame;

	[XuiBindComponent("WorldSize", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<int> cbxWorldSize;

	[XuiBindComponent("SaveDataLimitComboBox", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<SaveDataLimitType> cbxSaveDataLimit;

	[XuiBindComponent("PreviewQuality", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<XUiC_WorldGenerationWindow.PreviewQuality> cbxPreviewQuality;

	[XuiBindComponent("BiomeLayout", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.BiomeLayout> cbxBiomeLayout;

	[XuiBindComponent(false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_WorldGenerationWindow.XUiC_RwgBiome[] biomeBoxes;

	[XuiBindComponent("PlainsWeight", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxInt cbxPlainsWeight;

	[XuiBindComponent("HillsWeight", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxInt cbxHillsWeight;

	[XuiBindComponent("MountainsWeight", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxInt cbxMountainsWeight;

	[XuiBindComponent("Rivers", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxRivers;

	[XuiBindComponent("Craters", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxCraters;

	[XuiBindComponent("Cracks", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxCanyons;

	[XuiBindComponent("Lakes", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxLakes;

	[XuiBindComponent("Rural", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxRural;

	[XuiBindComponent("Town", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxTown;

	[XuiBindComponent("City", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxCity;

	[XuiBindComponent("Towns", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxTowns;

	[XuiBindComponent("Wilderness", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<WorldBuilder.GenerationSelections> cbxWilderness;

	[XuiBindComponent("btnDataManagement", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_Button btnManage;

	[XuiBindComponent("data_bar_controller", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_DataManagementBar dataManagementBar;

	[XuiBindComponent("GameSaveStorageComboBox", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<UserDataStorageType> cbxGameSaveStorage;

	[XuiBindComponent("WorldSaveStorage", false)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxEnum<UserDataStorageType> cbxWorldSaveStorage;

	[PublicizedFrom(EAccessModifier.Private)]
	public PrefabPreviewManager prefabPreviewManagerInstance;

	[PublicizedFrom(EAccessModifier.Private)]
	public long pendingBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public long totalAvailableBytes;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool validCountyName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string countyName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string statusText;

	[PublicizedFrom(EAccessModifier.Private)]
	public float oldFogDensity;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isClosing;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] biomeToUiName = new string[] { "xuiPineForest", "xuiBurntForest", "xuiDesert", "xuiSnow", "xuiWasteland" };

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isGenerating;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isCancelling;

	public enum PreviewQuality
	{
		NoPreview,
		Lowest,
		Low,
		Default,
		High,
		Highest
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Private)]
	public class XUiC_RwgBiome : XUiController
	{
		public event Action ValueChanged;

		public int BiomeIdx
		{
			get
			{
				return this.biomeIdx;
			}
			set
			{
				this.biomeIdx = value;
				this.combo.Value = (long)((this.BiomeIdx < WorldBuilderConstants.BiomeWeightDefaults.Length) ? WorldBuilderConstants.BiomeWeightDefaults[this.BiomeIdx] : 0);
				this.IsDirty = true;
			}
		}

		[XuiXmlBinding("biomename")]
		public string BiomeName
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (this.BiomeIdx >= XUiC_WorldGenerationWindow.biomeToUiName.Length)
				{
					return "";
				}
				return XUiC_WorldGenerationWindow.biomeToUiName[this.BiomeIdx];
			}
		}

		[XuiXmlBinding("biomecolor")]
		public Color BiomeColor
		{
			[PublicizedFrom(EAccessModifier.Private)]
			get
			{
				if (this.BiomeIdx >= WorldBuilderConstants.biomeColorList.Count)
				{
					return Color.clear;
				}
				return (WorldBuilderConstants.biomeColorList[this.BiomeIdx] * 0.7f).WithAlpha(1f);
			}
		}

		public int Value
		{
			get
			{
				return (int)this.combo.Value;
			}
		}

		[XuiBindEvent("OnValueChanged", "combo")]
		[PublicizedFrom(EAccessModifier.Private)]
		public void comboValueChanged(XUiController _sender, long _oldValue, long _newValue)
		{
			Action valueChanged = this.ValueChanged;
			if (valueChanged == null)
			{
				return;
			}
			valueChanged();
		}

		public override void Update(float _dt)
		{
			base.Update(_dt);
			base.handleDirtyUpdateDefault();
		}

		public void SetLabel(string _text)
		{
			this.combo.ValueTextOverride = _text;
		}

		[XuiBindComponent(true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_ComboBoxInt combo;

		[PublicizedFrom(EAccessModifier.Private)]
		public int biomeIdx;
	}
}
