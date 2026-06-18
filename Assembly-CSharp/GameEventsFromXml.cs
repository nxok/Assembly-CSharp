using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using GameEvent.SequenceActions;
using GameEvent.SequenceDecisions;
using GameEvent.SequenceLoops;
using GameEvent.SequenceRequirements;

public class GameEventsFromXml
{
	public static IEnumerator CreateGameEvents(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		GameEventManager.Current.ClearActions();
		if (!root.HasElements)
		{
			throw new Exception("No element <gameevents> found!");
		}
		MicroStopwatch msw = new MicroStopwatch(true);
		foreach (XElement xelement in root.Elements())
		{
			if (xelement.Name == "action_sequence")
			{
				GameEventsFromXml.ParseGameEventSequence(xelement);
			}
			else
			{
				if (!(xelement.Name == "category"))
				{
					string text = "Unrecognized xml element ";
					XName name = xelement.Name;
					throw new Exception(text + ((name != null) ? name.ToString() : null));
				}
				if (xelement.HasAttribute("name"))
				{
					string attribute = xelement.GetAttribute("name");
					string text2 = (xelement.HasAttribute("display_name") ? Localization.Get(xelement.GetAttribute("display_name"), false, null) : attribute);
					if (xelement.HasAttribute("icon"))
					{
						GameEventManager.Current.CategoryList.Add(new GameEventManager.Category
						{
							Name = attribute,
							DisplayName = text2,
							Icon = xelement.GetAttribute("icon")
						});
					}
					else
					{
						GameEventManager.Current.CategoryList.Add(new GameEventManager.Category
						{
							Name = attribute,
							DisplayName = text2,
							Icon = ""
						});
					}
				}
			}
			if (msw.ElapsedMilliseconds > (long)Constants.cMaxLoadTimePerFrameMillis)
			{
				yield return null;
				msw.ResetAndRestart();
			}
		}
		IEnumerator<XElement> enumerator = null;
		XElement xelement2 = root;
		if (xelement2.HasAttribute("max_entities"))
		{
			GameEventManager.Current.MaxSpawnCount = Convert.ToInt32(xelement2.GetAttribute("max_entities"));
		}
		yield break;
		yield break;
	}

	public static void Reload(XmlFile xmlFile)
	{
		ThreadManager.RunCoroutineSync(GameEventsFromXml.CreateGameEvents(xmlFile));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseGameEventSequence(XElement e)
	{
		DynamicProperties dynamicProperties = null;
		GameEventActionSequence gameEventActionSequence = new GameEventActionSequence();
		bool flag = false;
		if (e.HasAttribute("template") && GameEventManager.GameEventSequences.ContainsKey(e.GetAttribute("template")))
		{
			GameEventActionSequence gameEventActionSequence2 = GameEventManager.GameEventSequences[e.GetAttribute("template")];
			dynamicProperties = gameEventActionSequence.AssignValuesFrom(gameEventActionSequence2);
			flag = true;
		}
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
			if (!flag)
			{
				if (xelement.Name == "action")
				{
					GameEventsFromXml.ParseGameEventSequenceAction(xelement, gameEventActionSequence, true);
				}
				else if (xelement.Name == "requirement")
				{
					GameEventsFromXml.ParseGameEventSequenceRequirement(xelement, gameEventActionSequence);
				}
				else if (xelement.Name == "loop")
				{
					GameEventsFromXml.ParseGameEventSequenceLoop(xelement, gameEventActionSequence, true);
				}
				else if (xelement.Name == "wait")
				{
					GameEventsFromXml.ParseGameEventSequenceWait(xelement, gameEventActionSequence, true);
				}
				else if (xelement.Name == "decision")
				{
					GameEventsFromXml.ParseGameEventSequenceDecision(xelement, gameEventActionSequence, true);
				}
			}
			if (xelement.Name == "variable")
			{
				GameEventsFromXml.ParseGameEventSequenceVariable(xelement, gameEventActionSequence);
			}
		}
		if (e.HasAttribute("name"))
		{
			gameEventActionSequence.Name = e.GetAttribute("name");
		}
		if (dynamicProperties != null)
		{
			gameEventActionSequence.ParseProperties(dynamicProperties);
		}
		if (flag)
		{
			gameEventActionSequence.HandleTemplateInit();
		}
		gameEventActionSequence.Init();
		GameEventManager.Current.AddSequence(gameEventActionSequence);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseAction ParseGameEventSequenceAction(XElement e, GameEventActionSequence owner, bool addToSequence = true)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement xelement in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(xelement, false);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Sequence Action must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseAction baseAction = null;
		try
		{
			baseAction = (BaseAction)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceActions.Action", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event sequence action class '" + text + " found!");
		}
		baseAction.Owner = owner;
		foreach (XElement xelement2 in e.Elements("requirement"))
		{
			GameEventsFromXml.ParseGameEventActionRequirement(xelement2, baseAction);
		}
		if (dynamicProperties != null)
		{
			baseAction.ParseProperties(dynamicProperties);
		}
		baseAction.Init();
		if (addToSequence)
		{
			owner.Actions.Add(baseAction);
		}
		return baseAction;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseAction ParseGameEventSequenceLoop(XElement e, GameEventActionSequence owner, bool addToSequence = true)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement xelement in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(xelement, false);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Sequence Loop must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseLoop baseLoop = null;
		try
		{
			baseLoop = (BaseLoop)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceLoops.Loop", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event sequence loop class '" + text + " found!");
		}
		baseLoop.Owner = owner;
		foreach (XElement xelement2 in e.Elements())
		{
			if (xelement2.Name == "requirement")
			{
				GameEventsFromXml.ParseGameEventActionRequirement(xelement2, baseLoop);
			}
			else if (xelement2.Name == "action")
			{
				BaseAction baseAction = GameEventsFromXml.ParseGameEventSequenceAction(xelement2, owner, false);
				if (baseAction != null)
				{
					baseLoop.Actions.Add(baseAction);
				}
			}
			else if (xelement2.Name == "wait")
			{
				BaseAction baseAction2 = GameEventsFromXml.ParseGameEventSequenceWait(xelement2, owner, false);
				if (baseAction2 != null)
				{
					baseLoop.Actions.Add(baseAction2);
				}
			}
			else if (xelement2.Name == "decision")
			{
				BaseAction baseAction3 = GameEventsFromXml.ParseGameEventSequenceDecision(xelement2, owner, false);
				if (baseAction3 != null)
				{
					baseLoop.Actions.Add(baseAction3);
				}
			}
			else if (xelement2.Name == "loop")
			{
				BaseAction baseAction4 = GameEventsFromXml.ParseGameEventSequenceLoop(xelement2, owner, false);
				if (baseAction4 != null)
				{
					baseLoop.Actions.Add(baseAction4);
				}
			}
		}
		if (dynamicProperties != null)
		{
			baseLoop.ParseProperties(dynamicProperties);
		}
		baseLoop.Init();
		if (addToSequence)
		{
			owner.Actions.Add(baseLoop);
		}
		return baseLoop;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseAction ParseGameEventSequenceWait(XElement e, GameEventActionSequence owner, bool addToSequence = true)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement xelement in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(xelement, false);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Sequence Loop must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseWait baseWait = null;
		try
		{
			baseWait = (BaseWait)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceActions.Wait", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event sequence wait class '" + text + " found!");
		}
		baseWait.Owner = owner;
		foreach (XElement xelement2 in e.Elements("requirement"))
		{
			GameEventsFromXml.ParseGameEventActionRequirement(xelement2, baseWait);
		}
		if (dynamicProperties != null)
		{
			baseWait.ParseProperties(dynamicProperties);
		}
		baseWait.Init();
		if (addToSequence)
		{
			owner.Actions.Add(baseWait);
		}
		return baseWait;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseRequirement ParseGameEventSequenceRequirement(XElement e, GameEventActionSequence owner)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement xelement in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(xelement, false);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Sequence Requirement must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseRequirement baseRequirement = null;
		try
		{
			baseRequirement = (BaseRequirement)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceRequirements.Requirement", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event sequence requirement class '" + text + " found!");
		}
		baseRequirement.Owner = owner;
		if (dynamicProperties != null)
		{
			baseRequirement.ParseProperties(dynamicProperties);
		}
		baseRequirement.Init();
		owner.Requirements.Add(baseRequirement);
		return baseRequirement;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseAction ParseGameEventSequenceDecision(XElement e, GameEventActionSequence owner, bool addToSequence = true)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement xelement in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(xelement, false);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Sequence Decision must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseDecision baseDecision = null;
		try
		{
			baseDecision = (BaseDecision)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceDecisions.Decision", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event sequence decision class '" + text + " found!");
		}
		baseDecision.Owner = owner;
		foreach (XElement xelement2 in e.Elements())
		{
			if (xelement2.Name == "requirement")
			{
				GameEventsFromXml.ParseGameEventActionRequirement(xelement2, baseDecision);
			}
			else if (xelement2.Name == "action")
			{
				BaseAction baseAction = GameEventsFromXml.ParseGameEventSequenceAction(xelement2, owner, false);
				if (baseAction != null)
				{
					baseDecision.Actions.Add(baseAction);
				}
			}
		}
		if (dynamicProperties != null)
		{
			baseDecision.ParseProperties(dynamicProperties);
		}
		baseDecision.Init();
		if (addToSequence)
		{
			owner.Actions.Add(baseDecision);
		}
		return baseDecision;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseRequirement ParseGameEventActionRequirement(XElement e, BaseAction owner)
	{
		DynamicProperties dynamicProperties = null;
		foreach (XElement xelement in e.Elements("property"))
		{
			if (dynamicProperties == null)
			{
				dynamicProperties = new DynamicProperties();
			}
			dynamicProperties.Add(xelement, false);
		}
		string text = "";
		if (e.HasAttribute("class"))
		{
			text = e.GetAttribute("class");
		}
		else
		{
			if (!dynamicProperties.Contains("class"))
			{
				throw new Exception("Game Event Action Requirement must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseRequirement baseRequirement = null;
		try
		{
			baseRequirement = (BaseRequirement)Activator.CreateInstance(ReflectionHelpers.GetTypeWithPrefix("GameEvent.SequenceRequirements.Requirement", text));
		}
		catch (Exception)
		{
			throw new Exception("No game event requirement class '" + text + " found!");
		}
		baseRequirement.Owner = owner.Owner;
		if (dynamicProperties != null)
		{
			baseRequirement.ParseProperties(dynamicProperties);
		}
		baseRequirement.Init();
		owner.AddRequirement(baseRequirement);
		return baseRequirement;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseGameEventSequenceVariable(XElement e, GameEventActionSequence owner)
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
