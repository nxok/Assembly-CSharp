using System;
using Twitch;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionTwitchStartCooldown : ActionBaseClientAction
	{
		public override void OnClientPerform(Entity target)
		{
			TwitchManager twitchManager = TwitchManager.Current;
			if (!twitchManager.TwitchActive)
			{
				return;
			}
			float floatValue = GameEventManager.GetFloatValue(target as EntityAlive, this.cooldownTimeLeft, 5f);
			twitchManager.SetCooldown(floatValue, TwitchManager.CooldownTypes.Time, false, true);
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionTwitchStartCooldown.PropTime, ref this.cooldownTimeLeft);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionTwitchStartCooldown
			{
				cooldownTimeLeft = this.cooldownTimeLeft
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string cooldownTimeLeft;

		public static string PropTime = "time";
	}
}
