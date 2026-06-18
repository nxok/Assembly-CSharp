using System;
using UnityEngine.Scripting;

namespace Quests.Requirements
{
	[Preserve]
	public class RequirementLevel : BaseRequirement
	{
		public override void SetupRequirement()
		{
			string text = Localization.Get("RequirementLevel_keyword", false, null);
			this.expectedLevel = Convert.ToInt32(base.Value);
			base.Description = string.Format("{0} {1}", text, this.expectedLevel);
		}

		public override bool CheckRequirement()
		{
			return !base.OwnerQuest.Active || XUiM_Player.GetLevel(base.OwnerQuest.OwnerJournal.OwnerPlayer) >= this.expectedLevel;
		}

		public override BaseRequirement Clone()
		{
			return new RequirementLevel
			{
				ID = base.ID,
				Value = base.Value,
				Phase = base.Phase
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public int expectedLevel;
	}
}
