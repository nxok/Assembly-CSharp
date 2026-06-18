using System;
using System.Collections.Generic;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_BugReportSavesList : XUiC_List<XUiC_BugReportSavesList.ListEntry>
{
	public override void RebuildList(bool _resetFilter = false)
	{
	}

	public void RebuildList(IReadOnlyCollection<SaveInfoProvider.SaveEntryInfo> _saveEntryInfos, bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (SaveInfoProvider.SaveEntryInfo saveEntryInfo in _saveEntryInfos)
		{
			this.allEntries.Add(new XUiC_BugReportSavesList.ListEntry(saveEntryInfo));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[Preserve]
	public class ListEntry : XUiListEntry<XUiC_BugReportSavesList.ListEntry>
	{
		public ListEntry(SaveInfoProvider.SaveEntryInfo _saveEntryInfo)
		{
			this.SaveEntryInfo = _saveEntryInfo;
			this.SaveName = _saveEntryInfo.Name;
			this.SaveDisplayName = _saveEntryInfo.DisplayName;
			this.WorldKey = _saveEntryInfo.WorldEntry.WorldKey;
			this.SaveDirectory = _saveEntryInfo.SaveDir;
			this.LastSaved = _saveEntryInfo.LastSaved;
		}

		public override int CompareTo(XUiC_BugReportSavesList.ListEntry _otherEntry)
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

		public readonly string SaveDisplayName;

		public readonly string WorldKey;

		public readonly string SaveDirectory;

		public readonly DateTime LastSaved;

		public readonly SaveInfoProvider.SaveEntryInfo SaveEntryInfo;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_BugReportSavesList.ListEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("savename")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingSaveName()
		{
			XUiC_BugReportSavesList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.SaveDisplayName : null) ?? "";
		}

		[XuiXmlBinding("saveusesdatalimit")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingSaveUsesDataLimit()
		{
			return this.entryData != null && PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional && this.entryData.SaveEntryInfo.StorageType.UsesDataLimit();
		}

		[XuiXmlBinding("worldname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingWorldName()
		{
			XUiC_BugReportSavesList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.SaveEntryInfo.WorldEntry.Name : null) ?? "";
		}

		[XuiXmlBinding("worldusesdatalimit")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingWorldUsesDataLimit()
		{
			return this.entryData != null && PlatformManager.MultiPlatform.UserDataRoaming.IsRoamingOptional && this.entryData.SaveEntryInfo.WorldEntry.Location.StorageType.UsesDataLimit();
		}

		[XuiXmlBinding("lastplayed")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLastPlayed()
		{
			XUiC_BugReportSavesList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.LastSaved.ToString("yyyy-MM-dd HH:mm") : null) ?? "";
		}
	}
}
