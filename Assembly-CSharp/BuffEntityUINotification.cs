using System;
using UnityEngine;

public class BuffEntityUINotification : EntityUINotification
{
	public BuffEntityUINotification(EntityAlive _owner, BuffValue _buff)
	{
		this.owner = _owner;
		this.buff = _buff;
	}

	public BuffValue Buff
	{
		get
		{
			return this.buff;
		}
	}

	public string Icon
	{
		get
		{
			return this.buff.BuffClass.Icon;
		}
	}

	public Color GetColor()
	{
		return this.Buff.BuffClass.IconColor;
	}

	public float CurrentValue
	{
		get
		{
			if (this.buff != null && this.buff.BuffClass != null && this.buff.BuffClass.DisplayValueCVar != null)
			{
				return this.owner.Buffs.GetCustomVar(this.buff.BuffClass.DisplayValueCVar);
			}
			return 0f;
		}
	}

	public string Units
	{
		get
		{
			if (this.buff == null || this.buff.BuffClass == null || this.buff.BuffClass.DisplayValueCVar == null)
			{
				return "";
			}
			if (this.buff.BuffClass.DisplayValueCVar.StartsWith("$") || this.buff.BuffClass.DisplayValueCVar.StartsWith(".") || this.buff.BuffClass.DisplayValueCVar.StartsWith("_"))
			{
				return "cvar";
			}
			return this.buff.BuffClass.DisplayValueCVar;
		}
	}

	public EnumEntityUINotificationDisplayMode DisplayMode
	{
		get
		{
			EnumEntityUINotificationDisplayMode enumEntityUINotificationDisplayMode = ((this.buff != null && this.buff.BuffClass != null) ? this.buff.BuffClass.DisplayType : EnumEntityUINotificationDisplayMode.IconOnly);
			if (this.buff.BuffClass.DisplayValueCVar != null && this.buff.BuffClass.DisplayValueCVar != "")
			{
				enumEntityUINotificationDisplayMode = EnumEntityUINotificationDisplayMode.IconPlusCurrentValue;
			}
			return enumEntityUINotificationDisplayMode;
		}
	}

	public EnumEntityUINotificationSubject Subject
	{
		get
		{
			return EnumEntityUINotificationSubject.Buff;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public EntityAlive owner;

	[PublicizedFrom(EAccessModifier.Private)]
	public BuffValue buff;
}
