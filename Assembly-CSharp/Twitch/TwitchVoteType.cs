using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchVoteType
	{
		public bool IsInPreset(string preset)
		{
			return this.PresetNames == null || this.PresetNames.Contains(preset);
		}

		public bool CanUse()
		{
			return this.Enabled && (this.MaxTimesPerDay == -1 || this.MaxTimesPerDay > this.CurrentDayCount);
		}

		public virtual void ParseProperties(DynamicProperties properties)
		{
			properties.ParseString(TwitchVoteType.PropTitle, ref this.Title);
			if (properties.Values.ContainsKey(TwitchVoteType.PropTitleKey))
			{
				this.Title = Localization.Get(properties.Values[TwitchVoteType.PropTitleKey], false, null);
			}
			properties.ParseString(TwitchVoteType.PropIcon, ref this.Icon);
			properties.ParseBool(TwitchVoteType.PropSpawnBlocked, ref this.SpawnBlocked);
			properties.ParseInt(TwitchVoteType.PropMaxTimesPerDay, ref this.MaxTimesPerDay);
			properties.ParseInt(TwitchVoteType.PropAllowedStartHour, ref this.AllowedStartHour);
			properties.ParseInt(TwitchVoteType.PropAllowedEndHour, ref this.AllowedEndHour);
			properties.ParseBool(TwitchVoteType.PropBloodMoonDay, ref this.BloodMoonDay);
			properties.ParseBool(TwitchVoteType.PropBloodMoonAllowed, ref this.BloodMoonAllowed);
			properties.ParseString(TwitchVoteType.PropGuaranteedGroups, ref this.GuaranteedGroup);
			properties.ParseBool(TwitchVoteType.PropCooldownOnEnd, ref this.CooldownOnEnd);
			properties.ParseBool(TwitchVoteType.PropUseMystery, ref this.UseMystery);
			properties.ParseBool(TwitchVoteType.PropActionLockout, ref this.ActionLockout);
			properties.ParseString(TwitchVoteType.PropGroup, ref this.Group);
			properties.ParseBool(TwitchVoteType.PropEnabled, ref this.Enabled);
			properties.ParseBool(TwitchVoteType.PropAllowedWithActions, ref this.AllowedWithActions);
			properties.ParseInt(TwitchVoteType.PropVoteChoiceCount, ref this.VoteChoiceCount);
			properties.ParseBool(TwitchVoteType.PropIsBoss, ref this.IsBoss);
			properties.ParseBool(TwitchVoteType.PropManualStart, ref this.ManualStart);
			if (properties.Values.ContainsKey(TwitchVoteType.PropPresets))
			{
				this.PresetNames = new List<string>();
				this.PresetNames.AddRange(properties.Values[TwitchVoteType.PropPresets].Split(',', StringSplitOptions.None));
			}
		}

		public static string PropTitle = "title";

		public static string PropTitleKey = "title_key";

		public static string PropSpawnBlocked = "spawn_blocked";

		public static string PropExcludeTimeIndex = "exclude_time_index";

		public static string PropMaxTimesPerDay = "max_times_per_day";

		public static string PropAllowedStartHour = "allowed_start_hour";

		public static string PropAllowedEndHour = "allowed_end_hour";

		public static string PropBloodMoonDay = "blood_moon_day";

		public static string PropBloodMoonAllowed = "blood_moon_allowed";

		public static string PropGuaranteedGroups = "guaranteed_group";

		public static string PropCooldownOnEnd = "cooldown_on_end";

		public static string PropUseMystery = "use_mystery";

		public static string PropActionLockout = "action_lockout";

		public static string PropGroup = "group";

		public static string PropEnabled = "enabled";

		public static string PropVoteChoiceCount = "vote_choice_count";

		public static string PropAllowedWithActions = "allowed_with_actions";

		public static string PropIsBoss = "is_boss";

		public static string PropManualStart = "manual_start";

		public static string PropIcon = "icon";

		public static string PropPresets = "presets";

		public string Name;

		public string Title;

		public string Icon;

		public string Group;

		public bool SpawnBlocked = true;

		public bool BloodMoonDay = true;

		public bool BloodMoonAllowed = true;

		public bool CooldownOnEnd;

		public bool UseMystery;

		public bool ActionLockout;

		public bool AllowedWithActions = true;

		public int MaxTimesPerDay = -1;

		public int AllowedStartHour;

		public int AllowedEndHour = 24;

		public int VoteChoiceCount = 3;

		public int CurrentDayCount;

		public string GuaranteedGroup = "";

		public bool Enabled = true;

		public bool ManualStart;

		public bool IsBoss;

		public List<string> PresetNames;
	}
}
