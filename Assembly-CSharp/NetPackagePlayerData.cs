using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerData : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackagePlayerData Setup(EntityPlayer _player)
	{
		this.playerDataFile = new PlayerDataFile();
		if (_player != null)
		{
			this.playerDataFile.FromPlayer(_player);
		}
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.playerDataFile = new PlayerDataFile();
		this.playerDataFile.ReadNetwork(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		this.playerDataFile.WriteNetwork(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!base.ValidEntityIdForSender(this.playerDataFile.id, false))
		{
			return;
		}
		_callbacks.SavePlayerData(base.Sender, this.playerDataFile);
	}

	public override int GetLength()
	{
		return 50;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public PlayerDataFile playerDataFile;
}
