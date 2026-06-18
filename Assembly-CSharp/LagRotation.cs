using System;
using UnityEngine;

[AddComponentMenu("NGUI/Examples/Lag Rotation")]
public class LagRotation : MonoBehaviour
{
	public void OnRepositionEnd()
	{
		this.Interpolate(1000f);
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Interpolate(float delta)
	{
		if (this.mTrans != null)
		{
			Transform parent = this.mTrans.parent;
			if (parent != null)
			{
				this.mAbsolute = Quaternion.Slerp(this.mAbsolute, parent.rotation * this.mRelative, delta * this.speed);
				this.mTrans.rotation = this.mAbsolute;
			}
		}
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Start()
	{
		this.mTrans = base.transform;
		this.mRelative = this.mTrans.localRotation;
		this.mAbsolute = this.mTrans.rotation;
	}

	[PublicizedFrom(EAccessModifier.Private)]
	public void Update()
	{
		this.Interpolate(this.ignoreTimeScale ? RealTime.deltaTime : Time.deltaTime);
	}

	public float speed = 10f;

	public bool ignoreTimeScale;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Transform mTrans;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion mRelative;

	[PublicizedFrom(EAccessModifier.Private)]
	[NonSerialized]
	public Quaternion mAbsolute;
}
