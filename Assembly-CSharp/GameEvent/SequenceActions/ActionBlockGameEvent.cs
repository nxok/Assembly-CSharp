using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionBlockGameEvent : ActionBaseBlockAction
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		public override BlockChangeInfo UpdateBlock(World world, Vector3i currentPos, BlockValue blockValue)
		{
			if (!blockValue.isair)
			{
				GameEventManager.Current.HandleAction(this.gameEventNames[GameEventManager.Current.Random.RandomRange(0, this.gameEventNames.Count)], base.Owner.Requester, base.Owner.Target, false, currentPos, base.Owner.ExtraData, "", false, true, "", null, null);
			}
			return null;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(ActionBlockGameEvent.PropGameEventNames))
			{
				this.gameEventNames.AddRange(properties.Values[ActionBlockGameEvent.PropGameEventNames].Split(',', StringSplitOptions.None));
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionBlockGameEvent
			{
				gameEventNames = this.gameEventNames
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<string> gameEventNames = new List<string>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropGameEventNames = "game_events";
	}
}
