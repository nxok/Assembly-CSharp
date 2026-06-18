using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform;
using Platform.XBL.Save.Storage;

public static class LaunchPrefs
{
	public static IReadOnlyDictionary<string, ILaunchPref> All
	{
		get
		{
			return LaunchPrefs.s_launchPrefs;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ILaunchPref<T> Create<T>(T defaultValue, LaunchPrefs.LaunchPrefParser<T> parser, [CallerMemberName] string name = null)
	{
		if (parser == null)
		{
			throw new ArgumentNullException("parser");
		}
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		return new LaunchPrefs.LaunchPref<T>(name, defaultValue, parser);
	}

	public static void InitStart()
	{
		object obj = LaunchPrefs.s_initializationLock;
		lock (obj)
		{
			if (LaunchPrefs.s_initializing)
			{
				throw new InvalidOperationException("LaunchPrefs.InitStart has already been called.");
			}
			LaunchPrefs.s_initializing = true;
		}
	}

	public static void InitEnd()
	{
		object obj = LaunchPrefs.s_initializationLock;
		lock (obj)
		{
			if (!LaunchPrefs.s_initializing)
			{
				throw new InvalidOperationException("LaunchPrefs.InitStart has not been called yet.");
			}
			if (LaunchPrefs.s_initialized)
			{
				throw new InvalidOperationException("LaunchPrefs.InitEnd has already been called.");
			}
			LaunchPrefs.s_initialized = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static LaunchPrefs.LaunchPrefParser<OUT> ThenTransform<IN, OUT>(this LaunchPrefs.LaunchPrefParser<IN> parser, Func<IN, OUT> transform)
	{
		return delegate(string representation, out OUT value)
		{
			IN @in;
			if (!parser(representation, out @in))
			{
				value = default(OUT);
				return false;
			}
			value = transform(@in);
			return true;
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly object s_initializationLock = new object();

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_initializing;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool s_initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, ILaunchPref> s_launchPrefs = new Dictionary<string, ILaunchPref>(StringComparer.OrdinalIgnoreCase);

	public static readonly ILaunchPref<bool> SkipNewsScreen = LaunchPrefs.Create<bool>(false, LaunchPrefs.Parsers.BOOL, "SkipNewsScreen");

	public static readonly ILaunchPref<string> UserDataFolder = LaunchPrefs.Create<string>(string.Empty, LaunchPrefs.Parsers.STRING, "UserDataFolder");

	public static readonly ILaunchPref<bool> PlayerPrefsFile = LaunchPrefs.Create<bool>((DeviceFlag.XBoxSeriesS | DeviceFlag.XBoxSeriesX).IsCurrent(), LaunchPrefs.Parsers.BOOL, "PlayerPrefsFile");

	public static readonly ILaunchPref<bool> AllowCrossplay = LaunchPrefs.Create<bool>(true, LaunchPrefs.Parsers.BOOL, "AllowCrossplay");

	public static readonly ILaunchPref<MapChunkDatabaseType> MapChunkDatabase = LaunchPrefs.Create<MapChunkDatabaseType>(MapChunkDatabaseType.Region, LaunchPrefs.EnumParsers<MapChunkDatabaseType>.CASE_INSENSITIVE, "MapChunkDatabase");

	public static readonly ILaunchPref<bool> LoadSaveGame = LaunchPrefs.Create<bool>(false, LaunchPrefs.Parsers.BOOL, "LoadSaveGame");

	public static readonly ILaunchPref<bool> AllowJoinConfigModded = LaunchPrefs.Create<bool>((DeviceFlag.StandaloneWindows | DeviceFlag.StandaloneLinux | DeviceFlag.StandaloneOSX).IsCurrent(), LaunchPrefs.Parsers.BOOL, "AllowJoinConfigModded");

	public static readonly ILaunchPref<int> MaxWorldSizeHost = LaunchPrefs.Create<int>(PlatformOptimizations.DefaultMaxWorldSizeHost, LaunchPrefs.Parsers.INT, "MaxWorldSizeHost");

	public static readonly ILaunchPref<int> MaxWorldSizeClient = LaunchPrefs.Create<int>(-1, LaunchPrefs.Parsers.INT, "MaxWorldSizeClient");

	public static readonly ILaunchPref<string> SessionInvite = LaunchPrefs.Create<string>(string.Empty, LaunchPrefs.Parsers.STRING, "SessionInvite");

	public static readonly ILaunchPref<bool> EnableProps = LaunchPrefs.Create<bool>(false, LaunchPrefs.Parsers.BOOL, "EnableProps");

	public static readonly ILaunchPref<SaveStorageProvider> GameCoreSaveStorageProvider = LaunchPrefs.Create<SaveStorageProvider>(SaveStorageProvider.Blobs, LaunchPrefs.EnumParsers<SaveStorageProvider>.CASE_INSENSITIVE, "GameCoreSaveStorageProvider");

	public static readonly ILaunchPref<bool> GameCoreBlobCache = LaunchPrefs.Create<bool>(true, LaunchPrefs.Parsers.BOOL, "GameCoreBlobCache");

	public static readonly ILaunchPref<bool> GameCoreBlobCacheProactive = LaunchPrefs.Create<bool>(false, LaunchPrefs.Parsers.BOOL, "GameCoreBlobCacheProactive");

	public static readonly ILaunchPref<int> GameCoreBlobOperationQueueMaxTotalSize = LaunchPrefs.Create<int>(PlatformOptimizations.DefaultGameCoreBlobOperationQueueMaxTotalSize, LaunchPrefs.Parsers.INT, "GameCoreBlobOperationQueueMaxTotalSize");

	public static readonly ILaunchPref<string> RunAutomation = LaunchPrefs.Create<string>(string.Empty, LaunchPrefs.Parsers.STRING, "RunAutomation");

	public static readonly ILaunchPref<string> SubmissionOverride = LaunchPrefs.Create<string>(string.Empty, LaunchPrefs.Parsers.STRING, "SubmissionOverride");

	[PublicizedFrom(EAccessModifier.Private)]
	public delegate bool LaunchPrefParser<T>(string stringRepresentation, out T value);

	[PublicizedFrom(EAccessModifier.Private)]
	public static class Parsers
	{
		public static readonly LaunchPrefs.LaunchPrefParser<int> INT = delegate(string s, out int value)
		{
			return int.TryParse(s, out value);
		};

		public static readonly LaunchPrefs.LaunchPrefParser<long> LONG = delegate(string s, out long value)
		{
			return long.TryParse(s, out value);
		};

		public static readonly LaunchPrefs.LaunchPrefParser<ulong> ULONG = delegate(string s, out ulong value)
		{
			return ulong.TryParse(s, out value);
		};

		public static readonly LaunchPrefs.LaunchPrefParser<bool> BOOL = delegate(string s, out bool value)
		{
			return bool.TryParse(s, out value);
		};

		public static readonly LaunchPrefs.LaunchPrefParser<string> STRING = delegate(string s, out string value)
		{
			value = s;
			return true;
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static class EnumParsers<TEnum> where TEnum : struct, IConvertible
	{
		public static readonly LaunchPrefs.LaunchPrefParser<TEnum> CASE_SENSITIVE = delegate(string s, out TEnum value)
		{
			return EnumUtils.TryParse<TEnum>(s, out value, false);
		};

		public static readonly LaunchPrefs.LaunchPrefParser<TEnum> CASE_INSENSITIVE = delegate(string s, out TEnum value)
		{
			return EnumUtils.TryParse<TEnum>(s, out value, true);
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public abstract class LaunchPref : ILaunchPref
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public LaunchPref(string name)
		{
			if (LaunchPrefs.s_initializing)
			{
				throw new InvalidOperationException("LaunchPref should be instantiated before LaunchPrefs initialization begins.");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentException("LaunchPref requires a name", "name");
			}
			if (!LaunchPrefs.s_launchPrefs.TryAdd(name, this))
			{
				throw new InvalidOperationException("There is already a LaunchPref with the name '" + name + "'");
			}
			this.Name = name;
		}

		public string Name { get; }

		public abstract bool TrySet(string stringRepresentation);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public sealed class LaunchPref<T> : LaunchPrefs.LaunchPref, ILaunchPref<T>, ILaunchPref
	{
		public LaunchPref(string name, T defaultValue, LaunchPrefs.LaunchPrefParser<T> parser)
			: base(name)
		{
			if (name == null)
			{
				throw new ArgumentNullException("name");
			}
			this.m_value = defaultValue;
			this.m_parser = parser;
		}

		public override bool TrySet(string stringRepresentation)
		{
			if (!LaunchPrefs.s_initializing || LaunchPrefs.s_initialized)
			{
				throw new InvalidOperationException("LaunchPref can only be set during LaunchPrefs initialization.");
			}
			T t;
			if (!this.m_parser(stringRepresentation, out t))
			{
				return false;
			}
			this.m_value = t;
			return true;
		}

		public T Value
		{
			get
			{
				if (!LaunchPrefs.s_initialized)
				{
					throw new InvalidOperationException("LaunchPref can only be read after LaunchPrefs has finished initializing.");
				}
				return this.m_value;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public T m_value;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly LaunchPrefs.LaunchPrefParser<T> m_parser;
	}
}
