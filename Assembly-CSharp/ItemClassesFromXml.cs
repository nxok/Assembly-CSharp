using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;
using XMLData.Item;

public class ItemClassesFromXml
{
	public static void CreateItemsFromBlocks()
	{
		for (int i = 1; i < Block.ItemsStartHere; i++)
		{
			if (Block.list[i] != null)
			{
				ItemClassBlock itemClassBlock = new ItemClassBlock();
				itemClassBlock.SetId(Block.list[i].blockID);
				itemClassBlock.SetName(Block.list[i].GetBlockName());
				itemClassBlock.Stacknumber = new DataItem<int>(Block.list[i].Stacknumber);
				ItemClass.list[itemClassBlock.Id] = itemClassBlock;
				itemClassBlock.Init();
			}
		}
	}

	public static IEnumerator CreateItems(XmlFile _xmlFile)
	{
		XElement root = _xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <items> found!");
		}
		MicroStopwatch msw = new MicroStopwatch(true);
		foreach (XElement xelement in root.Elements())
		{
			if (xelement.Name == "item")
			{
				ItemClassesFromXml.parseItem(xelement);
			}
			else if (xelement.Name == "animation")
			{
				ItemClassesFromXml.parseAnimation(xelement);
			}
			else if (xelement.Name == "noise")
			{
				ItemClassesFromXml.parseNoise(xelement);
			}
			if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		IEnumerator<XElement> enumerator = null;
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseItem(XElement _node)
	{
		DynamicProperties dynamicProperties = new DynamicProperties();
		string attribute = _node.GetAttribute("name");
		if (attribute.Length == 0)
		{
			throw new Exception("Attribute 'name' missing on item");
		}
		RequirementGroup[] array = new RequirementGroup[3];
		foreach (XElement xelement in _node.Elements("property"))
		{
			dynamicProperties.Add(xelement, false);
			string attribute2 = xelement.GetAttribute("class");
			if (attribute2.StartsWith("Action"))
			{
				int num = (int)(attribute2[attribute2.Length - 1] - '0');
				array[num] = RequirementBase.ParseRequirementGroup(xelement);
			}
		}
		if (dynamicProperties.Values.ContainsKey("Extends"))
		{
			string text = dynamicProperties.Values["Extends"];
			ItemClass itemClass = ItemClass.GetItemClass(text, false);
			if (itemClass == null)
			{
				throw new Exception(string.Format("Extends item {0} is not specified for item {1}'", text, attribute));
			}
			HashSet<string> hashSet = new HashSet<string> { Block.PropCreativeMode };
			if (dynamicProperties.Params1.ContainsKey("Extends"))
			{
				foreach (string text2 in dynamicProperties.Params1["Extends"].Split(',', StringSplitOptions.None))
				{
					hashSet.Add(text2.Trim());
				}
			}
			DynamicProperties dynamicProperties2 = new DynamicProperties();
			dynamicProperties2.CopyFrom(itemClass.Properties, hashSet);
			dynamicProperties2.CopyFrom(dynamicProperties, null);
			dynamicProperties = dynamicProperties2;
		}
		ItemClass itemClass2;
		if (dynamicProperties.Values.ContainsKey("Class"))
		{
			string text3 = dynamicProperties.Values["Class"];
			try
			{
				itemClass2 = (ItemClass)Activator.CreateInstance(Type.GetType(text3));
				goto IL_01DB;
			}
			catch (Exception)
			{
				throw new Exception("No item class '" + text3 + " found!");
			}
		}
		itemClass2 = new ItemClass();
		IL_01DB:
		itemClass2.Properties = dynamicProperties;
		if (dynamicProperties.Params1.ContainsKey("Extends"))
		{
			string text4 = dynamicProperties.Values["Extends"];
			if (ItemClass.GetItemClass(text4, false) == null)
			{
				throw new Exception(string.Format("Extends item {0} is not specified for item {1}'", text4, attribute));
			}
		}
		itemClass2.Effects = MinEffectController.ParseXml(_node, null, MinEffectController.SourceParentType.ItemClass, itemClass2.Id);
		itemClass2.GSStatsParseXml(_node);
		itemClass2.SetName(attribute);
		itemClass2.setLocalizedItemName(Localization.Get(attribute, false, null));
		if (dynamicProperties.Values.ContainsKey("Stacknumber"))
		{
			itemClass2.Stacknumber = new DataItem<int>(int.Parse(dynamicProperties.Values["Stacknumber"]));
		}
		else
		{
			itemClass2.Stacknumber = new DataItem<int>(500);
		}
		if (dynamicProperties.Values.ContainsKey("Canhold"))
		{
			itemClass2.SetCanHold(StringParsers.ParseBool(dynamicProperties.Values["Canhold"], 0, -1, true));
		}
		if (dynamicProperties.Values.ContainsKey("Candrop"))
		{
			itemClass2.SetCanDrop(StringParsers.ParseBool(dynamicProperties.Values["Candrop"], 0, -1, true));
		}
		if (!dynamicProperties.Values.ContainsKey("Material"))
		{
			throw new Exception("Attribute 'material' missing on item '" + attribute + "'");
		}
		itemClass2.MadeOfMaterial = MaterialBlock.fromString(dynamicProperties.Values["Material"]);
		if (itemClass2.MadeOfMaterial == null)
		{
			throw new Exception(string.Concat(new string[]
			{
				"Attribute 'material' '",
				dynamicProperties.Values["Material"],
				"' refers to not existing material in item '",
				attribute,
				"'"
			}));
		}
		if (!dynamicProperties.Values.ContainsKey("Meshfile") && itemClass2.CanHold())
		{
			throw new Exception("Attribute 'Meshfile' missing on item '" + attribute + "'");
		}
		itemClass2.MeshFile = dynamicProperties.GetValue("Meshfile");
		DataLoader.PreloadBundle(itemClass2.MeshFile);
		StringParsers.TryParseFloat(dynamicProperties.GetValue("StickyOffset"), out itemClass2.StickyOffset);
		StringParsers.TryParseFloat(dynamicProperties.GetValue("StickyColliderRadius"), out itemClass2.StickyColliderRadius);
		StringParsers.TryParseSInt32(dynamicProperties.GetValue("StickyColliderUp"), out itemClass2.StickyColliderUp);
		StringParsers.TryParseFloat(dynamicProperties.GetValue("StickyColliderLength"), out itemClass2.StickyColliderLength);
		itemClass2.StickyMaterial = dynamicProperties.GetValue("StickyMaterial");
		if (dynamicProperties.Values.ContainsKey("ImageEffectOnActive"))
		{
			itemClass2.ImageEffectOnActive = new DataItem<string>(dynamicProperties.Values["ImageEffectOnActive"]);
		}
		if (dynamicProperties.Values.ContainsKey("Active"))
		{
			itemClass2.Active = new DataItem<bool>(false);
		}
		if (dynamicProperties.Values.ContainsKey(ItemClass.PropIsSticky))
		{
			itemClass2.IsSticky = StringParsers.ParseBool(dynamicProperties.Values[ItemClass.PropIsSticky], 0, -1, true);
		}
		if (dynamicProperties.Values.ContainsKey("DropMeshfile") && itemClass2.CanHold())
		{
			itemClass2.DropMeshFile = dynamicProperties.Values["DropMeshfile"];
			DataLoader.PreloadBundle(itemClass2.DropMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HandMeshfile") && itemClass2.CanHold())
		{
			itemClass2.HandMeshFile = dynamicProperties.Values["HandMeshfile"];
			DataLoader.PreloadBundle(itemClass2.HandMeshFile);
		}
		if (dynamicProperties.Values.ContainsKey("HoldType"))
		{
			string text5 = dynamicProperties.Values["HoldType"];
			int num2 = 0;
			if (!int.TryParse(text5, out num2))
			{
				throw new Exception("Cannot parse attribute hold_type for item '" + attribute + "'");
			}
			itemClass2.HoldType = new DataItem<int>(num2);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTools"))
		{
			string[] array3 = dynamicProperties.Values["RepairTools"].Replace(" ", "").Split(',', StringSplitOptions.None);
			DataItem<string>[] array4 = new DataItem<string>[array3.Length];
			for (int j = 0; j < array3.Length; j++)
			{
				array4[j] = new DataItem<string>(array3[j]);
			}
			itemClass2.RepairTools = new ItemData.DataItemArrayRepairTools(array4);
		}
		if (dynamicProperties.Values.ContainsKey("RepairAmount"))
		{
			int num3 = 0;
			int.TryParse(dynamicProperties.Values["RepairAmount"], out num3);
			itemClass2.RepairAmount = new DataItem<int>(num3);
		}
		if (dynamicProperties.Values.ContainsKey("RepairTime"))
		{
			float num4 = 0f;
			StringParsers.TryParseFloat(dynamicProperties.Values["RepairTime"], out num4);
			itemClass2.RepairTime = new DataItem<float>(num4);
		}
		else if (itemClass2.RepairAmount != null)
		{
			itemClass2.RepairTime = new DataItem<float>(1f);
		}
		if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			itemClass2.MaxUseTimes = new DataItem<int>(int.Parse(dynamicProperties.Values["Degradation"]));
		}
		else
		{
			itemClass2.MaxUseTimes = new DataItem<int>(0);
			itemClass2.MaxUseTimesBreaksAfter = new DataItem<bool>(false);
		}
		if (dynamicProperties.Values.ContainsKey("DegradationBreaksAfter"))
		{
			itemClass2.MaxUseTimesBreaksAfter = new DataItem<bool>(StringParsers.ParseBool(dynamicProperties.Values["DegradationBreaksAfter"], 0, -1, true));
		}
		else if (dynamicProperties.Values.ContainsKey("Degradation"))
		{
			itemClass2.MaxUseTimesBreaksAfter = new DataItem<bool>(true);
		}
		if (dynamicProperties.Values.ContainsKey("EconomicValue"))
		{
			itemClass2.EconomicValue = StringParsers.ParseFloat(dynamicProperties.Values["EconomicValue"], 0, -1, NumberStyles.Any);
		}
		if (dynamicProperties.Classes.ContainsKey("Preview"))
		{
			DynamicProperties dynamicProperties3 = dynamicProperties.Classes["Preview"];
			itemClass2.Preview = new PreviewData();
			if (dynamicProperties3.Values.ContainsKey("Zoom"))
			{
				itemClass2.Preview.Zoom = new DataItem<int>(int.Parse(dynamicProperties3.Values["Zoom"]));
			}
			if (dynamicProperties3.Values.ContainsKey("Pos"))
			{
				itemClass2.Preview.Pos = new DataItem<Vector2>(StringParsers.ParseVector2(dynamicProperties3.Values["Pos"]));
			}
			else
			{
				itemClass2.Preview.Pos = new DataItem<Vector2>(Vector2.zero);
			}
			if (dynamicProperties3.Values.ContainsKey("Rot"))
			{
				itemClass2.Preview.Rot = new DataItem<Vector3>(StringParsers.ParseVector3(dynamicProperties3.Values["Rot"], 0, -1));
			}
			else
			{
				itemClass2.Preview.Rot = new DataItem<Vector3>(Vector3.zero);
			}
		}
		for (int k = 0; k < itemClass2.Actions.Length; k++)
		{
			string text6 = ItemClass.itemActionNames[k];
			if (dynamicProperties.Classes.ContainsKey(text6))
			{
				if (dynamicProperties.Contains(text6, "Class"))
				{
					string @string = dynamicProperties.GetString(text6, "Class");
					ItemAction itemAction;
					try
					{
						itemAction = (ItemAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("ItemAction", @string));
						goto IL_0940;
					}
					catch (Exception)
					{
						throw new Exception("ItemAction class '" + @string + " could not be instantiated");
					}
					goto IL_090E;
					IL_0940:
					itemAction.item = itemClass2;
					itemAction.ActionIndex = k;
					itemAction.ReadFrom(dynamicProperties.Classes[text6]);
					if (array[k] != null)
					{
						itemAction.ExecutionRequirements = array[k];
					}
					itemClass2.Actions[k] = itemAction;
					goto IL_0983;
				}
				IL_090E:
				throw new Exception(string.Concat(new string[] { "No class attribute found on ", text6, " in item with '", attribute, "'" }));
			}
			IL_0983:;
		}
		itemClass2.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseAnimation(XElement _node)
	{
		foreach (XElement xelement in _node.Elements("hold_type"))
		{
			string attribute = xelement.GetAttribute("id");
			if (attribute.Length == 0)
			{
				throw new Exception("Attribute 'id' missing in hold");
			}
			int num;
			if (!int.TryParse(attribute, out num))
			{
				throw new Exception("Invalid hold id");
			}
			int num2 = 0;
			string attribute2 = xelement.GetAttribute("carry");
			if (attribute2.Length > 0 && !int.TryParse(attribute2, out num2))
			{
				throw new Exception("Invalid hold carry");
			}
			float num3 = 0f;
			if (xelement.HasAttribute("ray_cast"))
			{
				num3 = StringParsers.ParseFloat(xelement.GetAttribute("ray_cast"), 0, -1, NumberStyles.Any);
			}
			float num4 = num3;
			if (xelement.HasAttribute("ray_cast_moving"))
			{
				num3 = StringParsers.ParseFloat(xelement.GetAttribute("ray_cast_moving"), 0, -1, NumberStyles.Any);
			}
			float num5 = Constants.cMinHolsterTime;
			if (xelement.HasAttribute("holster"))
			{
				num5 = Utils.FastMax(StringParsers.ParseFloat(xelement.GetAttribute("holster"), 0, -1, NumberStyles.Any), num5);
			}
			float num6 = Constants.cMinUnHolsterTime;
			if (xelement.HasAttribute("unholster"))
			{
				num6 = Utils.FastMax(StringParsers.ParseFloat(xelement.GetAttribute("unholster"), 0, -1, NumberStyles.Any), num6);
			}
			bool flag = false;
			if (xelement.HasAttribute("two_handed"))
			{
				flag = StringParsers.ParseBool(xelement.GetAttribute("two_handed"), 0, -1, true);
			}
			AnimationDelayData.AnimationDelay[num] = new AnimationDelayData.AnimationDelays(num2, num3, num4, num5, num6, flag);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void parseNoise(XElement _node)
	{
		foreach (XElement xelement in _node.Elements("sound"))
		{
			if (!xelement.HasAttribute("name"))
			{
				throw new Exception("Attribute 'name' missing in noise/sound");
			}
			string attribute = xelement.GetAttribute("name");
			float num = 0f;
			if (xelement.HasAttribute("volume"))
			{
				num = StringParsers.ParseFloat(xelement.GetAttribute("volume"), 0, -1, NumberStyles.Any);
			}
			if (!xelement.HasAttribute("time"))
			{
				throw new Exception("Attribute 'time' missing in noise/sound name='" + attribute + "'");
			}
			float num2 = StringParsers.ParseFloat(xelement.GetAttribute("time"), 0, -1, NumberStyles.Any);
			float num3 = 1f;
			if (xelement.HasAttribute("muffled_when_crouched"))
			{
				num3 = StringParsers.ParseFloat(xelement.GetAttribute("muffled_when_crouched"), 0, -1, NumberStyles.Any);
			}
			float num4 = 0f;
			if (xelement.HasAttribute("heat_map_strength"))
			{
				num4 = StringParsers.ParseFloat(xelement.GetAttribute("heat_map_strength"), 0, -1, NumberStyles.Any);
			}
			float num5 = 100f;
			if (xelement.HasAttribute("heat_map_time"))
			{
				num5 = StringParsers.ParseFloat(xelement.GetAttribute("heat_map_time"), 0, -1, NumberStyles.Any);
			}
			num5 *= 10f;
			AIDirectorData.AddNoisySound(attribute, new AIDirectorData.Noise(attribute, num, num2, num3, num4, (ulong)num5));
		}
	}
}
