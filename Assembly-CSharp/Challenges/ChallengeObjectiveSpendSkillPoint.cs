using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveSpendSkillPoint : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.SpendSkillPoint;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return string.Format(Localization.Get("ObjectiveSpendSkillPoints_keyword", false, null), Localization.Get("goAnyValue", false, null)) + ":";
			}
		}

		public override void HandleOnCreated()
		{
			base.HandleOnCreated();
			this.CreateRequirements();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CreateRequirements()
		{
			if (!this.ShowRequirements)
			{
				return;
			}
			this.Owner.SetRequirementGroup(new RequirementObjectiveGroupWindowOpen("Skills"));
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.SkillPointSpent += this.Current_SkillPointSpent;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.SkillPointSpent -= this.Current_SkillPointSpent;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_SkillPointSpent(string skillName)
		{
			if (this.progressionName == "" || this.progressionName.EqualsCaseInsensitive(skillName))
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
			if (e.HasAttribute("skill_name"))
			{
				this.progressionName = e.GetAttribute("skill_name");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveSpendSkillPoint
			{
				progressionName = this.progressionName
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string progressionName = "";
	}
}
