using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveScrap : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Scrap;
			}
		}

		public override string DescriptionText
		{
			get
			{
				string text = ((this.expectedItemClass != null) ? this.expectedItemClass.GetLocalizedItemName() : Localization.Get("xuiItems", false, null));
				return Localization.Get("challengeObjectiveScrap", false, null) + " " + text + ":";
			}
		}

		public override void Init()
		{
			this.expectedItem = ItemClass.GetItem(this.itemClassID, false);
			this.expectedItemClass = ItemClass.GetItemClass(this.itemClassID, false);
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.ScrapItem += this.Current_ScrapItem;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_ScrapItem(ItemStack stack)
		{
			if (this.CheckBaseRequirements())
			{
				return;
			}
			if (this.expectedItemClass == null || stack.itemValue.type == this.expectedItem.type)
			{
				base.Current += stack.count;
				this.CheckObjectiveComplete(true);
			}
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.ScrapItem -= this.Current_ScrapItem;
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("item"))
			{
				this.itemClassID = e.GetAttribute("item");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveScrap
			{
				itemClassID = this.itemClassID,
				expectedItem = this.expectedItem,
				expectedItemClass = this.expectedItemClass
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemValue expectedItem = ItemValue.None;

		[PublicizedFrom(EAccessModifier.Private)]
		public ItemClass expectedItemClass;

		public string itemClassID = "";
	}
}
