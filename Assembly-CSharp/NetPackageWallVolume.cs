using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWallVolume : NetPackage
{
	public NetPackageWallVolume Setup(int _id, WallVolume _wallVolume)
	{
		this.id = _id;
		this.wallVolume = _wallVolume;
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
		this.id = _reader.ReadInt32();
		this.wallVolume = WallVolume.Read(_reader);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.id);
		this.wallVolume.Write(_writer);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !_callbacks.worldInitInfoReceived)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			_world.AddWallVolumeAt(this.id, this.wallVolume);
		}
	}

	public override int GetLength()
	{
		return 29;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	[PublicizedFrom(EAccessModifier.Private)]
	public WallVolume wallVolume;
}
