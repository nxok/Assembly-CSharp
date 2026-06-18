using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntitySetPartActive : NetPackage
{
	public NetPackageEntitySetPartActive Setup(Entity entity, string partName, bool active)
	{
		this.id = entity.entityId;
		this.active = active;
		this.partName = partName;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.id = _br.ReadInt32();
		this.active = _br.ReadBoolean();
		this.partName = _br.ReadString();
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.id);
		_bw.Write(this.active);
		_bw.Write(this.partName ?? "");
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		Entity entity = _world.GetEntity(this.id);
		if (entity == null)
		{
			Log.Out("Discarding " + base.GetType().Name);
			return;
		}
		if (string.IsNullOrEmpty(this.partName))
		{
			Log.Out("Discarding " + base.GetType().Name + " unexpected no part name");
			return;
		}
		entity.SetTransformActive(this.partName, this.active);
	}

	public override int GetLength()
	{
		return 20;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int id;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool active;

	[PublicizedFrom(EAccessModifier.Private)]
	public string partName;
}
