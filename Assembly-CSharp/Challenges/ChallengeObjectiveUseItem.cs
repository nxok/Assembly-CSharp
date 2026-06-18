using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveUseItem : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.Use;
			}
		}

		public override string DescriptionText
		{
			get
			{
				string text = ((this.overrideText != "") ? this.overrideText : Localization.Get(this.itemName, false, null));
				return Localization.Get("challengeObjectiveUse", false, null) + " " + text + ":";
			}
		}

		public override void Init()
		{
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
			if (this.itemNames.ContainsCaseInsensitive(itemValue.ItemClass.Name) || (!this.itemTags.IsEmpty && itemValue.ItemClass.ItemTags.Test_AnySet(this.itemTags)))
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

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("item"))
			{
				this.itemName = e.GetAttribute("item");
			}
			if (e.HasAttribute("item_tags"))
			{
				this.itemTags = FastTags<TagGroup.Global>.Parse(e.GetAttribute("item_tags"));
			}
			if (e.HasAttribute("override_text_key"))
			{
				this.overrideText = Localization.Get(e.GetAttribute("override_text_key"), false, null);
				return;
			}
			if (e.HasAttribute("override_text"))
			{
				this.overrideText = e.GetAttribute("override_text");
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveUseItem
			{
				itemName = this.itemName,
				itemNames = this.itemNames,
				itemTags = this.itemTags,
				overrideText = this.overrideText
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string itemName = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string[] itemNames;

		[PublicizedFrom(EAccessModifier.Private)]
		public string overrideText = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> itemTags = FastTags<TagGroup.Global>.none;
	}
}
