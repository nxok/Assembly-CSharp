using System;
using System.Collections.Generic;
using UnityEngine;

namespace Quests
{
	public class TrackingHandler
	{
		public bool Update(float deltaTime)
		{
			if (this.LocalPlayer == null)
			{
				return true;
			}
			Quest quest = this.LocalPlayer.QuestJournal.FindActiveQuest(this.QuestCode);
			if (quest == null || quest.OwnerJournal == null || quest.OwnerJournal.OwnerPlayer == null)
			{
				return false;
			}
			if (Vector3.Distance(this.LastCheckedPosition, this.LocalPlayer.position) > this.RefreshDistance || this.NeedsRefresh)
			{
				this.LastCheckedPosition = this.LocalPlayer.position;
				this.HandleTracking();
				this.NeedsRefresh = false;
			}
			return true;
		}

		public void AddTrackingEntry(ObjectiveModifierTrackBlocks track)
		{
			if (!this.trackingEntries.Contains(track))
			{
				this.trackingEntries.Add(track);
				this.NeedsRefresh = true;
			}
			QuestEventManager.Current.AddTrackerToBeUpdated(this);
		}

		public void RemoveTrackingEntry(ObjectiveModifierTrackBlocks track)
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
			foreach (Chunk chunk in chunkArrayCopySync)
			{
				for (int j = 0; j < this.trackingEntries.Count; j++)
				{
					this.trackingEntries[j].HandleTrack(chunk);
				}
			}
			for (int k = 0; k < this.trackingEntries.Count; k++)
			{
				this.trackingEntries[k].EndUpdate();
			}
		}

		public int QuestCode;

		public EntityPlayerLocal LocalPlayer;

		public List<ObjectiveModifierTrackBlocks> trackingEntries = new List<ObjectiveModifierTrackBlocks>();

		[PublicizedFrom(EAccessModifier.Protected)]
		public Vector3 LastCheckedPosition = new Vector3(0f, 9999f, 0f);

		public float RefreshDistance = 5f;

		public bool NeedsRefresh;
	}
}
