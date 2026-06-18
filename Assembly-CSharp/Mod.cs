using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Platform;
using UnityEngine;

[JsonObject(1)]
public class Mod
{
	public Mod.EModLoadState LoadState
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public string Path
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public string FolderName
	{
		[UsedImplicitly]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[JsonProperty]
	public string Name
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[JsonProperty]
	public string DisplayName
	{
		[UsedImplicitly]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[JsonProperty]
	public string Description
	{
		[UsedImplicitly]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[JsonProperty]
	public string Author
	{
		[UsedImplicitly]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Version Version
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[JsonProperty]
	public string VersionString
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[JsonProperty]
	public string Website
	{
		[UsedImplicitly]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[JsonProperty]
	public bool SkipLoadingWithAntiCheat
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[JsonProperty]
	public bool AntiCheatCompatible
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[JsonProperty]
	public bool GameConfigMod
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Mod()
	{
		this.AllAssemblies = new ReadOnlyCollection<Assembly>(this.allAssemblies);
	}

	public bool LoadMod()
	{
		this.LoadState = Mod.EModLoadState.Failed;
		if (ModManager.ModLoaded(this.Name))
		{
			Log.Warning("[MODS]     Mod with same name (" + this.Name + ") already loaded, ignoring");
			this.LoadState = Mod.EModLoadState.DuplicateModName;
			return false;
		}
		Mod.EModLoadState emodLoadState = this.LoadAssemblies();
		if (emodLoadState != Mod.EModLoadState.Success)
		{
			this.LoadState = emodLoadState;
			return false;
		}
		this.DetectContents();
		Log.Out(string.Concat(new string[]
		{
			"[MODS]     Loaded Mod: ",
			this.Name,
			" (",
			this.VersionString ?? "<unknown version>",
			")"
		}));
		this.LoadState = Mod.EModLoadState.Success;
		return this.LoadState == Mod.EModLoadState.Success;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Mod.EModLoadState LoadAssemblies()
	{
		string[] files = SdDirectory.GetFiles(this.Path);
		if (files.Length == 0)
		{
			return Mod.EModLoadState.Success;
		}
		foreach (string text in files)
		{
			if (text.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
			{
				if (!GameManager.IsDedicatedServer)
				{
					IAntiCheatClient antiCheatClient = PlatformManager.MultiPlatform.AntiCheatClient;
					if (antiCheatClient != null && antiCheatClient.ClientAntiCheatEnabled())
					{
						if (this.SkipLoadingWithAntiCheat)
						{
							Log.Out("[MODS]     AntiCheat enabled, mod skipped because it is set not to load");
							return Mod.EModLoadState.SkippedDueToAntiCheat;
						}
						if (!this.AntiCheatCompatible)
						{
							Log.Warning("[MODS]     Mod contains custom code, AntiCheat needs to be disabled to load it!");
							return Mod.EModLoadState.NotAntiCheatCompatible;
						}
					}
				}
				try
				{
					Assembly assembly = Mod.loadAssembly(text);
					this.allAssemblies.Add(assembly);
					Log.Out("[MODS]     Loaded assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(assembly, text));
				}
				catch (Exception ex)
				{
					Log.Error("[MODS]     Failed loading DLL " + text);
					Log.Exception(ex);
					return Mod.EModLoadState.FailedLoadingAssembly;
				}
			}
		}
		return Mod.EModLoadState.Success;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Assembly loadAssembly(string _path)
	{
		if (GameManager.IsDedicatedServer)
		{
			RuntimePlatform platform = Application.platform;
			if (platform == 44 || platform == 2 || platform == 7)
			{
				return Assembly.Load(SdFile.ReadAllBytes(_path));
			}
		}
		return Assembly.LoadFrom(_path);
	}

	public bool InitModCode()
	{
		if (this.allAssemblies.Count > 0)
		{
			Log.Out("[MODS]   Initializing mod " + this.Name);
			bool flag = false;
			Type typeFromHandle = typeof(IModApi);
			foreach (Assembly assembly in this.allAssemblies)
			{
				try
				{
					foreach (Type type in assembly.GetTypes())
					{
						if (typeFromHandle.IsAssignableFrom(type))
						{
							Log.Out("[MODS]     Found ModAPI in assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(assembly, null) + ", creating instance");
							IModApi modApi = (IModApi)Activator.CreateInstance(type);
							try
							{
								modApi.InitMod(this);
								Log.Out("[MODS]     Initialized code in mod '" + this.Name + "' in assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(assembly, null));
							}
							catch (Exception ex)
							{
								Log.Error("[MODS]     Failed initializing ModAPI instance on mod '" + this.Name + "' in assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(assembly, null));
								Log.Exception(ex);
							}
							flag = true;
						}
					}
				}
				catch (ReflectionTypeLoadException)
				{
					Log.Warning("[MODS]     Failed iterating types in assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(assembly, null));
				}
				catch (Exception ex2)
				{
					Log.Error("[MODS]     Failed creating ModAPI instance from assembly " + ReflectionHelpers.GetAssemblyNameWithLocation(assembly, null));
					Log.Exception(ex2);
					return false;
				}
			}
			if (!flag)
			{
				Log.Out("[MODS]     No ModAPI found in mod DLLs");
				return true;
			}
			return true;
		}
		return true;
	}

	public bool ContainsAssembly(Assembly _assembly)
	{
		return this.allAssemblies.Contains(_assembly);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void DetectContents()
	{
		string text = this.Path + "/Config";
		if (!SdDirectory.Exists(text))
		{
			return;
		}
		string[] fileSystemEntries = SdDirectory.GetFileSystemEntries(text);
		for (int i = 0; i < fileSystemEntries.Length; i++)
		{
			string fileName = global::System.IO.Path.GetFileName(fileSystemEntries[i]);
			if (!fileName.EqualsCaseInsensitive("XUi_Menu") && !fileName.EqualsCaseInsensitive("loadingscreen.xml") && !fileName.EqualsCaseInsensitive("Localization.csv"))
			{
				this.GameConfigMod = true;
				return;
			}
		}
	}

	public static Mod LoadDefinitionFromFolder(string _path)
	{
		string text = _path + "/ModInfo.xml";
		string fileName = global::System.IO.Path.GetFileName(_path);
		if (!SdFile.Exists(text))
		{
			Log.Warning("[MODS]     Folder " + fileName + " does not contain a ModInfo.xml, ignoring");
			return null;
		}
		XmlFile xmlFile = new XmlFile(_path, "ModInfo.xml", false, false);
		XElement root = xmlFile.XmlDoc.Root;
		if (root == null)
		{
			Log.Error("[MODS]     " + fileName + "/ModInfo.xml does not have a root element, ignoring");
			return null;
		}
		Mod mod = ((root.Element("ModInfo") != null) ? Mod.parseModInfoV1(_path, fileName, text, xmlFile) : Mod.parseModInfoV2(_path, fileName, root));
		if (mod == null)
		{
			Log.Error("[MODS]     Could not parse " + fileName + "/ModInfo.xml, ignoring");
			return null;
		}
		return mod;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mod parseModInfoV2(string _modPath, string _folderName, XElement _xmlRoot)
	{
		string elementAttributeValue = Mod.getElementAttributeValue(_folderName, _xmlRoot, "Name", true);
		if (elementAttributeValue == null)
		{
			return null;
		}
		if (elementAttributeValue.Length == 0)
		{
			Log.Error("[MODS]     " + _folderName + "/ModInfo.xml does not specify a non-empty Name, ignoring");
			return null;
		}
		if (!Mod.nameValidationRegex.IsMatch(elementAttributeValue))
		{
			Log.Error(string.Format("[MODS]     {0}/ModInfo.xml does not define a valid non-empty Name ({1}), ignoring", _folderName, Mod.nameValidationRegex));
			return null;
		}
		Version version = null;
		string text = Mod.getElementAttributeValue(_folderName, _xmlRoot, "Version", true);
		if (text != null)
		{
			if (text.Length == 0)
			{
				text = null;
			}
			else
			{
				Version.TryParse(text, out version);
			}
		}
		if (version == null)
		{
			Log.Warning("[MODS]     " + _folderName + "/ModInfo.xml does not define a valid Version. Please consider updating it for future compatibility.");
		}
		string elementAttributeValue2 = Mod.getElementAttributeValue(_folderName, _xmlRoot, "DisplayName", false);
		if (string.IsNullOrEmpty(elementAttributeValue2))
		{
			Log.Error("[MODS]     " + _folderName + "/ModInfo.xml does not define a non-empty DisplayName, ignoring");
			return null;
		}
		string elementAttributeValue3 = Mod.getElementAttributeValue(_folderName, _xmlRoot, "Description", false);
		string elementAttributeValue4 = Mod.getElementAttributeValue(_folderName, _xmlRoot, "Author", false);
		string elementAttributeValue5 = Mod.getElementAttributeValue(_folderName, _xmlRoot, "Website", false);
		string elementAttributeValue6 = Mod.getElementAttributeValue(_folderName, _xmlRoot, "SkipWithAntiCheat", false);
		bool flag = false;
		if (!string.IsNullOrEmpty(elementAttributeValue6) && !StringParsers.TryParseBool(elementAttributeValue6, out flag))
		{
			Log.Warning("[MODS]     " + _folderName + "/ModInfo.xml does have a SkipWithAntiCheat, but its value is not a valid boolean. Assuming 'false'");
			flag = false;
		}
		return new Mod
		{
			Path = _modPath,
			FolderName = _folderName,
			Name = elementAttributeValue,
			DisplayName = elementAttributeValue2,
			Description = elementAttributeValue3,
			Author = elementAttributeValue4,
			Version = version,
			VersionString = text,
			Website = elementAttributeValue5,
			SkipLoadingWithAntiCheat = flag
		};
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static string getElementAttributeValue(string _folderName, XElement _xmlParent, string _elementName, bool _logNonExisting = true)
	{
		List<XElement> list = _xmlParent.Elements(_elementName).ToList<XElement>();
		if (list.Count != 1)
		{
			if (_logNonExisting)
			{
				Log.Error(string.Concat(new string[] { "[MODS] ", _folderName, "/ModInfo.xml does not have exactly one '", _elementName, "' element, ignoring" }));
			}
			return null;
		}
		XAttribute xattribute = list[0].Attribute("value");
		if (xattribute == null)
		{
			Log.Error(string.Concat(new string[] { "[MODS] ", _folderName, "/ModInfo.xml '", _elementName, "' element does not have a 'value' attribute, ignoring" }));
			return null;
		}
		return xattribute.Value;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static Mod parseModInfoV1(string _modPath, string _folderName, string _modInfoFilename, XmlFile _xml)
	{
		Log.Error("[MODS]     " + _folderName + "/ModInfo.xml in legacy format. V2 required to load mod");
		return null;
	}

	public override string ToString()
	{
		return this.DisplayName;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<Assembly> allAssemblies = new List<Assembly>();

	public readonly ReadOnlyCollection<Assembly> AllAssemblies;

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Regex nameValidationRegex = new Regex("^[0-9a-zA-Z_\\-]+$", RegexOptions.Compiled);

	public enum EModLoadState
	{
		LoadNotRequested,
		Success,
		NotAntiCheatCompatible,
		SkippedDueToAntiCheat,
		DuplicateModName,
		FailedLoadingAssembly,
		Failed
	}
}
