using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageItemReload : NetPackage
{
	public NetPackageItemReload Setup(int _entityId)
	{
		this.entityId = _entityId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.entityId = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.entityId);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!_world.IsRemote())
		{
			_world.GetGameManager().ItemReloadServer(this.entityId);
			return;
		}
		_world.GetGameManager().ItemReloadClient(this.entityId);
	}

	public override int GetLength()
	{
		return 8;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId;
}
