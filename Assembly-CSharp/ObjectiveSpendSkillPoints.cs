using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveSpendSkillPoints : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Number;
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveSpendSkillPoints_keyword", false, null);
		this.pointCount = Convert.ToInt32(this.Value);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(this.keyword, (this.ID != null) ? this.ID : "Any");
		this.StatusText = string.Format("{0}/{1}", base.CurrentValue, this.pointCount);
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.SkillPointSpent += this.Current_SkillPointSpent;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.SkillPointSpent -= this.Current_SkillPointSpent;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_SkillPointSpent(string skillName)
	{
		if (base.Complete)
		{
			return;
		}
		if ((this.ID == null || skillName.EqualsCaseInsensitive(this.ID)) && base.OwnerQuest.CheckRequirements())
		{
			byte currentValue = base.CurrentValue;
			base.CurrentValue = currentValue + 1;
		}
		this.Refresh();
	}

	public override void Refresh()
	{
		if ((int)base.CurrentValue > this.pointCount)
		{
			base.CurrentValue = (byte)this.pointCount;
		}
		base.Complete = (int)base.CurrentValue >= this.pointCount;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override void RemoveObjectives()
	{
	}

	public override BaseObjective Clone()
	{
		ObjectiveSpendSkillPoints objectiveSpendSkillPoints = new ObjectiveSpendSkillPoints();
		this.CopyValues(objectiveSpendSkillPoints);
		return objectiveSpendSkillPoints;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int pointCount;
}
