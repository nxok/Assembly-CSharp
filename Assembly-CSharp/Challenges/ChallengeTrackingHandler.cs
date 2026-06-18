using System;
using System.Collections.Generic;
using UnityEngine;

namespace Challenges
{
	public class ChallengeTrackingHandler
	{
		public bool Update(float deltaTime)
		{
			if (this.LocalPlayer == null)
			{
				return true;
			}
			if (this.Owner == null || !this.Owner.IsActive)
			{
				return false;
			}
			if (this.LocalPlayer.IsInTrader != this.lastInTrader)
			{
				this.lastInTrader = this.LocalPlayer.IsInTrader;
				this.NeedsRefresh = true;
			}
			if (Vector3.Distance(this.LastCheckedPosition, this.LocalPlayer.position) > this.RefreshDistance || this.NeedsRefresh)
			{
				this.LastCheckedPosition = this.LocalPlayer.position;
				this.HandleTracking();
				this.NeedsRefresh = false;
			}
			return true;
		}

		public void AddTrackingEntry(TrackingEntry track)
		{
			if (!this.trackingEntries.Contains(track))
			{
				this.trackingEntries.Add(track);
			}
			QuestEventManager.Current.AddTrackerToBeUpdated(this);
			this.NeedsRefresh = true;
		}

		public void RemoveTrackingEntry(TrackingEntry track)
		{
			if (this.trackingEntries.Contains(track))
			{
				this.trackingEntries.Remove(track);
				this.NeedsRefresh = true;
			}
			if (this.trackingEntries.Count == 0)
			{
				QuestEventManager.Current.RemoveTrackerToBeUpdated(this);
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void HandleTracking()
		{
			NavObjectManager instance = NavObjectManager.Instance;
			List<Chunk> chunkArrayCopySync = GameManager.Instance.World.ChunkCache.GetChunkArrayCopySync();
			for (int i = 0; i < this.trackingEntries.Count; i++)
			{
				this.trackingEntries[i].StartUpdate();
			}
			if (!this.LocalPlayer.IsInTrader)
			{
				foreach (Chunk chunk in chunkArrayCopySync)
				{
					for (int j = 0; j < this.trackingEntries.Count; j++)
					{
						this.trackingEntries[j].HandleTrack(chunk);
					}
				}
			}
			for (int k = 0; k < this.trackingEntries.Count; k++)
			{
				this.trackingEntries[k].EndUpdate();
			}
		}

		public Challenge Owner;

		public EntityPlayerLocal LocalPlayer;

		public List<TrackingEntry> trackingEntries = new List<TrackingEntry>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public Vector3 LastCheckedPosition = new Vector3(0f, 9999f, 0f);

		public float RefreshDistance = 5f;

		public bool NeedsRefresh;

		[PublicizedFrom(EAccessModifier.Private)]
		public bool lastInTrader;
	}
}
