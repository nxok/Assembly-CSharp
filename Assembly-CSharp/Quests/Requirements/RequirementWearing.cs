using System;
using UnityEngine.Scripting;

namespace Quests.Requirements
{
	[Preserve]
	public class RequirementWearing : BaseRequirement
	{
		public override void SetupRequirement()
		{
			string text = Localization.Get("RequirementWearing_keyword", false, null);
			this.expectedItem = ItemClass.GetItem(base.ID, false);
			this.expectedItemClass = ItemClass.GetItemClass(base.ID, false);
			base.Description = string.Format("{0} {1}", text, this.expectedItemClass.GetLocalizedItemName());
		}

		public override bool CheckRequirement()
		{
			return !base.OwnerQuest.Active || LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerEquipment.IsWearing(this.expectedItem);
		}

		public override BaseRequirement Clone()
		{
			return new RequirementWearing
			{
				ID = base.ID,
				Value = base.Value,
				Phase = base.Phase
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemValue expectedItem = ItemValue.None;

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemClass expectedItemClass;
	}
}
