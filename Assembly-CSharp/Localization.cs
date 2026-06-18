using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Noemax.GZip;
using Platform;
using UnityEngine;

public static class Localization
{
	public static bool LocalizationChecks
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public static event Action<string> LanguageSelected;

	public static Dictionary<string, string[]> Dictionary
	{
		get
		{
			Localization.checkLoaded(false);
			return Localization.mDictionary;
		}
	}

	public static string[] KnownLanguages
	{
		get
		{
			Localization.checkLoaded(true);
			return Localization.allLanguages.ToArray();
		}
	}

	public static string ActiveLanguage
	{
		get
		{
			Localization.checkLoaded(true);
			if (Localization.currentLanguageIndex >= 0)
			{
				return Localization.allLanguages[Localization.currentLanguageIndex];
			}
			if (Localization.allLanguages.ContainsCaseInsensitive("english"))
			{
				return "english";
			}
			return "";
		}
		set
		{
			Debug.Log("Language selected: " + value);
		}
	}

	public static string RequestedLanguage
	{
		get
		{
			string launchArgument = GameUtils.GetLaunchArgument("language");
			if (!string.IsNullOrEmpty(launchArgument))
			{
				return launchArgument;
			}
			if (!string.IsNullOrEmpty(Localization.platformLanguage))
			{
				return Localization.platformLanguage;
			}
			return "english";
		}
	}

	public static int TotalKeys
	{
		get
		{
			Localization.checkLoaded(false);
			return Localization.mDictionary.Count;
		}
	}

	public static byte[] PatchedData
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void checkLoaded(bool _throwExc = false)
	{
		if (Localization.mDictionary == null)
		{
			Localization.LoadAndSelectLanguage(false);
		}
		if (Localization.mDictionary == null && _throwExc)
		{
			throw new Exception("Localization could not be loaded");
		}
	}

	public static void Init()
	{
		if (Localization.initialized)
		{
			return;
		}
		Localization.initialized = true;
		string launchArgument = GameUtils.GetLaunchArgument("language");
		if (!string.IsNullOrEmpty(launchArgument))
		{
			Log.Out("Localization language from command line: " + launchArgument);
		}
		else
		{
			string @string = GamePrefs.GetString(EnumGamePrefs.Language);
			if (!string.IsNullOrEmpty(@string))
			{
				Localization.platformLanguage = @string;
				Log.Out("Localization language from prefs: " + @string);
			}
			else
			{
				PlatformManager.NativePlatform.Api.ClientApiInitialized += delegate
				{
					IUtils utils = PlatformManager.NativePlatform.Utils;
					Localization.platformLanguage = ((utils != null) ? utils.GetAppLanguage() : null) ?? "english";
					Localization.LoadAndSelectLanguage(false);
					Log.Out("Localization language from platform: " + Localization.platformLanguage);
				};
			}
		}
		Localization.LoadAndSelectLanguage(false);
		Localization.LocalizationChecks = GameUtils.GetLaunchArgument("localizationchecks") != null;
	}

	public static bool ReloadBaseLocalization()
	{
		return Localization.LoadAndSelectLanguage(true);
	}

	public static bool LoadAndSelectLanguage(bool _forceReload = false)
	{
		if (_forceReload)
		{
			Localization.mDictionary = null;
			Localization.mDictionaryCaseInsensitive = null;
		}
		return (Localization.mDictionary != null || Localization.loadBaseDictionaries()) && Localization.selectLanguage();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool loadBaseDictionaries()
	{
		Localization.allLanguages.Clear();
		Localization.patchedCells.Clear();
		Localization.mDictionary = new Dictionary<string, string[]>();
		Localization.mDictionaryCaseInsensitive = new CaseInsensitiveStringDictionary<string[]>();
		if (!Localization.loadCsv(GameIO.GetGameDir("Data/Config") + "/Localization.csv", false))
		{
			Localization.mDictionary = null;
			Localization.mDictionaryCaseInsensitive = null;
			return false;
		}
		Localization.updateLanguages();
		Localization.WriteCsv();
		return true;
	}

	public static bool LoadPatchDictionaries(string _modName, string _folder, bool _loadingInGame)
	{
		Localization.checkLoaded(true);
		string text = _folder + "/Localization.csv";
		if (SdFile.Exists(text))
		{
			Log.Out("[MODS] Loading localization from mod: " + _modName);
			if (!Localization.loadCsv(text, true))
			{
				Log.Error("[MODS] Could not load localization from " + text);
			}
		}
		Localization.updateLanguages();
		Localization.selectLanguage();
		return true;
	}

	public static bool LoadServerPatchDictionary(byte[] _data)
	{
		Localization.checkLoaded(true);
		if (!Localization.loadCsv(_data, true, true))
		{
			Log.Error("Could not load localization from server!");
		}
		Localization.updateLanguages();
		Localization.selectLanguage();
		return true;
	}

	public static void UnloadMetaData()
	{
		string[] array;
		Localization.mDictionary.TryGetValue("KEY", out array);
		if (array == null)
		{
			Log.Error("Failed unloading Localization metadata columns, could not find header entry!");
			return;
		}
		int[] array2 = new int[Localization.ignoredMetaColumnNames.Length];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = -1;
		}
		for (int j = 0; j < Localization.ignoredMetaColumnNames.Length; j++)
		{
			string text = Localization.ignoredMetaColumnNames[j];
			for (int k = 0; k < array.Length; k++)
			{
				if (array[k].EqualsCaseInsensitive(text))
				{
					array2[j] = k;
					break;
				}
			}
		}
		foreach (KeyValuePair<string, string[]> keyValuePair in Localization.mDictionary)
		{
			string text2;
			string[] array3;
			keyValuePair.Deconstruct(out text2, out array3);
			string text3 = text2;
			string[] array4 = array3;
			if (!text3.EqualsCaseInsensitive("KEY"))
			{
				foreach (int num in array2)
				{
					if (num >= 0 && num < array4.Length)
					{
						array4[num] = null;
					}
				}
			}
		}
	}

	public static void UnloadUnusedLanguages()
	{
		string[] array;
		Localization.mDictionary.TryGetValue("KEY", out array);
		if (array == null)
		{
			Log.Error("Failed unloading Localization metadata columns, could not find header entry!");
			return;
		}
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].EqualsCaseInsensitive("UsedInMainMenu"))
			{
				num = i;
			}
			if (array[i].EqualsCaseInsensitive("KeepLoaded"))
			{
				num2 = i;
			}
		}
		foreach (KeyValuePair<string, string[]> keyValuePair in Localization.mDictionary)
		{
			string text;
			string[] array2;
			keyValuePair.Deconstruct(out text, out array2);
			string text2 = text;
			string[] array3 = array2;
			if (!text2.EqualsCaseInsensitive("KEY") && (num2 < 0 || num2 >= array3.Length || string.IsNullOrEmpty(array3[num2])))
			{
				for (int j = 0; j < array3.Length; j++)
				{
					if (j != num && j != Localization.currentLanguageIndex && (j != Localization.defaultLanguageIndex || Localization.currentLanguageIndex >= 0) && !string.IsNullOrEmpty(array3[Localization.currentLanguageIndex]))
					{
						array3[j] = null;
					}
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[return: TupleElementNames(new string[] { "defaultLanguage", "userLanguage" })]
	public static ValueTuple<int, int> findUserLanguageColumns(string[] _headerColumns, string _userLanguage)
	{
		if (_headerColumns == null || _headerColumns.Length == 0)
		{
			return new ValueTuple<int, int>(-1, -1);
		}
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < _headerColumns.Length; i++)
		{
			if (_headerColumns[i].EqualsCaseInsensitive("english"))
			{
				num2 = i;
			}
			if (_headerColumns[i].EqualsCaseInsensitive(_userLanguage))
			{
				num = i;
			}
		}
		return new ValueTuple<int, int>(num2, num);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool selectLanguage()
	{
		if (Localization.mDictionary == null)
		{
			return false;
		}
		string[] array;
		Localization.mDictionary.TryGetValue("KEY", out array);
		ValueTuple<int, int> valueTuple = Localization.findUserLanguageColumns(array, Localization.RequestedLanguage);
		Localization.defaultLanguageIndex = valueTuple.Item1;
		Localization.currentLanguageIndex = valueTuple.Item2;
		UIRoot.Broadcast("OnLocalize");
		Action<string> languageSelected = Localization.LanguageSelected;
		if (languageSelected != null)
		{
			languageSelected(Localization.ActiveLanguage);
		}
		return Localization.currentLanguageIndex >= 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void updateLanguages()
	{
		Localization.UnloadMetaData();
		Localization.allLanguages.Clear();
		Localization.languageToColumnIndex.Clear();
		if (Localization.mDictionary == null)
		{
			return;
		}
		for (int i = 0; i < Localization.mDictionary["KEY"].Length; i++)
		{
			string text = Localization.mDictionary["KEY"][i];
			Localization.allLanguages.Add(text);
			Localization.languageToColumnIndex[text] = i;
		}
	}

	public static void WriteCsv()
	{
		new MicroStopwatch(true);
		using (PooledMemoryStream pooledMemoryStream = MemoryPools.poolMS.AllocSync(true))
		{
			using (TextWriter textWriter = new StreamWriter(pooledMemoryStream, Encoding.UTF8, 1024, true))
			{
				textWriter.Write("KEY");
				string[] array = Localization.mDictionary["KEY"];
				for (int i = 0; i < array.Length; i++)
				{
					textWriter.Write(",");
					textWriter.Write(array[i]);
				}
				textWriter.WriteLine();
				int num = array.Length;
				foreach (KeyValuePair<string, bool[]> keyValuePair in Localization.patchedCells)
				{
					if (Localization.mDictionaryCaseInsensitive.TryGetValue(keyValuePair.Key, out array))
					{
						textWriter.Write(keyValuePair.Key);
						int j;
						for (j = 0; j < array.Length; j++)
						{
							textWriter.Write(',');
							if (keyValuePair.Value[j])
							{
								string text = array[j] ?? "";
								text = text.Replace("\n", "\\n");
								bool flag = text.IndexOf('"') >= 0 || text.IndexOf(',') >= 0;
								if (flag)
								{
									textWriter.Write('"');
									text = text.Replace("\"", "\"\"");
								}
								textWriter.Write(text);
								if (flag)
								{
									textWriter.Write('"');
								}
							}
						}
						while (j < num)
						{
							textWriter.Write(',');
							j++;
						}
						textWriter.WriteLine();
					}
				}
			}
			pooledMemoryStream.Position = 0L;
			using (PooledMemoryStream pooledMemoryStream2 = MemoryPools.poolMS.AllocSync(true))
			{
				using (DeflateOutputStream deflateOutputStream = new DeflateOutputStream(pooledMemoryStream2, 3))
				{
					StreamUtils.StreamCopy(pooledMemoryStream, deflateOutputStream, null, true);
					Localization.PatchedData = pooledMemoryStream2.ToArray();
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool getLanguageEntry(string[] _entry, int _languageColumn, out string _result, string _localizationCheckPrefix = "L_")
	{
		if (_languageColumn < 0 || _languageColumn >= _entry.Length)
		{
			_result = null;
			return false;
		}
		string text = _entry[_languageColumn];
		if (string.IsNullOrEmpty(text))
		{
			_result = null;
			return false;
		}
		if (!Localization.LocalizationChecks)
		{
			_result = text;
		}
		else
		{
			_result = _localizationCheckPrefix + text;
		}
		return true;
	}

	public static string Get(string _key, bool _caseInsensitive = false, string _languageName = null)
	{
		Localization.checkLoaded(false);
		if (string.IsNullOrEmpty(_key))
		{
			return "";
		}
		int num = -1;
		if (!string.IsNullOrEmpty(_languageName) && !Localization.languageToColumnIndex.TryGetValue(_languageName, out num))
		{
			Log.Warning(string.Concat(new string[] { "[Localization] Requested '", _key, "' for non-existing language '", _languageName, "'" }));
			return _key;
		}
		string[] array;
		if ((_caseInsensitive ? Localization.mDictionaryCaseInsensitive : Localization.mDictionary).TryGetValue(_key, out array))
		{
			string text2;
			if (num < 0)
			{
				string text;
				if (Localization.getLanguageEntry(array, Localization.currentLanguageIndex, out text, "L_") || Localization.getLanguageEntry(array, Localization.defaultLanguageIndex, out text, "LE_"))
				{
					return text;
				}
			}
			else if (Localization.getLanguageEntry(array, num, out text2, "L_"))
			{
				return text2;
			}
		}
		if (Localization.LocalizationChecks)
		{
			return "UL_" + _key;
		}
		return _key;
	}

	public static bool Exists(string _key, bool _caseInsensitive = false)
	{
		Localization.checkLoaded(false);
		if (string.IsNullOrEmpty(_key))
		{
			return false;
		}
		if (!_caseInsensitive)
		{
			return Localization.mDictionary.ContainsKey(_key);
		}
		return Localization.mDictionaryCaseInsensitive.ContainsKey(_key);
	}

	public static bool TryGet(string _key, out string _localizedString)
	{
		if (!Localization.Exists(_key, false))
		{
			_localizedString = _key;
			return false;
		}
		_localizedString = Localization.Get(_key, false, null);
		return true;
	}

	public static string FormatListAnd(params object[] _items)
	{
		return Localization.formatListX("listAndTwo", "listAndStart", "listAndMiddle", "listAndEnd", _items);
	}

	public static string FormatListOr(params object[] _items)
	{
		return Localization.formatListX("listOrTwo", "listOrStart", "listOrMiddle", "listOrEnd", _items);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string formatListX(string _listXTwo, string _listXStart, string _listXMiddle, string _listXEnd, object[] _items)
	{
		Localization.<>c__DisplayClass56_0 CS$<>8__locals1;
		CS$<>8__locals1._listXStart = _listXStart;
		CS$<>8__locals1._items = _items;
		CS$<>8__locals1._listXMiddle = _listXMiddle;
		CS$<>8__locals1._listXEnd = _listXEnd;
		int num = CS$<>8__locals1._items.Length;
		string text;
		if (num > 0)
		{
			if (num != 1)
			{
				if (num != 2)
				{
					text = Localization.<formatListX>g__FormatThreeOrMore|56_0(ref CS$<>8__locals1);
				}
				else
				{
					text = string.Format(Localization.Get(_listXTwo, false, null), CS$<>8__locals1._items);
				}
			}
			else
			{
				text = CS$<>8__locals1._items[0].ToString();
			}
		}
		else
		{
			text = string.Empty;
		}
		return text;
	}

	public static IEnumerable<KeyValuePair<string, string[]>> EnumerateAll()
	{
		foreach (KeyValuePair<string, string[]> keyValuePair in Localization.mDictionary)
		{
			yield return keyValuePair;
		}
		Dictionary<string, string[]>.Enumerator enumerator = default(Dictionary<string, string[]>.Enumerator);
		yield break;
		yield break;
	}

	public static string GetCurrentLocale()
	{
		string activeLanguage = Localization.ActiveLanguage;
		uint num = <PrivateImplementationDetails>.ComputeStringHash(activeLanguage);
		if (num <= 2499415067U)
		{
			if (num <= 599131013U)
			{
				if (num != 380651494U)
				{
					if (num != 505713757U)
					{
						if (num == 599131013U)
						{
							if (activeLanguage == "french")
							{
								return "fr-FR";
							}
						}
					}
					else if (activeLanguage == "brazilian")
					{
						return "pt-BR";
					}
				}
				else if (activeLanguage == "russian")
				{
					return "ru-RU";
				}
			}
			else if (num != 1901528810U)
			{
				if (num != 2471602315U)
				{
					if (num == 2499415067U)
					{
						if (activeLanguage == "english")
						{
							return "en-US";
						}
					}
				}
				else if (activeLanguage == "italian")
				{
					return "it-IT";
				}
			}
			else if (activeLanguage == "japanese")
			{
				return "ja-JP";
			}
		}
		else if (num <= 3210859552U)
		{
			if (num != 2805355685U)
			{
				if (num != 3180870988U)
				{
					if (num == 3210859552U)
					{
						if (activeLanguage == "koreana")
						{
							return "ko-KR";
						}
					}
				}
				else if (activeLanguage == "polish")
				{
					return "pl-PL";
				}
			}
			else if (activeLanguage == "schinese")
			{
				return "zh-Hans";
			}
		}
		else if (num <= 3405445907U)
		{
			if (num != 3264533134U)
			{
				if (num == 3405445907U)
				{
					if (activeLanguage == "german")
					{
						return "de-DE";
					}
				}
			}
			else if (activeLanguage == "tchinese")
			{
				return "zh-Hant";
			}
		}
		else if (num != 3719199419U)
		{
			if (num == 3739448251U)
			{
				if (activeLanguage == "turkish")
				{
					return "tr-TR";
				}
			}
		}
		else if (activeLanguage == "spanish")
		{
			return "es-ES";
		}
		throw new Exception("No language code mapping found for Language \"" + Localization.ActiveLanguage + "\".");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool loadCsv(string _filename, bool _patch = false)
	{
		return Localization.loadCsv(SdFile.ReadAllBytes(_filename), _patch, false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool loadCsv(byte[] _asset, bool _patch = false, bool _serverData = false)
	{
		ByteReader byteReader = new ByteReader(_asset);
		BetterList<string> betterList = byteReader.ReadCSV();
		if (betterList.size < 2)
		{
			return false;
		}
		betterList.buffer[0] = "KEY";
		if (!string.Equals(betterList.buffer[0], "KEY", StringComparison.OrdinalIgnoreCase))
		{
			Log.Error("Invalid localization CSV file. The first value is expected to be 'KEY', followed by language columns.\nInstead found '" + betterList.buffer[0] + "'");
			return false;
		}
		int[] array = null;
		int num = 0;
		int num2 = -1;
		if (!_patch)
		{
			Localization.mDictionary.Clear();
			Localization.mDictionaryCaseInsensitive.Clear();
		}
		else
		{
			string[] array2 = Localization.mDictionary["KEY"];
			num = array2.Length;
			array = new int[betterList.size];
			for (int i = 1; i < betterList.size; i++)
			{
				string text = betterList.buffer[i];
				if (!Localization.languageHeaderMatcher.IsMatch(text) && !text.StartsWith("context", StringComparison.OrdinalIgnoreCase))
				{
					Log.Error(string.Format("Invalid localization CSV file. The first row has to contain the column definition header. Column {0} is not a valid column definition (must be a non-empty string consisting of latin characters only): '{1}'", i + 1, text));
					return false;
				}
				int j;
				for (j = 0; j < array2.Length; j++)
				{
					if (text.EqualsCaseInsensitive(array2[j]))
					{
						array[i] = j;
						break;
					}
				}
				if (j >= array2.Length)
				{
					array[i] = num++;
				}
			}
			for (int k = 0; k < array2.Length; k++)
			{
				if (array2[k].EqualsCaseInsensitive("UsedInMainMenu"))
				{
					num2 = k;
					break;
				}
			}
		}
		while (betterList != null)
		{
			Localization.addCsv(betterList, array, num, num2, _serverData);
			betterList = byteReader.ReadCSV();
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void addCsv(BetterList<string> _temp, int[] _colTranslationTable, int _newLength, int _origColIndexUsedInMainMenu, bool _serverData = false)
	{
		if (_temp.size < 2)
		{
			return;
		}
		string text = _temp.buffer[0];
		if (string.IsNullOrEmpty(text))
		{
			Log.Warning("Localization: Entry missing a key! Please check Localization file. Skipping entry...");
			return;
		}
		if (_colTranslationTable != null)
		{
			string[] array;
			if (Localization.mDictionaryCaseInsensitive.TryGetValue(text, out array))
			{
				if (_serverData && _origColIndexUsedInMainMenu >= 0 && !string.IsNullOrEmpty(array[_origColIndexUsedInMainMenu]))
				{
					return;
				}
				if (array.Length < _newLength)
				{
					Array.Resize<string>(ref array, _newLength);
				}
				bool[] array2;
				if (Localization.patchedCells.TryGetValue(text, out array2))
				{
					if (array2.Length < _newLength)
					{
						Array.Resize<bool>(ref array2, _newLength);
					}
				}
				else
				{
					array2 = new bool[_newLength];
				}
				int num = 1;
				while (num < _temp.size && num < _colTranslationTable.Length)
				{
					if (!string.IsNullOrEmpty(_temp.buffer[num]))
					{
						int num2 = _colTranslationTable[num];
						array[num2] = _temp.buffer[num];
						array2[num2] = true;
					}
					num++;
				}
				if (_origColIndexUsedInMainMenu < 0 || string.IsNullOrEmpty(array[_origColIndexUsedInMainMenu]))
				{
					Localization.patchedCells[text] = array2;
				}
			}
			else
			{
				array = new string[_newLength];
				bool[] array3 = new bool[_newLength];
				int num3 = 1;
				while (num3 < _temp.size && num3 < _colTranslationTable.Length)
				{
					int num4 = _colTranslationTable[num3];
					array[num4] = _temp.buffer[num3];
					array3[num4] = true;
					num3++;
				}
				Localization.patchedCells.Add(text, array3);
			}
			Localization.mDictionary[text] = array;
			Localization.mDictionaryCaseInsensitive[text] = array;
			return;
		}
		string[] array4 = new string[_temp.size - 1];
		for (int i = 1; i < _temp.size; i++)
		{
			array4[i - 1] = _temp.buffer[i];
		}
		if (Localization.mDictionary.TryAdd(text, array4))
		{
			Localization.mDictionaryCaseInsensitive.Add(text, array4);
			return;
		}
		Log.Warning("Localization: Duplicate key \"" + text + "\" found! Please check Localization file. Skipping entry...");
	}

	public static int EstimateOwnedBytes()
	{
		int num = 0;
		foreach (KeyValuePair<string, string[]> keyValuePair in Localization.mDictionary)
		{
			string text;
			string[] array;
			keyValuePair.Deconstruct(out text, out array);
			string text2 = text;
			string[] array2 = array;
			num += MemoryTracker.GetSize(text2);
			num += MemoryTracker.GetSize<string>(array2);
			foreach (string text3 in array2)
			{
				num += MemoryTracker.GetSize(text3);
			}
		}
		return num;
	}

	[CompilerGenerated]
	[PublicizedFrom(EAccessModifier.Internal)]
	public static string <formatListX>g__FormatThreeOrMore|56_0(ref Localization.<>c__DisplayClass56_0 A_0)
	{
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder.AppendFormat(Localization.Get(A_0._listXStart, false, null), A_0._items[0], A_0._items[1]);
		for (int i = 2; i < A_0._items.Length - 1; i++)
		{
			stringBuilder2.AppendFormat(Localization.Get(A_0._listXMiddle, false, null), stringBuilder, A_0._items[i]);
			StringBuilder stringBuilder3 = stringBuilder2;
			StringBuilder stringBuilder4 = stringBuilder;
			stringBuilder = stringBuilder3;
			stringBuilder2 = stringBuilder4;
			stringBuilder2.Clear();
		}
		StringBuilder stringBuilder5 = stringBuilder2;
		string text = Localization.Get(A_0._listXEnd, false, null);
		object obj = stringBuilder;
		object[] items = A_0._items;
		stringBuilder5.AppendFormat(text, obj, items[items.Length - 1]);
		return stringBuilder2.ToString();
	}

	public const string DefaultLanguage = "english";

	public const string HeaderKey = "KEY";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string KeepLoadedColumnName = "KeepLoaded";

	[PublicizedFrom(EAccessModifier.Private)]
	public const string UsedInMainMenuColumnName = "UsedInMainMenu";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string[] ignoredMetaColumnNames = new string[] { "File", "Type", "NoTranslate", "Context / Alternate Text" };

	public const string MainLocalizationFilename = "Localization.csv";

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> allLanguages = new List<string>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, int> languageToColumnIndex = new CaseInsensitiveStringDictionary<int>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string[]> mDictionary;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Dictionary<string, string[]> mDictionaryCaseInsensitive;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, bool[]> patchedCells = new Dictionary<string, bool[]>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static int defaultLanguageIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int currentLanguageIndex = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public static string platformLanguage;

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool initialized;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex languageHeaderMatcher = new Regex("^[A-Za-z]+$");
}
