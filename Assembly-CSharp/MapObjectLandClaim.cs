using System;
using UnityEngine;

public class MapObjectLandClaim : MapObject
{
	public MapObjectLandClaim(Vector3 _position, Entity _entity)
		: base(EnumMapObjectType.LandClaim, _position, (long)MapObjectLandClaim.MapObjectLandCLaimKeys++, _entity, false)
	{
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_brick";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_brick";
	}

	public override bool IsOnCompass()
	{
		return this.IsMapIconEnabled();
	}

	public override int GetLayerForMapIcon()
	{
		return 0;
	}

	public override bool IsMapIconEnabled()
	{
		return this.entity != null && this.entity is EntityPlayerLocal;
	}

	public override Color GetMapIconColor()
	{
		return Color.white;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public static int MapObjectLandCLaimKeys;
}
