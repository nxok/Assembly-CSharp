using System;
using UnityEngine.Scripting;

[Preserve]
public class EntityVJeep : EntityDriveable
{
	[PublicizedFrom(EAccessModifier.Protected)]
	public override void UpdateWheelsSteering()
	{
		this.wheels[0].wheelC.steerAngle = this.wheelDir;
		this.wheels[1].wheelC.steerAngle = this.wheelDir;
	}
}
