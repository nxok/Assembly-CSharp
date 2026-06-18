using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_WorldSelectionList : XUiC_List<XUiC_WorldSelectionList.Entry>
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	public override void OnClose()
	{
		base.OnClose();
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (PathAbstractions.AbstractedLocation abstractedLocation in PathAbstractions.WorldsSearchPaths.GetAvailablePathsList(null, false, null))
		{
			GameUtils.WorldInfo worldInfo = GameUtils.WorldInfo.LoadWorldInfo(abstractedLocation);
			if (worldInfo != null)
			{
				this.allEntries.Add(new XUiC_WorldSelectionList.Entry(abstractedLocation, worldInfo.GameVersionCreated));
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[Preserve]
	public class Entry : XUiListEntry<XUiC_WorldSelectionList.Entry>
	{
		public Entry(PathAbstractions.AbstractedLocation _location, VersionInformation _version)
		{
			this.Location = _location;
			this.Version = _version;
		}

		public override int CompareTo(XUiC_WorldSelectionList.Entry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			return this.Location.CompareTo(_otherEntry.Location);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.Location.Name.EqualsCaseInsensitive(_searchString);
		}

		public readonly PathAbstractions.AbstractedLocation Location;

		public readonly VersionInformation Version;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_WorldSelectionList.Entry>.XUiC_ListEntry
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
	}
}
