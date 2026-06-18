using System;
using System.IO;
using Audio;
using UnityEngine;

public class PowerTrigger : PowerConsumer
{
	public override PowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return PowerItem.PowerItemTypes.Trigger;
		}
	}

	public PowerTrigger.TriggerPowerDelayTypes TriggerPowerDelay
	{
		get
		{
			return this.triggerPowerDelay;
		}
		set
		{
			this.triggerPowerDelay = value;
		}
	}

	public PowerTrigger.TriggerPowerDurationTypes TriggerPowerDuration
	{
		get
		{
			return this.triggerPowerDuration;
		}
		set
		{
			this.triggerPowerDuration = value;
		}
	}

	public virtual bool IsActive
	{
		get
		{
			if (this.TriggerType == PowerTrigger.TriggerTypes.Switch)
			{
				return this.isTriggered;
			}
			return this.isActive || this.parentTriggered;
		}
	}

	public virtual bool IsTriggered
	{
		get
		{
			return this.isTriggered;
		}
		set
		{
			if (this.TriggerType == PowerTrigger.TriggerTypes.Switch)
			{
				this.lastTriggered = this.isTriggered;
				this.isTriggered = value;
				if (this.isTriggered && !this.lastTriggered)
				{
					this.isActive = true;
				}
				base.SendHasLocalChangesToRoot();
				if (!this.isTriggered && this.lastTriggered)
				{
					this.HandleDisconnectChildren();
					this.isActive = false;
					return;
				}
			}
			else
			{
				this.isTriggered = value;
				if (this.isTriggered && !this.lastTriggered)
				{
					PowerTrigger.TriggerTypes triggerType = this.TriggerType;
					if (triggerType != PowerTrigger.TriggerTypes.Motion)
					{
						if (triggerType == PowerTrigger.TriggerTypes.TripWire)
						{
							Manager.BroadcastPlay(this.Position.ToVector3(), "trip_wire_trigger", 0f);
						}
					}
					else
					{
						Manager.BroadcastPlay(this.Position.ToVector3(), "motion_sensor_trigger", 0f);
					}
					base.SendHasLocalChangesToRoot();
				}
				this.lastTriggered = this.isTriggered;
				if (this.IsPowered && !this.isActive && this.delayStartTime == -1f)
				{
					this.lastPowerTime = Time.time;
					this.delayStartTime = -1f;
					switch (this.TriggerPowerDelay)
					{
					case PowerTrigger.TriggerPowerDelayTypes.OneSecond:
						this.delayStartTime = 1f;
						break;
					case PowerTrigger.TriggerPowerDelayTypes.TwoSecond:
						this.delayStartTime = 2f;
						break;
					case PowerTrigger.TriggerPowerDelayTypes.ThreeSecond:
						this.delayStartTime = 3f;
						break;
					case PowerTrigger.TriggerPowerDelayTypes.FourSecond:
						this.delayStartTime = 4f;
						break;
					case PowerTrigger.TriggerPowerDelayTypes.FiveSecond:
						this.delayStartTime = 5f;
						break;
					}
					if (this.delayStartTime == -1f)
					{
						this.isActive = true;
						this.SetupDurationTime();
					}
				}
				this.parentTriggered = false;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void SetupDurationTime()
	{
		this.lastPowerTime = Time.time;
		switch (this.TriggerPowerDuration)
		{
		case PowerTrigger.TriggerPowerDurationTypes.Always:
			this.powerTime = -1f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.Triggered:
			this.powerTime = 0f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.OneSecond:
			this.powerTime = 1f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.TwoSecond:
			this.powerTime = 2f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.ThreeSecond:
			this.powerTime = 3f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.FourSecond:
			this.powerTime = 4f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.FiveSecond:
			this.powerTime = 5f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.SixSecond:
			this.powerTime = 6f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.SevenSecond:
			this.powerTime = 7f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.EightSecond:
			this.powerTime = 8f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.NineSecond:
			this.powerTime = 9f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.TenSecond:
			this.powerTime = 10f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.FifteenSecond:
			this.powerTime = 15f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.ThirtySecond:
			this.powerTime = 30f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.FourtyFiveSecond:
			this.powerTime = 45f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.OneMinute:
			this.powerTime = 60f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.FiveMinute:
			this.powerTime = 300f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.TenMinute:
			this.powerTime = 600f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.ThirtyMinute:
			this.powerTime = 1800f;
			return;
		case PowerTrigger.TriggerPowerDurationTypes.SixtyMinute:
			this.powerTime = 3600f;
			return;
		default:
			return;
		}
	}

	public override bool PowerChildren()
	{
		return true;
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
				if (this.Children[i] is PowerTrigger)
				{
					PowerTrigger powerTrigger = this.Children[i] as PowerTrigger;
					this.HandleParentTriggering(powerTrigger);
					if ((this.TriggerType == PowerTrigger.TriggerTypes.Motion || this.TriggerType == PowerTrigger.TriggerTypes.PressurePlate || this.TriggerType == PowerTrigger.TriggerTypes.TripWire) && (powerTrigger.TriggerType == PowerTrigger.TriggerTypes.Motion || powerTrigger.TriggerType == PowerTrigger.TriggerTypes.PressurePlate || powerTrigger.TriggerType == PowerTrigger.TriggerTypes.TripWire))
					{
						powerTrigger.HandlePowerReceived(ref power);
					}
					else if (this.IsActive)
					{
						powerTrigger.HandlePowerReceived(ref power);
					}
				}
				else if (this.IsActive)
				{
					this.Children[i].HandlePowerReceived(ref power);
				}
				if (power <= 0)
				{
					return;
				}
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void CheckForActiveChange()
	{
		if (this.powerTime == 0f && this.lastTriggered && !this.isTriggered)
		{
			this.isActive = false;
			this.HandleDisconnectChildren();
			base.SendHasLocalChangesToRoot();
			this.powerTime = -1f;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleSingleUseDisable()
	{
		PowerTrigger.TriggerTypes triggerType = this.TriggerType;
		if (triggerType == PowerTrigger.TriggerTypes.PressurePlate || triggerType - PowerTrigger.TriggerTypes.Motion <= 1)
		{
			this.lastTriggered = this.isTriggered;
			this.isTriggered = false;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleSoundDisable()
	{
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
			if (this.Children[i] is PowerTrigger)
			{
				PowerTrigger powerTrigger = this.Children[i] as PowerTrigger;
				this.HandleParentTriggering(powerTrigger);
				this.Children[i].HandlePowerUpdate(this.isPowered && parentIsOn);
			}
			else if (this.IsActive)
			{
				this.Children[i].HandlePowerUpdate(this.isPowered && parentIsOn);
			}
		}
		this.hasChangesLocal = true;
		this.HandleSingleUseDisable();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public void HandleParentTriggering(PowerTrigger child)
	{
		if (!this.IsActive)
		{
			child.SetTriggeredByParent(false);
			return;
		}
		if ((this.TriggerType == PowerTrigger.TriggerTypes.Motion || this.TriggerType == PowerTrigger.TriggerTypes.PressurePlate || this.TriggerType == PowerTrigger.TriggerTypes.TripWire) && (child.TriggerType == PowerTrigger.TriggerTypes.Motion || child.TriggerType == PowerTrigger.TriggerTypes.PressurePlate || child.TriggerType == PowerTrigger.TriggerTypes.TripWire))
		{
			child.SetTriggeredByParent(true);
			return;
		}
		child.SetTriggeredByParent(false);
	}

	public void SetTriggeredByParent(bool triggered)
	{
		this.parentTriggered = triggered;
	}

	public virtual void CachedUpdateCall()
	{
		PowerTrigger.TriggerTypes triggerType = this.TriggerType;
		if (triggerType == PowerTrigger.TriggerTypes.PressurePlate || triggerType - PowerTrigger.TriggerTypes.Motion <= 1)
		{
			if (!this.hasChangesLocal)
			{
				if (this.isTriggered != this.lastTriggered)
				{
					base.SendHasLocalChangesToRoot();
				}
				this.CheckForActiveChange();
				this.HandleSingleUseDisable();
			}
			if (this.delayStartTime >= 0f)
			{
				if (Time.time - this.lastPowerTime >= this.delayStartTime)
				{
					base.SendHasLocalChangesToRoot();
					this.delayStartTime = -1f;
					this.isActive = true;
					this.SetupDurationTime();
				}
			}
			else if (this.powerTime > 0f && !this.parentTriggered && Time.time - this.lastPowerTime >= this.powerTime)
			{
				this.isActive = false;
				this.HandleDisconnectChildren();
				base.SendHasLocalChangesToRoot();
				this.powerTime = -1f;
			}
			this.hasChangesLocal = false;
			this.HandleSoundDisable();
		}
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		this.TriggerType = (PowerTrigger.TriggerTypes)_br.ReadByte();
		if (this.TriggerType == PowerTrigger.TriggerTypes.Switch)
		{
			this.isTriggered = _br.ReadBoolean();
		}
		else
		{
			this.isActive = _br.ReadBoolean();
		}
		if (this.TriggerType != PowerTrigger.TriggerTypes.Switch)
		{
			this.TriggerPowerDelay = (PowerTrigger.TriggerPowerDelayTypes)_br.ReadByte();
			this.TriggerPowerDuration = (PowerTrigger.TriggerPowerDurationTypes)_br.ReadByte();
			this.delayStartTime = _br.ReadSingle();
			this.powerTime = _br.ReadSingle();
		}
		if (this.TriggerType == PowerTrigger.TriggerTypes.Motion)
		{
			this.TargetType = (PowerTrigger.TargetTypes)_br.ReadInt32();
		}
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write((byte)this.TriggerType);
		if (this.TriggerType == PowerTrigger.TriggerTypes.Switch)
		{
			_bw.Write(this.isTriggered);
		}
		else
		{
			_bw.Write(this.isActive);
		}
		if (this.TriggerType != PowerTrigger.TriggerTypes.Switch)
		{
			_bw.Write((byte)this.TriggerPowerDelay);
			_bw.Write((byte)this.TriggerPowerDuration);
			_bw.Write(this.delayStartTime);
			_bw.Write(this.powerTime);
		}
		if (this.TriggerType == PowerTrigger.TriggerTypes.Motion)
		{
			_bw.Write((int)this.TargetType);
		}
	}

	public virtual void HandleDisconnectChildren()
	{
		this.HandlePowerUpdate(false);
		for (int i = 0; i < this.Children.Count; i++)
		{
			this.Children[i].HandleDisconnect();
		}
	}

	public override void HandleDisconnect()
	{
		this.parentTriggered = (this.isActive = false);
		base.HandleDisconnect();
	}

	public void ResetTrigger()
	{
		this.delayStartTime = -1f;
		this.powerTime = -1f;
		this.isActive = false;
		this.HandleDisconnectChildren();
		base.SendHasLocalChangesToRoot();
	}

	public PowerTrigger.TriggerTypes TriggerType;

	public byte Parameter;

	[PublicizedFrom(EAccessModifier.Protected)]
	public PowerTrigger.TriggerPowerDelayTypes triggerPowerDelay;

	[PublicizedFrom(EAccessModifier.Protected)]
	public PowerTrigger.TriggerPowerDurationTypes triggerPowerDuration = PowerTrigger.TriggerPowerDurationTypes.Triggered;

	public PowerTrigger.TargetTypes TargetType = PowerTrigger.TargetTypes.Self | PowerTrigger.TargetTypes.Allies;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float delayStartTime = -1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float powerTime;

	[PublicizedFrom(EAccessModifier.Protected)]
	public float lastPowerTime = -1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool lastTriggered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isTriggered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool parentTriggered;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isActive;

	public enum TriggerTypes
	{
		Switch,
		PressurePlate,
		TimerRelay,
		Motion,
		TripWire
	}

	public enum TriggerPowerDelayTypes
	{
		Instant,
		OneSecond,
		TwoSecond,
		ThreeSecond,
		FourSecond,
		FiveSecond
	}

	public enum TriggerPowerDurationTypes
	{
		Always,
		Triggered,
		OneSecond,
		TwoSecond,
		ThreeSecond,
		FourSecond,
		FiveSecond,
		SixSecond,
		SevenSecond,
		EightSecond,
		NineSecond,
		TenSecond,
		FifteenSecond,
		ThirtySecond,
		FourtyFiveSecond,
		OneMinute,
		FiveMinute,
		TenMinute,
		ThirtyMinute,
		SixtyMinute
	}

	[Flags]
	public enum TargetTypes
	{
		None = 0,
		Self = 1,
		Allies = 2,
		Strangers = 4,
		Zombies = 8
	}
}
