using System;
using UnityEngine;

public class MapObjectAnimal : MapObject
{
	public MapObjectAnimal(Entity _entity)
		: base(EnumMapObjectType.Entity, Vector3.zero, (long)_entity.entityId, _entity, false)
	{
	}

	public MapObjectAnimal(MapObjectAnimal _other)
		: base(EnumMapObjectType.Entity, _other.position, (long)_other.entity.entityId, _other.entity, false)
	{
	}

	public override void RefreshData()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity != null)
		{
			if (!((EntityAlive)this.entity).IsAlive())
			{
				this.isTracked = false;
				return;
			}
			EntityPlayerLocal primaryPlayer = this.entity.world.GetPrimaryPlayer();
			if (primaryPlayer != null && EffectManager.GetValue(PassiveEffects.Tracking, null, 0f, primaryPlayer, null, this.entity.EntityTags, true, true, true, true, true, 1, true, false) > 0f)
			{
				this.isTracked = true;
				return;
			}
		}
		this.isTracked = false;
	}

	public override bool IsOnCompass()
	{
		return this.isTracked;
	}

	public override string GetMapIcon()
	{
		if (!this.isTracked)
		{
			return this.entity.GetMapIcon();
		}
		return this.entity.GetTrackerIcon();
	}

	public override string GetCompassIcon()
	{
		if (!this.isTracked)
		{
			return this.entity.GetCompassIcon();
		}
		return this.entity.GetTrackerIcon();
	}

	public override bool NearbyCompassBlink()
	{
		return true;
	}

	public override bool IsMapIconEnabled()
	{
		return this.isTracked;
	}

	public override float GetMaxCompassIconScale()
	{
		return 1f;
	}

	public override float GetMinCompassIconScale()
	{
		return 0.6f;
	}

	public override bool UseUpDownCompassIcons()
	{
		return false;
	}

	public override float GetMaxCompassDistance()
	{
		return 500f;
	}

	public override bool IsShowName()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isTracked;
}
