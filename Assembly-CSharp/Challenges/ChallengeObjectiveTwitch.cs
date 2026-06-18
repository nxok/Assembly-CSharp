using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveTwitch : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Twitch;
			}
		}

		public override string DescriptionText
		{
			get
			{
				switch (this.TwitchObjectiveType)
				{
				case TwitchObjectiveTypes.Enabled:
					return Localization.Get("challengeObjectiveTwitchEnabled", false, null);
				case TwitchObjectiveTypes.EnableExtras:
					return Localization.Get("challengeObjectiveTwitchEnableExtras", false, null);
				case TwitchObjectiveTypes.HelperReward:
					return Localization.Get("challengeObjectiveTwitchHelperRewards", false, null);
				case TwitchObjectiveTypes.ChannelPointRedeems:
					return Localization.Get("challengeObjectiveTwitchChannelPointRedeems", false, null);
				case TwitchObjectiveTypes.VoteComplete:
					return Localization.Get("challengeObjectiveTwitchVotesCompleted", false, null);
				case TwitchObjectiveTypes.PimpPot:
					return Localization.Get("challengeObjectiveTwitchPimpPotRewarded", false, null);
				case TwitchObjectiveTypes.BitPot:
					return Localization.Get("challengeObjectiveTwitchBitPotRewarded", false, null);
				case TwitchObjectiveTypes.DefeatBossHorde:
					return Localization.Get("challengeObjectiveTwitchBossHordesDefeated", false, null);
				case TwitchObjectiveTypes.GoodAction:
					return Localization.Get("challengeObjectiveTwitchGoodActions", false, null);
				case TwitchObjectiveTypes.BadAction:
					return Localization.Get("challengeObjectiveTwitchBadActions", false, null);
				default:
					return "";
				}
			}
		}

		public override ChallengeClass.UINavTypes NavType
		{
			get
			{
				if (this.TwitchObjectiveType == TwitchObjectiveTypes.EnableExtras)
				{
					return ChallengeClass.UINavTypes.TwitchActions;
				}
				return ChallengeClass.UINavTypes.None;
			}
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.TwitchEventReceive += this.Current_TwitchEventReceive;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.TwitchEventReceive -= this.Current_TwitchEventReceive;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_TwitchEventReceive(TwitchObjectiveTypes action, string param)
		{
			if (this.CheckBaseRequirements())
			{
				return;
			}
			if (action == this.TwitchObjectiveType && (this.Param == "" || this.Param.EqualsCaseInsensitive(param)))
			{
				int num = base.Current;
				base.Current = num + 1;
			}
			if (base.Current >= this.MaxCount)
			{
				base.Current = this.MaxCount;
				this.CheckObjectiveComplete(true);
			}
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("objective_type"))
			{
				this.TwitchObjectiveType = (TwitchObjectiveTypes)Enum.Parse(typeof(TwitchObjectiveTypes), e.GetAttribute("objective_type"), true);
			}
			if (e.HasAttribute("objective_param"))
			{
				this.Param = e.GetAttribute("objective_param");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveTwitch
			{
				TwitchObjectiveType = this.TwitchObjectiveType,
				Param = this.Param
			};
		}

		public TwitchObjectiveTypes TwitchObjectiveType;

		public string Param = "";
	}
}
