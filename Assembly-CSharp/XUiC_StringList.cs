using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_StringList : XUiC_List<XUiC_StringList.Entry>
{
	public void ClearList()
	{
		this.allEntries.Clear();
	}

	public void AddEntry(object _entry, object _tag = null)
	{
		this.allEntries.Add(new XUiC_StringList.Entry(this, _entry, _tag));
	}

	public void SortList()
	{
		this.allEntries.Sort();
	}

	public bool SelectByString(string _toSelect)
	{
		int num = this.filteredEntries.FindIndex((XUiC_StringList.Entry _entry) => _entry.DataString.Equals(_toSelect));
		if (num < 0)
		{
			return false;
		}
		base.SelectedEntryIndex = num;
		return true;
	}

	public bool SelectByTag(object _tag)
	{
		if (_tag == null)
		{
			return false;
		}
		int num = this.filteredEntries.FindIndex((XUiC_StringList.Entry _entry) => _tag.Equals(_entry.Tag));
		if (num < 0)
		{
			return false;
		}
		base.SelectedEntryIndex = num;
		return true;
	}

	public StringComparison SortingMode = StringComparison.OrdinalIgnoreCase;

	[Preserve]
	public class Entry : XUiListEntry<XUiC_StringList.Entry>
	{
		public Entry(XUiC_StringList _parentList, object _data, object _tag)
		{
			this.parentList = _parentList;
			this.Data = _data;
			this.Tag = _tag;
			this.DataString = this.Data.ToString();
		}

		public override int CompareTo(XUiC_StringList.Entry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return string.Compare(this.DataString, _otherEntry.DataString, this.parentList.SortingMode);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.DataString.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_StringList parentList;

		public readonly object Data;

		public readonly string DataString;

		public readonly object Tag;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_StringList.Entry>.XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_StringList.Entry entryData = this.entryData;
			return ((entryData != null) ? entryData.DataString : null) ?? "";
		}
	}
}
