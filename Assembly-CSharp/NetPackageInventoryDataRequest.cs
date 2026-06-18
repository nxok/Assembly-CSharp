using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageInventoryDataRequest : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToServer;
		}
	}

	public NetPackageInventoryDataRequest Setup(TransactionalInventory.KeyHashPair _keyHash, Guid _managerToken)
	{
		this.keyHash = _keyHash;
		this.managerToken = _managerToken;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.keyHash = TransactionalInventory.KeyHashPair.Read(_br);
		this.managerToken = StreamUtils.ReadGuid(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		this.keyHash.Write(_bw);
		StreamUtils.Write(_bw, this.managerToken);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		TransactionalInventory transactionalInventory;
		if (!InventoryManager.Instance.TryGetTransactionalInventory(this.keyHash.Key, out transactionalInventory))
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageInventoryDataResponse>().Setup(false, string.Format("{0} Could not find inventory with key: {1}", "NetPackageInventoryDataRequest", this.keyHash.Key), this.keyHash.Key, null, this.managerToken), false, base.Sender.entityId, -1, -1, null, 192, false);
			return;
		}
		if (transactionalInventory.Hash == this.keyHash.Hash)
		{
			SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageInventoryDataResponse>().Setup(true, string.Empty, this.keyHash.Key, null, this.managerToken), false, base.Sender.entityId, -1, -1, null, 192, false);
			return;
		}
		SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageInventoryDataResponse>().Setup(true, string.Empty, this.keyHash.Key, transactionalInventory.GetItemsReadonly(), this.managerToken), false, base.Sender.entityId, -1, -1, null, 192, false);
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public TransactionalInventory.KeyHashPair keyHash;

	[PublicizedFrom(EAccessModifier.Private)]
	public Guid managerToken;
}
