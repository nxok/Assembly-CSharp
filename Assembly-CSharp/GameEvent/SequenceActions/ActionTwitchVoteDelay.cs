using System;
using Twitch;
using UnityEngine;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionTwitchVoteDelay : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			TwitchManager twitchManager = TwitchManager.Current;
			if (twitchManager.VotingManager.CurrentVoteState == TwitchVotingManager.VoteStateTypes.WaitingForNextVote)
			{
				float floatValue = GameEventManager.GetFloatValue(base.Owner.Target as EntityAlive, this.delayTimeText, 5f);
				twitchManager.VotingManager.VoteStartDelayTimeRemaining += floatValue;
			}
			else
			{
				Debug.LogWarning("Error: VoteDelay set in wrong state. " + twitchManager.VotingManager.CurrentVoteState.ToString());
			}
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionTwitchVoteDelay.PropTime, ref this.delayTimeText);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionTwitchVoteDelay
			{
				delayTimeText = this.delayTimeText
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string delayTimeText;

		public static string PropTime = "time";
	}
}
