using System;
using UnityEngine;

public class MapObjectWaypoint : MapObject
{
	public MapObjectWaypoint(Waypoint _w)
		: base(EnumMapObjectType.MapMarker, _w.pos.ToVector3(), (long)MapObjectWaypoint.MapObjectWaypointKeys, null, false)
	{
		this.waypoint = _w;
		_w.MapObjectKey = (long)MapObjectWaypoint.MapObjectWaypointKeys++;
		this.name = _w.name.Text;
		this.iconName = _w.icon;
	}

	public override string GetMapIcon()
	{
		return this.iconName;
	}

	public override string GetCompassIcon()
	{
		return this.iconName;
	}

	public override bool IsOnCompass()
	{
		return true;
	}

	public override bool IsTracked()
	{
		return this.waypoint.bTracked;
	}

	public override float GetMaxCompassDistance()
	{
		return (float)(this.waypoint.bTracked ? 1000 : 1000);
	}

	public override float GetMinCompassDistance()
	{
		return (float)(this.waypoint.bTracked ? 250 : 0);
	}

	public override float GetMaxCompassIconScale()
	{
		return base.GetMaxCompassIconScale();
	}

	public override float GetMinCompassIconScale()
	{
		return base.GetMinCompassIconScale();
	}

	public override int GetLayerForMapIcon()
	{
		return 0;
	}

	public override bool IsMapIconEnabled()
	{
		return true;
	}

	public override void SetPosition(Vector3 _pos)
	{
		this.position = _pos;
	}

	public override string GetName()
	{
		return this.waypoint.name.Text;
	}

	public override bool IsShowName()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public string name;

	[PublicizedFrom(EAccessModifier.Private)]
	public string iconName;

	public Waypoint waypoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int MapObjectWaypointKeys;
}
