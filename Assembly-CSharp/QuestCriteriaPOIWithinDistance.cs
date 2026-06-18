using System;

public class QuestCriteriaPOIWithinDistance : BaseQuestCriteria
{
	public override bool CheckForQuestGiver(EntityNPC entity)
	{
		int num = 0;
		int.TryParse(this.Value, out num);
		return false;
	}
}
