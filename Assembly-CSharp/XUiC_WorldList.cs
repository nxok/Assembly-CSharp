using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldList : XUiC_List<XUiC_WorldList.WorldListEntry>
{
	[PublicizedFrom(EAccessModifier.Private)]
	public long getWorldMemory(XUiC_WorldList.WorldListEntry _worldEntry)
	{
		PathAbstractions.AbstractedLocation location = _worldEntry.Location;
		if (!this.worldDataSizeCache.ContainsKey(location))
		{
			this.worldDataSizeCache.Add(location, GameIO.GetDirectorySize(location.FullPath, true));
		}
		return this.worldDataSizeCache[location];
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.worldDataSizeCache.Clear();
		this.RebuildList(false);
	}

	public override void OnClose()
	{
		base.OnClose();
		this.worldDataSizeCache.Clear();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.worldDataSizeCache.Clear();
		this.allEntries.Clear();
		foreach (PathAbstractions.AbstractedLocation abstractedLocation in PathAbstractions.WorldsSearchPaths.GetAvailablePathsList(null, false, null))
		{
			if (!XUiC_WorldList.forbiddenWorlds.ContainsWithComparer(abstractedLocation.Name, StringComparer.OrdinalIgnoreCase) && (this.userDataStorageTypeFilter.Count <= 0 || this.userDataStorageTypeFilter.Contains(abstractedLocation.StorageType)))
			{
				GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(abstractedLocation);
				if (worldInfo != null)
				{
					this.allEntries.Add(new XUiC_WorldList.WorldListEntry(abstractedLocation, GameIO.IsWorldGenerated(abstractedLocation.Name, abstractedLocation.StorageType), worldInfo.GameVersionCreated));
				}
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public void SetUserDataStorageTypeFilter(params UserDataStorageType[] _storageTypes)
	{
		this.userDataStorageTypeFilter.Clear();
		foreach (UserDataStorageType userDataStorageType in _storageTypes)
		{
			this.userDataStorageTypeFilter.Add(userDataStorageType);
		}
		this.filteredEntries.Clear();
		this.FilterResults(this.previousMatch);
		this.RefreshView(false, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FilterResults(string _textMatch)
	{
		base.FilterResults(_textMatch);
		if (this.userDataStorageTypeFilter.Count == 0)
		{
			return;
		}
		for (int i = this.filteredEntries.Count - 1; i >= 0; i--)
		{
			if (!this.userDataStorageTypeFilter.Contains(this.filteredEntries[i].Location.StorageType))
			{
				this.filteredEntries.RemoveAt(i);
				i--;
			}
		}
	}

	public bool SelectByName(string _name)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return false;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			if (this.filteredEntries[i].Location.Name.Equals(_name, StringComparison.OrdinalIgnoreCase))
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static readonly List<string> forbiddenWorlds = new List<string> { "Empty", "Playtesting" };

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<UserDataStorageType> userDataStorageTypeFilter = new List<UserDataStorageType>();

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly Dictionary<PathAbstractions.AbstractedLocation, long> worldDataSizeCache = new Dictionary<PathAbstractions.AbstractedLocation, long>();

	[Preserve]
	public class WorldListEntry : XUiListEntry<XUiC_WorldList.WorldListEntry>
	{
		public WorldListEntry(PathAbstractions.AbstractedLocation _location, bool _generatedWorld, VersionInformation _version)
		{
			this.Location = _location;
			this.GeneratedWorld = _generatedWorld;
			this.Version = _version;
			this.VersionComparison = this.Version.CompareToRunningBuild();
		}

		public override int CompareTo(XUiC_WorldList.WorldListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			return this.Location.CompareTo(_otherEntry.Location);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.Location.Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public readonly PathAbstractions.AbstractedLocation Location;

		public readonly bool GeneratedWorld;

		public readonly VersionInformation Version;

		public readonly VersionInformation.EVersionComparisonResult VersionComparison;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_WorldList.WorldListEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			if (this.entryData != null)
			{
				return this.entryData.Location.Name;
			}
			return "";
		}

		[XuiXmlBinding("location")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLocation()
		{
			if (this.entryData == null)
			{
				return "";
			}
			return this.entryData.Location.GetLocationTypeDisplayString();
		}

		[XuiXmlBinding("version")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersion()
		{
			if (this.entryData == null)
			{
				return "";
			}
			if (!this.entryData.Version.IsValid)
			{
				return VersionInformation.EGameReleaseType.V.ToString() + " " + 3.ToString();
			}
			return this.entryData.Version.LongStringNoBuild;
		}

		[XuiXmlBinding("versionvalid")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingValidVersion()
		{
			return this.entryData == null || !this.entryData.Version.IsValid || this.entryData.Version.Major == 3;
		}

		[XuiXmlBinding("worldsize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldSize()
		{
			if (this.entryData != null)
			{
				return ValueDisplayFormatters.MemoryMiB(this.parentList.getWorldMemory(this.entryData));
			}
			return "";
		}

		[XuiXmlBinding("versionstate")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersionState()
		{
			if (this.entryData == null)
			{
				return "";
			}
			if (!this.entryData.Version.IsValid)
			{
				return "same";
			}
			string text;
			switch (this.entryData.VersionComparison)
			{
			case VersionInformation.EVersionComparisonResult.SameBuild:
				text = "same";
				break;
			case VersionInformation.EVersionComparisonResult.SameMinor:
				text = "same";
				break;
			case VersionInformation.EVersionComparisonResult.NewerMinor:
				text = "compatible";
				break;
			case VersionInformation.EVersionComparisonResult.OlderMinor:
				text = "compatible";
				break;
			default:
				text = "incompatible";
				break;
			}
			return text;
		}

		[XuiBindParent(true)]
		[PublicizedFrom(EAccessModifier.Private)]
		public XUiC_WorldList parentList;
	}
}
