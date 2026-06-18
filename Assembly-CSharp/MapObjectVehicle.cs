using System;
using UnityEngine;

public class MapObjectVehicle : MapObject
{
	public MapObjectVehicle(Entity _entity)
		: base(EnumMapObjectType.Entity, Vector3.zero, (long)_entity.entityId, _entity, false)
	{
	}

	public MapObjectVehicle(MapObjectVehicle _other)
		: base(EnumMapObjectType.Entity, _other.position, (long)_other.entity.entityId, _other.entity, false)
	{
	}

	public override bool IsOnCompass()
	{
		return !(this.entity as EntityVehicle).HasDriver;
	}

	public override string GetCompassIcon()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity != null)
		{
			return this.entity.GetMapIcon();
		}
		return null;
	}

	public override Vector3 GetRotation()
	{
		return Vector3.zero;
	}
}
