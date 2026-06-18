using System;
using UnityEngine.Scripting;

namespace Quests.Requirements
{
	[Preserve]
	public class RequirementHolding : BaseRequirement
	{
		public override void SetupRequirement()
		{
			XUi xui = LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui;
			string text = Localization.Get("RequirementHolding_keyword", false, null);
			this.expectedItem = ((base.ID != "" && base.ID != null) ? ItemClass.GetItem(base.ID, false) : xui.PlayerInventory.Toolbelt.GetBareHandItemValue());
			this.expectedItemClass = ((base.ID != "" && base.ID != null) ? ItemClass.GetItemClass(base.ID, false) : xui.PlayerInventory.Toolbelt.GetBareHandItem());
			if (base.ID == "" || base.ID == null)
			{
				base.Description = "Bare Hands";
				return;
			}
			base.Description = string.Format("{0} {1}", text, this.expectedItemClass.GetLocalizedItemName());
		}

		public override bool CheckRequirement()
		{
			return !base.OwnerQuest.Active || LocalPlayerUI.GetUIForPlayer(base.OwnerQuest.OwnerJournal.OwnerPlayer).xui.PlayerInventory.Toolbelt.holdingItemStack.itemValue.type == this.expectedItem.type;
		}

		public override BaseRequirement Clone()
		{
			return new RequirementHolding
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
