using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveBlockActivate : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Boolean;
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveBlockActivate_keyword", false, null);
		this.localizedName = ((this.ID != "" && this.ID != null) ? Localization.Get(this.ID, false, null) : "Any Block");
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format(this.keyword, this.localizedName);
	}

	public override void AddHooks()
	{
	}

	public override void RemoveHooks()
	{
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_BlockActivate(string blockName)
	{
		if (base.Complete)
		{
			return;
		}
		if ((this.ID == null || this.ID == "" || this.ID.EqualsCaseInsensitive(blockName)) && base.OwnerQuest.CheckRequirements())
		{
			base.CurrentValue = 1;
			this.Refresh();
		}
	}

	public override void Refresh()
	{
		bool flag = base.CurrentValue == 1;
		base.Complete = flag;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveBlockActivate objectiveBlockActivate = new ObjectiveBlockActivate();
		this.CopyValues(objectiveBlockActivate);
		return objectiveBlockActivate;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string localizedName = "";
}
