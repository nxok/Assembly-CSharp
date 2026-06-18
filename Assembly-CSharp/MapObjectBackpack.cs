using System;
using UnityEngine;

public class MapObjectBackpack : MapObject
{
	public MapObjectBackpack(EntityPlayerLocal _epl, Vector3 _position, int _key)
		: base(EnumMapObjectType.Backpack, _position, (long)_key, null, false)
	{
		this.owningLocalPlayer = _epl;
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_backpack";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_backpack";
	}

	public override Color GetMapIconColor()
	{
		return Color.cyan;
	}

	public override bool IsOnCompass()
	{
		return true;
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

	public override bool IsCenterOnLeftBottomCorner()
	{
		return true;
	}

	public override Vector3 GetPosition()
	{
		if (this.myBackpack == null && Time.time - this.lastTimeBackpackChecked > 5f)
		{
			this.lastTimeBackpackChecked = Time.time;
			World world = GameManager.Instance.World;
			for (int i = world.Entities.list.Count - 1; i >= 0; i--)
			{
				if (world.Entities.list[i] is EntityBackpack && ((EntityBackpack)world.Entities.list[i]).RefPlayerId == this.owningLocalPlayer.entityId)
				{
					this.myBackpack = world.Entities.list[i];
					break;
				}
			}
		}
		if (this.myBackpack != null && this.myBackpack.IsMarkedForUnload())
		{
			this.myBackpack = null;
		}
		if (!(this.myBackpack != null))
		{
			return base.GetPosition();
		}
		return this.myBackpack.position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityPlayerLocal owningLocalPlayer;

	[PublicizedFrom(EAccessModifier.Private)]
	public Entity myBackpack;

	[PublicizedFrom(EAccessModifier.Private)]
	public float lastTimeBackpackChecked;
}
