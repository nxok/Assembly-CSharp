using System;
using UniLinq;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ProfilesList : XUiC_List<XUiC_ProfilesList.ListEntry>
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (string text in Archetype.s_Archetypes.Keys)
		{
			if (text != "BaseMale" && text != "BaseFemale")
			{
				this.allEntries.Add(new XUiC_ProfilesList.ListEntry(text, true));
			}
		}
		foreach (string text2 in (from s in ProfileSDF.GetProfiles()
			where Archetype.GetArchetype(s) == null && ProfileSDF.GetArchetype(s) != null && (ProfileSDF.GetArchetype(s).Equals("BaseMale") || ProfileSDF.GetArchetype(s).Equals("BaseFemale"))
			select s).ToArray<string>())
		{
			this.allEntries.Add(new XUiC_ProfilesList.ListEntry(text2, false));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
		this.SelectByName(ProfileSDF.CurrentProfileName());
	}

	public bool SelectByName(string _name)
	{
		if (!string.IsNullOrEmpty(_name))
		{
			for (int i = 0; i < this.filteredEntries.Count; i++)
			{
				if (this.filteredEntries[i].Name.Equals(_name, StringComparison.OrdinalIgnoreCase))
				{
					base.SelectedEntryIndex = i;
					return true;
				}
			}
		}
		return false;
	}

	[Preserve]
	public class ListEntry : XUiListEntry<XUiC_ProfilesList.ListEntry>
	{
		public ListEntry(string _name, bool _isArchetype)
		{
			this.Name = _name;
			this.isArchetype = _isArchetype;
		}

		public override int CompareTo(XUiC_ProfilesList.ListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			int num = -this.isArchetype.CompareTo(_otherEntry.isArchetype);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(this.Name, _otherEntry.Name, StringComparison.OrdinalIgnoreCase);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.Name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		public readonly string Name;

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly bool isArchetype;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_ProfilesList.ListEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_ProfilesList.ListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.Name : null) ?? "";
		}
	}
}
