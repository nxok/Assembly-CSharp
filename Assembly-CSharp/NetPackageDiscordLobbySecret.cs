using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageDiscordLobbySecret : NetPackage
{
	public NetPackageDiscordLobbySecret Setup(DiscordManager.ELobbyType _lobbyType, string _lobbySecret)
	{
		this.lobbyType = _lobbyType;
		this.lobbySecret = _lobbySecret;
		return this;
	}

	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.lobbyType = (DiscordManager.ELobbyType)_reader.ReadByte();
		this.lobbySecret = StreamUtils.ReadString(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((byte)this.lobbyType);
		StreamUtils.Write(_writer, this.lobbySecret);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		DiscordManager.Instance.ReceivedLobbySecret(this.lobbyType, this.lobbySecret);
	}

	public override int GetLength()
	{
		int num = 3;
		string text = this.lobbySecret;
		return num + ((text != null) ? text.Length : 0);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public DiscordManager.ELobbyType lobbyType;

	[PublicizedFrom(EAccessModifier.Private)]
	public string lobbySecret;
}
