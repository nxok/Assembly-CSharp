using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionSetStorm : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			float floatValue = GameEventManager.GetFloatValue(base.Owner.Target as EntityAlive, this.timeText, 1f);
			WeatherManager.Instance.SetStorm(null, (int)(floatValue * 1000f));
			WeatherManager.Instance.TriggerUpdate();
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionSetStorm.PropTime, ref this.timeText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionSetStorm
			{
				timeText = this.timeText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string timeText;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTime = "hours";
	}
}
