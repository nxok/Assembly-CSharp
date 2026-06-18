using System;
using System.Text;
using Platform;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageLobbyRegisterClient : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageLobbyRegisterClient Setup(PlatformLobbyId lobbyId, bool overwriteExistingLobby)
	{
		this.lobbyId = lobbyId;
		this.overwriteExistingLobby = overwriteExistingLobby;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.lobbyId = PlatformLobbyId.Read(_br);
		this.overwriteExistingLobby = _br.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		this.lobbyId.Write(_bw);
		_bw.Write(this.overwriteExistingLobby);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (this.lobbyId.PlatformIdentifier == PlatformManager.NativePlatform.PlatformIdentifier)
		{
			return;
		}
		if (this.lobbyId.PlatformIdentifier != base.Sender.PlatformId.PlatformIdentifier)
		{
			Log.Warning(string.Format("Received {0} for lobby with platform {1} but client is from {2}. This is not permitted, lobby will not be registered", "NetPackageLobbyRegisterClient", this.lobbyId.PlatformIdentifier, base.Sender.PlatformId.PlatformIdentifier));
			return;
		}
		PlatformManager.ClientLobbyManager.RegisterLobbyClient(this.lobbyId, base.Sender, this.overwriteExistingLobby);
	}

	public override int GetLength()
	{
		Encoding utf = Encoding.UTF8;
		PlatformLobbyId platformLobbyId = this.lobbyId;
		if (platformLobbyId == null)
		{
			return 0;
		}
		return platformLobbyId.GetWriteLength(utf);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformLobbyId lobbyId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool overwriteExistingLobby;
}
