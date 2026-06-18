using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Twitch
{
	public class TwitchEventPreset
	{
		public bool HasCustomEvents
		{
			get
			{
				return this.BitEvents.Count > 0 || this.SubEvents.Count > 0 || this.GiftSubEvents.Count > 0 || this.RaidEvents.Count > 0 || this.CharityEvents.Count > 0 || this.ChannelPointEvents.Count > 0 || this.HypeTrainEvents.Count > 0 || this.CreatorGoalEvents.Count > 0;
			}
		}

		public bool HasBitEvents
		{
			get
			{
				return this.BitEvents.Count > 0;
			}
		}

		public bool HasSubEvents
		{
			get
			{
				return this.SubEvents.Count > 0;
			}
		}

		public bool HasGiftSubEvents
		{
			get
			{
				return this.GiftSubEvents.Count > 0;
			}
		}

		public bool HasRaidEvents
		{
			get
			{
				return this.RaidEvents.Count > 0;
			}
		}

		public bool HasCharityEvents
		{
			get
			{
				return this.CharityEvents.Count > 0;
			}
		}

		public bool HasChannelPointEvents
		{
			get
			{
				return this.ChannelPointEvents.Count > 0;
			}
		}

		public bool HasHypeTrainEvents
		{
			get
			{
				return this.HypeTrainEvents.Count > 0;
			}
		}

		public bool HasCreatorGoalEvents
		{
			get
			{
				return this.CreatorGoalEvents.Count > 0;
			}
		}

		public void AddBitEvent(TwitchEventEntry entry)
		{
			this.BitEvents.Add(entry);
		}

		public void AddSubEvent(TwitchSubEventEntry entry)
		{
			this.SubEvents.Add(entry);
		}

		public void AddGiftSubEvent(TwitchSubEventEntry entry)
		{
			this.GiftSubEvents.Add(entry);
		}

		public void AddRaidEvent(TwitchEventEntry entry)
		{
			this.RaidEvents.Add(entry);
		}

		public void AddCharityEvent(TwitchEventEntry entry)
		{
			this.CharityEvents.Add(entry);
		}

		public void AddChannelPointEvent(TwitchChannelPointEventEntry entry)
		{
			this.ChannelPointEvents.Add(entry);
		}

		public void AddHypeTrainEvent(TwitchHypeTrainEventEntry entry)
		{
			this.HypeTrainEvents.Add(entry);
		}

		public void AddCreatorGoalEvent(TwitchCreatorGoalEventEntry entry)
		{
			this.CreatorGoalEvents.Add(entry);
		}

		public TwitchSubEventEntry HandleSubEvent(int months, TwitchSubEventEntry.SubTierTypes tier)
		{
			for (int i = 0; i < this.SubEvents.Count; i++)
			{
				if (this.SubEvents[i].IsValid(months, "", tier))
				{
					return this.SubEvents[i];
				}
			}
			return null;
		}

		public TwitchSubEventEntry HandleGiftSubEvent(int giftCounts, TwitchSubEventEntry.SubTierTypes tier)
		{
			for (int i = 0; i < this.GiftSubEvents.Count; i++)
			{
				if (this.GiftSubEvents[i].IsValid(giftCounts, "", tier))
				{
					return this.GiftSubEvents[i];
				}
			}
			return null;
		}

		public TwitchEventEntry HandleBitRedeem(int bitAmount)
		{
			for (int i = 0; i < this.BitEvents.Count; i++)
			{
				if (this.BitEvents[i].IsValid(bitAmount, "", TwitchSubEventEntry.SubTierTypes.Any))
				{
					return this.BitEvents[i];
				}
			}
			return null;
		}

		public TwitchChannelPointEventEntry HandleChannelPointsRedeem(string title)
		{
			for (int i = 0; i < this.ChannelPointEvents.Count; i++)
			{
				if (this.ChannelPointEvents[i].ChannelPointTitle == title)
				{
					return this.ChannelPointEvents[i];
				}
			}
			return null;
		}

		public TwitchEventEntry HandleRaid(int viewerAmount)
		{
			for (int i = 0; i < this.RaidEvents.Count; i++)
			{
				if (this.RaidEvents[i].IsValid(viewerAmount, "", TwitchSubEventEntry.SubTierTypes.Any))
				{
					return this.RaidEvents[i];
				}
			}
			return null;
		}

		public TwitchEventEntry HandleCharityRedeem(int charityAmount)
		{
			for (int i = 0; i < this.CharityEvents.Count; i++)
			{
				if (this.CharityEvents[i].IsValid(charityAmount, "", TwitchSubEventEntry.SubTierTypes.Any))
				{
					return this.CharityEvents[i];
				}
			}
			return null;
		}

		public TwitchEventEntry HandleHypeTrainRedeem(int hypeTrainLevel)
		{
			for (int i = 0; i < this.HypeTrainEvents.Count; i++)
			{
				if (this.HypeTrainEvents[i].IsValid(hypeTrainLevel, "", TwitchSubEventEntry.SubTierTypes.Any))
				{
					return this.HypeTrainEvents[i];
				}
			}
			return null;
		}

		public TwitchCreatorGoalEventEntry HandleCreatorGoalEvent(string goalType)
		{
			for (int i = 0; i < this.HypeTrainEvents.Count; i++)
			{
				if (this.CreatorGoalEvents[i].IsValid(-1, goalType, TwitchSubEventEntry.SubTierTypes.Any))
				{
					return this.CreatorGoalEvents[i];
				}
			}
			return null;
		}

		public void AddChannelPointRedemptions()
		{
			if (TwitchManager.Current.Authentication != null)
			{
				string userID = TwitchManager.Current.Authentication.userID;
				for (int i = 0; i < this.ChannelPointEvents.Count; i++)
				{
					if (this.ChannelPointEvents[i].ChannelPointID == "" && this.ChannelPointEvents[i].AutoCreate)
					{
						GameManager.Instance.StartCoroutine(TwitchChannelPointEventEntry.CreateCustomRewardPost(this.ChannelPointEvents[i].SetupRewardEntry(userID), delegate(string res)
						{
							TwitchChannelPointEventEntry.CreateCustomRewardResponses createCustomRewardResponses = JsonConvert.DeserializeObject<TwitchChannelPointEventEntry.CreateCustomRewardResponses>(res);
							for (int j = 0; j < this.ChannelPointEvents.Count; j++)
							{
								if (this.ChannelPointEvents[j].ChannelPointTitle == createCustomRewardResponses.data[0].title)
								{
									this.ChannelPointEvents[j].ChannelPointID = createCustomRewardResponses.data[0].id;
									return;
								}
							}
						}, delegate(string err)
						{
							Log.Out(err);
						}));
					}
				}
				this.ChannelPointsSetup = true;
			}
		}

		public void RemoveChannelPointRedemptions(TwitchEventPreset newPreset = null)
		{
			for (int i = 0; i < this.ChannelPointEvents.Count; i++)
			{
				TwitchChannelPointEventEntry twitchChannelPointEventEntry = this.ChannelPointEvents[i];
				if (!(twitchChannelPointEventEntry.ChannelPointID == "") && this.ChannelPointEvents[i].AutoCreate && (newPreset == null || !newPreset.ChannelPointEvents.Contains(twitchChannelPointEventEntry)))
				{
					GameManager.Instance.StartCoroutine(TwitchChannelPointEventEntry.DeleteCustomRewardsDelete(this.ChannelPointEvents[i].ChannelPointID, delegate(string res)
					{
					}, delegate(string err)
					{
						Debug.LogWarning("Remove Channel Point Redeem Failed: " + err);
					}));
					this.ChannelPointEvents[i].ChannelPointID = "";
				}
			}
			this.ChannelPointsSetup = false;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchEventEntry> BitEvents = new List<TwitchEventEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchSubEventEntry> SubEvents = new List<TwitchSubEventEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchSubEventEntry> GiftSubEvents = new List<TwitchSubEventEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchEventEntry> RaidEvents = new List<TwitchEventEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchEventEntry> CharityEvents = new List<TwitchEventEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchChannelPointEventEntry> ChannelPointEvents = new List<TwitchChannelPointEventEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchHypeTrainEventEntry> HypeTrainEvents = new List<TwitchHypeTrainEventEntry>();

		[PublicizedFrom(EAccessModifier.Private)]
		public List<TwitchCreatorGoalEventEntry> CreatorGoalEvents = new List<TwitchCreatorGoalEventEntry>();

		public string Name;

		public bool IsDefault;

		public bool IsEmpty;

		public bool ChannelPointsSetup;

		public string Title;

		public string Description;
	}
}
