using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class ItemClassModifier : ItemClass
{
	public static ItemClassModifier GetItemModWithAnyTags(FastTags<TagGroup.Global> tags, FastTags<TagGroup.Global> installedModTypes, GameRandom random)
	{
		for (int i = 0; i < ItemClass.list.Length; i++)
		{
			ItemClassModifier itemClassModifier = ItemClass.list[i] as ItemClassModifier;
			if (itemClassModifier != null && !itemClassModifier.HasAnyTags(installedModTypes) && itemClassModifier.InstallableTags.Test_AnySet(tags) && !itemClassModifier.DisallowedTags.Test_AnySet(tags))
			{
				ItemClassModifier.modIds.Add(itemClassModifier.Id);
			}
		}
		if (ItemClassModifier.modIds.Count == 0)
		{
			return null;
		}
		ItemClassModifier itemClassModifier2 = ItemClass.GetForId(ItemClassModifier.modIds[random.RandomRange(ItemClassModifier.modIds.Count)]) as ItemClassModifier;
		ItemClassModifier.modIds.Clear();
		return itemClassModifier2;
	}

	public override bool HasAnyTags(FastTags<TagGroup.Global> _tags)
	{
		return this.ModifierTags.Test_AnySet(_tags);
	}

	public override bool HasAllTags(FastTags<TagGroup.Global> _tags)
	{
		return this.ModifierTags.Test_AllSet(_tags);
	}

	public static ItemClassModifier GetCosmeticItemMod(FastTags<TagGroup.Global> itemTags, FastTags<TagGroup.Global> installedModTypes, GameRandom random)
	{
		bool isEmpty = installedModTypes.IsEmpty;
		for (int i = 0; i < ItemClass.list.Length; i++)
		{
			ItemClassModifier itemClassModifier = ItemClass.list[i] as ItemClassModifier;
			if (itemClassModifier != null && (isEmpty || !itemClassModifier.HasAnyTags(installedModTypes)) && itemClassModifier.HasAnyTags(ItemClassModifier.CosmeticModTypes) && itemClassModifier.InstallableTags.Test_AnySet(itemTags) && !itemClassModifier.DisallowedTags.Test_AnySet(itemTags) && random.RandomFloat <= itemClassModifier.CosmeticInstallChance)
			{
				ItemClassModifier.modIds.Add(itemClassModifier.Id);
			}
		}
		if (ItemClassModifier.modIds.Count == 0)
		{
			return null;
		}
		ItemClassModifier itemClassModifier2 = ItemClass.GetForId(ItemClassModifier.modIds[random.RandomRange(ItemClassModifier.modIds.Count)]) as ItemClassModifier;
		ItemClassModifier.modIds.Clear();
		return itemClassModifier2;
	}

	public static ItemClassModifier GetDesiredItemModWithAnyTags(FastTags<TagGroup.Global> tags, FastTags<TagGroup.Global> installedModTypes, FastTags<TagGroup.Global> desiredModTypes, GameRandom random)
	{
		bool isEmpty = installedModTypes.IsEmpty;
		bool isEmpty2 = desiredModTypes.IsEmpty;
		for (int i = 0; i < ItemClass.list.Length; i++)
		{
			ItemClassModifier itemClassModifier = ItemClass.list[i] as ItemClassModifier;
			if (itemClassModifier != null && (isEmpty || !itemClassModifier.HasAnyTags(installedModTypes)) && (isEmpty2 || itemClassModifier.HasAnyTags(desiredModTypes)) && itemClassModifier.InstallableTags.Test_AnySet(tags) && !itemClassModifier.DisallowedTags.Test_AnySet(tags))
			{
				ItemClassModifier.modIds.Add(itemClassModifier.Id);
			}
		}
		if (ItemClassModifier.modIds.Count == 0)
		{
			return null;
		}
		ItemClassModifier itemClassModifier2 = ItemClass.GetForId(ItemClassModifier.modIds[random.RandomRange(ItemClassModifier.modIds.Count)]) as ItemClassModifier;
		ItemClassModifier.modIds.Clear();
		return itemClassModifier2;
	}

	public bool GetPropertyOverride(string _propertyName, string _itemName, ref string _value)
	{
		if (this.PropertyOverrides.ContainsKey(_itemName) && this.PropertyOverrides[_itemName].Values.ContainsKey(_propertyName))
		{
			_value = this.PropertyOverrides[_itemName].Values[_propertyName];
			return true;
		}
		if (this.PropertyOverrides.ContainsKey("*") && this.PropertyOverrides["*"].Values.ContainsKey(_propertyName))
		{
			_value = this.PropertyOverrides["*"].Values[_propertyName];
			return true;
		}
		return false;
	}

	public static ItemClassModifier[] modList = new ItemClassModifier[1000];

	public FastTags<TagGroup.Global> InstallableTags;

	public FastTags<TagGroup.Global> DisallowedTags;

	public FastTags<TagGroup.Global> ModifierTags;

	public ItemClassModifier.ModifierTypes Type;

	public Dictionary<string, DynamicProperties> PropertyOverrides;

	public float CosmeticInstallChance;

	public static FastTags<TagGroup.Global> CosmeticModTypes = FastTags<TagGroup.Global>.Parse("dye,nametag,charm");

	public static FastTags<TagGroup.Global> CosmeticItemTags = FastTags<TagGroup.Global>.Parse("canHaveCosmetic");

	[PublicizedFrom(EAccessModifier.Private)]
	public static List<int> modIds = new List<int>();

	public enum ModifierTypes
	{
		Mod,
		Attachment
	}
}
