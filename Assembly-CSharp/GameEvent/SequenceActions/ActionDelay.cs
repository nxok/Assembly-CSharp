using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionDelay : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (this.currentTime == -999f)
			{
				this.currentTime = GameEventManager.GetFloatValue(base.Owner.Target as EntityAlive, this.delayTimeText, 5f);
			}
			this.currentTime -= Time.deltaTime;
			if (this.currentTime <= 0f)
			{
				return BaseAction.ActionCompleteStates.Complete;
			}
			return BaseAction.ActionCompleteStates.InComplete;
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override void OnReset()
		{
			this.currentTime = -999f;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionDelay.PropTime, ref this.delayTimeText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionDelay
			{
				delayTimeText = this.delayTimeText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string delayTimeText = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public float delayTime = 5f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float currentTime = -999f;

		public static string PropTime = "time";
	}
}
