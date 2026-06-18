using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_OptionsVideo : XUiC_OptionsVideoBase
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void initResolutionOptions()
	{
		if (this.optionResolution == null || this.comboResolution == null || this.optionFullscreen == null || this.comboFullscreen == null)
		{
			return;
		}
		this.optionFullscreen.GetSettingValue = delegate
		{
			FullScreenMode item = PlatformApplicationManager.Application.ScreenOptions.Item3;
			this.comboFullscreen.SelectedIndex = XUiC_OptionsVideo.<initResolutionOptions>g__ConvertFullScreenModeToIndex|6_9(item);
		};
		this.optionFullscreen.DiscardChanges = delegate
		{
			FullScreenMode item2 = PlatformApplicationManager.Application.ScreenOptions.Item3;
			this.comboFullscreen.SelectedIndex = XUiC_OptionsVideo.<initResolutionOptions>g__ConvertFullScreenModeToIndex|6_9(item2);
		};
		this.optionFullscreen.ApplyChanges = delegate(bool _)
		{
		};
		this.optionFullscreen.IsChangedDelegate = delegate
		{
			FullScreenMode item3 = PlatformApplicationManager.Application.ScreenOptions.Item3;
			return this.comboFullscreen.SelectedIndex != XUiC_OptionsVideo.<initResolutionOptions>g__ConvertFullScreenModeToIndex|6_9(item3);
		};
		this.optionResolution.GetSettingValue = delegate
		{
			this.comboResolution.Elements.Clear();
			foreach (Resolution resolution in PlatformApplicationManager.Application.SupportedResolutions)
			{
				XUiC_OptionsVideo.ResolutionInfo resolutionInfo = new XUiC_OptionsVideo.ResolutionInfo(resolution.width, resolution.height);
				if (!this.comboResolution.Elements.Contains(resolutionInfo))
				{
					this.comboResolution.Elements.Add(resolutionInfo);
				}
			}
			this.<initResolutionOptions>g__GetCurrentResolution|6_8();
		};
		this.optionResolution.DiscardChanges = delegate
		{
			this.comboResolution.SelectedIndex = this.comboResolution.Elements.IndexOf(this.lastResolution);
		};
		this.optionResolution.ApplyChanges = delegate(bool _)
		{
			XUiC_OptionsVideo.ResolutionInfo value = this.comboResolution.Value;
			GameOptionsManager.SetResolution(value.Width, value.Height, XUiC_OptionsVideo.<initResolutionOptions>g__ConvertIndexToFullScreenMode|6_10(this.comboFullscreen.SelectedIndex));
			this.<initResolutionOptions>g__GetCurrentResolution|6_8();
		};
		this.optionResolution.IsChangedDelegate = () => !this.comboResolution.Value.Equals(this.lastResolution);
	}

	public static bool Fsr3Supported
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return FSR3.FSR3Supported();
		}
	}

	public static bool DlssSupported
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return XUiC_OptionsVideo.Fsr3Supported && DLSS.DLSSSupported();
		}
	}

	[XuiXmlBinding("upscaler_mode")]
	public XUiC_OptionsVideo.UpscalerMode SelectedUpscalerMode
	{
		get
		{
			XUiC_ComboBoxList<string> xuiC_ComboBoxList = this.comboUpscaler;
			if (xuiC_ComboBoxList == null)
			{
				return XUiC_OptionsVideo.UpscalerMode.Off;
			}
			return (XUiC_OptionsVideo.UpscalerMode)xuiC_ComboBoxList.SelectedIndex;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initUpscalerModeOptions()
	{
		if (this.optionUpscaler == null || this.comboUpscaler == null)
		{
			return;
		}
		this.optionUpscaler.GetSettingValue = delegate
		{
			if (!XUiC_OptionsVideo.DlssSupported)
			{
				this.comboUpscaler.MaxIndex = 3;
			}
			if (!XUiC_OptionsVideo.Fsr3Supported)
			{
				this.comboUpscaler.MaxIndex = 2;
			}
			int num = XUiC_OptionsVideo.<initUpscalerModeOptions>g__GamePrefUpscalerModeIndex|16_4();
			if (num == -1)
			{
				if (XUiC_OptionsVideo.Fsr3Supported)
				{
					Log.Out(string.Format("Upscaler mode \"{0}\" is unsupported on this platform; defaulting to \"{1}\".", GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode), 2));
					num = 3;
					GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, 2);
				}
				else
				{
					Log.Out(string.Format("Upscaler mode \"{0}\" is unsupported on this platform; defaulting to \"{1}\".", GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode), 4));
					num = 2;
					GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, 4);
				}
			}
			this.comboUpscaler.SelectedIndex = num;
		};
		this.optionUpscaler.DiscardChanges = delegate
		{
			this.comboUpscaler.SelectedIndex = XUiC_OptionsVideo.<initUpscalerModeOptions>g__GamePrefUpscalerModeIndex|16_4();
		};
		this.optionUpscaler.ApplyChanges = delegate(bool _)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxUpscalerMode, this.<initUpscalerModeOptions>g__SelectionToUpscalerMode|16_5());
		};
		this.optionUpscaler.IsChangedDelegate = () => this.comboUpscaler.SelectedIndex != XUiC_OptionsVideo.<initUpscalerModeOptions>g__GamePrefUpscalerModeIndex|16_4();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void initQualityPresetBasedOptions()
	{
		this.optionQualityPreset.MapGamePrefToListIndex = delegate(int _prefValue)
		{
			switch (_prefValue)
			{
			case 0:
				return 0;
			case 1:
				return 1;
			case 2:
				return 2;
			case 3:
				return 3;
			case 4:
				return 4;
			case 6:
				return 6;
			case 8:
				return 7;
			}
			return 5;
		};
		this.optionQualityPreset.MapListIndexToGamePref = delegate(int _index)
		{
			switch (_index)
			{
			case 0:
				return 0;
			case 1:
				return 1;
			case 2:
				return 2;
			case 3:
				return 3;
			case 4:
				return 4;
			case 6:
				return 6;
			case 7:
				return 8;
			}
			return 5;
		};
		this.optionViewDistance.MapGamePrefToListIndex = delegate(int _prefValue)
		{
			int num;
			if (_prefValue != 5)
			{
				if (_prefValue != 6)
				{
					num = 2;
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
		};
		this.optionViewDistance.MapListIndexToGamePref = delegate(int _index)
		{
			int num2;
			if (_index != 0)
			{
				if (_index != 1)
				{
					num2 = 7;
				}
				else
				{
					num2 = 6;
				}
			}
			else
			{
				num2 = 5;
			}
			return num2;
		};
		if ((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX | DeviceFlag.PS5).IsCurrent())
		{
			this.comboQualityPreset.Elements.Add("ConsolePerformance");
			this.comboQualityPreset.Elements.Add("ConsoleQuality");
		}
		XUiC_OptionEntryAbs[] allOptions = this.AllOptions;
		for (int i = 0; i < allOptions.Length; i++)
		{
			XUiC_OptionEntryGamePrefAbs xuiC_OptionEntryGamePrefAbs = allOptions[i] as XUiC_OptionEntryGamePrefAbs;
			List<object> list;
			if (xuiC_OptionEntryGamePrefAbs != null && xuiC_OptionEntryGamePrefAbs.GamePref != null && GameOptionsManager.QualityPresets.TryGetValue(xuiC_OptionEntryGamePrefAbs.GamePref.Value, out list))
			{
				xuiC_OptionEntryGamePrefAbs.ValueChanged += this.OnQualityBasedOptionChanged;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnQualityBasedOptionChanged(XUiC_OptionEntryAbs _option)
	{
		if (this.changingFromCode)
		{
			return;
		}
		this.comboQualityPreset.SelectedIndex = 5;
	}

	[XuiBindEvent("OnValueChanged", "comboQualityPreset")]
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnQualityPresetChanged(XUiController _sender, string _oldValue, string _newValue)
	{
		GameOptionsPlatforms.GfxPreset gfxPreset = (GameOptionsPlatforms.GfxPreset)this.optionQualityPreset.SelectedValue;
		if (gfxPreset == GameOptionsPlatforms.GfxPreset.Custom)
		{
			return;
		}
		this.changingFromCode = true;
		int num = (int)gfxPreset;
		XUiC_OptionEntryAbs[] allOptions = this.AllOptions;
		for (int i = 0; i < allOptions.Length; i++)
		{
			XUiC_OptionEntryGamePrefAbs xuiC_OptionEntryGamePrefAbs = allOptions[i] as XUiC_OptionEntryGamePrefAbs;
			List<object> list;
			if (xuiC_OptionEntryGamePrefAbs != null && xuiC_OptionEntryGamePrefAbs.GamePref != null && GameOptionsManager.QualityPresets.TryGetValue(xuiC_OptionEntryGamePrefAbs.GamePref.Value, out list) && num < list.Count && list[num] != null)
			{
				xuiC_OptionEntryGamePrefAbs.SelectedValue = list[num];
			}
		}
		this.changingFromCode = false;
	}

	public static event Action OnSettingsChanged;

	public override bool SupportsDefaults
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			XUiC_TabSelector tabSelector = this.TabSelector;
			return ((tabSelector != null) ? tabSelector.SelectedTabKey : null) != "xuiOptionsVideoQuality";
		}
	}

	public override void Init()
	{
		base.Init();
		XUiC_OptionsVideo.ID = base.WindowGroup.Id;
		this.initResolutionOptions();
		this.initUpscalerModeOptions();
		this.initQualityPresetBasedOptions();
		this.optionDynamicMeshDistance.MapGamePrefToListIndex = (int _prefValue) => this.comboDynamicMeshDistance.Elements.IndexOf(_prefValue);
		this.optionDynamicMeshDistance.MapListIndexToGamePref = (int _index) => this.comboDynamicMeshDistance.Elements[_index];
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void afterChangesSaved()
	{
		base.afterChangesSaved();
		GamePrefs.Set(EnumGamePrefs.OptionsGfxMotionBlurEnabled, GamePrefs.GetInt(EnumGamePrefs.OptionsGfxMotionBlur) > 0);
		GameOptionsManager.ApplyAllOptions(this.xui.playerUI);
		QualitySettings.vSyncCount = GamePrefs.GetInt(base.VSyncCountPref);
		ReflectionManager.ApplyOptions(false);
		Action onSettingsChanged = XUiC_OptionsVideo.OnSettingsChanged;
		if (onSettingsChanged == null)
		{
			return;
		}
		onSettingsChanged();
	}

	[XuiXmlBinding("texture_quality_min")]
	public int TextureQualityMin
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameOptionsManager.CalcTextureQualityMin();
		}
	}

	[XuiXmlBinding("texture_quality_limited")]
	public bool TextureQualityLimited
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<string> xuiC_ComboBoxList = this.comboTextureQuality;
			return xuiC_ComboBoxList != null && xuiC_ComboBoxList.MinIndex > 0;
		}
	}

	[XuiXmlBinding("antialiasing_mode")]
	public int AntiAliasingMode
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			XUiC_ComboBoxList<string> xuiC_ComboBoxList = this.comboAntiAliasing;
			if (xuiC_ComboBoxList == null)
			{
				return 0;
			}
			return xuiC_ComboBoxList.SelectedIndex;
		}
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public void <initResolutionOptions>g__GetCurrentResolution|6_8()
	{
		ValueTuple<int, int, FullScreenMode> screenOptions = PlatformApplicationManager.Application.ScreenOptions;
		int item = screenOptions.Item1;
		int item2 = screenOptions.Item2;
		this.lastResolution = new XUiC_OptionsVideo.ResolutionInfo(item, item2);
		if (!this.comboResolution.Elements.Contains(this.lastResolution))
		{
			this.comboResolution.Elements.Add(this.lastResolution);
		}
		this.comboResolution.Elements.Sort();
		this.comboResolution.SelectedIndex = this.comboResolution.Elements.IndexOf(this.lastResolution);
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static int <initResolutionOptions>g__ConvertFullScreenModeToIndex|6_9(FullScreenMode _mode)
	{
		int num;
		if (_mode != null)
		{
			if (_mode == 1)
			{
				num = 1;
			}
			else
			{
				num = 0;
			}
		}
		else
		{
			num = 2;
		}
		return num;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static FullScreenMode <initResolutionOptions>g__ConvertIndexToFullScreenMode|6_10(int _index)
	{
		FullScreenMode fullScreenMode;
		if (_index != 1)
		{
			if (_index != 2)
			{
				fullScreenMode = 3;
			}
			else
			{
				fullScreenMode = 0;
			}
		}
		else
		{
			fullScreenMode = 1;
		}
		return fullScreenMode;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static int <initUpscalerModeOptions>g__GamePrefUpscalerModeIndex|16_4()
	{
		switch (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxUpscalerMode))
		{
		case 0:
			return 0;
		case 2:
			return XUiC_OptionsVideo.Fsr3Supported ? 3 : (-1);
		case 3:
			return 1;
		case 4:
			return 2;
		case 5:
			return XUiC_OptionsVideo.DlssSupported ? 4 : (-1);
		}
		return -1;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Private)]
	public int <initUpscalerModeOptions>g__SelectionToUpscalerMode|16_5()
	{
		int num;
		switch (this.comboUpscaler.SelectedIndex)
		{
		case 0:
			num = 0;
			break;
		case 1:
			num = 3;
			break;
		case 2:
			num = 4;
			break;
		case 3:
			num = (XUiC_OptionsVideo.Fsr3Supported ? 2 : GameOptionsPlatforms.DefaultUpscalerMode);
			break;
		case 4:
			num = ((XUiC_OptionsVideo.Fsr3Supported && XUiC_OptionsVideo.DlssSupported) ? 5 : GameOptionsPlatforms.DefaultUpscalerMode);
			break;
		default:
			num = GameOptionsPlatforms.DefaultUpscalerMode;
			break;
		}
		return num;
	}

	[XuiBindComponent("OptionResolution", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionResolution;

	[XuiBindComponent("Resolution", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<XUiC_OptionsVideo.ResolutionInfo> comboResolution;

	[XuiBindComponent("OptionFullscreen", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionFullscreen;

	[XuiBindComponent("Fullscreen", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboFullscreen;

	[PublicizedFrom(EAccessModifier.Private)]
	public XUiC_OptionsVideo.ResolutionInfo lastResolution;

	[XuiBindComponent("OptionUpscaler", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryCustom optionUpscaler;

	[XuiBindComponent("Upscaler", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboUpscaler;

	[XuiBindComponent("OptionQualityPreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefIntIndex optionQualityPreset;

	[XuiBindComponent("QualityPreset", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboQualityPreset;

	[XuiBindComponent("OptionViewDistance", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefIntIndex optionViewDistance;

	[XuiBindComponent("TextureQuality", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboTextureQuality;

	[XuiBindComponent("AntiAliasing", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<string> comboAntiAliasing;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool changingFromCode;

	public static string ID = "";

	[XuiBindComponent("OptionDynamicMeshDistance", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_OptionEntryGamePrefIntIndex optionDynamicMeshDistance;

	[XuiBindComponent("DynamicMeshDistance", true)]
	[PublicizedFrom(EAccessModifier.Private)]
	public readonly XUiC_ComboBoxList<int> comboDynamicMeshDistance;

	public readonly struct ResolutionInfo : IComparable<XUiC_OptionsVideo.ResolutionInfo>, IEquatable<XUiC_OptionsVideo.ResolutionInfo>
	{
		[return: TupleElementNames(new string[] { "_aspectRatio", "_aspectRatioFactor", "_aspectRatioString" })]
		public static ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string> DimensionsToAspectRatio(int _width, int _height)
		{
			if (_height == 0)
			{
				return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Unknown, 0f, "n/a");
			}
			int num = 1000 * _width / _height;
			if (num >= 1770)
			{
				if (num >= 3550)
				{
					if (num <= 3600)
					{
						return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_32_9, 3.555f, "32:9");
					}
				}
				else if (num >= 1890)
				{
					if (num <= 1900)
					{
						return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_17_9, 2.37f, "17:9");
					}
					if (num <= 2370)
					{
						if (num == 2333 || num == 2370)
						{
							return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_21_9, 2.37f, "21:9");
						}
					}
					else
					{
						if (num == 2400)
						{
							return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_24_10, 2.4f, "24:10");
						}
						if (num == 3200)
						{
							return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_32_10, 3.2f, "32:10");
						}
					}
				}
				else if (num <= 1780)
				{
					return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_16_9, 1.777f, "16:9");
				}
			}
			else if (num >= 1560)
			{
				if (num >= 1660)
				{
					if (num <= 1670)
					{
						return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_5_3, 1.666f, "5:3");
					}
				}
				else
				{
					if (num <= 1570)
					{
						return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_25_16, 1.562f, "25:16");
					}
					if (num == 1600)
					{
						return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_16_10, 1.6f, "16:10");
					}
				}
			}
			else if (num >= 1330)
			{
				if (num <= 1340)
				{
					return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_4_3, 1.333f, "4:3");
				}
				if (num == 1500)
				{
					return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_3_2, 1.5f, "3:2");
				}
			}
			else
			{
				if (num == 1000)
				{
					return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_1_1, 1f, "1:1");
				}
				if (num == 1250)
				{
					return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Aspect_5_4, 1.25f, "5:4");
				}
			}
			float num2 = (float)_width / (float)_height;
			return new ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string>(XUiC_OptionsVideo.ResolutionInfo.EAspectRatio.Unknown, num2, num2.ToCultureInvariantString("0.##") + ":1");
		}

		public ResolutionInfo(int _width, int _height)
		{
			this.Width = _width;
			if (_height > _width)
			{
				_height = _width;
			}
			this.Height = _height;
			ValueTuple<XUiC_OptionsVideo.ResolutionInfo.EAspectRatio, float, string> valueTuple = XUiC_OptionsVideo.ResolutionInfo.DimensionsToAspectRatio(_width, _height);
			this.AspectRatio = valueTuple.Item1;
			string item = valueTuple.Item3;
			this.label = string.Concat(new string[]
			{
				_width.ToString(),
				"x",
				_height.ToString(),
				" (",
				item,
				")"
			});
		}

		public int CompareTo(XUiC_OptionsVideo.ResolutionInfo _other)
		{
			int num = this.Width.CompareTo(_other.Width);
			if (num == 0)
			{
				return this.Height.CompareTo(_other.Height);
			}
			return num;
		}

		public bool Equals(XUiC_OptionsVideo.ResolutionInfo _other)
		{
			return this.Width == _other.Width && this.Height == _other.Height;
		}

		public override string ToString()
		{
			return this.label;
		}

		public readonly int Width;

		public readonly int Height;

		public readonly XUiC_OptionsVideo.ResolutionInfo.EAspectRatio AspectRatio;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly string label;

		public enum EAspectRatio
		{
			Aspect_32_9,
			Aspect_32_10,
			Aspect_24_10,
			Aspect_21_9,
			Aspect_17_9,
			Aspect_16_9,
			Aspect_5_3,
			Aspect_16_10,
			Aspect_25_16,
			Aspect_3_2,
			Aspect_4_3,
			Aspect_5_4,
			Aspect_1_1,
			Unknown
		}
	}

	public enum UpscalerMode
	{
		Off,
		Dynamic,
		Scale,
		FSR3,
		DLSS
	}

	public enum GraphicsMode
	{
		Lowest,
		Low,
		Medium,
		High,
		Ultra,
		Custom,
		ConsolePerformance,
		ConsoleQuality
	}
}
