using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class vp_Inventory : MonoBehaviour
{
	public Transform Transform
	{
		[PublicizedFrom(EAccessModifier.Protected)]
		get
		{
			if (this.m_Transform == null)
			{
				this.m_Transform = base.transform;
			}
			return this.m_Transform;
		}
	}

	public List<vp_UnitBankInstance> UnitBankInstances
	{
		get
		{
			return this.m_UnitBankInstances;
		}
	}

	public List<vp_UnitBankInstance> InternalUnitBanks
	{
		get
		{
			return this.m_InternalUnitBanks;
		}
	}

	public float TotalSpace
	{
		get
		{
			return Mathf.Max(-1f, this.m_TotalSpace);
		}
		set
		{
			this.m_TotalSpace = value;
		}
	}

	public float UsedSpace
	{
		get
		{
			return Mathf.Max(0f, this.m_UsedSpace);
		}
		set
		{
			this.m_UsedSpace = Mathf.Max(0f, value);
		}
	}

	[HideInInspector]
	public float RemainingSpace
	{
		get
		{
			return Mathf.Max(0f, this.TotalSpace - this.UsedSpace);
		}
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Awake()
	{
		this.SaveInitialState();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void Start()
	{
		this.Refresh();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		vp_TargetEventReturn<vp_Inventory>.Register(this.Transform, "GetInventory", new Func<vp_Inventory>(this.GetInventory));
		vp_TargetEventReturn<vp_ItemType, int, bool>.Register(this.Transform, "TryGiveItem", new Func<vp_ItemType, int, bool>(this.TryGiveItem));
		vp_TargetEventReturn<vp_ItemType, int, bool>.Register(this.Transform, "TryGiveItems", new Func<vp_ItemType, int, bool>(this.TryGiveItems));
		vp_TargetEventReturn<vp_UnitBankType, int, int, bool>.Register(this.Transform, "TryGiveUnitBank", new Func<vp_UnitBankType, int, int, bool>(this.TryGiveUnitBank));
		vp_TargetEventReturn<vp_UnitType, int, bool>.Register(this.Transform, "TryGiveUnits", new Func<vp_UnitType, int, bool>(this.TryGiveUnits));
		vp_TargetEventReturn<vp_UnitBankType, int, int, bool>.Register(this.Transform, "TryDeduct", new Func<vp_UnitBankType, int, int, bool>(this.TryDeduct));
		vp_TargetEventReturn<vp_ItemType, int>.Register(this.Transform, "GetItemCount", new Func<vp_ItemType, int>(this.GetItemCount));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
		vp_TargetEventReturn<vp_ItemType, int, bool>.Unregister(this.Transform, "TryGiveItem", new Func<vp_ItemType, int, bool>(this.TryGiveItem));
		vp_TargetEventReturn<vp_ItemType, int, bool>.Unregister(this.Transform, "TryGiveItems", new Func<vp_ItemType, int, bool>(this.TryGiveItems));
		vp_TargetEventReturn<vp_UnitBankType, int, int, bool>.Unregister(this.Transform, "TryGiveUnitBank", new Func<vp_UnitBankType, int, int, bool>(this.TryGiveUnitBank));
		vp_TargetEventReturn<vp_UnitType, int, bool>.Unregister(this.Transform, "TryGiveUnits", new Func<vp_UnitType, int, bool>(this.TryGiveUnits));
		vp_TargetEventReturn<vp_UnitBankType, int, int, bool>.Unregister(this.Transform, "TryDeduct", new Func<vp_UnitBankType, int, int, bool>(this.TryDeduct));
		vp_TargetEventReturn<vp_ItemType, int>.Unregister(this.Transform, "GetItemCount", new Func<vp_ItemType, int>(this.GetItemCount));
		vp_TargetEventReturn<vp_Inventory>.Unregister(this.Transform, "HasInventory", new Func<vp_Inventory>(this.GetInventory));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual vp_Inventory GetInventory()
	{
		return this;
	}

	public virtual bool TryGiveItems(vp_ItemType type, int amount)
	{
		bool flag = false;
		while (amount > 0)
		{
			if (this.TryGiveItem(type, 0))
			{
				flag = true;
			}
			amount--;
		}
		return flag;
	}

	public virtual bool TryGiveItem(vp_ItemType itemType, int id)
	{
		if (itemType == null)
		{
			Debug.LogError("Error (" + vp_Utility.GetErrorLocation(2, false) + ") Item type was null.");
			return false;
		}
		vp_UnitType vp_UnitType = itemType as vp_UnitType;
		if (vp_UnitType != null)
		{
			return this.TryGiveUnits(vp_UnitType, id);
		}
		vp_UnitBankType vp_UnitBankType = itemType as vp_UnitBankType;
		if (vp_UnitBankType != null)
		{
			return this.TryGiveUnitBank(vp_UnitBankType, vp_UnitBankType.Capacity, id);
		}
		if (this.CapsEnabled)
		{
			int itemCap = this.GetItemCap(itemType);
			if (itemCap != -1 && this.GetItemCount(itemType) >= itemCap)
			{
				return false;
			}
		}
		if (this.SpaceEnabled && this.UsedSpace + itemType.Space > this.TotalSpace)
		{
			return false;
		}
		this.DoAddItem(itemType, id);
		return true;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DoAddItem(vp_ItemType type, int id)
	{
		this.ItemInstances.Add(new vp_ItemInstance(type, id));
		if (this.SpaceEnabled)
		{
			this.m_UsedSpace += type.Space;
		}
		this.m_FirstItemsDirty = true;
		this.m_ItemDictionaryDirty = true;
		this.SetDirty();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DoRemoveItem(vp_ItemInstance item)
	{
		if (item is vp_UnitBankInstance)
		{
			this.DoRemoveUnitBank(item as vp_UnitBankInstance);
			return;
		}
		this.ItemInstances.Remove(item);
		this.m_FirstItemsDirty = true;
		this.m_ItemDictionaryDirty = true;
		if (this.SpaceEnabled)
		{
			this.m_UsedSpace = Mathf.Max(0f, this.m_UsedSpace - item.Type.Space);
		}
		this.SetDirty();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DoAddUnitBank(vp_UnitBankType unitBankType, int id, int unitsLoaded)
	{
		vp_UnitBankInstance vp_UnitBankInstance = new vp_UnitBankInstance(unitBankType, id, this);
		this.m_UnitBankInstances.Add(vp_UnitBankInstance);
		this.m_FirstItemsDirty = true;
		this.m_ItemDictionaryDirty = true;
		if (this.SpaceEnabled && !vp_UnitBankInstance.IsInternal)
		{
			this.m_UsedSpace += unitBankType.Space;
		}
		vp_UnitBankInstance.TryGiveUnits(unitsLoaded);
		if (this.SpaceEnabled && !vp_UnitBankInstance.IsInternal && this.SpaceMode == vp_Inventory.Mode.Weight && unitBankType.Unit != null)
		{
			this.m_UsedSpace += unitBankType.Unit.Space * (float)vp_UnitBankInstance.Count;
		}
		this.SetDirty();
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void DoRemoveUnitBank(vp_UnitBankInstance bank)
	{
		if (!bank.IsInternal)
		{
			this.m_UnitBankInstances.RemoveAt(this.m_UnitBankInstances.IndexOf(bank));
			this.m_FirstItemsDirty = true;
			this.m_ItemDictionaryDirty = true;
			if (this.SpaceEnabled)
			{
				this.m_UsedSpace -= bank.Type.Space;
				if (this.SpaceMode == vp_Inventory.Mode.Weight)
				{
					this.m_UsedSpace -= bank.UnitType.Space * (float)bank.Count;
				}
			}
		}
		else
		{
			this.m_InternalUnitBanks.RemoveAt(this.m_InternalUnitBanks.IndexOf(bank));
		}
		this.SetDirty();
	}

	public virtual bool DoAddUnits(vp_UnitBankInstance bank, int amount)
	{
		return bank.DoAddUnits(amount);
	}

	public virtual bool DoRemoveUnits(vp_UnitBankInstance bank, int amount)
	{
		return bank.DoRemoveUnits(amount);
	}

	public virtual bool TryGiveUnits(vp_UnitType unitType, int amount)
	{
		return this.GetItemCap(unitType) != 0 && this.TryGiveUnits(this.GetInternalUnitBank(unitType), amount);
	}

	public virtual bool TryGiveUnits(vp_UnitBankInstance bank, int amount)
	{
		if (bank == null)
		{
			return false;
		}
		amount = Mathf.Max(0, amount);
		if (this.SpaceEnabled && (bank.IsInternal || this.SpaceMode == vp_Inventory.Mode.Weight) && this.RemainingSpace < (float)amount * bank.UnitType.Space)
		{
			amount = (int)(this.RemainingSpace / bank.UnitType.Space);
			return this.DoAddUnits(bank, amount);
		}
		return this.DoAddUnits(bank, amount);
	}

	public virtual bool TryRemoveUnits(vp_UnitType unitType, int amount)
	{
		vp_UnitBankInstance internalUnitBank = this.GetInternalUnitBank(unitType);
		return internalUnitBank != null && this.DoRemoveUnits(internalUnitBank, amount);
	}

	public virtual bool TryGiveUnitBank(vp_UnitBankType unitBankType, int unitsLoaded, int id)
	{
		if (unitBankType == null)
		{
			Debug.LogError("Error (" + vp_Utility.GetErrorLocation(1, false) + ") 'unitBankType' was null.");
			return false;
		}
		if (this.CapsEnabled)
		{
			int itemCap = this.GetItemCap(unitBankType);
			if (itemCap != -1 && this.GetItemCount(unitBankType) >= itemCap)
			{
				return false;
			}
			if (unitBankType.Capacity != -1)
			{
				unitsLoaded = Mathf.Min(unitsLoaded, unitBankType.Capacity);
			}
		}
		if (this.SpaceEnabled)
		{
			vp_Inventory.Mode spaceMode = this.SpaceMode;
			if (spaceMode != vp_Inventory.Mode.Weight)
			{
				if (spaceMode == vp_Inventory.Mode.Volume)
				{
					if (this.UsedSpace + unitBankType.Space > this.TotalSpace)
					{
						return false;
					}
				}
			}
			else
			{
				if (unitBankType.Unit == null)
				{
					Debug.LogError("Error (vp_Inventory) UnitBank item type " + ((unitBankType != null) ? unitBankType.ToString() : null) + " can't be added because its unit type has not been set.");
					return false;
				}
				if (this.UsedSpace + unitBankType.Space + unitBankType.Unit.Space * (float)unitsLoaded > this.TotalSpace)
				{
					return false;
				}
			}
		}
		this.DoAddUnitBank(unitBankType, id, unitsLoaded);
		return true;
	}

	public virtual bool TryRemoveItems(vp_ItemType type, int amount)
	{
		bool flag = false;
		while (amount > 0)
		{
			if (this.TryRemoveItem(type, -1))
			{
				flag = true;
			}
			amount--;
		}
		return flag;
	}

	public virtual bool TryRemoveItem(vp_ItemType type, int id)
	{
		return this.TryRemoveItem(this.GetItem(type, id));
	}

	public virtual bool TryRemoveItem(vp_ItemInstance item)
	{
		if (item == null)
		{
			return false;
		}
		this.DoRemoveItem(item);
		this.SetDirty();
		return true;
	}

	public virtual bool TryRemoveUnitBanks(vp_UnitBankType type, int amount)
	{
		bool flag = false;
		while (amount > 0)
		{
			if (this.TryRemoveUnitBank(type, -1))
			{
				flag = true;
			}
			amount--;
		}
		return flag;
	}

	public virtual bool TryRemoveUnitBank(vp_UnitBankType type, int id)
	{
		return this.TryRemoveUnitBank(this.GetItem(type, id) as vp_UnitBankInstance);
	}

	public virtual bool TryRemoveUnitBank(vp_UnitBankInstance unitBank)
	{
		if (unitBank == null)
		{
			return false;
		}
		this.DoRemoveUnitBank(unitBank);
		this.SetDirty();
		return true;
	}

	public virtual bool TryReload(vp_ItemType itemType, int unitBankId)
	{
		return this.TryReload(this.GetItem(itemType, unitBankId) as vp_UnitBankInstance, -1);
	}

	public virtual bool TryReload(vp_ItemType itemType, int unitBankId, int amount)
	{
		return this.TryReload(this.GetItem(itemType, unitBankId) as vp_UnitBankInstance, amount);
	}

	public virtual bool TryReload(vp_UnitBankInstance bank)
	{
		return this.TryReload(bank, -1);
	}

	public virtual bool TryReload(vp_UnitBankInstance bank, int amount)
	{
		if (bank == null || bank.IsInternal || bank.ID == -1)
		{
			Debug.LogWarning("Warning (" + vp_Utility.GetErrorLocation(1, false) + ") 'TryReloadUnitBank' could not identify a target item. If you are trying to add units to the main inventory please instead use 'TryGiveUnits'.");
			return false;
		}
		int count = bank.Count;
		if (count >= bank.Capacity)
		{
			return false;
		}
		int unitCount = this.GetUnitCount(bank.UnitType);
		if (unitCount < 1)
		{
			return false;
		}
		if (amount == -1)
		{
			amount = bank.Capacity;
		}
		this.TryRemoveUnits(bank.UnitType, amount);
		int num = Mathf.Max(0, unitCount - this.GetUnitCount(bank.UnitType));
		if (!this.DoAddUnits(bank, num))
		{
			return false;
		}
		int num2 = Mathf.Max(0, bank.Count - count);
		if (num2 < 1)
		{
			return false;
		}
		if (num2 > 0 && num2 < num)
		{
			this.TryGiveUnits(bank.UnitType, num - num2);
		}
		return true;
	}

	public virtual bool TryDeduct(vp_UnitBankType unitBankType, int unitBankId, int amount)
	{
		vp_UnitBankInstance vp_UnitBankInstance = ((unitBankId < 1) ? (this.GetItem(unitBankType) as vp_UnitBankInstance) : (this.GetItem(unitBankType, unitBankId) as vp_UnitBankInstance));
		if (vp_UnitBankInstance == null)
		{
			return false;
		}
		if (!this.DoRemoveUnits(vp_UnitBankInstance, amount))
		{
			return false;
		}
		if (vp_UnitBankInstance.Count <= 0 && (vp_UnitBankInstance.Type as vp_UnitBankType).RemoveWhenDepleted)
		{
			this.DoRemoveUnitBank(vp_UnitBankInstance);
		}
		return true;
	}

	public virtual vp_ItemInstance GetItem(vp_ItemType itemType)
	{
		if (this.m_FirstItemsDirty)
		{
			this.m_FirstItemsOfType.Clear();
			foreach (vp_ItemInstance vp_ItemInstance in this.ItemInstances)
			{
				if (vp_ItemInstance != null && !this.m_FirstItemsOfType.ContainsKey(vp_ItemInstance.Type))
				{
					this.m_FirstItemsOfType.Add(vp_ItemInstance.Type, vp_ItemInstance);
				}
			}
			foreach (vp_UnitBankInstance vp_UnitBankInstance in this.UnitBankInstances)
			{
				if (vp_UnitBankInstance != null && !this.m_FirstItemsOfType.ContainsKey(vp_UnitBankInstance.Type))
				{
					this.m_FirstItemsOfType.Add(vp_UnitBankInstance.Type, vp_UnitBankInstance);
				}
			}
			this.m_FirstItemsDirty = false;
		}
		if (itemType == null || !this.m_FirstItemsOfType.TryGetValue(itemType, out this.m_GetFirstItemInstanceResult))
		{
			return null;
		}
		if (this.m_GetFirstItemInstanceResult == null)
		{
			this.m_FirstItemsDirty = true;
			return this.GetItem(itemType);
		}
		return this.m_GetFirstItemInstanceResult;
	}

	public vp_ItemInstance GetItem(vp_ItemType itemType, int id)
	{
		if (itemType == null)
		{
			Debug.LogError("Error (" + vp_Utility.GetErrorLocation(1, true) + ") Sent a null itemType to 'GetItem'.");
			return null;
		}
		if (id < 1)
		{
			return this.GetItem(itemType);
		}
		if (this.m_ItemDictionaryDirty)
		{
			this.m_ItemDictionary.Clear();
			this.m_ItemDictionaryDirty = false;
		}
		if (!this.m_ItemDictionary.TryGetValue(id, out this.m_GetItemResult))
		{
			this.m_GetItemResult = this.GetItemFromList(itemType, id);
			if (this.m_GetItemResult != null && id > 0)
			{
				this.m_ItemDictionary.Add(id, this.m_GetItemResult);
			}
		}
		else if (this.m_GetItemResult != null)
		{
			if (this.m_GetItemResult.Type != itemType)
			{
				Debug.LogWarning("Warning: (vp_Inventory) Player has vp_FPWeapons with identical, non-zero vp_ItemIdentifier IDs! This is much slower than using zero or differing IDs.");
				this.m_GetItemResult = this.GetItemFromList(itemType, id);
			}
		}
		else
		{
			this.m_ItemDictionary.Remove(id);
			this.GetItem(itemType, id);
		}
		return this.m_GetItemResult;
	}

	public virtual vp_ItemInstance GetItem(string itemTypeName)
	{
		for (int i = 0; i < this.InternalUnitBanks.Count; i++)
		{
			if (this.InternalUnitBanks[i].UnitType.name == itemTypeName)
			{
				return this.InternalUnitBanks[i];
			}
		}
		for (int j = 0; j < this.m_UnitBankInstances.Count; j++)
		{
			if (this.m_UnitBankInstances[j].Type.name == itemTypeName)
			{
				return this.m_UnitBankInstances[j];
			}
		}
		for (int k = 0; k < this.ItemInstances.Count; k++)
		{
			if (this.ItemInstances[k].Type.name == itemTypeName)
			{
				return this.ItemInstances[k];
			}
		}
		return null;
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual vp_ItemInstance GetItemFromList(vp_ItemType itemType, int id = -1)
	{
		for (int i = 0; i < this.m_UnitBankInstances.Count; i++)
		{
			if (!(this.m_UnitBankInstances[i].Type != itemType) && (id == -1 || this.m_UnitBankInstances[i].ID == id))
			{
				return this.m_UnitBankInstances[i];
			}
		}
		for (int j = 0; j < this.ItemInstances.Count; j++)
		{
			if (!(this.ItemInstances[j].Type != itemType) && (id == -1 || this.ItemInstances[j].ID == id))
			{
				return this.ItemInstances[j];
			}
		}
		return null;
	}

	public virtual bool HaveItem(vp_ItemType itemType, int id = -1)
	{
		return !(itemType == null) && this.GetItem(itemType, id) != null;
	}

	public virtual vp_UnitBankInstance GetInternalUnitBank(vp_UnitType unitType)
	{
		for (int i = 0; i < this.m_InternalUnitBanks.Count; i++)
		{
			if (!(this.m_InternalUnitBanks[i].GetType() != typeof(vp_UnitBankInstance)) && !(this.m_InternalUnitBanks[i].Type != null))
			{
				vp_UnitBankInstance vp_UnitBankInstance = this.m_InternalUnitBanks[i];
				if (!(vp_UnitBankInstance.UnitType != unitType))
				{
					return vp_UnitBankInstance;
				}
			}
		}
		this.SetDirty();
		vp_UnitBankInstance vp_UnitBankInstance2 = new vp_UnitBankInstance(unitType, this);
		vp_UnitBankInstance2.Capacity = this.GetItemCap(unitType);
		this.m_InternalUnitBanks.Add(vp_UnitBankInstance2);
		return vp_UnitBankInstance2;
	}

	public virtual bool HaveInternalUnitBank(vp_UnitType unitType)
	{
		for (int i = 0; i < this.m_InternalUnitBanks.Count; i++)
		{
			if (!(this.m_InternalUnitBanks[i].GetType() != typeof(vp_UnitBankInstance)) && !(this.m_InternalUnitBanks[i].Type != null) && !(this.m_InternalUnitBanks[i].UnitType != unitType))
			{
				return true;
			}
		}
		return false;
	}

	public virtual void Refresh()
	{
		for (int i = 0; i < this.m_InternalUnitBanks.Count; i++)
		{
			this.m_InternalUnitBanks[i].Capacity = this.GetItemCap(this.m_InternalUnitBanks[i].UnitType);
		}
		if (!this.SpaceEnabled)
		{
			return;
		}
		this.m_UsedSpace = 0f;
		for (int j = 0; j < this.ItemInstances.Count; j++)
		{
			this.m_UsedSpace += this.ItemInstances[j].Type.Space;
		}
		for (int k = 0; k < this.m_UnitBankInstances.Count; k++)
		{
			vp_Inventory.Mode spaceMode = this.SpaceMode;
			if (spaceMode != vp_Inventory.Mode.Weight)
			{
				if (spaceMode == vp_Inventory.Mode.Volume)
				{
					this.m_UsedSpace += this.m_UnitBankInstances[k].Type.Space;
				}
			}
			else
			{
				this.m_UsedSpace += this.m_UnitBankInstances[k].Type.Space + this.m_UnitBankInstances[k].UnitType.Space * (float)this.m_UnitBankInstances[k].Count;
			}
		}
		for (int l = 0; l < this.m_InternalUnitBanks.Count; l++)
		{
			this.m_UsedSpace += this.m_InternalUnitBanks[l].UnitType.Space * (float)this.m_InternalUnitBanks[l].Count;
		}
	}

	public virtual int GetItemCount(vp_ItemType type)
	{
		vp_UnitType vp_UnitType = type as vp_UnitType;
		if (vp_UnitType != null)
		{
			return this.GetUnitCount(vp_UnitType);
		}
		int num = 0;
		for (int i = 0; i < this.ItemInstances.Count; i++)
		{
			if (this.ItemInstances[i].Type == type)
			{
				num++;
			}
		}
		for (int j = 0; j < this.m_UnitBankInstances.Count; j++)
		{
			if (this.m_UnitBankInstances[j].Type == type)
			{
				num++;
			}
		}
		return num;
	}

	public virtual void SetItemCount(vp_ItemType type, int amount)
	{
		if (type is vp_UnitType)
		{
			this.SetUnitCount((vp_UnitType)type, amount);
			return;
		}
		bool capsEnabled = this.CapsEnabled;
		bool spaceEnabled = this.SpaceEnabled;
		this.CapsEnabled = false;
		this.SpaceEnabled = false;
		int num = amount - this.GetItemCount(type);
		if (num > 0)
		{
			this.TryGiveItems(type, amount);
		}
		else if (num < 0)
		{
			this.TryRemoveItems(type, -amount);
		}
		this.CapsEnabled = capsEnabled;
		this.SpaceEnabled = spaceEnabled;
	}

	public virtual void SetUnitCount(vp_UnitType unitType, int amount)
	{
		this.TrySetUnitCount(this.GetInternalUnitBank(unitType), amount);
	}

	public virtual void SetUnitCount(vp_UnitBankInstance bank, int amount)
	{
		if (bank == null)
		{
			return;
		}
		amount = Mathf.Max(0, amount);
		if (amount == bank.Count)
		{
			return;
		}
		int count = bank.Count;
		if (!this.DoRemoveUnits(bank, bank.Count))
		{
			bank.Count = count;
		}
		if (amount == 0)
		{
			return;
		}
		if (!this.DoAddUnits(bank, amount))
		{
			bank.Count = count;
		}
	}

	public virtual bool TrySetUnitCount(vp_UnitType unitType, int amount)
	{
		return this.TrySetUnitCount(this.GetInternalUnitBank(unitType), amount);
	}

	public virtual bool TrySetUnitCount(vp_UnitBankInstance bank, int amount)
	{
		if (bank == null)
		{
			return false;
		}
		amount = Mathf.Max(0, amount);
		if (amount == bank.Count)
		{
			return true;
		}
		int count = bank.Count;
		if (!this.DoRemoveUnits(bank, bank.Count))
		{
			bank.Count = count;
		}
		if (amount == 0)
		{
			return true;
		}
		if (bank.IsInternal)
		{
			this.m_Result = this.TryGiveUnits(bank.UnitType, amount);
			if (!this.m_Result)
			{
				bank.Count = count;
			}
			return this.m_Result;
		}
		this.m_Result = this.TryGiveUnits(bank, amount);
		if (!this.m_Result)
		{
			bank.Count = count;
		}
		return this.m_Result;
	}

	public virtual int GetItemCap(vp_ItemType type)
	{
		if (!this.CapsEnabled)
		{
			return -1;
		}
		for (int i = 0; i < this.m_ItemCapInstances.Count; i++)
		{
			if (this.m_ItemCapInstances[i].Type == type)
			{
				return this.m_ItemCapInstances[i].Cap;
			}
		}
		if (this.AllowOnlyListed)
		{
			return 0;
		}
		return -1;
	}

	public virtual void SetItemCap(vp_ItemType type, int cap, bool clamp = false)
	{
		this.SetDirty();
		int i = 0;
		while (i < this.m_ItemCapInstances.Count)
		{
			if (this.m_ItemCapInstances[i].Type == type)
			{
				this.m_ItemCapInstances[i].Cap = cap;
				IL_005B:
				if (type is vp_UnitType)
				{
					for (int j = 0; j < this.m_InternalUnitBanks.Count; j++)
					{
						if (this.m_InternalUnitBanks[j].UnitType != null && this.m_InternalUnitBanks[j].UnitType == type)
						{
							this.m_InternalUnitBanks[j].Capacity = cap;
							if (clamp)
							{
								this.m_InternalUnitBanks[j].ClampToCapacity();
							}
						}
					}
					return;
				}
				if (clamp && this.GetItemCount(type) > cap)
				{
					this.TryRemoveItems(type, this.GetItemCount(type) - cap);
				}
				return;
			}
			else
			{
				i++;
			}
		}
		this.m_ItemCapInstances.Add(new vp_Inventory.ItemCap(type, cap));
		goto IL_005B;
	}

	public virtual int GetUnitCount(vp_UnitType unitType)
	{
		vp_UnitBankInstance internalUnitBank = this.GetInternalUnitBank(unitType);
		if (internalUnitBank == null)
		{
			return 0;
		}
		return internalUnitBank.Count;
	}

	public virtual void SaveInitialState()
	{
		for (int i = 0; i < this.InternalUnitBanks.Count; i++)
		{
			this.m_StartItems.Add(new vp_Inventory.StartItemRecord(this.InternalUnitBanks[i].UnitType, 0, this.InternalUnitBanks[i].Count));
		}
		for (int j = 0; j < this.m_UnitBankInstances.Count; j++)
		{
			this.m_StartItems.Add(new vp_Inventory.StartItemRecord(this.m_UnitBankInstances[j].Type, this.m_UnitBankInstances[j].ID, this.m_UnitBankInstances[j].Count));
		}
		for (int k = 0; k < this.ItemInstances.Count; k++)
		{
			this.m_StartItems.Add(new vp_Inventory.StartItemRecord(this.ItemInstances[k].Type, this.ItemInstances[k].ID, 1));
		}
	}

	public virtual void Reset()
	{
		this.Clear();
		for (int i = 0; i < this.m_StartItems.Count; i++)
		{
			if (this.m_StartItems[i].Type.GetType() == typeof(vp_ItemType))
			{
				this.TryGiveItem(this.m_StartItems[i].Type, this.m_StartItems[i].ID);
			}
			else if (this.m_StartItems[i].Type.GetType() == typeof(vp_UnitBankType))
			{
				this.TryGiveUnitBank(this.m_StartItems[i].Type as vp_UnitBankType, this.m_StartItems[i].Amount, this.m_StartItems[i].ID);
			}
			else if (this.m_StartItems[i].Type.GetType() == typeof(vp_UnitType))
			{
				this.TryGiveUnits(this.m_StartItems[i].Type as vp_UnitType, this.m_StartItems[i].Amount);
			}
			else if (this.m_StartItems[i].Type.GetType().BaseType == typeof(vp_ItemType))
			{
				this.TryGiveItem(this.m_StartItems[i].Type, this.m_StartItems[i].ID);
			}
			else if (this.m_StartItems[i].Type.GetType().BaseType == typeof(vp_UnitBankType))
			{
				this.TryGiveUnitBank(this.m_StartItems[i].Type as vp_UnitBankType, this.m_StartItems[i].Amount, this.m_StartItems[i].ID);
			}
			else if (this.m_StartItems[i].Type.GetType().BaseType == typeof(vp_UnitType))
			{
				this.TryGiveUnits(this.m_StartItems[i].Type as vp_UnitType, this.m_StartItems[i].Amount);
			}
		}
	}

	public virtual void Clear()
	{
		for (int i = this.InternalUnitBanks.Count - 1; i > -1; i--)
		{
			this.DoRemoveUnitBank(this.InternalUnitBanks[i]);
		}
		for (int j = this.m_UnitBankInstances.Count - 1; j > -1; j--)
		{
			this.DoRemoveUnitBank(this.m_UnitBankInstances[j]);
		}
		for (int k = this.ItemInstances.Count - 1; k > -1; k--)
		{
			this.DoRemoveItem(this.ItemInstances[k]);
		}
	}

	public virtual void SetTotalSpace(float spaceLimitation)
	{
		this.SetDirty();
		this.TotalSpace = Mathf.Max(0f, spaceLimitation);
	}

	public virtual void SetDirty()
	{
	}

	public virtual void ClearItemDictionaries()
	{
	}

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Inventory.ItemRecordsSection m_ItemRecords;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Inventory.ItemCapsSection m_ItemCaps;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Inventory.SpaceLimitSection m_SpaceLimit;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Transform m_Transform;

	[SerializeField]
	[HideInInspector]
	public List<vp_ItemInstance> ItemInstances = new List<vp_ItemInstance>();

	[SerializeField]
	[HideInInspector]
	public List<vp_Inventory.ItemCap> m_ItemCapInstances = new List<vp_Inventory.ItemCap>();

	[SerializeField]
	[HideInInspector]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<vp_UnitBankInstance> m_UnitBankInstances = new List<vp_UnitBankInstance>();

	[SerializeField]
	[HideInInspector]
	[PublicizedFrom(EAccessModifier.Protected)]
	public List<vp_UnitBankInstance> m_InternalUnitBanks = new List<vp_UnitBankInstance>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int UNLIMITED = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int UNIDENTIFIED = -1;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public const int MAXCAPACITY = -1;

	[SerializeField]
	[HideInInspector]
	public bool CapsEnabled;

	[SerializeField]
	[HideInInspector]
	public bool SpaceEnabled;

	[SerializeField]
	[HideInInspector]
	public vp_Inventory.Mode SpaceMode;

	[SerializeField]
	[HideInInspector]
	public bool AllowOnlyListed;

	[SerializeField]
	[HideInInspector]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_TotalSpace = 100f;

	[SerializeField]
	[HideInInspector]
	[PublicizedFrom(EAccessModifier.Protected)]
	public float m_UsedSpace;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Result;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public List<vp_Inventory.StartItemRecord> m_StartItems = new List<vp_Inventory.StartItemRecord>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_FirstItemsDirty = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<vp_ItemType, vp_ItemInstance> m_FirstItemsOfType = new Dictionary<vp_ItemType, vp_ItemInstance>(100);

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_ItemInstance m_GetFirstItemInstanceResult;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_ItemDictionaryDirty = true;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public Dictionary<int, vp_ItemInstance> m_ItemDictionary = new Dictionary<int, vp_ItemInstance>();

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public vp_ItemInstance m_GetItemResult;

	[Serializable]
	public class ItemRecordsSection
	{
	}

	[Serializable]
	public class ItemCapsSection
	{
	}

	[Serializable]
	public class SpaceLimitSection
	{
	}

	[Serializable]
	public class ItemCap
	{
		public ItemCap(vp_ItemType type, int cap)
		{
			this.Type = type;
			this.Cap = cap;
		}

		[SerializeField]
		public vp_ItemType Type;

		[SerializeField]
		public int Cap;
	}

	public enum Mode
	{
		Weight,
		Volume
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public struct StartItemRecord
	{
		public StartItemRecord(vp_ItemType type, int id, int amount)
		{
			this.Type = type;
			this.ID = id;
			this.Amount = amount;
		}

		public vp_ItemType Type;

		public int ID;

		public int Amount;
	}
}
