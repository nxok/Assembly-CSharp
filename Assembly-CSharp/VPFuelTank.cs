using System;
using UnityEngine.Scripting;

[Preserve]
public class VPFuelTank : VehiclePart
{
	public VPFuelTank()
	{
		this.fuelLevel = 0f;
	}

	public override void SetProperties(DynamicProperties _properties)
	{
		base.SetProperties(_properties);
		StringParsers.TryParseFloat(base.GetProperty("capacity"), out this.fuelCapacity);
		if (this.fuelCapacity < 1f)
		{
			this.fuelCapacity = 1f;
		}
		this.fuelLevel = this.fuelCapacity;
	}

	public override void HandleEvent(VehiclePart.Event _event, VehiclePart _part, float _amount)
	{
		if (_event == VehiclePart.Event.FuelRemove)
		{
			this.AddFuel(-_amount);
		}
	}

	public override bool IsBroken()
	{
		return false;
	}

	public float GetFuelLevel()
	{
		if (this.IsBroken())
		{
			return 0f;
		}
		return this.fuelLevel;
	}

	public float GetMaxFuelLevel()
	{
		if (this.IsBroken())
		{
			return 0f;
		}
		return this.fuelCapacity * this.vehicle.EffectFuelMaxPer;
	}

	public float GetFuelLevelPercent()
	{
		if (this.IsBroken())
		{
			return 0f;
		}
		float num = this.fuelLevel / (this.fuelCapacity * this.vehicle.EffectFuelMaxPer);
		if (num > 1f)
		{
			num = 1f;
		}
		return num;
	}

	public void SetFuelLevel(float _fuelLevel)
	{
		if (_fuelLevel <= 0f)
		{
			this.fuelLevel = 0f;
			if (EntityVehicle.VehicleFuelUsageModifier > 0f)
			{
				this.vehicle.FireEvent(VehiclePart.Event.FuelEmpty, this, 0f);
				return;
			}
		}
		else
		{
			float num = this.fuelCapacity * this.vehicle.EffectFuelMaxPer;
			if (_fuelLevel > num)
			{
				_fuelLevel = num;
			}
			this.fuelLevel = _fuelLevel;
		}
	}

	public void AddFuel(float _amount)
	{
		this.SetFuelLevel(this.fuelLevel + _amount);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public float fuelCapacity;

	[PublicizedFrom(EAccessModifier.Private)]
	public float fuelLevel;
}
