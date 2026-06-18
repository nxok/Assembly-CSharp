using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchVoteEntry
	{
		public TwitchVoteEntry(string voteCommand, TwitchVote voteClass)
		{
			this.VoteCommand = voteCommand;
			this.VoteClass = voteClass;
		}

		public TwitchVote VoteClass;

		public string VoteCommand;

		public TwitchVotingManager Owner;

		public int VoteCount;

		public int Index = -1;

		public bool UIDirty = true;

		public bool Complete;

		public List<string> VoterNames = new List<string>();

		public List<int> ActiveSpawns = new List<int>();
	}
}
