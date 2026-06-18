using System;
using UnityEngine;

public class MapObjectSleepingBag : MapObject
{
	public MapObjectSleepingBag(Vector3 _position, Entity _entity)
		: base(EnumMapObjectType.SleepingBag, _position, (long)_entity.entityId, _entity, false)
	{
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_map_bed";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_map_bed";
	}

	public override bool IsOnCompass()
	{
		return this.IsMapIconEnabled() && this.entity is EntityPlayerLocal;
	}

	public override int GetLayerForMapIcon()
	{
		return 0;
	}

	public override bool IsMapIconEnabled()
	{
		EntityPlayer entityPlayer = this.entity as EntityPlayer;
		return entityPlayer == null || entityPlayer is EntityPlayerLocal || entityPlayer.IsFriendOfLocalPlayer;
	}

	public override Color GetMapIconColor()
	{
		EntityPlayer entityPlayer = this.entity as EntityPlayer;
		if (entityPlayer != null && !(entityPlayer is EntityPlayerLocal))
		{
			bool isFriendOfLocalPlayer = entityPlayer.IsFriendOfLocalPlayer;
			return Color.green * 0.75f;
		}
		return Color.white;
	}
}
