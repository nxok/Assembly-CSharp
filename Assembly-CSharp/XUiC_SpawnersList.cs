using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnersList : XUiC_List<XUiC_SpawnersList.SpawnerEntry>
{
	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (KeyValuePair<string, GameStageGroup> keyValuePair in GameStageGroup.Groups)
		{
			string key = keyValuePair.Key;
			this.allEntries.Add(new XUiC_SpawnersList.SpawnerEntry(key));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	[Preserve]
	public class SpawnerEntry : XUiListEntry<XUiC_SpawnersList.SpawnerEntry>
	{
		public SpawnerEntry(string _name)
		{
			this.name = _name;
			this.displayName = GameStageGroup.MakeDisplayName(_name);
		}

		public override int CompareTo(XUiC_SpawnersList.SpawnerEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return string.Compare(this.name, _otherEntry.name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public readonly string name;

		public readonly string displayName;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_SpawnersList.SpawnerEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_SpawnersList.SpawnerEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.displayName : null) ?? "";
		}
	}
}
