using System;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveBloodmoon : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Bloodmoon;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return Localization.Get("challengeObjectiveBloodMoonCompleted", false, null) + ":";
			}
		}

		public override string StatusText
		{
			get
			{
				if (!EntityFactory.EnemySpawnMode)
				{
					return "--";
				}
				return base.StatusText;
			}
		}

		public override void Init()
		{
		}

		public override void HandleAddHooks()
		{
			if (!EntityFactory.EnemySpawnMode)
			{
				base.Current = this.MaxCount;
				base.Complete = true;
				this.Owner.HandleComplete(false);
				if (this.Owner.ChallengeState == Challenge.ChallengeStates.Completed)
				{
					this.Owner.AutoCompleted = true;
					this.Owner.ChallengeState = Challenge.ChallengeStates.Redeemed;
					return;
				}
			}
			else
			{
				QuestEventManager.Current.BloodMoonSurvive += this.Current_BloodMoonSurvive;
			}
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.BloodMoonSurvive -= this.Current_BloodMoonSurvive;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_BloodMoonSurvive()
		{
			if (this.CheckBaseRequirements())
			{
				return;
			}
			int num = base.Current;
			base.Current = num + 1;
			this.CheckObjectiveComplete(true);
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveBloodmoon();
		}
	}
}
