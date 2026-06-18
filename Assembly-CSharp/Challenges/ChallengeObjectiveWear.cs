using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveWear : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Wear;
			}
		}

		public override string DescriptionText
		{
			get
			{
				if (this.expectedItemClass == null)
				{
					return Localization.Get("challengeObjectiveWear", false, null) + " " + this.wearName + ":";
				}
				return Localization.Get("challengeObjectiveWear", false, null) + " " + this.expectedItemClass.GetLocalizedItemName() + ":";
			}
		}

		public override void Init()
		{
			this.expectedItem = ItemClass.GetItem(this.itemClassID, false);
			this.expectedItemClass = ItemClass.GetItemClass(this.itemClassID, false);
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.WearItem -= this.Current_WearItem;
			XUi xui = LocalPlayerUI.GetUIForPlayer(this.Owner.Owner.Player).xui;
			XUiM_PlayerInventory playerInventory = xui.PlayerInventory;
			if (xui.PlayerEquipment.IsWearing(this.expectedItem))
			{
				base.Current = this.MaxCount;
				this.CheckObjectiveComplete(true);
				return;
			}
			QuestEventManager.Current.WearItem += this.Current_WearItem;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.WearItem -= this.Current_WearItem;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_WearItem(ItemValue itemValue)
		{
			if (this.CheckBaseRequirements())
			{
				return;
			}
			if (!this.armorTags.IsEmpty && !this.armorTags.Test_AnySet(itemValue.ItemClass.ItemTags))
			{
				return;
			}
			if (this.itemClassID != "" && this.expectedItem.type != itemValue.type)
			{
				return;
			}
			base.Current = this.MaxCount;
			this.CheckObjectiveComplete(true);
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("item"))
			{
				this.itemClassID = e.GetAttribute("item");
			}
			if (e.HasAttribute("tags"))
			{
				this.armorTags = FastTags<TagGroup.Global>.Parse(e.GetAttribute("tags"));
			}
			if (e.HasAttribute("wear_name_key"))
			{
				this.wearName = Localization.Get(e.GetAttribute("wear_name_key"), false, null);
				return;
			}
			if (e.HasAttribute("wear_name"))
			{
				this.wearName = e.GetAttribute("wear_name");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveWear
			{
				itemClassID = this.itemClassID,
				armorTags = this.armorTags,
				expectedItem = this.expectedItem,
				expectedItemClass = this.expectedItemClass,
				wearName = this.wearName
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemValue expectedItem = ItemValue.None;

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemClass expectedItemClass;

		public string itemClassID = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string wearName = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> armorTags;
	}
}
