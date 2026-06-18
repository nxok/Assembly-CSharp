using System;
using System.Runtime.CompilerServices;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsVideoSimplified : XUiC_OptionsVideoBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void initUpscalerModeOptions()
	{
		if (this.optionUpscaler == null || this.comboUpscaler == null)
		{
			return;
		}
		this.optionUpscaler.GetSettingValue = delegate
		{
			int num = XUiC_OptionsVideoSimplified.<initUpscalerModeOptions>g__GamePrefUpscalerModeIndex|3_4();
			if (num == -1)
			{
				Log.Out(string.Format("Upscaler mode \"{0}\" is unsupported on this platform; defaulting to \"{1}\".", GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode), 2));
				num = 0;
				GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, 2);
			}
			this.comboUpscaler.SelectedIndex = num;
		};
		this.optionUpscaler.DiscardChanges = delegate
		{
			this.comboUpscaler.SelectedIndex = XUiC_OptionsVideoSimplified.<initUpscalerModeOptions>g__GamePrefUpscalerModeIndex|3_4();
		};
		this.optionUpscaler.ApplyChanges = delegate(bool _)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, this.<initUpscalerModeOptions>g__SelectionToUpscalerMode|3_5());
		};
		this.optionUpscaler.IsChangedDelegate = () => this.comboUpscaler.SelectedIndex != XUiC_OptionsVideoSimplified.<initUpscalerModeOptions>g__GamePrefUpscalerModeIndex|3_4();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initQualityPresetBasedOptions()
	{
		this.optionQualityPreset.MapGamePrefToListIndex = delegate(int _prefValue)
		{
			XUiC_OptionsVideoSimplified.GraphicsMode graphicsMode;
			if (_prefValue != 6)
			{
				if (_prefValue != 8)
				{
					graphicsMode = XUiC_OptionsVideoSimplified.GraphicsMode.ConsoleCustom;
				}
				else
				{
					graphicsMode = XUiC_OptionsVideoSimplified.GraphicsMode.ConsoleQuality;
				}
			}
			else
			{
				graphicsMode = XUiC_OptionsVideoSimplified.GraphicsMode.ConsolePerformance;
			}
			return (int)graphicsMode;
		};
		this.optionQualityPreset.MapListIndexToGamePref = delegate(int _index)
		{
			GameOptionsPlatforms.GfxPreset gfxPreset;
			if (_index != 0)
			{
				if (_index != 1)
				{
					gfxPreset = GameOptionsPlatforms.GfxPreset.Custom;
				}
				else
				{
					gfxPreset = GameOptionsPlatforms.GfxPreset.ConsoleQuality;
				}
			}
			else
			{
				gfxPreset = GameOptionsPlatforms.GfxPreset.ConsolePerformance;
			}
			return (int)gfxPreset;
		};
	}

	public static event Action OnSettingsChanged;

	public override void Init()
	{
		base.Init();
		XUiC_OptionsVideoSimplified.ID = base.WindowGroup.Id;
		this.initUpscalerModeOptions();
		this.initQualityPresetBasedOptions();
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.updateCustomModeVisibility();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void updateCustomModeVisibility()
	{
		this.comboQualityPreset.MaxIndex = Math.Max(this.comboQualityPreset.SelectedIndex, 1);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void afterChangesSaved()
	{
		base.afterChangesSaved();
		this.updateCustomModeVisibility();
		GameOptionsManager.SetGraphicsQuality();
		GameOptionsManager.ApplyAllOptions(this.xui.playerUI);
		Action onSettingsChanged = XUiC_OptionsVideoSimplified.OnSettingsChanged;
		if (onSettingsChanged == null)
		{
			return;
		}
		onSettingsChanged();
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static int <initUpscalerModeOptions>g__GamePrefUpscalerModeIndex|3_4()
	{
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode);
		int num;
		if (@int != 2)
		{
			if (@int != 4)
			{
				num = -1;
			}
			else
			{
				num = 1;
			}
		}
		else
		{
			num = 0;
		}
		return num;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public int <initUpscalerModeOptions>g__SelectionToUpscalerMode|3_5()
	{
		int selectedIndex = this.comboUpscaler.SelectedIndex;
		int num;
		if (selectedIndex != 0)
		{
			if (selectedIndex != 1)
			{
				num = GameOptionsPlatforms.DefaultUpscalerMode;
			}
			else
			{
				num = 4;
			}
		}
		else
		{
			num = 2;
		}
		return num;
	}

	[XuiBindComponent("OptionConsoleUpscaler", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionUpscaler;

	[XuiBindComponent("ConsoleUpscaler", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboUpscaler;

	[XuiBindComponent("OptionQualityPreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefIntIndex optionQualityPreset;

	[XuiBindComponent("QualityPreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboQualityPreset;

	public static string ID = "";

	public enum UpscalerMode
	{
		FSR3,
		Scale
	}

	public enum GraphicsMode
	{
		ConsolePerformance,
		ConsoleQuality,
		ConsoleCustom
	}
}
