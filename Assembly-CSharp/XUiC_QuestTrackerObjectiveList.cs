using System;
using System.Collections.Generic;
using Challenges;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_QuestTrackerObjectiveList : XUiController
{
	public Quest Quest
	{
		get
		{
			return this.quest;
		}
		set
		{
			this.quest = value;
			this.IsDirty = true;
		}
	}

	public Challenge Challenge
	{
		get
		{
			return this.challenge;
		}
		set
		{
			if (this.challenge != null)
			{
				this.challenge.OnChallengeStateChanged -= this.CurrentChallenge_OnChallengeStateChanged;
			}
			this.challenge = value;
			if (this.challenge != null)
			{
				this.challenge.OnChallengeStateChanged += this.CurrentChallenge_OnChallengeStateChanged;
			}
			this.IsDirty = true;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void CurrentChallenge_OnChallengeStateChanged(Challenge challenge)
	{
		this.IsDirty = true;
	}

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_QuestTrackerObjectiveEntry>(null);
		XUiController[] array = childrenByType;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != null)
			{
				this.objectiveEntries.Add(array[i]);
			}
		}
	}

	public override void Update(float _dt)
	{
		if (this.IsDirty)
		{
			if (this.quest != null)
			{
				int count = this.objectiveEntries.Count;
				int count2 = this.quest.Objectives.Count;
				int num = 0;
				for (int i = 0; i < count2; i++)
				{
					if ((this.quest.Objectives[i].Phase == this.quest.CurrentPhase || this.quest.Objectives[i].Phase == 0) && !this.quest.Objectives[i].HiddenObjective)
					{
						if (num < count)
						{
							XUiC_QuestTrackerObjectiveEntry xuiC_QuestTrackerObjectiveEntry = this.objectiveEntries[num] as XUiC_QuestTrackerObjectiveEntry;
							if (xuiC_QuestTrackerObjectiveEntry != null)
							{
								xuiC_QuestTrackerObjectiveEntry.Owner = this;
								if (i < count2)
								{
									xuiC_QuestTrackerObjectiveEntry.QuestObjective = this.quest.Objectives[i];
								}
								else
								{
									xuiC_QuestTrackerObjectiveEntry.ClearObjective();
								}
							}
						}
						num++;
					}
				}
				if (num < count)
				{
					for (int j = num; j < count; j++)
					{
						XUiC_QuestTrackerObjectiveEntry xuiC_QuestTrackerObjectiveEntry2 = this.objectiveEntries[j] as XUiC_QuestTrackerObjectiveEntry;
						if (xuiC_QuestTrackerObjectiveEntry2 != null)
						{
							xuiC_QuestTrackerObjectiveEntry2.ClearObjective();
						}
					}
				}
			}
			else if (this.challenge != null)
			{
				List<BaseChallengeObjective> objectiveList = this.challenge.GetObjectiveList();
				int count3 = this.objectiveEntries.Count;
				int count4 = objectiveList.Count;
				int num2 = 0;
				for (int k = 0; k < count4; k++)
				{
					if (num2 < count3)
					{
						XUiC_QuestTrackerObjectiveEntry xuiC_QuestTrackerObjectiveEntry3 = this.objectiveEntries[num2] as XUiC_QuestTrackerObjectiveEntry;
						if (xuiC_QuestTrackerObjectiveEntry3 != null)
						{
							xuiC_QuestTrackerObjectiveEntry3.Owner = this;
							if (k < count4)
							{
								xuiC_QuestTrackerObjectiveEntry3.ChallengeObjective = objectiveList[k];
							}
							else
							{
								xuiC_QuestTrackerObjectiveEntry3.ClearObjective();
							}
						}
					}
					num2++;
				}
				if (num2 < count3)
				{
					for (int l = num2; l < count3; l++)
					{
						XUiC_QuestTrackerObjectiveEntry xuiC_QuestTrackerObjectiveEntry4 = this.objectiveEntries[l] as XUiC_QuestTrackerObjectiveEntry;
						if (xuiC_QuestTrackerObjectiveEntry4 != null)
						{
							xuiC_QuestTrackerObjectiveEntry4.ClearObjective();
						}
					}
				}
			}
			else
			{
				int count5 = this.objectiveEntries.Count;
				for (int m = 0; m < count5; m++)
				{
					XUiC_QuestTrackerObjectiveEntry xuiC_QuestTrackerObjectiveEntry5 = this.objectiveEntries[m] as XUiC_QuestTrackerObjectiveEntry;
					if (xuiC_QuestTrackerObjectiveEntry5 != null)
					{
						xuiC_QuestTrackerObjectiveEntry5.Owner = this;
						xuiC_QuestTrackerObjectiveEntry5.ClearObjective();
					}
				}
			}
			this.IsDirty = false;
		}
		base.Update(_dt);
	}

	public override bool ParseAttribute(string name, string value)
	{
		if (name == "complete_icon")
		{
			this.completeIconName = value;
			return true;
		}
		if (name == "incomplete_icon")
		{
			this.incompleteIconName = value;
			return true;
		}
		if (name == "complete_color")
		{
			Color32 color = StringParsers.ParseColor(value);
			this.completeColor = string.Format("{0},{1},{2},{3}", new object[] { color.r, color.g, color.b, color.a });
			this.completeHexColor = Utils.ColorToHex(color);
			return true;
		}
		if (!(name == "incomplete_color"))
		{
			return base.ParseAttribute(name, value);
		}
		Color32 color2 = StringParsers.ParseColor(value);
		this.incompleteColor = string.Format("{0},{1},{2},{3}", new object[] { color2.r, color2.g, color2.b, color2.a });
		this.incompleteHexColor = Utils.ColorToHex(color2);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Quest quest;

	[PublicizedFrom(EAccessModifier.Private)]
	public Challenge challenge;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<XUiController> objectiveEntries = new List<XUiController>();

	public string completeIconName = "";

	public string incompleteIconName = "";

	public string completeHexColor = "FF00FF00";

	public string incompleteHexColor = "FFB400";

	public string warningHexColor = "FFFF00FF";

	public string inactiveHexColor = "888888FF";

	public string activeHexColor = "FFFFFFFF";

	public string completeColor = "0,255,0,255";

	public string incompleteColor = "255, 180, 0, 255";

	public string warningColor = "255,255,0,255";
}
