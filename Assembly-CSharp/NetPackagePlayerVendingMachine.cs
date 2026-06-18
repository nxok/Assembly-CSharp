using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerVendingMachine : NetPackage
{
	public NetPackagePlayerVendingMachine Setup(PlatformUserIdentifierAbs _userId, Vector3i _position, bool _removing)
	{
		this.userId = _userId;
		this.position = _position;
		this.removing = _removing;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.userId = PlatformUserIdentifierAbs.FromStream(_reader, false, false);
		this.position = new Vector3i(_reader.ReadInt32(), _reader.ReadInt32(), _reader.ReadInt32());
		this.removing = _reader.ReadBoolean();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		this.userId.ToStream(_writer, false);
		_writer.Write(this.position.x);
		_writer.Write(this.position.y);
		_writer.Write(this.position.z);
		_writer.Write(this.removing);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		PersistentPlayerData persistentPlayerData2;
		if (this.removing)
		{
			PersistentPlayerData persistentPlayerData;
			if (_callbacks.persistentPlayers.Players.TryGetValue(this.userId, out persistentPlayerData))
			{
				persistentPlayerData.TryRemoveVendingMachinePosition(this.position);
				return;
			}
		}
		else if (_callbacks.persistentPlayers.Players.TryGetValue(this.userId, out persistentPlayerData2))
		{
			persistentPlayerData2.AddVendingMachinePosition(this.position);
		}
	}

	public override int GetLength()
	{
		return 0;
	}

	public PlatformUserIdentifierAbs userId;

	public Vector3i position;

	public bool removing;
}
