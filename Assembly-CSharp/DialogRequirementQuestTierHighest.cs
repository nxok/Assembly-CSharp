using System;
using System.Globalization;
using SandboxOptions;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementQuestTierHighest : BaseDialogRequirement
{
	public override BaseDialogRequirement.RequirementTypes RequirementType
	{
		get
		{
			return BaseDialogRequirement.RequirementTypes.QuestTier;
		}
	}

	public override string GetRequiredDescription(EntityPlayer player)
	{
		return "";
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		int num = StringParsers.ParseSInt32(base.Value, 0, -1, NumberStyles.Integer);
		EntityTrader entityTrader = talkingTo as EntityTrader;
		if (!SandboxOptionManager.GetBool(global::SandboxOptions.SandboxOptions.QuestsEnabled))
		{
			return false;
		}
		if (entityTrader == null)
		{
			return false;
		}
		if (player.QuestJournal.GetCurrentFactionTier(entityTrader.NPCInfo.QuestFaction, 0, false) < num)
		{
			return false;
		}
		int num2 = -1;
		if (entityTrader.activeQuests == null)
		{
			return false;
		}
		bool flag = player.GetCVar("DisableQuesting") == 0f;
		for (int i = 0; i < entityTrader.activeQuests.Count; i++)
		{
			QuestClass questClass = entityTrader.activeQuests[i].QuestClass;
			if ((int)questClass.DifficultyTier > num2 && questClass.UniqueKey == base.Tag && (flag || questClass.AlwaysAllow))
			{
				num2 = (int)questClass.DifficultyTier;
			}
		}
		return num == num2;
	}
}
