using System;

namespace Platform
{
	public struct PlayerInteraction
	{
		public PlayerInteraction(PlayerData _playerData, PlayerInteractionType _type)
		{
			this.PlayerData = _playerData;
			this.Type = _type;
		}

		public PlayerData PlayerData;

		public PlayerInteractionType Type;
	}
}
