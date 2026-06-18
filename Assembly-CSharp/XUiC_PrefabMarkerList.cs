using System;
using PrefabVolumes;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabMarkerList : XUiC_List<XUiC_PrefabMarkerList.PrefabMarkerEntry>
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		if (PrefabEditModeManager.Instance.VoxelPrefab == null)
		{
			return;
		}
		this.allEntries.Clear();
		foreach (SelectionBox selectionBox in POIMarkerToolManager.GetRegisteredBoxes())
		{
			this.allEntries.Add(new XUiC_PrefabMarkerList.PrefabMarkerEntry(this, selectionBox));
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public bool SelectByMarker(Marker _marker)
	{
		if (_marker == null)
		{
			return false;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			if (this.filteredEntries[i].Marker == _marker)
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}

	[Preserve]
	public class PrefabMarkerEntry : XUiListEntry<XUiC_PrefabMarkerList.PrefabMarkerEntry>
	{
		public Marker Marker
		{
			get
			{
				return this.Box.UserData as Marker;
			}
		}

		public PrefabMarkerEntry(XUiC_PrefabMarkerList _parentList, SelectionBox _box)
		{
			this.parentList = _parentList;
			this.Box = _box;
		}

		public override int CompareTo(XUiC_PrefabMarkerList.PrefabMarkerEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			int num = string.Compare(this.Marker.GroupName, _otherEntry.Marker.GroupName, StringComparison.OrdinalIgnoreCase);
			if (num != 0)
			{
				return num;
			}
			return string.Compare(this.Box.name, _otherEntry.Box.name, StringComparison.Ordinal);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.Marker.GroupName.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_PrefabMarkerList parentList;

		public readonly SelectionBox Box;

		[PublicizedFrom(EAccessModifier.Private)]
		public static readonly CachedStringFormatterXuiRgbaColor colorFormatter = new CachedStringFormatterXuiRgbaColor();
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_PrefabMarkerList.PrefabMarkerEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("groupname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingGroupName()
		{
			XUiC_PrefabMarkerList.PrefabMarkerEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.Marker.GroupName : null) ?? "";
		}

		[XuiXmlBinding("markertype")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingMarkerType()
		{
			XUiC_PrefabMarkerList.PrefabMarkerEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.Marker.MarkerType.ToString() : null) ?? "";
		}

		[XuiXmlBinding("groupcolor")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingGroupColor()
		{
			if (this.entryData == null)
			{
				return "0,0,0,0";
			}
			Color groupColor = this.entryData.Marker.GroupColor;
			groupColor.a = 1f;
			return groupColor.ToXuiColorString();
		}

		[XuiXmlBinding("markersize")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingMarkerSize()
		{
			if (this.entryData == null)
			{
				return "";
			}
			int num = Marker.MarkerSizes.IndexOf(this.entryData.Marker.size);
			if (num >= 0)
			{
				return ((Marker.MarkerSize)num).ToString();
			}
			return Marker.MarkerSize.Custom.ToString();
		}
	}
}
