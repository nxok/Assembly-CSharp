using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageSleeperPassiveChange : NetPackageEntityTargeted
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public new NetPackageSleeperPassiveChange Setup(int targetId)
	{
		base.Setup(targetId);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || !_world.IsRemote())
		{
			return;
		}
		EntityAlive entityAlive = _world.GetEntity(this.entityId) as EntityAlive;
		if (entityAlive == null)
		{
			return;
		}
		entityAlive.IsSleeperPassive = false;
	}

	public override int GetLength()
	{
		return 8;
	}
}
