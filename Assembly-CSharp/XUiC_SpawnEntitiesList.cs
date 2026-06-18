using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_SpawnEntitiesList : XUiC_List<XUiC_SpawnEntitiesList.SpawnEntityEntry>
{
	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (KeyValuePair<int, EntityClass> keyValuePair in EntityClass.list.Dict)
		{
			if (keyValuePair.Value.userSpawnType == EntityClass.UserSpawnType.Menu)
			{
				this.allEntries.Add(new XUiC_SpawnEntitiesList.SpawnEntityEntry(keyValuePair.Value.entityClassName, keyValuePair.Key));
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

	[Preserve]
	public class SpawnEntityEntry : XUiListEntry<XUiC_SpawnEntitiesList.SpawnEntityEntry>
	{
		public SpawnEntityEntry(string _name, int _key)
		{
			this.name = _name;
			this.key = _key;
			this.camelCase = this.name.SeparateCamelCase();
		}

		public override int CompareTo(XUiC_SpawnEntitiesList.SpawnEntityEntry _otherEntry)
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

		public readonly int key;

		public readonly string camelCase;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_SpawnEntitiesList.SpawnEntityEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_SpawnEntitiesList.SpawnEntityEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.name : null) ?? "";
		}
	}
}
