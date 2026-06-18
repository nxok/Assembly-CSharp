using System;

public struct DMSUpdateConditions
{
	public bool DoesPlayerExist
	{
		set
		{
			this.SetBoolHolder(value, 128);
		}
	}

	public bool IsGameUnPaused
	{
		set
		{
			this.SetBoolHolder(value, 64);
		}
	}

	public bool IsDMSInitialized
	{
		set
		{
			this.SetBoolHolder(value, 32);
		}
	}

	public bool IsDMSEnabled
	{
		set
		{
			this.SetBoolHolder(value, 16);
		}
	}

	public bool CanUpdate
	{
		get
		{
			return this.BoolHolder == 240;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetBoolHolder(bool _value, byte _place)
	{
		if (_value)
		{
			this.BoolHolder |= _place;
			return;
		}
		this.BoolHolder &= ~_place;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public byte BoolHolder;
}
