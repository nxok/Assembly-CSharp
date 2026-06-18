using System;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLobbyJoin : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageLobbyJoin Setup(PlatformLobbyId lobbyId)
	{
		this.serverLobbyId = lobbyId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.serverLobbyId = PlatformLobbyId.Read(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		this.serverLobbyId.Write(_bw);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		ILobbyHost lobbyHost = PlatformManager.NativePlatform.LobbyHost;
		if (lobbyHost == null)
		{
			Log.Warning(string.Format("Unexpected {0}, no lobby host for {1}", "NetPackageLobbyJoin", PlatformManager.NativePlatform.PlatformIdentifier));
			return;
		}
		if (PlatformManager.NativePlatform.PlatformIdentifier != this.serverLobbyId.PlatformIdentifier)
		{
			Log.Warning(string.Format("Received {0} for different platform: {1}", "NetPackageLobbyJoin", this.serverLobbyId.PlatformIdentifier));
			return;
		}
		string lobbyId = lobbyHost.LobbyId;
		if (lobbyId != null && lobbyId.Equals(this.serverLobbyId.LobbyId))
		{
			Log.Out("Received NetPackageLobbyJoin with " + this.serverLobbyId.LobbyId + " but we're already in the lobby");
			return;
		}
		lobbyHost.JoinLobby(this.serverLobbyId.LobbyId, delegate(LobbyHostJoinResult joinResult)
		{
			if (!joinResult.success)
			{
				Log.Warning("Failed to join server requested lobby, this client may be out of sync with the native lobby");
			}
		});
	}

	public override int GetLength()
	{
		Encoding utf = Encoding.UTF8;
		PlatformLobbyId platformLobbyId = this.serverLobbyId;
		if (platformLobbyId == null)
		{
			return 0;
		}
		return platformLobbyId.GetWriteLength(utf);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformLobbyId serverLobbyId;
}
