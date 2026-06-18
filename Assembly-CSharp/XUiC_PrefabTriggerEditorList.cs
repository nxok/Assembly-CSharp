using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabTriggerEditorList : XUiC_List<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry>
{
	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		Func<List<byte>> getCurrentPrefabTriggerLayers = this.GetCurrentPrefabTriggerLayers;
		List<byte> list = ((getCurrentPrefabTriggerLayers != null) ? getCurrentPrefabTriggerLayers() : null);
		if (list != null)
		{
			foreach (byte b in list)
			{
				this.allEntries.Add(new XUiC_PrefabTriggerEditorList.PrefabTriggerEntry(this, b));
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public Func<List<byte>> GetCurrentPrefabTriggerLayers;

	public Func<List<byte>> GetCurrentTriggerIndicesList;

	[Preserve]
	public class PrefabTriggerEntry : XUiListEntry<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry>
	{
		public PrefabTriggerEntry(XUiC_PrefabTriggerEditorList _parentList, byte _triggerLayer)
		{
			this.parentList = _parentList;
			this.TriggerLayer = _triggerLayer;
			this.name = _triggerLayer.ToString();
		}

		public override int CompareTo(XUiC_PrefabTriggerEditorList.PrefabTriggerEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			return this.TriggerLayer.CompareTo(_otherEntry.TriggerLayer);
		}

		public bool isSelected()
		{
			Func<List<byte>> getCurrentTriggerIndicesList = this.parentList.GetCurrentTriggerIndicesList;
			List<byte> list = ((getCurrentTriggerIndicesList != null) ? getCurrentTriggerIndicesList() : null);
			return list != null && list.Contains(this.TriggerLayer);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.name.IndexOf(_searchString, StringComparison.OrdinalIgnoreCase) >= 0;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public readonly XUiC_PrefabTriggerEditorList parentList;

		public readonly string name;

		public readonly byte TriggerLayer;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_PrefabTriggerEditorList.PrefabTriggerEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_PrefabTriggerEditorList.PrefabTriggerEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.name : null) ?? "";
		}

		[XuiXmlBinding("selected")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingSelected()
		{
			XUiC_PrefabTriggerEditorList.PrefabTriggerEntry entryData = this.entryData;
			return entryData != null && entryData.isSelected();
		}
	}
}
