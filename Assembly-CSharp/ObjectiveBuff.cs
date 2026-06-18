using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveBuff : BaseObjective
{
	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveBuff_keyword", false, null);
		this.name = BuffManager.GetBuff(this.ID).Name;
	}

	public override void SetupDisplay()
	{
		byte currentValue = base.CurrentValue;
		base.Description = string.Format(this.keyword, this.name);
		this.StatusText = "";
	}

	public override void AddHooks()
	{
	}

	public override void RemoveHooks()
	{
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		bool flag = base.CurrentValue == 1;
		base.Complete = flag;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveBuff objectiveBuff = new ObjectiveBuff();
		this.CopyValues(objectiveBuff);
		return objectiveBuff;
	}

	public override string ParseBinding(string bindingName)
	{
		string id = this.ID;
		string value = this.Value;
		if (bindingName == "name")
		{
			return BuffManager.GetBuff(id).Name;
		}
		return "";
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name = "";
}
