using System;

namespace Platform.EOS
{
	public static class NetworkCommonEos
	{
		public const int MaxUsedPacketSize = 1120;

		public enum ESteamNetChannels : byte
		{
			NetpackageChannel0,
			NetpackageChannel1,
			Authentication = 50,
			Ping = 60
		}

		public readonly struct SendInfo
		{
			public SendInfo(ClientInfo _clientInfo, ArrayListMP<byte> _data)
			{
				this.Recipient = _clientInfo;
				this.Data = _data;
			}

			public readonly ClientInfo Recipient;

			public readonly ArrayListMP<byte> Data;
		}
	}
}
