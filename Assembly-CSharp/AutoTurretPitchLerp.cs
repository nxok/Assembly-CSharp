using System;
using System.Globalization;
using UnityEngine;

public class AutoTurretPitchLerp : MonoBehaviour
{
	public float Pitch { get; set; }

	public float CurrentPitch
	{
		get
		{
			return this.myTransform.localRotation.eulerAngles.x - this.BaseRotation.x;
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
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Awake()
	{
		this.myTransform = base.transform;
	}

	public void SetPitch()
	{
		this.myTransform.localRotation = Quaternion.Euler(this.BaseRotation.x + this.Pitch, this.BaseRotation.y, this.BaseRotation.z);
	}

	public void UpdatePitch()
	{
		int num = (int)(this.myTransform.localRotation.eulerAngles.x * 1000f);
		this.myTransform.localRotation = Quaternion.Euler(Mathf.LerpAngle(this.myTransform.localRotation.eulerAngles.x, this.BaseRotation.x + this.Pitch, Time.deltaTime * ((this.IdleScan ? 0.25f : 1f) * this.degreesPerSecond)), this.BaseRotation.y, this.BaseRotation.z);
		this.IsTurning = (int)(this.myTransform.localRotation.eulerAngles.x * 1000f) != num;
	}

	public Vector3 BaseRotation = Vector3.zero;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float degreesPerSecond = 11.25f;

	public bool IdleScan;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform myTransform;
}
