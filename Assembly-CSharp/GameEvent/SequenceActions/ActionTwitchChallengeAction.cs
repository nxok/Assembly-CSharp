using System;
using Challenges;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionTwitchChallengeAction : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			QuestEventManager.Current.TwitchEventReceived(this.TwitchObjectiveType, this.param);
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseEnum<TwitchObjectiveTypes>(ActionTwitchChallengeAction.PropObjectiveType, ref this.TwitchObjectiveType);
			properties.ParseString(ActionTwitchChallengeAction.PropObjectiveParam, ref this.param);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionTwitchChallengeAction
			{
				TwitchObjectiveType = this.TwitchObjectiveType,
				param = this.param
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public TwitchObjectiveTypes TwitchObjectiveType;

		[PublicizedFrom(EAccessModifier.Private)]
		public string param = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropObjectiveType = "objective_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropObjectiveParam = "objective_param";
	}
}
