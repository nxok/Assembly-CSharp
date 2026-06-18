using System;
using UnityEngine.Scripting;

[Preserve]
public class EntityVGyroCopter : EntityDriveable
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateWheelsSteering()
	{
		this.wheels[0].wheelC.steerAngle = this.wheelDir;
	}
}
