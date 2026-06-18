using System;
using System.IO;
using Audio;
using UnityEngine;

public class PowerSource : PowerItem
{
	public bool IsOn
	{
		get
		{
			return this.isOn;
		}
		set
		{
			if (this.isOn != value)
			{
				base.SendHasLocalChangesToRoot();
				this.isOn = value;
				this.HandleOnOffSound();
				if (!this.isOn)
				{
					this.HandleDisconnect();
				}
				this.LastPowerUsed = 0;
				if (this.TileEntity != null)
				{
					this.TileEntity.Activate(this.isOn);
				}
			}
		}
	}

	public PowerSource()
	{
		this.Stacks = new ItemStack[6];
		for (int i = 0; i < this.Stacks.Length; i++)
		{
			this.Stacks[i] = ItemStack.Empty;
		}
	}

	public override bool IsPowered
	{
		get
		{
			return this.isOn;
		}
	}

	public virtual string OnSound
	{
		get
		{
			return "";
		}
	}

	public virtual string OffSound
	{
		get
		{
			return "";
		}
	}

	public void Refresh()
	{
		if (this.TileEntity != null)
		{
			this.TileEntity.Activate(this.isOn);
		}
	}

	public override bool CanParent(PowerItem newParent)
	{
		return false;
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		this.LastCurrentPower = (this.CurrentPower = _br.ReadUInt16());
		this.IsOn = _br.ReadBoolean();
		this.SetSlots(GameUtils.ReadItemStack(_br));
		this.hasChangesLocal = true;
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.CurrentPower);
		_bw.Write(this.IsOn);
		GameUtils.WriteItemStack(_bw, this.Stacks);
	}

	public virtual void Update()
	{
		this.HandleSendPower();
		if (this.hasChangesLocal)
		{
			for (int i = 0; i < this.Children.Count; i++)
			{
				this.Children[i].HandlePowerUpdate(this.IsOn);
			}
			this.hasChangesLocal = false;
		}
	}

	public virtual void HandleSendPower()
	{
		if (this.IsOn)
		{
			if (this.CurrentPower < this.MaxPower)
			{
				this.TickPowerGeneration();
			}
			else if (this.CurrentPower > this.MaxPower)
			{
				this.CurrentPower = this.MaxPower;
			}
			if (this.ShouldAutoTurnOff())
			{
				this.CurrentPower = 0;
				this.IsOn = false;
			}
			if (this.hasChangesLocal)
			{
				this.LastPowerUsed = 0;
				ushort num = (ushort)Mathf.Min((int)this.MaxOutput, (int)this.CurrentPower);
				ushort num2 = num;
				World world = GameManager.Instance.World;
				for (int i = 0; i < this.Children.Count; i++)
				{
					num = num2;
					this.Children[i].HandlePowerReceived(ref num2);
					this.LastPowerUsed += num - num2;
				}
			}
			this.CurrentPower -= this.LastPowerUsed;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual bool ShouldAutoTurnOff()
	{
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void TickPowerGeneration()
	{
	}

	public override void SetValuesFromBlock()
	{
		base.SetValuesFromBlock();
		Block block = Block.list[(int)this.BlockID];
		if (block.Properties.Values.ContainsKey("OutputPerStack"))
		{
			this.OutputPerStack = ushort.Parse(block.Properties.Values["OutputPerStack"]);
		}
		this.RequiredPower = (this.MaxPower = (this.MaxOutput = this.OutputPerStack * this.SlotCount));
	}

	public void SetSlots(ItemStack[] _stacks)
	{
		this.Stacks = _stacks;
		this.RefreshPowerStats();
	}

	public bool TryAddItemToSlot(ItemClass itemClass, ItemStack itemStack)
	{
		if (!this.IsOn)
		{
			for (int i = 0; i < this.Stacks.Length; i++)
			{
				if (this.Stacks[i].IsEmpty())
				{
					this.Stacks[i] = itemStack;
					this.RefreshPowerStats();
					return true;
				}
			}
		}
		return false;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void RefreshPowerStats()
	{
		this.SlotCount = 0;
		this.MaxOutput = 0;
		for (int i = 0; i < this.Stacks.Length; i++)
		{
			if (!this.Stacks[i].IsEmpty())
			{
				this.MaxOutput += (ushort)((float)this.OutputPerStack * Mathf.Lerp(0.5f, 1f, (float)this.Stacks[i].itemValue.Quality / 6f));
				this.SlotCount += 1;
			}
		}
		this.MaxOutput = (ushort)((float)this.MaxOutput * PowerSource.PowerOutputModifier);
		if (this.BlockID == 0 && this.TileEntity != null)
		{
			this.BlockID = (ushort)GameManager.Instance.World.GetBlock(this.TileEntity.ToWorldPos()).type;
			this.SetValuesFromBlock();
		}
		if (this.MaxPower == 0)
		{
			this.MaxPower = this.MaxOutput;
		}
		if (this.RequiredPower == 0)
		{
			this.RequiredPower = this.MaxOutput;
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void HandleOnOffSound()
	{
		if (GameManager.Instance.GameHasStarted)
		{
			Manager.BroadcastPlay(this.Position.ToVector3(), this.isOn ? this.OnSound : this.OffSound, 0f);
		}
	}

	public ushort OutputPerStack;

	public ushort SlotCount;

	public ushort MaxOutput;

	public ushort MaxPower = 60000;

	public ushort LastPowerUsed;

	public ushort CurrentPower;

	public ushort LastCurrentPower;

	public ItemStack[] Stacks;

	public static float PowerOutputModifier = 1f;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isOn;
}
