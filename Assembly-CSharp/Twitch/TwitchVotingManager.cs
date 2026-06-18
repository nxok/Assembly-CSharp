using System;
using System.Collections.Generic;
using Audio;
using Challenges;
using UniLinq;
using UnityEngine;

namespace Twitch
{
	public class TwitchVotingManager
	{
		public int MaxDailyVotes
		{
			get
			{
				return this.maxDailyVotes;
			}
			set
			{
				if (this.maxDailyVotes != value)
				{
					this.maxDailyVotes = value;
					this.lastGameDay = 1;
				}
			}
		}

		public int CurrentVoteDayTimeRange
		{
			get
			{
				return this.currentVoteDayTimeRange;
			}
			set
			{
				if (this.currentVoteDayTimeRange != value)
				{
					this.currentVoteDayTimeRange = value;
					this.lastGameDay = 1;
				}
			}
		}

		public bool VotingEnabled
		{
			get
			{
				return !this.Owner.CurrentVotePreset.IsEmpty;
			}
		}

		public bool VotingIsActive
		{
			get
			{
				return this.VotingEnabled && this.CurrentVoteState != TwitchVotingManager.VoteStateTypes.WaitingForNextVote && this.CurrentVoteState != TwitchVotingManager.VoteStateTypes.Init && this.CurrentVoteState != TwitchVotingManager.VoteStateTypes.ReadyForVoteStart && this.CurrentVoteState != TwitchVotingManager.VoteStateTypes.RequestedVoteStart && this.CurrentVoteState != TwitchVotingManager.VoteStateTypes.VoteReady;
			}
		}

		public int VoteCount
		{
			get
			{
				if (this.voterlist == null)
				{
					return 0;
				}
				return this.voterlist.Count;
			}
		}

		public string VoteTypeText
		{
			get
			{
				return this.CurrentVoteType.Title;
			}
		}

		public string VoteTip
		{
			get
			{
				if (this.CurrentEvent != null)
				{
					return this.CurrentEvent.VoteClass.VoteTip;
				}
				return "";
			}
		}

		public string VoteOffset
		{
			get
			{
				if (this.CurrentEvent != null)
				{
					return this.CurrentEvent.VoteClass.VoteHeight;
				}
				return "0";
			}
		}

		public bool UseMystery
		{
			get
			{
				return this.CurrentVoteType.UseMystery;
			}
		}

		public int NeededLines
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public TwitchVoteType CurrentVoteType
		{
			get; [PublicizedFrom(EAccessModifier.Private)]
			set;
		}

		public TwitchVotingManager(TwitchManager owner)
		{
			this.Owner = owner;
			this.SetupVoteDayTimeRanges();
		}

		public void CleanupData()
		{
			this.VoteTypes.Clear();
			this.VoteGroups.Clear();
		}

		public void SetupLocalization()
		{
			this.chatOutput_VoteStarted = Localization.Get("TwitchChat_VoteStarted", false, null);
			this.chatOutput_VoteFinished = Localization.Get("TwitchChat_VoteFinished", false, null);
			this.dayTimeRangeOutput = Localization.Get("xuiOptionsTwitchVoteDayTimeRangeDisplay", false, null);
			this.VoteOptionA = Localization.Get("TwitchVoteOption_A", false, null);
			this.VoteOptionB = Localization.Get("TwitchVoteOption_B", false, null);
			this.VoteOptionC = Localization.Get("TwitchVoteOption_C", false, null);
			this.VoteOptionD = Localization.Get("TwitchVoteOption_D", false, null);
			this.VoteOptionE = Localization.Get("TwitchVoteOption_E", false, null);
		}

		public void AddVoteType(TwitchVoteType voteType)
		{
			this.VoteTypes.Add(voteType.Name, voteType);
			for (int i = 0; i < this.VoteGroups.Count; i++)
			{
				if (this.VoteGroups[i].Name == voteType.Group)
				{
					this.VoteGroups[i].VoteTypes.Add(voteType);
					return;
				}
			}
			TwitchVoteGroup twitchVoteGroup = new TwitchVoteGroup(voteType.Group);
			twitchVoteGroup.VoteTypes.Add(voteType);
			this.VoteGroups.Add(twitchVoteGroup);
		}

		public TwitchVoteType GetVoteType(string voteTypeName)
		{
			if (this.VoteTypes.ContainsKey(voteTypeName))
			{
				return this.VoteTypes[voteTypeName];
			}
			return null;
		}

		public void AddVote(int index, string userName)
		{
			if (!this.voterlist.Contains(userName) && this.voteList.Count > index)
			{
				this.voteList[index].VoteCount++;
				Manager.PlayInsidePlayerHead("twitch_vote_received", -1, 0f, false, false);
				this.voterlist.Add(userName);
				this.UIDirty = true;
				for (int i = 0; i < this.voteList.Count; i++)
				{
					this.voteList[i].UIDirty = true;
				}
			}
		}

		public void ClearVotes()
		{
			for (int i = 0; i < this.voteList.Count; i++)
			{
				this.voteList[i].VoteCount = 0;
				this.voteList[i].VoterNames.Clear();
			}
			this.voterlist.Clear();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void CalculateVoteTimes()
		{
			this.DailyVoteTimes.Clear();
			GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
			TwitchVotingManager.VoteDayTimeRange voteDayTimeRange = this.VoteDayTimeRanges[this.CurrentVoteDayTimeRange];
			float num = (float)(voteDayTimeRange.EndHour - voteDayTimeRange.StartHour) / (float)this.MaxDailyVotes;
			float num2 = (float)voteDayTimeRange.StartHour;
			for (int i = 0; i < this.MaxDailyVotes; i++)
			{
				float num3 = -1f;
				if (i == 0)
				{
					gameRandom.RandomRange(0f, num);
				}
				else
				{
					gameRandom.RandomRange(num - 1f, num);
				}
				int num4 = (int)(num2 + num3);
				int num5 = gameRandom.RandomRange(0, 59);
				TwitchVotingManager.DailyVoteEntry dailyVoteEntry = new TwitchVotingManager.DailyVoteEntry();
				dailyVoteEntry.VoteStartTime = GameUtils.DayTimeToWorldTime(1, num4, num5);
				dailyVoteEntry.VoteEndTime = GameUtils.DayTimeToWorldTime(1, num4 + 1, num5);
				dailyVoteEntry.Index = i + 1;
				num2 += num;
				this.DailyVoteTimes.Add(dailyVoteEntry);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ResetVoteTypeDay()
		{
			foreach (TwitchVoteType twitchVoteType in this.VoteTypes.Values)
			{
				twitchVoteType.CurrentDayCount = 0;
			}
			foreach (TwitchVote twitchVote in TwitchActionManager.TwitchVotes.Values)
			{
				twitchVote.CurrentDayCount = 0;
			}
		}

		public bool SetupVoteList(List<TwitchVoteEntry> voteList)
		{
			World world = GameManager.Instance.World;
			TwitchVoteType currentVoteType = this.CurrentVoteType;
			string name = currentVoteType.Name;
			TwitchVote twitchVote = null;
			int highestGameStage = this.Owner.HighestGameStage;
			this.ClearVotes();
			voteList.Clear();
			this.tempSortList.Clear();
			this.tempVoteGroupList.Clear();
			EntityPlayer localPlayer = this.Owner.LocalPlayer;
			if (currentVoteType.GuaranteedGroup != "")
			{
				foreach (TwitchVote twitchVote2 in TwitchActionManager.TwitchVotes.Values)
				{
					if (twitchVote2.Enabled && twitchVote2.IsInPreset(this.Owner.CurrentVotePreset) && twitchVote2.VoteTypes.Contains(name) && twitchVote2.Group == currentVoteType.GuaranteedGroup && twitchVote2.CanUse(this.hour, highestGameStage, localPlayer))
					{
						this.tempSortList.Add(twitchVote2);
					}
				}
				for (int i = 0; i < this.tempSortList.Count * 3; i++)
				{
					int num = world.GetGameRandom().RandomRange(this.tempSortList.Count);
					int num2 = world.GetGameRandom().RandomRange(this.tempSortList.Count);
					if (num != num2)
					{
						TwitchVote twitchVote3 = this.tempSortList[num];
						this.tempSortList[num] = this.tempSortList[num2];
						this.tempSortList[num2] = twitchVote3;
					}
				}
				twitchVote = this.tempSortList[0];
				this.tempSortList.Clear();
			}
			foreach (TwitchVote twitchVote4 in TwitchActionManager.TwitchVotes.Values)
			{
				if (twitchVote4.Enabled && twitchVote4.IsInPreset(this.Owner.CurrentVotePreset) && twitchVote4.VoteTypes.Contains(name) && twitchVote4.CanUse(this.hour, highestGameStage, localPlayer))
				{
					this.tempSortList.Add(twitchVote4);
				}
			}
			for (int j = 0; j < this.tempSortList.Count * 3; j++)
			{
				int num3 = world.GetGameRandom().RandomRange(this.tempSortList.Count);
				int num4 = world.GetGameRandom().RandomRange(this.tempSortList.Count);
				if (num3 != num4)
				{
					TwitchVote twitchVote5 = this.tempSortList[num3];
					this.tempSortList[num3] = this.tempSortList[num4];
					this.tempSortList[num4] = twitchVote5;
				}
			}
			this.NeededLines = 1;
			int num5 = 0;
			if (twitchVote != null)
			{
				this.tempSortList.Insert(Random.Range(0, 3), twitchVote);
			}
			for (int k = 0; k < this.tempSortList.Count; k++)
			{
				TwitchVote twitchVote6 = this.tempSortList[k];
				if (!(twitchVote6.Group != "") || !this.tempVoteGroupList.Contains(twitchVote6.Group))
				{
					if (twitchVote6.Group != "")
					{
						this.tempVoteGroupList.Add(twitchVote6.Group);
					}
					string text = this.VoteOptionA;
					switch (num5)
					{
					case 1:
						text = this.VoteOptionB;
						break;
					case 2:
						text = this.VoteOptionC;
						break;
					case 3:
						text = this.VoteOptionD;
						break;
					case 4:
						text = this.VoteOptionE;
						break;
					}
					if (twitchVote6.VoteLine1 != "" && this.NeededLines < 2)
					{
						this.NeededLines = 2;
					}
					if (twitchVote6.VoteLine2 != "" && this.NeededLines < 3)
					{
						this.NeededLines = 3;
					}
					voteList.Add(new TwitchVoteEntry(text, twitchVote6)
					{
						Owner = this,
						Index = num5
					});
					num5++;
					if (num5 == currentVoteType.VoteChoiceCount)
					{
						break;
					}
				}
			}
			return voteList.Count != 0;
		}

		public TwitchVoteEntry GetVoteWinner()
		{
			this.tempVoteList.Clear();
			int num = -1;
			for (int i = 0; i < this.voteList.Count; i++)
			{
				if (this.voteList[i].VoteCount > num)
				{
					num = this.voteList[i].VoteCount;
					this.tempVoteList.Clear();
					this.tempVoteList.Add(this.voteList[i]);
				}
				else if (this.voteList[i].VoteCount == num)
				{
					this.tempVoteList.Add(this.voteList[i]);
				}
			}
			return this.tempVoteList[GameManager.Instance.World.GetGameRandom().RandomRange(0, this.tempVoteList.Count)];
		}

		public void ResetVoteOnDeath()
		{
			TwitchVotingManager.VoteStateTypes currentVoteState = this.CurrentVoteState;
			if (currentVoteState - TwitchVotingManager.VoteStateTypes.VoteStarted <= 1)
			{
				this.readyForVote = false;
				this.Owner.LocalPlayer.TwitchVoteLock = TwitchVoteLockTypes.None;
				this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.WaitingForNextVote;
				this.ResetVoteGroupsForVote();
				if (this.VoteEventEnded != null)
				{
					this.VoteEventEnded();
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ResetVoteGroupsForVote()
		{
			for (int i = 0; i < this.VoteGroups.Count; i++)
			{
				this.VoteGroups[i].SkippedThisVote = false;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool CheckAllVoteGroupsSkipped()
		{
			for (int i = 0; i < this.VoteGroups.Count; i++)
			{
				if (!this.VoteGroups[i].SkippedThisVote)
				{
					return false;
				}
			}
			return true;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool AllowVoting()
		{
			return this.Owner.CooldownType == TwitchManager.CooldownTypes.None || ((this.Owner.CooldownType == TwitchManager.CooldownTypes.QuestCooldown || this.Owner.CooldownType == TwitchManager.CooldownTypes.QuestDisabled) && this.AllowVotesDuringQuests) || ((this.Owner.CooldownType == TwitchManager.CooldownTypes.BloodMoonCooldown || this.Owner.CooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled) && this.AllowVotesDuringBloodmoon);
		}

		public void Update(float deltaTime)
		{
			switch (this.CurrentVoteState)
			{
			case TwitchVotingManager.VoteStateTypes.Init:
				GameEventManager.Current.GameEventApproved += this.Current_GameEventApproved;
				GameEventManager.Current.GameEventCompleted += this.Current_GameEventCompleted;
				GameEventManager.Current.GameEntitySpawned += this.Current_GameEntitySpawned;
				GameEventManager.Current.GameEntityKilled += this.Current_GameEntityKilled;
				this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.WaitingForNextVote;
				this.ShuffleVoteGroups();
				this.ShuffleVoteGroupVoteTypes();
				return;
			case TwitchVotingManager.VoteStateTypes.WaitingForNextVote:
			{
				if (this.QueuedVoteType != null)
				{
					this.CurrentVoteType = this.QueuedVoteType;
					if (this.SetupVoteList(this.voteList))
					{
						this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.ReadyForVoteStart;
						this.Owner.RefreshVoteLockedLevel();
					}
					this.QueuedVoteType = null;
				}
				if (this.VoteStartDelayTimeRemaining > 0f)
				{
					this.VoteStartDelayTimeRemaining -= deltaTime;
					return;
				}
				World world = GameManager.Instance.World;
				ulong num = world.worldTime;
				this.day = GameUtils.WorldTimeToDays(num);
				num %= 24000UL;
				this.hour = GameUtils.WorldTimeToHours(num);
				if (this.day == 1)
				{
					return;
				}
				TwitchVotingManager.VoteDayTimeRange voteDayTimeRange = this.VoteDayTimeRanges[this.CurrentVoteDayTimeRange];
				bool flag = !this.AllowVotesDuringBloodmoon && world.IsWorldEvent(World.WorldEvent.BloodMoon);
				if (this.VoteInProgress)
				{
					if (flag)
					{
						this.CancelVote();
					}
					if (this.hour < voteDayTimeRange.StartHour || this.hour > voteDayTimeRange.EndHour)
					{
						this.CancelVote();
					}
					return;
				}
				if (flag || this.hour < voteDayTimeRange.StartHour || this.hour > voteDayTimeRange.EndHour)
				{
					return;
				}
				if (this.day != this.lastGameDay)
				{
					this.CalculateVoteTimes();
					this.ResetVoteTypeDay();
					this.lastGameDay = this.day;
				}
				for (int i = 0; i < this.DailyVoteTimes.Count; i++)
				{
					if (this.DailyVoteTimes[i].LastVoteDay != this.day)
					{
						if (num > this.DailyVoteTimes[i].VoteStartTime && num < this.DailyVoteTimes[i].VoteEndTime)
						{
							if (!this.SetReadyForVote(this.DailyVoteTimes[i].Index))
							{
								if (this.CheckAllVoteGroupsSkipped())
								{
									this.DailyVoteTimes[i].LastVoteDay = this.day;
									this.ResetVoteGroupsForVote();
								}
								return;
							}
							this.ResetVoteGroupsForVote();
							if (this.SetupVoteList(this.voteList))
							{
								this.DailyVoteTimes[i].LastVoteDay = this.day;
								this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.ReadyForVoteStart;
								this.Owner.RefreshVoteLockedLevel();
							}
						}
						else if (num > this.DailyVoteTimes[i].VoteEndTime)
						{
							this.DailyVoteTimes[i].LastVoteDay = this.day;
						}
					}
				}
				return;
			}
			case TwitchVotingManager.VoteStateTypes.ReadyForVoteStart:
				if (this.VoteStartDelayTimeRemaining > 0f)
				{
					this.VoteStartDelayTimeRemaining -= deltaTime;
					return;
				}
				if (this.VotingEnabled && this.Owner.VoteLockedLevel == TwitchVoteLockTypes.None && this.AllowVoting() && (!this.CurrentVoteType.SpawnBlocked || this.Owner.ReadyForVote))
				{
					if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
					{
						SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTwitchVoteScheduling>().Setup(), false);
					}
					else
					{
						TwitchVoteScheduler.Current.AddParticipant(this.Owner.LocalPlayer.entityId);
					}
					this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.RequestedVoteStart;
					return;
				}
				break;
			case TwitchVotingManager.VoteStateTypes.RequestedVoteStart:
				break;
			case TwitchVotingManager.VoteStateTypes.VoteReady:
				if (this.VotingEnabled && this.Owner.VoteLockedLevel == TwitchVoteLockTypes.None && !this.CheckVoteLock() && this.AllowVoting() && (!this.CurrentVoteType.SpawnBlocked || this.Owner.ReadyForVote))
				{
					this.StartVote();
					return;
				}
				break;
			case TwitchVotingManager.VoteStateTypes.VoteStarted:
				if (this.VotingEnabled && this.AllowVoting())
				{
					this.VoteTimeRemaining -= Time.deltaTime;
					if (this.VoteTimeRemaining <= 0f)
					{
						this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.VoteFinished;
						return;
					}
				}
				break;
			case TwitchVotingManager.VoteStateTypes.VoteFinished:
				this.CurrentEvent = this.GetVoteWinner();
				this.CurrentEvent.ActiveSpawns.Clear();
				this.CurrentEvent.VoteClass.CurrentDayCount++;
				this.Owner.ircClient.SendChannelMessage(string.Format(this.chatOutput_VoteFinished, this.CurrentEvent.VoteClass.VoteDescription), true);
				this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.WaitingForActive;
				this.VoteTimeRemaining = 2f;
				QuestEventManager.Current.TwitchEventReceived(TwitchObjectiveTypes.VoteComplete, this.CurrentEvent.VoteClass.Group);
				return;
			case TwitchVotingManager.VoteStateTypes.WaitingForActive:
				if (this.VoteTimeRemaining > 0f)
				{
					this.VoteTimeRemaining -= deltaTime;
					return;
				}
				if (GameEventManager.Current.HandleAction(this.CurrentEvent.VoteClass.GameEvent, this.Owner.LocalPlayer, this.Owner.LocalPlayer, true, " ", "vote", this.Owner.AllowCrateSharing, true, "", null))
				{
					GameEventManager.Current.HandleGameEventApproved(this.CurrentEvent.VoteClass.GameEvent, this.Owner.LocalPlayer.entityId, " ", "vote");
					return;
				}
				this.VoteTimeRemaining = 10f;
				return;
			case TwitchVotingManager.VoteStateTypes.EventActive:
				if (this.VoteEventTimeRemaining < 0f)
				{
					if (this.VoteEventComplete)
					{
						this.HandleGameEventEnded(true);
						return;
					}
				}
				else
				{
					this.VoteEventTimeRemaining -= deltaTime;
				}
				break;
			default:
				return;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool CheckVoteLock()
		{
			return (!this.AllowVotesDuringQuests && QuestEventManager.Current.QuestBounds.width != 0f) || (!this.AllowVotesInSafeZone && this.Owner.IsSafe);
		}

		public bool IsHighest(TwitchVoteEntry vote)
		{
			for (int i = 0; i < this.voteList.Count; i++)
			{
				if (i != vote.Index && this.voteList[i].VoteCount > vote.VoteCount)
				{
					return false;
				}
			}
			return true;
		}

		public bool SetReadyForVote(int index)
		{
			return this.GetNextVoteType();
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ResetCurrentVote()
		{
			this.VoteTimeRemaining = this.VoteTime;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void SetupVoteDayTimeRanges()
		{
			this.VoteDayTimeRanges.Clear();
			this.VoteDayTimeRanges.Add(new TwitchVotingManager.VoteDayTimeRange
			{
				Name = Localization.Get("TwitchVoteDayTimeRange_Short", false, null),
				StartHour = 8,
				EndHour = 16
			});
			this.VoteDayTimeRanges.Add(new TwitchVotingManager.VoteDayTimeRange
			{
				Name = Localization.Get("TwitchVoteDayTimeRange_Average", false, null),
				StartHour = 6,
				EndHour = 18
			});
			this.VoteDayTimeRanges.Add(new TwitchVotingManager.VoteDayTimeRange
			{
				Name = Localization.Get("TwitchVoteDayTimeRange_Extended", false, null),
				StartHour = 4,
				EndHour = 20
			});
			this.VoteDayTimeRanges.Add(new TwitchVotingManager.VoteDayTimeRange
			{
				Name = Localization.Get("TwitchVoteDayTimeRange_All", false, null),
				StartHour = 0,
				EndHour = 23
			});
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ShuffleVoteGroups()
		{
			for (int i = 0; i <= this.VoteGroups.Count * this.VoteGroups.Count; i++)
			{
				int num = Random.Range(0, this.VoteGroups.Count);
				int num2 = Random.Range(0, this.VoteGroups.Count);
				if (num != num2)
				{
					TwitchVoteGroup twitchVoteGroup = this.VoteGroups[num];
					this.VoteGroups[num] = this.VoteGroups[num2];
					this.VoteGroups[num2] = twitchVoteGroup;
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void ShuffleVoteGroupVoteTypes()
		{
			for (int i = 0; i < this.VoteGroups.Count; i++)
			{
				this.VoteGroups[i].ShuffleVoteTypes();
			}
		}

		public void CancelVote()
		{
			if (this.CurrentVoteState == TwitchVotingManager.VoteStateTypes.WaitingForNextVote && this.readyForVote)
			{
				this.readyForVote = false;
			}
		}

		public void RequestApprovedToStart()
		{
			this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.VoteReady;
		}

		public void StartVote()
		{
			this.VoteTimeRemaining = this.VoteTime;
			this.voterlist.Clear();
			if (this.VoteStarted != null)
			{
				this.VoteStarted();
			}
			this.Owner.UIDirty = true;
			this.Owner.LocalPlayer.TwitchVoteLock = (this.CurrentVoteType.ActionLockout ? TwitchVoteLockTypes.ActionsLocked : TwitchVoteLockTypes.VoteLocked);
			this.Owner.ircClient.SendChannelMessage(this.chatOutput_VoteStarted, true);
			Manager.BroadcastPlay(this.Owner.LocalPlayer.position, "twitch_vote_started", 0f);
			this.readyForVote = false;
			this.CurrentVoteType.CurrentDayCount++;
			this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.VoteStarted;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameEntitySpawned(string gameEventID, int entityID, string tag)
		{
			if (this.CurrentEvent == null || tag != "vote")
			{
				return;
			}
			if (gameEventID == this.CurrentEvent.VoteClass.GameEvent)
			{
				this.CurrentEvent.ActiveSpawns.Add(entityID);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameEntityKilled(int entityID)
		{
			if (this.CurrentEvent == null)
			{
				return;
			}
			if (this.CurrentEvent.ActiveSpawns.Contains(entityID))
			{
				this.CurrentEvent.ActiveSpawns.Remove(entityID);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameEventCompleted(string gameEventID, int targetEntityID, string extraData, string tag)
		{
			if (this.CurrentEvent != null && this.CurrentEvent.VoteClass.GameEvent == gameEventID && tag == "vote")
			{
				this.VoteEventComplete = true;
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void HandleGameEventEnded(bool playSound)
		{
			if (this.CurrentVoteType.CooldownOnEnd && this.Owner.AllowActions && this.Owner.CurrentCooldownPreset.CooldownType == CooldownPreset.CooldownTypes.Fill)
			{
				this.Owner.SetCooldown((float)this.Owner.CurrentCooldownPreset.NextCooldownTime, TwitchManager.CooldownTypes.MaxReached, false, true);
			}
			this.CurrentEvent.VoteClass.HandleVoteComplete();
			this.CurrentEvent = null;
			if (playSound)
			{
				Manager.BroadcastPlay(this.Owner.LocalPlayer.position, "twitch_vote_ended", 0f);
			}
			this.Owner.LocalPlayer.TwitchVoteLock = TwitchVoteLockTypes.None;
			this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.WaitingForNextVote;
			this.ResetVoteGroupsForVote();
			if (this.VoteEventEnded != null)
			{
				this.VoteEventEnded();
			}
			this.VoteStartDelayTimeRemaining = 10f;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public bool GetNextVoteType()
		{
			if (this.voteGroupIndex == -1)
			{
				this.voteGroupIndex = GameManager.Instance.World.GetGameRandom().RandomRange(this.VoteGroups.Count);
			}
			bool flag = GameManager.Instance.World.IsWorldEvent(World.WorldEvent.BloodMoon);
			TwitchVoteGroup twitchVoteGroup = this.VoteGroups[this.voteGroupIndex];
			if (!twitchVoteGroup.SkippedThisVote)
			{
				for (int i = 0; i < twitchVoteGroup.VoteTypes.Count; i++)
				{
					TwitchVoteType nextVoteType = twitchVoteGroup.GetNextVoteType();
					if (nextVoteType.IsInPreset(this.Owner.CurrentVotePreset.Name) && !nextVoteType.ManualStart && nextVoteType.CanUse() && this.hour >= nextVoteType.AllowedStartHour && this.hour <= nextVoteType.AllowedEndHour && (!nextVoteType.IsBoss || this.Owner.CurrentVotePreset.BossVoteSetting != TwitchVotingManager.BossVoteSettings.Disabled) && (nextVoteType.AllowedWithActions || !this.Owner.AllowActions) && ((nextVoteType.IsBoss && this.Owner.CurrentVotePreset.BossVoteSetting == TwitchVotingManager.BossVoteSettings.Daily) || ((nextVoteType.BloodMoonDay || this.Owner.nextBMDay != this.day) && (nextVoteType.BloodMoonAllowed || !flag))))
					{
						this.CurrentVoteType = nextVoteType;
						this.voteGroupIndex++;
						if (this.voteGroupIndex >= this.VoteGroups.Count)
						{
							this.voteGroupIndex = 0;
						}
						return true;
					}
				}
				twitchVoteGroup.SkippedThisVote = true;
			}
			this.voteGroupIndex++;
			if (this.voteGroupIndex >= this.VoteGroups.Count)
			{
				this.voteGroupIndex = 0;
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_GameEventApproved(string gameEventID, int targetEntityID, string extraData, string tag)
		{
			if (this.CurrentVoteState == TwitchVotingManager.VoteStateTypes.WaitingForActive && this.CurrentEvent.VoteClass.GameEvent == gameEventID)
			{
				this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.EventActive;
				this.VoteEventComplete = false;
				this.VoteEventTimeRemaining = 10f;
				this.CurrentEvent.VoterNames.AddRange(this.voterlist);
				this.Owner.AddVoteHistory(this.CurrentEvent.VoteClass);
				if (this.VoteEventStarted != null)
				{
					this.VoteEventStarted();
				}
			}
		}

		public void HandleMessage(TwitchIRCClient.TwitchChatMessage message)
		{
			if (this.CurrentVoteState == TwitchVotingManager.VoteStateTypes.VoteStarted)
			{
				if (message.Message.EqualsCaseInsensitive(this.VoteOptionA))
				{
					this.AddVote(0, message.UserName);
					return;
				}
				if (message.Message.EqualsCaseInsensitive(this.VoteOptionB))
				{
					this.AddVote(1, message.UserName);
					return;
				}
				if (message.Message.EqualsCaseInsensitive(this.VoteOptionC))
				{
					this.AddVote(2, message.UserName);
					return;
				}
				if (message.Message.EqualsCaseInsensitive(this.VoteOptionD))
				{
					this.AddVote(3, message.UserName);
					return;
				}
				if (message.Message.EqualsCaseInsensitive(this.VoteOptionE))
				{
					this.AddVote(4, message.UserName);
				}
			}
		}

		public List<string> HandleKiller(TwitchVoteEntry voteEntry)
		{
			if (this.CurrentEvent == null && voteEntry == null)
			{
				return null;
			}
			if (this.CurrentEvent != null)
			{
				List<string> voterNames = this.CurrentEvent.VoterNames;
				this.CurrentEvent.Complete = true;
				this.HandleGameEventEnded(false);
				return this.voterlist;
			}
			if (voteEntry != null)
			{
				return voteEntry.VoterNames;
			}
			return null;
		}

		public string GetDayTimeRange(int tempVoteDayTimeRange)
		{
			TwitchVotingManager.VoteDayTimeRange voteDayTimeRange = this.VoteDayTimeRanges[tempVoteDayTimeRange];
			if (voteDayTimeRange.StartHour == 0 && voteDayTimeRange.EndHour == 23)
			{
				return voteDayTimeRange.Name;
			}
			return string.Format(this.dayTimeRangeOutput, voteDayTimeRange.StartHour, voteDayTimeRange.EndHour);
		}

		public void QueueVote(string voteType)
		{
			if (this.VoteTypes.ContainsKey(voteType))
			{
				this.QueuedVoteType = this.VoteTypes[voteType];
			}
		}

		public void ForceEndVote()
		{
			if (this.CurrentVoteState == TwitchVotingManager.VoteStateTypes.VoteStarted)
			{
				this.CurrentEvent = null;
				this.Owner.LocalPlayer.TwitchVoteLock = TwitchVoteLockTypes.None;
				this.CurrentVoteState = TwitchVotingManager.VoteStateTypes.WaitingForNextVote;
				if (this.VoteEventEnded != null)
				{
					this.VoteEventEnded();
				}
			}
		}

		public TwitchManager Owner;

		public TwitchVotingManager.VoteStateTypes CurrentVoteState;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_VoteStarted;

		[PublicizedFrom(EAccessModifier.Private)]
		public string chatOutput_VoteFinished;

		[PublicizedFrom(EAccessModifier.Private)]
		public string dayTimeRangeOutput;

		[PublicizedFrom(EAccessModifier.Private)]
		public string VoteOptionA;

		[PublicizedFrom(EAccessModifier.Private)]
		public string VoteOptionB;

		[PublicizedFrom(EAccessModifier.Private)]
		public string VoteOptionC;

		[PublicizedFrom(EAccessModifier.Private)]
		public string VoteOptionD;

		[PublicizedFrom(EAccessModifier.Private)]
		public string VoteOptionE;

		[PublicizedFrom(EAccessModifier.Private)]
		public int maxDailyVotes = 4;

		public int lastGameDay = 1;

		public bool WinnerShowing;

		[PublicizedFrom(EAccessModifier.Private)]
		public int currentVoteDayTimeRange = 2;

		public List<TwitchVotingManager.VoteDayTimeRange> VoteDayTimeRanges = new List<TwitchVotingManager.VoteDayTimeRange>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchVotingManager.DailyVoteEntry> DailyVoteTimes = new List<TwitchVotingManager.DailyVoteEntry>();

		public bool AllowVotesDuringBloodmoon;

		public bool AllowVotesDuringQuests;

		public bool AllowVotesInSafeZone;

		public List<TwitchVoteType> NextVotes = new List<TwitchVoteType>();

		public bool VoteInProgress;

		public float VoteTime = 60f;

		public int ViewerDefeatReward = 250;

		public float VoteStartDelayTimeRemaining;

		public float VoteEventTimeRemaining;

		public float VoteTimeRemaining;

		public bool UIDirty;

		public bool VoteEventComplete;

		public List<TwitchVoteEntry> voteList = new List<TwitchVoteEntry>();

		public List<string> voterlist = new List<string>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchVoteEntry> tempVoteList = new List<TwitchVoteEntry>();

		public OnGameEventVoteAction VoteStarted;

		public OnGameEventVoteAction VoteEventStarted;

		public OnGameEventVoteAction VoteEventEnded;

		public Dictionary<string, TwitchVoteType> VoteTypes = new Dictionary<string, TwitchVoteType>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int voteGroupIndex = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchVoteGroup> VoteGroups = new List<TwitchVoteGroup>();

		[PublicizedFrom(EAccessModifier.Private)]
		public TwitchVoteType QueuedVoteType;

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchVote> tempSortList = new List<TwitchVote>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<string> tempVoteGroupList = new List<string>();

		[PublicizedFrom(EAccessModifier.Private)]
		public int day = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public int hour = -1;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool readyForVote;

		public TwitchVoteEntry CurrentEvent;

		public enum VoteStateTypes
		{
			Init,
			WaitingForNextVote,
			ReadyForVoteStart,
			RequestedVoteStart,
			VoteReady,
			VoteStarted,
			VoteFinished,
			WaitingForActive,
			EventActive
		}

		public enum BossVoteSettings
		{
			Disabled,
			Standard,
			Daily
		}

		public class DailyVoteEntry
		{
			public ulong VoteStartTime;

			public ulong VoteEndTime;

			public int LastVoteDay;

			public int Index = -1;
		}

		public class VoteDayTimeRange
		{
			public string Name;

			public int StartHour;

			public int EndHour;
		}
	}
}
