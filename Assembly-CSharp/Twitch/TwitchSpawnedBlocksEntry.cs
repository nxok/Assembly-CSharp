using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchSpawnedBlocksEntry
	{
		public bool CheckPos(Vector3i pos)
		{
			for (int i = 0; i < this.blocks.Count; i++)
			{
				if (this.blocks[i] == pos)
				{
					return true;
				}
			}
			if (this.recentlyRemoved != null)
			{
				for (int j = 0; j < this.recentlyRemoved.Count; j++)
				{
					if (this.recentlyRemoved[j] == pos)
					{
						return true;
					}
				}
			}
			return false;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public bool RemoveBlock(Vector3i blockRemoved)
		{
			for (int i = this.blocks.Count - 1; i >= 0; i--)
			{
				if (this.blocks[i] == blockRemoved)
				{
					if (this.recentlyRemoved == null)
					{
						this.recentlyRemoved = new List<Vector3i>();
					}
					this.recentlyRemoved.Add(this.blocks[i]);
					this.blocks.RemoveAt(i);
				}
			}
			return this.blocks.Count == 0;
		}

		[PublicizedFrom(EAccessModifier.Internal)]
		public bool RemoveBlocks(List<Vector3i> blocksRemoved)
		{
			for (int i = this.blocks.Count - 1; i >= 0; i--)
			{
				for (int j = 0; j < blocksRemoved.Count; j++)
				{
					if (this.blocks[i] == blocksRemoved[j])
					{
						this.blocks.RemoveAt(i);
						break;
					}
				}
			}
			return this.blocks.Count == 0;
		}

		public List<Vector3i> blocks;

		public List<Vector3i> recentlyRemoved;

		public TwitchActionEntry Action;

		public TwitchEventActionEntry Event;

		public TwitchVoteEntry Vote;

		public int BlockGroupID = -1;

		public float TimeRemaining = -1f;

		public TwitchRespawnEntry RespawnEntry;
	}
}
