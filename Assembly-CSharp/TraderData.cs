using System;
using System.Collections.Generic;
using System.IO;

public class TraderData
{
	public TraderData Clone()
	{
		TraderData traderData = new TraderData();
		traderData.CopyFrom(this);
		return traderData;
	}

	public void CopyFrom(TraderData other)
	{
		if (other == null)
		{
			return;
		}
		this.lastInventoryUpdate = other.lastInventoryUpdate;
		this.TraderID = other.TraderID;
		this.AvailableMoney = other.AvailableMoney;
		this.PrimaryInventory.Clear();
		for (int i = 0; i < other.PrimaryInventory.Count; i++)
		{
			this.PrimaryInventoryAdd(other.PrimaryInventory[i].Clone());
		}
		this.TierItemGroups.Clear();
		for (int j = 0; j < other.TierItemGroups.Count; j++)
		{
			this.TierItemGroups.Add(ItemStack.Clone(other.TierItemGroups[j]));
		}
	}

	public void SetModified(ITrader _trader)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageTraderData>().Setup(_trader), false);
	}

	public TraderInfo TraderInfo
	{
		get
		{
			if (this.TraderID != -1)
			{
				return TraderInfo.traderInfoList[this.TraderID];
			}
			return null;
		}
	}

	public float FullTime
	{
		get
		{
			return (float)((this.TraderInfo != null) ? this.TraderInfo.ResetIntervalInTicks : 0);
		}
	}

	public float CurrentTime
	{
		get
		{
			if (this.lastInventoryUpdate == 0UL)
			{
				return 0f;
			}
			return (this.FullTime - (float)((int)(GameManager.Instance.World.GetWorldTime() - this.lastInventoryUpdate))) / 10f;
		}
	}

	public ulong NextResetTime
	{
		get
		{
			if (this.TraderInfo == null)
			{
				return 0UL;
			}
			return this.lastInventoryUpdate + (ulong)((long)this.TraderInfo.ResetIntervalInTicks);
		}
	}

	public void PrimaryInventoryAdd(TraderData.Entry _entry)
	{
		if (_entry.Item.count <= 0 || _entry.Item.itemValue.type == 0)
		{
			Log.Warning("TraderData PrimaryInventoryAdd count {0}, type {1}", new object[]
			{
				_entry.Item.count,
				_entry.Item.itemValue.type
			});
			return;
		}
		this.PrimaryInventory.Add(_entry);
	}

	public void AddToPrimaryInventory(ItemStack stack, bool addedByPlayer)
	{
		for (int i = 0; i < this.PrimaryInventory.Count; i++)
		{
			if (stack.itemValue.type == this.PrimaryInventory[i].Item.itemValue.type && this.PrimaryInventory[i].AddedByPlayer == addedByPlayer)
			{
				ItemClass forId = ItemClass.GetForId(stack.itemValue.type);
				if (forId.CanStack())
				{
					int num = Math.Min(stack.count, forId.Stacknumber.Value - this.PrimaryInventory[i].Item.count);
					stack.count -= num;
					this.PrimaryInventory[i].Item.count += num;
					if (stack.count == 0)
					{
						return;
					}
				}
			}
		}
		if (stack.count > 0)
		{
			this.PrimaryInventoryAdd(new TraderData.Entry(stack.Clone(), 0, addedByPlayer));
		}
	}

	public int GetPrimaryItemCount(ItemValue itemValue)
	{
		int num = 0;
		for (int i = 0; i < this.PrimaryInventory.Count; i++)
		{
			if (itemValue.type == this.PrimaryInventory[i].Item.itemValue.type)
			{
				num += this.PrimaryInventory[i].Item.count;
			}
		}
		return num;
	}

	public void Read(BinaryReader _br)
	{
		this.TraderID = _br.ReadInt32();
		this.lastInventoryUpdate = _br.ReadUInt64();
		byte b = _br.ReadByte();
		this.ReadInventoryData(b, _br);
	}

	public void ReadInventoryData(byte _readVersion, BinaryReader _br)
	{
		this.PrimaryInventory.Clear();
		this.TierItemGroups.Clear();
		if (_readVersion < 2)
		{
			ItemStack[] array = GameUtils.ReadItemStack(_br);
			int num = (int)_br.ReadByte();
			for (int i = 0; i < num; i++)
			{
				this.TierItemGroups.Add(GameUtils.ReadItemStack(_br));
			}
			this.AvailableMoney = _br.ReadInt32();
			int num2 = _br.ReadInt32();
			sbyte[] array2 = new sbyte[num2];
			for (int j = 0; j < num2; j++)
			{
				array2[j] = _br.ReadSByte();
			}
			for (int k = 0; k < array.Length; k++)
			{
				sbyte b = ((k < array2.Length) ? array2[k] : 0);
				this.PrimaryInventoryAdd(new TraderData.Entry(array[k], b, false));
			}
			return;
		}
		int num3 = _br.ReadInt32();
		for (int l = 0; l < num3; l++)
		{
			TraderData.Entry entry = new TraderData.Entry();
			entry.Read(_br);
			this.PrimaryInventoryAdd(entry);
		}
		int num4 = (int)_br.ReadByte();
		for (int m = 0; m < num4; m++)
		{
			this.TierItemGroups.Add(GameUtils.ReadItemStack(_br));
		}
		this.AvailableMoney = _br.ReadInt32();
	}

	public void Write(BinaryWriter _bw)
	{
		_bw.Write(this.TraderID);
		_bw.Write(this.lastInventoryUpdate);
		_bw.Write(TraderData.FileVersion);
		this.WriteInventoryData(_bw);
	}

	public void WriteInventoryData(BinaryWriter _bw)
	{
		_bw.Write(this.PrimaryInventory.Count);
		for (int i = 0; i < this.PrimaryInventory.Count; i++)
		{
			this.PrimaryInventory[i].Write(_bw);
		}
		_bw.Write((byte)this.TierItemGroups.Count);
		for (int j = 0; j < this.TierItemGroups.Count; j++)
		{
			GameUtils.WriteItemStack(_bw, this.TierItemGroups[j]);
		}
		_bw.Write(this.AvailableMoney);
	}

	public List<TraderData.Entry> PrimaryInventory = new List<TraderData.Entry>();

	public List<ItemStack[]> TierItemGroups = new List<ItemStack[]>();

	public ulong lastInventoryUpdate;

	public int TraderID = -1;

	public int AvailableMoney;

	public static byte FileVersion = 2;

	public class Entry
	{
		public Entry()
		{
		}

		public Entry(ItemStack _item, sbyte _markup = 0, bool _addedByPlayer = false)
		{
			this.Item = _item;
			this.Markup = _markup;
			this.AddedByPlayer = _addedByPlayer;
		}

		public TraderData.Entry Clone()
		{
			return new TraderData.Entry(this.Item.Clone(), this.Markup, this.AddedByPlayer);
		}

		public void IncreaseMarkup()
		{
			if (this.Markup < 100)
			{
				this.Markup += 1;
			}
		}

		public void DecreaseMarkup()
		{
			if (this.Markup > -4)
			{
				this.Markup -= 1;
			}
		}

		public void Read(BinaryReader _br)
		{
			this.Item = new ItemStack().Read(_br);
			this.Markup = _br.ReadSByte();
			this.AddedByPlayer = _br.ReadBoolean();
		}

		public void Write(BinaryWriter _bw)
		{
			this.Item.Write(_bw);
			_bw.Write(this.Markup);
			_bw.Write(this.AddedByPlayer);
		}

		public ItemStack Item;

		public sbyte Markup;

		public bool AddedByPlayer;
	}
}
