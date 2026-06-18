using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveTrader : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Trader;
			}
		}

		public override string DescriptionText
		{
			get
			{
				if (string.IsNullOrEmpty(this.TraderName))
				{
					if (!this.BuyItems)
					{
						return Localization.Get("challengeObjectiveSellItems", false, null);
					}
					return Localization.Get("challengeObjectiveBuyItems", false, null);
				}
				else
				{
					if (!this.BuyItems)
					{
						return string.Format(Localization.Get("challengeObjectiveSellItemsTo", false, null), Localization.Get(this.TraderName, false, null));
					}
					return string.Format(Localization.Get("challengeObjectiveBuyItemsFrom", false, null), Localization.Get(this.TraderName, false, null));
				}
			}
		}

		public override void Init()
		{
		}

		public override void HandleAddHooks()
		{
			if (this.BuyItems)
			{
				QuestEventManager.Current.BuyItems += this.Current_BuyItems;
				return;
			}
			QuestEventManager.Current.SellItems += this.Current_SellItems;
		}

		public override void HandleRemoveHooks()
		{
			if (this.BuyItems)
			{
				QuestEventManager.Current.BuyItems -= this.Current_BuyItems;
				return;
			}
			QuestEventManager.Current.SellItems -= this.Current_SellItems;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_BuyItems(string traderName, int itemCounts)
		{
			if (this.CheckBaseRequirements())
			{
				return;
			}
			if (this.TraderName == "" || traderName == this.TraderName)
			{
				base.Current += itemCounts;
				if (base.Current >= this.MaxCount)
				{
					base.Current = this.MaxCount;
					this.CheckObjectiveComplete(true);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_SellItems(string traderName, int itemCounts)
		{
			if (this.CheckBaseRequirements())
			{
				return;
			}
			if (this.TraderName == "" || traderName == this.TraderName)
			{
				base.Current += itemCounts;
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
			if (e.HasAttribute("is_buy"))
			{
				this.BuyItems = StringParsers.ParseBool(e.GetAttribute("is_buy"), 0, -1, true);
			}
			if (e.HasAttribute("trader_name"))
			{
				this.TraderName = e.GetAttribute("trader_name");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveTrader
			{
				BuyItems = this.BuyItems,
				TraderName = this.TraderName
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool BuyItems;

		[PublicizedFrom(EAccessModifier.Private)]
		public string TraderName = "";
	}
}
