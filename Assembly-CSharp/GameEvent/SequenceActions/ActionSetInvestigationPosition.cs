using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionSetInvestigationPosition : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			EntityAlive entityAlive = target as EntityAlive;
			if (entityAlive != null)
			{
				entityAlive.SetInvestigatePosition(base.Owner.TargetPosition, (int)(this.investigateTime * 20f), this.isAlert);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseFloat(ActionSetInvestigationPosition.PropTime, ref this.investigateTime);
			properties.ParseBool(ActionSetInvestigationPosition.PropIsAlert, ref this.isAlert);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionSetInvestigationPosition
			{
				investigateTime = this.investigateTime,
				isAlert = this.isAlert,
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public float investigateTime;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool isAlert;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTime = "time";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropIsAlert = "is_alert";
	}
}
