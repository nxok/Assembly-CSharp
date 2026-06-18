using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_GameEventsList : XUiC_List<XUiC_GameEventsList.GameEventEntry>
{
	public string Category
	{
		get
		{
			return this.category;
		}
		set
		{
			this.category = value;
			this.RebuildList(false);
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (KeyValuePair<string, GameEventActionSequence> keyValuePair in GameEventManager.GameEventSequences)
		{
			if (keyValuePair.Value.AllowUserTrigger && (this.category == "" || (keyValuePair.Value.CategoryNames != null && keyValuePair.Value.CategoryNames.ContainsCaseInsensitive(this.category))))
			{
				this.allEntries.Add(new XUiC_GameEventsList.GameEventEntry(keyValuePair.Key));
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (this.allEntries.Count == 0)
		{
			this.RebuildList(false);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string category = "";

	[Preserve]
	public class GameEventEntry : XUiListEntry<XUiC_GameEventsList.GameEventEntry>
	{
		public GameEventEntry(string _name)
		{
			this.name = _name;
		}

		public override int CompareTo(XUiC_GameEventsList.GameEventEntry _otherEntry)
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

		public readonly string camelCase;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_GameEventsList.GameEventEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_GameEventsList.GameEventEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.name : null) ?? "";
		}
	}
}
