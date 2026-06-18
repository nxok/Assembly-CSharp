using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionCallGameEvent : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			GameEventActionSequence gameEventActionSequence = ((base.Owner.OwnerSequence != null) ? base.Owner.OwnerSequence : base.Owner);
			if (this.targetGroup != "")
			{
				List<Entity> entityGroup = base.Owner.GetEntityGroup(this.targetGroup);
				if (entityGroup != null)
				{
					for (int i = 0; i < entityGroup.Count; i++)
					{
						GameEventManager.Current.HandleAction(this.gameEventNames[GameEventManager.Current.Random.RandomRange(0, this.gameEventNames.Count)], base.Owner.Requester, entityGroup[i], base.Owner.TwitchActivated, base.Owner.ExtraData, "", false, true, "", gameEventActionSequence);
					}
				}
			}
			else
			{
				GameEventManager.Current.HandleAction(this.gameEventNames[GameEventManager.Current.Random.RandomRange(0, this.gameEventNames.Count)], base.Owner.Requester, base.Owner.Target, base.Owner.TwitchActivated, base.Owner.ExtraData, "", false, true, "", gameEventActionSequence);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			if (properties.Values.ContainsKey(ActionCallGameEvent.PropGameEventNames))
			{
				this.gameEventNames.AddRange(properties.Values[ActionCallGameEvent.PropGameEventNames].Split(',', StringSplitOptions.None));
			}
			properties.ParseString(ActionCallGameEvent.PropTargetGroup, ref this.targetGroup);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionCallGameEvent
			{
				gameEventNames = this.gameEventNames,
				targetGroup = this.targetGroup
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<string> gameEventNames = new List<string>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public string targetGroup = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropGameEventNames = "game_events";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTargetGroup = "target_group";
	}
}
