using System;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveSurvive : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Survive;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return Localization.Get("challengeObjectiveSurvive", false, null) + ":";
			}
		}

		public override string StatusText
		{
			get
			{
				return string.Format("{0}/{1}", XUiM_PlayerBuffs.GetTimeString((float)this.current * 60f), XUiM_PlayerBuffs.GetTimeString((float)this.MaxCount * 60f));
			}
		}

		public override void Init()
		{
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.TimeSurvive += this.Current_TimeSurvive;
			base.Current = (int)base.Player.longestLife;
			if (base.Current >= this.MaxCount)
			{
				base.Current = this.MaxCount;
				this.CheckObjectiveComplete(true);
			}
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.TimeSurvive -= this.Current_TimeSurvive;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_TimeSurvive(float val)
		{
			if (this.CheckBaseRequirements())
			{
				return;
			}
			base.Current = (int)val;
			if (base.Current >= this.MaxCount)
			{
				base.Current = this.MaxCount;
				this.CheckObjectiveComplete(true);
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveSurvive();
		}
	}
}
