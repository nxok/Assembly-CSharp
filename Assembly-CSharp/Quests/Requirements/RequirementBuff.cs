using System;
using UnityEngine.Scripting;

namespace Quests.Requirements
{
	[Preserve]
	public class RequirementBuff : BaseRequirement
	{
		public override void SetupRequirement()
		{
			string text = Localization.Get("RequirementBuff_keyword", false, null);
			base.Description = string.Format("{0} {1}", text, BuffManager.GetBuff(base.ID).Name);
		}

		public override bool CheckRequirement()
		{
			return !base.OwnerQuest.Active || base.OwnerQuest.OwnerJournal.OwnerPlayer.Buffs.HasBuff(base.ID);
		}

		public override BaseRequirement Clone()
		{
			return new RequirementBuff
			{
				ID = base.ID,
				Value = base.Value,
				Phase = base.Phase
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string name = "";
	}
}
