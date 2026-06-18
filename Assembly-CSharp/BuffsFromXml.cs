using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine;

public class BuffsFromXml
{
	public static IEnumerator CreateBuffs(XmlFile xmlFile)
	{
		BuffManager.Buffs = new CaseInsensitiveStringDictionary<BuffClass>();
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <buffs> found!");
		}
		MicroStopwatch msw = new MicroStopwatch(true);
		foreach (XElement xelement in root.Elements("buff"))
		{
			BuffsFromXml.ParseBuff(xelement);
			if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		IEnumerator<XElement> enumerator = null;
		BuffsFromXml.clearBuffValueLinks();
		yield break;
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void clearBuffValueLinks()
	{
		if (!GameManager.Instance)
		{
			return;
		}
		World world = GameManager.Instance.World;
		DictionaryList<int, Entity> dictionaryList = ((world != null) ? world.Entities : null);
		if (dictionaryList == null || dictionaryList.Count == 0)
		{
			return;
		}
		foreach (Entity entity in dictionaryList.list)
		{
			EntityAlive entityAlive = entity as EntityAlive;
			if (entityAlive != null)
			{
				entityAlive.Buffs.ClearBuffClassLinks();
			}
		}
	}

	public static void Reload(XmlFile xmlFile)
	{
		ThreadManager.RunCoroutineSync(BuffsFromXml.CreateBuffs(xmlFile));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseBuff(XElement _element)
	{
		BuffClass buffClass = new BuffClass("");
		string text = _element.GetAttribute("name");
		if (text.Length > 0)
		{
			buffClass.Name = text.ToLower();
			buffClass.NameTag = FastTags<TagGroup.Global>.Parse(text);
			text = _element.GetAttribute("name_key");
			if (text.Length > 0)
			{
				buffClass.LocalizedName = Localization.Get(text, false, null);
			}
			else
			{
				buffClass.LocalizedName = Localization.Get(buffClass.Name, false, null);
			}
			text = _element.GetAttribute("description_key");
			if (text.Length > 0)
			{
				buffClass.DescriptionKey = text;
				buffClass.Description = Localization.Get(text, false, null);
			}
			text = _element.GetAttribute("tooltip_key");
			if (text.Length > 0)
			{
				buffClass.TooltipKey = text;
				buffClass.Tooltip = Localization.Get(text, false, null);
			}
			text = _element.GetAttribute("icon");
			buffClass.Icon = ((text.Length > 0) ? text : null);
			text = _element.GetAttribute("hidden");
			buffClass.Hidden = text.Length > 0 && StringParsers.ParseBool(text, 0, -1, true);
			text = _element.GetAttribute("showonhud");
			buffClass.ShowOnHUD = text.Length == 0 || StringParsers.ParseBool(text, 0, -1, true);
			text = _element.GetAttribute("update_rate");
			buffClass.UpdateRateTicks = ((text.Length > 0) ? ((int)(StringParsers.ParseFloat(text, 0, -1, NumberStyles.Any) * 20f)) : 20);
			text = _element.GetAttribute("allow_in_editor");
			buffClass.AllowInEditor = text.Length > 0 && StringParsers.ParseBool(text, 0, -1, true);
			text = _element.GetAttribute("required_game_stat");
			buffClass.RequiredGameStat = ((text.Length > 0) ? Enum.Parse<EnumGameStats>(text) : EnumGameStats.Last);
			text = _element.GetAttribute("remove_on_death");
			buffClass.RemoveOnDeath = text.Length == 0 || StringParsers.ParseBool(text, 0, -1, true);
			text = _element.GetAttribute("display_type");
			buffClass.DisplayType = ((text.Length > 0) ? EnumUtils.Parse<EnumEntityUINotificationDisplayMode>(text, false) : EnumEntityUINotificationDisplayMode.IconOnly);
			text = _element.GetAttribute("icon_color");
			buffClass.IconColor = ((text.Length > 0) ? StringParsers.ParseColor32(text) : Color.white);
			text = _element.GetAttribute("icon_blink");
			buffClass.IconBlink = text.Length > 0 && StringParsers.ParseBool(text, 0, -1, true);
			buffClass.DamageSource = EnumDamageSource.Internal;
			buffClass.DamageType = EnumDamageTypes.None;
			buffClass.StackType = BuffEffectStackTypes.Replace;
			buffClass.DurationMax = 0f;
			foreach (XElement xelement in _element.Elements())
			{
				string localName = xelement.Name.LocalName;
				text = xelement.GetAttribute("value");
				if (localName == "display_value")
				{
					buffClass.DisplayValueCVar = ((text.Length > 0) ? text : null);
				}
				else if (localName == "display_value_key")
				{
					buffClass.DisplayValueKey = ((text.Length > 0) ? text : null);
				}
				else if (localName == "display_value_format")
				{
					if (text.Length > 0)
					{
						Enum.TryParse<BuffClass.CVarDisplayFormat>(text, true, out buffClass.DisplayValueFormat);
					}
				}
				else if (localName == "damage_source")
				{
					if (text.Length > 0)
					{
						buffClass.DamageSource = EnumUtils.Parse<EnumDamageSource>(text, true);
					}
				}
				else if (localName == "damage_type")
				{
					if (text.Length > 0)
					{
						buffClass.DamageType = EnumUtils.Parse<EnumDamageTypes>(text, true);
					}
				}
				else if (localName == "stack_type")
				{
					if (text.Length > 0)
					{
						buffClass.StackType = EnumUtils.Parse<BuffEffectStackTypes>(text, true);
					}
				}
				else if (localName == "tags")
				{
					if (text.Length > 0)
					{
						buffClass.Tags = FastTags<TagGroup.Global>.Parse(text);
					}
				}
				else if (localName == "duration")
				{
					if (text.Length > 0)
					{
						buffClass.DurationMax = StringParsers.ParseFloat(text, 0, -1, NumberStyles.Any);
					}
				}
				else if (localName == "update_rate")
				{
					buffClass.UpdateRateTicks = ((text.Length > 0) ? ((int)(StringParsers.ParseFloat(text, 0, -1, NumberStyles.Any) * 20f)) : 20);
				}
				else if (localName == "remove_on_death")
				{
					buffClass.RemoveOnDeath = text.Length == 0 || StringParsers.ParseBool(text, 0, -1, true);
				}
				else
				{
					buffClass.Requirements = RequirementBase.ParseRequirementGroup(_element);
				}
			}
			buffClass.Effects = MinEffectController.ParseXml(_element, null, MinEffectController.SourceParentType.BuffClass, buffClass.Name);
			BuffManager.AddBuff(buffClass);
			return;
		}
		throw new Exception("buff must have an name!");
	}
}
