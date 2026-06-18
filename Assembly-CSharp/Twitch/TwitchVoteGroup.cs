using System;
using System.Collections.Generic;
using UnityEngine;

namespace Twitch
{
	public class TwitchVoteGroup
	{
		public TwitchVoteGroup(string name)
		{
			this.Name = name;
		}

		public TwitchVoteType GetNextVoteType()
		{
			this.index++;
			if (this.index >= this.VoteTypes.Count)
			{
				this.index = 0;
			}
			return this.VoteTypes[this.index];
		}

		public void ShuffleVoteTypes()
		{
			for (int i = 0; i <= this.VoteTypes.Count * this.VoteTypes.Count; i++)
			{
				int num = Random.Range(0, this.VoteTypes.Count);
				int num2 = Random.Range(0, this.VoteTypes.Count);
				if (num != num2)
				{
					TwitchVoteType twitchVoteType = this.VoteTypes[num];
					this.VoteTypes[num] = this.VoteTypes[num2];
					this.VoteTypes[num2] = twitchVoteType;
				}
			}
		}

		public string Name = "";

		public List<TwitchVoteType> VoteTypes = new List<TwitchVoteType>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int index;

		public bool SkippedThisVote;
	}
}
