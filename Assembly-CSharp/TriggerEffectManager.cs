using System;
using System.Collections.Generic;
using InControl;
using Platform;
using TriggerEffects;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

[Preserve]
public class TriggerEffectManager : IDisposable
{
	public bool Enabled
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this._enabled;
		}
		[PublicizedFrom(EAccessModifier.Private)]
		set
		{
			this._enabled = value;
			this._stateChanged = true;
		}
	}

	public TriggerEffectManager()
	{
		this.PollSetting();
		this._stateChanged = false;
		for (int i = 0; i < this.vibrationAudioSources.Length; i++)
		{
			this.vibrationAudioSources[i] = new AudioGamepadRumbleSource();
		}
		this.Init();
		InputManager.OnDeviceAttached += this.OnControllerConnected;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnControllerConnected(InputDevice device)
	{
		this.Shutdown();
		this.PollSetting();
		this.Init();
		this._stateChanged = true;
		GameManager.Instance.triggerEffectManager.EnableVibration();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnGamePrefChanged(EnumGamePrefs _pref)
	{
		if (_pref == EnumGamePrefs.OptionsControllerTriggerEffects)
		{
			TriggerEffectManager.SetEnabled(GamePrefs.GetBool(_pref));
			return;
		}
		if (_pref != EnumGamePrefs.OptionsControllerVibrationStrength)
		{
			return;
		}
		TriggerEffectManager.UpdateControllerVibrationStrength();
	}

	public void Init()
	{
		try
		{
			TriggerEffectDualsensePC.InitTriggerEffectManager(ref this._controllersConnected);
		}
		catch (DllNotFoundException ex)
		{
			Log.Warning(string.Format("[TriggerEffectManager] Failed to load ControllerExt, disabling. Details: {0}", ex));
			GamePrefs.Set(EnumGamePrefs.OptionsControllerTriggerEffects, false);
			return;
		}
		TriggerEffectDualsense.InitTriggerEffectManager();
		TriggerEffectManager.UpdateControllerVibrationStrength();
		this.EnableVibration();
		this.InitializeLightbarGradient();
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged += this.OnLastInputStyleChanged;
		GamePrefs.OnGamePrefChanged += this.OnGamePrefChanged;
	}

	public void EnableVibration()
	{
		TriggerEffectDualsensePC.EnableVibration();
		TriggerEffectDualsense.EnableVibration();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void InitializeLightbarGradient()
	{
		TriggerEffectManager.lightbarGradients = Resources.Load<LightbarGradients>("Data/LightBarGradients");
	}

	public static void SetEnabled(bool _enabled)
	{
		GameManager.Instance.triggerEffectManager.Enabled = _enabled;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte FloatToByte(float value)
	{
		return (byte)(Mathf.Clamp01(value) * 255f);
	}

	public void Update()
	{
		if (GameManager.Instance.World != null && TriggerEffectManager.audioRumbleStrength > 0f && !GameManager.Instance.IsPaused() && PlatformManager.NativePlatform.Input.CurrentInputStyle != PlayerInputManager.InputStyle.Keyboard)
		{
			int num = 0;
			int num2 = 0;
			this.targetLeftAudioStrength = 0f;
			this.targetRightAudioStrength = 0f;
			Transform transform;
			if (GameManager.Instance.World != null)
			{
				EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
				transform = ((primaryPlayer != null) ? primaryPlayer.cameraTransform : null);
			}
			else
			{
				transform = LocalPlayerUI.primaryUI.uiCamera.transform;
			}
			foreach (AudioGamepadRumbleSource audioGamepadRumbleSource in this.vibrationAudioSources)
			{
				if (audioGamepadRumbleSource.audioSrc != null)
				{
					if (audioGamepadRumbleSource.audioSrc.isPlaying)
					{
						float num3 = audioGamepadRumbleSource.GetSample(0);
						if (audioGamepadRumbleSource.locationBased)
						{
							float num4 = 1f - Vector3.Distance(transform.position, audioGamepadRumbleSource.audioSrc.transform.position) / audioGamepadRumbleSource.audioSrc.maxDistance;
							if (num4 < 0.9f)
							{
								goto IL_0184;
							}
							num3 *= num4;
						}
						num3 *= audioGamepadRumbleSource.strengthMultiplier * audioGamepadRumbleSource.audioSrc.pitch * TriggerEffectManager.audioRumbleStrength;
						if (num3 > 0f)
						{
							num2++;
							this.targetRightAudioStrength += num3;
						}
						else if (num3 < 0f)
						{
							num++;
							this.targetLeftAudioStrength += num3;
						}
					}
					else
					{
						audioGamepadRumbleSource.Clear();
					}
				}
				IL_0184:;
			}
			if (num > 0 || num2 > 0)
			{
				if (num > 0)
				{
					this.targetLeftAudioStrength /= (float)num;
				}
				if (num2 > 0)
				{
					this.targetRightAudioStrength /= (float)num2;
				}
				this.leftAudioStrength = Mathf.Lerp(this.leftAudioStrength, this.targetLeftAudioStrength, Time.deltaTime * 15f);
				this.rightAudioStrength = Mathf.Lerp(this.rightAudioStrength, this.targetRightAudioStrength, Time.deltaTime * 15f);
				InputManager.ActiveDevice.Vibrate(-this.leftAudioStrength, this.rightAudioStrength);
				this.SetGamepadVibration(-this.leftAudioStrength + this.rightAudioStrength);
				this.SetDualSenseVibration(this.FloatToByte(-this.leftAudioStrength * 0.25f), this.FloatToByte(this.rightAudioStrength * 0.25f));
			}
			else
			{
				this.SetDualSenseVibration(0, 0);
				InputManager.ActiveDevice.StopVibration();
				this.SetGamepadVibration(0f);
			}
		}
		TriggerEffectDualsensePC.PCConnectedUpdate(ref this._controllersConnected, this._currentEffectLeft.DualsenseEffect, this._currentEffectRight.DualsenseEffect);
		TriggerEffectDualsense.ConnectedUpdate(this._currentEffectLeft.DualsenseEffect, this._currentEffectRight.DualsenseEffect);
		if (this.Enabled && !this.inUI)
		{
			TriggerEffectDualsensePC.PCTriggerUpdate(this._stateChanged, this._currentEffectLeft.DualsenseEffect, this._currentEffectRight.DualsenseEffect);
			TriggerEffectDualsense.Update(this._currentEffectLeft.DualsenseEffect, this._currentEffectRight.DualsenseEffect);
		}
		else if (this._stateChanged && (!this.Enabled || this.inUI))
		{
			this.SetGamepadTriggerEffectOff(TriggerEffectManager.GamepadTrigger.RightTrigger);
			this.SetGamepadTriggerEffectOff(TriggerEffectManager.GamepadTrigger.LeftTrigger);
			TriggerEffectDualsensePC.DualsensePCSetEffectToOff();
			TriggerEffectDualsense.SetEffectToOff();
			this._stateChanged = true;
		}
		this._triggerSetLeft = false;
		this._triggerSetRight = false;
		this._stateChanged = false;
	}

	public void SetGamepadTriggerEffectOff(TriggerEffectManager.GamepadTrigger trigger)
	{
		if (trigger != TriggerEffectManager.GamepadTrigger.LeftTrigger)
		{
			if (trigger != TriggerEffectManager.GamepadTrigger.RightTrigger)
			{
				throw new ArgumentOutOfRangeException("trigger", trigger, null);
			}
			this._currentEffectRight = TriggerEffectManager.NoneEffect;
		}
		else
		{
			this._currentEffectLeft = TriggerEffectManager.NoneEffect;
		}
		this._stateChanged = true;
	}

	public void SetWeaponEffect(int userID, TriggerEffectManager.GamepadTrigger trigger, byte startPosition, byte endPosition, byte strength)
	{
		if (!this.Enabled)
		{
			return;
		}
		TriggerEffectDualsensePC.SetWeaponEffect(userID, trigger, startPosition, endPosition, strength);
		TriggerEffectDualsense.SetWeaponEffect(userID, trigger, startPosition, endPosition, strength);
		this._stateChanged = true;
	}

	public void ResetControllerIdentification()
	{
		TriggerEffectDualsensePC.ResetControllerIdentification();
		TriggerEffectDualsense.ResetControllerIdentification();
		this._stateChanged = true;
	}

	public void SetControllerIdentification()
	{
		TriggerEffectDualsensePC.SetControllerIdentification();
		TriggerEffectDualsense.SetControllerIdentification();
		this._stateChanged = true;
	}

	public void SetTriggerEffectVibration(int userID, TriggerEffectManager.GamepadTrigger trigger, byte position, byte amplitude, byte frequency)
	{
		if (!this.Enabled)
		{
			return;
		}
		TriggerEffectDualsensePC.SetTriggerEffectVibration(userID, trigger, position, amplitude, frequency);
		TriggerEffectDualsense.SetTriggerEffectVibration(userID, trigger, position, amplitude, frequency);
		this._stateChanged = true;
	}

	public void SetTriggerEffectVibrationMultiplePosition(int userID, TriggerEffectManager.GamepadTrigger trigger, byte[] amplitudes, byte frequency)
	{
		if (!this.Enabled)
		{
			return;
		}
		TriggerEffectDualsensePC.SetTriggerEffectVibrationMultiplePosition(userID, trigger, amplitudes, frequency);
		TriggerEffectDualsense.SetTriggerEffectVibrationMultiplePosition(userID, trigger, amplitudes, frequency);
		this._stateChanged = true;
	}

	public void Shutdown()
	{
		this.StopGamepadVibration(true);
		TriggerEffectManager.LibShutdown();
		TriggerEffectDualsense.ResetControllerIdentification();
		TriggerEffectDualsensePC.ResetControllerIdentification();
		for (int i = 1; i < 5; i++)
		{
			this._controllersConnected[i - 1] = false;
		}
		PlatformManager.NativePlatform.Input.OnLastInputStyleChanged -= this.OnLastInputStyleChanged;
		GamePrefs.OnGamePrefChanged -= this.OnGamePrefChanged;
		this.Enabled = false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void LibShutdown()
	{
		TriggerEffectDualsensePC.LibShutdown();
	}

	public void Dispose()
	{
		this.Shutdown();
		InputManager.OnDeviceAttached -= this.OnControllerConnected;
	}

	public void PollSetting()
	{
		this.Enabled = GamePrefs.GetBool(EnumGamePrefs.OptionsControllerTriggerEffects);
	}

	public bool inUI
	{
		get
		{
			return this._inUI;
		}
		set
		{
			if (this._inUI != value)
			{
				this._inUI = value;
				this._stateChanged = true;
			}
		}
	}

	public void SetGamepadVibration(float strength)
	{
		this._currentControllerVibrationStrength = strength;
		this._stateChanged = true;
	}

	public void StopGamepadVibration(bool clearSources = false)
	{
		if (clearSources)
		{
			AudioGamepadRumbleSource[] array = this.vibrationAudioSources;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].Clear();
			}
		}
		this._currentControllerVibrationStrength = 0f;
		this._stateChanged = true;
		this.SetDualSenseVibration(0, 0);
		InputManager.ActiveDevice.StopVibration();
	}

	public void SetTriggerEffect(TriggerEffectManager.ControllerTriggerEffect effect)
	{
		if (!this.Enabled)
		{
			return;
		}
		this.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.LeftTrigger, effect, false);
		this.SetTriggerEffect(TriggerEffectManager.GamepadTrigger.RightTrigger, effect, false);
	}

	public void SetTriggerEffect(TriggerEffectManager.GamepadTrigger trigger, TriggerEffectManager.ControllerTriggerEffect effect, bool asap = false)
	{
		if (!this.Enabled)
		{
			return;
		}
		this._stateChanged = true;
		if (trigger == TriggerEffectManager.GamepadTrigger.LeftTrigger)
		{
			this._currentEffectLeft.DualsenseEffect = effect.DualsenseEffect;
			if (asap)
			{
				TriggerEffectDualsensePC.ApplyImmediate(trigger, this._currentEffectLeft);
			}
			this._triggerSetLeft = true;
			return;
		}
		if (trigger != TriggerEffectManager.GamepadTrigger.RightTrigger)
		{
			return;
		}
		this._currentEffectRight.DualsenseEffect = effect.DualsenseEffect;
		if (asap)
		{
			TriggerEffectDualsensePC.ApplyImmediate(trigger, this._currentEffectRight);
		}
		this._triggerSetRight = true;
	}

	public static TriggerEffectManager.ControllerTriggerEffect GetTriggerEffect(ValueTuple<string, string> triggerEffectNames)
	{
		if (string.IsNullOrEmpty(triggerEffectNames.Item1) || string.IsNullOrEmpty(triggerEffectNames.Item2) || (triggerEffectNames.Item1.Contains("NoEffect") && triggerEffectNames.Item2.Contains("NoEffect")) || (triggerEffectNames.Item1.Contains("NoneEffect") && triggerEffectNames.Item2.Contains("NoneEffect")))
		{
			return TriggerEffectManager.NoneEffect;
		}
		TriggerEffectManager.ControllerTriggerEffect controllerTriggerEffect = new TriggerEffectManager.ControllerTriggerEffect
		{
			DualsenseEffect = TriggerEffectManager.NoneEffectDs,
			XboxTriggerEffect = TriggerEffectManager.NoneEffectXb
		};
		TriggerEffectManager.TriggerEffectDS triggerEffectDS;
		if (TriggerEffectManager.ControllerTriggerEffectsDS.TryGetValue(triggerEffectNames.Item1, out triggerEffectDS))
		{
			controllerTriggerEffect.DualsenseEffect = triggerEffectDS;
		}
		else
		{
			Log.Warning("Failed to find trigger effect DS: " + triggerEffectNames.Item1);
		}
		TriggerEffectManager.TriggerEffectXB triggerEffectXB;
		if (TriggerEffectManager.ControllerTriggerEffectsXb.TryGetValue(triggerEffectNames.Item2, out triggerEffectXB))
		{
			controllerTriggerEffect.XboxTriggerEffect = triggerEffectXB;
		}
		else
		{
			Log.Warning("Failed to find trigger effect XB: " + triggerEffectNames.Item2);
		}
		return controllerTriggerEffect;
	}

	public static TriggerEffectManager.ControllerTriggerEffect GetTriggerEffect(string dualsenseTrigger, string impulseTrigger)
	{
		if (string.IsNullOrEmpty(dualsenseTrigger) || string.IsNullOrEmpty(impulseTrigger) || (dualsenseTrigger.Contains("NoEffect") && impulseTrigger.Contains("NoEffect")) || (dualsenseTrigger.Contains("NoneEffect") && impulseTrigger.Contains("NoneEffect")))
		{
			return TriggerEffectManager.NoneEffect;
		}
		TriggerEffectManager.ControllerTriggerEffect controllerTriggerEffect = new TriggerEffectManager.ControllerTriggerEffect
		{
			DualsenseEffect = TriggerEffectManager.NoneEffectDs,
			XboxTriggerEffect = TriggerEffectManager.NoneEffectXb
		};
		TriggerEffectManager.TriggerEffectDS triggerEffectDS;
		if (TriggerEffectManager.ControllerTriggerEffectsDS.TryGetValue(dualsenseTrigger, out triggerEffectDS))
		{
			controllerTriggerEffect.DualsenseEffect = triggerEffectDS;
		}
		else
		{
			Log.Warning("Failed to find trigger effect DS: " + dualsenseTrigger);
		}
		TriggerEffectManager.TriggerEffectXB triggerEffectXB;
		if (TriggerEffectManager.ControllerTriggerEffectsXb.TryGetValue(impulseTrigger, out triggerEffectXB))
		{
			controllerTriggerEffect.XboxTriggerEffect = triggerEffectXB;
		}
		else
		{
			Log.Warning("Failed to find trigger effect XB: " + impulseTrigger);
		}
		return controllerTriggerEffect;
	}

	public static ValueTuple<TriggerEffectManager.TriggerEffectDS, TriggerEffectManager.TriggerEffectXB> GetTriggerEffectAsTuple(ValueTuple<string, string> triggerEffectNames)
	{
		if (string.IsNullOrEmpty(triggerEffectNames.Item1) || string.IsNullOrEmpty(triggerEffectNames.Item2) || (triggerEffectNames.Item1.Contains("NoEffect") && triggerEffectNames.Item2.Contains("NoEffect")) || (triggerEffectNames.Item1.Contains("NoneEffect") && triggerEffectNames.Item2.Contains("NoneEffect")))
		{
			return new ValueTuple<TriggerEffectManager.TriggerEffectDS, TriggerEffectManager.TriggerEffectXB>(TriggerEffectManager.NoneEffect.DualsenseEffect, TriggerEffectManager.NoneEffect.XboxTriggerEffect);
		}
		ValueTuple<TriggerEffectManager.TriggerEffectDS, TriggerEffectManager.TriggerEffectXB> valueTuple = new ValueTuple<TriggerEffectManager.TriggerEffectDS, TriggerEffectManager.TriggerEffectXB>(TriggerEffectManager.NoneEffect.DualsenseEffect, TriggerEffectManager.NoneEffect.XboxTriggerEffect);
		TriggerEffectManager.TriggerEffectDS triggerEffectDS;
		if (TriggerEffectManager.ControllerTriggerEffectsDS.TryGetValue(triggerEffectNames.Item1, out triggerEffectDS))
		{
			valueTuple.Item1 = triggerEffectDS;
		}
		else
		{
			Log.Warning("Failed to find trigger effect DS: " + triggerEffectNames.Item1);
		}
		TriggerEffectManager.TriggerEffectXB triggerEffectXB;
		if (TriggerEffectManager.ControllerTriggerEffectsXb.TryGetValue(triggerEffectNames.Item2, out triggerEffectXB))
		{
			valueTuple.Item2 = triggerEffectXB;
		}
		else
		{
			Log.Warning("Failed to find trigger effect XB: " + triggerEffectNames.Item2);
		}
		return valueTuple;
	}

	public static bool SettingDefaultValue()
	{
		return Application.platform == 38 || Application.platform == 36 || Application.platform == 7;
	}

	public void SetAudioRumbleSource(AudioSource _audioSource, float _strengthMultiplier, bool _locationBased)
	{
		AudioGamepadRumbleSource audioGamepadRumbleSource = null;
		float num = float.MaxValue;
		foreach (AudioGamepadRumbleSource audioGamepadRumbleSource2 in this.vibrationAudioSources)
		{
			if (!(audioGamepadRumbleSource2.audioSrc != null))
			{
				audioGamepadRumbleSource2.SetAudioSource(_audioSource, _strengthMultiplier, _locationBased);
				return;
			}
			if (audioGamepadRumbleSource2.timeAdded < num)
			{
				audioGamepadRumbleSource = audioGamepadRumbleSource2;
				num = audioGamepadRumbleSource2.timeAdded;
			}
		}
		if (audioGamepadRumbleSource != null)
		{
			audioGamepadRumbleSource.SetAudioSource(_audioSource, _strengthMultiplier, _locationBased);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDualSenseVibration(byte _smallMotor, byte _largeMotor)
	{
		TriggerEffectDualsense.SetDualSenseVibration(_smallMotor, _largeMotor);
		TriggerEffectDualsensePC.SetDualSenseVibration(_smallMotor, _largeMotor);
	}

	public static void UpdateControllerVibrationStrength()
	{
		switch (GamePrefs.GetInt(EnumGamePrefs.OptionsControllerVibrationStrength))
		{
		case 0:
			TriggerEffectManager.audioRumbleStrength = 0f;
			return;
		case 1:
			TriggerEffectManager.audioRumbleStrength = 0.5f;
			return;
		case 2:
			TriggerEffectManager.audioRumbleStrength = 1f;
			return;
		case 3:
			TriggerEffectManager.audioRumbleStrength = 2f;
			return;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void OnLastInputStyleChanged(PlayerInputManager.InputStyle _style)
	{
		if (_style == PlayerInputManager.InputStyle.Keyboard)
		{
			this.StopGamepadVibration(false);
		}
	}

	public static void UpdateDualSenseLightFromWeather(WeatherManager.BiomeWeather weather)
	{
		if (TriggerEffectManager.lightbarGradients == null || weather == null)
		{
			return;
		}
		float num = GameManager.Instance.World.GetWorldTime() % 24000UL;
		float num2 = SkyManager.GetDawnTime() * 1000f;
		float num3 = SkyManager.GetDuskTime() * 1000f;
		bool flag = num < num2 || num > num3;
		if (!flag && SkyManager.isAllTimeNight)
		{
			flag = true;
		}
		if (flag && SkyManager.isAllTimeDay)
		{
			flag = false;
		}
		float num6;
		if (flag)
		{
			float num4 = 24000f - num3;
			float num5 = num4 + num2;
			if (SkyManager.isAllTimeNight)
			{
				num6 = 0.5f;
			}
			else if (num >= num3 && num < 24000f)
			{
				num6 = (num - num3) / num5;
			}
			else
			{
				num6 = num4 / num5 + num / (num2 + num4);
			}
		}
		else if (SkyManager.isAllTimeDay)
		{
			num6 = 0.5f;
		}
		else
		{
			float num7 = num3 - num2;
			num6 = (num - num2) / num7;
		}
		Color color;
		if (SkyManager.BloodMoonVisiblePercent() == 1f)
		{
			float num8 = (1f + Mathf.Sin(Time.time)) / 2f;
			color = TriggerEffectManager.lightbarGradients.bloodmoonGradient.Evaluate(num8);
		}
		else if (weather.rainParam.value >= 0.5f || (weather.biomeDefinition != null && (weather.biomeDefinition.m_BiomeType == BiomeDefinition.BiomeType.Wasteland || weather.biomeDefinition.m_BiomeType == BiomeDefinition.BiomeType.burnt_forest)))
		{
			if (flag)
			{
				color = TriggerEffectManager.lightbarGradients.cloudNightGradient.Evaluate(num6);
			}
			else
			{
				color = TriggerEffectManager.lightbarGradients.cloudDayGradient.Evaluate(num6);
			}
		}
		else if (flag)
		{
			color = TriggerEffectManager.lightbarGradients.nightGradient.Evaluate(num6);
		}
		else
		{
			color = TriggerEffectManager.lightbarGradients.dayGradient.Evaluate(num6);
		}
		TriggerEffectManager.SetDualSenseLightbarColor(color);
	}

	public static void SetDualSenseLightbarColor(Color color)
	{
		for (int i = 1; i < 5; i++)
		{
			TriggerEffectDualsense.SetLightbar(i, (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f));
			TriggerEffectDualsensePC.SetLightbar(i, (byte)(color.r * 255f), (byte)(color.g * 255f), (byte)(color.b * 255f));
		}
	}

	public static void SetMainMenuLightbarColor()
	{
		TriggerEffectManager.SetDualSenseLightbarColor(TriggerEffectManager.lightbarGradients.mainMenuColor);
	}

	public static readonly Dictionary<string, TriggerEffectManager.TriggerEffectDS> ControllerTriggerEffectsDS = new Dictionary<string, TriggerEffectManager.TriggerEffectDS>();

	public static readonly Dictionary<string, TriggerEffectManager.TriggerEffectXB> ControllerTriggerEffectsXb = new Dictionary<string, TriggerEffectManager.TriggerEffectXB>();

	[PublicizedFrom(EAccessModifier.Internal)]
	public static readonly TriggerEffectManager.TriggerEffectDS NoneEffectDs = new TriggerEffectManager.TriggerEffectDS
	{
		Effect = TriggerEffectManager.EffectDualsense.Off,
		AmplitudeEndStrength = 0,
		Frequency = 0,
		Position = 0,
		EndPosition = 0,
		Strength = 0,
		Strengths = Array.Empty<byte>()
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly TriggerEffectManager.TriggerEffectXB NoneEffectXb = new TriggerEffectManager.TriggerEffectXB
	{
		Effect = TriggerEffectManager.EffectXbox.Off,
		Strength = 0f
	};

	public static readonly TriggerEffectManager.ControllerTriggerEffect NoneEffect = new TriggerEffectManager.ControllerTriggerEffect
	{
		DualsenseEffect = TriggerEffectManager.NoneEffectDs,
		XboxTriggerEffect = TriggerEffectManager.NoneEffectXb
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public bool[] _controllersConnected = new bool[4];

	[PublicizedFrom(EAccessModifier.Private)]
	public float _currentControllerVibrationStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _enabled;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _triggerSetLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _triggerSetRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _stateChanged;

	[PublicizedFrom(EAccessModifier.Private)]
	public TriggerEffectManager.ControllerTriggerEffect _currentEffectLeft;

	[PublicizedFrom(EAccessModifier.Private)]
	public TriggerEffectManager.ControllerTriggerEffect _currentEffectRight;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAudioRumbleStrengthSubtle = 0.5f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAudioRumbleStrengthStandard = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cAudioRumbleStrengthStrong = 2f;

	[PublicizedFrom(EAccessModifier.Private)]
	public const float cDualSenseRumbleStrengthMultiplier = 0.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	public static float audioRumbleStrength = 1f;

	public AudioGamepadRumbleSource[] vibrationAudioSources = new AudioGamepadRumbleSource[5];

	[PublicizedFrom(EAccessModifier.Protected)]
	public static LightbarGradients lightbarGradients;

	[PublicizedFrom(EAccessModifier.Private)]
	public float leftAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public float rightAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetLeftAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public float targetRightAudioStrength;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool _inUI;

	public enum GamepadTrigger
	{
		LeftTrigger,
		RightTrigger
	}

	public enum EffectDualsense
	{
		Off,
		WeaponSingle,
		WeaponMultipoint,
		FeedbackSingle,
		VibrationSingle,
		FeedbackSlope,
		VibrationSlope,
		FeedbackMultipoint,
		VibrationMultipoint
	}

	public struct TriggerEffectDS
	{
		public TriggerEffectManager.EffectDualsense Effect;

		public byte Position;

		public byte EndPosition;

		public byte Frequency;

		public byte AmplitudeEndStrength;

		public byte Strength;

		public byte[] Strengths;
	}

	public enum EffectXbox
	{
		Off,
		FeedbackSingle,
		VibrationSingle,
		FeedbackSlope,
		VibrationSlope
	}

	public struct TriggerEffectXB
	{
		public TriggerEffectManager.EffectXbox Effect;

		public float Strength;

		[FormerlySerializedAs("endStrength")]
		[FormerlySerializedAs("Amplitude")]
		public float EndStrength;

		public float StartPosition;

		public float EndPosition;
	}

	public struct ControllerTriggerEffect
	{
		public TriggerEffectManager.TriggerEffectDS DualsenseEffect;

		public TriggerEffectManager.TriggerEffectXB XboxTriggerEffect;
	}
}
