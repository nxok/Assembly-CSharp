using System;
using Twitch;
using UnityEngine.Scripting;

[Preserve]
public class ObjectiveTwitchVote : BaseObjective
{
	public override bool useUpdateLoop
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			return true;
		}
	}

	public override bool ShowInQuestLog
	{
		get
		{
			return false;
		}
	}

	public override void SetupObjective()
	{
		this.keyword = Localization.Get("ObjectiveAssemble_keyword", false, null);
	}

	public override void SetupDisplay()
	{
		base.Description = "";
		this.StatusText = "";
	}

	public override void Update(float updateTime)
	{
		TwitchManager twitchManager = TwitchManager.Current;
		switch (this.GameEventState)
		{
		case ObjectiveTwitchVote.TwitchVoteStates.Start:
		{
			if (this.voteType == "" || !twitchManager.IsReady || !twitchManager.VotingManager.VotingEnabled)
			{
				base.CurrentValue = 1;
				this.Refresh();
				return;
			}
			TwitchVotingManager votingManager = twitchManager.VotingManager;
			votingManager.VoteStarted = (OnGameEventVoteAction)Delegate.Combine(votingManager.VoteStarted, new OnGameEventVoteAction(this.VoteStarted));
			twitchManager.VotingManager.QueueVote(this.voteType);
			this.GameEventState = ObjectiveTwitchVote.TwitchVoteStates.Waiting;
			return;
		}
		case ObjectiveTwitchVote.TwitchVoteStates.Waiting:
			break;
		case ObjectiveTwitchVote.TwitchVoteStates.Complete:
			base.CurrentValue = 1;
			this.Refresh();
			break;
		default:
			return;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void VoteStarted()
	{
		if (TwitchManager.Current.VotingManager.CurrentVoteType.Name == this.voteType)
		{
			this.GameEventState = ObjectiveTwitchVote.TwitchVoteStates.Complete;
		}
	}

	public override void Refresh()
	{
		if (base.Complete)
		{
			return;
		}
		base.Complete = base.CurrentValue == 1;
		if (base.Complete)
		{
			base.OwnerQuest.RefreshQuestCompletion(QuestClass.CompletionTypes.AutoComplete, null, true, null);
		}
	}

	public override BaseObjective Clone()
	{
		ObjectiveTwitchVote objectiveTwitchVote = new ObjectiveTwitchVote();
		this.CopyValues(objectiveTwitchVote);
		objectiveTwitchVote.voteType = this.voteType;
		return objectiveTwitchVote;
	}

	public override void ParseProperties(DynamicProperties properties)
	{
		base.ParseProperties(properties);
		properties.ParseString(ObjectiveTwitchVote.PropVoteType, ref this.voteType);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public string voteType = "";

	[PublicizedFrom(EAccessModifier.Protected)]
	public float timeRemaining = 2f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public static string PropVoteType = "vote_type";

	[PublicizedFrom(EAccessModifier.Protected)]
	public ObjectiveTwitchVote.TwitchVoteStates GameEventState;

	[PublicizedFrom(EAccessModifier.Protected)]
	public enum TwitchVoteStates
	{
		Start,
		Waiting,
		Complete
	}
}
