using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Platform;
using UnityEngine;

public class GameStartupHelper
{
	public static GameStartupHelper Instance
	{
		get
		{
			GameStartupHelper gameStartupHelper;
			if ((gameStartupHelper = GameStartupHelper.instance) == null)
			{
				gameStartupHelper = (GameStartupHelper.instance = new GameStartupHelper());
			}
			return gameStartupHelper;
		}
	}

	public static string[] GetCommandLineArgs()
	{
		return Environment.GetCommandLineArgs();
	}

	public bool InitCommandLine()
	{
		if (this.initCommandLineOk != null)
		{
			return this.initCommandLineOk.Value;
		}
		Log.Out(string.Concat(new string[]
		{
			"Version: ",
			Constants.cVersionInformation.LongString,
			" Compatibility Version: ",
			Constants.cVersionInformation.LongStringNoBuild,
			", Build: ",
			Application.platform.ToStringCached<RuntimePlatform>(),
			" ",
			Constants.Is32BitOs ? "32" : "64",
			" Bit"
		}));
		this.PrintSystemInfo();
		Log.Out(string.Format("Local UTC offset: {0} hours", TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).Hours));
		Utils.InitStatic();
		string[] commandLineArgs = GameStartupHelper.GetCommandLineArgs();
		LaunchPrefs.InitStart();
		this.parsedGamePrefs = new EnumDictionary<EnumGamePrefs, object>();
		this.initCommandLineOk = new bool?(this.ParseCommandLine(commandLineArgs));
		LaunchPrefs.InitEnd();
		return this.initCommandLineOk.Value;
	}

	public bool InitGamePrefs()
	{
		if (this.initCommandLineOk == null || !this.initCommandLineOk.Value)
		{
			return false;
		}
		if (this.initGamePrefsOk != null)
		{
			return this.initGamePrefsOk.Value;
		}
		Log.Out("Last played version: " + GamePrefs.GetString(EnumGamePrefs.GameVersion));
		GamePrefs.Set(EnumGamePrefs.GameVersion, Constants.cVersionInformation.LongStringNoBuild);
		this.initGamePrefsOk = new bool?(this.ApplyParsedGamePrefs());
		return this.initGamePrefsOk.Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void PrintSystemInfo()
	{
		Log.Out("System information:");
		Log.Out("   OS: " + SystemInfo.operatingSystem);
		Log.Out(string.Format("   CPU: {0} (cores: {1})", SystemInfo.processorType, SystemInfo.processorCount));
		Log.Out(string.Format("   RAM: {0} MB", SystemInfo.systemMemorySize));
		Log.Out(string.Format("   GPU: {0} ({1} MB)", SystemInfo.graphicsDeviceName, SystemInfo.graphicsMemorySize));
		Log.Out(string.Format("   Graphics API: {0} (shader level {1:0.0})", SystemInfo.graphicsDeviceVersion, (float)SystemInfo.graphicsShaderLevel / 10f));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ParseCommandLine(string[] _args)
	{
		this.printCommandline(_args);
		Dictionary<string, string> dictionary = this.parseRawCommandline(_args);
		if (dictionary == null)
		{
			return false;
		}
		string text;
		if (dictionary.TryGetValue("configfile", out text))
		{
			if (!text.Contains("."))
			{
				text += ".xml";
			}
			if (!this.LoadConfigFile(text))
			{
				return false;
			}
			dictionary.Remove("configfile");
		}
		foreach (KeyValuePair<string, string> keyValuePair in dictionary)
		{
			this.ParsePref(keyValuePair.Key, keyValuePair.Value, false, true);
		}
		if (GameManager.IsDedicatedServer && !this.bConfigFileLoaded)
		{
			Log.Error("====================================================================================================");
			Log.Error("No server config file loaded (\"-configfile=somefile.xml\" not given or could not be loaded)");
			Log.Error("====================================================================================================");
			Application.Quit();
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ApplyParsedGamePrefs()
	{
		if (this.parsedGamePrefs == null)
		{
			Log.Error("Expected parsed game prefs.");
			Application.Quit();
			return false;
		}
		foreach (KeyValuePair<EnumGamePrefs, object> keyValuePair in this.parsedGamePrefs)
		{
			EnumGamePrefs enumGamePrefs;
			object obj;
			keyValuePair.Deconstruct(out enumGamePrefs, out obj);
			EnumGamePrefs enumGamePrefs2 = enumGamePrefs;
			object obj2 = obj;
			GamePrefs.SetObject(enumGamePrefs2, obj2);
		}
		this.parsedGamePrefs = null;
		if (GameManager.IsDedicatedServer)
		{
			if (!GameUtils.ValidateGameName(GamePrefs.GetString(EnumGamePrefs.GameName)))
			{
				Log.Error("====================================================================================================");
				Log.Error("Error parsing configfile: GameName is empty or contains invalid characters");
				Log.Out("Allowed characters: A-Z, a-z, 0-9, dot (.), underscore (_), dash (-) and space ( )");
				Log.Error("====================================================================================================");
				Application.Quit();
				return false;
			}
			this.SetDedicatedServerSettings();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void printCommandline(string[] _args)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("Command line arguments:");
		for (int i = 0; i < _args.Length; i++)
		{
			stringBuilder.Append(' ');
			string text = _args[i];
			Match match = GameStartupHelper.filterPasswordsRegex.Match(text);
			if (match.Success)
			{
				Group group = match.Groups[1];
				text = ((group != null) ? group.ToString() : null) + "***";
			}
			if (i > 0 && _args[i - 1].EqualsCaseInsensitive("+password"))
			{
				text = "***";
			}
			stringBuilder.Append(IPlatformApplication.EscapeArg(text));
		}
		Log.Out(stringBuilder.ToString());
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, string> parseRawCommandline(string[] _args)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 0; i < _args.Length; i++)
		{
			try
			{
				if (!(_args[i] == "[REMOVE_ON_RESTART]"))
				{
					bool flag = i + 1 < _args.Length && _args[i + 1] == "[REMOVE_ON_RESTART]";
					if (_args[i].StartsWith("-port="))
					{
						dictionary["ServerPort"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-ip="))
					{
						dictionary["ServerIP"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-maxplayers="))
					{
						dictionary["ServerMaxPlayerCount"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-gamemode="))
					{
						dictionary["GameMode"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-difficulty="))
					{
						dictionary["GameDifficulty"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-name="))
					{
						dictionary["GameName"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-world="))
					{
						dictionary["GameWorld"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-configfile="))
					{
						dictionary["configfile"] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					else if (_args[i].StartsWith("-autopilot"))
					{
						string text = _args[i];
						dictionary["AutopilotMode"] = ((text.Length > "-autopilot".Length) ? int.Parse(text.Substring("-autopilot".Length)) : 1).ToString();
					}
					else if (_args[i].StartsWith("-nographics"))
					{
						dictionary["NoGraphicsMode"] = "true";
					}
					else if (_args[i].StartsWith("-") && _args[i].Contains("="))
					{
						dictionary[_args[i].Substring(1, _args[i].IndexOf('=') - 1)] = _args[i].Substring(_args[i].IndexOf('=') + 1);
					}
					if (flag)
					{
						i++;
					}
				}
			}
			catch (ArgumentException)
			{
				Log.Error("====================================================================================================");
				Log.Error("Command line argument '" + _args[i] + "' given multiple times!");
				Log.Error("====================================================================================================");
				Application.Quit();
				return null;
			}
		}
		return dictionary;
	}

	public static string[] RemoveTemporaryArguments(string[] args)
	{
		List<string> list = new List<string>();
		int num = 0;
		while (num < args.Length && !(args[num] == "[REMOVE_ON_RESTART]"))
		{
			list.Add(args[num]);
			num++;
		}
		return list.ToArray();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool LoadConfigFile(string _filename)
	{
		if (!_filename.Contains("/") && !_filename.Contains("\\"))
		{
			_filename = GameIO.GetApplicationPath() + "/" + _filename;
		}
		if (!SdFile.Exists(_filename))
		{
			Log.Error("====================================================================================================");
			Log.Error("Specified configfile not found: " + _filename);
			Log.Error("====================================================================================================");
			Application.Quit();
			return false;
		}
		Log.Out("Parsing server configfile: " + _filename);
		XDocument xdocument;
		try
		{
			xdocument = SdXDocument.Load(_filename);
		}
		catch (Exception ex)
		{
			Log.Error("====================================================================================================");
			Log.Error("Error parsing configfile: ");
			Log.Exception(ex);
			Log.Error("====================================================================================================");
			Application.Quit();
			return false;
		}
		IEnumerable<XElement> enumerable = from s in xdocument.Elements("ServerSettings")
			from p in s.Elements("property")
			select p;
		DynamicProperties dynamicProperties = new DynamicProperties();
		foreach (XElement xelement in enumerable)
		{
			dynamicProperties.Add(xelement, false);
		}
		foreach (KeyValuePair<string, string> keyValuePair in dynamicProperties.Values)
		{
			if (dynamicProperties.Values[keyValuePair.Key] == null)
			{
				this.ShowConfigError(keyValuePair.Key, "Value not set", null, true);
				return false;
			}
			if (!this.ParsePref(keyValuePair.Key, dynamicProperties.Values[keyValuePair.Key], true, false))
			{
				return false;
			}
		}
		Log.Out("Parsing server configfile successfully completed");
		this.bConfigFileLoaded = true;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ParsePref(string _name, string _value, bool _quitOnError = true, bool _ignoreCase = false)
	{
		bool flag;
		try
		{
			string text = ((_name != null) ? _name.Trim() : null);
			ILaunchPref launchPref;
			EnumGamePrefs enumGamePrefs;
			if (string.IsNullOrEmpty(text))
			{
				this.ShowConfigError(text, "Empty config option name", null, _quitOnError);
				flag = false;
			}
			else if (LaunchPrefs.All.TryGetValue(text, out launchPref))
			{
				flag = this.ParseLaunchPref(text, launchPref, _value, _quitOnError);
			}
			else if (EnumUtils.TryParse<EnumGamePrefs>(text, out enumGamePrefs, _ignoreCase))
			{
				flag = this.ParseGamePref(text, enumGamePrefs, _value, _quitOnError);
			}
			else
			{
				if (_quitOnError)
				{
					this.ShowConfigError(text, "Unknown config option", null, true);
				}
				else
				{
					Log.Warning("Command line argument '" + _name + "' is not a configfile property, ignoring.");
				}
				flag = false;
			}
		}
		catch (Exception ex)
		{
			this.ShowConfigError(_name, null, ex, _quitOnError);
			flag = false;
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ParseLaunchPref(string trimmedName, ILaunchPref launchPref, string _value, bool _quitOnError = true)
	{
		if (!launchPref.TrySet(_value))
		{
			this.ShowConfigError(trimmedName, "Could not parse config value '" + _value + "'", null, _quitOnError);
			return false;
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ParseGamePref(string trimmedName, EnumGamePrefs gpEnum, string _value, bool _quitOnError = true)
	{
		object obj = GamePrefs.Parse(gpEnum, _value);
		if (obj == null)
		{
			this.ShowConfigError(trimmedName, "Could not parse config value '" + _value + "'", null, _quitOnError);
			return false;
		}
		this.parsedGamePrefs[gpEnum] = obj;
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void ShowConfigError(string _name, string _message, Exception _exc = null, bool _quitOnError = true)
	{
		if (_quitOnError)
		{
			Log.Error("====================================================================================================");
		}
		if (_message != null)
		{
			Log.Error("Error parsing configfile property '" + _name + "': " + _message);
		}
		else
		{
			Log.Error("Error parsing configfile property '" + _name + "'");
		}
		if (_exc != null)
		{
			Log.Exception(_exc);
		}
		if (_quitOnError)
		{
			Log.Out("Make sure your configfile is updated the current server version!");
			Log.Out("Startup aborted due to the given error in server configfile");
			Log.Error("====================================================================================================");
			Application.Quit();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetDedicatedServerSettings()
	{
		Log.Out("Starting dedicated server level=" + GamePrefs.GetString(EnumGamePrefs.GameWorld) + " game name=" + GamePrefs.GetString(EnumGamePrefs.GameName));
		Log.Out(string.Format("Maximum allowed players: {0}", GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount)));
		Log.Out("Game mode: " + GamePrefs.GetString(EnumGamePrefs.GameMode));
		Log.Out(string.Format("Crossplay: {0}", GamePrefs.GetBool(EnumGamePrefs.ServerAllowCrossplay)));
		foreach (EnumGamePrefs enumGamePrefs in EnumUtils.Values<EnumGamePrefs>())
		{
			if (GamePrefs.Exists(enumGamePrefs))
			{
				GamePrefs.SetPersistent(enumGamePrefs, false);
			}
		}
		this.OpenMainMenuAfterAwake = false;
	}

	public const string RemoveOnRestartArgFlag = "[REMOVE_ON_RESTART]";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex filterPasswordsRegex = new Regex("^(-[^=]*(Password|Secret)[^=]*=).*$", RegexOptions.IgnoreCase);

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameStartupHelper instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool? initCommandLineOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool? initGamePrefsOk;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool bConfigFileLoaded;

	public bool OpenMainMenuAfterAwake = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public EnumDictionary<EnumGamePrefs, object> parsedGamePrefs;
}
