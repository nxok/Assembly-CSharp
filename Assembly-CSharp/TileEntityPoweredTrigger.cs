using System;

public class TileEntityPoweredTrigger : TileEntityPowered
{
	public bool IsTriggered
	{
		get
		{
			return ((PowerTrigger)this.PowerItem).IsTriggered;
		}
		set
		{
			PowerTrigger powerTrigger = this.PowerItem as PowerTrigger;
			powerTrigger.IsTriggered = value;
			if (powerTrigger.TriggerType == PowerTrigger.TriggerTypes.PressurePlate)
			{
				(powerTrigger as PowerPressurePlate).Pressed = true;
			}
		}
	}

	public TileEntityPoweredTrigger(Chunk _chunk)
		: base(_chunk)
	{
	}

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
		base.SetModified();
	}

	public bool Activate(bool activated, bool isOn)
	{
		World world = GameManager.Instance.World;
		BlockValue block = this.chunk.GetBlock(base.localChunkPos);
		return block.Block.ActivateBlock(world, base.ToWorldPos(), block, isOn, activated);
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public override PowerItem CreatePowerItem()
	{
		BlockPowered blockPowered = (BlockPowered)this.chunk.GetBlock(base.localChunkPos).Block;
		if (blockPowered is BlockPressurePlate)
		{
			this.TriggerType = PowerTrigger.TriggerTypes.PressurePlate;
		}
		else if (blockPowered is BlockMotionSensor)
		{
			this.TriggerType = PowerTrigger.TriggerTypes.Motion;
		}
		else if (blockPowered is BlockTripWire)
		{
			this.TriggerType = PowerTrigger.TriggerTypes.TripWire;
		}
		else if (blockPowered is BlockTimerRelay)
		{
			this.TriggerType = PowerTrigger.TriggerTypes.TimerRelay;
		}
		else if (blockPowered is BlockSwitch)
		{
			this.TriggerType = PowerTrigger.TriggerTypes.Switch;
		}
		if (this.TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
		{
			return new PowerTimerRelay
			{
				TriggerType = this.TriggerType
			};
		}
		if (this.TriggerType == PowerTrigger.TriggerTypes.TripWire)
		{
			return new PowerTripWireRelay
			{
				TriggerType = PowerTrigger.TriggerTypes.TripWire
			};
		}
		if (this.TriggerType == PowerTrigger.TriggerTypes.Motion)
		{
			return new PowerTrigger
			{
				TriggerType = this.TriggerType,
				TriggerPowerDuration = PowerTrigger.TriggerPowerDurationTypes.Triggered,
				TriggerPowerDelay = PowerTrigger.TriggerPowerDelayTypes.Instant
			};
		}
		if (this.TriggerType == PowerTrigger.TriggerTypes.PressurePlate)
		{
			return new PowerPressurePlate
			{
				TriggerType = this.TriggerType
			};
		}
		return new PowerTrigger
		{
			TriggerType = this.TriggerType
		};
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
		this.TriggerType = (PowerTrigger.TriggerTypes)_br.ReadByte();
		if (this.TriggerType == PowerTrigger.TriggerTypes.Motion)
		{
			this.ownerID = PlatformUserIdentifierAbs.FromStream(_br, false, false);
		}
		if (_eStreamMode != TileEntity.StreamModeRead.Persistency)
		{
			if (_eStreamMode == TileEntity.StreamModeRead.FromClient)
			{
				if (this.PowerItem == null)
				{
					this.PowerItem = base.CreatePowerItemForTileEntity((ushort)this.chunk.GetBlock(base.localChunkPos).type);
				}
				if (this.TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
				{
					(this.PowerItem as PowerTimerRelay).StartTime = _br.ReadByte();
					(this.PowerItem as PowerTimerRelay).EndTime = _br.ReadByte();
				}
				else if (this.TriggerType != PowerTrigger.TriggerTypes.Switch)
				{
					(this.PowerItem as PowerTrigger).TriggerPowerDelay = (PowerTrigger.TriggerPowerDelayTypes)_br.ReadByte();
					(this.PowerItem as PowerTrigger).TriggerPowerDuration = (PowerTrigger.TriggerPowerDurationTypes)_br.ReadByte();
					if (_br.ReadBoolean())
					{
						(this.PowerItem as PowerTrigger).ResetTrigger();
					}
				}
				if (this.TriggerType == PowerTrigger.TriggerTypes.Motion)
				{
					this.TargetType = _br.ReadInt32();
					return;
				}
			}
			else
			{
				if (this.TriggerType == PowerTrigger.TriggerTypes.TripWire)
				{
					this.ClientData.ShowTriggerOptions = _br.ReadBoolean();
					this.ClientData.HasChanges = true;
				}
				if (this.TriggerType != PowerTrigger.TriggerTypes.Switch)
				{
					this.ClientData.Property1 = _br.ReadByte();
					this.ClientData.Property2 = _br.ReadByte();
					this.ClientData.HasChanges = true;
				}
				if (this.TriggerType == PowerTrigger.TriggerTypes.Motion)
				{
					int num = _br.ReadInt32();
					if (!this.bUserAccessing)
					{
						this.TargetType = num;
					}
				}
			}
		}
	}

	public void ResetTrigger()
	{
		if (this.TriggerType != PowerTrigger.TriggerTypes.TimerRelay)
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(this.PowerItem as PowerTrigger).ResetTrigger();
				return;
			}
			this.ClientData.ResetTrigger = true;
			base.SetModified();
		}
	}

	public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
	{
		base.write(_bw, _eStreamMode);
		if (_eStreamMode == TileEntity.StreamModeWrite.Persistency)
		{
			_bw.Write(18);
		}
		_bw.Write((byte)this.TriggerType);
		if (this.TriggerType == PowerTrigger.TriggerTypes.Motion)
		{
			this.ownerID.ToStream(_bw, false);
		}
		if (_eStreamMode != TileEntity.StreamModeWrite.Persistency)
		{
			if (_eStreamMode == TileEntity.StreamModeWrite.ToServer)
			{
				if (this.TriggerType != PowerTrigger.TriggerTypes.Switch)
				{
					_bw.Write(this.ClientData.Property1);
					_bw.Write(this.ClientData.Property2);
					_bw.Write(this.ClientData.ResetTrigger);
					this.ClientData.ResetTrigger = false;
				}
				if (this.TriggerType == PowerTrigger.TriggerTypes.Motion)
				{
					_bw.Write(this.TargetType);
					return;
				}
			}
			else
			{
				if (this.PowerItem == null)
				{
					this.PowerItem = base.CreatePowerItemForTileEntity((ushort)this.chunk.GetBlock(base.localChunkPos).type);
				}
				if (this.TriggerType == PowerTrigger.TriggerTypes.TripWire)
				{
					PowerTripWireRelay powerTripWireRelay = this.PowerItem as PowerTripWireRelay;
					_bw.Write(powerTripWireRelay.Parent != null && powerTripWireRelay.Parent is PowerTripWireRelay);
				}
				if (this.TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
				{
					_bw.Write((this.PowerItem as PowerTimerRelay).StartTime);
					_bw.Write((this.PowerItem as PowerTimerRelay).EndTime);
				}
				else if (this.TriggerType != PowerTrigger.TriggerTypes.Switch)
				{
					_bw.Write((byte)(this.PowerItem as PowerTrigger).TriggerPowerDelay);
					_bw.Write((byte)(this.PowerItem as PowerTrigger).TriggerPowerDuration);
				}
				if (this.TriggerType == PowerTrigger.TriggerTypes.Motion)
				{
					_bw.Write(this.TargetType);
				}
			}
		}
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
		((XUiC_PowerTriggerWindowGroup)((XUiWindowGroup)uiforPrimaryPlayer.windowManager.GetWindow("powertrigger")).Controller).TileEntity = this;
		uiforPrimaryPlayer.windowManager.Open("powertrigger", true);
	}

	public byte Property1
	{
		get
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return this.ClientData.Property1;
			}
			if (this.TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
			{
				return (this.PowerItem as PowerTimerRelay).StartTime;
			}
			return (byte)(this.PowerItem as PowerTrigger).TriggerPowerDelay;
		}
		set
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.ClientData.Property1 = value;
				return;
			}
			if (this.TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
			{
				(this.PowerItem as PowerTimerRelay).StartTime = value;
				return;
			}
			(this.PowerItem as PowerTrigger).TriggerPowerDelay = (PowerTrigger.TriggerPowerDelayTypes)value;
		}
	}

	public byte Property2
	{
		get
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return this.ClientData.Property2;
			}
			if (this.TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
			{
				return (this.PowerItem as PowerTimerRelay).EndTime;
			}
			return (byte)(this.PowerItem as PowerTrigger).TriggerPowerDuration;
		}
		set
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				this.ClientData.Property2 = value;
				return;
			}
			if (this.TriggerType == PowerTrigger.TriggerTypes.TimerRelay)
			{
				(this.PowerItem as PowerTimerRelay).EndTime = value;
				return;
			}
			(this.PowerItem as PowerTrigger).TriggerPowerDuration = (PowerTrigger.TriggerPowerDurationTypes)value;
		}
	}

	public int TargetType
	{
		get
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return (int)(this.PowerItem as PowerTrigger).TargetType;
			}
			return this.ClientData.TargetType;
		}
		set
		{
			if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				(this.PowerItem as PowerTrigger).TargetType = (PowerTrigger.TargetTypes)value;
				return;
			}
			this.ClientData.TargetType = value;
		}
	}

	public bool ShowTriggerOptions
	{
		get
		{
			if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
			{
				return this.ClientData.ShowTriggerOptions;
			}
			if (this.TriggerType == PowerTrigger.TriggerTypes.TripWire)
			{
				PowerTripWireRelay powerTripWireRelay = this.PowerItem as PowerTripWireRelay;
				return powerTripWireRelay.Parent != null && powerTripWireRelay.Parent is PowerTripWireRelay;
			}
			return true;
		}
	}

	public bool TargetSelf
	{
		get
		{
			return (this.TargetType & 1) == 1;
		}
	}

	public bool TargetAllies
	{
		get
		{
			return (this.TargetType & 2) == 2;
		}
	}

	public bool TargetStrangers
	{
		get
		{
			return (this.TargetType & 4) == 4;
		}
	}

	public bool TargetZombies
	{
		get
		{
			return (this.TargetType & 8) == 8;
		}
	}

	public override TileEntityType GetTileEntityType()
	{
		return TileEntityType.Trigger;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public new const int Version = 18;

	public PowerTrigger.TriggerTypes TriggerType;

	public TileEntityPoweredTrigger.ClientTriggerData ClientData = new TileEntityPoweredTrigger.ClientTriggerData();

	[PublicizedFrom(EAccessModifier.Private)]
	public PlatformUserIdentifierAbs ownerID;

	public class ClientTriggerData
	{
		public byte Property1;

		public byte Property2;

		public int TargetType = 3;

		public bool ShowTriggerOptions;

		public bool ResetTrigger;

		public bool HasChanges;
	}
}
