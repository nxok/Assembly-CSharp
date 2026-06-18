using System;
using System.IO;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PrefabFolderList : XUiC_List<XUiC_PrefabFolderList.PrefabFolderEntry>
{
	public PathAbstractions.EAbstractedLocationType LocationType
	{
		get
		{
			return this.locationType;
		}
		set
		{
			if (value == this.locationType)
			{
				return;
			}
			this.locationType = value;
			this.RebuildList(false);
		}
	}

	public Mod Mod
	{
		get
		{
			return this.mod;
		}
		set
		{
			if (value == this.mod)
			{
				return;
			}
			this.mod = value;
			this.RebuildList(false);
		}
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		string basePath = PathAbstractions.PrefabsSearchPaths.GetBasePath(this.locationType, this.mod);
		if (SdDirectory.Exists(basePath))
		{
			foreach (string text in SdDirectory.GetDirectories(basePath))
			{
				string fileName = Path.GetFileName(text);
				this.allEntries.Add(new XUiC_PrefabFolderList.PrefabFolderEntry(fileName, fileName, text));
			}
			this.allEntries.Sort();
		}
		this.allEntries.Insert(0, new XUiC_PrefabFolderList.PrefabFolderEntry("<Root>", "", basePath));
		base.RebuildList(_resetFilter);
		base.SelectedEntryIndex = 0;
	}

	public bool SelectByName(string _name)
	{
		if (string.IsNullOrEmpty(_name))
		{
			return false;
		}
		for (int i = 0; i < this.filteredEntries.Count; i++)
		{
			if (this.filteredEntries[i].Name.Equals(_name, StringComparison.OrdinalIgnoreCase))
			{
				base.SelectedEntryIndex = i;
				return true;
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PathAbstractions.EAbstractedLocationType locationType = PathAbstractions.EAbstractedLocationType.GameData;

	[PublicizedFrom(EAccessModifier.Private)]
	public Mod mod;

	public class PrefabFolderEntry : XUiListEntry<XUiC_PrefabFolderList.PrefabFolderEntry>
	{
		public PrefabFolderEntry(string _name, string _relativePath, string _absolutePath)
		{
			this.Name = _name;
			this.RelativePath = _relativePath;
			this.AbsolutePath = _absolutePath;
		}

		public override int CompareTo(XUiC_PrefabFolderList.PrefabFolderEntry _otherEntry)
		{
			return string.Compare(this.Name, _otherEntry.Name, StringComparison.Ordinal);
		}

		public override bool MatchesSearch(string _searchString)
		{
			return this.Name.ContainsCaseInsensitive(_searchString);
		}

		public readonly string Name;

		public readonly string RelativePath;

		public readonly string AbsolutePath;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_PrefabFolderList.PrefabFolderEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_PrefabFolderList.PrefabFolderEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.Name : null) ?? "";
		}
	}
}
