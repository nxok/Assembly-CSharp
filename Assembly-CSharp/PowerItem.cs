using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PowerItem
{
	public virtual bool IsPowered
	{
		get
		{
			return this.isPowered;
		}
	}

	public PowerItem()
	{
		this.Children = new List<PowerItem>();
	}

	public virtual bool CanParent(PowerItem newParent)
	{
		return true;
	}

	public virtual int InputCount
	{
		get
		{
			return 1;
		}
	}

	public virtual PowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return PowerItem.PowerItemTypes.Consumer;
		}
	}

	public virtual void AddTileEntity(TileEntityPowered tileEntityPowered)
	{
		if (this.TileEntity == null)
		{
			this.TileEntity = tileEntityPowered;
			this.TileEntity.CreateWireDataFromPowerItem();
		}
		this.TileEntity.MarkWireDirty();
	}

	public void RemoveTileEntity(TileEntityPowered tileEntityPowered)
	{
		if (this.TileEntity == tileEntityPowered)
		{
			this.TileEntity = null;
		}
	}

	public virtual PowerItem GetRoot()
	{
		if (this.Parent != null)
		{
			return this.Parent.GetRoot();
		}
		return this;
	}

	public virtual void read(BinaryReader _br, byte _version)
	{
		this.BlockID = _br.ReadUInt16();
		this.SetValuesFromBlock();
		this.Position = StreamUtils.ReadVector3i(_br);
		if (_br.ReadBoolean())
		{
			PowerManager.Instance.SetParent(this, PowerManager.Instance.GetPowerItemByWorldPos(StreamUtils.ReadVector3i(_br)));
		}
		int num = (int)_br.ReadByte();
		this.Children.Clear();
		for (int i = 0; i < num; i++)
		{
			PowerItem powerItem = PowerItem.CreateItem((PowerItem.PowerItemTypes)_br.ReadByte());
			powerItem.read(_br, _version);
			PowerManager.Instance.AddPowerNode(powerItem, this);
		}
	}

	public void RemoveSelfFromParent()
	{
		PowerManager.Instance.RemoveParent(this);
	}

	public virtual void write(BinaryWriter _bw)
	{
		_bw.Write(this.BlockID);
		StreamUtils.Write(_bw, this.Position);
		_bw.Write(this.Parent != null);
		if (this.Parent != null)
		{
			StreamUtils.Write(_bw, this.Parent.Position);
		}
		_bw.Write((byte)this.Children.Count);
		for (int i = 0; i < this.Children.Count; i++)
		{
			_bw.Write((byte)this.Children[i].PowerItemType);
			this.Children[i].write(_bw);
		}
	}

	public virtual bool PowerChildren()
	{
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void IsPoweredChanged(bool newPowered)
	{
	}

	public virtual void HandlePowerReceived(ref ushort power)
	{
		ushort num = (ushort)Mathf.Min((int)this.RequiredPower, (int)power);
		bool flag = num == this.RequiredPower;
		if (flag != this.isPowered)
		{
			this.isPowered = flag;
			this.IsPoweredChanged(flag);
			if (this.TileEntity != null)
			{
				this.TileEntity.SetModified();
			}
		}
		power -= num;
		if (power <= 0)
		{
			return;
		}
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

	[PublicizedFrom(EAccessModifier.Internal)]
	public PowerItem GetChild(Vector3 childPosition)
	{
		Vector3i vector3i = new Vector3i(childPosition);
		for (int i = 0; i < this.Children.Count; i++)
		{
			if (this.Children[i].Position == vector3i)
			{
				return this.Children[i];
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public bool HasChild(Vector3 child)
	{
		Vector3i vector3i = new Vector3i(child);
		for (int i = 0; i < this.Children.Count; i++)
		{
			if (this.Children[i].Position == vector3i)
			{
				return true;
			}
		}
		return false;
	}

	public virtual void HandlePowerUpdate(bool isOn)
	{
	}

	public virtual void HandleDisconnect()
	{
		if (this.isPowered)
		{
			this.IsPoweredChanged(false);
		}
		this.isPowered = false;
		this.HandlePowerUpdate(false);
		for (int i = 0; i < this.Children.Count; i++)
		{
			this.Children[i].HandleDisconnect();
		}
	}

	public static PowerItem CreateItem(PowerItem.PowerItemTypes itemType)
	{
		switch (itemType)
		{
		case PowerItem.PowerItemTypes.Consumer:
			return new PowerConsumer();
		case PowerItem.PowerItemTypes.ConsumerToggle:
			return new PowerConsumerToggle();
		case PowerItem.PowerItemTypes.Trigger:
			return new PowerTrigger();
		case PowerItem.PowerItemTypes.Timer:
			return new PowerTimerRelay();
		case PowerItem.PowerItemTypes.Generator:
			return new PowerGenerator();
		case PowerItem.PowerItemTypes.SolarPanel:
			return new PowerSolarPanel();
		case PowerItem.PowerItemTypes.BatteryBank:
			return new PowerBatteryBank();
		case PowerItem.PowerItemTypes.RangedTrap:
			return new PowerRangedTrap();
		case PowerItem.PowerItemTypes.ElectricWireRelay:
			return new PowerElectricWireRelay();
		case PowerItem.PowerItemTypes.TripWireRelay:
			return new PowerTripWireRelay();
		case PowerItem.PowerItemTypes.PressurePlate:
			return new PowerPressurePlate();
		default:
			return new PowerItem();
		}
	}

	public virtual void SetValuesFromBlock()
	{
		Block block = Block.list[(int)this.BlockID];
		if (block.Properties.Values.ContainsKey("RequiredPower"))
		{
			this.RequiredPower = ushort.Parse(block.Properties.Values["RequiredPower"]);
		}
	}

	public void ClearChildren()
	{
		for (int i = 0; i < this.Children.Count; i++)
		{
			PowerManager.Instance.RemoveChild(this.Children[i]);
		}
		if (this.TileEntity != null)
		{
			this.TileEntity.DrawWires();
		}
	}

	public void SendHasLocalChangesToRoot()
	{
		this.hasChangesLocal = true;
		for (PowerItem powerItem = this.Parent; powerItem != null; powerItem = powerItem.Parent)
		{
			powerItem.hasChangesLocal = true;
		}
	}

	public PowerItem Parent;

	public Vector3i Position;

	public PowerItem Root;

	public ushort Depth = ushort.MaxValue;

	public ushort BlockID;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool hasChangesLocal;

	public ushort RequiredPower = 5;

	public List<PowerItem> Children;

	public TileEntityPowered TileEntity;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isPowered;

	public enum PowerItemTypes
	{
		None,
		Consumer,
		ConsumerToggle,
		Trigger,
		Timer,
		Generator,
		SolarPanel,
		BatteryBank,
		RangedTrap,
		ElectricWireRelay,
		TripWireRelay,
		PressurePlate
	}
}
