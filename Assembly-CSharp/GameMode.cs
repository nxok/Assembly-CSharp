using System;
using System.Collections.Generic;
using System.Reflection;
using Platform;

public abstract class GameMode
{
	public abstract GameMode.ModeGamePref[] GetSupportedGamePrefsInfo();

	public Dictionary<EnumGamePrefs, GameMode.ModeGamePref> GetGamePrefs()
	{
		if (this.gamePrefs == null)
		{
			this.gamePrefs = new EnumDictionary<EnumGamePrefs, GameMode.ModeGamePref>();
			GameMode.ModeGamePref[] supportedGamePrefsInfo = this.GetSupportedGamePrefsInfo();
			int length = supportedGamePrefsInfo.GetLength(0);
			for (int i = 0; i < length; i++)
			{
				EnumGamePrefs gamePref = supportedGamePrefsInfo[i].GamePref;
				GamePrefs.EnumType valueType = supportedGamePrefsInfo[i].ValueType;
				object defaultValue = supportedGamePrefsInfo[i].DefaultValue;
				this.gamePrefs.Add(gamePref, new GameMode.ModeGamePref(gamePref, valueType, defaultValue, null));
			}
		}
		return this.gamePrefs;
	}

	public abstract void ResetGamePrefs();

	public abstract string GetDescription();

	public abstract int GetID();

	public abstract void Init();

	public abstract string GetName();

	public string GetTypeName()
	{
		if (this.cachedTypeName == null)
		{
			this.cachedTypeName = base.GetType().Name;
		}
		return this.cachedTypeName;
	}

	public abstract int GetRoundCount();

	public abstract void StartRound(int _idx);

	public abstract void EndRound(int _idx);

	public virtual string GetAdditionalGameInfo(World _world)
	{
		return string.Empty;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void InitGameModeDict()
	{
		if (GameMode.gameModes == null)
		{
			GameMode.gameModes = new Dictionary<int, GameMode>();
			Type typeFromHandle = typeof(GameMode);
			foreach (Type type in Assembly.GetCallingAssembly().GetTypes())
			{
				if (!type.IsAbstract && typeFromHandle.IsAssignableFrom(type))
				{
					GameMode gameMode = Activator.CreateInstance(type) as GameMode;
					GameMode.gameModes.Add(gameMode.GetID(), gameMode);
				}
			}
		}
	}

	public static GameMode GetGameModeForId(int _id)
	{
		GameMode.InitGameModeDict();
		if (GameMode.gameModes.ContainsKey(_id))
		{
			return GameMode.gameModes[_id];
		}
		return null;
	}

	public static GameMode GetGameModeForName(string _name)
	{
		GameMode.InitGameModeDict();
		foreach (KeyValuePair<int, GameMode> keyValuePair in GameMode.gameModes)
		{
			if (keyValuePair.Value.GetTypeName().EqualsCaseInsensitive(_name))
			{
				return keyValuePair.Value;
			}
		}
		return null;
	}

	public override string ToString()
	{
		if (this.localizedName == null)
		{
			this.localizedName = Localization.Get(this.GetName(), false, null);
		}
		return this.localizedName;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameMode()
	{
	}

	public static GameMode[] AvailGameModes = new GameMode[]
	{
		new GameModeSurvival()
	};

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<EnumGamePrefs, GameMode.ModeGamePref> gamePrefs;

	[PublicizedFrom(EAccessModifier.Private)]
	public string cachedTypeName;

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<int, GameMode> gameModes;

	public struct ModeGamePref
	{
		public ModeGamePref(EnumGamePrefs _gamePref, GamePrefs.EnumType _valueType, object _defaultValue, Dictionary<DeviceFlag, object> _deviceDefaults = null)
		{
			this.GamePref = _gamePref;
			this.ValueType = _valueType;
			if (_deviceDefaults != null && _deviceDefaults.ContainsKey(DeviceFlag.StandaloneWindows))
			{
				this.DefaultValue = _deviceDefaults[DeviceFlag.StandaloneWindows];
				return;
			}
			this.DefaultValue = _defaultValue;
		}

		public EnumGamePrefs GamePref;

		public GamePrefs.EnumType ValueType;

		public object DefaultValue;
	}
}
