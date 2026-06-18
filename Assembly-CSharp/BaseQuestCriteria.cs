using System;
using UnityEngine.Scripting;

[Preserve]
public class BaseQuestCriteria
{
	public virtual void HandleVariables()
	{
	}

	public virtual bool CheckForQuestGiver(EntityNPC entity)
	{
		return true;
	}

	public virtual bool CheckForPlayer(EntityPlayer player)
	{
		return true;
	}

	public string ID;

	public string Value;

	public QuestClass OwnerQuestClass;

	public BaseQuestCriteria.CriteriaTypes CriteriaType;

	public enum CriteriaTypes
	{
		QuestGiver,
		Player
	}
}
