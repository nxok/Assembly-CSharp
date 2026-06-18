using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageRequestToSpawnEntity : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageRequestToSpawnEntity Setup(EntityCreationData _es)
	{
		this.ecd = _es;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.ecd = new EntityCreationData();
		this.ecd.read(_reader, true);
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		this.ecd.write(_writer, true);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		_world.GetGameManager().RequestToSpawnEntityServer(this.ecd);
	}

	public override int GetLength()
	{
		return 32;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityCreationData ecd;
}
