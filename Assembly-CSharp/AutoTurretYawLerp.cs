using System;
using System.Globalization;
using UnityEngine;

public class AutoTurretYawLerp : MonoBehaviour
{
	public float Yaw { get; set; }

	public float CurrentYaw
	{
		get
		{
			return this.myTransform.localRotation.eulerAngles.y - this.BaseRotation.y;
		}
	}

	public bool IsTurning
	{
		get; [PublicizedFrom(EAccessModifier.Private)]
		set;
	}

	public void Init(DynamicProperties _properties)
	{
		if (_properties.Values.ContainsKey("TurnSpeed"))
		{
			this.degreesPerSecond = StringParsers.ParseFloat(_properties.Values["TurnSpeed"], 0, -1, NumberStyles.Any);
		}
		if (_properties.Values.ContainsKey("TurnSpeedIdle"))
		{
			this.idleDegreesPerSecond = StringParsers.ParseFloat(_properties.Values["TurnSpeedIdle"], 0, -1, NumberStyles.Any);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.myTransform = base.transform;
	}

	public void SetYaw()
	{
		this.myTransform.localRotation = Quaternion.Euler(this.BaseRotation.x, this.BaseRotation.y + this.Yaw, this.BaseRotation.z);
	}

	public void UpdateYaw()
	{
		float num = Mathf.LerpAngle(this.myTransform.localRotation.eulerAngles.y, this.BaseRotation.y + this.Yaw, Time.deltaTime * (this.IdleScan ? this.idleDegreesPerSecond : this.degreesPerSecond));
		this.myTransform.localRotation = Quaternion.Euler(this.BaseRotation.x, num, this.BaseRotation.z);
		this.IsTurning = (int)num != (int)((this.BaseRotation.y + this.Yaw) * 100f);
	}

	public Vector3 BaseRotation = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float degreesPerSecond = 11.25f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float idleDegreesPerSecond = 0.5f;

	public bool IdleScan;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform myTransform;
}
