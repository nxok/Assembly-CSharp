using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerId : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackagePlayerId Setup(int _id, int _teamNumber, PlayerDataFile _playerDataFile, int _chunkViewDim)
	{
		this.id = _id;
		this.teamNumber = _teamNumber;
		this.playerDataFile = _playerDataFile;
		this.chunkViewDim = _chunkViewDim;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.id = _reader.ReadInt32();
		this.teamNumber = (int)_reader.ReadInt16();
		this.playerDataFile = new PlayerDataFile();
		this.playerDataFile.ReadNetwork(_reader);
		this.chunkViewDim = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.id);
		_writer.Write((short)this.teamNumber);
		this.playerDataFile.WriteNetwork(_writer);
		_writer.Write(this.chunkViewDim);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.PlayerId(this.id, this.teamNumber, this.playerDataFile, this.chunkViewDim);
	}

	public override int GetLength()
	{
		return 40;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	[PublicizedFrom(EAccessModifier.Private)]
	public int teamNumber;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerDataFile playerDataFile;

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkViewDim;
}
