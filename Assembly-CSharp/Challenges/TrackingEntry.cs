using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting;

namespace Challenges
{
	[Preserve]
	public class TrackingEntry
	{
		public void AddHooks()
		{
			if (this.TrackingHelper != null)
			{
				this.TrackingHelper.AddTrackingEntry(this);
			}
			QuestEventManager.Current.BlockChange -= this.Current_BlockChange;
			QuestEventManager.Current.BlockChange += this.Current_BlockChange;
		}

		public void RemoveHooks()
		{
			if (this.TrackingHelper != null)
			{
				this.TrackingHelper.RemoveTrackingEntry(this);
			}
			QuestEventManager.Current.BlockChange -= this.Current_BlockChange;
			NavObjectManager instance = NavObjectManager.Instance;
			for (int i = this.TrackedBlocks.Count - 1; i >= 0; i--)
			{
				instance.UnRegisterNavObject(this.TrackedBlocks[i].NavObject);
				this.TrackedBlocks.RemoveAt(i);
			}
		}

		[PublicizedFrom(EAccessModifier.Private)]
		public void Current_BlockChange(Block blockOld, Block blockNew, Vector3i blockPos)
		{
			if (blockOld.IndexName == this.blockIndexName)
			{
				for (int i = 0; i < this.TrackedBlocks.Count; i++)
				{
					if (this.TrackedBlocks[i].WorldPos == blockPos)
					{
						NavObjectManager.Instance.UnRegisterNavObject(this.TrackedBlocks[i].NavObject);
						this.TrackedBlocks.RemoveAt(i);
						return;
					}
				}
			}
		}

		public void StartUpdate()
		{
			if (this.localPlayer == null)
			{
				this.localPlayer = this.Owner.Owner.Owner.Player;
			}
			for (int i = 0; i < this.TrackedBlocks.Count; i++)
			{
				this.TrackedBlocks[i].KeepAlive = false;
			}
		}

		public void HandleTrack(Chunk c)
		{
			List<Vector3i> list;
			if (c.IndexedBlocks.TryGetValue(this.blockIndexName, out list))
			{
				foreach (Vector3i vector3i in list)
				{
					Vector3i vector3i2 = c.ToWorldPos(vector3i);
					if (!c.GetBlock(vector3i).ischild && Vector3.Distance(vector3i2, this.localPlayer.position) < this.trackDistance)
					{
						this.HandleAddTrackedBlock(vector3i2);
					}
				}
			}
		}

		public void EndUpdate()
		{
			NavObjectManager instance = NavObjectManager.Instance;
			for (int i = this.TrackedBlocks.Count - 1; i >= 0; i--)
			{
				if (!this.TrackedBlocks[i].KeepAlive)
				{
					instance.UnRegisterNavObject(this.TrackedBlocks[i].NavObject);
					this.TrackedBlocks.RemoveAt(i);
				}
			}
		}

		[PublicizedFrom(EAccessModifier.Protected)]
		public void HandleAddTrackedBlock(Vector3i pos)
		{
			for (int i = 0; i < this.TrackedBlocks.Count; i++)
			{
				if (pos == this.TrackedBlocks[i].WorldPos)
				{
					this.TrackedBlocks[i].KeepAlive = true;
				}
			}
			this.TrackedBlocks.Add(new TrackingEntry.TrackedBlock(pos, this.navObjectName));
		}

		public float trackDistance = 20f;

		[PublicizedFrom(EAccessModifier.Private)]
		public EntityPlayerLocal localPlayer;

		[PublicizedFrom(EAccessModifier.Protected)]
		public List<TrackingEntry.TrackedBlock> TrackedBlocks = new List<TrackingEntry.TrackedBlock>();

		public ItemClass TrackedItem;

		public BaseChallengeObjective Owner;

		public ChallengeTrackingHandler TrackingHelper;

		public string blockIndexName = "quest_wood";

		public string navObjectName = "quest_resource";

		public class TrackedBlock
		{
			public TrackedBlock(Vector3i worldPos, string NavObjectName)
			{
				this.WorldPos = worldPos;
				this.NavObject = NavObjectManager.Instance.RegisterNavObject(NavObjectName, this.WorldPos.ToVector3Center(), "", false, -1, null);
				this.KeepAlive = true;
			}

			public Vector3i WorldPos;

			public NavObject NavObject;

			public bool KeepAlive;
		}
	}
}
