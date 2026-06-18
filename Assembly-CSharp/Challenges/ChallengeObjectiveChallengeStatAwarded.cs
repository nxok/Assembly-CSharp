using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeObjectiveChallengeStatAwarded : BaseChallengeObjective
	{
		public override ChallengeObjectiveType ObjectiveType
		{
			get
			{
				return ChallengeObjectiveType.ChallengeStatAwarded;
			}
		}

		public override string DescriptionText
		{
			get
			{
				return Localization.Get(this.statText, false, null);
			}
		}

		public override void HandleAddHooks()
		{
			QuestEventManager.Current.ChallengeAwardCredit += this.Current_ChallengeAwardCredit;
			if (this.trackerIndexName != null && this.trackingEntry == null)
			{
				this.trackingEntry = new TrackingEntry
				{
					Owner = this,
					blockIndexName = this.trackerIndexName,
					navObjectName = ((this.trackerNavObjectName != null) ? this.trackerNavObjectName : "quest_resource"),
					trackDistance = this.trackDistance
				};
				this.trackingEntry.TrackingHelper = this.Owner.GetTrackingHelper();
			}
		}

		public override void HandleRemoveHooks()
		{
			QuestEventManager.Current.ChallengeAwardCredit -= this.Current_ChallengeAwardCredit;
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_ChallengeAwardCredit(string stat, int awardCount)
		{
			if (this.CheckBaseRequirements())
			{
				return;
			}
			if (this.challengeStat.EqualsCaseInsensitive(stat))
			{
				base.Current += awardCount;
				if (base.Current >= this.MaxCount)
				{
					base.Current = this.MaxCount;
					this.CheckObjectiveComplete(true);
				}
			}
		}

		public override void HandleTrackingStarted()
		{
			base.HandleTrackingStarted();
			if (this.trackingEntry != null)
			{
				this.Owner.AddTrackingEntry(this.trackingEntry);
				this.trackingEntry.TrackingHelper = this.Owner.TrackingHandler;
				this.trackingEntry.AddHooks();
			}
		}

		public override void HandleTrackingEnded()
		{
			base.HandleTrackingEnded();
			if (this.trackingEntry != null)
			{
				this.trackingEntry.RemoveHooks();
				this.Owner.RemoveTrackingEntry(this.trackingEntry);
			}
		}

		public override void ParseElement(XElement e)
		{
			base.ParseElement(e);
			if (e.HasAttribute("challenge_stat"))
			{
				this.challengeStat = e.GetAttribute("challenge_stat");
			}
			if (e.HasAttribute("stat_text_key"))
			{
				this.statText = Localization.Get(e.GetAttribute("stat_text_key"), false, null);
			}
			else if (e.HasAttribute("stat_text"))
			{
				this.statText = e.GetAttribute("stat_text");
			}
			if (e.HasAttribute("tracker_index"))
			{
				this.trackerIndexName = e.GetAttribute("tracker_index");
			}
			if (e.HasAttribute("tracker_nav_object"))
			{
				this.trackerNavObjectName = e.GetAttribute("tracker_nav_object");
			}
			if (e.HasAttribute("track_distance"))
			{
				this.trackDistance = StringParsers.ParseFloat(e.GetAttribute("track_distance"), 0, -1, NumberStyles.Any);
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return new ChallengeObjectiveChallengeStatAwarded
			{
				challengeStat = this.challengeStat,
				statText = this.statText,
				trackerIndexName = this.trackerIndexName,
				trackerNavObjectName = this.trackerNavObjectName,
				trackDistance = this.trackDistance
			};
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public string challengeStat = "";

		[PublicizedFrom(EAccessModifier.Private)]
		public string statText = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string trackerIndexName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string trackerNavObjectName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float trackDistance = 20f;

		public TrackingEntry trackingEntry;
	}
}
