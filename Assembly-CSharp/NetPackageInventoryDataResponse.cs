using System;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageInventoryDataResponse : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageInventoryDataResponse Setup(bool _success, string _errorMsg, Guid _inventoryKey, ItemStack[] _items, Guid _managerToken)
	{
		this.success = _success;
		this.errorMsg = _errorMsg;
		this.inventoryKey = _inventoryKey;
		this.items = _items;
		this.managerToken = _managerToken;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.success = _br.ReadBoolean();
		this.errorMsg = _br.ReadString();
		this.inventoryKey = StreamUtils.ReadGuid(_br);
		this.items = ItemStack.ReadArray(_br);
		this.managerToken = StreamUtils.ReadGuid(_br);
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		_bw.Write(this.success);
		_bw.Write(this.errorMsg);
		StreamUtils.Write(_bw, this.inventoryKey);
		ItemStack.WriteArray(_bw, this.items);
		StreamUtils.Write(_bw, this.managerToken);
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
		if (!this.success)
		{
			Log.Warning("Error during transaction request: " + this.errorMsg);
			return;
		}
		TransactionalInventory transactionalInventory;
		if (InventoryManager.Instance.TryGetTransactionalInventory(this.inventoryKey, out transactionalInventory))
		{
			transactionalInventory.UpdateInventory(this.items, this.managerToken);
			return;
		}
		Log.Error(string.Format("{0} Could not find inventory with key: {1}", "NetPackageInventoryDataResponse", this.inventoryKey));
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool success;

	[PublicizedFrom(EAccessModifier.Private)]
	public string errorMsg = string.Empty;

	[PublicizedFrom(EAccessModifier.Private)]
	public Guid inventoryKey;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;

	[PublicizedFrom(EAccessModifier.Private)]
	public Guid managerToken;
}
