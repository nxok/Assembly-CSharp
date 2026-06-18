using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityPrimeDetonator : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageEntityPrimeDetonator Setup(EntityZombieCop entity)
	{
		this.id = entity.entityId;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.id = _br.ReadInt32();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.id);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityZombieCop entityZombieCop = _world.GetEntity(this.id) as EntityZombieCop;
		if (entityZombieCop == null)
		{
			Log.Out("Discarding " + base.GetType().Name);
			return;
		}
		entityZombieCop.PrimeDetonator();
	}

	public override int GetLength()
	{
		return 20;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int id;
}
