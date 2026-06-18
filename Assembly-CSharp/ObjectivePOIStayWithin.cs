using System;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class ObjectivePOIStayWithin : BaseObjective
{
	public override BaseObjective.ObjectiveValueTypes ObjectiveValueType
	{
		get
		{
			return BaseObjective.ObjectiveValueTypes.Boolean;
		}
	}

	public override bool UpdateUI
	{
		get
		{
			return base.ObjectiveState != BaseObjective.ObjectiveStates.Failed;
		}
	}

	public override bool useUpdateLoop
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public override bool ShowInQuestLog
	{
		get
		{
			return false;
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveStayWithin_keyword", false, null);
	}

	public override void SetupDisplay()
	{
		base.Description = string.Format("{0}", this.keyword);
	}

	public override void AddHooks()
	{
		QuestEventManager.Current.AddObjectiveToBeUpdated(this);
	}

	public override void RemoveHooks()
	{
		QuestEventManager.Current.RemoveObjectiveToBeUpdated(this);
		if (base.OwnerQuest == base.OwnerQuest.OwnerJournal.ActiveQuest)
		{
			QuestEventManager.Current.QuestBounds = default(Rect);
		}
		if (this.goBounds != null)
		{
			Object.Destroy(this.goBounds);
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

	public override BaseObjective Clone()
	{
		ObjectivePOIStayWithin objectivePOIStayWithin = new ObjectivePOIStayWithin();
		this.CopyValues(objectivePOIStayWithin);
		return objectivePOIStayWithin;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CopyValues(BaseObjective objective)
	{
		base.CopyValues(objective);
		ObjectivePOIStayWithin objectivePOIStayWithin = (ObjectivePOIStayWithin)objective;
		objectivePOIStayWithin.outerRect = this.outerRect;
		objectivePOIStayWithin.innerRect = this.innerRect;
		objectivePOIStayWithin.offset = this.offset;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public GameObject CreateBoundsViewer()
	{
		if (this.prefabBounds == null)
		{
			this.prefabBounds = Resources.Load<GameObject>("Prefabs/prefabPOIBounds");
		}
		GameObject gameObject = Object.Instantiate<GameObject>(this.prefabBounds);
		gameObject.name = "QuestBounds";
		return gameObject;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 GetPosition()
	{
		Vector3 vector;
		if (base.OwnerQuest.GetPositionData(out this.position, Quest.PositionDataTypes.POIPosition) && base.OwnerQuest.GetPositionData(out vector, Quest.PositionDataTypes.POISize))
		{
			PrefabInstance prefabAtPosition = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabAtPosition(this.position, null, null);
			if (prefabAtPosition != null)
			{
				base.OwnerQuest.Position = this.position;
				this.positionSet = true;
				this.outerRect = new Rect(this.position.x - this.offset, this.position.z - this.offset, vector.x + this.offset * 2f, vector.z + this.offset * 2f);
				this.innerRect = new Rect(this.position.x, this.position.z, vector.x, vector.z);
				float rotationAngle = prefabAtPosition.RotationAngle;
				this.outerRect = GeometryUtils.RotateRectAboutY(this.outerRect, rotationAngle);
				this.innerRect = GeometryUtils.RotateRectAboutY(this.innerRect, rotationAngle);
				QuestEventManager.Current.QuestBounds = this.outerRect;
				if (this.goBounds == null)
				{
					this.goBounds = this.CreateBoundsViewer();
				}
				this.goBounds.GetComponent<POIBoundsHelper>().SetPosition(new Vector3(this.outerRect.center.x, base.OwnerQuest.OwnerJournal.OwnerPlayer.position.y, this.outerRect.center.y) - Origin.position, new Vector3(this.outerRect.width, 200f, this.outerRect.height));
				base.CurrentValue = 2;
				return this.position;
			}
		}
		return Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_NeedSetup()
	{
		this.GetPosition() != Vector3.zero;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateState_Update()
	{
		if (!this.positionSet)
		{
			this.GetPosition();
			return;
		}
		Vector3 vector = base.OwnerQuest.OwnerJournal.OwnerPlayer.position;
		Vector3 vector2 = base.OwnerQuest.Position;
		vector.y = vector.z;
		if (!this.outerRect.Contains(vector))
		{
			base.Complete = false;
			base.ObjectiveState = BaseObjective.ObjectiveStates.Failed;
			base.OwnerQuest.CloseQuest(Quest.QuestState.Failed, null);
			return;
		}
		if (this.innerRect.Contains(vector))
		{
			base.ObjectiveState = BaseObjective.ObjectiveStates.Complete;
			return;
		}
		base.ObjectiveState = BaseObjective.ObjectiveStates.Warning;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		if (properties.Values.ContainsKey(ObjectivePOIStayWithin.PropRadius))
		{
			this.offset = (float)StringParsers.ParseSInt32(properties.Values[ObjectivePOIStayWithin.PropRadius], 0, -1, NumberStyles.Integer);
		}
	}

	public static string PropRadius = "radius";

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 position;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool positionSet;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect outerRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public Rect innerRect;

	[PublicizedFrom(EAccessModifier.Private)]
	public float offset;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject prefabBounds;

	[PublicizedFrom(EAccessModifier.Private)]
	public GameObject goBounds;
}
