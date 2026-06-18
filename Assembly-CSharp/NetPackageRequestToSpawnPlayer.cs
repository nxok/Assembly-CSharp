using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageRequestToSpawnPlayer : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageRequestToSpawnPlayer Setup(int _chunkViewDim, PlayerProfile _playerProfile, int _nearEntityId)
	{
		this.chunkViewDim = _chunkViewDim;
		this.playerProfile = _playerProfile;
		this.nearEntityId = _nearEntityId;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.chunkViewDim = (int)_reader.ReadInt16();
		this.playerProfile = PlayerProfile.Read(_reader);
		this.nearEntityId = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write((short)this.chunkViewDim);
		this.playerProfile.Write(_writer);
		_writer.Write(this.nearEntityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		_callbacks.RequestToSpawnPlayer(base.Sender, this.chunkViewDim, this.playerProfile, this.nearEntityId);
	}

	public override int GetLength()
	{
		return 50;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int chunkViewDim;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlayerProfile playerProfile;

	[PublicizedFrom(EAccessModifier.Private)]
	public int nearEntityId;
}
