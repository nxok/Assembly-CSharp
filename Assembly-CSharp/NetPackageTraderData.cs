using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageTraderData : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageTraderData Setup(ITrader _trader)
	{
		EntityTrader entityTrader = _trader as EntityTrader;
		if (entityTrader != null)
		{
			return this.Setup(entityTrader);
		}
		TileEntityVendingMachine tileEntityVendingMachine = _trader as TileEntityVendingMachine;
		if (tileEntityVendingMachine != null)
		{
			return this.Setup(tileEntityVendingMachine);
		}
		this.entityId = -1;
		this.tePosition = Vector3i.zero;
		this.traderData = null;
		return this;
	}

	public NetPackageTraderData Setup(EntityTrader _entityTrader)
	{
		this.entityId = _entityTrader.entityId;
		this.tePosition = Vector3i.zero;
		TraderData traderData = _entityTrader.TraderData;
		this.traderData = ((traderData != null) ? traderData.Clone() : null);
		return this;
	}

	public NetPackageTraderData Setup(TileEntityVendingMachine _vendingMachine)
	{
		this.entityId = -1;
		this.tePosition = _vendingMachine.ToWorldPos();
		TraderData traderData = _vendingMachine.TraderData;
		this.traderData = ((traderData != null) ? traderData.Clone() : null);
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		bool flag = _br.ReadBoolean();
		this.entityId = -1;
		this.tePosition = Vector3i.zero;
		if (flag)
		{
			this.entityId = _br.ReadInt32();
		}
		else
		{
			this.tePosition = StreamUtils.ReadVector3i(_br);
		}
		if (_br.ReadBoolean())
		{
			if (this.traderData == null)
			{
				this.traderData = new TraderData();
			}
			this.traderData.Read(_br);
			return;
		}
		this.traderData = null;
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		bool flag = this.entityId != -1;
		_bw.Write(flag);
		if (flag)
		{
			_bw.Write(this.entityId);
		}
		else
		{
			StreamUtils.Write(_bw, this.tePosition);
		}
		_bw.Write(this.traderData != null);
		if (this.traderData != null)
		{
			this.traderData.Write(_bw);
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (_world == null || this.traderData == null || !SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
		{
			return;
		}
		if (this.entityId != -1)
		{
			EntityTrader entityTrader = _world.GetEntity(this.entityId) as EntityTrader;
			if (entityTrader == null)
			{
				return;
			}
			entityTrader.TraderData.CopyFrom(this.traderData);
			return;
		}
		else
		{
			TileEntityVendingMachine tileEntityVendingMachine = _world.GetTileEntity(this.tePosition) as TileEntityVendingMachine;
			if (tileEntityVendingMachine == null)
			{
				return;
			}
			tileEntityVendingMachine.TraderData.CopyFrom(this.traderData);
			tileEntityVendingMachine.NotifyListeners();
			return;
		}
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public int entityId = -1;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3i tePosition;

	[PublicizedFrom(EAccessModifier.Private)]
	public TraderData traderData;
}
