using System;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class VPSteering : VehiclePart
{
	public override void InitPrefabConnections()
	{
		StringParsers.TryParseFloat(base.GetProperty("steerMaxAngle"), out this.steerMaxAngle);
		this.properties.ParseVec("steerAngle", ref this.steerAngles);
		this.steeringJoint = base.GetTransform();
		if (this.steeringJoint)
		{
			this.baseRotation = this.steeringJoint.localRotation;
		}
		base.InitIKTarget(2, this.steeringJoint);
		base.InitIKTarget(3, this.steeringJoint);
	}

	public override void Update(float _dt)
	{
		if (this.steerMaxAngle != 0f)
		{
			this.steeringJoint.localRotation = this.baseRotation * Quaternion.AngleAxis(this.vehicle.CurrentSteeringPercent * this.steerMaxAngle, Vector3.up);
		}
		if (this.steerAngles.sqrMagnitude != 0f)
		{
			float currentSteeringPercent = this.vehicle.CurrentSteeringPercent;
			this.steeringJoint.localRotation = this.baseRotation * Quaternion.Euler(currentSteeringPercent * this.steerAngles.x, currentSteeringPercent * this.steerAngles.y, currentSteeringPercent * this.steerAngles.z);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public Transform steeringJoint;

	[PublicizedFrom(EAccessModifier.Private)]
	public Quaternion baseRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	public float steerMaxAngle;

	[PublicizedFrom(EAccessModifier.Private)]
	public Vector3 steerAngles;
}
