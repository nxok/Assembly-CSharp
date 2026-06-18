using System;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementQuestsAvailable : BaseDialogRequirement
{
	public override BaseDialogRequirement.RequirementTypes RequirementType
	{
		get
		{
			return BaseDialogRequirement.RequirementTypes.QuestsAvailable;
		}
	}

	public override string GetRequiredDescription(EntityPlayer player)
	{
		return "";
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(GameManager.Instance.World.GetPrimaryPlayer());
		EntityTrader entityTrader = uiforPlayer.xui.Dialog.Respondent as EntityTrader;
		if (!SandboxOptionManager.GetBool(global::SandboxOptions.SandboxOptions.QuestsEnabled))
		{
			return false;
		}
		bool flag = false;
		if (entityTrader.activeQuests != null)
		{
			int currentFactionTier = uiforPlayer.entityPlayer.QuestJournal.GetCurrentFactionTier(entityTrader.NPCInfo.QuestFaction, 0, false);
			for (int i = 0; i < entityTrader.activeQuests.Count; i++)
			{
				if (entityTrader.activeQuests[i].QuestClass.QuestType == base.Value && (!entityTrader.activeQuests[i].QuestClass.ExtraTags.Test_AnySet(DialogRequirementQuestsAvailable.introtag) || QuestJournal.IntroQuestEnabled) && (int)entityTrader.activeQuests[i].QuestClass.DifficultyTier <= currentFactionTier && (entityTrader.activeQuests[i].QuestClass.Repeatable || uiforPlayer.entityPlayer.QuestJournal.FindActiveOrCompleteQuest(entityTrader.activeQuests[i].ID, (int)entityTrader.NPCInfo.QuestFaction) == null))
				{
					flag = true;
					break;
				}
			}
		}
		return flag;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static FastTags<TagGroup.Global> introtag = FastTags<TagGroup.Global>.Parse("introquest");
}
