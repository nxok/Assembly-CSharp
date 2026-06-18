using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;
using XMLData.Item;

public class ItemModificationsFromXml
{
	public static IEnumerator Load(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <item_modifiers> found!");
		}
		ItemModificationsFromXml.ParseNode(root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement root)
	{
		foreach (XElement xelement in root.Elements("item_modifier"))
		{
			ItemModificationsFromXml.ParseModifier(xelement);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseModifier(XElement _element)
	{
		ItemClassModifier itemClassModifier = new ItemClassModifier();
		itemClassModifier.Groups = new string[] { "Mods" };
		if (_element.HasAttribute("installable_tags"))
		{
			itemClassModifier.InstallableTags = FastTags<TagGroup.Global>.Parse(_element.GetAttribute("installable_tags"));
		}
		if (_element.HasAttribute("blocked_tags"))
		{
			itemClassModifier.DisallowedTags = FastTags<TagGroup.Global>.Parse(_element.GetAttribute("blocked_tags"));
		}
		if (_element.HasAttribute("modifier_tags"))
		{
			itemClassModifier.ModifierTags = FastTags<TagGroup.Global>.Parse(_element.GetAttribute("modifier_tags"));
		}
		if (_element.HasAttribute("type"))
		{
			itemClassModifier.Type = EnumUtils.Parse<ItemClassModifier.ModifierTypes>(_element.GetAttribute("type"), true);
		}
		ItemModificationsFromXml.parseItem(_element, itemClassModifier);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseItem(XElement elementItem, ItemClassModifier item)
	{
		string text = string.Empty;
		DynamicProperties dynamicProperties = new DynamicProperties();
		if (!elementItem.HasAttribute("name"))
		{
			throw new Exception("Attribute 'name' missing on item");
		}
		text = elementItem.GetAttribute("name");
		item.CosmeticInstallChance = 1f;
		if (elementItem.HasAttribute("cosmetic_install_chance"))
		{
			item.CosmeticInstallChance = StringParsers.ParseFloat(elementItem.GetAttribute("cosmetic_install_chance"), 0, -1, NumberStyles.Any);
		}
		item.PropertyOverrides = new Dictionary<string, DynamicProperties>();
		RequirementGroup[] array = new RequirementGroup[3];
		foreach (XElement xelement in elementItem.Elements())
		{
			if (xelement.Name == XNames.property)
			{
				dynamicProperties.Add(xelement, false);
				string attribute = xelement.GetAttribute(XNames.class_);
				if (attribute.StartsWith("Action"))
				{
					int num = (int)(attribute[attribute.Length - 1] - '0');
					array[num] = RequirementBase.ParseRequirementGroup(xelement);
				}
			}
			else if (xelement.Name == "item_property_overrides" && xelement.HasAttribute(XNames.name))
			{
				string attribute2 = xelement.GetAttribute(XNames.name);
				DynamicProperties dynamicProperties2 = new DynamicProperties();
				foreach (XElement xelement2 in xelement.Elements(XNames.property))
				{
					dynamicProperties2.Add(xelement2, false);
				}
				if (dynamicProperties2.Values.Count > 0)
				{
					item.PropertyOverrides[attribute2] = dynamicProperties2;
				}
			}
		}
		if (dynamicProperties.Values.ContainsKey("Extends"))
		{
			string text2 = dynamicProperties.Values["Extends"];
			ItemClass itemClass = ItemClass.GetItemClass(text2, false);
			if (itemClass == null)
			{
				throw new Exception(string.Format("Extends item {0} is not specified for item {1}'", text2, text));
			}
			HashSet<string> hashSet = new HashSet<string> { Block.PropCreativeMode };
			if (dynamicProperties.Params1.ContainsKey("Extends"))
			{
				foreach (string text3 in dynamicProperties.Params1["Extends"].Split(',', StringSplitOptions.None))
				{
					hashSet.Add(text3.Trim());
				}
			}
			DynamicProperties dynamicProperties3 = new DynamicProperties();
			dynamicProperties3.CopyFrom(itemClass.Properties, hashSet);
			dynamicProperties3.CopyFrom(dynamicProperties, null);
			dynamicProperties = dynamicProperties3;
		}
		item.Properties = dynamicProperties;
		item.Effects = MinEffectController.ParseXml(elementItem, null, MinEffectController.SourceParentType.ItemModifierClass, item.Id);
		item.SetName(text);
		item.setLocalizedItemName(Localization.Get(text, false, null));
		if (dynamicProperties.Values.ContainsKey("Stacknumber"))
		{
			item.Stacknumber = new DataItem<int>(int.Parse(dynamicProperties.Values["Stacknumber"]));
		}
		else
		{
			item.Stacknumber = new DataItem<int>(500);
		}
		if (dynamicProperties.Values.ContainsKey("Canhold"))
		{
			item.SetCanHold(StringParsers.ParseBool(dynamicProperties.Values["Canhold"], 0, -1, true));
		}
		if (dynamicProperties.Values.ContainsKey("Candrop"))
		{
			item.SetCanDrop(StringParsers.ParseBool(dynamicProperties.Values["Candrop"], 0, -1, true));
		}
		if (dynamicProperties.Values.ContainsKey("Material"))
		{
			item.MadeOfMaterial = MaterialBlock.fromString(dynamicProperties.Values["Material"]);
		}
		else
		{
			item.MadeOfMaterial = MaterialBlock.fromString("Miron");
		}
		if (dynamicProperties.Values.ContainsKey("Meshfile") && item.CanHold())
		{
			item.MeshFile = dynamicProperties.Values["Meshfile"];
			DataLoader.PreloadBundle(item.MeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("StickyOffset"))
		{
			StringParsers.TryParseFloat(dynamicProperties.Values["StickyOffset"], out item.StickyOffset);
		}
		if (dynamicProperties.Values.ContainsKey("ImageEffectOnActive"))
		{
			item.ImageEffectOnActive = new DataItem<string>(dynamicProperties.Values["ImageEffectOnActive"]);
		}
		if (dynamicProperties.Values.ContainsKey("Active"))
		{
			item.Active = new DataItem<bool>(false);
		}
		if (dynamicProperties.Values.ContainsKey("DropMeshfile") && item.CanHold())
		{
			item.DropMeshFile = dynamicProperties.Values["DropMeshfile"];
			DataLoader.PreloadBundle(item.DropMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HandMeshfile") && item.CanHold())
		{
			item.HandMeshFile = dynamicProperties.Values["HandMeshfile"];
			DataLoader.PreloadBundle(item.HandMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HoldType"))
		{
			string text4 = dynamicProperties.Values["HoldType"];
			int num2 = 0;
			if (!int.TryParse(text4, out num2))
			{
				throw new Exception("Cannot parse attribute hold_type for item '" + text + "'");
			}
			item.HoldType = new DataItem<int>(num2);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTools"))
		{
			string[] array3 = dynamicProperties.Values["RepairTools"].Replace(" ", "").Split(',', StringSplitOptions.None);
			DataItem<string>[] array4 = new DataItem<string>[array3.Length];
			for (int j = 0; j < array3.Length; j++)
			{
				array4[j] = new DataItem<string>(array3[j]);
			}
			item.RepairTools = new ItemData.DataItemArrayRepairTools(array4);
		}
		if (dynamicProperties.Values.ContainsKey("RepairAmount"))
		{
			int num3 = 0;
			int.TryParse(dynamicProperties.Values["RepairAmount"], out num3);
			item.RepairAmount = new DataItem<int>(num3);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTime"))
		{
			float num4 = 0f;
			StringParsers.TryParseFloat(dynamicProperties.Values["RepairTime"], out num4);
			item.RepairTime = new DataItem<float>(num4);
		}
		else if (item.RepairAmount != null)
		{
			item.RepairTime = new DataItem<float>(1f);
		}
		if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			item.MaxUseTimes = new DataItem<int>(int.Parse(dynamicProperties.Values["Degradation"]));
		}
		else
		{
			item.MaxUseTimes = new DataItem<int>(0);
			item.MaxUseTimesBreaksAfter = new DataItem<bool>(false);
		}
		if (dynamicProperties.Values.ContainsKey("DegradationBreaksAfter"))
		{
			item.MaxUseTimesBreaksAfter = new DataItem<bool>(StringParsers.ParseBool(dynamicProperties.Values["DegradationBreaksAfter"], 0, -1, true));
		}
		else if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			item.MaxUseTimesBreaksAfter = new DataItem<bool>(true);
		}
		if (dynamicProperties.Values.ContainsKey("EconomicValue"))
		{
			item.EconomicValue = StringParsers.ParseFloat(dynamicProperties.Values["EconomicValue"], 0, -1, NumberStyles.Any);
		}
		if (dynamicProperties.Classes.ContainsKey("Preview"))
		{
			DynamicProperties dynamicProperties4 = dynamicProperties.Classes["Preview"];
			item.Preview = new PreviewData();
			if (dynamicProperties4.Values.ContainsKey("Zoom"))
			{
				item.Preview.Zoom = new DataItem<int>(int.Parse(dynamicProperties4.Values["Zoom"]));
			}
			if (dynamicProperties4.Values.ContainsKey("Pos"))
			{
				item.Preview.Pos = new DataItem<Vector2>(StringParsers.ParseVector2(dynamicProperties4.Values["Pos"]));
			}
			else
			{
				item.Preview.Pos = new DataItem<Vector2>(Vector2.zero);
			}
			if (dynamicProperties4.Values.ContainsKey("Rot"))
			{
				item.Preview.Rot = new DataItem<Vector3>(StringParsers.ParseVector3(dynamicProperties4.Values["Rot"], 0, -1));
			}
			else
			{
				item.Preview.Rot = new DataItem<Vector3>(Vector3.zero);
			}
		}
		for (int k = 0; k < item.Actions.Length; k++)
		{
			string text5 = ItemClass.itemActionNames[k];
			if (dynamicProperties.Classes.ContainsKey(text5))
			{
				ItemAction itemAction = null;
				if (dynamicProperties.Contains(text5, "Class"))
				{
					string @string = dynamicProperties.GetString(text5, "Class");
					try
					{
						itemAction = (ItemAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("ItemAction", @string));
						goto IL_08A8;
					}
					catch (Exception)
					{
						throw new Exception("ItemAction class '" + @string + " could not be instantiated");
					}
					goto IL_0876;
					IL_08A8:
					itemAction.item = item;
					itemAction.ReadFrom(dynamicProperties.Classes[text5]);
					if (array[k] != null)
					{
						itemAction.ExecutionRequirements = array[k];
					}
					item.Actions[k] = itemAction;
					goto IL_08E0;
				}
				IL_0876:
				throw new Exception(string.Concat(new string[] { "No class attribute found on ", text5, " in item with '", text, "'" }));
			}
			IL_08E0:;
		}
		item.Init();
	}
}
