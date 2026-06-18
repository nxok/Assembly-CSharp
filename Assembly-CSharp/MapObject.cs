using System;
using UnityEngine;

public class MapObject
{
	public MapObject(EnumMapObjectType _type, Vector3 _position, long _key, Entity _entity, bool _bSelectable)
	{
		this.type = _type;
		this.position = _position;
		this.key = _key;
		this.bSelectable = _bSelectable;
		this.entity = _entity;
	}

	public MapObject(MapObject _other)
	{
		this.type = _other.type;
		this.position = _other.position;
		this.key = _other.key;
		this.bSelectable = _other.bSelectable;
		this.entity = _other.entity;
	}

	public virtual Vector3 GetPosition()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity != null)
		{
			return this.entity.GetPosition();
		}
		return this.position;
	}

	public virtual void SetPosition(Vector3 _pos)
	{
		if (this.type == EnumMapObjectType.Entity)
		{
			throw new Exception("Setting of position not allowed!");
		}
		this.position = _pos;
	}

	public virtual Vector3 GetRotation()
	{
		if (this.type != EnumMapObjectType.Entity || !(this.entity != null))
		{
			return Vector3.zero;
		}
		if (this.entity.AttachedToEntity != null)
		{
			return this.entity.AttachedToEntity.rotation;
		}
		return this.entity.rotation;
	}

	public virtual bool IsTracked()
	{
		return true;
	}

	public virtual bool IsMapIconEnabled()
	{
		return this.type != EnumMapObjectType.Entity || !(this.entity != null) || this.entity.IsDrawMapIcon();
	}

	public virtual float GetMaxCompassDistance()
	{
		return 1024f;
	}

	public virtual float GetMinCompassDistance()
	{
		return 0f;
	}

	public virtual float GetMaxCompassIconScale()
	{
		return 1.25f;
	}

	public virtual float GetMinCompassIconScale()
	{
		return 0.5f;
	}

	public virtual bool IsCompassIconClamped()
	{
		return false;
	}

	public virtual bool NearbyCompassBlink()
	{
		return false;
	}

	public virtual Vector3 GetMapIconScale()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity != null)
		{
			return this.entity.GetMapIconScale();
		}
		return Vector3.one;
	}

	public virtual string GetMapIcon()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity != null)
		{
			return this.entity.GetMapIcon();
		}
		return "";
	}

	public virtual string GetCompassIcon()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity != null)
		{
			return this.entity.GetCompassIcon();
		}
		return null;
	}

	public virtual string GetCompassUpIcon()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity != null)
		{
			return this.entity.GetCompassUpIcon();
		}
		return "";
	}

	public virtual string GetCompassDownIcon()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity != null)
		{
			return this.entity.GetCompassDownIcon();
		}
		return "";
	}

	public virtual bool UseUpDownCompassIcons()
	{
		return this.type == EnumMapObjectType.Entity && this.entity != null && this.entity.GetCompassDownIcon() != null;
	}

	public virtual bool IsMapIconBlinking()
	{
		return this.type == EnumMapObjectType.Entity && this.entity != null && this.entity.IsMapIconBlinking();
	}

	public virtual Color GetMapIconColor()
	{
		if (this.type != EnumMapObjectType.Entity || !(this.entity != null))
		{
			return Color.white;
		}
		EntityPlayerLocal primaryPlayer = this.entity.world.GetPrimaryPlayer();
		if (primaryPlayer != null && primaryPlayer.Party != null && primaryPlayer.Party.MemberList.Contains(this.entity as EntityPlayer))
		{
			int num = primaryPlayer.Party.MemberList.IndexOf(this.entity as EntityPlayer);
			return Constants.TrackedFriendColors[num % Constants.TrackedFriendColors.Length];
		}
		return this.entity.GetMapIconColor();
	}

	public virtual bool CanMapIconBeSelected()
	{
		return this.type == EnumMapObjectType.Entity && this.entity != null && this.entity.CanMapIconBeSelected();
	}

	public virtual bool IsOnCompass()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity != null)
		{
			EntityPlayerLocal primaryPlayer = this.entity.world.GetPrimaryPlayer();
			if (primaryPlayer != null && primaryPlayer.Party != null && this.entity != primaryPlayer)
			{
				return primaryPlayer.Party.MemberList.Contains(this.entity as EntityPlayer);
			}
		}
		return false;
	}

	public virtual int GetLayerForMapIcon()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity != null)
		{
			return this.entity.GetLayerForMapIcon();
		}
		return 2;
	}

	public virtual string GetName()
	{
		if (this.type == EnumMapObjectType.Entity && this.entity is EntityAlive)
		{
			bool flag = !SingletonMonoBehaviour<ConnectionManager>.Instance.IsSinglePlayer;
			EntityPlayerLocal primaryPlayer = this.entity.world.GetPrimaryPlayer();
			if (!(this.entity is EntityPlayerLocal) && !(this.entity is EntityVehicle))
			{
				return ((EntityAlive)this.entity).EntityName;
			}
			if (primaryPlayer == this.entity && flag)
			{
				return Localization.Get("xuiMapSelfLabel", false, null);
			}
		}
		return null;
	}

	public virtual bool IsShowName()
	{
		return true;
	}

	public virtual bool IsCenterOnLeftBottomCorner()
	{
		return false;
	}

	public virtual float GetCompassIconScale(float _distance)
	{
		float num = 1f - _distance / this.GetMaxCompassDistance();
		return Mathf.Lerp(this.GetMinCompassIconScale(), this.GetMaxCompassIconScale(), num);
	}

	public virtual void RefreshData()
	{
	}

	public EnumMapObjectType type;

	public long key;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Vector3 position;

	public bool bSelectable;

	[PublicizedFrom(EAccessModifier.Protected)]
	public Entity entity;
}
