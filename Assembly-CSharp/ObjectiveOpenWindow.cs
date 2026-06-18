using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveOpenWindow : BaseObjective
{
	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveOpenWindow_keyword", false, null);
	}

	public override void SetupDisplay()
	{
		byte currentValue = base.CurrentValue;
		base.Description = string.Format(this.keyword, this.ID);
		this.StatusText = "";
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.WindowChanged += this.Current_WindowOpened;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.WindowChanged -= this.Current_WindowOpened;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_WindowOpened(string windowName)
	{
		if (windowName.EqualsCaseInsensitive(this.ID) && base.OwnerQuest.CheckRequirements())
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

	public override void RemoveObjectives()
	{
	}

	public override BaseObjective Clone()
	{
		ObjectiveOpenWindow objectiveOpenWindow = new ObjectiveOpenWindow();
		this.CopyValues(objectiveOpenWindow);
		return objectiveOpenWindow;
	}
}
