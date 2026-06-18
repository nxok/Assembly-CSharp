using System;
using System.Xml.Linq;
using UnityEngine.Scripting;

[Preserve]
public class LootEntryRequirementQuestTags : BaseLootEntryRequirement
{
	public override void Init(XElement e)
	{
		base.Init(e);
		string text = "";
		if (e.ParseAttribute("quest_tags", ref text))
		{
			this.questTags = FastTags<TagGroup.Global>.Parse(text);
		}
	}

	public override bool CheckRequirement(EntityPlayer player)
	{
		return player.QuestJournal.ActiveQuest != null && player.QuestJournal.ActiveQuest.QuestTags.Test_AnySet(this.questTags);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public FastTags<TagGroup.Global> questTags;
}
