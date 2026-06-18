using System;

namespace Twitch
{
	public class TwitchActionHistoryEntry
	{
		public TwitchActionHistoryEntry(string username, string usercolor, TwitchAction action, TwitchVote vote, TwitchEventActionEntry eventEntry)
		{
			this.UserName = username;
			this.Action = action;
			this.Vote = vote;
			this.EventEntry = eventEntry;
			this.UserColor = usercolor;
			ValueTuple<int, int, int> valueTuple = GameUtils.WorldTimeToElements(GameManager.Instance.World.worldTime);
			int item = valueTuple.Item1;
			int item2 = valueTuple.Item2;
			int item3 = valueTuple.Item3;
			this.ActionTime = string.Format("{0} {1}, {2:00}:{3:00}", new object[]
			{
				Localization.Get("xuiDay", false, null),
				item,
				item2,
				item3
			});
		}

		public string Command
		{
			get
			{
				if (this.Action != null)
				{
					return string.Format("{0}({1})", this.Action.Command, this.PointsSpent);
				}
				if (this.Vote != null)
				{
					return this.Vote.VoteDescription;
				}
				if (this.EventEntry != null)
				{
					return this.EventEntry.Event.Description(this.EventEntry);
				}
				return "";
			}
		}

		public string Title
		{
			get
			{
				if (this.Action != null)
				{
					return this.Action.Title;
				}
				if (this.Vote != null)
				{
					return this.Vote.VoteDescription;
				}
				if (this.EventEntry != null)
				{
					return this.EventEntry.Event.EventTitle;
				}
				return "";
			}
		}

		public string Description
		{
			get
			{
				if (this.Action != null)
				{
					return this.Action.Description;
				}
				if (this.Vote != null)
				{
					return this.Vote.Description;
				}
				if (this.EventEntry != null)
				{
					return this.EventEntry.Event.EventTitle;
				}
				return "";
			}
		}

		public string HistoryType
		{
			get
			{
				if (this.Action != null)
				{
					return "action";
				}
				if (this.Vote != null)
				{
					return "vote";
				}
				if (this.EventEntry != null)
				{
					return "event";
				}
				return "";
			}
		}

		public bool IsRefunded
		{
			get
			{
				return this.EntryState == TwitchActionHistoryEntry.EntryStates.Refunded;
			}
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public bool IsValid()
		{
			return this.UserName != null && ((this.Action != null && this.Action.Command != null) || this.Vote != null || this.EventEntry != null);
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public void Refund()
		{
			if (this.EntryState != TwitchActionHistoryEntry.EntryStates.Refunded)
			{
				TwitchManager twitchManager = TwitchManager.Current;
				twitchManager.ViewerData.ReimburseAction(this.UserName, this.PointsSpent, this.Action);
				twitchManager.LocalPlayer.PlayOneShot("ui_vending_purchase", false, false, false, null, 1f);
				this.EntryState = TwitchActionHistoryEntry.EntryStates.Refunded;
			}
		}

		public void Retry()
		{
			if (!this.HasRetried)
			{
				if (this.Action != null)
				{
					TwitchManager.Current.HandleExtensionMessage(this.UserID, string.Format("{0} {1}", this.Action.Command, this.Target), true, 0, 0);
				}
				else if (this.EventEntry != null)
				{
					this.EventEntry.IsSent = false;
					this.EventEntry.IsRetry = true;
					TwitchManager.Current.EventQueue.Add(this.EventEntry);
				}
				this.HasRetried = true;
			}
		}

		public bool CanRetry()
		{
			if (this.HasRetried)
			{
				return false;
			}
			TwitchManager.CooldownTypes cooldownType = TwitchManager.Current.CooldownType;
			if (this.Action != null)
			{
				if (TwitchManager.Current.VotingManager.VotingIsActive)
				{
					return false;
				}
				if (!this.Action.IgnoreCooldown)
				{
					return cooldownType != TwitchManager.CooldownTypes.BloodMoonDisabled && cooldownType != TwitchManager.CooldownTypes.Time && cooldownType != TwitchManager.CooldownTypes.QuestDisabled && ((cooldownType != TwitchManager.CooldownTypes.MaxReachedWaiting && cooldownType != TwitchManager.CooldownTypes.SafeCooldown) || !this.Action.WaitingBlocked) && !this.HasRetried;
				}
				return !this.HasRetried;
			}
			else
			{
				if (this.EventEntry != null)
				{
					if (!this.EventEntry.Event.CooldownAllowed)
					{
						if (cooldownType == TwitchManager.CooldownTypes.BloodMoonDisabled || cooldownType == TwitchManager.CooldownTypes.Time || cooldownType == TwitchManager.CooldownTypes.QuestDisabled)
						{
							return false;
						}
						if (cooldownType == TwitchManager.CooldownTypes.MaxReachedWaiting)
						{
							return false;
						}
					}
					return (this.EventEntry.Event.StartingCooldownAllowed || cooldownType != TwitchManager.CooldownTypes.Startup) && (this.EventEntry.Event.VoteEventAllowed || !TwitchManager.Current.VotingManager.VotingIsActive) && !this.HasRetried;
				}
				return false;
			}
		}

		public bool CanRefund()
		{
			return this.PointsSpent > 0 && this.EntryState != TwitchActionHistoryEntry.EntryStates.Refunded && this.EntryState != TwitchActionHistoryEntry.EntryStates.Reimbursed && this.Action != null;
		}

		public string UserName;

		public string UserColor;

		public string Target;

		public int UserID;

		public TwitchAction Action;

		public TwitchVote Vote;

		public TwitchActionEntry ActionEntry;

		public TwitchEventActionEntry EventEntry;

		public int PointsSpent;

		public bool HasRetried;

		public string ActionTime;

		public TwitchActionHistoryEntry.EntryStates EntryState;

		public enum EntryStates
		{
			Waiting,
			Completed,
			Reimbursed,
			Despawned,
			Refunded
		}
	}
}
