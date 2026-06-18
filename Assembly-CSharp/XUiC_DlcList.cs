using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_DlcList : XUiC_List<XUiC_DlcList.DlcEntry>
{
	[XuiXmlBinding("dlc_count")]
	public int DlcCount
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.allEntries.Count;
		}
	}

	[XuiXmlBinding("paging_active")]
	public bool PagingRequired
	{
		[PublicizedFrom(EAccessModifier.Private)]
		get
		{
			return this.allEntries.Count > base.PageLength;
		}
	}

	public override void RebuildList(bool _resetFilter = false)
	{
		this.allEntries.Clear();
		foreach (XUiC_DlcList.DlcEntry dlcEntry in XUiC_DlcList.DlcEntries)
		{
			if (EntitlementManager.Instance.IsAvailableOnPlatform(dlcEntry.DlcSet) && EntitlementManager.Instance.IsEntitlementPurchasable(dlcEntry.DlcSet))
			{
				this.allEntries.Add(dlcEntry);
			}
		}
		base.RebuildList(_resetFilter);
	}

	public override void OnOpen()
	{
		base.OnOpen();
		this.RebuildList(false);
	}

	public static readonly List<XUiC_DlcList.DlcEntry> DlcEntries = new List<XUiC_DlcList.DlcEntry>
	{
		new XUiC_DlcList.DlcEntry("armorClassicSurvivorSet", "Data/DlcInfo/banner_classicsurvivor_outfit", EntitlementSetEnum.ClassicSurvivorCosmetic),
		new XUiC_DlcList.DlcEntry("armorHolidayHatsSet", "Data/DlcInfo/banner_holidayhats", EntitlementSetEnum.ChristmasCosmetics),
		new XUiC_DlcList.DlcEntry("armorButcherSet", "Data/DlcInfo/banner_butcher_outfit", EntitlementSetEnum.ButcherCosmetic),
		new XUiC_DlcList.DlcEntry("armorHellreaverSet", "Data/DlcInfo/banner_hellreaver_outfit", EntitlementSetEnum.HellreaverCosmetic),
		new XUiC_DlcList.DlcEntry("armorPirateSet", "Data/DlcInfo/banner_pirate_outfit", EntitlementSetEnum.PirateCosmetic),
		new XUiC_DlcList.DlcEntry("armorHoarderSet", "Data/DlcInfo/banner_hoarder_outfit", EntitlementSetEnum.HoarderCosmetic),
		new XUiC_DlcList.DlcEntry("armorMarauderSet", "Data/DlcInfo/banner_marauder_outfit", EntitlementSetEnum.MarauderCosmetic),
		new XUiC_DlcList.DlcEntry("armorDesertSet", "Data/DlcInfo/banner_desert_outfit", EntitlementSetEnum.DesertCosmetic)
	};

	[Preserve]
	public class DlcEntry : XUiListEntry<XUiC_DlcList.DlcEntry>
	{
		public DlcEntry(string _nameKey, string _imageResourceUri, EntitlementSetEnum _dlcSet)
		{
			this.Name = Localization.Get(_nameKey, false, null);
			this.ImageResourceUri = _imageResourceUri;
			this.DlcSet = _dlcSet;
		}

		public override int CompareTo(XUiC_DlcList.DlcEntry _otherEntry)
		{
			if (_otherEntry == null)
			{
				return -1;
			}
			return string.Compare(this.Name, _otherEntry.Name, StringComparison.OrdinalIgnoreCase);
		}

		public readonly string Name;

		public readonly string ImageResourceUri;

		public readonly EntitlementSetEnum DlcSet;
	}

	[Preserve]
	[PublicizedFrom(EAccessModifier.Protected)]
	public class DlcListEntryController : XUiC_List<XUiC_DlcList.DlcEntry>.XUiC_ListEntry
	{
		public override void Init()
		{
			base.Init();
			XUiController childById = base.GetChildById("dlcImage");
			XUiV_Texture xuiV_Texture = ((childById != null) ? childById.ViewComponent : null) as XUiV_Texture;
			if (xuiV_Texture != null)
			{
				this.textureScaler = xuiV_Texture.UiTransform.gameObject.AddComponent<TweenScale>();
				this.textureScaler.enabled = false;
				this.textureScaler.from = Vector3.one;
				this.textureScaler.to = Vector3.one * 1.15f;
				this.textureScaler.duration = 0.25f;
				this.textureScaler.ResetToBeginning();
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnHovered(bool _isOver)
		{
			base.OnHovered(_isOver);
			if (this.textureScaler == null)
			{
				return;
			}
			if (_isOver)
			{
				this.textureScaler.PlayForward();
				return;
			}
			this.textureScaler.PlayReverse();
		}

		[XuiXmlBinding("name")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingName()
		{
			XUiC_DlcList.DlcEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.Name : null) ?? "";
		}

		[XuiXmlBinding("texture_uri")]
		[PublicizedFrom(EAccessModifier.Private)]
		public string bindingTextureUri()
		{
			XUiC_DlcList.DlcEntry entryData = this.entryData;
			return ((entryData != null) ? entryData.ImageResourceUri : null) ?? "";
		}

		[XuiXmlBinding("owned")]
		[PublicizedFrom(EAccessModifier.Private)]
		public bool bindingOwned()
		{
			return this.entryData != null && EntitlementManager.Instance.HasEntitlement(this.entryData.DlcSet);
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public const float HoverDuration = 0.25f;

		[PublicizedFrom(EAccessModifier.Private)]
		public const float HoverScale = 1.15f;

		[PublicizedFrom(EAccessModifier.Private)]
		public TweenScale textureScaler;
	}
}
