using System;
using System.Collections.Generic;
using Twitch;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionTwitchAddActionCooldown : ActionBaseClientAction
	{
		public override bool CanPerform(Entity target)
		{
			EntityPlayer entityPlayer = target as EntityPlayer;
			if (entityPlayer != null && entityPlayer.TwitchEnabled && entityPlayer.TwitchActionsEnabled != EntityPlayer.TwitchActionsStates.Disabled)
			{
				return this.searchType != ActionTwitchAddActionCooldown.SearchTypes.Name || !(this.twitchActions == "");
			}
			return base.CanPerform(target);
		}

		public override void OnClientPerform(Entity target)
		{
			TwitchManager twitchManager = TwitchManager.Current;
			if (!twitchManager.TwitchActive)
			{
				return;
			}
			float currentUnityTime = twitchManager.CurrentUnityTime;
			switch (this.searchType)
			{
			case ActionTwitchAddActionCooldown.SearchTypes.Name:
			{
				string[] array = this.twitchActions.Split(',', StringSplitOptions.None);
				for (int i = 0; i < array.Length; i++)
				{
					if (TwitchActionManager.TwitchActions.ContainsKey(array[i]))
					{
						TwitchAction twitchAction = TwitchActionManager.TwitchActions[array[i]];
						twitchAction.tempCooldown = this.cooldownTime;
						twitchAction.tempCooldownSet = currentUnityTime;
					}
				}
				return;
			}
			case ActionTwitchAddActionCooldown.SearchTypes.Positive:
			{
				using (Dictionary<string, TwitchAction>.ValueCollection.Enumerator enumerator = twitchManager.AvailableCommands.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						TwitchAction twitchAction2 = enumerator.Current;
						if (twitchAction2.IsPositive)
						{
							twitchAction2.tempCooldown = this.cooldownTime;
							twitchAction2.tempCooldownSet = currentUnityTime;
						}
					}
					return;
				}
				break;
			}
			case ActionTwitchAddActionCooldown.SearchTypes.Negative:
				break;
			default:
				return;
			}
			foreach (TwitchAction twitchAction3 in twitchManager.AvailableCommands.Values)
			{
				if (!twitchAction3.IsPositive)
				{
					twitchAction3.tempCooldown = this.cooldownTime;
					twitchAction3.tempCooldownSet = currentUnityTime;
				}
			}
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseFloat(ActionTwitchAddActionCooldown.PropTime, ref this.cooldownTime);
			properties.ParseString(ActionTwitchAddActionCooldown.PropTwitchActions, ref this.twitchActions);
			properties.ParseEnum<ActionTwitchAddActionCooldown.SearchTypes>(ActionTwitchAddActionCooldown.PropSearchType, ref this.searchType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionTwitchAddActionCooldown
			{
				twitchActions = this.twitchActions,
				cooldownTime = this.cooldownTime,
				searchType = this.searchType
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public float cooldownTime = 5f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string twitchActions = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public ActionTwitchAddActionCooldown.SearchTypes searchType;

		public static string PropTime = "time";

		public static string PropTwitchActions = "action_name";

		public static string PropSearchType = "search_type";

		[PublicizedFrom(EAccessModifier.Protected)]
		public enum SearchTypes
		{
			Name,
			Positive,
			Negative
		}
	}
}
