using System;
using UnityEngine;

public class vp_ItemIdentifier : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnEnable()
	{
		vp_TargetEventReturn<vp_ItemType>.Register(base.transform, "GetItemType", new Func<vp_ItemType>(this.GetItemType));
		vp_TargetEventReturn<int>.Register(base.transform, "GetItemID", new Func<int>(this.GetItemID));
	}

	[PublicizedFrom(EAccessModifier.Protected)]
	public virtual void OnDisable()
	{
	}

	public virtual vp_ItemType GetItemType()
	{
		return this.Type;
	}

	public virtual int GetItemID()
	{
		return this.ID;
	}

	public vp_ItemType Type;

	public int ID;
}
