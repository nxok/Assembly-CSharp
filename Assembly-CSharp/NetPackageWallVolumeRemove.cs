using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageWallVolumeRemove : NetPackage
{
	public NetPackageWallVolumeRemove Setup(int _index)
	{
		this.index = _index;
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
		this.index = _reader.ReadInt32();
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.index);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			_world.RemoveWallVolumeAt(this.index);
		}
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int index;
}
