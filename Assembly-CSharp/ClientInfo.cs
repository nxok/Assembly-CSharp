using System;
using System.Collections.Generic;
using Platform.Local;
using Platform.Steam;

public class ClientInfo : IEquatable<ClientInfo>
{
	public PlatformUserIdentifierAbs InternalId
	{
		get
		{
			return this.CrossplatformId ?? this.PlatformId;
		}
	}

	public string ip
	{
		get
		{
			return this.network.GetIP(this);
		}
	}

	public ClientInfo()
	{
		int num;
		do
		{
			num = ++ClientInfo.lastClientNumber;
			if (num > 1000000)
			{
				num = (ClientInfo.lastClientNumber = 1);
			}
		}
		while (SingletonMonoBehaviour<ConnectionManager>.Instance.Clients.ForClientNumber(num) != null);
		this.ClientNumber = num;
	}

	public override string ToString()
	{
		string text = null;
		UserIdentifierSteam userIdentifierSteam = this.PlatformId as UserIdentifierSteam;
		if (userIdentifierSteam != null)
		{
			UserIdentifierSteam ownerId = userIdentifierSteam.OwnerId;
			text = ((ownerId != null) ? ownerId.CombinedString : null);
		}
		string text2 = "EntityID={0}, PltfmId='{1}', CrossId='{2}', OwnerID='{3}', PlayerName='{4}', ClientNumber='{5}'";
		object[] array = new object[6];
		array[0] = this.entityId;
		int num = 1;
		PlatformUserIdentifierAbs platformId = this.PlatformId;
		array[num] = ((platformId != null) ? platformId.CombinedString : null) ?? "<unknown>";
		int num2 = 2;
		PlatformUserIdentifierAbs crossplatformId = this.CrossplatformId;
		array[num2] = ((crossplatformId != null) ? crossplatformId.CombinedString : null) ?? "<unknown/none>";
		array[3] = text ?? "<unknown/none>";
		array[4] = this.playerName;
		array[5] = this.ClientNumber;
		return string.Format(text2, array);
	}

	public void UpdatePing()
	{
		this.ping = this.network.GetPing(this);
	}

	public void SendPackage(NetPackage _package)
	{
		if (!_package.AllowedBeforeAuth && !this.loginDone)
		{
			Log.Warning(string.Format("Ignoring {0}, not logged in yet", _package));
			return;
		}
		this.netConnection[_package.Channel].AddToSendQueue(_package);
		if (_package.FlushQueue)
		{
			this.netConnection[_package.Channel].FlushSendQueue();
		}
	}

	public void SetAntiCheatEncryption(IEncryptionModule encryptionModule)
	{
		INetConnection[] array = this.netConnection;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetEncryptionModule(encryptionModule);
		}
	}

	public bool Equals(ClientInfo _other)
	{
		return this == _other;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int lastClientNumber;

	public INetworkServer network;

	public readonly int ClientNumber;

	public long litenetPeerConnectId = -1L;

	public PlatformUserIdentifierAbs PlatformId = new UserIdentifierLocal("<none>");

	public PlatformUserIdentifierAbs CrossplatformId;

	public ulong DiscordUserId;

	public bool requiresAntiCheat = true;

	public ClientInfo.EDeviceType device = ClientInfo.EDeviceType.Unknown;

	public bool loginDone;

	public bool acAuthDone;

	public INetConnection[] netConnection;

	public bool bAttachedToEntity;

	public int entityId = -1;

	public string playerName;

	public string compatibilityVersion;

	public readonly Dictionary<string, int> groupMemberships = new Dictionary<string, int>(StringComparer.Ordinal);

	public int groupMembershipsWaiting;

	public PlayerDataFile latestPlayerData;

	public int ping;

	public bool disconnecting;

	public enum EDeviceType
	{
		Linux,
		Mac,
		Windows,
		PlayStation,
		Xbox,
		Unknown
	}
}
