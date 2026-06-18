using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveQuestComplete : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.QuestComplete;
			}
		}

		public override string DescriptionText
		{
			get
			{
				if (this.questText == "")
				{
					this.questText = Localization.Get("challengeTargetAnyQuest", false, null);
				}
				return this.questText + " " + Localization.Get("challengeObjectiveQuestCompleted", false, null) + ":";
			}
		}

		public override void Init()
		{
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.QuestComplete += this.Current_QuestComplete;
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.QuestComplete -= this.Current_QuestComplete;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_QuestComplete(FastTags<TagGroup.Global> questTags, QuestClass questClass)
		{
			if (this.questTag.IsEmpty || questTags.Test_AnySet(this.questTag))
			{
				if (this.CheckBaseRequirements())
				{
					return;
				}
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
			if (e.HasAttribute("quest_tag"))
			{
				this.questTagText = e.GetAttribute("quest_tag");
				this.questTag = FastTags<TagGroup.Global>.Parse(this.questTagText);
			}
			else
			{
				this.questTag = FastTags<TagGroup.Global>.none;
			}
			if (e.HasAttribute("quest_text_key"))
			{
				this.questText = Localization.Get(e.GetAttribute("quest_text_key"), false, null);
			}
			if (e.HasAttribute("tier"))
			{
				this.tier = StringParsers.ParseSInt32(e.GetAttribute("tier"), 0, -1, NumberStyles.Integer);
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveQuestComplete
			{
				questTag = this.questTag,
				questText = this.questText,
				tier = this.tier
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string questTagText;

		[PublicizedFrom(EAccessModifier.Private)]
		public FastTags<TagGroup.Global> questTag = FastTags<TagGroup.Global>.none;

		[PublicizedFrom(EAccessModifier.Private)]
		public int tier;

		[PublicizedFrom(EAccessModifier.Private)]
		public string questText = "";
	}
}
