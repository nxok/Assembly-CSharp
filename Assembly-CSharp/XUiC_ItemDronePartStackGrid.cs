using System;
using UnityEngine.Scripting;

[Preserve]
public class XUiC_ItemDronePartStackGrid : XUiC_ItemPartStackGrid
{
	public EntityDrone CurrentVehicle { get; set; }

	public override void Init()
	{
		base.Init();
		XUiController[] childrenByType = base.GetChildrenByType<XUiC_ItemDronePartStack>(null);
		this.itemControllers = childrenByType;
	}
}
