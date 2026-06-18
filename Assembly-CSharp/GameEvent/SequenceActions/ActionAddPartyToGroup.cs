using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddPartyToGroup : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			EntityPlayer entityPlayer = base.Owner.Target as EntityPlayer;
			if (entityPlayer != null)
			{
				List<Entity> list = new List<Entity>();
				if (entityPlayer.Party != null)
				{
					list.AddRange(entityPlayer.Party.MemberList);
				}
				else
				{
					list.Add(base.Owner.Target);
				}
				if (this.excludeTarget)
				{
					list.Remove(base.Owner.Target);
				}
				if (this.excludeTwitchActive)
				{
					for (int i = list.Count - 1; i >= 0; i--)
					{
						EntityPlayer entityPlayer2 = list[i] as EntityPlayer;
						if (entityPlayer2 != null && entityPlayer2.TwitchEnabled && entityPlayer2 != base.Owner.Target)
						{
							list.RemoveAt(i);
						}
					}
				}
				base.Owner.AddEntitiesToGroup(this.groupName, list, this.twitchNegative);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionAddPartyToGroup.PropGroupName, ref this.groupName);
			properties.ParseBool(ActionAddPartyToGroup.PropTwitchNegative, ref this.twitchNegative);
			properties.ParseBool(ActionAddPartyToGroup.PropExcludeTarget, ref this.excludeTarget);
			properties.ParseBool(ActionAddPartyToGroup.PropExcludeTwitchActive, ref this.excludeTwitchActive);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddPartyToGroup
			{
				groupName = this.groupName,
				twitchNegative = this.twitchNegative,
				excludeTarget = this.excludeTarget,
				excludeTwitchActive = this.excludeTwitchActive
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string groupName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool twitchNegative = true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool excludeTarget;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool excludeTwitchActive;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropGroupName = "group_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTwitchNegative = "twitch_negative";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropExcludeTarget = "exclude_target";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropExcludeTwitchActive = "exclude_twitch_active";
	}
}
