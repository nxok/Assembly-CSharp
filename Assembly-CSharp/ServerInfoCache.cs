using System;
using System.Collections.Generic;
using System.Text;

public class ServerInfoCache
{
	public static ServerInfoCache Instance
	{
		get
		{
			ServerInfoCache serverInfoCache;
			if ((serverInfoCache = ServerInfoCache.instance) == null)
			{
				serverInfoCache = (ServerInfoCache.instance = new ServerInfoCache());
			}
			return serverInfoCache;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int serverInfoToPersistentKey(GameServerInfo _gsi)
	{
		return (_gsi.GetValue(GameInfoString.IP) + _gsi.GetValue(GameInfoInt.Port).ToString()).GetHashCode();
	}

	public void SavePassword(GameServerInfo _gsi, string _password)
	{
		Dictionary<int, string> passwordCacheList = this.GetPasswordCacheList();
		int num = this.serverInfoToPersistentKey(_gsi);
		passwordCacheList[num] = _password;
		GamePrefs.Set(EnumGamePrefs.ServerPasswordCache, this.DictToString<int, string>(passwordCacheList));
	}

	public string GetPassword(GameServerInfo _gsi)
	{
		Dictionary<int, string> passwordCacheList = this.GetPasswordCacheList();
		int num = this.serverInfoToPersistentKey(_gsi);
		string text;
		if (!passwordCacheList.TryGetValue(num, out text))
		{
			return "";
		}
		return text;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<int, string> GetPasswordCacheList()
	{
		Dictionary<int, string> dictionary;
		try
		{
			dictionary = this.StringToDict<int, string>(GamePrefs.GetString(EnumGamePrefs.ServerPasswordCache), new Func<string, int>(Convert.ToInt32), (string _valueString) => _valueString);
		}
		catch (Exception)
		{
			GamePrefs.Set(EnumGamePrefs.ServerPasswordCache, "");
			dictionary = new Dictionary<int, string>();
		}
		return dictionary;
	}

	public void AddHistory(GameServerInfo _info)
	{
		try
		{
			this.getFavHistoryCacheList();
			ServerInfoCache.FavoritesHistoryKey keyFromInfo = this.getKeyFromInfo(_info);
			ServerInfoCache.FavoritesHistoryValue favoritesHistoryValue;
			if (!this.favoritesHistoryCache.TryGetValue(keyFromInfo, out favoritesHistoryValue))
			{
				favoritesHistoryValue = new ServerInfoCache.FavoritesHistoryValue(0U, false);
				this.favoritesHistoryCache[keyFromInfo] = favoritesHistoryValue;
			}
			favoritesHistoryValue.LastPlayedTime = Utils.CurrentUnixTime;
			this.saveFavHistoryCacheList();
			Log.Out("[NET] Added server to history: " + keyFromInfo.Address);
		}
		catch (Exception ex)
		{
			Log.Error("Could not add server " + _info.GetValue(GameInfoString.IP) + " to history:");
			Log.Exception(ex);
		}
	}

	public uint IsHistory(GameServerInfo _info)
	{
		uint num;
		try
		{
			this.getFavHistoryCacheList();
			ServerInfoCache.FavoritesHistoryKey keyFromInfo = this.getKeyFromInfo(_info);
			if (keyFromInfo == null)
			{
				Log.Warning("Could not check if server " + _info.GetValue(GameInfoString.IP) + " is in history: Invalid IP/port");
				num = 0U;
			}
			else
			{
				ServerInfoCache.FavoritesHistoryValue favoritesHistoryValue;
				num = ((!this.favoritesHistoryCache.TryGetValue(keyFromInfo, out favoritesHistoryValue)) ? 0U : favoritesHistoryValue.LastPlayedTime);
			}
		}
		catch (Exception ex)
		{
			Log.Error("Could not check if server " + _info.GetValue(GameInfoString.IP) + " is in history:");
			Log.Exception(ex);
			num = 0U;
		}
		return num;
	}

	public void ToggleFavorite(GameServerInfo _info)
	{
		try
		{
			this.getFavHistoryCacheList();
			ServerInfoCache.FavoritesHistoryKey keyFromInfo = this.getKeyFromInfo(_info);
			ServerInfoCache.FavoritesHistoryValue favoritesHistoryValue;
			if (!this.favoritesHistoryCache.TryGetValue(keyFromInfo, out favoritesHistoryValue))
			{
				favoritesHistoryValue = new ServerInfoCache.FavoritesHistoryValue(0U, false);
				this.favoritesHistoryCache[keyFromInfo] = favoritesHistoryValue;
			}
			_info.IsFavorite = (favoritesHistoryValue.IsFavorite = !favoritesHistoryValue.IsFavorite);
			if (!favoritesHistoryValue.IsFavorite && favoritesHistoryValue.LastPlayedTime == 0U)
			{
				this.favoritesHistoryCache.Remove(keyFromInfo);
			}
			this.saveFavHistoryCacheList();
			Log.Out(string.Format("[NET] Toggled server favorite: {0} - {1}", keyFromInfo.Address, favoritesHistoryValue.IsFavorite));
		}
		catch (Exception ex)
		{
			Log.Error("Could not toggle server " + _info.GetValue(GameInfoString.IP) + " favorite:");
			Log.Exception(ex);
		}
	}

	public bool IsFavorite(GameServerInfo _info)
	{
		bool flag;
		try
		{
			this.getFavHistoryCacheList();
			ServerInfoCache.FavoritesHistoryKey keyFromInfo = this.getKeyFromInfo(_info);
			if (keyFromInfo == null)
			{
				Log.Warning("Could not check if server " + _info.GetValue(GameInfoString.IP) + " is favorite: Invalid IP/port");
				flag = false;
			}
			else
			{
				ServerInfoCache.FavoritesHistoryValue favoritesHistoryValue;
				flag = this.favoritesHistoryCache.TryGetValue(keyFromInfo, out favoritesHistoryValue) && favoritesHistoryValue.IsFavorite;
			}
		}
		catch (Exception ex)
		{
			Log.Error("Could not check if server " + _info.GetValue(GameInfoString.IP) + " is favorite:");
			Log.Exception(ex);
			flag = false;
		}
		return flag;
	}

	public Dictionary<ServerInfoCache.FavoritesHistoryKey, ServerInfoCache.FavoritesHistoryValue>.Enumerator GetFavoriteServersEnumerator()
	{
		this.getFavHistoryCacheList();
		return this.favoritesHistoryCache.GetEnumerator();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ServerInfoCache.FavoritesHistoryKey getKeyFromInfo(GameServerInfo _info)
	{
		string value = _info.GetValue(GameInfoString.IP);
		int value2 = _info.GetValue(GameInfoInt.Port);
		if (string.IsNullOrEmpty(value) || value2 < 1 || value2 > 65535)
		{
			return null;
		}
		return new ServerInfoCache.FavoritesHistoryKey(value, value2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void getFavHistoryCacheList()
	{
		if (this.favoritesHistoryCache != null)
		{
			return;
		}
		try
		{
			this.favoritesHistoryCache = this.StringToDict<ServerInfoCache.FavoritesHistoryKey, ServerInfoCache.FavoritesHistoryValue>(GamePrefs.GetString(EnumGamePrefs.ServerHistoryCache), new Func<string, ServerInfoCache.FavoritesHistoryKey>(ServerInfoCache.FavoritesHistoryKey.FromString), new Func<string, ServerInfoCache.FavoritesHistoryValue>(ServerInfoCache.FavoritesHistoryValue.FromString));
		}
		catch (Exception)
		{
			GamePrefs.Set(EnumGamePrefs.ServerHistoryCache, "");
			this.favoritesHistoryCache = new Dictionary<ServerInfoCache.FavoritesHistoryKey, ServerInfoCache.FavoritesHistoryValue>();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void saveFavHistoryCacheList()
	{
		GamePrefs.Set(EnumGamePrefs.ServerHistoryCache, this.DictToString<ServerInfoCache.FavoritesHistoryKey, ServerInfoCache.FavoritesHistoryValue>(this.favoritesHistoryCache));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string escapeString(string _input)
	{
		return _input.Replace(ServerInfoCache.elementSeparatorString, ServerInfoCache.elementSeparatorEscaped).Replace(ServerInfoCache.fieldSeparatorString, ServerInfoCache.fieldSeparatorEscaped);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string unescapeString(string _input)
	{
		return _input.Replace(ServerInfoCache.elementSeparatorEscaped, ServerInfoCache.elementSeparatorString).Replace(ServerInfoCache.fieldSeparatorEscaped, ServerInfoCache.fieldSeparatorString);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string DictToString<TKey, TValue>(Dictionary<TKey, TValue> _dictionary)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (KeyValuePair<TKey, TValue> keyValuePair in _dictionary)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(';');
			}
			StringBuilder stringBuilder2 = stringBuilder;
			TKey key = keyValuePair.Key;
			stringBuilder2.Append(this.escapeString(key.ToString()));
			stringBuilder.Append(':');
			StringBuilder stringBuilder3 = stringBuilder;
			TValue value = keyValuePair.Value;
			stringBuilder3.Append(this.escapeString(value.ToString()));
		}
		return stringBuilder.ToString();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<TKey, TValue> StringToDict<TKey, TValue>(string _input, Func<string, TKey> _keyParser, Func<string, TValue> _valueParser)
	{
		if (string.IsNullOrEmpty(_input))
		{
			return new Dictionary<TKey, TValue>();
		}
		Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>(Math.Max(_input.Length >> 5, 16));
		int i = 0;
		while (i < _input.Length)
		{
			int num = this.findNextSeparator(_input, ';', i);
			int num2 = this.findNextSeparator(_input, ':', i);
			if (num2 >= num)
			{
				Log.Warning("Invalid cache elementy: No field separator found in '" + _input.Substring(i, num - i) + "'");
				i = num + 1;
			}
			else
			{
				string text = this.unescapeString(_input.Substring(i, num2 - i));
				string text2 = this.unescapeString(_input.Substring(num2 + 1, num - num2 - 1));
				i = num + 1;
				TKey tkey = _keyParser(text);
				TValue tvalue = _valueParser(text2);
				if (dictionary.ContainsKey(tkey))
				{
					Log.Warning(string.Format("Cache contains multiple elements for '{0}'", tkey));
				}
				dictionary[tkey] = tvalue;
			}
		}
		return dictionary;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int findNextSeparator(string _input, char _separator, int _startInclusive)
	{
		int num = _input.IndexOf(_separator, _startInclusive);
		while (num >= 0 && num < _input.Length - 1 && _input[num + 1] == _separator)
		{
			num = _input.IndexOf(_separator, num + 2);
		}
		if (num < 0)
		{
			return _input.Length;
		}
		return num;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static ServerInfoCache instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<ServerInfoCache.FavoritesHistoryKey, ServerInfoCache.FavoritesHistoryValue> favoritesHistoryCache;

	[PublicizedFrom(EAccessModifier.Private)]
	public const char elementSeparator = ';';

	[PublicizedFrom(EAccessModifier.Private)]
	public const char fieldSeparator = ':';

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string elementSeparatorString = ';'.ToString();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string fieldSeparatorString = ':'.ToString();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string elementSeparatorEscaped = new string(';', 2);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string fieldSeparatorEscaped = new string(':', 2);

	public class FavoritesHistoryKey : IEquatable<ServerInfoCache.FavoritesHistoryKey>
	{
		public FavoritesHistoryKey(string _address, int _port)
		{
			if (string.IsNullOrEmpty(_address))
			{
				throw new ArgumentException("Parameter must contain a valid IP", "_address");
			}
			if (_port < 1 || _port > 65535)
			{
				throw new ArgumentException(string.Format("Parameter needs to be a valid port (is: {0})", _port), "_port");
			}
			this.Address = _address;
			this.Port = _port;
		}

		public override string ToString()
		{
			return string.Format("{0}${1}", this.Address, this.Port);
		}

		public static ServerInfoCache.FavoritesHistoryKey FromString(string _input)
		{
			int num = _input.IndexOf('$');
			string text = _input.Substring(0, num);
			int num2 = int.Parse(_input.Substring(num + 1));
			return new ServerInfoCache.FavoritesHistoryKey(text, num2);
		}

		public bool Equals(ServerInfoCache.FavoritesHistoryKey _other)
		{
			return _other != null && (this == _other || (string.Equals(this.Address, _other.Address, StringComparison.OrdinalIgnoreCase) && this.Port == _other.Port));
		}

		public override bool Equals(object _obj)
		{
			return _obj != null && (this == _obj || (_obj.GetType() == base.GetType() && this.Equals((ServerInfoCache.FavoritesHistoryKey)_obj)));
		}

		public override int GetHashCode()
		{
			return (StringComparer.OrdinalIgnoreCase.GetHashCode(this.Address) * 397) ^ this.Port;
		}

		public readonly string Address;

		public readonly int Port;
	}

	public class FavoritesHistoryValue
	{
		public FavoritesHistoryValue(uint _lastPlayedTime, bool _isFavorite)
		{
			this.LastPlayedTime = _lastPlayedTime;
			this.IsFavorite = _isFavorite;
		}

		public override string ToString()
		{
			return string.Format("{0}${1}", this.LastPlayedTime, this.IsFavorite);
		}

		public static ServerInfoCache.FavoritesHistoryValue FromString(string _input)
		{
			int num = _input.IndexOf('$');
			string text = _input.Substring(0, num);
			string text2 = _input.Substring(num + 1);
			uint num2 = uint.Parse(text);
			bool flag = bool.Parse(text2);
			return new ServerInfoCache.FavoritesHistoryValue(num2, flag);
		}

		public uint LastPlayedTime;

		public bool IsFavorite;
	}
}
