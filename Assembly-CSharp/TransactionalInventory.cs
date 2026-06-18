using System;
using System.IO;
using System.Runtime.CompilerServices;

public class TransactionalInventory : ILockTarget
{
	public bool DataReadyLocally
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public Guid Key
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public int Hash
	{
		get
		{
			if (this.hashDirty)
			{
				this.hash = this.items.GetArrayHashCode();
				this.hashDirty = false;
			}
			return this.hash;
		}
	}

	public TransactionalInventory.KeyHashPair KeyHash
	{
		get
		{
			return new TransactionalInventory.KeyHashPair(this.Key, this.Hash);
		}
	}

	public bool NeedsSaving
	{
		get
		{
			return this.needsSaving;
		}
	}

	public int Length
	{
		get
		{
			return this.items.Length;
		}
	}

	public LockTargetType LockTargetType
	{
		get
		{
			return LockTargetType.TransactionalInventory;
		}
	}

	public bool IsSharedLock(ushort _channel)
	{
		return false;
	}

	public TransactionalInventory(Guid _key, ItemStack[] _items, Guid _managerToken)
	{
		this.DataReadyLocally = true;
		this.items = _items;
		this.Key = _key;
		InventoryManager.Instance.ValidateToken(_managerToken);
	}

	public ItemStack GetItemReadonly(int _slot)
	{
		if (_slot > this.items.Length)
		{
			return null;
		}
		return this.items[_slot].Clone();
	}

	public ItemStack[] GetItemsReadonly()
	{
		return ItemStack.Clone(this.items);
	}

	public void ResizeInventory(int _size, Guid _managerToken)
	{
		if (!InventoryManager.Instance.ValidateToken(_managerToken))
		{
			return;
		}
		if (_size < this.items.Length)
		{
			Log.Warning("[TransactionalInventory] Inventory was resized to a smaller size, items may be lost.");
		}
		Array.Copy(this.items, 0, this.items, 0, (_size > this.items.Length) ? this.items.Length : _size);
	}

	public void UpdateInventory(ItemStack[] _items, Guid _managerToken)
	{
		if (!InventoryManager.Instance.ValidateToken(_managerToken))
		{
			return;
		}
		if (_items != null)
		{
			if (this.items == null)
			{
				this.items = ItemStack.Clone(_items);
			}
			else if (_items.Length != this.items.Length)
			{
				this.ResizeInventory(_items.Length, _managerToken);
			}
			else
			{
				Array.Copy(_items, this.items, this.items.Length);
			}
		}
		this.hashDirty = true;
		this.DataReadyLocally = true;
		Action action = this.inventoryRequestComplete;
		if (action != null)
		{
			action();
		}
		this.inventoryRequestComplete = null;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool CheckInventoryReady()
	{
		if (!this.DataReadyLocally)
		{
			Log.Warning("[TransactionalInventory] Data has not yet been fetched properly.");
			return false;
		}
		if (!LockManager.Instance.IsLockedByLocalPlayer(this, 0))
		{
			Log.Warning("[TransactionalInventory] Local player does not have a lock for this inventory.");
			return false;
		}
		return true;
	}

	public bool IsEmpty()
	{
		if (!this.CheckInventoryReady())
		{
			return false;
		}
		for (int i = 0; i < this.items.Length; i++)
		{
			if (!this.items[i].IsEmpty())
			{
				return false;
			}
		}
		return true;
	}

	public int HasItem(ItemValue _itemValue)
	{
		if (!this.CheckInventoryReady())
		{
			return -1;
		}
		if (_itemValue == null)
		{
			return -1;
		}
		for (int i = 0; i < this.items.Length; i++)
		{
			if (_itemValue.ItemClass.Equals(this.items[i].itemValue.ItemClass))
			{
				return i;
			}
		}
		return -1;
	}

	public bool TrySetItem(int _idx, ItemStack _stack)
	{
		if (!this.CheckInventoryReady())
		{
			return false;
		}
		if (_idx < 0 || _idx >= this.items.Length)
		{
			Log.Warning("[TransactionalInventory] Supplied index was out of bounds.");
			return false;
		}
		InventoryTransaction inventoryTransaction = new InventoryTransaction().SetStackAbsolute(this, _stack, _idx);
		InventoryManager.Instance.TransactionRequestLocal(inventoryTransaction);
		return true;
	}

	public bool TryAddItem(ItemStack _stack)
	{
		if (!this.CheckInventoryReady())
		{
			return false;
		}
		for (int i = 0; i < this.items.Length; i++)
		{
			if (this.items[i].IsEmpty())
			{
				InventoryTransaction inventoryTransaction = new InventoryTransaction().SetStackAbsolute(this, _stack, i);
				InventoryManager.Instance.TransactionRequestLocal(inventoryTransaction);
				return true;
			}
		}
		return false;
	}

	[return: TupleElementNames(new string[] { "success", "stacked", "total" })]
	public ValueTuple<bool, int, int> TryStackItem(int _startIndex, ItemStack _stack)
	{
		if (!this.CheckInventoryReady())
		{
			return new ValueTuple<bool, int, int>(false, 0, _stack.count);
		}
		int count = _stack.count;
		InventoryTransaction inventoryTransaction = new InventoryTransaction();
		for (int i = _startIndex; i < this.items.Length; i++)
		{
			int num;
			if (!this.items[i].CanStackPartlyWith(_stack, out num))
			{
				return new ValueTuple<bool, int, int>(false, 0, count);
			}
			ItemStack itemStack = _stack.Clone();
			itemStack.count = num;
			_stack.count -= num;
			inventoryTransaction.SetStackRelative(this, itemStack, i);
			if (_stack.count == 0)
			{
				break;
			}
		}
		InventoryManager.Instance.TransactionRequestLocal(inventoryTransaction);
		return new ValueTuple<bool, int, int>(true, count - _stack.count, count);
	}

	public bool TryClearItem(int _idx)
	{
		return this.TrySetItem(_idx, ItemStack.Empty);
	}

	public bool ClearAllItems()
	{
		throw new NotImplementedException();
	}

	public void Rollback()
	{
		throw new NotImplementedException();
	}

	public void StartTransaction(Guid _managerToken)
	{
		if (!InventoryManager.Instance.ValidateToken(_managerToken))
		{
			return;
		}
		this.buffer = this.GetItemsReadonly();
	}

	public void FinalizeTransaction(bool _success, Guid _managerToken)
	{
		if (!InventoryManager.Instance.ValidateToken(_managerToken))
		{
			return;
		}
		if (_success)
		{
			this.UpdateInventory(this.buffer, _managerToken);
			this.needsSaving = true;
		}
		this.buffer = null;
	}

	public bool ProcessOperation(InventoryOperation _operation, Guid _managerToken)
	{
		if (!InventoryManager.Instance.ValidateToken(_managerToken))
		{
			return false;
		}
		switch (_operation.Operation)
		{
		case InventoryOperation.EnumOperation.SetAbsolute:
			return this.ProcessSetAbsolute(_operation);
		case InventoryOperation.EnumOperation.SetRelative:
			return this.ProcessSetRelative(_operation);
		case InventoryOperation.EnumOperation.SetAll:
			return this.ProcessSetAll(_operation);
		default:
			return false;
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ProcessSetAbsolute(InventoryOperation _op)
	{
		if (this.buffer == null || _op.Operation != InventoryOperation.EnumOperation.SetAbsolute)
		{
			return false;
		}
		if (_op.Index >= this.buffer.Length)
		{
			return false;
		}
		if (_op.Stack.count < 0)
		{
			return false;
		}
		this.buffer[_op.Index] = _op.Stack.Clone();
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ProcessSetRelative(InventoryOperation _op)
	{
		if (this.buffer == null || _op.Operation != InventoryOperation.EnumOperation.SetRelative)
		{
			return false;
		}
		if (_op.Index >= this.buffer.Length)
		{
			return false;
		}
		ItemStack itemStack = this.buffer[_op.Index];
		if (_op.Stack.count > 0)
		{
			if (!_op.Stack.CanStackWith(itemStack, false))
			{
				return false;
			}
			this.buffer[_op.Index].count = _op.Stack.count + itemStack.count;
		}
		else
		{
			if (!_op.Stack.itemValue.Equals(itemStack.itemValue))
			{
				return false;
			}
			int num = _op.Stack.count + itemStack.count;
			if (num == 0)
			{
				this.buffer[_op.Index] = ItemStack.Empty;
			}
			else
			{
				if (num <= 0)
				{
					return false;
				}
				this.buffer[_op.Index].count = _op.Stack.count + itemStack.count;
			}
		}
		return true;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public bool ProcessSetAll(InventoryOperation _op)
	{
		if (this.buffer == null || _op.NewStacks == null || _op.Operation != InventoryOperation.EnumOperation.SetAll)
		{
			return false;
		}
		if (_op.NewStacks.Length != this.buffer.Length)
		{
			return false;
		}
		for (int i = 0; i < this.buffer.Length; i++)
		{
			if (_op.Stack.count < 0)
			{
				return false;
			}
			this.buffer[i] = _op.NewStacks[i].Clone();
		}
		return true;
	}

	public bool CanLockOnServer(int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
		return true;
	}

	public bool CanLockLocally(ILockContext _context, ushort _channel)
	{
		return true;
	}

	public void OnLockedServer(bool _success, int _lockingPlayerID, ILockContext _context, ushort _channel)
	{
	}

	public void OnLockedLocal(bool _success, ILockContext _context, ushort _channel)
	{
		throw new NotImplementedException("[TransactionalInventory] This class is designed to be locked with the custom UI action RequestInventoryAndShowUI");
	}

	public void OnUnlockedServer(int _unlockingPlayerId, ushort _channel)
	{
	}

	public void RequestInventoryAndShowUI(bool _success, Action _showUi)
	{
		if (!_success)
		{
			return;
		}
		if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
		{
			this.DataReadyLocally = false;
			this.inventoryRequestComplete = _showUi;
			InventoryManager.Instance.RequestInventoryFromServer(this.KeyHash);
			return;
		}
		_showUi();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] items;

	[PublicizedFrom(EAccessModifier.Private)]
	public Action inventoryRequestComplete;

	[PublicizedFrom(EAccessModifier.Private)]
	public ItemStack[] buffer;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool hashDirty = true;

	[PublicizedFrom(EAccessModifier.Private)]
	public int hash;

	[PublicizedFrom(EAccessModifier.Private)]
	public bool needsSaving = true;

	public struct KeyHashPair
	{
		public KeyHashPair(Guid _key, int _hash)
		{
			this.Key = _key;
			this.Hash = _hash;
		}

		public static TransactionalInventory.KeyHashPair Read(BinaryReader _br)
		{
			Guid guid = StreamUtils.ReadGuid(_br);
			int num = _br.ReadInt32();
			return new TransactionalInventory.KeyHashPair(guid, num);
		}

		public void Write(BinaryWriter _bw)
		{
			StreamUtils.Write(_bw, this.Key);
			_bw.Write(this.Hash);
		}

		public Guid Key;

		public int Hash;
	}
}
