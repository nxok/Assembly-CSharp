using System;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveStatAwarded : BaseObjective
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
		this.keyword = this.statText;
		this.neededCount = Convert.ToInt32(this.Value);
	}

	public override void SetupDisplay()
	{
		base.Description = this.statText;
		this.StatusText = string.Format("{0}/{1}", base.CurrentValue, this.neededCount);
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.QuestAwardCredit += this.Current_QuestAwardCredit;
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.QuestAwardCredit -= this.Current_QuestAwardCredit;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Current_QuestAwardCredit(string stat, int awardCount)
	{
		if (base.Complete)
		{
			return;
		}
		if (this.statName.EqualsCaseInsensitive(stat))
		{
			base.CurrentValue += (byte)awardCount;
			if ((int)base.CurrentValue >= this.neededCount)
			{
				this.Refresh();
			}
		}
	}

	public override void Refresh()
	{
		if ((int)base.CurrentValue > this.neededCount)
		{
			base.CurrentValue = (byte)this.neededCount;
		}
		base.Complete = (int)base.CurrentValue >= this.neededCount;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override void RemoveObjectives()
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveStatAwarded objectiveStatAwarded = (ObjectiveStatAwarded)objective;
		objectiveStatAwarded.statName = this.statName;
		objectiveStatAwarded.statText = this.statText;
	}

	public override BaseObjective Clone()
	{
		ObjectiveStatAwarded objectiveStatAwarded = new ObjectiveStatAwarded();
		this.CopyValues(objectiveStatAwarded);
		return objectiveStatAwarded;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		string text = "";
		properties.ParseString(ObjectiveStatAwarded.PropStatName, ref this.statName);
		properties.ParseString(ObjectiveStatAwarded.PropStatText, ref this.statText);
		if (properties.Contains(ObjectiveStatAwarded.PropNeededCount))
		{
			int num = 0;
			properties.ParseInt(ObjectiveStatAwarded.PropNeededCount, ref num);
			this.Value = num.ToString();
		}
		properties.ParseString(ObjectiveStatAwarded.PropStatTextKey, ref text);
		if (text != "")
		{
			this.statText = Localization.Get(text, false, null);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string statName = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public string statText = "";

	[PublicizedFrom(EAccessModifier.Private)]
	public int neededCount;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStatName = "stat_name";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStatText = "stat_text";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropStatTextKey = "stat_text_key";

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropNeededCount = "count";
}
