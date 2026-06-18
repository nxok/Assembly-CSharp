using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityAddExpServer : NetPackageEntityAddExpClient
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageEntityAddExpServer Setup(int _entityId, int _experience)
	{
		base.Setup(_entityId, _experience, Progression.XPTypes.Other, null);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		EntityPlayer entityPlayer = (EntityPlayer)_world.GetEntity(this.entityId);
		if (entityPlayer == null)
		{
			return;
		}
		if (entityPlayer.isEntityRemote)
		{
			entityPlayer.Progression.AddLevelExp(this.xp, "_xpOther", Progression.XPTypes.Other, true, true, -1, this.usedItem);
		}
	}
}
