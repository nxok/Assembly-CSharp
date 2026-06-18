using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionAddPlayerToGroup : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			List<Entity> list = new List<Entity>();
			EntityPlayer entityPlayer = base.Owner.Target as EntityPlayer;
			if (entityPlayer != null)
			{
				if (entityPlayer.Party != null)
				{
					for (int i = 0; i < entityPlayer.Party.MemberList.Count; i++)
					{
						if (entityPlayer.Party.MemberList[i].EntityName.ToLower() == this.playerName.ToLower())
						{
							list.Add(entityPlayer.Party.MemberList[i]);
						}
					}
				}
				else if (entityPlayer.EntityName.ToLower() == this.playerName.ToLower())
				{
					list.Add(base.Owner.Target);
				}
				base.Owner.AddEntitiesToGroup(this.groupName, list, this.twitchNegative);
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionAddPlayerToGroup.PropGroupName, ref this.groupName);
			properties.ParseString(ActionAddPlayerToGroup.PropPlayerName, ref this.playerName);
			properties.ParseBool(ActionAddPlayerToGroup.PropTwitchNegative, ref this.twitchNegative);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionAddPlayerToGroup
			{
				groupName = this.groupName,
				playerName = this.playerName,
				twitchNegative = this.twitchNegative
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string groupName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string playerName = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool twitchNegative = true;

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropGroupName = "group_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropPlayerName = "player_name";

		[PublicizedFrom(EAccessModifier.Protected)]
		public static string PropTwitchNegative = "twitch_negative";
	}
}
