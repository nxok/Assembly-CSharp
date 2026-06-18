using System;
using System.Text;
using LiteNetLib;

public static class NetworkCommonLiteNetLib
{
	public static bool InitConfig(NetManager _manager)
	{
		_manager.UnsyncedEvents = true;
		_manager.UnsyncedDeliveryEvent = true;
		_manager.UnsyncedReceiveEvent = true;
		_manager.AutoRecycle = true;
		_manager.DisconnectOnUnreachable = true;
		if (GameManager.IsDedicatedServer)
		{
			_manager.UseNativeSockets = true;
		}
		return true;
	}

	public static byte[] CreateRejectMessage(string _customText)
	{
		int byteCount = Encoding.UTF8.GetByteCount(_customText);
		byte[] array = new byte[2 + byteCount];
		array[0] = byte.MaxValue;
		array[1] = (byte)Encoding.UTF8.GetBytes(_customText, 0, _customText.Length, array, 2);
		return array;
	}

	public const int PORT_OFFSET = 2;

	public const int MTU_OVERRIDE = 1024;

	public const byte ChallengePackageChannelId = 202;

	public enum EAdditionalDisconnectCause : byte
	{
		InvalidPassword,
		RateLimit,
		PendingConnection,
		ServerShutdown,
		ClientSideDisconnect,
		Other = 255
	}
}
