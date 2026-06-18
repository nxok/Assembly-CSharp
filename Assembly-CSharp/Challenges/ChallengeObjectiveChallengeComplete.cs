using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveChallengeComplete : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.ChallengeComplete;
			}
		}

		public override string DescriptionText
		{
			get
			{
				string text = Localization.Get("challengeTargetAnyChallenge", false, null);
				if (this.ChallengeName != "")
				{
					if (this.IsGroup)
					{
						ChallengeGroup challengeGroup = ChallengeGroup.s_ChallengeGroups[this.ChallengeName];
						if (challengeGroup != null)
						{
							text = challengeGroup.Title;
						}
					}
					else
					{
						ChallengeClass challenge = ChallengeClass.GetChallenge(this.ChallengeName);
						if (challenge != null)
						{
							text = challenge.Title;
						}
					}
				}
				if (this.IsRedeemed)
				{
					return Localization.Get("challengeObjectiveRedeem", false, null) + " [DECEA3]" + text + "[-]:";
				}
				return Localization.Get("challengeObjectiveComplete", false, null) + " [DECEA3]" + text + "[-]:";
			}
		}

		public override void BaseInit()
		{
			base.BaseInit();
			this.UpdateMax();
		}

		public override void HandleOnCreated()
		{
			base.HandleOnCreated();
			this.CreateRequirements();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void UpdateMax()
		{
			if (this.IsGroup)
			{
				ChallengeGroup challengeGroup = ChallengeGroup.s_ChallengeGroups[this.ChallengeName];
				this.MaxCount = challengeGroup.ChallengeClasses.Count;
				if (this.OwnerClass.ChallengeGroup == challengeGroup)
				{
					this.MaxCount--;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CreateRequirements()
		{
			if (!this.ShowRequirements)
			{
				return;
			}
			this.Owner.SetRequirementGroup(new RequirementObjectiveGroupWindowOpen("Challenges"));
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.ChallengeComplete += this.Current_ChallengeComplete;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.ChallengeComplete -= this.Current_ChallengeComplete;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_ChallengeComplete(ChallengeClass _challenge, bool _isRedeemed)
		{
			if (this.IsGroup)
			{
				base.Current = 0;
				List<ChallengeClass> challengeClasses = ChallengeGroup.s_ChallengeGroups[this.ChallengeName].ChallengeClasses;
				int i = 0;
				while (i < challengeClasses.Count)
				{
					Challenge challenge = this.Owner.Owner.ChallengeDictionary[challengeClasses[i].Name];
					bool flag = false;
					if (challenge.ChallengeState != Challenge.ChallengeStates.Active)
					{
						goto IL_0065;
					}
					if (challenge == this.Owner)
					{
						flag = true;
						goto IL_0065;
					}
					IL_009C:
					i++;
					continue;
					IL_0065:
					if (!flag && (!this.IsRedeemed || challenge.ChallengeState == Challenge.ChallengeStates.Redeemed) && (this.IsRedeemed || challenge.ChallengeState == Challenge.ChallengeStates.Completed))
					{
						int num = base.Current;
						base.Current = num + 1;
						goto IL_009C;
					}
					goto IL_009C;
				}
				if (base.Current >= this.MaxCount)
				{
					base.Current = this.MaxCount;
					this.CheckObjectiveComplete(true);
					return;
				}
			}
			else if ((string.IsNullOrEmpty(this.ChallengeName) || string.Compare(_challenge.Name, this.ChallengeName, true) == 0) && _isRedeemed == this.IsRedeemed)
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
			if (e.HasAttribute("challenge"))
			{
				this.ChallengeName = e.GetAttribute("challenge");
			}
			if (e.HasAttribute("is_group"))
			{
				this.IsGroup = StringParsers.ParseBool(e.GetAttribute("is_group"), 0, -1, true);
				if (this.IsGroup)
				{
					this.MaxCount = -1;
				}
			}
			if (e.HasAttribute("is_redeemed"))
			{
				this.IsRedeemed = StringParsers.ParseBool(e.GetAttribute("is_redeemed"), 0, -1, true);
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveChallengeComplete
			{
				ChallengeName = this.ChallengeName,
				IsRedeemed = this.IsRedeemed,
				IsGroup = this.IsGroup
			};
		}

		public string ChallengeName = "";

		public bool IsGroup;

		public bool IsRedeemed;
	}
}
