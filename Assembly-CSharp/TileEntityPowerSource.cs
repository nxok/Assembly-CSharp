using System;
using System.Collections.Generic;
using UnityEngine;

public class TileEntityPowerSource : TileEntityPowered
{
	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(this.ownerID);
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerID;
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.ownerID = _userIdentifier;
		this.setModified();
	}

	public ItemClass SlotItem
	{
		get
		{
			if (this.slotItem == null)
			{
				this.slotItem = ItemClass.GetItemClass((this.chunk.GetBlock(base.localChunkPos).Block as BlockPowerSource).SlotItemName, false);
			}
			return this.slotItem;
		}
		set
		{
			this.slotItem = value;
		}
	}

	public TileEntityPowerSource(Chunk _chunk)
		: base(_chunk)
	{
		this.ownerID = null;
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void SetSendSlots()
	{
		this.ClientData.SendSlots = true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TileEntityPowerSource(TileEntityPowerSource _other)
		: base(null)
	{
		this.ownerID = _other.ownerID;
		this.PowerItem = _other.PowerItem;
	}

	public override TileEntity Clone()
	{
		return new TileEntityPowerSource(this);
	}

	public override bool Activate(bool activated)
	{
		World world = GameManager.Instance.World;
		BlockValue block = this.chunk.GetBlock(base.localChunkPos);
		return block.Block.ActivateBlock(world, base.ToWorldPos(), block, activated, activated);
	}

	public override bool CanHaveParent(IPowered powered)
	{
		return this.PowerItemType == PowerItem.PowerItemTypes.BatteryBank;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (this.IsOn && base.IsByWater(world, base.ToWorldPos()))
		{
			(this.PowerItem as PowerSource).IsOn = false;
			base.SetModified();
		}
		if (this.bUserAccessing && this.IsOn)
		{
			base.SetModified();
		}
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeRead.Persistency)
		{
			if (base.UseLocalVersioning())
			{
				_br.ReadUInt16();
			}
			else
			{
				base.GetLegacyForkVersion();
			}
		}
		if (this.ClientData == null)
		{
			this.ClientData = new TileEntityPowerSource.ClientPowerData();
		}
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			if (_eStreamMode == TileEntity.StreamModeRead.FromClient)
			{
				this.bUserAccessing = _br.ReadBoolean();
				if (this.PowerItem == null)
				{
					this.PowerItem = base.CreatePowerItemForTileEntity((ushort)this.chunk.GetBlock(base.localChunkPos).type);
				}
				this.ClientData.AddedFuel = _br.ReadUInt16();
				if (this.ClientData.AddedFuel > 0)
				{
					ushort num = this.CurrentFuel + this.ClientData.AddedFuel;
					if (num > this.MaxFuel)
					{
						num = this.MaxFuel;
					}
					(this.PowerItem as PowerGenerator).CurrentFuel = num;
					this.ClientData.AddedFuel = 0;
					base.SetModified();
				}
				if (_br.ReadBoolean())
				{
					this.ClientData.ItemSlots = GameUtils.ReadItemStack(_br);
					(this.PowerItem as PowerSource).SetSlots(this.ClientData.ItemSlots);
					return;
				}
			}
			else if (_br.ReadBoolean())
			{
				this.ClientData.IsOn = _br.ReadBoolean();
				if (this.PowerItemType == PowerItem.PowerItemTypes.Generator)
				{
					this.ClientData.MaxFuel = _br.ReadUInt16();
					this.ClientData.CurrentFuel = _br.ReadUInt16();
				}
				else if (this.PowerItemType == PowerItem.PowerItemTypes.SolarPanel)
				{
					this.ClientData.SolarInput = _br.ReadUInt16();
				}
				ItemStack[] array = GameUtils.ReadItemStack(_br);
				if (!this.bUserAccessing || (this.bUserAccessing && this.IsOn))
				{
					this.ClientData.ItemSlots = array;
				}
				this.ClientData.MaxOutput = _br.ReadUInt16();
				this.ClientData.LastOutput = _br.ReadUInt16();
			}
		}
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
		if (_eStreamMode != TileEntity.StreamModeWrite.Persistency)
		{
			if (_eStreamMode == TileEntity.StreamModeWrite.ToServer)
			{
				_bw.Write(this.bUserAccessing);
				_bw.Write(this.ClientData.AddedFuel);
				this.ClientData.AddedFuel = 0;
				_bw.Write(this.ClientData.SendSlots);
				if (this.ClientData.SendSlots)
				{
					GameUtils.WriteItemStack(_bw, this.ClientData.ItemSlots);
					this.ClientData.SendSlots = false;
					return;
				}
			}
			else
			{
				PowerSource powerSource = this.PowerItem as PowerSource;
				_bw.Write(powerSource != null);
				if (powerSource != null)
				{
					_bw.Write(powerSource.IsOn);
					if (this.PowerItemType == PowerItem.PowerItemTypes.Generator)
					{
						_bw.Write((powerSource as PowerGenerator).MaxFuel);
						_bw.Write((powerSource as PowerGenerator).CurrentFuel);
					}
					else if (this.PowerItemType == PowerItem.PowerItemTypes.SolarPanel)
					{
						_bw.Write((powerSource as PowerSolarPanel).InputFromSun);
					}
					GameUtils.WriteItemStack(_bw, powerSource.Stacks);
					_bw.Write(powerSource.MaxOutput);
					_bw.Write(powerSource.LastPowerUsed);
				}
			}
		}
	}

	public bool HasSlottedItems()
	{
		ItemStack[] itemSlots = this.ItemSlots;
		for (int i = 0; i < itemSlots.Length; i++)
		{
			if (!itemSlots[i].IsEmpty())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsOn
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				PowerSource powerSource = this.PowerItem as PowerSource;
				return powerSource != null && powerSource.IsOn;
			}
			return this.ClientData.IsOn;
		}
	}

	public ushort CurrentFuel
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (this.PowerItem as PowerGenerator).CurrentFuel;
			}
			return this.ClientData.CurrentFuel;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(this.PowerItem as PowerGenerator).CurrentFuel = value;
				return;
			}
			this.ClientData.CurrentFuel = value;
			base.SetModified();
		}
	}

	public ushort MaxFuel
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (this.PowerItem as PowerGenerator).MaxFuel;
			}
			return this.ClientData.MaxFuel;
		}
	}

	public ushort MaxOutput
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (this.PowerItem as PowerSource).MaxOutput;
			}
			return this.ClientData.MaxOutput;
		}
	}

	public ushort LastOutput
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (this.PowerItem as PowerSource).LastPowerUsed;
			}
			return this.ClientData.LastOutput;
		}
	}

	public ItemStack[] ItemSlots
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (this.PowerItem as PowerSource).Stacks;
			}
			return this.ClientData.ItemSlots;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(this.PowerItem as PowerSource).SetSlots(value);
				return;
			}
			this.ClientData.ItemSlots = value;
			base.SetModified();
		}
	}

	public bool TryAddItemToSlot(ItemClass itemClass, ItemStack itemStack)
	{
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return (this.PowerItem as PowerSource).TryAddItemToSlot(itemClass, itemStack);
		}
		if (!this.IsOn)
		{
			for (int i = 0; i < this.ClientData.ItemSlots.Length; i++)
			{
				if (this.ClientData.ItemSlots[i].IsEmpty())
				{
					this.ClientData.ItemSlots[i] = itemStack;
					this.ClientData.SendSlots = true;
					base.SetModified();
					return true;
				}
			}
		}
		return false;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			PowerSource powerSource = this.PowerItem as PowerSource;
			powerSource.HandleDisconnect();
			PowerManager.Instance.RemovePowerNode(powerSource);
		}
	}

	public override void ReplacedBy(BlockValue _bvOld, BlockValue _bvNew, TileEntity _teNew)
	{
		base.ReplacedBy(_bvOld, _bvNew, _teNew);
		TileEntityPowerSource tileEntityPowerSource;
		if (_teNew.TryGetSelfOrFeature(out tileEntityPowerSource))
		{
			return;
		}
		List<ItemStack> list = new List<ItemStack>();
		list.AddRange(this.ItemSlots);
		if (this.PowerItemType == PowerItem.PowerItemTypes.Generator && this.CurrentFuel > 0)
		{
			ItemValue itemValue = new ItemValue(ItemClass.GetItemWithTag(XUiC_PowerSourceStats.tag).Id, false);
			int value = itemValue.ItemClass.Stacknumber.Value;
			int num;
			for (int i = (int)this.CurrentFuel; i > 0; i -= num)
			{
				num = Mathf.Min(i, value);
				list.Add(new ItemStack(itemValue, num));
			}
		}
		Vector3 vector = base.ToWorldCenterPos();
		vector.y += 0.9f;
		GameManager.Instance.DropContentInLootContainerServer(-1, "DroppedLootContainer", vector, list.ToArray(), true, null);
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.PowerSource;
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_success)
		{
			GameManager.ShowTooltip(uiforPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractItem", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			return;
		}
		((XUiC_PowerSourceWindowGroup)((XUiWindowGroup)uiforPrimaryPlayer.windowManager.GetWindow("powersource")).Controller).TileEntity = this;
		uiforPrimaryPlayer.windowManager.Open("powersource", true);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new const int Version = 18;

	public bool syncNeeded = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemClass slotItem;

	public TileEntityPowerSource.ClientPowerData ClientData = new TileEntityPowerSource.ClientPowerData();

	public class ClientPowerData
	{
		public ClientPowerData()
		{
			for (int i = 0; i < this.ItemSlots.Length; i++)
			{
				this.ItemSlots[i] = ItemStack.Empty;
			}
		}

		public bool IsOn;

		public ushort MaxFuel;

		public ushort CurrentFuel;

		public ushort SolarInput;

		public ushort MaxOutput;

		public ushort LastOutput;

		public ushort AddedFuel;

		public bool SendSlots;

		public ItemStack[] ItemSlots = new ItemStack[6];
	}
}
