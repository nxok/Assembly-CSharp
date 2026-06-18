using System;
using System.Collections.Generic;
using System.IO;

public class InventoryTransaction
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void AddOperation(TransactionalInventory _inventory, InventoryOperation _operation)
	{
		if (!this.InventoryOps.ContainsKey(_inventory))
		{
			this.InventoryOps.Add(_inventory, new InventoryTransaction.OperationData(_inventory.Hash));
		}
		this.InventoryOps[_inventory].Ops.Add(_operation);
	}

	public InventoryTransaction SetStackAbsolute(TransactionalInventory _inventory, ItemStack _stack, int _index)
	{
		if (_inventory == null)
		{
			Log.Warning("[InventoryTransaction] supplied inventory as null.");
			return this;
		}
		if (_stack.count < 0)
		{
			Log.Warning("[InventoryTransaction] stack count cannot be negative when using SetStackAbsolute.");
			return this;
		}
		this.AddOperation(_inventory, InventoryOperation.CreateSetAbsolute(_stack, _index));
		return this;
	}

	public InventoryTransaction SetStackRelative(TransactionalInventory _inventory, ItemStack _stack, int _index)
	{
		if (_inventory == null)
		{
			Log.Warning("[InventoryTransaction] supplied inventory as null.");
			return this;
		}
		this.AddOperation(_inventory, InventoryOperation.CreateSetRelative(_stack, _index));
		return this;
	}

	public InventoryTransaction SetStacksAll(TransactionalInventory _inventory, ItemStack[] _newStacks)
	{
		if (_inventory == null)
		{
			Log.Warning("[InventoryTransaction] supplied inventory as null.");
			return this;
		}
		this.AddOperation(_inventory, InventoryOperation.CreateSetAll(_newStacks));
		return this;
	}

	public bool Apply(Guid managerToken)
	{
		if (this.InventoryOps.Count == 0)
		{
			return false;
		}
		bool flag = true;
		foreach (KeyValuePair<TransactionalInventory, InventoryTransaction.OperationData> keyValuePair in this.InventoryOps)
		{
			TransactionalInventory key = keyValuePair.Key;
			InventoryTransaction.OperationData value = keyValuePair.Value;
			if (key.Hash != value.InitialHash)
			{
				Log.Warning("[InventoryTransaction] transaction failed, an initial hash was out of date.");
				flag = false;
				break;
			}
			key.StartTransaction(managerToken);
		}
		if (flag)
		{
			foreach (KeyValuePair<TransactionalInventory, InventoryTransaction.OperationData> keyValuePair2 in this.InventoryOps)
			{
				TransactionalInventory key2 = keyValuePair2.Key;
				foreach (InventoryOperation inventoryOperation in keyValuePair2.Value.Ops)
				{
					if (!key2.ProcessOperation(inventoryOperation, managerToken))
					{
						flag = false;
						break;
					}
				}
				if (!flag)
				{
					break;
				}
			}
		}
		foreach (KeyValuePair<TransactionalInventory, InventoryTransaction.OperationData> keyValuePair3 in this.InventoryOps)
		{
			TransactionalInventory key3 = keyValuePair3.Key;
			InventoryTransaction.OperationData value2 = keyValuePair3.Value;
			key3.FinalizeTransaction(flag, managerToken);
			if (flag)
			{
				value2.FinalHash = key3.Hash;
			}
		}
		return flag;
	}

	public bool ValidateFinalHashes()
	{
		bool flag = true;
		foreach (KeyValuePair<TransactionalInventory, InventoryTransaction.OperationData> keyValuePair in this.InventoryOps)
		{
			TransactionalInventory key = keyValuePair.Key;
			InventoryTransaction.OperationData value = keyValuePair.Value;
			if (value.FinalHash != key.Hash)
			{
				flag = false;
				Log.Error(string.Format("[{0}] transaction failed, final hash was mismatched. Expected: {1}, Acutal: {2}.", "InventoryTransaction", key.Hash, value.FinalHash));
			}
		}
		return flag;
	}

	public static InventoryTransaction Read(BinaryReader _br)
	{
		InventoryTransaction inventoryTransaction = new InventoryTransaction();
		bool flag = false;
		int num = _br.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			Guid guid = StreamUtils.ReadGuid(_br);
			int num2 = _br.ReadInt32();
			int num3 = _br.ReadInt32();
			InventoryTransaction.OperationData operationData = new InventoryTransaction.OperationData(num2)
			{
				FinalHash = num3
			};
			int num4 = _br.ReadInt32();
			for (int j = 0; j < num4; j++)
			{
				operationData.Ops.Add(InventoryOperation.Read(_br));
			}
			TransactionalInventory transactionalInventory;
			if (!InventoryManager.Instance.TryGetTransactionalInventory(guid, out transactionalInventory))
			{
				Log.Error(string.Format("[{0}] Could not find inventory with key {1}", "InventoryTransaction", guid));
				flag = true;
			}
			else
			{
				inventoryTransaction.InventoryOps[transactionalInventory] = operationData;
			}
		}
		if (flag)
		{
			inventoryTransaction.InventoryOps.Clear();
		}
		return inventoryTransaction;
	}

	public static void Write(BinaryWriter _bw, InventoryTransaction tx)
	{
		if (tx == null || tx.InventoryOps == null || tx.InventoryOps.Count == 0)
		{
			_bw.Write(0);
			return;
		}
		_bw.Write(tx.InventoryOps.Count);
		foreach (KeyValuePair<TransactionalInventory, InventoryTransaction.OperationData> keyValuePair in tx.InventoryOps)
		{
			TransactionalInventory key = keyValuePair.Key;
			InventoryTransaction.OperationData value = keyValuePair.Value;
			StreamUtils.Write(_bw, key.Key);
			_bw.Write(value.InitialHash);
			_bw.Write(value.FinalHash);
			_bw.Write(value.Ops.Count);
			foreach (InventoryOperation inventoryOperation in value.Ops)
			{
				inventoryOperation.Write(_bw);
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Dictionary<TransactionalInventory, InventoryTransaction.OperationData> InventoryOps = new Dictionary<TransactionalInventory, InventoryTransaction.OperationData>();

	[PublicizedFrom(EAccessModifier.Private)]
	public class OperationData
	{
		public OperationData(int _initialHash)
		{
			this.InitialHash = _initialHash;
			this.FinalHash = 0;
			this.Ops = new List<InventoryOperation>();
		}

		public int InitialHash;

		public int FinalHash;

		public List<InventoryOperation> Ops;
	}
}
