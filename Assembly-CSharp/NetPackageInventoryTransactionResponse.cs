using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

[Preserve]
public class NetPackageInventoryTransactionResponse : NetPackage
{
	public override NetPackageDirection PackageDirection
	{
		get
		{
			return NetPackageDirection.ToClient;
		}
	}

	public NetPackageInventoryTransactionResponse Setup(bool _success, IList<Guid> _keys, IList<ItemStack[]> _inventoriesUpdated)
	{
		if (_keys == null != (_inventoriesUpdated == null) || (_keys != null && _keys.Count != _inventoriesUpdated.Count))
		{
			Log.Error("[NetPackageInventoryTransactionResponse] Mismatch in count of supplied keys and inventories.");
			this.success = false;
			this.keys = null;
			this.inventories = null;
			return this;
		}
		this.success = _success;
		this.keys = _keys;
		this.inventories = _inventoriesUpdated;
		return this;
	}

	public override void read(PooledBinaryReader _br)
	{
		this.success = _br.ReadBoolean();
		int num = _br.ReadInt32();
		List<Guid> list = new List<Guid>(num);
		List<ItemStack[]> list2 = new List<ItemStack[]>(num);
		for (int i = 0; i < num; i++)
		{
			list.Add(StreamUtils.ReadGuid(_br));
			if (_br.ReadBoolean())
			{
				list2.Add(ItemStack.ReadArray(_br));
			}
			else
			{
				list2.Add(null);
			}
		}
		this.keys = list;
		this.inventories = list2;
	}

	public override void write(PooledBinaryWriter _bw)
	{
		base.write(_bw);
		if (this.keys == null || this.inventories == null)
		{
			_bw.Write(false);
			_bw.Write(0);
			return;
		}
		_bw.Write(this.success);
		_bw.Write(this.keys.Count);
		for (int i = 0; i < this.keys.Count; i++)
		{
			StreamUtils.Write(_bw, this.keys[i]);
			bool flag = this.inventories[i] != null;
			_bw.Write(flag);
			if (flag)
			{
				ItemStack.WriteArray(_bw, this.inventories[i]);
			}
		}
	}

	public override void ProcessPackage(World _world, GameManager _callbacks)
	{
	}

	public override int GetLength()
	{
		return 0;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool success;

	[PublicizedFrom(EAccessModifier.Private)]
	public IList<Guid> keys;

	[PublicizedFrom(EAccessModifier.Private)]
	public IList<ItemStack[]> inventories;
}
