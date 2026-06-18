using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveCureDebuff : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.CureDebuff;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return Localization.Get("challengeObjectiveCure", false, null) + " " + BuffManager.GetBuff(this.buffName).LocalizedName + ":";
			}
		}

		public override void Init()
		{
			if (this.buffName != null)
			{
				this.buffNames = this.buffName.Split(',', StringSplitOptions.None);
				if (this.buffNames.Length > 1)
				{
					this.buffName = this.buffNames[0];
				}
			}
			if (this.itemName != null)
			{
				this.itemNames = this.itemName.Split(',', StringSplitOptions.None);
				if (this.itemNames.Length > 1)
				{
					this.itemName = this.itemNames[0];
				}
			}
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.UseItem += this.Current_UseItem;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.UseItem -= this.Current_UseItem;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_UseItem(ItemValue itemValue)
		{
			if (this.CheckBaseRequirements())
			{
				return;
			}
			if (this.itemNames.ContainsCaseInsensitive(itemValue.ItemClass.Name) && this.PlayerHasBuff())
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

		[PublicizedFrom(EAccessModifier.Private)]
		public bool PlayerHasBuff()
		{
			EntityBuffs buffs = this.Owner.Owner.Player.Buffs;
			for (int i = 0; i < this.buffNames.Length; i++)
			{
				if (buffs.HasBuff(this.buffNames[i]))
				{
					return true;
				}
			}
			return false;
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("debuff"))
			{
				this.buffName = e.GetAttribute("debuff");
			}
			if (e.HasAttribute("item"))
			{
				this.itemName = e.GetAttribute("item");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveCureDebuff
			{
				buffName = this.buffName,
				buffNames = this.buffNames,
				itemName = this.itemName,
				itemNames = this.itemNames
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string buffName = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string[] buffNames;

		[PublicizedFrom(EAccessModifier.Private)]
		public string itemName = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string[] itemNames;
	}
}
