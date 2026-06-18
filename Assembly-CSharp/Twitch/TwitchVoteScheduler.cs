using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchVoteScheduler
	{
		public static TwitchVoteScheduler Current
		{
			get
			{
				if (TwitchVoteScheduler.instance == null)
				{
					TwitchVoteScheduler.instance = new TwitchVoteScheduler();
				}
				return TwitchVoteScheduler.instance;
			}
		}

		public static bool HasInstance
		{
			get
			{
				return TwitchVoteScheduler.instance != null;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public TwitchVoteScheduler()
		{
		}

		public void Cleanup()
		{
			this.ClearParticipants();
			TwitchVoteScheduler.instance = null;
		}

		public void AddParticipant(int entityID)
		{
			if (!this.votingParticipants.Contains(entityID))
			{
				this.votingParticipants.Add(entityID);
			}
		}

		public void ClearParticipants()
		{
			this.votingParticipants.Clear();
		}

		public void Init()
		{
		}

		public void Update(float deltaTime)
		{
			if (GameManager.Instance.World == null || GameManager.Instance.World.Players == null || GameManager.Instance.World.Players.Count == 0)
			{
				return;
			}
			if (this.nextVoteTime > 0f)
			{
				this.nextVoteTime -= deltaTime;
			}
			if (this.votingParticipants.Count == 0)
			{
				return;
			}
			if (this.nextVoteTime <= 0f)
			{
				if (GameManager.Instance.World.GetPrimaryPlayerId() == this.votingParticipants[0])
				{
					TwitchManager.Current.VotingManager.RequestApprovedToStart();
				}
				else
				{
					SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageTwitchVoteScheduling>().Setup(), false, this.votingParticipants[0], -1, -1, null, 192, false);
				}
				this.votingParticipants.RemoveAt(0);
				this.nextVoteTime = 3f;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public static TwitchVoteScheduler instance;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<int> votingParticipants = new List<int>();

		[PublicizedFrom(EAccessModifier.Private)]
		public float nextVoteTime;
	}
}
