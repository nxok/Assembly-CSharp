using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageInventoryTransactionRequest : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageInventoryTransactionRequest Setup(InventoryTransaction _tx)
	{
		this.tx = _tx;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.tx = InventoryTransaction.Read(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		InventoryTransaction.Write(_bw, this.tx);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		InventoryManager.Instance.TransactionRequestServer(this.tx, base.Sender.entityId);
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public InventoryTransaction tx;
}
