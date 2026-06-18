using System;

public class QuestCriteriaLevel : BaseQuestCriteria
{
	public override bool CheckForPlayer(EntityPlayer player)
	{
		int num = 0;
		return int.TryParse(this.Value, out num) && player.Progression.Level >= num;
	}
}
