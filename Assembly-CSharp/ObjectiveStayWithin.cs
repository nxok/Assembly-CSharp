using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveStayWithin : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Distance;
		}
	}

	public override bool UpdateUI
	{
		get
		{
			return base.ObjectiveState != BaseObjective.ObjectiveStates.Failed;
		}
	}

	public override string StatusText
	{
		get
		{
			if (base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
			{
				if (this.currentDistance < this.maxDistance)
				{
					return ValueDisplayFormatters.Distance(this.currentDistance) + "/" + this.displayDistance;
				}
				base.ObjectiveState = BaseObjective.ObjectiveStates.Failed;
				return Localization.Get("failed", false, null);
			}
			else
			{
				if (base.OwnerQuest.CurrentState == Quest.QuestState.NotStarted)
				{
					return this.displayDistance;
				}
				if (base.ObjectiveState == BaseObjective.ObjectiveStates.Failed)
				{
					return Localization.Get("failed", false, null);
				}
				return Localization.Get("completed", false, null);
			}
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveStayWithin_keyword", false, null);
	}

	public override void SetupDisplay()
	{
		base.Description = this.keyword;
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		if (base.OwnerQuest == base.OwnerQuest.OwnerJournal.ActiveQuest || base.OwnerQuest.OwnerJournal.ActiveQuest == null)
		{
			QuestEventManager.Current.QuestBounds = default(Rect);
		}
	}

	public override void Refresh()
	{
		this.SetupDisplay();
		if (base.ObjectiveState == BaseObjective.ObjectiveStates.NotStarted && base.OwnerQuest.CurrentState == Quest.QuestState.InProgress)
		{
			return;
		}
		base.Complete = base.OwnerQuest.CurrentState != Quest.QuestState.Failed;
	}

	public override void Read(BinaryReader _br)
	{
	}

	public override void Write(BinaryWriter _bw)
	{
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectiveStayWithin objectiveStayWithin = (ObjectiveStayWithin)objective;
		objectiveStayWithin.maxDistance = this.maxDistance;
		objectiveStayWithin.displayDistance = this.displayDistance;
	}

	public override BaseObjective Clone()
	{
		ObjectiveStayWithin objectiveStayWithin = new ObjectiveStayWithin();
		this.CopyValues(objectiveStayWithin);
		return objectiveStayWithin;
	}

	public override void Update(float updateTime)
	{
		Vector3 position = base.OwnerQuest.OwnerJournal.OwnerPlayer.position;
		Vector3 position2 = base.OwnerQuest.Position;
		if (!this.positionSetup)
		{
			if (base.OwnerQuest.GetPositionData(out position2, Quest.PositionDataTypes.Location))
			{
				base.OwnerQuest.Position = position2;
				QuestEventManager.Current.QuestBounds = new Rect(position2.x, position2.z, this.maxDistance, this.maxDistance);
				this.positionSetup = true;
			}
			else if (base.OwnerQuest.GetPositionData(out position2, Quest.PositionDataTypes.POIPosition))
			{
				base.OwnerQuest.Position = position2;
				QuestEventManager.Current.QuestBounds = new Rect(position2.x, position2.z, this.maxDistance, this.maxDistance);
				this.positionSetup = true;
			}
		}
		position.y = 0f;
		position2.y = 0f;
		this.currentDistance = (position - position2).magnitude;
		float num = this.currentDistance / this.maxDistance;
		if (num > 1f)
		{
			base.Complete = false;
			base.ObjectiveState = BaseObjective.ObjectiveStates.Failed;
			base.OwnerQuest.CloseQuest(Quest.QuestState.Failed, null);
			return;
		}
		if (num > 0.75f)
		{
			base.ObjectiveState = BaseObjective.ObjectiveStates.Warning;
			return;
		}
		base.ObjectiveState = BaseObjective.ObjectiveStates.Complete;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectiveStayWithin.PropRadius))
		{
			this.maxDistance = StringParsers.ParseFloat(properties.Values[ObjectiveStayWithin.PropRadius], 0, -1, NumberStyles.Any);
			this.displayDistance = ValueDisplayFormatters.Distance(this.maxDistance);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float maxDistance = 50f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float currentDistance;

	[PublicizedFrom(EAccessModifier.Private)]
	public string displayDistance = "0 km";

	public static string PropRadius = "radius";

	[PublicizedFrom(EAccessModifier.Private)]
	public bool positionSetup;
}
