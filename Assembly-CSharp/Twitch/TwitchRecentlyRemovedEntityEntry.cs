using System;

namespace Twitch
{
	public class TwitchRecentlyRemovedEntityEntry
	{
		public TwitchRecentlyRemovedEntityEntry(TwitchSpawnedEntityEntry entry)
		{
			this.SpawnedEntity = entry.SpawnedEntity;
			this.SpawnedEntityID = entry.SpawnedEntityID;
			this.Action = entry.Action;
			this.Event = entry.Event;
			this.Vote = entry.Vote;
			this.TimeRemaining = 60f;
		}

		public Entity SpawnedEntity;

		public int SpawnedEntityID = -1;

		public TwitchActionEntry Action;

		public TwitchEventActionEntry Event;

		public TwitchVoteEntry Vote;

		public float TimeRemaining;
	}
}
