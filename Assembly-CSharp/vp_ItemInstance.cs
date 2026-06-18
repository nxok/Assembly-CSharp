using System;
using UnityEngine;

[Serializable]
public class vp_ItemInstance
{
	public vp_ItemInstance(vp_ItemType type, int id)
	{
		this.ID = id;
		this.Type = type;
	}

	public virtual void SetUniqueID()
	{
		this.ID = vp_Utility.UniqueID;
	}

	[SerializeField]
	public vp_ItemType Type;

	[SerializeField]
	public int ID;
}
