using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Pan With Mouse")]
public class PanWithMouse : MonoBehaviour
{
	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.mTrans = base.transform;
		this.mStart = this.mTrans.localRotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		float deltaTime = RealTime.deltaTime;
		Vector3 vector = UICamera.lastEventPosition;
		float num = (float)Screen.width * 0.5f;
		float num2 = (float)Screen.height * 0.5f;
		if (this.range < 0.1f)
		{
			this.range = 0.1f;
		}
		float num3 = Mathf.Clamp((vector.x - num) / num / this.range, -1f, 1f);
		float num4 = Mathf.Clamp((vector.y - num2) / num2 / this.range, -1f, 1f);
		this.mRot = Vector2.Lerp(this.mRot, new Vector2(num3, num4), deltaTime * 5f);
		this.mTrans.localRotation = this.mStart * Quaternion.Euler(-this.mRot.y * this.degrees.y, this.mRot.x * this.degrees.x, 0f);
	}

	public Vector2 degrees = new Vector2(5f, 3f);

	public float range = 1f;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform mTrans;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion mStart;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Vector2 mRot = Vector2.zero;
}
