using System;
using System.IO;
using Audio;
using UnityEngine;

public class PowerTimerRelay : PowerTrigger
{
	public override PowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return PowerItem.PowerItemTypes.Timer;
		}
	}

	public byte StartTime
	{
		get
		{
			return this.startTime;
		}
		set
		{
			this.startTime = value;
			int num = (int)(this.startTime / 2);
			bool flag = this.startTime % 2 == 1;
			this.startTimeInTicks = GameUtils.DayTimeToWorldTime(1, num, flag ? 30 : 0);
		}
	}

	public byte EndTime
	{
		get
		{
			return this.endTime;
		}
		set
		{
			this.endTime = value;
			int num = (int)(this.endTime / 2);
			bool flag = this.endTime % 2 == 1;
			this.endTimeInTicks = GameUtils.DayTimeToWorldTime(1, num, flag ? 30 : 0);
		}
	}

	public PowerTimerRelay()
	{
		this.StartTime = 0;
		this.EndTime = 24;
	}

	public override bool IsTriggered
	{
		get
		{
			return this.isTriggered;
		}
		set
		{
			if (this.lastTriggered != value)
			{
				this.lastTriggered = this.isTriggered;
				this.isTriggered = value;
				if (!this.isTriggered && this.lastTriggered)
				{
					if (this.isPowered)
					{
						Manager.BroadcastPlay(this.Position.ToVector3(), "timer_start", 0f);
					}
					this.HandleDisconnect();
					return;
				}
				if (this.isPowered)
				{
					Manager.BroadcastPlay(this.Position.ToVector3(), "timer_stop", 0f);
				}
				this.isActive = true;
				base.SendHasLocalChangesToRoot();
			}
		}
	}

	public override bool PowerChildren()
	{
		return this.IsTriggered;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override void CheckForActiveChange()
	{
		if (GameManager.Instance.World != null)
		{
			ulong num = GameManager.Instance.World.worldTime % 24000UL;
			if (this.StartTime < this.EndTime)
			{
				this.IsTriggered = this.startTimeInTicks < num && num < this.endTimeInTicks;
				return;
			}
			if (this.EndTime < this.StartTime)
			{
				this.IsTriggered = num > this.startTimeInTicks || num < this.endTimeInTicks;
				return;
			}
			this.IsTriggered = false;
		}
	}

	public override void CachedUpdateCall()
	{
		if (Time.time > this.updateTime)
		{
			this.updateTime = Time.time + 1f;
			this.CheckForActiveChange();
		}
	}

	public override void HandlePowerReceived(ref ushort power)
	{
		ushort num = (ushort)Mathf.Min((int)this.RequiredPower, (int)power);
		num = (ushort)Mathf.Min((int)num, (int)this.RequiredPower);
		this.isPowered = num == this.RequiredPower;
		power -= num;
		if (power <= 0)
		{
			return;
		}
		this.CheckForActiveChange();
		if (this.PowerChildren())
		{
			for (int i = 0; i < this.Children.Count; i++)
			{
				this.Children[i].HandlePowerReceived(ref power);
				if (power <= 0)
				{
					return;
				}
			}
		}
	}

	public override void HandlePowerUpdate(bool parentIsOn)
	{
		if (this.TileEntity != null)
		{
			((TileEntityPoweredTrigger)this.TileEntity).Activate(this.isPowered && parentIsOn, this.isTriggered);
			this.TileEntity.SetModified();
		}
		for (int i = 0; i < this.Children.Count; i++)
		{
			this.Children[i].HandlePowerUpdate(this.isPowered && parentIsOn);
		}
		this.hasChangesLocal = true;
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		this.StartTime = _br.ReadByte();
		this.EndTime = _br.ReadByte();
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.StartTime);
		_bw.Write(this.EndTime);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte startTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public byte endTime = 12;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong startTimeInTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong endTimeInTicks;

	[PublicizedFrom(EAccessModifier.Private)]
	public float updateTime;
}
