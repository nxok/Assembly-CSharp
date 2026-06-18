using System;
using UnityEngine.Scripting;

[Preserve]
public class DialogRequirementQuestStatus : BaseDialogRequirement
{
	public override BaseDialogRequirement.RequirementTypes RequirementType
	{
		get
		{
			return BaseDialogRequirement.RequirementTypes.QuestStatus;
		}
	}

	public override string GetRequiredDescription(EntityPlayer player)
	{
		return "";
	}

	public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
	{
		DialogRequirementQuestStatus.QuestStatuses questStatuses = EnumUtils.Parse<DialogRequirementQuestStatus.QuestStatuses>(base.Value, false);
		string id = base.ID;
		if (string.IsNullOrEmpty(base.ID))
		{
			EntityNPC respondent = LocalPlayerUI.GetUIForPlayer(GameManager.Instance.World.GetPrimaryPlayer()).xui.Dialog.Respondent;
			if (respondent != null)
			{
				int entityId = respondent.entityId;
				if (questStatuses == DialogRequirementQuestStatus.QuestStatuses.NotStarted)
				{
					Quest quest = player.QuestJournal.FindActiveQuestByGiver(entityId, base.Tag);
					return quest == null;
				}
				if (questStatuses == DialogRequirementQuestStatus.QuestStatuses.InProgress)
				{
					Quest quest = player.QuestJournal.FindActiveQuestByGiver(entityId, base.Tag);
					return quest != null;
				}
			}
		}
		else
		{
			switch (questStatuses)
			{
			case DialogRequirementQuestStatus.QuestStatuses.NotStarted:
			{
				Quest quest = player.QuestJournal.FindNonSharedQuest(id);
				if (quest == null || quest.CurrentState == Quest.QuestState.Completed)
				{
					return true;
				}
				break;
			}
			case DialogRequirementQuestStatus.QuestStatuses.InProgress:
			{
				Quest quest = player.QuestJournal.FindNonSharedQuest(id);
				if (quest != null && quest.Active)
				{
					for (int i = 0; i < quest.Objectives.Count; i++)
					{
						if (!quest.Objectives[i].Complete)
						{
							return true;
						}
					}
				}
				break;
			}
			case DialogRequirementQuestStatus.QuestStatuses.TurnInReady:
			{
				Quest quest = player.QuestJournal.FindQuest(id, (int)talkingTo.NPCInfo.QuestFaction);
				if (quest != null && quest.Active)
				{
					for (int j = 0; j < quest.Objectives.Count; j++)
					{
						if (!quest.Objectives[j].Complete)
						{
							return false;
						}
					}
					return true;
				}
				break;
			}
			case DialogRequirementQuestStatus.QuestStatuses.Completed:
			{
				Quest quest = player.QuestJournal.FindQuest(id, (int)talkingTo.NPCInfo.QuestFaction);
				if (quest.CurrentState == Quest.QuestState.Completed)
				{
					return true;
				}
				break;
			}
			case DialogRequirementQuestStatus.QuestStatuses.CanReceive:
			{
				Quest quest2 = player.QuestJournal.FindLatestNonSharedQuest(id);
				if (quest2 == null)
				{
					return true;
				}
				if (quest2.CurrentState == Quest.QuestState.Completed)
				{
					int num = (int)(GameManager.Instance.World.worldTime / 24000UL);
					int num2 = (int)(quest2.FinishTime / 24000UL);
					if (num != num2)
					{
						return true;
					}
				}
				break;
			}
			case DialogRequirementQuestStatus.QuestStatuses.CannotReceive:
			{
				Quest quest3 = player.QuestJournal.FindLatestNonSharedQuest(id);
				if (quest3 != null)
				{
					if (quest3.CurrentState != Quest.QuestState.Completed)
					{
						return true;
					}
					int num3 = (int)(GameManager.Instance.World.worldTime / 24000UL);
					int num4 = (int)(quest3.FinishTime / 24000UL);
					if (num3 == num4)
					{
						return true;
					}
				}
				break;
			}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public enum QuestStatuses
	{
		NotStarted,
		InProgress,
		TurnInReady,
		Completed,
		CanReceive,
		CannotReceive
	}
}
