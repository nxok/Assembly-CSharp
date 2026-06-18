using System;

public class PreferenceTracker
{
	public int PlayerID
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ItemStack[] toolbelt
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ItemStack[] bag
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public ItemValue[] equipment
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public bool AnyPreferences
	{
		get
		{
			return this.toolbelt != null || this.bag != null || this.equipment != null;
		}
	}

	public PreferenceTracker(int playerId)
	{
		this.PlayerID = playerId;
	}

	public PreferenceTracker Clone()
	{
		PreferenceTracker preferenceTracker = new PreferenceTracker(this.PlayerID);
		preferenceTracker.toolbelt = ((this.toolbelt != null) ? ItemStack.Clone(this.toolbelt) : null);
		preferenceTracker.bag = ((this.bag != null) ? ItemStack.Clone(this.bag) : null);
		if (this.equipment != null)
		{
			preferenceTracker.equipment = new ItemValue[this.equipment.Length];
			for (int i = 0; i < this.equipment.Length; i++)
			{
				preferenceTracker.equipment[i] = this.equipment[i].Clone();
			}
		}
		return preferenceTracker;
	}

	public void SetToolbelt(ItemStack[] _itemStacks, Predicate<ItemStack> _includeCondition)
	{
		if (_itemStacks == null || _itemStacks.Length == 0)
		{
			return;
		}
		this.toolbelt = new ItemStack[_itemStacks.Length];
		for (int i = 0; i < this.toolbelt.Length; i++)
		{
			if (_includeCondition(_itemStacks[i]))
			{
				this.toolbelt[i] = _itemStacks[i].Clone();
			}
			else
			{
				this.toolbelt[i] = new ItemStack();
			}
		}
	}

	public void SetBag(ItemStack[] _itemStacks, Predicate<ItemStack> _includeCondition)
	{
		if (_itemStacks == null || _itemStacks.Length == 0)
		{
			return;
		}
		this.bag = new ItemStack[_itemStacks.Length];
		for (int i = 0; i < this.bag.Length; i++)
		{
			if (_includeCondition(_itemStacks[i]))
			{
				this.bag[i] = _itemStacks[i].Clone();
			}
			else
			{
				this.bag[i] = new ItemStack();
			}
		}
	}

	public void SetEquipment(ItemValue[] _itemValues, Predicate<ItemValue> _includeCondition)
	{
		if (_itemValues == null && _itemValues.Length != 0)
		{
			return;
		}
		this.equipment = new ItemValue[_itemValues.Length];
		for (int i = 0; i < this.equipment.Length; i++)
		{
			if (_includeCondition(_itemValues[i]))
			{
				this.equipment[i] = _itemValues[i].Clone();
			}
			else
			{
				this.equipment[i] = new ItemValue();
			}
		}
	}

	public void Write(PooledBinaryWriter _bw)
	{
		_bw.Write(this.PlayerID);
		bool flag = this.toolbelt != null && this.toolbelt.Length != 0;
		_bw.Write(flag);
		if (flag)
		{
			GameUtils.WriteItemStack(_bw, this.toolbelt);
		}
		bool flag2 = this.equipment != null && this.equipment.Length != 0;
		_bw.Write(flag2);
		if (flag2)
		{
			GameUtils.WriteItemValueArray(_bw, this.equipment);
		}
		bool flag3 = this.bag != null && this.bag.Length != 0;
		_bw.Write(flag3);
		if (flag3)
		{
			GameUtils.WriteItemStack(_bw, this.bag);
		}
	}

	public void Read(PooledBinaryReader _br)
	{
		this.PlayerID = _br.ReadInt32();
		if (_br.ReadBoolean())
		{
			this.toolbelt = GameUtils.ReadItemStack(_br);
		}
		if (_br.ReadBoolean())
		{
			this.equipment = GameUtils.ReadItemValueArray(_br);
		}
		if (_br.ReadBoolean())
		{
			this.bag = GameUtils.ReadItemStack(_br);
		}
	}
}
