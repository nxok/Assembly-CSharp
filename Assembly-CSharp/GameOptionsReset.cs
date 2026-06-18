using System;
using System.Collections.Generic;

public static class GameOptionsReset
{
	public static void Init()
	{
		GameOptionsReset.Audio = new GameOptionsReset.AudioGroup();
		GameOptionsReset.Graphics = new GameOptionsReset.GraphicsGroup();
		GameOptionsReset.Controls = new GameOptionsReset.ControlsGroup();
		GameOptionsReset.Controller = new GameOptionsReset.ControllerGroup();
		GameOptionsReset.Bindings = new GameOptionsReset.BindingsGroup();
		GameOptionsReset.groups = new List<GameOptionsReset.IGroup>
		{
			GameOptionsReset.Audio,
			GameOptionsReset.Graphics,
			GameOptionsReset.Controls,
			GameOptionsReset.Controller,
			GameOptionsReset.Bindings
		};
	}

	public static string GetGroupId(EnumGamePrefs enumGamePref)
	{
		return GameOptionsReset.GetGroupId(enumGamePref.ToStringCached<EnumGamePrefs>());
	}

	public static string GetGroupId(string prefName)
	{
		foreach (GameOptionsReset.IGroup group in GameOptionsReset.groups)
		{
			if (group.HasPref(prefName))
			{
				return group.VersionId;
			}
		}
		return GameOptionsReset.defaultGroupId;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static PrefType EnumTypeToPrefType(GamePrefs.EnumType enumType)
	{
		switch (enumType)
		{
		case GamePrefs.EnumType.Int:
			return PrefType.Int;
		case GamePrefs.EnumType.Float:
			return PrefType.Float;
		case GamePrefs.EnumType.String:
			return PrefType.String;
		case GamePrefs.EnumType.Bool:
			return PrefType.Int;
		case GamePrefs.EnumType.Binary:
			return PrefType.String;
		default:
			throw new Exception(string.Format("Unmapped {0} for {1}: {2}", "PrefType", "EnumType", enumType));
		}
	}

	public static bool TryGetPrefType(EnumGamePrefs enumGamePref, out PrefType prefType)
	{
		GamePrefs.EnumType? prefType2 = GamePrefs.GetPrefType(enumGamePref);
		if (prefType2 == null)
		{
			Log.Error(string.Format("Unknown enum type for {0}", enumGamePref));
			prefType = PrefType.Float;
			return false;
		}
		prefType = GameOptionsReset.EnumTypeToPrefType(prefType2.Value);
		return true;
	}

	public static bool NeedsResetGame()
	{
		return 13 != GamePrefs.GetInt(EnumGamePrefs.LastGameResetRevision);
	}

	public static void ResetGame(PrefVersionStore versionedPrefs)
	{
		GamePrefs.PropertyDecl[] propertyList = GamePrefs.GetPropertyList();
		for (int i = 0; i < propertyList.Length; i++)
		{
			GameOptionsReset.ResetPref(propertyList[i].name, versionedPrefs);
		}
		GameOptionsReset.Graphics.Reset(versionedPrefs);
		GameOptionsReset.Controls.Reset(versionedPrefs);
		GameOptionsReset.Bindings.Reset(versionedPrefs);
		GamePrefs.Set(EnumGamePrefs.LastGameResetRevision, 13);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ResetPref(EnumGamePrefs enumGamePref, PrefVersionStore versionedPrefs)
	{
		object @default;
		if (versionedPrefs == null || !versionedPrefs.TryGetGamePref(enumGamePref, out @default))
		{
			@default = GamePrefs.GetDefault(enumGamePref);
		}
		GamePrefs.SetObject(enumGamePref, @default);
	}

	public static GameOptionsReset.AudioGroup Audio;

	public static GameOptionsReset.GraphicsGroup Graphics;

	public static GameOptionsReset.ControlsGroup Controls;

	public static GameOptionsReset.ControllerGroup Controller;

	public static GameOptionsReset.BindingsGroup Bindings;

	public static List<GameOptionsReset.IGroup> groups;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string defaultGroupId = string.Format("DefaultGroup_{0}", 13);

	public interface IGroup
	{
		string VersionId { get; }

		bool HasPref(string prefName);

		bool NeedsReset();

		void Reset(PrefVersionStore versionedPrefs);
	}

	public abstract class EnumGamePrefGroup : GameOptionsReset.IGroup
	{
		public abstract string VersionId { get; }

		[PublicizedFrom(EAccessModifier.Protected)]
		public EnumGamePrefGroup(params EnumGamePrefs[] enumGamePrefs)
		{
			foreach (EnumGamePrefs enumGamePrefs2 in enumGamePrefs)
			{
				this.AddEnumGamePref(enumGamePrefs2);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void AddEnumGamePref(EnumGamePrefs enumGamePref)
		{
			if (!this.EnumPrefs.Add(enumGamePref))
			{
				return;
			}
			this.PrefNames.Add(enumGamePref.ToStringCached<EnumGamePrefs>());
		}

		public virtual bool HasPref(string prefName)
		{
			return this.PrefNames.Contains(prefName);
		}

		public virtual bool NeedsReset()
		{
			return false;
		}

		public virtual void Reset(PrefVersionStore versionedPrefs)
		{
			foreach (EnumGamePrefs enumGamePrefs in this.EnumPrefs)
			{
				GameOptionsReset.ResetPref(enumGamePrefs, versionedPrefs);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<EnumGamePrefs> EnumPrefs = new HashSet<EnumGamePrefs>();

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly HashSet<string> PrefNames = new HashSet<string>();
	}

	public class AudioGroup : GameOptionsReset.EnumGamePrefGroup
	{
		public override string VersionId
		{
			get
			{
				return string.Format("Audio_{0}_0", 13);
			}
		}

		public AudioGroup()
			: base(new EnumGamePrefs[]
			{
				EnumGamePrefs.OptionsAmbientVolumeLevel,
				EnumGamePrefs.OptionsMusicVolumeLevel,
				EnumGamePrefs.OptionsMenuMusicVolumeLevel,
				EnumGamePrefs.OptionsDynamicMusicEnabled,
				EnumGamePrefs.OptionsDynamicMusicDailyTime,
				EnumGamePrefs.OptionsMicVolumeLevel,
				EnumGamePrefs.OptionsVoiceVolumeLevel,
				EnumGamePrefs.OptionsOverallAudioVolumeLevel,
				EnumGamePrefs.OptionsVoiceChatEnabled,
				EnumGamePrefs.OptionsAudioOcclusion,
				EnumGamePrefs.OptionsSubtitlesEnabled
			})
		{
		}

		public override void Reset(PrefVersionStore versionedPrefs)
		{
			Log.Out("Resetting game options AudioGroup");
			base.Reset(versionedPrefs);
		}
	}

	public class GraphicsGroup : GameOptionsReset.EnumGamePrefGroup
	{
		public override string VersionId
		{
			get
			{
				return string.Format("Graphics_{0}_{1}", 13, 4);
			}
		}

		public GraphicsGroup()
			: base(new EnumGamePrefs[]
			{
				EnumGamePrefs.OptionsGfxResolution,
				EnumGamePrefs.OptionsGfxVsync,
				EnumGamePrefs.OptionsGfxDynamicMode,
				EnumGamePrefs.OptionsGfxUpscalerMode,
				EnumGamePrefs.OptionsGfxFSRPreset,
				EnumGamePrefs.OptionsGfxDynamicScale,
				EnumGamePrefs.OptionsGfxBrightness,
				EnumGamePrefs.OptionsGfxSignQuality,
				EnumGamePrefs.DynamicMeshEnabled,
				EnumGamePrefs.DynamicMeshDistance,
				EnumGamePrefs.NoGraphicsMode
			})
		{
			base.AddEnumGamePref(EnumGamePrefs.OptionsGfxQualityPreset);
			foreach (EnumGamePrefs enumGamePrefs in GameOptionsManager.QualityPresets.Keys)
			{
				base.AddEnumGamePref(enumGamePrefs);
			}
		}

		public override bool NeedsReset()
		{
			return 4 != GamePrefs.GetInt(EnumGamePrefs.OptionsGfxResetRevision);
		}

		public override void Reset(PrefVersionStore versionedPrefs)
		{
			Log.Out("Resetting game options GraphicsGroup");
			base.Reset(versionedPrefs);
			GameOptionsPlatforms.GfxPreset gfxPreset = GameOptionsPlatforms.CalcDefaultGfxPreset();
			GamePrefs.Set(EnumGamePrefs.OptionsGfxQualityPreset, (int)gfxPreset);
			GameOptionsManager.SetGraphicsQuality();
			GamePrefs.Set(EnumGamePrefs.OptionsGfxResetRevision, 4);
		}
	}

	public class ControlsGroup : GameOptionsReset.EnumGamePrefGroup
	{
		public override string VersionId
		{
			get
			{
				return string.Format("Controls_{0}_{1}", 13, 7);
			}
		}

		public ControlsGroup()
			: base(new EnumGamePrefs[]
			{
				EnumGamePrefs.OptionsLookSensitivity,
				EnumGamePrefs.OptionsZoomSensitivity,
				EnumGamePrefs.OptionsZoomAccel,
				EnumGamePrefs.OptionsInvertMouse,
				EnumGamePrefs.OptionsVehicleLookSensitivity,
				EnumGamePrefs.OptionsControlsSprintLock
			})
		{
		}

		public override bool NeedsReset()
		{
			return 7 != GamePrefs.GetInt(EnumGamePrefs.OptionsControlsResetRevision);
		}

		public override void Reset(PrefVersionStore versionedPrefs)
		{
			Log.Out("Resetting game options ControlsGroup");
			base.Reset(versionedPrefs);
			GamePrefs.Set(EnumGamePrefs.OptionsControlsResetRevision, 7);
		}
	}

	public class ControllerGroup : GameOptionsReset.EnumGamePrefGroup
	{
		public override string VersionId
		{
			get
			{
				return string.Format("Controller_{0}_0", 13);
			}
		}

		public ControllerGroup()
			: base(new EnumGamePrefs[]
			{
				EnumGamePrefs.OptionsAllowController,
				EnumGamePrefs.OptionsControllerVibration,
				EnumGamePrefs.OptionsInterfaceSensitivity,
				EnumGamePrefs.OptionsControllerSensitivityX,
				EnumGamePrefs.OptionsControllerSensitivityY,
				EnumGamePrefs.OptionsControllerLookInvert,
				EnumGamePrefs.OptionsControllerJoystickLayout,
				EnumGamePrefs.OptionsControllerLookAcceleration,
				EnumGamePrefs.OptionsControllerZoomSensitivity,
				EnumGamePrefs.OptionsControllerLookAxisDeadzone,
				EnumGamePrefs.OptionsControllerMoveAxisDeadzone,
				EnumGamePrefs.OptionsControllerCursorSnap,
				EnumGamePrefs.OptionsControllerCursorHoverSensitivity,
				EnumGamePrefs.OptionsControllerVehicleSensitivity,
				EnumGamePrefs.OptionsControllerAimAssists,
				EnumGamePrefs.OptionsControllerWeaponAiming,
				EnumGamePrefs.OptionsControlsSprintLock,
				EnumGamePrefs.OptionsControllerTriggerEffects,
				EnumGamePrefs.OptionsControllerIconStyle
			})
		{
		}

		public override void Reset(PrefVersionStore versionedPrefs)
		{
			Log.Out("Resetting game options ControllerGroup");
			base.Reset(versionedPrefs);
		}
	}

	public class BindingsGroup : GameOptionsReset.IGroup
	{
		public string VersionId
		{
			get
			{
				return string.Format("Bindings_{0}_{1}", 13, 1);
			}
		}

		public bool HasPref(string prefName)
		{
			if (prefName.Equals("Controls"))
			{
				return true;
			}
			if (prefName.Equals("ActionSetsSaved"))
			{
				return true;
			}
			if (!prefName.StartsWith("ActionSet_"))
			{
				return false;
			}
			foreach (ValueTuple<PlayerActionsBase, string> valueTuple in GameOptionsControls.ActionSetPrefs)
			{
				string item = valueTuple.Item2;
				if (prefName.Equals(item))
				{
					return true;
				}
			}
			return false;
		}

		public bool NeedsReset()
		{
			return 1 != GamePrefs.GetInt(EnumGamePrefs.OptionsBindingsResetRevision);
		}

		public void Reset(PrefVersionStore versionedPrefs)
		{
			Log.Out("Resetting game options BindingsGroup");
			foreach (ValueTuple<PlayerActionsBase, string> valueTuple in GameOptionsControls.ActionSetPrefs)
			{
				PlayerActionsBase item = valueTuple.Item1;
				string item2 = valueTuple.Item2;
				string text;
				if (versionedPrefs != null && versionedPrefs.TryGetString(item2, out text) && !string.IsNullOrEmpty(text))
				{
					item.Load(text);
				}
				else
				{
					item.Reset();
				}
			}
			GameOptionsControls.Save();
			GamePrefs.Set(EnumGamePrefs.OptionsBindingsResetRevision, 1);
		}
	}
}
