using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace Quests.Requirements
{
	[Preserve]
	public class RequirementGroup : BaseRequirement
	{
		public RequirementGroup.GroupOperator Operator { get; set; }

		public override void SetupRequirement()
		{
			this.Operator = EnumUtils.Parse<RequirementGroup.GroupOperator>(base.Value, false);
			for (int i = 0; i < this.ChildRequirements.Count; i++)
			{
				this.ChildRequirements[i].OwnerQuest = base.OwnerQuest;
				this.ChildRequirements[i].SetupRequirement();
			}
			if (string.IsNullOrEmpty(base.ID))
			{
				if (this.ChildRequirements.Count > 0)
				{
					base.Description = this.ChildRequirements[0].Description;
					return;
				}
			}
			else
			{
				base.Description = Localization.Get(base.ID, false, null);
			}
		}

		public override bool CheckRequirement()
		{
			if (!base.OwnerQuest.Active)
			{
				return true;
			}
			bool flag = this.Operator == RequirementGroup.GroupOperator.AND;
			for (int i = 0; i < this.ChildRequirements.Count; i++)
			{
				bool flag2 = this.ChildRequirements[i].CheckRequirement();
				if (this.Operator == RequirementGroup.GroupOperator.AND)
				{
					if (!flag2)
					{
						return false;
					}
				}
				else if (this.Operator == RequirementGroup.GroupOperator.OR && flag2)
				{
					return true;
				}
			}
			return flag;
		}

		public override BaseRequirement Clone()
		{
			RequirementGroup requirementGroup = new RequirementGroup();
			requirementGroup.ID = base.ID;
			requirementGroup.Value = base.Value;
			requirementGroup.Phase = base.Phase;
			for (int i = 0; i < this.ChildRequirements.Count; i++)
			{
				requirementGroup.ChildRequirements.Add(this.ChildRequirements[i].Clone());
			}
			return requirementGroup;
		}

		public List<BaseRequirement> ChildRequirements = new List<BaseRequirement>();

		public enum GroupOperator
		{
			AND,
			OR
		}
	}
}
