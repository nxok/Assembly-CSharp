using System;
using UnityEngine.Scripting;

[Preserve]
public class VPSeat : VehiclePart
{
	public override void InitPrefabConnections()
	{
		base.InitIKTarget(2, null);
		base.InitIKTarget(3, null);
		base.InitIKTarget(0, null);
		base.InitIKTarget(1, null);
	}
}
