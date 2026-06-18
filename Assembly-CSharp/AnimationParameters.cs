using System;
using UnityEngine;

public class AnimationParameters : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.anim = base.GetComponent<Animator>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		this.currentRotation = base.transform.rotation;
		Quaternion quaternion = this.currentRotation * Quaternion.Inverse(this.previousRotation);
		this.previousRotation = this.currentRotation;
		float num;
		Vector3 vector;
		quaternion.ToAngleAxis(ref num, ref vector);
		num *= 0.017453292f;
		this.angularVelocity = 1f / Time.deltaTime * num * vector;
		this.deltaYaw = Mathf.SmoothDamp(this.deltaYaw, this.angularVelocity.y, ref this.turnVelocity, this.deltaYawSmoothTime);
		if (this.debugMode)
		{
			if (Mathf.Abs(this.deltaYaw) > 0.001f)
			{
				Debug.Log("DeltaYaw: " + this.deltaYaw.ToString());
			}
			if (this.deltaYaw < this.deltaYawMin)
			{
				this.deltaYawMin = this.deltaYaw;
			}
			if (this.deltaYaw > this.deltaYawMax)
			{
				this.deltaYawMax = this.deltaYaw;
			}
		}
		this.anim.SetFloat("deltaYaw", this.deltaYaw);
		this.anim.SetFloat("TurnPlayRate", this.deltaYaw);
		if (Mathf.Abs(this.angularVelocity.y) > 0.1f)
		{
			this.anim.SetBool("Turning", true);
			return;
		}
		this.anim.SetBool("Turning", false);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Animator anim;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 currentEulerAngles;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion currentRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion previousRotation;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float currentYaw;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float lastYaw;

	public float turnPlayRateMultiplier;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float turnPlayRate;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float deltaYawTarget;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float deltaYaw;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float angle;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 angularVelocity;

	public bool debugMode;

	public float deltaYawMin;

	public float deltaYawMax;

	public float deltaYawSmoothTime = 0.3f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float turnVelocity;
}
