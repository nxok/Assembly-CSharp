using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageEntityTeleport : NetPackageEntityPosAndRot
{
	public new NetPackageEntityTeleport Setup(Entity _entity)
	{
		base.Setup(_entity);
		return this;
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (!base.ValidEntityIdForSender(this.entityId, true))
		{
			return;
		}
		Entity entity = _world.GetEntity(this.entityId);
		if (entity != null)
		{
			entity.serverPos = NetEntityDistributionEntry.EncodePos(this.pos);
			entity.SetPosAndRotFromNetwork(this.pos, this.rot, 0);
			entity.SetPosition(this.pos, true);
			entity.SetRotation(this.rot);
			entity.SetLastTickPos(this.pos);
			entity.onGround = this.onGround;
			return;
		}
		Log.Out("Discarding " + base.GetType().Name + " for entity Id=" + this.entityId.ToString());
	}

	public override int GetLength()
	{
		return 20;
	}
}
