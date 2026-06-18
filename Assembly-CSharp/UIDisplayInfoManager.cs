using System;
using System.Collections.Generic;

public class UIDisplayInfoManager
{
	public static UIDisplayInfoManager Current
	{
		get
		{
			if (UIDisplayInfoManager.instance == null)
			{
				UIDisplayInfoManager.instance = new UIDisplayInfoManager();
			}
			return UIDisplayInfoManager.instance;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public UIDisplayInfoManager()
	{
	}

	public static bool HasInstance
	{
		get
		{
			return UIDisplayInfoManager.instance != null;
		}
	}

	public static void Reset()
	{
		if (UIDisplayInfoManager.HasInstance)
		{
			UIDisplayInfoManager.Current.Cleanup();
		}
	}

	public bool ContainsItemDisplayStats(string tag)
	{
		return this.ItemDisplayStats.ContainsKey(tag);
	}

	public bool ContainsCraftingCategoryList(string tag)
	{
		return this.CraftingCategoryDisplayLists.ContainsKey(tag);
	}

	public ItemDisplayEntry GetDisplayStatsForTag(string tag)
	{
		if (this.ItemDisplayStats.ContainsKey(tag))
		{
			return this.ItemDisplayStats[tag];
		}
		return null;
	}

	public void AddItemDisplayStats(string tag, string group)
	{
		if (!this.ItemDisplayStats.ContainsKey(tag))
		{
			this.ItemDisplayStats.Add(tag, new ItemDisplayEntry
			{
				DisplayGroup = group
			});
		}
	}

	public void AddItemDisplayInfo(string tag, DisplayInfoEntry displayInfo)
	{
		this.ItemDisplayStats[tag].DisplayStats.Add(displayInfo);
		if (!this.StatLocalizationDictionary.ContainsKey(displayInfo.StatType))
		{
			this.StatLocalizationDictionary.Add(displayInfo.StatType, Localization.Get(displayInfo.StatType.ToStringCached<PassiveEffects>(), false, null));
		}
	}

	public void Cleanup()
	{
		this.ItemDisplayStats.Clear();
		this.CharacterDisplayStats.Clear();
		this.CraftingCategoryDisplayLists.Clear();
	}

	public string GetLocalizedName(PassiveEffects statType)
	{
		if (this.StatLocalizationDictionary.ContainsKey(statType))
		{
			return this.StatLocalizationDictionary[statType];
		}
		return "";
	}

	public void AddCharacterDisplayInfo(DisplayInfoEntry displayInfo)
	{
		this.CharacterDisplayStats.Add(displayInfo);
		if (!this.StatLocalizationDictionary.ContainsKey(displayInfo.StatType))
		{
			this.StatLocalizationDictionary.Add(displayInfo.StatType, Localization.Get(displayInfo.StatType.ToStringCached<PassiveEffects>(), false, null));
		}
	}

	public List<DisplayInfoEntry> GetCharacterDisplayInfo()
	{
		return this.CharacterDisplayStats;
	}

	public void AddCraftingCategoryDisplayItem(string categoryListName, CraftingCategoryDisplayEntry entry)
	{
		if (!this.CraftingCategoryDisplayLists.ContainsKey(categoryListName))
		{
			this.CraftingCategoryDisplayLists.Add(categoryListName, new List<CraftingCategoryDisplayEntry>());
		}
		this.CraftingCategoryDisplayLists[categoryListName].Add(entry);
	}

	public List<CraftingCategoryDisplayEntry> GetCraftingCategoryDisplayList(string categoryListName)
	{
		if (this.CraftingCategoryDisplayLists.ContainsKey(categoryListName))
		{
			return this.CraftingCategoryDisplayLists[categoryListName];
		}
		return null;
	}

	public void AddTraderCategoryDIsplayItem(CraftingCategoryDisplayEntry entry)
	{
		if (!this.TraderCategoryDisplayDict.ContainsKey(entry.Name))
		{
			this.TraderCategoryDisplayDict.Add(entry.Name, entry);
		}
	}

	public CraftingCategoryDisplayEntry GetTraderCategoryDisplay(string name)
	{
		if (this.TraderCategoryDisplayDict.ContainsKey(name))
		{
			return this.TraderCategoryDisplayDict[name];
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static UIDisplayInfoManager instance;

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, ItemDisplayEntry> ItemDisplayStats = new Dictionary<string, ItemDisplayEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public List<DisplayInfoEntry> CharacterDisplayStats = new List<DisplayInfoEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, List<CraftingCategoryDisplayEntry>> CraftingCategoryDisplayLists = new Dictionary<string, List<CraftingCategoryDisplayEntry>>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<string, CraftingCategoryDisplayEntry> TraderCategoryDisplayDict = new Dictionary<string, CraftingCategoryDisplayEntry>();

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<PassiveEffects, string> StatLocalizationDictionary = new EnumDictionary<PassiveEffects, string>();
}
