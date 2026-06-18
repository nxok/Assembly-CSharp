using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform;
using UnityEngine;
using UnityStandardAssets.ImageEffects;

public static class GameOptionsManager
{
	public static event Action<int, int> ResolutionChanged;

	public static event Action<int> TextureQualityChanged;

	public static event Action<int> TextureFilterChanged;

	public static event Action<int> ShadowDistanceChanged;

	public static event Action OnGameOptionsApplied;

	[PublicizedFrom(EAccessModifier.Private)]
	static GameOptionsManager()
	{
		GameOptionsManager.initQualityPresets();
	}

	public static void ValidateGamePrefs()
	{
		GamePrefs.OnGamePrefChanged += GameOptionsManager.OnGamePrefChanged;
		GameOptionsManager.ValidateFoV();
		GameOptionsManager.ValidateFoV3P();
		GameOptionsManager.ValidateTreeDistance();
		GameOptionsManager.ValidateHudSize();
		GameOptionsManager.ValidateShadowDistance();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref <= EnumGamePrefs.OptionsHudSize)
		{
			if (_pref <= EnumGamePrefs.OptionsGfxShadowDistance)
			{
				if (_pref == EnumGamePrefs.OptionsOverallAudioVolumeLevel)
				{
					AudioListener.volume = GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel);
					return;
				}
				if (_pref != EnumGamePrefs.OptionsGfxShadowDistance)
				{
					return;
				}
				GameOptionsManager.ValidateShadowDistance();
				return;
			}
			else
			{
				if (_pref == EnumGamePrefs.OptionsGfxFOV)
				{
					GameOptionsManager.ValidateFoV();
					return;
				}
				if (_pref != EnumGamePrefs.OptionsHudSize)
				{
					return;
				}
				GameOptionsManager.ValidateHudSize();
				return;
			}
		}
		else if (_pref <= EnumGamePrefs.OptionsGfxTreeDistance)
		{
			if (_pref == EnumGamePrefs.OptionsGfxWaterPtlLimiter)
			{
				WaterSplashCubes.particleLimiter = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter);
				return;
			}
			if (_pref != EnumGamePrefs.OptionsGfxTreeDistance)
			{
				return;
			}
			GameOptionsManager.ValidateTreeDistance();
			return;
		}
		else
		{
			if (_pref == EnumGamePrefs.OptionsMumblePositionalAudioSupport)
			{
				GameOptionsManager.UpdateMumblePositionalAudioState();
				return;
			}
			if (_pref != EnumGamePrefs.OptionsGfxFOV3P)
			{
				return;
			}
			GameOptionsManager.ValidateFoV3P();
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateFoV()
	{
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV);
		if (@int < Constants.cMinCameraFieldOfView)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, Constants.cMinCameraFieldOfView);
			return;
		}
		if (@int > Constants.cMaxCameraFieldOfView)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV, Constants.cMaxCameraFieldOfView);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateFoV3P()
	{
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV3P);
		if (@int < Constants.cMinCameraFieldOfView)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV3P, Constants.cMinCameraFieldOfView);
			return;
		}
		if (@int > Constants.cMaxCameraFieldOfView)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxFOV3P, Constants.cMaxCameraFieldOfView);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateTreeDistance()
	{
		if (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTreeDistance) < 2)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxTreeDistance, 2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateHudSize()
	{
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsHudSize);
		if ((double)@float < 0.01)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsHudSize, 1f);
			return;
		}
		if ((double)@float < 0.5)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsHudSize, 0.5f);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void UpdateMumblePositionalAudioState()
	{
		if (GamePrefs.GetBool(EnumGamePrefs.OptionsMumblePositionalAudioSupport))
		{
			MumblePositionalAudio.Init();
			return;
		}
		MumblePositionalAudio.Destroy();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ValidateShadowDistance()
	{
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
		if (@int >= 5 && @int < 20)
		{
			GamePrefs.Set(EnumGamePrefs.OptionsGfxShadowDistance, 20);
		}
	}

	public static void ApplyAllOptions(LocalPlayerUI _playerUi)
	{
		QualitySettings.antiAliasing = 0;
		float streamingMipmapBudget = GameOptionsPlatforms.GetStreamingMipmapBudget();
		QualitySettings.streamingMipmapsMemoryBudget = streamingMipmapBudget;
		Log.Out("ApplyAllOptions streaming budget {0} MB", new object[] { streamingMipmapBudget });
		QualitySettings.softParticles = GamePrefs.GetFloat(EnumGamePrefs.OptionsGfxWaterPtlLimiter) >= 0.51f;
		GameOptionsManager.ApplyScreenResolution();
		AudioListener.volume = Math.Min(GamePrefs.GetFloat(EnumGamePrefs.OptionsOverallAudioVolumeLevel), 1f);
		GameOptionsManager.ApplyShadowQuality();
		GameOptionsManager.ApplyTextureQuality(-1);
		GameOptionsManager.ApplyTextureFilter();
		GameOptionsManager.ApplyCameraOptions(null);
		Shader.globalMaximumLOD = 400 + GamePrefs.GetInt(EnumGamePrefs.OptionsGfxObjQuality) * 100;
		QualitySettings.lodBias = GameOptionsManager.GetLODBias();
		GameOptionsManager.ApplyTerrainOptions();
		MeshDescription.SetGrassQuality();
		MeshDescription.SetWaterQuality();
		SignTextureManager.Instance.SetQuality((SignTextureManager.SignTextureQuality)GamePrefs.GetInt(EnumGamePrefs.OptionsGfxSignQuality));
		SignTextureManager.Instance.SetTileSizeForCurrentQuality();
		Action onGameOptionsApplied = GameOptionsManager.OnGameOptionsApplied;
		if (onGameOptionsApplied == null)
		{
			return;
		}
		onGameOptionsApplied();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyScreenResolution()
	{
		ValueTuple<int, int, FullScreenMode> screenOptions = PlatformApplicationManager.Application.ScreenOptions;
		int item = screenOptions.Item1;
		int item2 = screenOptions.Item2;
		FullScreenMode item3 = screenOptions.Item3;
		Resolution currentResolution = PlatformApplicationManager.Application.GetCurrentResolution();
		Log.Out("ApplyAllOptions current screen {0} x {1}, {2}hz, window {3} x {4}, mode {5}", new object[]
		{
			currentResolution.width,
			currentResolution.height,
			currentResolution.refreshRateRatio.value,
			Screen.width,
			Screen.height,
			Screen.fullScreenMode
		});
		GameOptionsManager.SetResolution(item, item2, item3);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ApplyShadowQuality()
	{
		Vector3 vector;
		vector..ctor(0.06f, 0.15f, 0.35f);
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowDistance);
		switch (@int)
		{
		case 0:
			QualitySettings.shadowDistance = 35f;
			QualitySettings.shadowCascades = 2;
			QualitySettings.shadowCascade2Split = 0.33f;
			goto IL_00C8;
		case 1:
			QualitySettings.shadowDistance = 80f;
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowCascade4Split = vector;
			goto IL_00C8;
		case 2:
			QualitySettings.shadowDistance = 120f;
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowCascade4Split = vector;
			goto IL_00C8;
		case 3:
			QualitySettings.shadowDistance = 200f;
			QualitySettings.shadowCascades = 4;
			QualitySettings.shadowCascade4Split = vector * 0.8f;
			goto IL_00C8;
		}
		QualitySettings.shadowDistance = 300f;
		QualitySettings.shadowCascades = 4;
		QualitySettings.shadowCascade4Split = vector * 0.6f;
		IL_00C8:
		if (GameOptionsManager.ShadowDistanceChanged != null)
		{
			GameOptionsManager.ShadowDistanceChanged(@int);
		}
		switch (GamePrefs.GetInt(EnumGamePrefs.OptionsGfxShadowQuality))
		{
		case 0:
			QualitySettings.shadows = 0;
			return;
		case 1:
			QualitySettings.shadows = 1;
			QualitySettings.shadowResolution = 1;
			return;
		case 2:
			QualitySettings.shadows = 2;
			QualitySettings.shadowResolution = 1;
			return;
		case 3:
			QualitySettings.shadows = 2;
			QualitySettings.shadowResolution = 2;
			return;
		case 4:
			QualitySettings.shadows = 2;
			QualitySettings.shadowResolution = 2;
			return;
		case 5:
			QualitySettings.shadows = 2;
			QualitySettings.shadowResolution = 3;
			return;
		default:
			return;
		}
	}

	public static float GetLODBias()
	{
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxObjQuality);
		switch (@int)
		{
		case 0:
			return 0.5f;
		case 1:
			return 0.65f;
		case 2:
			return 0.8f;
		case 3:
			return 1.2f;
		case 4:
			return 1.7f;
		default:
			return (float)@int / 100f;
		}
	}

	public static void CheckResolution()
	{
		if (GameOptionsManager.screenExclusiveCheckDelay > 0 && --GameOptionsManager.screenExclusiveCheckDelay == 0)
		{
			GameOptionsManager.screenExclusiveCheckDelay = 10;
			if (Screen.width != GameOptionsManager.screenWidth || Screen.height != GameOptionsManager.screenHeight)
			{
				Log.Warning("Fullscreen Exclusive failed! Reverting to {0} x {1}", new object[]
				{
					GameOptionsManager.screenWidth,
					GameOptionsManager.screenHeight
				});
				GameOptionsManager.SetResolution(GameOptionsManager.screenWidth, GameOptionsManager.screenHeight, 3);
			}
		}
	}

	public static void SetResolution(int _width, int _height, FullScreenMode _fullscreen = 3)
	{
		if (Screen.width == _width && Screen.height == _height && Screen.fullScreenMode == _fullscreen)
		{
			return;
		}
		Resolution currentResolution = PlatformApplicationManager.Application.GetCurrentResolution();
		Log.Out("SetResolution was screen {0} x {1}, {2}hz, window {3} x {4}, mode {5}", new object[]
		{
			currentResolution.width,
			currentResolution.height,
			currentResolution.refreshRateRatio.value,
			Screen.width,
			Screen.height,
			Screen.fullScreenMode
		});
		Log.Out("SetResolution to {0} x {1}, mode {2}", new object[] { _width, _height, _fullscreen });
		GameOptionsManager.screenWidth = _width;
		GameOptionsManager.screenHeight = _height;
		GameOptionsManager.screenExclusiveCheckDelay = ((_fullscreen == null) ? 10 : 0);
		PlatformApplicationManager.Application.SetResolution(_width, _height, _fullscreen);
		if (GameOptionsManager.ResolutionChanged != null)
		{
			GameOptionsManager.ResolutionChanged(_width, _height);
		}
	}

	public static int GetTextureQuality(int _overrideValue = -1)
	{
		if (_overrideValue != -1)
		{
			return _overrideValue;
		}
		int num = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTexQuality);
		if (Constants.Is32BitOs && num < 2)
		{
			num = 2;
		}
		return Utils.FastMax(num, GameOptionsManager.CalcTextureQualityMin());
	}

	public static int CalcTextureQualityMin()
	{
		return GameOptionsPlatforms.CalcTextureQualityMin();
	}

	public static void ApplyTextureQuality(int _overrideValue = -1)
	{
		int textureQuality = GameOptionsManager.GetTextureQuality(_overrideValue);
		QualitySettings.streamingMipmapsActive = true;
		QualitySettings.streamingMipmapsMaxLevelReduction = Math.Max(3, GameRenderManager.TextureMipmapLimit);
		GameRenderManager.TextureMipmapLimit = textureQuality;
		float num = 0.6776996f;
		if (textureQuality > 0)
		{
			if (textureQuality == 1)
			{
				num = 0.6f;
			}
			else if (textureQuality == 2)
			{
				num = 0.5f;
			}
			else if (textureQuality == 3)
			{
				num = 0.4f;
			}
		}
		Shader.SetGlobalFloat("_MipSlope", num);
		if (GameOptionsManager.TextureQualityChanged != null)
		{
			GameOptionsManager.TextureQualityChanged(textureQuality);
		}
		Log.Out("Texture quality is set to " + GameRenderManager.TextureMipmapLimit.ToString());
	}

	public static int GetTextureFilter()
	{
		return GameOptionsPlatforms.ApplyTextureFilterLimit(GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTexFilter));
	}

	public static void ApplyTextureFilter()
	{
		int textureFilter = GameOptionsManager.GetTextureFilter();
		QualitySettings.anisotropicFiltering = ((textureFilter == 0) ? 0 : ((textureFilter <= 3) ? 1 : 2));
		if (GameOptionsManager.TextureFilterChanged != null)
		{
			GameOptionsManager.TextureFilterChanged(textureFilter);
		}
		Log.Out("ApplyTextureFilter {0}, AF {1}", new object[]
		{
			textureFilter,
			QualitySettings.anisotropicFiltering
		});
	}

	public static void ApplyCameraOptions(EntityPlayerLocal _playerLocal = null)
	{
		bool flag = false;
		GameRenderManager.ApplyCameraOptions(_playerLocal);
		Camera[] array;
		if (_playerLocal)
		{
			array = _playerLocal.GetComponentsInChildren<Camera>();
		}
		else
		{
			array = Camera.allCameras;
		}
		foreach (Camera camera in array)
		{
			if ((camera.cullingMask & 4096) == 0)
			{
				DepthOfField depthOfField;
				if (camera.TryGetComponent<DepthOfField>(ref depthOfField))
				{
					depthOfField.enabled = flag;
				}
				camera.allowHDR = true;
				int num = (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled) ? GamePrefs.GetInt(EnumGamePrefs.OptionsGfxFOV) : Constants.cDefaultCameraFieldOfView);
				camera.fieldOfView = (float)num;
				camera.renderingPath = 3;
				if (QualitySettings.antiAliasing != 0 && camera.actualRenderingPath == 3)
				{
					Log.Warning("QualitySettings antialiasing has been enabled but the rendering path is set to deferred. This is incompatible and wastes memory and so will be disabled");
					QualitySettings.antiAliasing = 0;
				}
				camera.farClipPlane = 2000f;
			}
		}
	}

	public static void ApplyTerrainOptions()
	{
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxTerrainQuality);
		if (@int <= 1)
		{
			Shader.EnableKeyword("GAME_TERRAINLOWQ");
		}
		else
		{
			Shader.DisableKeyword("GAME_TERRAINLOWQ");
		}
		if (@int == 0)
		{
			Shader.DisableKeyword("_MAX3LAYER");
			Shader.EnableKeyword("_MAX2LAYER");
		}
		else if (@int <= 1)
		{
			Shader.DisableKeyword("_MAX2LAYER");
			Shader.EnableKeyword("_MAX3LAYER");
		}
		else
		{
			Shader.DisableKeyword("_MAX2LAYER");
			Shader.DisableKeyword("_MAX3LAYER");
		}
		Log.Out("ApplyTerrainOptions {0}", new object[] { @int });
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void initQualityPresets()
	{
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxAA] = new List<object> { 0, 1, 2, 4, 4, null, 3, null, 3 };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxAASharpness] = new List<object> { 0f, 0f, 0f, 0.5f, 0.6f, null, 0.85f, null, 0.85f };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxMotionBlur] = new List<object> { 0, 0, 1, 1, 2, null, 2, null, 2 };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxTexQuality] = new List<object>
		{
			3,
			2,
			1,
			0,
			0,
			null,
			DeviceFlag.XBoxSeriesS.IsCurrent() ? 1 : 0,
			null,
			DeviceFlag.XBoxSeriesS.IsCurrent() ? 1 : 0
		};
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxTexFilter] = new List<object> { 0, 0, 1, 2, 3, null, 3, null, 3 };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxReflectQuality] = new List<object> { 0, 0, 1, 2, 3, null, 3, null, 3 };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxReflectShadows] = new List<object> { false, false, false, false, true, null, true, null, true };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxShadowDistance] = new List<object> { 0, 0, 1, 2, 3, null, 1, null, 3 };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxShadowQuality] = new List<object>
		{
			0,
			1,
			2,
			3,
			4,
			null,
			2,
			null,
			DeviceFlag.XBoxSeriesS.IsCurrent() ? 2 : 3
		};
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxLODDistance] = new List<object> { 0f, 0.25f, 0.5f, 0.75f, 1f, null, 0.75f, null, 0.75f };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxTerrainQuality] = new List<object> { 0, 1, 2, 3, 4, null, 2, null, 4 };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxObjQuality] = new List<object>
		{
			0,
			1,
			2,
			3,
			4,
			null,
			2,
			null,
			DeviceFlag.XBoxSeriesS.IsCurrent() ? 3 : 4
		};
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxGrassDistance] = new List<object> { 0, 1, 2, 3, 3, null, 2, null, 3 };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxBloom] = new List<object> { false, false, true, true, true, null, true, null, true };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxDOF] = new List<object> { false, false, false, false, false, null, false, null, false };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxSSAO] = new List<object> { false, false, true, true, true, null, true, null, true };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxSSReflections] = new List<object> { 0, 1, 1, 2, 3, null, 1, null, 1 };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxSunShafts] = new List<object> { false, false, true, true, true, null, true, null, true };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxWaterQuality] = new List<object> { 0, 0, 1, 1, 1, null, 1, null, 1 };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxWaterPtlLimiter] = new List<object> { 0f, 0.2f, 0.5f, 0.75f, 1f, null, 0.75f, null, 0.75f };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxSignQuality] = new List<object> { 0, 1, 2, 3, 4, null, 2, null, 2 };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxOcclusion] = new List<object> { false, true, true, true, true, null, true, null, true };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxViewDistance] = new List<object> { 5, 5, 6, 6, 7, null, 6, null, 6 };
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxDynamicScale] = new List<object>
		{
			null,
			null,
			null,
			null,
			null,
			null,
			DeviceFlag.XBoxSeriesS.IsCurrent() ? 0.6f : 0.5f,
			null,
			0.75f
		};
		GameOptionsManager.QualityPresets[EnumGamePrefs.OptionsGfxFSRPreset] = new List<object> { null, null, null, null, null, null, 0, null, 2 };
	}

	public static void SetGraphicsQuality()
	{
		int @int = GamePrefs.GetInt(EnumGamePrefs.OptionsGfxQualityPreset);
		if (@int < 0)
		{
			Log.Warning(string.Format("SetGraphicsQuality: Selected preset is negative ({0})", @int));
			return;
		}
		foreach (KeyValuePair<EnumGamePrefs, List<object>> keyValuePair in GameOptionsManager.QualityPresets)
		{
			EnumGamePrefs enumGamePrefs;
			List<object> list;
			keyValuePair.Deconstruct(out enumGamePrefs, out list);
			EnumGamePrefs enumGamePrefs2 = enumGamePrefs;
			List<object> list2 = list;
			if (@int >= list2.Count)
			{
				Log.Warning(string.Format("SetGraphicsQuality: Skipping GamePref {0}, selected preset ({1}) outside of defined values ({2})", enumGamePrefs2.ToStringCached<EnumGamePrefs>(), @int, list2.Count));
			}
			else
			{
				object obj = list2[@int];
				if (obj == null)
				{
					Log.Warning(string.Format("SetGraphicsQuality: Skipping GamePref {0}, selected preset ({1}) does not have a value defined for this setting", enumGamePrefs2.ToStringCached<EnumGamePrefs>(), @int));
				}
				else
				{
					GamePrefs.SetObject(enumGamePrefs2, obj);
				}
			}
		}
	}

	public static double GetUiSizeLimit(double _aspectRation)
	{
		int num = 0;
		while (_aspectRation > GameOptionsManager.uiScaleLimits[num].Item1)
		{
			num++;
		}
		return GameOptionsManager.uiScaleLimits[num].Item2;
	}

	public static double GetUiSizeLimit()
	{
		Vector2i currentScreenSize = GameOptionsManager.CurrentScreenSize;
		return GameOptionsManager.GetUiSizeLimit((double)currentScreenSize.x / (double)currentScreenSize.y);
	}

	public static float GetActiveUiScale()
	{
		float num = (float)GameOptionsManager.GetUiSizeLimit();
		float @float = GamePrefs.GetFloat(EnumGamePrefs.OptionsHudSize);
		return Utils.FastMin(num, @float);
	}

	public static Vector2i CurrentScreenSize
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return new Vector2i(Screen.width, Screen.height);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int screenWidth;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int screenHeight;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int screenExclusiveCheckDelay;

	[PublicizedFrom(EAccessModifier.Private)]
	public const int cScreenExclusiveFrameWait = 10;

	public static readonly EnumDictionary<EnumGamePrefs, List<object>> QualityPresets = new EnumDictionary<EnumGamePrefs, List<object>>();

	[TupleElementNames(new string[] { "aspectLimit", "scaleLimit" })]
	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly ValueTuple<double, double>[] uiScaleLimits = new ValueTuple<double, double>[]
	{
		new ValueTuple<double, double>(1.2, 0.65),
		new ValueTuple<double, double>(1.26, 0.7),
		new ValueTuple<double, double>(1.34, 0.75),
		new ValueTuple<double, double>(1.51, 0.85),
		new ValueTuple<double, double>(1.61, 0.9),
		new ValueTuple<double, double>(1000.0, 1.0)
	};
}
