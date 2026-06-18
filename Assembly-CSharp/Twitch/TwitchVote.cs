using System;
using System.Collections.Generic;
using UnityEngine;

namespace Twitch
{
	public class TwitchVote
	{
		public string VoteDescription
		{
			get
			{
				switch (this.DisplayType)
				{
				case TwitchVote.VoteDisplayTypes.Single:
				case TwitchVote.VoteDisplayTypes.Special:
					return this.Title;
				case TwitchVote.VoteDisplayTypes.GoodBad:
					return this.Title + " / " + this.VoteLine1;
				case TwitchVote.VoteDisplayTypes.HordeBuffed:
					return this.Title + " (" + this.VoteLine1 + ")";
				default:
					return "";
				}
			}
		}

		public string VoteHeight
		{
			get
			{
				TwitchVote.VoteDisplayTypes displayType = this.DisplayType;
				if (displayType == TwitchVote.VoteDisplayTypes.Single || displayType == TwitchVote.VoteDisplayTypes.Special)
				{
					return "-50";
				}
				return "-90";
			}
		}

		public bool IsInPreset(TwitchVotePreset preset)
		{
			return (this.PresetNames == null && !preset.IsEmpty) || (this.PresetNames != null && this.PresetNames.Contains(preset.Name));
		}

		public bool CanUse(int hour, int gamestage, EntityPlayer player)
		{
			if ((this.StartGameStage != -1 && this.StartGameStage > gamestage) || (this.EndGameStage != -1 && this.EndGameStage < gamestage) || hour < this.AllowedStartHour || hour > this.AllowedEndHour || (this.MaxTimesPerDay != -1 && this.MaxTimesPerDay <= this.CurrentDayCount))
			{
				return false;
			}
			if (this.tempCooldown > 0f && TwitchManager.Current.CurrentUnityTime - this.tempCooldownSet < this.tempCooldown)
			{
				return false;
			}
			this.tempCooldown = 0f;
			this.tempCooldownSet = 0f;
			if (this.TwitchRequirements == null)
			{
				return true;
			}
			for (int i = 0; i < this.TwitchRequirements.Count; i++)
			{
				if (!this.TwitchRequirements[i].CanPerform(player))
				{
					return false;
				}
			}
			return true;
		}

		public virtual void ParseProperties(DynamicProperties properties)
		{
			this.Properties = properties;
			string text = "";
			string text2 = "";
			properties.ParseLocalizedString(TwitchVote.PropTitleVarKey, ref text);
			properties.ParseLocalizedString(TwitchVote.PropDisplayVarKey, ref text2);
			if (text == "" && text2 != "")
			{
				text = text2;
			}
			else if (text != "" && text2 == "")
			{
				text2 = text;
			}
			properties.ParseLocalizedString(TwitchVote.PropTitleKey, ref this.Title);
			if (this.Title == "")
			{
				properties.ParseString(TwitchVote.PropTitle, ref this.Title);
			}
			string text3 = "";
			properties.ParseLocalizedString(TwitchVote.PropTitleFormatKey, ref text3);
			if (text3 != "")
			{
				this.Title = string.Format(text3, text);
			}
			properties.ParseLocalizedString(TwitchVote.PropDescriptionKey, ref this.Description);
			if (this.Description == "")
			{
				properties.ParseString(TwitchVote.PropDescription, ref this.Description);
			}
			text3 = "";
			properties.ParseLocalizedString(TwitchVote.PropDescriptionFormatKey, ref text3);
			if (text3 != "")
			{
				this.Description = string.Format(text3, text);
			}
			if (properties.Values.ContainsKey(TwitchVote.PropDisplayKey))
			{
				this.Display = Localization.Get(properties.Values[TwitchVote.PropDisplayKey], false, null);
			}
			else
			{
				properties.ParseString(TwitchVote.PropDisplay, ref this.Display);
			}
			text3 = "";
			properties.ParseLocalizedString(TwitchVote.PropDisplayFormatKey, ref text3);
			if (text3 != "")
			{
				this.Display = string.Format(text3, text2);
			}
			properties.ParseString(TwitchVote.PropEventName, ref this.GameEvent);
			properties.ParseString(TwitchVote.PropGroup, ref this.Group);
			properties.ParseInt(TwitchVote.PropStartGameStage, ref this.StartGameStage);
			properties.ParseInt(TwitchVote.PropEndGameStage, ref this.EndGameStage);
			properties.ParseInt(TwitchVote.PropAllowedStartHour, ref this.AllowedStartHour);
			properties.ParseInt(TwitchVote.PropAllowedEndHour, ref this.AllowedEndHour);
			properties.ParseString(TwitchVote.PropVoteLine1, ref this.VoteLine1);
			properties.ParseString(TwitchVote.PropVoteLine2, ref this.VoteLine2);
			properties.ParseEnum<TwitchVote.VoteDisplayTypes>(TwitchVote.PropDisplayType, ref this.DisplayType);
			properties.ParseInt(TwitchVote.PropMaxTimesPerDay, ref this.MaxTimesPerDay);
			string text4 = "";
			properties.ParseString(TwitchVote.PropVoteType, ref text4);
			if (text4 != "")
			{
				this.VoteTypes = text4.Split(',', StringSplitOptions.None);
				this.MainVoteType = TwitchManager.Current.VotingManager.GetVoteType(this.VoteTypes[0]);
			}
			properties.ParseBool(TwitchVote.PropEnabled, ref this.Enabled);
			this.OriginalEnabled = this.Enabled;
			properties.ParseString(TwitchVote.PropTitleColor, ref this.TitleColor);
			if (this.Display == "")
			{
				this.Display = this.Title;
			}
			this.Properties.ParseLocalizedString(TwitchVote.PropVoteLine1Key, ref this.VoteLine1);
			this.Properties.ParseLocalizedString(TwitchVote.PropVoteLine2Key, ref this.VoteLine2);
			this.VoteTip = this.Description;
			this.Properties.ParseString(TwitchVote.PropVoteTip, ref this.VoteTip);
			this.Properties.ParseLocalizedString(TwitchVote.PropVoteTipKey, ref this.VoteTip);
			if (properties.Values.ContainsKey(TwitchVote.PropPresets))
			{
				this.PresetNames = new List<string>();
				this.PresetNames.AddRange(properties.Values[TwitchVote.PropPresets].Split(',', StringSplitOptions.None));
			}
			if (!GameEventManager.GameEventSequences.ContainsKey(this.GameEvent))
			{
				Debug.LogError(string.Format("TwitchVote: Game Event Sequence '{0}' does not exist!", this.GameEvent));
			}
		}

		public void AddCooldownAddition(TwitchActionCooldownAddition newCooldown)
		{
			if (this.CooldownAdditions == null)
			{
				this.CooldownAdditions = new List<TwitchActionCooldownAddition>();
			}
			this.CooldownAdditions.Add(newCooldown);
		}

		public void AddRequirement(BaseTwitchRequirement requirement)
		{
			if (this.TwitchRequirements == null)
			{
				this.TwitchRequirements = new List<BaseTwitchRequirement>();
			}
			this.TwitchRequirements.Add(requirement);
		}

		public void HandleVoteComplete()
		{
			if (this.CooldownAdditions != null)
			{
				float actionCooldownModifier = TwitchManager.Current.ActionCooldownModifier;
				for (int i = 0; i < this.CooldownAdditions.Count; i++)
				{
					TwitchActionCooldownAddition twitchActionCooldownAddition = this.CooldownAdditions[i];
					if (twitchActionCooldownAddition.IsAction && TwitchActionManager.TwitchActions.ContainsKey(twitchActionCooldownAddition.ActionName))
					{
						TwitchAction twitchAction = TwitchActionManager.TwitchActions[twitchActionCooldownAddition.ActionName];
						twitchAction.tempCooldown = twitchActionCooldownAddition.CooldownTime * actionCooldownModifier;
						twitchAction.tempCooldownSet = Time.time;
					}
					else if (!twitchActionCooldownAddition.IsAction && TwitchActionManager.TwitchVotes.ContainsKey(twitchActionCooldownAddition.ActionName))
					{
						TwitchVote twitchVote = TwitchActionManager.TwitchVotes[twitchActionCooldownAddition.ActionName];
						twitchVote.tempCooldown = twitchActionCooldownAddition.CooldownTime * actionCooldownModifier;
						twitchVote.tempCooldownSet = Time.time;
					}
				}
			}
		}

		public static string PropTitleVarKey = "title_var_key";

		public static string PropDisplayVarKey = "display_var_key";

		public static string PropTitle = "title";

		public static string PropTitleKey = "title_key";

		public static string PropDescription = "description";

		public static string PropDescriptionKey = "description_key";

		public static string PropDisplay = "display";

		public static string PropDisplayKey = "display_key";

		public static string PropEventName = "event_name";

		public static string PropVoteType = "vote_type";

		public static string PropGroup = "group";

		public static string PropTitleFormatKey = "title_format_key";

		public static string PropDescriptionFormatKey = "description_format_key";

		public static string PropDisplayFormatKey = "display_format_key";

		public static string PropStartGameStage = "start_gamestage";

		public static string PropEndGameStage = "end_gamestage";

		public static string PropAllowedStartHour = "allowed_start_hour";

		public static string PropAllowedEndHour = "allowed_end_hour";

		public static string PropDisplayType = "display_type";

		public static string PropTitleColor = "title_color";

		public static string PropVoteLine1 = "line1_desc";

		public static string PropVoteLine1Key = "line1_desc_key";

		public static string PropVoteLine2 = "line2_desc";

		public static string PropVoteLine2Key = "line2_desc_key";

		public static string PropEnabled = "enabled";

		public static string PropMaxTimesPerDay = "max_times_per_day";

		public static string PropVoteTip = "vote_tip";

		public static string PropVoteTipKey = "vote_tip_key";

		public static string PropPresets = "presets";

		public static HashSet<string> ExtendsExcludes = new HashSet<string>
		{
			TwitchVote.PropStartGameStage,
			TwitchVote.PropEndGameStage
		};

		public string VoteName;

		public string Title;

		public string Description;

		public string Display = "";

		public string GameEvent;

		public string[] VoteTypes;

		public TwitchVoteType MainVoteType;

		public string Group = "";

		public bool Enabled = true;

		public bool OriginalEnabled;

		public string TitleColor = "";

		public int StartGameStage = -1;

		public int EndGameStage = -1;

		public int AllowedStartHour;

		public int AllowedEndHour = 24;

		public string VoteLine1 = "";

		public string VoteLine2 = "";

		public int MaxTimesPerDay = -1;

		public int CurrentDayCount;

		public float tempCooldownSet;

		public float tempCooldown;

		public string VoteTip = "";

		public DynamicProperties Properties;

		public List<TwitchActionCooldownAddition> CooldownAdditions;

		public List<BaseTwitchRequirement> TwitchRequirements;

		public TwitchVote.VoteDisplayTypes DisplayType = TwitchVote.VoteDisplayTypes.GoodBad;

		public List<string> PresetNames;

		public enum VoteDisplayTypes
		{
			Single,
			GoodBad,
			Special,
			HordeBuffed
		}
	}
}
