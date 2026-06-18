using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Spin")]
public class Spin : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.mTrans = base.transform;
		this.mRb = base.GetComponent<Rigidbody>();
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		if (this.mRb == null)
		{
			this.ApplyDelta(this.ignoreTimeScale ? RealTime.deltaTime : Time.deltaTime);
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void FixedUpdate()
	{
		if (this.mRb != null)
		{
			this.ApplyDelta(Time.deltaTime);
		}
	}

	public void ApplyDelta(float delta)
	{
		delta *= 360f;
		Quaternion quaternion = Quaternion.Euler(this.rotationsPerSecond * delta);
		if (this.mRb == null)
		{
			this.mTrans.rotation = this.mTrans.rotation * quaternion;
			return;
		}
		this.mRb.MoveRotation(this.mRb.rotation * quaternion);
	}

	public Vector3 rotationsPerSecond = new Vector3(0f, 0.1f, 0f);

	public bool ignoreTimeScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Rigidbody mRb;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform mTrans;
}
