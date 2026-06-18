using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public static class ModManager
{
	public static string ModsBasePath
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return GameIO.GetDeviceLocalUserGameDataDir() + "/Mods";
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void initModManager()
	{
	}

	public static void LoadMods()
	{
		ModManager.initModManager();
		bool flag = ModManager.loadModsFromFolder(ModManager.ModsBasePath);
		bool flag2 = GameIO.PathsEquals(ModManager.ModsBasePath, ModManager.ModsBasePathLegacy, true) || ModManager.loadModsFromFolder(ModManager.ModsBasePathLegacy);
		if (!flag && !flag2)
		{
			Log.Out("[MODS] No mods folder found");
			return;
		}
		int num = ModManager.loadedMods.list.FindIndex((Mod _mod) => _mod.Name == "TFP_Harmony");
		if (num >= 0)
		{
			Mod mod = ModManager.loadedMods.list[num];
			ModManager.loadedMods.list.RemoveAt(num);
			ModManager.loadedMods.list.Insert(0, mod);
		}
		Log.Out("[MODS] Initializing mod code");
		foreach (Mod mod2 in ModManager.loadedMods.list)
		{
			mod2.InitModCode();
		}
		Log.Out("[MODS] Loading done");
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool loadModsFromFolder(string _folder)
	{
		if (!SdDirectory.Exists(_folder))
		{
			return false;
		}
		Log.Out("[MODS] Start loading from: '" + _folder + "'");
		string[] directories = SdDirectory.GetDirectories(_folder);
		Array.Sort<string>(directories);
		foreach (string text in directories)
		{
			Log.Out("[MODS]   Trying to load from folder: '" + Path.GetFileName(text) + "'");
			try
			{
				Mod mod = Mod.LoadDefinitionFromFolder(text);
				if (mod != null)
				{
					if (!mod.LoadMod())
					{
						ModManager.failedMods.Add(mod);
					}
					else
					{
						ModManager.loadedMods.Add(mod.Name, mod);
					}
				}
			}
			catch (Exception ex)
			{
				Log.Error("[MODS]     Failed loading mod from folder: '" + Path.GetFileName(text) + "'");
				Log.Exception(ex);
			}
		}
		return true;
	}

	public static bool ModLoaded(string _modName)
	{
		return ModManager.loadedMods.dict.ContainsKey(_modName);
	}

	public static Mod GetMod(string _modName, bool _onlyLoaded = false)
	{
		if (!ModManager.ModLoaded(_modName))
		{
			return null;
		}
		return ModManager.loadedMods.dict[_modName];
	}

	public static List<Mod> GetLoadedMods()
	{
		return ModManager.loadedMods.list;
	}

	public static List<Mod> GetFailedMods(Mod.EModLoadState? _failureReason = null)
	{
		if (_failureReason == null)
		{
			return ModManager.failedMods;
		}
		List<Mod> list = new List<Mod>();
		foreach (Mod mod in ModManager.failedMods)
		{
			if (mod.LoadState == _failureReason.Value)
			{
				list.Add(mod);
			}
		}
		return list;
	}

	public static List<Assembly> GetLoadedAssemblies()
	{
		List<Assembly> list = new List<Assembly>();
		for (int i = 0; i < ModManager.loadedMods.Count; i++)
		{
			Mod mod = ModManager.loadedMods.list[i];
			list.AddRange(mod.AllAssemblies);
		}
		return list;
	}

	public static Mod GetModForAssembly(Assembly _asm)
	{
		for (int i = 0; i < ModManager.loadedMods.Count; i++)
		{
			Mod mod = ModManager.loadedMods.list[i];
			if (mod.ContainsAssembly(_asm))
			{
				return mod;
			}
		}
		return null;
	}

	public static bool AnyConfigModActive()
	{
		for (int i = 0; i < ModManager.loadedMods.Count; i++)
		{
			if (ModManager.loadedMods.list[i].GameConfigMod)
			{
				return true;
			}
		}
		return false;
	}

	public static bool TryPatchModPathString(string _pathString, out string _modPath)
	{
		_modPath = null;
		if (_pathString.IndexOf('@') < 0)
		{
			return false;
		}
		int num = _pathString.IndexOf("@modfolder(", StringComparison.OrdinalIgnoreCase);
		if (num < 0)
		{
			return false;
		}
		int num2 = _pathString.IndexOf("):", StringComparison.Ordinal);
		int num3 = num + "@modfolder(".Length;
		string text = _pathString.Substring(num3, num2 - num3);
		string text2 = _pathString.Substring(0, num);
		int num4 = num2 + 2;
		while (_pathString[num4] == '/')
		{
			num4++;
		}
		string text3 = _pathString.Substring(num4);
		Mod mod = ModManager.GetMod(text, true);
		if (mod != null)
		{
			_pathString = text2 + mod.Path + "/" + text3;
			_modPath = _pathString;
			return true;
		}
		Log.Error("[MODS] Mod reference for a mod that is not loaded: '" + text + "'");
		return false;
	}

	public static string PatchModPathString(string _pathString)
	{
		string text;
		ModManager.TryPatchModPathString(_pathString, out text);
		return text;
	}

	public static IEnumerator LoadPatchStuff(bool _isLoadingInGame)
	{
		yield return ModManager.LoadUiAtlases(_isLoadingInGame);
		yield return ModManager.LoadLocalizations(_isLoadingInGame);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadUiAtlases(bool _isLoadingInGame)
	{
		if (GameManager.IsDedicatedServer)
		{
			yield break;
		}
		if (_isLoadingInGame)
		{
			yield break;
		}
		int num;
		for (int i = 0; i < ModManager.loadedMods.Count; i = num + 1)
		{
			Mod mod = ModManager.loadedMods.list[i];
			string text = mod.Path + "/UIAtlases";
			if (SdDirectory.Exists(text))
			{
				string[] array = null;
				try
				{
					array = SdDirectory.GetDirectories(text);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
				if (array != null)
				{
					string[] array2 = array;
					for (int j = 0; j < array2.Length; j++)
					{
						string text2 = array2[j];
						string fileName = Path.GetFileName(text2);
						ModManager.AtlasManagerEntry ame;
						if (!ModManager.atlasManagers.TryGetValue(fileName, out ame))
						{
							Log.Out(string.Concat(new string[] { "[MODS] Creating new atlas '", fileName, "' for mod '", mod.Name, "'" }));
							ModManager.RegisterAtlasManager(MultiSourceAtlasManager.Create(ModManager.atlasesParentGo, fileName), true, ModManager.defaultShader, null);
							ame = ModManager.atlasManagers[fileName];
						}
						yield return UIAtlasFromFolder.CreateUiAtlasFromFolder(text2, ame.Shader, delegate(UIAtlas _atlas)
						{
							_atlas.transform.parent = ame.Manager.transform;
							ame.Manager.AddAtlas(_atlas, _atlas.gameObject, _isLoadingInGame);
							Action<INGUIAtlas, bool> onNewAtlasLoaded = ame.OnNewAtlasLoaded;
							if (onNewAtlasLoaded == null)
							{
								return;
							}
							onNewAtlasLoaded(_atlas, _isLoadingInGame);
						});
					}
					array2 = null;
					mod = null;
				}
			}
			num = i;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static IEnumerator LoadLocalizations(bool _isLoadingInGame)
	{
		if (_isLoadingInGame)
		{
			yield break;
		}
		for (int i = 0; i < ModManager.loadedMods.Count; i++)
		{
			Mod mod = ModManager.loadedMods.list[i];
			string text = mod.Path + "/Config";
			if (SdDirectory.Exists(text))
			{
				try
				{
					Localization.LoadPatchDictionaries(mod.Name, text, _isLoadingInGame);
				}
				catch (Exception ex)
				{
					Log.Error("[MODS] Failed loading localization from mod: '" + mod.Name + "'");
					Log.Exception(ex);
				}
			}
		}
		Localization.WriteCsv();
		yield break;
	}

	public static void ModAtlasesDefaults(GameObject _parentGo, Shader _defaultShader)
	{
		ModManager.atlasesParentGo = _parentGo;
		ModManager.defaultShader = _defaultShader;
	}

	public static void RegisterAtlasManager(MultiSourceAtlasManager _atlasManager, bool _createdByMod, Shader _shader, Action<INGUIAtlas, bool> _onNewAtlasLoaded = null)
	{
		ModManager.atlasManagers.Add(_atlasManager.name, new ModManager.AtlasManagerEntry(_atlasManager, _createdByMod, _shader, _onNewAtlasLoaded));
	}

	public static MultiSourceAtlasManager GetAtlasManager(string _name)
	{
		ModManager.AtlasManagerEntry atlasManagerEntry;
		if (!ModManager.atlasManagers.TryGetValue(_name, out atlasManagerEntry))
		{
			return null;
		}
		return atlasManagerEntry.Manager;
	}

	public static void GameEnded()
	{
		foreach (KeyValuePair<string, ModManager.AtlasManagerEntry> keyValuePair in ModManager.atlasManagers)
		{
			keyValuePair.Value.Manager.CleanupAfterGame();
		}
		Localization.ReloadBaseLocalization();
		Localization.UnloadUnusedLanguages();
		ThreadManager.RunCoroutineSync(ModManager.LoadLocalizations(false));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly string ModsBasePathLegacy = ((Application.platform == 1 || Application.platform == 45) ? (Application.dataPath + "/../../Mods") : (Application.dataPath + "/../Mods"));

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly DictionaryList<string, Mod> loadedMods = new DictionaryList<string, Mod>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<Mod> failedMods = new List<Mod>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Dictionary<string, ModManager.AtlasManagerEntry> atlasManagers = new CaseInsensitiveStringDictionary<ModManager.AtlasManagerEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static GameObject atlasesParentGo;

	[PublicizedFrom(EAccessModifier.Private)]
	public static Shader defaultShader;

	[PublicizedFrom(EAccessModifier.Private)]
	public class AtlasManagerEntry
	{
		public AtlasManagerEntry(MultiSourceAtlasManager _manager, bool _createdByMod, Shader _shader, Action<INGUIAtlas, bool> _onNewAtlasLoaded)
		{
			this.Manager = _manager;
			this.CreatedByMod = _createdByMod;
			this.Shader = _shader;
			this.OnNewAtlasLoaded = _onNewAtlasLoaded;
		}

		public readonly MultiSourceAtlasManager Manager;

		public readonly bool CreatedByMod;

		public readonly Shader Shader;

		public readonly Action<INGUIAtlas, bool> OnNewAtlasLoaded;
	}
}
