using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveMeetTrader : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.MeetTrader;
			}
		}

		public override string DescriptionText
		{
			get
			{
				if (string.IsNullOrEmpty(this.TraderName))
				{
					return Localization.Get("challengeObjectiveMeetAnyTrader", false, null);
				}
				return Localization.Get("challengeObjectiveMeet", false, null) + " " + Localization.Get(this.TraderName, false, null) + ":";
			}
		}

		public override void Init()
		{
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.NPCMeet += this.Current_NPCMeet;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.NPCMeet -= this.Current_NPCMeet;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_NPCMeet(EntityNPC npc)
		{
			if (this.CheckBaseRequirements())
			{
				return;
			}
			if (this.TraderName == "" || npc.EntityName == this.TraderName)
			{
				int num = base.Current;
				base.Current = num + 1;
				if (base.Current >= this.MaxCount)
				{
					base.Current = this.MaxCount;
					this.CheckObjectiveComplete(true);
				}
			}
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("trader_name"))
			{
				this.TraderName = e.GetAttribute("trader_name");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveMeetTrader
			{
				TraderName = this.TraderName
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string TraderName = "";
	}
}
