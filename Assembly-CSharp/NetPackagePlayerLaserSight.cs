using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class NetPackagePlayerLaserSight : NetPackage
{
	public NetPackagePlayerLaserSight Setup(int _entityId, bool _laserSightActive, Vector3 _laserSightPosition)
	{
		this.entityId = _entityId;
		this.laserSightActive = _laserSightActive;
		this.laserSightPosition = _laserSightPosition;
		return this;
	}

	public override void read(PooledBinaryReader _reader)
	{
		this.entityId = _reader.ReadInt32();
		this.laserSightActive = _reader.ReadBoolean();
		if (this.laserSightActive)
		{
			this.laserSightPosition = StreamUtils.ReadVector3(_reader);
		}
	}

	public override void write(PooledBinaryWriter _writer)
	{
		base.write(_writer);
		_writer.Write(this.entityId);
		_writer.Write(this.laserSightActive);
		if (this.laserSightActive)
		{
			StreamUtils.Write(_writer, this.laserSightPosition);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePlayerLaserSight>().Setup(this.entityId, this.laserSightActive, this.laserSightPosition), false, -1, base.Sender.entityId, -1, null, 192, false);
		}
		EntityAlive entityAlive = (EntityAlive)_world.GetEntity(this.entityId);
		if (entityAlive != null && entityAlive.inventory.holdingItem != null)
		{
			ItemActionRanged.ItemActionDataRanged itemActionDataRanged = entityAlive.inventory.holdingItemData.actionData[0] as ItemActionRanged.ItemActionDataRanged;
			if (itemActionDataRanged != null && itemActionDataRanged.Laser != null)
			{
				itemActionDataRanged.Laser.gameObject.SetActive(this.laserSightActive);
				itemActionDataRanged.Laser.position = this.laserSightPosition - Origin.position;
			}
		}
	}

	public override int GetLength()
	{
		return 8;
	}

	public int entityId;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool laserSightActive;

	public Vector3 laserSightPosition;
}
