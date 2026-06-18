using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DMWorldList : XUiC_DMBaseList<XUiC_DMWorldList.ListEntry>
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

	public void RebuildList(IReadOnlyCollection<SaveInfoProvider.WorldEntryInfo> _worldEntryInfos, bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (SaveInfoProvider.WorldEntryInfo worldEntryInfo in _worldEntryInfos)
		{
			this.allEntries.Add(new XUiC_DMWorldList.ListEntry(worldEntryInfo, this.MatchingVersionColor, this.CompatibleVersionColor, this.IncompatibleVersionColor));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public void ClearList()
	{
		this.allEntries.Clear();
		this.RebuildList(true);
	}

	public bool SelectByKey(string _key)
	{
		if (string.IsNullOrEmpty(_key))
		{
			return false;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			if (this.filteredEntries[i].Key.Equals(_key, StringComparison.OrdinalIgnoreCase))
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}

	public void UpdateHiddenEntryVisibility()
	{
		this.filteredEntries.Clear();
		this.FilterResults(this.previousMatch);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void FilterResults(string _textMatch)
	{
		base.FilterResults(_textMatch);
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			XUiC_DMWorldList.ListEntry listEntry = this.filteredEntries[i];
			if (listEntry.HideIfEmpty && listEntry.SaveDataSizeTotal == 0L)
			{
				this.filteredEntries.RemoveAt(i);
				i--;
			}
		}
	}

	[Preserve]
	public class ListEntry : XUiListEntry<XUiC_DMWorldList.ListEntry>
	{
		public ListEntry(SaveInfoProvider.WorldEntryInfo _worldEntryInfo, string _matchingColor, string _compatibleColor, string _incompatibleColor)
		{
			this.WorldEntryInfo = _worldEntryInfo;
			this.Key = _worldEntryInfo.WorldKey;
			this.Name = _worldEntryInfo.Name;
			this.DisplayName = _worldEntryInfo.DisplayName;
			this.Type = _worldEntryInfo.Type;
			this.Location = _worldEntryInfo.Location;
			this.Deletable = _worldEntryInfo.Deletable;
			this.Moveable = _worldEntryInfo.Moveable;
			this.WorldDataSize = _worldEntryInfo.WorldDataSize;
			this.Version = _worldEntryInfo.Version;
			VersionInformation version = this.Version;
			this.VersionComparison = ((version != null) ? version.CompareToRunningBuild() : VersionInformation.EVersionComparisonResult.SameBuild);
			this.SaveDataSizeTotal = _worldEntryInfo.SaveDataSizeTotal;
			this.SaveDataSizeForLimit = _worldEntryInfo.SaveDataSizeForLimit;
			this.SaveDataCount = _worldEntryInfo.SaveDataCount;
			this.UsesDataLimit = _worldEntryInfo.UsesDataLimit;
			this.MatchingColor = _matchingColor;
			this.CompatibleColor = _compatibleColor;
			this.IncompatibleColor = _incompatibleColor;
			this.HideIfEmpty = _worldEntryInfo.HideIfEmpty;
		}

		public override int CompareTo(XUiC_DMWorldList.ListEntry _otherEntry)
		{
			if (_otherEntry != null)
			{
				return this.WorldEntryInfo.CompareTo(_otherEntry.WorldEntryInfo);
			}
			return 1;
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public readonly string Key;

		public readonly string Name;

		public readonly string DisplayName;

		public readonly string Type;

		public readonly PathAbstractions.AbstractedLocation Location;

		public readonly bool Deletable;

		public readonly bool Moveable;

		public readonly long WorldDataSize;

		public readonly VersionInformation Version;

		public readonly VersionInformation.EVersionComparisonResult VersionComparison;

		public readonly long SaveDataSizeTotal;

		public readonly long SaveDataSizeForLimit;

		public readonly int SaveDataCount;

		public readonly bool HideIfEmpty;

		public readonly SaveInfoProvider.WorldEntryInfo WorldEntryInfo;

		public readonly bool UsesDataLimit;

		public readonly string MatchingColor;

		public readonly string CompatibleColor;

		public readonly string IncompatibleColor;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_DMWorldList.ListEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_DMWorldList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.DisplayName : null) ?? "";
		}

		[XuiXmlBinding("type")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingType()
		{
			XUiC_DMWorldList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.Type : null) ?? "";
		}

		[XuiXmlBinding("worldDataSize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldDataSize()
		{
			if (this.entryData == null)
			{
				return "";
			}
			if (!this.entryData.Deletable)
			{
				return "-";
			}
			return ValueDisplayFormatters.MemoryMiB(this.entryData.WorldDataSize);
		}

		[XuiXmlBinding("saveDataSize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveDataSize()
		{
			if (this.entryData != null)
			{
				return ValueDisplayFormatters.MemoryMiB(this.entryData.SaveDataSizeTotal);
			}
			return "";
		}

		[XuiXmlBinding("saveDataCount")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveDataCount()
		{
			if (this.entryData != null)
			{
				return string.Format("{0} {1}", this.entryData.SaveDataCount, Localization.Get("xuiDmSaves", false, null));
			}
			return "";
		}

		[XuiXmlBinding("isRoamingOptional")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingIsRoamingOptional()
		{
			return PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional;
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

		[XuiXmlBinding("totalDataSizeForLimit")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingtTotalDataSizeForLimit()
		{
			if (this.entryData != null)
			{
				return ValueDisplayFormatters.MemoryMiB(this.entryData.UsesDataLimit ? (this.entryData.WorldDataSize + this.entryData.SaveDataSizeForLimit) : this.entryData.SaveDataSizeForLimit);
			}
			return "";
		}

		[XuiXmlBinding("totalDataSizeNotLimited")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingTotalDataSizeNotLimited()
		{
			if (this.entryData == null)
			{
				return "";
			}
			long num = this.entryData.SaveDataSizeTotal - this.entryData.SaveDataSizeForLimit;
			return ValueDisplayFormatters.MemoryMiB((!this.entryData.UsesDataLimit) ? (this.entryData.WorldDataSize + num) : num);
		}

		[XuiXmlBinding("totalDataSize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingTotalDataSize()
		{
			if (this.entryData != null)
			{
				return ValueDisplayFormatters.MemoryMiB(this.entryData.UsesDataLimit ? (this.entryData.WorldDataSize + this.entryData.SaveDataSizeTotal) : this.entryData.SaveDataSizeTotal);
			}
			return "";
		}

		[XuiXmlBinding("versioncolor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersioncolor()
		{
			if (this.entryData == null)
			{
				return "0,0,0,0";
			}
			if (this.entryData.Version == null)
			{
				return this.entryData.IncompatibleColor;
			}
			if (this.entryData.Version.Major < 0)
			{
				return this.entryData.MatchingColor;
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

		[XuiXmlBinding("version")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingVersion()
		{
			XUiC_DMWorldList.ListEntry entryData = this.entryData;
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
			if (this.entryData.Version == null || this.entryData.Version.Major < 0)
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
	}
}
