using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabFileList : XUiC_List<XUiC_PrefabFileList.PrefabFileEntry>
{
	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		this.prefabSearchList.Clear();
		PrefabEditModeManager.Instance.FindPrefabs(this.groupFilter, this.prefabSearchList);
		foreach (PathAbstractions.AbstractedLocation abstractedLocation in this.prefabSearchList)
		{
			this.allEntries.Add(new XUiC_PrefabFileList.PrefabFileEntry(abstractedLocation));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public void SetGroupFilter(string _filter)
	{
		this.groupFilter = _filter;
		this.RebuildList(true);
	}

	public bool SelectByName(string _name)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < this.filteredEntries.Count; i++)
			{
				if (this.filteredEntries[i].location.Name.EqualsCaseInsensitive(_name))
				{
					base.SelectedEntryIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	public bool SelectByLocation(PathAbstractions.AbstractedLocation _location)
	{
		if (_location.Type == PathAbstractions.EAbstractedLocationType.None)
		{
			return false;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			if (this.filteredEntries[i].location == _location)
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string groupFilter;

	[PublicizedFrom(EAccessModifier.Private)]
	public readonly List<PathAbstractions.AbstractedLocation> prefabSearchList = new List<PathAbstractions.AbstractedLocation>();

	public class PrefabFileEntry : XUiListEntry<XUiC_PrefabFileList.PrefabFileEntry>
	{
		public PrefabFileEntry(PathAbstractions.AbstractedLocation _location)
		{
			this.location = _location;
		}

		public override int CompareTo(XUiC_PrefabFileList.PrefabFileEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return this.location.CompareTo(_otherEntry.location);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.location.Name.ContainsCaseInsensitive(_searchString);
		}

		public readonly PathAbstractions.AbstractedLocation location;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_PrefabFileList.PrefabFileEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			if (this.entryData == null)
			{
				return "";
			}
			PathAbstractions.EAbstractedLocationType type = this.entryData.location.Type;
			string text;
			if (type != PathAbstractions.EAbstractedLocationType.Mods)
			{
				if (type != PathAbstractions.EAbstractedLocationType.GameData)
				{
					text = " (from " + this.entryData.location.Type.ToStringCached<PathAbstractions.EAbstractedLocationType>() + ")";
				}
				else
				{
					text = "";
				}
			}
			else
			{
				text = " (Mod: " + this.entryData.location.ContainingMod.Name + ")";
			}
			string text2 = text;
			return this.entryData.location.Name + text2;
		}

		[XuiXmlBinding("localizedname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLocalizedName()
		{
			if (this.entryData != null)
			{
				return Localization.Get(this.entryData.location.Name, false, null);
			}
			return "";
		}
	}
}
