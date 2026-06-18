using System;
using UnityEngine;

public class MapObjectSleeperVolume : MapObject
{
	public MapObjectSleeperVolume(Vector3 _position)
		: base(EnumMapObjectType.SleeperVolume, _position, (long)(++MapObjectSleeperVolume.newID), null, false)
	{
	}

	public override string GetMapIcon()
	{
		return "ui_game_symbol_enemy_dot";
	}

	public override string GetCompassIcon()
	{
		return "ui_game_symbol_enemy_dot";
	}

	public override string GetCompassDownIcon()
	{
		return "ui_game_symbol_enemy_dot_down";
	}

	public override string GetCompassUpIcon()
	{
		return "ui_game_symbol_enemy_dot_up";
	}

	public override bool UseUpDownCompassIcons()
	{
		return true;
	}

	public override bool IsOnCompass()
	{
		return this.IsShowing;
	}

	public override bool IsCompassIconClamped()
	{
		return true;
	}

	public override bool IsMapIconEnabled()
	{
		return false;
	}

	public override float GetMaxCompassIconScale()
	{
		return 1f;
	}

	public override float GetMinCompassIconScale()
	{
		return 0.6f;
	}

	public override float GetMaxCompassDistance()
	{
		return 32f;
	}

	public override Color GetMapIconColor()
	{
		return new Color32(byte.MaxValue, 180, 0, byte.MaxValue);
	}

	public bool IsShowing = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public static int newID;
}
