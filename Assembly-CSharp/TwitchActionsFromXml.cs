using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Twitch;

public class TwitchActionsFromXml
{
	public static IEnumerator CreateTwitchActions(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		TwitchActionManager.Current.Cleanup();
		if (!root.HasElements)
		{
			throw new Exception("No element <twitch> found!");
		}
		TwitchActionsFromXml.HandleTwitchSettings(root);
		MicroStopwatch msw = new MicroStopwatch(true);
		foreach (XElement xelement in root.Elements())
		{
			if (xelement.Name == "action")
			{
				TwitchActionsFromXml.ParseTwitchActions(xelement);
			}
			else if (xelement.Name == "actions_preset")
			{
				TwitchActionsFromXml.ParseActionsPreset(xelement);
			}
			else if (xelement.Name == "votes_preset")
			{
				TwitchActionsFromXml.ParseVotesPreset(xelement);
			}
			else if (xelement.Name == "vote_entry")
			{
				TwitchActionsFromXml.ParseVoteEntry(xelement);
			}
			else if (xelement.Name == "vote_type")
			{
				TwitchActionsFromXml.ParseVoteTypeEntry(xelement);
			}
			else if (xelement.Name == "command_permission")
			{
				TwitchActionsFromXml.ParseCommandPermission(xelement);
			}
			else if (xelement.Name == "cooldown_preset")
			{
				TwitchActionsFromXml.ParseCooldownPreset(xelement);
			}
			else if (xelement.Name == "category")
			{
				XElement xelement2 = xelement;
				if (xelement2.HasAttribute("name"))
				{
					string attribute = xelement2.GetAttribute("name");
					string text = (xelement2.HasAttribute("display_name") ? Localization.Get(xelement2.GetAttribute("display_name"), false, null) : attribute);
					bool flag = !xelement2.HasAttribute("show_command_list") || StringParsers.ParseBool(xelement2.GetAttribute("show_command_list"), 0, -1, true);
					bool flag2 = xelement2.HasAttribute("always_show_in_menu") && StringParsers.ParseBool(xelement2.GetAttribute("always_show_in_menu"), 0, -1, true);
					if (xelement2.HasAttribute("icon"))
					{
						TwitchActionManager.Current.CategoryList.Add(new TwitchActionManager.ActionCategory
						{
							Name = attribute,
							DisplayName = text,
							Icon = xelement2.GetAttribute("icon"),
							ShowInCommandList = flag,
							AlwaysShowInMenu = flag2
						});
					}
					else
					{
						TwitchActionManager.Current.CategoryList.Add(new TwitchActionManager.ActionCategory
						{
							Name = attribute,
							DisplayName = text,
							Icon = "",
							ShowInCommandList = flag,
							AlwaysShowInMenu = flag2
						});
					}
				}
			}
			else if (xelement.Name == "random_group")
			{
				TwitchActionsFromXml.ParseRandomGroup(xelement);
			}
			else
			{
				if (!(xelement.Name == "tip"))
				{
					string text2 = "Unrecognized xml element ";
					XName name = xelement.Name;
					throw new Exception(text2 + ((name != null) ? name.ToString() : null));
				}
				XElement xelement3 = xelement;
				if (xelement3.HasAttribute("name"))
				{
					TwitchManager.Current.AddTip(xelement3.GetAttribute("name"));
				}
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

	public static IEnumerator CreateTwitchEvents(XmlFile xmlFile)
	{
		XElement root = xmlFile.XmlDoc.Root;
		TwitchManager.Current.CleanupEventData();
		if (root == null)
		{
			throw new Exception("No element <twitch_events> found!");
		}
		TwitchActionsFromXml.HandleTwitchSettings(root);
		MicroStopwatch msw = new MicroStopwatch(true);
		foreach (XElement xelement in root.Elements())
		{
			if (xelement.Name == "bit_event")
			{
				TwitchActionsFromXml.ParseEvent(xelement, xelement.Name.LocalName);
			}
			else if (xelement.Name == "events_preset")
			{
				TwitchActionsFromXml.ParseEventsPreset(xelement, xelement.Name.LocalName);
			}
			else if (xelement.Name == "sub_event")
			{
				TwitchActionsFromXml.ParseSubEvent(xelement, xelement.Name.LocalName);
			}
			else if (xelement.Name == "gift_sub_event")
			{
				TwitchActionsFromXml.ParseSubEvent(xelement, xelement.Name.LocalName);
			}
			else if (xelement.Name == "raid_event")
			{
				TwitchActionsFromXml.ParseEvent(xelement, xelement.Name.LocalName);
			}
			else if (xelement.Name == "charity_event")
			{
				TwitchActionsFromXml.ParseEvent(xelement, xelement.Name.LocalName);
			}
			else if (xelement.Name == "hype_train_event")
			{
				TwitchActionsFromXml.ParseHypeTrainEvent(xelement, xelement.Name.LocalName);
			}
			else if (xelement.Name == "channel_point_event")
			{
				TwitchActionsFromXml.ParseChannelPointEvent(xelement);
			}
			else
			{
				if (!(xelement.Name == "creator_goal_event"))
				{
					string text = "Unrecognized xml element ";
					XName name = xelement.Name;
					throw new Exception(text + ((name != null) ? name.ToString() : null));
				}
				TwitchActionsFromXml.ParseCreatorGoalEvent(xelement, xelement.Name.LocalName);
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
	public static void HandleTwitchSettings(XElement e)
	{
		TwitchManager twitchManager = TwitchManager.Current;
		if (e.HasAttribute("starting_points"))
		{
			twitchManager.ViewerData.StartingPoints = StringParsers.ParseSInt32(e.GetAttribute("starting_points"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("party_kill_reward_max"))
		{
			int num = StringParsers.ParseSInt32(e.GetAttribute("party_kill_reward_max"), 0, -1, NumberStyles.Integer);
			if (num < 0)
			{
				num = 0;
			}
			twitchManager.PartyKillRewardMax = num;
		}
		if (e.HasAttribute("chat_activity_time"))
		{
			float num2 = StringParsers.ParseFloat(e.GetAttribute("chat_activity_time"), 0, -1, NumberStyles.Any);
			if (num2 < 0f)
			{
				num2 = 0f;
			}
			TwitchViewerData.ChattingAddedTimeAmount = num2;
		}
		if (e.HasAttribute("nonsub_pp_cap"))
		{
			twitchManager.ViewerData.NonSubPointCap = StringParsers.ParseFloat(e.GetAttribute("nonsub_pp_cap"), 0, -1, NumberStyles.Any);
		}
		if (e.HasAttribute("sub_pp_cap"))
		{
			twitchManager.ViewerData.SubPointCap = StringParsers.ParseFloat(e.GetAttribute("sub_pp_cap"), 0, -1, NumberStyles.Any);
		}
		if (e.HasAttribute("pimp_pot_type"))
		{
			string text = e.GetAttribute("pimp_pot_type").ToLower();
			if (!(text == "pp"))
			{
				if (!(text == "sp"))
				{
					if (text == "disabled")
					{
						twitchManager.PimpPotType = TwitchManager.PimpPotSettings.Disabled;
					}
				}
				else
				{
					twitchManager.PimpPotType = TwitchManager.PimpPotSettings.EnabledSP;
				}
			}
			else
			{
				twitchManager.PimpPotType = TwitchManager.PimpPotSettings.EnabledPP;
			}
		}
		if (e.HasAttribute("pimp_pot_default"))
		{
			TwitchManager.PimpPotDefault = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_default"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("denied_crate_event"))
		{
			twitchManager.DeniedCrateEvent = e.GetAttribute("denied_crate_event");
		}
		if (e.HasAttribute("stealing_crate_event"))
		{
			twitchManager.StealingCrateEvent = e.GetAttribute("stealing_crate_event");
		}
		if (e.HasAttribute("party_respawn_event"))
		{
			twitchManager.PartyRespawnEvent = e.GetAttribute("party_respawn_event");
		}
		if (e.HasAttribute("on_death_event"))
		{
			twitchManager.OnPlayerDeathEvent = e.GetAttribute("on_death_event");
		}
		if (e.HasAttribute("on_player_respawn_event"))
		{
			twitchManager.OnPlayerRespawnEvent = e.GetAttribute("on_player_respawn_event");
		}
		if (e.HasAttribute("sub_tier1_points"))
		{
			twitchManager.ViewerData.SubPointAddTier1 = StringParsers.ParseSInt32(e.GetAttribute("sub_tier1_points"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("sub_tier2_points"))
		{
			twitchManager.ViewerData.SubPointAddTier2 = StringParsers.ParseSInt32(e.GetAttribute("sub_tier2_points"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("sub_tier3_points"))
		{
			twitchManager.ViewerData.SubPointAddTier3 = StringParsers.ParseSInt32(e.GetAttribute("sub_tier3_points"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("gift_sub_tier1_points"))
		{
			twitchManager.ViewerData.GiftSubPointAddTier1 = StringParsers.ParseSInt32(e.GetAttribute("gift_sub_tier1_points"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("gift_sub_tier2_points"))
		{
			twitchManager.ViewerData.GiftSubPointAddTier2 = StringParsers.ParseSInt32(e.GetAttribute("gift_sub_tier2_points"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("gift_sub_tier3_points"))
		{
			twitchManager.ViewerData.GiftSubPointAddTier3 = StringParsers.ParseSInt32(e.GetAttribute("gift_sub_tier3_points"), 0, -1, NumberStyles.Integer);
		}
	}

	public static void Reload(XmlFile xmlFile)
	{
		ThreadManager.RunCoroutineSync(TwitchActionsFromXml.CreateTwitchActions(xmlFile));
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DynamicProperties HandleExtends(TwitchAction extendedClass)
	{
		if (extendedClass.Properties != null)
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			dynamicProperties.CopyFrom(extendedClass.Properties, TwitchAction.ExtendsExcludes);
			return dynamicProperties;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseActionsPreset(XElement e)
	{
		TwitchActionPreset twitchActionPreset = new TwitchActionPreset();
		if (e.HasAttribute("name"))
		{
			twitchActionPreset.Name = e.GetAttribute("name");
		}
		if (e.HasAttribute("title"))
		{
			twitchActionPreset.Title = e.GetAttribute("title");
		}
		if (e.HasAttribute("title_key"))
		{
			twitchActionPreset.Title = Localization.Get(e.GetAttribute("title_key"), false, null);
		}
		if (e.HasAttribute("description"))
		{
			twitchActionPreset.Description = e.GetAttribute("description");
		}
		if (e.HasAttribute("description_key"))
		{
			twitchActionPreset.Description = Localization.Get(e.GetAttribute("description_key"), false, null);
		}
		if (e.HasAttribute("default"))
		{
			twitchActionPreset.IsDefault = StringParsers.ParseBool(e.GetAttribute("default"), 0, -1, true);
		}
		if (e.HasAttribute("enabled"))
		{
			twitchActionPreset.IsEnabled = StringParsers.ParseBool(e.GetAttribute("enabled"), 0, -1, true);
		}
		if (e.HasAttribute("is_empty"))
		{
			twitchActionPreset.IsEmpty = StringParsers.ParseBool(e.GetAttribute("is_empty"), 0, -1, true);
		}
		if (e.HasAttribute("allow_point_generation"))
		{
			twitchActionPreset.AllowPointGeneration = StringParsers.ParseBool(e.GetAttribute("allow_point_generation"), 0, -1, true);
		}
		if (e.HasAttribute("use_helper_reward"))
		{
			twitchActionPreset.UseHelperReward = StringParsers.ParseBool(e.GetAttribute("use_helper_reward"), 0, -1, true);
		}
		if (e.HasAttribute("show_new_commands"))
		{
			twitchActionPreset.ShowNewCommands = StringParsers.ParseBool(e.GetAttribute("show_new_commands"), 0, -1, true);
		}
		foreach (XElement xelement in e.Elements())
		{
			if (xelement.Name == "cooldown_modifier")
			{
				TwitchActionsFromXml.ParseCooldownModifier(xelement, twitchActionPreset);
			}
		}
		TwitchManager.Current.AddTwitchActionPreset(twitchActionPreset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCooldownModifier(XElement e, TwitchActionPreset actionPreset)
	{
		TwitchActionCooldownModifier twitchActionCooldownModifier = new TwitchActionCooldownModifier();
		if (e.HasAttribute("category"))
		{
			twitchActionCooldownModifier.CategoryName = e.GetAttribute("category");
		}
		if (e.HasAttribute("action"))
		{
			twitchActionCooldownModifier.ActionName = e.GetAttribute("action");
		}
		if (e.HasAttribute("value"))
		{
			twitchActionCooldownModifier.Value = StringParsers.ParseFloat(e.GetAttribute("value"), 0, -1, NumberStyles.Any);
		}
		if (e.HasAttribute("modifier"))
		{
			twitchActionCooldownModifier.Modifier = EnumUtils.Parse<PassiveEffect.ValueModifierTypes>(e.GetAttribute("modifier"), false);
		}
		actionPreset.AddCooldownModifier(twitchActionCooldownModifier);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseVotesPreset(XElement e)
	{
		TwitchVotePreset twitchVotePreset = new TwitchVotePreset();
		if (e.HasAttribute("name"))
		{
			twitchVotePreset.Name = e.GetAttribute("name");
		}
		if (e.HasAttribute("title"))
		{
			twitchVotePreset.Title = e.GetAttribute("title");
		}
		if (e.HasAttribute("title_key"))
		{
			twitchVotePreset.Title = Localization.Get(e.GetAttribute("title_key"), false, null);
		}
		if (e.HasAttribute("description"))
		{
			twitchVotePreset.Description = e.GetAttribute("description");
		}
		if (e.HasAttribute("description_key"))
		{
			twitchVotePreset.Description = Localization.Get(e.GetAttribute("description_key"), false, null);
		}
		if (e.HasAttribute("default"))
		{
			twitchVotePreset.IsDefault = StringParsers.ParseBool(e.GetAttribute("default"), 0, -1, true);
		}
		if (e.HasAttribute("is_empty"))
		{
			twitchVotePreset.IsEmpty = StringParsers.ParseBool(e.GetAttribute("is_empty"), 0, -1, true);
		}
		if (e.HasAttribute("boss_vote_setting"))
		{
			twitchVotePreset.BossVoteSetting = (TwitchVotingManager.BossVoteSettings)Enum.Parse(typeof(TwitchVotingManager.BossVoteSettings), e.GetAttribute("boss_vote_setting"), true);
		}
		TwitchManager.Current.AddTwitchVotePreset(twitchVotePreset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseTwitchActions(XElement e)
	{
		DynamicProperties dynamicProperties = null;
		if (e.HasAttribute("extends"))
		{
			string attribute = e.GetAttribute("extends");
			if (TwitchActionManager.TwitchActions.ContainsKey(attribute))
			{
				TwitchAction twitchAction = TwitchActionManager.TwitchActions[attribute];
				if (twitchAction == null)
				{
					throw new Exception(string.Format("Extends twitch action {0} is not specified.'", attribute));
				}
				dynamicProperties = TwitchActionsFromXml.HandleExtends(twitchAction);
			}
		}
		TwitchAction twitchAction2 = new TwitchAction();
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
			else if (xelement.Name == "requirement")
			{
				TwitchActionsFromXml.ParseRequirement(xelement, twitchAction2);
			}
			else if (xelement.Name == "cooldown_addition")
			{
				TwitchActionsFromXml.ParseCooldownAddition(xelement, twitchAction2, null);
			}
		}
		if (e.HasAttribute("name"))
		{
			twitchAction2.Name = e.GetAttribute("name");
		}
		bool flag = true;
		if (dynamicProperties != null)
		{
			flag = twitchAction2.ParseProperties(dynamicProperties);
		}
		twitchAction2.Init();
		if (flag)
		{
			TwitchActionManager.Current.AddAction(twitchAction2);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static DynamicProperties HandleExtends(TwitchVote extendedClass)
	{
		if (extendedClass.Properties != null)
		{
			DynamicProperties dynamicProperties = new DynamicProperties();
			dynamicProperties.CopyFrom(extendedClass.Properties, TwitchVote.ExtendsExcludes);
			return dynamicProperties;
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseVoteEntry(XElement e)
	{
		DynamicProperties dynamicProperties = null;
		if (e.HasAttribute("extends"))
		{
			string attribute = e.GetAttribute("extends");
			if (TwitchActionManager.TwitchVotes.ContainsKey(attribute))
			{
				TwitchVote twitchVote = TwitchActionManager.TwitchVotes[attribute];
				if (twitchVote == null)
				{
					throw new Exception(string.Format("Extends twitch vote {0} is not specified.'", attribute));
				}
				dynamicProperties = TwitchActionsFromXml.HandleExtends(twitchVote);
			}
		}
		TwitchVote twitchVote2 = new TwitchVote();
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
			else if (xelement.Name == "cooldown_addition")
			{
				TwitchActionsFromXml.ParseCooldownAddition(xelement, null, twitchVote2);
			}
			else if (xelement.Name == "requirement")
			{
				TwitchActionsFromXml.ParseRequirement(xelement, twitchVote2);
			}
		}
		twitchVote2.VoteName = e.GetAttribute("name");
		if (dynamicProperties != null)
		{
			twitchVote2.ParseProperties(dynamicProperties);
		}
		TwitchActionManager.Current.AddVoteClass(twitchVote2);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCommandPermission(XElement e)
	{
		if (!e.HasAttribute("command") || !e.HasAttribute("permission"))
		{
			return;
		}
		string attribute = e.GetAttribute("command");
		BaseTwitchCommand.PermissionLevels permissionLevels = Enum.Parse<BaseTwitchCommand.PermissionLevels>(e.GetAttribute("permission"));
		BaseTwitchCommand.AddCommandPermissionOverride(attribute, permissionLevels);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseVoteTypeEntry(XElement e)
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
		TwitchVoteType twitchVoteType = new TwitchVoteType();
		twitchVoteType.Name = e.GetAttribute("name");
		if (dynamicProperties != null)
		{
			twitchVoteType.ParseProperties(dynamicProperties);
		}
		TwitchManager.Current.VotingManager.AddVoteType(twitchVoteType);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCooldownPreset(XElement e)
	{
		CooldownPreset cooldownPreset = new CooldownPreset();
		cooldownPreset.Name = e.GetAttribute("name");
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
			else if (xelement.Name == "cooldown_entry")
			{
				TwitchActionsFromXml.ParseCooldownEntry(xelement, cooldownPreset);
			}
		}
		if (dynamicProperties != null)
		{
			cooldownPreset.ParseProperties(dynamicProperties);
		}
		TwitchManager.Current.AddCooldownPreset(cooldownPreset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCooldownEntry(XElement e, CooldownPreset preset)
	{
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		int num4 = 180;
		if (e.HasAttribute("start"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("start"), out num);
		}
		if (e.HasAttribute("end"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("end"), out num2);
		}
		if (e.HasAttribute("cooldown_max"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("cooldown_max"), out num3);
		}
		if (e.HasAttribute("cooldown_max"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("cooldown_max"), out num3);
		}
		if (e.HasAttribute("cooldown_time"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("cooldown_time"), out num4);
		}
		if (num3 != -1)
		{
			preset.AddCooldownMaxEntry(num, num2, num3, num4);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseRandomGroup(XElement e)
	{
		string text = "";
		int num = -1;
		if (e.HasAttribute("random_count"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("random_count"), out num);
		}
		if (e.HasAttribute("name"))
		{
			text = e.GetAttribute("name");
		}
		if (text != "")
		{
			TwitchManager.Current.AddRandomGroup(text, num);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCooldownAddition(XElement e, TwitchAction action, TwitchVote vote)
	{
		string text = "";
		int num = 0;
		bool flag = true;
		if (e.HasAttribute("name"))
		{
			text = e.GetAttribute("name");
		}
		if (e.HasAttribute("time"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("time"), out num);
		}
		if (e.HasAttribute("is_action"))
		{
			flag = StringParsers.ParseBool(e.GetAttribute("is_action"), 0, -1, true);
		}
		if (text != "" && num != 0)
		{
			if (action != null)
			{
				action.AddCooldownAddition(new TwitchActionCooldownAddition
				{
					ActionName = text,
					CooldownTime = (float)num,
					IsAction = flag
				});
				return;
			}
			if (vote != null)
			{
				vote.AddCooldownAddition(new TwitchActionCooldownAddition
				{
					ActionName = text,
					CooldownTime = (float)num,
					IsAction = flag
				});
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static BaseTwitchRequirement ParseRequirement(XElement e)
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
				throw new Exception("Twitch Action Requirement must have a class!");
			}
			text = dynamicProperties.Values["class"];
		}
		BaseTwitchRequirement baseTwitchRequirement = null;
		text = string.Format("Twitch.TwitchRequirement{0}", text);
		try
		{
			baseTwitchRequirement = (BaseTwitchRequirement)Activator.CreateInstance(Type.GetType(text));
		}
		catch (Exception)
		{
			throw new Exception("No twitch requirement class '" + text + " found!");
		}
		if (dynamicProperties != null)
		{
			baseTwitchRequirement.ParseProperties(dynamicProperties);
		}
		baseTwitchRequirement.Init();
		return baseTwitchRequirement;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseRequirement(XElement e, TwitchAction action)
	{
		BaseTwitchRequirement baseTwitchRequirement = TwitchActionsFromXml.ParseRequirement(e);
		if (baseTwitchRequirement != null)
		{
			action.AddRequirement(baseTwitchRequirement);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseRequirement(XElement e, TwitchVote vote)
	{
		BaseTwitchRequirement baseTwitchRequirement = TwitchActionsFromXml.ParseRequirement(e);
		if (baseTwitchRequirement != null)
		{
			vote.AddRequirement(baseTwitchRequirement);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseEventsPreset(XElement e, string nodeName)
	{
		TwitchEventPreset twitchEventPreset = new TwitchEventPreset();
		if (e.HasAttribute("name"))
		{
			twitchEventPreset.Name = e.GetAttribute("name");
		}
		if (e.HasAttribute("title"))
		{
			twitchEventPreset.Title = e.GetAttribute("title");
		}
		if (e.HasAttribute("title_key"))
		{
			twitchEventPreset.Title = Localization.Get(e.GetAttribute("title_key"), false, null);
		}
		if (e.HasAttribute("description"))
		{
			twitchEventPreset.Description = e.GetAttribute("description");
		}
		if (e.HasAttribute("description_key"))
		{
			twitchEventPreset.Description = Localization.Get(e.GetAttribute("description_key"), false, null);
		}
		if (e.HasAttribute("default"))
		{
			twitchEventPreset.IsDefault = StringParsers.ParseBool(e.GetAttribute("default"), 0, -1, true);
		}
		if (e.HasAttribute("is_empty"))
		{
			twitchEventPreset.IsEmpty = StringParsers.ParseBool(e.GetAttribute("is_empty"), 0, -1, true);
		}
		TwitchManager.Current.AddTwitchEventPreset(twitchEventPreset);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseEvent(XElement e, string nodeName)
	{
		TwitchEventEntry twitchEventEntry = new TwitchEventEntry();
		if (e.HasAttribute("start_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("start_amount"), out twitchEventEntry.StartAmount);
		}
		if (e.HasAttribute("end_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("end_amount"), out twitchEventEntry.EndAmount);
		}
		if (e.HasAttribute("event_name"))
		{
			twitchEventEntry.EventName = e.GetAttribute("event_name");
		}
		if (e.HasAttribute("event_title"))
		{
			twitchEventEntry.EventTitle = e.GetAttribute("event_title");
		}
		if (e.HasAttribute("event_title_key"))
		{
			twitchEventEntry.EventTitle = Localization.Get(e.GetAttribute("event_title_key"), false, null);
		}
		if (e.HasAttribute("safe_allowed"))
		{
			twitchEventEntry.SafeAllowed = StringParsers.ParseBool(e.GetAttribute("safe_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("cooldown_allowed"))
		{
			twitchEventEntry.CooldownAllowed = StringParsers.ParseBool(e.GetAttribute("cooldown_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("vote_allowed"))
		{
			twitchEventEntry.VoteEventAllowed = StringParsers.ParseBool(e.GetAttribute("vote_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("pp_add_amount"))
		{
			twitchEventEntry.PPAmount = StringParsers.ParseSInt32(e.GetAttribute("pp_add_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("sp_add_amount"))
		{
			twitchEventEntry.SPAmount = StringParsers.ParseSInt32(e.GetAttribute("sp_add_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("pimp_pot_add"))
		{
			twitchEventEntry.PimpPotAdd = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("bit_pot_add"))
		{
			twitchEventEntry.BitPotAdd = StringParsers.ParseSInt32(e.GetAttribute("bit_pot_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("cooldown_add"))
		{
			twitchEventEntry.CooldownAdd = StringParsers.ParseSInt32(e.GetAttribute("cooldown_add"), 0, -1, NumberStyles.Integer);
		}
		string text = "";
		if (e.HasAttribute("presets"))
		{
			text = e.GetAttribute("presets");
		}
		if (text == "")
		{
			return;
		}
		string[] array = text.Split(',', StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			TwitchEventPreset eventPreset = TwitchManager.Current.GetEventPreset(array[i]);
			if (eventPreset != null && (twitchEventEntry.EventName != "" || twitchEventEntry.SPAmount > 0 || twitchEventEntry.PPAmount > 0 || twitchEventEntry.PimpPotAdd > 0 || twitchEventEntry.BitPotAdd > 0 || twitchEventEntry.CooldownAdd > 0))
			{
				if (nodeName == "bit_event")
				{
					twitchEventEntry.EventType = BaseTwitchEventEntry.EventTypes.Bits;
					eventPreset.AddBitEvent(twitchEventEntry);
				}
				else if (nodeName == "raid_event")
				{
					twitchEventEntry.EventType = BaseTwitchEventEntry.EventTypes.Raid;
					eventPreset.AddRaidEvent(twitchEventEntry);
				}
				else if (nodeName == "charity_event")
				{
					twitchEventEntry.EventType = BaseTwitchEventEntry.EventTypes.Charity;
					eventPreset.AddCharityEvent(twitchEventEntry);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseHypeTrainEvent(XElement e, string nodeName)
	{
		TwitchHypeTrainEventEntry twitchHypeTrainEventEntry = new TwitchHypeTrainEventEntry();
		if (e.HasAttribute("start_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("start_amount"), out twitchHypeTrainEventEntry.StartAmount);
		}
		if (e.HasAttribute("end_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("end_amount"), out twitchHypeTrainEventEntry.EndAmount);
		}
		if (e.HasAttribute("event_name"))
		{
			twitchHypeTrainEventEntry.EventName = e.GetAttribute("event_name");
		}
		if (e.HasAttribute("event_title"))
		{
			twitchHypeTrainEventEntry.EventTitle = e.GetAttribute("event_title");
		}
		if (e.HasAttribute("event_title_key"))
		{
			twitchHypeTrainEventEntry.EventTitle = Localization.Get(e.GetAttribute("event_title_key"), false, null);
		}
		if (e.HasAttribute("safe_allowed"))
		{
			twitchHypeTrainEventEntry.SafeAllowed = StringParsers.ParseBool(e.GetAttribute("safe_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("starting_cooldown_allowed"))
		{
			twitchHypeTrainEventEntry.StartingCooldownAllowed = StringParsers.ParseBool(e.GetAttribute("starting_cooldown_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("cooldown_allowed"))
		{
			twitchHypeTrainEventEntry.CooldownAllowed = StringParsers.ParseBool(e.GetAttribute("cooldown_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("vote_allowed"))
		{
			twitchHypeTrainEventEntry.VoteEventAllowed = StringParsers.ParseBool(e.GetAttribute("vote_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("pp_add_amount"))
		{
			twitchHypeTrainEventEntry.PPAmount = StringParsers.ParseSInt32(e.GetAttribute("pp_add_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("sp_add_amount"))
		{
			twitchHypeTrainEventEntry.SPAmount = StringParsers.ParseSInt32(e.GetAttribute("sp_add_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("pimp_pot_add"))
		{
			twitchHypeTrainEventEntry.PimpPotAdd = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("bit_pot_add"))
		{
			twitchHypeTrainEventEntry.BitPotAdd = StringParsers.ParseSInt32(e.GetAttribute("bit_pot_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("cooldown_add"))
		{
			twitchHypeTrainEventEntry.CooldownAdd = StringParsers.ParseSInt32(e.GetAttribute("cooldown_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("reward_amount"))
		{
			twitchHypeTrainEventEntry.RewardAmount = StringParsers.ParseSInt32(e.GetAttribute("reward_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("reward_type"))
		{
			twitchHypeTrainEventEntry.RewardType = (TwitchAction.PointTypes)Enum.Parse(typeof(TwitchAction.PointTypes), e.GetAttribute("reward_type"));
		}
		string text = "";
		if (e.HasAttribute("presets"))
		{
			text = e.GetAttribute("presets");
		}
		if (text == "")
		{
			return;
		}
		string[] array = text.Split(',', StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			TwitchEventPreset eventPreset = TwitchManager.Current.GetEventPreset(array[i]);
			if (eventPreset != null && (twitchHypeTrainEventEntry.EventName != "" || twitchHypeTrainEventEntry.SPAmount > 0 || twitchHypeTrainEventEntry.PPAmount > 0 || twitchHypeTrainEventEntry.PimpPotAdd > 0 || twitchHypeTrainEventEntry.BitPotAdd > 0 || twitchHypeTrainEventEntry.CooldownAdd > 0))
			{
				twitchHypeTrainEventEntry.EventType = BaseTwitchEventEntry.EventTypes.HypeTrain;
				eventPreset.AddHypeTrainEvent(twitchHypeTrainEventEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseCreatorGoalEvent(XElement e, string nodeName)
	{
		TwitchCreatorGoalEventEntry twitchCreatorGoalEventEntry = new TwitchCreatorGoalEventEntry();
		if (e.HasAttribute("goal_type"))
		{
			twitchCreatorGoalEventEntry.GoalType = e.GetAttribute("goal_type").ToLower();
		}
		else
		{
			twitchCreatorGoalEventEntry.GoalType = "Subs";
		}
		if (e.HasAttribute("event_name"))
		{
			twitchCreatorGoalEventEntry.EventName = e.GetAttribute("event_name");
		}
		if (e.HasAttribute("event_title"))
		{
			twitchCreatorGoalEventEntry.EventTitle = e.GetAttribute("event_title");
		}
		if (e.HasAttribute("event_title_key"))
		{
			twitchCreatorGoalEventEntry.EventTitle = Localization.Get(e.GetAttribute("event_title_key"), false, null);
		}
		if (e.HasAttribute("safe_allowed"))
		{
			twitchCreatorGoalEventEntry.SafeAllowed = StringParsers.ParseBool(e.GetAttribute("safe_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("starting_cooldown_allowed"))
		{
			twitchCreatorGoalEventEntry.StartingCooldownAllowed = StringParsers.ParseBool(e.GetAttribute("starting_cooldown_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("cooldown_allowed"))
		{
			twitchCreatorGoalEventEntry.CooldownAllowed = StringParsers.ParseBool(e.GetAttribute("cooldown_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("vote_allowed"))
		{
			twitchCreatorGoalEventEntry.VoteEventAllowed = StringParsers.ParseBool(e.GetAttribute("vote_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("pp_add_amount"))
		{
			twitchCreatorGoalEventEntry.PPAmount = StringParsers.ParseSInt32(e.GetAttribute("pp_add_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("sp_add_amount"))
		{
			twitchCreatorGoalEventEntry.SPAmount = StringParsers.ParseSInt32(e.GetAttribute("sp_add_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("pimp_pot_add"))
		{
			twitchCreatorGoalEventEntry.PimpPotAdd = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("bit_pot_add"))
		{
			twitchCreatorGoalEventEntry.BitPotAdd = StringParsers.ParseSInt32(e.GetAttribute("bit_pot_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("cooldown_add"))
		{
			twitchCreatorGoalEventEntry.CooldownAdd = StringParsers.ParseSInt32(e.GetAttribute("cooldown_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("reward_amount"))
		{
			twitchCreatorGoalEventEntry.RewardAmount = StringParsers.ParseSInt32(e.GetAttribute("reward_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("reward_type"))
		{
			twitchCreatorGoalEventEntry.RewardType = (TwitchAction.PointTypes)Enum.Parse(typeof(TwitchAction.PointTypes), e.GetAttribute("reward_type"));
		}
		string text = "";
		if (e.HasAttribute("presets"))
		{
			text = e.GetAttribute("presets");
		}
		if (text == "")
		{
			return;
		}
		string[] array = text.Split(',', StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			TwitchEventPreset eventPreset = TwitchManager.Current.GetEventPreset(array[i]);
			if (eventPreset != null && (twitchCreatorGoalEventEntry.EventName != "" || twitchCreatorGoalEventEntry.SPAmount > 0 || twitchCreatorGoalEventEntry.PPAmount > 0 || twitchCreatorGoalEventEntry.PimpPotAdd > 0 || twitchCreatorGoalEventEntry.BitPotAdd > 0 || twitchCreatorGoalEventEntry.CooldownAdd > 0))
			{
				twitchCreatorGoalEventEntry.EventType = BaseTwitchEventEntry.EventTypes.CreatorGoal;
				eventPreset.AddCreatorGoalEvent(twitchCreatorGoalEventEntry);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseSubEvent(XElement e, string nodeName)
	{
		TwitchSubEventEntry twitchSubEventEntry = new TwitchSubEventEntry();
		if (e.HasAttribute("start_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("start_amount"), out twitchSubEventEntry.StartAmount);
		}
		if (e.HasAttribute("end_amount"))
		{
			StringParsers.TryParseSInt32(e.GetAttribute("end_amount"), out twitchSubEventEntry.EndAmount);
		}
		if (e.HasAttribute("event_name"))
		{
			twitchSubEventEntry.EventName = e.GetAttribute("event_name");
		}
		if (e.HasAttribute("event_title"))
		{
			twitchSubEventEntry.EventTitle = e.GetAttribute("event_title");
		}
		if (e.HasAttribute("event_title_key"))
		{
			twitchSubEventEntry.EventTitle = Localization.Get(e.GetAttribute("event_title_key"), false, null);
		}
		if (e.HasAttribute("safe_allowed"))
		{
			twitchSubEventEntry.SafeAllowed = StringParsers.ParseBool(e.GetAttribute("safe_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("cooldown_allowed"))
		{
			twitchSubEventEntry.CooldownAllowed = StringParsers.ParseBool(e.GetAttribute("cooldown_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("vote_allowed"))
		{
			twitchSubEventEntry.VoteEventAllowed = StringParsers.ParseBool(e.GetAttribute("vote_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("rewards_bit_pot"))
		{
			twitchSubEventEntry.RewardsBitPot = StringParsers.ParseBool(e.GetAttribute("rewards_bit_pot"), 0, -1, true);
		}
		if (e.HasAttribute("pp_add_amount"))
		{
			twitchSubEventEntry.PPAmount = StringParsers.ParseSInt32(e.GetAttribute("pp_add_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("sp_add_amount"))
		{
			twitchSubEventEntry.SPAmount = StringParsers.ParseSInt32(e.GetAttribute("sp_add_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("pimp_pot_add"))
		{
			twitchSubEventEntry.PimpPotAdd = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("bit_pot_add"))
		{
			twitchSubEventEntry.BitPotAdd = StringParsers.ParseSInt32(e.GetAttribute("bit_pot_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("cooldown_add"))
		{
			twitchSubEventEntry.CooldownAdd = StringParsers.ParseSInt32(e.GetAttribute("cooldown_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("sub_tier"))
		{
			twitchSubEventEntry.SubTier = (TwitchSubEventEntry.SubTierTypes)Enum.Parse(typeof(TwitchSubEventEntry.SubTierTypes), e.GetAttribute("sub_tier"));
		}
		string text = "";
		if (e.HasAttribute("presets"))
		{
			text = e.GetAttribute("presets");
		}
		if (text == "")
		{
			return;
		}
		string[] array = text.Split(',', StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			TwitchEventPreset eventPreset = TwitchManager.Current.GetEventPreset(array[i]);
			if (eventPreset != null && (twitchSubEventEntry.EventName != "" || twitchSubEventEntry.SPAmount > 0 || twitchSubEventEntry.PPAmount > 0 || twitchSubEventEntry.PimpPotAdd > 0 || twitchSubEventEntry.BitPotAdd > 0 || twitchSubEventEntry.CooldownAdd > 0))
			{
				if (nodeName == "sub_event")
				{
					twitchSubEventEntry.EventType = BaseTwitchEventEntry.EventTypes.Subs;
					eventPreset.AddSubEvent(twitchSubEventEntry);
				}
				else if (nodeName == "gift_sub_event")
				{
					twitchSubEventEntry.EventType = BaseTwitchEventEntry.EventTypes.GiftSubs;
					eventPreset.AddGiftSubEvent(twitchSubEventEntry);
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static void ParseChannelPointEvent(XElement e)
	{
		TwitchChannelPointEventEntry twitchChannelPointEventEntry = new TwitchChannelPointEventEntry();
		if (e.HasAttribute("channel_point_title"))
		{
			twitchChannelPointEventEntry.ChannelPointTitle = e.GetAttribute("channel_point_title");
		}
		if (e.HasAttribute("channel_point_title_key"))
		{
			twitchChannelPointEventEntry.ChannelPointTitle = Localization.Get(e.GetAttribute("channel_point_title_key"), false, null);
		}
		if (e.HasAttribute("event_name"))
		{
			twitchChannelPointEventEntry.EventName = e.GetAttribute("event_name");
		}
		if (e.HasAttribute("event_title"))
		{
			twitchChannelPointEventEntry.EventTitle = e.GetAttribute("event_title");
		}
		if (e.HasAttribute("event_title_key"))
		{
			twitchChannelPointEventEntry.EventTitle = Localization.Get(e.GetAttribute("event_title_key"), false, null);
		}
		if (e.HasAttribute("pp_add_amount"))
		{
			twitchChannelPointEventEntry.PPAmount = StringParsers.ParseSInt32(e.GetAttribute("pp_add_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("sp_add_amount"))
		{
			twitchChannelPointEventEntry.SPAmount = StringParsers.ParseSInt32(e.GetAttribute("sp_add_amount"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("safe_allowed"))
		{
			twitchChannelPointEventEntry.SafeAllowed = StringParsers.ParseBool(e.GetAttribute("safe_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("cooldown_allowed"))
		{
			twitchChannelPointEventEntry.CooldownAllowed = StringParsers.ParseBool(e.GetAttribute("cooldown_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("vote_allowed"))
		{
			twitchChannelPointEventEntry.VoteEventAllowed = StringParsers.ParseBool(e.GetAttribute("vote_allowed"), 0, -1, true);
		}
		if (e.HasAttribute("pimp_pot_add"))
		{
			twitchChannelPointEventEntry.PimpPotAdd = StringParsers.ParseSInt32(e.GetAttribute("pimp_pot_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("bit_pot_add"))
		{
			twitchChannelPointEventEntry.BitPotAdd = StringParsers.ParseSInt32(e.GetAttribute("bit_pot_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("cooldown_add"))
		{
			twitchChannelPointEventEntry.CooldownAdd = StringParsers.ParseSInt32(e.GetAttribute("cooldown_add"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("cost"))
		{
			twitchChannelPointEventEntry.Cost = StringParsers.ParseSInt32(e.GetAttribute("cost"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("max_per_stream"))
		{
			twitchChannelPointEventEntry.MaxPerStream = StringParsers.ParseSInt32(e.GetAttribute("max_per_stream"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("max_per_user_per_stream"))
		{
			twitchChannelPointEventEntry.MaxPerUserPerStream = StringParsers.ParseSInt32(e.GetAttribute("max_per_user_per_stream"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("global_cooldown"))
		{
			twitchChannelPointEventEntry.GlobalCooldown = StringParsers.ParseSInt32(e.GetAttribute("global_cooldown"), 0, -1, NumberStyles.Integer);
		}
		if (e.HasAttribute("auto_create"))
		{
			twitchChannelPointEventEntry.AutoCreate = StringParsers.ParseBool(e.GetAttribute("auto_create"), 0, -1, true);
		}
		string text = "";
		if (e.HasAttribute("presets"))
		{
			text = e.GetAttribute("presets");
		}
		if (text == "")
		{
			return;
		}
		string[] array = text.Split(',', StringSplitOptions.None);
		for (int i = 0; i < array.Length; i++)
		{
			TwitchEventPreset eventPreset = TwitchManager.Current.GetEventPreset(array[i]);
			if (eventPreset != null && (twitchChannelPointEventEntry.EventName != "" || twitchChannelPointEventEntry.SPAmount > 0 || twitchChannelPointEventEntry.PPAmount > 0 || twitchChannelPointEventEntry.PimpPotAdd > 0 || twitchChannelPointEventEntry.BitPotAdd > 0 || twitchChannelPointEventEntry.CooldownAdd > 0))
			{
				twitchChannelPointEventEntry.EventType = BaseTwitchEventEntry.EventTypes.ChannelPoints;
				eventPreset.AddChannelPointEvent(twitchChannelPointEventEntry);
			}
		}
	}
}
