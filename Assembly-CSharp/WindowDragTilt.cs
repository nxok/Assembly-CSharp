using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Window Drag Tilt")]
public class WindowDragTilt : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void OnEnable()
	{
		this.mTrans = base.transform;
		this.mLastPos = this.mTrans.position;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		Vector3 vector = this.mTrans.position - this.mLastPos;
		this.mLastPos = this.mTrans.position;
		this.mAngle += vector.x * this.degrees;
		this.mAngle = NGUIMath.SpringLerp(this.mAngle, 0f, 20f, Time.deltaTime);
		this.mTrans.localRotation = Quaternion.Euler(0f, 0f, -this.mAngle);
	}

	public int updateOrder;

	public float degrees = 30f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector3 mLastPos;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform mTrans;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public float mAngle;
}
