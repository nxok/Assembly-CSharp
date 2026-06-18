using System;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SavegamesList : XUiC_List<XUiC_SavegamesList.ListEntry>
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		GameIO.GetPlayerSaves(new GameIO.FoundSave(this.addSaveToEntries), false);
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public bool SelectByName(string _name)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < this.filteredEntries.Count; i++)
			{
				if (this.filteredEntries[i].SaveName.Equals(_name, StringComparison.OrdinalIgnoreCase))
				{
					base.SelectedEntryIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	public void SelectEntry(string _worldName, string _saveName, UserDataStorageType _storageType)
	{
		if (this.filteredEntries == null)
		{
			Log.Error("filteredEntries is null");
			return;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			XUiC_SavegamesList.ListEntry listEntry = this.filteredEntries[i];
			if (listEntry.WorldName.EqualsCaseInsensitive(_worldName) && listEntry.SaveName.EqualsCaseInsensitive(_saveName) && listEntry.StorageType == _storageType)
			{
				base.SelectedEntryIndex = i;
				return;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void addSaveToEntries(UserDataStorageType _storage, string _saveName, string _worldName, DateTime _lastSaved, WorldState _worldState, bool _isArchived)
	{
		this.allEntries.Add(new XUiC_SavegamesList.ListEntry(_storage, _saveName, _worldName, _lastSaved, _worldState));
	}

	[Preserve]
	public class ListEntry : XUiListEntry<XUiC_SavegamesList.ListEntry>
	{
		public GameMode GameMode
		{
			get
			{
				return GameMode.GetGameModeForId(this.WorldState.activeGameMode);
			}
		}

		public ListEntry(UserDataStorageType _storage, string _saveName, string _worldName, DateTime _lastSaved, WorldState _worldState)
		{
			this.StorageType = _storage;
			this.SaveName = _saveName;
			this.WorldName = _worldName;
			this.LastSaved = _lastSaved;
			this.WorldState = _worldState;
			this.WorldExists = PathAbstractions.Contextual.DoesWorldExist(_worldName);
			this.SaveVersion = this.WorldState.gameVersion;
			this.VersionComparison = XUiC_SavegamesList.ListEntry.checkSaveCompatibility(this.SaveVersion);
			XUiC_SavegamesList.ListEntry.ESaveCompatibility versionComparison = this.VersionComparison;
			this.Playable = versionComparison == XUiC_SavegamesList.ListEntry.ESaveCompatibility.SameVersion || versionComparison == XUiC_SavegamesList.ListEntry.ESaveCompatibility.NewerMinor || versionComparison == XUiC_SavegamesList.ListEntry.ESaveCompatibility.Compatible;
			if (PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional)
			{
				this.DisplaySaveName = this.SaveName + " [808080][i](" + this.StorageType.LocalizedName() + ")[/i][-]";
				return;
			}
			this.DisplaySaveName = this.SaveName;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static XUiC_SavegamesList.ListEntry.ESaveCompatibility checkSaveCompatibility(VersionInformation _saveVersion)
		{
			VersionInformation cVersionInformation = Constants.cVersionInformation;
			if (_saveVersion.ReleaseType < cVersionInformation.ReleaseType)
			{
				return XUiC_SavegamesList.ListEntry.ESaveCompatibility.OldVersion;
			}
			if (_saveVersion.ReleaseType > cVersionInformation.ReleaseType)
			{
				return XUiC_SavegamesList.ListEntry.ESaveCompatibility.FutureVersion;
			}
			if (_saveVersion.Major > cVersionInformation.Major)
			{
				return XUiC_SavegamesList.ListEntry.ESaveCompatibility.FutureVersion;
			}
			if (_saveVersion.Major == cVersionInformation.Major)
			{
				if (_saveVersion.Minor == cVersionInformation.Minor)
				{
					return XUiC_SavegamesList.ListEntry.ESaveCompatibility.SameVersion;
				}
				if (_saveVersion.Minor > cVersionInformation.Minor)
				{
					return XUiC_SavegamesList.ListEntry.ESaveCompatibility.NewerMinor;
				}
				return XUiC_SavegamesList.ListEntry.ESaveCompatibility.Compatible;
			}
			else
			{
				if (_saveVersion.Major < 2)
				{
					return XUiC_SavegamesList.ListEntry.ESaveCompatibility.OldVersion;
				}
				return XUiC_SavegamesList.ListEntry.ESaveCompatibility.Compatible;
			}
		}

		public string GetSaveDir()
		{
			return GameIO.GetSaveGameDir(this.WorldName, this.SaveName, this.StorageType);
		}

		public override int CompareTo(XUiC_SavegamesList.ListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			return -1 * this.LastSaved.CompareTo(_otherEntry.LastSaved);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.SaveName.ContainsCaseInsensitive(_searchString);
		}

		public readonly UserDataStorageType StorageType;

		public readonly string SaveName;

		public readonly string DisplaySaveName;

		public readonly string WorldName;

		public readonly DateTime LastSaved;

		public readonly WorldState WorldState;

		public readonly bool WorldExists;

		public readonly VersionInformation SaveVersion;

		public readonly XUiC_SavegamesList.ListEntry.ESaveCompatibility VersionComparison;

		public readonly bool Playable;

		public enum ESaveCompatibility
		{
			SameVersion,
			NewerMinor,
			Compatible,
			FutureVersion,
			OldVersion
		}
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_SavegamesList.ListEntry>.XUiC_ListEntry
	{
		public override void Init()
		{
			base.Init();
			XUiController childById = base.GetChildById("Version");
			childById.OnScroll += delegate(XUiController _, float _delta)
			{
				base.Scrolled(_delta);
			};
			childById.OnPress += delegate(XUiController _, int _button)
			{
				this.Pressed(_button);
			};
			childById.OnDoubleClick += delegate(XUiController _, int _button)
			{
				base.DoubleClicked(_button);
			};
			childById.OnHover += delegate(XUiController _, bool _over)
			{
				base.ForceHovered = _over;
				base.Hovered(_over);
			};
			XUiController childById2 = base.GetChildById("World");
			childById2.OnScroll += delegate(XUiController _, float _delta)
			{
				base.Scrolled(_delta);
			};
			childById2.OnPress += delegate(XUiController _, int _button)
			{
				this.Pressed(_button);
			};
			childById2.OnDoubleClick += delegate(XUiController _, int _button)
			{
				base.DoubleClicked(_button);
			};
			childById2.OnHover += delegate(XUiController _, bool _over)
			{
				base.ForceHovered = _over;
				base.Hovered(_over);
			};
		}

		[XuiXmlBinding("savename")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveName()
		{
			XUiC_SavegamesList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.DisplaySaveName : null) ?? "";
		}

		[XuiXmlBinding("worldname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldName()
		{
			XUiC_SavegamesList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.WorldName : null) ?? "";
		}

		[XuiXmlBinding("worldtooltip")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldTooltip()
		{
			if (this.entryData != null && !this.entryData.WorldExists)
			{
				return Localization.Get("xuiSavegameWorldNotFound", false, null);
			}
			return "";
		}

		[XuiXmlBinding("mode")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingMode()
		{
			if (this.entryData == null)
			{
				return "";
			}
			GameMode gameMode = this.entryData.GameMode;
			if (gameMode != null)
			{
				return Localization.Get(gameMode.GetName(), false, null);
			}
			return "-Unknown-";
		}

		[XuiXmlBinding("version")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersion()
		{
			if (this.entryData == null)
			{
				return "";
			}
			WorldState worldState = this.entryData.WorldState;
			if (worldState.gameVersion.Major < 0)
			{
				return worldState.gameVersionString;
			}
			return worldState.gameVersion.ShortString;
		}

		[XuiXmlBinding("versiontooltip")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersionTooltip()
		{
			if (this.entryData == null)
			{
				return "";
			}
			string text;
			switch (this.entryData.VersionComparison)
			{
			case XUiC_SavegamesList.ListEntry.ESaveCompatibility.SameVersion:
				text = "";
				break;
			case XUiC_SavegamesList.ListEntry.ESaveCompatibility.NewerMinor:
				text = Localization.Get("xuiSavegameNewerMinor", false, null);
				break;
			case XUiC_SavegamesList.ListEntry.ESaveCompatibility.Compatible:
				text = Localization.Get("xuiSavegameOlderMinor", false, null);
				break;
			case XUiC_SavegamesList.ListEntry.ESaveCompatibility.FutureVersion:
				text = Localization.Get("xuiSavegameFutureMajor", false, null);
				break;
			case XUiC_SavegamesList.ListEntry.ESaveCompatibility.OldVersion:
				text = Localization.Get("xuiSavegameOldVersion", false, null);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return text;
		}

		[XuiXmlBinding("lastplayed")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLastPlayed()
		{
			XUiC_SavegamesList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.LastSaved.ToString("yyyy-MM-dd HH:mm") : null) ?? "";
		}

		[XuiXmlBinding("usesdatalimit")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingUsesDataLimit()
		{
			return this.entryData != null && PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional && this.entryData.StorageType.UsesDataLimit();
		}

		[XuiXmlBinding("worldstate")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldState()
		{
			if (this.entryData == null)
			{
				return "";
			}
			if (!this.entryData.WorldExists)
			{
				return "missing";
			}
			return "ok";
		}

		[XuiXmlBinding("versionstate")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersionState()
		{
			if (this.entryData == null)
			{
				return "";
			}
			string text;
			switch (this.entryData.VersionComparison)
			{
			case XUiC_SavegamesList.ListEntry.ESaveCompatibility.SameVersion:
				text = "same";
				break;
			case XUiC_SavegamesList.ListEntry.ESaveCompatibility.NewerMinor:
				text = "compatible";
				break;
			case XUiC_SavegamesList.ListEntry.ESaveCompatibility.Compatible:
				text = "compatible";
				break;
			case XUiC_SavegamesList.ListEntry.ESaveCompatibility.FutureVersion:
				text = "incompatible";
				break;
			case XUiC_SavegamesList.ListEntry.ESaveCompatibility.OldVersion:
				text = "incompatible";
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return text;
		}
	}
}
