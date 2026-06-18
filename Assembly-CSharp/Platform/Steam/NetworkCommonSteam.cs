using System;
using Steamworks;

namespace Platform.Steam
{
	public static class NetworkCommonSteam
	{
		public enum ESteamNetChannels : byte
		{
			NetpackageChannel0,
			NetpackageChannel1,
			Authentication = 50,
			Ping = 60
		}

		public readonly struct SendInfo
		{
			public SendInfo(CSteamID _recipient, ArrayListMP<byte> _data)
			{
				this.Recipient = _recipient;
				this.Data = _data;
			}

			public readonly CSteamID Recipient;

			public readonly ArrayListMP<byte> Data;
		}
	}
}
