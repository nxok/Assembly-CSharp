using System;
using System.Collections;
using System.Globalization;
using System.Xml.Linq;
using Quests.Requirements;
using UnityEngine;

public class QuestsFromXml
{
	public static IEnumerator CreateQuests(XmlFile xmlFile)
	{
		QuestClass.s_Quests.Clear();
		QuestList.s_QuestLists.Clear();
		XElement root = xmlFile.XmlDoc.Root;
		if (!root.HasElements)
		{
			throw new Exception("No element <quests> found!");
		}
		QuestsFromXml.ParseNode(root);
		yield break;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseNode(XElement root)
	{
		foreach (XElement xelement in root.Elements())
		{
			if (xelement.Name == "quest")
			{
				QuestsFromXml.ParseQuest(xelement);
			}
			else if (xelement.Name == "quest_list")
			{
				QuestsFromXml.ParseQuestList(xelement);
			}
			else if (xelement.Name == "quest_items")
			{
				QuestsFromXml.ParseQuestItems(xelement);
			}
			else
			{
				if (!(xelement.Name == "quest_tier_rewards"))
				{
					string text = "Unrecognized xml element ";
					XName name = xelement.Name;
					throw new Exception(text + ((name != null) ? name.ToString() : null));
				}
				QuestsFromXml.ParseQuestTierRewards(xelement);
			}
		}
		if (root.HasAttribute("max_quest_tier"))
		{
			Quest.MaxQuestTier = Convert.ToInt32(root.GetAttribute("max_quest_tier"));
		}
		if (root.HasAttribute("quests_per_tier"))
		{
			Quest.QuestsPerTier = Convert.ToInt32(root.GetAttribute("quests_per_tier"));
		}
		if (root.HasAttribute("starter_quest"))
		{
			Quest.StarterQuest = root.GetAttribute("starter_quest");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseQuest(XElement e)
	{
		DynamicProperties dynamicProperties = null;
		if (!e.HasAttribute("id"))
		{
			throw new Exception("quest must have an id attribute");
		}
		string attribute = e.GetAttribute("id");
		QuestClass questClass = QuestClass.NewClass(attribute);
		if (questClass == null)
		{
			throw new Exception("quest with an id of '" + attribute + "' already exists!");
		}
		bool flag = false;
		if (e.HasAttribute("template") && QuestClass.s_Quests.ContainsKey(e.GetAttribute("template")))
		{
			QuestClass questClass2 = QuestClass.s_Quests[e.GetAttribute("template")];
			dynamicProperties = questClass.AssignValuesFrom(questClass2);
			flag = true;
		}
		foreach (XElement xelement in e.Elements())
		{
			if (!flag)
			{
				if (xelement.Name == "property")
				{
					if (dynamicProperties == null)
					{
						dynamicProperties = new DynamicProperties();
					}
					dynamicProperties.Add(xelement, false);
				}
				else if (xelement.Name == "action")
				{
					BaseQuestAction baseQuestAction = QuestsFromXml.ParseAction(questClass, xelement);
					if (baseQuestAction != null)
					{
						questClass.AddAction(baseQuestAction);
					}
				}
				else if (xelement.Name == "event")
				{
					QuestsFromXml.ParseEvent(questClass, xelement);
				}
				else if (xelement.Name == "requirement")
				{
					BaseRequirement baseRequirement = QuestsFromXml.ParseRequirement(questClass, xelement);
					questClass.AddRequirement(baseRequirement);
				}
				else if (xelement.Name == "objective")
				{
					QuestsFromXml.ParseObjective(questClass, xelement);
				}
				else if (xelement.Name == "quest_criteria")
				{
					QuestsFromXml.ParseCriteria(questClass, BaseQuestCriteria.CriteriaTypes.QuestGiver, xelement);
				}
				else if (xelement.Name == "offer_criteria")
				{
					QuestsFromXml.ParseCriteria(questClass, BaseQuestCriteria.CriteriaTypes.Player, xelement);
				}
			}
			if (xelement.Name == "reward")
			{
				QuestsFromXml.ParseReward(questClass, null, xelement);
			}
			else if (xelement.Name == "variable")
			{
				QuestsFromXml.ParseQuestVariable(questClass, xelement);
			}
		}
		questClass.Properties = dynamicProperties;
		if (flag)
		{
			questClass.HandleVariablesForProperties(dynamicProperties);
			questClass.HandleTemplateInit();
		}
		questClass.Init();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseQuestAction ParseAction(QuestClass questClass, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Quest Action must have a type!");
		}
		BaseQuestAction baseQuestAction = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseQuestAction = (BaseQuestAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("QuestAction", attribute));
		}
		catch (Exception)
		{
			throw new Exception("No action class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseQuestAction.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseQuestAction.Value = e.GetAttribute("value");
		}
		if (e.HasAttribute("phase"))
		{
			byte b = Convert.ToByte(e.GetAttribute("phase"));
			baseQuestAction.Phase = (int)b;
		}
		DynamicProperties dynamicProperties = null;
		foreach (XElement xelement in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(xelement, false);
		}
		if (dynamicProperties != null)
		{
			baseQuestAction.Owner = questClass;
			baseQuestAction.ParseProperties(dynamicProperties);
		}
		return baseQuestAction;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseEvent(QuestClass questClass, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Quest Event must have a type!");
		}
		QuestEvent questEvent = new QuestEvent(e.GetAttribute("type"));
		questEvent.Owner = questClass;
		if (e.HasAttribute("chance"))
		{
			questEvent.Chance = StringParsers.ParseFloat(e.GetAttribute("chance"), 0, -1, NumberStyles.Any);
		}
		if (e.HasAttribute("server_only"))
		{
			questEvent.IsServerOnly = StringParsers.ParseBool(e.GetAttribute("server_only"), 0, -1, true);
		}
		DynamicProperties dynamicProperties = null;
		foreach (XElement xelement in e.Elements())
		{
			if (xelement.Name == "property")
			{
				if (dynamicProperties == null)
				{
					dynamicProperties = new DynamicProperties();
				}
				dynamicProperties.Add(xelement, false);
			}
			else if (xelement.Name == "action")
			{
				BaseQuestAction baseQuestAction = QuestsFromXml.ParseAction(questClass, xelement);
				if (baseQuestAction != null)
				{
					baseQuestAction.SetupAction();
					questEvent.Actions.Add(baseQuestAction);
				}
			}
		}
		if (dynamicProperties != null)
		{
			questEvent.Owner = questClass;
			questEvent.ParseProperties(dynamicProperties);
		}
		questClass.AddEvent(questEvent);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseRequirement ParseRequirement(QuestClass questClass, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Requirement must have a type!");
		}
		BaseRequirement baseRequirement = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseRequirement = (BaseRequirement)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("Requirement", attribute));
		}
		catch (Exception)
		{
			throw new Exception("No requirement class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseRequirement.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseRequirement.Value = e.GetAttribute("value");
		}
		if (e.HasAttribute("phase"))
		{
			byte b = Convert.ToByte(e.GetAttribute("phase"));
			baseRequirement.Phase = (int)b;
		}
		if (attribute.EqualsCaseInsensitive("requirementgroup"))
		{
			foreach (XElement xelement in e.Elements("requirement"))
			{
				BaseRequirement baseRequirement2 = QuestsFromXml.ParseRequirement(questClass, xelement);
				((Quests.Requirements.RequirementGroup)baseRequirement).ChildRequirements.Add(baseRequirement2);
			}
		}
		return baseRequirement;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseObjective(QuestClass quest, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Objective must have a type!");
		}
		BaseObjective baseObjective = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseObjective = (BaseObjective)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("Objective", attribute));
			quest.AddObjective(baseObjective);
		}
		catch (Exception ex)
		{
			throw new Exception("No objective class '" + attribute + " found!", ex);
		}
		if (e.HasAttribute("id"))
		{
			baseObjective.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseObjective.Value = e.GetAttribute("value");
		}
		if (e.HasAttribute("optional"))
		{
			baseObjective.Optional = Convert.ToBoolean(e.GetAttribute("optional"));
		}
		if (e.HasAttribute("phase"))
		{
			byte b = Convert.ToByte(e.GetAttribute("phase"));
			baseObjective.Phase = b;
			if (b > quest.HighestPhase)
			{
				quest.HighestPhase = b;
			}
		}
		DynamicProperties dynamicProperties = null;
		foreach (XElement xelement in e.Elements())
		{
			if (xelement.Name == "property")
			{
				if (dynamicProperties == null)
				{
					dynamicProperties = new DynamicProperties();
				}
				dynamicProperties.Add(xelement, false);
			}
			if (xelement.Name == "modifier")
			{
				QuestsFromXml.ParseObjectiveModifier(quest, baseObjective, xelement);
			}
		}
		if (dynamicProperties != null)
		{
			baseObjective.ParseProperties(dynamicProperties);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseObjectiveModifier(QuestClass quest, BaseObjective objective, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Objective Modifier must have a type!");
		}
		BaseObjectiveModifier baseObjectiveModifier = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseObjectiveModifier = (BaseObjectiveModifier)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("ObjectiveModifier", attribute));
			objective.AddModifier(baseObjectiveModifier);
		}
		catch (Exception ex)
		{
			throw new Exception("No objective class '" + attribute + " found!", ex);
		}
		DynamicProperties dynamicProperties = null;
		foreach (XElement xelement in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(xelement, false);
		}
		if (dynamicProperties != null)
		{
			baseObjectiveModifier.ParseProperties(dynamicProperties);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseReward(QuestClass quest, QuestTierReward tierReward, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Reward must have a type!");
		}
		BaseReward baseReward = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseReward = (BaseReward)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("Reward", attribute));
			if (quest != null)
			{
				quest.AddReward(baseReward);
			}
			else if (tierReward != null)
			{
				tierReward.Rewards.Add(baseReward);
			}
		}
		catch (Exception)
		{
			throw new Exception("No reward class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseReward.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseReward.Value = e.GetAttribute("value");
		}
		if (e.HasAttribute("hidden"))
		{
			baseReward.HiddenReward = Convert.ToBoolean(e.GetAttribute("hidden"));
		}
		if (e.HasAttribute("stage"))
		{
			string attribute2 = e.GetAttribute("stage");
			if (!(attribute2 == "start"))
			{
				if (!(attribute2 == "complete"))
				{
					if (attribute2 == "aftercomplete")
					{
						baseReward.ReceiveStage = BaseReward.ReceiveStages.AfterCompleteNotification;
					}
				}
				else
				{
					baseReward.ReceiveStage = BaseReward.ReceiveStages.QuestCompletion;
				}
			}
			else
			{
				baseReward.ReceiveStage = BaseReward.ReceiveStages.QuestStart;
			}
		}
		if (e.HasAttribute("optional"))
		{
			baseReward.Optional = Convert.ToBoolean(e.GetAttribute("optional"));
		}
		if (e.HasAttribute("ischosen"))
		{
			baseReward.isChosenReward = Convert.ToBoolean(e.GetAttribute("ischosen"));
		}
		if (e.HasAttribute("isfixed"))
		{
			baseReward.isFixedLocation = Convert.ToBoolean(e.GetAttribute("isfixed"));
		}
		if (e.HasAttribute("chainreward"))
		{
			baseReward.isChainReward = Convert.ToBoolean(e.GetAttribute("chainreward"));
		}
		DynamicProperties dynamicProperties = null;
		foreach (XElement xelement in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(xelement, false);
		}
		if (dynamicProperties != null)
		{
			baseReward.ParseProperties(dynamicProperties);
		}
		baseReward.SetupGlobalRewardSettings();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCriteria(QuestClass quest, BaseQuestCriteria.CriteriaTypes criteriaType, XElement e)
	{
		if (!e.HasAttribute("type"))
		{
			throw new Exception("Quest Criteria must have a type!");
		}
		BaseQuestCriteria baseQuestCriteria = null;
		string attribute = e.GetAttribute("type");
		try
		{
			baseQuestCriteria = (BaseQuestCriteria)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("QuestCriteria", attribute));
			baseQuestCriteria.OwnerQuestClass = quest;
			quest.AddCriteria(baseQuestCriteria);
			baseQuestCriteria.CriteriaType = criteriaType;
		}
		catch (Exception)
		{
			throw new Exception("No action class '" + attribute + " found!");
		}
		if (e.HasAttribute("id"))
		{
			baseQuestCriteria.ID = e.GetAttribute("id");
		}
		if (e.HasAttribute("value"))
		{
			baseQuestCriteria.Value = e.GetAttribute("value");
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseQuestList(XElement e)
	{
		if (!e.HasAttribute("id"))
		{
			throw new Exception("quest list must have an id attribute");
		}
		string attribute = e.GetAttribute("id");
		QuestList questList = QuestList.NewList(attribute);
		if (questList == null)
		{
			throw new Exception("quest with an id of '" + attribute + "' already exists!");
		}
		foreach (XElement xelement in e.Elements("quest"))
		{
			float num = 1f;
			if (xelement.HasAttribute("prob") && !StringParsers.TryParseFloat(xelement.GetAttribute("prob"), out num))
			{
				throw new Exception(string.Concat(new string[]
				{
					"Parsing error prob '",
					xelement.GetAttribute("prob"),
					"' in '",
					attribute,
					"'"
				}));
			}
			int num2 = -1;
			if (xelement.HasAttribute("start"))
			{
				num2 = StringParsers.ParseSInt32(xelement.GetAttribute("start"), 0, -1, NumberStyles.Integer);
			}
			int num3 = -1;
			if (xelement.HasAttribute("end"))
			{
				num3 = StringParsers.ParseSInt32(xelement.GetAttribute("end"), 0, -1, NumberStyles.Integer);
			}
			string text = "";
			if (xelement.HasAttribute("cvar"))
			{
				text = xelement.GetAttribute("cvar");
			}
			if (xelement.HasAttribute("id"))
			{
				QuestEntry questEntry = new QuestEntry(xelement.GetAttribute("id"), num, num2, num3, text);
				questList.Quests.Add(questEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseQuestItems(XElement e)
	{
		if (e.HasAttribute("max_count"))
		{
			ItemClassQuest.questItemList = new ItemClassQuest[int.Parse(e.GetAttribute("max_count"))];
		}
		else
		{
			ItemClassQuest.questItemList = new ItemClassQuest[100];
		}
		foreach (XElement xelement in e.Elements("quest_item"))
		{
			string text = "questItem";
			if (xelement.HasAttribute("item_template"))
			{
				text = xelement.GetAttribute("item_template");
			}
			ItemClassQuest itemClassQuest = new ItemClassQuest();
			ItemClass itemClass = ItemClass.GetItemClass(text, false);
			itemClassQuest.Properties.CopyFrom(itemClass.Properties, null);
			int num = -1;
			if (xelement.HasAttribute("id"))
			{
				num = int.Parse(xelement.GetAttribute("id"));
			}
			if (num < ItemClassQuest.questItemList.Length)
			{
				ItemClassQuest.questItemList[num] = itemClassQuest;
			}
			else
			{
				Log.Error("ID '{0}' too high. Increase max_count on <quest_items> or change the id", new object[] { num.ToString() });
			}
			if (!xelement.HasAttribute("name"))
			{
				throw new Exception("quest item must have an name!");
			}
			itemClassQuest.SetName(xelement.GetAttribute("name"));
			itemClassQuest.setLocalizedItemName(Localization.Get(xelement.GetAttribute("name"), false, null));
			itemClassQuest.Init();
			if (xelement.HasAttribute("icon"))
			{
				itemClassQuest.CustomIcon = new DataItem<string>(xelement.GetAttribute("icon"));
			}
			if (xelement.HasAttribute("icon_color"))
			{
				itemClassQuest.CustomIconTint = StringParsers.ParseHexColor(xelement.GetAttribute("icon_color"));
			}
			else
			{
				itemClassQuest.CustomIconTint = Color.white;
			}
			itemClassQuest.MeshFile = itemClassQuest.Properties.GetString("Meshfile");
			itemClassQuest.SetCanDrop(false);
			itemClassQuest.IsQuestItem = true;
			itemClassQuest.DisplayType = "";
			itemClassQuest.Stacknumber.Value = 1;
			itemClassQuest.DescriptionKey = "";
			if (xelement.HasAttribute("description_key"))
			{
				itemClassQuest.DescriptionKey = xelement.GetAttribute("description_key");
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseQuestTierRewards(XElement e)
	{
		foreach (XElement xelement in e.Elements("quest_tier_reward"))
		{
			QuestTierReward questTierReward = new QuestTierReward();
			int num = -1;
			if (xelement.HasAttribute("tier"))
			{
				num = int.Parse(xelement.GetAttribute("tier"));
			}
			questTierReward.Tier = num;
			foreach (XElement xelement2 in xelement.Elements("reward"))
			{
				QuestsFromXml.ParseReward(null, questTierReward, xelement2);
			}
			QuestEventManager.Current.AddQuestTierReward(questTierReward);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseQuestVariable(QuestClass owner, XElement e)
	{
		string text = "";
		string text2 = "";
		if (e.HasAttribute("name"))
		{
			text = e.GetAttribute("name");
		}
		if (e.HasAttribute("value"))
		{
			text2 = e.GetAttribute("value");
		}
		if (text != "" && !owner.Variables.ContainsKey(text))
		{
			owner.Variables.Add(text, text2);
		}
	}
}
