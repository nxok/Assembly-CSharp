using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DMSavegamesList : XUiC_DMBaseList<XUiC_DMSavegamesList.ListEntry>
{
	[XuiXmlAttribute("matching_version_color", false)]
	public string MatchingVersionColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("compatible_version_color", false)]
	public string CompatibleVersionColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	[XuiXmlAttribute("incompatible_version_color", false)]
	public string IncompatibleVersionColor
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get;
		[PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(IReadOnlyCollection<SaveInfoProvider.SaveEntryInfo> _saveEntryInfos, bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in _saveEntryInfos)
		{
			this.allEntries.Add(new XUiC_DMSavegamesList.ListEntry(saveEntryInfo, this.MatchingVersionColor, this.CompatibleVersionColor, this.IncompatibleVersionColor));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public void ClearList()
	{
		this.allEntries.Clear();
		this.RebuildList(true);
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

	public void SetWorldFilter(string _worldKey)
	{
		this.worldFilter = _worldKey;
		this.filteredEntries.Clear();
		this.FilterResults(this.previousMatch);
		this.RefreshView(false, true);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FilterResults(string _textMatch)
	{
		base.FilterResults(_textMatch);
		if (this.worldFilter == null)
		{
			return;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			if (this.filteredEntries[i].WorldKey != this.worldFilter)
			{
				this.filteredEntries.RemoveAt(i);
				i--;
			}
		}
	}

	public IEnumerable<XUiC_DMSavegamesList.ListEntry> GetSavesInWorld(string _worldKey)
	{
		if (string.IsNullOrEmpty(_worldKey))
		{
			yield break;
		}
		int num;
		for (int i = 0; i < this.allEntries.Count; i = num + 1)
		{
			if (this.allEntries[i].WorldKey == _worldKey)
			{
				yield return this.allEntries[i];
			}
			num = i;
		}
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string worldFilter;

	[Preserve]
	public class ListEntry : XUiListEntry<XUiC_DMSavegamesList.ListEntry>
	{
		public ListEntry(SaveInfoProvider.SaveEntryInfo _saveEntryInfo, string _matchingColor = "255,255,255", string _compatibleColor = "255,255,255", string _incompatibleColor = "255,255,255")
		{
			this.SaveEntryInfo = _saveEntryInfo;
			this.SaveName = _saveEntryInfo.Name;
			this.DisplayName = _saveEntryInfo.DisplayName;
			this.WorldKey = _saveEntryInfo.WorldEntry.WorldKey;
			this.SaveDirectory = _saveEntryInfo.SaveDir;
			this.LastSaved = _saveEntryInfo.LastSaved;
			this.Version = _saveEntryInfo.Version;
			VersionInformation version = this.Version;
			this.VersionComparison = ((version != null) ? version.CompareToRunningBuild() : VersionInformation.EVersionComparisonResult.SameBuild);
			this.MatchingColor = _matchingColor;
			this.CompatibleColor = _compatibleColor;
			this.IncompatibleColor = _incompatibleColor;
		}

		public override int CompareTo(XUiC_DMSavegamesList.ListEntry _otherEntry)
		{
			if (_otherEntry != null)
			{
				return this.SaveEntryInfo.CompareTo(_otherEntry.SaveEntryInfo);
			}
			return 1;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.SaveName.ContainsCaseInsensitive(_searchString);
		}

		public readonly string SaveName;

		public readonly string DisplayName;

		public readonly string WorldKey;

		public readonly string SaveDirectory;

		public readonly DateTime LastSaved;

		public readonly VersionInformation Version;

		public readonly VersionInformation.EVersionComparisonResult VersionComparison;

		public readonly SaveInfoProvider.SaveEntryInfo SaveEntryInfo;

		public readonly string MatchingColor;

		public readonly string CompatibleColor;

		public readonly string IncompatibleColor;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_DMSavegamesList.ListEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("savename")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveName()
		{
			XUiC_DMSavegamesList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.DisplayName : null) ?? "";
		}

		[XuiXmlBinding("worldname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldName()
		{
			XUiC_DMSavegamesList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.WorldKey : null) ?? "";
		}

		[XuiXmlBinding("version")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersion()
		{
			XUiC_DMSavegamesList.ListEntry entryData = this.entryData;
			if (((entryData != null) ? entryData.Version : null) == null)
			{
				return "";
			}
			if (this.entryData.Version.Major < 0)
			{
				return Constants.cVersionInformation.LongStringNoBuild;
			}
			return this.entryData.Version.LongStringNoBuild;
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
			case VersionInformation.EVersionComparisonResult.SameBuild:
				text = "";
				break;
			case VersionInformation.EVersionComparisonResult.SameMinor:
				text = "";
				break;
			case VersionInformation.EVersionComparisonResult.NewerMinor:
				text = Localization.Get("xuiSavegameNewerMinor", false, null);
				break;
			case VersionInformation.EVersionComparisonResult.OlderMinor:
				text = Localization.Get("xuiSavegameOlderMinor", false, null);
				break;
			default:
				text = Localization.Get("xuiSavegameDifferentMajor", false, null);
				break;
			}
			return text;
		}

		[XuiXmlBinding("lastplayed")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLastPlayed()
		{
			XUiC_DMSavegamesList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.LastSaved.ToString("yyyy-MM-dd HH:mm") : null) ?? "";
		}

		[XuiXmlBinding("lastplayedinfo")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLastPlayedInfo()
		{
			if (this.entryData == null)
			{
				return "";
			}
			if (this.entryData.SaveEntryInfo.SizeInfo.IsArchived)
			{
				return "[fabc02ff]" + Localization.Get("xuiDmArchivedLabel", false, null) + "[-]";
			}
			int num = (int)(DateTime.Now - this.entryData.LastSaved).TotalDays;
			return string.Format("[ffffff88]{0} {1}[-]", num, Localization.Get("xuiDmDaysAgo", false, null));
		}

		[XuiXmlBinding("savesize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveSize()
		{
			if (this.entryData == null)
			{
				return "";
			}
			string text = (this.entryData.SaveEntryInfo.SizeInfo.IsArchived ? "fabc02ff" : "ffffffbb");
			return string.Concat(new string[]
			{
				"[",
				text,
				"]",
				ValueDisplayFormatters.MemoryMiB(this.entryData.SaveEntryInfo.SizeInfo.ReportedSize),
				"[-]"
			});
		}

		[XuiXmlBinding("nameColor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingNameColor()
		{
			if (this.entryData != null)
			{
				return "255,255,255";
			}
			return "0,0,0,0";
		}

		[XuiXmlBinding("saveSizeColor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveSizeColor()
		{
			if (this.entryData != null)
			{
				return "255,255,255";
			}
			return "0,0,0,0";
		}

		[XuiXmlBinding("versioncolor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersionColor()
		{
			if (this.entryData == null)
			{
				return "0,0,0,0";
			}
			string text;
			switch (this.entryData.VersionComparison)
			{
			case VersionInformation.EVersionComparisonResult.SameBuild:
				text = this.entryData.MatchingColor;
				break;
			case VersionInformation.EVersionComparisonResult.SameMinor:
				text = this.entryData.MatchingColor;
				break;
			case VersionInformation.EVersionComparisonResult.NewerMinor:
				text = this.entryData.CompatibleColor;
				break;
			case VersionInformation.EVersionComparisonResult.OlderMinor:
				text = this.entryData.CompatibleColor;
				break;
			default:
				text = this.entryData.IncompatibleColor;
				break;
			}
			return text;
		}

		[XuiXmlBinding("entrycolor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingEntryColor()
		{
			if (this.entryData == null)
			{
				return "0,0,0,0";
			}
			string text;
			switch (this.entryData.VersionComparison)
			{
			case VersionInformation.EVersionComparisonResult.SameBuild:
				text = this.entryData.MatchingColor;
				break;
			case VersionInformation.EVersionComparisonResult.SameMinor:
				text = this.entryData.MatchingColor;
				break;
			case VersionInformation.EVersionComparisonResult.NewerMinor:
				text = this.entryData.CompatibleColor;
				break;
			case VersionInformation.EVersionComparisonResult.OlderMinor:
				text = this.entryData.CompatibleColor;
				break;
			default:
				text = this.entryData.IncompatibleColor;
				break;
			}
			return text;
		}
	}
}
