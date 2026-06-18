using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Platform;

public static class PathAbstractions
{
	public static void InvalidateCaches()
	{
		foreach (PathAbstractions.SearchDefinition searchDefinition in PathAbstractions.allSearchDefs)
		{
			searchDefinition.InvalidateCache();
		}
	}

	public static bool CacheEnabled
	{
		get
		{
			return PathAbstractions.cacheEnabled;
		}
		set
		{
			PathAbstractions.cacheEnabled = value;
			if (!value)
			{
				PathAbstractions.InvalidateCaches();
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<PathAbstractions.SearchDefinition> allSearchDefs = new List<PathAbstractions.SearchDefinition>();

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Func<string> deviceLocalUserDataPath = new Func<string>(GameIO.GetDeviceLocalUserGameDataDir);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Func<string> gameDataPath = new Func<string>(GameIO.GetApplicationPath);

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly Func<UserDataStorageType> getWorldsStorageType = () => (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.UserWorldStorageType);

	public static readonly PathAbstractions.SearchDefinition WorldsSearchPaths = new PathAbstractions.SearchDefinition(true, null, false, false, new PathAbstractions.SearchPath[]
	{
		new PathAbstractions.SearchPathSaves("World"),
		new PathAbstractions.SearchPathUserData("GeneratedWorlds", PathAbstractions.getWorldsStorageType),
		new PathAbstractions.SearchPathMods("Worlds"),
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.GameData, PathAbstractions.gameDataPath, "Data/Worlds", null)
	});

	public static readonly PathAbstractions.SearchDefinition PrefabsSearchPaths = new PathAbstractions.SearchDefinition(false, ".tts", true, true, new PathAbstractions.SearchPath[]
	{
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.UserDataPath, PathAbstractions.deviceLocalUserDataPath, "LocalPrefabs", null),
		new PathAbstractions.SearchPathMods("Prefabs"),
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.GameData, PathAbstractions.gameDataPath, "Data/Prefabs", null)
	});

	public static readonly PathAbstractions.SearchDefinition PrefabImpostersSearchPaths = new PathAbstractions.SearchDefinition(false, ".mesh", false, true, new PathAbstractions.SearchPath[]
	{
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.UserDataPath, PathAbstractions.deviceLocalUserDataPath, "LocalPrefabs", null),
		new PathAbstractions.SearchPathMods("Prefabs"),
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.GameData, PathAbstractions.gameDataPath, "Data/Prefabs", null)
	});

	public static readonly PathAbstractions.SearchDefinition RwgStampsSearchPaths = new PathAbstractions.SearchDefinition(false, "", true, true, new PathAbstractions.SearchPath[]
	{
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.UserDataPath, PathAbstractions.deviceLocalUserDataPath, "LocalStamps", null),
		new PathAbstractions.SearchPathMods("Stamps"),
		new PathAbstractions.SearchPathBasic(PathAbstractions.EAbstractedLocationType.GameData, PathAbstractions.gameDataPath, "Data/Stamps", null)
	});

	[PublicizedFrom(EAccessModifier.Private)]
	public static bool cacheEnabled;

	public enum EAbstractedLocationType
	{
		HostSave,
		LocalSave,
		UserDataPath,
		Mods,
		GameData,
		None
	}

	public readonly struct AbstractedLocation : IEquatable<PathAbstractions.AbstractedLocation>, IComparable<PathAbstractions.AbstractedLocation>, IComparable
	{
		public string FullPath
		{
			get
			{
				return this.Folder + "/" + this.FileNameNoExtension + this.Extension;
			}
		}

		public string FullPathNoExtension
		{
			get
			{
				return this.Folder + "/" + this.FileNameNoExtension;
			}
		}

		public AbstractedLocation(PathAbstractions.EAbstractedLocationType _type, string _name, string _fullPath, string _relativePath, bool _isFolder, Mod _containingMod = null)
		{
			_fullPath = ((_fullPath != null) ? Path.GetFullPath(_fullPath).Replace("\\", "/") : null);
			this.Type = _type;
			this.Name = Path.GetFileName(_name);
			string directoryName = Path.GetDirectoryName(_fullPath);
			this.Folder = ((directoryName != null) ? directoryName.Replace("\\", "/") : null);
			this.RelativePath = _relativePath;
			this.FileNameNoExtension = Path.GetFileNameWithoutExtension(_fullPath);
			this.Extension = Path.GetExtension(_fullPath);
			this.Extension = (string.IsNullOrEmpty(this.Extension) ? null : this.Extension);
			this.IsFolder = _isFolder;
			this.ContainingMod = _containingMod;
			this.StorageType = ((this.Folder != null && GameIO.IsRoamingUserDataPath(this.Folder)) ? UserDataStorageType.Roaming : UserDataStorageType.DeviceLocal);
		}

		public AbstractedLocation(PathAbstractions.EAbstractedLocationType _type, string _name, string _folder, string _relativePath, string _fileNameNoExtension, string _extension, bool _isFolder, Mod _containingMod = null)
		{
			this.Type = _type;
			this.Name = _name;
			this.Folder = ((_folder != null) ? Path.GetFullPath(_folder).Replace("\\", "/") : null);
			this.RelativePath = _relativePath;
			this.FileNameNoExtension = _fileNameNoExtension;
			this.Extension = _extension;
			this.IsFolder = _isFolder;
			this.ContainingMod = _containingMod;
			this.StorageType = ((this.Folder != null && GameIO.IsRoamingUserDataPath(this.Folder)) ? UserDataStorageType.Roaming : UserDataStorageType.DeviceLocal);
		}

		public bool Exists()
		{
			if (this.Type == PathAbstractions.EAbstractedLocationType.None)
			{
				return false;
			}
			if (!this.IsFolder)
			{
				return SdFile.Exists(this.FullPath);
			}
			return SdDirectory.Exists(this.FullPath);
		}

		public bool Equals(PathAbstractions.AbstractedLocation _other)
		{
			return this.Type == _other.Type && (this.Type == PathAbstractions.EAbstractedLocationType.None || (this.IsFolder == _other.IsFolder && string.Equals(this.Name, _other.Name) && GameIO.PathsEquals(this.FullPath, _other.FullPath, true)));
		}

		public override bool Equals(object _obj)
		{
			if (_obj == null)
			{
				return false;
			}
			if (_obj is PathAbstractions.AbstractedLocation)
			{
				PathAbstractions.AbstractedLocation abstractedLocation = (PathAbstractions.AbstractedLocation)_obj;
				return this.Equals(abstractedLocation);
			}
			return false;
		}

		public static bool operator ==(PathAbstractions.AbstractedLocation _a, PathAbstractions.AbstractedLocation _b)
		{
			return _a.Equals(_b);
		}

		public static bool operator !=(PathAbstractions.AbstractedLocation _a, PathAbstractions.AbstractedLocation _b)
		{
			return !(_a == _b);
		}

		public override int GetHashCode()
		{
			return (((((this.Name != null) ? this.Name.GetHashCode() : 0) * 397) ^ (int)this.Type) * 397) ^ ((this.FullPath != null) ? this.FullPath.GetHashCode() : 0);
		}

		public override string ToString()
		{
			return string.Concat(new string[]
			{
				this.Name,
				" (src: ",
				this.Type.ToStringCached<PathAbstractions.EAbstractedLocationType>(),
				", ",
				this.StorageType.ToStringCached<UserDataStorageType>(),
				")"
			});
		}

		public string GetLocationTypeDisplayString()
		{
			switch (this.Type)
			{
			case PathAbstractions.EAbstractedLocationType.UserDataPath:
				if (PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional)
				{
					return "User Data: " + this.StorageType.LocalizedName();
				}
				return "User Data";
			case PathAbstractions.EAbstractedLocationType.Mods:
				return "Mod: " + this.ContainingMod.Name;
			case PathAbstractions.EAbstractedLocationType.GameData:
				return "Vanilla";
			default:
				return this.Type.ToStringCached<PathAbstractions.EAbstractedLocationType>();
			}
		}

		public int CompareTo(PathAbstractions.AbstractedLocation _other)
		{
			int num = string.Compare(this.Name, _other.Name, StringComparison.OrdinalIgnoreCase);
			if (num != 0)
			{
				return num;
			}
			int num2 = this.Type.CompareTo(_other.Type);
			if (num2 != 0)
			{
				return num2;
			}
			int num3 = string.Compare(this.FileNameNoExtension, _other.FileNameNoExtension, StringComparison.OrdinalIgnoreCase);
			if (num3 != 0)
			{
				return num3;
			}
			int num4 = string.Compare(this.Extension, _other.Extension, StringComparison.OrdinalIgnoreCase);
			if (num4 != 0)
			{
				return num4;
			}
			return string.Compare(this.Folder, _other.Folder, StringComparison.OrdinalIgnoreCase);
		}

		public int CompareTo(object _obj)
		{
			if (_obj == null)
			{
				return 1;
			}
			if (_obj is PathAbstractions.AbstractedLocation)
			{
				PathAbstractions.AbstractedLocation abstractedLocation = (PathAbstractions.AbstractedLocation)_obj;
				return this.CompareTo(abstractedLocation);
			}
			throw new ArgumentException("Object must be of type AbstractedLocation");
		}

		public static readonly PathAbstractions.AbstractedLocation None = new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.None, null, null, null, false, null);

		public readonly PathAbstractions.EAbstractedLocationType Type;

		public readonly string Name;

		public readonly string Folder;

		public readonly string RelativePath;

		public readonly string FileNameNoExtension;

		public readonly string Extension;

		public readonly bool IsFolder;

		public readonly Mod ContainingMod;

		public readonly UserDataStorageType StorageType;
	}

	public class SearchDefinition
	{
		public SearchDefinition(bool _isFolder, string _extension, bool _removeExtension, bool _recursive, params PathAbstractions.SearchPath[] _paths)
		{
			this.IsFolder = _isFolder;
			this.Extension = _extension;
			this.RemoveExtension = _removeExtension;
			this.Recursive = _recursive;
			if (this.IsFolder && this.Recursive)
			{
				throw new Exception("SearchDefinition can not be set to target folders and search recursively at the same time!");
			}
			this.paths = new List<PathAbstractions.SearchPath>(_paths);
			for (int i = 0; i < _paths.Length; i++)
			{
				_paths[i].SetOwner(this);
			}
			PathAbstractions.allSearchDefs.Add(this);
		}

		public PathAbstractions.AbstractedLocation GetLocation(string _name, PathAbstractions.EAbstractedLocationType? _onlyLocationType = null, UserDataStorageType? _userDataHint = null)
		{
			foreach (PathAbstractions.SearchPath searchPath in this.paths)
			{
				if (searchPath.CanMatch && (_onlyLocationType == null || searchPath.CanHandleLocationType(_onlyLocationType.Value)))
				{
					PathAbstractions.AbstractedLocation location = searchPath.GetLocation(_name, _userDataHint);
					if (location.Type != PathAbstractions.EAbstractedLocationType.None)
					{
						return location;
					}
				}
			}
			return new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.None, _name, null, null, this.IsFolder, null);
		}

		public List<PathAbstractions.AbstractedLocation> GetAvailablePathsList(Regex _nameMatch = null, bool _ignoreDuplicateNames = false, PathAbstractions.EAbstractedLocationType? _onlyLocationType = null)
		{
			List<PathAbstractions.AbstractedLocation> list = new List<PathAbstractions.AbstractedLocation>();
			this.GetAvailablePathsList(ref list, _nameMatch, _ignoreDuplicateNames, _onlyLocationType);
			return list;
		}

		public void GetAvailablePathsList(ref List<PathAbstractions.AbstractedLocation> _resultList, Regex _nameMatch = null, bool _ignoreDuplicateNames = false, PathAbstractions.EAbstractedLocationType? _onlyLocationType = null)
		{
			if (_resultList == null)
			{
				_resultList = new List<PathAbstractions.AbstractedLocation>();
			}
			foreach (PathAbstractions.SearchPath searchPath in this.paths)
			{
				if (searchPath.CanMatch && (_onlyLocationType == null || searchPath.CanHandleLocationType(_onlyLocationType.Value)))
				{
					searchPath.GetAvailablePathsList(_resultList, _nameMatch, _ignoreDuplicateNames);
				}
			}
		}

		public PathAbstractions.AbstractedLocation? BuildLocation(PathAbstractions.EAbstractedLocationType _locationType, string _subFolder, string _elementName, string _modName = null, UserDataStorageType? _userStorageHint = null)
		{
			Mod mod = null;
			if (_locationType == PathAbstractions.EAbstractedLocationType.Mods)
			{
				if (string.IsNullOrEmpty(_modName))
				{
					throw new ArgumentException("BuildLocation for LocationType==Mods requires a Mod name", "_modName");
				}
				mod = ModManager.GetMod(_modName, true);
				if (mod == null)
				{
					throw new ArgumentException("BuildLocation for LocationType==Mods requires the name of a loaded Mod", "_modName");
				}
			}
			return this.BuildLocation(_locationType, _subFolder, _elementName, mod, _userStorageHint);
		}

		public PathAbstractions.AbstractedLocation? BuildLocation(PathAbstractions.EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod = null, UserDataStorageType? _userStorageHint = null)
		{
			foreach (PathAbstractions.SearchPath searchPath in this.paths)
			{
				if (searchPath.CanHandleLocationType(_locationType))
				{
					return searchPath.TryBuildLocation(_locationType, _subFolder, _elementName, _mod, _userStorageHint);
				}
			}
			return null;
		}

		public string GetBasePath(PathAbstractions.EAbstractedLocationType _locationType, string _modName = null)
		{
			Mod mod = null;
			if (_locationType == PathAbstractions.EAbstractedLocationType.Mods)
			{
				if (string.IsNullOrEmpty(_modName))
				{
					throw new ArgumentException("GetBasePath for LocationType==Mods requires a Mod name", "_modName");
				}
				mod = ModManager.GetMod(_modName, true);
				if (mod == null)
				{
					throw new ArgumentException("GetBasePath for LocationType==Mods requires the name of a loaded Mod", "_modName");
				}
			}
			return this.GetBasePath(_locationType, mod);
		}

		public string GetBasePath(PathAbstractions.EAbstractedLocationType _locationType, Mod _mod = null)
		{
			foreach (PathAbstractions.SearchPath searchPath in this.paths)
			{
				if (searchPath.CanHandleLocationType(_locationType))
				{
					return searchPath.TryGetBasePath(_locationType, _mod);
				}
			}
			return null;
		}

		public void InvalidateCache()
		{
			foreach (PathAbstractions.SearchPath searchPath in this.paths)
			{
				searchPath.InvalidateCache();
			}
		}

		public readonly bool IsFolder;

		public readonly string Extension;

		public readonly bool RemoveExtension;

		public readonly bool Recursive;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly List<PathAbstractions.SearchPath> paths;
	}

	public abstract class SearchPath
	{
		public virtual bool CanMatch
		{
			get
			{
				return true;
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public SearchPath(string _relativePath)
		{
			this.RelativePath = _relativePath;
		}

		public void SetOwner(PathAbstractions.SearchDefinition _owner)
		{
			this.Owner = _owner;
		}

		public abstract PathAbstractions.AbstractedLocation GetLocation(string _name, UserDataStorageType? _userDataHint);

		public abstract void GetAvailablePathsList(List<PathAbstractions.AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames);

		[PublicizedFrom(EAccessModifier.Protected)]
		public PathAbstractions.AbstractedLocation getLocationSingleBase(PathAbstractions.EAbstractedLocationType _locationType, string _basePath, string _name, Mod _containingMod, string _subfolder = null)
		{
			this.UseCache();
			if (!SdDirectory.Exists(_basePath))
			{
				return PathAbstractions.AbstractedLocation.None;
			}
			string text = _basePath + "/" + _name + this.Owner.Extension;
			if (this.Owner.IsFolder)
			{
				if (SdDirectory.Exists(text))
				{
					return new PathAbstractions.AbstractedLocation(_locationType, _name, text, _subfolder, this.Owner.IsFolder, _containingMod);
				}
			}
			else
			{
				if (SdFile.Exists(text))
				{
					string text2 = (this.Owner.RemoveExtension ? GameIO.RemoveExtension(_name, this.Owner.Extension) : _name);
					return new PathAbstractions.AbstractedLocation(_locationType, text2, text, _subfolder, this.Owner.IsFolder, _containingMod);
				}
				if (this.Owner.Recursive)
				{
					foreach (string text3 in SdDirectory.GetDirectories(_basePath))
					{
						PathAbstractions.AbstractedLocation locationSingleBase = this.getLocationSingleBase(_locationType, text3, _name, _containingMod, Path.GetFileName(text3));
						if (!locationSingleBase.Equals(PathAbstractions.AbstractedLocation.None))
						{
							return locationSingleBase;
						}
					}
				}
			}
			return PathAbstractions.AbstractedLocation.None;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void getAvailablePathsSingleBase(List<PathAbstractions.AbstractedLocation> _targetList, PathAbstractions.EAbstractedLocationType _locationType, string _basePath, Regex _nameMatch, bool _ignoreDuplicateNames, Mod _containingMod, string _subfolder = null)
		{
			if (!SdDirectory.Exists(_basePath))
			{
				return;
			}
			SdDirectoryInfo sdDirectoryInfo = new SdDirectoryInfo(_basePath);
			SdFileSystemInfo[] array;
			SdFileSystemInfo[] array2;
			if (this.Owner.IsFolder)
			{
				array = sdDirectoryInfo.GetDirectories();
				array2 = array;
			}
			else
			{
				array = sdDirectoryInfo.GetFiles("*" + this.Owner.Extension, SearchOption.TopDirectoryOnly);
				array2 = array;
			}
			array = array2;
			for (int i = 0; i < array.Length; i++)
			{
				SdFileSystemInfo sdFileSystemInfo = array[i];
				if (this.Owner.Extension == null || sdFileSystemInfo.Name.EndsWith(this.Owner.Extension, StringComparison.Ordinal))
				{
					string filename = (this.Owner.RemoveExtension ? GameIO.RemoveExtension(sdFileSystemInfo.Name, this.Owner.Extension) : sdFileSystemInfo.Name);
					if ((_nameMatch == null || _nameMatch.IsMatch(filename)) && (!_ignoreDuplicateNames || !_targetList.Exists((PathAbstractions.AbstractedLocation _location) => _location.Name.Equals(filename))))
					{
						_targetList.Add(new PathAbstractions.AbstractedLocation(_locationType, filename, sdFileSystemInfo.FullName, _subfolder, this.Owner.IsFolder, _containingMod));
					}
				}
			}
			if (!this.Owner.IsFolder && this.Owner.Recursive)
			{
				foreach (string text in SdDirectory.GetDirectories(_basePath))
				{
					this.getAvailablePathsSingleBase(_targetList, _locationType, text, _nameMatch, _ignoreDuplicateNames, _containingMod, Path.GetFileName(text));
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool UseCache()
		{
			bool cacheEnabled = PathAbstractions.CacheEnabled;
			if (cacheEnabled && !this.locationsCachePopulated)
			{
				this.PopulateCache();
			}
			return cacheEnabled;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public PathAbstractions.AbstractedLocation GetCachedLocation(string _name, bool _ignoreName = false)
		{
			if (this.locationsCache.Count == 0)
			{
				return PathAbstractions.AbstractedLocation.None;
			}
			if (_ignoreName)
			{
				foreach (KeyValuePair<string, IList<PathAbstractions.AbstractedLocation>> keyValuePair in this.locationsCache)
				{
					if (keyValuePair.Value.Count != 0)
					{
						return keyValuePair.Value[0];
					}
				}
				return PathAbstractions.AbstractedLocation.None;
			}
			IList<PathAbstractions.AbstractedLocation> list;
			if (!this.locationsCache.TryGetValue(this.Owner.RemoveExtension ? _name : (_name + this.Owner.Extension), out list))
			{
				return PathAbstractions.AbstractedLocation.None;
			}
			if (list.Count <= 0)
			{
				return PathAbstractions.AbstractedLocation.None;
			}
			return list[0];
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void GetCachedPathList(List<PathAbstractions.AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			if (this.locationsCache.Count == 0)
			{
				return;
			}
			foreach (KeyValuePair<string, IList<PathAbstractions.AbstractedLocation>> keyValuePair in this.locationsCache)
			{
				using (IEnumerator<PathAbstractions.AbstractedLocation> enumerator2 = keyValuePair.Value.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						PathAbstractions.AbstractedLocation loc = enumerator2.Current;
						if ((_nameMatch == null || _nameMatch.IsMatch(loc.Name)) && (!_ignoreDuplicateNames || !_targetList.Exists((PathAbstractions.AbstractedLocation location) => location.Name.Equals(loc.Name))))
						{
							_targetList.Add(loc);
						}
					}
				}
			}
		}

		public void InvalidateCache()
		{
			this.locationsCache.Clear();
			this.locationsCachePopulated = false;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public abstract void PopulateCache();

		public abstract PathAbstractions.AbstractedLocation? TryBuildLocation(PathAbstractions.EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, UserDataStorageType? _userStorageHint);

		public abstract string TryGetBasePath(PathAbstractions.EAbstractedLocationType _locationType, Mod _mod);

		public abstract bool CanHandleLocationType(PathAbstractions.EAbstractedLocationType _locationType);

		[PublicizedFrom(EAccessModifier.Protected)]
		public PathAbstractions.SearchDefinition Owner;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly string RelativePath;

		[PublicizedFrom(EAccessModifier.Protected)]
		public readonly Dictionary<string, IList<PathAbstractions.AbstractedLocation>> locationsCache = new CaseInsensitiveStringDictionary<IList<PathAbstractions.AbstractedLocation>>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool locationsCachePopulated;
	}

	public class SearchPathBasic : PathAbstractions.SearchPath
	{
		public SearchPathBasic(PathAbstractions.EAbstractedLocationType _locationType, Func<string> _basePath, string _relativePath, Func<bool> _canMatch = null)
			: base(_relativePath)
		{
			this.locationType = _locationType;
			this.basePath = _basePath;
			this.canMatch = _canMatch;
		}

		public override bool CanMatch
		{
			get
			{
				Func<bool> func = this.canMatch;
				if (func == null)
				{
					return base.CanMatch;
				}
				return func();
			}
		}

		public override PathAbstractions.AbstractedLocation GetLocation(string _name, UserDataStorageType? _userDataHint)
		{
			if (base.UseCache())
			{
				return base.GetCachedLocation(_name, false);
			}
			PathAbstractions.AbstractedLocation locationSingleBase = base.getLocationSingleBase(this.locationType, this.basePath() + "/" + this.RelativePath, _name, null, null);
			if (!locationSingleBase.Equals(PathAbstractions.AbstractedLocation.None))
			{
				return locationSingleBase;
			}
			return PathAbstractions.AbstractedLocation.None;
		}

		public override void GetAvailablePathsList(List<PathAbstractions.AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			if (base.UseCache())
			{
				base.GetCachedPathList(_targetList, _nameMatch, _ignoreDuplicateNames);
				return;
			}
			base.getAvailablePathsSingleBase(_targetList, this.locationType, this.basePath() + "/" + this.RelativePath, _nameMatch, _ignoreDuplicateNames, null, null);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			List<PathAbstractions.AbstractedLocation> list = new List<PathAbstractions.AbstractedLocation>();
			base.getAvailablePathsSingleBase(list, this.locationType, this.basePath() + "/" + this.RelativePath, null, false, null, null);
			this.locationsCache.Clear();
			for (int i = 0; i < list.Count; i++)
			{
				PathAbstractions.AbstractedLocation abstractedLocation = list[i];
				IList<PathAbstractions.AbstractedLocation> list2;
				if (!this.locationsCache.TryGetValue(abstractedLocation.Name, out list2))
				{
					list2 = new List<PathAbstractions.AbstractedLocation>();
					this.locationsCache[abstractedLocation.Name] = list2;
				}
				list2.Add(abstractedLocation);
			}
			this.locationsCachePopulated = true;
		}

		public override PathAbstractions.AbstractedLocation? TryBuildLocation(PathAbstractions.EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, UserDataStorageType? _userStorageHint = null)
		{
			return new PathAbstractions.AbstractedLocation?(new PathAbstractions.AbstractedLocation(_locationType, _elementName, this.basePath() + "/" + this.RelativePath + ((!string.IsNullOrEmpty(_subFolder)) ? ("/" + _subFolder) : ""), _subFolder, _elementName, this.Owner.Extension, this.Owner.IsFolder, null));
		}

		public override string TryGetBasePath(PathAbstractions.EAbstractedLocationType _locationType, Mod _mod)
		{
			return this.basePath() + "/" + this.RelativePath;
		}

		public override bool CanHandleLocationType(PathAbstractions.EAbstractedLocationType _locationType)
		{
			return _locationType == this.locationType;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly PathAbstractions.EAbstractedLocationType locationType;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Func<string> basePath;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Func<bool> canMatch;
	}

	public class SearchPathUserData : PathAbstractions.SearchPath
	{
		public SearchPathUserData(string _relativePath, Func<UserDataStorageType> _getStorageTarget)
			: base(_relativePath)
		{
			this.getStorageTarget = _getStorageTarget;
		}

		public override PathAbstractions.AbstractedLocation GetLocation(string _name, UserDataStorageType? _userDataHint)
		{
			UserDataStorageType userDataStorageType = ((_userDataHint != null) ? _userDataHint.Value : this.getStorageTarget());
			if (base.UseCache())
			{
				return base.GetCachedLocation(PathAbstractions.SearchPathUserData.GetCacheLookupName(_name, userDataStorageType), false);
			}
			string text = Path.Combine(GameIO.GetUserGameDataDir(userDataStorageType), this.RelativePath);
			return base.getLocationSingleBase(PathAbstractions.EAbstractedLocationType.UserDataPath, text, _name, null, null);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CollectLocations(List<PathAbstractions.AbstractedLocation> _list, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			string text = Path.Combine(GameIO.GetUserGameDataDir(UserDataStorageType.DeviceLocal), this.RelativePath);
			base.getAvailablePathsSingleBase(_list, PathAbstractions.EAbstractedLocationType.UserDataPath, text, _nameMatch, _ignoreDuplicateNames, null, null);
			if (PlatformManager.MultiPlatform.UserDataRoaming.IsSupported)
			{
				string text2 = Path.Combine(GameIO.GetUserGameDataDir(UserDataStorageType.Roaming), this.RelativePath);
				base.getAvailablePathsSingleBase(_list, PathAbstractions.EAbstractedLocationType.UserDataPath, text2, _nameMatch, _ignoreDuplicateNames, null, null);
			}
		}

		public override void GetAvailablePathsList(List<PathAbstractions.AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			if (base.UseCache())
			{
				base.GetCachedPathList(_targetList, _nameMatch, _ignoreDuplicateNames);
				return;
			}
			this.CollectLocations(_targetList, _nameMatch, _ignoreDuplicateNames);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static string GetCacheLookupName(string _name, UserDataStorageType _storageType)
		{
			return string.Format("{0}_{1}", _name, _storageType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			List<PathAbstractions.AbstractedLocation> list = new List<PathAbstractions.AbstractedLocation>();
			this.CollectLocations(list, null, false);
			this.locationsCache.Clear();
			foreach (PathAbstractions.AbstractedLocation abstractedLocation in list)
			{
				string cacheLookupName = PathAbstractions.SearchPathUserData.GetCacheLookupName(abstractedLocation.Name, abstractedLocation.StorageType);
				IList<PathAbstractions.AbstractedLocation> list2;
				if (!this.locationsCache.TryGetValue(cacheLookupName, out list2))
				{
					list2 = new List<PathAbstractions.AbstractedLocation>();
					this.locationsCache.Add(cacheLookupName, list2);
				}
				list2.Add(abstractedLocation);
			}
			this.locationsCachePopulated = true;
		}

		public override PathAbstractions.AbstractedLocation? TryBuildLocation(PathAbstractions.EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, UserDataStorageType? _userStorageHint = null)
		{
			string userGameDataDir = GameIO.GetUserGameDataDir((_userStorageHint != null) ? _userStorageHint.Value : this.getStorageTarget());
			return new PathAbstractions.AbstractedLocation?(new PathAbstractions.AbstractedLocation(_locationType, _elementName, userGameDataDir + "/" + this.RelativePath + ((!string.IsNullOrEmpty(_subFolder)) ? ("/" + _subFolder) : ""), _subFolder, _elementName, this.Owner.Extension, this.Owner.IsFolder, null));
		}

		public override string TryGetBasePath(PathAbstractions.EAbstractedLocationType _locationType, Mod _mod)
		{
			return GameIO.GetUserGameDataDir(this.getStorageTarget()) + "/" + this.RelativePath;
		}

		public override bool CanHandleLocationType(PathAbstractions.EAbstractedLocationType _locationType)
		{
			return _locationType == PathAbstractions.EAbstractedLocationType.UserDataPath;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly Func<UserDataStorageType> getStorageTarget;
	}

	public class SearchPathSaves : PathAbstractions.SearchPath
	{
		public override bool CanMatch
		{
			get
			{
				return SingletonMonoBehaviour<ConnectionManager>.Instance != null;
			}
		}

		public SearchPathSaves(string _relativePath)
			: base(_relativePath)
		{
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ValueTuple<string, PathAbstractions.EAbstractedLocationType> GetSaveFolder()
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return new ValueTuple<string, PathAbstractions.EAbstractedLocationType>(GameIO.GetSaveGameDir(), PathAbstractions.EAbstractedLocationType.HostSave);
			}
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
			{
				return new ValueTuple<string, PathAbstractions.EAbstractedLocationType>(GameIO.GetSaveGameLocalDir(), PathAbstractions.EAbstractedLocationType.LocalSave);
			}
			return new ValueTuple<string, PathAbstractions.EAbstractedLocationType>(null, PathAbstractions.EAbstractedLocationType.None);
		}

		public override PathAbstractions.AbstractedLocation GetLocation(string _name, UserDataStorageType? _userDataHint)
		{
			if (base.UseCache())
			{
				return base.GetCachedLocation(_name, true);
			}
			ValueTuple<string, PathAbstractions.EAbstractedLocationType> saveFolder = this.GetSaveFolder();
			string item = saveFolder.Item1;
			PathAbstractions.EAbstractedLocationType item2 = saveFolder.Item2;
			if (item == null)
			{
				return PathAbstractions.AbstractedLocation.None;
			}
			string text = item + "/" + this.RelativePath;
			if ((this.Owner.IsFolder && SdDirectory.Exists(text)) || (!this.Owner.IsFolder && SdFile.Exists(text)))
			{
				return new PathAbstractions.AbstractedLocation(item2, this.RelativePath, text, null, this.Owner.IsFolder, null);
			}
			return PathAbstractions.AbstractedLocation.None;
		}

		public override void GetAvailablePathsList(List<PathAbstractions.AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			if (base.UseCache())
			{
				base.GetCachedPathList(_targetList, null, _ignoreDuplicateNames);
				return;
			}
			ValueTuple<string, PathAbstractions.EAbstractedLocationType> saveFolder = this.GetSaveFolder();
			string item = saveFolder.Item1;
			PathAbstractions.EAbstractedLocationType item2 = saveFolder.Item2;
			if (item == null)
			{
				return;
			}
			string text = item + "/" + this.RelativePath;
			if ((this.Owner.IsFolder && SdDirectory.Exists(text)) || (!this.Owner.IsFolder && SdFile.Exists(text)))
			{
				_targetList.Add(new PathAbstractions.AbstractedLocation(item2, this.RelativePath, text, null, this.Owner.IsFolder, null));
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			ValueTuple<string, PathAbstractions.EAbstractedLocationType> saveFolder = this.GetSaveFolder();
			string item = saveFolder.Item1;
			PathAbstractions.EAbstractedLocationType item2 = saveFolder.Item2;
			this.locationsCache.Clear();
			this.locationsCachePopulated = true;
			if (item == null)
			{
				return;
			}
			string text = item + "/" + this.RelativePath;
			if ((this.Owner.IsFolder && SdDirectory.Exists(text)) || (!this.Owner.IsFolder && SdFile.Exists(text)))
			{
				PathAbstractions.AbstractedLocation abstractedLocation = new PathAbstractions.AbstractedLocation(item2, this.RelativePath, text, null, this.Owner.IsFolder, null);
				List<PathAbstractions.AbstractedLocation> list = new List<PathAbstractions.AbstractedLocation>();
				this.locationsCache[abstractedLocation.Name] = list;
				list.Add(abstractedLocation);
			}
		}

		public override PathAbstractions.AbstractedLocation? TryBuildLocation(PathAbstractions.EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, UserDataStorageType? _userStorageHint = null)
		{
			ValueTuple<string, PathAbstractions.EAbstractedLocationType> saveFolder = this.GetSaveFolder();
			string item = saveFolder.Item1;
			PathAbstractions.EAbstractedLocationType item2 = saveFolder.Item2;
			if (item == null)
			{
				return null;
			}
			return new PathAbstractions.AbstractedLocation?(new PathAbstractions.AbstractedLocation(item2, _elementName, item + "/" + this.RelativePath + ((!string.IsNullOrEmpty(_subFolder)) ? ("/" + _subFolder) : ""), _subFolder, _elementName, this.Owner.Extension, this.Owner.IsFolder, null));
		}

		public override string TryGetBasePath(PathAbstractions.EAbstractedLocationType _locationType, Mod _mod)
		{
			string item = this.GetSaveFolder().Item1;
			if (item == null)
			{
				return null;
			}
			return item + "/" + this.RelativePath;
		}

		public override bool CanHandleLocationType(PathAbstractions.EAbstractedLocationType _locationType)
		{
			return _locationType == PathAbstractions.EAbstractedLocationType.HostSave || _locationType == PathAbstractions.EAbstractedLocationType.LocalSave;
		}
	}

	public class SearchPathMods : PathAbstractions.SearchPath
	{
		public override bool CanMatch
		{
			get
			{
				return SingletonMonoBehaviour<ConnectionManager>.Instance != null;
			}
		}

		public SearchPathMods(string _relativePath)
			: base(_relativePath)
		{
		}

		public override PathAbstractions.AbstractedLocation GetLocation(string _name, UserDataStorageType? _userDataHint)
		{
			if (base.UseCache())
			{
				return base.GetCachedLocation(_name, false);
			}
			foreach (Mod mod in ModManager.GetLoadedMods())
			{
				PathAbstractions.AbstractedLocation locationSingleBase = base.getLocationSingleBase(PathAbstractions.EAbstractedLocationType.Mods, mod.Path + "/" + this.RelativePath, _name, mod, null);
				if (!locationSingleBase.Equals(PathAbstractions.AbstractedLocation.None))
				{
					return locationSingleBase;
				}
			}
			return PathAbstractions.AbstractedLocation.None;
		}

		public override void GetAvailablePathsList(List<PathAbstractions.AbstractedLocation> _targetList, Regex _nameMatch, bool _ignoreDuplicateNames)
		{
			if (base.UseCache())
			{
				base.GetCachedPathList(_targetList, _nameMatch, _ignoreDuplicateNames);
				return;
			}
			foreach (Mod mod in ModManager.GetLoadedMods())
			{
				base.getAvailablePathsSingleBase(_targetList, PathAbstractions.EAbstractedLocationType.Mods, mod.Path + "/" + this.RelativePath, _nameMatch, _ignoreDuplicateNames, mod, null);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void PopulateCache()
		{
			List<PathAbstractions.AbstractedLocation> list = new List<PathAbstractions.AbstractedLocation>();
			foreach (Mod mod in ModManager.GetLoadedMods())
			{
				base.getAvailablePathsSingleBase(list, PathAbstractions.EAbstractedLocationType.Mods, mod.Path + "/" + this.RelativePath, null, false, mod, null);
			}
			this.locationsCache.Clear();
			foreach (PathAbstractions.AbstractedLocation abstractedLocation in list)
			{
				IList<PathAbstractions.AbstractedLocation> list2;
				if (!this.locationsCache.TryGetValue(abstractedLocation.Name, out list2))
				{
					list2 = new List<PathAbstractions.AbstractedLocation>();
					this.locationsCache[abstractedLocation.Name] = list2;
				}
				list2.Add(abstractedLocation);
			}
			this.locationsCachePopulated = true;
		}

		public override PathAbstractions.AbstractedLocation? TryBuildLocation(PathAbstractions.EAbstractedLocationType _locationType, string _subFolder, string _elementName, Mod _mod, UserDataStorageType? _userStorageHint = null)
		{
			if (_mod == null)
			{
				throw new ArgumentException("BuildLocation for LocationType==Mods requires a loaded Mod", "_mod");
			}
			return new PathAbstractions.AbstractedLocation?(new PathAbstractions.AbstractedLocation(_locationType, _elementName, _mod.Path + "/" + this.RelativePath + ((!string.IsNullOrEmpty(_subFolder)) ? ("/" + _subFolder) : ""), _subFolder, _elementName, this.Owner.Extension, this.Owner.IsFolder, _mod));
		}

		public override string TryGetBasePath(PathAbstractions.EAbstractedLocationType _locationType, Mod _mod)
		{
			if (_mod == null)
			{
				throw new ArgumentException("GetBasePath for LocationType==Mods requires a loaded Mod", "_mod");
			}
			return _mod.Path + "/" + this.RelativePath;
		}

		public override bool CanHandleLocationType(PathAbstractions.EAbstractedLocationType _locationType)
		{
			return _locationType == PathAbstractions.EAbstractedLocationType.Mods;
		}
	}

	public static class Contextual
	{
		public static PathAbstractions.AbstractedLocation FindDownloadedRemoteWorld(string _saveDir)
		{
			string text = Path.Combine(_saveDir, "World");
			if (!SdDirectory.Exists(text))
			{
				return PathAbstractions.AbstractedLocation.None;
			}
			return new PathAbstractions.AbstractedLocation(PathAbstractions.EAbstractedLocationType.LocalSave, "World", text, null, true, null);
		}

		public static void FindActiveWorld(out string _name, out PathAbstractions.AbstractedLocation _location)
		{
			_name = GamePrefs.GetString(EnumGamePrefs.GameWorld);
			PathAbstractions.EAbstractedLocationType @int = (PathAbstractions.EAbstractedLocationType)GamePrefs.GetInt(EnumGamePrefs.GameWorldLocationType);
			UserDataStorageType int2 = (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.UserWorldStorageType);
			_location = PathAbstractions.Contextual.FindWorld(_name, @int, int2);
		}

		public static PathAbstractions.AbstractedLocation FindActiveWorldLocation()
		{
			return PathAbstractions.Contextual.FindWorld(GamePrefs.GetString(EnumGamePrefs.GameWorld), (PathAbstractions.EAbstractedLocationType)GamePrefs.GetInt(EnumGamePrefs.GameWorldLocationType), (UserDataStorageType)GamePrefs.GetInt(EnumGamePrefs.UserWorldStorageType));
		}

		public static bool DoesWorldExist(string _name)
		{
			UserDataStorageType? userDataStorageType;
			if (PlatformManager.MultiPlatform.UserDataRoaming.SaveRoamingEnabled)
			{
				PathAbstractions.SearchDefinition worldsSearchPaths = PathAbstractions.WorldsSearchPaths;
				userDataStorageType = new UserDataStorageType?(UserDataStorageType.Roaming);
				if (worldsSearchPaths.GetLocation(_name, null, userDataStorageType).Exists())
				{
					return true;
				}
			}
			PathAbstractions.SearchDefinition worldsSearchPaths2 = PathAbstractions.WorldsSearchPaths;
			userDataStorageType = new UserDataStorageType?(UserDataStorageType.DeviceLocal);
			return worldsSearchPaths2.GetLocation(_name, null, userDataStorageType).Exists();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static PathAbstractions.AbstractedLocation FindWorld(string _name, PathAbstractions.EAbstractedLocationType _locationTypeHint, UserDataStorageType _storageTypeHint)
		{
			if (_locationTypeHint == PathAbstractions.EAbstractedLocationType.None)
			{
				PathAbstractions.SearchDefinition worldsSearchPaths = PathAbstractions.WorldsSearchPaths;
				UserDataStorageType? userDataStorageType = new UserDataStorageType?(_storageTypeHint);
				return worldsSearchPaths.GetLocation(_name, null, userDataStorageType);
			}
			return PathAbstractions.WorldsSearchPaths.GetLocation(_name, new PathAbstractions.EAbstractedLocationType?(_locationTypeHint), new UserDataStorageType?(_storageTypeHint));
		}
	}
}
