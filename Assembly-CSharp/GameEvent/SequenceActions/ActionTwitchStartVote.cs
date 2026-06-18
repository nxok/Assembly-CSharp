using System;
using Twitch;
using UnityEngine.Scripting;

namespace GameEvent.SequenceActions
{
	[Preserve]
	public class ActionTwitchStartVote : BaseAction
	{
		public override BaseAction.ActionCompleteStates OnPerformAction()
		{
			if (this.voteType == "")
			{
				return BaseAction.ActionCompleteStates.InCompleteRefund;
			}
			TwitchManager twitchManager = TwitchManager.Current;
			if (!twitchManager.TwitchActive || !twitchManager.VotingManager.VotingEnabled)
			{
				return BaseAction.ActionCompleteStates.InCompleteRefund;
			}
			twitchManager.VotingManager.QueueVote(this.voteType);
			return BaseAction.ActionCompleteStates.Complete;
		}

		public override void ParseProperties(DynamicProperties properties)
		{
			base.ParseProperties(properties);
			properties.ParseString(ActionTwitchStartVote.PropVoteType, ref this.voteType);
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public override BaseAction CloneChildSettings()
		{
			return new ActionTwitchStartVote
			{
				voteType = this.voteType
			};
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public string voteType = "";

		public static string PropVoteType = "vote_type";
	}
}
