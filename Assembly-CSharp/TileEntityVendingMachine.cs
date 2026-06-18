using System;
using System.Collections.Generic;
using Platform;
using UnityEngine;
using UnityEngine.Scripting;

public class TileEntityVendingMachine : TileEntity, ILockable, ITrader
{
	public TraderData TraderData { get; set; }

	public TileEntityVendingMachine(Chunk _chunk)
		: base(_chunk)
	{
		this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
		this.isLocked = true;
		this.ownerID = null;
		this.passwordHash = "";
		this.rentalEndTime = 0UL;
		this.rentalEndDay = 0;
		this.nextAutoBuy = 0UL;
		this.TraderData = new TraderData();
	}

	public override TileEntity Clone()
	{
		return new TileEntityVendingMachine(null)
		{
			bUserAccessing = this.bUserAccessing,
			TraderData = this.TraderData.Clone(),
			allowedUserIds = new List<PlatformUserIdentifierAbs>(this.allowedUserIds),
			isLocked = this.isLocked,
			ownerID = this.ownerID,
			passwordHash = this.passwordHash,
			rentalEndTime = this.rentalEndTime,
			rentalEndDay = this.rentalEndDay,
			nextAutoBuy = this.nextAutoBuy
		};
	}

	public TileEntityVendingMachine.RentResult CanRent()
	{
		if (this.ownerID != null && !this.ownerID.Equals(PlatformManager.InternalLocalUserIdentifier))
		{
			return TileEntityVendingMachine.RentResult.AlreadyRented;
		}
		if (GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui.PlayerInventory.CurrencyAmount < this.TraderData.TraderInfo.RentCost)
		{
			return TileEntityVendingMachine.RentResult.NotEnoughMoney;
		}
		if (this.checkAlreadyRentingVM())
		{
			return TileEntityVendingMachine.RentResult.AlreadyRentingVM;
		}
		return TileEntityVendingMachine.RentResult.Allowed;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool checkAlreadyRentingVM()
	{
		EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
		Vector3i rentedVMPosition = primaryPlayer.RentedVMPosition;
		return !(rentedVMPosition == base.ToWorldPos()) && !(rentedVMPosition == Vector3i.zero) && primaryPlayer.RentalEndDay > GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime);
	}

	public bool IsLocked()
	{
		return this.isLocked;
	}

	public void SetLocked(bool _isLocked)
	{
		this.isLocked = _isLocked;
		this.setModified();
	}

	public void SetOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		this.ownerID = _userIdentifier;
		this.setModified();
	}

	public bool IsUserAllowed(PlatformUserIdentifierAbs _userIdentifier)
	{
		return (_userIdentifier != null && _userIdentifier.Equals(this.ownerID)) || this.allowedUserIds.Contains(_userIdentifier);
	}

	public bool LocalPlayerIsOwner()
	{
		return this.IsOwner(PlatformManager.InternalLocalUserIdentifier);
	}

	public bool IsOwner(PlatformUserIdentifierAbs _userIdentifier)
	{
		return _userIdentifier != null && _userIdentifier.Equals(this.ownerID);
	}

	public PlatformUserIdentifierAbs GetOwner()
	{
		return this.ownerID;
	}

	public bool HasPassword()
	{
		return !string.IsNullOrEmpty(this.passwordHash);
	}

	public string GetPasswordHash()
	{
		return this.passwordHash;
	}

	public bool SetPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (this.IsOwner(_userIdentifier) && _passwordHash != this.passwordHash)
		{
			this.passwordHash = _passwordHash;
			this.allowedUserIds.Clear();
			base.SetModified();
			return true;
		}
		return false;
	}

	public bool CheckPasswordHash(string _passwordHash, PlatformUserIdentifierAbs _userIdentifier)
	{
		if (this.IsOwner(_userIdentifier) || !this.HasPassword())
		{
			return true;
		}
		if (_passwordHash == this.passwordHash)
		{
			this.allowedUserIds.Add(_userIdentifier);
			base.SetModified();
			return true;
		}
		return false;
	}

	public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
	{
		base.read(_br, _eStreamMode);
		int num = _br.ReadInt32();
		if (this.TraderData == null)
		{
			this.TraderData = new TraderData();
		}
		if (this.allowedUserIds == null)
		{
			this.allowedUserIds = new List<PlatformUserIdentifierAbs>();
		}
		this.allowedUserIds.Clear();
		if (num < 3)
		{
			this.TraderData.Read(_br);
			int num2 = _br.ReadInt32();
			this.isLocked = _br.ReadBoolean();
			this.ownerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
			this.passwordHash = _br.ReadString();
			int num3 = _br.ReadInt32();
			for (int i = 0; i < num3; i++)
			{
				this.allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br, false, false));
			}
			if (num2 > 1)
			{
				this.rentalEndDay = _br.ReadInt32();
				this.rentalEndTime = 0UL;
			}
			else
			{
				this.rentalEndTime = _br.ReadUInt64();
				this.rentalEndDay = GameUtils.WorldTimeToDays(this.rentalEndTime);
			}
			this.TraderData.Read(_br);
			this.nextAutoBuy = (this.TraderData.TraderInfo.Rentable ? _br.ReadUInt64() : 0UL);
			return;
		}
		this.isLocked = _br.ReadBoolean();
		this.ownerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		this.passwordHash = _br.ReadString();
		int num4 = _br.ReadInt32();
		for (int j = 0; j < num4; j++)
		{
			this.allowedUserIds.Add(PlatformUserIdentifierAbs.FromStream(_br, false, false));
		}
		this.rentalEndDay = _br.ReadInt32();
		this.rentalEndTime = 0UL;
		this.TraderData.Read(_br);
		this.nextAutoBuy = (this.TraderData.TraderInfo.Rentable ? _br.ReadUInt64() : 0UL);
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		_bw.Write(3);
		_bw.Write(this.isLocked);
		this.ownerID.ToStream(_bw, false);
		_bw.Write(this.passwordHash);
		_bw.Write(this.allowedUserIds.Count);
		for (int i = 0; i < this.allowedUserIds.Count; i++)
		{
			this.allowedUserIds[i].ToStream(_bw, false);
		}
		_bw.Write(this.rentalEndDay);
		this.TraderData.Write(_bw);
		if (this.TraderData.TraderInfo.Rentable)
		{
			_bw.Write(this.nextAutoBuy);
		}
	}

	public override void UpgradeDowngradeFrom(TileEntity _other)
	{
		base.UpgradeDowngradeFrom(_other);
		if (_other is ILockable)
		{
			ILockable lockable = _other as ILockable;
			this.SetLocked(lockable.IsLocked());
			this.SetOwner(lockable.GetOwner());
			this.allowedUserIds = new List<PlatformUserIdentifierAbs>(lockable.GetUsers());
			this.passwordHash = lockable.GetPasswordHash();
			this.setModified();
		}
	}

	public List<PlatformUserIdentifierAbs> GetUsers()
	{
		return this.allowedUserIds;
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.VendingMachine;
	}

	public override bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return LocalPlayerUI.GetUIForPrimaryPlayer() != null;
	}

	public override void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		if (!_success)
		{
			return;
		}
		TileEntityVendingMachine.VendingMachineLockContext vendingMachineLockContext = _context as TileEntityVendingMachine.VendingMachineLockContext;
		if (vendingMachineLockContext != null)
		{
			GameManager.Instance.traderManager.TraderInventoryRequested(this.TraderData, _lockingPlayerID);
			vendingMachineLockContext.TraderData = this.TraderData.Clone();
		}
	}

	public override void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		LocalPlayerUI uiforPrimaryPlayer = LocalPlayerUI.GetUIForPrimaryPlayer();
		if (!_success)
		{
			GameManager.ShowTooltip(uiforPrimaryPlayer.entityPlayer, Localization.Get("ttNoInteractPerson", false, null), string.Empty, "ui_denied", null, false, false, 0f);
			uiforPrimaryPlayer.entityPlayer.OverrideFOV = -1f;
			uiforPrimaryPlayer.xui.Dialog.KeepZoomOnClose = false;
			return;
		}
		TileEntityVendingMachine.VendingMachineLockContext vendingMachineLockContext = _context as TileEntityVendingMachine.VendingMachineLockContext;
		if (vendingMachineLockContext != null && vendingMachineLockContext.TraderData != null)
		{
			if (this.TraderData == null)
			{
				this.TraderData = new TraderData();
			}
			this.TraderData.CopyFrom(vendingMachineLockContext.TraderData);
		}
		uiforPrimaryPlayer.xui.Trader.Trader = this;
		uiforPrimaryPlayer.windowManager.CloseAllOpenModalWindows(null, false);
		uiforPrimaryPlayer.windowManager.Open("trader", true);
	}

	public override void OnUnlockedServer(int _unlockingPlayerID, ushort _channel)
	{
		this.TraderData.SetModified(this);
	}

	[PublicizedFrom(EAccessModifier.Internal)]
	public void ClearOwner()
	{
		this.ownerID = null;
	}

	public bool IsRentable
	{
		get
		{
			return this.TraderData.TraderInfo.Rentable;
		}
	}

	public float RentTimeRemaining
	{
		get
		{
			return (float)(this.rentalEndDay - GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime));
		}
	}

	public int RentalEndDay
	{
		get
		{
			return this.rentalEndDay;
		}
	}

	public bool Rent()
	{
		if ((this.ownerID != null && !this.ownerID.Equals(PlatformManager.InternalLocalUserIdentifier)) || !this.TraderData.TraderInfo.Rentable)
		{
			return false;
		}
		XUi xui = GameManager.Instance.World.GetPrimaryPlayer().PlayerUI.xui;
		if (xui.PlayerInventory.CurrencyAmount >= this.TraderData.TraderInfo.RentCost)
		{
			ItemStack itemStack = new ItemStack(ItemClass.GetItem(TraderInfo.CurrencyItem, false), this.TraderData.TraderInfo.RentCost);
			xui.PlayerInventory.RemoveItem(itemStack);
			if (this.ownerID == null)
			{
				this.ownerID = PlatformManager.InternalLocalUserIdentifier;
				this.rentalEndDay = GameUtils.WorldTimeToDays(GameManager.Instance.World.worldTime) + 30;
				this.SetAutoBuyTime(true);
			}
			else
			{
				this.rentalEndDay += 30;
			}
			EntityPlayerLocal primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
			primaryPlayer.RentedVMPosition = base.ToWorldPos();
			primaryPlayer.RentalEndDay = this.rentalEndDay;
			this.setModified();
			return true;
		}
		return false;
	}

	public void ClearVendingMachine()
	{
		this.TraderData.AvailableMoney = 0;
		this.TraderData.PrimaryInventory.Clear();
		this.ownerID = null;
		this.allowedUserIds.Clear();
		this.rentalEndTime = 0UL;
		this.passwordHash = "";
		this.setModified();
	}

	public bool TryAutoBuy(bool isInitial = true)
	{
		if (this.nextAutoBuy == 0UL)
		{
			this.SetAutoBuyTime(true);
		}
		if (GameManager.Instance.World.worldTime > this.nextAutoBuy)
		{
			GameRandom random = GameManager.Instance.lootManager.Random;
			if (random.RandomFloat < this.autoBuyThreshold && this.TraderData.PrimaryInventory.Count > this.minimumAutoBuyCount)
			{
				int num = random.RandomRange(1, Mathf.Max(this.TraderData.PrimaryInventory.Count / 10, 1));
				Log.Warning("Items Purchased: " + num.ToString());
				for (int i = 0; i < num; i++)
				{
					int num2 = 0;
					for (int j = 0; j < this.TraderData.PrimaryInventory.Count; j++)
					{
						if (this.TraderData.PrimaryInventory[j].Markup <= 0)
						{
							ItemStack item = this.TraderData.PrimaryInventory[j].Item;
							if ((item.itemValue.ItemClass.IsBlock() ? Block.list[item.itemValue.type].EconomicValue : item.itemValue.ItemClass.EconomicValue) > 0f && item.itemValue.ItemClass.SellableToTrader)
							{
								num2++;
							}
						}
					}
					if (num2 > 0)
					{
						int num3 = random.RandomRange(num2);
						num2 = 0;
						for (int k = 0; k < this.TraderData.PrimaryInventory.Count; k++)
						{
							if (this.TraderData.PrimaryInventory[k].Markup <= 0)
							{
								ItemStack item2 = this.TraderData.PrimaryInventory[k].Item;
								if ((item2.itemValue.ItemClass.IsBlock() ? Block.list[item2.itemValue.type].EconomicValue : item2.itemValue.ItemClass.EconomicValue) > 0f && item2.itemValue.ItemClass.SellableToTrader)
								{
									if (num2 == num3)
									{
										int count = item2.count;
										int buyPrice = XUiM_Trader.GetBuyPrice(LocalPlayerUI.GetUIForPrimaryPlayer().xui, item2.itemValue, count, null, k);
										this.TraderData.PrimaryInventory.RemoveAt(k);
										this.TraderData.AvailableMoney += buyPrice;
										break;
									}
									num2++;
								}
							}
						}
					}
				}
				this.autoBuyThreshold = this.autoBuyThresholdStep;
			}
			else
			{
				this.autoBuyThreshold += this.autoBuyThresholdStep;
			}
			this.SetAutoBuyTime(false);
			return this.TryAutoBuy(false);
		}
		return !isInitial;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void SetAutoBuyTime(bool isInitial)
	{
		uint num = 24000U;
		if (isInitial)
		{
			this.nextAutoBuy = GameManager.Instance.World.worldTime + (ulong)num;
			return;
		}
		this.nextAutoBuy += (ulong)num;
	}

	public override void UpdateTick(World world)
	{
		base.UpdateTick(world);
		if (!this.TraderData.TraderInfo.PlayerOwned && this.TraderData.TraderInfo.Rentable && this.ownerID != null && this.rentalEndDay <= GameUtils.WorldTimeToDays(world.worldTime))
		{
			this.ClearVendingMachine();
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public const int ver = 3;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool isLocked;

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	[PublicizedFrom(EAccessModifier.Private)]
	public List<PlatformUserIdentifierAbs> allowedUserIds;

	[PublicizedFrom(EAccessModifier.Private)]
	public string passwordHash;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong rentalEndTime;

	[PublicizedFrom(EAccessModifier.Private)]
	public ulong nextAutoBuy;

	[PublicizedFrom(EAccessModifier.Private)]
	public int rentalEndDay;

	[PublicizedFrom(EAccessModifier.Private)]
	public float autoBuyThresholdStep = 0.33333334f;

	[PublicizedFrom(EAccessModifier.Private)]
	public float autoBuyThreshold = 0.33333334f;

	[PublicizedFrom(EAccessModifier.Private)]
	public int minimumAutoBuyCount = 5;

	public enum RentResult
	{
		Allowed,
		AlreadyRented,
		AlreadyRentingVM,
		NotEnoughMoney
	}

	[Preserve]
	public class VendingMachineLockContext : ILockContext
	{
		public VendingMachineLockContext()
		{
		}

		public VendingMachineLockContext(TraderData _traderData)
		{
			this.TraderData = _traderData.Clone();
		}

		public void Read(PooledBinaryReader _br)
		{
			if (this.TraderData == null)
			{
				this.TraderData = new TraderData();
			}
			this.TraderData.Read(_br);
		}

		public void Write(PooledBinaryWriter _bw)
		{
			this.TraderData.Write(_bw);
		}

		public TraderData TraderData;
	}
}
