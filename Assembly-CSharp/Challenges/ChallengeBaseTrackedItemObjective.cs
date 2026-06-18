using System;
using System.Globalization;
using System.Xml.Linq;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class ChallengeBaseTrackedItemObjective : BaseChallengeObjective
	{
		public override void Init()
		{
			this.expectedItem = ItemClass.GetItem(this.itemClassID, false);
			this.expectedItemClass = ItemClass.GetItemClass(this.itemClassID, false);
		}

		public void SetupItem(string itemID)
		{
			this.itemClassID = itemID;
		}

		public override void HandleAddHooks()
		{
			if (this.expectedItemClass != null)
			{
				string text = ((this.overrideTrackerIndexName != null) ? this.overrideTrackerIndexName : this.expectedItemClass.TrackerIndexName);
				if (text != null && this.trackingEntry == null && !this.disableTracking)
				{
					this.trackingEntry = new TrackingEntry
					{
						TrackedItem = this.expectedItemClass,
						Owner = this,
						blockIndexName = text,
						navObjectName = ((this.expectedItemClass.TrackerNavObject != null) ? this.expectedItemClass.TrackerNavObject : "quest_resource"),
						trackDistance = this.trackDistance
					};
					this.trackingEntry.TrackingHelper = this.Owner.GetTrackingHelper();
				}
			}
			base.HandleAddHooks();
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
			if (e.HasAttribute("item"))
			{
				this.itemClassID = e.GetAttribute("item");
			}
			if (e.HasAttribute("override_tracker_index"))
			{
				this.overrideTrackerIndexName = e.GetAttribute("override_tracker_index");
			}
			if (e.HasAttribute("track_distance"))
			{
				this.trackDistance = StringParsers.ParseFloat(e.GetAttribute("track_distance"), 0, -1, NumberStyles.Any);
			}
			if (e.HasAttribute("disable_tracking"))
			{
				this.disableTracking = StringParsers.ParseBool(e.GetAttribute("disable_tracking"), 0, -1, true);
			}
		}

		public override void CopyValues(BaseChallengeObjective obj, BaseChallengeObjective objFromClass)
		{
			base.CopyValues(obj, objFromClass);
			ChallengeBaseTrackedItemObjective challengeBaseTrackedItemObjective = objFromClass as ChallengeBaseTrackedItemObjective;
			if (challengeBaseTrackedItemObjective != null)
			{
				this.itemClassID = challengeBaseTrackedItemObjective.itemClassID;
				this.overrideTrackerIndexName = challengeBaseTrackedItemObjective.overrideTrackerIndexName;
				this.trackDistance = challengeBaseTrackedItemObjective.trackDistance;
				this.disableTracking = challengeBaseTrackedItemObjective.disableTracking;
			}
		}

		public override BaseChallengeObjective Clone()
		{
			return null;
		}

		public override void CompleteObjective(bool handleComplete = true)
		{
			base.Current = this.MaxCount;
			base.Complete = true;
			if (handleComplete)
			{
				this.Owner.HandleComplete(true);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public ItemValue expectedItem = ItemValue.None;

		[PublicizedFrom(EAccessModifier.Protected)]
		public ItemClass expectedItemClass;

		[PublicizedFrom(EAccessModifier.Protected)]
		public string itemClassID = "";

		[PublicizedFrom(EAccessModifier.Protected)]
		public string overrideTrackerIndexName;

		[PublicizedFrom(EAccessModifier.Protected)]
		public float trackDistance = 20f;

		[PublicizedFrom(EAccessModifier.Protected)]
		public bool disableTracking;

		public TrackingEntry trackingEntry;
	}
}
