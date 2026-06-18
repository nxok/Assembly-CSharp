using System;
using System.Collections.Generic;

namespace Twitch
{
	public class TwitchRespawnEntry
	{
		public TwitchRespawnEntry(string username, int respawnsLeft, EntityPlayer target, TwitchAction action)
		{
			this.UserName = username;
			this.Target = target;
			this.Action = action;
			this.RespawnsLeft = respawnsLeft;
		}

		public bool CheckRespawn(string username, EntityPlayer target, TwitchAction action)
		{
			return this.UserName == username && this.Target == target && this.Action == action;
		}

		public bool RemoveSpawnedEntry(int entityID, bool checkForRemove)
		{
			bool flag = false;
			for (int i = this.SpawnedEntities.Count - 1; i >= 0; i--)
			{
				if (this.SpawnedEntities[i] == entityID)
				{
					flag = true;
					this.SpawnedEntities.RemoveAt(i);
				}
			}
			if (checkForRemove)
			{
				this.CheckReadyForRemove();
			}
			return flag;
		}

		public bool RemoveSpawnedBlock(Vector3i pos, bool checkForRemove)
		{
			bool flag = false;
			for (int i = this.SpawnedBlocks.Count - 1; i >= 0; i--)
			{
				if (this.SpawnedBlocks[i] == pos)
				{
					flag = true;
					this.SpawnedBlocks.RemoveAt(i);
				}
			}
			if (checkForRemove)
			{
				this.CheckReadyForRemove();
			}
			return flag;
		}

		public bool RemoveAllSpawnedBlock(bool checkForRemove)
		{
			bool flag = false;
			if (this.SpawnedBlocks.Count > 0)
			{
				flag = true;
				this.SpawnedBlocks.Clear();
			}
			if (checkForRemove)
			{
				this.CheckReadyForRemove();
			}
			return flag;
		}

		public void CheckReadyForRemove()
		{
			TwitchAction.RespawnCountTypes respawnCountType = this.Action.RespawnCountType;
			if (respawnCountType == TwitchAction.RespawnCountTypes.SpawnsOnly)
			{
				this.ReadyForRemove = this.SpawnedEntities.Count == 0;
				return;
			}
			if (respawnCountType == TwitchAction.RespawnCountTypes.BlocksOnly)
			{
				this.ReadyForRemove = this.SpawnedBlocks.Count == 0;
				return;
			}
			this.ReadyForRemove = this.SpawnedEntities.Count == 0 && this.SpawnedBlocks.Count == 0;
		}

		public TwitchActionEntry RespawnAction()
		{
			TwitchActionEntry twitchActionEntry = this.Action.SetupActionEntry();
			twitchActionEntry.UserName = this.UserName;
			twitchActionEntry.Target = this.Target;
			twitchActionEntry.Action = this.Action;
			twitchActionEntry.IsRespawn = true;
			twitchActionEntry.IsBitAction = this.Action.PointType == TwitchAction.PointTypes.Bits;
			this.RespawnsLeft--;
			this.NeedsRespawn = false;
			if (this.RespawnsLeft <= 0)
			{
				this.ReadyForRemove = true;
			}
			return twitchActionEntry;
		}

		public bool CanRespawn(TwitchManager tm)
		{
			return this.NeedsRespawn && tm.CheckCanRespawnEvent(this.Target);
		}

		public string UserName;

		public EntityPlayer Target;

		public TwitchAction Action;

		public int RespawnsLeft;

		public bool NeedsRespawn;

		public bool ReadyForRemove;

		public List<int> SpawnedEntities = new List<int>();

		public List<Vector3i> SpawnedBlocks = new List<Vector3i>();
	}
}
