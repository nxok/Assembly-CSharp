using System;
using UnityEngine;

[Serializable]
public class vp_UnitBankInstance : vp_ItemInstance
{
	public int Capacity
	{
		get
		{
			if (this.Type != null)
			{
				this.m_Capacity = ((vp_UnitBankType)this.Type).Capacity;
			}
			return this.m_Capacity;
		}
		set
		{
			this.m_Capacity = Mathf.Max(-1, value);
		}
	}

	public vp_UnitBankInstance(vp_UnitBankType unitBankType, int id, vp_Inventory inventory)
		: base(unitBankType, id)
	{
		this.UnitType = unitBankType.Unit;
		this.m_Inventory = inventory;
	}

	public vp_UnitBankInstance(vp_UnitType unitType, vp_Inventory inventory)
		: base(null, 0)
	{
		this.UnitType = unitType;
		this.m_Inventory = inventory;
	}

	public virtual bool TryRemoveUnits(int amount)
	{
		if (this.Count <= 0)
		{
			return false;
		}
		amount = Mathf.Max(0, amount);
		if (amount == 0)
		{
			return false;
		}
		this.Count = Mathf.Max(0, this.Count - amount);
		return true;
	}

	public virtual bool TryGiveUnits(int amount)
	{
		if (this.Type != null && !((vp_UnitBankType)this.Type).Reloadable)
		{
			return false;
		}
		if (this.Capacity != -1 && this.Count >= this.Capacity)
		{
			return false;
		}
		amount = Mathf.Max(0, amount);
		if (amount == 0)
		{
			return false;
		}
		this.Count += amount;
		if (this.Count <= this.Capacity)
		{
			return true;
		}
		if (this.Capacity == -1)
		{
			return true;
		}
		this.Count = this.Capacity;
		return true;
	}

	public virtual bool IsInternal
	{
		get
		{
			return this.Type == null;
		}
	}

	public virtual bool DoAddUnits(int amount)
	{
		this.m_PrevCount = this.Count;
		this.m_Result = this.TryGiveUnits(amount);
		if (this.m_Inventory.SpaceEnabled && this.m_Result && !this.IsInternal && this.m_Inventory.SpaceMode == vp_Inventory.Mode.Weight)
		{
			this.m_Inventory.UsedSpace += (float)(this.Count - this.m_PrevCount) * this.UnitType.Space;
		}
		this.m_Inventory.SetDirty();
		return this.m_Result;
	}

	public virtual bool DoRemoveUnits(int amount)
	{
		this.m_PrevCount = this.Count;
		this.m_Result = this.TryRemoveUnits(amount);
		if (this.m_Inventory.SpaceEnabled && this.m_Result && !this.IsInternal && this.m_Inventory.SpaceMode == vp_Inventory.Mode.Weight)
		{
			this.m_Inventory.UsedSpace = Mathf.Max(0f, this.m_Inventory.UsedSpace - (float)(this.m_PrevCount - this.Count) * this.UnitType.Space);
		}
		this.m_Inventory.SetDirty();
		return this.m_Result;
	}

	public virtual int ClampToCapacity()
	{
		int count = this.Count;
		if (this.Capacity != -1)
		{
			this.Count = Mathf.Clamp(this.Count, 0, this.Capacity);
		}
		this.Count = Mathf.Max(0, this.Count);
		return count - this.Count;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public const int UNLIMITED = -1;

	[SerializeField]
	public vp_UnitType UnitType;

	[SerializeField]
	public int Count;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public int m_Capacity = -1;

	[SerializeField]
	[PublicizedFrom(EAccessModifier.Protected)]
	public vp_Inventory m_Inventory;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public bool m_Result;

	[PublicizedFrom(EAccessModifier.Protected)]
	[NonSerialized]
	public int m_PrevCount;
}
