using System;
using System.Collections.Generic;

namespace Challenges
{
	public class ChallengeGroupEntry
	{
		public ChallengeGroupEntry(ChallengeGroup group)
		{
			this.ChallengeGroup = group;
		}

		public void CreateChallenges(EntityPlayer player)
		{
			this.ResetChallenges(player);
			if (this.ChallengeGroup.DayReset != -1)
			{
				this.LastUpdateDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) + this.ChallengeGroup.DayReset;
			}
		}

		public void Update(int day, EntityPlayer player)
		{
			if (this.ChallengeGroup.DayReset == -1)
			{
				return;
			}
			if (this.LastUpdateDay <= day)
			{
				this.ResetChallenges(player);
				this.LastUpdateDay = day + this.ChallengeGroup.DayReset;
			}
		}

		public void AddAnyMissingChallenges(EntityPlayer player)
		{
			ChallengeJournal challengeJournal = player.challengeJournal;
			if (!this.ChallengeGroup.IsRandom)
			{
				int activeChallengeCount = this.ChallengeGroup.ActiveChallengeCount;
				int num = 0;
				while (num < this.ChallengeGroup.ChallengeClasses.Count && num < activeChallengeCount)
				{
					if (!challengeJournal.ChallengeDictionary.ContainsKey(this.ChallengeGroup.ChallengeClasses[num].Name))
					{
						Challenge challenge = this.ChallengeGroup.ChallengeClasses[num].CreateChallenge(challengeJournal);
						challenge.ChallengeGroup = this.ChallengeGroup;
						challengeJournal.AddChallenge(challenge);
					}
					num++;
				}
			}
		}

		public void ResetChallenges(EntityPlayer player)
		{
			ChallengeJournal challengeJournal = player.challengeJournal;
			if (this.ChallengeGroup.IsRandom)
			{
				challengeJournal.RemoveChallengesForGroup(this.ChallengeGroup);
				int activeChallengeCount = this.ChallengeGroup.ActiveChallengeCount;
				List<ChallengeClass> challengeClassesForCreate = this.ChallengeGroup.GetChallengeClassesForCreate();
				for (int i = 0; i < challengeClassesForCreate.Count; i++)
				{
					if (i >= activeChallengeCount)
					{
						return;
					}
					Challenge challenge = challengeClassesForCreate[i].CreateChallenge(challengeJournal);
					challenge.ChallengeGroup = this.ChallengeGroup;
					challengeJournal.AddChallenge(challenge);
					challenge.StartChallenge();
					if (challenge.IsTracked)
					{
						LocalPlayerUI.GetUIForPrimaryPlayer().xui.QuestTracker.TrackedChallenge = challenge;
					}
				}
			}
			else
			{
				int activeChallengeCount2 = this.ChallengeGroup.ActiveChallengeCount;
				this.ChallengeGroup.IsVisible(player);
				int num = 0;
				while (num < this.ChallengeGroup.ChallengeClasses.Count && num < activeChallengeCount2)
				{
					Challenge challenge2 = this.ChallengeGroup.ChallengeClasses[num].CreateChallenge(challengeJournal);
					challenge2.ChallengeGroup = this.ChallengeGroup;
					if (challengeJournal.Challenges.Count == 0)
					{
						challenge2.IsTracked = true;
					}
					challengeJournal.AddChallenge(challenge2);
					num++;
				}
			}
		}

		public ChallengeGroup ChallengeGroup;

		public int LastUpdateDay = -1;
	}
}
