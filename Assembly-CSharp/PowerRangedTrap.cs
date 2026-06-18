using System;
using System.IO;

public class PowerRangedTrap : PowerConsumer
{
	public override PowerItem.PowerItemTypes PowerItemType
	{
		get
		{
			return PowerItem.PowerItemTypes.RangedTrap;
		}
	}

	public PowerRangedTrap()
	{
		this.Stacks = new ItemStack[3];
		for (int i = 0; i < this.Stacks.Length; i++)
		{
			this.Stacks[i] = ItemStack.Empty;
		}
	}

	public bool IsLocked
	{
		get
		{
			return this.isLocked;
		}
		set
		{
			if (this.isLocked != value)
			{
				this.isLocked = value;
			}
		}
	}

	public bool TryStackItem(ItemStack itemStack)
	{
		int num = 0;
		for (int i = 0; i < this.Stacks.Length; i++)
		{
			num = itemStack.count;
			if (this.Stacks[i].IsEmpty())
			{
				this.Stacks[i] = itemStack.Clone();
				itemStack.count = 0;
				return true;
			}
			if (this.Stacks[i].itemValue.type == itemStack.itemValue.type && this.Stacks[i].CanStackPartly(ref num))
			{
				this.Stacks[i].count += num;
				itemStack.count -= num;
				if (itemStack.count == 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool AddItem(ItemStack _itemStack)
	{
		if (!_itemStack.CanMoveTo(XUiC_ItemStack.StackLocationTypes.Workstation))
		{
			return false;
		}
		if (!this.isLocked)
		{
			for (int i = 0; i < this.Stacks.Length; i++)
			{
				if (this.Stacks[i].IsEmpty())
				{
					this.Stacks[i] = _itemStack;
					return true;
				}
			}
		}
		return false;
	}

	public void SetSlots(ItemStack[] _stacks)
	{
		this.Stacks = _stacks;
	}

	public override void read(BinaryReader _br, byte _version)
	{
		base.read(_br, _version);
		this.isLocked = _br.ReadBoolean();
		this.SetSlots(GameUtils.ReadItemStack(_br));
		this.TargetType = (PowerRangedTrap.TargetTypes)_br.ReadInt32();
	}

	public override void write(BinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.isLocked);
		GameUtils.WriteItemStack(_bw, this.Stacks);
		_bw.Write((int)this.TargetType);
	}

	public ItemStack[] Stacks;

	public PowerRangedTrap.TargetTypes TargetType = PowerRangedTrap.TargetTypes.Strangers | PowerRangedTrap.TargetTypes.Zombies;

	[PublicizedFrom(EAccessModifier.Protected)]
	public bool isLocked;

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
