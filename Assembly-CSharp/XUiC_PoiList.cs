using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_PoiList : XUiC_List<XUiC_PoiList.PoiListEntry>
{
	public bool FilterSmallPois
	{
		get
		{
			return this.filterSmallPois;
		}
		set
		{
			if (this.filterSmallPois != value)
			{
				this.filterSmallPois = value;
				this.RebuildList(false);
			}
		}
	}

	public int FilterTier
	{
		get
		{
			return this.filterTier;
		}
		set
		{
			if (this.filterTier != value)
			{
				this.filterTier = value;
				this.RebuildList(false);
			}
		}
	}

	public int MinTier
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int MaxTier
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		if (GameManager.Instance != null)
		{
			DynamicPrefabDecorator dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
			if (dynamicPrefabDecorator != null)
			{
				List<PrefabInstance> list = new List<PrefabInstance>();
				dynamicPrefabDecorator.GetWorldPrefabs(list);
				foreach (PrefabInstance prefabInstance in list)
				{
					if (!this.openedBefore)
					{
						this.MinTier = Mathf.Min((int)prefabInstance.prefab.DifficultyTier, this.MinTier);
						this.MaxTier = Mathf.Max((int)prefabInstance.prefab.DifficultyTier, this.MaxTier);
					}
					if ((!this.filterSmallPois || prefabInstance.boundingBoxSize.Volume() >= 100) && (this.filterTier < 0 || (int)prefabInstance.prefab.DifficultyTier == this.filterTier))
					{
						this.allEntries.Add(new XUiC_PoiList.PoiListEntry(prefabInstance));
					}
				}
			}
		}
		this.allEntries.Sort();
		base.RebuildList(_resetFilter);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		if (!this.openedBefore || this.allEntries.Count == 0)
		{
			this.RebuildList(false);
		}
		this.openedBefore = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int SmallPoiVolumeLimit = 100;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool filterSmallPois = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int filterTier = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool openedBefore;

	[Preserve]
	public class PoiListEntry : XUiListEntry<XUiC_PoiList.PoiListEntry>
	{
		public PoiListEntry(PrefabInstance _prefabInstance)
		{
			this.prefabInstance = _prefabInstance;
		}

		public override int CompareTo(XUiC_PoiList.PoiListEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return 1;
			}
			int num = string.Compare(this.prefabInstance.name, _otherEntry.prefabInstance.name, StringComparison.OrdinalIgnoreCase);
			if (num != 0)
			{
				return num;
			}
			num = this.prefabInstance.boundingBoxPosition.x - _otherEntry.prefabInstance.boundingBoxPosition.x;
			if (num != 0)
			{
				return num;
			}
			return this.prefabInstance.boundingBoxPosition.z - _otherEntry.prefabInstance.boundingBoxPosition.z;
		}

		public override bool MatchesSearch(string _searchString)
		{
			Prefab prefab = this.prefabInstance.prefab;
			if (prefab.PrefabName.ContainsCaseInsensitive(_searchString) || prefab.LocalizedName.ContainsCaseInsensitive(_searchString))
			{
				return true;
			}
			Vector3i boundingBoxPosition = this.prefabInstance.boundingBoxPosition;
			BiomeDefinition biomeInWorld = GameManager.Instance.World.GetBiomeInWorld(boundingBoxPosition.x, boundingBoxPosition.z);
			return biomeInWorld != null && biomeInWorld.LocalizedName.ContainsCaseInsensitive(_searchString);
		}

		public readonly PrefabInstance prefabInstance;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class EntryController : XUiC_List<XUiC_PoiList.PoiListEntry>.XUiC_ListEntry
	{
		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_PoiList.PoiListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.prefabInstance.prefab.PrefabName : null) ?? "";
		}

		[XuiXmlBinding("localizedname")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingLocalizedName()
		{
			XUiC_PoiList.PoiListEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.prefabInstance.prefab.LocalizedName : null) ?? "";
		}

		[XuiXmlBinding("coords")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingCoords()
		{
			if (this.entryData == null)
			{
				return "";
			}
			Vector3i boundingBoxPosition = this.entryData.prefabInstance.boundingBoxPosition;
			BiomeDefinition biomeInWorld = GameManager.Instance.World.GetBiomeInWorld(boundingBoxPosition.x, boundingBoxPosition.z);
			return "(" + ValueDisplayFormatters.WorldPos(boundingBoxPosition.ToVector3(), " ", false) + ")\n" + ((biomeInWorld != null) ? biomeInWorld.LocalizedName : null);
		}
	}
}
