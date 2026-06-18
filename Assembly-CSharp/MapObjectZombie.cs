using System;
using UnityEngine;

public class MapObjectZombie : MapObject
{
	public MapObjectZombie(Entity _entity)
		: base(EnumMapObjectType.Entity, Vector3.zero, (long)_entity.entityId, _entity, false)
	{
	}

	public MapObjectZombie(MapObjectZombie _other)
		: base(EnumMapObjectType.Entity, _other.position, (long)_other.entity.entityId, _other.entity, false)
	{
	}

	public override void RefreshData()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity != null)
		{
			EntityAlive entityAlive = (EntityAlive)this.entity;
			this.entity.world.GetPrimaryPlayer();
		}
		this.trackingType = MapObjectZombie.TrackingTypes.None;
	}

	public override bool IsOnCompass()
	{
		return this.trackingType > MapObjectZombie.TrackingTypes.None;
	}

	public override string GetMapIcon()
	{
		if (this.trackingType != MapObjectZombie.TrackingTypes.Tracking)
		{
			return this.entity.GetMapIcon();
		}
		return this.entity.GetTrackerIcon();
	}

	public override string GetCompassIcon()
	{
		if (this.trackingType != MapObjectZombie.TrackingTypes.Tracking)
		{
			return this.entity.GetCompassIcon();
		}
		return this.entity.GetTrackerIcon();
	}

	public override bool UseUpDownCompassIcons()
	{
		return this.trackingType == MapObjectZombie.TrackingTypes.Quest;
	}

	public override bool IsCompassIconClamped()
	{
		return this.trackingType == MapObjectZombie.TrackingTypes.Quest;
	}

	public override bool NearbyCompassBlink()
	{
		return true;
	}

	public override bool IsMapIconEnabled()
	{
		return this.trackingType == MapObjectZombie.TrackingTypes.Tracking;
	}

	public override float GetMaxCompassIconScale()
	{
		return 1f;
	}

	public override float GetMinCompassIconScale()
	{
		return 0.6f;
	}

	public override Color GetMapIconColor()
	{
		if (this.trackingType != MapObjectZombie.TrackingTypes.Quest)
		{
			return this.entity.GetMapIconColor();
		}
		return Color.red;
	}

	public override float GetMaxCompassDistance()
	{
		return (float)((this.trackingType == MapObjectZombie.TrackingTypes.Quest) ? 32 : 100);
	}

	public override bool IsShowName()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public MapObjectZombie.TrackingTypes trackingType;

	[PublicizedFrom(EAccessModifier.Private)]
	public enum TrackingTypes
	{
		None,
		Tracking,
		Quest
	}
}
