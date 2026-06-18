using System;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionSetEventFlag : ActionBaseTargetAction
	{
		public override BaseAction.ActionCompleteStates PerformTargetAction(Entity target)
		{
			GameEventManager.Current.SetGameEventFlag(this.eventFlag, this.enable, GameEventManager.GetFloatValue(target as EntityAlive, this.durationText, 0f), false);
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<GameEventManager.GameEventFlagTypes>(ActionSetEventFlag.PropEventFlag, ref this.eventFlag);
			properties.ParseBool(ActionSetEventFlag.PropEnable, ref this.enable);
			properties.ParseString(ActionSetEventFlag.PropDuration, ref this.durationText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionSetEventFlag
			{
				targetGroup = this.targetGroup,
				eventFlag = this.eventFlag,
				enable = this.enable,
				durationText = this.durationText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public GameEventManager.GameEventFlagTypes eventFlag = GameEventManager.GameEventFlagTypes.Invalid;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool enable;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string durationText;

		public static string PropEventFlag = "event_flag";

		public static string PropEnable = "enable";

		public static string PropDuration = "duration";
	}
}
